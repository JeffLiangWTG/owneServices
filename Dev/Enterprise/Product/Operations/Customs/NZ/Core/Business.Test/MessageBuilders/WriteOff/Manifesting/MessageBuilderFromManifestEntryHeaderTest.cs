using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.ZArchitecture.Environment;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;
using NZCMessage = Enterprise.Customs.NZ.Business.Declaration.NZCMessage;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting.Testing
{
	using System;
	using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Messaging.Business;

	public class MessageBuilderFromManifestEntryHeaderTest : MessageBuilders.Testing.MessageBuilderTest
	{
		public void TestCreatedCNIInSequence()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			var creator = new TestManifestCreator(EntryHeader, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2005, 11, 30), new ZDateTime(2005, 12, 1));
			var declaration1 = creator.AddDeclaration(JobMessageTypeList.Codes.Import, creator.Supplier1, creator.Importer1, "HOUSEBILL1", "1", 15.2m, 4, 45.54m);
			var declaration2 = creator.AddDeclaration(JobMessageTypeList.Codes.Import, creator.Supplier1, creator.Importer1, "HOUSEBILL2", "1", 15.2m, 4, 45.54m);

			declaration1.JE_DeclarationReference = EntryHeader.CH_BGMReference + "-2";
			declaration2.JE_DeclarationReference = EntryHeader.CH_BGMReference + "-1";

			var message = new MessageBuilderFromManifestEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original).GetMessageText();
			Assert(message.IndexOf("CNI+1") < message.IndexOf("CNI+2"));
		}

		#region Test4ConsignmentOriginal
		public void Test4ConsignmentOriginal()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			TestManifestCreator manifestCreator = new TestManifestCreator(EntryHeader, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2005, 11, 30), new ZDateTime(2005, 12, 1));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer2, "HOUSEBILL2", "RATS TAILS", 14.3m, 3, 36.63m);
			JobDeclaration declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL3", "RATS TEETH", 13.4m, 2, 27.72m);
			JobDeclaration declaration4 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL4", "RATS CLAWS", 12.5m, 1, 18.81m);
			EntryHeader.CH_CustomsMessageRemarks = "PLEASE IGNORE THE MONKEY\r\nWE KNOW IT LIKES TO BE SPANKED\r\n";

			CreateMessageAndTestResultAgainstString(Expected4ConsignmentOriginalMessage, ECIMessageGenerator.MessageTypes.Original);
		}
		#region Expected4ConsignmentOriginalMessage
		const string Expected4ConsignmentOriginalMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++TEST CARRIER
FTX+AAI+++PLEASE IGNORE THE MONKEY:WE KNOW IT LIKES TO BE SPANKED
TDT+20++4+++++:::QF253
LOC+22+NZAKL
DTM+132:20051201:102
GIS+10:105:143
GIS+N:109:143
CNT+10:4
CNI+1
RFF+HWB:HOUSEBILL1
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+4:PK
FTX+AAA+++RATS HEADS
MEA+WT+AAG+KGM:15.200
MOA+14:45.54:NZD
LOC+4+USLAX
CNI+2
RFF+HWB:HOUSEBILL2
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+3:PK
FTX+AAA+++RATS TAILS
MEA+WT+AAG+KGM:14.300
MOA+14:36.63:NZD
LOC+4+USLAX
CNI+3
RFF+HWB:HOUSEBILL3
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+2:PK
FTX+AAA+++RATS TEETH
MEA+WT+AAG+KGM:13.400
MOA+14:27.72:NZD
LOC+4+USDNQ
CNI+4
RFF+HWB:HOUSEBILL4
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+1:PK
FTX+AAA+++RATS CLAWS
MEA+WT+AAG+KGM:12.500
MOA+14:18.81:NZD
LOC+4+USDNQ
UNT+61+<<MSGNO PLACEHOLDER>>
";
		#endregion
		#endregion

		#region Test3ConsignmentReplacementHeader
		public void Test3ConsignmentReplacementHeader()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			TestManifestCreator manifestCreator = new TestManifestCreator(EntryHeader, "081-11111111", "QF254", "USDNQ", "NZCHC", new ZDateTime(2005, 11, 27), new ZDateTime(2005, 12, 4));
			manifestCreator.Carrier.OH_FullName = "NEW TEST CARRIER";
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer2, "HOUSEBILL2", "RATS TAILS", 14.3m, 3, 36.63m);
			JobDeclaration declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL3", "RATS TEETH", 13.4m, 2, 27.72m);
			EntryHeader.CH_CustomsMessageRemarks = "LEFT HANDED SPANNERS SHOULD BE WASTED ON THE POOR";

			CreateMessageAndTestResultAgainstString(Expected3ConsignmentReplacementHeaderMessage, ECIMessageGenerator.MessageTypes.ReplaceHeader);
		}
		#region Expected3ConsignmentReplacementHeaderMessage
		const string Expected3ConsignmentReplacementHeaderMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+20
