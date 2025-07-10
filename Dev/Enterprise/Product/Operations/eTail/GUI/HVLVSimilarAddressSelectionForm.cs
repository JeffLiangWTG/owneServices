using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ConsignmentAddressType = Enterprise.Integration.Customs.ConsignmentAddressType;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVSimilarAddressSelectionForm : DuplicateOrgForm
	{
		public HVLVSimilarAddressSelectionForm(HVLVConsignment consignment, ConsignmentAddressType addressType)
			: this(GenerateTemporaryOrganization(consignment, addressType))
		{
			Consignment = consignment;
			AddressType = addressType;
			UpdateWarningLabel(AddressType);
		}

		HVLVSimilarAddressSelectionForm(OrgHeader organisation)
			: base(organisation)
		{
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);
			organisation.SimilarOrgFinder.FindSimilarOrganisations();

			InitializeComponent();
			DialogResult = DialogResult.Cancel;
		}

		public static bool Convert(HVLVConsignment consignment, ConsignmentAddressType addressType)
		{
			using var form = new HVLVSimilarAddressSelectionForm(consignment, addressType);
			ZFormModaliser.ShowDialogWithoutDispose(form);
			return ContinueWithSave.Yes == form.UserDecision;
		}

#if DEBUG
		public
#endif
		readonly HVLVConsignment Consignment;

#if DEBUG
		public
