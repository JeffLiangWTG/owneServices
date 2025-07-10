using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ZOrganisationsForm))]
	public class OrganisationFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public override void TestMinimumSizeNotTooBig()
		{
			string formName;
			using (Form testForm = GetFormToBash())
			{
				formName = testForm.Name;

				int minScreenWidthSupported = ControlDpiScalingHelper.ScaleToCurrentDpiX(1200);
				int minScreenHeightSupported = ControlDpiScalingHelper.ScaleToCurrentDpiY(768);
				int typicalTaskbarHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(43);

				int maxSizeWidth = minScreenWidthSupported;
				int maxSizeHeight = minScreenHeightSupported - typicalTaskbarHeight;

				Assert("Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(), testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(), testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		public void TestSave()
		{
			var orgInEditForm = Factory.NewWithValidTestData<OrgHeader>();
			orgInEditForm.OH_Code = "ABC";
			orgInEditForm.OH_FullName = "Test Org";
			orgInEditForm.MainAddress.OA_Address1 = "Address 1";
			orgInEditForm.MainAddress.OA_RN_NKCountryCode = "AU";
			orgInEditForm.OH_RL_NKClosestPort = "AUSYD";
			orgInEditForm.MainAddress.Postcode = "2000";
			orgInEditForm.MainAddress.State = "NSW";
			orgInEditForm.MainAddress.City = "Sydney";
			orgInEditForm.OH_IsConsignee = true;
			orgInEditForm.OH_IsBroker = true;

			using (var editForm = new OrgFormForTest(orgInEditForm))
			{
				editForm.Show();
				editForm.FireSaveButton();
				AssertEquals("There should be 0 message: " + UnitTestUserNotification.Instance.LastMessage, 0, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			}
		}

		public void TestDefaultRelatedParties_SaveWithoutViewing()
		{
			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg.OH_IsBroker = true;
			OrganisationsDataRegistry.Instance.ImportSeaBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, relatedOrg.PK.ToGuid());
			OrganisationsDataRegistry.Instance.ImportAirBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, relatedOrg.PK.ToGuid());
			OrganisationsDataRegistry.Instance.ExportSeaBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, relatedOrg.PK.ToGuid());
			OrganisationsDataRegistry.Instance.ExportAirBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, relatedOrg.PK.ToGuid());

			var orgInEditForm = Factory.NewWithValidTestData<OrgHeader>();
			orgInEditForm.OH_Code = "ABC";
			orgInEditForm.OH_FullName = "Test Org";
			orgInEditForm.MainAddress.OA_Address1 = "Address 1";
			orgInEditForm.MainAddress.OA_RN_NKCountryCode = "AU";
			orgInEditForm.OH_RL_NKClosestPort = "AUSYD";
			orgInEditForm.MainAddress.Postcode = "2000";
			orgInEditForm.MainAddress.State = "NSW";
			orgInEditForm.MainAddress.City = "Sydney";
			orgInEditForm.OH_IsConsignee = true;
			orgInEditForm.OH_IsBroker = true;

			using (var editForm = new OrgFormForTest(orgInEditForm))
			{
				editForm.Show();
				//try to save before accessing RelatedParties tab/property and verify save happens with no validation error stopping it
				editForm.FireSaveButton();

				AssertEquals("There should be 0 message: " + UnitTestUserNotification.Instance.LastMessage, 0, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			}

			//verify correct data in RelatedParties
			AssertRelatedParty(relatedOrg, GlbCompany.CurrentCompany, orgInEditForm.AllRelatedParties, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			AssertRelatedParty(relatedOrg, GlbCompany.CurrentCompany, orgInEditForm.AllRelatedParties, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			AssertRelatedParty(relatedOrg, GlbCompany.CurrentCompany, orgInEditForm.AllRelatedParties, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			AssertRelatedParty(relatedOrg, GlbCompany.CurrentCompany, orgInEditForm.AllRelatedParties, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
		}

		void AssertRelatedParty(OrgHeader expectedOrg, GlbCompany expectedCompany, OrgRelatedPartyCompanySpecificCollection parties, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			var party = parties.GetRelatedParty(partyType, direction, transportMode, containerMode);
			AssertEquals("PR_OH_RelatedParty", expectedOrg.PK, party.PR_OH_RelatedParty);
			AssertEquals("PR_GC", expectedCompany.PK, party.PR_GC);
		}

		/// <summary>
		///  The reason is because refresh binding must be called after successful save for Org security to be refreshed and take effect.
		/// </summary>
		public void TestSavingFormCallsRefreshBindingIncludingChildren()
		{
			bool headerRefreshed = false;
			bool contactRefreshed = false;

			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact testContact = testHeader.Contacts.AddNew();

			((IBusiness)testHeader).ListChanged += delegate
			{
				headerRefreshed = true;
			};

			((IBusiness)testContact).ListChanged += delegate
			{
				contactRefreshed = true;
			};

			using (ZOrganisationsForm testForm = new ZOrganisationsForm(testHeader))
			{
				testForm.Show();
				Factory.Save();
			}

			Factory.Save();
			AssertEquals("The header was refreshed", true, headerRefreshed);
			AssertEquals("The contact was refreshed", true, contactRefreshed);
		}

		[RequiresSTA]
		public void TestSaveAndDeleteOrganization_DifferentFactories()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CONSPA";
			org.OH_RL_NKClosestPort = "AUSYD";
			var mainAddress = org.Addresses.MainAddress;
			mainAddress.Address1 = "74 O'RIORDAN STREET";
			mainAddress.Postcode = "2015";
			mainAddress.City = "ALEXANDRIA";
			Factory.Save();

			// In functional testing, there should be 2 instances of CW1 running.
			// In here, we disable factory refresh to mimic that.
			var orgInEditForm = Factory.CreateNewFactory().Load<OrgHeader>(org.PK);
			var orgToBeDeleted = Factory.CreateNewFactory().Load<OrgHeader>(org.PK);
			orgInEditForm.Factory.RefreshEnabled = false;
			orgToBeDeleted.Factory.RefreshEnabled = false;

			using (var editForm = new OrgFormForTest(orgInEditForm))
			{
				editForm.Show();
				var detailsControl = (MainDetailsUserControl)editForm.OrganisationsTabControl.TabPages["DetailsTabPage"].Controls["DetailsControl"];
				detailsControl.DetailsTabControl.SelectedIndex = 3;
				orgInEditForm.OH_IsDebtor = true;
				orgInEditForm.CompanyData.OB_OJ_ARDebtorGroup = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery()).PK;
				orgInEditForm.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
				orgInEditForm.Factory.Save();

				// The org is deleted in one factory
				orgToBeDeleted.Delete();
				orgToBeDeleted.Factory.Save();

				// The org is then saved in another factory with concurrency exception handling
				UnitTestUserNotification.Instance.ClearMessages();
				editForm.FireSaveButton();

				AssertEquals("There should be 1 message", 1, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				Assert(lastMessage.WasError);
				AssertEquals("Because this Organization (CONSPASYD) has been deleted while you were editing it, this form will be closed.", lastMessage.Text);

				Assert("Form should be closed", !editForm.Visible);
			}
		}

		public void TestEditAndDeleteOrganization_SameFactory()
		{
			// This test looks impractical but it is a simplified version of a real situation:
			// - An organization is loaded in an editing form
			// - From Organization Module, we choose the org and delete it
			// - The Organization controller looks for opening form and change its display mode to Delete
			// - Because of that, the form, the org itself and its BizOFactory do not change.
			var org = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				var detailsControl = (MainDetailsUserControl)form.OrganisationsTabControl.TabPages["DetailsTabPage"].Controls["DetailsControl"];
				detailsControl.DetailsTabControl.SelectedIndex = 3;
				org.OH_IsDebtor = true;
				org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

				// Let's skip ShowDeleteForm, ShowLoadedForm, SwitchToDeleteForm and jump to the deletion end
				AssertNoExceptionThrown(() =>
				{
					form.DisplayMode = ODisplayMode.Delete;
					form.PostButton.PerformClick();
				});
			}
		}

		public void TestExternalValidationProgressFormIsNotShown()
		{
			var rawRegistryValue = DataRegistry.Instance.EnableExternalValidationService;

			try
			{
				var testHeader = Factory.NewWithValidTestData<OrgHeader>();
				DataRegistry.Instance.EnableExternalValidationService = false;

				using (var testForm = new ZOrganisationsForm(testHeader))
				{
					testForm.Show();
					Application.DoEvents();
					Factory.Save();
					Application.DoEvents();
					Assert("External validation progress form is not shown.", !testForm.isExternalValidationProgressFormShownForTest);
				}
			}
			finally
			{
				DataRegistry.Instance.EnableExternalValidationService = rawRegistryValue;
			}
		}

		public void TestRegenerateProgressFormShown()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "ABC DEF XYZ";
			org.OH_IsNationalAccount = false;
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_Address1 = "Bourke Street3";
			address.OA_Email = "XXXXXX@wisetech.com";
			address.OA_Phone = "1234567909";
			Factory.Save();

			using (ZOrganisationsForm testForm = new ZOrganisationsForm(org))
			{
				testForm.Show();
				Application.DoEvents();
				org.PatternMatchingRecalculator.Regenerate(ObjectFactory.Get<IMasterDataProvider>().GetDeduplicationOrgHeader(org));
				Assert("Regenerate progressForm form is  shown.", testForm.isrecalculateprogressFormShownForTest);
			}
		}

		public void TestExternalValidationProgressFormIsShown()
		{
			var rawRegistryValue = DataRegistry.Instance.EnableExternalValidationService;

			try
			{
				var testHeader = Factory.NewWithValidTestData<OrgHeader>();
				DataRegistry.Instance.EnableExternalValidationService = true;
				WeakReference<ExternalValidationProgressForm> progressFormWeakReference;

				using (var testForm = new ZOrganisationsForm(testHeader))
				{
					testForm.Show();
					Application.DoEvents();
					Factory.Save();
					Application.DoEvents();
					progressFormWeakReference = new WeakReference<ExternalValidationProgressForm>(testForm.progressForm);
					Assert("External validation progress form is shown.", testForm.isExternalValidationProgressFormShownForTest);
				}

				Assert(!progressFormWeakReference.TryGetTarget(out ExternalValidationProgressForm progressForm) || progressForm.IsDisposed);
			}
			finally
			{
				DataRegistry.Instance.EnableExternalValidationService = rawRegistryValue;
			}
		}

		[RequiresSTA]
		public void TestSaveFormAsPerRequireARContact()
		{
			AssertRequireARContact(false, RawDataRegistry.Instance.OrgDebtorRequiredFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, true)
					.GetRegistryValue()));

			AssertRequireARContact(true, RawDataRegistry.Instance.TempOrgDebtorRequiredFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, true)
					.GetRegistryValue()));
		}

		void AssertRequireARContact(bool isTempAccount, IDisposable registryItem)
		{
			var arDocument = Factory.NewWithValidTestData<OrgDocument>();
			arDocument.OD_DocumentGroup = ContactType.Receivables.Code;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			arDocument.OD_OC = contact.PK;
			contact.Documents.Add(arDocument);
			contact.OC_IsActive = true;

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.OH_IsDebtor = true;
			testHeader.OH_IsTempAccount = isTempAccount;
			testHeader.Contacts.Add(contact);

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals(true, contact.Documents.Cast<OrgDocument>().Any(d => d.OD_DocumentGroup == ContactType.Receivables.Code));
				AssertEquals(true, testHeader.OH_IsDebtor);
				AssertEquals(isTempAccount, testHeader.OH_IsTempAccount);
				AssertEquals(testHeader, contact.ParentOrg);
				AssertEquals(1, testHeader.GetActiveContacts().Count);
			});

			using (registryItem)
			{
				using (var testForm = new OrgFormForTest(testHeader))
				{
					testForm.Show();

					UnitTestUserNotification.Instance.ClearMessages();
					testForm.ValidateAndSave();
					AssertNotEquals("There should always be at least one active A/R Contact on Debtor Organizations.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				contact.OC_IsActive = false;
				AssertEquals("Precondition: ", 0, testHeader.GetActiveContacts().Count);

				using (var testForm = new OrgFormForTest(testHeader))
				{
					testForm.Show();

					UnitTestUserNotification.Instance.ClearMessages();
					testForm.ValidateAndSave();
					AssertEquals("There should always be at least one active A/R Contact on Debtor Organizations.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				testHeader.OH_IsDebtor = false;
				AssertEquals("Precondition: ", false, testHeader.OH_IsDebtor);

				using (var testForm = new OrgFormForTest(testHeader))
				{
					testForm.Show();

					UnitTestUserNotification.Instance.ClearMessages();
					testForm.ValidateAndSave();
					AssertNotEquals("There should always be at least one active A/R Contact on Debtor Organizations.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestOrgFormClosedWhenUserClickSaveAndCloseButton()
		{
			var rawRegistryValue = DataRegistry.Instance.EnableExternalValidationService;

			try
			{
				var testHeader = Factory.NewWithValidTestData<OrgHeader>();
				DataRegistry.Instance.EnableExternalValidationService = true;

				using (var testForm = new OrgFormForTest(testHeader))
				{
					testForm.Show();
					Application.DoEvents();
					Factory.Save();
					Application.DoEvents();
					var progressFormWeakReference = new WeakReference<ExternalValidationProgressForm>(testForm.progressForm);
					Assert("Precondition", testForm.Visible);
					Assert("External validation progress form is shown.", testForm.isExternalValidationProgressFormShownForTest);

					testForm.IsSaveAndCloseButtonClickedForTest = false;
					testForm.CloseProgressFormAndOrgFormIfNeededForTest();
					Assert(testForm.Visible);
					Assert(!progressFormWeakReference.TryGetTarget(out var progressForm) || progressForm.IsDisposed);

					testForm.IsSaveAndCloseButtonClickedForTest = true;
					testForm.CloseProgressFormAndOrgFormIfNeededForTest();
					Assert(!testForm.Visible);
				}
			}
			finally
			{
				DataRegistry.Instance.EnableExternalValidationService = rawRegistryValue;
			}
		}

		public void TestOrganisationDelete_SetsInactiveIfFKConstraint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;
			var shipment = Factory.New<IForwardingShipment>();
			var job = Factory.New<JobDocAddress>();
			var orgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, org.PK));
			job.E2_OA_Address = orgAddress.PK;
			job.E2_AddressType = AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			job.E2_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.E2_ParentID = shipment.PK;

			Factory.Save();

			AssertEquals(true, org.OH_IsActive);

			using (var testForm = new OrgFormForTest(org))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"This record is in use by other records in the system. Would you like to mark this record as Inactive?"
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				testForm.AcceptButton.PerformClick();

				var messages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text).ToList();

				AssertEquals(
					"Messages should have been displayed to user",
					true,
					messages.Contains("This record is in use by other records in the system. Would you like to mark this record as Inactive?"));

				var reloadedForm = new ZFormUtilitiesTest().GetFormCreatedByReloading(testForm) as BaseOrganisationsForm;
				var orgInReloadedForm = (OrgHeader)reloadedForm.BusinessEntity;
				orgInReloadedForm.Addresses[1].OA_PostCode = "1234";
				orgInReloadedForm.Addresses[1].OA_State = "NSW";
				orgInReloadedForm.Addresses[1].OA_City = "SYD";
				AssertEquals("Old form is disposed.", true, testForm.IsDisposed);
				AssertEquals("Object is deactivated.", true, orgInReloadedForm.IsCancelled);
				AssertEquals("DisplayMode is Edit.", ODisplayMode.Edit, reloadedForm.DisplayMode);
				reloadedForm.ButtonsUserControl.SaveAndCloseButton.PerformClick();
				reloadedForm.Dispose();
			}

			var orgInNewFactory = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			AssertEquals("Save successfully", false, orgInNewFactory.OH_IsActive);
		}

		public void TestFormClosedShown()
		{
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				form.Show();
				form.ViewEnabledTabsOnlyClick(null, EventArgs.Empty);
				form.Close();
			}
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				form.Show();
				Assert(form.enabledTabsOnly);
				form.ViewAllTabsClick(null, EventArgs.Empty);
				form.Close();
			}
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				form.Show();
				Assert(!form.enabledTabsOnly);
			}
		}

		public void TestAddMenuItems()
		{
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				Assert(form.ActionsMenuItem_Exposed.MenuItems.Contains(form.viewAllTabsMenuItem));
				Assert(form.ActionsMenuItem_Exposed.MenuItems.Contains(form.viewEnabledTabsOnlyMenuItem));
			}
		}

		public void TestChange_RecalculateCodeMenuItemAccessible()
		{
			var originalValue = Env.Security.OrganisationCanRecalculateCodesOnDemand.IsAllowed;
			try
			{
				UnitTestUserNotification.Instance.ClearMessages();
				var testHeader = Factory.NewWithValidTestData<OrgHeader>();
				testHeader.OH_FullName = "DEMO TEST COMPANY PTY LTD";
				testHeader.OH_RL_NKClosestPort = "CO8SG";
				testHeader.OH_Code = "DEMTESSYD";
				var oca = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
				oca.AllowRecalculatedOrgCodeByUser = true;
				using (OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oca))
				using (var testForm = new ZOrganisationsForm(testHeader))
				{
					Env.Security.OrganisationCanRecalculateCodesOnDemand.IsAllowed = true;
					testForm.Show();
					testForm.recalculateCodeMenuItem.PerformClick();
					Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

					Env.Security.OrganisationCanRecalculateCodesOnDemand.IsAllowed = false;
					testForm.recalculateCodeMenuItem.PerformClick();
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> Edit -> Main Details -> Can Recalculate Organization Codes On Demand", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.OrganisationCanRecalculateCodesOnDemand.IsAllowed = originalValue;
			}
		}

		public void TestChange_RecalculateCodeMenuItem_Visibility()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var oca = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;

			oca.AllowRecalculatedOrgCodeByUser = false;
			using (OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oca))
			using (var testForm = new ZOrganisationsForm(testHeader))
			{
				testForm.Show();
				Assert("Recalculate Code MenuItem is not visiable.", !testForm.recalculateCodeMenuItem.Visible);
			}

			oca.AllowRecalculatedOrgCodeByUser = true;
			using (OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oca))
			using (var testForm = new ZOrganisationsForm(testHeader))
			{
				testForm.Show();
				Assert("Recalculate Code MenuItem is visiable.", testForm.recalculateCodeMenuItem.Visible);
			}
		}

		public void TestAdd_FormatAllcontactPhoneNumbers_MenuItem()
		{
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText("Format All Contact Phone Numbers");
				Assert("This menu item should be found", item != null);
			}
		}

		public void TestViewAllTabsClicked_SetsEnabledTabsOnlyRegistryFalse()
		{
			using (OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				AssertEquals("Precondition:", true, OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).Value);
				form.ViewAllTabsClick(null, EventArgs.Empty);

				AssertEquals("Clicking View All Tabs should update the registry", false, OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).Value);
			}
		}

		public void TestAdd_UpdateRelatedJobScreeningStatus_MenuItem()
		{
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText("Update Related Job Screening Status");
				Assert("This menu item should be found", item != null);
			}
		}

		public void TestViewEnabledTabsOnlyClicked_SetsEnabledTabsOnlyRegistryTrue()
		{
			using (OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				AssertEquals("Precondition:", false, OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).Value);
				form.ViewEnabledTabsOnlyClick(null, EventArgs.Empty);

				AssertEquals("Clicking View Enabled Tabs Only should update the registry", true, OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).Value);
			}
		}

		public void TestViewAllTabsClick()
		{
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				form.Org.OH_IsConsignee = false;
				form.Org.OH_IsSalesLead = false;
				form.ViewEnabledTabsOnlyClick(null, EventArgs.Empty);
				form.ViewAllTabsClick(null, EventArgs.Empty);
				AssertTabNames(form.OrgTabControl.TabPages);
			}
		}

		[RequiresSTA]
		public void TestViewEnabledTabsOnlyClick()
		{
			using (OrgFormForTest form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				form.Org.OH_IsConsignee = true;
				form.Org.OH_IsDebtor = true;
				form.Org.OH_IsCreditor = true;
				form.Org.OH_IsConsignee = true;
				form.Org.OH_IsConsignor = true;
				form.Org.OH_IsWarehouseClient = true;
				form.Org.OH_IsShippingProvider = true;
				form.Org.OH_IsForwarder = true;
				form.Org.OH_IsSalesLead = true;
				form.Org.OH_IsCompetitor = true;
				form.Org.OH_IsMiscFreightServices = true;

				form.ViewEnabledTabsOnlyClick(null, EventArgs.Empty);

				Assert(form.OrgTabControl.TabPages.Contains(form.ReceivablesTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.PayablesTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.ConsigneeTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.ConsignorTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.WhsFacilityTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.TransportTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.ForwarderTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.SalesTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.CompetitorTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.MiscServicesTabPage));

				form.Org.OH_IsConsignee = false;

				Assert(form.OrgTabControl.TabPages.Contains(form.ReceivablesTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.PayablesTabPage));
				Assert(!form.OrgTabControl.TabPages.Contains(form.ConsigneeTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.ConsignorTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.WhsFacilityTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.TransportTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.ForwarderTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.SalesTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.CompetitorTabPage));
				Assert(form.OrgTabControl.TabPages.Contains(form.MiscServicesTabPage));

				form.Org.OH_IsConsignee = true;
				AssertTabNames(form.OrgTabControl.TabPages);
			}
		}

		public void TestNavigateToWorkflowItem()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_IsSalesLead = true;
			OrgOpportunity opp = testHeader.SalesOpportunities.AddNew();

			OrgOpportunity opp2 = testHeader.SalesOpportunities.AddNew();
			opp2.WorkflowItems.AddNew();
			opp2.WorkflowItems.AddNew();
			ProcessTask task5 = opp2.WorkflowItems.AddNew();

			ProcessTask otherTask = Factory.New<ProcessTask>();

			using (OrgFormForTest form = new OrgFormForTest(testHeader))
			{
				form.Show();
				((IWorkflowTaskNavigationOverridable)form).NavigateToWorkflowItem(task5);

				AssertEquals("Sales", form.OrgTabControl.SelectedTab.Text);
				SalesUserControl salesControl = null;
				foreach (Control ctrl in form.OrgTabControl.SelectedTab.Controls)
				{
					salesControl = ctrl as SalesUserControl;
					if (salesControl != null)
					{
						break;
					}
				}

				AssertEquals("Opportunity Management", salesControl.SalesTabControl.SelectedTab.Text);
				OpportunityManagementControl oppControl = null;
				foreach (Control ctrl in salesControl.SalesTabControl.SelectedTab.Controls)
				{
					oppControl = ctrl as OpportunityManagementControl;
					if (oppControl != null)
					{
						break;
					}
				}

				AssertEquals(1, oppControl.OpportunitiesGrid.InnerGrid.CurrentRowIndex);
				AssertEquals(1, oppControl.OpportunitiesGrid.InnerGrid.ListManager.Position);
				AssertEquals(2, oppControl.OppTasksControl.TasksGrid.CurrentRowIndex);
				AssertEquals(2, oppControl.OppTasksControl.TasksGrid.ListManager.Position);

				((IWorkflowTaskNavigationOverridable)form).NavigateToWorkflowItem(otherTask);

				AssertEquals(1, oppControl.OpportunitiesGrid.InnerGrid.CurrentRowIndex);
				AssertEquals(1, oppControl.OpportunitiesGrid.InnerGrid.ListManager.Position);
				AssertEquals(2, oppControl.OppTasksControl.TasksGrid.CurrentRowIndex);
				AssertEquals(2, oppControl.OppTasksControl.TasksGrid.ListManager.Position);
			}
		}

		public void TestNoValidationErrorsOnOpportunityForNonSalesOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_IsSalesLead = false;

			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_OpportunityType = "TST";
			opportunity.P8_OpportunityDescription = "Test Opp";
			opportunity.P8_Stage = "UDF";
			opportunity.P8_Status = "CRT";
			opportunity.WorkflowItems.AddNew();

			Factory.Save();

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				form.WorkflowTabPage.TabVisible = true;
				form.WorkflowTabPage.ClearNotificationImage();
				org.OH_IsTransportClient = true;

				form.ValidateAndSave();
				Assert("Should not have errors for deleted OpportunityType when Org is not Sales", !org.NotificationsIncludingChildren.Any(x => x.Message == "Error - P8_OpportunityType: Enter a valid Opportunity Type."));

				org.OH_IsSalesLead = true;
				form.ValidateAndSave();
				Assert("Should have errors for deleted OpportunityType", org.NotificationsIncludingChildren.Any(x => x.Message == "Error - P8_OpportunityType: Enter a valid Opportunity Type."));

				org.OH_IsSalesLead = false;
				form.ValidateAndSave();
				Assert("Should not have errors for deleted OpportunityType when Org is not Sales", !org.NotificationsIncludingChildren.Any(x => x.Message == "Error - P8_OpportunityType: Enter a valid Opportunity Type."));
			}
		}

		[RequiresSTA]
		public void TestConstructWithTabPage()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			OrgFormForTest form;
			string[] expectedTabPageColumnHeadings;

			using (form = new OrgFormForTest(testHeader)) // Initialise Values and test that the number of tab pages matches the enum
			{
				expectedTabPageColumnHeadings = new string[] {
					form.DetailsTabPage_Exposed.Text,
					form.AddressesTabPage_Exposed.Text,
					form.ContactsTabPage_Exposed.Text,
					form.SalesTabPage.Text,
					"SalesTabPage+SalesClientRelTabPage",
					"DetailsTabPage+ConfigTabPage+EDICodeMappingTabPage",
					OrganisationTabPages.ServiceLevel.Name,
				};
			}

			Type orgTabPagesType = typeof(OrganisationTabPages);
			var members = orgTabPagesType.GetMembers(BindingFlags.Static | BindingFlags.Public).Where(x => x.MemberType == MemberTypes.Property);

			AssertEquals("If this fails, then you need to add your new OrganisationsTabPage to this list of Column headings", expectedTabPageColumnHeadings.Length, members.Count());
		}

		public void AssertTabNames(ZTabControl.TabPageCollection tabPages)
		{
			AssertEquals("OrganisationTabControl.TabPages[0]", "DetailsTabPage", tabPages[0].Name);
			AssertEquals("OrganisationTabControl.TabPages[1]", "AddressesTabPage", tabPages[1].Name);
			AssertEquals("OrganisationTabControl.TabPages[2]", "ContactsTabPage", tabPages[2].Name);
			AssertEquals("OrganisationTabControl.TabPages[3]", "ReceivablesTabPage", tabPages[3].Name);
			AssertEquals("OrganisationTabControl.TabPages[4]", "PayablesTabPage", tabPages[4].Name);
			AssertEquals("OrganisationTabControl.TabPages[5]", "ConsignorTabPage", tabPages[5].Name);
			AssertEquals("OrganisationTabControl.TabPages[6]", "ConsigneeTabPage", tabPages[6].Name);
			AssertEquals("OrganisationTabControl.TabPages[7]", "WhsFacilityTabPage", tabPages[7].Name);
			AssertEquals("OrganisationTabControl.TabPages[8]", "ForwarderTabPage", tabPages[8].Name);
			AssertEquals("OrganisationTabControl.TabPages[9]", "TransportTabPage", tabPages[9].Name);
			AssertEquals("OrganisationTabControl.TabPages[10]", "MiscServicesTabPage", tabPages[10].Name);
			AssertEquals("OrganisationTabControl.TabPages[11]", "SalesTabPage", tabPages[11].Name);
			AssertEquals("OrganisationTabControl.TabPages[12]", "CompetitorTabPage", tabPages[12].Name);
			AssertEquals("OrganisationTabControl.TabPages[13]", "WorkflowTabPage", tabPages[13].Name);
			AssertEquals("OrganisationTabControl.TabPages[14]", "UserDefinedTabPage", tabPages[14].Name);
		}

		public void TestOrderOfTabPages()
		{
			OrgFormForTest form;
			using (form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				form.Show();
				form.ViewAllTabsClick(null, EventArgs.Empty);
				AssertTabNames(form.OrgTabControl.TabPages);
			}
		}

		public void TestSetTabsReadonlynessBasedOnIsGlobalSupplierSecurity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsGlobalAccount = true;
			Factory.Save();

			Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = true;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();

				// Check some random controls to see if they have the proper reado-only value
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "DetailsTabPage", "DetailsControl", "OH_IsCreditorBoundCheckEdit", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "DetailsTabPage", "DetailsControl", "OH_IsDebtorBoundCheckEdit", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "DetailsTabPage", "NameAndAddressDetailsControl", "Address1BoundTextBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "AddressesTabPage", "AddressesPageControl2", "OA_CompanyNameOverrideBoundTextBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "ReceivablesTabPage", "ReceivablesPageControl", "ExternalDebtorCodeTextBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "PayablesTabPage", "PayablesControl", "ExternalCreditorCodeTextBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "ConsignorTabPage", "DetailsControl", "PreAllocationTextBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "ConsigneeTabPage", "ConsigneeControl", "OM_IMLastOrderReferenceTextBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "WhsFacilityTabPage", "WhsFacilityTabControl", "ProductWarehouseTabPage", "whsDetailsUserControl1", "EXDefaultDGContactGuidFindBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "ForwarderTabPage", "forwarderDetailsUserControl1", "OM_FWIATACodeBoundTextBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "TransportTabPage", "TransportPageControl", "OM_CRCarrierCategoryBoundDropEdit", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "MiscServicesTabPage", "MiscServicesControl", "OH_IsPackDepotBoundCheckBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "SalesTabPage", "salesProspectControl1", "OM_CMEstablishedDateDateEdit", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "CompetitorTabPage", "competitorDetailsUserControl1", "OM_CIStrengthBoundTextBox", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "WorkflowTabPage", "TasksControl", "TasksGrid", true, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "eDocsTabPage", "eDocsUserControl", "ShowDeletedDocumentsCheckBox", true, false);
			}

			Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = false;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();

				// Check some random controls to see if they have the proper reado-only value
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "DetailsTabPage", "DetailsControl", "OH_IsCreditorBoundCheckEdit", false, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "DetailsTabPage", "DetailsControl", "OH_IsDebtorBoundCheckEdit", false, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "DetailsTabPage", "NameAndAddressDetailsControl", "Address1BoundTextBox", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "AddressesTabPage", "AddressesPageControl2", "OA_CompanyNameOverrideBoundTextBox", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "ReceivablesTabPage", "ReceivablesPageControl", "ExternalDebtorCodeTextBox", false, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "PayablesTabPage", "PayablesControl", "ExternalCreditorCodeTextBox", false, false);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "ConsignorTabPage", "DetailsControl", "PreAllocationTextBox", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "ConsigneeTabPage", "ConsigneeControl", "OM_IMLastOrderReferenceTextBox", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "WhsFacilityTabPage", "WhsFacilityTabControl", "ProductWarehouseTabPage", "whsDetailsUserControl1", "EXDefaultDGContactGuidFindBox", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "ForwarderTabPage", "forwarderDetailsUserControl1", "OM_FWIATACodeBoundTextBox", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "TransportTabPage", "TransportPageControl", "OM_CRCarrierCategoryBoundDropEdit", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "MiscServicesTabPage", "MiscServicesControl", "OH_IsPackDepotBoundCheckBox", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "SalesTabPage", "salesProspectControl1", "OM_CMEstablishedDateDateEdit", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "CompetitorTabPage", "competitorDetailsUserControl1", "OM_CIStrengthBoundTextBox", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "WorkflowTabPage", "TasksControl", "TasksGrid", false, true);
				AssertReadonlynessBasedOnIsGlobalSupplierSecurity(form, "eDocsTabPage", "eDocsUserControl", "ShowDeletedDocumentsCheckBox", false, true);
			}
		}

		void AssertReadonlynessBasedOnIsGlobalSupplierSecurity(OrgFormForTest form, string tabName, string containingControlName, string controlToCheckName, bool hasRight, bool shouldBeReadonly)
		{
			var tab = form.OrganisationsTabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == tabName);
			form.OrganisationsTabControl.SelectedTab = tab;
			var containingControl = tab.Controls.Find(containingControlName, true)[0];
			var controlToCheck = containingControl.Controls.Find(controlToCheckName, true)[0];
			AssertEquals("I " + (hasRight ? string.Empty : "DON'T ") + "have OrgGlobalSupplierDetailsModify right, so control " + controlToCheckName + " should " + (shouldBeReadonly ? string.Empty : "NOT ") + "be read-only", shouldBeReadonly, controlToCheck.GetReadOnly());
		}

		void AssertReadonlynessBasedOnIsGlobalSupplierSecurity(OrgFormForTest form, string tabName, string subTabControlName, string subTabName,string containingControlName, string controlToCheckName, bool hasRight, bool shouldBeReadonly)
		{
			var tab = form.OrganisationsTabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == tabName);
			form.OrganisationsTabControl.SelectedTab = tab;
			var subTabControl = tab.Controls.Find(subTabControlName, true)[0] as ZTemplateTabControl;
			subTabControl.SelectedTab = subTabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == subTabName);
			var containingControl = subTabControl.SelectedTab.Controls.Find(containingControlName, true)[0];
			var controlToCheck = containingControl.Controls.Find(controlToCheckName, true)[0];
			AssertEquals("I " + (hasRight ? string.Empty : "DON'T ") + "have OrgGlobalSupplierDetailsModify right, so control " + controlToCheckName + " should " + (shouldBeReadonly ? string.Empty : "NOT ") + "be read-only", shouldBeReadonly, controlToCheck.GetReadOnly());
		}

		public void TestCarrierReadOnlyInNewForm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Env.Security.OrgDetailsNewOrganisationType.IsAllowed = false;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				Application.DoEvents();
				var isShippingProviderBoundCheckEdit = FindCheckBox(form.DetailsControl, "OH_IsShippingProviderBoundCheckEdit");
				AssertEquals(true, isShippingProviderBoundCheckEdit.GetReadOnly());
			}

			Env.Security.OrgDetailsNewOrganisationType.IsAllowed = true;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				Application.DoEvents();
				var isShippingProviderBoundCheckEdit = FindCheckBox(form.DetailsControl, "OH_IsShippingProviderBoundCheckEdit");
				AssertEquals(false, isShippingProviderBoundCheckEdit.GetReadOnly());
			}
		}

		[ExpectNoExceptions]
		public void TestMainAddressIsMaintainedAfterDeleteFails()
		{
			var uNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var company1 = Factory.NewWithValidTestData<OrgHeader>();
			company1.OH_FullName = "Company1";
			company1.OH_RL_NKClosestPort = uNLOCO.Code;
			company1.MainAddress.OA_Address1 = "Test Address";

			var company2 = Factory.NewWithValidTestData<OrgHeader>();
			company2.OH_FullName = "Company2";
			company2.OH_RL_NKClosestPort = uNLOCO.Code;
			company2.MainAddress.OA_Address1 = "Address for Company2";
			company2.SetRelatedParty(company1, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);

			Factory.Save();

			using (var form = new OrgFormForTest(company1))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"This record is in use by other records in the system. Would you like to mark this record as Inactive?"
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				form.AcceptButton.PerformClick();

				var reloadedForm = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
				var orgInReloadedForm = (OrgHeader)reloadedForm.BusinessEntity;
				AssertEquals("Old form is disposed.", true, form.IsDisposed);
				AssertEquals("Object is deactivated.", true, orgInReloadedForm.IsCancelled);
				AssertEquals("DisplayMode is Edit.", ODisplayMode.Edit, reloadedForm.DisplayMode);
				reloadedForm.Dispose();
			}

			AssertEquals("Organisation's main address should not be changed after the deletion failed", "Test Address", company1.MainAddress.OA_Address1);
			AssertEquals("Organisation should still have one address after the deletion failed", 1, company1.Addresses.Count);
		}

		[ExpectNoExceptions]
		public void TestDeleteOrgWithTradeLanes()
		{
			RefUNLOCO uNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			OrgHeader company1 = Factory.NewWithValidTestData<OrgHeader>();
			company1.OH_FullName = "Company1";
			company1.OH_RL_NKClosestPort = uNLOCO.Code;
			company1.MainAddress.OA_Address1 = "Test Address";
			company1.OH_IsSalesLead = true;

			OrgSales sales = Factory.NewWithValidTestData<OrgSales>();
			company1.SalesCollection.Add(sales);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader company2 = newFactory.Load<OrgHeader>(company1.PK);
			AssertNotNull("Company was saved to the database", company2);
			using (OrgFormForTest form = new OrgFormForTest(company2))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				form.OrgTabControl.SelectedIndex = 11;
				form.AcceptButton.PerformClick();
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteOrgWithoutMainAddress()
		{
			OrgHeader company1 = Factory.New<OrgHeader>();
			company1.OH_IsActive = false;
			company1.OH_FullName = "Company1";
			company1.OH_RL_NKClosestPort = (Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD")).Code;
			company1.Addresses.RemoveAndDeleteAll();

			Factory.Save();

			using (OrgFormForTest form = new OrgFormForTest(company1))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				form.AcceptButton.PerformClick();
			}
		}

		public void TestShowMessage()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCXYZ";
			org.MainAddress.OA_Address1 = "Address 1";
			Factory.Save();

			OrgFormForTest form;
			using (form = new OrgFormForTest(org))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				org.CompanyData.OB_IsDebtor = true;
				org.CompanyData.SetARTaxApplicable(true);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestCorrectControlsOnTabPagesVisible()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			using (OrgFormForTest form = new OrgFormForTest(org))
			{
				form.Show();
				form.ViewAllTabsClick(null, EventArgs.Empty);
				ExposeAllTabPages(form);
				AssertControlVisibilityForOrgType(org.OH_IsDebtorInfo, form.OrgTabControl, 3, form.ReceivablesTabPage.Controls["ReceivablesControl"], form.ReceivablesTabPage.Controls["ReceivablesNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsCreditorInfo, form.OrgTabControl, 4, form.PayablesTabPage.Controls["PayablesControl"], form.PayablesTabPage.Controls["PayablesNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsConsignorInfo, form.OrgTabControl, 5, form.ConsignorTabPage.Controls["ConsignorControl"], form.ConsignorTabPage.Controls["ConsignorNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsConsigneeInfo, form.OrgTabControl, 6, form.ConsigneeTabPage.Controls["ConsigneeControl"], form.ConsigneeTabPage.Controls["ConsigneeNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsWarehouseClientInfo, form.OrgTabControl, 7, form.WhsFacilityTabPage.Controls["WarehouseUserControl"], form.WhsFacilityTabPage.Controls["WarehouseNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsForwarderInfo, form.OrgTabControl, 8, form.ForwarderTabPage.Controls["ForwarderControl"], form.ForwarderTabPage.Controls["ForwarderNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsShippingProviderInfo, form.OrgTabControl, 9, form.TransportTabPage.Controls["TransportControl"], form.TransportTabPage.Controls["TransportNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsMiscFreightServicesInfo, form.OrgTabControl, 10, form.MiscServicesTabPage.Controls["MiscServicesControl"], form.MiscServicesTabPage.Controls["MiscServicesNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsSalesLeadInfo, form.OrgTabControl, 11, form.SalesTabPage.Controls["SalesControl"], form.SalesTabPage.Controls["SalesNotSelectedLabel"]);
				AssertControlVisibilityForOrgType(org.OH_IsCompetitorInfo, form.OrgTabControl, 12, form.CompetitorTabPage.Controls["CompetitorControl"], form.CompetitorTabPage.Controls["CompetitorNotSelectedLabel"]);
			}
		}

		public void TestExporter()
		{
			var org = Factory.New<OrgHeader>();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			using (var form = new ZOrganisationsForm(org))
			{
				var exporter = form.Exporter;
				AssertNotNull(exporter);
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestOnExportToXml()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var org = Factory.New<OrgHeader>();

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText(ExportXmlMenuItemHelper.VerboseMenuItemText);
				item.PerformClick();
				AssertEquals("Export has been done", true, ((TestXmlDataTransferExporter)form.DataTransferExporter).ExportDone);
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText(ExportXmlMenuItemHelper.LightWeightMenuItemText);
				item.PerformClick();
				AssertEquals("Export has been done", true, ((TestXmlDataTransferExporter)form.DataTransferExporter).ExportDone);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestOnImportEDICodeMapping()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			using (OrgFormForTest form = new OrgFormForTest(org))
			{
				form.Show();
				MenuItem item = form.ActionsMenuItem_Exposed.MenuItems.FindByText("Import EDI Code Mapping from CSV File");
				item.PerformClick();
				AssertEquals("Import has been done", true, form.OnImportEDICodeMappingClicked);
			}
		}

		public void TestUpdateRelatedJobScreeningStatus()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader[OrgHeaderSchema.OH_Code] = "NEWCOD";

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (OrgFormForTest form = new OrgFormForTest(orgHeader))
			{
				var job = (BusinessObject)Factory.New<IForwardingShipment>();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress[OrgAddressSchema.OA_OH] = orgHeader.PK;

				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader[JobHeaderSchema.JH_OA_LocalChargesAddr] = orgAddress.PK;
				jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;

				Factory.Save();

				var initialLoadFactory = new BusinessObjectFactory();
				var initialJob = (BusinessObject)initialLoadFactory.Load<IForwardingShipment>(job.PK);
				AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, initialJob[JobShipmentSchema.JS_ScreeningStatus].ToString());

				var forPartyFactory = new BusinessObjectFactory();
				var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
				partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				forPartyFactory.Save();

				form.Show();
				var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText("Update Related Job Screening Status");
				item.PerformClick();

				var loadFactory = new BusinessObjectFactory();
				var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
				AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
			}
		}

		[RequiresSTA]
		public void TestUpdateRelatedJobScreeningStatusLogCreated()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader[OrgHeaderSchema.OH_Code] = "NEWCOB";

			using (OrgFormForTest form = new OrgFormForTest(orgHeader))
			{
				var job = (BusinessObject)Factory.New<IForwardingShipment>();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress[OrgAddressSchema.OA_OH] = orgHeader.PK;

				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader[JobHeaderSchema.JH_OA_LocalChargesAddr] = orgAddress.PK;
				jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;

				Factory.Save();

				var initialLoadFactory = new BusinessObjectFactory();
				var initialJob = (BusinessObject)initialLoadFactory.Load<IForwardingShipment>(job.PK);

				var forPartyFactory = new BusinessObjectFactory();
				var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
				partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				forPartyFactory.Save();

				var query = new ZQuery(StmEntityScreeningLogSchema.PJ_Status, DeniedPartyConstants.LogsScreeningStatus.UpdateRelatedJobs);
				var logsBefore = Factory.Load<StmEntityScreeningLog>(query);

				form.Show();
				var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText("Update Related Job Screening Status");
				item.PerformClick();

				var logsAfter = Factory.Load<StmEntityScreeningLog>(query);

				CombineAssertions(() =>
				{
					AssertEquals("There should be 1 new log added because of the update", logsBefore.Length + 1, logsAfter.Length);
					AssertEquals("ParentTableCode should be OH", orgHeader.TableCode, logsAfter[0].PJ_ParentTableCode);
					AssertEquals("PJ_status of the new log should be the constant UpdateRelatedJobs", DeniedPartyConstants.LogsScreeningStatus.UpdateRelatedJobs, logsAfter[0].PJ_Status);
				});
			}
		}

		public void TestNotAllowedUpdateJobs()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();

			var rawValue = Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.IsAllowed;
			try
			{
				Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (ZOrganisationsForm dummyForm = new ZOrganisationsForm(orgHeader))
				{
					dummyForm.Show();
					var actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
					var item = actionsMenuItem.MenuItems.FindByText("Update Related Job Screening Status");
					item.PerformClick();
					AssertEquals(Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.IsAllowed = rawValue;
			}
		}

		public void TestAllowUpdateJobs()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader[OrgHeaderSchema.OH_Code] = "NEWCOD";
			orgHeader[OrgHeaderSchema.OH_FullName] = "New Org";
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			Factory.Save();

			var rawValue = Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.IsAllowed;
			try
			{
				Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (ZOrganisationsForm dummyForm = new ZOrganisationsForm(orgHeader))
				{
					dummyForm.Show();
					var actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
					var item = actionsMenuItem.MenuItems.FindByText("Update Related Job Screening Status");
					item.PerformClick();
					foreach (UnitTestUserNotification.PreviousMessage message in UnitTestUserNotification.Instance.PreviousMessages)
					{
						AssertNotEquals(Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.ErrorMessageForNotAllowed, message);
					}
				}
			}
			finally
			{
				Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.IsAllowed = rawValue;
			}
		}

		[RequiresSTA]
		public void TestLabelCSTermin()
		{
			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			AssertText(FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestTestTest");
			AssertText("TestTestTest");
		}

		[RequiresSTA]
		public void TestTabPageNotificationIcons_ForPayablesAndReceivables()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			using (OrgFormForTest form = new OrgFormForTest(org))
			{
				form.Show();
				Application.DoEvents();

				FindCheckBox(form.DetailsControl, "OH_IsDebtorBoundCheckEdit").Checked = true;
				form.ValidateAndSave();
				UserIdleWorker.Flush();
				AssertEquals("Entering no data into the A/R tab and saving results in a tab page error icon", Icons.GetImageIndex(IconTypes.Error), form.OrganisationsTabControl.TabPages["ReceivablesTabPage"].ImageIndex);

				FindCheckBox(form.DetailsControl, "OH_IsCreditorBoundCheckEdit").Checked = true;
				form.ValidateAndSave(); // it's possible the first save could prevent the 2nd save from showing the 2nd error icon
				UserIdleWorker.Flush();
				AssertEquals("Entering no data into the A/P tab and saving results in a tab page error icon", Icons.GetImageIndex(IconTypes.Error), form.OrganisationsTabControl.TabPages["PayablesTabPage"].ImageIndex);
			}
		}

		public void TestShouldAlwaysHandleValidationForRegistryWhenSave()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Email = string.Empty;
			org.OH_IsConsignee = true;
			org.MainAddress.MarkLightValidationAsValidForTesting();
			var requiredFieldsEmailOnly = new OrgRequiredFields(false, false, false, false, false, false, false, true, false, false, false);
			Env.Registry.SetOrgConsigneeRequiredFields(requiredFieldsEmailOnly);

			using (var form = new OrgFormForTest(org))
			{
				form.ValidateAndSave();
				AssertHasErrors("Email is required", form.Org.MainAddress.OA_EmailInfo);
			}
		}

		public void TestModifyRatingAndTariffsSecurityOnDistanceCalculationProvider()
		{
			bool oldOrgDetailsModify = Env.Security.OrgDetailsModify.IsAllowed;
			bool oldOrgDetailsModifyRatingAndTariffs = Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed;
			try
			{
				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_IsDebtor = true;
				org.OH_Code = "ABCXYZ";
				org.MainAddress.OA_Address1 = "Address 1";
				Factory.Save();

				Env.Security.OrgDetailsModify.IsAllowed = false;
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;

				using (OrgFormForTest form = new OrgFormForTest(org))
				{
					form.Show();
					Application.DoEvents();
					form.DetailsControl.DetailsTabControl.SelectedIndex = 3;
					Application.DoEvents();
					var control = FindControl(form, "DistanceCalcProviderDropEdit");
					Assert(control.GetReadOnly());
				}

				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
				using (OrgFormForTest form = new OrgFormForTest(org))
				{
					form.Show();
					Application.DoEvents();
					form.DetailsControl.DetailsTabControl.SelectedIndex = 3;
					Application.DoEvents();
					var control = FindControl(form, "DistanceCalcProviderDropEdit");
					Assert(!control.GetReadOnly());
				}
			}
			finally
			{
				Env.Security.OrgDetailsModify.IsAllowed = oldOrgDetailsModify;
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = oldOrgDetailsModifyRatingAndTariffs;
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteMainAddress_DoesNotOverflow()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			var mainAddress = org.Addresses[0];
			AssertNotNull(mainAddress);
			Assert(mainAddress.IsMainAddress);

			using (var form = new ZOrganisationsForm(org))
			{
				form.Show();
				form.OrganisationsTabControl.SelectedTab = form.AddressesTabPage;
				var grid = form.AddressesPageControl2.OrgAddressBoundGrid;
				typeof(ZGrid).GetMethod("HandleDelete", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(grid, new object[] { 0 });
			}
		}

		public void TestSalesAndCompetitorTabPagesSecurityView()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.OrgSalesView.IsAllowed = true;
			Env.Security.OrgCompetitorView.IsAllowed = true;

			using (var form = new ZOrganisationsForm(org))
			{
				form.Show();

				org.OH_IsSalesLead = false;
				org.OH_IsCompetitor = false;

				form.OrganisationsTabControl.SelectedTab = form.SalesTabPage;
				SalesUserControl salesControl = null;
				foreach (Control ctrl in form.SalesTabPage.Controls)
				{
					salesControl = ctrl as SalesUserControl;
					if (salesControl != null)
					{
						break;
					}
				}
				foreach (ZTabPage tabPage in salesControl.SalesTabControl.TabPages)
				{
					AssertNull("Licence checkpoint should not be attached if org is not flagged as sales lead", tabPage.LicenceCheckpoint);
				}

				form.OrganisationsTabControl.SelectedTab = form.CompetitorTabPage;
				CompetitorUserControl competitorControl = null;
				foreach (Control ctrl in form.CompetitorTabPage.Controls)
				{
					competitorControl = ctrl as CompetitorUserControl;
					if (competitorControl != null)
					{
						break;
					}
				}
				foreach (ZTabPage tabPage in competitorControl.CompetitorTabControl.TabPages)
				{
					AssertNull("Licence checkpoint should not be attached if org is not flagged as competitor", tabPage.LicenceCheckpoint);
				}

				org.OH_IsSalesLead = true;
				org.OH_IsCompetitor = true;
				foreach (ZTabPage tabPage in salesControl.SalesTabControl.TabPages)
				{
					AssertNotNull("Licence checkpoint should be attached if org is flagged as sales lead", tabPage.LicenceCheckpoint);
				}
				foreach (ZTabPage tabPage in competitorControl.CompetitorTabControl.TabPages)
				{
					AssertNotNull("Licence checkpoint should be assigned if org is flagged as competitor", tabPage.LicenceCheckpoint);
				}
			}

			Env.Security.OrgSalesView.IsAllowed = false;
			Env.Security.OrgCompetitorView.IsAllowed = false;
			using (var form = new ZOrganisationsForm(org))
			{
				form.Show();
				form.OrganisationsTabControl.SelectedTab = form.SalesTabPage;
				AssertEquals("coveringLabel", form.SalesTabPage.Controls[0].Name);
				form.OrganisationsTabControl.SelectedTab = form.CompetitorTabPage;
				AssertEquals("coveringLabel", form.CompetitorTabPage.Controls[0].Name);
			}
		}

		public void TestReceivablesAndPayablesTabPageTabPagesSecurityView()
		{
			var rawOrgReceivablesView = Env.Security.OrgReceivablesView.IsAllowed;
			var rawOrgPayablesView = Env.Security.OrgPayablesView.IsAllowed;

			try
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();

				Env.Security.OrgReceivablesView.IsAllowed = true;
				using (var form = new ZOrganisationsForm(org))
				{
					form.Show();
					form.OrganisationsTabControl.SelectedTab = form.ReceivablesTabPage;
					AssertNotEquals("coveringLabel", form.ReceivablesTabPage.Controls[0].Name);
				}

				Env.Security.OrgReceivablesView.IsAllowed = false;
				using (var form = new ZOrganisationsForm(org))
				{
					form.Show();
					form.OrganisationsTabControl.SelectedTab = form.ReceivablesTabPage;
					AssertEquals("coveringLabel", form.ReceivablesTabPage.Controls[0].Name);
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> View Receivables
", form.ReceivablesTabPage.Controls[0].Text);
				}

				Env.Security.OrgPayablesView.IsAllowed = true;
				using (var form = new ZOrganisationsForm(org))
				{
					form.Show();
					form.OrganisationsTabControl.SelectedTab = form.PayablesTabPage;
					AssertNotEquals("coveringLabel", form.PayablesTabPage.Controls[0].Name);
				}

				Env.Security.OrgPayablesView.IsAllowed = false;
				using (var form = new ZOrganisationsForm(org))
				{
					form.Show();
					form.OrganisationsTabControl.SelectedTab = form.PayablesTabPage;
					AssertEquals("coveringLabel", form.PayablesTabPage.Controls[0].Name);
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> View Payables
", form.PayablesTabPage.Controls[0].Text);
				}
			}
			finally
			{
				Env.Security.OrgReceivablesView.IsAllowed = rawOrgReceivablesView;
				Env.Security.OrgPayablesView.IsAllowed = rawOrgPayablesView;
			}
		}

		#region Activation/Deactivation

		public void TestDeactivate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "XYZ";
			org.OH_IsGlobalAccount = true;
			Factory.Save();
			using (var form = new OrgFormForTest(org))
			{
				form.Show();

				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = false;
				org.HasChanges = true;
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Deactivate").PerformClick();
				AssertEquals("You do not have rights to activate/deactivate organizations.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!org.Factory.IsValidationSuspended);
				Assert(!org.ReadOnly);
				Assert(form.ButtonsUserControl.SaveButton.Visible);

				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
				Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = false;
				org.HasChanges = true;
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Deactivate").PerformClick();
				AssertEquals("You do not have rights to modify organizations flagged as Global Supplier.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!org.Factory.IsValidationSuspended);
				Assert(!org.ReadOnly);
				Assert(form.ButtonsUserControl.SaveButton.Visible);

				Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = true;
				org.HasChanges = true;
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Deactivate").PerformClick();
				AssertEquals("Save or cancel changes before deactivating organization.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!org.Factory.IsValidationSuspended);
				Assert(!org.ReadOnly);
				Assert(form.ButtonsUserControl.SaveButton.Visible);

				org.OH_IsActive = false;
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Deactivate").PerformClick();
				AssertEquals("Organization is already inactive.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!org.Factory.IsValidationSuspended);
				Assert(!org.ReadOnly);
				Assert(form.ButtonsUserControl.SaveButton.Visible);

				org.OH_IsActive = true;
				org.HasChanges = false;
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Deactivate").PerformClick();
				Assert(org.Factory.IsValidationSuspended);
				Assert(org.ReadOnly);
				Assert(!form.ButtonsUserControl.SaveButton.Visible);
				Assert(form.ButtonsUserControl.SaveAndCloseButton.Visible);
				AssertEquals("Deactivate", form.ButtonsUserControl.SaveAndCloseButton.Text);

				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
				Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = true;
				org.OH_IsActive = true;
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_OH_OrgProxy = org.PK;
				Factory.Save();

				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Deactivate").PerformClick();
				Assert(org.OH_IsActive);
			}
		}

		public void TestActivate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "XYZ";
			org.OH_IsActive = true;
			org.OH_IsGlobalAccount = true;
			Factory.Save();

			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = false;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				Assert(!org.ReadOnly);
				Assert(!org.Factory.IsValidationSuspended);
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Activate").PerformClick();
				AssertEquals("You do not have rights to activate/deactivate organizations.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(form.ButtonsUserControl.SaveButton.Visible);
				Assert(!org.Factory.IsValidationSuspended);
			}

			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				Assert(!org.ReadOnly);
				Assert(!org.Factory.IsValidationSuspended);
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Activate").PerformClick();
				AssertEquals("Organization is already active.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(form.ButtonsUserControl.SaveButton.Visible);
				Assert(!org.Factory.IsValidationSuspended);
			}

			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
			Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = false;
			org.OH_IsActive = false;
			Factory.Save();
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				Assert(org.ReadOnly);
				Assert(org.Factory.IsValidationSuspended);
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Activate").PerformClick();
				AssertEquals("You do not have rights to modify organizations flagged as Global Supplier.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(form.ButtonsUserControl.SaveButton.Visible);
				Assert(org.Factory.IsValidationSuspended);
			}

			Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = true;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				Assert(org.ReadOnly);
				Assert(org.Factory.IsValidationSuspended);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Activate").PerformClick();
				Assert(form.IsDisposed);
				using (var newForm = form.formNewlyShown)
				{
					Assert(!newForm.IsDisposed);
					Assert(!newForm.ButtonsUserControl.SaveButton.Visible);
					Assert(newForm.ButtonsUserControl.SaveAndCloseButton.Visible);
					AssertEquals(ODisplayMode.Edit, newForm.DisplayMode);
					AssertEquals("Activate", newForm.ButtonsUserControl.SaveAndCloseButton.Text);
					Assert(!newForm.BusinessEntity.Factory.IsValidationSuspended);
				}
				Assert(form.formNewlyShown.IsDisposed);
			}
		}

		public void TestDeactivateOrgWithActiveTransactions()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsActive = true;
			organisation.OH_Code = "AAA";
			SetupTestData(organisation);
			Factory.Save();

			var expectedErrorMEssage = @"You cannot De-activate this organization as there are active AR and/or AP transactions in the following system companies.
	- Company Code: QAA: Accrual, Accounts Receivable, Incomplete, Unapproved
	- Company Code: QBB: Accounts Payable, Pending Allocation, WIP
	- Company Code: QCC: Accrual";

			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;

			using (var form = new OrgFormForTest(organisation))
			{
				form.Show();
				Assert(!organisation.ReadOnly);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ActionsMenuItem_Exposed.MenuItems.FindByText("Deactivate").PerformClick();
				AssertEquals(expectedErrorMEssage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should not be deactivated because it has an active transaction.", organisation.OH_IsActive);
			}
		}

		void SetupTestData(OrgHeader organisation)
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			companyA.GC_Code = "QAA";

			var companyBranchA = Factory.NewWithValidTestData<GlbBranch>();
			companyBranchA.GB_GC = companyA.PK;

			var companyB = Factory.NewWithValidTestData<GlbCompany>();
			companyB.GC_Code = "QBB";

			var companyBranchB = Factory.NewWithValidTestData<GlbBranch>();
			companyBranchB.GB_GC = companyB.PK;

			var companyC = Factory.NewWithValidTestData<GlbCompany>();
			companyC.GC_Code = "QCC";

			var companyBranchC = Factory.NewWithValidTestData<GlbBranch>();
			companyBranchC.GB_GC = companyC.PK;

			var activeACR1 = GetNewTransactionLine(organisation, TransactionLineTypes.Accrual, 20m, companyBranchA);
			var unpaidARInvoice = GetNewTransactionHeader(organisation, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 100m, companyBranchA);
			unpaidARInvoice.AH_InvoiceAmount = 100m;
			var unapprovedAPInvoice = GetNewTransactionHeader(organisation, LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice, 100m, companyBranchA);
			unapprovedAPInvoice.AH_InvoiceAmount = 100m;
			var incompleteAPInvoice = GetNewTransactionHeader(organisation, LedgerTypes.IncompleteTransactions, TransactionTypes.IncompleteInvoice, 100m, companyBranchA);
			incompleteAPInvoice.AH_InvoiceAmount = 100m;

			var activeWIP = GetNewTransactionLine(organisation, TransactionLineTypes.WIP, 20m, companyBranchB);
			var unallocatedAPInvoice = GetNewTransactionHeader(organisation, LedgerTypes.TransactionsPendingAllocation, TransactionTypes.InvoicePendingAllocation, 100m, companyBranchB);
			unallocatedAPInvoice.AH_InvoiceAmount = 100m;
			var unpaidAPInvoice = GetNewTransactionHeader(organisation, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 100m, companyBranchB);
			unpaidAPInvoice.AH_InvoiceAmount = 100m;

			var activeACR2 = GetNewTransactionLine(organisation, TransactionLineTypes.Accrual, 20m, companyBranchC);
			var paidARInvoice = GetNewTransactionHeader(organisation, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 0m, companyBranchC);
			paidARInvoice.AH_FullyPaidDate = ZDateTime.Today;
		}

		AccTransactionHeader GetNewTransactionHeader(OrgHeader org, string ledger, string transactionType, decimal outstandingAmount, GlbBranch branch)
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = ledger;
			invoice.AH_OutstandingAmount = outstandingAmount;
			invoice.AH_IsCancelled = false;
			invoice.AH_GB = branch.PK;
			invoice.AH_TransactionType = transactionType;
			return invoice;
		}

		AccTransactionLines GetNewTransactionLine(OrgHeader org, string lineType, decimal lineAmount, GlbBranch branch)
		{
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = lineType;
			line.AL_OH = org.PK;
			line.AL_GB = branch.PK;
			line.AL_LineAmount = lineAmount;
			return line;
		}

		#endregion

		[RequiresSTA]
		public void TestPreviousNextItem()
		{
			var dummyOrg = Factory.NewWithValidTestData<OrgHeader>();
			dummyOrg.OH_Code = "PNTABC";
			var dummyOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			dummyOrg1.OH_Code = "PNTDEF";
			dummyOrg1.OH_IsActive = false;
			Factory.Save();
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				var filter = module.FilterBusinessObject["Active Status"];
				((ModuleTextFilter)filter).Property = "All";
				((ModuleTextFilter)filter).IsActive = true;

				module.AddAdditionalDisplayFilter = AddModuleAdditionalFilter;
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				Application.DoEvents();

				AssertEquals("Loaded 2", 2, ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Organisation).Count);

				using (var form = ((IFilterGridModuleInternalsForTesting)module).ShowEditForm(dummyOrg))
				{
					Assert("Form is editable", form.DisplayMode != ODisplayMode.ReadOnly);
					AssertReadonlyness((ZOrganisationsForm)form, "DetailsTabPage", "DetailsControl", "OH_IsCreditorBoundCheckEdit", false);
					Application.DoEvents();
					((IPreviousNextControlProvider)form).PreviousNextControlForTesting.FireNextButtonForTesting();
					Application.DoEvents();
					using (var form1 = OpenedFormCache.GetInstance().GetForm(dummyOrg1.PK.ToGuid(), ModuleIDs.Organisation.ToString()))
					{
						Assert("Form is editable", ((IZForm)form1).DisplayMode != ODisplayMode.ReadOnly);
						AssertReadonlyness((ZOrganisationsForm)form1, "DetailsTabPage", "DetailsControl", "OH_IsCreditorBoundCheckEdit", true);
					}
				}
			}
		}

		void AddModuleAdditionalFilter(ZQuery query)
		{
			var codeQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "PNT") { OrderBy = "OH_Code" };
			query.AddToFilter(codeQuery);
		}

		void AssertReadonlyness(ZOrganisationsForm form, string tabName, string containingControlName, string controlToCheckName, bool shouldBeReadonly)
		{
			var tab = form.OrganisationsTabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == tabName);
			form.OrganisationsTabControl.SelectedTab = tab;
			var containingControl = tab.Controls.Find(containingControlName, true)[0];
			var controlToCheck = containingControl.Controls.Find(controlToCheckName, true)[0];
			AssertEquals(shouldBeReadonly ? "Control is read-only" : "Control is editable", shouldBeReadonly, controlToCheck.GetReadOnly());
		}

		[RequiresSTA]
		public void TestChangeUNLOCO()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			const string expectString =
				@"You have changed the UNLOCO of this organization. This may have an effect on the tax treatment of this organization. It is strongly recommended that you review the AR tax details of this organization.

Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Debtor will NOT change.";

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var header = Factory.New<OrgHeader>();
			header.OH_IsDebtor = true;
			header.OH_RL_NKClosestPort = nzakl.Code;

			using (var form = new OrgFormForTest(header))
			{
				form.Show();

				form.OrganisationsTabControl.SelectedTab = form.DetailsTabPage_Exposed;
				UnitTestUserNotification.Instance.ClearMessages();
				header.OH_RL_NKClosestPort = ausyd.Code;
				AssertEquals(form.OrganisationsTabControl.SelectedTab, form.ReceivablesTabPage);
				AssertEquals(expectString, UnitTestUserNotification.Instance.LastMessage.Text);

				form.OrganisationsTabControl.SelectedTab = form.DetailsTabPage_Exposed;
				UnitTestUserNotification.Instance.ClearMessages();
				header.OH_RL_NKClosestPort = aumel.Code;
				AssertEquals(form.OrganisationsTabControl.SelectedTab, form.DetailsTabPage_Exposed);
				AssertNull("Shouldn't show message.", UnitTestUserNotification.Instance.LastMessage.Text);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				form.OrganisationsTabControl.SelectedTab = form.DetailsTabPage_Exposed;
				UnitTestUserNotification.Instance.ClearMessages();
				header.OH_RL_NKClosestPort = nzakl.Code;
				AssertEquals(form.OrganisationsTabControl.SelectedTab, form.DetailsTabPage_Exposed);
				AssertNull("Shouldn't show message.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestSetOrderOfTabPagesShouldNotBreakBindingContext()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = null;

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_Phone_Formatted = "2 9025 2222";

			using (var form = new OrgFormForTest(orgHeader))
			{
				form.Show();
				Application.DoEvents();
				form.OrgTabControl.SelectedTab = form.AddressesTabPage_Exposed;
				orgHeader.MainAddress.OA_Phone_Formatted = "23"; // Give a wrong formatted phone number
				form.ViewEnabledTabsOnlyClick(null, EventArgs.Empty);
			}
		}

		public void TestTSAKnownAddressChanged()
		{
			var expectedMessage = "A US TSA Known Shipper address has been modified. This may cause TSA record inconsistency. All previous approved TSA Known Shipper linked to this address are changed to not approved. Refer to the TSA Known Shipper tab which shows the details that have been last reviewed with TSA.";

			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "Test Org";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_RN_NKCountryCode = "AU";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.State = "NSW";
			org.MainAddress.City = "Sydney";
			org.OH_IsConsignee = true;

			var mainAddress = org.MainAddress;
			var orgCountryDataTSARecord = Factory.New<OrgCountryData>();
			orgCountryDataTSARecord.OV_OA_ApprovedLocation = mainAddress.PK;
			orgCountryDataTSARecord.OV_EXApprovedOrMajorExporter = "Yes";
			orgCountryDataTSARecord.OV_EXApprovalNumber = "1234";
			orgCountryDataTSARecord.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
			orgCountryDataTSARecord.OV_OH_OrgHeader = org.PK;

			Factory.Save();

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.Address1 = "Test Address 1";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, expectedMessage);
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.Address2 = "Test Address 2";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, expectedMessage);
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.Postcode = "2001";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, expectedMessage);
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.State = "ACT";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, expectedMessage);
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.City = "Alexandria";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, expectedMessage);
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.OA_RN_NKCountryCode = "US";
				mainAddress.State = "CA";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, expectedMessage);
			}
		}

		public void TestMIDAddressChanged()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "Test Org";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_RN_NKCountryCode = "AU";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.State = "NSW";
			org.MainAddress.City = "Sydney";
			org.OH_IsConsignee = true;

			var mainAddress = org.MainAddress;
			var orgCusCodeMIDRecord = Factory.New<OrgCusCode>();
			orgCusCodeMIDRecord.OK_OA_PremisesAddress = mainAddress.PK;
			orgCusCodeMIDRecord.OK_CodeType = "MID";
			orgCusCodeMIDRecord.OK_CustomsRegNo = "abcd1234";
			orgCusCodeMIDRecord.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgCusCodeMIDRecord.OK_OH = org.PK;

			Factory.Save();

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.Address1 = "Test Address 1";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number.");
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.Address2 = "Test Address 2";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number.");
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.Postcode = "2001";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number.");
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.State = "ACT";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number.");
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.City = "Alexandria";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number.");
			}

			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				mainAddress.OA_RN_NKCountryCode = "US";
				mainAddress.State = "CA";
				form.ButtonsUserControl.SaveButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number.");
			}
		}

		public void TestNewLinkRelationship()
		{
			var orgNew = Factory.NewWithValidTestData<OrgHeader>();
			orgNew.OH_Code = "ABC";
			orgNew.OH_FullName = "Test Org";
			orgNew.MainAddress.OA_Address1 = "Address 1";
			orgNew.MainAddress.OA_RN_NKCountryCode = "AU";
			orgNew.OH_RL_NKClosestPort = "AUSYD";
			orgNew.MainAddress.Postcode = "2000";
			orgNew.MainAddress.State = "NSW";
			orgNew.MainAddress.City = "Sydney";
			orgNew.OH_IsConsignee = true;

			using (var form = new OrgFormForTest(orgNew))
			{
				var factory = form.Organisation.Factory;
				var orgChild = factory.NewWithValidTestData<OrgHeader>();
				orgChild.OH_FullName = "MYCHILDORG";
				orgChild.OH_RL_NKClosestPort = "AUBRN";

				var mockUserNotification = new Mock<IUserNotification>();
				var link = new OrgHeaderLink(factory, mockUserNotification.Object, orgNew, orgChild);

				link.TryAddNewOrgRelatedParty(orgNew, orgChild);

				var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgNew.PK);
				query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK);
				var relatedPartyInDb = factory.ExistsInDatabase(OrgRelatedParty.Schema.TableName, query);
				Assert("Relationship should not exist", !relatedPartyInDb);

				form.Show();
				form.ButtonsUserControl.SaveButton.PerformClick();

				relatedPartyInDb = factory.ExistsInDatabase(OrgRelatedParty.Schema.TableName, query);
				Assert("Relationship should exist", relatedPartyInDb);
			}
		}

		public void TestDebtorAndCreditorReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			Env.Security.OrgDetailsModifyOrganisationType.IsAllowed = false;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				Application.DoEvents();
				var isDebtorBoundCheckEdit = FindCheckBox(form.DetailsControl, "OH_IsDebtorBoundCheckEdit");
				var isCreditorBoundCheckEdit = FindCheckBox(form.DetailsControl, "OH_IsCreditorBoundCheckEdit");
				AssertEquals(true, isDebtorBoundCheckEdit.GetReadOnly());
				AssertEquals(true, isCreditorBoundCheckEdit.GetReadOnly());
			}

			Env.Security.OrgDetailsModifyOrganisationType.IsAllowed = true;
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				Application.DoEvents();
				var isDebtorBoundCheckEdit = FindCheckBox(form.DetailsControl, "OH_IsDebtorBoundCheckEdit");
				var isCreditorBoundCheckEdit = FindCheckBox(form.DetailsControl, "OH_IsCreditorBoundCheckEdit");
				AssertEquals(false, isDebtorBoundCheckEdit.GetReadOnly());
				AssertEquals(false, isCreditorBoundCheckEdit.GetReadOnly());
			}
		}

		public void TestShowForm_CarrierServiceLevelsHaveBadData_ShowValidationErrors()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var level1 = org.MiscServ.CarrierServiceLevels.AddNew();
			level1.PL_Code = "PRY";
			level1.PL_CarrierServiceCode = "UEXP";
			level1.PL_CarrierServiceLevelDescription = "Priority";

			var level2 = org.MiscServ.CarrierServiceLevels.AddNew();
			level2.PL_Code = "EXP";
			level2.PL_CarrierServiceCode = "UEXP";
			level2.PL_CarrierServiceLevelDescription = "Express";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newOrg = newFactory.Load<OrgHeader>(org.PK);

			using (var form = new OrgFormForTest(newOrg))
			{
				form.Show();
				Application.DoEvents();

				form.ValidateAndSave();
				var notifications = newOrg.NotificationsIncludingChildren.Select(n => n.Message).ToArray();
				AssertCollectionContains("Error - PL_CarrierServiceCode: The Carrier Service Code has been duplicated and must be unique.", notifications);
			}
		}

		#region ExportPatternMatchOverride

		public void TestPatternOverrideCreateExportMenuItem()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var orgForm = new OrgFormForTest(orgHeader))
			{
				orgForm.ActionsMenuItem_Exposed.ShowPopupMenu();

				var newMenu = orgForm.ActionsMenuItem_Exposed.MenuItems.FindByText("Export Pattern Match Overrides as Native XML");
				AssertNotNull("Export Pattern Override Menu Item", newMenu);

				var existingMenu = orgForm.ActionsMenuItem_Exposed.MenuItems.FindByText(ExportXmlMenuItemHelper.NativeMenuItemText);
				Assert("Pre-condition", existingMenu != null);

				AssertEquals($"New Menu is placed after '{ExportXmlMenuItemHelper.NativeMenuItemText}' menu", existingMenu.Index + 1, newMenu.Index);

				MenuItem duplicateMenu = null;
				if (orgForm.ActionsMenuItem_Exposed.MenuItems.Count > newMenu.Index + 1)
				{
					duplicateMenu = orgForm.ActionsMenuItem_Exposed.MenuItems[newMenu.Index + 1];
				}

				Assert("Menu must not be added twice", duplicateMenu == null || duplicateMenu.Text != "Export Pattern Match Overrides as Native XML");
			}
		}

		public void TestPatternOverrideUtilHasExpectedOrgs()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			using (var orgModule = new OrgFormForTest(org))
			{
				AssertNotNull("Util has GetOrgHeaders delegate set", orgModule.ExportPatternMatchOverridesUtil_Exposed.GetOrgHeadersExposedForTesting);
				AssertContainsExactElementsInAnyOrder("Util orgs matched grids selected orgs", new List<ZGuid>() { org.PK }, orgModule.ExportPatternMatchOverridesUtil_Exposed?.GetOrgHeadersExposedForTesting().Select(x => x.PK));
			}
		}

		#endregion ExportPatternMatchOverride

		#region ProductivityWise Mode

		[RequiresSTA]
		public void TestProductivityWiseMode_ShouldHideCustomsMessagingMenuItem()
		{
			var org = Factory.New<OrgHeader>();

			void AssertMenuItems(string message, params string[] expectedMenuItems)
			{
				using (var form = new ZOrganisationsForm(org))
				{
					form.Show();
					Application.DoEvents();
					var items = new List<string>(5);
					items.AddRange(from MenuItem item in ((IFileMenuItemsProvider)form).MainMenu.MenuItems select item.Text);

					AssertContainsExactElementsInAnyOrder(message, expectedMenuItems, items);

					var actionMenuItems = new List<string>();
					actionMenuItems.AddRange(from MenuItem item in ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems select item.Text);

					AssertCollectionContains("View &All Tabs", actionMenuItems);
					AssertCollectionContains("View &Enabled Tabs Only", actionMenuItems);
				}
			}

			AssertMenuItems("'Customs Messaging' should be shown, and the tab menu items included, when ProductivityWise mode is not enabled. SAD!", "&File", "&Edit", "Actio&ns", "Customs Messaging", "Customs Messaging", "&Documents", "&Help"); // Customs appears twice in the list but only once functionally. There's no way of knowing why, and it doesn't matter.

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertMenuItems("Logistics-related menu items (e.g. Customs Messaging and the tab menu items) should be removed when ProductivityWise mode is enabled. SAD!", "&File", "&Edit", "Actio&ns", "&Documents", "&Help");
		}

		public void TestProductivityWiseMode_ShouldHideCustomsMessagingTab()
		{
			var org = Factory.New<OrgHeader>();

			AssertTabs("All tabs should be shown when ProductivityWise mode is disabled and 'Show All Tabs' is selected. SAD!", org,
				"Details", "Address", "Contact", "A/R", "A/P", "Consignor", "Consignee", "Warehouse", "Fwd/Agent", "Carrier", "Services", "Sales", "Competitor", "Workflow && Tracking", "Custom", "Customs Messaging", "Doc Data", "eDocs", "Notes", "Logs");

			SetShowTabsRegistryItem(TabShowingMode.ShowTabsForSelectedTypes);

			AssertTabs("'Customs Messaging' should still be shown with ProductivityWise mode disabled and 'Show All Tabs' selected. SAD!", org,
				"Details", "Address", "Contact", "Workflow && Tracking", "Custom", "Customs Messaging", "Doc Data", "eDocs", "Notes", "Logs");

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertTabs("Logistics-related tabs (e.g. Customs Messaging) should be removed when ProductivityWise mode is enabled. SAD!", org, "Details", "Address", "Contact", "Workflow && Tracking", "Custom", "Doc Data", "eDocs", "Notes", "Logs");

			SetShowTabsRegistryItem(TabShowingMode.ShowAllTabs);
			AssertTabs("Logistics-related tabs (e.g. Customs Messaging) should still be removed when ProductivityWise mode is enabled, even with 'Show All Tabs' selected. SAD!", org,
				"Details", "Address", "Contact", "A/R", "A/P", "Sales", "Workflow && Tracking", "Custom", "Doc Data", "eDocs", "Notes", "Logs");
		}

		public void TestProductivityWiseMode_WhenOrgIsConfiguredForLogistics_ShouldStillHideLogisticsElements()
		{
			SetShowTabsRegistryItem(TabShowingMode.ShowTabsForSelectedTypes);
			var org = Factory.New<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;
			org.OH_IsConsignor = true;
			org.OH_IsConsignee = true;
			org.OH_IsWarehouseClient = true;
			org.OH_IsShippingProvider = true;
			org.OH_IsForwarder = true;
			org.OH_IsSalesLead = true;
			org.OH_IsCompetitor = true;
			org.OH_IsMiscFreightServices = true;

			AssertTabs("All tabs should be shown when ProductivityWise mode is disabled and 'Show All Tabs' is not selected, but the options to show those tabs are enabled. SAD!", org,
				"Details", "Address", "Contact", "A/R", "A/P", "Consignor", "Consignee", "Warehouse", "Fwd/Agent", "Carrier", "Services", "Sales", "Competitor", "Workflow && Tracking", "Custom", "Customs Messaging", "Doc Data", "eDocs", "Notes", "Logs");

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertTabs("Logistics-related tabs should be removed when ProductivityWise mode is enabled, even if those options are enabled on the organisation. SAD!", org,
				"Details", "Address", "Contact", "A/R", "A/P", "Sales", "Workflow && Tracking", "Custom", "Doc Data", "eDocs", "Notes", "Logs");
		}

		static void SetShowTabsRegistryItem(TabShowingMode mode)
		{
			OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, mode == TabShowingMode.ShowTabsForSelectedTypes);
		}

		enum TabShowingMode // So that these tests can be understood
		{
			ShowAllTabs,
			ShowTabsForSelectedTypes
		}

		static void AssertTabs(string message, OrgHeader org, params string[] expectedTabs)
		{
			using (var form = new ZOrganisationsForm(org))
			{
				form.Show();
				Application.DoEvents();
				Application.DoEvents();
				var tabs = form.OrganisationsTabControl.AllTabPages.Select(x => x.Text);

				AssertContainsExactElementsInAnyOrder(message, expectedTabs, tabs);
			}
		}

		#endregion

		#region Test Modify Org Proxies

		[RequiresSTA]
		public void TestReadOnlyWhenChangeOrgBranchProxiesEditSecurity()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Env.Security.OrgBranchProxiesEdit.IsAllowed = false;
			using (var form = new OrgFormForTest(organization))
			{
				form.Show();
				AssertOrgFormReadOnly(form, false);
			}

			Env.Security.OrgBranchProxiesEdit.IsAllowed = true;
			using (var form = new OrgFormForTest(organization))
			{
				form.Show();
				AssertOrgFormReadOnly(form, false);
			}

			var branch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);
			branch.GB_OH_OrgProxy = organization.PK;
			Factory.Save();

			Env.Security.OrgBranchProxiesEdit.IsAllowed = false;
			using (var form = new OrgFormForTest(organization))
			{
				form.Show();
				AssertOrgFormReadOnly(form, true);
			}

			Env.Security.OrgBranchProxiesEdit.IsAllowed = true;
			using (var form = new OrgFormForTest(organization))
			{
				form.Show();
				AssertOrgFormReadOnly(form, false);
			}
		}

		[RequiresSTA]
		public void TestReadOnlyWhenChangeCompanyProxiesEditSecurity()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Env.Security.OrgCompanyProxiesEdit.IsAllowed = false;
			using (var form = new OrgFormForTest(organization))
			{
				form.Show();
				AssertOrgFormReadOnly(form, false);
			}

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = true;
			using (var form = new OrgFormForTest(organization))
			{
				form.Show();
				AssertOrgFormReadOnly(form, false);
			}

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_OH_OrgProxy = organization.PK;
			Factory.Save();

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = false;
			using (var form = new OrgFormForTest(organization))
			{
				form.Show();
				AssertOrgFormReadOnly(form, true);
			}

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = true;
			using (var form = new OrgFormForTest(organization))
			{
				form.Show();
				AssertOrgFormReadOnly(form, false);
			}
		}

		public void TestDeleteButtonIsShownWhenChangeProxiesEditSecurityAndInDeleteMode()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_OH_OrgProxy = organization.PK;
			Factory.Save();

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = false;
			using (var form = new OrgFormForTest(organization))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();

				AssertEquals("Delete button is shown in delete mode.", true, form.PostButton.Visible);
			}
		}

		void AssertOrgFormReadOnly(OrgFormForTest form, bool isReadOnly)
		{
			var tabControl = form.OrgTabControl;
			var button = tabControl.TabPages[0].Controls.Find("OrgProxiesSecurity", false)[0];

			CombineAssertions(() =>
			{
				AssertNotNull(button);
				AssertEquals(!isReadOnly, button.Enabled);
				AssertEquals(!isReadOnly, form.PostButton.Visible);
				AssertEquals(!isReadOnly, form.ApplyButton.Visible);
				AssertEquals(isReadOnly, form.DisplayMode == ODisplayMode.ReadOnly);
			});
		}

		#endregion

		#region E-Payment Default Payment Reason

		public void TestAccountDetailsWithoutDefaultPaymentReason()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "My Test Organisation";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.OH_IsCreditor = true;
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			organisation.CompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			var mainAddress = organisation.Addresses.MainAddress;
			mainAddress.OA_City = "Sydney";
			mainAddress.OA_Address1 = "8 George Street";
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.OA_PostCode = "2000";
			var accountDetails = organisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			organisation.RunPreSaveValidation();
			AssertNoErrors(organisation);
			Factory.Save();

			using (var orgForm = new ZOrganisationsForm(organisation))
			{
				orgForm.Show();

				var accountDetail1 = accountDetails.AddNew();
				accountDetail1.A1_IsDefaultAccount = true;
				accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetail1.A1_RX_NKAccountCurrency = Constants.CurrencyCodes.UnitedStates;
				accountDetail1.A1_EPaymentReasonCode = ZString.Empty;
				accountDetail1.A1_EPaymentReferenceType = EPaymentReferenceTypes.InvoiceNumbers;

				Assert(!accountDetail1.IsInDatabase);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				orgForm.FireSaveButton();
				AssertEquals("You have not selected a Default Payment Method. If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider. Do you wish to continue without adding a Default Payment Method?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!accountDetail1.IsInDatabase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				orgForm.FireSaveButton();
				AssertEquals("You have not selected a Default Payment Method. If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider. Do you wish to continue without adding a Default Payment Method?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(accountDetail1.IsInDatabase);

				var accountDetail2 = accountDetails.AddNew();
				accountDetail2.A1_IsDefaultAccount = true;
				accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetail2.A1_RX_NKAccountCurrency = Constants.CurrencyCodes.EuropeanUnion;
				accountDetail2.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
				accountDetail2.A1_EPaymentReferenceType = EPaymentReferenceTypes.InvoiceNumbers;

				Assert(!accountDetail2.IsInDatabase);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				orgForm.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not selected a Default Payment Method.If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider.Do you wish to continue without adding a Default Payment Method?"));
				Assert(accountDetail2.IsInDatabase);
			}
		}

		#endregion

		#region Enroll for Electronic Bill of Lading

		public void TestEnrollForElectronicBillOfLadingMenuItem_Visible()
		{
			AssertEnrollForElectronicBillOfLadingMenuItem(true, true);
		}

		public void TestEnrollForElectronicBillOfLadingMenuItem_HiddenDueToRegistrySetting()
		{
			AssertEnrollForElectronicBillOfLadingMenuItem(false, true);
		}

		public void TestEnrollForElectronicBillOfLadingMenuItem_HiddenDueToSecuritySetting()
		{
			AssertEnrollForElectronicBillOfLadingMenuItem(true, false);
		}

		public void TestEnrollForElectronicBillOfLading()
		{
			AssertEnrollForElectronicBillOfLading(OrgCusCode.CodeTypes.BoleroTitleRegisterID, false, true);
			AssertEnrollForElectronicBillOfLading(OrgCusCode.CodeTypes.OrganizationNumber, false, true);
			AssertEnrollForElectronicBillOfLading(OrgCusCode.CodeTypes.OrganizationNumber, true, true);
			AssertEnrollForElectronicBillOfLading(OrgCusCode.CodeTypes.OrganizationNumber, true, false);
			AssertEnrollForElectronicBillOfLading(OrgCusCode.CodeTypes.OrganizationNumber, false, false);
		}

		public void TestEnrollForElectronicBillOfLading_ResponseIsAwaited()
		{
			AssertEnrollForElectronicBillOfLading_ResponseIsAwaited(true);
			AssertEnrollForElectronicBillOfLading_ResponseIsAwaited(false);
		}

		#endregion

		#region OnFormClosing

		public void TestFormOnClosing()
		{
			AssertFormOnClosing(false, false);
			AssertFormOnClosing(true, true);
		}

		public void TestResendInviteShowConfirmationPromptIfRequestIsPendingMSN_PressYesShowBoleroForm()
		{
			AssertResendInviteConfirmationPromptValidation(
				true,
				true,
				Events.MessageAccepted.Code,
				Events.MessageSent.Code);
		}

		public void TestResendInviteShowConfirmationPromptIfRequestIsPendingMSN_PressNo()
		{
			AssertResendInviteConfirmationPromptValidation(
				true,
				false,
				Events.MessageAccepted.Code,
				Events.MessageSent.Code);
		}

		public void TestResendInviteShowConfirmationPromptIfRequestIsPendingMPP()
		{
			AssertResendInviteConfirmationPromptValidation(
				true,
				false,
				Events.MessageAccepted.Code,
				Events.MessageSent.Code,
				Events.MessagePendingProcessing.Code
				);
		}

		public void TestResendInviteDonNotShowConfirmationPromptIfRequestIsAccepted()
		{
			AssertResendInviteConfirmationPromptValidation(
				false,
				true,
				Events.MessageAccepted.Code
			);
		}

		public void TestResendInviteDonNotShowConfirmationPromptIfRequestIsRejected()
		{
			AssertResendInviteConfirmationPromptValidation(
				false,
				true,
				Events.MessageAccepted.Code,
				Events.MessageSent.Code,
				Events.MessagePendingProcessing.Code,
				Events.MessageRejected.Code
			);

			AssertResendInviteConfirmationPromptValidation(
				false,
				true,
				Events.MessageAccepted.Code,
				Events.MessageSent.Code,
				Events.MessagePendingProcessing.Code,
				Events.InterchangeRejected.Code
			);
		}
		#endregion

		#region Implementation
		void AssertResendInviteConfirmationPromptValidation(bool showConfirmation, bool promptConfirmed, params string[] eventCodes)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			foreach (var eventCode in eventCodes)
			{
				var log = org.Logs.AddNew();
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = eventCode;
					log.SL_Reference = "DEP=Bolero";
					Factory.Save();
				}
			}

			Env.Security.EnrollForElectronicBillOfLading.IsAllowed = true;
			var boleroEBLForOrganisationConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLForOrganisationConfiguration))
			{
				using (var form = new OrgFormForTest(org))
				{
					form.Show();

					var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText(
						"Enroll For Electronic Bill Of Lading");

					AssertNotNull("Enroll For Electronic Bill Of Lading", item);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					if (promptConfirmed)
					{
						UnitTestUserNotification.Instance.AddYesAnswer();
					}

					item.PerformClick();

					if (showConfirmation)
					{
						AssertEquals(
							"The Bolero system is processing the previously submitted Enrollment Request. Resending may cause errors or duplication.\r\nDo you want to re-submit this Enrollment Request?",
							"The Bolero system is processing the previously submitted Enrollment Request. Resending may cause errors or duplication.\r\nDo you want to re-submit this Enrollment Request?",
							UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					}

					var boleroInvitationForm = ZFormModaliser.LastFormShownDialogForTest as BoleroInvitationForm;
					AssertEquals("Bolero Invitation Form", promptConfirmed, boleroInvitationForm != null);
					boleroInvitationForm?.Close();
				}
			}
		}

		void AssertEnrollForElectronicBillOfLading_ResponseIsAwaited(bool boleroEnrollmentRequestSent)
		{
			var boleroEBLConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				using (var form = new OrgFormForTest(org))
				{
					form.BoleroEnrollmentRequestSent = boleroEnrollmentRequestSent;
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText("Enroll For Electronic Bill Of Lading");
					item.PerformClick();

					if (boleroEnrollmentRequestSent)
					{
						AssertEquals("You have already sent an enrollment request, and a response is awaited.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		void AssertFormOnClosing(bool boleroEnrollmentRequestSent, bool expectShouldDisplayMessage)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new OrgFormForTest(org))
			{
				form.Show();
				form.BoleroEnrollmentRequestSent = boleroEnrollmentRequestSent;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Close();
				if (expectShouldDisplayMessage)
				{
					AssertEquals("The Bolero system is processing the Enrollment Request you submitted. Closing the Organization form will lead to termination of the request.\r\nDo you want to still close the Organization form?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		void AssertEnrollForElectronicBillOfLadingMenuItem(bool registrySetting, bool securitySetting)
		{
			Env.Security.EnrollForElectronicBillOfLading.IsAllowed = securitySetting;
			var boleroEBLForOrganisationConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = registrySetting,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLForOrganisationConfiguration))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();

				using (var form = new OrgFormForTest(org))
				{
					form.Show();
					var item = form.ActionsMenuItem_Exposed.MenuItems.FindByText("Enroll For Electronic Bill Of Lading");
					AssertEquals(registrySetting && securitySetting, item != null);

					if (item != null)
					{
						var branch = Factory.NewWithValidTestData<GlbBranch>();
						branch.GB_OH_OrgProxy = org.PK;
						branch.GB_IsActive = true;

						var company = Factory.NewWithValidTestData<GlbCompany>();
						company.GC_IsActive = true;
						branch.GB_GC = company.PK;
						Factory.Save();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						AssertEquals("Enrollment request not supported on internal organizations", UnitTestUserNotification.Instance.LastMessage.Text);

						branch.GB_OH_OrgProxy = Guid.Empty;
						Factory.Save();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

						branch.GB_OH_OrgProxy = org.PK;
						branch.GB_IsActive = true;
						company.GC_IsActive = false;
						Factory.Save();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

						branch.GB_IsActive = false;
						company.GC_IsActive = true;
						Factory.Save();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		void AssertEnrollForElectronicBillOfLading(string cusCodeType, bool disposeOrgForm, bool invitationFormDialogResultIsOk)
		{
			var boleroEBLForOrganisationConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLForOrganisationConfiguration))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
				cusCode.OK_OH = org.PK;
				cusCode.OK_CodeType = cusCodeType;
				org.CustomsCodes.Add(cusCode);

				using (var form = new OrgFormForTest(org))
				{
					form.Show();

					if (disposeOrgForm)
					{
						Factory.Save();
						form.Close();
					}

					var query = new ZQuery(StmALogSchema.SL_Parent, org.PK);
					query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageSentCode);
					var logs = Factory.Load<StmALog>(query);
					var previousMessageSentLogCount = logs.Length;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = invitationFormDialogResultIsOk ? DialogResult.OK : DialogResult.Cancel;

					AsyncTaskSynchronizer.Run(form.EnrollForElectronicBillOfLadingForTest);

					var invitationForm = ZFormModaliser.LastFormShownDialogForTest as BoleroInvitationForm;

					if (cusCode.OK_CodeType == OrgCusCode.CodeTypes.BoleroTitleRegisterID)
					{
						AssertNull(invitationForm);
						AssertEquals($"The customer, {org.OH_FullName}, is already onboarded with Bolero to use the Electronic Bill of Lading functionality. You are not allowed to send another enrollment request.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						logs = Factory.Load<StmALog>(query);
						AssertEquals(invitationFormDialogResultIsOk ? (previousMessageSentLogCount + 1) : previousMessageSentLogCount, logs.Length);
						AssertNotNull(invitationForm);

						if (disposeOrgForm || !invitationFormDialogResultIsOk)
						{
							AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						}
						else
						{
							AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
						}

						invitationForm.Close();
					}

					Assert(!form.BoleroEnrollmentRequestSent);
				}
			}
		}

		void AssertControlVisibilityForOrgType(ZPropertyInfo orgType, ZTemplateTabControl tabControl, int index, Control control, Control label)
		{
			tabControl.SelectedIndex = 0;
			orgType.Value = ZBool.False;
			tabControl.SelectedIndex = index;
			Assert("Control should not be visible", !control.Visible);
			Assert("Page not available Label should be visible", label.Visible);

			tabControl.SelectedIndex = 0;
			orgType.Value = ZBool.True;
			tabControl.SelectedIndex = index;
			Assert("Control should be visible", control.Visible);
			Assert("Page not available Label should not be visible", !label.Visible);
		}

		static CheckBox FindCheckBox(Control control, string name)
		{
			return (CheckBox)FindControl(control, name);
		}

		static Control FindControl(Control control, string name)
		{
			foreach (Control child in control.Controls)
			{
				if (child.Name == name)
				{
					return child;
				}
				Control result = FindControl(child, name);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		void AssertText(string expectedValue)
		{
			OrgFormForTest form;
			using (form = new OrgFormForTest(Factory.New<OrgHeader>()))
			{
				form.Show();
				form.ViewAllTabsClick(null, EventArgs.Empty);
				ExposeAllTabPages(form);
				AssertEquals("TabPage must be contained " + expectedValue, expectedValue, form.OrgTabControl.TabPages[5].Text);
				AssertEquals("ConsignorNotSelectedLabel must be contained " + expectedValue, $"You must select an Organization type of {expectedValue} from the Details page to use this page.", form.ConsignorTabPage.Controls["ConsignorNotSelectedLabel"].Text);
			}
		}

		#region Mock Organisation Form

		public class OrgFormForTest : ZOrganisationsForm
		{
			public OrgFormForTest(OrgHeader organisation)
				: base(organisation)
			{
				OnImportEDICodeMappingClicked = false;
				Exporter = DataTransferExporter;
				AddTestOrgProxiesSecurityButton();
			}

			public async Task EnrollForElectronicBillOfLadingForTest() => await EnrollForElectronicBillOfLading();

			public new MainDetailsUserControl DetailsControl
			{
				get { return base.DetailsControl; }
			}

			public ZTemplateTabControl OrgTabControl
			{
				get { return OrganisationsTabControl; }
			}

			public ZTabPage DetailsTabPage_Exposed
			{
				get { return DetailsTabPage; }
			}

			public ZTabPage AddressesTabPage_Exposed
			{
				get { return AddressesTabPage; }
			}

			public ZTabPage ContactsTabPage_Exposed
			{
				get { return ContactsTabPage; }
			}

			public new void SetPageTabOrder()
			{
				base.SetPageTabOrder();
			}

			public OrgHeader Org
			{
				get { return Organisation; }
			}

			public MenuItem ActionsMenuItem_Exposed
			{
				get { return ActionsMenuItem; }
			}

			public new ContinueWithSave ValidateAndSave()
			{
				return base.ValidateAndSave();
			}

			public IXmlDataTransferExporter DataTransferExporter
			{
				get
				{
					if (dataTransferExporter == null)
					{
						dataTransferExporter = new TestXmlDataTransferExporter();
					}
					return dataTransferExporter;
				}
			}
			IXmlDataTransferExporter dataTransferExporter;

			protected override DialogResult ShowConfirmationForDelete()
			{
				return DialogResult.OK;
			}

			protected override void OnImportEDICodeMapping(object sender, EventArgs e)
			{
				OnImportEDICodeMappingClicked = true;
			}
			public bool OnImportEDICodeMappingClicked;

			public void CloseProgressFormAndOrgFormIfNeededForTest()
			{
				CloseProgressFormAndOrgFormIfNeeded();
			}

			public bool IsSaveAndCloseButtonClickedForTest
			{
				get => IsSaveAndCloseButtonClicked;
				set => IsSaveAndCloseButtonClicked = value;
			}

			public IButton PostButton => fPostButton;
			public IButton ApplyButton => fApplyButton;

			void AddTestOrgProxiesSecurityButton()
			{
				OrgTabControl.TabPages[0].Controls.Add(new ZButton { Name = "OrgProxiesSecurity", Enabled = true });
			}

			public ExportPatternMatchOverridesUtil ExportPatternMatchOverridesUtil_Exposed { get { return ExportPatternMatchOverridesUtil; } }
		}

		class TestXmlDataTransferExporter : IXmlDataTransferExporter
		{
			#region IXmlDataTransferDirector Members

			public void PromptUserAndExport(System.Collections.IList selectedElements)
			{
				ExportDone = true;
			}

			#endregion

			public bool ExportDone;

			public void PromptUserAndExport(ZQuery exportQuery)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Org With Everything On

		public OrgHeader OrgWithEverythingOn
		{
			get
			{
				if (orgWithEverythingOn == null)
				{
					orgWithEverythingOn = Factory.New<OrgHeader>();
					using (orgWithEverythingOn.SuspendSettingHasChanges())
					using (orgWithEverythingOn.CompanyData.SuspendSettingHasChanges())
					{
						orgWithEverythingOn.OH_IsCreditor = true;
						orgWithEverythingOn.OH_IsDebtor = true;
						orgWithEverythingOn.OH_IsConsignee = true;
						orgWithEverythingOn.OH_IsConsignor = true;
						orgWithEverythingOn.OH_IsTransportClient = true;
						orgWithEverythingOn.OH_IsWarehouseClient = true;
						orgWithEverythingOn.OH_IsShippingProvider = true;
						orgWithEverythingOn.OH_IsForwarder = true;
						orgWithEverythingOn.OH_IsBroker = true;
						orgWithEverythingOn.OH_IsMiscFreightServices = true;
						orgWithEverythingOn.OH_IsCompetitor = true;
						orgWithEverythingOn.OH_IsSalesLead = true;
					}
				}
				return orgWithEverythingOn;
			}
		}
		OrgHeader orgWithEverythingOn;

		#endregion

		protected override Form GetFormToBashCore()
		{
			ZOrganisationsForm orgForm = new ZOrganisationsForm(OrgWithEverythingOn);
			OrgWithEverythingOn.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			return orgForm;
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		#endregion
	}
}
