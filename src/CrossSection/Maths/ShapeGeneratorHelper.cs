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
using System.Collections.Generic;

namespace CrossSection.Triangulation
{
    public class ShapeGeneratorHelper
    {

        public List<Point2D> CreateCircle(double r, int n)
        {
            return CreateCircle(0.0, 0.0, r, n);
        }

        public List<Point2D> CreateCircle(double x, double y, double r, int n)
        {
            return CreateEllipse(0.0, 0.0, r, 1.0, 1.0, n);
        }

        public List<Point2D> CreateEllipse(double r, double a, double b, int n)
        {
            return CreateEllipse(0.0, 0.0, r, a, b, n);
        }

        public List<Point2D> CreateEllipse(double x, double y, double r, double a, double b, int n)
        {
            var contour = new List<Point2D>(n);

            double dphi = 2 * Math.PI / n;

            for (int i = 0; i < n; i++)
            {
                contour.Add(new Point2D(x + a * r * Math.Cos(i * dphi), y + b * r * Math.Sin(i * dphi)));
            }

            return contour;
        }



        /// <summary>
        /// Constructs a rectangular section with the bottom left corner at the  origin* (0, 0)*, with depth *d* and width* b*.
        /// </summary>
        /// <param name="d">Depth (y) of the rectangle</param>
        /// <param name="b">Width (x) of the rectangle</param>
        /// <returns></returns>
        public List<Point2D> CreateRectangle(double d, double b)
        {
            var contour = new List<Point2D>();

            contour.Add(new Point2D(0, 0));
            contour.Add(new Point2D(b, 0));
            contour.Add(new Point2D(b, d));
            contour.Add(new Point2D(0, d));
            return contour;
        }

        #region Chs

        /// <summary>
        /// Constructs a circular hollow section centered at the origin *(0, 0)*
        /// </summary>
        /// <param name="d">Outer diameter of the CHS</param>
        /// <param name="t">Thickness of the CHS</param>
        /// <param name="n">Number of points discretising the inner and outer circles</param>
        /// <returns></returns>
        public (List<Point2D> outer, List<Point2D> inner) CreateChsShape(double d, double t, int n)
        {
            var outer = CreateCircle(d / 2, n);
            var inner = CreateCircle(d / 2 - t, n);


            return (outer, inner);
        }
        #endregion

        #region RHS
        /// <summary>
        /// Constructs a rectangular hollow section centered at *(b/2, d/2)*
        /// </summary>
        /// <param name="d">Depth of the RHS</param>
        /// <param name="b">Width of the RHS</param>
        /// <param name="t">Thickness of the RHS</param>
        /// <param name="r_out">Outer radius of the RHS</param>
        /// <param name="n_r">Number of points discretising the inner and outer radii</param>
        /// <returns></returns>
        public (List<Point2D> outer, List<Point2D> inner) CreateRhsShape(double d, double b, double t, double r_out, int n_r)
        {
            var outer = new List<Point2D>();
            //# calculate internal radius
            var r_in = Math.Max(r_out - t, 0);


            //# construct the outer radius points
            AddRadius(outer, new Point2D(r_out, r_out), r_out, Math.PI, n_r);
            AddRadius(outer, new Point2D(b - r_out, r_out), r_out, 1.5 * Math.PI, n_r);
            AddRadius(outer, new Point2D(b - r_out, d - r_out), r_out, 0, n_r);
            AddRadius(outer, new Point2D(r_out, d - r_out), r_out, 0.5 * Math.PI, n_r);



            var inner = new List<Point2D>();
            //# construct the inner radius points
            AddRadius(inner, new Point2D(t + r_in, t + r_in), r_in, Math.PI, n_r);
            AddRadius(inner, new Point2D(b - t - r_in, t + r_in), r_in, 1.5 * Math.PI, n_r);
            AddRadius(inner, new Point2D(b - t - r_in, d - t - r_in), r_in, 0, n_r);
            AddRadius(inner, new Point2D(t + r_in, d - t - r_in), r_in, 0.5 * Math.PI, n_r);



            return (outer, inner);
        }
        #endregion

        #region Ehs

