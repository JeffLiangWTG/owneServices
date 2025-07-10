using CargoWise.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(EDIMessage))]
	sealed class EDIMessageTest : EDIMessageAbstractTest<EDIMessage>
	{
		public void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.USeManifest, message.EM_ApplicationCode);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestOriginalMessageWhenDuplicateMessageNumberExistsOnDifferentJobs()
		{
			var trip1 = Factory.New<Trip>();
			trip1.BH_JobReference = "MAN0008004";
			trip1.BH_VoyageNumber = "MAN0008004";
			trip1.BH_CarrierSCAC = "VLTK";
			var outgoingMessage1 = Factory.New<EDIMessage>();
			outgoingMessage1.EM_MessageType = MessageTypes.Codes.eManifest;
			outgoingMessage1.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageText = "UNH+11516+CUSCAR:D:03B:UN'BGM+85:::STANDARD+VLTKMAN0008004+22'DTM+132:202307171039:203'LOC+60+2506:77'RFF+ABO:MAN11516'NAD+CA+VLTK:172'NAD+VW+0014044674:109+++8601 S US HIGHWAY 85+BUCKEYE+AZ:163+85326'TDT+11++03+:::TR++I++:146::4V4NC9EH9PN334433'TDT+11++03+:::TR++I++:215::R619008:US'LOC+89+TX:163'EQD+TF'RFF+ABZ:132653T'LOC+89+TN:163'LOC+89+US:162'CNI+1+:23'RFF+AAM:VLTKDSV24178565'LOC+9+97102:78'LOC+103+W084:276'GEI+7+135'TDT+11'RFF+AWM'TSR+9'NAD+OS+++MASONITE MEXICO SA DE CV+CARRETERA MONTERREY A LAREDO+CIENEGA DE FLORES+NLE:163+65550+MX'NAD+CN+++MASONITE CORPORATION+3632 PETERSEN RD+STOCKTON+CA:163+95215+US'GID+1'PAC+16++BDL'FTX+AAA+++SKIN DOORS'MEA+AAI++K:42116'SGP+132653T:215'UNT+30+11516'";
			outgoingMessage1.EM_Status = EDIMessage.Status.Sent;
			trip1.Messages.Add(outgoingMessage1);

			var trip2 = Factory.New<Trip>();
			trip2.BH_JobReference = "MAN0008008";
			trip2.BH_VoyageNumber = "MAN0008008";
			trip2.BH_CarrierSCAC = "VLTK";
			var outgoingMessage2 = Factory.New<EDIMessage>();
			outgoingMessage2.EM_MessageType = MessageTypes.Codes.eManifest;
			outgoingMessage2.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageText = "UNH+11516+CUSCAR:D:03B:UN'BGM+85:::STANDARD+VLTKMAN0008008+22'DTM+132:202307171100:203'LOC+60+2507:77'RFF+ABO:MAN11516'NAD+CA+VLTK:172'NAD+VW+0013678900:109+++8601 S US HIGHWAY 85+BUCKEYE+AZ:163+85326'TDT+11++03+:::TR++I++:146::1FUJGLD59GLHE6678'TDT+11++03+:::TR++I++:215::R422102:US'LOC+89+TX:163'UNT+11+11516'";
			outgoingMessage2.EM_Status = EDIMessage.Status.Sent;
			trip2.Messages.Add(outgoingMessage2);
			Factory.Save();

			outgoingMessage1.EM_MessageNum = "11516";
			outgoingMessage2.EM_MessageNum = "11516";
			Factory.Save();

			var incomingMessage1 = Factory.New<EDIMessage>();
			incomingMessage1.EM_MessageType = MessageTypes.Codes.eManifest;
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_MessageNum = "11516";
			incomingMessage1.EM_MessageOwner = "MAN0008004";
			incomingMessage1.EM_MessageText = "UNH+999999999+CUSRES:D:03B:UN'BGM+132+VLTKMAN0008004'DTM+132:202307171039:203'FTX+AIQ+++MAN11516'TDT+11++03+:::TR+VLTK+++:8::2C283537B3FA0E000004EDE4'TDT+11++03+:::TR+VLTK+++:146::4V4NC9EH9PN334433'LOC+60+2506'ERP+1'ERC+081'FTX+AAO+++Manifest Transmittal'UNT+11+999999999'";

			var incomingMessage2 = Factory.New<EDIMessage>();
			incomingMessage2.EM_MessageType = MessageTypes.Codes.eManifest;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_MessageNum = "11516";
			incomingMessage2.EM_MessageOwner = "MAN0008008";
			incomingMessage2.EM_MessageText = "UNH+000133041+CUSRES:D:03B:UN'BGM+34+VLTKMAN0008008'DTM+132:202307171100:203'FTX+AIQ+++MAN11516'TDT+11++03+:::TR+VLTK+++:8::2C283537B3FA0E0000041C6F'TDT+11++03+:::TR+VLTK+++:146::1FUJGLD59GLHE6678'LOC+24+2507:77'RFF+ACD:R422102'LOC+89+TX:163'LOC+89+US:162'ERP+1'ERC+SN038'FTX+AAH+++Release Trip'UNT+14+000133041'";

			AssertEquals("OutgoingMessage1 should be matched based on message number '11516' & trip reference 'VLTKMAN0008004'", outgoingMessage1.PK, incomingMessage1.OriginalMessage.PK);
			AssertEquals("OutgoingMessage2 should be matched based on message number '11516' & trip reference 'VLTKMAN0008008'", outgoingMessage2.PK, incomingMessage2.OriginalMessage.PK);
		}

		public void TestReportErrorIfMessageNumberHasBeenUsed()
		{
			ErrorReporter.Clear();
			var numberFountain = Env.NumberFountains.EDIFACTNumberFountain("M", "MAN", "USC");
			var trip = Factory.New<Trip>();
			trip.BH_JobReference = "MAN0008004";

			var outgoingMessage1 = Factory.New<EDIMessage>();
			outgoingMessage1.EM_MessageType = MessageTypes.Codes.eManifest;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			trip.Messages.Add(outgoingMessage1);
			Factory.Save();

			outgoingMessage1.EM_MessageNum = "11516";
			Factory.Save();

			numberFountain.SetNext(Factory, 11516);
			var outgoingMessage2 = Factory.New<EDIMessage>();
			trip.Messages.Add(outgoingMessage2);
			outgoingMessage2.EM_MessageType = MessageTypes.Codes.eManifest;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			Factory.Save();
			AssertEquals(11517, long.Parse(outgoingMessage2.EM_MessageNum));

			try
			{
				AssertContains("Developers exception thrown", $"Message Number 11516 has been used by e-Manifest job MAN0008004, a new message number will be allocated.", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
	}
}
