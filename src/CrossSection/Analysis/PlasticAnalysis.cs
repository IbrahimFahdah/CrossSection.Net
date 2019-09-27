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
using System.Threading.Tasks;
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

        public void Solve(SectionDefinition sec, Mesh mesh)
        {
            _sec = sec;
            _mesh = mesh;

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
           var u2 = new[] { 0, 1.0 };
                var fibres_y = new[] { fibres.u_min, fibres.u_max };
                   var u = new[] { 1.0, 0 };
                    var fibres_x = new[] { fibres.v_min, fibres.v_max };

         
             var   t1 = Task.Run(() =>
                {
                //  # 1a) Calculate x-axis plastic centroid
                  (double y_pc, bool r, double f, (double x, double y) c_top, (double x, double y) c_bot) =
                        new pc_algorithm(sec, mesh).Run(u, fibres_x, 1);

                    secPro.y_pc = y_pc;
                    secPro.sxx = f * Math.Abs(c_top.y - c_bot.y);
                });
            
            var t2 = Task.Run(() =>
            {
                //# 1b) Calculate y-axis plastic centroid
                  (double x_pc, bool r2, double f2, (double x, double y2) c_top2, (double x, double y) c_bot2) =
                    new pc_algorithm(sec, mesh).Run(u2, fibres_y, 2);
                
                    secPro.x_pc = x_pc;
                    secPro.syy = f2 * Math.Abs(c_top2.x - c_bot2.x);
              
            });

            //# 2) Calculate plastic properties for principal axis
            //# convert principal axis angle to radians
            var angle = secPro.phi * Math.PI / 180;

            //# unit vectors in the axis directions
            var ux = new[] { Math.Cos(angle), Math.Sin(angle) };
            var uy = new[] { -Math.Sin(angle), Math.Cos(angle) };

            fibres = calculate_extreme_fibres(sec, mesh, secPro.phi);
            var t3 = Task.Run(() =>
            {
                //# 2a) Calculate 11-axis plastic centroid
                var fibres_x2 = new[] { fibres.v_min, fibres.v_max };
                (double y22_pc, bool r11, double f11, (double x, double y) c_top11, (double x, double y) c_bot11) =
                    new pc_algorithm(sec, mesh).Run(ux, fibres_x2, 1);


                //# calculate the centroids in the principal coordinate system
                var c_top_p11 = _fea.principal_coordinate(secPro.phi, c_top11.x, c_top11.y);
                var c_bot_p11 = _fea.principal_coordinate(secPro.phi, c_bot11.x, c_bot11.y);

                //self.check_convergence(r, '11-axis')
                secPro.y22_pc = y22_pc;
                secPro.s11 = f11 * Math.Abs(c_top_p11.y2 - c_bot_p11.y2);
            });

             var t4 = Task.Run(() =>
            {
                //# 2b) Calculate 22-axis plastic centroid
                var fibres_y2 = new[] { fibres.u_min, fibres.u_max };
                (double x11_pc, bool r22, double f22, (double x, double y) c_top22, (double x, double y) c_bot22) =
                    new pc_algorithm(sec, mesh).Run(uy, fibres_y2, 2);

                // # calculate the centroids in the principal coordinate system
                var c_top_p22 = _fea.principal_coordinate(secPro.phi, c_top22.x, c_top22.y);
                var c_bot_p22 = _fea.principal_coordinate(secPro.phi, c_bot22.x, c_bot22.y);

                //self.check_convergence(r, '22-axis')
                secPro.x11_pc = x11_pc;
                secPro.s22 = f22 * Math.Abs(c_top_p22.x2 - c_bot_p22.x2);
            });

           Task.WaitAll(new []{ t1, t2,t3, t4 });
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


     
    }
}