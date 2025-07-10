using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.Business.OrgSupplierPart;

namespace Enterprise.Customs.GUI.PlugIn.Testing
{
	sealed class BaseBrokeragePlugInBaseOnlyTest : TestCaseWithFactory
	{
		public void TestJobComInvoiceLinePartSynchronisationManager()
		{
			var factory = new BusinessObjectFactory();
			var importer = factory.NewWithValidTestData<OrgHeader>();
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			var part = factory.New<OrgSupplierPart>();
			part.RelatedOrganisations.AddOwner(importer);
			part.RelatedOrganisations.AddSupplier(supplier);
			part.OP_PartNum = "P001";
			part.OP_Desc = "DESC";
			factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(part.OP_Desc, invoiceLine.JI_Description);

			part.OP_Desc = "DESC1";
			factory.Save();
			AssertNotEquals(part.OP_Desc, invoiceLine.JI_Description);

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.ControllerID = ControllerIDs.JobShipment;
				shipmentForm.Show();
				shipmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.Customs.JobDeclaration);

				part.OP_Desc = "DESC2";
				factory.Save();
				AssertEquals(part.OP_Desc, invoiceLine.JI_Description);
			}

			part.OP_Desc = "DESC3";
			factory.Save();
			AssertNotEquals(part.OP_Desc, invoiceLine.JI_Description);
		}

		public void TestTabPage()
		{
			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(Factory.New<ForwardingShipment>()))
			{
				AssertEquals("TabPage", typeof(BrokerageAutoSizedTabPagePlugIn), plugIn.TabPage.GetType());
			}
		}

		public void TestDeclarationDeletingIsReported()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "TST456";
			consignee.OH_IsConsignee = true;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "TST123";
			consignor.OH_IsConsignor = true;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HB1907011132";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;
			Factory.RefreshEnabled = false;
			Factory.Save();
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			ChildEditableService.SetState(newFactory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.ControllerID = ControllerIDs.JobShipment;
				shipmentForm.Show();
				shipmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.Customs.JobDeclaration);
				var plugIn = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclaration);
				var control = plugIn.UserControl as BaseCustomsBrokerageUserControl;
				control.MainTabControl.SelectedTab = control.DeclarationTabPage;
				var declaration = control.JobDeclaration;
				AssertEquals("JE_HouseBill", "HB1907011132", declaration.JE_HouseBill);
				var row = ((INeedRow)declaration).Row;
				var rows = row.Table.Rows;
				AssertEquals(false, declaration.IsInDatabase);
				ErrorReporter.Clear();
				var newDec = newFactory.New<BaseJobDeclaration>();
				var newRow = ((INeedRow)newDec).Row;
				rows.Remove(newRow);
				AssertEquals("Logging should only be done for the Plugin's Declaration", "", ErrorReporter.LastKeyReported);
				CombineAssertions(() =>
				{
					rows.Remove(row);
					AssertEquals("LastKeyReported", "Declaration on plugin should not be deleted", ErrorReporter.LastKeyReported);
					AssertEquals("LastMessageReported", $"Declaration row is being deleted (Action=Delete, State=Added,PK={declaration.PK})", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				});
			}
			var shipmentDec = newFactory.New<BaseJobDeclaration>();
			shipmentDec.JE_JS = shipment.PK;
			var shipmentDecRow = ((INeedRow)shipmentDec).Row;
			shipmentDecRow.Table.Rows.Remove(shipmentDecRow);
			AssertEquals("Logging should have been removed when plugin was disposed", "", ErrorReporter.LastKeyReported);
		}

		public void TestTabPageMinimumAutoSizedIsSetFromUserControl()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			var mockPlugIn = new Mock<BaseBrokeragePlugIn>(new object[] { shipment });
			mockPlugIn.CallBase = true;
			using (BaseBrokeragePlugIn plugIn = mockPlugIn.Object)
			{
				ZAutoSizedTabPagePlugIn tabPagePlugIn = new ZAutoSizedTabPagePlugIn(plugIn);
				tabPagePlugIn.MinimumAutoSizedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
				tabPagePlugIn.MinimumAutoSizedHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(10);
				BaseCustomsBrokerageUserControl userControl = new BaseCustomsBrokerageUserControl();
				userControl.MinimumSize = ControlDpiScalingHelper.NewScaledSize(50, 60);
				userControl.Size = ControlDpiScalingHelper.NewScaledSize(55, 65);

				mockPlugIn.Protected().Setup<BaseCustomsBrokerageUserControl>("CreateBrokerageUserControl").Returns(userControl);
				mockPlugIn.Protected().Setup<ZTabPagePlugIn>("GetTabPage").Returns(tabPagePlugIn);
				plugIn.UpdateTabPageMinimumAutoSized();
				AssertEquals("MinimumAutoSizedWidth", ControlDpiScalingHelper.ScaleToCurrentDpiX(50), tabPagePlugIn.MinimumAutoSizedWidth);
				AssertEquals("MinimumAutoSizedHeight", ControlDpiScalingHelper.ScaleToCurrentDpiY(60), tabPagePlugIn.MinimumAutoSizedHeight);
			}
			mockPlugIn.VerifyAll();
		}

		public void TestDoNotCreateDeclarationIfShipmentIsReadOnly()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.ReadOnly = true;

			using (var plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertEquals("QueryUserShouldPlugInGUIAndBusinessEntityBeCreated", false, plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
				AssertNull("InternalJobDeclaration", plugIn.InternalJobDeclaration);
				AssertEquals("PlugInNotDisplayedMessage", "No declaration exists for this Login Company", plugIn.PlugInNotDisplayedMessage);
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertEquals("Declaration not created yet", false, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
				AssertEquals("PlugIn can be shown when declaration has been created", true, plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
				AssertEquals("PlugIn can be shown when declaration has been created", true, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
			}
		}

		public void TestQueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.Mutex.Lock();
				AssertEquals("Mutex is locked by this plugin", true, plugIn.Mutex.HasLock);

				using (BaseBrokeragePlugInForTest plugIn2 = new BaseBrokeragePlugInForTest(shipment))
				{
					bool result = plugIn2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
					AssertEquals("Cannot create PlugInGUI and Business Entity as it is mutex-locked", false, result);

					AssertEquals("PlugInNotDisplayedMessage", BaseBrokeragePlugInForTest.MutexLockText, plugIn2.PlugInNotDisplayedMessage);
				}
			}
		}

		public void TestQueryUserShouldPlugInGUIAndBusinessEntityBeCreated_WhenSelectedFromDropdown_ShouldPromptUser()
		{
			var consol = new BusinessObjectFactory().New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			using (var plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.Mutex.Lock();
				AssertEquals("Mutex is locked by this plugin", true, plugIn.Mutex.HasLock);

				using (var plugIn2 = new BaseBrokeragePlugInForTest(shipment))
				{
					AssertEquals("Cannot create PlugInGUI and Business Entity as it is mutex-locked", false, plugIn2.ShouldPluginDropdownMenuBeCreatedInternal());
					AssertEquals("PlugInNotDisplayedMessage", BaseBrokeragePlugInForTest.MutexLockText, plugIn2.PlugInNotDisplayedMessage);
				}
			}
		}

		public void TestImportBrokerLicenceWillBeCheckedAgainstWhenMessageTypeIsImport()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.OnGUIShown();
				plugIn.CreateNewDec();

				Env.Licence.ImportBroker.ForceLogout();
				AssertEquals("Broker IsLoggedIn", false, Env.Licence.ImportBroker.IsLoggedIn);
				plugIn.InternalJobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				plugIn.InternalJobDec.Validation.ValidateJE_MessageType();

				AssertEquals("Broker IsLoggedIn", true, Env.Licence.ImportBroker.IsLoggedIn);
			}
		}

		public void TestBondedWarehouseLicenceWillBeCheckedAgainstWhenSupportsBondedWarehousing()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());

			using (var plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.OnGUIShown();
				plugIn.CreateNewDec();

				Env.Licence.BondedWarehouse.ForceLogout();
				AssertEquals("BondedWarehouse IsLoggedIn", false, Env.Licence.BondedWarehouse.IsLoggedIn);
				plugIn.InternalJobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("BondedWarehouse IsLoggedIn", false, Env.Licence.BondedWarehouse.IsLoggedIn);
				plugIn.InternalJobDec.SetSupportsBondedWarehousingForTesting(true);
				var invoice = plugIn.InternalJobDec.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				plugIn.InternalJobDec.Validation.ValidateJE_OH_Importer();
				AssertEquals("BondedWarehouse IsLoggedIn", true, Env.Licence.BondedWarehouse.IsLoggedIn);
			}
		}

		public void TestSecurityCheckOnShowPreSaveDialogs()
		{
			var expectedMessage = "You do not have security rights to save an Import Customs job. ";
			expectedMessage += Env.Security.ImportEdit.DisplayTextPathToSecurityRight;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Env.Security.ImportEdit.IsAllowed = false;
			BaseJobDeclaration declaration = null;
			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				//not in database, no gui shown, no security check
				plugIn.CreateNewDec();
				plugIn.InternalJobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//not in database, gui shown, no security check
				plugIn.OnGUIShown();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//not in database, user control shown, security check
				plugIn.OnUserControlShown();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				declaration = plugIn.InternalJobDec;
			}
			declaration.Delete();

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				//not in database, menu item selected control shown, security check
				plugIn.CreateNewDec();
				plugIn.InternalJobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				plugIn.OnGUIShown();

				if (plugIn.TopLevelMenu.MenuItems.Count > 0)
				{
					plugIn.TopLevelMenu.MenuItems[1].PerformSelect();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					plugIn.ShowPreSaveDialogs();
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				//saved, no changes, user control shown, no security check
				Env.Security.ImportEdit.IsAllowed = true;
				plugIn.Factory.Save();
				Env.Security.ImportEdit.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//saved, haschanges, user control shown, security check
				plugIn.InternalJobDec.JE_HouseBill = "TEST";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowPreSaveDialogsCoreForSupervisorOverrides()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Env.Security.ImportEdit.IsAllowed = false;

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.CreateNewDec();
				var declaration = plugIn.InternalJobDec;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(ContinueWithSave.Yes, plugIn.ShowPreSaveDialogs());
			}

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment2))
			{
				plugIn.CreateNewDec();
				var declaration = plugIn.InternalJobDec;
				declaration.JE_MergeBy = "ZZZ";
				declaration.SetSupportsBondedWarehousingForTesting(true);

				bool oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
				bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
				bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
				bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
				var oldMergeByDefaultAllowed = Env.Security.MergeByDefault.IsAllowed;
				try
				{
					Env.Security.SupervisorOverrides.IsAllowed = false;
					Env.Security.AllowMessageErrors.IsAllowed = false;
					Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
					Env.Security.MergeByDefault.IsAllowed = false;
					GlbStaff.CurrentUser.GS_IsController = false;
					GlbStaff staff = Factory.New<GlbStaff>();
					staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
					staff.GS_IsActive = true;

					GlbSecurity se = Factory.New<GlbSecurity>();
					se.GU_SecurityRight = Env.Security.MergeByDefault.Code;
					se.GU_SecurityItemIsAllowed = true;
					se.GU_GS = staff.PK;
					Factory.Save();

					declaration.JE_MergeBy = "YYY";
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals(ContinueWithSave.No, plugIn.ShowPreSaveDialogs());
				}
				finally
				{
					Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
					Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
					Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
					GlbStaff.CurrentUser.GS_IsController = oldIsController;
					Env.Security.MergeByDefault.IsAllowed = oldMergeByDefaultAllowed;
				}
			}
		}

		public void TestBondedWarehouseSecurityCheckOnShowPreSaveDialogs()
		{
			var expectedMessage = "You do not have security rights to save a Bonded Warehousing Customs job. ";
			expectedMessage += Env.Security.ImportEditBondedWarehouse.DisplayTextPathToSecurityRight;

			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			Env.Security.ImportEditBondedWarehouse.IsAllowed = false;
			BaseJobDeclaration declaration = null;
			using (var plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				//not in database, no gui shown, no security check
				plugIn.CreateNewDec();
				declaration = plugIn.InternalJobDec;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//not in database, gui shown, no security check
				plugIn.OnGUIShown();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//not in database, user control shown, security check
				plugIn.OnUserControlShown();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			declaration.Delete();

			using (var plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				//not in database, menu item selected control shown, security check
				plugIn.CreateNewDec();
				declaration = plugIn.InternalJobDec;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				plugIn.OnGUIShown();

				if (plugIn.TopLevelMenu.MenuItems.Count > 0)
				{
					plugIn.TopLevelMenu.MenuItems[1].PerformSelect();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					plugIn.ShowPreSaveDialogs();
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				//saved, no changes, user control shown, no security check
				Env.Security.ImportEditBondedWarehouse.IsAllowed = true;
				plugIn.Factory.Save();
				Env.Security.ImportEditBondedWarehouse.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//saved, haschanges, user control shown, security check
				declaration.JE_HouseBill = "TEST";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestImportBrokerLicenceWillNOTBeCheckedAgainstWhenMessageTypeIsImportAndWhenPlugInHasNotBeenShown()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.CreateNewDec();

				Env.Licence.ImportBroker.ForceLogout();
				AssertEquals("Broker IsLoggedIn", false, Env.Licence.ImportBroker.IsLoggedIn);
				plugIn.InternalJobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				plugIn.InternalJobDec.Validation.ValidateJE_MessageType();

				AssertEquals("Broker IsLoggedIn", false, Env.Licence.ImportBroker.IsLoggedIn);
			}
		}

		public void TestBondedWarehouseLicenceWillNOTBeCheckedAgainstWhenImporterWhenPlugInHasNotBeenShown()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			using (var plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.CreateNewDec();

				Env.Licence.BondedWarehouse.ForceLogout();
				AssertEquals("BondedWarehouse IsLoggedIn", false, Env.Licence.BondedWarehouse.IsLoggedIn);
				plugIn.InternalJobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				plugIn.InternalJobDec.SetSupportsBondedWarehousingForTesting(true);
				plugIn.InternalJobDec.Validation.ValidateJE_OH_Importer();
				AssertEquals("BondedWarehouse IsLoggedIn", false, Env.Licence.BondedWarehouse.IsLoggedIn);
			}
		}

		public void TestExportBrokerLicenceWillBeCheckedAgainstWhenMessageTypeIsExport()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.OnGUIShown();
				plugIn.CreateNewDec();

				Env.Licence.ExportBroker.ForceLogout();
				AssertEquals("Broker IsLoggedIn", false, Env.Licence.ExportBroker.IsLoggedIn);
				plugIn.InternalJobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				plugIn.InternalJobDec.Validation.ValidateJE_MessageType();
				AssertEquals("Broker IsLoggedIn", true, Env.Licence.ExportBroker.IsLoggedIn);
			}
		}
		[ExpectNoExceptions]
		public void TestGetDeclarationFromShipmentWhenMultiCountryInvolved()
		{
			GlbBranch branchInThisCountry = GlbCompany.CurrentCompany.Branches.AddNew();
			branchInThisCountry.GB_RL_NKHomePort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "YYY";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			BaseJobDeclaration decInCurrentCountry = Factory.New<BaseJobDeclaration>();
			decInCurrentCountry.JE_GB = branchInThisCountry.PK;
			decInCurrentCountry.JE_JS = shipment.PK;

			GlbBranch branchInOtherCountry = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew();
			branchInOtherCountry.GB_RL_NKHomePort = "XXYYY";

			BaseJobDeclaration decInOtherCountry = Factory.New<BaseJobDeclaration>();
			decInOtherCountry.JE_GB = branchInOtherCountry.PK;
			decInOtherCountry.JE_JS = shipment.PK;

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.OnGUIShown();
			}
		}

		public void TestApportionIfDirtyWhenSaving()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			BaseJobDeclaration jobDec = BaseJobDeclaration.New(Factory);
			jobDec.JE_JS = shipment.PK;

			using (BaseBrokeragePlugInForTest plugIn = new BaseBrokeragePlugInForTest(shipment))
			{
				plugIn.OnGUIShown();

				AssertNotNull("Cannot find BrokeragePlugIn on Shipment Form", plugIn);

				plugIn.OnGUIShown();

				jobDec.ApportionmentDirty = true;

				AssertEquals("Apportionment is pending", true, jobDec.ApportionmentDirty);
				plugIn.ShowPreSaveDialogs();
				AssertEquals("Apportionment is complete", false, jobDec.ApportionmentDirty);
			}
		}

		[ExpectNoExceptions()]
		public void TestBrokeragePlugInSavingOKWithoutInstantiatingBizO()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentForm testForm = new ShipmentForm(shipment))
			{
				foreach (ZPlugIn plugIn in testForm.PlugIns.Instances)
				{
					plugIn.ShowPreSaveDialogs();
				}
			}
		}

		public void TestSettingImportCustomsBrokerAsksToCreateJobDeclaration()
		{
			FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			OrgHeader otherBroker = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUBNE";

			AssertNotNull("precondition: expecting to have an org proxy", GlbBranch.CurrentBranch.OrgProxy);
			AssertEquals("precondition: Shipment should be an import shipment", true, shipment.IsImport());

			using (var plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertNull("Should not already have a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = otherBroker.PK;
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = ZGuid.Empty;
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = false;

			using (var plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertNull("Should not already have a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);
				Assert("Should not have asked any questions", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			}

			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
			shipment.JS_OH_ImportBroker = ZGuid.Empty;

			using (var plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertNull("Should not already have a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Expecting to ask the user.", 1, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNotNull("Should have created the Declaration now", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = ZGuid.Empty;
				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Expecting not to ask the user.", 1, plugin.QueryUserToCreateDeclarationCountForTesting);
			}
		}

		public void TestAsksToCreateJobDeclarationWithNoSecurity()
		{
			FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = false;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUBNE";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertEquals("Declaration should not be created, because user has no security rights", false, plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
				AssertNull("Should not already have a declaration.", plugin.InternalJobDec);
				var userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Last text", true, userNotification.Contains(Env.Security.CustomsDeclarationEnquiryNew.ErrorMessageForNotAllowed));

				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
				AssertEquals("Declaration should be created, user has security rights", true, plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
				AssertNotNull("Should have created the Declaration now", plugin.InternalJobDec);
			}
		}

		public void TestBrokerageJobAutoCreationRegistryItem_NoAutoCreate()
		{
			FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			OrgHeader otherBroker = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUBNE";

			AssertNotNull("precondition: expecting to have an org proxy", GlbBranch.CurrentBranch.OrgProxy);
			AssertEquals("precondition: Shipment should be an import shipment", true, shipment.IsImport());

			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertNull("Should not already have a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Expecting to ask the user.", 1, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNotNull("Should have created the Declaration now", plugin.InternalJobDec);
			}
		}

		public void TestBrokerageJobAutoCreationRegistryItem_AutoCreate()
		{
			FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			OrgHeader otherBroker = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUBNE";

			AssertNotNull("precondition: expecting to have an org proxy", GlbBranch.CurrentBranch.OrgProxy);
			AssertEquals("precondition: Shipment should be an import shipment", true, shipment.IsImport());

			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertNull("Should not already have a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Do NOT ask the user as auto-create registry is on", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNotNull("Should have created the Declaration now", plugin.InternalJobDec);
			}
		}

		public void TestSettingExportCustomsBrokerAsksToCreateJobDeclaration()
		{
			FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			OrgHeader otherBroker = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			AssertNotNull("precondition: expecting to have an org proxy", GlbBranch.CurrentBranch.OrgProxy);
			AssertEquals("precondition: Shipment should be an export shipment", true, shipment.IsExport());

			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertNull("Should not already have a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ExportBroker = otherBroker.PK;
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ExportBroker = ZGuid.Empty;
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);

				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = false;
				shipment.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);

				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
				shipment.JS_OH_ExportBroker = ZGuid.Empty;
				shipment.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Expecting to ask the user.", 1, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNotNull("Should have created the Declaration now", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = ZGuid.Empty;
				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				AssertEquals("Expecting not to ask the user.", 1, plugin.QueryUserToCreateDeclarationCountForTesting);
			}
		}

		public void TestSettingImportCustomsBrokerWhenFactoryInTransaction()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			GlbBranch.CurrentBranch.OrgProxy.OH_IsBroker = true;
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "Consignee";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_IsConsignee = true;
			consignee.OH_IsConsignor = true;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "Consignor";
			consignor.OH_RL_NKClosestPort = "SGSIN";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsConsignee = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.ConsigneePK = GlbBranch.CurrentBranch.OrgProxy.PK;
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();

			using (var plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				Assert("precondition: Shipment should be an import shipment", shipment.IsImport());

				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Edit";
				trigger.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
				notification.PQ_FieldName = JobShipmentSchema.JS_OH_ImportBroker.Name;
				notification.PQ_FieldValue = "<ConsigneePK>";

				shipment.JS_ActualWeight = 100m;
				Factory.Save();

				AssertEquals("JS_OH_ImportBroker should be set", GlbBranch.CurrentBranch.OrgProxy.PK, shipment.JS_OH_ImportBroker);
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ImportBroker = ZGuid.Empty;
				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.OrgProxy.PK;
				AssertEquals("Should have asked the user", 1, plugin.QueryUserToCreateDeclarationCountForTesting);
			}
		}

		public void TestSettingExportCustomsBrokerWhenFactoryInTransaction()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			GlbBranch.CurrentBranch.OrgProxy.OH_IsBroker = true;
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "Consignee";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_IsConsignee = true;
			consignee.OH_IsConsignor = true;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "Consignor";
			consignor.OH_RL_NKClosestPort = "SGSIN";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsConsignee = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.ConsigneePK = consignor.PK;
			shipment.ConsignorPK = GlbBranch.CurrentBranch.OrgProxy.PK;

			Factory.Save();

			using (var plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				Assert("precondition: Shipment should be an export shipment", shipment.IsExport());

				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Edit";
				trigger.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
				notification.PQ_FieldName = JobShipmentSchema.JS_OH_ExportBroker.Name;
				notification.PQ_FieldValue = "<ConsignorPK>";

				shipment.JS_ActualWeight = 100m;
				Factory.Save();

				AssertEquals("JS_OH_ExportBroker should be set", GlbBranch.CurrentBranch.OrgProxy.PK, shipment.JS_OH_ExportBroker);
				AssertEquals("Should not have asked the user", 0, plugin.QueryUserToCreateDeclarationCountForTesting);
				AssertNull("Should not have created a declaration.", plugin.InternalJobDec);

				shipment.JS_OH_ExportBroker = ZGuid.Empty;
				shipment.JS_OH_ExportBroker = GlbBranch.CurrentBranch.OrgProxy.PK;
				AssertEquals("Should have asked the user", 1, plugin.QueryUserToCreateDeclarationCountForTesting);
			}
		}

		public void TestJobDeclarationFilterIgnoreActiveFilter()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				AssertEquals("IgnoreActiveFilter is on", true, plugin.CreateJobDeclarationFilter().IgnoreActiveFilter);
			}
		}

		public void TestImportJobDeclaration()
		{
			GlbCompany nZCompany = Factory.New<GlbCompany>();
			GlbBranch nZBranch = nZCompany.Branches.AddNew();
			nZBranch.GB_RL_NKHomePort = "NZAKL";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_GoodsDescription = "DESCRIPTION";

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_GB = nZBranch.PK;
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				plugin.CreateNewDec();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Last text", true, userNotification.Contains(BaseBrokeragePlugIn.ImportDeclarationFromOtherCountryQuery));
			}
		}

		public void TestAuditMenuItem()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment)) // This sets shipment.IsTopLevel to true.
			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				plugin.CreateNewDec();
				Tester.Test(
					(WriteToLogMenuItem)plugin.TopLevelMenu.MenuItems[plugin.TopLevelMenu.MenuItems.Count - 1],
					plugin.Shipment,
					plugin.InternalJobDeclaration,
					Env.Security.CustomsDeclarationAudit,
					"Audit Customs Declaration");
			}
		}

		public void TestTwoDeclarationsCanNotBeAddedToSameShipment()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingShipment shipment1 = factory1.New<ForwardingShipment>();
			factory1.Save();
			ForwardingShipment shipment2 = factory2.Load<ForwardingShipment>(shipment1.PK);
			using (BaseBrokeragePlugInForTest plugIn1 = new BaseBrokeragePlugInForTest(shipment1))
			{
				plugIn1.Mutex.Lock();
				BaseJobDeclaration declaration1 = factory1.New<BaseJobDeclaration>();
				declaration1.JE_JS = shipment1.PK;
				declaration1.JE_MasterBill = "MASTER";
				using (BaseBrokeragePlugInForTest plugIn2 = new BaseBrokeragePlugInForTest(shipment2))
				{
					AssertEquals("Cannot create PlugInGUI and Business Entity as it is mutex-locked", false, plugIn2.ShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					factory1.Save();
					AssertEquals("Can create PlugInGUI and Business Entity as it is now not mutex-locked", true, plugIn2.ShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					BaseJobDeclaration declaration2 = plugIn2.GetDeclarationFromShipment();
					AssertEquals("Existing Attached declaration should be found", false, declaration2.IsNull);
					AssertEquals("Existing Attached declaration should be saved one", "MASTER", declaration2.JE_MasterBill);
				}
			}
		}

		public void TestPluginIsDisposedCorrectlyWhenThereAreEventsHook()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				dec.JE_MessageType = ZString.Empty;
				plugin.InternalJobDeclaration = dec;
				plugin.JE_MessageTypeInfo_ValueChangedWasCalled = false;
				plugin.JS_OH_ImportBrokerInfo_ValueChangedWasCalled = false;
				plugin.JS_OH_ExportBrokerInfo_ValueChangedWasCalled = false;
				shipment.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				dec.JE_MessageType = "EXP";
				AssertEquals(true, plugin.JE_MessageTypeInfo_ValueChangedWasCalled);
				AssertEquals(true, plugin.JS_OH_ImportBrokerInfo_ValueChangedWasCalled);
				AssertEquals(true, plugin.JS_OH_ExportBrokerInfo_ValueChangedWasCalled);

				plugin.Dispose();
				plugin.JE_MessageTypeInfo_ValueChangedWasCalled = false;
				plugin.JS_OH_ImportBrokerInfo_ValueChangedWasCalled = false;
				plugin.JS_OH_ExportBrokerInfo_ValueChangedWasCalled = false;
				shipment.JS_OH_ExportBroker = ZGuid.Empty;
				shipment.JS_OH_ImportBroker = ZGuid.Empty;
				dec.JE_MessageType = "IMP";
				AssertEquals(false, plugin.JE_MessageTypeInfo_ValueChangedWasCalled);
				AssertEquals(false, plugin.JS_OH_ImportBrokerInfo_ValueChangedWasCalled);
				AssertEquals(false, plugin.JS_OH_ExportBrokerInfo_ValueChangedWasCalled);
			}
		}

		public void TestDefaultFreightChargesWhenCreatingANewDeclaration()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualChargeable = 2000m;
			shipment.JS_UnitFreightRate = 1.5m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";

			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				plugin.CreateNewDec();
				AssertNotNull("declaration has been created", plugin.JobDeclaration);
				AssertEquals("freight has been defaulted", 1, plugin.JobDeclaration.JobComInvoiceGroupHeaders[0].Charges.Count);

				BaseGroupInvoiceCharge freightDefaulted = plugin.JobDeclaration.JobComInvoiceGroupHeaders[0].Charges[0];
				AssertEquals(3000m, freightDefaulted.J7_Amount);
				AssertEquals("AUD", freightDefaulted.J7_RX_NKCurrency);
			}

			using (BaseBrokeragePlugInForTest plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				var declarationLoaded = plugin.InternalJobDec;
				AssertNotNull("PreCondition", declarationLoaded);
				AssertEquals("Freight is not defaulted as this is an existing job", 1, declarationLoaded.JobComInvoiceGroupHeaders[0].Charges.Count);
			}
		}

		public void TestRaiseJobDeclarationCreated()
		{
			var count = 0;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.OnJobDeclarationCreated += (object sender, JobDeclarationCreationEventArgs e) =>
				{
					count++;
				};
			using (var form = new ZForm(shipment))
			using (var plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				plugin.CreateNewDec();
				AssertEquals(1, count);
			}
		}

		public void TestProductClassificationCreationConfirmationFormCheckOnShowPreSaveDialogs()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = "SUPPLIER";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SomePart";
			part.OP_Desc = "Description of product";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;

			var relOrg = part.RelatedOrganisations.AddNew();
			relOrg.OU_OH = declarationSupplier.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = declarationSupplier.PK;

			var invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_PartNo = part.OP_PartNum;
			invLine.JI_Tariff = "123";

			Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed = true;

			using (var plugin = new BaseBrokeragePlugInForTest(shipment))
			{
				plugin.ShowPreSaveDialogs();
				AssertEquals("ProductClassificationCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
			}
		}

		public void TestDeclarationCreatedCorrectlyBetweenUSAndPR()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "IANTESTPR";
			orgHeader.OH_RL_NKClosestPort = "PRBAS";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "DEFRA";
			consol.JK_MasterBillNum = "01618112010";
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgHeader.PK;
			shipment.JS_ActualWeight = 22m;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				consol.JK_RL_NKDischargePort = "PRBQN";
				BaseJobDeclaration declaration = null;
				using (var plugin = new BaseBrokeragePlugInForTest(shipment))
				{
					plugin.CreateNewDec();
					AssertNotNull("declaration has been created", plugin.JobDeclaration);
					AssertEquals("Master bill has been defaulted", "01618112010", plugin.JobDeclaration.JE_MasterBill);
					AssertEquals("Total weight has been defaulted", 22m, plugin.JobDeclaration.JE_TotalWeight);

					declaration = plugin.JobDeclaration;
				}
				declaration.Delete();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				consol.JK_RL_NKDischargePort = "USCHI";
				BaseJobDeclaration declaration = null;
				using (var plugin = new BaseBrokeragePlugInForTest(shipment))
				{
					plugin.CreateNewDec();
					AssertNotNull("declaration has been created", plugin.JobDeclaration);
					AssertEquals("Master bill has been defaulted", "01618112010", plugin.JobDeclaration.JE_MasterBill);
					AssertEquals("Total weight has been defaulted", 22m, plugin.JobDeclaration.JE_TotalWeight);

					declaration = plugin.JobDeclaration;
				}
				declaration.Delete();
			}
		}
	}

	sealed class BaseBrokeragePlugInForTest : BaseBrokeragePlugIn
	{
		public BaseBrokeragePlugInForTest(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public void CreateNewDec()
		{
			CreateDeclarationHelper.CreateDeclaration(Shipment, Mutex, null);
		}

		public bool JE_MessageTypeInfo_ValueChangedWasCalled;

		public bool JS_OH_ExportBrokerInfo_ValueChangedWasCalled;

		public bool JS_OH_ImportBrokerInfo_ValueChangedWasCalled;

		public BaseJobDeclaration InternalJobDec => InternalJobDeclaration;

		public BaseJobDeclaration TriggerGetDeclarationFromShipment() => GetDeclarationFromShipment();

		protected override bool QueryUser(ForwardingShipment shipment, ICollection<CreateBrokerageQuestion> questions, CreateDeclarationHelper.QuestionType type)
		{
			var result = base.QueryUser(shipment, questions, type);

			if (type != CreateDeclarationHelper.QuestionType.ImportDeclarationQuery)
			{
				return true;
			}

			return result;
		}

		protected override MenuItem GetNewTopLevelMenuCore() => EDIMenu;

		EDIMenu ediMenu;
		EDIMenu EDIMenu => ediMenu ?? (ediMenu = new EDIMenu());

		protected override void HookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
			base.HookDeclarationEventsCore(declaration);
			declaration.JE_MessageTypeInfo.ValueChanged += new EventHandler(JE_MessageTypeInfo_ValueChanged);
		}

		protected override void UnHookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageTypeInfo.ValueChanged -= new EventHandler(JE_MessageTypeInfo_ValueChanged);
			base.UnHookDeclarationEventsCore(declaration);
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			JE_MessageTypeInfo_ValueChangedWasCalled = true;
		}

		protected override void JS_OH_ExportBrokerInfo_ValueChanged(object sender, EventArgs e)
		{
			JS_OH_ExportBrokerInfo_ValueChangedWasCalled = true;
			base.JS_OH_ExportBrokerInfo_ValueChanged(sender, e);
		}

		protected override void JS_OH_ImportBrokerInfo_ValueChanged(object sender, EventArgs e)
		{
			JS_OH_ImportBrokerInfo_ValueChangedWasCalled = true;
			base.JS_OH_ImportBrokerInfo_ValueChanged(sender, e);
		}

		internal bool ShouldPlugInGUIAndBusinessEntityBeCreatedInternal() => ShouldPlugInGUIAndBusinessEntityBeCreated();

		internal bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
	}
}
