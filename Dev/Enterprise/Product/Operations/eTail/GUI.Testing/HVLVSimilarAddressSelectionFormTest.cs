using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVSimilarAddressSelectionForm))]
	public class HVLVSimilarAddressSelectionFormTest : ZFormBasherTest
	{
		public void TestSelectedAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Company";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Test Address";
			address.OA_City = "Test City";
			Factory.Save();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ConsigneeName = "Test Company";
			consignment.HVC_ConsigneeAddress1 = "Test Address";

			using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee))
			{
				form.Show();

				var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
				similarAddressGrid.Select(0);

				var selectAddressButton = form.Controls.Find("SaveNewOrgButton", true).Single() as ZButton;
				selectAddressButton.PerformClick();

				AssertEquals("Test City", consignment.HVC_ConsigneeCity);
			}
		}

		public void TestOpenNewOrganisationForm()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ShipperName = "Test Company";
			consignment.HVC_ShipperAddress1 = "Test Address";
			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_ShipperPostcode = "123456";

			using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Shipper))
			{
				form.Show();

				var address = default(OrgAddress);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
				{
					var organizationForm = shownForm as ZOrganisationsForm;
					AssertNotNull("Should open organization form", organizationForm);

					address = ((OrgHeader)organizationForm.BusinessEntity).MainAddress;
				});

				var newOrganizationButton = form.Controls.Find("newOrganizationButton", true).Single() as ZButton;
				newOrganizationButton.PerformClick();

				CombineAssertions("Form should be pre-filled with consignment details", () =>
				{
					AssertEquals("Test Address", address.OA_Address1);
					AssertEquals("AU", address.OA_RN_NKCountryCode);
					AssertEquals("NSW", address.OA_State);
					AssertEquals("123456", address.OA_PostCode);
				});
			}
		}

		public void TestSelectedAddress_ShouldUpdateWhenNewOrganisationCreated()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ShipperName = "Test Company";
			consignment.HVC_ShipperAddress1 = "Test Address";
			consignment.HVC_ShipperCity = "Sydney";
			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_ShipperPostcode = "123456";

			using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Shipper))
			{
				form.Show();

				var orgHeader = default(OrgHeader);
				var address = default(OrgAddress);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
				{
					var organizationForm = shownForm as ZOrganisationsForm;
					AssertNotNull("Should open organization form", organizationForm);

					orgHeader = organizationForm.BusinessEntity as OrgHeader;
					orgHeader.OH_RL_NKClosestPort = "AUSYD";
					address = orgHeader.MainAddress;

					organizationForm.FireSaveButton();
				});

				var newOrganizationButton = form.Controls.Find("newOrganizationButton", true).Single() as ZButton;
				newOrganizationButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("New organization should be created", true, orgHeader.IsInDatabase);
					AssertEquals("Should update selected address", address.PK, consignment.HVC_OA_ShipperAddress);
				});
			}
		}

		public void TestOpenNewOrganizationForm_WhenUserDoesNotHaveSecurityRight_ShowsWarning()
		{
			var initialValue = Env.Security.OrganisationNew.IsAllowed;
			try
			{
				var tempHeader = Factory.New<OrgHeader>();
				tempHeader.MainAddress.OA_Address1 = "Test Address";
				tempHeader.MainAddress.OA_RN_NKCountryCode = "AU";
				tempHeader.MainAddress.OA_State = "NSW";
				tempHeader.MainAddress.OA_PostCode = "123456";

				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_ShipperName = "Test Company";
				consignment.HVC_ShipperAddress1 = "Test Address";
				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_ShipperState = "NSW";
				consignment.HVC_ShipperPostcode = "123456";

				using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Shipper))
				{
					form.Show();

					var newOrganizationButton = form.Controls.Find("newOrganizationButton", true).Single() as ZButton;

					Env.Security.OrganisationNew.IsAllowed = false;
					newOrganizationButton.PerformClick();

					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					CombineAssertions("Should show insufficient access error", () =>
					{
						AssertEquals("Message type was error", true, lastMessage.WasError);
						AssertEquals("Message caption", "Insufficient Access", lastMessage.Caption);
						AssertEquals("Message text", "You do not have sufficient security rights to create a new Organization.", lastMessage.Text);
					});

					UnitTestUserNotification.Instance.ClearMessages();
					Env.Security.OrganisationNew.IsAllowed = true;
					newOrganizationButton.PerformClick();

					AssertEquals("No message shown when access is granted", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
			finally
			{
				Env.Security.OrganisationNew.IsAllowed = initialValue;
			}
		}

		public void TestWarningLabel_Consignee()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee))
			{
				form.Show();
				var warningLabel = form.Controls.Find("WarningLabel", true).Single() as ZLabel;

				AssertEquals("Similar organizations that match Consignee details already exist. You may select one of them or create a new one.", warningLabel.Text);
			}
		}

		public void TestWarningLabel_Shipper()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee))
			{
				form.Show();
				var warningLabel = form.Controls.Find("WarningLabel", true).Single() as ZLabel;

				AssertEquals("Similar organizations that match Consignee details already exist. You may select one of them or create a new one.", warningLabel.Text);
			}
		}

		public void TestSelectAddress_WhenNoAddressSelected_ShowsError()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Company";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Test Address";
			address.OA_City = "Test City";
			Factory.Save();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ConsigneeName = "Test Company";
			consignment.HVC_ConsigneeAddress1 = "Test Address";

			using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee))
			{
				form.Show();

				var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
				similarAddressGrid.UnSelectAll();

				var selectAddressButton = form.Controls.Find("SaveNewOrgButton", true).Single() as ZButton;
				selectAddressButton.PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions("Should show no org selected error", () =>
				{
					AssertEquals("Message type was error", true, lastMessage.WasError);
					AssertEquals("Message text", "Please select an address.", lastMessage.Text);
				});
			}
		}

		public void TestSelectAddress_WhenMultipleAddressesSelected_ShowsError()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG1";
			orgHeader2.OH_Code = "TESTORG2";
			orgHeader1.OH_FullName = "Test Company";
			orgHeader2.OH_FullName = "Test Company";

			var address1 = orgHeader1.Addresses.AddNew();
			var address2 = orgHeader2.Addresses.AddNew();
			address1.OA_Code = "TESTADD1";
			address1.OA_Code = "TESTADD2";
			address1.OA_Address1 = "Test Address";
			address2.OA_Address1 = "Test Address";
			address1.OA_City = "Test City";
			address2.OA_City = "Test City";

			Factory.Save();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ConsigneeName = "Test Company";
			consignment.HVC_ConsigneeAddress1 = "Test Address";

			using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee))
			{
				form.Show();

				var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
				similarAddressGrid.SelectAllElements();

				var selectAddressButton = form.Controls.Find("SaveNewOrgButton", true).Single() as ZButton;
				selectAddressButton.PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions("Should show multiple orgs selected error", () =>
				{
					AssertEquals("Message type was error", true, lastMessage.WasError);
					AssertEquals("Message text", "Please select only one address.", lastMessage.Text);
				});
			}
		}

		public void TestConsignment()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			using (var form = new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee))
			{
				AssertEquals("Should store correct consignment", consignment.PK, form.Consignment.PK);
			}
		}

		public void TestAddressType()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();

			using (var form1 = new HVLVSimilarAddressSelectionForm(consignment1, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee))
			using (var form2 = new HVLVSimilarAddressSelectionForm(consignment2, Enterprise.Integration.Customs.ConsignmentAddressType.Shipper))
			{
				CombineAssertions("Should store correct address type", () =>
				{
					AssertEquals(Enterprise.Integration.Customs.ConsignmentAddressType.Consignee, form1.AddressType);
					AssertEquals(Enterprise.Integration.Customs.ConsignmentAddressType.Shipper, form2.AddressType);
				});
			}
		}

		#region Implementations

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.New<HVLVForwardingShipment>();
			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			consignment.Items.AddNew();

			Factory.Save();

			return new HVLVSimilarAddressSelectionForm(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Shipper);
		}

		const bool isFormBusinessEntityANewTempOrgHeaderMustHasChanges = true;
		protected override bool AllowHasChangesOnFormOpen => isFormBusinessEntityANewTempOrgHeaderMustHasChanges;

		#endregion
	}
}
