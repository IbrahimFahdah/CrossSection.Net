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
using System.Diagnostics;

namespace CrossSection.Maths
{
    /// <summary>Cholesky Decomposition.</summary>
    /// <remarks>
    /// For a symmetric, positive definite matrix A, the Cholesky decomposition
    /// is an lower triangular matrix L so that A = L*L'.
    /// </remarks>
    public class CholeskyDecomOptimized : CholeskyDecomBase
    {

        /// <summary>Cholesky algorithm for symmetric and positive definite matrix.
        /// Optimized to improve performance. This is mainly done by skipping the zeros values since the matrix is a sparse matrix, and also rearranging some bits.</summary>
        /// <param name="Arg">Square, symmetric matrix.</param>
        /// <returns>Structure to access L and isspd flag.</returns>
        public CholeskyDecomOptimized(double[,] Arg)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            CholeskySparseMatrix Rows = new CholeskySparseMatrix(Arg);

            var sum2 = 0.0;
            var lastNonZero = -1;
            var firstNonZero = -1;
            //var k_ind = 0;
            int i, j;
            int n = Arg.RowCount();

            for (i = 0; i < n; i++)
            {
                double[] Rik = new double[i];
              
                lastNonZero = -1;
                firstNonZero = -1;

                var Vi = Rows[i];
                var sum = Vi[i];
                for (var k = Vi.ValueCount-1; k >= 0; k--)
                {
                    ref var k_ind = ref Vi.Indices[k];
                    if (k_ind > i - 1)
                    {
                        continue;
                    }

                    var Lik = Vi.Values[k];
                    sum -= Lik * Lik;

                    Rik[k_ind] = Lik;
                    if (Lik != 0)
                    {
                        firstNonZero  = k_ind;
                        if (lastNonZero == -1)
                        {
                            lastNonZero = k_ind;
                        }
                    }
                }

                if (sum <= 0.0) //A, with rounding errors, is not positive-definite.
                {
                 throw new System.SystemException("Cholesky failed");
                }

                Vi[i] = Math.Sqrt(sum);
                var vii = Vi[i];
                for (j = i + 1; j < n; j++)
                {
                    sum2 = Vi[j];
                    var Vj = Rows[j];
                    if (firstNonZero != -1)
                    {
                        for (var k = Vj.ValueCount - 1; k >= 0; k--)
                        {
                            ref var k_ind = ref Vj.Indices[k];
                           
                            if (k_ind > lastNonZero)
                            {
                                continue;
                            }
                            if (k_ind < firstNonZero)
                            {
                                break;
                            }

                            // (k_ind >= firstNonZero && k_ind <= lastNonZero)
                            sum2 -= Rik[k_ind] * Vj.Values[k];

                        }
                    }

                    Vj[i] = sum2 / vii;
                }
            }


            //sw.Stop();
            //Console.WriteLine("Elapsed (Cholesky)={0}", sw.Elapsed);

            L = new double[n, n];
            for (i = 0; i < n; i++)
            {
                for (j = 0; j <= i; j++)
                {
                    L[i, j] = Rows[i, j];
                }
            }

            //CholeskyDecom tmp = new CholeskyDecom(Arg);
            //for (i = 0; i < n; i++)
            //{
            //    for (j = 0; j <= i; j++)
            //    {
            //        if(tmp.L[i, j] - L[i, j]!=0)
            //        Debug.WriteLine(tmp.L[i, j] - L[i, j]);
            //    }
            //}


        }

    }

}
