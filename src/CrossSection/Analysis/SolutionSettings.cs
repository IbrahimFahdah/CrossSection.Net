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

namespace CrossSection.Analysis
{
    public class SolutionSettings
    {
        public SolutionSettings(double roughness=0.01,
            double maximumArea=0,
            double minimumAngle=30,
            double maximumAngle = 0,
            bool conformingDelaunay = true,
            double plasticAxisAccuracy = 1e-5,
            int plasticAxisMaxIterations = 500)
        {
            Roughness = roughness;
            MinimumAngle = minimumAngle;
            MaximumArea = maximumArea;
            ConformingDelaunay = conformingDelaunay;
            PlasticAxisAccuracy = plasticAxisAccuracy;
            PlasticAxisMaxIterations = plasticAxisMaxIterations;
            RunPlasticAnalysis = true;
            RunWarpingAnalysis = true;
           
        }

        /// <summary>
        /// Gets or sets a maximum triangle area constraint for triangulation as a ratio of the section area. 
        /// value must be larger than zero.
        /// </summary>
        public double Roughness { get; set; }

        /// <summary>
        ///Gets or sets a minimum angle constraint used for triangulation.
        /// </summary>
        public double MinimumAngle { get; set; }

        /// <summary>
        /// Gets or sets a maximum angle constraint.
        /// Set to zero to ignore this constraint, otherwise it must be less than 180.
        /// </summary>
        public double MaximumAngle { get; set; }

        /// <summary>
        /// Gets or sets a maximum triangle area constraint for triangulation. 
        /// The capitulation might take long time if the area is too small.
        /// If the MaximumArea is set to zero, the software will automatically choose a value based on the roughness input.
        /// </summary>
        public double MaximumArea { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create a Conforming
        /// Delaunay triangulation.
        /// </summary>
        public bool ConformingDelaunay { get; set; }

        /// <summary>
        /// The accuracy to use when determining the location of the plastic axes. 
        /// </summary>
        public double PlasticAxisAccuracy { get; set; }

        /// <summary>
        /// The max number of iterations to terminating the   to use when determining the location of the plastic axes. 
        /// </summary>
        public int PlasticAxisMaxIterations { get; set; }

        /// <summary>
        /// If set to False, the plastic section properties will not be calculated.
        /// </summary>
        public bool RunPlasticAnalysis { get; set; }

        /// <summary>
        /// If set to False, the torsion section properties will not be calculated.
        /// </summary>
        public bool RunWarpingAnalysis { get; set; }
    }

}
