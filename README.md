ProceduralTerrainBlending

Unity Project, blending different landmass types seamlessly to create believable "infinite" procedural  terrain.

Based on https://github.com/sotos82/ProceduralTerrainUnity

Terrain height data is created using 4 different "landmass" noise types: plains, hills, mountains, ridged mountains.
Each terrain object has random combination of landmass types, and adjacent terrains are blended seamlessly using bell shape sine curve falloff function. 

![Example image](ScreenShots/example_01.PNG?raw=true "Example image")

Example image above is using https://github.com/Scrawk/Ceto for water:

This is work in progress, so there are some optimizations to be made, and some obvious bugs that need attention.

Unity version used: 2018.4.6f1
