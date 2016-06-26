using UnityEngine;
using System.Threading;
using System.Collections;
[System.Serializable]
public class MainRecognition : MonoBehaviour {

	#region PUBLIC_EDITOR_FIELDS
	[HideInInspector] [SerializeField]
	public bool EachFrame = false;
	[HideInInspector] [SerializeField]
	public bool RecognizePlateCode = false;
	[HideInInspector] [SerializeField]
	public bool ApplyPlateCodeSlantCorrection = false;
	[HideInInspector] [SerializeField]
	public bool ExtractPlateString = false;
	[HideInInspector] [SerializeField]
	public SpriteRenderer ReadImage;
	[HideInInspector] [SerializeField]
	public SpriteRenderer WriteImage;
	[HideInInspector] [SerializeField]
	public MeshRenderer ReadImageMat;
	[HideInInspector] [SerializeField]
	public MeshRenderer WriteImageMat;
	[HideInInspector] [SerializeField]
	public float ContrastValue;
	[HideInInspector] [SerializeField]
	public float GammaCorrectionValue;
	[HideInInspector] [SerializeField]
	public float BynarizationThreshold;	
	[HideInInspector] [SerializeField]
	public int MedianGroupPixel;
	[HideInInspector] [SerializeField]
	public Vector2 DimensionOfPlateCharX_Min_Max;
	[HideInInspector] [SerializeField]
	public Vector2 DimensionOfPlateCharY_Min_Max;
	[HideInInspector] [SerializeField]
	public int MinLenghtOfPlateRow;
	[HideInInspector] [SerializeField]
	public int NumberOfPlatesToTrack;
	[HideInInspector] [SerializeField]
	public float SlantThreshold;
	
	#endregion

	#region PRIVATE_FIELDS
	
	private static int MinCodeLenghtOfPlateStatic;
	private static int NumberOfPlatesToTrackStatic;

	[HideInInspector] [SerializeField]
	private ImageManipulation ImageRecognitionOptimized;
	[HideInInspector] [SerializeField]
	private CalculateEdges EdgeCalculation;
	[HideInInspector] [SerializeField]
	private PlateLocalization LocalizePlate;
	[HideInInspector] [SerializeField]
	private OpenFileDatabase OpenDatabase;
	[HideInInspector]
	private Color[] SpritePixels;
	
	private readonly object LockThread = new object();
	[HideInInspector]
	private bool finished = false;
	[HideInInspector]
	private WebCamDevice[] 	WebCams;
	public static WebCamTexture StreamingCamera;
	
	#endregion

	#region PUBLIC_METHODS
	
	public void InitializeComponents(MeshRenderer ReadImageMat, MeshRenderer WriteImageMat, int CameraIndex)
	{
		this.ReadImageMat = ReadImageMat;
		this.WriteImageMat = WriteImageMat;
		
		WebCams = WebCamTexture.devices;
		
		if ((WebCams != null) && (WebCams.Length > 0) && CameraIndex != 0) 
		{
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR
			CameraIndex = 1;
#endif

			StreamingCamera = new WebCamTexture(WebCams[CameraIndex - 1].name, 512, 512, 15);
			StreamingCamera.Play();
			this.ReadImageMat.material.mainTexture = StreamingCamera; 
		}
		
		int ImageWidth = ReadImageMat.material.mainTexture.width;
		int ImageHeight = ReadImageMat.material.mainTexture.height;
		
		ImageRecognitionOptimized = new ImageManipulation(ImageWidth, ImageHeight);
		EdgeCalculation = new CalculateEdges();
		OpenDatabase = gameObject.AddComponent<OpenFileDatabase>();
		LocalizePlate = new PlateLocalization(ImageWidth, ImageHeight, OpenDatabase);
	}

	public void InitializeComponents(SpriteRenderer ReadImage, SpriteRenderer WriteImage)
	{		
		this.ReadImage = ReadImage;
		this.WriteImage = WriteImage;

		int ImageWidth = ReadImage.sprite.texture.width;
		int ImageHeight = ReadImage.sprite.texture.height;

		ImageRecognitionOptimized = new ImageManipulation(ImageWidth, ImageHeight);
		EdgeCalculation = new CalculateEdges();
		OpenDatabase = gameObject.AddComponent<OpenFileDatabase>();
		LocalizePlate = new PlateLocalization(ImageWidth, ImageHeight, OpenDatabase);
	}