NAD+CA++NEW TEST CARRIER
FTX+AAI+++LEFT HANDED SPANNERS SHOULD BE WASTED ON THE POOR
TDT+20++4+++++:::QF254
LOC+22+NZCHC
DTM+132:20051204:102
GIS+10:105:143
GIS+N:109:143
UNT+10+<<MSGNO PLACEHOLDER>>
";
		#endregion
		#endregion

		#region Test2ConsignmentReplacementLines
		public void Test2ConsignmentReplacementLines()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			TestManifestCreator manifestCreator = new TestManifestCreator(EntryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3));
			manifestCreator.Carrier.OH_FullName = "ANOTHER TEST CARRIER";
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);
			EntryHeader.CH_CustomsMessageRemarks = "RIGHT HANDED SCREWDRIVERS HAVE A LIMITED USE";

			CreateMessageAndTestResultAgainstString(Expected2ConsignmentReplacementLinesMessage, ECIMessageGenerator.MessageTypes.ReplaceConsignment);
		}
		#region Expected2ConsignmentReplacementLinesMessage
		const string Expected2ConsignmentReplacementLinesMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+21
FTX+AAI+++RIGHT HANDED SCREWDRIVERS HAVE A LIMITED USE
CNT+10:2
CNI+1
RFF+HWB:HOUSEBILL1
LOC+9+USSFO
LOC+11+NZWLG
LOC+27+US
LOC+7+NZAKL
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+4:PK
FTX+AAA+++RATS HEADS
MEA+WT+AAG+KGM:15.200
MOA+14:45.54:NZD
LOC+4+USLAX
CNI+2
RFF+HWB:HOUSEBILL2
LOC+9+USSFO
LOC+11+NZWLG
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+2:PK
FTX+AAA+++RATS TEETH
MEA+WT+AAG+KGM:13.400
MOA+14:27.72:NZD
LOC+4+USDNQ
UNT+31+<<MSGNO PLACEHOLDER>>
";
		#endregion
		#endregion

		#region Test5ConsignmentCancellation
		public void Test5ConsignmentCancellation()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			TestManifestCreator manifestCreator = new TestManifestCreator(EntryHeader, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2005, 11, 30), new ZDateTime(2005, 12, 1));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer2, "HOUSEBILL2", "RATS TAILS", 14.3m, 3, 36.63m);
			JobDeclaration declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL3", "RATS TEETH", 13.4m, 2, 27.72m);
			JobDeclaration declaration4 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL4", "RATS CLAWS", 12.5m, 1, 18.81m);
			JobDeclaration declaration5 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL5", "RATS NOSES", 11.6m, 99, 99.88m);
			EntryHeader.CH_CustomsMessageRemarks = "OK I GUESS WE JUST SCERWED UP";

			CreateMessageAndTestResultAgainstString(Expected5ConsignmentCancellationMessage, ECIMessageGenerator.MessageTypes.CancelECI);
		}
		#region Expected5ConsignmentCancellationMessage
		const string Expected5ConsignmentCancellationMessage =
@"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+1
FTX+AAI+++OK I GUESS WE JUST SCERWED UP
UNT+4+<<MSGNO PLACEHOLDER>>
";
		#endregion
		#endregion

		public void Test10ConsignmentMessageGetsConsignmentIDsRight()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			TestManifestCreator manifestCreator = new TestManifestCreator(EntryHeader, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2005, 11, 30), new ZDateTime(2005, 12, 1));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer2, "HOUSEBILL2", "RATS TAILS", 14.3m, 3, 36.63m);
			JobDeclaration declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL3", "RATS TEETH", 13.4m, 2, 27.72m);
			JobDeclaration declaration4 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL4", "RATS CLAWS", 12.5m, 1, 18.81m);
			JobDeclaration declaration5 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL5", "RATS NOSES", 11.6m, 99, 99.88m);
			JobDeclaration declaration6 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL6", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration7 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer2, "HOUSEBILL7", "RATS TAILS", 14.3m, 3, 36.63m);
			JobDeclaration declaration8 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL8", "RATS TEETH", 13.4m, 2, 27.72m);
			JobDeclaration declaration9 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL9", "RATS CLAWS", 12.5m, 1, 18.81m);
			JobDeclaration declaration10 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL10", "RATS NOSES", 11.6m, 99, 99.88m);

			CreateMessageAndTestResultAgainstString(Expected10ConsignmentOriginalMessage, ECIMessageGenerator.MessageTypes.Original);
		}
		#region Expected10ConsignmentOriginalMessage
		const string Expected10ConsignmentOriginalMessage =
