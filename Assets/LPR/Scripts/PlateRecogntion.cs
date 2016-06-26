using UnityEngine;
[System.Serializable]
public class PlateRecogntion : MonoBehaviour 
{
	#region PUBLIC_EDITOR_FIELDS

	public bool ExecuteOnStart = false;
	public bool EachFrame = false;
	public bool RecognizePlateCode = false;
	public bool ApplyPlateCodeSlantCorrection = false;

	public void SetApplyPlateCodeSlantCorrection(bool ApplyPlateCodeSlantCorrection)
	{
		this.ApplyPlateCodeSlantCorrection = ApplyPlateCodeSlantCorrection;
	}

	public bool ExtractPlateString = false;

	public void SetExtractPlateString(bool ExtractPlateString)
	{
		this.ExtractPlateString = ExtractPlateString;
	}

	public float ContrastValue = 45f;

	public void SetContrastValue(float ContrastValue)
	{
		this.ContrastValue = ContrastValue;
	}

	public float GammaCorrectionValue = 2f;

	public void SetGammaCorrectionValue(float GammaCorrectionValue)
	{
		this.GammaCorrectionValue = GammaCorrectionValue;
	}

	public float BynarizationThreshold = .8f;	

	public void SetBynarizationThreshold(float BynarizationThreshold)
	{
		this.BynarizationThreshold = BynarizationThreshold;
	}

	public int MedianGroupPixel = 3;

	public void SetMedianGroupPixel(float MedianGroupPixel)
	{
		this.MedianGroupPixel = (int)MedianGroupPixel;
	}

	public int CameraIndex = 0;
	public int MinLenghtOfPlateRow = 4;
	public int NumberOfPlatesToTrack = 1;
	public float SlantThreshold = 2;

	[SerializeField]
	public Transform ReadImage;
	[SerializeField]
	public Transform WriteImage;

	public Vector2 DimensionOfPlateCharX_Min_Max = new Vector2(9.5f, 100f);
	public Vector2 DimensionOfPlateCharY_Min_Max = new Vector2(10f, 100f);

	[SerializeField]
	private MainRecognition MainRecognitionBehaviour;

	public CREATE_OPTIONS ImageOptions;
	public enum CREATE_OPTIONS 
	{ 
		SPRITE = 0, 
		TEXTURE = 1, 
	}

	#endregion

	#region PRIVATE_METHODS

	private void Start()
	{
		Application.runInBackground = true;

		if (ExecuteOnStart)
		{
			StartProcessing();
		}
	}

	#endregion

	#region PUBLIC_METHODS

	public void StartProcessing()
	{
		if (ImageOptions == CREATE_OPTIONS.SPRITE)
		{
			try
			{
				MainRecognitionBehaviour.InitializeComponents( ReadImage.GetComponent<SpriteRenderer>(), WriteImage.GetComponent<SpriteRenderer>());
			}
			catch
			{
				Debug.LogError("Read & Write Transform must contain a SpriteRenderer Component with a Sprite Image");
			}
		}
		else if (ImageOptions == CREATE_OPTIONS.TEXTURE)
		{
			try
			{
				MainRecognitionBehaviour.InitializeComponents( ReadImage.GetComponent<MeshRenderer>(), WriteImage.GetComponent<MeshRenderer>(), CameraIndex);
			}
			catch
			{
				Debug.LogError("Read & Write Transform must contain a MeshRenderer with a Material Component (And a Texture if not using the Camera Stream)");
			}
		}

		MainRecognitionBehaviour.StartProcessing();
	}

	#region EDITOR_METHOD

	public void UpdateValues()
	{
		if ( MainRecognitionBehaviour == null )
		{
			MainRecognitionBehaviour = gameObject.AddComponent<MainRecognition>();
		}

		MainRecognitionBehaviour.UpdateValues(EachFrame, RecognizePlateCode, ApplyPlateCodeSlantCorrection, ExtractPlateString, ContrastValue, GammaCorrectionValue
		                                    	, BynarizationThreshold, MedianGroupPixel, DimensionOfPlateCharX_Min_Max, DimensionOfPlateCharY_Min_Max
			                                    , MinLenghtOfPlateRow, NumberOfPlatesToTrack, SlantThreshold);
	}

	#endregion

	#endregion
}