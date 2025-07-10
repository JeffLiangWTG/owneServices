using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	public class CusresMessageProcessorProviderTest : BlockingParallelProcessingProviderTest
	{
		protected override LinkedBusinessObjectMetaData GenerateExpectedLinkedObject() => new LinkedBusinessObjectMetaData(destination.TableName, destination.PK, outgoingMessage.EM_GB, ZString.Empty);

		protected override ProcessingResult<SerializationKeysResult> GenerateExpectedSerializationKeysResult() => ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { voyage.PK.ToString() }));

		protected override void GenerateMessageWithNoLinkedObject()
		{
			incomingMessage = Factory.New<StowPlanMessage>();
			incomingMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			incomingMessage.EM_MessageText = "XXX";
			incomingMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
		}

		JobVoyage voyage;
		VoyageDestination destination;

		protected override void PrepareTestingData()
		{
			voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);
			destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(10);
			voyage.GenerateSailings();

			outgoingMessage = Factory.New<StowPlanMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = destination;
			outgoingMessage.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";

			Factory.Save();

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.StowPlan;
			interchange.EI_From = "USCBPTST";
			interchange.EI_To = "8CAR";
			interchange.EI_HeaderText = "UNB+UNOA:4+USCBPTST:ZZZ+8CAR+130128:2239+100000114586++++0'UNG+CUSRES+USCBPTST+8CAR+130128:2239+1086431+UN+D:05B'";
			interchange.EI_BodyText = "UNH+1086431+CUSRES:D:05B:UN'BGM+294+" + outgoingMessage.EM_MessageNum + "'TDT+20+1299+1++APLU:172:ZZZ:APL BAHRAIN+++9395927:146:11:APL BAHRAIN'RFF+AAA'DTM+133:20120420'LOC+5+USBAL'RFF+AAA'DTM+132:20120430'LOC+61'ERP+1'ERC+SB4'FTX+AAH+++VESSEL IMO NOT ON FILE'ERP+1'ERC+SBB'FTX+AAH+++VESSEL ARRIVAL DATE IN THE PAST'ERP+1'ERC+SB8'FTX+AAH+++MISSING VESSEL ARRIVAL PORT'ERP+1'ERC+SBT'FTX+AAH+++VESSEL DEPARTURE DATE MORE THAN 30 DAYS IN THE PAST'ERP+1'ERC+S01'FTX+AAH+++REJECTED'UNT+25+1086431'";
			interchange.EI_FooterText = "UNE+1+1086431'UNZ+1+100000114586'";

			incomingMessage = Factory.New<StowPlanMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_EI = interchange.PK;
			incomingMessage.EM_MessageText = interchange.EI_BodyText;

			Factory.Save();
		}
	}
}