@"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++TEST CARRIER
TDT+20++4+++++:::QF253
LOC+22+NZAKL
DTM+132:20051201:102
GIS+10:105:143
GIS+N:109:143
CNT+10:10
CNI+1
RFF+HWB:HOUSEBILL1
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+4:PK
FTX+AAA+++RATS HEADS
MEA+WT+AAG+KGM:15.200
MOA+14:45.54:NZD
LOC+4+USLAX
CNI+2
RFF+HWB:HOUSEBILL2
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+3:PK
FTX+AAA+++RATS TAILS
MEA+WT+AAG+KGM:14.300
MOA+14:36.63:NZD
LOC+4+USLAX
CNI+3
RFF+HWB:HOUSEBILL3
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+2:PK
FTX+AAA+++RATS TEETH
MEA+WT+AAG+KGM:13.400
MOA+14:27.72:NZD
LOC+4+USDNQ
CNI+4
RFF+HWB:HOUSEBILL4
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+1:PK
FTX+AAA+++RATS CLAWS
MEA+WT+AAG+KGM:12.500
MOA+14:18.81:NZD
LOC+4+USDNQ
CNI+5
RFF+HWB:HOUSEBILL5
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+99:PK
FTX+AAA+++RATS NOSES
MEA+WT+AAG+KGM:11.600
MOA+14:99.88:NZD
LOC+4+USDNQ
CNI+6
RFF+HWB:HOUSEBILL6
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+4:PK
FTX+AAA+++RATS HEADS
MEA+WT+AAG+KGM:15.200
MOA+14:45.54:NZD
LOC+4+USLAX
CNI+7
RFF+HWB:HOUSEBILL7
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+3:PK
FTX+AAA+++RATS TAILS
MEA+WT+AAG+KGM:14.300
MOA+14:36.63:NZD
LOC+4+USLAX
CNI+8
RFF+HWB:HOUSEBILL8
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+2:PK
FTX+AAA+++RATS TEETH
MEA+WT+AAG+KGM:13.400
MOA+14:27.72:NZD
LOC+4+USDNQ
CNI+9
RFF+HWB:HOUSEBILL9
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+1:PK
FTX+AAA+++RATS CLAWS
MEA+WT+AAG+KGM:12.500
MOA+14:18.81:NZD
LOC+4+USDNQ
CNI+10
RFF+HWB:HOUSEBILL10
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+99:PK
FTX+AAA+++RATS NOSES
MEA+WT+AAG+KGM:11.600
MOA+14:99.88:NZD
LOC+4+USDNQ
UNT+134+<<MSGNO PLACEHOLDER>>";
		#endregion

		#region TestGenerateTestMessage
		public override void TestGenerateTestMessage()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			TestManifestCreator manifestCreator = new TestManifestCreator(EntryHeader, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2005, 11, 30), new ZDateTime(2005, 12, 1));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer2, "HOUSEBILL2", "RATS TAILS", 14.3m, 3, 36.63m);
			EntryHeader.CH_CustomsMessageRemarks = "PLEASE IGNORE THE MONKEY\r\nWE KNOW IT LIKES TO BE SPANKED\r\n";

			messageBuilder = new MessageBuilderFromManifestEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("0", messageBuilder.PostedMessageNumber);
			messageBuilder.GenerateMessage();
			Assert(messageBuilder.PostedMessageNumber != "0");

			EDIMessage message = messageBuilder.MessageBusinessObject;
			AssertNotNull("Message", message);
			AssertEquals("Message.IsInDatabase", false, message.IsInDatabase);
			message.Factory.Save();
			AssertEquals("Message.EM_MessageText.IndexOf(NZCMessage.SendersReferencePlaceHolder)", -1, message.EM_MessageText.IndexOf(NZCMessage.SendersReferencePlaceHolder));
			AssertEquals("Message.EM_MessageText.IndexOf(NZCMessage.MessageNumberPlaceHolder)", -1, message.EM_MessageText.IndexOf(NZCMessage.MessageNumberPlaceHolder));
			AssertEquals("Message.EM_Status", NZCMessage.Status.Queued, message.EM_Status);
			AssertEquals("Message.EM_IsTestMessage", true, message.EM_IsTestMessage);
			AssertEquals("Message.EM_MessageSubType", NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original, message.EM_MessageSubType);
			AssertEquals("Message.EM_LinkUniqueID == EntryHeader.PK", EntryHeader.PK, message.EM_LinkUniqueID);

			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, EntryHeader.CH_EntryStatus);

			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Declaration1.JE_EDITransmitDate", declaration1.CachedTodaysDate, declaration1.JE_EDITransmitDate);
			AssertEquals("Declaration1.JE_EntrySubmittedDate", declaration1.CachedTodaysDate, declaration1.JE_EntrySubmittedDate);

			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EDITransmitDate", declaration1.CachedTodaysDate, declaration2.JE_EDITransmitDate);
			AssertEquals("Declaration2.JE_EntrySubmittedDate", declaration1.CachedTodaysDate, declaration2.JE_EntrySubmittedDate);

			Assert2MessagesAreTheSame(PostedTestMessage, message.EM_MessageText);
		}
		#region PostedTestMessage
		const string PostedTestMessage = @"UNH+1+CUSCAR:D:98A:UN
