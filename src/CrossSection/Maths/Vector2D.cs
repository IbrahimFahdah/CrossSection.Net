using System;
using System.Collections.Generic;
using System.Text;

namespace CrossSection.Maths
{
    internal class Vector2D
    {
        public double X;
        public double Y;

        // Constructors.
        public Vector2D(double x, double y) { X = x; Y = y; }
        public Vector2D() : this(double.NaN, double.NaN) { }

        public static Vector2D operator -(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X - w.X, v.Y - w.Y);
        }

        public static Vector2D operator +(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X + w.X, v.Y + w.Y);
        }

        public static double operator *(Vector2D v, Vector2D w)
        {
            return v.X * w.X + v.Y * w.Y;
        }

        public static Vector2D operator *(Vector2D v, double mult)
        {
            return new Vector2D(v.X * mult, v.Y * mult);
        }

        public static Vector2D operator *(double mult, Vector2D v)
        {
            return new Vector2D(v.X * mult, v.Y * mult);
        }

        public double Cross(Vector2D v)
        {
            return Cross(this, v);// X * v.Y - Y * v.X;
        }

        public static double Cross(Vector2D v1, Vector2D v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

     

    }
   
}
