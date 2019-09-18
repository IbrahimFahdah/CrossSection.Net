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

namespace CrossSection.DataModel
{
    public class SectionProperties
    {
        #region elastic
        /// <summary>
        /// Cross-sectional area
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Modulus weighted area (axial rigidity)
        /// </summary>
        public double ea { get; set; }

        /// <summary>
        ///  Modulus weighted product of shear modulus and area
        /// </summary>
        public double ga { get; set; }

        /// <summary>
        ///  Effective Poisson’s ratio
        /// </summary>
        public double nu_eff { get; set; }

        /// <summary>
        /// First moment of area about the x-axis
        /// </summary>
        public double qx { get; set; }

        /// <summary>
        ///  First moment of area about the y-axis
        /// </summary>
        public double qy { get; set; } //

        /// <summary>
        /// Second moment of area about the global x-axis
        /// </summary>
        public double ixx_g { get; set; }

        /// <summary>
        ///  Second moment of area about the global y-axis
        /// </summary>
        public double iyy_g { get; set; } //

        /// <summary>
        /// Second moment of area about the global xy-axis
        /// </summary>
        public double ixy_g { get; set; }

        /// <summary>
        /// X coordinate of the elastic centroid
        /// </summary>
        public double cx { get; set; }

        /// <summary>
        ///  Y coordinate of the elastic centroid
        /// </summary>
        public double cy { get; set; } //

        /// <summary>
        ///  Second moment of area about the centroidal x-axis
        /// </summary>
        public double ixx_c { get; set; } //

        /// <summary>
        /// Second moment of area about the centroidal y-axis
        /// </summary>
        public double iyy_c { get; set; }

        /// <summary>
        /// Second moment of area about the centroidal xy-axis
        /// </summary>
        public double ixy_c { get; set; }

        /// <summary>
        /// Section modulus about the centroidal x-axis for stresses at the positive extreme value of y
        /// </summary>
        public double zxx_plus { get; set; }

        /// <summary>
        /// Section modulus about the centroidal x-axis for stresses at the negative extreme value of y
        /// </summary>
        public double zxx_minus { get; set; }

        /// <summary>
        /// Section modulus about the centroidal y-axis for stresses at the positive extreme value of x
        /// </summary>
        public double zyy_plus { get; set; }

        /// <summary>
        ///  Section modulus about the centroidal y-axis for stresses at the negative extreme value of x
        /// </summary>
        public double zyy_minus { get; set; }

        /// <summary>
        ///  Radius of gyration about the centroidal x-axis.
        /// </summary>
        public double rx_c { get; set; }

        /// <summary>
        /// Radius of gyration about the centroidal y-axis.
        /// </summary>
        public double ry_c { get; set; }

        /// <summary>
        ///  Second moment of area about the centroidal 11-axis
        /// </summary>
        public double i11_c { get; set; }

        /// <summary>
        /// Second moment of area about the centroidal 22-axis
        /// </summary>
        public double i22_c { get; set; }

        /// <summary>
        ///  Principal axis angle
        /// </summary>
        public double phi { get; set; }

        /// <summary>
        /// Section modulus about the principal 11-axis for stresses at the positive extreme value of the 22-axis
        /// </summary>
        public double z11_plus { get; set; }

        /// <summary>
        /// Section modulus about the principal 11-axis for stresses at the negative extreme value of the 22-axis
        /// </summary>
        public double z11_minus { get; set; }

        /// <summary>
        /// Section modulus about the principal 22-axis for stresses at the positive extreme value of the 11-axis
        /// </summary>
        public double z22_plus { get; set; }

        /// <summary>
        /// Section modulus about the principal 22-axis for stresses at the negative extreme value of the 11-axis
        /// </summary>
        public double z22_minus { get; set; }

        /// <summary>
        /// Radius of gyration about the principal 11-axis.
        /// </summary>
        public double r11_c { get; set; }

        /// <summary>
        /// Radius of gyration about the principal 22-axis.
        /// </summary>
        public double r22_c { get; set; }

        #endregion

        #region plastic
        /// <summary>
        /// X coordinate of the global plastic centroid.
        /// This is relative to the elastic centroid.
        /// </summary>
        public double x_pc { get; set; }

        /// <summary>
        /// Y coordinate of the global plastic centroid.
        /// This is relative to the elastic centroid.
        /// </summary>
        public double y_pc { get; set; }

        /// <summary>
        /// 11 coordinate of the principal plastic centroid.
        /// This is relative to the elastic centroid.
        /// </summary>
        public double x11_pc { get; set; }

        /// <summary>
        /// 22 coordinate of the principal plastic centroid.
        /// This is relative to the elastic centroid.
        /// </summary>
        public double y22_pc { get; set; }

        /// <summary>
        /// Plastic section modulus about the centroidal x-axis
        /// </summary>
        public double sxx { get; set; }

        /// <summary>
        /// Plastic section modulus about the centroidal y-axis
        /// </summary>
        public double syy { get; set; }

        /// <summary>
        /// Plastic section modulus about the 11-axis
        /// </summary>
        public double s11 { get; set; }

