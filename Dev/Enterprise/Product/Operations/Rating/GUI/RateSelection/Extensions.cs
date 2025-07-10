using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;

namespace Enterprise.Rating.GUI.RateChooser
{
	public static class Extensions
	{
		public static Image ToImage(this ZBlob blob)
		{
			if (blob.IsEmpty)
			{
				return null;
			}

			using (var memoryStream = new MemoryStream(blob))
			{
				return memoryStream.ToImage();
			}
		}

		public static Image ToImage(this Icon icon)
		{
			var bmp = icon.ToBitmap();
			bmp.MakeTransparent();
			return bmp;
		}

		/// <summary>
		///		Takes a bitmap image stream and converts it to an image that can be handled by WPF
		/// </summary>
		/// <param name="stream">
		///		A bitmap image stream
		/// </param>
		/// <param name="cacheOption">
		///		Cache option to use for this instance of BitmapImage
		/// </param>
		/// <returns>
		///		The image as a BitmapImage for WPF
		/// </returns>
		/// <see cref="https://stackoverflow.com/questions/26260654/wpf-converting-bitmap-to-imagesource"/>
		public static Image ToImage(this Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			var image = new Bitmap(stream);
			return image;
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "Winzor Compatibility")]
		public static void SetTooltip(this Control control, string tooltip)
		{
			if (!string.IsNullOrEmpty(tooltip))
			{
				ToolTipService.SetToolTip(control, tooltip);
			}
		}
	}
}