        /// <summary>
        /// "Constructs an elliptical hollow section centered at the origin *(0, 0)*, 
        /// with outer vertical diameter *d_y*, outer horizontal diameter* d_x*, and
        /// thickness* t*, using * n* points to construct the inner and outer ellipses.
        /// </summary>
        /// <param name="d_x"> Diameter of the ellipse in the x-dimension</param>
        /// <param name="d_y"> Diameter of the ellipse in the y-dimension</param>
        /// <param name="t">Thickness of the EHS</param>
        /// <param name="n">Number of points discretising the inner and outer ellipses</param>
        /// <returns></returns>
        public (List<Point2D> outer, List<Point2D> inner) CreateEhsShape(double d_x, double d_y, double t, int n)
        {
            var outer = CreateEllipse(0, 0, 1, d_x / 2.0, d_y / 2.0, n);
            var inner = CreateEllipse(0, 0, 1, (d_x) / 2.0 - t, (d_y) / 2.0 - t, n);


            return (outer, inner);
        }
        #endregion


        /// <summary>
        /// Constructs a Tee section with the top left corner at *(0, d)*, with
        /// depth* d*, width *b*, flange thickness *t_f*, web thickness *t_w* and root
        /// radius* r*, using * n_r* points to construct the root radius.
        /// </summary>
        /// <param name="d">Depth of the Tee section</param>
        /// <param name="b">Width of the Tee section</param>
        /// <param name="t_f">Flange thickness of the Tee section</param>
        /// <param name="t_w">Web thickness of the Tee section</param>
        /// <param name="r">Root radius of the Tee section</param>
        /// <param name="n_r">Number of points discretising the root radius</param>
        /// <returns></returns>
        public List<Point2D> TeeSection(double d, double b, double t_f,
     double t_w, double r, int n_r)
        {
            var contour = new List<Point2D>();
            //# add first two points
            contour.Add(new Point2D(b * 0.5 - t_w * 0.5, 0));
            contour.Add(new Point2D(b * 0.5 + t_w * 0.5, 0));

            // # construct the top right radius
            var pt = new Point2D(b * 0.5 + t_w * 0.5 + r, d - t_f - r);
            AddRadius(contour, pt, r, Math.PI, n_r, false);

            // # add next four points
            contour.Add(new Point2D(b, d - t_f));
            contour.Add(new Point2D(b, d));
            contour.Add(new Point2D(0, d));
            contour.Add(new Point2D(0, d - t_f));

            //  # construct the top left radius
            pt = new Point2D(b * 0.5 - t_w * 0.5 - r, d - t_f - r);
            AddRadius(contour, pt, r, 0.5 * Math.PI, n_r, false);


            return contour;
        }


        #region MonoISection
        public List<Point2D> CreateMonoISection(double ht, double bt, double tft, double twt, double rt,
             double hb, double bb, double tfb, double twb, double rb,
             int n_r)
        {
            var contour = new List<Point2D>();

            //# calculate central axis
            var x_central = Math.Max(bt, bb) * 0.5;

            var d = ht + hb;

            //# add first three points
            contour.Add(new Point2D(x_central - bb * 0.5, 0));
            contour.Add(new Point2D(x_central + bb * 0.5, 0));
            contour.Add(new Point2D(x_central + bb * 0.5, tfb));

            //# construct the bottom right radius
            var pt = new Point2D(x_central + twb * 0.5 + rb, tfb + rb);
            AddRadius(contour, pt, rb, 1.5 * Math.PI, n_r, false);

            if (twt != twb)
            {
                contour.Add(new Point2D(x_central + twb * 0.5, hb));
                contour.Add(new Point2D(x_central + twt * 0.5, hb));
            }

            contour.Add(new Point2D(x_central + twt * 0.5, hb + ht - tft - rt));

            //# construct the top right radius
            pt = new Point2D(x_central + twt * 0.5 + rt, d - tft - rt);
            AddRadius(contour, pt, rt, Math.PI, n_r, false);

            // # add the next four points
            contour.Add(new Point2D(x_central + bt * 0.5, d - tft));
            contour.Add(new Point2D(x_central + bt * 0.5, d));
            contour.Add(new Point2D(x_central - bt * 0.5, d));
            contour.Add(new Point2D(x_central - bt * 0.5, d - tft));

            //# construct the top left radius
            pt = new Point2D(x_central - twt * 0.5 - rt, d - tft - rt);
            AddRadius(contour, pt, rt, 0.5 * Math.PI, n_r, false);

            if (twt != twb)
            {
                contour.Add(new Point2D(x_central - twt * 0.5, hb));
                contour.Add(new Point2D(x_central - twb * 0.5, hb));
            }

            //# construct the bottom left radius
            pt = new Point2D(x_central - twb * 0.5 - rb, tfb + rb);
            AddRadius(contour, pt, rb, 0, n_r, false);

            //# add the last point
            contour.Add(new Point2D(x_central - bb * 0.5, tfb));

            return contour;
        }
        #endregion

