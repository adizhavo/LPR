using UnityEngine;
using System.Collections.Generic;

public class PossiblePlateChar
{
	#region PRIVATE_FIELDS
	// Lista me te gjitha indekset te kodeve
	private List<int> AllignedIndexes = new List<int>();
	// List qe numeron sa kode te ngjashme eksistojn per ate indeks
	private List<int> SimilarPatterns = new List<int>();
	// Pikat qe perbejn drejtekendeshin qe perfshin kodin
	private List<Vector2> MaxPoints = new List<Vector2>();
	private List<Vector2> MinPoints = new List<Vector2>();
	// Rreshtat me kodet qe mund te jene pjese te targes
	private List<Rows> PotentialPlateRows = new List<Rows>();
	// Konturet te nje kodi
	private List<Contour> CodeContours = new List<Contour>();

	#endregion

	#region PUBLIC_METHODS
	// Konstruktor per inicializimin fillestar
	public PossiblePlateChar(int startPattern, Vector2 MaxPoint, Vector2 MinPoint, Contour newCont)
	{
		AddPattern(startPattern, MaxPoint, MinPoint, newCont);
	}
	// Shtimi ne fund te listes te elementeve
	public void AddPattern(int similarPatterns, Vector2 MaxPoint, Vector2 MinPoint, Contour CodeContours)
	{
		this.SimilarPatterns.Add( similarPatterns );
		this.MaxPoints.Add( MaxPoint );
		this.MinPoints.Add( MinPoint );
		this.CodeContours.Add( CodeContours );
	}
	// Eshte kjo vlere e shpetuar me perpara
	public bool IsPatternStored(int SimilarPatterns)
	{
		return this.SimilarPatterns.Contains( SimilarPatterns );
	}
	// Kthene madhesin e elementave
	public int GetSize()
	{
		return SimilarPatterns.Count;
	}
	// Kthen pikat Max qe perbejn drejtekendeshin qe perfshin kodin
	public List<Vector2> GetMaxList()
	{
		return MaxPoints;
	}
	// Kthen pikat Min qe perbejn drejtekendeshin qe perfshin kodin
	public List<Vector2> GetMinList()
	{
		return MinPoints;
	}
	// Kthen rreshtat qe perbehet nga nje grup kodesh
	public List<Rows> GetPlateRows()
	{
		return PotentialPlateRows;
	}
	// Kthen konturet te kodeve
	public List<Contour> GetCodeContours()
	{
		return CodeContours;
	}
	// Mbishkruan rreshtat te kodeve
	public void SetPlateRows(List<Rows> PotentialPlateRows)
	{
		this.PotentialPlateRows = PotentialPlateRows;
	}
	// Hapat pe procesimin te kodeve te ngjashme per te krijuar rreshtat
	public void ProcessTheSimilarCode()
	{
		SortTheChars();
		CreateRows();
		SortTheRows();
		FilterCodeNoises();
		FindSlantAngle();
	}	
	// Visaton nje vize qe percakton grupin e rreshtave
	public void DrawCharLine(float[,] matrix) 
	{
		float[,] tempMatrix = matrix;
		// Ekzekutohet per cdo rresht qe eksiston
		for (int i = 0; i <  PotentialPlateRows.Count; i++)
		{
			// Merren pikat qe perbejn drejtekendeshin qe perfshin kodin
			List<Vector2> MaxPoint = PotentialPlateRows[i].GetMaxList();
			List<Vector2> MinPoint = PotentialPlateRows[i].GetMinList();
			// Per cdo drejtkendesh
			for (int j = 0; j < PotentialPlateRows[i].GetSize() - 1; j++)
			{
				// Merren qendrat te dy drejtkendshave te njepasnjeshme
				Vector2 centerPointCurrent = (MinPoint[j] - MaxPoint[j]) / 2f + MaxPoint[j];
				Vector2 centerPointTest = (MinPoint[j+1] - MaxPoint[j+1]) / 2f + MaxPoint[j+1];
				// Check si eshte leximi i kodit, sensi
				bool directionLeft = (centerPointTest.x > centerPointCurrent.x);
				
				if (directionLeft)
				{
					for (int k = (int)centerPointCurrent.x; k < centerPointTest.x; k++)
					{
						// Ne varesi te vleres X mbishkruajm dhe vleren Y
						float xDist = (float)k - centerPointCurrent.x;
						float DistancePercentage = xDist / (centerPointTest.x - centerPointCurrent.x);
						float yDistance = DistancePercentage * (centerPointTest.y - centerPointCurrent.y);
						int y = (int)( centerPointCurrent.y + yDistance);
						
						try
						{
							// Ngjyre jeshile
							tempMatrix[k, y] = -3;
						}
						catch
						{
							Debug.LogError("Line Outside the image Width * Height");
						}
					}
				}
				else
				{
					for (int k = (int)centerPointCurrent.x; k > centerPointTest.x; k--)
					{
						float xDist = (float)k - centerPointTest.x;
						float DistancePercentage = xDist / (centerPointCurrent.x - centerPointTest.x);
						float yDistance = DistancePercentage * (centerPointCurrent.y - centerPointTest.y);
						int y = (int)( centerPointTest.y + yDistance);
						
						try
						{
							tempMatrix[k, y] = -1; 
						}
						catch
						{
							Debug.LogError("Line Outside the image Width * Height");
						}
					}
				}
			}
		}
	}