#endif
		readonly ConsignmentAddressType AddressType;

		new OrgHeader BusinessEntity => (OrgHeader)base.BusinessEntity;

		static OrgHeader GenerateTemporaryOrganization(HVLVConsignment consignment, ConsignmentAddressType addressType)
		{
			var result = consignment.Factory.New<OrgHeader>();
			result.OverrideIsSavedByFactory(false);
			FillOrgHeaderWithConsignmentDetails(result, consignment, addressType);
			return result;
		}

		void UpdateWarningLabel(ConsignmentAddressType addressType)
		{
			ResourceStringData caption;
			switch (addressType)
			{
				case ConsignmentAddressType.Consignee:
					caption = Res.GetData("HVLVSimilarAddressSelectionForm|c1d9b7ba-0a93-46d3-b864-51f214331496", "Similar organizations that match Consignee details already exist. You may select one of them or create a new one.");
					break;
				case ConsignmentAddressType.Shipper:
					caption = Res.GetData("HVLVSimilarAddressSelectionForm|d4d888f7-b04f-4451-9e24-4864a9e8c8fd", "Similar organizations that match Shipper details already exist. You may select one of them or create a new one.");
					break;
				case ConsignmentAddressType.Return:
					caption = Res.GetData("HVLVSimilarAddressSelectionForm|858ec651-2d70-408c-accd-2a65e278228a", "Similar organizations that match Return Location details already exist. You may select one of them or create a new one.");
					break;
				default:
					return;
			}

			WarningLabel.CaptionResourceString = caption;
		}

		static void FillOrgHeaderWithConsignmentDetails(OrgHeader org, HVLVConsignment consignment, ConsignmentAddressType addressType)
		{
			var mainAddress = org.MainAddress;
			if (addressType == ConsignmentAddressType.Shipper)
			{
				org.OH_IsConsignor = true;
				org.OH_FullName = consignment.HVC_ShipperName.Substring(0, org.OH_FullNameInfo.MaxLength);
				mainAddress.OA_Address1 = consignment.HVC_ShipperAddress1.Substring(0, mainAddress.OA_Address1Info.MaxLength);
				mainAddress.OA_Address2 = consignment.HVC_ShipperAddress2.Substring(0, mainAddress.OA_Address2Info.MaxLength);
				mainAddress.OA_City = consignment.HVC_ShipperCity;
				mainAddress.OA_State = consignment.HVC_ShipperState.Substring(0, mainAddress.OA_StateInfo.MaxLength);
				mainAddress.OA_PostCode = consignment.HVC_ShipperPostcode.Substring(0, mainAddress.OA_PostCodeInfo.MaxLength);
				mainAddress.OA_RN_NKCountryCode = consignment.HVC_RN_NKShipperCountryCode;
				mainAddress.OA_Phone = consignment.HVC_ShipperPhone;
				mainAddress.OA_Mobile = consignment.HVC_ShipperMobile;
				mainAddress.OA_Fax = consignment.HVC_ShipperFax;
				mainAddress.OA_Email = consignment.HVC_ShipperEmail;
			}
			else if (addressType == ConsignmentAddressType.Consignee)
			{
				org.OH_IsConsignee = true;
				org.OH_FullName = consignment.HVC_ConsigneeName.Substring(0, org.OH_FullNameInfo.MaxLength);
				mainAddress.OA_Address1 = consignment.HVC_ConsigneeAddress1.Substring(0, mainAddress.OA_Address1Info.MaxLength);
				mainAddress.OA_Address2 = consignment.HVC_ConsigneeAddress2.Substring(0, mainAddress.OA_Address2Info.MaxLength);
				mainAddress.OA_City = consignment.HVC_ConsigneeCity;
				mainAddress.OA_State = consignment.HVC_ConsigneeState.Substring(0, mainAddress.OA_StateInfo.MaxLength);
				mainAddress.OA_PostCode = consignment.HVC_ConsigneePostcode.Substring(0, mainAddress.OA_PostCodeInfo.MaxLength);
				mainAddress.OA_RN_NKCountryCode = consignment.HVC_RN_NKConsigneeCountryCode;
				mainAddress.OA_Phone = consignment.HVC_ConsigneePhone;
				mainAddress.OA_Mobile = consignment.HVC_ConsigneeMobile;
				mainAddress.OA_Fax = consignment.HVC_ConsigneeFax;
				mainAddress.OA_Email = consignment.HVC_ConsigneeEmail;
			}
		}

		void SimilarOrgDisplayGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				SaveNewOrgButton.PerformClick();
			}
		}

		void NewOrganizationButton_Click(object sender, EventArgs e)
		{
			if (Consignment != null)
			{
				if (Env.Security.OrganisationNew.IsAllowed)
				{
					var factory = new BusinessObjectFactory();
					var newOrganisation = factory.New<OrgHeader>();
					FillOrgHeaderWithConsignmentDetails(newOrganisation, Consignment, AddressType);
					var form = new ZOrganisationsForm(newOrganisation);

					form.Closed += delegate
					{
						var organization = form.BusinessEntity as OrgHeader;
						if (organization.IsInDatabase)
						{
							LinkAddressToConsignment(organization.MainAddress.PK);
							UserDecision = ContinueWithSave.Yes;
							DialogResult = DialogResult.Yes;
						}
					};
					ZFormModaliser.ShowDialogAndDispose(form);
				}
				else
				{
					Globals.Message.ShowError(
						Res.GetString("067048eb-b2f1-4c0e-b586-e6a96db73997", "You do not have sufficient security rights to create a new Organization."),
						Res.GetString("fc8e95d5-a97b-4f1c-a16e-e27a164a14c1", "Insufficient Access"));
				}
			}
		}

		protected override bool SaveCore()
		{
			var result = base.SaveCore();
			if (result)
			{
				if (SimilarOrgsDisplayGrid.SelectedRowCount < 1)
				{
					Globals.Message.ShowError(Res.GetString("ca314357-db20-414f-8fa3-6438241ca275", "Please select an address."));
					return false;
				}

				if (SimilarOrgsDisplayGrid.SelectedRowCount > 1)
				{
					Globals.Message.ShowError(Res.GetString("c525e9e1-1f9b-4332-b9ed-1eb7549632a5", "Please select only one address."));
					return false;
				}

				LinkAddressToConsignment(((OrgPatternMatch)SimilarOrgsDisplayGrid.GetFirstSelectedRow()).Address.PK);
			}

			return result;
		}

		void LinkAddressToConsignment(ZGuid addressPK)
		{
			switch (AddressType)
			{
				case ConsignmentAddressType.Consignee:
				Consignment.HVC_OA_ConsigneeAddress = addressPK;
				break;
				case ConsignmentAddressType.Shipper:
				Consignment.HVC_OA_ShipperAddress = addressPK;
				break;
				case ConsignmentAddressType.Return:
				Consignment.HVC_OA_ReturnLocation = addressPK;
				break;
				default:
				break;
			}
		}

		protected override void Dispose(bool disposing)
		{
			BusinessEntity?.Delete();
			base.Dispose(disposing);
		}
	}
}