	public void UpdateValues(bool EachFrame, bool RecognizePlateCode, bool ApplyPlateCodeSlantCorrection, bool ExtractPlateString,
	                         float ContrastValue, float GammaCorrectionValue, float BynarizationThreshold, int MedianGroupPixel, 
	                         Vector2 DimensionOfPlateCharX_Min_Max, Vector2 DimensionOfPlateCharY_Min_Max, int MinLenghtOfPlateRow,
	                         int NumberOfPlatesToTrack, float SlantThreshold)
	{
		this.EachFrame = EachFrame;
		this.RecognizePlateCode = RecognizePlateCode;
		this.ApplyPlateCodeSlantCorrection = ApplyPlateCodeSlantCorrection;
		this.ExtractPlateString = ExtractPlateString;
		this.ContrastValue = ContrastValue;
		this.GammaCorrectionValue = GammaCorrectionValue;
		this.BynarizationThreshold = BynarizationThreshold;
		this.MedianGroupPixel = MedianGroupPixel;
		this.DimensionOfPlateCharX_Min_Max = DimensionOfPlateCharX_Min_Max;
		this.DimensionOfPlateCharY_Min_Max = DimensionOfPlateCharY_Min_Max;
		this.MinLenghtOfPlateRow = MinLenghtOfPlateRow;
		this.NumberOfPlatesToTrack = NumberOfPlatesToTrack;
		this.SlantThreshold = SlantThreshold;
	}

	public void StartProcessing()
	{
		StartCoroutine( LicensePlateRecognize() );
	}

	public void StopProcessing()
	{
		StopAllCoroutines();
	}
	
	#endregion

	
	#region PRIVATE_METHODS

	private IEnumerator LicensePlateRecognize()
	{
		MinCodeLenghtOfPlateStatic = MinLenghtOfPlateRow;
		NumberOfPlatesToTrackStatic = NumberOfPlatesToTrack;

		if (ReadImage != null)
		{
			SpritePixels = ReadImage.sprite.texture.GetPixels();
		}
		else if (ReadImageMat != null)
		{
			if (StreamingCamera != null)
			{
				while(!StreamingCamera.isPlaying) yield return null;

				SpritePixels = StreamingCamera.GetPixels();
			}
			else
			{
				Texture2D ProcessedTexture = (Texture2D)ReadImageMat.material.mainTexture;
				SpritePixels = ProcessedTexture.GetPixels();
			}
		}

		
		Thread thread = new Thread(ThreadFunctions);
		thread.Start();

		while(!finished)
		{
			yield return new WaitForEndOfFrame();
		}
		
		yield return new WaitForEndOfFrame();
		finished = false;
		
		#region DRAWING_FUNCTIONS

		if (WriteImage != null)
		{
			WriteImage.sprite.texture.SetPixels(SpritePixels);
			WriteImage.sprite.texture.Apply();
		}
		else
		{
			Texture2D ProcessedTexture = new Texture2D(StreamingCamera.width, StreamingCamera.height);
			ProcessedTexture.SetPixels(SpritePixels);
			ProcessedTexture.Apply();
			
			yield return new WaitForEndOfFrame();
			
			WriteImageMat.material.mainTexture = ProcessedTexture;
		}

		while (!EachFrame)
		{
			yield return new WaitForEndOfFrame();
		}
		
		StartCoroutine( LicensePlateRecognize() );
		
		#endregion
	}

	private void ThreadFunctions()
	{
		lock(LockThread)
		{
			ImageRecognitionOptimized.ConvertPixelArrayToBynarMatrix(SpritePixels, 
			                                                         ContrastValue, 
			                                                         GammaCorrectionValue, 
			                                                         MedianGroupPixel, 
			                                                         BynarizationThreshold );

			EdgeCalculation.CalcualteBorders(ImageRecognitionOptimized.GetMatrix(),
			                                 ImageRecognitionOptimized.GetWidth(),
			                                 ImageRecognitionOptimized.GetHeight() );

			if (RecognizePlateCode)
			{
				LocalizePlate.InitializePlateLocalizationParameters(EdgeCalculation.GetContour(), 
				                                                    ImageRecognitionOptimized.GetMatrix(), 
				                                                    DimensionOfPlateCharX_Min_Max, 
				                                                    DimensionOfPlateCharY_Min_Max);
				LocalizePlate.LocalizePlateString();
				
				if (ApplyPlateCodeSlantCorrection)
				{
					LocalizePlate.SlantCorrection( SlantThreshold );
				}

				#region DRAWING_FUNCTIONS
				
				LocalizePlate.DrawContour();
				LocalizePlate.DrawArrangedPossiblePlate();
				
				#endregion
				
				if (ExtractPlateString)
				{
					LocalizePlate.CalculateStringAccuracy();
				}
			}
			
			SpritePixels = ImageRecognitionOptimized.ConvertBynariMatrixToPixelArray();
			
			finished = true;
		}
	}
	
	#endregion

	#region PUBLIC_STATIC_METHODS
	
	public static int GetMinCodeLenghtOfPlateRow()
	{
		return MinCodeLenghtOfPlateStatic;
	}
	
	public static int GetNumberOfPlatesToTrack()
	{
		return NumberOfPlatesToTrackStatic;
	}
	
	#endregion
}
