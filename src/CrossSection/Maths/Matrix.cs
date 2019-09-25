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
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossSection.Maths
{
    /// <summary>
    /// Matrix operation wrapper
    /// </summary>
    public class Matrix
    {
        public Matrix<double> _m;
        public static int count;
        public Matrix(Matrix<double> m)
        {
            _m = m;
        }

        public Matrix(int rows, int columns)
        {
            _m = Matrix<double>.Build.Dense(rows, columns);
            count++;
        }

        public Matrix(double[,] array)
        {
            _m = Matrix<double>.Build.DenseOfArray(array); count++;
        }

        public Matrix(double[][] array)
        {
            _m = Matrix<double>.Build.DenseOfRowArrays(array); count++;
        }

        public double this[int row, int column]
        {
            get { return _m[row, column]; }
            set { _m[row, column] = value; }
        }

        public int RowCount { get { return _m.RowCount; } }
        public int ColumnCount { get { return _m.ColumnCount; } }

        public Matrix Clear()
        {
            _m.Clear();
            return this;
        }

        public Vector Row(int index)
        {
            return new Vector(_m.Row(index).ToArray());
        }

        public Matrix Transpose
        {
            get { return new Matrix(_m.Transpose()); }
        }

        public Matrix Inverse
        {
            get { return new Matrix(_m.Inverse()); }
        }

        public double Determinant
        {
            get { return _m.Determinant(); }
        }

        public double[,] ToArray()
        {
            return _m.ToArray();
        }

        public void Append(Matrix source)
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    _m[i, j] += source[i, j];
                }
            }
        }

        public Vector Column(int index)
        {
            return new Vector(_m.Column(index));
        }

        public static Matrix operator *(double leftSide, Matrix rightSide)
        {
            return new Matrix(rightSide._m.Multiply(leftSide));
        }

        public static Matrix operator *(Matrix leftSide, double rightSide)
        {
            return new Matrix(leftSide._m.Multiply(rightSide));
        }

        public static Matrix operator *(Matrix leftSide, Matrix rightSide)
        {
            return new Matrix(leftSide._m.Multiply(rightSide._m));
        }

        public static Vector operator *(Matrix leftSide, Vector rightSide)
        {
            return new Vector(leftSide._m.Multiply(rightSide._v));
        }

        public static Vector operator *(Vector leftSide, Matrix rightSide)
        {
            return new Vector(rightSide._m.LeftMultiply(leftSide._v));
        }

        public static Matrix operator +(Matrix rightSide)
        {
            return new Matrix(rightSide._m.Clone());
        }

        public static Matrix operator -(Matrix rightSide)
        {
            return new Matrix(rightSide._m.Negate());
        }

        public static Matrix operator +(Matrix leftSide, Matrix rightSide)
        {
            return new Matrix(leftSide._m.Add(rightSide._m));
        }

        public static Matrix operator +(Matrix leftSide, double rightSide)
        {
            return new Matrix(leftSide._m.Add(rightSide));
        }

        public static Matrix operator +(double leftSide, Matrix rightSide)
        {
            return new Matrix(rightSide._m.Add(leftSide));
        }

        public static Matrix operator -(Matrix leftSide, Matrix rightSide)
        {
            return new Matrix(leftSide._m.Subtract(rightSide._m));
        }

        public static Matrix operator -(Matrix leftSide, double rightSide)
        {
            return new Matrix(leftSide._m.Subtract(rightSide));
        }

        public static Matrix operator -(double leftSide, Matrix rightSide)
        {
            return new Matrix(rightSide._m.SubtractFrom(leftSide));
        }
    }
}
