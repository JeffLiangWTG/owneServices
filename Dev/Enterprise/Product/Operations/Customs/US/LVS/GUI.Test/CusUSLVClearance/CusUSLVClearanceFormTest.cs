using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(CusUSLVClearanceForm))]
	public class CusUSLVClearanceFormTest : ZFormBasherTest
	{
		public void TestSendingMessage_MessageStatusIsPendingForSendOriginalMessage_ShouldBeIgnored()
		{
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				clearance.CusUSLVConsignments.AddNew();

				var originalRequestPendingConsignment = clearance.CusUSLVConsignments.AddNew();
				originalRequestPendingConsignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("No message was sent.", "0 original message(s) generated; 2 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				clearance.CusUSLVConsignments.AddNew();

				var originalRequestPendingConsignment = clearance.CusUSLVConsignments.AddNew();
				originalRequestPendingConsignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("Right message", "1 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendingMessage_ShouldIgnoreInactiveConsignments()
		{
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
				var inactiveConsignment = clearance.CusUSLVConsignments.AddNew();
				inactiveConsignment.ULB_NumberOfPacks = 1;
				inactiveConsignment.ULB_IsActive = false;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("No message was sent.", "0 original message(s) generated; 2 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
				var inactiveConsignment = clearance.CusUSLVConsignments.AddNew();
				inactiveConsignment.ULB_NumberOfPacks = 1;
				inactiveConsignment.ULB_IsActive = false;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("Right message", "1 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendingMessage_ShouldIgnoreWhenClearanceSendCustomsMessageMutexLocked()
		{
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);
				UnitTestUserNotification.Instance.ClearUserResponses();

				try
				{
					if (clearance.LockSendCustomsMessageMutex())
					{
						sendOriginalMessageMenuItem.PerformClick();
						AssertEquals("Right message", "0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					clearance.UnlockSendCustomsMessageMutex();
				}
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_HasSubMenuItems()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals(3, convertToStandAloneDeclarationMenuItem.MenuItems.Count);

				AssertNotNull(convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("manualSelection"));
				AssertNotNull(convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withMessageErrors"));
				AssertNotNull(convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withPGARequirements"));
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_ManualSelection()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var manualSelectionMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("manualSelection");
				manualSelectionMenuItem.PerformClick();

				using (var bulkConvertToStandAloneDeclarationForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType<BulkConvertToStandAloneDeclarationForm>(bulkConvertToStandAloneDeclarationForm);
				}
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithMessageErrors_MessageText()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var manualSelectionMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withMessageErrors");
				manualSelectionMenuItem.PerformClick();

				using (var msg = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox)
				{
					AssertNotNull(msg);
					AssertEquals("This will convert all consignments with a message error to a standalone declaration, please choose from the below:", msg.MessageMultilingual);
				}
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithMessageErrors_Individual()
		{
			var grouping = Factory.NewWithValidTestData<RefDataGrouping>();
			grouping.ZZZ_DataGrouping = "US";
			grouping.ZZZ_Description = "United States";
			var deminimus = Factory.New<RefCusTaxOrFee>();
			deminimus.ZZF_ZZZ_NKDataGrouping = "US";
			deminimus.ZZF_Code = "DEM";
			deminimus.ZZF_StartDate = new ZDateTime(1960, 1, 1);
			deminimus.ZZF_EndDate = ZDateTime.Today.AddYears(1);
			deminimus.ZZF_Value = 800;
			deminimus.ZZF_Description = "a value";
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				Assert("precondition : no log in clearance ", !clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode).Any());

				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.ULB_SellerName = "someone";
				consignment1.ULB_SellerCity = "somewhere";
				consignment1.ULB_SellerAddress1 = "somewhere";
				consignment1.ULB_RN_NKSellerCountry = "AU";
				consignment1.ULB_ConsigneeName = "Ian";
				consignment1.ULB_ConsigneeCity = "syd";
				consignment1.ULB_ConsigneeAddress1 = "xx";
				consignment1.ULB_RN_NKConsigneeCountry = "AU";
				consignment1.RunPreSaveValidation();

				Assert("precondition : consignment has message error", consignment1.HasMessageErrors);

				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.ULB_NumberOfPacks = 1;
				consignment2.FirstCusUSLVItemTariff = "2517.10.0015";
				consignment2.FirstCusUSLVItemCountryOfOrigin = "US";
				consignment2.FirstCusUSLVItemLineValue = 5.0;
				consignment2.ULB_SellerName = "someone";
				consignment2.ULB_SellerCity = "somewhere";
				consignment2.ULB_SellerAddress1 = "somewhere";
				consignment2.ULB_RN_NKSellerCountry = "AU";
				consignment2.ULB_ConsigneeName = "Ian";
				consignment2.ULB_ConsigneeCity = "syd";
				consignment2.ULB_ConsigneeAddress1 = "xx";
				consignment2.ULB_RN_NKConsigneeCountry = "AU";

				consignment2.RunPreSaveValidation();
				Assert("precondition : consignment has no message error", !consignment2.HasMessageErrors);

				ClearanceForm.Show();

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var manualSelectionMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withMessageErrors");
				manualSelectionMenuItem.PerformClick();

				var tciLogs = clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);
				CombineAssertions("would not convert without message error", () =>
				{
					AssertEquals(1, tciLogs.Count());
					Assert(tciLogs.SingleOrDefault().SL_Reference.Contains(consignment1.PK.ToString()));
				});

				ZFormModaliser.LastFormShownDialogForTest.Close();
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithMessageErrors_Individual_WouldNotConvertAgainIfAlreadyConverted()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			clearance.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, consignment2.PK.ToString(), null);
			consignment2.ULB_IsActive = false;

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			using (var form = new CusUSLVClearanceFormForTest(new BusinessObjectFactory().Load<CusUSLVClearance>(clearance.PK)))
			{
				form.Show();

				var convertToStandAloneDeclarationMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var withMessageErrorsMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withMessageErrors");
				withMessageErrorsMenuItem.PerformClick();

				var tciLogs = clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);

				CombineAssertions("would not convert again if already converted", () =>
				{
					AssertEquals(2, tciLogs.Count());
					Assert(tciLogs.Any(l => l.SL_Reference.Contains(consignment1.PK.ToString())));
					Assert(tciLogs.Any(l => l.SL_Reference.Contains(consignment2.PK.ToString())));
				});

				ZFormModaliser.LastFormShownDialogForTest.Close();
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithMessageErrors_Individual_WouldNotConvertIfEntryNumIsNotEmpty()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				Assert("precondition : no log in clearance ", !clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode).Any());

				var consignment1 = clearance.CusUSLVConsignments.AddNew();

				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.CE_EntryNum = "EntryNum";
				Assert("precondition : consignment2.CE_EntryNum is not empty", !consignment2.CE_EntryNum.IsEmpty);

				ClearanceForm.Show();

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var manualSelectionMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withMessageErrors");
				manualSelectionMenuItem.PerformClick();

				var tciLogs = clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);

				CombineAssertions("would not convert if entry num is empty", () =>
				{
					AssertEquals(1, tciLogs.Count());
					Assert(tciLogs.SingleOrDefault().SL_Reference.Contains(consignment1.PK.ToString()));
				});

				ZFormModaliser.LastFormShownDialogForTest.Close();
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithMessageErrors_Combined()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;

				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.RunPreSaveValidation();

				Assert("precondition : consignment has message error", consignment1.HasMessageErrors);

				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.RunPreSaveValidation();

				Assert("precondition : consignment has message error", consignment2.HasMessageErrors);

				ClearanceForm.Show();

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var manualSelectionMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withMessageErrors");
				manualSelectionMenuItem.PerformClick();

				var tciLogs = clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);

				CombineAssertions("converts consignments combined (1 log) and sets ULB_ConvertAction", () =>
				{
					AssertEquals(1, tciLogs.Count());
					Assert(tciLogs.Any(l => l.Parameters.TryGetValue(EventReferenceParameters.Codes.Reason, out var reason) && reason == "Combine consignments"));
					AssertEquals(ULBConvertActionList.Codes.Combined, consignment1.ULB_ConvertAction);
					AssertEquals(ULBConvertActionList.Codes.Combined, consignment2.ULB_ConvertAction);
				});

				ZFormModaliser.LastFormShownDialogForTest.Close();
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithMessageErrors_Combined_WouldNotConvertAgainIfAlreadyConverted()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();

			clearance.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, consignment1.PK.ToString(), null);
			consignment1.ULB_IsActive = false;

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = new CusUSLVClearanceFormForTest(new BusinessObjectFactory().Load<CusUSLVClearance>(clearance.PK)))
			{
				form.Show();

				var convertToStandAloneDeclarationMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var withMessageErrorsMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withMessageErrors");
				withMessageErrorsMenuItem.PerformClick();

				var tciLogs = clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);

				CombineAssertions("would not convert again if already converted", () =>
				{
					AssertEquals(1, tciLogs.Count());
					Assert(tciLogs.Any(l => l.SL_Reference.Contains(consignment1.PK.ToString())));
				});

				ZFormModaliser.LastFormShownDialogForTest.Close();
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithPGARequirements_MessageText()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var manualSelectionMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withPGARequirements");
				manualSelectionMenuItem.PerformClick();

				AssertEquals("This will convert all consignments with PGA requirements into individual standalone declarations, do you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithPGARequirements()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				var clearance = ClearanceForm.BusinessEntity;
				Assert("precondition : no log in clearance ", !clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode).Any());

				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment1.ULB_HasPGAPending = true;
				consignment2.ULB_HasPGAPending = false;

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var manualSelectionMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withPGARequirements");
				manualSelectionMenuItem.PerformClick();

				var tciLogs = clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);
				CombineAssertions("would not convert if HasPGAPending is false", () =>
				{
					AssertEquals(1, tciLogs.Count());
					Assert(tciLogs.FirstOrDefault().SL_Reference.Contains(consignment1.PK.ToString()));
				});
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItem_WithPGARequirements_WouldNotConvertIfEntryNumIsNotEmpty()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				var clearance = ClearanceForm.BusinessEntity;
				Assert("precondition : no log in clearance ", !clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode).Any());

				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.ULB_HasPGAPending = true;

				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.ULB_HasPGAPending = true;
				consignment2.CE_EntryNum = "EntryNum";

				var convertToStandAloneDeclarationMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertToStandAloneDeclarationMenuItem.OnPopup(EventArgs.Empty);

				var manualSelectionMenuItem = convertToStandAloneDeclarationMenuItem.MenuItems.FindByName("withPGARequirements");
				manualSelectionMenuItem.PerformClick();

				var tciLogs = clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);

				CombineAssertions("would not convert again if entry num is not empty", () =>
				{
					AssertEquals(1, tciLogs.Count());
					Assert(tciLogs.FirstOrDefault().SL_Reference.Contains(consignment1.PK.ToString()));
				});
			}
		}

		public void TestConvertToStandAloneDeclarationMenuItemHouseBillGrid()
		{
			using (InitializeTestingForm())
			{
				var consignment1 = Shipment.CusUSLVConsignments.AddNew();
				var consignment2 = Shipment.CusUSLVConsignments.AddNew();

				AssertNullOrEmpty("precondition: No stand alone declarations created", consignment1.CE_EntryLineReference);
				AssertNullOrEmpty("precondition: No stand alone declarations created", consignment2.CE_EntryLineReference);
				Assert(consignment1.ULB_IsActive);
				Assert(consignment2.ULB_IsActive);

				ClearanceForm.Show();
				Application.DoEvents();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var convertToStandAloneDeclarationMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByName("menuItemConvertToStandAloneDeclaration", false);
				AssertNotNull(convertToStandAloneDeclarationMenuItem);
				AssertEquals("Convert to Stand Alone Declaration", convertToStandAloneDeclarationMenuItem.Text);
				Assert("convertToStandAloneDeclarationMenuItem should show", convertToStandAloneDeclarationMenuItem.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				Environment.Env.Security.USLVClearanceConvertToFormalDeclaration.IsAllowed = false;
				convertToStandAloneDeclarationMenuItem.PerformClick();
				AssertMultilineASCIIEquals(
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
Operate -> Customs -> Low Value Entries -> Convert To Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);

				Environment.Env.Security.USLVClearanceConvertToFormalDeclaration.IsAllowed = true;
				convertToStandAloneDeclarationMenuItem.PerformClick();
				AssertMultilineASCIIEquals(@"2 Stand Alone Declarations queued for processing, check the individual consignment for status", UnitTestUserNotification.Instance.LastMessage.Text);

				var logForConsignment1 = Shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.TransferToCustomsImportsDecCode)).Single(x => x.SL_Reference.Contains(consignment1.PK.ToString()));
				var logForConsignment2 = Shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.TransferToCustomsImportsDecCode)).Single(x => x.SL_Reference.Contains(consignment2.PK.ToString()));
				AssertNotNull("precondition: Stand alone declarations created", logForConsignment1);
				AssertNotNull("precondition: Stand alone declarations created", logForConsignment2);

				houseBillGrid.ContextMenu.DoPopup();
				Assert("convertToStandAloneDeclarationMenuItem should be hidden", !convertToStandAloneDeclarationMenuItem.Visible);

				var consignment3 = Shipment.CusUSLVConsignments.AddNew();
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();
				Assert("convertToStandAloneDeclarationMenuItem should show", convertToStandAloneDeclarationMenuItem.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				convertToStandAloneDeclarationMenuItem.PerformClick();
				AssertMultilineASCIIEquals(
@"3 Consignment(s) have been selected
2 Consignment(s) have previously been converted and will be ignored
1 Consignment(s) will be converted to a Stand Alone Declaration
", UnitTestUserNotification.Instance.LastMessage.Text);

				var logForConsignment3 = Shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.TransferToCustomsImportsDecCode)).Where(x => x.SL_Reference.Contains(consignment3.PK.ToString())).ToArray();
				AssertEquals("No stand alone declaration created for consignment #3 because operation cancelled", 0, logForConsignment3.Length);
			}
		}

		public void TestConsolidatedSummaryMenuItem_WhenNoConsignmentHasEntryTypeInformalFreeDutiable_ShouldHideMenuItem()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "Bill01";

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_EntryType = EntryTypeList.Codes.LowValue;

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_EntryType = EntryTypeList.Codes.LowValue;

			Factory.Save();

			using (InitializeTestingForm(clearance))
			{
				ClearanceForm.Show();

				var messagingMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);

				var createConsolidatedSummaryMenuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");
				var openConsolidatedSummaryMenuItem = messagingMenuItem.MenuItems.FindByText("Open Consolidated Summary");

				CombineAssertions("Consolidated summary menu item should not be created", () =>
				{
					AssertNull(createConsolidatedSummaryMenuItem);
					AssertNull(openConsolidatedSummaryMenuItem);
				});
			}
		}

		public void TestConsolidatedSummaryMenuItem_WhenNoMatchingDeclaration_ShouldShowCreateMenuItem()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "55555";

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_EntryType = EntryTypeList.Codes.LowValue;

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MasterBill = "NOTAMATCH";

			Factory.Save();

			using (InitializeTestingForm(clearance))
			{
				ClearanceForm.Show();
				var messagingMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);

				var createConsolidatedSummaryMenuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");

				CombineAssertions(() =>
				{
					AssertNotNull("Create Menu item exists", createConsolidatedSummaryMenuItem);
					AssertEquals("Create Menu is visible", true, createConsolidatedSummaryMenuItem.Visible);

					createConsolidatedSummaryMenuItem.PerformClick();
					Assert($"type was not as expected: {ZFormModaliser.LastFormShownDialogForTest.GetType()}", ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
				});

				ZFormModaliser.LastFormShownDialogForTest.Close();
			}
		}

		public void TestConsolidatedSummaryMenuItem_WhenHasMatchingDeclaration_ShouldShowOpenMenuItem()
		{
			var matchingJobNumber = "B0001";
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "55555";

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_EntryType = EntryTypeList.Codes.LowValue;

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MasterBill = "44444";
			jobDeclaration.JE_DeclarationReference = matchingJobNumber;

			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, matchingJobNumber));

			Factory.Save();

			using (InitializeTestingForm(clearance))
			{
				ClearanceForm.Show();
				var messagingMenuItem = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);

				var openConsolidatedMenuItem = messagingMenuItem.MenuItems.FindByText("Open Consolidated Summary");

				CombineAssertions(() =>
				{
					AssertNotNull("Open Menu item exists", openConsolidatedMenuItem);
					AssertEquals("Open Menu is visible", true, openConsolidatedMenuItem.Visible);
				});
			}
		}

		public void TestMessagineMenuItemsInHouseBillGridContextMenu()
		{
			using (InitializeTestingForm())
			{
				Shipment.CusUSLVConsignments.AddNew();

				ClearanceForm.Show();
				Application.DoEvents();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);
				var sendReplaceMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Replacement Messages", true);

				AssertNotNull(sendOriginalMessageMenuItem);
				AssertEquals("Send Original Messages", sendOriginalMessageMenuItem.Text);
				Assert("Send Original Messages menu item should be visible", sendOriginalMessageMenuItem.Visible);

				AssertNotNull(sendReplaceMessageMenuItem);
				AssertEquals("Send Replacement Messages", sendReplaceMessageMenuItem.Text);
				Assert("Send Replacement Messages menu item should be visible", sendReplaceMessageMenuItem.Visible);
			}
		}

		public void TestSendOriginalMessages()
		{
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("No message was sent.", "0 original message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

				ClearanceForm.Close();
			}

			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
				clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("Right message", "3 original message(s) generated; 0 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

				ClearanceForm.Close();
			}
		}

		public void TestSendOriginalMessages_WithInvalidConsignmentStatus_AnswerYes()
		{
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.ULB_NumberOfPacks = 1;
				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.ULB_NumberOfPacks = 1;
				var consignment3 = clearance.CusUSLVConsignments.AddNew();
				consignment3.ULB_NumberOfPacks = 1;
				consignment3.ULB_MessageStatus = "CSA";

				var entryNumber = Factory.New<CusEntryNumber>();
				entryNumber.CE_EntryType = "ENS";
				entryNumber.CE_EntryStatus = "REL";
				entryNumber.CE_ParentID = consignment2.PK;
				entryNumber.CE_ParentTable = consignment2.TableName;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("No message was sent.", "0 original message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

				ClearanceForm.Close();
			}

			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.ULB_NumberOfPacks = 1;
				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.ULB_NumberOfPacks = 1;
				var consignment3 = clearance.CusUSLVConsignments.AddNew();
				consignment3.ULB_NumberOfPacks = 1;
				consignment3.ULB_MessageStatus = "CSA";

				var entryNumber = Factory.New<CusEntryNumber>();
				entryNumber.CE_EntryType = "ENS";
				entryNumber.CE_EntryStatus = "REL";
				entryNumber.CE_ParentID = consignment2.PK;
				entryNumber.CE_ParentTable = consignment2.TableName;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("Expected invalid consignment with CSA message status and empty entry status message to be sent", "3 original message(s) generated; 0 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

				ClearanceForm.Close();
			}
		}

		public void TestSendOriginalMessages_WithInvalidConsignmentStatus_AnswerNo()
		{
			InitializeTestingForm();

			var clearance = ClearanceForm.BusinessEntity;
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 1;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;

			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_EntryType = "ENS";
			entryNumber1.CE_EntryStatus = "HLD";
			entryNumber1.CE_ParentID = consignment1.PK;
			entryNumber1.CE_ParentTable = consignment1.TableName;

			var entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.CE_EntryType = "ENS";
			entryNumber2.CE_EntryStatus = "REL";
			entryNumber2.CE_ParentID = consignment2.PK;
			entryNumber2.CE_ParentTable = consignment2.TableName;

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			ClearanceForm.Show();
			var houseBillGrid = ClearanceForm.HouseBillGrid;
			houseBillGrid.SelectAllElements();
			houseBillGrid.ContextMenu.DoPopup();

			var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			sendOriginalMessageMenuItem.PerformClick();
			AssertEquals("Right message", "0 original message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

			ClearanceForm.Close();
		}

		public void TestSendOriginalMessages_FailToGenerateEntryNumber_ShouldBeIgnored()
		{
			InitializeTestingForm();

			var clearance = ClearanceForm.BusinessEntity;
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 1;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10001);

			Factory.Save();

			ClearanceForm.Show();
			var houseBillGrid = ClearanceForm.HouseBillGrid;
			houseBillGrid.SelectAllElements();
			houseBillGrid.ContextMenu.DoPopup();

			var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Original Messages", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			sendOriginalMessageMenuItem.PerformClick();
			AssertEquals("Right message", "0 original message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

			ClearanceForm.Close();
		}

		public void TestSendReplacementMessages()
		{
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.ULB_NumberOfPacks = 1;
				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.ULB_NumberOfPacks = 1;
				var consignment3 = clearance.CusUSLVConsignments.AddNew();
				consignment3.ULB_NumberOfPacks = 1;

				var entryNumber1 = Factory.New<CusEntryNumber>();
				entryNumber1.CE_EntryType = "ENS";
				entryNumber1.CE_EntryStatus = "HLD";
				entryNumber1.CE_ParentID = consignment1.PK;
				entryNumber1.CE_ParentTable = consignment1.TableName;

				var entryNumber2 = Factory.New<CusEntryNumber>();
				entryNumber2.CE_EntryType = "ENS";
				entryNumber2.CE_EntryStatus = "HLD";
				entryNumber2.CE_ParentID = consignment2.PK;
				entryNumber2.CE_ParentTable = consignment2.TableName;

				var entryNumber3 = Factory.New<CusEntryNumber>();
				entryNumber3.CE_EntryType = "ENS";
				entryNumber3.CE_EntryStatus = "HLD";
				entryNumber3.CE_ParentID = consignment3.PK;
				entryNumber3.CE_ParentTable = consignment3.TableName;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Replacement Messages", true);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("No message was sent.", "0 replacement message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

				ClearanceForm.Close();
			}

			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.ULB_NumberOfPacks = 1;
				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.ULB_NumberOfPacks = 1;
				var consignment3 = clearance.CusUSLVConsignments.AddNew();
				consignment3.ULB_NumberOfPacks = 1;

				var entryNumber1 = Factory.New<CusEntryNumber>();
				entryNumber1.CE_EntryType = "ENS";
				entryNumber1.CE_EntryStatus = "HLD";
				entryNumber1.CE_ParentID = consignment1.PK;
				entryNumber1.CE_ParentTable = consignment1.TableName;

				var entryNumber2 = Factory.New<CusEntryNumber>();
				entryNumber2.CE_EntryType = "ENS";
				entryNumber2.CE_EntryStatus = "HLD";
				entryNumber2.CE_ParentID = consignment2.PK;
				entryNumber2.CE_ParentTable = consignment2.TableName;

				var entryNumber3 = Factory.New<CusEntryNumber>();
				entryNumber3.CE_EntryType = "ENS";
				entryNumber3.CE_EntryStatus = "HLD";
				entryNumber3.CE_ParentID = consignment3.PK;
				entryNumber3.CE_ParentTable = consignment3.TableName;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Replacement Messages", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageMenuItem.PerformClick();
				AssertEquals("Right message", "3 replacement message(s) generated; 0 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

				ClearanceForm.Close();
			}
		}

		public void TestSendReplacementMessages_WithInvalidConsignmentStatus_AnswerYes()
		{
			using (InitializeTestingForm())
			{
				var clearance = ClearanceForm.BusinessEntity;
				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.ULB_NumberOfPacks = 1;
				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				consignment2.ULB_NumberOfPacks = 1;
				var consignment3 = clearance.CusUSLVConsignments.AddNew();
				consignment3.ULB_NumberOfPacks = 1;

				var entryNumber1 = Factory.New<CusEntryNumber>();
				entryNumber1.CE_EntryType = "ENS";
				entryNumber1.CE_EntryStatus = "HLD";
				entryNumber1.CE_ParentID = consignment1.PK;
				entryNumber1.CE_ParentTable = consignment1.TableName;

				var entryNumber2 = Factory.New<CusEntryNumber>();
				entryNumber2.CE_EntryType = "ENS";
				entryNumber2.CE_EntryStatus = "REL";
				entryNumber2.CE_ParentID = consignment2.PK;
				entryNumber2.CE_ParentTable = consignment2.TableName;

				clearance.ULH_EntryFilerCode = "XJ5";
				var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
				stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

				Factory.Save();

				ClearanceForm.Show();
				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Replacement Messages", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageMenuItem.PerformClick();

				AssertContains("Right message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Right message", "3 replacement message(s) generated; 0 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

				ClearanceForm.Close();
			}
		}

		public void TestSendReplacementMessages_WithInvalidConsignmentStatus_AnswerNo()
		{
			InitializeTestingForm();

			var clearance = ClearanceForm.BusinessEntity;
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 1;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;

			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_EntryType = "ENS";
			entryNumber1.CE_EntryStatus = "HLD";
			entryNumber1.CE_ParentID = consignment1.PK;
			entryNumber1.CE_ParentTable = consignment1.TableName;

			var entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.CE_EntryType = "ENS";
			entryNumber2.CE_EntryStatus = "REL";
			entryNumber2.CE_ParentID = consignment2.PK;
			entryNumber2.CE_ParentTable = consignment2.TableName;

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			ClearanceForm.Show();
			var houseBillGrid = ClearanceForm.HouseBillGrid;
			houseBillGrid.SelectAllElements();
			houseBillGrid.ContextMenu.DoPopup();

			var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Replacement Messages", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			sendOriginalMessageMenuItem.PerformClick();

			AssertEquals("Right message", @"There is no ACE Cargo Release acceptance message on file for one of the selected Consignments. A replace will be rejected if it was never accepted at ABI.
Would you like to continue?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Right message", "0 replacement message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

			ClearanceForm.Close();
		}

		public void TestSendReplacementMessages_FailToGenerateEntryNumber_ShouldBeIgnored()
		{
			InitializeTestingForm();

			var clearance = ClearanceForm.BusinessEntity;
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 1;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10001);

			Factory.Save();

			ClearanceForm.Show();
			var houseBillGrid = ClearanceForm.HouseBillGrid;
			houseBillGrid.SelectAllElements();
			houseBillGrid.ContextMenu.DoPopup();

			var sendOriginalMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Replacement Messages", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			sendOriginalMessageMenuItem.PerformClick();
			AssertEquals("Right message", "0 replacement message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);

			ClearanceForm.Close();
		}

		public void TestSendDeletionMessages()
		{
			InitializeTestingForm();

			var clearance = ClearanceForm.BusinessEntity;
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 1;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;

			consignment1.CE_EntryStatus = "HLD";
			consignment2.CE_EntryStatus = "HLD";
			consignment3.CE_EntryStatus = "HLD";

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			ClearanceForm.Show();
			var houseBillGrid = ClearanceForm.HouseBillGrid;
			houseBillGrid.SelectAllElements();
			houseBillGrid.ContextMenu.DoPopup();

			var sendDeleteMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Deletion Messages", true);

			ZFormModaliser.ShowDialogsInTest = true;
			using (ZFormModaliser.SuspendDispose())
			{
				sendDeleteMessageMenuItem.PerformClick();

				var messagesSubmitForm = ZFormModaliser.LastFormShownDialogForTest;
				CombineAssertions("Should show MessagesSubmitForm", () =>
				{
					AssertNotNull(messagesSubmitForm);
					AssertType<MessagesSubmitForm>(messagesSubmitForm);
				});

				var clearanceWrapper = ((MessagesSubmitForm)messagesSubmitForm).BusinessEntity;
				AssertEquals("MessagesSubmitForm should list 3 selected bills", 3, clearanceWrapper.CusUSLVConsignmentsToSend.Count);
			}

			ClearanceForm.Close();
		}

		public void TestSendDeletionMessages_WithInvalidConsignmentStatus_ShouldBeIgnored()
		{
			InitializeTestingForm();

			var clearance = ClearanceForm.BusinessEntity;
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 1;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;
			consignment3.ULB_MessageStatus = "CSA";

			consignment1.CE_EntryStatus = "HLD";
			consignment2.CE_EntryStatus = "HLD";
			consignment3.CE_EntryStatus = "DEL";

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			ClearanceForm.Show();
			var houseBillGrid = ClearanceForm.HouseBillGrid;
			houseBillGrid.SelectAllElements();
			houseBillGrid.ContextMenu.DoPopup();

			var sendDeleteMessageMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Send Deletion Messages", true);

			ZFormModaliser.ShowDialogsInTest = true;
			using (ZFormModaliser.SuspendDispose())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendDeleteMessageMenuItem.PerformClick();

				var messagesSubmitForm = ZFormModaliser.LastFormShownDialogForTest;
				CombineAssertions("Should show MessagesSubmitForm", () =>
				{
					AssertNotNull(messagesSubmitForm);
					AssertType<MessagesSubmitForm>(messagesSubmitForm);
				});

				var clearanceWrapper = ((MessagesSubmitForm)messagesSubmitForm).BusinessEntity;
				AssertEquals("MessagesSubmitForm should list 2 selected bills", 2, clearanceWrapper.CusUSLVConsignmentsToSend.Count);
				AssertContainsExactElementsInAnyOrder("Consignment3 should be ignored",
					new[] { consignment1.PK, consignment2.PK },
					clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Select(x => x.Consignment.PK));
			}

			ClearanceForm.Close();
		}

		public void TestHouseBillGridCertainColumnsAreUpperCased()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				var houseBillGridColumnNamesShouldBeUpperCase = new[]
				{
					AutoCusUSLVConsignment.Schema.ULB_HouseBill,
					AutoCusUSLVConsignment.Schema.ULB_OwnerReferenceNumber,
					AutoCusUSLVConsignment.Schema.ULB_EquipmentNumber,
					"CE_RailReferenceNumber",
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeQualifier,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeName,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeAddress1,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeAddress2,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeCity,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeIdentifier,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneePostCode,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeState,
					AutoCusUSLVConsignment.Schema.ULB_RN_NKConsigneeCountry,
					AutoCusUSLVConsignment.Schema.ULB_SellerName,
					AutoCusUSLVConsignment.Schema.ULB_SellerAddress1,
					AutoCusUSLVConsignment.Schema.ULB_SellerAddress2,
					AutoCusUSLVConsignment.Schema.ULB_SellerCity,
					AutoCusUSLVConsignment.Schema.ULB_SellerPostCode,
					AutoCusUSLVConsignment.Schema.ULB_SellerState,
					AutoCusUSLVConsignment.Schema.ULB_RN_NKSellerCountry,
					"ITNumber"
				};

				var itemGridColumnNamesShouldBeUpperCase = new[]
				{
					AutoCusUSLVItem.Schema.ULI_GoodsDescription
				};

				CombineAssertions(() =>
				{
					foreach (var columnName in houseBillGridColumnNamesShouldBeUpperCase)
					{
						AssertColumnIsUpperCasing(columnName, ClearanceForm.HouseBillGrid);
					}

					foreach (var columnName in itemGridColumnNamesShouldBeUpperCase)
					{
						AssertColumnIsUpperCasing(columnName, ClearanceForm.GridCommodityDetail);
					}
				});

				void AssertColumnIsUpperCasing(string columnName, ZGrid grid)
				{
					var columnStyleInfo = grid.ColumnStyles.ToArray().OfType<ZGridColumnInfo>().Single(c => c.ColumnName == columnName);
					AssertEquals($"Column '{columnName}' should be uppercase", CharacterCasing.Upper, columnStyleInfo.CharacterCasing);
				}
			}
		}

		public void TestHouseBillGridVisibleColumnsByDefault()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				var houseBillGridVisibleColumnsByDefault = new[]
				{
					AutoCusUSLVConsignment.Schema.ULB_HouseBillIssuerSCAC,
					AutoCusUSLVConsignment.Schema.ULB_HouseBill,
					AutoCusUSLVConsignment.Schema.ULB_EntryType,
					CusUSLVConsignment.Schema.FirstCusUSLVItemProductCode,
					CusUSLVConsignment.Schema.FirstCusUSLVItemTariff,
					CusUSLVConsignment.Schema.FirstCusUSLVItemGoodsDescription,
					CusUSLVConsignment.Schema.FirstCusUSLVItemLineValue,
					CusUSLVConsignment.Schema.FirstCusUSLVItemCurrency,
					CusUSLVConsignment.Schema.FirstCusUSLVItemExchangeRate,
					CusUSLVConsignment.Schema.FirstCusUSLVItemCountryOfOrigin,
					CusUSLVConsignment.Schema.FirstCusUSLVItemAntiDumping,
					CusUSLVConsignment.Schema.FirstCusUSLVItemCountervailing,
					AutoCusUSLVConsignment.Schema.ULB_SubmittedDate,
					AutoCusEntryNum.Schema.CE_IssueDate,
					AutoCusEntryNum.Schema.CE_EntryStatus,
					AutoCusUSLVConsignment.Schema.ULB_MessageStatus,
					AutoCusEntryNum.Schema.CE_EntryNum,
					AutoCusUSLVConsignment.Schema.ULB_OwnerReferenceNumber,
					AutoCusUSLVConsignment.Schema.ULB_EquipmentNumber,
					CusUSLVConsignment.Schema.ULB_GoodsValue,
					CusUSLVConsignment.Schema.ULB_Currency,
					AutoCusUSLVConsignment.Schema.ULB_NumberOfPacks,
					AutoCusUSLVConsignment.Schema.ULB_PackType,
					AutoCusUSLVConsignment.Schema.ULB_NonAMSIndicator,
					AutoCusUSLVConsignment.Schema.ULB_DISIndicator,
					CusUSLVConsignment.Schema.ConsigneeOrgPK,
					AutoCusUSLVConsignment.Schema.ULB_OA_Consignee,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeQualifier,
					AutoCusUSLVConsignment.Schema.ULB_ConsigneeIdentifier,
					CusUSLVConsignment.Schema.SellerOrgPK,
					AutoCusUSLVConsignment.Schema.ULB_OA_Seller,
					AutoCusUSLVConsignment.Schema.ULB_SellerIdentifier,
				};

				var columnNames = ClearanceForm.HouseBillGrid.ColumnStyles.ToArray().OfType<ZGridColumnInfo>().Where(col => col.IsVisible).Select(col => col.ColumnName);
				AssertContainsExactElementsInAnyOrder(columnNames, houseBillGridVisibleColumnsByDefault);
			}
		}

		public void TestHouseBillGridReleaseDateColumnOnlyShowDate()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();

				var releaseDateColumnStyleInfo = ClearanceForm.HouseBillGrid.ColumnStyles.ToArray().OfType<ZGridColumnInfo>().Single(c => c.ColumnName == "CE_IssueDate") as ZDateEditColumnStyleInfo;
				AssertEquals("Date time format should be short", ZDateTimePickerFormat.Short, releaseDateColumnStyleInfo.DateTimeFormat);
			}
		}

		public void TestContainerModeHidesWhenTransportModeIsMail()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();

				var clearance = ClearanceForm.BusinessEntity;
				clearance.ULH_TransportMode = TransportModes.Sea;
				Assert(ClearanceForm.DropEditContainerMode.Visible);

				clearance.ULH_TransportMode = TransportModes.Mail;
				Assert(!ClearanceForm.DropEditContainerMode.Visible);

				clearance.ULH_TransportMode = TransportModes.Rail;
				Assert(ClearanceForm.DropEditContainerMode.Visible);

				clearance.ULH_TransportMode = TransportModes.Air;
				Assert(ClearanceForm.DropEditContainerMode.Visible);

				clearance.ULH_TransportMode = TransportModes.Truck;
				Assert(ClearanceForm.DropEditContainerMode.Visible);
			}
		}

		public void TestConsignmentEntryLineReferenceColumn()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();
				var entryLineReferenceColumnInfo = ClearanceForm.HouseBillGrid.GetColumnStyle("CE_EntryLineReference");
				Assert("Should be invisible by default", !entryLineReferenceColumnInfo.IsVisible);
				AssertEquals(100, entryLineReferenceColumnInfo.Width);
			}
		}

		public void TestPGARequirementsControlBindings()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				ClearanceForm.HousebillMessagesTabPage.Show();
				AssertEquals("CusUSLVConsignments.CusUSLVItems", ClearanceForm.PGARequirementsControl.GetBindingMember());
			}
		}

		public void TestMiscTabPage()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();
				var miscTabPage = ClearanceForm.MiscTabPage;
				AssertEquals("Misc", miscTabPage.Text);
			}
		}

		public void TestFilerTextBox()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();
				var miscTabPage = ClearanceForm.MiscTabPage;
				ClearanceForm.RightTabControl.SelectedTab = miscTabPage;
				var filerBox = miscTabPage.Controls.Find("textBoxFiler", true).OfType<ZTextBox>().Single();
				AssertEquals(3, filerBox.MaxLength);
				AssertEquals(35, filerBox.Width);
				Assert(filerBox.ReadOnly);
			}
		}

		public void TestFormMinimumSize()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();
				AssertEquals(new System.Drawing.Size(1366, 725), ClearanceForm.MinimumSize);
			}
		}

		public void TestBottomTabControlHeightAdjustable()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();

				AssertEquals(230, ClearanceForm.SplitterBottom.MinSize);
			}
		}

		public void TestSplitterBottomMinExtra()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();

				AssertEquals(90, ClearanceForm.SplitterBottom.MinExtra);
			}
		}

		public void TestSplitContainerHouseBillDetailsPanelMinSize()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();

				AssertEquals(74, ClearanceForm.SplitContainer.Panel1MinSize);
				AssertEquals(74, ClearanceForm.SplitContainer.Panel2MinSize);
			}
		}

		public void TestBottomTabControlHeight()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();

				AssertEquals(400, ClearanceForm.BottomTabControl.Size.Height);
			}
		}

		public void TestImportMessagesUserControl()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();
				ClearanceForm.BottomTabControl.SelectTab(ClearanceForm.HousebillMessagesTabPage);

				Assert("Message header grid should be hidden", !ClearanceForm.ImportMessagesUserControl.ShowMessageHeaderGrid);
			}
		}

		public void TestTransportDetailsUserControlBindings()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				var transportDetailsControl = ClearanceForm.Controls.Find("transportDetailsControl", true)[0] as TransportDetailsUserControl;
				AssertNotNull(transportDetailsControl);

				AssertControlHasKBinding("panelTransportDetailGroup", nameof(CusUSLVClearance.IsNotEmptyForBinding));
				AssertControlHasKBinding("textBoxFlightNo", nameof(CusUSLVClearance.IsAirForBinding));
				AssertControlHasKBinding("masterBillControl", nameof(CusUSLVClearance.IsAirForBinding));
				AssertControlHasKBinding("codeFindBoxVessel", nameof(CusUSLVClearance.IsSeaForBinding));
				AssertControlHasKBinding("textBoxVoyageNo", nameof(CusUSLVClearance.IsSeaForBinding));
				AssertControlHasKBinding("textBoxOceanBill", nameof(CusUSLVClearance.IsSeaForBinding));
				AssertControlHasKBinding("textBoxMailReference", nameof(CusUSLVClearance.IsMailForBinding));
				AssertControlHasKBinding("textBoxJourney", nameof(CusUSLVClearance.IsRailForBinding));
				AssertControlHasKBinding("textBoxMasterBill", nameof(CusUSLVClearance.IsRailOrRoadForBinding));
				AssertControlHasKBinding("textBoxTripID", nameof(CusUSLVClearance.IsRailOrRoadOrMailForBinding));
				AssertControlHasKBinding("dropEditContainerMode", nameof(CusUSLVClearance.IsContainerSupported));

				void AssertControlHasKBinding(string controlName, string bindingMember)
				{
					var fieldInfo = typeof(TransportDetailsUserControl).GetField(controlName, BindingFlags.NonPublic | BindingFlags.Instance);
					var control = fieldInfo.GetValue(transportDetailsControl) as Control;
					var binding = control.DataBindings["IsVisibleForBinding"] as KBinding;
					AssertEquals(bindingMember, binding.BindingMemberInfo.BindingMember);
				}
			}
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			var unLOCO = Factory.New<RefUNLOCO>();
			unLOCO.RL_Code = "!ZZ22";
			unLOCO.RL_PortName = "Crystal Lawns";
			CreateRefLocoMap("4001", "!ZZ22", USLocoMapSystemUsageList.Codes.Sea);
			CreateRefLocoMap("4002", "!ZZ22", USLocoMapSystemUsageList.Codes.Sea);
			CreateRefLocoMap("4003", "!ZZ22", USLocoMapSystemUsageList.Codes.Air);
			CreateRefLocoMap("60001", "!ZZ22", USLocoMapSystemUsageList.Codes.SCK);
			CreateRefLocoMap("60002", "!ZZ22", USLocoMapSystemUsageList.Codes.SCK);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4001", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4002", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4003", "Test Name", startDate, endDate);
			Factory.Save();

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = "SEA";
			using (InitializeTestingForm(clearance))
			{
				ClearanceForm.Show();
				AssertEquals(false, ClearanceForm.FindSingle<ZDropEdit>("dropEditLoadingPort").Visible);
				AssertEquals(true, ClearanceForm.FindSingle<ZCodeFindBox>("codeFindBoxLoadingPort").Visible);

				AssertEquals(false, ClearanceForm.FindSingle<ZDropEdit>("dropEditDischargePort").Visible);
				AssertEquals(true, ClearanceForm.FindSingle<ZCodeFindBox>("codeFindBoxDischargePort").Visible);

				clearance.ULH_RL_NKPortOfLoading = "!ZZ22";
				AssertEquals(true, ClearanceForm.FindSingle<ZDropEdit>("dropEditLoadingPort").Visible);
				AssertEquals(false, ClearanceForm.FindSingle<ZCodeFindBox>("codeFindBoxLoadingPort").Visible);

				clearance.ULH_RL_NKPortOfDischarge = "!ZZ22";
				AssertEquals(true, ClearanceForm.FindSingle<ZDropEdit>("dropEditDischargePort").Visible);
				AssertEquals(false, ClearanceForm.FindSingle<ZCodeFindBox>("codeFindBoxDischargePort").Visible);

				clearance.ULH_TransportMode = "AIR";
				AssertEquals(false, ClearanceForm.FindSingle<ZDropEdit>("dropEditDischargePort").Visible);
				AssertEquals(true, ClearanceForm.FindSingle<ZCodeFindBox>("codeFindBoxDischargePort").Visible);
			}
		}

		void CreateRefLocoMap(string localPortCode, string locoPort, string usage)
		{
			var locoMapping = Factory.New<RefLocoMap>();
			locoMapping.RY_LocalPortCode = localPortCode;
			locoMapping.RY_RL_NKLocoPort = locoPort;
			locoMapping.RY_SystemUsage = usage;
			locoMapping.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping.RY_IsSystem = false;
		}

		public void TestCommodityDetailsGrid()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				ClearanceForm.HousebillMessagesTabPage.Show();
				AssertEquals(true, ClearanceForm.GridCommodityDetail.Visible);
				AssertEquals(9, ClearanceForm.GridCommodityDetail.Columns.Count);
				AssertEquals("Product Code", ClearanceForm.GridCommodityDetail.Columns[0].ColumnStyle.HeaderText);
				AssertEquals(false, ClearanceForm.GridCommodityDetail.Columns[0].ColumnStyle.ReadOnly);
				AssertEquals("Tariff", ClearanceForm.GridCommodityDetail.Columns[1].ColumnStyle.HeaderText);
				AssertEquals(false, ClearanceForm.GridCommodityDetail.Columns[1].ColumnStyle.ReadOnly);
				AssertEquals("Goods Description", ClearanceForm.GridCommodityDetail.Columns[2].ColumnStyle.HeaderText);
				AssertEquals(false, ClearanceForm.GridCommodityDetail.Columns[2].ColumnStyle.ReadOnly);
				AssertEquals("Line Value", ClearanceForm.GridCommodityDetail.Columns[3].ColumnStyle.HeaderText);
				AssertEquals(false, ClearanceForm.GridCommodityDetail.Columns[3].ColumnStyle.ReadOnly);
				AssertEquals("Line Currency", ClearanceForm.GridCommodityDetail.Columns[4].ColumnStyle.HeaderText);
				AssertEquals(false, ClearanceForm.GridCommodityDetail.Columns[4].ColumnStyle.ReadOnly);
				AssertEquals("Exchange Rate", ClearanceForm.GridCommodityDetail.Columns[5].ColumnStyle.HeaderText);
				AssertEquals(true, ClearanceForm.GridCommodityDetail.Columns[5].ColumnStyle.ReadOnly);
				AssertEquals("Ctry/Rgn. of Origin", ClearanceForm.GridCommodityDetail.Columns[6].ColumnStyle.HeaderText);
				AssertEquals(false, ClearanceForm.GridCommodityDetail.Columns[6].ColumnStyle.ReadOnly);
				AssertEquals("ADD N/A", ClearanceForm.GridCommodityDetail.Columns[7].ColumnStyle.HeaderText);
				AssertEquals(false, ClearanceForm.GridCommodityDetail.Columns[7].ColumnStyle.ReadOnly);
				AssertEquals("CVD N/A", ClearanceForm.GridCommodityDetail.Columns[8].ColumnStyle.HeaderText);
				AssertEquals(false, ClearanceForm.GridCommodityDetail.Columns[8].ColumnStyle.ReadOnly);
			}
		}

		public void TestDispositionGrid()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				ClearanceForm.HousebillSummaryTabPage.Show();
				AssertEquals("Should show disposition grid", true, ClearanceForm.GridDisposition.Visible);
				AssertEquals("Should have 6 columns", 6, ClearanceForm.GridDisposition.Columns.Count);
				AssertEquals("Code ID", ClearanceForm.GridDisposition.Columns[0].ColumnStyle.HeaderText);
				AssertEquals(true, ClearanceForm.GridDisposition.Columns[0].ColumnStyle.ReadOnly);
				AssertEquals("Narrative", ClearanceForm.GridDisposition.Columns[1].ColumnStyle.HeaderText);
				AssertEquals(true, ClearanceForm.GridDisposition.Columns[1].ColumnStyle.ReadOnly);
				AssertEquals("Date", ClearanceForm.GridDisposition.Columns[2].ColumnStyle.HeaderText);
				AssertEquals(true, ClearanceForm.GridDisposition.Columns[2].ColumnStyle.ReadOnly);
				AssertEquals("Release Date", ClearanceForm.GridDisposition.Columns[3].ColumnStyle.HeaderText);
				AssertEquals(true, ClearanceForm.GridDisposition.Columns[3].ColumnStyle.ReadOnly);
				AssertEquals("Release Origin", ClearanceForm.GridDisposition.Columns[4].ColumnStyle.HeaderText);
				AssertEquals(true, ClearanceForm.GridDisposition.Columns[4].ColumnStyle.ReadOnly);
				AssertEquals("Release Origin Description", ClearanceForm.GridDisposition.Columns[5].ColumnStyle.HeaderText);
				AssertEquals(true, ClearanceForm.GridDisposition.Columns[5].ColumnStyle.ReadOnly);
			}
		}

		public void TestMessagingMenuItem()
		{
			using (InitializeTestingForm())
			{
				var messagingMenuItems = ClearanceForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems;
				AssertEquals(7, messagingMenuItems.Count);
				AssertEquals("Send Original Messages", messagingMenuItems[0].Text);
				AssertEquals("Send Replacement Messages", messagingMenuItems[1].Text);
				AssertEquals("Send Update Messages", messagingMenuItems[2].Text);
				AssertEquals("Send Deletion Messages", messagingMenuItems[3].Text);
				AssertEquals("Convert to Stand Alone Declaration", messagingMenuItems[5].Text);
			}
		}

		public void TestDisclaimApplicablePGAsMenuItem()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2853000059";
			tariff.UE_PGACodes = "FD1";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "HB111111";
			consignment.CusUSLVItems.AddNew().ULI_Tariff = "2853000059";
			Factory.Save();

			using (InitializeTestingForm(clearance))
			{
				ClearanceForm.Show();
				Application.DoEvents();

				var actionsMenuItem = ClearanceForm.ActionsMenuItem;
				var item = actionsMenuItem.MenuItems.FindByText(DisclaimApplicablePGAsHelper.DisclaimApplicablePGAsMenuCaption);
				AssertNotNull("Menu action '" + DisclaimApplicablePGAsHelper.DisclaimApplicablePGAsMenuCaption + "' should not be null.", item);
				item.PerformClick();

				using (var disclaimApplicablePGAsForm = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(disclaimApplicablePGAsForm);
					AssertType<DisclaimApplicablePGAsChildForm>(disclaimApplicablePGAsForm);
					AssertType<DisclaimApplicablePGAsApplicator>(disclaimApplicablePGAsForm.LastDataSourceForTest);
				}
			}
		}

		public void TestDisplayFirstCommodityLineInConsignmentGrid()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_DepartureDate = ZDateTime.Now.Date;
			var consignment = clearance.CusUSLVConsignments.AddNew();

			var uSLVitem1 = consignment.CusUSLVItems.AddNew();
			uSLVitem1.ULI_Tariff = "1122.33.44";
			uSLVitem1.ULI_GoodsDescription = "Test1";
			uSLVitem1.ULI_GoodsValue = 2;
			uSLVitem1.ULI_RX_NKCurrency = "USD";
			uSLVitem1.ULI_RN_NKCountryOfOrigin = "AU";
			uSLVitem1.ULI_AntiDumping = true;
			uSLVitem1.ULI_Countervailing = true;

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var houseBillGrid = form.Controls.Find("gridHouseBills", true)[0] as ZGrid;
				var houseBillGridColumns = houseBillGrid.Columns;
				var houseBillGridColumnsHeaderTexts = houseBillGridColumns.Select(n => n.ColumnStyle.HeaderText);
				var commodityDetailsGridGrid = form.Controls.Find("gridCommodityDetails", true)[0] as ZGrid;
				var commodityDetailsGridColumns = commodityDetailsGridGrid.Columns;

				var currentConsignment = houseBillGrid.GetCurrent() as CusUSLVConsignment;

				CombineAssertions("ValueOfFirstCommodityLine", () =>
				{
					AssertEquals("1122.33.44", currentConsignment.FirstCusUSLVItem.ULI_TariffFormatted);
					AssertEquals("Test1", currentConsignment.FirstCusUSLVItem.ULI_GoodsDescription);
					AssertEquals(2m, currentConsignment.FirstCusUSLVItem.ULI_GoodsValue);
					AssertEquals("USD", currentConsignment.FirstCusUSLVItem.ULI_RX_NKCurrency);
					AssertEquals("AU", currentConsignment.FirstCusUSLVItem.ULI_RN_NKCountryOfOrigin);
					AssertEquals(uSLVitem1.Currency.GetCustomsRate(ZDateTime.Now.Date), currentConsignment.FirstCusUSLVItem.ULI_RX_NKCurrEXRate);
					AssertEquals(true, currentConsignment.FirstCusUSLVItem.ULI_AntiDumping);
					AssertEquals(true, currentConsignment.FirstCusUSLVItem.ULI_Countervailing);
				});

				var currentUSLVItem = commodityDetailsGridGrid.GetCurrent() as CusUSLVItem;

				currentConsignment.FirstCusUSLVItem.ULI_TariffFormatted = "2211.44.33";
				AssertEquals("2211.44.33", currentUSLVItem.ULI_TariffFormatted);

				currentConsignment.FirstCusUSLVItem.ULI_GoodsDescription = "Test2";
				AssertEquals("Test2", currentUSLVItem.ULI_GoodsDescription);

				currentConsignment.FirstCusUSLVItem.ULI_GoodsValue = 3;
				AssertEquals(3m, currentUSLVItem.ULI_GoodsValue);

				currentConsignment.FirstCusUSLVItem.ULI_RX_NKCurrency = "AUD";
				AssertEquals("AUD", currentUSLVItem.ULI_RX_NKCurrency);

				currentConsignment.FirstCusUSLVItem.ULI_RN_NKCountryOfOrigin = "US";
				AssertEquals("US", currentUSLVItem.ULI_RN_NKCountryOfOrigin);

				currentConsignment.FirstCusUSLVItem.ULI_AntiDumping = false;
				AssertEquals(false, currentUSLVItem.ULI_AntiDumping);

				currentConsignment.FirstCusUSLVItem.ULI_Countervailing = false;
				AssertEquals(false, currentUSLVItem.ULI_Countervailing);

				currentUSLVItem.ULI_TariffFormatted = "3311.44.33";
				AssertEquals("3311.44.33", currentConsignment.FirstCusUSLVItem.ULI_TariffFormatted);

				currentUSLVItem.ULI_GoodsDescription = "Test3";
				AssertEquals("Test3", currentConsignment.FirstCusUSLVItem.ULI_GoodsDescription);

				currentUSLVItem.ULI_GoodsValue = 4;
				AssertEquals(4m, currentConsignment.FirstCusUSLVItem.ULI_GoodsValue);

				currentUSLVItem.ULI_RX_NKCurrency = "USD";
				AssertEquals("USD", currentConsignment.FirstCusUSLVItem.ULI_RX_NKCurrency);

				currentUSLVItem.ULI_RN_NKCountryOfOrigin = "NZ";
				AssertEquals("NZ", currentConsignment.FirstCusUSLVItem.ULI_RN_NKCountryOfOrigin);

				currentUSLVItem.ULI_AntiDumping = true;
				AssertEquals(true, currentConsignment.FirstCusUSLVItem.ULI_AntiDumping);

				currentUSLVItem.ULI_Countervailing = true;
				AssertEquals(true, currentConsignment.FirstCusUSLVItem.ULI_Countervailing);
			}
		}

		public void TestOpenConsignmentFormByDoubleClickHouseBillsGridRow()
		{
			using (InitializeTestingForm())
			{
				var consignment = Shipment.CusUSLVConsignments.AddNew();
				ClearanceForm.Show();

				var grid = (ZGrid)ClearanceForm.Controls.Find("gridHouseBills", true)[0];
				grid.Select(0);

				var doubleClick = typeof(CusUSLVClearanceForm).GetMethod("gridHouseBills_DoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				doubleClick.Invoke(ClearanceForm, new object[] { null, EventArgs.Empty });

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(CusUSLVConsignmentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestJobComInvoiceLinePartSynchronisationManager()
		{
			var factory = new BusinessObjectFactory();
			var importer = factory.NewWithValidTestData<OrgHeader>();
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			var part = factory.New<MasterFiles.Business.OrgSupplierPart>();
			part.RelatedOrganisations.AddOwner(importer);
			part.RelatedOrganisations.AddSupplier(supplier);
			part.OP_PartNum = "P001";
			part.OP_Desc = "DESC";
			factory.Save();

			var shipment = Factory.New<CusUSLVClearance>();
			var cusUSLVConsignment = shipment.CusUSLVConsignments.AddNew();
			cusUSLVConsignment.SellerOrgPK = supplier.PK;
			var cusUSLVItem = cusUSLVConsignment.CusUSLVItems.AddNew();
			cusUSLVItem.ULI_PartNo = part.OP_PartNum;
			AssertEquals(part.OP_Desc, cusUSLVItem.ULI_GoodsDescription);

			part.OP_Desc = "DESC1";
			factory.Save();
			AssertNotEquals(part.OP_Desc, cusUSLVItem.ULI_GoodsDescription);

			using (var form = new CusUSLVClearanceFormForTest(shipment))
			{
				form.Show();

				part.OP_Desc = "DESC2";
				factory.Save();
				AssertEquals(part.OP_Desc, cusUSLVItem.ULI_GoodsDescription);
			}

			part.OP_Desc = "DESC3";
			factory.Save();
			AssertNotEquals(part.OP_Desc, cusUSLVItem.ULI_GoodsDescription);
		}

		public void TestFormHeading_New()
		{
			var newClearance = Factory.New<CusUSLVClearance>();
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.US.USLowValueEntries);
			using (var formForNewEntity = controller.ShowFormOfGivenDisplayType(newClearance, ODisplayMode.New) as ZForm)
			{
				AssertEquals("New Low Value Entries", formForNewEntity.FormHeading);
			}
		}

		#region Consolidated Summary

		public void TestConsolidatedSummaryMenuItems()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EntryType = EntryTypeList.Codes.LowValue;

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");

				AssertNull("Menu item should not be visible as there is no eligible consignment", menuItem);

				consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
				messagingMenuItem.OnPopup(null);
				menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");

				AssertNotNull("Create Consolidated Summary menu item should show when there is eligible consignment", menuItem);

				var matchingJobNumber = "B0001";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MasterBill = "55555";
				declaration.JE_DeclarationReference = matchingJobNumber;
				clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, matchingJobNumber));

				Factory.Save();

				messagingMenuItem.OnPopup(null);
				menuItem = messagingMenuItem.MenuItems.FindByText("Open Consolidated Summary");

				AssertNotNull("Open Consolidated Summary menu item should show when the clearance has consolidated summary created", menuItem);
			}
		}

		public void TestCreateConsolidatedSummary_SaveFormFirst()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_ContactName = "Contact";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			Assert("Precondition: clearance Has Changes", clearance.HasChanges);

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");

				UnitTestUserNotification.Instance.AddOKAnswer();
				menuItem.PerformClick();

				AssertEquals("Please save the form before converting.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateConsolidatedSummary_MatchedJobDeclaration()
		{
			var matchingJobNumber = "B0001";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "55555";
			declaration.JE_DeclarationReference = matchingJobNumber;

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = "12345";
			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, matchingJobNumber));

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			Factory.Save();

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Open Consolidated Summary");
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Matched declaration is shown", declaration.PK, ZFormModaliser.LastIBusinessShownOnDialogForTest.Identifier);
					AssertType<JobDeclarationForm>(ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Browse form is shown", ODisplayMode.Browse, (ZFormModaliser.LastFormShownDialogForTest as IZForm).DisplayMode);
				});
			}
		}

		public void TestCreateConsolidatedSummary_UnMatchedJobDeclaration()
		{
			var masterBillNumber = "12345";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = masterBillNumber;

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = masterBillNumber;

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			Factory.Save();

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertNotEquals("New declaration is shown", declaration.PK, ZFormModaliser.LastIBusinessShownOnDialogForTest.Identifier);
					AssertType<JobDeclarationForm>(ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("New form is shown", ODisplayMode.New, (ZFormModaliser.LastFormShownDialogForTest as IZForm).DisplayMode);
				});
			}
		}

		public void TestCreateConsolidatedSummary_Mapping()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			JobDeclaration declaration = default;
			BaseJobComInvoiceHeader headerAUD = default;
			ZFormModaliser.SetDelegateToCallOnFormShown((form) =>
			{
				if (form is JobDeclarationForm declarationForm)
				{
					declaration = declarationForm.BusinessEntity as JobDeclaration;
					headerAUD = declaration.Invoices.Single(x => x.JZ_RX_NKInvoice_Currency == "AUD");
					declaration.Factory.Save();
				}
			});

			var importerOH = Factory.NewWithValidTestData<OrgHeader>();
			var sellerOA = Factory.NewWithValidTestData<OrgAddress>();
			sellerOA.Header.OH_IsConsignor = true;
			var consigneeOA = Factory.NewWithValidTestData<OrgAddress>();
			consigneeOA.Header.OH_IsConsignee = true;

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_RL_NKPortOfLoading = "AUSYD";
			clearance.ULH_RL_NKPortOfDischarge = "USLAX";
			clearance.ULH_OH_Importer = importerOH.PK;
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			consignment1.ULB_OA_Consignee = consigneeOA.PK;
			consignment1.ULB_OA_Seller = sellerOA.PK;
			var commodity1 = consignment1.CusUSLVItems.AddNew();
			commodity1.ULI_RX_NKCurrency = "AUD";

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			var commodity2 = consignment2.CusUSLVItems.AddNew();
			var commodity3 = consignment2.CusUSLVItems.AddNew();

			Factory.Save();

			using (USCustomsDataRegistry.Instance.DoDefaultShipTo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertType<JobDeclarationForm>(ZFormModaliser.LastFormShownDialogForTest);
					Assert("Declaration is consolidated", declaration.US_ConsolACE);
					AssertEquals("Entry type", "11", declaration.US_EntryType);
					Assert("Enable Ent Sum", declaration.US_EnableENS);
					AssertEquals("1 Invoice Header per consignment", 2, declaration.Invoices.Count);
					AssertEquals("1 Invoice Line per Commodity", 3, declaration.InvoiceLines.Count);
					AssertEquals("Importer", importerOH.PK, declaration.JE_OH_Importer);
					AssertEquals("Manufacturer", sellerOA.PK, headerAUD.JZ_OA_ManufacturerAddress);
					AssertEquals("Supplier", sellerOA.PK, headerAUD.JZ_OA_SupplierAddress);
					AssertEquals("Seller", sellerOA.PK, headerAUD.JZ_OA_SellerAddress);
					AssertEquals("Exporter", sellerOA.PK, headerAUD.JZ_OA_ExporterAddress);
					AssertEquals("Consignee", consigneeOA.PK, headerAUD.JZ_OA_ConsigneeAddress);
					AssertEquals("SoldTo", consigneeOA.PK, headerAUD.JZ_OA_SoldToPartyAddress);
					AssertEquals("ShipTo", consigneeOA.PK, headerAUD.JZ_OA_ShipToPartyAddress);
				});
			}
		}

		public void TestCreateConsolidatedSummary_OnSaving()
		{
			JobDeclaration declaration = default;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown((form) =>
			{
				if (form is JobDeclarationForm declarationForm)
				{
					declarationForm.FireSaveButton();
					declaration = declarationForm.BusinessEntity as JobDeclaration;
				}
			});
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			Factory.Save();

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");
				menuItem.PerformClick();

				AssertNotNull(declaration);
				Assert("Precondition: declaration saved", declaration.IsInDatabase);
				AssertNotNullOrEmpty("Precondition: declaration reference is not empty", declaration.JE_DeclarationReference);

				CombineAssertions(() =>
				{
					var log = clearance.Logs.MostRecentLogByEventTime(AutoEvents.ConsolidatedEntryChanged);
					AssertNotNull("log is found", log);
					StmALog.GetParametersFromReference(log.SL_Reference).TryGetValue(EventReferenceParameters.Codes.JobNumber, out var declarationReferenceFromLog);
					AssertEquals("log property references declaration", declaration.JE_DeclarationReference, declarationReferenceFromLog);

					var transferedLog = declaration.Logs.MostRecentLogByEventTime(AutoEvents.Transferred);
					AssertNotNull("transfered log is found", transferedLog);
					StmALog.GetParametersFromReference(transferedLog.SL_Reference).TryGetValue(EventReferenceParameters.Codes.JobNumber, out var transferedLogJobNumber);
					AssertEquals("transfered log job number", clearance.ULH_JobNumber.ToString(), transferedLogJobNumber);
					StmALog.GetParametersFromReference(transferedLog.SL_Reference).TryGetValue(EventReferenceParameters.Codes.Type, out var transferedLogType);
					AssertEquals("transfered log type", "USLV", transferedLogType);

					declaration.Factory.Save();
					AssertEquals("unhook factory saving on first save", 1, clearance.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.ConsolidatedEntryChangedCode).Count());
				});
			}
		}

		public void TestCreateConsolidatedSummary_CloseWithoutSaving()
		{
			JobDeclaration declaration = default;
			var formWasShown = false;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown((form) =>
			{
				if (form is JobDeclarationForm declarationForm)
				{
					formWasShown = true;
					declaration = declarationForm.Declaration;
					declarationForm.Close();
				}
			});
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			Factory.Save();

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");
				menuItem.PerformClick();

				Factory.Save();

				CombineAssertions(() =>
				{
					Assert("Form was shown", formWasShown);
					var log = clearance.Logs.MostRecentLogByEventTime(AutoEvents.ConsolidatedEntryChanged);
					AssertNull("log is not found", log);
					Assert("declaration is not saved when clearance is saved", !declaration.IsInDatabase);
				});
			}
		}

		public void TestCreateConsolidatedSummary_MultipleSummaries()
		{
			var formShown = false;
			ModuleIdentifier moduleID = null;
			ZFormModaliser.ShowDialogsInTest = true;
			JobDeclarationFilterBusinessObject filterBizo = null;
			ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
			{
				if (obj is EmbeddedModulePopup popup)
				{
					formShown = true;
					moduleID = popup.CurrentModule.ModuleID;
					var filterControl = popup.FindSingle<ZFilterStripControl>();
					filterBizo = filterControl?.FilterBusinessObject as JobDeclarationFilterBusinessObject;
				}
			});

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "MasterBill112233";
			clearance.ULH_EntryFilerCode = "AAA";
			for (var i = 0; i < 1000; i++)
			{
				var consignment = clearance.CusUSLVConsignments.AddNew();
				consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
				_ = consignment.CusUSLVItems.AddNew();
			}

			Factory.Save();

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				menuItem.PerformClick();

				const string messageBeforeCreating = "2 Consolidated Summaries will be created as it exceeds the 999 invoice lines limit.";
				const string messageAfterCreated = "2 Consolidated Summaries are created, open each Consolidated Summary via Messaging > Open Consolidated Summary.";
				CombineAssertions("Messages should be displayed for multiple consolidated summaries", () =>
				{
					Assert("Message before creating 2 summaries", UnitTestUserNotification.Instance.PreviousMessages.Any(m => m.Text.Equals(messageBeforeCreating)));
					Assert("Message after created 2 summaries", UnitTestUserNotification.Instance.PreviousMessages.Any(m => m.Text.Equals(messageAfterCreated)));
				});
			}

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Open Consolidated Summary");
				menuItem.PerformClick();

				var filterBizoType = typeof(FilterBusinessObject);
				var defaultsField = filterBizoType.GetField("Defaults", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				var defaultsFieldValue = defaultsField?.GetValue(filterBizo) as FilterBusinessObjectDefaults;
				var defaults = defaultsFieldValue?.OfType<FilterBusinessObjectDefault>().ToList();
				CombineAssertions("", () =>
				{
					Assert("Module should be shown", formShown);
					AssertEquals("Module ID should be CusDec", ModuleIDs.Customs.JobDeclaration, moduleID);
					AssertNotNull("FilterBizo should be JobDeclarationFilterBusinessObject", filterBizo);
					AssertNotNull(defaults);
					Assert(defaults.Any(d => d.FilterName == "Entry Type" && d.Value.ToString() == EntryTypeList.Codes.InformalFreeDutiable));
					Assert(defaults.Any(d => d.FilterName == "Shipment Type" && d.Value.ToString() == "IMP"));
					Assert(defaults.Any(d => d.FilterName == "Master Bill" && d.Value.ToString() == "MasterBill112233"));
					Assert(defaults.Any(d => d.FilterName == "Created Time" && d.Value.ToString() == ModuleDateFilter.DateRangeSearchTexts.Last12Mths));
				});
			}
		}

		public void TestCreateConsolidatedSummary_SkipsValidation()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			JobDeclaration declaration = default;
			ZFormModaliser.SetDelegateToCallOnFormShown((form) =>
			{
				if (form is JobDeclarationForm declarationForm)
				{
					declaration = declarationForm.BusinessEntity as JobDeclaration;
					declaration.Factory.Save();
				}
			});

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "!!!";
			clearance.ULH_RL_NKPortOfDischarge = "USLAX";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			Factory.Save();

			using (USCustomsDataRegistry.Instance.DoDefaultShipTo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertType<JobDeclarationForm>(ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Master Bill number is correctly written and read", "!!!", declaration.JE_MasterBill);
					AssertNoWarnings("JE_MasterBill should have no warnings as validation is suspended during Job Declaration Read", declaration.JE_MasterBillInfo);
					declaration.Validation.ValidateJE_MasterBill();
					AssertHasWarnings("JE_MasterBill should have warnings once validated as it has non alphanumeric characters ", declaration.JE_MasterBillInfo);
				});
			}
		}

		public void TestCreateConsolidatedSummary_BulkSave()
		{
			JobDeclaration declaration = default;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown((form) =>
			{
				if (form is JobDeclarationForm declarationForm)
				{
					declaration = declarationForm.BusinessEntity as JobDeclaration;
				}
			});
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			for (var i = 0; i < 101; i++)
			{
				var consignment = clearance.CusUSLVConsignments.AddNew();
				consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
				_ = consignment.CusUSLVItems.AddNew();
			}

			Factory.Save();

			using (var form = new CusUSLVClearanceFormForTest(clearance))
			{
				form.Show();
				var messagingMenuItem = form.MainMenuForTest.MenuItems.FindByText("Messaging");
				messagingMenuItem.OnPopup(null);
				var menuItem = messagingMenuItem.MenuItems.FindByText("Create Consolidated Summary");
				menuItem.PerformClick();

				CombineAssertions("All these tables should have bulk save sql event", () =>
				{
					using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
					{
						AssertNoExceptionThrown(declaration.Factory.Save);
						Assert(bulkCopyEventTracker.HasBulkCopyEvent(OrgHeaderSchema.Constants.TableName, ["FireTriggers"]));
						Assert(bulkCopyEventTracker.HasBulkCopyEvent(OrgAddressSchema.Constants.TableName, ["FireTriggers"]));
						Assert(bulkCopyEventTracker.HasBulkCopyEvent(OrgMiscServSchema.Constants.TableName, ["FireTriggers"]));
						Assert(bulkCopyEventTracker.HasBulkCopyEvent(OrgAddressCapabilitySchema.Constants.TableName, ["FireTriggers"]));
						Assert(bulkCopyEventTracker.HasBulkCopyEvent(OrgCompanyDataSchema.Constants.TableName, ["FireTriggers"]));
						Assert(bulkCopyEventTracker.HasBulkCopyEvent(StmALogSchema.Constants.TableName, ["FireTriggers"]));
					}
				});
			}
		}

		#endregion

		public void TestConvertPartyAddressesToNewOrgContextMenuItem_IsAddedToContextMenu()
		{
			using (InitializeTestingForm())
			{
				var consignment = Shipment.CusUSLVConsignments.AddNew();

				ClearanceForm.Show();
				Application.DoEvents();

				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var convertPartyAddressesMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Convert Party Addresses to Organizations", true);

				AssertNotNull("Convert Party Addresses to Organizations menu item should exist", convertPartyAddressesMenuItem);
				Assert("Convert Party Addresses to Organizations menu item should be visible", convertPartyAddressesMenuItem.Visible);
			}
		}

		public void TestConvertPartyAddressesToNewOrgContextMenuItem_IsNotAddedMultipleTimes()
		{
			using (InitializeTestingForm())
			{
				var consignment = Shipment.CusUSLVConsignments.AddNew();

				ClearanceForm.Show();
				Application.DoEvents();

				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();

				houseBillGrid.ContextMenu.DoPopup();
				houseBillGrid.ContextMenu.DoPopup();

				var convertPartyAddressesMenuItems = houseBillGrid.ContextMenu.MenuItems
					.OfType<MenuItem>()
					.Where(item => item.Text == "Convert Party Addresses to Organizations")
					.ToArray();

				AssertEquals("Menu item should be added only once", 1, convertPartyAddressesMenuItems.Length);
			}
		}

		public void TestConvertPartyAddressesToNewOrgContextMenuItem_TextIsCorrect()
		{
			using (InitializeTestingForm())
			{
				var consignment = Shipment.CusUSLVConsignments.AddNew();

				ClearanceForm.Show();
				Application.DoEvents();

				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var convertPartyAddressesMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Convert Party Addresses to Organizations", true);

				AssertNotNull("Convert Party Addresses to New Organizations menu item should exist", convertPartyAddressesMenuItem);
				AssertEquals("Menu text should be correct", "Convert Party Addresses to Organizations", convertPartyAddressesMenuItem.Text);
			}
		}

		public void TestConvertPartyAddressesToNewOrgContextMenuItem_Functionality()
		{
			using (InitializeTestingForm())
			{
				var consignment = Shipment.CusUSLVConsignments.AddNew();
				consignment.ULB_ConsigneeName = "Test Consignee";
				consignment.ULB_ConsigneeAddress1 = "123 Test Street";
				consignment.ULB_ConsigneeCity = "Test City";
				consignment.ULB_RN_NKConsigneeCountry = "US";
				consignment.ULB_ConsigneeState = "CA";

				consignment.ULB_SellerName = "Test Seller";
				consignment.ULB_SellerAddress1 = "456 Seller Ave";
				consignment.ULB_SellerCity = "Seller City";
				consignment.ULB_RN_NKSellerCountry = "US";

				Assert("Precondition: consignee should not be linked to an organization", !consignment.ConsigneeIsOrganisation);

				ClearanceForm.Show();
				Application.DoEvents();

				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var convertPartyAddressesMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Convert Party Addresses to Organizations", true);

				ZFormModaliser.ShowDialogsInTest = true;
				using (ZFormModaliser.SuspendDispose())
				{
					convertPartyAddressesMenuItem.PerformClick();

					var freeTextAddressForm = ZFormModaliser.LastFormShownDialogForTest;
					CombineAssertions("Should show FreeTextAddressConversionForm", () =>
					{
						AssertNotNull(freeTextAddressForm);
						AssertType<FreeTextAddressConversionForm<CusUSLVConsignment>>(freeTextAddressForm);
					});
				}
			}
		}

		public void TestConvertPartyAddressesToNewOrgContextMenuItem_AllConsignmentsHaveOrganizations()
		{
			using (InitializeTestingForm())
			{
				var consignment = Shipment.CusUSLVConsignments.AddNew();

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				var sellerOrg = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				consignment.ConsigneeOrgPK = consigneeOrg.PK;
				consignment.SellerOrgPK = sellerOrg.PK;

				Assert("Precondition: consignee should be linked to an organization", consignment.ConsigneeIsOrganisation);
				Assert("Precondition: seller should be linked to an organization", consignment.ShipperIsOrganisation);

				ClearanceForm.Show();
				Application.DoEvents();

				var houseBillGrid = ClearanceForm.HouseBillGrid;
				houseBillGrid.SelectAllElements();
				houseBillGrid.ContextMenu.DoPopup();

				var convertPartyAddressesMenuItem = houseBillGrid.ContextMenu.MenuItems.FindByText("Convert Party Addresses to Organizations", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				convertPartyAddressesMenuItem.PerformClick();

				AssertEquals("Message should indicate all consignments have organizations", 
					"All consignments have linked to organizations", 
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertPartyAddressesToNewOrgActionMenuItem_IsAddedToActionsMenu()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();

				var actionsMenuItem = ClearanceForm.ActionsMenuItem;
				var convertPartyAddressesMenuItem = actionsMenuItem.MenuItems.FindByText("Convert Party Addresses to Organizations", true);

				AssertNotNull("Convert Party Addresses to New Organizations menu item should exist", convertPartyAddressesMenuItem);
				Assert("Convert Party Addresses to New Organizations menu item should be visible", convertPartyAddressesMenuItem.Visible);
			}
		}

		public void TestConvertPartyAddressesToNewOrgActionMenuItem_TextIsCorrect()
		{
			using (InitializeTestingForm())
			{
				ClearanceForm.Show();
				Application.DoEvents();

				var actionsMenuItem = ClearanceForm.ActionsMenuItem;
				var convertPartyAddressesMenuItem = actionsMenuItem.MenuItems.FindByText("Convert Party Addresses to Organizations", true);

				AssertNotNull("Convert Party Addresses to New Organizations menu item should exist", convertPartyAddressesMenuItem);
				AssertEquals("Menu text should be correct", "Convert Party Addresses to Organizations", convertPartyAddressesMenuItem.Text);
			}
		}

		public void TestConvertPartyAddressesToNewOrgActionMenuItem_Functionality()
		{
			using (InitializeTestingForm())
			{
				var consignment1 = Shipment.CusUSLVConsignments.AddNew();
				consignment1.ULB_ConsigneeName = "Test Consignee 1";
				consignment1.ULB_ConsigneeAddress1 = "123 Test Street";
				consignment1.ULB_ConsigneeCity = "Test City";
				consignment1.ULB_RN_NKConsigneeCountry = "US";

				var consignment2 = Shipment.CusUSLVConsignments.AddNew();
				consignment2.ULB_SellerName = "Test Seller 2";
				consignment2.ULB_SellerAddress1 = "456 Seller Ave";
				consignment2.ULB_SellerCity = "Seller City";
				consignment2.ULB_RN_NKSellerCountry = "US";

				Assert("Precondition: consignee should not be linked to an organization", !consignment1.ConsigneeIsOrganisation);
				Assert("Precondition: seller should not be linked to an organization", !consignment2.ShipperIsOrganisation);

				ClearanceForm.Show();
				Application.DoEvents();

				var actionsMenuItem = ClearanceForm.ActionsMenuItem;
				var convertPartyAddressesMenuItem = actionsMenuItem.MenuItems.FindByText("Convert Party Addresses to Organizations", true);

				ZFormModaliser.ShowDialogsInTest = true;
				using (ZFormModaliser.SuspendDispose())
				{
					convertPartyAddressesMenuItem.PerformClick();
					var freeTextAddressForm = ZFormModaliser.LastFormShownDialogForTest;
					CombineAssertions("Should show FreeTextAddressConversionForm", () =>
					{
						AssertNotNull(freeTextAddressForm);
						AssertType<FreeTextAddressConversionForm<CusUSLVConsignment>>(freeTextAddressForm);
					});
				}
			}
		}

		public void TestConvertPartyAddressesToNewOrgActionMenuItem_AllConsignmentsHaveOrganizations()
		{
			using (InitializeTestingForm())
			{
				var consignment1 = Shipment.CusUSLVConsignments.AddNew();
				var consignment2 = Shipment.CusUSLVConsignments.AddNew();

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				var sellerOrg = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				consignment1.ConsigneeOrgPK = consigneeOrg.PK;
				consignment1.SellerOrgPK = sellerOrg.PK;
				consignment2.ConsigneeOrgPK = consigneeOrg.PK;
				consignment2.SellerOrgPK = sellerOrg.PK;

				CombineAssertions("Precondition: all parties should be linked to organizations", () =>
				{
					Assert(consignment1.ConsigneeIsOrganisation);
					Assert(consignment1.ShipperIsOrganisation);
					Assert(consignment2.ConsigneeIsOrganisation);
					Assert(consignment2.ShipperIsOrganisation);
				});

				ClearanceForm.Show();
				Application.DoEvents();

				var actionsMenuItem = ClearanceForm.ActionsMenuItem;
				var convertPartyAddressesMenuItem = actionsMenuItem.MenuItems.FindByText("Convert Party Addresses to Organizations", true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				convertPartyAddressesMenuItem.PerformClick();

				AssertEquals("Message should indicate all consignments have organizations", 
					"All consignments have linked to organizations", 
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var form = new CusUSLVClearanceForm(Factory.New<CusUSLVClearance>());
			form.ControllerID = ControllerIDs.Customs.US.USLowValueEntries;
			return form;
		}

		CusUSLVClearanceFormForTest ClearanceForm { get; set; }
		CusUSLVClearance Shipment { get; set; }

		IDisposable InitializeTestingForm(CusUSLVClearance clearance = null)
		{
			if (clearance == null)
			{
				clearance = Factory.New<CusUSLVClearance>();
			}

			return ClearanceForm = new CusUSLVClearanceFormForTest(Shipment = clearance);
		}

		#endregion
	}

	public class CusUSLVClearanceFormForTest : CusUSLVClearanceForm
	{
		public CusUSLVClearanceFormForTest(CusUSLVClearance shipment) : base(shipment) { }

		public ImportMessagesUserControl ImportMessagesUserControl => Controls.Find("importMessagesUserControl", true).OfType<ImportMessagesUserControl>().Single();

		public ZTabPage HousebillMessagesTabPage => Controls.Find("tabPageHouseBillMessages", true).OfType<ZTabPage>().Single();

		public ZTabPage HousebillSummaryTabPage => Controls.Find("tabPageHouseBillSummary", true).OfType<ZTabPage>().Single();

		public ZTabControl BottomTabControl => Controls.Find("tabControlBottom", true).OfType<ZTabControl>().Single();

		public ZTabControl RightTabControl => Controls.Find("RightTabControl", true).OfType<ZTabControl>().Single();

		public ZGrid GridCommodityDetail => Controls.Find("gridCommodityDetails", true).OfType<ZGrid>().Single();

		public OGAPGARequirementsControl PGARequirementsControl => Controls.Find("pgaRequirementsControl", true).OfType<OGAPGARequirementsControl>().Single();

		public ZGrid GridDisposition => Controls.Find("gridDisposition", true).OfType<ZGrid>().Single();

		public KSplitter SplitterBottom => Controls.Find("splitterBottom", true).OfType<KSplitter>().Single();

		public KSplitContainer SplitContainer => Controls.Find("splitContainerHouseBillDetails", true).OfType<KSplitContainer>().Single();

		public ZTabPage MiscTabPage => Controls.Find("miscTabPage", true).OfType<ZTabPage>().Single();

		public BaseDeclarationTabPage PGARequirementsTabPage => Controls.Find("pgaRequirementsTabPage", true).OfType<BaseDeclarationTabPage>().Single();

		public ZCheckBox CheckBoxRemoteLocationFiling => Controls.Find("checkBoxRemoteLocationFiling", true).OfType<ZCheckBox>().Single();

		public ZDropEdit DropEditContainerMode => Controls.Find("dropEditContainerMode", true).OfType<ZDropEdit>().Single();

		public ZGrid HouseBillGrid => Controls.Find("gridHouseBills", true).OfType<ZGrid>().Single();

		public MainMenu MainMenuForTest => MainMenu;

		public new MenuItem ActionsMenuItem => base.ActionsMenuItem;
	}
}
