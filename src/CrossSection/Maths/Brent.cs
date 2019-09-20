// <copyright file="Brent.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;


namespace CrossSection.Maths
{
    /// <summary>
    /// Algorithm by Brent, Van Wijngaarden, Dekker et al.
    /// Implementation inspired by Press, Teukolsky, Vetterling, and Flannery, "Numerical Recipes in C", 2nd edition, Cambridge University Press
    /// </summary>
    public static class Brent
    {
        /// <summary>
        /// The number of binary digits used to represent the binary number for a double precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        const int DoubleWidth = 53;

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 double-precision floating numbers (64 bit).
        /// According to the definition of Prof. Demmel and used in LAPACK and Scilab.
        /// </summary>
        public static readonly double DoublePrecision = Math.Pow(2, -DoubleWidth);

        /// <summary>
        /// Value representing 10 * 2^(-53) = 1.11022302462516E-15
        /// </summary>
        static readonly double DefaultDoubleAccuracy = DoublePrecision * 10;

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 double-precision floating numbers (64 bit).
        /// According to the definition of Prof. Higham and used in the ISO C standard and MATLAB.
        /// </summary>
        public static readonly double PositiveDoublePrecision = 2 * DoublePrecision;

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy, int maxIterations, out double root)
        {
            double fmin = f(lowerBound);
            double fmax = f(upperBound);
            double froot = fmax;
            double d = 0.0, e = 0.0;

            root = upperBound;
            double xMid = double.NaN;

            // Root must be bracketed.
            if (Math.Sign(fmin) == Math.Sign(fmax))
            {
                return false;
            }

            for (int i = 0; i <= maxIterations; i++)
            {
                // adjust bounds
                if (Math.Sign(froot) == Math.Sign(fmax))
                {
                    upperBound = lowerBound;
                    fmax = fmin;
                    e = d = root - lowerBound;
                }

                if (Math.Abs(fmax) < Math.Abs(froot))
                {
                    lowerBound = root;
                    root = upperBound;
                    upperBound = lowerBound;
                    fmin = froot;
                    froot = fmax;
                    fmax = fmin;
                }

                // convergence check
                double xAcc1 = PositiveDoublePrecision*Math.Abs(root) + 0.5*accuracy;
                double xMidOld = xMid;
                xMid = (upperBound - root)/2.0;

                if (Math.Abs(xMid) <= xAcc1 || froot.AlmostEqualNormRelative(0, froot, accuracy))
                {
                    return true;
                }

                if (xMid == xMidOld)
                {
                    // accuracy not sufficient, but cannot be improved further
                    return false;
                }

                if (Math.Abs(e) >= xAcc1 && Math.Abs(fmin) > Math.Abs(froot))
                {
                    // Attempt inverse quadratic interpolation
                    double s = froot/fmin;
                    double p;
                    double q;
                    if (lowerBound.AlmostEqualRelative(upperBound))
                    {
                        p = 2.0*xMid*s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = fmin/fmax;
                        double r = froot/fmax;
                        p = s*(2.0*xMid*q*(q - r) - (root - lowerBound)*(r - 1.0));
                        q = (q - 1.0)*(r - 1.0)*(s - 1.0);
                    }

                    if (p > 0.0)
                    {
                        // Check whether in bounds
                        q = -q;
                    }

                    p = Math.Abs(p);
                    if (2.0*p < Math.Min(3.0*xMid*q - Math.Abs(xAcc1*q), Math.Abs(e*q)))
                    {
                        // Accept interpolation
                        e = d;
                        d = p/q;
                    }
                    else
                    {
                        // Interpolation failed, use bisection
                        d = xMid;
                        e = d;
                    }
                }
                else
                {
                    // Bounds decreasing too slowly, use bisection
                    d = xMid;
                    e = d;
                }

                lowerBound = root;
                fmin = froot;
                if (Math.Abs(d) > xAcc1)
                {
                    root += d;
                }
                else
                {
                    root += Sign(xAcc1, xMid);
                }

                froot = f(root);
            }

            return false;
        }

        /// <summary>Helper method useful for preventing rounding errors.</summary>
        /// <returns>a*sign(b)</returns>
        static double Sign(double a, double b)
        {
            return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a);
        }


        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        static bool AlmostEqualRelative(this double a, double b)
        {
            return AlmostEqualNormRelative(a, b, a - b, DefaultDoubleAccuracy);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal
        /// within the specified maximum error.
        /// </summary>
        /// <param name="a">The norm of the first value (can be negative).</param>
        /// <param name="b">The norm of the second value (can be negative).</param>
        /// <param name="diff">The norm of the difference of the two values (can be negative).</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        /// <returns>True if both doubles are almost equal up to the specified maximum error, false otherwise.</returns>
         static bool AlmostEqualNormRelative(this double a, double b, double diff, double maximumError)
        {
            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a == b;
            }

            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            // If one is almost zero, fall back to absolute equality
            if (Math.Abs(a) < DoublePrecision || Math.Abs(b) < DoublePrecision)
            {
                return Math.Abs(diff) < maximumError;
            }

            if ((a == 0 && Math.Abs(b) < maximumError) || (b == 0 && Math.Abs(a) < maximumError))
            {
                return true;
            }

            return Math.Abs(diff) < maximumError * Math.Max(Math.Abs(a), Math.Abs(b));
        }
    }
}
