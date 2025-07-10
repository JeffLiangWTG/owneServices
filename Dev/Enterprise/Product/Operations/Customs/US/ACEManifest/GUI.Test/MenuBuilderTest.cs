using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestSendBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RL_NKPortOfLoading = "DEFRA";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "Pack1";

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // YES, proceed with errors
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Send Bills").PerformClick();
				AssertEquals("1 Air Import Message(s) Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertContains("WBL/FRA/T0/K0/", eHubMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Sent, bill.ABL_MessageStatus);
			AssertNullOrEmpty(header.AMA_MessageStatus);
		}

		public void TestSendBills_ArrivalSegment()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RL_NKPortOfLoading = "DEFRA";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";
			header.AMA_CarrierCode = "SHA2";
			header.AMA_Voyage = "V0569";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "Pack1";

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Send Bills").PerformClick();
				AssertEquals("1 Air Import Message(s) Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertNotContains("Send Bills for FRI & FRC should not include ARR & CCL lines", "\r\nARR/", eHubMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Sent, bill.ABL_MessageStatus);
			AssertNullOrEmpty(header.AMA_MessageStatus);
		}

		public void TestSendBills_NoBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed

				menu.MenuItems.FindByText("Send Bills").PerformClick();
				AssertContains("Please capture the bills and packs required for the respective manifest types", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendBills_OneBillSelected()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "DEFRA";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";
			var pack1 = bill1.Packs.AddNew();
			pack1.APA_GoodsDescription = "Pack1";
			var pack2 = bill2.Packs.AddNew();
			pack2.APA_GoodsDescription = "Pack2";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == bill1.PK);
				});

				menu.MenuItems.FindByText("Send Bills").PerformClick();
				AssertEquals("1 Air Import Message(s) Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertContains("WBL/FRA/T0/K0/", eHubMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Sent, bill1.ABL_MessageStatus);
			AssertNullOrEmpty(bill2.ABL_MessageStatus);
			AssertNullOrEmpty(header.AMA_MessageStatus);
		}

		public void TestSendBills_NoBillSelected()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "DEFRA";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(o => false);
				});

				menu.MenuItems[0].PerformClick();
				AssertContains("US Air Import Manifests can only be submitted by a company with an Originator Code. This can be added under CFS Address > Details > Config > Registration Numbers / Codes, (using type 'AMO')", UnitTestUserNotification.Instance.LastMessage.Text);

				header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				menu.MenuItems[0].PerformClick();
				AssertContains("At least one Bill must be selected when sending a Bill-level manifest message", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(0, header.Messages.Count);
			AssertNullOrEmpty(bill1.ABL_MessageStatus);
			AssertNullOrEmpty(bill2.ABL_MessageStatus);
			AssertNullOrEmpty(header.AMA_MessageStatus);
		}

		public void TestSendBills_SaveException()
		{
			var factory = new BusinessObjectFactory();

			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Test1";
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "Test Address";
			orgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);

			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RL_NKPortOfLoading = "DEFRA";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_ManifestType = "IAM";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var bill = header.Bills.AddNew();

			factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // YES, proceed with errors
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();

					factory.Saving += (f) => throw new ZSaveException(new ZDataException(new Exception("TEST ERROR"), null, null), f);
				});

				menu.MenuItems.FindByText("Send Bills").PerformClick();
			}

			var expectedMessage = $@"An Error occurred while saving the changes.

** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = TEST ERROR

