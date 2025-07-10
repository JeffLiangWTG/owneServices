using System;
using System.Drawing;
using System.IO;
using Tesseract;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public static class CaptchaSolver
	{
		public static string SolveCaptcha(Stream imageToSolve)
		{
			_ = imageToSolve ?? throw new ArgumentNullException(nameof(imageToSolve));

			using var stream = EnhanceImage(imageToSolve);
			using var tesseract = CreateTesseractEngine();
			using var img = Pix.LoadFromMemory(((MemoryStream)stream).ToArray());
			using var page = tesseract.Process(img, PageSegMode.SparseTextOsd);
			var captcha = page.GetText().Trim().Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
			return captcha;
		}

		static void RemoveNonBlueColor(Bitmap source)
		{
			for (int y = 0; y < source.Height; y++)
			{
				for (int x = 0; x < source.Width; x++)
				{
					var color = source.GetPixel(x, y);
					if (color.A == 255 && color.B == 255 && color.G == 0 && color.R == 0)
					{
						continue;
					}
					source.SetPixel(x, y, Color.White);
				}
			}
		}

		static Stream EnhanceImage(Stream stream)
		{
			_ = stream ?? throw new ArgumentNullException(nameof(stream));

			stream.Position = 0;
			using var image = (Bitmap)Image.FromStream(stream);
			RemoveNonBlueColor(image);
			using var newStream = new MemoryStream();
			image.Save(newStream, System.Drawing.Imaging.ImageFormat.Png);
			newStream.Position = 0;
			return newStream;
		}

		static TesseractEngine CreateTesseractEngine()
		{
			var assemblyPath = Path.GetDirectoryName(typeof(CaptchaSolver).Assembly.Location);
			var engine = new TesseractEngine(Path.Combine(assemblyPath, "tessdata"), "eng", EngineMode.LstmOnly, Path.Combine(assemblyPath, "ImageProcessingConfig.txt"));
			return engine;
		}
	}
}
