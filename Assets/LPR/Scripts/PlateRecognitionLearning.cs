using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PlateRecognitionLearning : MonoBehaviour {

	#region PUBLIC_FIELDS

	public SpriteRenderer ReadImage;
	public SpriteRenderer WriteImage;

	[Range(0f, 1f)]
	public float Threshold;
	[Range(0, 40)]
	public int MinSqareView;

	public GameObject InputField;

	#endregion

	#region PRIVATE_FIELDS

	private int counterSave = 0;
	private int width;
	private int height;

	private List<Contour> ContourPath = new List<Contour>();
	private List<Vector2> MaxPoints = new List<Vector2>();
	private List<Vector2> MinPoints = new List<Vector2>();
	private List<string> charToSave = new List<string>();

	private float[,] matrix;

	#endregion

	void Start()
	{
		ImageProcessing();
	}

	#region PUBLIC_METHODS

	public void SaveLetter(string charToSet)
	{		
		for (int i = (int)MinPoints[counterSave].x; i < MaxPoints[counterSave].x; i++)
		{
			matrix[i, (int)MaxPoints[counterSave].y] = matrix[i, (int)MinPoints[counterSave].y] = 0;
		}
		
		for (int i = (int)MinPoints[counterSave].y; i < MaxPoints[counterSave].y; i++)
		{
			matrix[(int)MaxPoints[counterSave].x, i] = matrix[(int)MinPoints[counterSave].x, i] = 0;
		}

		charToSave.Add(charToSet);

		if (counterSave < MaxPoints.Count - 1)
		{
			counterSave ++;
			
			StartSaving();
		}
		else
		{
			ReadPixelsFromBoxesAndSave();
			
			Color[] spritePixels = ConvertGrayScaleMatrixToPixelArray();
			
			WriteImage.sprite.texture.SetPixels(spritePixels);
			WriteImage.sprite.texture.Apply();
		}
	}

	#endregion

	#region PRIVATE_METHODS

	private void StartSaving()
	{
		InputField.SetActive(true);

		for (int i = (int)MinPoints[counterSave].x; i < MaxPoints[counterSave].x; i++)
		{
			matrix[i, (int)MaxPoints[counterSave].y] = matrix[i, (int)MinPoints[counterSave].y] = -2;
		}
		
		for (int i = (int)MinPoints[counterSave].y; i < MaxPoints[counterSave].y; i++)
		{
			matrix[(int)MaxPoints[counterSave].x, i] = matrix[(int)MinPoints[counterSave].x, i] = -2;
		}

		Color[] spritePixels = ConvertGrayScaleMatrixToPixelArray();
		
		WriteImage.sprite.texture.SetPixels(spritePixels);
		WriteImage.sprite.texture.Apply();
	}

	private void ImageProcessing()
	{
		Color[] spritePixels = ReadImage.sprite.texture.GetPixels();
		
		width = ReadImage.sprite.texture.width;
		height = ReadImage.sprite.texture.height;
		
		ConvertPixelArrayToGrayScaleMatrix(spritePixels);

		ContourPath.Clear();

		CalcualteBorders();

		CalculateRectangles();
		
		spritePixels = ConvertGrayScaleMatrixToPixelArray();

		WriteImage.sprite.texture.SetPixels(spritePixels);
		WriteImage.sprite.texture.Apply();

		StartSaving();
	}

	private void CalculateRectangles()
	{
		MaxPoints.Clear();
		MinPoints.Clear();

		for (int cont = 0; cont < ContourPath.Count; cont ++)
		{
			List<int> xVal = ContourPath[cont].GetXValue();
			List<int> yVal = ContourPath[cont].GetYValue();

			Vector2 maxXY = new Vector2(-1, -1), minXY = new Vector2(width + 1, height + 1);

			for (int i = 0; i < xVal.Count; i++)
			{
				if (xVal[i] > maxXY.x)
				{
					maxXY.x = xVal[i];
				}

				if (yVal[i] > maxXY.y )
				{
					maxXY.y = yVal[i];
				}

				if (xVal[i] < minXY.x )
				{
					minXY.x = xVal[i];
				}

				if (yVal[i] < minXY.y )
				{
					minXY.y = yVal[i];
				}
			}

			Vector2 centerPoint = (minXY - maxXY) / 2f + maxXY;

			if (!IsContourInsideABox(centerPoint))
			{
				int distanceMinMaxX = (int)Vector2.Distance(new Vector2(minXY.x, 0f), new Vector2(maxXY.x, 0f));
				
				if (distanceMinMaxX - MinSqareView < 0)
				{
					int add = (distanceMinMaxX - MinSqareView)/2 + (distanceMinMaxX - MinSqareView)%2;
					minXY.x += add;
					maxXY.x -= add;
				}
				
				distanceMinMaxX = (int)Vector2.Distance(new Vector2(minXY.x, 0f), new Vector2(maxXY.x, 0f));
				int distanceMinMaxY = (int)Vector2.Distance(new Vector2(0f, minXY.y), new Vector2(0f, maxXY.y));
				
				if ((float)distanceMinMaxY / (float)distanceMinMaxX > 1f 
				    && (float)distanceMinMaxY / (float)distanceMinMaxX < 3f
				    && (int)minXY.x > 0 
				    && (int)minXY.y > 0
				    && (int)maxXY.y > 0
				    && (int)maxXY.x > 0)
				{

					MaxPoints.Add(maxXY);
					MinPoints.Add(minXY);
				}
			}
		}
	}

	private bool IsContourInsideABox(Vector2 center)
	{
		for (int i = 0; i < MaxPoints.Count; i++)
		{
			if (center.x <= MaxPoints[i].x && center.x >= MinPoints[i].x && center.y <= MaxPoints[i].y && center.y >= MinPoints[i].y)
			{
				return true;
			}
		}

		return false;
	}

	private void ReadPixelsFromBoxesAndSave()
	{
		string createText = System.String.Empty;

		for (int i = 0; i < MaxPoints.Count; i++)
		{
			if (charToSave[i] != "*")
			{
				createText += ((int)MaxPoints[i].y - (int)MinPoints[i].y).ToString() + ":"
						+ ((int)MaxPoints[i].x - (int)MinPoints[i].x).ToString() + ":"
						+ charToSave[i] + ":";

				for (int y = (int)MinPoints[i].y; y < (int)MaxPoints[i].y; y++)
				{
					for (int x = (int)MinPoints[i].x; x < (int)MaxPoints[i].x; x++)
					{
						createText += (matrix[x, y] == 0) ? "0," : "1,";
					}
				}

				createText += ";";
			}
		}

		File.WriteAllText(Application.streamingAssetsPath  + "\\textTrainedPlates.txt", createText);
	}

	private void CalcualteBorders()
	{
		for (int y = 0; y < height; y++)
		{		
			for (int x = 0; x < width; x++)
			{
				if (matrix[x, y] == 1 && isBorder(x, y))
				{
					ContourPath.Add(new Contour());
					CreatePath( x, y, ContourPath[ContourPath.Count - 1] );
				}
			}
		}
	}

	private void CreatePath(int x, int y, Contour pathToSave)
	{
		matrix[x, y] = -1f;

		pathToSave.AddPoint(x, y);

		if (x < width -1 && y < height -1 && x > 0 && y > 0) 
		{
			if (matrix[x+1, y] == 1)
			{
				if (isBorder( x + 1, y))
				{
					CreatePath(x + 1, y, pathToSave);
				}
				else
				{
					int nextBorderValue = IsABorderContinued(x + 1, y);

					if(nextBorderValue != 0)
					{
						if (nextBorderValue == 1) CreatePath(x + 2, y, pathToSave);
						if (nextBorderValue == 2) CreatePath(x + 1 , y + 1, pathToSave);
						if (nextBorderValue == -2) CreatePath(x + 1, y - 1, pathToSave);
					}
				}
			}

			if (matrix[x-1, y] == 1)
			{
				if (isBorder( x - 1, y) )
				{
					CreatePath(x - 1, y, pathToSave);
				}
				else
				{
					int nextBorderValue = IsABorderContinued( x - 1, y);
					
					if(nextBorderValue != 0)
					{
						if (nextBorderValue == -1) CreatePath( x - 2, y, pathToSave);
						if (nextBorderValue == 2) CreatePath( x - 1 , y + 1, pathToSave);
						if (nextBorderValue == -2) CreatePath( x - 1, y - 1, pathToSave);
					}
				}
			}

			if ( matrix[x, y+1] == 1)
			{
				if (isBorder( x, y + 1))
				{
					CreatePath( x, y + 1, pathToSave);
				}
				else
				{
					int nextBorderValue = IsABorderContinued( x, y + 1);
					
					if(nextBorderValue != 0)
					{
						if (nextBorderValue == 1) CreatePath( x + 1, y + 1, pathToSave);
						if (nextBorderValue == -1) CreatePath( x - 1, y + 1, pathToSave);
						if (nextBorderValue == 2) CreatePath( x , y + 2, pathToSave);
					}
				}
			}

			if (matrix[x, y-1] == 1)
			{
				if (isBorder(x, y - 1))
				{
					CreatePath(x, y - 1, pathToSave);
				}
				else
				{
					int nextBorderValue = IsABorderContinued(x, y - 1);
					
					if(nextBorderValue != 0)
					{
						if (nextBorderValue == 1) CreatePath(x + 1, y - 1, pathToSave);
						if (nextBorderValue == -1) CreatePath(x - 1, y - 1, pathToSave);
						if (nextBorderValue == -2) CreatePath( x , y - 2, pathToSave);
					}
				}
			}
		}
	}

	private int IsABorderContinued(int x,int y)
	{
		if (x >= width -1 || y >= height -1 || x <= 0 || y <= 0) return 0;

		if (matrix[x+1, y] == 1)
		{
			if (isBorder(x + 1, y))
			{
				return 1;
			}
		}

		if (matrix[x-1, y] == 1)
		{
			if (isBorder(x - 1, y) )
			{
				return -1;
			}
		}

		if ( matrix[x, y+1] == 1)
		{
			if (isBorder(x, y + 1))
			{
				return 2;
			}
		}

		if (matrix[x, y-1] == 1)
		{
			if (isBorder(x, y - 1))
			{
				return -2;
			}
		}

		return 0;
	}

	private bool isBorder(int x,int y)
	{
		if (x >= width -1 || y >= height -1 || x <= 0 || y <= 0) return false;

		if (matrix[x-1, y] == 0)
		{
			return true;
		}
		else if (matrix[x+1, y] == 0)
		{
			return true;
		}
		else if (matrix[x, y-1] == 0)
		{
			return true;
		}
		else if (matrix[x, y+1] == 0)
		{
			return true;
		}

		return false;
	}

	private Color[] ConvertGrayScaleMatrixToPixelArray()
	{
		int x = 0, y = 0;
		Color[] spritePixels = new Color[width * height];
		
		for (int i = 0; i < spritePixels.Length; i++)
		{		
			if (matrix[x,y] == -1f)
			{
				spritePixels[i] = Color.red;
			}
			else if (matrix[x,y] == -2f)
			{
				spritePixels[i] = Color.blue;
			}
			else if (matrix[x,y] == 1)
			{
				spritePixels[i] = Color.black;
			}
			else
			{
				spritePixels[i] = Color.white;
			}
			
			if ( (i+1) % width == 0 )
			{
				y++;
				x = 0;
			}
			else
			{
				x++;
			}
		}
		 
		return spritePixels;
	}

	private void ConvertPixelArrayToGrayScaleMatrix(Color[] spritePixels)
	{		
		matrix = new float[width, height];
		int y = 0;
		int x = 0;

		for (int i = 0; i < spritePixels.Length; i++)
		{
			float grayScaleValue = spritePixels[i].grayscale;
			
			if (grayScaleValue > Threshold)
			{
				matrix[x, y] = 0;
			}
			else
			{
				matrix[x, y] = 1;
			}
			
			if ( (i+1) % width == 0 )
			{
				y++;
				x = 0;
			}
			else
			{
				x++;
			}
		}
	}

	#endregion
}