";

			AssertEquals("Exception reported", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);

			AssertContains("Send Air Import Message", ErrorReporter.LastMessageReported);
			UnitTestUserNotification.Instance.ClearMessages();
			ErrorReporter.Clear();

			AssertEquals("There should be no pending change on the main Bizo", false, header.HasChanges);
		}

		public void TestSendArrivalMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bll1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bll2";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FLT1";
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill1 = transferHeader.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill1.ABL_BillNumber;
			transferBill1.ATB_ABL_Bill = bill1.PK;

			var transferBill2 = transferHeader.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = bill2.ABL_BillNumber;
			transferBill2.ATB_ABL_Bill = bill2.PK;

			var transferHeader2 = arrivalHeader.TransferHeaders.AddNew();
			transferHeader2.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;

			Factory.Save();

			var chooserItemsCount = 0;

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
					chooserItemsCount = dialog.BusinessEntity.ChooserItems.Count;
				});

				menu.MenuItems.FindByText("Arrival Messages").PerformClick();
				var notificationMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("Arrival message with [FLT1/Bll1] queued for sending.", notificationMessage);
				AssertContains("Arrival message with [FLT1/Bll2] queued for sending.", notificationMessage);

				AssertEquals("TransferHeader which does not have any transfer bills is not in the list", 1, chooserItemsCount);
			}
		}

		public void TestSendArrivalMessage_SaveException()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bll1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bll2";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FLT1";
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill1 = transferHeader.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill1.ABL_BillNumber;
			transferBill1.ATB_ABL_Bill = bill1.PK;

			var transferBill2 = transferHeader.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = bill2.ABL_BillNumber;
			transferBill2.ATB_ABL_Bill = bill2.PK;

			var transferHeader2 = arrivalHeader.TransferHeaders.AddNew();
			transferHeader2.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;

			factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // YES, proceed with errors
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();

					factory.Saving += (f) => throw new ZSaveException(new ZDataException(new Exception("TEST ERROR"), null, null), f);
				});

				menu.MenuItems.FindByText("Arrival Messages").PerformClick();
			}
			UnitTestUserNotification.Instance.ClearMessages();
			ErrorReporter.Clear();

			AssertEquals("There should be no pending change on the main Bizo", false, header.HasChanges);
		}

		[TestTimeZoneUNLOCO("USCHI")]
		[TestDate(2020, 07, 24, 07, 30, 00, 00)]
		public void TestSendDepartureMessage()
		{
			TestDateAttribute.UseUNLOCO = true;
			GlbCompany.CurrentCompany.SetCountry("US");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USCHI";
			AssertEquals("Precondition to verify timezone is being applied", "2020-07-24T02:30:00", ZDateTime.Now.ToISO8601String());

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "NZAKL";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_ManifestType = "IAM";
			header.AMA_Voyage = "KLM325";
			header.AMA_E_DEP = new ZDateTime(2020, 07, 24, 00, 00, 00, 00);
			header.AMA_E_ARV = new ZDateTime(2020, 07, 25, 06, 30, 00, 00);
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bll1";

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				AssertNull(menu.MenuItems.FindByText("Send Flight Departure Message (FDM)"));
			}

			using (ManifestCustomsDataRegistry.Instance.EnableFDMMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (var menu = new AsycudaMenuForTest(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					var departureDatePopupShown = false;
					var departureDate = ZDateTime.Empty;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						if (obj is DepartureSelectionDialog departureDialog)
						{
							departureDatePopupShown = true;
							var additionalMessageInformation = (AdditionalMessageInformation)departureDialog.BusinessEntity;
							additionalMessageInformation.AM_FlightDepartureTime = new ZDateTime(2020, 07, 24, 15, 30, 00, 00);
						}
					});

					var departure = menu.MenuItems.FindByText("Send Flight Departure Message (FDM)");
					AssertEquals("Preceded by spacer", "-", menu.MenuItems[departure.Index - 1].Text);

					departure.PerformClick();
					AssertContains("Departure message for [KLM325] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("DepartureDate Popup Shown", departureDatePopupShown);
				}

				header.AMA_ManifestType = "";
				using (var menu = new AsycudaMenuForTest(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					AssertNull(menu.MenuItems.FindByText("Send Flight Departure Message (FDM)"));
				}
			}

			var expectedMessage =
				@"FDM
DEP/KLM325/25JUL/24JUL0330
";
			AssertEquals("Departure Time in message is UTC", expectedMessage, header.Messages[0].EM_MessageText);
		}

		public void TestSendFSQMAWB()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "VOG";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bll1";
			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Freight Status Query (MAWB)").PerformClick();
				AssertContains("Freight Status Query (MAWB) [SHA-123456789] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var expectedMessage =
@"FSQ
LAXVOG
SHA-12345678
FSQ/05";
			AssertMultilineASCIIEquals(expectedMessage, header.Messages[0].EM_MessageText);
		}

		public void TestSendFSQMAWBWithReference()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "VOG";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bll1";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FLT1";
			arrivalHeader.ATH_Reference = "A";
			var arrivalDetail = arrivalHeader.ArrivalDetails.AddNew();
			arrivalDetail.ATL_ABL_AsycudaBill = header.MasterBill.PK;
			var arrivalDetail2 = arrivalHeader.ArrivalDetails.AddNew();
			arrivalDetail2.ATL_ABL_AsycudaBill = bill.PK;
			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var selectionItemDescriptions = new List<ZString>();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					foreach (MessageChooserItem item in dialog.BusinessEntity.ChooserItems)
					{
						selectionItemDescriptions.Add(item.Description);
						item.Checked = item.Description == "SHA-123456789 A";
					}
				});

				menu.MenuItems.FindByText("Freight Status Query (MAWB)").PerformClick();

				AssertContainsExactElementsInAnyOrder(new string[] { "SHA-123456789", "SHA-123456789 A" }, selectionItemDescriptions);
				AssertContains("Freight Status Query (MAWB) [SHA-123456789 A] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var expectedMessage =
@"FSQ
LAXVOG
SHA-12345678-A
FSQ/05";
			AssertMultilineASCIIEquals(expectedMessage, header.Messages[0].EM_MessageText);
		}

		public void TestSendFSQMAWB_RequestType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "VOG";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bll1";
			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					var chooser = (AIMMessageChooser)dialog.BusinessEntity;
					chooser.RequestCode = AIMFreightStatusRequestCodes.Codes.RequestForCurrentRecordStatus;
					foreach (MessageChooserItem item in chooser.ChooserItems)
					{
						item.Checked = item.Description == "SHA-123456789";
					}
				});

				menu.MenuItems.FindByText("Freight Status Query (MAWB)").PerformClick();
				AssertContains("Freight Status Query (MAWB) [SHA-123456789] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var expectedMessage =
@"FSQ
LAXVOG
SHA-12345678
FSQ/02";
			AssertMultilineASCIIEquals(expectedMessage, header.Messages[0].EM_MessageText);
		}

		public void TestSendFSQHAWB()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bll1";
			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					foreach (MessageChooserItem item in dialog.BusinessEntity.ChooserItems)
					{
						item.Checked = item.Description == "Bll1";
					}
				});

				menu.MenuItems.FindByText("Freight Status Query (HAWB)").PerformClick();
				AssertContains("Freight Status Query (HAWB) [Bll1] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var expectedMessage =
@"FSQ
SHA-12345678-BLL1
FSQ/05";
			AssertMultilineASCIIEquals(expectedMessage, header.Messages[0].EM_MessageText);
		}

		public void TestSendFSQHAWBWithReference()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "VOG";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bll1";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FLT1";
			arrivalHeader.ATH_Reference = "A";
			var arrivalDetail = arrivalHeader.ArrivalDetails.AddNew();
			arrivalDetail.ATL_ABL_AsycudaBill = header.MasterBill.PK;
			var arrivalDetail2 = arrivalHeader.ArrivalDetails.AddNew();
			arrivalDetail2.ATL_ABL_AsycudaBill = bill.PK;
			arrivalDetail2.ATL_Reference = "A";
			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var selectionItemDescriptions = new List<ZString>();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					foreach (MessageChooserItem item in dialog.BusinessEntity.ChooserItems)
					{
						selectionItemDescriptions.Add(item.Description.Trim());
						item.Checked = item.Description == "Bll1 A";
					}
				});

				menu.MenuItems.FindByText("Freight Status Query (HAWB)").PerformClick();

				AssertContainsExactElementsInAnyOrder(new string[] { "Bll1", "Bll1 A" }, selectionItemDescriptions);
				AssertContains("Freight Status Query (HAWB) [Bll1 A] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var expectedMessage =
@"FSQ
SHA-12345678-BLL1-A
FSQ/05";
			AssertMultilineASCIIEquals(expectedMessage, header.Messages[0].EM_MessageText);
		}

		public void TestSendFSQHAWB_RequestType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bll1";
			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					var chooser = (AIMMessageChooser)dialog.BusinessEntity;
					chooser.RequestCode = AIMFreightStatusRequestCodes.Codes.RequestForCurrentRecordStatus;
					foreach (MessageChooserItem item in chooser.ChooserItems)
					{
						item.Checked = item.Description == "Bll1";
					}
				});

				menu.MenuItems.FindByText("Freight Status Query (HAWB)").PerformClick();
				AssertContains("Freight Status Query (HAWB) [Bll1] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var expectedMessage =
@"FSQ
SHA-12345678-BLL1
FSQ/02";
			AssertMultilineASCIIEquals(expectedMessage, header.Messages[0].EM_MessageText);
		}

		public void TestMAWBMessagingMenu()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			_ = header.Bills.AddNew();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				AssertNull(menu.MenuItems.FindByText("Send MAWB Message (FRI)"));
				AssertNull(menu.MenuItems.FindByText("Send MAWB Amendment Message (FRC)"));
				AssertNull(menu.MenuItems.FindByText("Send MAWB Cancellation Message (FRX)"));
			}

			using (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				header.FillWithValidTestData();
				using (var menu = new AsycudaMenuForTest(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					var mawbMessage = menu.MenuItems.FindByText("Send MAWB Message (FRI)");
					var mawbAmendment = menu.MenuItems.FindByText("Send MAWB Amendment Message (FRC)");
					var mawbCancellation = menu.MenuItems.FindByText("Send MAWB Cancellation Message (FRX)");

					AssertEquals("-", menu.MenuItems[mawbMessage.Index - 1].Text);
					AssertGreaterThan("Amend follows Message", mawbAmendment.Index, mawbMessage.Index);
					AssertGreaterThan("Cancel follows Amend", mawbCancellation.Index, mawbAmendment.Index);
				}
			}
		}

		public void TestMAWBMessagingMenuWhenHeaderIsExpressCourier()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.IsExpressCourier = true;
			_ = header.Bills.AddNew();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				AssertNull(menu.MenuItems.FindByText("Send MAWB Message (FXI)"));
				AssertNull(menu.MenuItems.FindByText("Send MAWB Amendment Message (FXC)"));
				AssertNull(menu.MenuItems.FindByText("Send MAWB Cancellation Message (FXX)"));
			}

			using (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				header.FillWithValidTestData();
				using (var menu = new AsycudaMenuForTest(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					var mawbMessage = menu.MenuItems.FindByText("Send MAWB Message (FXI)");
					var mawbAmendment = menu.MenuItems.FindByText("Send MAWB Amendment Message (FXC)");
					var mawbCancellation = menu.MenuItems.FindByText("Send MAWB Cancellation Message (FXX)");

					AssertEquals("-", menu.MenuItems[mawbMessage.Index - 1].Text);
					AssertGreaterThan("Amend follows Message", mawbAmendment.Index, mawbMessage.Index);
					AssertGreaterThan("Cancel follows Amend", mawbCancellation.Index, mawbAmendment.Index);
				}
			}
		}

		public void TestSendMAWBMessage()
		{
			var header = PrepareDataForMAWBMessage();

			using (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				menu.MenuItems.FindByText("Send MAWB Message (FRI)").PerformClick();
				AssertEquals("Air Import Manifest Message Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertContains("FRI", eHubMessage.EM_MessageText);
			AssertContains("AirWayBill block has ConsolidationIdentifier", "SHA-12345678-M", eHubMessage.EM_MessageText);
			AssertContains("WayBill pieces and weight are summed, description is CONSOLIDATION", "WBL/AKL/T5/K8.4/CONSOLIDATION", eHubMessage.EM_MessageText);
		}

		public void TestSendMAWBMessageWhenHeaderIsExpressCourier()
		{
			var header = PrepareDataForMAWBMessage(true);

			using (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				menu.MenuItems.FindByText("Send MAWB Message (FXI)").PerformClick();
				AssertEquals("Air Import Manifest Message Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertContains("FXI", eHubMessage.EM_MessageText);
			AssertContains("AirWayBill block has ConsolidationIdentifier", "SHA-12345678-M", eHubMessage.EM_MessageText);
			AssertContains("WayBill pieces and weight are summed, description is CONSOLIDATION", "WBL/AKL/T5/K8.4/CONSOLIDATION", eHubMessage.EM_MessageText);
		}

		public void TestSendMAWBAmendmentMessage()
		{
			var header = PrepareDataForMAWBMessage();

			using (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = obj as ASYCUDA.GUI.AsycudaItemSelectionDialog;
					if (dialog != null)
					{
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.PK);
						var chooser = (AIMMessageChooser)dialog.BusinessEntity;
						chooser.Reason = "03";
					}
				});

				menu.MenuItems.FindByText("Send MAWB Amendment Message (FRC)").PerformClick();
				AssertEquals("Air Import Manifest Message Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertContains("FRC", eHubMessage.EM_MessageText);
			AssertContains("AirWayBill block has ConsolidationIdentifier", "SHA-12345678-M", eHubMessage.EM_MessageText);
			AssertContains("WayBill pieces and weight are summed, description is CONSOLIDATION", "WBL/AKL/T5/K8.4/CONSOLIDATION", eHubMessage.EM_MessageText);
			AssertContains("Reason for amendment", "RFA/03", eHubMessage.EM_MessageText);
		}

		public void TestSendMAWBAmendmentMessageWhenHeaderIsExpressCourier()
		{
			var header = PrepareDataForMAWBMessage(true);

			using (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = obj as ASYCUDA.GUI.AsycudaItemSelectionDialog;
					if (dialog != null)
					{
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.PK);
						var chooser = (AIMMessageChooser)dialog.BusinessEntity;
						chooser.Reason = "03";
					}
				});

				menu.MenuItems.FindByText("Send MAWB Amendment Message (FXC)").PerformClick();
				AssertEquals("Air Import Manifest Message Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertContains("FXC", eHubMessage.EM_MessageText);
			AssertContains("AirWayBill block has ConsolidationIdentifier", "SHA-12345678-M", eHubMessage.EM_MessageText);
			AssertContains("WayBill pieces and weight are summed, description is CONSOLIDATION", "WBL/AKL/T5/K8.4/CONSOLIDATION", eHubMessage.EM_MessageText);
			AssertContains("Reason for amendment", "RFA/03", eHubMessage.EM_MessageText);
		}

		public void TestSendMAWBCancellationMessage()
		{
			var header = PrepareDataForMAWBMessage();

			using (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = obj as ASYCUDA.GUI.AsycudaItemSelectionDialog;
					if (dialog != null)
					{
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.PK);
						var chooser = (AIMMessageChooser)dialog.BusinessEntity;
						chooser.Reason = "05";
					}
				});

				menu.MenuItems.FindByText("Send MAWB Cancellation Message (FRX)").PerformClick();
				AssertEquals("Air Import Manifest Message Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertContains("FRX", eHubMessage.EM_MessageText);
			AssertContains("AirWayBill block has ConsolidationIdentifier", "SHA-12345678-M", eHubMessage.EM_MessageText);
			AssertContains("Reason for cancellation", "RFA/05", eHubMessage.EM_MessageText);
		}

		public void TestSendMAWBCancellationMessageWhenHeaderIsExpressCourier()
		{
			var header = PrepareDataForMAWBMessage(true);

			using (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = obj as ASYCUDA.GUI.AsycudaItemSelectionDialog;
					if (dialog != null)
					{
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.PK);
						var chooser = (AIMMessageChooser)dialog.BusinessEntity;
						chooser.Reason = "05";
					}
				});

				menu.MenuItems.FindByText("Send MAWB Cancellation Message (FXX)").PerformClick();
				AssertEquals("Air Import Manifest Message Created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Send Air Import Message", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var eHubMessage = header.Messages[0];
			AssertContains("FXX", eHubMessage.EM_MessageText);
			AssertContains("AirWayBill block has ConsolidationIdentifier", "SHA-12345678-M", eHubMessage.EM_MessageText);
			AssertContains("Reason for cancellation", "RFA/05", eHubMessage.EM_MessageText);
		}

		AsycudaManifestHeader PrepareDataForMAWBMessage(bool isExpressCourier = false)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "SHA-12345678";
			header.AMA_RL_NKPortOfLoading = "NZAKL";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_Voyage = "KLM325";
			header.AMA_E_DEP = new ZDateTime(2020, 07, 24, 00, 00, 00, 00);
			header.AMA_E_ARV = new ZDateTime(2020, 07, 25, 06, 30, 00, 00);
			header.IsExpressCourier = isExpressCourier;
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bll1";
			bill1.ABL_ManifestQty = 2;
			bill1.ABL_ManifestUQ = "BOX";
			bill1.ABL_GrossWeight = 2.14m;
			bill1.ABL_GrossWeightUQ = "KG";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bll2";
			bill2.ABL_ManifestQty = 3;
			bill2.ABL_ManifestUQ = "BOX";
			bill2.ABL_GrossWeight = 6.3m;
			bill2.ABL_GrossWeightUQ = "KG";

			return header;
		}

		public void TestRequestTransferMenu()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var requestTransferMenuItemIndex = menu.MenuItems.FindByText("Request Transfer").Index;
				var cancelTransferMenuItemIndex = menu.MenuItems.FindByText("Cancel Transfer").Index;
				var arrivalMessagesMenuItemIndex = menu.MenuItems.FindByText("Arrival Messages").Index;

				AssertEquals("Cancel Transfer sits below Request Transfer", requestTransferMenuItemIndex + 1, cancelTransferMenuItemIndex);
				AssertEquals("Arrival Messages sits below Cancel Transfer", cancelTransferMenuItemIndex + 1, arrivalMessagesMenuItemIndex);
			}
		}

		public void TestRequestTransfer()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_CustomsOffice = "X";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill01";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = "FLT1";
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			transferHeader1.ATF_RL_NKDestinationPortCode = "USLAX";
			transferHeader1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferHeader1.ATF_CarrierID = "13-150279800";
			transferHeader1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill1.ABL_BillNumber;

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill02";
			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			arrivalHeader2.ATH_VoyageFlightNo = "FLT2";
			var transferHeader2 = arrivalHeader2.TransferHeaders.AddNew();
			transferHeader2.ATF_RL_NKDestinationPortCode = "USCHI";
			transferHeader2.ATF_TransferType = ManifestBase.TransferTypeList.Codes.International;
			transferHeader2.ATF_CarrierID = "13-987654321";
			transferHeader2.ATF_DestinationWarehouseID = "4321";
			var transferBill2 = transferHeader2.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = bill2.ABL_BillNumber;
			transferBill2.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;

			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "Bill03";
			var arrivalHeader3 = header.ArrivalHeaders.AddNew();
			arrivalHeader3.ATH_VoyageFlightNo = "FLT3";
			var transferHeader3 = arrivalHeader3.TransferHeaders.AddNew();
			transferHeader3.ATF_RL_NKDestinationPortCode = "USCHI";
			transferHeader3.ATF_TransferType = ManifestBase.TransferTypeList.Codes.International;
			transferHeader3.ATF_CarrierID = "13-987654321";
			transferHeader3.ATF_DestinationWarehouseID = "4321";
			var transferBill3 = transferHeader3.TransferBills.AddNew();
			transferBill3.ATB_BillNumber = bill3.ABL_BillNumber;
			transferBill3.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferCancelled;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				ZString chooserDialogTitle = null;
				ZString[] selectionItemIds = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					chooserDialogTitle = dialog.Text;
					dialog.SelectAll();
					selectionItemIds = dialog.BusinessEntity.ChooserItems.Cast<TransferHeaderMessageChooserItem>().Select(x => x.VoyageFlightNo).ToArray();
				});

				menu.MenuItems.FindByText("Request Transfer").PerformClick();

				AssertEquals("Transfer to Send", chooserDialogTitle);
				AssertContainsExactElementsInAnyOrder(new string[] { "FLT1", "FLT2" }, selectionItemIds);

				var expectedMessage = @"Transfer message for [FLT1/Bill01] queued for sending.
