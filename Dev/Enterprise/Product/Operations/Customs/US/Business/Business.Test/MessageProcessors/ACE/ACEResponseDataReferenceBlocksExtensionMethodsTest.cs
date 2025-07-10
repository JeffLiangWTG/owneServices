using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEResponseDataReferenceBlocksExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetPGAParameter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("No response message yet", 0, coll.ENSERecords.Count);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			message.EM_MessageText =
				"B001101SV9AX                                               HYEDUSCMT_162451     " +
				"E0 SUMMRY 000001 REF ID: SV9 71009920 B00163412                                 " +
				"E0 LINITM 0002   REF ID:2                                                       " +
				"E0 TARIFF 000001 REF ID:2921196010                                              " +
				"E0 OI            REF ID: PESTICIDES                                             " +
				"E0 PG01          REF ID: 001EPAPS1   Y                        130.027           " +
				"E1 PPH6   MISSING DIS DOCUMENTATION                                             " +
				"E0 PG02          REF ID: P125                                                   " +
				"E1 FP61   INVALID PRODUCT CODE QUALIFIER                                        " +
				"E0 PSTLIN 000001 REF ID: SV9 71009920 B00163412                                 " +
				"E1 FPGA   PGA DATA REJECTED                       SV9  71009920     B00163412   " +
				"E1RF998   TRANSACTION DATA REJECTED               SV9  71009920     B00163412   " +
				"Y  1101SV9AX00004";

			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			var messageBlocks = message.MessageBlock.MessageBlocks.OfType<ITariffNumberStatusAndErrors>().ToArray();

			var e1block1 = messageBlocks[0];
			AssertNotNull(e1block1);
			AssertEquals("EPA", e1block1.PGAAgencyCode);
			AssertEquals("001", e1block1.PGALineNo);

			var e1block2 = messageBlocks[1];
			AssertNotNull(e1block2);
			AssertEquals("EPA", e1block2.PGAAgencyCode);
			AssertEquals("001", e1block2.PGALineNo);
		}

		MQEDIMessage CreateMessage(string receiveTransmit, string applicationIdentifier)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var result = mock.Object;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = receiveTransmit;
			result.EM_MessageType = applicationIdentifier;
			result.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";

			return result;
		}
	}
}