BGM+785+M01010101+9
NAD+CA++TEST CARRIER
FTX+AAI+++PLEASE IGNORE THE MONKEY:WE KNOW IT LIKES TO BE SPANKED
TDT+20++4+++++:::QF253
LOC+22+NZAKL
DTM+132:20051201:102
GIS+10:105:143
GIS+N:109:143
CNT+10:2
CNI+1
RFF+HWB:HOUSEBILL1
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+4:PK
FTX+AAA+++RATS HEADS
MEA+WT+AAG+KGM:15.200
MOA+14:45.54:NZD
LOC+4+USLAX
CNI+2
RFF+HWB:HOUSEBILL2
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+3:PK
FTX+AAA+++RATS TAILS
MEA+WT+AAG+KGM:14.300
MOA+14:36.63:NZD
LOC+4+USLAX
UNT+36+1
";
		#endregion
		#endregion

		#region TestGenerateLiveMessage
		public override void TestGenerateLiveMessage()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			TestManifestCreator manifestCreator = new TestManifestCreator(EntryHeader, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2005, 11, 30), new ZDateTime(2005, 12, 1));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer1, "HOUSEBILL3", "RATS TEETH", 13.4m, 2, 27.72m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer2, "HOUSEBILL2", "RATS TAILS", 14.3m, 3, 36.63m);
			EntryHeader.CH_CustomsMessageRemarks = "PLEASE IGNORE THE MONKEY\r\nWE KNOW IT LIKES TO BE SPANKED\r\n";

			messageBuilder = new MessageBuilderFromManifestEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.ReplaceConsignment);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("0", messageBuilder.PostedMessageNumber);
			messageBuilder.GenerateMessage();
			Assert(messageBuilder.PostedMessageNumber != "0");

			EDIMessage message = messageBuilder.MessageBusinessObject;
			AssertNotNull("Message", message);
			AssertEquals("Message.IsInDatabase", false, message.IsInDatabase);
			message.Factory.Save();
			AssertEquals("Message.EM_MessageText.IndexOf(NZCMessage.SendersReferencePlaceHolder)", -1, message.EM_MessageText.IndexOf(NZCMessage.SendersReferencePlaceHolder));
			AssertEquals("Message.EM_MessageText.IndexOf(NZCMessage.MessageNumberPlaceHolder)", -1, message.EM_MessageText.IndexOf(NZCMessage.MessageNumberPlaceHolder));
			AssertEquals("Message.EM_Status", NZCMessage.Status.Queued, message.EM_Status);
			AssertEquals("Message.EM_IsTestMessage", false, message.EM_IsTestMessage);
			AssertEquals("Message.EM_MessageSubType", NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.ReplaceLines, message.EM_MessageSubType);
			AssertEquals("Message.EM_LinkUniqueID == EntryHeader.PK", EntryHeader.PK, message.EM_LinkUniqueID);

			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, EntryHeader.CH_EntryStatus);

			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Declaration1.JE_EDITransmitDate", declaration1.CachedTodaysDate, declaration1.JE_EDITransmitDate);
			AssertEquals("Declaration1.JE_EntrySubmittedDate", declaration1.CachedTodaysDate, declaration1.JE_EntrySubmittedDate);

			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EDITransmitDate", declaration1.CachedTodaysDate, declaration2.JE_EDITransmitDate);
			AssertEquals("Declaration2.JE_EntrySubmittedDate", declaration1.CachedTodaysDate, declaration2.JE_EntrySubmittedDate);

			Assert2MessagesAreTheSame(PostedLiveMessage, message.EM_MessageText);
		}
		#region PostedLiveMessage
		const string PostedLiveMessage = @"UNH+1+CUSCAR:D:98A:UN
