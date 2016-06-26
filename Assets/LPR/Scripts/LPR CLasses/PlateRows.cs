using UnityEngine;
using System.Collections.Generic;

// Klasa qe mban informacione rreth rreshtat te nje grup kodesh
public class Rows
{
	#region PRIVATE_FIELDS
	// konturat te seciles kod
	private List<Contour> RowContours = new List<Contour>();
	// Pikat X,Y minimale dhe maksimale te katrorit qe e perfshin kodin
	private List<Vector2> MinPoints = new List<Vector2>();
	private List<Vector2> MaxPoints = new List<Vector2>();
	// Qendra ku do rrotullohet rreshti per permiresimin te prespektives
	private Vector2 CenterOfRotation;
	// Qendra e grupit te kodit per shtimin te kodeve paralele
	private Vector2 BlobCenter;
	// Kendi i prespektives
	private float SlantAngle = 0f;
	// Gjeresia mesatare te kodeve ne aksin Y
	private float AverangeYDistance;
	// Gjeresia mesatare te kodeve ne aksin X
	private float AverangeXDistance;

	#endregion

	#region PUBLIC_METHODS
	// Kthen qendren e grupit te kodit
	public Vector2 GetCenterOfRows()
	{
		return BlobCenter;
	}
	// Vendos kendin e prespektives dhe pika e rrotullimit
	public void SetSlantAngleAndCenter(float SlantAngle, Vector2 CenterOfRotation)
	{
		this.SlantAngle = SlantAngle;
		this.CenterOfRotation = CenterOfRotation;
	}
	// Kthen kendin e prespektives
	public float GetSlantAngle()
	{
		return SlantAngle;
	}
	// Kthen qendren e rrotullimit
	public Vector2 GetCenterOfRotation()
	{
		return CenterOfRotation;
	}
	
	public float GetAverangeXDistance()
	{
		return AverangeXDistance;
	}
	
	public float GetAverangeYDistance()
	{
		return AverangeYDistance;
	}
	// Konstruktor per kodin fillestar
	public Rows(Vector2 MaxPoints, Vector2 MinPoints, Contour newCont)
	{
		AddElementsToRows(MaxPoints, MinPoints, newCont);
	}
	// Shtimi i kodeve ne rresht
	public void AddElementsToRows(Vector2 MaxPoints, Vector2 MinPoints, Contour newCont)
	{
		this.MinPoints.Add(MinPoints);
		this.MaxPoints.Add(MaxPoints);
		this.RowContours.Add(newCont);		
	}
	// Shtimi i kodeve ne rresht sipas nje indeksi posicionimi
	public void InsertRows(int indexMax, List<Vector2> MaxPoints, int indexMin, List<Vector2> MinPoints, int indexContour, List<Contour> RowContours)
	{
		this.MinPoints.InsertRange(indexMax, MinPoints);
		this.MaxPoints.InsertRange(indexMin, MaxPoints);
		this.RowContours.InsertRange(indexContour, RowContours);
	}
	// Mbivendosje te kodeve
	public void SetRows(List<Vector2> MaxPoints, List<Vector2> MinPoints, List<Contour> RowContours)
	{
		this.MinPoints = MinPoints;
		this.MaxPoints = MaxPoints;
		this.RowContours = RowContours;
	}
	// Kthen grupin e pikave maksimale per konturet
	public List<Vector2> GetMaxList()
	{
		return MaxPoints;
	}
	// Kthen grupin e pikave minimale per konturet
	public List<Vector2> GetMinList()
	{
		return MinPoints;
	}
	// Kthen  konturet te kodeve
	public List<Contour> GetRowContours()
	{
		return RowContours;
	}
	// Kthen nr e kodeve qe jane pjese ne kete rresht
	public int GetSize()
	{
		return MaxPoints.Count;
	}
	// Nis perpunimin te te dhenave
	public void CalculateRowsPropeties()
	{
		CalculateTheRowCenter();
		CalculateAverangeXYDistances();
	}

	#endregion

	#region PRIVATE_METHODS
	// Percakton gjeresite mesatare te kodeve te rreshtit
	private void CalculateAverangeXYDistances()
	{
		float xDis = 0f;
		float yDis = 0f;
		
		for (int i = 0; i < GetSize(); i++)
		{
			xDis += Vector2.Distance( new Vector2(MinPoints[i].x, 0f), new Vector2(MaxPoints[i].x, 0f));
			yDis += Vector2.Distance( new Vector2(0f, MinPoints[i].y), new Vector2(0f, MaxPoints[i].y));
		}
		
		AverangeXDistance = xDis / (float)GetSize();
		AverangeYDistance = yDis / (float)GetSize();
	}
	// Percakton qendren mesatare te rreshtin ne Figure 
	private void CalculateTheRowCenter()
	{
		for (int i = 1; i < GetSize(); i++)
		{
			Vector2 firstCenter = (MinPoints[0] - MaxPoints[0]) / 2f + MaxPoints[0];
			Vector2 currentCenter = (MinPoints[i] - MaxPoints[i]) / 2f + MaxPoints[i];
			
			BlobCenter = (currentCenter - firstCenter) /2f + firstCenter;
		}
	}

	#endregion
}