        #region Cruciform

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d">Depth of the cruciform section</param>
        /// <param name="b">Width of the cruciform section</param>
        /// <param name="t">Thickness of the cruciform section</param>
        /// <param name="r">Root radius of the cruciform section</param>
        /// <param name="n_r">Number of points discretising the root radius</param>
        /// <returns></returns>
        public List<Point2D> CreateCruciformShape(double d, double b, double t, double r, int n_r)
        {
            var self = new List<Point2D>();

            //# add first two points
            self.Add(new Point2D(-t * 0.5, -d * 0.5));
            self.Add(new Point2D(t * 0.5, -d * 0.5));

            //   # construct the bottom right radius
            var pt = new Point2D(0.5 * t + r, -0.5 * t - r);
            AddRadius(self, pt, r, Math.PI, n_r, false);

            // # add the next two points
            self.Add(new Point2D(0.5 * b, -t * 0.5));
            self.Add(new Point2D(0.5 * b, t * 0.5));

            //  # construct the top right radius
            pt = new Point2D(0.5 * t + r, 0.5 * t + r);
            AddRadius(self, pt, r, 1.5 * Math.PI, n_r, false);

            //  # add the next two points
            self.Add(new Point2D(t * 0.5, 0.5 * d));
            self.Add(new Point2D(-t * 0.5, 0.5 * d));

            //  # construct the top left radius
            pt = new Point2D(-0.5 * t - r, 0.5 * t + r);
            AddRadius(self, pt, r, 0, n_r, false);

            // # add the next two points
            self.Add(new Point2D(-0.5 * b, t * 0.5));
            self.Add(new Point2D(-0.5 * b, -t * 0.5));

            // # construct the bottom left radius
            pt = new Point2D(-0.5 * t - r, -0.5 * t - r);
            AddRadius(self, pt, r, 0.5 * Math.PI, n_r, false);


            return self;

        }
        #endregion



        /// <summary>
        /// Constructs an angle section with the bottom left corner at the origin
        /// *(0, 0)*, with depth *d*, width* b*, thickness *t*, root radius *r_r* and
        /// toe radius *r_t*, using *n_r* points to construct the radii.
        /// </summary>
        /// <param name="d">Depth of the angle section</param>
        /// <param name="b">Width of the angle section</param>
        /// <param name="t">Thickness of the angle section</param>
        /// <param name="r_r">Root radius of the angle section</param>
        /// <param name="r_t">Toe radius of the angle section</param>
        /// <param name="n_r">Number of points discretising the radii</param>
        /// <returns></returns>
        public List<Point2D> CreateAngleShape(double d, double b, double t, double r_r, double r_t, int n_r)
        {
            var contour = new List<Point2D>();

            //# add first two points
            contour.Add(new Point2D(0, 0));
            contour.Add(new Point2D(b, 0));

            //# construct the bottom toe radius
            var pt = new Point2D(b - r_t, t - r_t);
            AddRadius(contour, pt, r_t, 0, n_r);

            //# construct the root radius
            pt = new Point2D(t + r_r, t + r_r);
            AddRadius(contour, pt, r_r, 1.5 * Math.PI, n_r, false);

            // # construct the top toe radius
            pt = new Point2D(t - r_t, d - r_t);
            AddRadius(contour, pt, r_t, 0, n_r);

            // # add the next point
            contour.Add(new Point2D(0, d));


            return contour;
        }