Transfer message for [FLT2/Bill02] queued for sending.";
				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			header.Messages.Reload(true);

			var allMessages = header.Messages.Cast<EDIMessage>().ToArray();
			var message1 = allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "Bill01");
			AssertContains("TRN/LAX-D/13-150279800/1234", message1.EM_MessageText);
			var message2 = allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "Bill02");
			AssertContains("TRN/CHI-I/13-987654321/4321", message2.EM_MessageText);
			AssertNull("No message for Bill03", allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "Bill03"));
			AssertEquals(2, allMessages.Length);

			transferBill1.Reload();
			AssertEquals("Status updated", AIMTransferStatusCodes.Codes.TransferSent, transferBill1.ATB_MessageStatus);
			transferBill2.Reload();
			AssertEquals("Status updated", AIMTransferStatusCodes.Codes.TransferSent, transferBill2.ATB_MessageStatus);
			transferBill3.Reload();
			AssertEquals("Status unchanged", AIMTransferStatusCodes.Codes.TransferCancelled, transferBill3.ATB_MessageStatus);
		}

		public void TestRequestTransfer_SaveException()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_CustomsOffice = "X";

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill01";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = "FLT1";
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			transferHeader1.ATF_RL_NKDestinationPortCode = "USLAX";
			transferHeader1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferHeader1.ATF_CarrierID = "13-150279800";
			transferHeader1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill1.ABL_BillNumber;

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill02";
			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			arrivalHeader2.ATH_VoyageFlightNo = "FLT2";
			var transferHeader2 = arrivalHeader2.TransferHeaders.AddNew();
			transferHeader2.ATF_RL_NKDestinationPortCode = "USCHI";
			transferHeader2.ATF_TransferType = ManifestBase.TransferTypeList.Codes.International;
			transferHeader2.ATF_CarrierID = "13-987654321";
			transferHeader2.ATF_DestinationWarehouseID = "4321";
			var transferBill2 = transferHeader2.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = bill2.ABL_BillNumber;
			transferBill2.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;

			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "Bill03";
			var arrivalHeader3 = header.ArrivalHeaders.AddNew();
			arrivalHeader3.ATH_VoyageFlightNo = "FLT3";
			var transferHeader3 = arrivalHeader3.TransferHeaders.AddNew();
			transferHeader3.ATF_RL_NKDestinationPortCode = "USCHI";
			transferHeader3.ATF_TransferType = ManifestBase.TransferTypeList.Codes.International;
			transferHeader3.ATF_CarrierID = "13-987654321";
			transferHeader3.ATF_DestinationWarehouseID = "4321";
			var transferBill3 = transferHeader3.TransferBills.AddNew();
			transferBill3.ATB_BillNumber = bill3.ABL_BillNumber;
			transferBill3.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferCancelled;

			factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				ZString chooserDialogTitle;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					chooserDialogTitle = dialog.Text;
					dialog.SelectAll();
					factory.Saving += (f) => throw new ZSaveException(new ZDataException(new Exception("TEST ERROR"), null, null), f);
				});

				menu.MenuItems.FindByText("Request Transfer").PerformClick();
			}
			UnitTestUserNotification.Instance.ClearMessages();
			ErrorReporter.Clear();
			AssertEquals("There should be no pending change on the main Bizo", false, header.HasChanges);
		}

		public void TestRequestTransfer_MAWB()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_CustomsOffice = "X";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill01";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = "FLT1";
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			transferHeader1.ATF_RL_NKDestinationPortCode = "USLAX";
			transferHeader1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferHeader1.ATF_CarrierID = "13-150279800";
			transferHeader1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = header.MasterBill.ABL_BillNumber;

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill02";
			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			arrivalHeader2.ATH_VoyageFlightNo = "FLT2";
			var transferHeader2 = arrivalHeader2.TransferHeaders.AddNew();
			transferHeader2.ATF_RL_NKDestinationPortCode = "USCHI";
			transferHeader2.ATF_TransferType = ManifestBase.TransferTypeList.Codes.International;
			transferHeader2.ATF_CarrierID = "13-987654321";
			transferHeader2.ATF_DestinationWarehouseID = "4321";
			var transferBill2 = transferHeader2.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = bill2.ABL_BillNumber;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var selectionItemDescriptions = new List<ZString>();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					foreach (TransferHeaderMessageChooserItem item in dialog.BusinessEntity.ChooserItems)
					{
						selectionItemDescriptions.Add(item.VoyageFlightNo);
						item.Checked = item.VoyageFlightNo == "FLT1";
					}
				});

				menu.MenuItems.FindByText("Request Transfer").PerformClick();

				AssertContainsExactElementsInAnyOrder(new string[] { "FLT1", "FLT2" }, selectionItemDescriptions);
				AssertContains("Transfer message for [FLT1/SHA-123456789] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			header.Messages.Reload(true);

			var allMessages = header.Messages.Cast<EDIMessage>().ToArray();
			var message1 = allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "SHA-123456789");
			AssertContains("SHA-12345678-M", message1.EM_MessageText);
			AssertContains("TRN/LAX-D/13-150279800/1234", message1.EM_MessageText);
			AssertNull("No message for Bill01", allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "Bill01"));
			AssertEquals(1, allMessages.Length);
		}

		public void TestRequestTransfer_NoSelection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_CustomsOffice = "X";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill01";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = "FLT1";
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			transferHeader1.ATF_RL_NKDestinationPortCode = "USLAX";
			transferHeader1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferHeader1.ATF_CarrierID = "13-150279800";
			transferHeader1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill1.ABL_BillNumber;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				menu.MenuItems.FindByText("Request Transfer").PerformClick();
				AssertContains("At least one Transfer must be selected when sending a Transfer-level manifest message.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			header.Messages.Reload(true);
			var allMessages = header.Messages.Cast<EDIMessage>().ToArray();
			AssertEquals(0, allMessages.Length);
		}

		public void TestCancelTransfer()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_CustomsOffice = "X";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill01";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = "FLT1";
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			transferHeader1.ATF_RL_NKDestinationPortCode = "USLAX";
			transferHeader1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferHeader1.ATF_CarrierID = "13-150279800";
			transferHeader1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill1.ABL_BillNumber;
			transferBill1.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferSent;

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill02";
			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			arrivalHeader2.ATH_VoyageFlightNo = "FLT2";
			var transferHeader2 = arrivalHeader2.TransferHeaders.AddNew();
			transferHeader2.ATF_RL_NKDestinationPortCode = "USCHI";
			transferHeader2.ATF_TransferType = ManifestBase.TransferTypeList.Codes.International;
			transferHeader2.ATF_CarrierID = "13-987654321";
			transferHeader2.ATF_DestinationWarehouseID = "4321";
			var transferBill2 = transferHeader2.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = bill2.ABL_BillNumber;
			transferBill2.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferAccepted;

			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "Bill03";
			var arrivalHeader3 = header.ArrivalHeaders.AddNew();
			arrivalHeader3.ATH_VoyageFlightNo = "FLT3";
			var transferHeader3 = arrivalHeader3.TransferHeaders.AddNew();
			transferHeader3.ATF_RL_NKDestinationPortCode = "USCHI";
			transferHeader3.ATF_TransferType = "I";
			transferHeader3.ATF_CarrierID = "13-987654321";
			transferHeader3.ATF_DestinationWarehouseID = "4321";
			var transferBill3 = transferHeader3.TransferBills.AddNew();
			transferBill3.ATB_BillNumber = bill3.ABL_BillNumber;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				ZString chooserDialogTitle = null;
				ZString[] selectionItemIds = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					chooserDialogTitle = dialog.Text;
					dialog.SelectAll();
					selectionItemIds = dialog.BusinessEntity.ChooserItems.Cast<TransferHeaderMessageChooserItem>().Select(x => x.VoyageFlightNo).ToArray();
				});

				menu.MenuItems.FindByText("Cancel Transfer").PerformClick();

				AssertEquals("Transfer to Send", chooserDialogTitle);
				AssertContainsExactElementsInAnyOrder(new string[] { "FLT1", "FLT2" }, selectionItemIds);
				AssertContains("Transfer message for [FLT1/Bill01] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			header.Messages.Reload(true);
			var allMessages = header.Messages.Cast<EDIMessage>().ToArray();
			var message1 = allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "Bill01");
			AssertContains("TRN/000", message1.EM_MessageText);
			var message2 = allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "Bill02");
			AssertContains("TRN/000", message2.EM_MessageText);
			AssertNull("No message for Bill03", allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "Bill03"));
			AssertEquals(2, allMessages.Length);
		}

		public void TestCancelTransfer_MAWB()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_CustomsOffice = "X";
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill01";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = "FLT1";
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			transferHeader1.ATF_RL_NKDestinationPortCode = "USLAX";
			transferHeader1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferHeader1.ATF_CarrierID = "13-150279800";
			transferHeader1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = header.MasterBill.ABL_BillNumber;
			transferBill1.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferSent;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				ZString[] selectionItemIds = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
					selectionItemIds = dialog.BusinessEntity.ChooserItems.Cast<TransferHeaderMessageChooserItem>().Select(x => x.VoyageFlightNo).ToArray();
				});

				menu.MenuItems.FindByText("Cancel Transfer").PerformClick();

				AssertContainsExactElementsInAnyOrder(new string[] { "FLT1" }, selectionItemIds);
				AssertContains("Transfer message for [FLT1/SHA-123456789] queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			header.Messages.Reload(true);

			var allMessages = header.Messages.Cast<EDIMessage>().ToArray();
			var message1 = allMessages.FirstOrDefault(m => m.EM_ApplicationReference == "SHA-123456789");
			AssertContains("SHA-12345678-M", message1.EM_MessageText);
			AssertContains("TRN/000", message1.EM_MessageText);
		}

		protected override void SetUp()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Test1";
			orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "Test Address";
			orgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);
			base.SetUp();
		}
		OrgAddress orgAddress;
	}
}
