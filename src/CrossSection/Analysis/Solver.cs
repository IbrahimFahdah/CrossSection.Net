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
using CrossSection.Triangulation;
using TriangleNet;

namespace CrossSection.Analysis
{
    public class Solver
    {
        private readonly fea _fea = new fea();

        public void Solve(SectionDefinition sec)
        {

            var mesh = sec.Triangulate();

            GeomAnalysis(sec, mesh);

            //# shift contours such that the origin is at the centroid
            sec.ShiftPoints(-sec.Output.SectionProperties.cx, -sec.Output.SectionProperties.cy);

            if(sec.SolutionSettings.RunPlasticAnalysis)
            {
                new PlasticAnalysis().Solve(sec);
            }
            if (sec.SolutionSettings.RunWarpingAnalysis)
            {
                new WarpingAnalysis().Solve(sec); 
            }

            // restore contours original location
            sec.ShiftPoints(sec.Output.SectionProperties.cx, sec.Output.SectionProperties.cy);
        }

        private void GeomAnalysis(SectionDefinition sec, Mesh mesh)
        {
            foreach (var item in mesh.Triangles)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                var e = mat.elastic_modulus;
                var g = mat.shear_modulus;

               var  ( area,  qx,  qy,  ixx,  iyy,  ixy) = _fea.geometric_properties(item.GetTriCoords());

                sec.Output.SectionProperties.Area += area;
                sec.Output.SectionProperties.ea += area * e;
                sec.Output.SectionProperties.ga += area * g;
                sec.Output.SectionProperties.qx += qx * e;
                sec.Output.SectionProperties.qy += qy * e;
                sec.Output.SectionProperties.ixx_g += ixx * e;
                sec.Output.SectionProperties.iyy_g += iyy * e;
                sec.Output.SectionProperties.ixy_g += ixy * e;
            }

            sec.Output.SectionProperties.nu_eff = sec.Output.SectionProperties.ea / (
                                                      2 * sec.Output.SectionProperties.ga) - 1;
            calculate_elastic_centroid(sec);
            calculate_centroidal_properties(sec, mesh);
        }

       private void calculate_centroidal_properties(SectionDefinition sec, Mesh mesh)
        {
            SectionProperties secPro = sec.Output.SectionProperties;

            /*Calculates the geometric section properties about the centroidal and
          principal axes based on the results about the global axis.
         */

            // calculate second moments of area about the centroidal xy axis
            secPro.ixx_c = secPro.ixx_g - secPro.qx * secPro.qx / secPro.ea;
            secPro.iyy_c = secPro.iyy_g - secPro.qy * secPro.qy / secPro.ea;
            secPro.ixy_c = secPro.ixy_g - secPro.qx * secPro.qy / secPro.ea;

            // calculate section moduli about the centroidal xy axis
            var xmax = mesh.Vertices.Max(t => t.X);
            var xmin = mesh.Vertices.Min(t => t.X);
            var ymax = mesh.Vertices.Max(t => t.Y);
            var ymin = mesh.Vertices.Min(t => t.Y);

            secPro.zxx_plus = secPro.ixx_c / Math.Abs(ymax - secPro.cy);
            secPro.zxx_minus = secPro.ixx_c / Math.Abs(ymin - secPro.cy);
            secPro.zyy_plus = secPro.iyy_c / Math.Abs(xmax - secPro.cx);
            secPro.zyy_minus = secPro.iyy_c / Math.Abs(xmin - secPro.cx);

            // calculate radii of gyration about centroidal xy axis
            secPro.rx_c = Math.Pow(secPro.ixx_c / secPro.ea, 0.5);
            secPro.ry_c = Math.Pow(secPro.iyy_c / secPro.ea, 0.5);

            // calculate prinicpal 2nd moments of area about the centroidal xy axis
            var Delta = Math.Pow(Math.Pow((secPro.ixx_c - secPro.iyy_c) / 2, 2) + secPro.ixy_c * secPro.ixy_c, 0.5);
            secPro.i11_c = (secPro.ixx_c + secPro.iyy_c) / 2 + Delta;
            secPro.i22_c = (secPro.ixx_c + secPro.iyy_c) / 2 - Delta;

            // calculate initial principal axis angle
            if (Math.Abs(secPro.ixx_c - secPro.i11_c) < 1e-12 * secPro.i11_c)
            {
                secPro.phi = 0;
            }
            else
            {
                secPro.phi = Math.Atan2(
                               secPro.ixx_c - secPro.i11_c, secPro.ixy_c) * 180 / Math.PI;
            }

            //// calculate section moduli about the principal axis
            var x1max = 0.0;
            var x1min = 0.0;
            var y2max = 0.0;
            var y2min = 0.0;
            var initialise = true;

            foreach (var pt in mesh.Vertices)
            {
                var x = pt.X - secPro.cx;
                var y = pt.Y - secPro.cy;
                // determine the coordinate of the point wrt the principal axis
                (double x1, double y2) = _fea.principal_coordinate(secPro.phi, x, y);

                //// initialise min, max variables
                if (initialise)
                {
                    x1max = x1;
                    x1min = x1;
                    y2max = y2;
                    y2min = y2;
                    initialise = false;
                }

                //// update the mins and maxs where necessary
                x1max = Math.Max(x1max, x1);
                x1min = Math.Min(x1min, x1);
                y2max = Math.Max(y2max, y2);
                y2min = Math.Min(y2min, y2);
            }


            // evaluate principal section moduli
            secPro.z11_plus = secPro.i11_c / Math.Abs(y2max);
            secPro.z11_minus = secPro.i11_c / Math.Abs(y2min);
            secPro.z22_plus = secPro.i22_c / Math.Abs(x1max);
            secPro.z22_minus = secPro.i22_c / Math.Abs(x1min);

            // calculate radii of gyration about centroidal principal axis
            secPro.r11_c = Math.Pow(secPro.i11_c / secPro.ea, 0.5);
            secPro.r22_c = Math.Pow(secPro.i22_c / secPro.ea, 0.5);
        }

        private void calculate_elastic_centroid(SectionDefinition sec)
        {
            SectionProperties sectionProperties = sec.Output.SectionProperties;
            sectionProperties.cx = sectionProperties.qy / sectionProperties.ea;
            sectionProperties.cy = sectionProperties.qx / sectionProperties.ea;
        }

    }
}