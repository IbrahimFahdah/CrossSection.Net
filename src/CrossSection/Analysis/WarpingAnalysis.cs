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
using System.Linq;
using CrossSection.DataModel;
using CrossSection.Maths;
using CrossSection.Triangulation;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace CrossSection.Analysis
{
    public partial class WarpingAnalysis
    {

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

            _mesh = _sec.Triangulate();

            var maxId = _mesh.Vertices.Max(x => x.ID);

            List<ExtendedTri> Tris = BuildTriNodesList(_mesh, out var nodeCount);


            //  # assemble stiffness matrix and load vector for warping function
            (var K, var f_torsion) = assemble_torsion(sec, _mesh, Tris, nodeCount);

            //solve Cholesky decomposition. 
            CholeskyDecomBase Cholesky = new CholeskyDecomOptimized(K);

            //# solve for warping function
            var omega = Cholesky.Solve(f_torsion);

            sec.Output.SectionProperties.omega = omega.ToArray();

            //# determine the torsion constant
            sec.Output.SectionProperties.j = sec.Output.SectionProperties.ixx_c + sec.Output.SectionProperties.iyy_c - omega.Dot(K).Dot(omega);

            //======================# assemble shear function load vectors
            var f_psi = new double[nodeCount];
            var f_phi = new double[nodeCount];

            int n0, n1, n2, n3, n4, n5;

            foreach (var item in Tris)
            {

                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                item.TriangleData(out n0, out n1, out n2, out n3, out n4, out n5);

                var ixx_c = sec.Output.SectionProperties.ixx_c;
                var iyy_c = sec.Output.SectionProperties.iyy_c;
                var ixy_c = sec.Output.SectionProperties.ixy_c;
                (Vector f_psi, Vector f_phi) el = _fea.shear_load_vectors(mat, item, ixx_c, iyy_c,
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

            foreach (var item in Tris)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                item.TriangleData(out n0, out n1, out n2, out n3, out n4, out n5);

                var ixx_c = sec.Output.SectionProperties.ixx_c;
                var iyy_c = sec.Output.SectionProperties.iyy_c;
                var ixy_c = sec.Output.SectionProperties.ixy_c;

                var n = new[] { n0, n1, n2, n3, n4, n5 };

                var omega2 = new double[6];
                for (int i = 0; i < 6; i++)
                {
                    omega2[i] = omega[n[i]];
                }

                (double sc_xint, double sc_yint, double q_omega, double i_omega, double i_xomega, double i_yomega) el =
                    _fea.shear_warping_integrals(mat, item, ixx_c, iyy_c, ixy_c, omega2);


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
                                     f_torsion.Dot(phi_shear));
            var y_se = (1 / Delta_s) * ((sec.Output.SectionProperties.nu_eff / 2 * sc_yint) +
                                     f_torsion.Dot(psi_shear));
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


            foreach (var item in Tris)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                item.TriangleData(out n0, out n1, out n2, out n3, out n4, out n5);

                var ixx_c = sec.Output.SectionProperties.ixx_c;
                var iyy_c = sec.Output.SectionProperties.iyy_c;
                var ixy_c = sec.Output.SectionProperties.ixy_c;
                var nu_eff = sec.Output.SectionProperties.nu_eff;
                var n = new[] { n0, n1, n2, n3, n4, n5 };

                var psi_shear2 = new double[6];
                var phi_shear2 = new double[6];
                for (int i = 0; i < 6; i++)
                {
                    psi_shear2[i] = psi_shear[n[i]];
                    phi_shear2[i] = phi_shear[n[i]];
                }

                (var kappa_x_el, var kappa_y_el, var kappa_xy_el) =
                    _fea.shear_coefficients(mat, item, ixx_c, iyy_c, ixy_c, psi_shear2, phi_shear2, nu_eff);


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
            var R = new double[,]{ { Math.Cos (phi_rad), Math.Sin (phi_rad) },
                       { -Math.Sin (phi_rad), Math.Cos (phi_rad) } };

            var alpha = new double[,]{ { alpha_xx, alpha_xy },
                       { alpha_xy, alpha_yy } };

            var rotatedAlpha = R.Dot(alpha).Dot(R.Transpose());

            //# recalculate the shear area based on the rotated alpha value
            sec.Output.SectionProperties.A_s11 = sec.Output.SectionProperties.Area / rotatedAlpha[0, 0];
            sec.Output.SectionProperties.A_s22 = sec.Output.SectionProperties.Area / rotatedAlpha[1, 1];

            //\======================

            //======================  # calculate the monosymmetry consants

            var int_x = 0.0;
            var int_y = 0.0;
            var int_11 = 0.0;
            var int_22 = 0.0;
            foreach (var item in Tris)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;
                item.TriangleData(out n0, out n1, out n2, out n3, out n4, out n5);


                var phi = sec.Output.SectionProperties.phi;
                var n = new[] { n0, n1, n2, n3, n4, n5 };

                (var int_x_el, var int_y_el, var int_11_el, var int_22_el) =
                    _fea.monosymmetry_integrals(mat, item, phi);


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


        private (double[,] K, double[] f_torsion) assemble_torsion(SectionDefinition sec, Mesh mesh, List<ExtendedTri> Tris, int nodeCount)
        {
            double[] ff = new double[nodeCount];
            double[,] kk = new double[nodeCount, nodeCount];

            //# initialise stiffness matrix and load vector
            double[,] k_el = null;
            double[] f_el = null;
            int n0, n1, n2, n3, n4, n5;

            foreach (var item in Tris)
            {
                var mat = sec.Contours.First(c => c.Material?.Id == item.Label).Material;

                item.TriangleData(out n0, out n1, out n2, out n3, out n4, out n5);

                // # calculate the element stiffness matrix and torsion load vector
                _fea.torsion_properties(mat, item, out k_el, out f_el);

                var n = new[] { n0, n1, n2, n3, n4, n5 };

                ff[n0] += f_el[0];
                ff[n1] += f_el[1];
                ff[n2] += f_el[2];
                ff[n3] += f_el[3];
                ff[n4] += f_el[4];
                ff[n5] += f_el[5];

                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        kk[n[i], n[j]] += k_el[i, j];
                    }
                }
            }

            return (kk, ff);

        }


        private List<ExtendedTri> BuildTriNodesList(Mesh mesh, out int nodeCount)
        {
            List<ExtendedTri> tri = new List<ExtendedTri>();
            List<TriNode> nodes = new List<TriNode>();

            int index = 0;
            foreach (var item in mesh.Triangles)
            {
                Vertex v0 = item.GetVertex(0);
                Vertex v1 = item.GetVertex(1);
                Vertex v2 = item.GetVertex(2);

                var n0 = nodes.FirstOrDefault(n => v0.ID == n.VertexId1);
                if (n0 == null)
                {
                    n0 = new TriNode(index++, v0.ID, v0.ID);
                    nodes.Add(n0);
                }

                var n1 = nodes.FirstOrDefault(n => v1.ID == n.VertexId1);
                if (n1 == null)
                {
                    n1 = new TriNode(index++, v1.ID, v1.ID);
                    nodes.Add(n1);
                }

                var n2 = nodes.FirstOrDefault(n => v2.ID == n.VertexId1);
                if (n2 == null)
                {
                    n2 = new TriNode(index++, v2.ID, v2.ID);
                    nodes.Add(n2);
                }

                //n3
                var n3 = nodes.FirstOrDefault(n => (n.VertexId1 == v0.ID && n.VertexId2 == v1.ID) || (n.VertexId1 == v1.ID && n.VertexId2 == v0.ID));
                if (n3 == null)
                {
                    n3 = new TriNode(index++, v0.ID, v1.ID);
                    nodes.Add(n3);
                }


                //n4
                var n4 = nodes.FirstOrDefault(n => (n.VertexId1 == v1.ID && n.VertexId2 == v2.ID) || (n.VertexId1 == v2.ID && n.VertexId2 == v1.ID));
                if (n4 == null)
                {
                    n4 = new TriNode(index++, v1.ID, v2.ID);
                    nodes.Add(n4);
                }


                //n5
                var n5 = nodes.FirstOrDefault(n => (n.VertexId1 == v2.ID && n.VertexId2 == v0.ID) || (n.VertexId1 == v0.ID && n.VertexId2 == v2.ID));
                if (n5 == null)
                {
                    n5 = new TriNode(index++, v0.ID, v2.ID);
                    nodes.Add(n5);
                }


                tri.Add(new ExtendedTri(item, new[] { n0, n1, n2, n3, n4, n5 }));
            }
            nodeCount = nodes.Count;
            return tri;
        }

    }
}
