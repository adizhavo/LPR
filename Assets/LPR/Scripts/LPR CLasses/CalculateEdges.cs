using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class CalculateEdges 
{
	#region PRIVATE_FIELDS
	// Width && Height te figures
	private int ImageWidth;
	private int ImageHeight;
	// Konturet e kapura nga sistemi
	[SerializeField]
	private List<Contour> ContourPath = new List<Contour>();

	#endregion

	#region PUBLIC_METHODS
	// Fillon procesi i gjetjeve te Edges dhe Konture
	public void CalcualteBorders(float[,] ImageMatrix, int ImageWidth, int ImageHeight)
	{
		this.ImageWidth = ImageWidth;
		this.ImageHeight = ImageHeight;
		// Fshin Grupin e kontureve 
		ContourPath.Clear();
		
		for (int y = 0; y < ImageHeight; y++)
		{		
			for (int x = 0; x < ImageWidth; x++)
			{
				// Nis kur kap nje pixel me vleren 1 (zeze) dhe nqs afer atij pixelit ekziston nje me velren 0 (bordure)
				if (ImageMatrix[x, y] == 1 && isBorder(x, y, ImageMatrix))
				{
					// Shton konturin
					ContourPath.Add(new Contour());
					// fillon te krijoje Path ose konturin per ate Edge
					CreatePath( x, y, ContourPath[ContourPath.Count - 1], ImageMatrix );
				}
			}
		}
	}
	// Kthen konturet e marra
	public List<Contour> GetContour()
	{
		return ContourPath;
	}

	#endregion

	#region PRIVATE_METHODS
	// Krijon Pathin
	private void CreatePath(int x, int y, Contour pathToSave, float[,] matrix)
	{
		// Vendosim ngjyren e kuqe
		matrix[x, y] = -1f;
		// Shtojm kete pike tek konturi
		pathToSave.AddPoint(x, y);
		// Check per posicionin te kesaj pike
		if (x < ImageWidth -1 && y < ImageHeight -1 && x > 0 && y > 0) 
		{
			// Zgjedhim drejtimin per te vazhduar konturin (djathtas)
			if (matrix[x+1, y] == 1)
			{
				// Check nqs eshte bordure
				if (isBorder( x + 1, y, matrix))
				{
					// Therret ne rekursiv
					CreatePath(x + 1, y, pathToSave, matrix);
				}
				else
				{
					// Check nqs eshte nje pike e brendshme afer nje konture dhe drejtimin e tij
					int nextBorderValue = IsABorderContinued(x + 1, y, matrix);
					// ka nje drejtim
					if(nextBorderValue != 0)
					{
						// Zgjedhim ku te vazhdojm te kerkojm per ndonje pike te konturit
						if (nextBorderValue == 1) CreatePath(x + 2, y, pathToSave, matrix);
						if (nextBorderValue == 2) CreatePath(x + 1 , y + 1, pathToSave, matrix);
						if (nextBorderValue == -2) CreatePath(x + 1, y - 1, pathToSave, matrix);
					}
				}
			}
			// vazhdojm dhe me pikat e tjera ne drejtimet e mbetura (majtas)
			if (matrix[x-1, y] == 1)
			{
				if (isBorder( x - 1, y, matrix) )
				{
					CreatePath(x - 1, y, pathToSave, matrix);
				}
				else
				{
					int nextBorderValue = IsABorderContinued( x - 1, y, matrix);
					
					if(nextBorderValue != 0)
					{
						if (nextBorderValue == -1) CreatePath( x - 2, y, pathToSave, matrix);
						if (nextBorderValue == 2) CreatePath( x - 1 , y + 1, pathToSave, matrix);
						if (nextBorderValue == -2) CreatePath( x - 1, y - 1, pathToSave,matrix);
					}
				}
			}
			// (siper)
			if ( matrix[x, y+1] == 1)
			{
				if (isBorder( x, y + 1, matrix))
				{
					CreatePath( x, y + 1, pathToSave, matrix);
				}
				else
				{
					int nextBorderValue = IsABorderContinued( x, y + 1, matrix);
					
					if(nextBorderValue != 0)
					{
						if (nextBorderValue == 1) CreatePath( x + 1, y + 1, pathToSave, matrix);
						if (nextBorderValue == -1) CreatePath( x - 1, y + 1, pathToSave, matrix);
						if (nextBorderValue == 2) CreatePath( x , y + 2, pathToSave, matrix);
					}
				}
			}
			// (posht)
			if (matrix[x, y-1] == 1)
			{
				if (isBorder(x, y - 1, matrix))
				{
					CreatePath(x, y - 1, pathToSave, matrix);
				}
				else
				{
					int nextBorderValue = IsABorderContinued(x, y - 1, matrix);
					
					if(nextBorderValue != 0)
					{
						if (nextBorderValue == 1) CreatePath(x + 1, y - 1, pathToSave, matrix);
						if (nextBorderValue == -1) CreatePath(x - 1, y - 1, pathToSave, matrix);
						if (nextBorderValue == -2) CreatePath( x , y - 2, pathToSave, matrix);
					}
				}
			}
		}
	}
	// Check qns pika eshte e konturit e izoluar por ka nje pike afer qe eshte nje bordur
	private int IsABorderContinued(int x,int y, float[,] matrix)
	{
		// Check te pos te pikes
		if (x >= ImageWidth -1 || y >= ImageHeight -1 || x <= 0 || y <= 0) return 0;
		// shohim nqs pika ngjitur eshte bordure (djathtas)
		if (matrix[x+1, y] == 1)
		{
			if (isBorder(x + 1, y, matrix))
			{
				return 1;
			}
		}
		// (majtas)
		if (matrix[x-1, y] == 1)
		{
			if (isBorder(x - 1, y, matrix) )
			{
				return -1;
			}
		}
		// (siper)
		if ( matrix[x, y+1] == 1)
		{
			if (isBorder(x, y + 1, matrix))
			{
				return 2;
			}
		}
		// (posht)
		if (matrix[x, y-1] == 1)
		{
			if (isBorder(x, y - 1, matrix))
			{
				return -2;
			}
		}
		
		return 0;
	}
	// Check qns pika eshte nje bordure
	private bool isBorder(int x,int y, float[,] matrix)
	{
		// Check te pos te pikes
		if (x >= ImageWidth -1 || y >= ImageHeight -1 || x <= 0 || y <= 0) return false;
		// checkojm nqs ka nje pike me vlere 0 afer (majtas)
		if (matrix[x-1, y] == 0)
		{
			return true;
		} // (djathtas)
		else if (matrix[x+1, y] == 0)
		{
			return true;
		}// (posht)
		else if (matrix[x, y-1] == 0)
		{
			return true;
		}// (siper)
		else if (matrix[x, y+1] == 0)
		{
			return true;
		}
		// nk eshte kontur
		return false;
	}

	#endregion
}