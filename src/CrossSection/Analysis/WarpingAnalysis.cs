// <copyright>
//https://github.com/IFahdah/CrossSection.Net

//Copyright(c) 2019 IFahdah

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
using System.Linq;
using CrossSection.DataModel;
using CrossSection.Triangulation;
using McritTorEngine.MathFun;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace CrossSection.Analysis
{
    public class WarpingAnalysis
    {
        class TriNode
        {
            public int Index { get; set; }
            public int VertexId1 { get; set; }
            public int VertexId2 { get; set; }

            public TriNode(int index, int id1, int id2)
            {
                Index = index;
                VertexId1 = id1;
                VertexId2 = id2;
            }

        }

        fea _fea = new fea();

        Mesh _mesh;
        SectionDefinition _sec;

        /// <summary>
        /// Calculates all the warping properties of the cross-section
        /// The following warping section properties are calculated:
        ///* Torsion constant
        ///* Shear centre
        ///* Shear area
        ///* Warping constant
        ///* Monosymmetry constant
        /// </summary>
        /// <param name="sec"></param>
        public void Solve(SectionDefinition sec)
        {

            _sec = sec;

            _mesh = _sec.Triangulate(_sec.BuildPolygon());

            var maxId = _mesh.Vertices.Max(x => x.ID);

            List<TriNode> nodes = BuildTriNodesList(_mesh);

            //  # assemble stiffness matrix and load vector for warping function
            (var K, var f_torsion) = assemble_torsion(sec, _mesh, nodes);

            CholeskyDecom Cholesky = new CholeskyDecom(K);

            //# solve for warping function
            var omega = Cholesky.Solve(f_torsion);
            sec.Output.SectionProperties.omega = omega.ToArray();

            //# determine the torsion constant
            sec.Output.SectionProperties.j = omega * K * omega;

            //======================# assemble shear function load vectors
            Vector f_psi =new Vector(nodes.Count);
            Vector f_phi = new Vector(nodes.Count);

            foreach (var item in _mesh.Triangles)
            {

                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                (var coords, var n0, var n1, var n2, var n3, var n4, var n5) = TriangleData(item, sec, nodes);

                var ixx_c = sec.Output.SectionProperties.ixx_c;
                var iyy_c = sec.Output.SectionProperties.iyy_c;
                var ixy_c = sec.Output.SectionProperties.ixy_c;
                (Vector f_psi, Vector f_phi) el = _fea.shear_load_vectors(mat, coords, ixx_c, iyy_c,
                    ixy_c, sec.Output.SectionProperties.nu_eff);

                var n = new[] { n0, n1, n2, n3, n4, n5 };

                for (int i = 0; i < 6; i++)
                {
                    f_psi[n[i]] += el.f_psi[i];
                    f_phi[n[i]] += el.f_phi[i];
                }
            }

            //# solve for shear functions psi and phi
            var psi_shear = Cholesky.Solve(f_psi);
            var phi_shear = Cholesky.Solve(f_phi);
            sec.Output.SectionProperties.psi_shear = psi_shear.ToArray();
            sec.Output.SectionProperties.phi_shear = phi_shear.ToArray();
            //\======================

            //======================# assemble shear centre and warping moment integrals
            var sc_xint = 0.0;
            var sc_yint = 0.0;
            var q_omega = 0.0;
            var i_omega = 0.0;
            var i_xomega = 0.0;
            var i_yomega = 0.0;

            foreach (var item in _mesh.Triangles)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                (var coords, var n0, var n1, var n2, var n3, var n4, var n5) = TriangleData(item, sec, nodes);

                var ixx_c = sec.Output.SectionProperties.ixx_c;
                var iyy_c = sec.Output.SectionProperties.iyy_c;
                var ixy_c = sec.Output.SectionProperties.ixy_c;

                var n = new[] { n0, n1, n2, n3, n4, n5 };

                Vector omega2 = new Vector(6);
                for (int i = 0; i < 6; i++)
                {
                    omega2[i] = omega[n[i]];
                }

                (double sc_xint, double sc_yint, double q_omega, double i_omega, double i_xomega, double i_yomega) el =
                    _fea.shear_warping_integrals(mat, coords, ixx_c, iyy_c, ixy_c, omega2);


                sc_xint += el.sc_xint;
                sc_yint += el.sc_yint;
                q_omega += el.q_omega;
                i_omega += el.i_omega;
                i_xomega += el.i_xomega;
                i_yomega += el.i_yomega;

            }

            //\======================

            //======================# calculate shear centres (elasticity approach)
            var Delta_s = 2 * (1 + sec.Output.SectionProperties.nu_eff) * (
            sec.Output.SectionProperties.ixx_c * sec.Output.SectionProperties.iyy_c -
             sec.Output.SectionProperties.ixy_c * sec.Output.SectionProperties.ixy_c);
            var x_se = (1 / Delta_s) * ((sec.Output.SectionProperties.nu_eff / 2 * sc_xint) -
                                    f_torsion * phi_shear);
            var y_se = (1 / Delta_s) * ((sec.Output.SectionProperties.nu_eff / 2 * sc_yint) +
                                     f_torsion * psi_shear);
            (double x11_se, double y22_se) = _fea.principal_coordinate(sec.Output.SectionProperties.phi,
                                            x_se, y_se);

            //# calculate shear centres (Trefftz's approach)
            var x_st = (sec.Output.SectionProperties.ixy_c *
                    i_xomega - sec.Output.SectionProperties.iyy_c * i_yomega) / (
                 sec.Output.SectionProperties.ixx_c * sec.Output.SectionProperties.iyy_c -
                 sec.Output.SectionProperties.ixy_c * sec.Output.SectionProperties.ixy_c);
            var y_st = (sec.Output.SectionProperties.ixx_c *
                      i_xomega - sec.Output.SectionProperties.ixy_c * i_yomega) / (
                  sec.Output.SectionProperties.ixx_c * sec.Output.SectionProperties.iyy_c -
                  sec.Output.SectionProperties.ixy_c * sec.Output.SectionProperties.ixy_c);

            sec.Output.SectionProperties.Delta_s = Delta_s;
            sec.Output.SectionProperties.x_se = x_se;
            sec.Output.SectionProperties.y_se = y_se;
            sec.Output.SectionProperties.x11_se = x11_se;
            sec.Output.SectionProperties.y22_se = y22_se;
            sec.Output.SectionProperties.x_st = x_st;
            sec.Output.SectionProperties.y_st = y_st;
            //\======================

            //# ======================calculate warping constant

            sec.Output.SectionProperties.gamma = (
            i_omega - q_omega * q_omega / sec.Output.SectionProperties.ea -
            y_se * i_xomega + x_se * i_yomega);
            //\======================


            //# ======================# assemble shear deformation coefficients
            var kappa_x = 0.0;
            var kappa_y = 0.0;
            var kappa_xy = 0.0;


            foreach (var item in _mesh.Triangles)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                (var coords, var n0, var n1, var n2, var n3, var n4, var n5) = TriangleData(item, sec, nodes);

                var ixx_c = sec.Output.SectionProperties.ixx_c;
                var iyy_c = sec.Output.SectionProperties.iyy_c;
                var ixy_c = sec.Output.SectionProperties.ixy_c;
                var nu_eff = sec.Output.SectionProperties.nu_eff;
                var n = new[] { n0, n1, n2, n3, n4, n5 };

                Vector psi_shear2 =new Vector(6);
                Vector phi_shear2 = new Vector(6);
                for (int i = 0; i < 6; i++)
                {
                    psi_shear2[i] = psi_shear[n[i]];
                    phi_shear2[i] = phi_shear[n[i]];
                }

       //         coords = Matrix<double>.Build.DenseOfArray( new double[,] { { 9.50372537, 6.85005177, 9.74078778, 8.17688857, 8.29541977,
       //9.62225658}, { -173.80193004, -175.62613327, -176.98904027, -174.71403166,
       //-176.30758677, -175.39548516} });
       //          ixx_c = 23459492268502.53;
       //          iyy_c = 8729239901048.53;
       //          ixy_c = -0.046875;
       //          nu_eff = sec.Output.SectionProperties.nu_eff;
       //         psi_shear2 = Vector<double>.Build.DenseOfArray(new[] { -3.070950077988491e+18, -3.063716348285501e+18, -2.82982854023284e+18, -3.067562639799091e+18, -2.957073412520266e+18, -2.932929755075249e+18 });
       //         phi_shear2 = Vector<double>.Build.DenseOfArray(new[] { -3.5507060616196666e+20, -3.546341445905855e+20, -3.558156029629006e+20, -3.5479764664843266e+20, -3.552366229858129e+20, -3.554702311099845e+20 });

                (var kappa_x_el, var kappa_y_el, var kappa_xy_el) =
                    _fea.shear_coefficients(mat, coords, ixx_c, iyy_c, ixy_c, psi_shear2, phi_shear2, nu_eff);


                kappa_x += kappa_x_el;
                kappa_y += kappa_y_el;
                kappa_xy += kappa_xy_el;

            }

            //# calculate shear areas wrt global axis
            sec.Output.SectionProperties.A_sx = Delta_s * Delta_s / kappa_x;
            sec.Output.SectionProperties.A_sy = Delta_s * Delta_s / kappa_y;
            sec.Output.SectionProperties.A_sxy = Delta_s * Delta_s / kappa_xy;

            //# calculate shear areas wrt principal bending axis:
            var alpha_xx = kappa_x * sec.Output.SectionProperties.Area / (Delta_s * Delta_s);
            var alpha_yy = kappa_y * sec.Output.SectionProperties.Area / (Delta_s * Delta_s);
            var alpha_xy = kappa_xy * sec.Output.SectionProperties.Area / (Delta_s * Delta_s);

            //# rotate the tensor by the principal axis angle
            var phi_rad = sec.Output.SectionProperties.phi * Math.PI / 180;
            Matrix R =new Matrix(new double[,]{ { Math.Cos (phi_rad), Math.Sin (phi_rad) },
                       { -Math.Sin (phi_rad), Math.Cos (phi_rad) } });

            Matrix alpha = new Matrix(new double[,]{ { alpha_xx, alpha_xy },
                       { alpha_xy, alpha_yy } });

            Matrix rotatedAlpha = R * alpha * R.Transpose;

            //# recalculate the shear area based on the rotated alpha value
            sec.Output.SectionProperties.A_s11 = sec.Output.SectionProperties.Area / rotatedAlpha[0, 0];
            sec.Output.SectionProperties.A_s22 = sec.Output.SectionProperties.Area / rotatedAlpha[1, 1];

            //\======================

            //======================  # calculate the monosymmetry consants

            var int_x = 0.0;
            var int_y = 0.0;
            var int_11 = 0.0;
            var int_22 = 0.0;
            foreach (var item in _mesh.Triangles)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                (var coords, var n0, var n1, var n2, var n3, var n4, var n5) = TriangleData(item, sec, nodes);


                var phi = sec.Output.SectionProperties.phi;
                var n = new[] { n0, n1, n2, n3, n4, n5 };

                Vector psi_shear2 =new Vector(6);
                Vector phi_shear2 = new Vector(6);
                for (int i = 0; i < 6; i++)
                {
                    psi_shear2[i] = psi_shear[n[i]];
                    phi_shear2[i] = phi_shear[n[i]];
                }

                (var int_x_el, var int_y_el, var int_11_el, var int_22_el) =
                    _fea.monosymmetry_integrals(mat, coords, phi);


                int_x += int_x_el;
                int_y += int_y_el;
                int_11 += int_11_el;
                int_22 += int_22_el;

            }

            //# calculate the monosymmetry constants
            sec.Output.SectionProperties.beta_x_plus = (
                 -int_x / sec.Output.SectionProperties.ixx_c + 2 * sec.Output.SectionProperties.y_se);
            sec.Output.SectionProperties.beta_x_minus = (
                 int_x / sec.Output.SectionProperties.ixx_c - 2 * sec.Output.SectionProperties.y_se);
            sec.Output.SectionProperties.beta_y_plus = (
                 -int_y / sec.Output.SectionProperties.iyy_c + 2 * sec.Output.SectionProperties.x_se);
            sec.Output.SectionProperties.beta_y_minus = (
                 int_y / sec.Output.SectionProperties.iyy_c - 2 * sec.Output.SectionProperties.x_se);
            sec.Output.SectionProperties.beta_11_plus = (
                 -int_11 / sec.Output.SectionProperties.i11_c + 2 * sec.Output.SectionProperties.y22_se);
            sec.Output.SectionProperties.beta_11_minus = (
                 int_11 / sec.Output.SectionProperties.i11_c - 2 * sec.Output.SectionProperties.y22_se);
            sec.Output.SectionProperties.beta_22_plus = (
                 -int_22 / sec.Output.SectionProperties.i22_c + 2 * sec.Output.SectionProperties.x11_se);
            sec.Output.SectionProperties.beta_22_minus = (
                 int_22 / sec.Output.SectionProperties.i22_c - 2 * sec.Output.SectionProperties.x11_se);
            //\======================
        }


        private (Matrix K, Vector f_torsion) assemble_torsion(SectionDefinition sec, Mesh mesh, List<TriNode> nodes)
        {
            Vector ff =new Vector(nodes.Count);
            Matrix kk = new Matrix(nodes.Count, nodes.Count);


            foreach (var item in mesh.Triangles)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                (var coords, var n0, var n1, var n2, var n3, var n4, var n5) = TriangleData(item, sec, nodes);
                //var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                //var e = mat.elastic_modulus;
                //var g = mat.shear_modulus;

                //Vertex v0 = item.GetVertex(0);
                //Vertex v1 = item.GetVertex(1);
                //Vertex v2 = item.GetVertex(2);

                ////Node 3 between 0,1
                ////Node 4 between 1,2
                ////Node 5 between 2,0

                //var x0 = item.GetVertex(0).X;
                //var x1 = item.GetVertex(1).X;
                //var x2 = item.GetVertex(2).X;
                //var y0 = item.GetVertex(0).Y;
                //var y1 = item.GetVertex(1).Y;
                //var y2 = item.GetVertex(2).Y;

                //Vertex v3 = new Vertex((x0 + x1) * 0.5, (y0 + y1) * 0.5);


                //var x3 = (x0 + x1) * 0.5;
                //var x4 = (x1 + x2) * 0.5;
                //var x5 = (x2 + x0) * 0.5;
                //var y3 = (y0 + y1) * 0.5;
                //var y4 = (y1 + y2) * 0.5;
                //var y5 = (y2 + y0) * 0.5;

                //Matrix<double> coords = M.DenseOfArray(new double[,]
                //{ { x0, x1, x2,  x3, x4, x5},
                //         {  y0,y1,y2, y3,y4,y5}, });



                // # calculate the element stiffness matrix and torsion load vector
                (Matrix k_el, Vector f_el) el = _fea.torsion_properties(mat, coords);

                //var n = new TriNode[6];
                //n[0] = FindNode(nodes, v0.ID, v0.ID);
                //n[1] = FindNode(nodes, v1.ID, v1.ID);
                //n[2] = FindNode(nodes, v2.ID, v2.ID);
                //n[3] = FindNode(nodes, v0.ID, v1.ID);
                //n[4] = FindNode(nodes, v1.ID, v2.ID);
                //n[5] = FindNode(nodes, v0.ID, v2.ID);

                var n = new[] { n0, n1, n2, n3, n4, n5 };

                ff[n0] += el.f_el[0];
                ff[n1] += el.f_el[1];
                ff[n2] += el.f_el[2];
                ff[n3] += el.f_el[3];
                ff[n4] += el.f_el[4];
                ff[n5] += el.f_el[5];

                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        kk[n[i], n[j]] += el.k_el[i, j];
                    }
                }
            }

            return (kk, ff);

        }

        private (Matrix coords, int n0, int n1, int n2, int n3, int n4, int n5)
            TriangleData(Triangle item, SectionDefinition sec, List<TriNode> nodes)
        {
           
            var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;

            var e = mat.elastic_modulus;
            var g = mat.shear_modulus;

            Vertex v0 = item.GetVertex(0);
            Vertex v1 = item.GetVertex(1);
            Vertex v2 = item.GetVertex(2);

            //Node 3 between 0,1
            //Node 4 between 1,2
            //Node 5 between 2,0

            var x0 = item.GetVertex(0).X;
            var x1 = item.GetVertex(1).X;
            var x2 = item.GetVertex(2).X;
            var y0 = item.GetVertex(0).Y;
            var y1 = item.GetVertex(1).Y;
            var y2 = item.GetVertex(2).Y;

            Vertex v3 = new Vertex((x0 + x1) * 0.5, (y0 + y1) * 0.5);


            var x3 = (x0 + x1) * 0.5;
            var x4 = (x1 + x2) * 0.5;
            var x5 = (x2 + x0) * 0.5;
            var y3 = (y0 + y1) * 0.5;
            var y4 = (y1 + y2) * 0.5;
            var y5 = (y2 + y0) * 0.5;

            Matrix coords = new Matrix(new double[,]
            { { x0, x1, x2,  x3, x4, x5},
                         {  y0,y1,y2, y3,y4,y5}, });


            var n = new TriNode[6];
            n[0] = FindNode(nodes, v0.ID, v0.ID);
            n[1] = FindNode(nodes, v1.ID, v1.ID);
            n[2] = FindNode(nodes, v2.ID, v2.ID);
            n[3] = FindNode(nodes, v0.ID, v1.ID);
            n[4] = FindNode(nodes, v1.ID, v2.ID);
            n[5] = FindNode(nodes, v0.ID, v2.ID);

            return (coords, n[0].Index, n[1].Index, n[2].Index, n[3].Index, n[4].Index, n[5].Index);

        }
        private static List<TriNode> BuildTriNodesList(Mesh mesh)
        {
            List<TriNode> nodes = new List<TriNode>();

            int index = 0;
            int index2 = 0;
            foreach (var item in mesh.Triangles)
            {
                Vertex v0 = item.GetVertex(0);
                Vertex v1 = item.GetVertex(1);
                Vertex v2 = item.GetVertex(2);
                if (!nodes.Any(n => v0.ID == n.VertexId1))
                {
                    nodes.Add(new TriNode(index++, v0.ID, v0.ID));
                }
                if (!nodes.Any(n => v1.ID == n.VertexId1))
                {
                    nodes.Add(new TriNode(index++, v1.ID, v1.ID));
                }
                if (!nodes.Any(n => v2.ID == n.VertexId1))
                {
                    nodes.Add(new TriNode(index++, v2.ID, v2.ID));
                }

                // Vertex v3 = new Vertex((x0 + x1) * 0.5, (y0 + y1) * 0.5);
                if (!nodes.Any(n => (n.VertexId1 == v0.ID && n.VertexId2 == v1.ID) || (n.VertexId1 == v1.ID && n.VertexId2 == v0.ID)))
                {
                    nodes.Add(new TriNode(index++, v0.ID, v1.ID));
                }
                else
                {
                    index2++;
                }
                if (!nodes.Any(n => (n.VertexId1 == v1.ID && n.VertexId2 == v2.ID) || (n.VertexId1 == v2.ID && n.VertexId2 == v1.ID)))
                {
                    nodes.Add(new TriNode(index++, v1.ID, v2.ID));
                }
                else
                {
                    index2++;
                }
                if (!nodes.Any(n => (n.VertexId1 == v2.ID && n.VertexId2 == v0.ID) || (n.VertexId1 == v0.ID && n.VertexId2 == v2.ID)))
                {
                    nodes.Add(new TriNode(index++, v0.ID, v2.ID));
                }
                else
                {
                    index2++;
                }
            }
            return nodes;
        }
        private static TriNode FindNode(List<TriNode> nodes, int id1, int id2)
        {
            return nodes.First(n => (n.VertexId1 == id1 && n.VertexId2 == id2) || (n.VertexId1 == id2 && n.VertexId2 == id1));
        }



    }
}