        /// <summary>
        /// Constructs a Cee section with the bottom left corner at the origin
        /// *(0, 0)*, with depth *d*, width* b*, lip *l*, thickness* t* and outer
        /// radius* r_out*, using * n_r* points to construct the radius.If the outer
        /// radius is less than the thickness of the Cee Section, the inner radius is
        /// set to zero.
        /// </summary>
        /// <param name="d">Depth of the Cee section</param>
        /// <param name="b">Width of the Cee section</param>
        /// <param name="l">Lip of the Cee section</param>
        /// <param name="t">Thickness of the Cee section</param>
        /// <param name="r_out">Outer radius of the Cee section</param>
        /// <param name="n_r">Number of points discretising the outer radius</param>
        /// <returns></returns>
        public List<Point2D> CreateCeeShape(double d, double b, double l, double t, double r_out, int n_r)
        {
            var contour = new List<Point2D>();
            // # calculate internal radius
            var r_in = Math.Max(r_out - t, 0);

            // # construct the outer bottom left radius
            AddRadius(contour, new Point2D(r_out, r_out), r_out, Math.PI, n_r);

            //  # construct the outer bottom right radius
            AddRadius(contour, new Point2D(b - r_out, r_out), r_out, 1.5 * Math.PI, n_r);

            if (r_out != l)
            {
                // # add next two points
                contour.Add(new Point2D(b, l));
                contour.Add(new Point2D(b - t, l));
            }


            //   # construct the inner bottom right radius
            AddRadius(contour, new Point2D(b - t - r_in, t + r_in), r_in, 0, n_r, false);

            //# construct the inner bottom left radius
            AddRadius(contour, new Point2D(t + r_in, t + r_in), r_in, 1.5 * Math.PI, n_r, false);

            //  # construct the inner top left radius
            AddRadius(contour, new Point2D(t + r_in, d - t - r_in), r_in, Math.PI, n_r, false);

            //  # construct the inner top right radius
            AddRadius(contour, new Point2D(b - t - r_in, d - t - r_in), r_in, 0.5 * Math.PI, n_r, false);

            if (r_out != l)
            {
                // # add next two points
                contour.Add(new Point2D(b - t, d - l));
                contour.Add(new Point2D(b, d - l));
            }


            //    # construct the outer top right radius
            AddRadius(contour, new Point2D(b - r_out, d - r_out), r_out, 0, n_r);

            //    # construct the outer top left radius
            AddRadius(contour, new Point2D(r_out, d - r_out), r_out, 0.5 * Math.PI, n_r);

            return contour;
        }


        /// <summary>
        /// Constructs a Zed section with the bottom left corner at the origin
        /// *(0, 0)*, with depth *d*, left flange width* b_l*, right flange width
        /// *b_r*, lip* l*, thickness *t* and outer radius *r_out*, using *n_r* points
        /// to construct the radius.If the outer radius is less than the thickness of
        /// the Zed Section, the inner radius is set to zero.
        /// </summary>
        /// <param name="d">Depth of the Zed section</param>
        /// <param name="b_l">Left flange width of the Zed section</param>
        /// <param name="b_r">Right flange width of the Zed section</param>
        /// <param name="l">Lip of the Zed section</param>
        /// <param name="t"> Thickness of the Zed section</param>
        /// <param name="r_out">Outer radius of the Zed section</param>
        /// <param name="n_r">Number of points discretising the outer radius</param>
        /// <returns></returns>
        public List<Point2D> CreateZedShape(double d, double b_l, double b_r, double l, double t, double r_out, int n_r)
        {
            var contour = new List<Point2D>();

            //# calculate internal radius
            var r_in = Math.Max(r_out - t, 0);

            //  # construct the outer bottom left radius
            AddRadius(contour, new Point2D(r_out, r_out), r_out, Math.PI, n_r);

            //  # construct the outer bottom right radius
            AddRadius(contour, new Point2D(b_r - r_out, r_out), r_out, 1.5 * Math.PI, n_r);

            if (r_out != l)
            {
                //# add next two points
                contour.Add(new Point2D(b_r, l));
                contour.Add(new Point2D(b_r - t, l));
            }


            //  # construct the inner bottom right radius
            AddRadius(contour, new Point2D(b_r - t - r_in, t + r_in), r_in, 0, n_r, false);

            //  # construct the inner bottom left radius
            AddRadius(contour, new Point2D(t + r_in, t + r_in), r_in, 1.5 * Math.PI, n_r, false);

            //   # construct the outer top right radius
            AddRadius(contour, new Point2D(t - r_out, d - r_out), r_out, 0, n_r);

            // # construct the outer top left radius
            AddRadius(contour, new Point2D(t - b_l + r_out, d - r_out), r_out, 0.5 * Math.PI, n_r);

            if (r_out != l)
            {
                //# add the next two points
                contour.Add(new Point2D(t - b_l, d - l));
                contour.Add(new Point2D(t - b_l + t, d - l));
            }


            //   # construct the inner top left radius
            AddRadius(contour, new Point2D(2 * t - b_l + r_in, d - t - r_in), r_in, Math.PI, n_r, false);

            //  # construct the inner top right radius
            AddRadius(contour, new Point2D(-r_in, d - t - r_in), r_in, 0.5 * Math.PI, n_r, false);

            return contour;
        }

