using System.Drawing;
using NUnit.Framework;
using static Enterprise.DeniedPartyScreening.GUI.Properties.Resources;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class DpsImagesWinformConverterTest : TestCase
	{
		public void TestToBitmap()
		{
			CombineAssertions(() =>
			{
				AssertBitmapSame(entityDrawingGroup, DpsImageSources.Organization.ToBitmap());
				AssertBitmapSame(vesselDrawingGroup, DpsImageSources.Vessel.ToBitmap());
				AssertBitmapSame(countryDrawingGroup, DpsImageSources.Country.ToBitmap());
				AssertBitmapSame(personDrawingGroup, DpsImageSources.Person.ToBitmap());
				AssertBitmapSame(warningDrawingGroup, DpsImageSources.Warning.ToBitmap());
				AssertBitmapSame(warning_orangeDrawingGroup, DpsImageSources.WarningOrange.ToBitmap());
				AssertBitmapSame(warning_redDrawingGroup, DpsImageSources.WarningRed.ToBitmap());
				AssertBitmapSame(warning_whiteDrawingGroup, DpsImageSources.WarningWhite.ToBitmap());
				AssertBitmapSame(null, DpsImageSources.None.ToBitmap());
			});

			void AssertBitmapSame(Bitmap bitmapA, Bitmap bitmapB)
			{
				if (bitmapA == null && bitmapB == null)
				{
					Assert(true);
					return;
				}

				if (bitmapA.Width != bitmapB.Width || bitmapA.Height != bitmapB.Height)
				{
					Assert("Bitmaps not the same", false);
					return;
				}

				for (var x = 0; x < bitmapA.Width; x++)
				{
					for (var y = 0; y < bitmapA.Height; y++)
					{
						var pixel1 = bitmapA.GetPixel(x, y);
						var pixel2 = bitmapB.GetPixel(x, y);

						if (pixel1 != pixel2)
						{
							Assert("Bitmaps not the same", false);
							return;
						}
					}
				}

				Assert("Bitmaps are the same", true);
			}
		}
	}
}
