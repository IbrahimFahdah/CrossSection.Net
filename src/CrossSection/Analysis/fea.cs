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
using CrossSection.Analysis;
using CrossSection.DataModel;
using CrossSection.Maths;
using CrossSection.LinearAlgebra;
namespace CrossSection
{
    public class fea
    {

        /// <summary>
        /// Calculates the geometric properties for the current finite element.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns>Tuple containing the geometric properties and the elastic
        /// and shear moduli of the element: *(area, qx, qy, ixx, iyy, ixy, e, g)</returns>
        public (double area, double qx, double qy, double ixx, double iyy, double ixy) geometric_properties(double[][] coords)
        {
            // initialise geometric properties
            var area = 0.0;
            var qx = 0.0;
            var qy = 0.0;
            var ixx = 0.0;
            var iyy = 0.0;
            var ixy = 0.0;

            //Gauss points for 6 point Gaussian integration
            var gps = ShapeFunctionHelper.gauss_points(6);
            Matrix B = null;
            double[,] BB;
            double[] N;
            double j;
            for (int i = 0; i < gps.RowCount(); i++)
            {
                var gp = gps.Row(i);

                ShapeFunctionHelper.shape_function(coords, gp, out N, out BB, out j);
                B = new Matrix(BB);
                area += gp[0] * j;

                var x = N.Dot(coords.Row(1));
                var y = N.Dot(coords.Row(0));
                qx += gp[0] * x * j;
                qy += gp[0] * y * j;
                ixx += gp[0] * x * x * j;
                iyy += gp[0] * y * y * j;
                ixy += gp[0] * x * y * j;
            }

            return (area, qx, qy, ixx, iyy, ixy);
        }

        /// <summary>
        /// Calculates total force resisted by the element when subjected to a
        /// stress equal to the yield strength. Also returns the modulus weighted
        /// area and first moments of area, and determines whether or not the
        /// element is above or below the line defined by the unit vector *u* and
        /// point* p*.
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="coords"></param>
        /// <param name="u">Unit vector in the direction of the line</param>
        /// <param name="p">Point on the line</param>
        /// <returns></returns>
        public (double f_el, double ea_el, double qx_el, double qy_el, bool is_above) plastic_properties(SectionMaterial mat,
               double[][] coords, double[] u, double[] p)
        {
            //# initialise geometric properties
            var e = mat.elastic_modulus;
            var area = 0.0;
            var qx = 0.0;
            var qy = 0.0;
            var force = 0.0;

            var gps = ShapeFunctionHelper.gauss_points(3);
            Matrix B = null;
            double[] N;
            double j;
            double[,] BB;
            for (int i = 0; i < gps.RowCount(); i++)
            {
                var gp = gps.Row(i);

                ShapeFunctionHelper.shape_function(coords, gp, out N, out BB, out j);
                B = new Matrix(BB);
                area += gp[0] * j;

                var x = N.Dot(coords.Row(1));
                var y = N.Dot(coords.Row(0));
                qx += gp[0] * x * j;
                qy += gp[0] * y * j;
                force += gp[0] * j * mat.yield_strength;
            }

            //# calculate element centroid
            var cx = qy / area;
            var cy = qx / area;

            //# determine if the element is above the line p + u
            bool is_above = point_above_line(u, p[0], p[1], cx, cy);
            return (force, area * e, qx * e, qy * e, is_above);
        }

        /// <summary>
        ///Calculates the element stiffness matrix used for warping analysis
        ///and the torsion load vector.
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="coords"></param>
        /// <returns>Element stiffness matrix *(k_el)* and element torsion load vector* (f_el) *</returns>
        internal void torsion_properties(SectionMaterial mat, ExtendedTri tri, out double[,] k_el, out double[] f_el)
        {
            //# initialise stiffness matrix and load vector
            k_el = new double[6, 6];
            f_el = new double[6];


            //# Gauss points for 6 point Gaussian integration
            var gps = ShapeFunctionHelper.gauss_points(6);
            var Nxy = new double[2];

            for (int i = 0; i < gps.RowCount(); i++)
            {
                var gp = gps.Row(i);

                var B = tri.ShapeInfo[i].B;
                double[] N = tri.ShapeInfo[i].N;
                double j = tri.ShapeInfo[i].j;

                //# determine x and y position at Gauss point
                var Nx = N.Dot(tri.coords.Row(0));
                var Ny = N.Dot(tri.coords.Row(1));
                var B_Transpose = B.Transpose();

                //# calculated modulus weighted stiffness matrix and load vector
                k_el.Append(B_Transpose.Dot(B).Dot(gp[0] * j * mat.elastic_modulus));
                Nxy[0] = Ny;
                Nxy[1] = -Nx;
                f_el.Append(B_Transpose.Dot(Nxy).Dot(gp[0] * j * mat.elastic_modulus));
            }
        }

