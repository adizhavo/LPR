using UnityEngine;
using System.Linq;
using System.Collections.Generic;
[System.Serializable]
public class PlateLocalization 
{
	#region PRIVATE_FIELDS
	// Width && Height te figures
	[SerializeField]
	private int ImageWidth;
	[SerializeField]
	private int ImageHeight;
	// Konturet 
	private List<Contour> ContourPath = new List<Contour>();
	// GRupi i kodeve me mundesi me te larte per te qene nje targe
	private PossiblePlateChar PossiblePlate;
	// Klasa qe permban informacione rreth fjaleve te databases
	private GetWordDatabase WordsData;

	public void SetGetWordDatabase(GetWordDatabase WordsData)
	{
		this.WordsData = WordsData;
	}
	// Matrica te pixelave te figures
	private float[,] ImageMatrix;
	// Madesite minimale dhe maksimale qe nje kode te kosiderohet germ
	private Vector2 DimensionOfPlateCharX_Min_Max;
	private Vector2 DimensionOfPlateCharY_Min_Max;

	public delegate void LettersTracked(string letters);
	public static event LettersTracked OnLettersTracked;

	#endregion

	#region PUBLIC_METHODS
	// Kosntruktor per te marr Width && Height dhe loadimi i databases
	public PlateLocalization(int ImageWidth, int ImageHeight, OpenFileDatabase LoadData)
	{
		this.ImageWidth = ImageWidth;
		this.ImageHeight = ImageHeight;

		LoadData.OpenFile(this);
	}
	// Inicializon klasen me variabla mbi te cilen do bazohet njohja
	public void InitializePlateLocalizationParameters(List<Contour> ContourPath, float[,] ImageMatrix, Vector2 DimensionOfPlateCharX_Min_Max, Vector2 DimensionOfPlateCharY_Min_Max)
	{
		this.ContourPath = ContourPath;
		this.ImageMatrix = ImageMatrix;
		this.DimensionOfPlateCharX_Min_Max = DimensionOfPlateCharX_Min_Max;
		this.DimensionOfPlateCharY_Min_Max = DimensionOfPlateCharY_Min_Max;	

	}
	// Lokalizon Kodet me te mundshme per te qene nje Targe
	// Grupon kodet e ngjashme dhe  me vone filtrohen
	public void LocalizePlateString()
	{
		// Gjene pikat Max, Min qe perbejn konturin qe do permbaje nje kod
		ProcessedContour GetContourResultsProcess = FindBoundBoxOfAContour( ContourPath );	
		// Shpetohen localisht keto konture
		List<Vector2> MaxPoints = GetContourResultsProcess.MaxPoints;
		List<Vector2> MinPoints = GetContourResultsProcess.MinPoints;
		// Dhe Kodet
		List<Contour> CodeContour = GetContourResultsProcess.SavedCont;
		
		List<PossiblePlateChar> PossiblePlateGroups = new List<PossiblePlateChar>();
		
		List<int> DrawsIndexes = new List<int>();
		List<int> NumberOfSimilarity = new List<int>();
		// Per cdo kontur
		for (int i = 0; i < MaxPoints.Count ; i++)
		{
			// Nqs ky kontur nk eshte procesuar
			if (!DrawsIndexes.Contains(i))
			{
				DrawsIndexes.Add(i);
				NumberOfSimilarity.Add(1);
				// Shtohet konturi se kodi nje kode qe nk ngjason me asnje tjeter
				PossiblePlateGroups.Add ( new PossiblePlateChar( i, MaxPoints[i], MinPoints[i], CodeContour[i] ) );
			}
			// Per cdo konture
			for (int j = 0; j < MaxPoints.Count ; j++)
			{
				// Llogarisim gjeresin te konturave qe do krahasohen
				float yDistanceCurrent = Vector2.Distance(new Vector2(0f, MinPoints[i].y), new Vector2(0f, MaxPoints[i].y));
				float yDistanceTesting = Vector2.Distance(new Vector2(0f, MinPoints[j].y), new Vector2(0f, MaxPoints[j].y));
				// Llogarisim Ratio
				float ratio =  yDistanceCurrent / yDistanceTesting ;
				// Raposti i ngjashmerise duhet te jete brenda vlerave 
				if (ratio > .8f && ratio < 1.2f && i != j)
				{
					// Nqs konturi qe po krahasohet nk eshte procesuar ose pjese te nje grupi
					if (!DrawsIndexes.Contains(j))
					{
						DrawsIndexes.Add(j);
						NumberOfSimilarity.Add(1);
						// Shtohet ky kontur tek grupi i ngajshmerive
						PossiblePlateGroups.Add ( new PossiblePlateChar( j, MaxPoints[j], MinPoints[j], CodeContour[j] ) );
						// Shtohen elementet te ngjshme te nje indexi
						NumberOfSimilarity[ DrawsIndexes.IndexOf( i ) ] ++;
						PossiblePlateGroups[ DrawsIndexes.IndexOf( i ) ].AddPattern( j, MaxPoints[j], MinPoints[j], CodeContour[j] );
					}
				}
			}
		}
		// Nqs kemi konture te ngjashme
		if (NumberOfSimilarity.Count > 0)
		{
			// Gjem ate me vleren maximale
			int MaxIndex = NumberOfSimilarity.IndexOf( NumberOfSimilarity.Max() );
			// Shpetojm grupin e kodeve me m shume ngjashmeri
			PossiblePlate = PossiblePlateGroups[MaxIndex];
			// Filtrojm kodet
			PossiblePlate.ProcessTheSimilarCode();
		}
	}
	// Rregullon prespektive dhe rrotullimin te nje grup konturesh
	public void SlantCorrection(float SlantThreshold)
	{
		// Nqs kemi nje potencial targe
		if (PossiblePlate != null)
		{
			// Vlerat qe do permbajn indekset te 1 corrected, 0 blancked
			List<Vector3> CorrectedValues = new List<Vector3>(); 
			List<Vector2> BlanckedValues = new List<Vector2>();
			// Marrim rreshtat te kesaj targe (mudn te jene dhe 2)
			List<Rows> PlateRows = PossiblePlate.GetPlateRows();
			// Per cdo rresht
			for (int i = 0; i <  PlateRows.Count; i++)
			{
				// Marrim kendin e prespektives
				float slantAngle = PlateRows[i].GetSlantAngle();
				// Nqs kalon nje Thresholde
				if (Mathf.Abs( slantAngle ) > SlantThreshold)
				{
					// Marrim kendin ne radioan
					slantAngle *= (Mathf.PI / 180f);
					// Qendren e rrotullimit
					Vector2 centerOfSlantCorrection = PlateRows[i].GetCenterOfRotation();
					// Listen te kotureve qe permbejn kete rresht
					List<Vector2> MaxPoints = PlateRows[i].GetMaxList();
					List<Vector2> MinPoints = PlateRows[i].GetMinList();
					List<Contour> RotatedContour = new List<Contour>();
					// Per cdo kontur
					for (int j = 0; j < MaxPoints.Count; j++)
					{
						// Shtojm konturin e procesuar
						RotatedContour.Add(new Contour());
						
						for (int x = (int)MinPoints[j].x; x <= MaxPoints[j].x; x++)
						{
							for (int y = (int)MinPoints[j].y; y <= MaxPoints[j].y; y++)
							{
								// Llogarisim koordinatat e rrotulluara
								Vector2 rotatedPoint = RotatePoint( (float) x, (float) y, centerOfSlantCorrection, slantAngle);
								// Shpetojm pikat qe do ndryshohen dhe pikat e ndryshuara
								CorrectedValues.Add( new Vector3( rotatedPoint.x, rotatedPoint.y, ImageMatrix[x, y]) );
								BlanckedValues.Add( new Vector3(x, y) );
								// Rrotullojm dhe konturet me vlere -1
								if (ImageMatrix[x, y] == -1)
								{
									RotatedContour[ RotatedContour.Count - 1 ].AddPoint((int)rotatedPoint.x, (int)rotatedPoint.y);
								}
							}
						}					
					}
					// Gjene pikat Max, Min qe perbejn konturin qe do permbaje nje kod
					ProcessedContour reprocessBoundBoxAfterRotation = FindBoundBoxOfAContour( RotatedContour );
					// Mbivendosim rreshtat
					PlateRows[i].SetRows(reprocessBoundBoxAfterRotation.MaxPoints,
					                     reprocessBoundBoxAfterRotation.MinPoints,
					                     reprocessBoundBoxAfterRotation.SavedCont); 
				}			
			}

			#region DRAWING_FUNCTIONS

			try
			{
				// Mbishkruajm grafikisht imazhin
				for (int i = 0; i < BlanckedValues.Count; i++)
				{
					ImageMatrix[(int)BlanckedValues[i].x, (int)BlanckedValues[i].y] = 0;
				}
				
				for (int i = 0; i < CorrectedValues.Count; i++)
				{
					ImageMatrix[(int)CorrectedValues[i].x, (int)CorrectedValues[i].y] = CorrectedValues[i].z;
				}
			}
			catch
			{
				Debug.Log("Error Correcting Slant");
			}

			#endregion
		}
	}
	// Zgjedhim Germen qe i perafrohet me shume kodit
	public void CalculateStringAccuracy()
	{		
		if (PossiblePlate != null)
		{
			string TrackedPlate = string.Empty;
			// Marrim rreshtat
			List<Rows> PlateRow = PossiblePlate.GetPlateRows();
			// Per cdo rresht
			for (int k = 0; k < PlateRow.Count; k++)
			{
				// Marrim konturet te kodeve
				List<Vector2> MaxPoints = PlateRow[k].GetMaxList();
				List<Vector2> MinPoints = PlateRow[k].GetMinList();
				// Per cdo kontur
				for (int i = 0; i < MinPoints.Count; i++)
				{
					Vector2 minXY = MinPoints[i];
					Vector2 maxXY = MaxPoints[i];
					
					int maxValue = -1;

					List<float> percentageMagnitude = new List<float>();
					// Per cdo fjale e marr nga database
					for (int j = 0; j < WordsData.WordsBynaryValues.Count; j++)
					{
						// Marrim vektorin e saktesise
						Vector2 tempPercentageValue = PresicionData(minXY, maxXY, WordsData.WordsHeight[j], WordsData.WordsWidth[j], WordsData.WordsBynaryValues[j]);
						// Nje filtrim minimal
						//if (tempPercentageValue.x > 20f && tempPercentageValue.y > 20f)
						{
							// Shtojm hipotenuzen e ketij vektori
							percentageMagnitude.Add( tempPercentageValue.magnitude );
						}
					}

					try
					{
						// Marrim indeksin te vleres maksimale
#if UNITY_EDITOR
						maxValue = percentageMagnitude.IndexOf( percentageMagnitude.Max() );		
#else
						maxValue = GetIndexOfMax( percentageMagnitude );
#endif
						
						if (maxValue != -1)
						{
						#region DRAWING_FUNCTION
							// Visatojm germen qe mbush kodin
							PaintFilter(minXY, maxXY, WordsData.WordsHeight[maxValue], WordsData.WordsWidth[maxValue], WordsData.WordsBynaryValues[maxValue]);						
						#endregion
							// Shpeton germen
							TrackedPlate += WordsData.WordsChar[maxValue] ;
						}
					}
					catch
					{
						Debug.LogError("Invalind Object State");
					}
				}

				TrackedPlate += ";";
			}
			// Leshon eventin me stringun e targes
			if (OnLettersTracked != null)
			{
				OnLettersTracked(TrackedPlate);
			}
		}
	}
	// Vizaton vizen ndermjet kodeve te nje rreshtit
	public void DrawArrangedPossiblePlate()
	{
		if (PossiblePlate != null)
		{
			PossiblePlate.DrawCharLine(ImageMatrix);
		}
	}
	// Vizaton katrori mbajtes te nje konturi
	public void DrawContour()
	{			
		if (PossiblePlate != null)
		{
			List<Rows> PlateRow = PossiblePlate.GetPlateRows();
			// Per cdo rresht
			for (int j = 0; j < PlateRow.Count; j++)
			{
				List<Vector2> Max = PlateRow[j].GetMaxList();
				List<Vector2> Min = PlateRow[j].GetMinList();				
				// Per cdo pike Max e min te Katrorit
				for (int i = 0; i < PlateRow[j].GetSize(); i++)
				{
					try
					{
						for (int k = (int)Min[i].x; k < Max[i].x; k++)
						{
							ImageMatrix[k, (int)Max[i].y] = ImageMatrix[k, (int)Min[i].y] = -2;
						}
						
						for (int k = (int)Min[i].y; k < Max[i].y; k++)
						{
							ImageMatrix[(int)Max[i].x, k] = ImageMatrix[(int)Min[i].x, k] = -2;
						}
					}
					catch
					{
						Debug.LogError("Error Drawing Squares");
					}
				}
			}
		}
	}

