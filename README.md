ProceduralTerrainBlending

Unity Project, blending different landmass types seamlessly to create believable procedural  infinite terrain.

Based on https://github.com/Hsantos/ProceduralTerrainUnity

Terrain height data is created using 4 different "landmass" noise types: plains, hills, mountains, ridged mountains.
each terrain object has random combination of land mass types, and adjcent terrains are blended seamlessly using bells shape sine curve falloff function. 

This is work in progress, so there are some optimizations to be, and some obvious bugs that need some attewntion.
