using System.Drawing;
using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR.GUI
{
	public static class JobDeclarationColorHelper
	{
		public static class ColorConstants
		{
			public static readonly Color Undefined = Color.Empty;
			public static readonly Color Registered = Color.FromArgb(198, 236, 198);
			public static readonly Color Question = Color.FromArgb(255, 179, 179);
			public static readonly Color SupportingDocument = Color.FromArgb(255, 179, 179);
			public static readonly Color Warning = Color.FromArgb(255, 179, 179);
		}

		public static Color GetColorForEntryStatus(string status)
		{
			switch (status)
			{
				case EntryStatusTypeList.Codes.REG:
					return ColorConstants.Registered;
				case EntryStatusTypeList.Codes.QUE:
					return ColorConstants.Question;
				case EntryStatusTypeList.Codes.SDO:
					return ColorConstants.SupportingDocument;
				case EntryStatusTypeList.Codes.WRN:
					return ColorConstants.Warning;
				default:
					return ColorConstants.Undefined;
			}
		}
	}
}
