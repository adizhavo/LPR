using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SaveLetter : MonoBehaviour {

	public PlateRecognitionLearning SaveChars;

	public void SaveLetters()
	{
		SaveChars.SaveLetter(GetComponent<Text>().text);
	}

	public void SkipLetters()
	{
		SaveChars.SaveLetter("*");
	}
}
