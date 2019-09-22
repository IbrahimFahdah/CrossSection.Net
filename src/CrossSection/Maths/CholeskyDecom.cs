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
    public class CholeskyDecom : CholeskyDecomBase
    {

        /// <summary>Cholesky algorithm for symmetric and positive definite matrix.</summary>
        /// <param name="Arg">Square, symmetric matrix.</param>
        /// <returns>Structure to access L and isspd flag.</returns>
        public CholeskyDecom(Matrix Arg)
        {

            // Initialize.
            L = Arg.ToArray();

            var myMatrix = Matrix<double>.Build.SparseOfArray(L);
            List<Vector<double>> sparseRows = new List<Vector<double>>(myMatrix.EnumerateRows());

            int i, j, k;
            var sum2 = 0.0;
            for (i = 0; i < n; i++)
            {
                double[] Ri = new double[i];
                var sum = 0.0;

                foreach (var item in sparseRows[i].EnumerateIndexed(Zeros.AllowSkip).Where(x => x.Item1 <= i - 1))
                {
                    var Lik = item.Item2;
                    sum -= Lik * Lik;

                    Ri[item.Item1] = Lik;
                }

                ref var Lii = ref L[i, i];

                Lii = Math.Sqrt(Lii + sum);
                //\========


                for (j = i + 1; j < n; j++)
                {
                    sum2 = L[i, j];

                    if (sum != 0)
                    {
                        foreach (var item in sparseRows[j].EnumerateIndexed(Zeros.AllowSkip).Where(x => x.Item1 <= i - 1))
                        {
                            sum2 -= Ri[item.Item1] * item.Item2;
                        }
                    }

                    sparseRows[j][i] = sum2 / Lii;
                    L[j, i] = sparseRows[j][i];

                }


            }

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