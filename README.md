ProceduralTerrainBlending

Unity Project, blending different landmass types seamlessly to create believable "infinite" procedural  terrain.

Based on https://github.com/sotos82/ProceduralTerrainUnity

Terrain height data is created using 4 different "landmass" noise types: plains, hills, mountains, ridged mountains.
Each terrain object has random combination of landmass types, and adjacent terrains are blended seamlessly using bell shape sine curve falloff function. 

![Example image](ScreenShots/example_01.PNG?raw=true "Example image")

Example image above is using https://github.com/Scrawk/Ceto for water.


Unity version used: 2018.4.6f1

Disclaimer:
This is a work in progress hobby project, so there are some optimizations to be made, and code is not polished.

Future Plans: Once Unity has DOTS terrain available, I will test if this can be used with that.  
