using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public partial class USExportVisitedPortUserControl : ZUserControl, IAdditionalTabPage
	{
		public USExportVisitedPortUserControl()
		{
			InitializeComponent();
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("04C02E4F-21D8-4B99-8DC5-9027DBD541A2", "Visited Ports");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);
		public int TabPageSequence => 2;
	}
}
