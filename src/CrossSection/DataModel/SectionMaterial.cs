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

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossSection.DataModel
{
    public class SectionMaterial
    {

        public SectionMaterial(string name,int id, double elastic_modulus, double poissons_ratio, double yield_strength)
        {
            Name = name;
            Id = id;
            this.elastic_modulus = elastic_modulus;
            this.poissons_ratio = poissons_ratio;
            this.yield_strength = yield_strength;
        }
        public string Name { get; set; }
        public int Id { get; set; }
        public double elastic_modulus { get; set; }
        public double poissons_ratio { get; set; }
        public double yield_strength { get; set; }
        public double shear_modulus => elastic_modulus / (2 * (1 + poissons_ratio));

    }
}
