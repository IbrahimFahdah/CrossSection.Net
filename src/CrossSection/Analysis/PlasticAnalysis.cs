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

using System;
using System.Linq;
using CrossSection.DataModel;
using CrossSection.Maths;
using CrossSection.Triangulation;
using TriangleNet;

namespace CrossSection.Analysis
{
    public class PlasticAnalysis
    {
        private readonly fea _fea = new fea();

        Mesh _mesh;
        SectionDefinition _sec;

        (double, double) _c_top, _c_bot;
        double _f_top;
       Vector evaluate_force_eq_u;
       Vector evaluate_force_eq_u_p;

        public void Solve(SectionDefinition sec)
        {
            _sec = sec;

            //need to triangulate as the section may have been shifted 
            _mesh = _sec.Triangulate();

            calculate_plastic_properties(sec, _mesh);
        }

        /// <summary>
        /// Calculates the plastic properties of the cross-section
        /// </summary>
        private void calculate_plastic_properties(SectionDefinition sec, Mesh mesh)
        {
            var secPro = sec.Output.SectionProperties;

            //# calculate distances to the extreme fibres
            var fibres = calculate_extreme_fibres(sec, mesh, 0);

            var u = new Vector(new[] { 1.0, 0 });

            //  # 1a) Calculate x-axis plastic centroid
            var fibres_x =new Vector(new[] { fibres.v_min, fibres.v_max });
            (double y_pc, bool r, double f, (double x, double y) c_top, (double x, double y) c_bot) =
                pc_algorithm(u, fibres_x, 1);

            secPro.y_pc = y_pc;
            secPro.sxx = f * Math.Abs(c_top.y - c_bot.y);


            //# 1b) Calculate y-axis plastic centroid
            u = new Vector(new[] { 0, 1.0 });
           Vector fibres_y = new Vector(new[] { fibres.u_min, fibres.u_max });
            (double x_pc, bool r2, double f2, (double x, double y) c_top2, (double x, double y) c_bot2) =
                pc_algorithm(u, fibres_y, 2);

            secPro.x_pc = x_pc;
            secPro.syy = f2 * Math.Abs(c_top2.x - c_bot2.x);

            //# 2) Calculate plastic properties for principal axis
            //# convert principal axis angle to radians
            var angle = secPro.phi * Math.PI / 180;

            //# unit vectors in the axis directions
           Vector ux = new Vector(new[] { Math.Cos(angle), Math.Sin(angle) });
           Vector uy = new Vector(new[] { -Math.Sin(angle), Math.Cos(angle) });

            fibres = calculate_extreme_fibres(sec, mesh, secPro.phi);

            //# 2a) Calculate 11-axis plastic centroid
            fibres_x =new Vector(new[] { fibres.v_min, fibres.v_max });
            (double y22_pc, bool r11, double f11, (double x, double y) c_top11, (double x, double y) c_bot11) =
                pc_algorithm(ux, fibres_x, 1);

            //# calculate the centroids in the principal coordinate system
            var c_top_p = _fea.principal_coordinate(secPro.phi, c_top11.x, c_top11.y);
            var c_bot_p = _fea.principal_coordinate(secPro.phi, c_bot11.x, c_bot11.y);

            //self.check_convergence(r, '11-axis')
            secPro.y22_pc = y22_pc;
            secPro.s11 = f * Math.Abs(c_top_p.y2 - c_bot_p.y2);

            //# 2b) Calculate 22-axis plastic centroid
            fibres_y = new Vector(new[] { fibres.u_min, fibres.u_max });
            (double x11_pc, bool r22, double f22, (double x, double y) c_top22, (double x, double y) c_bot22) =
                pc_algorithm(uy, fibres_y, 2);

            // # calculate the centroids in the principal coordinate system
            c_top_p = _fea.principal_coordinate(secPro.phi, c_top22.x, c_top22.y);
            c_bot_p = _fea.principal_coordinate(secPro.phi, c_bot22.x, c_bot22.y);

            //self.check_convergence(r, '22-axis')
            secPro.x11_pc = x11_pc;
            secPro.s22 = f * Math.Abs(c_top_p.x2 - c_bot_p.x2);
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
        private (double d, bool r, double f_top, (double, double) c_top, (double, double) c_bot) pc_algorithm(
           Vector u,Vector dlim, int axis)
        {
            //# calculate vector perpendicular to u
           Vector u_p;
            if (axis == 1)
            {
                u_p =new Vector(new[] { -u[1], u[0] });
            }
            else
            {
                u_p = new Vector(new[] { u[1], -u[0] });
            }

            var a = dlim[0];
            var b = dlim[1];

            evaluate_force_eq_u = u;
            evaluate_force_eq_u_p = u_p;
            SolutionSettings settings = _sec.SolutionSettings; 

            var r = Brent.TryFindRoot(evaluate_force_eq, a, b, settings.PlasticAxisAccuracy,
                settings.PlasticAxisMaxIterations, out var d);
            return (d, r, _f_top, _c_top, _c_bot);
        }

        /// <summary>
        /// Calculates the locations of the extreme fibres along and perpendicular to the axis specified by 'angle'
        /// </summary>
        /// <param name="angle">Angle(in radians) along which to calculate the extreme fibre locations</param>
        /// <returns>The location of the extreme fibres parallel(u) and perpendicular(v) to the axis * (u_min, u_max, v_min, v_max) *</returns>
        private (double u_min, double u_max, double v_min, double v_max) calculate_extreme_fibres
           (SectionDefinition sec, Mesh mesh, double angle)
        {
            var u_max = 0.0;
            var u_min = 0.0;
            var v_max = 0.0;
            var v_min = 0.0;
            var initialise = true;

            foreach (var pt in mesh.Vertices)
            {
                var x = pt.X;
                var y = pt.Y;
                // determine the coordinate of the point wrt the principal axis
                var (u, v) = _fea.principal_coordinate(angle, x, y);

                //// initialise min, max variables
                if (initialise)
                {
                    u_max = u;
                    u_min = u;
                    v_max = v;
                    v_min = v;
                    initialise = false;
                }

                //// update the mins and maxs where necessary
                u_max = Math.Max(u_max, u);
                u_min = Math.Min(u_min, u);
                v_max = Math.Max(v_max, v);
                v_min = Math.Min(v_min, v);
            }

            return (u_min, u_max, v_min, v_max);
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
           Vector u = evaluate_force_eq_u;

            // Unit vector perpendicular to the direction of the axis
           Vector u_p = evaluate_force_eq_u_p;
            var p = new Vector(new[] { d * u_p[0], d * u_p[1] });

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
        private (double f_top, double f_bot) calculate_plastic_force(SectionDefinition sec, Mesh mesh,Vector u,
           Vector p)
        {
            //# initialise variables
            var f = (top: 0.0, bot: 0.0);
            var ea = (top: 0.0, bot: 0.0);
            var qx = (top: 0.0, bot: 0.0);
            var qy = (top: 0.0, bot: 0.0);

            Matrix coords = null;
            foreach (var item in mesh.Triangles)
            {
                var contour = sec.Contours.FirstOrDefault(c => c.Material?.Id == item.Label);
                 var mat = contour.Material;

                item.GetTriCoords(ref coords);

                var (f_el, ea_el, qx_el, qy_el, is_above) =
                    _fea.plastic_properties(mat, coords, u, p);

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


            //# calculate the centroid of the top and bottom segments and save
            this._c_top = (qy.top / ea.top, qx.top / ea.top);
            this._c_bot = (qy.bot / ea.bot, qx.bot / ea.bot);
            this._f_top = f.top;

            return (f.top, f.bot);
        }
    }
}