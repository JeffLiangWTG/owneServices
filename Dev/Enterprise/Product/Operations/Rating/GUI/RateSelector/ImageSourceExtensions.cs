using System.Drawing;
using System.IO;
using CargoWise.Types;

namespace Enterprise.Rating.GUI.RateSelector
{
	public static class ImageSourceExtensions
	{
		public static Bitmap ToBitmapImage(this ZBlob blob)
		{
			if (blob == null || blob.IsEmpty)
			{
				return null;
			}

			using (var ms = new MemoryStream(blob))
			{
				return new Bitmap(ms);
			}
		}
	}
}
