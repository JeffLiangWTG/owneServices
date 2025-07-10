using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class InvoiceHeaderOrganisationsUserControl : ZUserControl
	{
		public InvoiceHeaderOrganisationsUserControl()
		{
			InitializeComponent();

			JZ_OA_ManufacturerAddressAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			JZ_OA_SupplierAddressAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			InvoicerDocAddressAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			ACEForeignExporterAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
		}

		public void UpdateControlVisibilityAndCaptions(bool isACE)
		{
			AIIExporterGuidFindBox.Visible = !isACE;
			InvoicerDocAddressAddressControl.Visible = !isACE;
			AIISellingAgentGuidFindBox.Visible = !isACE;
			AIIBuyingAgentGuidFindBox.Visible = !isACE;

			ACEForeignExporterAddressControl.Visible = isACE;
			JZ_OA_ShipperAddressControl.Visible = isACE;
			JZ_OA_DistributorAddressControl.Visible = isACE;
			JZ_OA_PackagerAddressControl.Visible = isACE;

			if (isACE)
			{
				JZ_OA_ConsigneeAddressControl.GetExtension<ILabelCaptionRenderer>().Caption = "Consignee";
			}
			else
			{
				JZ_OA_ConsigneeAddressControl.GetExtension<ILabelCaptionRenderer>().Caption = "Ultimate Consignee";
			}
		}
	}
}
