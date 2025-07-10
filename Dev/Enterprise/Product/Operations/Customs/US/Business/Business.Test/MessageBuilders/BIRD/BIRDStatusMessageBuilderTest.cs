using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class BIRDStatusMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateENRecord()
		{
			EntryHeader.Declaration.ImportEntryNumber = "123456789";
			EntryHeader.Declaration.US_US_NKLocationOfGoods = "A000";
			EntryHeader.Declaration.US_BRDRefNo = "1234567890";

			BIRDStatusMessageBuilder builder = new BIRDStatusMessageBuilder(EntryHeader);
			builder.BuildENRecord();

			MQEDIMessage message = builder.PopulateMessage();

			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.BIRDStatusRecords, message.EM_MessageSubType);
			AssertContains("AASTATXJ5", message.EM_MessageText);
			AssertContains("EN                    XJ5123456789A000                                          ", message.EM_MessageText);
			AssertEquals(EntryHeader, message.EM_LinkedObject);

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertContains("1234567890", ((BRDAA)message.MessageBlock.B).OriginatingBrokerRef);
			AssertEquals(EDIMessage.Status.Acknowledged, message.EM_Status);
		}

		public void TestGenerateENRecordWhenExternalBrokerExists()
		{
			EntryHeader.Declaration.ImportEntryNumber = "123456789";
			EntryHeader.Declaration.US_US_NKLocationOfGoods = "A000";
			EntryHeader.Declaration.US_BRDRefNo = "1234567890";

			EntryHeader.Declaration.SetExternalBrokerForTesting();
			BIRDStatusMessageBuilder builder = new BIRDStatusMessageBuilder(EntryHeader);
			builder.BuildENRecord();

			MQEDIMessage message = builder.PopulateMessage();

			AssertEquals("Service task will look at this status", EDIMessage.Status.Pending, message.EM_Status);
		}

		public void TestGenerateDTRecords()
		{
			GetPopulatedDeclaration();

			BIRDStatusMessageBuilder builder = new BIRDStatusMessageBuilder(EntryHeader);
			builder.BuildDTRecords(new string[]
				{
					BIRDDateQualifierList.Codes.ArrivalAtFirstPortUnlading,
					BIRDDateQualifierList.Codes.ArrivalAtPortOfEntry,
					BIRDDateQualifierList.Codes.DutyDueDate,
					BIRDDateQualifierList.Codes.DutyPaid,
					BIRDDateQualifierList.Codes.Liquidation,
					BIRDDateQualifierList.Codes.Statement,
				});

			MQEDIMessage message = builder.PopulateMessage();
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Acknowledged, message.EM_Status);
			AssertMessageGenerated(message);
		}

		public void TestGenerateDTRecordsWithAllCodes()
		{
			GetPopulatedDeclaration();
			EntryHeader.Declaration.SetExternalBrokerForTesting();

			BIRDStatusMessageBuilder builder = new BIRDStatusMessageBuilder(EntryHeader);
			builder.BuildDTRecords();

			MQEDIMessage message = builder.PopulateMessage();
			AssertMessageGenerated(message);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Pending, message.EM_Status);
		}

		public void TestGenerateCargoReleaseProcessingResult()
		{
			MQEDIMessage message1 = Factory.New<MQEDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			message1.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message1.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 010000480191-013199000B00001004FTRDADMIRALENGRACHT     1356 061307    R4            13465865424                         00001000KG   AAAD             R5062207100306ENTRY DOCUMENTS REQUIRED                                          Y018888XJ5RR00003";
			EntryHeader.Declaration.Messages.Add(message1);

			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			message2.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message2.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 0100004811576-06-3002 B00001159NZ                      008  020609    R4            08693584352                         00000011CT                    R5021009134101COND RELEASE GEN EXAM                                             R5021009134171AMS AIR CARRIER/CFS NOTIFIED                                      R5021009134122RELEASE DATE UPDATE                     02100901                  R6FDA    021009134102FDA HOLD                                                   R6FDA    0210091341  FDA DOCUMENTS REQUIRED           140011001THRU0011001      Y012801M15RR00007";
			EntryHeader.Declaration.Messages.Add(message2);

			Factory.Save();

			message1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 10, 1);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2009, 9, 1);

			BIRDStatusMessageBuilder builder = new BIRDStatusMessageBuilder(EntryHeader.Declaration);
			builder.BuildTheLatestCargoProcessingResult();

			MQEDIMessage message = builder.PopulateMessage();

			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.BIRDStatusRecords, message.EM_MessageSubType);
			AssertContains("AASTATXJ5", message.EM_MessageText);
			AssertContains("R18888XJ5 010000480191-013199000B00001004FTRDADMIRALENGRACHT     1356 061307    R4            13465865424                         00001000KG   AAAD             R5062207100306ENTRY DOCUMENTS REQUIRED                                          ", message.EM_MessageText);
			AssertEquals(EntryHeader.Declaration, message.EM_LinkedObject);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals(EDIMessage.Status.Acknowledged, message.EM_Status);
		}

		void GetPopulatedDeclaration()
		{
			EntryHeader.Declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 1);
			EntryHeader.Declaration.US_EntryDate = new ZDateTime(2009, 2, 1);
			EntryHeader.Declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 2, 15);
			EntryHeader.Declaration.US_PaymentDueDate = new ZDateTime(2009, 2, 18);
			EntryHeader.Declaration.US_PaymentDate = new ZDateTime(2009, 2, 16);
			EntryHeader.Declaration.US_BRDRefNo = "1234567890";

			CusLiquidation liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_LiquidationDate = new ZDateTime(2009, 3, 9);
			liquidation.B8_SystemCreateDate = new ZDateTime(2009, 3, 9);
			EntryHeader.Declaration.Liquidations.Add(liquidation);

			CusLiquidation liquidation2 = Factory.New<CusLiquidation>();
			liquidation2.B8_LiquidationDate = new ZDateTime(2009, 3, 15);
			liquidation2.B8_SystemCreateDate = new ZDateTime(2009, 3, 15);
			EntryHeader.Declaration.Liquidations.Add(liquidation2);

			CusLiquidation liquidation3 = Factory.New<CusLiquidation>();
			liquidation3.B8_LiquidationDate = new ZDateTime(2009, 2, 27);
			liquidation3.B8_SystemCreateDate = new ZDateTime(2009, 2, 27);
			EntryHeader.Declaration.Liquidations.Add(liquidation3);
		}

		void AssertMessageGenerated(MQEDIMessage message)
		{
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.BIRDStatusRecords, message.EM_MessageSubType);
			AssertContains("AASTATXJ5", message.EM_MessageText);
			AssertContains(BIRDDateQualifierList.Codes.ArrivalAtFirstPortUnlading + " 20090101", message.EM_MessageText);
			AssertContains(BIRDDateQualifierList.Codes.ArrivalAtPortOfEntry + " 20090201", message.EM_MessageText);
			AssertContains(BIRDDateQualifierList.Codes.DutyDueDate + " 20090218", message.EM_MessageText);
			AssertContains(BIRDDateQualifierList.Codes.DutyPaid + " 20090216", message.EM_MessageText);
			AssertContains(BIRDDateQualifierList.Codes.Liquidation + " 20090315", message.EM_MessageText);
			AssertContains(BIRDDateQualifierList.Codes.Statement + " 20090215", message.EM_MessageText);

			AssertEquals("2 DT records are contrusted", 2, message.MessageBlock.MessageBlocks.FindAll(x => x is BRDDT).Count);

			AssertEquals(EntryHeader, message.EM_LinkedObject);

			AssertNoExceptionThrown(() => Factory.Save());

			AssertContains("1234567890", ((BRDAA)message.MessageBlock.B).OriginatingBrokerRef);
		}

		CusEntryHeader EntryHeader
		{
			get
			{
				if (entryHeader == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					declaration.US_EntryFilerCode = "XJ5";
					declaration.US_EnableENS = true;

					declaration.Invoices.AddNew();
					declaration.InvoiceLines.AddNew();

					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

					entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				}
				return entryHeader;
			}
		}
		CusEntryHeader entryHeader;
	}
}