	#endregion

	#region PRIVATE_METHODS
	// Marrim indeksin te vleres me te madhe dhe nuk perdorim librarin Linq
	private int GetIndexOfMax(List<float> sampleList)
	{
		float max = - Mathf.Infinity;
		int index = -1;
		
		for (int i = 0; i < sampleList.Count; i++)
		{
			if (sampleList[i] > max)
			{
				max = sampleList[i];
				index = i;
			}
		}
		
		return index;
	}
	// Gjen katrorin mbajtes te nje konturi
	private ProcessedContour FindBoundBoxOfAContour(List<Contour> CodeContour)
	{
		// Do permbaje te gjitha informacionet
		ProcessedContour results = new ProcessedContour();
		// Per cdo kontur
		for (int cont = 0; cont < CodeContour.Count; cont ++)
		{
			// Merren pikat X & Y te konturit
			List<int> xVal = CodeContour[cont].GetXValue();
			List<int> yVal = CodeContour[cont].GetYValue();
			// Vendosen vlerat mnimale dhe maksimale qe do mbishkruhen
			Vector2 maxXY = new Vector2(-1, -1), minXY = new Vector2(ImageWidth + 1, ImageHeight + 1);
			// Per cdo vlere te X, gjem maksimumin dhe minimumin
			for (int i = 0; i < xVal.Count; i++)
			{
				if (xVal[i] >= maxXY.x)
				{
					maxXY.x = xVal[i];
				}
				
				if (yVal[i] >= maxXY.y )
				{
					maxXY.y = yVal[i];
				}
				
				if (xVal[i] <= minXY.x )
				{
					minXY.x = xVal[i];
				}
				
				if (yVal[i] <= minXY.y )
				{
					minXY.y = yVal[i];
				}
			}
			// Llogarisim distancen , gjeresin e lartesin
			int distanceMinMaxX = (int)Vector2.Distance(new Vector2(minXY.x, 0f), new Vector2(maxXY.x, 0f));
			int distanceMinMaxY = (int)Vector2.Distance(new Vector2(0f, minXY.y), new Vector2(0f, maxXY.y));
			// Filtrim ne varesi te distances (gjeresi dhe lartesi), vleres x y, germa eshte me e gjate se e gjere
			if (distanceMinMaxX > DimensionOfPlateCharX_Min_Max.x
			    && distanceMinMaxY > DimensionOfPlateCharY_Min_Max.x
			    && distanceMinMaxX < DimensionOfPlateCharX_Min_Max.y
			    && distanceMinMaxY < DimensionOfPlateCharY_Min_Max.y
			    && distanceMinMaxY > distanceMinMaxX
			    && (int)minXY.y > 0
			    && (int)minXY.x > 0
			    && (int)maxXY.y > 0
			    && (int)maxXY.x > 0)
			{
				
				results.MaxPoints.Add( maxXY );
				results.MinPoints.Add( minXY );
				results.SavedCont.Add( CodeContour[cont] );
			}
		}
		// Kthejem rezultatet
		return results;
	}
	// Llogarisim perqindjen e saktesise te germes kod dhe ate ne databas
	private Vector3 PresicionData(Vector2 minXY, Vector2 maxXY, int heights, int widths, List<int> Values)
	{
		float precision = 0f;
		// Marrim gjeresin te gjermes dhe kodit
		int currentRectHeigth = Mathf.Abs ((int)(maxXY.y - minXY.y));
		int currentRectWidth = Mathf.Abs ((int)(maxXY.x - minXY.x));
		// Raportet e gjatsise dhe gjeresise
		float heightRatio = (float)currentRectHeigth / (float)heights;
		float widthRatio = (float)currentRectWidth / (float)widths;

		int portion = 0;
		int valuedPointsOfMapped = 0;
		int valuedPointsOfCaptured = 0;
		
		float x = 0f;
		float y = 0f;
		// Nqs germa nk eshte bosh
		if (currentRectHeigth > 0 && currentRectWidth > 0)
		{
			// Per cdo vlere te germes ne databases
			for (int i = 0; i < Values.Count; i++)
			{
				// Konvertim te Array 1D ne Array 2D
				if (i - portion < widths)
				{					
					x += widthRatio;
				}
				else
				{
					portion += widths;
					x = 0;
					y += heightRatio;
				}
				// eshte kapur ndonje vlere jo 0 ?
				if (minXY.x < ImageWidth 
				    && minXY.y < ImageHeight 
				    && maxXY.x < ImageWidth 
				    && maxXY.y < ImageHeight
				    && minXY.x > 0f 
				    && minXY.y > 0f
				    && maxXY.x > 0f
				    && maxXY.y > 0f)
				{
					bool found = (ImageMatrix[(int)minXY.x + (int)x, (int)minXY.y + (int)y] == 0) ? false : true ;
					
					if (found)
					{
						// Shtojm vleren
						precision += Values[i];
						// Llogarisim pikat e kapura te kodit
						valuedPointsOfCaptured++;
					}
				}
				
				if (Values[i] != 0)
				{ 
					// Llogarisim pikat jo 0 te germes ne database
					valuedPointsOfMapped ++; 
				}
			}
		}
		// Llogarisim perqeindjet te ngjashmerise ndermjet germes ne databse dhe kodit
		int valuePercentageMapped = (int)( (precision / (float)valuedPointsOfMapped) * 100f);
		int valuePercentageCaptured = (int)( (precision / (float)valuedPointsOfCaptured) * 100f);
		// Bejm nje Clamp
		if (valuePercentageMapped > 100 || valuePercentageMapped < 0) valuePercentageMapped = 0;
		if (valuePercentageCaptured > 100 || valuePercentageCaptured < 0) valuePercentageCaptured = 0;
		// Krijojm vektorin
		Vector2 percentages = new Vector2(valuePercentageMapped, valuePercentageCaptured);
		// Kthejm te dyja vlerat
		return percentages;
	}
	// Llogjike per afishimin vizual te germes qe mbushet me perqindje me te larte
	private void PaintFilter(Vector2 minXY, Vector2 maxXY, int heights, int widths, List<int> Values)
	{
		int currentRectHeigth = Mathf.Abs ((int)(maxXY.y - minXY.y));
		int currentRectWidth = Mathf.Abs ((int)(maxXY.x - minXY.x));
		
		float heightRatio = (float)currentRectHeigth / (float)heights;
		float widthRatio = (float)currentRectWidth / (float)widths;
		
		int portion = 0;
		
		float x = 0f;
		float y = 0f;
		
		if (currentRectHeigth > 0 && currentRectWidth > 0)
		{
			for (int i = 0; i < Values.Count; i++)
			{
				if (i - portion < widths)
				{					
					x += widthRatio;
				}
				else
				{
					portion += widths;
					x = 0;
					y += heightRatio;
				}
				
				bool found = (ImageMatrix[(int)minXY.x + (int)x, (int)minXY.y + (int)y] == 0) ? false : true ;
				
				if (found)
				{					
					if (Values[i] != 0)
					{
						ImageMatrix[(int)minXY.x + (int)x, (int)minXY.y + (int)y] = -2;
					}
				}
				else if (Values[i] != 0) 
				{
					ImageMatrix[(int)minXY.x + (int)x, (int)minXY.y + (int)y] = -1;
				}
			}
		}
	}	
	// Llogaritja te rrotullimit te nje pike
	private Vector2 RotatePoint( Vector2 StartPos, Vector2 CenterOfRotation, float Angle )
	{
		return RotatePoint(StartPos.x, StartPos.y, CenterOfRotation, Angle);
	}
	
	private Vector2 RotatePoint( float x, float y, Vector2 CenterOfRotation, float Angle )
	{
		float localX = x - CenterOfRotation.x;
		float localY = y - CenterOfRotation.y;
		
		float xPrim = localX * Mathf.Cos(Angle) - localY * Mathf.Sin(Angle) + CenterOfRotation.x;
		float yPrim = localX * Mathf.Sin(Angle) + localY * Mathf.Cos(Angle) + CenterOfRotation.y;
		
		return new Vector2(xPrim, yPrim);
	}

	#endregion
}