	#endregion

	#region PRIVATE_METHODS
	// Rreshton rreshtat sipas koordinates Y
	private void SortTheRows()
	{
		// Eshte perdorur Bubble Sort (pasi nr i rreshtave esht i ulet)
		for (int i = 0; i < PotentialPlateRows.Count - 1 ; i++)
		{
			for (int j = i + 1; j < PotentialPlateRows.Count; j++)
			{					
				if (PotentialPlateRows[i].GetCenterOfRows().y < PotentialPlateRows[j].GetCenterOfRows().y)
				{
					Rows tempRow = PotentialPlateRows[i];						
					PotentialPlateRows[i] = PotentialPlateRows[j];						
					PotentialPlateRows[j] = tempRow;
				}
			}
		}
	}
	// Rreshton kodet sipas koordinates X
	private void SortTheChars()
	{
		// Per cdo drejtekendesh
		// Eshte perdorur Bubble Sort (pasi nr i kodeve esht i ulet)
		for (int i = 0; i < GetSize() -1 ; i++)
		{
			for (int j = i + 1; j < GetSize(); j++)
			{
				// Marrim qendren e drejtekendeshit
				Vector2 centerPointCurrent = (MinPoints[i] - MaxPoints[i]) / 2f + MaxPoints[i];
				Vector2 centerPointTest = (MinPoints[j] - MaxPoints[j]) / 2f + MaxPoints[j];
				
				if (centerPointTest.x < centerPointCurrent.x)
				{
					Vector2 tempMin = MinPoints[i];
					Vector2 tempMax = MaxPoints[i];
					
					MinPoints[i] = MinPoints[j];
					MaxPoints[i] = MaxPoints[j];
					
					MinPoints[j] = tempMin;
					MaxPoints[j] = tempMax;
				}
			}
		}
	}
	// Krijimi i rreshtave
	private void CreateRows()
	{
		// Per cdo element qe eshte shpetuar, per cdo 3 grupe kode
		for (int i = 0; i < GetSize() - 2; i++)
		{
			// Merret qendra e kodit
			Vector2 centerPointCurrent = (MinPoints[i] - MaxPoints[i]) / 2f + MaxPoints[i];
			// Qendra e marr me perpara krahasohet me cdo element tjeter te kodit ( i + 1 )
			for (int j = i + 1; j < GetSize() - 1; j++)
			{
				// Merret qendra e kodit
				Vector2 centerPointTest = (MinPoints[j] - MaxPoints[j]) / 2f + MaxPoints[j];
				// Qendra e marr me perpara krahasohet me cdo element tjeter te kodit ( j + 1 )
				for (int k = j + 1; k <GetSize(); k++)
				{
					// Merret qendra e kodit
					Vector2 centerPoint = (MinPoints[k] - MaxPoints[k]) / 2f + MaxPoints[k];
					// Gjendet kendi ndemjet 3 qendrave te marra me siper ( 3 pika = 2 vektor)
					float angle = Vector2.Angle( centerPointTest - centerPointCurrent, centerPoint - centerPointTest );
					angle = (angle > 180f) ? - (angle - 360f) : angle;
					// kendi me i vogel se 15 grade qe te konsiderohet ne nje rresht
					if ( angle < 15f
					    // Jane indekset e shpetuara me perpara ?
					    && ( !AllignedIndexes.Contains(i) || !AllignedIndexes.Contains(j) || !AllignedIndexes.Contains(k)) 
					    // Jane indekset mjaftueshem afer nj-tj per tu konsideruar pjese e grupit ?
					    &&  Mathf.Abs (centerPointTest.x - centerPointCurrent.x) < Mathf.Abs( MaxPoints[i].x - MinPoints[i].x ) * 4f 
					    &&  Mathf.Abs (centerPoint.x - centerPointTest.x) < Mathf.Abs( MaxPoints[j].x - MinPoints[j].x ) * 4f)
					{
						// Shpeton indeksin dhe shton eleemntin ne rresht nqs nk eshte i pranishem
						if (!AllignedIndexes.Contains(i))
						{
							PotentialPlateRows.Add( new Rows( MaxPoints[i], MinPoints[i], CodeContours[i] ) );
							AllignedIndexes.Add(i);
						}
						// Shpeton indeksin dhe shton eleemntin ne rresht nqs nk eshte i pranishem
						if (!AllignedIndexes.Contains(j))
						{
							PotentialPlateRows[ PotentialPlateRows.Count - 1 ].AddElementsToRows(MaxPoints[j], MinPoints[j], CodeContours[j]);
							AllignedIndexes.Add(j);								
						}
						// Shpeton indeksin dhe shton eleemntin ne rresht nqs nk eshte i pranishem
						if (!AllignedIndexes.Contains(k))
						{
							PotentialPlateRows[ PotentialPlateRows.Count -1 ].AddElementsToRows(MaxPoints[k], MinPoints[k], CodeContours[k]);
							AllignedIndexes.Add(k);
						}
						// Shpetohen vektoret qe te llogaritet perseri kendi ndermjet nje grup 3she
						centerPointCurrent = centerPointTest;
						centerPointTest = centerPoint;
					}
				}
			}
		}
	}
	// Filtrimi i rreshtave, disa grup kodesh mund te jene te shkaktuar nga zhurmat e ambjentit ose stema te jashtme
	// Bahkojm dy rreshtat qe jane shume afer nj-tj (targe me dy rreshta) dhe shuajm rreshtat jasht standarteve
	private void FilterCodeNoises()
	{
		// Filtrojm nqs kemi 2 ose me shume rreshta, zgjedhim me te pershtatshimn
		if (PotentialPlateRows.Count > 1)
		{
			List<Vector2> CentersOfRows = new List<Vector2>(); 
			
			for (int i = 0; i < PotentialPlateRows.Count; i++)
			{
				// Nisim procesin te llogaritjeve te rreshtit
				PotentialPlateRows[i].CalculateRowsPropeties();
				// Shpetojm qendren te rreshtit
				CentersOfRows.Add( PotentialPlateRows[i].GetCenterOfRows() );
			}
			// Per cdo vlere te qendres se rreshtit bejm disa llogaritje
			for (int i = 0; i < CentersOfRows.Count - 1; i++)
			{
				// Duhet te zgjedhim rreshtat qe jane me afer nj-tj dhe qe plotesoj disa kushte
				float distance = Mathf.Infinity;

				for (int j = i + 1; j < CentersOfRows.Count ; j++)
				{
					// nqs rreshtat ne fjale jane te n dryshme nga nj-tj (pasi kemi nje ndryshim indeksi me posht)
					if (i != j)
					{
						// Marrim distancen ndermjet rreshtave
						float distanceOfCenterRows = Vector2.Distance(CentersOfRows[i], CentersOfRows[j]);
						// Distancen ne X
						float xDis = Vector2.Distance( new Vector2(CentersOfRows[i].x, 0f), new Vector2(CentersOfRows[j].x, 0f));
						// Distancen ne Y
						float yDis = Vector2.Distance( new Vector2(0f, CentersOfRows[i].y), new Vector2(0f, CentersOfRows[j].y));
						// Rreshti me disatnce me te vogle dhe me distance brenda kufijve mesatare * 2
						if ( distanceOfCenterRows < distance 
						    && xDis <= PotentialPlateRows[i].GetAverangeXDistance() * 2f
						    && yDis <= PotentialPlateRows[i].GetAverangeYDistance() * 2f)
						{
							// Duhet ta shtojm ne fund kete rresht apo ne fillimi ?
							if (CentersOfRows[j].y < CentersOfRows[i].y)
							{
								// Shtojm ne fund rreshtin e llogaritur
								PotentialPlateRows[i].InsertRows(PotentialPlateRows[i].GetSize(), PotentialPlateRows[j].GetMaxList(), 
								                                 PotentialPlateRows[i].GetSize(), PotentialPlateRows[j].GetMinList(),
								                                 PotentialPlateRows[i].GetSize(), PotentialPlateRows[j].GetRowContours());
							}
							else
							{
								// Shtojm ne fillim rreshtin e llogaritur
								PotentialPlateRows[i].InsertRows(0 , PotentialPlateRows[j].GetMaxList(), 
								                                 0 , PotentialPlateRows[j].GetMinList(),
								                                 0 , PotentialPlateRows[j].GetRowContours());
							}
							// Shpetojm distancen
							distance = distanceOfCenterRows;
							// Fshijm rreshtin qe sapo kemi bashkuar dhe qendren e tij
							PotentialPlateRows.RemoveAt(j);
							CentersOfRows.RemoveAt(j);
							// Zbresim indeksin qe te mos kalojm "CentersOfRows.Count" dhe te mos humbasim vlera
							j--;
						}
					}
				}
			}
		}
		// Per cdo rresht te mbetur
		for (int i = 0; i < PotentialPlateRows.Count; i++)
		{
			// Kodet qe permban ai rresht jane te mjaftueshme per tu konsideruar Grup germash ?
			if (PotentialPlateRows[i].GetSize() < MainRecognition.GetMinCodeLenghtOfPlateRow() )
			{
				// Fshirja te rreshtit jasht standartit
				PotentialPlateRows.RemoveAt(i);
				i--;
			}
		}

		if (PotentialPlateRows.Count >= MainRecognition.GetNumberOfPlatesToTrack() && PotentialPlateRows.Count != 1)
		{
			// Rreshtojm rreshtat sipas atyj qe ka grup kodesh me te madh
			// Bubble Sort pasi nk jane shume elementet
			for (int i = 0; i < PotentialPlateRows.Count - 1; i++)
			{
				for (int j = i + 1; j < PotentialPlateRows.Count; j++)
				{
					if (PotentialPlateRows[i].GetSize() < PotentialPlateRows[j].GetSize() )
					{							
						Rows tempRow = PotentialPlateRows[i];						
						PotentialPlateRows[i] = PotentialPlateRows[j];						
						PotentialPlateRows[j] = tempRow;
					}
				}
			}
			// Sa targa duhen te trackohen njekohesisht ?
			// fshijm rreshtat qe kane pak elemente te bazuar sipas trackimit paralel
			for (int i = MainRecognition.GetNumberOfPlatesToTrack(); i < PotentialPlateRows.Count ; i++)
			{
				PotentialPlateRows.RemoveAt(i);
				i--;
			}
		}
	}
	// Gjetjen te kendit te prespektives
	private void FindSlantAngle()
	{
		// Per cdo rresht (pasi mund te trackojm me shume se 2) marrim qendren te 2 elementeve te fundit
		for (int i = 0; i < PotentialPlateRows.Count; i++)
		{
			Vector2 MinPointFirst = PotentialPlateRows[i].GetMinList()[PotentialPlateRows[i].GetSize() - 2];
			Vector2 MinPointLast = PotentialPlateRows[i].GetMinList()[PotentialPlateRows[i].GetSize() - 1];	
			
			Vector2 MaxPointFirst = PotentialPlateRows[i].GetMaxList()[PotentialPlateRows[i].GetSize() - 2];
			Vector2 MaxPointLast = PotentialPlateRows[i].GetMaxList()[PotentialPlateRows[i].GetSize() - 1];
			// Qendra te 2 elementeve te fundit
			Vector2 centerPointCurrent = (MinPointFirst - MaxPointFirst) / 2f + MaxPointFirst;
			Vector2 centerPointTest = (MinPointLast - MaxPointLast) / 2f + MaxPointLast;
			// Gjem kendin qe formon ky vektor me aksin X
			float SlantAngle = 180f - Vector2.Angle(centerPointTest - centerPointCurrent, 
			                                        new Vector2(centerPointCurrent.x, centerPointTest.y ) - centerPointTest);
			// E marrim me vlere negative dhe jo me vlere > 180*
			if (centerPointTest.y > centerPointCurrent.y) SlantAngle = -SlantAngle;
			// Shpetoj per cdo rresht kendin e prespektives dhe qendren e rrotullimit
			PotentialPlateRows[i].SetSlantAngleAndCenter(SlantAngle, MaxPointLast);
		}
	}

	#endregion
}