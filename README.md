# CrossSection.Net
CrossSection.Net is a C# package for the analysis of arbitrary cross sections using finite elements. This project is based on a python package written by Robbie van Leeuwen https://github.com/robbievanleeuwen/section-properties. CrossSection.Net uses Triangle.NET for section triangulation https://archive.codeplex.com/?p=triangle.


# Current Capabilities:
- [x] Global axis geometric section properties:
  - [x] Area
  - [x] First moments of area
  - [x] Second moments of area
  - [x] Elastic centroid
- [x] Centroidal axis geometric section properties:
  - [x] Second moments of area
  - [x] Elastic section moduli
  - [ ] Yield moment
  - [x] Radii of gyration
  - [x] Plastic centroid
  - [x] Plastic section moduli
  - [x] Shape factors
- [x] Principal axis geometric section properties:
  - [x] Second moments of area
  - [x] Elastic section moduli
  - [ ] Yield moment
  - [x] Radii of gyration
  - [x] Plastic centroid
  - [x] Plastic section moduli
  - [x] Shape factors
- [x] Warping section properties:
  - [x] Torsion constant
  - [x] Warping constant
- [x] Shear section properties:
  - [x] Shear centre (elastic method)
  - [x] Shear centre (Trefftz's method)
  - [x] Shear areas (global axis)
  - [x] Shear areas (principal axis)

# Validation:
The output from CrossSection.Net has been validated against the output from the Python package and other software.

# Peformance:
Optimization to many parts of the code has been done to make the calculation speed as fast as possible. The finer the mesh the more time is required for the calculation. If you don't need the warping or plastic section properties, you can switch the calculation of these off to speed up the calculation. Use the solution settings to control the mesh size and the flags to switch ON/Off the calculation of the warping and plastic properties.
