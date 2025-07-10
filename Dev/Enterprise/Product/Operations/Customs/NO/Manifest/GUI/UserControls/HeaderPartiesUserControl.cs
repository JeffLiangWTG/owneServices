using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI
{
	sealed partial class HeaderPartiesUserControl : ZUserControl, IAdditionalTabPage
	{
		public HeaderPartiesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DynamicHeaderPartiesPanel.UpdateLayout(new NOBillPartiesLayout());
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("D36C821D-2689-467F-84A5-59047568EBF9", "Header Parties");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 1;

		#endregion
	}
}
