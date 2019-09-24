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

namespace CrossSection.Maths
{

    /// <summary>Cholesky Decomposition.</summary>
    /// <remarks>
    /// For a symmetric, positive definite matrix A, the Cholesky decomposition
    /// is an lower triangular matrix L so that A = L*L'.
    /// </remarks>
    public class CholeskyDecom : CholeskyDecomBase
    {
        public CholeskyDecom(Matrix Arg)
        {

            // Initialize.
            L = Arg.ToArray();

            int n = L.GetLength(0);

            int i, j, k;

            double sum;
            for (i = 0; i < n; i++)
            {
                for (j = i; j < n; j++)
                {
                    for (sum = L[i, j], k = i - 1; k >= 0; k--)
                    {
                        sum -= L[i, k] * L[j, k];
                    }

                    if (i == j)
                    {
                        if (sum <= 0.0) //A, with rounding errors, is not positive-definite.
                        {
                            throw new System.SystemException("Cholesky failed");
                        }

                        L[i, i] = System.Math.Sqrt(sum);
                    }
                    else
                    {
                        L[j, i] = sum / L[i, i];
                    }
                }
            }

            for (i = 0; i < n; i++)
                for (j = 0; j < i; j++)
                    L[j, i] = 0.0;

        }

     
    }
}