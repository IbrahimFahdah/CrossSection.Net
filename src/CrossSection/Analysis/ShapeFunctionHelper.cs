using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CrossSection.Maths;
using CrossSection.Triangulation;
using TriangleNet.Topology;
namespace CrossSection.Analysis
{
    class ShapeFunctionHelper
    {
        internal static readonly ConcurrentDictionary<int, double[,]> _gauss_points = new ConcurrentDictionary<int, double[,]>();

        private readonly static Matrix tmp = new Matrix(new double[,]
                                          {
                                                        {0, 0},
                                                        {1, 0},
                                                        {0, 1}
                                          });

        private readonly static Vector J_upper = new Vector(new[] { 1.0, 1.0, 1.0 });

        /// <summary>
        /// Computes shape functions, shape function derivatives and the determinant
        /// of the Jacobian matrix for a tri 6 element at a given Gauss point.
        /// </summary>
        /// <param name="coords">Global coordinates of the quadratic triangle vertices</param>
        /// <param name="gauss_point">Gaussian weight and isoparametric location of the Gauss point</param>
        /// <param name="N">The value of the shape functions at the given Gauss point [1 x 6]</param>
        /// <param name="B">the derivative of the shape functions in the j-th global direction* B(i, j)* [2 x 6]</param>
        /// <param name="j">the determinant of the Jacobian matrix *j*</param>
        internal static void shape_function(double[][] coords, double[] gauss_point, out double[] N, out Matrix B, out double j)
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


            var B_iso = new double[][]
                                                 {
                                                 new double[]     {4 * eta - 1, 0, 0, 4 * xi, 0, 4 * zeta},
                                                  new double[]      {0, 4 * xi - 1, 0, 4 * eta, 4 * zeta, 0},
                                                  new double[]      {0, 0, 4 * zeta - 1, 0, 4 * xi, 4 * eta}
                                                 };

            var B_iso_Transpose = B_iso.Transpose();

            var J_lower = coords.Dot(B_iso_Transpose);

            var rows = new double[3][];
            rows[0] = J_upper.ToArray();
            for (int i = 0; i < 2; i++)
            {
                rows[i + 1] = J_lower.Row(i);
            }

            Matrix J = new Matrix(rows);

            //calculate the jacobian
            j = 0.5 * J.Determinant;

            B = new Matrix(2, 6);

            if (j != 0)
            {
                //#if the area of the element is not zero
                //# cacluate the P matrix

                var P = J.Inverse * tmp;

                //# calculate the B matrix in terms of cartesian co-ordinates
                B.Append((new Matrix(B_iso_Transpose) * P).Transpose);
            }

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
