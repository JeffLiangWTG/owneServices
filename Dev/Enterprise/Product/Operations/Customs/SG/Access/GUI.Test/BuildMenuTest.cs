using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.Access.Business.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	public class BuildMenuTest : TestCaseWithFactory
	{
		public void TestCancelOptionWhenCustomsStatusIsNotCANAndMessageStatusIsAWA()
		{
			TestCancelOption(ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting);
		}

		public void TestCancelOptionWhenCustomsStatusIsNotCANAndMessageStatusIsACP()
		{
			TestCancelOption(ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted);
		}

		public void TestCancelOption(ZString messageStatus)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
			ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SGManifestTestHelper.SetUpSGAccessTestUser(Factory);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "DEFRA";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = "MGE";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			registrationNumber.CE_EntryNum = "PM1031";
			packedItem.API_MessageStatus = messageStatus;
			AssertEquals("Pre-Req - cancel is allowed", true, packedItem.MessageStatusProvider.AllowCancellationMessage(packedItem));
			Factory.Save();
			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					AssertEquals("&Cancel Bills", menu.MenuItems[0].Text);
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (AsycudaItemSelectionDialog)obj;
						dialog.SelectAll();
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					menu.MenuItems[0].PerformClick();
					AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
					Assert(typeof(AsycudaItemSelectionDialog).IsAssignableFrom(ZFormModaliser.LastFormShownDialogForTest.GetType()));
					AssertEquals(1, header.Messages.Count);
					var message = ((IEDIMessageCollectionProvider)header).Messages.LastOutgoingMessage;
					AssertContains("Cancellation message text\r\n" + message.EM_MessageText, "<EventReference>|MSB=CNL|MST=MGE</EventReference>", message.EM_MessageText);
				}
			}
		}

		public void TestAmendOptionWhenRegistrationNumberPresent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
			ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SGManifestTestHelper.SetUpSGAccessTestUser(Factory);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "DEFRA";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = "MGE";
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsDescription = "BillTest1";
			header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			entryNum.CE_EntryNum = "ENum2";
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.API_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Updated;
			var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			registrationNumber.CE_EntryNum = "REGNO1234";
			AssertEquals("Pre-Req - amend is allowed", true, packedItem.MessageStatusProvider.AllowModificationMessage(packedItem));
			Factory.Save();
			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					AssertEquals("&Amend Bills", menu.MenuItems[0].Text);
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (AsycudaItemSelectionDialog)obj;
						dialog.SelectAll();
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					menu.MenuItems[0].PerformClick();
					AssertEquals(1, header.Messages.Count);
					var message = ((IEDIMessageCollectionProvider)header).Messages.LastOutgoingMessage;
					AssertContains("Amendment message text\r\n" + message.EM_MessageText, "<EventReference>|MSB=CHG|MST=MGE</EventReference>", message.EM_MessageText);
				}
			}
		}

		public void TestCancelManifestOptionWhenRegistrationNumberPresent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
			ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SGManifestTestHelper.SetUpSGAccessTestUser(Factory);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "DEFRA";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = "MGE";
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsDescription = "BIL1";
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			entryNum.CE_EntryNum = "Bill123";
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			registrationNumber.CE_EntryNum = "PM1031";
			packedItem.API_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted;
			AssertEquals("Pre-Req - cancel is allowed", true, packedItem.MessageStatusProvider.AllowCancellationMessage(packedItem));
			Factory.Save();
			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					AssertEquals("&Cancel Bills", menu.MenuItems[0].Text);
					AssertEquals("Cancel Manifest", menu.MenuItems[1].Text);
					menu.MenuItems[1].PerformClick();
					AssertEquals(1, header.Messages.Count);
					var message = ((IEDIMessageCollectionProvider)header).Messages.LastOutgoingMessage;
					AssertContains("Cancellation message text\r\n" + message.EM_MessageText, "<EventReference>|MSB=CNM|MST=MGE</EventReference>", message.EM_MessageText);
				}
			}
		}
	}
}