        /// <summary>
        /// Calculates the element shear load vectors used to evaluate the shear         functions.
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="coords"></param>
        /// <param name="ixx">Second moment of area about the centroidal x-axis</param>
        /// <param name="iyy">Second moment of area about the centroidal y-axis</param>
        /// <param name="ixy">Second moment of area about the centroidal xy-axis</param>
        /// <param name="nu">Effective Poisson's ratio for the cross-section</param>
        /// <returns>Element shear load vector psi *(f_psi)* and phi *(f_phi)*</returns>
        internal (double[] f_psi, double[] f_phi) shear_load_vectors(SectionMaterial mat,
            ExtendedTri tri, double ixx, double iyy, double ixy, double nu)
        {
            //# initialise stiffness matrix and load vector
            var f_psi = new double[6];
            var f_phi = new double[6];

            //# Gauss points for 6 point Gaussian integration
            var gps = ShapeFunctionHelper.gauss_points(6);

            var d = new double[2];
            var h = new double[2];

            for (int i = 0; i < gps.RowCount(); i++)
            {
                var gp = gps.Row(i);

                // shape_function(coords, gp, out N, ref B, out j);
                var B = tri.ShapeInfo[i].B;
                double[] N = tri.ShapeInfo[i].N;
                double j = tri.ShapeInfo[i].j;

                //# determine x and y position at Gauss point
                var Nx = N.Dot(tri.coords.Row(0));
                var Ny = N.Dot(tri.coords.Row(1));

                //# determine shear parameters
                var r = Nx * Nx - Ny * Ny;
                var q = 2 * Nx * Ny;
                var d1 = ixx * r - ixy * q;
                var d2 = ixy * r + ixx * q;
                var h1 = -ixy * r + iyy * q;
                var h2 = -iyy * r - ixy * q;

                //d[0, 0] = d1;
                //d[1, 0] = d2;
                //h[0, 0] = h1;
                //h[1, 0] = h2;

                //var B_Transpose = B.Transpose;

                //Vector tmp = new Vector(N);
                //tmp *= 2 * (1 + nu);

                //f_psi += gp[0] * (nu / 2 * (B_Transpose * d).Transpose.Row(0) +
                //                 tmp * (ixx * Nx - ixy * Ny)) * j * mat.elastic_modulus;

                //f_phi += gp[0] * (nu / 2 * (B_Transpose * h).Transpose.Row(0) +
                //                 tmp * (iyy * Ny - ixy * Nx)) * j * mat.elastic_modulus;

                d[0] = d1;
                d[1] = d2;
                h[0] = h1;
                h[1] = h2;

                var B_Transpose = B.Transpose();

                var NN = N.Dot(2 * (1 + nu));

                f_psi.Append(B_Transpose.Dot(d).Dot(nu / 2).Append(NN.Dot(ixx * Nx - ixy * Ny)).Dot(gp[0] * j * mat.elastic_modulus));
                f_phi.Append(B_Transpose.Dot(h).Dot(nu / 2).Append(NN.Dot(iyy * Ny - ixy * Nx)).Dot(gp[0] * j * mat.elastic_modulus));

            }

            return (f_psi, f_phi);
        }