BGM+785+M01010101+21
FTX+AAI+++PLEASE IGNORE THE MONKEY:WE KNOW IT LIKES TO BE SPANKED
CNT+10:2
CNI+1
RFF+HWB:HOUSEBILL3
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
NAD+CN++TEST IMPORTER 1:1 IMPORTER ROAD:IMPORTERVILLE:AUCKLAND AUK 1012
NAD+CZ++TEST SUPPLIER 2:2 SUPPLIER ROAD:SUPPLIERVILLE:DENVER PA 90210
GID+1+2:PK
FTX+AAA+++RATS TEETH
MEA+WT+AAG+KGM:13.400
MOA+14:27.72:NZD
LOC+4+USDNQ
CNI+2
RFF+HWB:HOUSEBILL2
LOC+9+USLAX
LOC+11+NZAKL
LOC+27+US
LOC+7+NZCHC
NAD+CN++TEST IMPORTER 2:2 IMPORTER ROAD:IMPORTERVILLE:CHRISTCHURCH CAN 2012
NAD+CZ++TEST SUPPLIER 1:1 SUPPLIER ROAD:SUPPLIERVILLE:LOS ANGELES CA 90120
GID+1+3:PK
FTX+AAA+++RATS TAILS
MEA+WT+AAG+KGM:14.300
MOA+14:36.63:NZD
LOC+4+USLAX
UNT+30+1
";
		#endregion
		#endregion

		public void TestEmptyCusEntryHeaderThrowsNoExceptionsWithOldReferernce()
		{
			EntryHeader.CH_BGMReference = NumberFountains.OldECIManifestReferencePrefix + "01010101";
			SingleDeclaration.LinkToManifest(EntryHeader, 1);
			EntryHeader.CH_JE = SingleDeclaration.PK;
			messageBuilder = new MessageBuilderFromManifestEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			AssertNoExceptionThrown(delegate
			{ messageBuilder.GetMessageText(); });
		}

		public void TestEmptyCusEntryHeaderThrowsNoExceptionsWithNewReferernce()
		{
			EntryHeader.CH_BGMReference = NumberFountains.ECIManifestReferencePrefix + "01010101";
			SingleDeclaration.LinkToManifest(EntryHeader, 1);
			EntryHeader.CH_JE = SingleDeclaration.PK;
			messageBuilder = new MessageBuilderFromManifestEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			AssertNoExceptionThrown(delegate
			{ messageBuilder.GetMessageText(); });
		}

		#region Implementation
		#region CreateMessageAndTestResultAgainstString
		protected void CreateMessageAndTestResultAgainstString(string expectedMessageText, ECIMessageGenerator.MessageTypes messageType)
		{
			messageBuilder = new MessageBuilderFromManifestEntryHeader(EntryHeader, messageType);
			TestMessageBuilderResultAgainstString(expectedMessageText.Replace("'", ""));
		}
		#endregion

		#region EntryHeader
		protected CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = Factory.New<CusEntryHeader>();
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;
		#endregion

		#region SingleDeclaration
		protected JobDeclaration SingleDeclaration
		{
			get
			{
				if (fSingleDeclaration == null)
				{
					fSingleDeclaration = JobDeclaration.New(Factory);
				}
				return fSingleDeclaration;
			}
		}
		JobDeclaration fSingleDeclaration;
		#endregion

		#region messageBuilder
		protected new MessageBuilderFromManifestEntryHeader messageBuilder
		{
			get { return (MessageBuilderFromManifestEntryHeader)base.messageBuilder; }
			set { base.messageBuilder = value; }
		}
		#endregion
		#endregion
	}
}
