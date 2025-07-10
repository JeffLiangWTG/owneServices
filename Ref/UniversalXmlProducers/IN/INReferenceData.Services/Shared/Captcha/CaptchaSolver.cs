using System;
using System.IO;
using OpenCvSharp;
using Tesseract;

namespace CargoWise.RefDbRepo.INReferenceData.Services;

public static class CaptchaSolver
{
	public static string SolveCaptcha(byte[] file)
	{
		using var stream = ImageEnhancer.Process(file);
		using var img = Pix.LoadFromMemory(stream.ToArray());
		using var engine = CreateTesseractEngine();
		using var page = engine.Process(img, PageSegMode.SingleLine);
		return page.GetText().Trim().Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
	}

	static TesseractEngine CreateTesseractEngine()
	{
		var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Shared\Captcha\Tessdata");
		var engine = new TesseractEngine(dataPath, "eng", EngineMode.LstmOnly);
		engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz123456789");
		return engine;
	}

	static class ImageEnhancer
	{
		public static MemoryStream Process(byte[] image)
		{
			using var src = Cv2.ImDecode(image, ImreadModes.Grayscale);
			using var binary = Binarisation(src);
			using var processed = EnsureLightBackground(binary);
			using var enlarged = Rescaling(processed);
			return enlarged.ToMemoryStream();
		}

		static Mat Binarisation(Mat src)
		{
			using var binary = new Mat();
			Cv2.Threshold(src, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
			return binary.Clone();
		}

		static Mat EnsureLightBackground(Mat src)
		{
			var whitePixels = src.CountNonZero();
			var totalPixels = src.Rows * src.Cols;
			var whiteRatio = (double)whitePixels / totalPixels;

			if (whiteRatio < 0.5)
			{
				using var inverted = new Mat();
				Cv2.Subtract(new Mat(src.Rows, src.Cols, src.Type(), Scalar.All(255)), src, inverted);
				return inverted.Clone();
			}
			return src.Clone();
		}

		static Mat Rescaling(Mat src)
		{
			using var enlarged = new Mat();
			Cv2.Resize(src, enlarged, new Size(0, 0), 3.0, 3.0, InterpolationFlags.Nearest);
			return enlarged.Clone();
		}
	}
}
