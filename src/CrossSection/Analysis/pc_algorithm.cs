// <copyright>
//https://github.com/IbrahimFahdah/CrossSection.Net

//Copyright(c) 2019 Ibrahim Fahdah

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
//</copyright>

using CrossSection.DataModel;
using CrossSection.Maths;
using CrossSection.Triangulation;
using System.Collections.Generic;
using System.Linq;
using TriangleNet;
using TriangleNet.Topology;

namespace CrossSection.Analysis
{
    public class pc_algorithm
    {
        private readonly fea _fea = new fea();

        (double, double) _c_top, _c_bot;
        double _f_top;
        double[] evaluate_force_eq_u;
        double[] evaluate_force_eq_u_p;

        Mesh _mesh;
        SectionDefinition _sec;

        public pc_algorithm(SectionDefinition sec, Mesh mesh)
        {
            _sec = sec;

            //need to triangulate as the section may have been shifted 
            _mesh = mesh;

        }


        /// <summary>
        /// An algorithm used for solving for the location of the plastic
        /// centroid. The algorithm searches for the location of the axis, defined
        /// by unit vector *u* and within the section depth, that satisfies force
        /// equilibrium.
        /// </summary>
        /// <param name="u">Unit vector defining the direction of the axis</param>
        /// <param name="dlim">[dmax, dmin] containing the distances from the centroid to the extreme fibres perpendicular to the axis</param>
        /// <param name="axis">he current axis direction: 1 (e.g. x or 11) or 2 (e.g.y or 22)</param>
        /// <returns>The distance to the plastic centroid axis *d*, the result bool *r* if solution is found, 
        /// the force in the top of the section* f_top* and the location of the centroids of the top and bottom areas *c_top* and *c_bottom*
        /// </returns>
        internal (double d, bool r, double f_top, (double, double) c_top, (double, double) c_bot) Run(
           double[] u, double[] dlim, int axis)
        {
            //# calculate vector perpendicular to u
            double[] u_p;
            if (axis == 1)
            {
                u_p = new[] { -u[1], u[0] };
            }
            else
            {
                u_p = new[] { u[1], -u[0] };
            }

            var a = dlim[0];
            var b = dlim[1];

            evaluate_force_eq_u = u;
            evaluate_force_eq_u_p = u_p;
            SolutionSettings settings = _sec.SolutionSettings;

            var r = new Brent().TryFindRoot(evaluate_force_eq, a, b, settings.PlasticAxisAccuracy,
                settings.PlasticAxisMaxIterations, out var d);
            return (d, r, _f_top, _c_top, _c_bot);
        }



        /// <summary>
        /// Given a distance *d* from the centroid to an axis (defined by unit vector* u*),
        /// calculates the force equilibrium. The resultant force, as a ratio of the total force, is returned.
        /// </summary>
        /// <param name="d">Distance from the centroid to current axis</param>
        /// <returns>The force equilibrium norm</returns>
        private double evaluate_force_eq(double d)
        {
            //Unit vector defining the direction of the axis
            var u = evaluate_force_eq_u;

            // Unit vector perpendicular to the direction of the axis
            var u_p = evaluate_force_eq_u_p;
            var p = new[] { d * u_p[0], d * u_p[1] };

            //# calculate force equilibrium
            var (f_top, f_bot) = calculate_plastic_force(_sec, _mesh, u, p);

            //# calculate the force norm
            var f_norm = (f_top - f_bot) / (f_top + f_bot);

            return f_norm;
        }


        /// <summary>
        /// Sums the forces above and below the axis defined by unit vector *u*
        /// and point *p*. Also returns the force centroid of the forces above and below the axis.
        /// </summary>
        /// <param name="u">Unit vector defining the direction of the axis</param>
        /// <param name="p">Point on the axis</param>
        /// <returns> Force in the top and bottom areas *(f_top, f_bot)*</returns>
        private (double f_top, double f_bot) calculate_plastic_force(SectionDefinition sec, Mesh mesh, double[] u,
           double[] p)
        {
            //# initialise variables
            var f = (top: 0.0, bot: 0.0);
            var ea = (top: 0.0, bot: 0.0);
            var qx = (top: 0.0, bot: 0.0);
            var qy = (top: 0.0, bot: 0.0);

            foreach (var item in mesh.Triangles)
            {
                var contour = sec.Contours.FirstOrDefault(c => c.Material?.Id == item.Label);
                var mat = contour.Material;

                var lst = SplitTri(item, u, p);
                foreach (var Coords in lst)
                {
                    var (f_el, ea_el, qx_el, qy_el, is_above) =
                  _fea.plastic_properties(mat, Coords, u, p);

                    //# assign force and area properties to the top or bottom segments
                    if (is_above)
                    {
                        f.top += f_el;
                        ea.top += ea_el;
                        qx.top += qx_el;
                        qy.top += qy_el;
                    }
                    else
                    {
                        f.bot += f_el;
                        ea.bot += ea_el;
                        qx.bot += qx_el;
                        qy.bot += qy_el;
                    }
                }

            }


            //# calculate the centroid of the top and bottom segments and save
            this._c_top = (qy.top / ea.top, qx.top / ea.top);
            this._c_bot = (qy.bot / ea.bot, qx.bot / ea.bot);
            this._f_top = f.top;

            return (f.top, f.bot);
        }

