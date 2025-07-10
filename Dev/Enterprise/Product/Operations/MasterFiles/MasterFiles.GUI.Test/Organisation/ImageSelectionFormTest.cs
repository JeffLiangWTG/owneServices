using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ImageSelectionForm))]
	sealed class ImageSelectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ImageSelectionForm();
		}

		public void TestImageImageConvertedToTiff()
		{
			using (var file = TempFile.New())
			{
				Image img = new Bitmap(1, 1);
				img.Save(file.Filename, ImageFormat.Jpeg);
				Image source = Image.FromFile(file.Filename);
				AssertEquals(ImageFormat.Jpeg.Guid, source.RawFormat.Guid);

				Image destination = null;
				try
				{
					using (ImageSelectionFormForTest form = new ImageSelectionFormForTest())
					{
						byte[] convertedImageBytes = form.ConvertImageToByte_Exposed(source);
						using (MemoryStream ms = new MemoryStream(convertedImageBytes))
						{
							destination = Bitmap.FromStream(ms, true);
						}
					}
					AssertNotNull(destination);
					AssertEquals(ImageFormat.Tiff.Guid, destination.RawFormat.Guid);
				}
				finally
				{
					img.Dispose();
					source.Dispose();
					destination.Dispose();
				}
			}
		}

		class ImageSelectionFormForTest : ImageSelectionForm
		{
			public byte[] ConvertImageToByte_Exposed(Image img)
			{
				return base.ConvertImageToByte(img);
			}
		}
	}
}
