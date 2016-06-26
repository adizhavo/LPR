using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Hapja te databases te fjaleve per njohjen
public class OpenFileDatabase : MonoBehaviour
{
	[HideInInspector]
	private PlateLocalization PlateToSetDatabse;

	#region PUBLIC_METHODS

	public void OpenFile(PlateLocalization ReturnDataToPlate)
	{
		PlateToSetDatabse = ReturnDataToPlate;
		// File mund te fshihet ose te modifikohet, vendosim nje Try
		try
		{
#if UNITY_EDITOR
			// Hapim filin
			string[] parts = OpenFileData(Application.streamingAssetsPath  + "\\textTrainedPlates.txt", ';');
			// E ndajm sipas nje llogjike
			ProcessData( parts );
#else
			StartCoroutine( OpenFileOnMobile(Application.streamingAssetsPath  + "/textTrainedPlates.txt", ';') );
#endif
		}
		catch
		{
			Debug.LogError("Couldn't open the file \"textTrainedPlates.txt\", please try to restore it or retrain the system !");
		}
	}

	#endregion

	#region PRIVATE_METHODS
	
	private string[] OpenFileData(string filePath, char splitChar)
	{
		// Hapet File
		StreamReader theReader = new StreamReader(filePath, Encoding.Default);			
		// Perdorim using, harxhon pak memorie
		using (theReader)
		{
			// Lexojm deri ne fund dhe i bejm nje split
			string[] textOfFile = theReader.ReadToEnd().Trim().Split(splitChar); 
			theReader.Close();
			return textOfFile;
		}
	}
	// Coroutine per te hapur ne mobile filet
	private IEnumerator OpenFileOnMobile(string filePath, char splitChar)
	{
		// Marrim pathin dhe startojm nje WWW
		WWW data = new WWW(filePath);
		// Presim sa te ngarkohet WWW
		yield return data;
		// Shohim nqs ka Errore
		if(string.IsNullOrEmpty(data.error))
		{
			string[] textOfFile = data.text.Trim().Split(splitChar);
			ProcessData( textOfFile );
		}
		else
		{
			Debug.LogError("The file \""+ filePath +"\" is corrupted, please try to restore it or retrain the system !");
		}           
	}

	private void ProcessData(string[] parts)
	{
		// Klasa qe do permbaje informacionet
		GetWordDatabase WordsData = new GetWordDatabase();
		// Per cdo komponent te file
		for (int i = 0; i < parts.Length - 1; i++)
		{
			// Nqs nje nga komponenta eshte bosh nk vazhdojm m tutje
			if (!string.IsNullOrEmpty(parts[i]))
			{
				// shtojm vlera ne klasen qe do permbaje informacione
				WordsData.WordsHeight.Add( int.Parse(parts[i].Split(':')[0]) );
				WordsData.WordsWidth.Add( int.Parse(parts[i].Split(':')[1]) );
				WordsData.WordsChar.Add( parts[i].Split(':')[2] );
				
				WordsData.WordsBynaryValues.Add( new List<int>() );
				
				string[] values = parts[i].Split(':')[3].Split(',');
				
				for (int j = 0; j < values.Length; j++)
				{
					if (!string.IsNullOrEmpty(values[j].Trim()))
					{
						WordsData.WordsBynaryValues[WordsData.WordsBynaryValues.Count - 1].Add( int.Parse( values[j].Trim() ) );
					}
				}
			}
		}

		PlateToSetDatabse.SetGetWordDatabase( WordsData );
	}

	#endregion
}
// Klasa qe do permbaje informacionet
public class GetWordDatabase
{
	#region PUBLIC_FIELDS

	public List<int> WordsHeight = new List<int>();
	public List<int> WordsWidth = new List<int>();
	
	public List<List<int>> WordsBynaryValues = new List<List<int>>();
	
	public List<string> WordsChar = new List<string>();

	#endregion
}
