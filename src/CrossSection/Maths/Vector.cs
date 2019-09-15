// <copyright>
//https://github.com/IFahdah/CrossSection.Net

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

using MathNet.Numerics.LinearAlgebra;

namespace CrossSection.Triangulation
{
    /// <summary>
    /// Vector operations wrapper
    /// </summary>
    public class Vector
    {
        internal Vector<double> _v;

        public Vector(int n)
        {
            _v = Vector<double>.Build.Dense(n);
        }

        public Vector(Vector<double> v) 
        {
            _v = v;
        }

        public Vector(double[] array) 
        {
            _v = Vector<double>.Build.DenseOfArray(array);
        }

        public double this[int index]
        {
            get { return _v[index]; }
            set { _v[index] = value; }
        }

        public int Count { get { return _v.Count; } }
        public double[] ToArray()
        {
            return _v.ToArray();
        }

        public Matrix ToRowMatrix()
        {
            return new Matrix(_v.ToRowMatrix());
        }

        public Matrix ToColumnMatrix()
        {
            return new Matrix(_v.ToColumnMatrix());
        }

        public static Vector operator +(Vector rightSide)
        {
            return new Vector(rightSide._v.Clone());
        }

        public static Vector operator -(Vector rightSide)
        {
            return new Vector(rightSide._v.Negate());
        }

        public static Vector operator +(Vector leftSide, Vector rightSide)
        {
            return new Vector(leftSide._v.Add(rightSide._v));
        }

        public static Vector operator +(Vector leftSide, double rightSide)
        {
            return new Vector(leftSide._v.Add(rightSide));
        }

        public static Vector operator +(double leftSide, Vector rightSide)
        {
            return new Vector(rightSide._v.Add(leftSide));
        }

        public static Vector operator -(Vector leftSide, Vector rightSide)
        {
            return new Vector(leftSide._v.Subtract(rightSide._v));
        }

        public static Vector operator -(Vector leftSide, double rightSide)
        {
            return new Vector(leftSide._v.Subtract(rightSide));
        }

        public static Vector operator -(double leftSide, Vector rightSide)
        {
            return new Vector(rightSide._v.SubtractFrom(leftSide));
        }

        public static Vector operator *(Vector leftSide, double rightSide)
        {
            return new Vector(leftSide._v.Multiply(rightSide));
        }

        public static Vector operator *(double leftSide, Vector rightSide)
        {
            return new Vector(rightSide._v.Multiply(leftSide));
        }

        public static double operator *(Vector leftSide, Vector rightSide)
        {
            return leftSide._v.DotProduct(rightSide._v);
        }
    }
}
