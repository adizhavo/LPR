using UnityEngine;
using System.Collections.Generic;

// Mban te dhena rreth kontureve te kapur
public class Contour
{
	#region PRIVATE_FIELDS
	// Pozicionet XY ne matricen e figures
	private List<int> xOfPoint = new List<int>();
	private List<int> yOfPoint = new List<int>();

	#endregion

	#region PUBLIC_METHODS
	// Konstruktor bosh
	public Contour(){}
	// Konstruktor me te dhenat fillestare
	public Contour(int xOfPoint, int yOfPoint)
	{
		AddPoint(xOfPoint, yOfPoint);
	}
	// Shtimi i pikave ne listat per trackimin te posicioneve XY
	public void AddPoint(int xOfPoint, int yOfPoint)
	{
		this.xOfPoint.Add ( xOfPoint );
		this.yOfPoint.Add( yOfPoint );
	}
	// Kthen nr e elementeve te shpetuar
	public int GetLenghtOfPath()
	{
		return xOfPoint.Count;
	}
	// Kthen posicionet X
	public List<int> GetXValue()
	{
		return xOfPoint;
	}
	// Kthen posicionet Y
	public List<int> GetYValue()
	{
		return yOfPoint;
	}
	// Vlera x, y eshte eksition ne listat ?
	public bool IsValueContained(int x, int y)
	{
		return ( xOfPoint.Contains(x) && yOfPoint.Contains(y) );
	}
	// Reseton klasen
	public void Clear()
	{
		xOfPoint.Clear();
		yOfPoint.Clear();
	}

	#endregion
}
// Mbajtes i disa te dhenave
public class ProcessedContour
{
	#region PUBLIC_FIELDS

	public List<Vector2> MaxPoints = new List<Vector2>();
	public List<Vector2> MinPoints = new List<Vector2>();
	public List<Contour> SavedCont = new List<Contour>();

	#endregion
}
