using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GetRecognizedWords : MonoBehaviour 
{
	void Start () 
	{
		PlateLocalization.OnLettersTracked += PostLicensePlate;
	}

	void OnDestroy()
	{
		PlateLocalization.OnLettersTracked -= PostLicensePlate;
	}

	void PostLicensePlate(string LicensePlateText)
	{
		GetComponent<Text>().text = LicensePlateText;
	}
}
