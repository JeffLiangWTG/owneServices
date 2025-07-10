using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AddressLevelExporterSchemeControl : ZUserControl
	{
		public AddressLevelExporterSchemeControl()
		{
			InitializeComponent();
		}

		public AddressLevelExporterSchemeControl(ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				if (!supplyChainSecurityConfiguration.UseIssuingAuthorityCountry)
				{
					CountryDataCollectionForThisCompanyGrid.SetAvailability(false, "OV_RN_NKIssuingAuthorityCountry");
				}
			}
		}

		protected virtual ResourceStringData GroupBoxCaption => Res.GetData("AddressLevelExporterSchemeControl|4f3030e2-b8c7-4332-8b7b-f82ec8842eee", "Aviation Security");
	}
}