        /// <summary>
        /// Plastic section modulus about the 22-axis
        /// </summary>
        public double s22 { get; set; }

        ///// <summary>
        ///// Shape factor for bending about the x-axis with respect to the top fibre
        ///// </summary>
        //public double sf_xx_plus { get; set; }

        ///// <summary>
        ///// Shape factor for bending about the x-axis with respect to the bottom fibre
        ///// </summary>
        //public double sf_xx_minus { get; set; }

        ///// <summary>
        ///// Shape factor for bending about the y-axis with respect to the top fibre
        ///// </summary>
        //public double sf_yy_plus { get; set; }

        ///// <summary>
        ///// Shape factor for bending about the y-axis with respect to the bottom fibre
        ///// </summary>
        //public double sf_yy_minus { get; set; }

        ///// <summary>
        /////  Shape factor for bending about the 11-axis with respect to the top fibre
        ///// </summary>
        //public double sf_11_plus { get; set; } //

        ///// <summary>
        ///// Shape factor for bending about the 11-axis with respect to the bottom fibre
        ///// </summary>
        //public double sf_11_minus { get; set; }

        ///// <summary>
        ///// Shape factor for bending about the 22-axis with respect to the top fibre
        ///// </summary>
        //public double sf_22_plus { get; set; }

        ///// <summary>
        ///// Shape factor for bending about the 22-axis with respect to the bottom fibre
        ///// </summary>
        //public double sf_22_minus { get; set; }

        #endregion

        /// <summary>
        /// Torsion constant
        /// </summary>
        public double j { get; set; }

        /// <summary>
        /// Shear factor
        /// </summary>
        public double Delta_s { get; set; }

        /// <summary>
        /// X coordinate of the shear centre(elasticity approach).
        /// This is relative to the elastic centroid.
        /// </summary>
        public double x_se { get; set; }

        /// <summary>
        /// Y coordinate of the shear centre(elasticity approach).
        /// This is relative to the elastic centroid.
        /// </summary>
        public double y_se { get; set; }

        /// <summary>
        /// 11 coordinate of the shear centre(elasticity approach).
        /// This is relative to the elastic centroid.
        /// </summary>
        public double x11_se { get; set; }

        /// <summary>
        /// 22 coordinate of the shear centre(elasticity approach).
        /// This is relative to the elastic centroid.
        /// </summary>
        public double y22_se { get; set; }

        /// <summary>
        /// X coordinate of the shear centre(Trefftz’s approach).
        /// This is relative to the elastic centroid.
        /// </summary>
        public double x_st { get; set; }

        /// <summary>
        /// Y coordinate of the shear centre(Trefftz’s approach).
        /// This is relative to the elastic centroid.
        /// </summary>
        public double y_st { get; set; }

        /// <summary>
        /// Warping constant
        /// </summary>
        public double gamma { get; set; }

        /// <summary>
        /// Shear area about the x-axis
        /// </summary>
        public double A_sx { get; set; }

        /// <summary>
        /// Shear area about the y-axis
        /// </summary>
        public double A_sy { get; set; }

        /// <summary>
        /// Shear area about the xy-axis
        /// </summary>
        public double A_sxy { get; set; }

        /// <summary>
        /// Shear area about the 11 bending axis
        /// </summary>
        public double A_s11 { get; set; }

        /// <summary>
        /// Shear area about the 22 bending axis
        /// </summary>
        public double A_s22 { get; set; }

        /// <summary>
        /// Monosymmetry constant for bending about the x-axis with the top flange in compression
        /// </summary>
        public double beta_x_plus { get; set; }

        /// <summary>
        /// Monosymmetry constant for bending about the x-axis with the bottom flange in compression
        /// </summary>
        public double beta_x_minus { get; set; }

        /// <summary>
        /// Monosymmetry constant for bending about the y-axis with the top flange in compression
        /// </summary>
        public double beta_y_plus { get; set; }

        /// <summary>
        /// Monosymmetry constant for bending about the y-axis with the bottom flange in compression
        /// </summary>
        public double beta_y_minus { get; set; }

        /// <summary>
        /// Monosymmetry constant for bending about the 11-axis with the top flange in compression
        /// </summary>
        public double beta_11_plus { get; set; }

        /// <summary>
        /// Monosymmetry constant for bending about the 11-axis with the bottom flange in compression
        /// </summary>
        public double beta_11_minus { get; set; }

        /// <summary>
        /// Monosymmetry constant for bending about the 22-axis with the top flange in compression
        /// </summary>
        public double beta_22_plus { get; set; }

        /// <summary>
        /// Monosymmetry constant for bending about the 22-axis with the bottom flange in compression
        /// </summary>
        public double beta_22_minus { get; set; }

        /// <summary>
        ///  Warping function
        /// </summary>
        public double[] omega { get; set; }

        /// <summary>
        /// Psi shear function
        /// </summary>
        public double[] psi_shear { get; set; }

        /// <summary>
        ///  Phi shear function
        /// </summary>
        public double[] phi_shear { get; set; }
    }
}