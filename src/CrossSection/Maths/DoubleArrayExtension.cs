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
using System.Text;

namespace CrossSection.Maths
{
    internal static class DoubleArrayExtension
    {

        public static void Append(this double[,] A, double[,] B)
        {
            int rowCount = A.RowCount();
            int columnCount = A.ColumnCount();

            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < columnCount; j++)
                    A[i, j] += B[i, j];
        }

        public static double[] Append(this double[] A, double[] B)
        {

            for (int i = 0; i < A.Length; i++)
                A[i] += B[i];
            return A;

        }

        public static double[,] Transpose(this double[,] A)
        {
            int rowCount = A.RowCount();
            int columnCount = A.ColumnCount();
            double[,] transposed = new double[columnCount, rowCount];

            for (int column = 0; column < columnCount; column++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    transposed[column, row] = A[row, column];
                }
            }
            return transposed;

        }

        public static double[][] Transpose(this double[][] arr)
        {
            int rowCount = arr.Length;
            int columnCount = arr[0].Length;
            double[][] transposed = new double[columnCount][];

            for (int column = 0; column < columnCount; column++)
            {
                transposed[column] = new double[rowCount];
                for (int row = 0; row < rowCount; row++)
                {
                    transposed[column][row] = arr[row][column];
                }
            }
            return transposed;

        }

        public static double[,] Dot(this double[,] A, double[,] B)
        {
            var R = new double[A.RowCount(), B.ColumnCount()];
            for (int i = 0; i < R.RowCount(); i++)
                for (int j = 0; j < R.ColumnCount(); j++)
                    for (int k = 0; k < A.ColumnCount(); k++)
                        R[i, j] += A[i, k] * B[k, j];
            return R;

        }

        public static double[,] Dot(this double[,] A, double x)
        {
            int rowCount = A.RowCount();
            int columnCount = A.ColumnCount();

            var R = new double[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < columnCount; j++)
                    R[i, j] = A[i, j] * x;


            return R;

        }

        public static double[,] Dot(this double x, double[,] A)
        {
            int rowCount = A.RowCount();
            int columnCount = A.ColumnCount();
            var R = new double[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < columnCount; j++)
                    R[i, j] = A[i, j] * x;


            return R;

        }

        public static double[] Dot(this double x, double[] A)
        {
            var R = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
                R[i] = A[i] * x;
            return R;

        }

        public static double[] Dot(this double[] A, double x)
        {
            var R = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
                R[i] = A[i] * x;
            return R;

        }

        public static double[][] Dot(this double[][] A, double[,] B)
        {

            var R = new double[A.RowCount()][];
            for (int i = 0; i < A.RowCount(); i++)
            {
                R[i] = new double[B.ColumnCount()];
                for (int j = 0; j < B.ColumnCount(); j++)
                {
                    for (int k = 0; k < A.ColumnCount(); k++)
                    {
                        R[i][j] += A[i][k] * B[k, j];
                    }
                }
            }

            return R;

        }

        public static double[][] Dot(this double[][] A, double[][] B)
        {

            var R = new double[A.RowCount()][];
            for (int i = 0; i < A.RowCount(); i++)
            {
                R[i] = new double[B.ColumnCount()];
                for (int j = 0; j < B.ColumnCount(); j++)
                {
                    for (int k = 0; k < A.ColumnCount(); k++)
                    {
                        R[i][j] += A[i][k] * B[k][j];
                    }
                }
            }

            return R;

        }

        public static double Dot(this double[] A, double[] B)
        {

            var R = 0.0;
            for (int i = 0; i < A.Length; i++)
            {
                R += A[i] * B[i];
            }

            return R;

        }

        public static double[] Subtract(this double[] A, double[] B)
        {

            var R = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                R[i] = A[i] - B[i];
            }

            return R;

        }

        public static double[] Dot(this double[,] A, double[] V)
        {
            int rowCount = A.RowCount();
            int columnCount = A.ColumnCount();

            var R = new double[rowCount];

            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < columnCount; j++)
                    R[i] += A[i, j] * V[j];

            return R;

        }

        public static double[] Dot(this double[] V, double[,] A)
        {
            int rowCount = A.RowCount();
            int columnCount = A.ColumnCount();

            var R = new double[columnCount];

            for (int j = 0; j < columnCount; j++)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    R[j] += V[i] * A[i, j];
                }
            }

            return R;

        }

        public static double[] Row(this double[,] A, int i)
        {
            var R = new double[A.ColumnCount()];
            for (int j = 0; j < A.ColumnCount(); j++)
            {
                R[j] = A[i, j];
            }

            return R;
        }

        public static double[] Row(this double[][] A, int i)
        {
            return A[i];
        }

        public static int RowCount(this double[][] A)
        {
            return A.Length;
        }

        public static int ColumnCount(this double[][] A)
        {
            return A[0].Length;
        }

        public static int RowCount(this double[,] A)
        {
            return A.GetLength(0);
        }

        public static int ColumnCount(this double[,] A)
        {
            return A.GetLength(1);
        }

        //public static bool istheSameAs(this double[,] A, double[,] B)
        //{
        //    for (int i = 0; i < A.RowCount(); i++)
        //    {
        //        for (int j = 0; j < A.ColumnCount(); j++)
        //        {
        //            if (A[i, j] != B[i, j])
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    return true;

        //}

        //public static double[,] MultiplyBy(this double[,] A, double x)
        //{
        //    for (int i = 0; i < A.RowCount(); i++)
        //        for (int j = 0; j < A.ColumnCount(); j++)
        //            A[i, j] = A[i, j] * x;
        //    return A;

        //}

        //public static double[,] MultiplyBy(this double x, double[,] A)
        //{
        //    for (int i = 0; i < A.RowCount(); i++)
        //        for (int j = 0; j < A.ColumnCount(); j++)
        //            A[i, j] = A[i, j] * x;
        //    return A;

        //}

        //public static double[] MultiplyBy(this double x, double[] A)
        //{
        //    for (int i = 0; i < A.Length; i++)
        //        A[i] = A[i] * x;
        //    return A;

        //}

        //public static double[] MultiplyBy(this double[] A, double x)
        //{
        //    for (int i = 0; i < A.Length; i++)
        //        A[i] = A[i] * x;
        //    return A;

        //}

    }
}
