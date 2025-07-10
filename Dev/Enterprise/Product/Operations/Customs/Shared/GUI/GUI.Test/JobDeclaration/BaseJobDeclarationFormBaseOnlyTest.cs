using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DeniedPartyScreening.GUI.Test;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseJobDeclarationFormBaseOnlyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSaveWithoutExceptionRegardingBindingOnSaving()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			using (BaseJobDeclarationFormForTest theForm = new BaseJobDeclarationFormForTest(testDec))
			{
				theForm.Show();
				theForm.FireHandleSave();
			}
		}

		public void TestJE_VoyageFlightNoBoundTextBoxCaption()
		{
			// Test against country that doesn't use form layout for TransportDetailsGroupBox
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				using (var form = new BaseJobDeclarationForm(dec))
				{
					form.Show();
					var userControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
					var labelCaptionRenderer = userControl.JE_VoyageFlightNoBoundTextBox.GetExtension<ZLabelCaptionRenderer>();
					dec.JE_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("JE_VoyageFlightNoBoundTextBox - Road", "Registration", labelCaptionRenderer.Caption);
					dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("JE_VoyageFlightNoBoundTextBox - Sea", "Voyage", labelCaptionRenderer.Caption);
					dec.JE_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("JE_VoyageFlightNoBoundTextBox - Air", "Flight Number", labelCaptionRenderer.Caption);
				}
			}
		}

		public void TestApportionIfDirtyWhenSaving()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			using (BaseJobDeclarationFormForTest theForm = new BaseJobDeclarationFormForTest(testDec))
			{
				theForm.Show();
				testDec.ApportionmentDirty = true;
				AssertEquals("Apportionment is pending", true, testDec.ApportionmentDirty);
				theForm.FireShowPreSaveDialog();
				AssertEquals("Apportionment is not dirty any more", false, testDec.ApportionmentDirty);
			}
		}

		public void TestSecurityCheckOnShowPreSaveDialogs()
		{
			var expectedMessage = "You do not have security rights to save an Import Customs job. ";
			expectedMessage += Env.Security.ImportEdit.DisplayTextPathToSecurityRight;

			var declaration = BaseJobDeclaration.New(Factory);
			Env.Security.ImportEdit.IsAllowed = false;

			using (BaseJobDeclarationFormForTest form = new BaseJobDeclarationFormForTest(declaration))
			{
				form.Show();

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireShowPreSaveDialog();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.ImportEdit.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireShowPreSaveDialog();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProductClassificationCreationConfirmationFormCheckOnShowPreSaveDialogs()
		{
			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = "SUPPLIER";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";

			var part = Factory.New<Business.OrgSupplierPart>();
			part.OP_PartNum = "SomePart";
			part.OP_Desc = "Description of product";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;

			var relOrg = part.RelatedOrganisations.AddNew();
			relOrg.OU_OH = declarationSupplier.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = declarationSupplier.PK;

			var invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_PartNo = part.OP_PartNum;
			invLine.JI_Tariff = "123";

			Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed = true;

			using (BaseJobDeclarationFormForTest form = new BaseJobDeclarationFormForTest(declaration))
			{
				form.Show();
				form.FireShowPreSaveDialog();
				AssertEquals("ProductClassificationCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
			}
		}

		public void TestBondedWarehouseSecurityCheckOnShowPreSaveDialogs()
		{
			var expectedMessage = "You do not have security rights to save a Bonded Warehousing Customs job. ";
			expectedMessage += Env.Security.ImportEditBondedWarehouse.DisplayTextPathToSecurityRight;

			var declaration = Factory.New<BaseJobDeclaration>();
			Env.Security.ImportEditBondedWarehouse.IsAllowed = false;

			using (var form = new BaseJobDeclarationFormForTest(declaration))
			{
				form.Show();

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireShowPreSaveDialog();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.ImportEditBondedWarehouse.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireShowPreSaveDialog();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.ImportEditBondedWarehouse.IsAllowed = false;
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireShowPreSaveDialog();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestScreenPromptOnlyOnce()
		{
			var message = ((UnitTestUserNotification)Globals.Message);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			_ = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			using (BaseJobDeclarationFormForTest testForm = new BaseJobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();

				message.ClearMessagesAndAnswers();
				testForm.FireShowPreSaveDialog();

				AssertEquals(2, message.PreviousMessages.Length);
				AssertEquals("You can't merge this entry because there is an invoice header with no invoice lines.", message.PreviousMessages[0].Text);
				AssertEquals(null, message.PreviousMessages[1].Text);
			}
		}

		public void TestShowInvoiceLinesAreNotLinkedToTheContainerDialog()
		{
			UnitTestUserNotification message = ((UnitTestUserNotification)Globals.Message);
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BaseJobComInvoiceLine invLine = invoice.InvoiceLines.AddNew();
			using (BaseJobDeclarationFormForTest testForm = new BaseJobDeclarationFormForTest(declaration))
			{
				invLine.JI_ContainerMode = Core.Constants.ContainerModes.NonContainerised;

				testForm.ShowInvoiceLinesAreNotLinkedToTheContainerDialog_Exposed();
				AssertEquals("Dialog shouldn't be called when invoice line not containerised", true, message.LastMessage.WasNone);

				invLine.JI_ContainerMode = Core.Constants.ContainerModes.Containerised;
				invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				testForm.ShowInvoiceLinesAreNotLinkedToTheContainerDialog_Exposed();
				AssertEquals("Dialog shouldn't be called when invoice line containerised and is linked to container", true, message.LastMessage.WasNone);

				invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
				message.AddAnswer(DialogResult.Yes);
				testForm.ShowInvoiceLinesAreNotLinkedToTheContainerDialog_Exposed();
				AssertEquals("Dialog should be called when invoice line containerised but not linked to container", true, message.LastMessage.WasQuestion);
				AssertEquals("Invoice Line should be linked to container when DialogResult.Yes", true, invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

				message.ClearMessagesAndAnswers();
				invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
				message.AddAnswer(DialogResult.No);
				testForm.ShowInvoiceLinesAreNotLinkedToTheContainerDialog_Exposed();
				AssertEquals("Dialog should be called when invoice line containerised but not linked to container", true, message.LastMessage.WasQuestion);
				AssertEquals("Invoice Line shouldn't be linked to container when DialogResult.No", false, invLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

				message.ClearMessagesAndAnswers();
				BaseCusContainer container2 = declaration.CusContainers.AddNew();
				testForm.ShowInvoiceLinesAreNotLinkedToTheContainerDialog_Exposed();
				AssertEquals("Dialog shouldn't be called when container count not equal 1", true, message.LastMessage.WasNone);
			}
		}

		public void TestScreeningLogsTabPage()
		{
			AssertScreeningLogsTabPage(false);
			AssertScreeningLogsTabPage(true);

			void AssertScreeningLogsTabPage(bool enableCompliance)
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var testForm = new BaseJobDeclarationFormForTest(declaration))
				{
					var tabControl = (ZTabControl)testForm.CustomsBrokerageUserControl.EventTabPage.Controls[0].Controls[0];
					AssertEquals(2, tabControl.TabPages.Count);
					if (enableCompliance)
					{
						AssertEquals("Compliance Logs", tabControl.TabPages[1].Text);
						AssertEquals(typeof(Enterprise.ComplianceRisk.GUI.ComplianceLogUserControl), tabControl.TabPages[1].Controls[0].GetType());
					}
					else
					{
						AssertEquals("Denied Party Screening Logs", tabControl.TabPages[1].Text);
						AssertEquals(typeof(DeniedPartyScreening.GUI.RelatedDeniedPartyScreeningStatusControl), tabControl.TabPages[1].Controls[0].GetType());
					}
				}
			}
		}

		public void TestHasDeniedPartyActionMenu()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new BaseJobDeclarationFormForTest(declaration))
			{
				AssertHasDeniedPartyActionMenu(form, true);
			}
		}

		public void TestPackingDetailsIsAvailable()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("brokerageControl.PackingTabPage.TabVisible should be " + declaration.IsPackingInformationRelevant.ToString() + " base on declaration.IsPackingInformationRelevant for Import", declaration.IsPackingInformationRelevant, brokerageControl.PackingTabPage.TabVisible);
				declaration.JE_MessageType = ZString.Empty;
				AssertEquals("brokerageControl.PackingTabPage.TabVisible should be " + declaration.IsPackingInformationRelevant.ToString() + " base on declaration.IsPackingInformationRelevant for empty.", declaration.IsPackingInformationRelevant, brokerageControl.PackingTabPage.TabVisible);
			}
		}

		public void TestResynchronizeScreeningStatus_WhenStatusIsJbeOrJce_ShouldNotChangeStatusOnFormLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			declaration.JE_OH_Consignee = consignee.PK;
			Factory.Save();

			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobBlockedExternal;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, declaration.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.JobBlockedExternal, declaration.JE_ScreeningStatus);
			});

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.JobBlockedExternal, declaration.JE_ScreeningStatus);
				});
			}

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobClearedExternal;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.JobClearedExternal, declaration.JE_ScreeningStatus);

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.JobClearedExternal, declaration.JE_ScreeningStatus);
				});
			}
		}

		public void TestResynchronizeScreeningStatus_WhenStatusIsJbeOrJce_ShouldNotChangeStatusOnResynchronizeMenuClick()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var manager = new DeniedPartyScreeningPresentationManager();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(declaration))
			{
				manager.CreateMenusForJob(form);
				form.Show();
				var menu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Resynchronize Screening Status");
				AssertNotNull(menu);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.OrgDeniedPartyScreeningAllowResynchronize.IsAllowed = true;

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobBlockedExternal;
				Factory.Save();
				menu.PerformClick();
				AssertEquals($"{declaration.Description} is Job Blocked Externally, no need for resynchronization.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobClearedExternal;
				Factory.Save();
				menu.PerformClick();
				AssertEquals($"{declaration.Description} is Job Cleared Externally, no need for resynchronization.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				Factory.Save();
				menu.PerformClick();
				AssertEquals("Resynchronize to CLR", ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
			}
		}

		public void TestResynchronizeScreeningStatusOnFormLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			declaration.JE_OH_Consignee = consignee.PK;
			Factory.Save();

			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, declaration.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
			});

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
					AssertEquals(1, declaration.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Count());
					AssertContains("|NEW=CLR|OLD=MAT|TYP=SYNC", declaration.Logs.MostRecentLogByEventTime(ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated).SL_Reference);
				});
			}

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				AssertEquals(ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
				AssertEquals("The number of logs is still 1.", 1, declaration.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Count());
			}
		}

		public void TestResynchronizeScreeningStatusOnFormLoad_NoDeveloperNotificationExceptionThrown()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			declaration.JE_OH_Supplier = consignor.PK;
			Factory.Save();

			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				using (TestingState.SuspendIsRunningTests())
				using (var form = new BaseJobDeclarationForm(declaration))
				{
					form.Show();
					AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
					AssertEquals("Declaration screening status CLR.", ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
				}
			});
		}

		public void TestNoNeedToResynchronizeScreeningStatusOnFormLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			declaration.JE_OH_Consignee = consignee.PK;
			Factory.Save();

			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, declaration.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
			});

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(false, declaration.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Any());
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
				});
			}
		}

		public void TestJobComInvoiceLinePartSynchronisationManager()
		{
			var factory = new BusinessObjectFactory();
			var importer = factory.NewWithValidTestData<OrgHeader>();
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			var part = factory.New<Business.OrgSupplierPart>();
			part.RelatedOrganisations.AddOwner(importer);
			part.RelatedOrganisations.AddSupplier(supplier);
			part.OP_PartNum = "P001";
			part.OP_Desc = "DESC";
			factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(part.OP_Desc, invoiceLine.JI_Description);

			part.OP_Desc = "DESC1";
			factory.Save();
			AssertNotEquals(part.OP_Desc, invoiceLine.JI_Description);

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();

				part.OP_Desc = "DESC2";
				factory.Save();
				AssertEquals(part.OP_Desc, invoiceLine.JI_Description);
			}

			part.OP_Desc = "DESC3";
			factory.Save();
			AssertNotEquals(part.OP_Desc, invoiceLine.JI_Description);
		}

		public void TestMarkAsJobClearMenuItemExist()
		{
			AssertMarkAsJobClearMenuItemExist(true);
			AssertMarkAsJobClearMenuItemExist(false);
		}

		public void TestMarkAsJobClearWhenSecurityRightsIsDenied_ShouldErrorMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			var declaration = Factory.New<BaseJobDeclaration>();

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new BaseJobDeclarationFormTestDirector(declaration))
			{
				form.Show();

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				Factory.Save();

				tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = false;

				new DpsMarkJobScreeningStatusClearTest().AssertSecurityRightsAccessibilityCheckpoint(form, tmpSecurityCore);
			}
		}

		public void TestMarkJobClearWhenScreeningStatusIsJCLorCLR_ShouldShowWarningMessage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var declationForm = new BaseJobDeclarationFormTestDirector(declaration))
			{
				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				AssertEquals("Precondition job declaration screening status", "JCL", declaration.JE_ScreeningStatus);
				Factory.Save();

				var markJobClearTestHelper = new DpsMarkJobScreeningStatusClearTest();
				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(declationForm);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Precondition job declaration screening status", "CLR", declaration.JE_ScreeningStatus);
				Factory.Save();

				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(declationForm);
			}
		}

		public void TestMarkJobClearWhenJobIsNotSaved_ShouldShowWarningMessage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var declationForm = new BaseJobDeclarationFormTestDirector(declaration))
			{
				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				AssertEquals("Precondition job declaration screening status", "NOT", declaration.JE_ScreeningStatus);
				new DpsMarkJobScreeningStatusClearTest().AssertSaveBeforeMarkingClear(declationForm);
			}
		}

		public void TestJobShipmentWhenMarkingJobClear_ShouldUpdateJobScreeningStatusToJCL()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var declationForm = new BaseJobDeclarationFormTestDirector(declaration))
			{
				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Precondition job declaration screening status", "MAT", declaration.JE_ScreeningStatus);
				Factory.Save();

				new DpsMarkJobScreeningStatusClearTest().AssertScreeenigStatusToJCL(declationForm, declaration);
			}
		}

		public void TestSendEMailMenu()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (BaseJobDeclarationForm form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				MenuItem item = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == Enterprise.MasterFiles.GUI.EmailSender.GetSendEmailMenuItemTextForTest());
				AssertNotNull(item);
			}
		}

		public void TestActionsMenuContainsLocalCartageJobExport()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			AssertEquals("Precondition: Declaration is Internal Cartage Enabled", true, ((IHaveInternalCartage)declaration).InternalCartageEnabled);
			using (var form = new BaseJobDeclarationFormTestDirector(declaration))
			{
				form.Show();
				var mainMenu = form.MainMenuInternal.MenuItems.FindByText("Port Transport");
				var exportToEmailMenu = mainMenu.MenuItems[0].MenuItems.FindByText("Export Port Transport Booking To XML");
				AssertNotNull("Should contain 'Export Cartage Booking To Xml' menu item", exportToEmailMenu);
				var exportToFileMenu = mainMenu.MenuItems[0].MenuItems.FindByText("Export Port Transport Booking To XML: Store As File");
				AssertNotNull("Should contain 'Export Cartage Booking To Xml' menu item", exportToFileMenu);
			}
		}

		public void TestActionsMenuContainsLocalCartageJobExportForExportDec()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();

			AssertEquals("Precondition: Declaration is not Internal Cartage Enabled", false, ((IHaveInternalCartage)declaration).InternalCartageEnabled);
			using (var form = new BaseJobDeclarationFormTestDirector(declaration))
			{
				form.Show();
				MenuItem mainMenu = form.MainMenuInternal.MenuItems.FindByText("Port Transport");
				MenuItem exportToEmailMenu = mainMenu.MenuItems[0].MenuItems.FindByText("Export Port Transport Booking To XML");
				AssertNotNull("Should contain 'Export Cartage Booking To Xml' menu item", exportToEmailMenu);
				MenuItem exportToFileMenu = mainMenu.MenuItems[0].MenuItems.FindByText("Export Port Transport Booking To XML: Store As File");
				AssertNotNull("Should contain 'Export Cartage Booking To Xml' menu item", exportToFileMenu);
			}
		}

		public void TestActionsMenuContainsDeniedPartyScreening()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				var actionMenu = form.Menu.MenuItems.FindByText("Actio&ns");
				AssertNotNull(actionMenu);

				AssertNotNull(actionMenu.MenuItems.FindByText("View Compliance Status"));
			}
		}

		public void TestAuditMenuItem()
		{
			var declaration = BaseJobDeclaration.New(Factory);

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				Tester.Test(
					(WriteToLogMenuItem)form.TopLevelMenu.MenuItems[form.TopLevelMenu.MenuItems.Count - 1],
					declaration,
					Env.Security.CustomsDeclarationAudit,
					"Audit Customs Declaration");
			}
		}

		public void TestTransportToPrintForm()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				var args = new System.ComponentModel.CancelEventArgs();
				declaration.FireOnGetTransportToPrint(args);
				AssertType<TransportToPrintForm>("Should popup a TransportToPrintForm", ZFormModaliser.LastFormShownDialogForTest);
				Assert("CancelEventArgs.Cancel should be set", args.Cancel);
			}
		}

		public void TestTabVisibilityDeciderPersistence()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				var tabPage = form.CustomsBrokerageUserControl.DeclarationTabPage;
				tabPage.TabVisible = false;
				var tabVisibilityDecider = (ITabVisibilityDeciderPersistence)form;
				Assert("HasTabVisiblePersisted", !tabVisibilityDecider.HasTabVisiblePersisted);
				Assert("RetrieveTabPageVisible", tabVisibilityDecider.RetrieveTabPageVisible(tabPage));

				tabVisibilityDecider.StoreTabVisible(tabPage);
				Assert("HasTabVisiblePersisted", tabVisibilityDecider.HasTabVisiblePersisted);
				AssertEquals("JE_InvisibleTabsXML", "<InvisibleTabs>;DeclarationTabPage;</InvisibleTabs>", declaration.JE_InvisibleTabsXML);
				Assert("RetrieveTabPageVisible", !tabVisibilityDecider.RetrieveTabPageVisible(tabPage));
			}
		}

		public void TestViewMenuItem()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			using (var form = new BaseJobDeclarationFormTestDirector(declaration))
			{
				form.Show();

				ZTabPage tab1 = (ZTabPage)form.TopLevelTabControlInternal.AllTabPages[0];
				ZTabPage tab2 = (ZTabPage)form.TopLevelTabControlInternal.AllTabPages[1];

				AssertEquals(true, tab1.TabVisible);
				AssertEquals(true, tab2.TabVisible);

				MenuItem viewMenu = form.MainMenuInternal.MenuItems.FindByText("View");
				viewMenu.OnPopup(EventArgs.Empty);

				viewMenu.MenuItems[0].PerformClick();
				AssertEquals(false, tab1.TabVisible);
				AssertEquals(true, tab2.TabVisible);

				viewMenu.MenuItems[1].PerformClick();
				AssertEquals(false, tab1.TabVisible);
				AssertEquals(false, tab2.TabVisible);

				viewMenu.MenuItems[0].PerformClick();
				AssertEquals(true, tab1.TabVisible);
				AssertEquals(false, tab2.TabVisible);

				viewMenu.MenuItems[1].PerformClick();
				AssertEquals(true, tab1.TabVisible);
				AssertEquals(true, tab2.TabVisible);
			}
		}

		public void TestEntryInstructionDetailsTabPage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new BaseJobDeclarationFormForTest(declaration))
			{
				form.Show();

				var viewMenu = form.MainMenu.MenuItems.FindByText("View");
				viewMenu.OnPopup(EventArgs.Empty);
				var areMultipleEntryInstructionsAllowed = declaration.AreMultipleEntryInstructionsAllowed && form.CustomsBrokerageUserControl.EntryInstructionsTabVisibleForCountry;
				AssertEquals(areMultipleEntryInstructionsAllowed, viewMenu.MenuItems.FindByText("Entry Instructions") != null);
				AssertEquals(areMultipleEntryInstructionsAllowed, form.fCustomsBrokerageUserControl.EntryInstructionDetailsTabPage.TabRelevant);
				AssertEquals(areMultipleEntryInstructionsAllowed, form.fCustomsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible);
			}
		}

		[TestDate(2006, 9, 28, 12, 0, 0)]
		public void TestExportToXml_DeclarationReferenceInFileName()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DateOfFirstArrival = ZDateTime.Today;
			Factory.Save();
			SystemDataRegistry.Instance.CustomDeclarationExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);

			string expectedFileName = declaration.JE_DeclarationReference + "_" + ZDateTime.Today.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
			using (var form = new BaseJobDeclarationFormTestExporterDirector(declaration))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.Show();
				Application.DoEvents();

				MenuItem exportToXmlMenu = form.ActionsMenuItem.MenuItems.FindByText("Export to XML (Verbose)");
				if (exportToXmlMenu != null)
				{
					exportToXmlMenu.PerformClick();
				}
				AssertEquals("Default File Name", expectedFileName, form.Director.DefaultFileName);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestExportDeclarationToXml()
		{
			using (var form = new BaseJobDeclarationFormTestExporterDirector(Factory.New<BaseJobDeclaration>()))
			{
				form.Show();
				form.Exporter.PromptUserAndExport(new[] { (BaseJobDeclaration)form.BusinessEntity });
				Assert(form.Director.IsExportCalled);
				form.Close();
			}
		}

		public void TestValidateAll_ShouldShowErrorsInPlugIns()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			// Add invalid address
			var address = declaration.DocAddresses.AddNew();
			address.E2_AddressOverride = ZBool.True;
			address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			address.E2_State = "ABC";

			Factory.Save();
			Assert("Precondition: IsInDatabaseIncludingChildren", declaration.IsInDatabaseIncludingChildren);

			using var form = new BaseJobDeclarationFormForTest(declaration);
			form.Show();
			form.FireValidateAll();

			// Allow TabPageNotificationsExposer to perform binding asynchronously
			Application.DoEvents();

			var expectedIndex = Icons.GetImageIndex(IconTypes.Error);
			var jobInvoicingPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			var docAddressesPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses);

			CombineAssertions(() =>
			{
				AssertEquals(ControllerIDs.JobInvoicing.Name, expectedIndex, jobInvoicingPlugIn.TabPage.ImageIndex);
				AssertEquals(ControllerIDs.DocAddresses.Name, expectedIndex, docAddressesPlugIn.TabPage.ImageIndex);
			});
		}

		public void TestAuditPlugin()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using var form = new BaseJobDeclarationFormForTest(declaration);
			Assert("Audit PlugIn should be available", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
		}

		static SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		void AssertMarkAsJobClearMenuItemExist(bool registryValue)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var form = new BaseJobDeclarationFormTestDirector(declaration))
			{
				form.Show();
				new DpsMarkJobScreeningStatusClearTest().AssertMenuItemAccessibilityCheckpoint(form, registryValue);
			}
		}

		static void AssertHasDeniedPartyActionMenu(BaseJobDeclarationForm form, bool visibility)
		{
			var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
			var containsDeniedParty = false;
			foreach (MenuItem item in actionsMenu.MenuItems)
			{
				if (item.Text.Contains("Screen"))
				{
					containsDeniedParty = true;
					break;
				}
			}
			AssertEquals("Actions menu contains 'Denied Party' menu item", visibility, containsDeniedParty);
		}

		#region ComplianceRiskPlugin

		public void TestIncidentDefaultModuleOnComplianceRiskTab()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			using (ObjectFactory.Substitute(featureControlMock.Object))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new BaseJobDeclarationFormTest(declaration))
			{
				form.Show();
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);

				form.MainTabControl.SelectTab("ComplianceRiskTabPage");
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);
			}
		}

		class BaseJobDeclarationFormTest : BaseJobDeclarationForm
		{
			public BaseJobDeclarationFormTest(BaseJobDeclaration bO)
				: base(bO)
			{
			}

			public ZTabControl MainTabControl
			{
				get { return base.TopLevelTabControl; }
			}
		}

		#endregion ComplianceRiskPlugin

		sealed class BaseJobDeclarationFormForTest : BaseJobDeclarationForm
		{
			public BaseJobDeclarationFormForTest(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			public void FireValidateAll() => ValidateAll(ValidationType.Full);

			public void FireHandleSave() => ValidateAndSave();

			public void FireShowPreSaveDialog() => ShowPreSaveDialogs();

			public void ShowInvoiceLinesAreNotLinkedToTheContainerDialog_Exposed() => ShowInvoiceLinesAreNotLinkedToTheContainerDialog();

			public new MainMenu MainMenu => base.MainMenu;
		}

		sealed class BaseJobDeclarationFormTestDirector : BaseJobDeclarationForm
		{
			public BaseJobDeclarationFormTestDirector(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			internal MainMenu MainMenuInternal => MainMenu;
			internal ZTabControl TopLevelTabControlInternal => TopLevelTabControl;

			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}

			public TestDeclarationXmlDataTransferDirector Director = new TestDeclarationXmlDataTransferDirector(DeclarationValueObjectDataAdapter.New(), true);
		}

		sealed class BaseJobDeclarationFormTestExporterDirector : BaseJobDeclarationForm
		{
			public BaseJobDeclarationFormTestExporterDirector(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override DeclarationXmlDataTransferExporter GetNewDeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter adapter)
			{
				return Director;
			}

			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}

			public TestDeclarationXmlDataTransferExporter Director = new TestDeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter.New(), true);
		}

		sealed class TestDeclarationXmlDataTransferDirector : DeclarationXmlDataTransferDirector
		{
			public TestDeclarationXmlDataTransferDirector(DeclarationValueObjectDataAdapter adapter, bool checkLicence)
				: base(adapter, checkLicence)
			{
			}

			protected override void PromptUserAndImportCore(BillingInterfaceName interfaceName)
			{
				IsImportCalled = true;
			}

			public bool IsImportCalled;
		}

		sealed class TestDeclarationXmlDataTransferExporter : DeclarationXmlDataTransferExporter
		{
			public TestDeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter adapter, bool checkLicence)
				: base(adapter, checkLicence)
			{
			}

			protected override void PromptUserAndExportCore(IList selectedElements)
			{
				IsExportCalled = true;
			}
			public bool IsExportCalled;
		}
	}
}
