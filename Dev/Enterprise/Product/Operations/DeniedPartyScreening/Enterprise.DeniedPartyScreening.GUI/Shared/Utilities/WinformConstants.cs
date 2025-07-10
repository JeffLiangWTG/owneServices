using System.Drawing;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class WinformConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Arrow constants")]
		public const string ArrowUp = "⯅ ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Arrow constants")]
		public const string ArrowDown = "⯆ ";
		public static readonly Color SelectedColor = Color.FromArgb(203, 232, 246);
		public static readonly Color UnselectedColor = Color.White;
		public static readonly Color BorderColor = Color.FromArgb(38, 160, 218);
		public static readonly Color EnableButtonColor = Color.FromArgb(0, 168, 225);
		public static readonly Color DisableButtonColor = Color.FromArgb(169, 170, 169);
		public const int MatchItemPanelHeight = 16;
	}
}
