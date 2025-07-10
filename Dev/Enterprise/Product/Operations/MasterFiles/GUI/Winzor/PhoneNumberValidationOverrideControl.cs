using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class PhoneNumberValidationOverrideControl : ZUserControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Styling strings")]
		public override string ExtraStyleString
		{
			get
			{
				switch (BorderStyle)
				{
					case BorderStyle.FixedSingle:
						return "border: 1px solid #646464;";
					case BorderStyle.Fixed3D:
						return "border: 1px solid; border-color: #a0a0a0 #fff #fff #a0a0a0;";
					case BorderStyle.None:
					default:
						return "border: none;";
				}
			}
		}
	}
}
