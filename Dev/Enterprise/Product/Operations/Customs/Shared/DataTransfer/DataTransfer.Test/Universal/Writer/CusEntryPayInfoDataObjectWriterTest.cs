using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest
	{
		public void TestCustomsEntryPayInfoMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var entryPayInfoBO = SetupEntryPayInfo(header.EntryPayInfos.AddNew(), new ZDateTime(2021, 10, 29), new ZDate(2021, 10, 30), "A10", "BRK", "PEN", true, false, 101m, "Reference1234", "DAI11101354198");

			var writer = new CusEntryPayInfoDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryPayInfoBO)), CurrentCompanyHelper);
			var entryPayInfoDataObject = writer.GetDataObject(entryPayInfoBO);

			AssertContents(entryPayInfoDataObject, new ZDateTime(2021, 10, 29), new ZDateTime(2021, 10, 30), "A10", "BRK", "Broker", "PEN", "Pending", true, false, 101m, "Reference1234", "DAI11101354198");
		}

		CusEntryPayInfo SetupEntryPayInfo(CusEntryPayInfo entryPayInfo, ZDateTime paymentDate, ZDate receiptDate, ZString transactionType, ZString paymentParty, ZString paymentStatus, ZBool cusResReceived, ZBool remAdvReceived, ZDecimal paymentAmount, ZString paymentReference, ZString incomingPayResponseNo)
		{
			entryPayInfo.C9_PaymentDate = paymentDate;
			entryPayInfo.C9_ReceiptDate = receiptDate;
			entryPayInfo.C9_TransactionType = transactionType;
			entryPayInfo.C9_PaymentParty = paymentParty;
			entryPayInfo.C9_PaymentStatus = paymentStatus;
			entryPayInfo.C9_CusResReceived = cusResReceived;
			entryPayInfo.C9_RemAdvReceived = remAdvReceived;
			entryPayInfo.C9_PaymentAmount = paymentAmount;
			entryPayInfo.C9_PaymentReference = paymentReference;
			entryPayInfo.C9_IncomingPayResponseNo = incomingPayResponseNo;
			return entryPayInfo;
		}

		void AssertContents(EntryHeaderPaymentInformation entryHeaderPaymentInformationDataObject, ZDateTime expectedPaymentDate, ZDateTime expectedEntryReleaseDate, ZString expectedTransactionTypeCode, ZString expectedPaymentPartyCode, ZString expectedPaymentPartyDescription, ZString expectedPaymentStatusCode, ZString expectedPaymentStatusDescription, ZBool expectedCusResReceived, ZBool expectedRemAdvReceived, ZDecimal expectedPaymentAmount, ZString expectedPaymentReference, ZString expectedIncomingPayResponseNo)
		{
			AssertNotNull("Precondition: entryHeaderPaymentInformationDataObject", entryHeaderPaymentInformationDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("entryHeaderPaymentInformationDataObject.PaymentDate", expectedPaymentDate, entryHeaderPaymentInformationDataObject.PaymentDate);
				AssertEquals("entryHeaderPaymentInformationDataObject.EntryReleaseDate", expectedEntryReleaseDate, entryHeaderPaymentInformationDataObject.EntryReleaseDate);
				AssertEquals("entryHeaderPaymentInformationDataObject.TransactionType.Code", expectedTransactionTypeCode, entryHeaderPaymentInformationDataObject.TransactionType.Code);
				AssertNull("entryHeaderPaymentInformationDataObject.TransactionType.Description", entryHeaderPaymentInformationDataObject.TransactionType.Description);
				AssertEquals("entryHeaderPaymentInformationDataObject.PaymentParty.Code", expectedPaymentPartyCode, entryHeaderPaymentInformationDataObject.PaymentParty.Code);
				AssertEquals("entryHeaderPaymentInformationDataObject.PaymentParty.Description", expectedPaymentPartyDescription, entryHeaderPaymentInformationDataObject.PaymentParty.Description);
				AssertEquals("entryHeaderPaymentInformationDataObject.PaymentStatus.Code", expectedPaymentStatusCode, entryHeaderPaymentInformationDataObject.PaymentStatus.Code);
				AssertEquals("entryHeaderPaymentInformationDataObject.PaymentStatus.Description", expectedPaymentStatusDescription, entryHeaderPaymentInformationDataObject.PaymentStatus.Description);
				AssertEquals("entryHeaderPaymentInformationDataObject.CustomsResponseReceived", expectedCusResReceived, entryHeaderPaymentInformationDataObject.CustomsResponseReceived);
				AssertEquals("entryHeaderPaymentInformationDataObject.RemittanceAdviceReceived", expectedRemAdvReceived, entryHeaderPaymentInformationDataObject.RemittanceAdviceReceived);
				AssertEquals("entryHeaderPaymentInformationDataObject.PaymentAmount", expectedPaymentAmount, entryHeaderPaymentInformationDataObject.PaymentAmount);
				AssertEquals("entryHeaderPaymentInformationDataObject.PaymentReference", expectedPaymentReference, entryHeaderPaymentInformationDataObject.PaymentReference);
				AssertEquals("entryHeaderPaymentInformationDataObject.IncomingPaymentResponseNumber", expectedIncomingPayResponseNo, entryHeaderPaymentInformationDataObject.IncomingPaymentResponseNumber);
			});
		}
	}
}
