using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class TerrainSaver
{

	[SerializeField] private static float[,] testArray;

	public static void Save(string pathraw, TerrainData terrain)
	{
		//Get full directory to save to
		var filePath = Path.Combine(Application.persistentDataPath, pathraw);
		var path = new FileInfo(filePath);
		Directory.CreateDirectory(path.DirectoryName);

		// makes no sense to delete 
		// ... rather simply overwrite the file if exists
		//File.Delete(path.FullName);
		Debug.Log(path);

		//Get the width and height of the heightmap, and the heights of the terrain
		var w = terrain.heightmapWidth;
		var h = terrain.heightmapHeight;
		var tData = terrain.GetHeights(0, 0, w, h);


		testArray =  terrain.GetHeights(0, 0, w, h);
		

		Debug.Log("w " + w);

		// put the string together
		// StringBuilder is more efficient then using
		// someString += "xyz" because latter always allocates a new string
		var stringBuilder = new StringBuilder();
		for (var y = 0; y < h; y++)
		{
			for (var x = 0; x < w; x++)
			{
				//                                                         also add the linebreak if needed
				stringBuilder.Append(Mathf.Round(tData[x, y] * 100) / 100).Append(';').Append('\n');
			}
		}

		using (var file = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
		{
			using (var streamWriter = new StreamWriter(file, Encoding.UTF8))
			{
				streamWriter.Write(stringBuilder.ToString());
			}
		}
	}


	public static TerrainData LoadTerrainData ()
	{
		{
			string pathraw = "testSaving";
			var filePath = Path.Combine(Application.persistentDataPath, pathraw);
			//Read the text from directly from the test.txt file
			StreamReader reader = new StreamReader(filePath);
			
			//Debug.Log(reader.ReadToEnd());
			reader.Close();

			// reader.ReadToEnd()

			UnityEngine.TerrainData ter = new TerrainData();
			// put

			return ter;

		}


	}
}