        /// <summary>
        /// Calculates the element shear centre and warping integrals required for shear analysis of the cross-section.
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="coords"></param>
        /// <param name="ixx">Second moment of area about the centroidal x-axis</param>
        /// <param name="iyy">Second moment of area about the centroidal y-axis</param>
        /// <param name="ixy">Second moment of area about the centroidal xy-axis</param>
        /// <param name="omega">Values of the warping function at the element nodes</param>
        /// <returns>Shear centre integrals about the x and y-axes *(sc_xint, sc_yint)*,
        /// warping integrals *(q_omega, i_omega, i_xomega, i_yomega)*</returns>
        internal (double sc_xint, double sc_yint, double q_omega, double i_omega, double i_xomega, double i_yomega)
            shear_warping_integrals(SectionMaterial mat, ExtendedTri tri, double ixx, double iyy, double ixy, double[] omega)
        {
            //# initialise integrals
            var sc_xint = 0.0;
            var sc_yint = 0.0;
            var q_omega = 0.0;
            var i_omega = 0.0;
            var i_xomega = 0.0;
            var i_yomega = 0.0;

            var gps = ShapeFunctionHelper.gauss_points(6);


            for (int i = 0; i < gps.RowCount(); i++)
            {
                var gp = gps.Row(i);

                //shape_function(coords, gp, out N, ref B, out j);
                Matrix B = new Matrix(tri.ShapeInfo[i].B);
                double[] N = tri.ShapeInfo[i].N;
                double j = tri.ShapeInfo[i].j;

                //# determine x and y position at Gauss point
                var Nx = N.Dot(tri.coords.Row(0));
                var Ny = N.Dot(tri.coords.Row(1));

                var Nomega = N.Dot(omega);

                sc_xint += gp[0] * (iyy * Nx + ixy * Ny) * (Nx * Nx + Ny * Ny) * j * mat.elastic_modulus;
                sc_yint += gp[0] * (ixx * Ny + ixy * Nx) * (Nx * Nx + Ny * Ny) * j * mat.elastic_modulus;
                q_omega += gp[0] * Nomega * j * mat.elastic_modulus;
                i_omega += gp[0] * Nomega * Nomega * j * mat.elastic_modulus;
                i_xomega += gp[0] * Nx * Nomega * j * mat.elastic_modulus;
                i_yomega += gp[0] * Ny * Nomega * j * mat.elastic_modulus;
            }

            return (sc_xint, sc_yint, q_omega, i_omega, i_xomega, i_yomega);
        }


        /// <summary>
        /// Calculates the variables used to determine the shear deformation coefficients.
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="coords"></param>
        /// <param name="ixx">Second moment of area about the centroidal x-axis</param>
        /// <param name="iyy">Second moment of area about the centroidal y-axis</param>
        /// <param name="ixy">Second moment of area about the centroidal xy-axis</param>
        /// <param name="psi_shear">Values of the psi shear function at the element nodes</param>
        /// <param name="phi_shear">Values of the phi shear function at the element nodes</param>
        /// <param name="nu">Effective Poisson's ratio for the cross-section</param>
        /// <returns></returns>
        internal (double kappa_x, double kappa_y, double kappa_xy) shear_coefficients(SectionMaterial mat,
           ExtendedTri tri, double ixx, double iyy, double ixy, double[] psi_shear2, double[] phi_shear2, double nu)
        {
            //# initialise integrals
            var kappa_x = 0.0;
            var kappa_y = 0.0;
            var kappa_xy = 0.0;

            var gps = ShapeFunctionHelper.gauss_points(6);
            Vector d = new Vector(2);
            Vector h = new Vector(2);
            var psi_shear = new Vector(psi_shear2);
            var phi_shear = new Vector(phi_shear2);

            for (int i = 0; i < gps.RowCount(); i++)
            {
                var gp = gps.Row(i);

                //shape_function(coords, gp, out N, ref B, out j);
                Matrix B = new Matrix(tri.ShapeInfo[i].B);
                double[] N = tri.ShapeInfo[i].N;
                double j = tri.ShapeInfo[i].j;

                //# determine x and y position at Gauss point
                var Nx = N.Dot(tri.coords.Row(0));
                var Ny = N.Dot(tri.coords.Row(1));

                //# determine shear parameters
                var r = Nx * Nx - Ny * Ny;
                var q = 2 * Nx * Ny;
                var d1 = ixx * r - ixy * q;
                var d2 = ixy * r + ixx * q;
                var h1 = -ixy * r + iyy * q;
                var h2 = -iyy * r - ixy * q;

                d[0] = d1;
                d[1] = d2;
                h[0] = h1;
                h[1] = h2;

                var B_Transpose = B.Transpose;

                var psi_shearXB_Transpose = psi_shear * B_Transpose - nu / 2 * d;
                var BXphi_shear = (B * phi_shear - nu / 2 * h);

                kappa_x += (gp[0] * (psi_shearXB_Transpose)
                  * (B * psi_shear - nu / 2 * d) * j * mat.elastic_modulus);

                kappa_y += (gp[0] * (phi_shear * B_Transpose - nu / 2 * h)
                    * (BXphi_shear) * j * mat.elastic_modulus);

                kappa_xy += (gp[0] * (psi_shearXB_Transpose)
                  * (BXphi_shear) * j * mat.elastic_modulus);

            }

            return (kappa_x, kappa_y, kappa_xy);
        }

