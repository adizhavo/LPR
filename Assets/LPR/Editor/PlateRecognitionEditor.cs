using UnityEngine;
using UnityEditor;
[System.Serializable]
[CustomEditor(typeof(PlateRecogntion))]
public class PlateRecogntionEditor : Editor
{
	[SerializeField]
	PlateRecogntion PlateRecognitionUnity;
	[SerializeField]
	private bool FoldoutImageProcessing = false;

	private WebCamDevice[] Cameras = WebCamTexture.devices;
	private string[] CamerasNames;
	private Transform ReadImageTransformObject ;
	private Transform WriteImageTransformObject ;

	public override void OnInspectorGUI()
	{ 
		PlateRecognitionUnity = (PlateRecogntion)target;

		DrawTitle();
		DrawImagesOptions();
		DrawImageProcessing();
		RecognizePlateCode();

		PlateRecognitionUnity.UpdateValues();
	}

	void DrawTitle()
	{
		GUILayout.Space(10f);
		GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
		GUILayout.Label("Unity License Plate Recognition Version 1.0");
		GUI.skin.label.fontStyle = FontStyle.Normal;
		GUILayout.Space(10f);
	}

	void DrawImagesOptions()
	{
		PlateRecognitionUnity.EachFrame = EditorGUILayout.Toggle( "Process Every Frame", PlateRecognitionUnity.EachFrame);

		if (!Application.isPlaying)
		{
			PlateRecognitionUnity.ExecuteOnStart = EditorGUILayout.Toggle( "Execute On Start", PlateRecognitionUnity.ExecuteOnStart);
			
			PlateRecognitionUnity.ImageOptions = (PlateRecogntion.CREATE_OPTIONS) EditorGUILayout.EnumPopup("Read Mode", PlateRecognitionUnity.ImageOptions);
			
			PlateRecognitionUnity.ReadImage = (Transform) EditorGUILayout.ObjectField("Read Image", PlateRecognitionUnity.ReadImage, typeof (Transform), true);	
			PlateRecognitionUnity.WriteImage = (Transform) EditorGUILayout.ObjectField("Write Image", PlateRecognitionUnity.WriteImage, typeof (Transform), true);
			
			if (PlateRecognitionUnity.ImageOptions == PlateRecogntion.CREATE_OPTIONS.TEXTURE)
			{
				EditorGUILayout.HelpBox("On \"Texture Read\" mode you can process Live Camera Feed Images from your Web Cam", MessageType.Info);

				CamerasNames = new string[Cameras.Length + 1];
				CamerasNames[0] = "None";

				for (int i = 0; i < Cameras.Length; i++)
				{
					CamerasNames[i+1] = Cameras[i].name;
				}

				PlateRecognitionUnity.CameraIndex = EditorGUILayout.Popup( "Select Camera for live Streaming" , PlateRecognitionUnity.CameraIndex, CamerasNames);
			}
		}
	}

	void DrawImageProcessing()
	{
		EditorGUILayout.Space();
		FoldoutImageProcessing = EditorGUILayout.Foldout(FoldoutImageProcessing, "Image Processing");

		if (FoldoutImageProcessing)
		{
			PlateRecognitionUnity.ContrastValue = EditorGUILayout.Slider("Image Contrast", PlateRecognitionUnity.ContrastValue, 0f, 100f );
			PlateRecognitionUnity.GammaCorrectionValue = EditorGUILayout.Slider("Gamma Correction", PlateRecognitionUnity.GammaCorrectionValue, 0f, 20f );
			PlateRecognitionUnity.MedianGroupPixel = EditorGUILayout.IntSlider("Median Filter", PlateRecognitionUnity.MedianGroupPixel, 1, 21 );
			PlateRecognitionUnity.BynarizationThreshold = EditorGUILayout.Slider("Bynarization Threshold", PlateRecognitionUnity.BynarizationThreshold, 0f, 1f );
			GUILayout.Space(10f);
			EditorGUILayout.HelpBox("1- Image Contrast: Increase the image contrast\n\n" +
									"2- Gamma Correction: Adjust the gamma value of the image\n\n" +
									"3- Median Filter: Group of pixels to filter\n\n" +
			                        "4- Bynarization Threshold: Clamping value for a bynary black and white image", MessageType.None );
		}
	}

	void RecognizePlateCode()
	{
		GUILayout.Space(10f);
		PlateRecognitionUnity.RecognizePlateCode = EditorGUILayout.Toggle( "Recognize Plate Code", PlateRecognitionUnity.RecognizePlateCode);

		if (PlateRecognitionUnity.RecognizePlateCode)
		{
			GUILayout.Space(5f);
			PlateRecognitionUnity.DimensionOfPlateCharX_Min_Max.x = EditorGUILayout.FloatField("Min Char Width:", PlateRecognitionUnity.DimensionOfPlateCharX_Min_Max.x);
			PlateRecognitionUnity.DimensionOfPlateCharX_Min_Max.y = EditorGUILayout.FloatField("Max Char Width:", PlateRecognitionUnity.DimensionOfPlateCharX_Min_Max.y);
			EditorGUILayout.MinMaxSlider(ref PlateRecognitionUnity.DimensionOfPlateCharX_Min_Max.x, ref PlateRecognitionUnity.DimensionOfPlateCharX_Min_Max.y, 0f, 500f);
			GUILayout.Space(5f);
			PlateRecognitionUnity.DimensionOfPlateCharY_Min_Max.x = EditorGUILayout.FloatField("Min Char Height:", PlateRecognitionUnity.DimensionOfPlateCharY_Min_Max.x);
			PlateRecognitionUnity.DimensionOfPlateCharY_Min_Max.y = EditorGUILayout.FloatField("Max Char Height:", PlateRecognitionUnity.DimensionOfPlateCharY_Min_Max.y);
			EditorGUILayout.MinMaxSlider(ref PlateRecognitionUnity.DimensionOfPlateCharY_Min_Max.x, ref PlateRecognitionUnity.DimensionOfPlateCharY_Min_Max.y, 0f, 500f);
			GUILayout.Space(5f);
			PlateRecognitionUnity.MinLenghtOfPlateRow = EditorGUILayout.IntSlider("Min Chars in a Plate", PlateRecognitionUnity.MinLenghtOfPlateRow, 1, 10 );
			PlateRecognitionUnity.NumberOfPlatesToTrack = EditorGUILayout.IntSlider("Simultaneous Plate to Track", PlateRecognitionUnity.NumberOfPlatesToTrack, 1, 4 );

			GUILayout.Space(10f);
			PlateRecognitionUnity.ApplyPlateCodeSlantCorrection = EditorGUILayout.Toggle( "Slant Anlge Correction", PlateRecognitionUnity.ApplyPlateCodeSlantCorrection);
			                			
			if (PlateRecognitionUnity.ApplyPlateCodeSlantCorrection)
			{
				PlateRecognitionUnity.SlantThreshold = EditorGUILayout.Slider("Slant Angle Threshold", PlateRecognitionUnity.SlantThreshold, 0f, 15f );
			}

			GUILayout.Space(10f);
			PlateRecognitionUnity.ExtractPlateString = EditorGUILayout.Toggle( "Extract Plate String", PlateRecognitionUnity.ExtractPlateString);
		}
	}
}

[CustomEditor(typeof(MainRecognition))]
public class PlateMain : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Main Recognition Algorithm", MessageType.None);
	}
}