        /// <summary>
        /// Split the triangle to three ones if the axis intersect with the triangle.
        /// </summary>
        /// <param name="tri"></param>
        /// <param name="u">axis direction</param>
        /// <param name="p">point on the axis</param>
        /// <returns></returns>
        private List<double[,]> SplitTri(Triangle tri, double[] u, double[] p)
        {
            var coords = tri.GetTriCoords3();

            var x1 = coords[0, 0];
            var x2 = coords[0, 1];
            var x3 = coords[0, 2];

            var y1 = coords[1, 0];
            var y2 = coords[1, 1];
            var y3 = coords[1, 2];

            var p12 = LineSegementsIntersect(x1, y1, x2, y2, u, p);
            var p13 = LineSegementsIntersect(x1, y1, x3, y3, u, p);
            var p23 = LineSegementsIntersect(x2, y2, x3, y3, u, p);

            List<double[,]> lst = new List<double[,]>();

            /*N.B. the numbering orientation of the coordinates of the new triangles are important.
             * They follow the original triangle (anti-clockwise).*/
            if (p12 != null && p13 != null)
            {
                //tri1 => p1->p12->p13
                //tri2 => p13->p12->p3
                //tri3 => p12->p2->p3
                var tri1 = TriExtensions.GetTriCoords6(x1, p12.X, p13.X, y1, p12.Y, p13.Y);
                var tri2 = TriExtensions.GetTriCoords6(p13.X, p12.X, x3, p13.Y, p12.Y, y3);
                var tri3 = TriExtensions.GetTriCoords6(p12.X, x2, x3, p12.Y, y2, y3);

                lst.Add(tri1);
                lst.Add(tri2);
                lst.Add(tri3);
            }
            else if (p12 != null && p23 != null)
            {
                var tri1 = TriExtensions.GetTriCoords6(x1, p12.X, x3, y1, p12.Y, y3);
                var tri2 = TriExtensions.GetTriCoords6(p12.X, p23.X, x3, p12.Y, p23.Y, y3);
                var tri3 = TriExtensions.GetTriCoords6(p12.X, x2, p23.X, p12.Y, y2, p23.Y);

                lst.Add(tri1);
                lst.Add(tri2);
                lst.Add(tri3);
            }
            else if (p13 != null && p23 != null)
            {
                var tri1 = TriExtensions.GetTriCoords6(p13.X, p23.X, x3, p13.Y, p23.Y, y3);
                var tri2 = TriExtensions.GetTriCoords6(x1, p23.X, p13.X, y1, p23.Y, p13.Y);
                var tri3 = TriExtensions.GetTriCoords6(x1, x2, p23.X, y1, y2, p23.Y);

                lst.Add(tri1);
                lst.Add(tri2);
                lst.Add(tri3);
            }
            else
            {
                //no intersection or intersection at vertex. tri => p1->p2->p3
                lst.Add(tri.GetTriCoords());
            }

            return lst;
        }

        private Point2D LineSegementsIntersect(double x1, double y1, double x2, double y2, double[] rr, double[] pp)
        {
            var q1 = new Vector2D(x1, y1);
            var q2 = new Vector2D(x2, y2);
            var s = q2 - q1;

            var p = new Vector2D(pp[0], pp[1]);
            var r = new Vector2D(rr[0], rr[1]);

            //# cacluate intersection point between p -> p + r and q -> q + s
            //#if the lines are not parallel
            if (r.Cross(s) != 0)
            {
                //# calculate t and u
                var t = Vector2D.Cross(q1 - p, s) / Vector2D.Cross(r, s);
                var u = Vector2D.Cross(p - q1, r) / Vector2D.Cross(s, r);
                var new_pt = p + t * r;

                // #if the line lies within q -> q + s and the point hasn't
                if (u >= 0 && u <= 1)
                {
                    return new Point2D(new_pt.X, new_pt.Y);
                }
            }


            return null;

        }

    }
}