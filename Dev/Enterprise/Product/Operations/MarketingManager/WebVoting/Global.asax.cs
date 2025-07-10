using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.MarketingManager.WebVoting
{
	public class Global : ZGlobal
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public override string DefaultPage
		{
			get { return "~/Default.aspx"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public override string LoginPage
		{
			get { return "~/Login.aspx"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is part of a URL.")]
		public override string LogoImage
		{
			get { return "~/Images/logo.png"; }
		}
	}
}
