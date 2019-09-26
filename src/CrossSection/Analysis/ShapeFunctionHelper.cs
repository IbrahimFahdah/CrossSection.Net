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
using System.Collections.Concurrent;
using CrossSection.Maths;
namespace CrossSection.Analysis
{
    class ShapeFunctionHelper
    {
        internal static readonly ConcurrentDictionary<int, double[,]> _gauss_points = new ConcurrentDictionary<int, double[,]>();

        private readonly static double[,] tmp =  
                                          {
                                                        {0, 0},
                                                        {1, 0},
                                                        {0, 1}
                                          };

        private readonly static double[] J_upper = new[] { 1.0, 1.0, 1.0 };

        /// <summary>
        /// Computes shape functions, shape function derivatives and the determinant
        /// of the Jacobian matrix for a tri 6 element at a given Gauss point.
        /// </summary>
        /// <param name="coords">Global coordinates of the quadratic triangle vertices</param>
        /// <param name="gauss_point">Gaussian weight and isoparametric location of the Gauss point</param>
        /// <param name="N">The value of the shape functions at the given Gauss point [1 x 6]</param>
        /// <param name="B">the derivative of the shape functions in the j-th global direction* B(i, j)* [2 x 6]</param>
        /// <param name="j">the determinant of the Jacobian matrix *j*</param>
        internal static void shape_function(double[][] coords, double[] gauss_point, out double[] N, out double[,] B, out double j)
        {
            // location of isoparametric co-ordinates for each Gauss point
            var eta = gauss_point[1];
            var xi = gauss_point[2];
            var zeta = gauss_point[3];


            N = new[]
                        {
                            eta * (2 * eta - 1),
                            xi * (2 * xi - 1),
                            zeta * (2 * zeta - 1),
                            4 * eta * xi,
                            4 * xi * zeta,
                            4 * eta * zeta
                        };


            var B_iso = new double[,]
                                                 {
                                                   {4 * eta - 1, 0, 0, 4 * xi, 0, 4 * zeta},
                                                    {0, 4 * xi - 1, 0, 4 * eta, 4 * zeta, 0},
                                                  {0, 0, 4 * zeta - 1, 0, 4 * xi, 4 * eta}
                                                 };

            var B_iso_Transpose = B_iso.Transpose();

            var J_lower = coords.Dot(B_iso_Transpose);


            var J = new double[3,3];
            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if(i==0)
                    {
                        J[0, k] = J_upper[k];
                        continue;
                    }
                    J[i, k] = J_lower[i-1][k];
                }
            }

            var det = 0.0;
            for (int i = 0; i < 3; i++)
                det = det + (J[0, i] * (J[1, (i + 1) % 3] * J[2, (i + 2) % 3] - J[1, (i + 2) % 3] * J[2, (i + 1) % 3]));

            //calculate the jacobian
            j = 0.5 * det;// J.Determinant;

            B = new double[2, 6];

            if (j != 0)
            {
                //#if the area of the element is not zero
                //# cacluate the P matrix

                var P =InvertJ(J).Dot(tmp);

                //# calculate the B matrix in terms of cartesian co-ordinates
                B=(B_iso_Transpose.Dot(P)).Transpose();
            }

         
        }


        private static double[,] InvertJ(double[,] m)
        {
            // computes the inverse of a matrix m
            double det = m[0, 0] * (m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2]) -
                         m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
                         m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);

            double invdet = 1 / det;

            double[,] minv=new double[3,3]; // inverse of matrix m
            minv[0, 0] = (m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2]) * invdet;
            minv[0, 1] = (m[0, 2] * m[2, 1] - m[0, 1] * m[2, 2]) * invdet;
            minv[0, 2] = (m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1]) * invdet;
            minv[1, 0] = (m[1, 2] * m[2, 0] - m[1, 0] * m[2, 2]) * invdet;
            minv[1, 1] = (m[0, 0] * m[2, 2] - m[0, 2] * m[2, 0]) * invdet;
            minv[1, 2] = (m[1, 0] * m[0, 2] - m[0, 0] * m[1, 2]) * invdet;
            minv[2, 0] = (m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1]) * invdet;
            minv[2, 1] = (m[2, 0] * m[0, 1] - m[0, 0] * m[2, 1]) * invdet;
            minv[2, 2] = (m[0, 0] * m[1, 1] - m[1, 0] * m[0, 1]) * invdet;
            return minv;
        }

        /// <summary>
        /// the Gaussian weights and locations for *n* point Gaussian integration of a quadratic triangular element.
        /// </summary>
        /// <param name="n"> Number of Gauss points (1, 3 or 6)</param>
        /// <returns>An *n x 4* matrix consisting of the integration weight and the
        /// eta, xi and zeta locations for *n* Gauss points</returns>
        internal static double[,] gauss_points(int n)
        {
            double[,] x = null;

            if (_gauss_points.ContainsKey(n))
            {
                return _gauss_points[n];
            }

            if (n == 1)

            {
                // one point gaussian integration
                x = new double[,] { { 1, 1.0 / 3, 1.0 / 3, 1.0 / 3 } };
            }

            if (n == 3)
            {
                // three point gaussian integration
                x = new double[,]
                    {
                        {
                            1.0 / 3, 2.0 / 3, 1.0 / 6, 1.0 / 6
                        },
                        {1.0 / 3, 1.0 / 6, 2.0 / 3, 1.0 / 6},
                        {1.0 / 3, 1.0 / 6, 1.0 / 6, 2.0 / 3}
                    };
            }

            if (n == 6)
            {
                // six point gaussian integration
                var g1 = 1.0 / 18 * (8 - Math.Sqrt(10) + Math.Sqrt(38 - 44 * Math.Sqrt(2.0 / 5)));
                var g2 = 1.0 / 18 * (8 - Math.Sqrt(10) - Math.Sqrt(38 - 44 * Math.Sqrt(2.0 / 5)));
                var w1 = (620 + Math.Sqrt(213125 - 53320 * Math.Sqrt(10))) / 3720;
                var w2 = (620 - Math.Sqrt(213125 - 53320 * Math.Sqrt(10))) / 3720;

                x = new double[,]
                    {
                        {w2, 1 - 2 * g2, g2, g2},
                        {w2, g2, 1 - 2 * g2, g2},
                        {w2, g2, g2, 1 - 2 * g2},
                        {w1, g1, g1, 1 - 2 * g1},
                        {w1, 1 - 2 * g1, g1, g1},
                        {w1, g1, 1 - 2 * g1, g1}
                    };
            }



            _gauss_points.TryAdd(n, x);
            return x;
        }
    }
}
