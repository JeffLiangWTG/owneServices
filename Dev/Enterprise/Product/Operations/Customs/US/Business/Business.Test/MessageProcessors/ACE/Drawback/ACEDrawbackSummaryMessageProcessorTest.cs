using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ACEDrawbackSummaryMessageProcessorTest : ABIProcessorTest<ACEDrawbackSummaryMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQuery;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQueryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal;
			message.EM_MessageText =
"B003902SV9DX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022098 B00000001                                 " +
"E1 F198   INITIAL PAY TYP CANNOT BE INDIVD PAYMENTSV9  70022098     B00000001   " +
"E0 BNDDTL 000001 REF ID: 8 B 566                                                " +
"E0 IMPORT 000001 REF ID: XJ5  00000001 0001                                     " +
"E0 CLASSI 000001 REF ID: 8471704065                                             " +
"E0 QTYUOM 000001 REF ID: KG                                                     " +
"E0 HDRREV 000001 REF ID: 501 00000210                                           " +
"E0 MANUFD 000001 REF ID: 8471704065 000000151380081516                          " +
"E0 EXPDES 000001 REF ID: D8471704056 08151646810215                             " +
"E0 NOIHDR 000001 REF ID:                                                        " +
"E0 NOIEWR 000001 REF ID: A10210000                                              " +
"E0 NAFTAD 000001 REF ID: 8471704065          0815169001021000 CA                " +
"E0 TFTEAE 000001 REF ID: C8471704065 08151670022098                             " +
"E0 TOTALS 000001 REF ID:                                                        " +
"E0 REVTOT 000001 REF ID: 053 00000165900                                        " +
"E1 W27C   *CENSUS* OR-LO VAL XJ5 70022098 2       SV9  70022098     B00000001   " +
"E1 F103   QTY CLMD > IMP QTY WFN86315179 5        SV9  70022098     B00000001   " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00000001   " +
"Y  3902SV9DX00015";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration, message.EM_LinkedObject);
			AssertEquals("Status changed", DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryOriginal, declaration.JE_MessageStatus);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var expectedBodyMessage = @"<tr><th colspan=""4"" align=""left"">Entry Summary Identifier</th></tr><tr><td colspan=""4"">Entry Filer Code: <strong>SV9</strong> Entry Number: <strong>70022098</strong> Broker Reference Number: <strong>B00000001</strong></td></tr><tr><th>Disposition</th><th>Severity</th><th>Condition Code</th><th>Text</th></tr><tr><td>&nbsp;</td><td>F</td><td>198</td><td> <strong>INITIAL PAY TYP CANNOT BE INDIVD PAYMENT</strong></td></tr><tr><th colspan=""4"" align=""left"">Bond</th></tr><tr><td colspan=""4"">Bond Type Code: <strong>8</strong> Bond Designation Type Code: <strong>B</strong> Surety Company Code: <strong>566</strong></td></tr><tr><th colspan=""4"" align=""left"">Import Grouping</th></tr><tr><td colspan=""4"">Release Entry Filer Code: XJ5 Import Entry Summary Number: 00000001 Import Entry Summary Line Number: 0001</td></tr><tr><th colspan=""4"" align=""left"">Import Entry Summary Classification</th></tr><tr><td colspan=""4"">H T S Number: <strong>8471704065</strong></td></tr><tr><th colspan=""4"" align=""left"">Import Entry Summary Quantity & UOM</th></tr><tr><td colspan=""4"">Unit of Measure Code: <strong>KG</strong></td></tr><tr><th colspan=""4"" align=""left"">Import Entry Summary Revenue Claimed</th></tr><tr><td colspan=""4"">Accounting Class Code: <strong>501</strong> Revenue Amount: <strong>00000210</strong></td></tr><tr><th colspan=""4"" align=""left"">Manufactured/Produced Articles</th></tr><tr><td colspan=""4"">H T S Number: <strong>8471704065</strong> Manufacturing Ruling Number: <strong>000000151380</strong> Production Date: <strong>15-Aug-16</strong></td></tr><tr><th colspan=""4"" align=""left"">Export/Destroy Articles</th></tr><tr><td colspan=""4"">Export Destroy  Indicator: <strong>D</strong> H T S Number: <strong>8471704056</strong> Export Destroy  Date: <strong>15-Aug-16</strong> Unique Identifier: <strong>46810215</strong></td></tr><tr><th colspan=""4"" align=""left"">&nbsp;</th></tr><tr><td colspan=""4"">&nbsp;</td></tr><tr><th colspan=""4"" align=""left"">Notice of Intent</th></tr><tr><td colspan=""4"">Record Indicator: <strong>A</strong> C B P Personnel Badge: <strong>10210000</strong></td></tr><tr><th colspan=""4"" align=""left"">NAFTA Details</th></tr><tr><td colspan=""4"">Entry Number: <strong>8471704065</strong> Entry Date: <strong>15-Aug-16</strong> H T S 1: <strong>9001021000</strong> Country of Export: <strong>CA</strong></td></tr><tr><th colspan=""4"" align=""left"">Export/Destroy Articles</th></tr><tr><td colspan=""4"">Export Destroy Indicator: <strong>C</strong> H T S Number: <strong>8471704065</strong> Export Destroy Date: <strong>15-Aug-16</strong> Unique Identifier: <strong>70022098</strong></td></tr><tr><th colspan=""4"" align=""left"">&nbsp;</th></tr><tr><td colspan=""4"">&nbsp;</td></tr><tr><th colspan=""4"" align=""left"">Total</th></tr><tr><td colspan=""4"">Accounting Class Code: <strong>053</strong> Total Fee Amount: <strong>00000165900</strong></td></tr><tr><th>Disposition</th><th>Severity</th><th>Condition Code</th><th>Text</th></tr><tr><td>&nbsp;</td><td>W</td><td>27C</td><td> <strong>*CENSUS* OR-LO VAL XJ5 70022098 2</strong></td></tr><tr><th>Disposition</th><th>Severity</th><th>Condition Code</th><th>Text</th></tr><tr><td>&nbsp;</td><td>F</td><td>103</td><td> <strong>QTY CLMD > IMP QTY WFN86315179 5</strong></td></tr><tr><th>Disposition</th><th>Severity</th><th>Condition Code</th><th>Text</th></tr><tr><td>Rejected</td><td>F</td><td>998</td><td> <strong>TRANSACTION DATA REJECTED</strong></td></tr></table>";
			AssertContains(expectedBodyMessage, email.Body);
		}

		MQEDIMessage GetMessageToProcess()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
