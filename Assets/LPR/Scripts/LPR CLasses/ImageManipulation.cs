using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Rrit contrastin, Rregullon vleren Gamma, Filtron zhurmat me Median, Binarizon figuren ne 0 1
[System.Serializable]
public class ImageManipulation
{
	#region PRIVATE_FIELDS
	// Width / Height te figures
	[SerializeField]
	private int ImageWidth;
	[SerializeField]
	private int ImageHeight;
	// Shpteon localisht Vleren Gamma
	private float initialGammaCorrectionValue;
	// Matrica e Figures
	private float[,] ImageMatrix;
	// Velrat Grayscale te rregulluara te figures
	private List<float> GrayScaleImageArray;

	#endregion

	#region PUBLIC_METHODS
	// Konstruktor per Width dhe Height
	public ImageManipulation(int ImageWidth, int ImageHeight)
	{
		this.ImageWidth = ImageWidth;
		this.ImageHeight = ImageHeight;
		GrayScaleImageArray = new List<float>();
	}
	// Konvertimi i figures me ngjyra ne figure binare
	public void ConvertPixelArrayToBynarMatrix(Color[] spritePixels, float ContrastValue, float GammaCorrectionValue, int MedianGroupPixel, float BynarizationThreshold)
	{	
		// Rregullon konstratin dge velren Gamma
		ChangeImageContrast( spritePixels, ContrastValue, GammaCorrectionValue );
		// krijohet matrica
		ImageMatrix = new float[ImageWidth, ImageHeight];
		// sherben per konvertim nga vector 1 dim ne 2 dim
		int y = 0;
		int x = 0;
		// per cdo element grayScale
		for (int i = 0; i < GrayScaleImageArray.Count; i++)
		{
			// Nqs Vlera Median eshte 1, binarizo pa filtruar
			if (MedianGroupPixel == 1)
			{
				ImageMatrix[x, y] = (GrayScaleImageArray[i] > BynarizationThreshold) ? 0 : 1;
			}
			else
			{
				// Per filtrim
				ImageMatrix[x, y] = GrayScaleImageArray[i];
			}
			// Numeron rreshtat dhe kollonat
			if ( (i+1) % ImageWidth == 0 )
			{
				y++;
				x = 0;
			}
			else
			{
				x++;
			}
		}
		// Filtrim te zhuramve
		MedianFilter( MedianGroupPixel, BynarizationThreshold );
	}
	
	public void ChangeImageContrast(Color[] spritePixels, float ContrastValue, float GammaCorrectionValue)
	{
		// Vendosjen te vlerave te kontrastit
		byte[] contrastLookup = new byte[256];
		double newValue = 0;
		double c = (100f + ContrastValue) / 100f;
		
		c *= c;
		
		for (int i = 0; i < 256; i++)
		{
			newValue = (double)i;
			newValue /= 255.0;
			newValue -= 0.5;
			newValue *= c;
			newValue += 0.5;
			newValue *= 255;
			
			if (newValue < 0)
				newValue = 0;
			
			if (newValue > 255)
				newValue = 255;
			
			contrastLookup[i] = (byte)newValue;
		}
		// Per cdo pixel te figures
		for (int i = 1; i < spritePixels.Length ; i++)
		{
			// Merret vlera grayScale
			float grayscale = spritePixels[i].grayscale ;
			// Rregullohet Gamma
			grayscale = Mathf.Pow( grayscale , 1f / GammaCorrectionValue ) * 255f;		
			// Rregullohet Contrasti
			grayscale = (float)contrastLookup[ (int)grayscale ] / 255f;
			// Shpetohet vlera ose mbishkruhet nqs ka ndryshime te medha
			if (GrayScaleImageArray.Count < spritePixels.Length)
			{
				GrayScaleImageArray.Add( grayscale );
			}
			else
			{
				if ( Mathf.Abs( GrayScaleImageArray[i] - grayscale) > .1f || initialGammaCorrectionValue != GammaCorrectionValue) GrayScaleImageArray[i] = grayscale;
			}
		}
		// Shpetohet vlera Gamma
		initialGammaCorrectionValue = GammaCorrectionValue;
	}
	// Filter, bene nje blur te Imazhit
	public void MedianFilter(int MedianGroupPixel, float BynarizationThreshold)
	{
		// E aplikojm vetem nqs vlera eshte > 1
		if (MedianGroupPixel != 1)
		{
			// Krijojm nje matice temporale
			float[,] tempMedian = ImageMatrix;
			// Per co vlere te matrices
			for (int i = 0; i < ImageWidth; i ++)
			{
				for (int j = 0; j < ImageHeight; j ++)
				{
					// Shuma e grayscale dhe sa pixel meren ne shqyrtim
					float sum = 0f;
					int counter = 0;
					// Per ulje rezolucioni behet : y,x += MedianGroupPixel
					for (int x = - MedianGroupPixel / 2 ; x < MedianGroupPixel / 2 ; x ++)
					{
						for (int y = - MedianGroupPixel / 2 ; y < MedianGroupPixel / 2 ; y ++)
						{
							// Marrim te gjitha pixelat qe rrethojn qelizen ne poiscionin i, j dhe bejm shumen
							if ( i + x > 0 && i + x < ImageWidth && j + y > 0 && j + y < ImageHeight)
							{
								sum += ImageMatrix[ i + x, j + y];
								counter ++;
							}
						}
					}
					// ne poz i, j vendosim mesataren e velres
					tempMedian[i, j] = sum / (float)counter;
				}
			}
			// Vendosim vlerat e binarizuara
			for (int i = 0; i < ImageWidth; i ++)
			{
				for (int j = 0; j < ImageHeight; j ++)
				{
					ImageMatrix[i, j] = (tempMedian[i, j] > BynarizationThreshold) ? 0 : 1;
				}
			}
		}
	}
	// Convertojm Matricen ne nje Figure
	public Color[] ConvertBynariMatrixToPixelArray()
	{
		int x = 0, y = 0;
		Color[] spritePixels = new Color[ImageWidth * ImageHeight];
		
		for (int i = 0; i < spritePixels.Length; i++)
		{		
			if (ImageMatrix[x,y] == -1f)
			{
				spritePixels[i] = Color.red;
			}
			else if (ImageMatrix[x,y] == -3f)
			{
				spritePixels[i] = Color.green;
			}
			else if (ImageMatrix[x,y] == -2f)
			{
				spritePixels[i] = Color.blue;
			}
			else if (ImageMatrix[x,y] == 1)
			{
				spritePixels[i] = Color.black;
			}
			else
			{
				spritePixels[i] = Color.white;
			}
			
			if ( (i+1) % ImageWidth == 0 )
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
	// Kthen Matricen e manipuluar
	public float[,] GetMatrix()
	{
		return ImageMatrix;
	}

	public int GetWidth()
	{
		return ImageWidth;
	}

	public int GetHeight()
	{
		return ImageHeight;
	}

	#endregion
}