        /// <summary>
        /// Calculates the integrals used to evaluate the monosymmetry constant about both global axes and both prinicipal axes.
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="coords"></param>
        /// <param name="phi">Principal bending axis angle</param>
        /// <returns></returns>
        internal (double int_x, double int_y, double int_11, double int_22) monosymmetry_integrals(SectionMaterial mat, ExtendedTri tri, double phi)
        {
            //# initialise integrals
            var int_x = 0.0;
            var int_y = 0.0;
            var int_11 = 0.0;
            var int_22 = 0.0;

            var gps = ShapeFunctionHelper.gauss_points(6);


            for (int i = 0; i < gps.RowCount(); i++)
            {
                var gp = gps.Row(i);

                double[] N = tri.ShapeInfo[i].N;
                double j = tri.ShapeInfo[i].j;

                //# determine x and y position at Gauss point
                var Nx = N.Dot(tri.coords.Row(0));
                var Ny = N.Dot(tri.coords.Row(1));

                //# determine 11 and 22 position at Gauss point
                (var Nx_11, var Ny_22) = principal_coordinate(phi, Nx, Ny);

                //# weight the monosymmetry integrals by the section elastic modulus
                int_x += (gp[0] * (Nx * Nx * Ny + Ny * Ny * Ny) * j * mat.elastic_modulus);
                int_y += (gp[0] * (Ny * Ny * Nx + Nx * Nx * Nx) * j * mat.elastic_modulus);
                int_11 += (gp[0] * (Nx_11 * Nx_11 * Ny_22 + Ny_22 * Ny_22 * Ny_22) * j * mat.elastic_modulus);
                int_22 += (gp[0] * (Ny_22 * Ny_22 * Nx_11 + Nx_11 * Nx_11 * Nx_11) * j * mat.elastic_modulus);


            }

            return (int_x, int_y, int_11, int_22);
        }

        /// <summary>
        /// Determines the coordinates of the cartesian point *(x, y)* in the principal axis system given an axis rotation angle phi
        /// </summary>
        /// <param name="phi">Prinicpal bending axis angle (degrees)</param>
        /// <param name="x">x coordinate in the global axis</param>
        /// <param name="y">y coordinate in the global axis</param>
        /// <returns>Principal axis coordinates *(x1, y1)*</returns>
        public (double x2, double y2) principal_coordinate(double phi, double x, double y)
        {
            // convert principal axis angle to radians
            var phi_rad = phi * Math.PI / 180;

            //form rotation matrix
            var R = new double[,]
                                   {
                                       {Math.Cos(phi_rad), Math.Sin(phi_rad)},
                                       {-Math.Sin(phi_rad), Math.Cos(phi_rad)}
                                   };

            var a = new double[,] { { x }, { y } };

            //calculate rotated x and y coordinates
            var rotated = R.Dot(a);
            return (rotated[0, 0], rotated[1, 0]);
        }



        /// <summary>
        /// Determines whether a point *(x, y)* is a above or below the line defined
        /// by the parallel unit vector* u* and the point* (px, py) *.
        /// </summary>
        /// <param name="u">Unit vector parallel to the line [1 x 2]</param>
        /// <param name="px">x coordinate of a point on the line</param>
        /// <param name="py">y coordinate of a point on the line</param>
        /// <param name="x">x coordinate of the point to be tested</param>
        /// <param name="y">y coordinate of the point to be tested</param>
        /// <returns> *True* if the point is above the line or *False* if the point is below the line</returns>
        private bool point_above_line(double[] u, double px, double py, double x, double y)
        {
            //# vector from point to point on line
            var PQ = new[] { px - x, py - y };
            return PQ[0] * u[1] - PQ[1] * u[0] > 0;
        }
    }
}