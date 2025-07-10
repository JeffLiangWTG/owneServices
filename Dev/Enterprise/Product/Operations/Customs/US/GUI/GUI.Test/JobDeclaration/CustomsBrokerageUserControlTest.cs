using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;
using TransportTypeList = Enterprise.Customs.US.Business.TransportTypeList;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
			{
				AssertEquals("MessagesTabPage.Text", "Messages", userControl.MessagesTabPage.Text);
			}
		}

		public void TestITBondIsNotShownForExportDeclarationLinkedToShipment()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = ZBool.False;
			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentForm form = new ShipmentForm(shipment))
			{
				form.PlugInIDToSelectOnLoaded = ZArchitecture.Modules.ControllerIDs.Customs.JobDeclarationPluggedIntoShipment;
				form.Show();
				ZTabControl mainTab = form.Controls[0] as ZTabControl;
				AssertNotNull("form.Controls[0] is a MainTabControl", mainTab);
				ZPlugIn plugIn = form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				AssertNotNull("Brokerage plugIn exists", plugIn);
				mainTab.SelectedTab = plugIn.TabPage;
				CustomsBrokerageUserControl control = plugIn.UserControl as CustomsBrokerageUserControl;
				UserIdleWorker.Flush();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestInvoiceHeaderCreation_CS00133873()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				AssertEquals("PreCondition: No Invoice", 0, declaration.Invoices.Count);
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.LoadInvoiceLinesTabPage(); //This can happen while binding after users save a form and if we create here just because binding happens, the newly created invoice never gets a chance to be saved to the db unless users do another saving
				AssertEquals("Should not create an invoice just because invoice line tab is loaded", 0, declaration.Invoices.Count);
				form.FireSaveButton();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(1, declaration.Invoices.Count);
				Assert(declaration.HasChanges); //users are forced to save
			}
		}

		// WI00009857
		public void TestITBondIsNotShownForNewExportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var userControl = new CustomsBrokerageUserControlTestClass())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals("InvoiceGroupingTabPage.TabRelevant", false, userControl.InvoiceGroupingTabPage.TabRelevant);
				AssertEquals("MiscOptionsTabPage.TabRelevant", true, userControl.MiscOptionsTabPage.TabRelevant);
			}
		}

		public void TestGetMessageUserControl()
		{
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControlTestClass userControl = new CustomsBrokerageUserControlTestClass())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				userControl.MainTabControl.SelectedTab = userControl.MessagesTabPage;
				AssertEquals("Message Tab should be default", typeof(ImportMessagesUserControl), userControl.MessageUserControl.GetType());
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				userControl.MainTabControl.SelectedTab = userControl.MessagesTabPage;
				AssertEquals(typeof(AESMessageUserControl), userControl.MessageUserControl.GetType());
			}
		}

		public void TestInvoiceHeaderUserControl()
		{
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				userControl.MainTabControl.SelectedTab = userControl.InvoicesTabPage;
				AssertEquals("Import Invoice Header is expected", typeof(USImportSupplierHeaderUserControl), userControl.SupplierHeaderUserControl.GetType());
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				userControl.MainTabControl.SelectedTab = userControl.InvoicesTabPage;
				AssertEquals("Export Invoice Header is expected", typeof(USExportSupplierHeaderUserControl), userControl.SupplierHeaderUserControl.GetType());
			}
		}

		public void TestInvoiceLineUserControl()
		{
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;
				AssertEquals("ACS Import Invoice Line is expected", typeof(USACSImportInvoiceLineUserControl), userControl.InvoiceLinesUserControl.GetType());
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;
				AssertEquals("ACE Import Invoice Line is expected", typeof(USACEImportInvoiceLineUserControl), userControl.InvoiceLinesUserControl.GetType());
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;
				AssertEquals("Export Invoice Line is expected", typeof(USExportInvoiceLineUserControl), userControl.InvoiceLinesUserControl.GetType());
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;
				AssertEquals("ACE Import Invoice Line is expected", typeof(USACEImportInvoiceLineUserControl), userControl.InvoiceLinesUserControl.GetType());
			}
		}

		public void TestDeclarationUserControl()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				AssertEquals("Normal declaration control is expected", typeof(USJobDeclarationUserControl), userControl.DeclarationUserControl.GetType());
			}
		}

		public void TestContainerUserControl()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControlTestClass customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				customsBrokerageUserControl.JobDeclaration = Declaration;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
				AssertNotNull(customsBrokerageUserControl.MainTabControl.TabPages["ContainerTabPage"]);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Declaration.JE_TransportMode = Declaration.TransportModeRailCodeForTesting;
				AssertNull(customsBrokerageUserControl.MainTabControl.TabPages["ContainerTabPage"]);
				using (Control containerUserControl = customsBrokerageUserControl.GetContainerUserControlInternal())
				{
					Assert(containerUserControl is ContainerUserControl);
				}
			}
		}

		public void TestMiscOptionsUserControl()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			using (var form = new ZForm(Declaration))
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				customsBrokerageUserControl.JobDeclaration = Declaration;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
				AssertNotNull(customsBrokerageUserControl.MainTabControl.TabPages["MiscOptionsTabPage"]);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Declaration.JE_TransportMode = Declaration.TransportModeRailCodeForTesting;
				AssertNotNull(customsBrokerageUserControl.MainTabControl.TabPages["MiscOptionsTabPage"]);
				using (var miscOptionsUserControl = customsBrokerageUserControl.GetMiscOptionsUserControlInternal())
				{
					Assert(miscOptionsUserControl is MiscOptionsUserControl);
				}
			}
		}

		public void TestGroupInvoiceUserControl()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControlTestClass customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				customsBrokerageUserControl.JobDeclaration = Declaration;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
				AssertNotNull(customsBrokerageUserControl.MainTabControl.TabPages["InvoiceGroupingTabPage"]);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertNull(customsBrokerageUserControl.MainTabControl.TabPages["InvoiceGroupingTabPage"]);
				using (Control groupInvoiceUserControl = customsBrokerageUserControl.GetInvoiceGroupingUserControlInternal())
				{
					Assert(groupInvoiceUserControl is GroupInvoiceUserControl);
				}
			}
		}

		public void TestPackingUserControl()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControlTestClass customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				customsBrokerageUserControl.JobDeclaration = Declaration;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
				AssertNotNull(customsBrokerageUserControl.MainTabControl.TabPages["PackingTabPage"]);
				Declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
				AssertNull(customsBrokerageUserControl.MainTabControl.TabPages["PackingTabPage"]);
				Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
				AssertNotNull(customsBrokerageUserControl.MainTabControl.TabPages["PackingTabPage"]);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertNull(customsBrokerageUserControl.MainTabControl.TabPages["PackingTabPage"]);
				using (Control packingUserControl = (Control)customsBrokerageUserControl.GetPackingUserControlInternal())
				{
					Assert(packingUserControl is Customs.GUI.BasePackingControl);
				}
			}
		}

		public void TestWHSPackTabVisibility()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.CompanyData.OB_IMUsedBondedWhs = true;
			var org2 = Factory.New<OrgHeader>();
			org2.CompanyData.OB_IMUsedBondedWhs = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			using (var form = new ZForm(declaration))
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				customsBrokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
				Assert(customsBrokerageUserControl.MainTabControl.TabPages.Contains(customsBrokerageUserControl.WHSPacksTabPage));
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
				Assert(!customsBrokerageUserControl.MainTabControl.TabPages.Contains(customsBrokerageUserControl.WHSPacksTabPage));
				declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
				Assert(customsBrokerageUserControl.MainTabControl.TabPages.Contains(customsBrokerageUserControl.WHSPacksTabPage));
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert(!customsBrokerageUserControl.MainTabControl.TabPages.Contains(customsBrokerageUserControl.WHSPacksTabPage));
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
				declaration.US_EnableENS = true;
				Assert(customsBrokerageUserControl.MainTabControl.TabPages.Contains(customsBrokerageUserControl.WHSPacksTabPage));
				declaration.JE_OH_Importer = org2.PK;
				Assert(!customsBrokerageUserControl.MainTabControl.TabPages.Contains(customsBrokerageUserControl.WHSPacksTabPage));
				declaration.JE_OH_Importer = org1.PK;
				Assert(customsBrokerageUserControl.MainTabControl.TabPages.Contains(customsBrokerageUserControl.WHSPacksTabPage));
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			using (var form = new ZForm(declaration))
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				customsBrokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
				Assert(customsBrokerageUserControl.MainTabControl.TabPages.Contains(customsBrokerageUserControl.WHSPacksTabPage));
			}
		}

		[ExpectNoExceptions]
		public void TestLoadingWithNullDeclaration()
		{
			using (var form = new ZForm(null))
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControl())
			{
				customsBrokerageUserControl.JobDeclaration = null;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
			}
		}

		public void TestTabPageOrder_Export_Containerised()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportModeCodes.Codes.VesselContainer;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControlTestClass customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				customsBrokerageUserControl.JobDeclaration = Declaration;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
				var mainTabControl = customsBrokerageUserControl.MainTabControl;
				CombineAssertions(() =>
				{
					AssertEquals("ContainerTabPage", customsBrokerageUserControl.ContainerTabPage, mainTabControl.TabPages[1]);
					AssertEquals("PickupTabPage", customsBrokerageUserControl.PickupTabPage, mainTabControl.TabPages[2]);
					AssertEquals("InvoicesTabPage", customsBrokerageUserControl.InvoicesTabPage, mainTabControl.TabPages[3]);
					AssertEquals("InvoiceLinesTabPage", customsBrokerageUserControl.InvoiceLinesTabPage, mainTabControl.TabPages[4]);
					AssertEquals("MiscOptionsTabPage", customsBrokerageUserControl.MiscOptionsTabPage, mainTabControl.TabPages[5]);
					AssertEquals("MessagesTabPage", customsBrokerageUserControl.MessagesTabPage, mainTabControl.TabPages[6]);
				});
			}
		}

		public void TestTabPageOrder_Import_Containerised()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_OH_Importer = org.PK;
			Declaration.JE_TransportMode = TransportModeCodes.Codes.VesselContainer;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			Declaration.US_EnableENS = true;
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			using (ZForm form = new ZForm(Declaration))
			using (CustomsBrokerageUserControlTestClass customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				customsBrokerageUserControl.JobDeclaration = Declaration;
				form.Controls.Add(customsBrokerageUserControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("ContainerTabPage", customsBrokerageUserControl.ContainerTabPage, customsBrokerageUserControl.MainTabControl.TabPages[1]);
					AssertEquals("PackingTabPage", customsBrokerageUserControl.PackingTabPage, customsBrokerageUserControl.MainTabControl.TabPages[2]);
					AssertEquals("DeliveryTabPage", customsBrokerageUserControl.DeliveryTabPage, customsBrokerageUserControl.MainTabControl.TabPages[3]);
					AssertEquals("InvoiceGroupingTabPage", customsBrokerageUserControl.InvoiceGroupingTabPage, customsBrokerageUserControl.MainTabControl.TabPages[4]);
					AssertEquals("InvoicesTabPage", customsBrokerageUserControl.InvoicesTabPage, customsBrokerageUserControl.MainTabControl.TabPages[5]);
					AssertEquals("InvoiceLinesTabPage", customsBrokerageUserControl.InvoiceLinesTabPage, customsBrokerageUserControl.MainTabControl.TabPages[6]);
					AssertEquals("WHSPacksTabPage", customsBrokerageUserControl.WHSPacksTabPage, customsBrokerageUserControl.MainTabControl.TabPages[7]);
					AssertEquals("MiscOptionsTabPage", customsBrokerageUserControl.MiscOptionsTabPage, customsBrokerageUserControl.MainTabControl.TabPages[8]);
					AssertEquals("StatusTabPage", customsBrokerageUserControl.StatusTabPage, customsBrokerageUserControl.MainTabControl.TabPages[9]);
					AssertEquals("MessagesTabPage", customsBrokerageUserControl.MessagesTabPage, customsBrokerageUserControl.MainTabControl.TabPages[10]);
				});
			}
		}

		public void TestDISFeatures_PromptToMerge()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			OrgHeader consignee = Factory.New<OrgHeader>();
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignor.OH_IsConsignor = true;
			consignee.OH_Code = "AAA";
			consignor.OH_Code = "BBB";
			consignee.OH_FullName = "Consignee/Buyer";
			consignor.OH_FullName = "Consignor/Supplier";
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = TransportTypeList.Codes.Air;
			shipment.DocsAndCartage.RequiredDocuments.AddNew().EQ_DocType = Core.Constants.RefDocTypes.Invoice;

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			Factory.Save();
			Assert("declaration is not merged", !declaration.IsMergeDone);
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.PlugInIDToSelectOnLoaded = ZArchitecture.Modules.ControllerIDs.Customs.JobDeclarationPluggedIntoShipment;
				form.Show();
				var disDataButton = GetDisDataButton(form);
				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				shipment.HasChanges = true;
				disDataButton.PerformClick();
				AssertNull("PromptToMergeForm did not popped up", ZFormModaliser.LastFormShownDialogForTest);
				AssertNull("DIS form did not popped up", ZFormModaliser.LastFormShownForTest);
				Assert("shipment should not be saved", shipment.HasChanges);
				Assert("declaration should not be merged", !declaration.IsMergeDone);
				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				disDataButton.PerformClick();
				AssertEquals("PromptToMergeForm popped up", "Prompt to Merge", ZFormModaliser.LastFormShownDialogForTest.Text);
				AssertEquals("Merge failed", "You can't merge this entry because there are no invoice headers.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("DIS form did not popped up", ZFormModaliser.LastFormShownForTest);
				Assert("shipment should be saved", !shipment.HasChanges);
				Assert("declaration should not be merged", !declaration.IsMergeDone);
				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				shipment.HasChanges = true;
				disDataButton.PerformClick();
				AssertEquals("DIS form popped up", "Document Image System", ZFormModaliser.LastFormShownForTest.Text);
				Assert("shipment should be saved", !shipment.HasChanges);
				Assert("declaration should not be merged", !declaration.IsMergeDone);
			}

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var disDataButton = GetDisDataButton(form);
				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CustomsQuantity = 100;
				shipment.HasChanges = true;
				Assert("shipment has been changed", shipment.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				disDataButton.PerformClick();
				AssertEquals("PromptToMergeForm popped up", "Prompt to Merge", ZFormModaliser.LastFormShownDialogForTest.Text);
				AssertEquals("DIS form popped up", "Document Image System", ZFormModaliser.LastFormShownForTest.Text);
				Assert("declaration should be merged", declaration.IsMergeDone);
				Assert("shipment should be saved", !shipment.HasChanges);
			}

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var disDataButton = GetDisDataButton(form);
				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Invoices[0].InvoiceLines.AddNew().JI_CustomsQuantity = 100;
				shipment.HasChanges = true;
				Assert("shipment has been changed", declaration.HasChanges);
				Assert("declaration require merge", declaration.MergeManager.RequiresMerge);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				disDataButton.PerformClick();
				AssertNull("PromptToMergeForm did not popped up", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("DIS form popped up", "Document Image System", ZFormModaliser.LastFormShownForTest.Text);
				Assert("declaration should not require merge", !declaration.MergeManager.RequiresMerge);
				Assert("shipment should be saved", !shipment.HasChanges);
			}

			ZFormModaliser.LastFormShownForTest = null;
			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		ZButton GetDisDataButton(ShipmentForm form)
		{
			var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
			form.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);
			var requiredDocumentUserControl = JobDeclarationFormBaseOnlyTest.FindMatchingControl(plugIn.UserControl, x => x.GetType() == typeof(MasterFiles.GUI.RequiredDocumentsUserControl));
			var disDataButton = (ZButton)JobDeclarationFormBaseOnlyTest.FindMatchingControl(requiredDocumentUserControl, x => x is ZButton && x.Name == "DISDataButton");
			var control = form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration).UserControl as CustomsBrokerageUserControl;
			control.MainTabControl.SelectedTab = control.DeclarationTabPage;
			form.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);
			return disDataButton;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.US_EntryFilerCode = "XJS";
				}

				return declaration;
			}
		}

		sealed class CustomsBrokerageUserControlTestClass : CustomsBrokerageUserControl
		{
			internal Customs.GUI.BaseMiscOptionsUserControl GetMiscOptionsUserControlInternal() => GetMiscOptionsUserControl();

			internal Customs.GUI.BaseCustomsCusContainersUserControl GetContainerUserControlInternal() => GetContainerUserControl();

			internal Customs.GUI.IBasePackingControl GetPackingUserControlInternal() => GetPackingUserControl();

			internal Customs.GUI.BaseInvoiceGroupingUserControl GetInvoiceGroupingUserControlInternal() => GetInvoiceGroupingUserControl();
		}
	}
}
