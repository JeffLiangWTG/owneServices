using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Control for entering RefUNLOCO Coordinates
	/// </summary>
	public partial class CoordinateEntryControl : ZUserControl
	{
		public CoordinateEntryControl() : base()
		{
			InitializeComponent();
		}

		ZString fBindTo;

		[Browsable(true),
		Category(ZGUIConstants.DesignerCategory),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string BindToTest
		{
			get { return fBindTo; }
			set
			{
				fBindTo = value;

				LatitudeTextBox.BindTo = "Latitude";
				LongitudeTextBox.BindTo = "Longitude";
			}
		}
	}
}
