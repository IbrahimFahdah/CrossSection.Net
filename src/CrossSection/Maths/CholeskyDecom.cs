// <copyright>
//https://github.com/IbrahimFahdah/CrossSection.Net

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
using CrossSection.Triangulation;


namespace McritTorEngine.MathFun
{
    /// <summary>Cholesky Decomposition.</summary>
    /// <remarks>
    /// For a symmetric, positive definite matrix A, the Cholesky decomposition
    /// is an lower triangular matrix L so that A = L*L'.
    /// </remarks>
    public class CholeskyDecom
    {
        #region Class variables

        /// <summary>Array for internal storage of decomposition.</summary>
        private double[,] L;

        /// <summary>Row and column dimension (square matrix).</summary>
        private int n
        {
            get { return L.GetLength(0); }
        }

        #endregion 

        public CholeskyDecom()
        {
        }
        /// <summary>Cholesky algorithm for symmetric and positive definite matrix.</summary>
        /// <param name="Arg">Square, symmetric matrix.</param>
        /// <returns>Structure to access L and isspd flag.</returns>
        public CholeskyDecom(Matrix Arg)
        {
            // Initialize.
            L = Arg.ToArray();

            int i, j, k;

            double sum;
            for (i = 0; i < n; i++)
            {
                for (j = i; j < n; j++)
                {
                    for (sum = L[i, j], k = i - 1; k >= 0; k--) sum -= L[i, k] * L[j, k];
                    if (i == j)
                    {
                        if (sum <= 0.0) //A, with rounding errors, is not positive-definite.
                            throw new System.SystemException("Cholesky failed");
                        L[i, i] = System.Math.Sqrt(sum);
                    }
                    else L[j, i] = sum / L[i, i];
                }
            }

            for (i = 0; i < n; i++)
                for (j = 0; j < i; j++)
                    L[j, i] = 0.0;

        }


        #region Public Methods

        /// <summary>Return triangular factor.</summary>
        /// <returns>L</returns>
        public Matrix GetL()
        {
            return new Matrix(L);
        }

        /// <summary>Solve A*X = B</summary>
        /// <param name="B">  A Matrix with as many rows as A and any number of columns.</param>
        /// <returns>X so that L*L'*X = B</returns>
        /// <exception cref="System.ArgumentException">Matrix row dimensions must agree.</exception>
        /// <exception cref="System.SystemException">Matrix is not symmetric positive definite.</exception>
        public Vector Solve(Vector B)
        {
            if (B.Count != n)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            // Copy right hand side.
            double[] X = B.ToArray();

            double sum;
            int kk;
            for (int i = 0; i < n; i++)
            {
                sum = B[i];
                for (kk = i - 1; kk >= 0; kk--)
                    sum -= L[i, kk] * X[kk];

                X[i] = sum / L[i, i];
            }

            for (int i = n - 1; i >= 0; i--)
            {
                sum = X[i];
                for (kk = i + 1; kk < n; kk++)
                    sum -= L[kk, i] * X[kk];

                X[i] = sum / L[i, i];
            }

            return new Vector(X);
        }
        #endregion //  Public Methods
    }
}