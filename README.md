shp2svg
=======
Shapefile to SVG converter tool

To convert shp file to svg run shp2svg with arguments:
* -p, required, path to the shapefile or catalog with shapefiles 
* -a, required, name of shapefile metadata attribute to find matching id
* -w, optional, width of svg, default value is 900
* -h, optional, height of svg, default value if 600
* -t, optional, tolerance of smoothing, default value is 3. Min value 1
* -m, optional, generate file with shapefile metadata.

**Note**: Regions file should be called "Regions.json" and located in the same directory with shapefile.

### Example
* shp2svg -p c:\Users\Polyakova\Downloads\DZA_adm -a name_2
* shp2svg -p c:\Users\Polyakova\Downloads\DZA_adm\DZA_adm2.shp -a name_2
* shp2svg -p c:\Users\Polyakova\Downloads\DZA_adm -a name_2 -w 400 -h 150 -t 4

Path can be in 3 kind of formats:


* **c:/foo/file.shp** - path to particular file
* **c:/foo/*adm*.shp** - path with file mask
* **c:/foo** - path to catalog
