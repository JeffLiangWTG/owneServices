using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class JobMainOrgsControl : ZUserControl
	{
		public JobMainOrgsControl()
		{
			InitializeComponent();
			ManufacturerAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
		}

		internal void BondedWarehouseDocAddressRelatedFieldChanged(bool isBondedWarehouseEditable)
		{
			BondedWarehouseDocAddressControl.Visible = isBondedWarehouseEditable;
		}

		internal void ShowOrHideFTZControls(bool isVisibleForFTZ, bool isConsumptionFTZ)
		{
			if (isVisibleForFTZ)
			{
				BondedWarehouseDocAddressControl.CaptionResourceString = new ResourceStringData("USJobDeclarationUserControl|B2635829-19A7-43ED-87A4-6DF181ECD4BF", "FTZ Operator");
				BondedWarehouseDocAddressControl.Text = ResString.GetMultilingualString("USJobDeclarationUserControl|B2635829-19A7-43ED-87A4-6DF181ECD4BF", "FTZ Operator");
			}
			else
			{
				if (isConsumptionFTZ)
				{
					BondedWarehouseDocAddressControl.CaptionResourceString = new ResourceStringData("USJobDeclarationUserControl|113AF109-C154-474F-8FA9-9C0D95AF3882", "FTZ");
					BondedWarehouseDocAddressControl.Text = ResString.GetMultilingualString("USJobDeclarationUserControl|113AF109-C154-474F-8FA9-9C0D95AF3882", "FTZ");
				}
				else
				{
					BondedWarehouseDocAddressControl.CaptionResourceString = new ResourceStringData("USJobDeclarationUserControl|5D6F208A-873A-45BD-B915-92CD7E3D85F0", "Bonded Warehouse");
					BondedWarehouseDocAddressControl.Text = ResString.GetMultilingualString("USJobDeclarationUserControl|5D6F208A-873A-45BD-B915-92CD7E3D85F0", "Bonded Warehouse");
				}
			}
			ApplicantOrgControl.Visible = isVisibleForFTZ;
			ImporterOfRecordOrgControl.Visible = !isVisibleForFTZ;
			PTTCarrierOrgControl.Visible = isVisibleForFTZ;
		}

		internal void UpdateControlCaption(bool isACE)
		{
			if (isACE)
			{
				UltimateConsigneeAddressControl.CaptionResourceString = new ResourceStringData("USJobDeclarationUserControl|2639B035-45AD-4331-8C3D-45DAE11FCE82", "Consignee");
			}
			else
			{
				UltimateConsigneeAddressControl.CaptionResourceString = new ResourceStringData("USJobDeclarationUserControl|69D637C1-DB5F-4567-8A1A-2CF751E6C8C6", "Ultimate Consignee");
			}
		}
	}
}
