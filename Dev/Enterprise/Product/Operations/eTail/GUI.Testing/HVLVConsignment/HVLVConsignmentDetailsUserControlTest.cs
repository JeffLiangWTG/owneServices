using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.Module;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.GUI.Testing
{
	class HVLVConsignmentDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestConsigneeConvertToOrganizationButton_WhenClickedWithoutConsignment_DoesNotThrowAnException()
		{
			AssertClickConvertToOrganizationButtonShouldNotThrowExceptionWhenConsignmentIsNull("zButtonConsigneeConvertToOrganisation");
		}

		public void TestShipperConvertToOrganizationButton_WhenClickedWithoutConsignment_DoesNotThrowAnException()
		{
			AssertClickConvertToOrganizationButtonShouldNotThrowExceptionWhenConsignmentIsNull("zButtonShipperConvertToOrganisation");
		}

		void AssertClickConvertToOrganizationButtonShouldNotThrowExceptionWhenConsignmentIsNull(string buttonName)
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				AssertNull("Precondition: current selected consignment is null", form.ConsignmentsGrid.GetCurrent());

				var button = (ZButton)form.Controls.Find(buttonName, true)[0];

				AssertNoExceptionThrown("Conversion should not happen when consignment is null", () => button.PerformClick());
			}
		}

		public void TestConsigneeConvertToOrganizationButton_WhenClicked_ShowSimilarAddressSelectionForm()
		{
			AssertClickConvertToOrganizationButtonShouldShowSimilarAddressSelectionForm("zButtonConsigneeConvertToOrganisation");
		}

		public void TestShipperConvertToOrganizationButton_WhenClicked_ShowSimilarAddressSelectionForm()
		{
			AssertClickConvertToOrganizationButtonShouldShowSimilarAddressSelectionForm("zButtonShipperConvertToOrganisation");
		}

		void AssertClickConvertToOrganizationButtonShouldShowSimilarAddressSelectionForm(ZString buttonName)
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.AddNew();
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var button = (ZButton)form.Controls.Find(buttonName, true)[0];
				button.PerformClick();

				AssertType<HVLVSimilarAddressSelectionForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestUpdatePreScreeningStatusMenuItemClick()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN100";
			Factory.Save();

			AssertEquals("Precondition: ", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration { IsEnabled = true };

			var rule = preScreeningConfiguration.Rules.AddNew();
			rule.TransportMode = TransportModes.Sea;
			rule.OriginCountryCode = CountryCodes.China;
			rule.DestinationCountryCode = CountryCodes.NewZealand;

			var field = rule.Fields.AddNew();
			field.FieldDescription = "Shipper";
			field.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Error;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var rescreenButton = groupBox.Controls.Find("zButtonRescreen", true)[0] as ZButton;
				var originalNotificationsCount = UnitTestUserNotification.Instance.PreviousMessages.Length;

				rescreenButton.PerformClick();
				AssertNotEquals("The Pre-Screening Status should be updated", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);
				AssertEquals("No new notification should be shown when pre-screening is enabled", originalNotificationsCount, UnitTestUserNotification.Instance.PreviousMessages.Length);
			}
		}

		public void TestConsignmentStatusDropDownMenu()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_Status = "BKD";

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;
				var dropEdit = groupBox.Controls.Find("zDropEditConsignmentStatus", true)[0] as ZDropEdit;

				AssertNotNull(dropEdit);
				AssertEquals("BKD", dropEdit.Text);
			}
		}

		public void TestConvertToStandAloneDeclarationButton_WhenNoAttachedShipment_IsNotVisible()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			bookingHeader.Consignments.AddNew();
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				AssertEquals("The 'Convert to Stand Alone Declaration' button should NOT be visible", false, newButton.Visible);
			}
		}

		public void TestConvertToStandAloneDeclarationButton_WhenStandAloneDeclarationDoesNotExist_IsVisible()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.NewZealand;

			using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				AssertEquals("The 'Convert to Stand Alone Declaration' button should be visible", true, newButton.Visible);
			}
		}

		public void TestConvertToStandAloneDeclarationButton_WhenStandAloneDeclarationAlreadyExists_IsNotVisible()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "Test Ref";

			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			consignment.HVC_JE_ImportDeclaration = declaration.PK;

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				AssertEquals("The 'Convert to Stand Alone Declaration' button should not be visible", false, newButton.Visible);
			}
		}

		public void TestEditStandAloneDeclarationButton_WhenStandAloneDeclarationAlreadyExists_IsVisible()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "Test Ref";

			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			consignment.HVC_JE_ImportDeclaration = declaration.PK;

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var editButton = groupBox.Controls.Find("zButtonEditStandAloneDeclaration", true)[0] as ZButton;

				AssertEquals("The 'Convert to Stand Alone Declaration' button should be visible", true, editButton.Visible);
			}
		}

		public void TestEditStandAloneDeclarationButton_WhenStandAloneDeclarationDoesNotExists_IsNotVisible()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var editButton = groupBox.Controls.Find("zButtonEditStandAloneDeclaration", true)[0] as ZButton;

				AssertEquals("The 'Edit Stand Alone Declaration' button should not be visible", false, editButton.Visible);
			}
		}

		public void TestConvertToStandAloneDeclarationButton_Tooltip()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				AssertEquals("Convert to Stand Alone Declaration", newButton.ToolTipCaption);
			}
		}

		public void TestEditStandAloneDeclarationButton_Tooltip()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var editButton = groupBox.Controls.Find("zButtonEditStandAloneDeclaration", true)[0] as ZButton;

				AssertEquals("Edit Stand Alone Declaration", editButton.ToolTipCaption);
			}
		}

		public void TestConvertToStandAloneDeclarationButtonClick_WhenSaveDeclarationForm_CreatesNewDeclaration()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
				{
					var declarationForm = dialog as ZForm;

					declarationForm.BusinessEntity.Factory.Save();
					declarationForm.DialogResult = DialogResult.OK;
				});

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				newButton.PerformClick();

				AssertNotNull(consignment.StandAloneDeclarationForCurrentCompany);

				CombineAssertions("Standalone-declaration-related controls are refreshed accordingly", () =>
				{
					var declarationReferenceField = groupBox.Controls.Find("zTextBoxStandAloneDeclaration", true)[0] as ZTextBox;
					Assert("Declaration Reference should be updated", !string.IsNullOrEmpty(declarationReferenceField.Text));

					var editDeclarationButton = groupBox.Controls.Find("zButtonEditStandAloneDeclaration", true)[0] as ZButton;
					Assert("Edit Stand Alone Declaration button should be visible", editDeclarationButton.Visible);
					Assert("Created Stand Alone Declaration button should be invisible", !newButton.Visible);
				});
			}
		}

		public void TestConvertToStandAloneDeclarationButtonClick_WhenCancelDeclarationForm_DoesNotCreatesNewDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
			{
				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
				{
					var declarationForm = dialog as ZForm;

					declarationForm.DialogResult = DialogResult.Cancel;
				});

				newButton.PerformClick();

				AssertNull(consignment.StandAloneDeclarationForCurrentCompany);

				CombineAssertions("Standalone-declaration-related controls are showed accordingly", () =>
				{
					var declarationReferenceField = groupBox.Controls.Find("zTextBoxStandAloneDeclaration", true)[0] as ZTextBox;
					Assert("Declaration Reference should be empty", string.IsNullOrEmpty(declarationReferenceField.Text));

					var editButton = groupBox.Controls.Find("zButtonEditStandAloneDeclaration", true)[0] as ZButton;
					Assert("Edit Stand Alone Declaration button should be invisible", !editButton.Visible);
					Assert("Created Stand Alone Declaration button should be visible", newButton.Visible);
				});
			}
		}

		public void TestConvertToStandAloneDeclarationButtonClick_WhenDeclarationExisted_MessagePopupAndAutoSaved()
		{
			var dummyOrg = Factory.NewWithValidTestData<OrgHeader>();
			dummyOrg.OH_Code = "DMY";
			dummyOrg.MainAddress.OA_Address1 = "Add";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "123456";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USSYD";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "100001";
			consignment.Items.AddNew();
			consignment.HVC_OA_ConsigneeAddress = dummyOrg.MainAddress.PK;
			consignment.HVC_OA_ShipperAddress = dummyOrg.MainAddress.PK;

			Factory.Save();

			AssertEquals("Pre-condition: Stand Alone Declaration is not created", false, consignment.HasDeclarationForCurrentDirection);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.UnitedStates))
			using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_TransportMode = TransportModes.Sea;
				declaration.JE_DeclarationReference = "A123456";
				declaration.JE_MasterBill = "123456";
				declaration.JE_HouseBill = "100001";
				declaration.JE_GoodsDescription = "Only declarations created by the same company will be counted as matching";
				Factory.Save();

				AssertEquals("Pre-condition: 1 declaration is in database", 1, Factory.Load<BaseJobDeclaration>(new ZQuery()).Length);

				form.Show();
				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;
				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				newButton.PerformClick();

				CombineAssertions("Should not link to existing declaration", () =>
				{
					AssertEquals(false, consignment.HasDeclarationForCurrentDirection);
					AssertEquals("No new declaration is created", 1, Factory.Load<BaseJobDeclaration>(new ZQuery()).Length);
				});

				var lastForm = ZFormModaliser.LastFormShownDialogForTest as BaseJobDeclarationForm;
				AssertNull("Declaration form is not opened", lastForm);

				AssertEquals("Message to notify the user that existing declaration is found", "An existing Stand Alone Declaration A123456 is found containing matching details, proceeding will link this Declaration to the Consignment, would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				newButton.PerformClick();

				CombineAssertions("Should link to existing declaration", () =>
				{
					AssertEquals(true, consignment.HasDeclarationForCurrentDirection);
					AssertEquals("No new declaration is created", 1, Factory.Load<BaseJobDeclaration>(new ZQuery()).Length);
				});

				lastForm = ZFormModaliser.LastFormShownDialogForTest as BaseJobDeclarationForm;
				AssertNotNull("Declaration form is opened", lastForm);

				var openedDeclaration = ZFormModaliser.LastIBusinessShownOnDialogForTest as BaseJobDeclaration;
				AssertEquals("Declaration is auto saved", false, openedDeclaration.HasChanges);
				AssertEquals("post assertion: opened declaration is the existing one", declaration.PK, openedDeclaration.PK);
			}
		}

		public void TestEditDeclarationButtonClick_OpensDeclarationForm()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "Test Ref";

			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			consignment.HVC_JE_ImportDeclaration = declaration.PK;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;
				var editButton = groupBox.Controls.Find("zButtonEditStandAloneDeclaration", true)[0] as ZButton;

				editButton.PerformClick();

				using (var formDeclaration = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name.Contains("JobDeclarationForm")))
				{
					AssertNotNull(formDeclaration);
				}
			}
		}

		public void TestCustomClearanceStatusTextBox_WithExportConsignment_ShowsExportStatus()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var importStatusTextBox = form.Controls.Find("zTextBoxImportCustomsClearanceStatus", true)[0] as ZTextBox;
					var exportStatusTextBox = form.Controls.Find("zTextBoxExportCustomsClearanceStatus", true)[0] as ZTextBox;

					var consignment = bookingHeader.Consignments.AddNew();
					consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
					consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.UnitedStates;

					CombineAssertions(() =>
					{
						AssertEquals("Export status should be visible", true, exportStatusTextBox.Visible);
						AssertEquals("Import status should not be Visible", false, importStatusTextBox.Visible);
					});
				}
			}
		}

		public void TestCustomClearanceStatusTextBox_WithImportConsignment_ShowsImportStatus()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var importStatusTextBox = form.Controls.Find("zTextBoxImportCustomsClearanceStatus", true)[0] as ZTextBox;
					var exportStatusTextBox = form.Controls.Find("zTextBoxExportCustomsClearanceStatus", true)[0] as ZTextBox;

					CombineAssertions(() =>
					{
						AssertEquals("Export status should not be visible", false, exportStatusTextBox.Visible);
						AssertEquals("Import status should be Visible", true, importStatusTextBox.Visible);
					});
				}
			}
		}

		public void TestCustomClearanceStatusTextBox_ConsignmentIsNotImportOrExportButHasExportCustomsClearanceStatus_ShowsExportStatus()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var importStatusTextBox = form.Controls.Find("zTextBoxImportCustomsClearanceStatus", true)[0] as ZTextBox;
					var exportStatusTextBox = form.Controls.Find("zTextBoxExportCustomsClearanceStatus", true)[0] as ZTextBox;

					consignment.HVC_ExportCustomsClearanceStatus = "&&&";

					CombineAssertions(() =>
					{
						AssertEquals("Export status should be visible", true, exportStatusTextBox.Visible);
						AssertEquals("Import status should not be Visible", false, importStatusTextBox.Visible);
					});
				}
			}
		}

		public void TestCustomClearanceStatusTextBox_ConsignmentIsNotImportOrExportButHasImportCustomsClearanceStatus_ShowsImportStatus()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!");
				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var importStatusTextBox = form.Controls.Find("zTextBoxImportCustomsClearanceStatus", true)[0] as ZTextBox;
					var exportStatusTextBox = form.Controls.Find("zTextBoxExportCustomsClearanceStatus", true)[0] as ZTextBox;

					consignment.HVC_ImportCustomsClearanceStatus = "&&&";

					CombineAssertions(() =>
					{
						AssertEquals("Export status should not be visible", false, exportStatusTextBox.Visible);
						AssertEquals("Import status should be Visible", true, importStatusTextBox.Visible);
					});
				}
			}
		}

		public void TestCustomClearanceStatusTextBox_DefaultsToShowImportStatus()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();
					AssertEquals("Precondition: ", 0, bookingHeader.ConsignmentsFilteredView.Count);

					var importStatusTextBox = form.Controls.Find("zTextBoxImportCustomsClearanceStatus", true)[0] as ZTextBox;
					var exportStatusTextBox = form.Controls.Find("zTextBoxExportCustomsClearanceStatus", true)[0] as ZTextBox;

					CombineAssertions(() =>
					{
						AssertEquals("Import status should be visible", true, importStatusTextBox.Visible);
						AssertEquals("Export status should not be visible", false, exportStatusTextBox.Visible);
					});
				}
			}
		}

		public void TestImportCustomsClearanceStatusIsDisplayed_WhenShipmentIsImport()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				AssertEquals("Precondition: ", true, consignment.IsImport);

				var exportTextBox = (ZTextBox)form.Controls.Find("zTextBoxExportCustomsClearanceStatus", true)[0];
				var importTextBox = (ZTextBox)form.Controls.Find("zTextBoxImportCustomsClearanceStatus", true)[0];

				CombineAssertions(() =>
				{
					AssertEquals("The export customs clearance status should not be displayed", false, exportTextBox.Visible);
					AssertEquals("The import customs clearance status should be displayed", true, importTextBox.Visible);
				});
			}
		}

		public void TestExportCustomsClearanceStatusIsDisplayed_WhenShipmentIsExport()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				AssertEquals("Precondition: ", true, consignment.IsExport);

				var exportTextBox = (ZTextBox)form.Controls.Find("zTextBoxExportCustomsClearanceStatus", true)[0];
				var importTextBox = (ZTextBox)form.Controls.Find("zTextBoxImportCustomsClearanceStatus", true)[0];

				CombineAssertions(() =>
				{
					AssertEquals("The export customs clearance status should be displayed", true, exportTextBox.Visible);
					AssertEquals("The import customs clearance status should not be displayed", false, importTextBox.Visible);
				});
			}
		}

		public void TestImportCustomsClearanceStatusIsDisplayed_WhenShipmentDestinationAndOriginDoesNotMatchCurrentCompany()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Japan;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var exportTextBox = (ZTextBox)form.Controls.Find("zTextBoxExportCustomsClearanceStatus", true)[0];
				var importTextBox = (ZTextBox)form.Controls.Find("zTextBoxImportCustomsClearanceStatus", true)[0];

				CombineAssertions(() =>
				{
					AssertEquals("The export customs clearance status should not be displayed", false, exportTextBox.Visible);
					AssertEquals("The import customs clearance status should be displayed", true, importTextBox.Visible);
				});
			}
		}

		public void TestDetailTestCustomsStatusDetailButtonClick_ForExportConsignment_ShowsExportStatusMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.UnitedStates;

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;
					var detailButton = groupBox.Controls.Find("zButtonStatusDetails", true)[0] as ZButton;

					consignment.HVC_ExportCustomsClearanceStatus = "&&&";

					detailButton.PerformClick();

					AssertContains(consignment.ExportCustomsClearanceStatusDescription, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("Export Customs clearance status:", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCustomsStatusDetailButtonClick_ForExportConsignment_WithMissingExportCustomsClearanceStatus_ShowsExportStatusEmptyMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.UnitedStates;

				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;
					var detailButton = groupBox.Controls.Find("zButtonStatusDetails", true)[0] as ZButton;

					detailButton.PerformClick();

					AssertContains("Export Customs clearance status is empty.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDetailTestCustomsStatusDetailButtonClick_ForImportConsignment_ShowsImportStatusMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!");
				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;
					var detailButton = groupBox.Controls.Find("zButtonStatusDetails", true)[0] as ZButton;

					consignment.HVC_ImportCustomsClearanceStatus = "&&&";

					detailButton.PerformClick();

					AssertContains(consignment.ImportCustomsClearanceStatusDescription, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("Import Customs clearance status: ", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCustomsStatusDetailButtonClick_ForImportConsignment_WithMissingImportCustomsClearanceStatus_ShowsImportStatusEmptyMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!");
				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;
					var detailButton = groupBox.Controls.Find("zButtonStatusDetails", true)[0] as ZButton;

					consignment.HVC_ImportCustomsClearanceStatus = "&&&";

					detailButton.PerformClick();

					AssertContains(consignment.ImportCustomsClearanceStatusDescription, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("Import Customs clearance status: ", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCustomsStatusDetailButtonClick_WhenNoConsignments_DoesNotShowAMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;
					var detailButton = groupBox.Controls.Find("zButtonStatusDetails", true)[0] as ZButton;

					detailButton.PerformClick();

					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPaymentTermDisplay_FreightCollect()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_INCO = IncoTerms.FreeOnBoard;
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var label = form.Controls.Find("zLabelPaymentTermDisplay", true)[0] as ZLabel;
				AssertEquals("Freight Collect", label.Text);
			}
		}

		public void TestPaymentTermDisplay_FreightPrepaid()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_INCO = IncoTerms.DeliveredAtPlace;
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var label = form.Controls.Find("zLabelPaymentTermDisplay", true)[0] as ZLabel;
				AssertEquals("Freight Prepaid", label.Text);
			}
		}

		public void TestACASTabPageVisibility_UsesShipmentOriginAndDestination()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertEquals("pre condition", false, consignment.RequiresACAS);

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var result = form.Controls.Find("tabPageACAS", true);
				AssertEquals("Should find no ACAS tab page", 0, result.Length);
			}

			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.HVC_RN_NKConsigneeCountryCode = "US";

			AssertEquals(true, consignment.RequiresACAS);

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
				AssertEquals("ACAS tab page should be visible", true, tabPage.TabVisible);
			}

			consignment.HVC_RN_NKConsigneeCountryCode = "NZ";

			AssertEquals(true, consignment.RequiresACAS);

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
				AssertEquals("ACAS tab page should be visible", true, tabPage.TabVisible);
			}

			shipment.JS_RL_NKDestination = "NZAKL";

			AssertEquals(false, consignment.RequiresACAS);

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var result = form.Controls.Find("tabPageACAS", true);
				AssertEquals("Should find no ACAS tab page", 0, result.Length);
			}
		}

		public void TestACASTabPageVisibility_ShowForNonUSToUSShipmentRegardlessOfBranch()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertEquals("pre condition", false, consignment.RequiresACAS);

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var result = form.Controls.Find("tabPageACAS", true);
				AssertEquals("Should find no ACAS tab page", 0, result.Length);
			}

			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			AssertEquals(true, consignment.RequiresACAS);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.UnitedStates))
			{
				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
					AssertEquals("ACAS tab page should be visible", true, tabPage.TabVisible);
				}
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				using (var form = new ConsignmentUserControlTestForm(bookingHeader))
				{
					form.Show();

					var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
					AssertEquals("ACAS tab page should be visible", true, tabPage.TabVisible);
				}
			}
		}

		public void TestACASTabPageVisibility_DoesNotShowForUSDomesticShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertEquals("pre condition", false, consignment.RequiresACAS);

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var result = form.Controls.Find("tabPageACAS", true);
				AssertEquals("Should find no ACAS tab page", 0, result.Length);
			}

			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "USLAX";

			AssertEquals(false, consignment.RequiresACAS);

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var result = form.Controls.Find("tabPageACAS", true);
				AssertEquals("Should find no ACAS tab page", 0, result.Length);
			}
		}

		public void TestFlowLayoutPanel_DockModeIsFill()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var flowLayoutPanel = form.Controls.Find("FlowLayoutPanel1", true)[0] as KFlowLayoutPanel;
				AssertEquals("Dock mode for FlowLayoutPanel is fill.", DockStyle.Fill, flowLayoutPanel.Dock);
			}
		}

		public void TestDensityVisualisationControlDisplayed()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var densityVisualisationControl = form.Controls.Find("densityVisualisationControl", true)[0] as DensityVisualisationControl;

				AssertNotNull("Density Visualisation Control", densityVisualisationControl);
				Assert("Density Visualisation Control should be visible", densityVisualisationControl.Visible);
			}
		}

		public void TestDestinationDepotIsZAddressControl()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var destinationDepotAddressControl = form.Controls.Find("zAddressControlDestinationDepot", true)[0];
				AssertType(typeof(ZAddressControl), destinationDepotAddressControl);
			}
		}

		public void TestConsignmentTabPageVisualisationControlDisplayed()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			Factory.Save();

			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();
				var tabReferenceNumbers = form.Controls.Find("tabReferenceNumbers", true)[0] as ZTabPage;
				AssertEquals("tabReferenceNumbers should be visible", true, tabReferenceNumbers.TabVisible);
			}
		}

		public void TestConsigneeOrganisationCodeIsNotEmptyWhenReloadingForm()
		{
			var factory = NewFactory();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_OH = consigneeOrg.PK;
			consigneeAddress.OA_Code = "CONSIGNEE";
			consigneeOrg.OH_Code = "ABC";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_GoodsDescription = "test";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "ABC";
			staff.GS_IsController = true;
			Factory.Save();

			var shipmentInNewFactory = factory.Load<ForwardingShipment>(shipment.PK);
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);

			using (new TemporaryUserContext { StaffLoginName = staff.GS_LoginName }.Set())
			using (var form = new ShipmentForm(shipmentInNewFactory))
			{
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.SelectTabPage();

				var shipmentUserControl = plugin.UserControl as HVLVShipmentUserControl;
				var consignmentDetails = shipmentUserControl.Controls.Find("consignmentDetailsUserControl", true).Single() as HVLVConsignmentDetailsUserControl;
				var consignee = consignmentDetails.Controls.Find("zAddressConsignee", true)[0] as ZAddressControl;
				var findBox = consignee.Controls.Find("OrganisationFindBox", true)[0];
				var codeBox = findBox.Controls.Find("CodeBox", true)[0];

				Assert(consignee.Visible);
				AssertNotNullOrEmpty(codeBox.Text);
				AssertEquals(consigneeOrg.OH_Code, codeBox.Text);
			}
		}

		public void TestDeniedPartyScreeningStatusDropEditDisplayed()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var zDropEditDeniedPartyScreeningStatus = form.Controls.Find("zDropEditDeniedPartyScreeningStatus", true)[0] as DeniedPartyScreeningStatusDropEdit;

				AssertNotNull("ZDropEdit is not null", zDropEditDeniedPartyScreeningStatus);
				Assert("ZDropEdit is visible", zDropEditDeniedPartyScreeningStatus.Visible);
			}
		}

		public void TestGoodsValueInLocalCurrencyTextBoxDisplayed()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var groupBox = (ZGroupBox)form.Controls.Find("groupDetails", true).First();
				var zTextBoxGoodsValueInLocalCurrency = (ZTextBox)groupBox.Controls.Find("zTextBoxGoodsValueInLocalCurrency", true).First();

				AssertNotNull("ZTextBox is not null", zTextBoxGoodsValueInLocalCurrency);
				Assert("ZTextBox is visible", zTextBoxGoodsValueInLocalCurrency.Visible);
			}
		}

		public void TestLocalCurrencyCodeTextBoxDisplayed()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var groupBox = (ZGroupBox)form.Controls.Find("groupDetails", true).First();
				var zTextBoxLocalCurrency = (ZTextBox)groupBox.Controls.Find("zTextBoxLocalCurrency", true).First();

				AssertNotNull("ZTextBox is not null", zTextBoxLocalCurrency);
				Assert("ZTextBox is visible", zTextBoxLocalCurrency.Visible);
			}
		}
	}
}
