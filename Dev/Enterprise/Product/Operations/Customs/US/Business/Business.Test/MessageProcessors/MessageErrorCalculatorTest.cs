using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	sealed class MessageErrorCalculatorTest : TestCaseWithFactory
	{
		public void TestGetLongDescription()
		{
			MessageError error = ABIError.Instance.GetErrorInfoByCode("VQS");
			MessageErrorCalculator calculator = new MessageErrorCalculator(Factory);
			string result = calculator.GetLongDescription(error.ErrorCode, error.ShortDesc);
			AssertContains("NO NARRATIVE GIVEN", result);
			result = calculator.GetLongDescription("abc", error.ShortDesc);

			AssertContains(error.ShortDesc, result);
		}

		JobDeclaration dec;
		CusEntryHeader entryHeader;
		MQEDIMessage message;
		MQEDIMessage message2;
		protected override void SetUp()
		{
			base.SetUp();

			dec = Factory.New<JobDeclaration>();
			entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B01       QP                                                                    10A61            ADFG3901     00010000             N                            Y         QP00014000000000000000000000000";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			entryHeader.Messages.Add(message);

			message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageNum = "~15000";
			message2.EM_MessageText = "B018888XJ5ER                                               ~15000               " +
"10A8888                                                   XJ5         01     Ca " +
"22            OBL123                              00000010PCS                   " +
"E228888XJ5            ADJ01   ISSUER CODE REQUIRED                              " +
"40001                                                                           " +
"E408888XJ5         00153201   SEQUENCE ERROR-LINE ITEM RECORD                   " +
"E408888XJ5         00154901   INVALID COLL MODE/PAY TYPE/DATE                   " +
"E408888XJ5         00152401   TRANSACTION DATA REJECTED                         " +
"Y  8888XJ5ER00007000000000000000000000000";

			Factory.Save();
		}
	}
}