        public List<Point2D> CreateIShape(double d, double b, double t_f,
        double t_w, double r, int n_r)
        {
            var contour = new List<Point2D>();
            //# add first three points
            contour.Add(new Point2D(0, 0));
            contour.Add(new Point2D(b, 0));
            contour.Add(new Point2D(b, t_f));

            //# construct the bottom right radius
            var pt = new Point2D(b * 0.5 + t_w * 0.5 + r, t_f + r);
            AddRadius(contour, pt, r, 1.5 * Math.PI, n_r, false);

            //# construct the top right radius
            pt = new Point2D(b * 0.5 + t_w * 0.5 + r, d - t_f - r);
            AddRadius(contour, pt, r, Math.PI, n_r, false);

            //# add the next four points
            contour.Add(new Point2D(b, d - t_f));
            contour.Add(new Point2D(b, d));
            contour.Add(new Point2D(0, d));
            contour.Add(new Point2D(0, d - t_f));

            //# construct the top left radius
            pt = new Point2D(b * 0.5 - t_w * 0.5 - r, d - t_f - r);
            AddRadius(contour, pt, r, 0.5 * Math.PI, n_r, false);

            //# construct the bottom left radius
            pt = new Point2D(b * 0.5 - t_w * 0.5 - r, t_f + r);
            AddRadius(contour, pt, r, 0, n_r, false);

            //# add the last point
            contour.Add(new Point2D(0, t_f));


            return contour;
        }


        /// <summary>
        /// Constructs a regular hollow polygon section centered at *(0, 0)*, with   
        /// a pitch circle diameter of bounding polygon* d*, thickness *t*, number of
        /// sides* n_sides* and an optional inner radius* r_in*, using * n_r* points to
        /// construct the inner and outer radii(if radii is specified)
        /// </summary>
        /// <param name="d">Pitch circle diameter of the outer bounding polygon 
        /// (i.e. diameter of circle that passes through all vertices of the outer polygon)</param>
        /// <param name="t">Thickness of the polygon section wall</param>
        /// <param name="n_sides"></param>
        /// <param name="r_in">Inner radius of the polygon corners. By default, if not
        /// specified, a polygon with no corner radii is generated.</param>
        /// <param name="n_r">Number of points discretising the inner and outer radii,
        /// ignored if no inner radii is specified</param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public List<Point2D> CreatePolygonShape(double d, double t, int n_sides, double r_in = 0, int n_r = 1, double rot = 0)
        {
            return null;
        }

        /// <summary>
        /// Adds a quarter radius of points to the points list - centered at point pt.
        /// </summary>
        /// <param name="contour"></param>
        /// <param name="r">Radius</param>
        /// <param name="theta">starting at angle</param>
        /// <param name="n">Number of points</param>
        /// <param name="anti">Anticlockwise rotation</param>
        void AddRadius(List<Point2D> contour, Point2D pt, double r, double theta, int n, bool anti = true)
        {
            if (r == 0)
            {
                contour.Add(pt);
            }

            int mult = anti ? 1 : -1;

            //calculate radius of points
            for (int i = 0; i < n; i++)
            {
                //determine angle
                var t = theta + mult * i * 1.0 / Math.Max(1, n - 1) * Math.PI * 0.5;

                var x = pt.X + r * Math.Cos(t);
                var y = pt.Y + r * Math.Sin(t);
                contour.Add(new Point2D(x, y));
            }

        }
    }
}

