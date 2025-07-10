using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ZoneSelectionControl : ZUserControl
	{
		public ZoneSelectionControl()
		{
			InitializeComponent();

			isDomesticCheckBox.CheckedChanged += isDomesticCheckBox_CheckedChanged;
		}

		void isDomesticCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			var isDomestic = isDomesticCheckBox.Checked;
			domesticZoneOwnerFindBox.Visible = isDomestic;
			domesticZoneCountryFindBox.Visible = isDomestic;
			internationalZoneCarrierFindBox.Visible = !isDomestic;
		}
	}
}
