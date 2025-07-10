using System.Drawing;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class DpsImagesWinformConverter
	{
		public static Bitmap ToBitmap(this DpsImageSources dpsImageSource)
		{
			Bitmap result;

			switch (dpsImageSource)
			{
				case DpsImageSources.Organization:
					result = Properties.Resources.entityDrawingGroup;
					break;
				case DpsImageSources.Vessel:
					result = Properties.Resources.vesselDrawingGroup;
					break;
				case DpsImageSources.Country:
					result = Properties.Resources.countryDrawingGroup;
					break;
				case DpsImageSources.Person:
					result = Properties.Resources.personDrawingGroup;
					break;
				case DpsImageSources.Warning:
					result = Properties.Resources.warningDrawingGroup;
					break;
				case DpsImageSources.WarningOrange:
					result = Properties.Resources.warning_orangeDrawingGroup;
					break;
				case DpsImageSources.WarningRed:
					result = Properties.Resources.warning_redDrawingGroup;
					break;
				case DpsImageSources.WarningWhite:
					result = Properties.Resources.warning_whiteDrawingGroup;
					break;
				default:
					return null;
			}

			return result;
		}
	}
}
