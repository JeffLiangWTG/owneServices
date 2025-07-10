using System;
using Enterprise.Customs.Common;
using IMessageParent = Enterprise.Customs.ASYCUDA.Business.IMessageParent;
using MessageStatusCodeList = Enterprise.Customs.ASYCUDA.Business.MessageStatusCodeList;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			RunAllowCancellationMessageTests(parent => statusProvider.AllowCancellationMessage(parent));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			RunAllowCancellationMessageTests(parent => statusProvider.AllowManifestCancellationMessage(parent));
		}

		public override void TestAllowModificationMessage()
		{
			packedItem.Header.AMA_ManifestType = SGManifestTypes.Codes.MGE;
			var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			Assert("Cannot modify without a RegistrationNumber", !statusProvider.AllowModificationMessage(packedItem));
			registrationNumber.CE_EntryNum = "REGNO1234";
			Assert("Modification is not allowed because Bills are locked.", !statusProvider.AllowModificationMessage(packedItem));
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Updated;
			Assert("Modification is allowed when we have a RegistrationNumber", statusProvider.AllowModificationMessage(packedItem));
			var bill = packedItem.Pack.Bill;
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			Assert("Modification is not allowed when status is not updated", !statusProvider.AllowModificationMessage(bill));
		}

		public override void TestAllowOriginalMessage()
		{
			var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			Assert("Original allowed while we do not have a RegistrationNumber", statusProvider.AllowOriginalMessage(packedItem));
			registrationNumber.CE_EntryNum = "REGNO1234";
			Assert("Cannot send an original when we have a RegistrationNumber", !statusProvider.AllowOriginalMessage(packedItem));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			Assert("Not Accepted since we do not have a RegistrationNumber", !statusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			registrationNumber.CE_EntryNum = "REGNO1234";
			Assert("Manifest has been accepted when we have a RegistrationNumber", statusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			Assert("Not Submitted since we do not have a RegistrationNumber or MessageStatus", !statusProvider.HasManifestBeenSubmittedToCustoms(packedItem));
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
			Assert("Manifest has been sent when we have a 'Sent' MessageStatus", statusProvider.HasManifestBeenSubmittedToCustoms(packedItem));
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Error;
			Assert("Manifest must no longer be seent when we have an 'Error' MessageStatus", !statusProvider.HasManifestBeenSubmittedToCustoms(packedItem));
			registrationNumber.CE_EntryNum = "REGNO1234";
			Assert("Manifest has been sent when we have a RegistrationNumber regardless of the MessageStatus", statusProvider.HasManifestBeenSubmittedToCustoms(packedItem));
		}

		public override void TestMessageStatusCanBeReset()
		{
			var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			Assert("Cannot reset by default", !statusProvider.MessageStatusCanBeReset(packedItem));
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
			Assert("Can reset when sent but not accepted", statusProvider.MessageStatusCanBeReset(packedItem));
			registrationNumber.CE_EntryNum = "REGNO1234";
			Assert("Cannot reset when accepted", !statusProvider.MessageStatusCanBeReset(packedItem));
			packedItem.API_PackStatus = "CAN";
			Assert("Can reset when cancelled", statusProvider.MessageStatusCanBeReset(packedItem));
			packedItem.API_PackStatus = "";
			packedItem.Pack.Bill.ABL_BillStatus = "";
			Assert("Cannot reset when bill is not cancelled.", !statusProvider.MessageStatusCanBeReset(packedItem));
			packedItem.API_PackStatus = "";
			packedItem.Pack.Bill.ABL_BillStatus = "CN";
			Assert("Can reset when bill is cancelled.", statusProvider.MessageStatusCanBeReset(packedItem));
		}

		public void TestHasBeenCancelled()
		{
			var bill = packedItem.Pack.Bill;
			packedItem.API_MessageStatus = "ACP";
			bill.ABL_MessageStatus = "ACP";
			packedItem.API_PackStatus = "";
			AssertEquals("HasBeenCancelled on pack level.", false, ((MessageStatusProvider)statusProvider).HasBeenCancelled(packedItem));
			packedItem.API_PackStatus = "CAN";
			AssertEquals("HasBeenCancelled on pack level.", true, ((MessageStatusProvider)statusProvider).HasBeenCancelled(packedItem));
			bill.ABL_BillStatus = "";
			AssertEquals("HasBeenCancelled on bill level.", false, ((MessageStatusProvider)statusProvider).HasBeenCancelled(bill));
			bill.ABL_BillStatus = "CN";
			AssertEquals("HasBeenCancelled on bill level.", true, ((MessageStatusProvider)statusProvider).HasBeenCancelled(bill));
		}

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider() => statusProvider;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			statusProvider = header.MessageStatusProvider;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			packedItem = pack.PackedItem;
		}

		AsycudaPackedItem packedItem;
		ASYCUDA.Business.MessageStatusProvider statusProvider;

		void RunAllowCancellationMessageTests(Func<IMessageParent, bool> conditionFunc)
		{
			CombineAssertions(() =>
			{
				packedItem.Header.AMA_ManifestType = Constants.ManifestType.Import;
				var registrationNumber = packedItem.CustomsEntryNumbers.AddNew();
				registrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
				registrationNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
				registrationNumber.CE_EntryNum = string.Empty;
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.NotSent;
				Assert("Cannot cancel when message status is NOT", !conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Error;
				Assert("Cannot cancel when message status is ERR", !conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Registered;
				Assert("Cannot cancel when message status is REG", !conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
				Assert("Cannot cancel when message status is SNT", !conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Unknown;
				Assert("Cannot cancel when message status is UNK", !conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Updated;
				Assert("Cannot cancel when message status is UPD", !conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
				Assert("Cancellation is allowed when message status is AWA", conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Accepted;
				Assert("Cancellation is allowed when message status is ACP", conditionFunc(packedItem));
				packedItem.Header.AMA_ManifestType = Constants.ManifestType.Export;
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
				Assert("Cannot cancel when the manifest is Export without a Permit Number.", !conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Accepted;
				Assert("Cannot cancel when the manifest is Export without a Permit Number.", !conditionFunc(packedItem));
				registrationNumber.CE_EntryNum = "PM1031";
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
				Assert("Cancellation is allowed when the manifest is Export with a Permit Number.", conditionFunc(packedItem));
				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Accepted;
				Assert("Cancellation is allowed when the manifest is Export with a Permit Number.", conditionFunc(packedItem));
				registrationNumber.CE_EntryNum = string.Empty;
				packedItem.Header.AMA_ManifestType = Constants.ManifestType.Import;
				packedItem.API_PackStatus = Constants.CustomsStatusCode.Cancelled;
				Assert("Cannot cancel when message custom status is CAN", !conditionFunc(packedItem));
			});
		}
	}
}
