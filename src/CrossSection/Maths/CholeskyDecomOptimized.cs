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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public CholeskyDecomOptimized(Matrix Arg)
        {

            // Initialize.
            L = Arg.ToArray();

            var myMatrix = Matrix<double>.Build.SparseOfArray(L);
            List<SparseVectorStorage<double>> sparseRows = myMatrix.EnumerateRows().Select(s => s.Storage).Cast<SparseVectorStorage<double>>().ToList();

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            int i, j;
            var sum2 = 0.0;
            var lastNonZero = -1;
            var firstNonZero = -1;
            var k_ind = 0;
            for (i = 0; i < n; i++)
            {
                double[] Rik = new double[i];
                var sum = 0.0;
                lastNonZero = -1;
                firstNonZero = -1;

                var Vi = sparseRows[i];
                for (var k = 0; k < Vi.ValueCount; k++)
                {
                    k_ind = Vi.Indices[k];
                    if (k_ind > i - 1)
                    {
                        break;
                    }

                    var Lik = Vi.Values[k];
                    sum -= Lik * Lik;

                    Rik[k_ind] = Lik;
                    if (Lik != 0)
                    {
                        lastNonZero = k_ind;
                        if (firstNonZero == -1)
                        {
                            firstNonZero = lastNonZero;
                        }
                    }
                }

                ref var Lii = ref L[i, i];
                Lii = Math.Sqrt(Lii + sum);

                for (j = i + 1; j < n; j++)
                {
                    sum2 = L[i, j];

                    if (lastNonZero != -1)
                    {
                        var Vj = sparseRows[j];
                        for (var k = 0; k < Vj.ValueCount; k++)
                        {
                            if (Vj.Indices[k] < firstNonZero)
                            {
                                continue;
                            }
                            if (Vj.Indices[k] > lastNonZero)
                            {
                                break;
                            }

                           // (k_ind >= firstNonZero && k_ind <= lastNonZero)
                            sum2 -= Rik[Vj.Indices[k]] * Vj.Values[k];
                          
                        }
                    }

                    L[j, i] = sparseRows[j][i] = sum2 / Lii;
                }
            }


            //sw.Stop();
            //Console.WriteLine("Elapsed (Cholesky)={0}", sw.Elapsed);

            //zero the top part so we have only the lower part
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < i; j++)
                {
                    L[j, i] = 0.0;
                }
            }
        }

    }
}