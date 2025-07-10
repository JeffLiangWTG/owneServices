using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	[TestedType(typeof(StowPlanUniversalCustomsMessageProcessor))]
	[TestDate(2025, 3, 3)]
	public sealed class StowPlanUniversalCustomsMessageProcessorTest : CommonUniversalCustomsMessageProcessorTest<StowPlanUniversalCustomsMessageProcessor>
	{
		protected override string ApplicationCode => BaseEDIMessage.ApplicationCodes.StowPlan;

		protected override CommonUniversalCustomsMessageProcessor CreateProcessor() => new StowPlanUniversalCustomsMessageProcessor();

		public void TestStowPlanMessageCanBeProcessed()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "Z!Z";
			branch1.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);
			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "USCHI";
			destination1.JB_E_ARV = ZDateTime.Today.AddDays(10);
			voyage.GenerateSailings();

			var originalMessage = Factory.New<StowPlanMessage>();
			originalMessage.EM_GB = branch1.PK;
			originalMessage.EM_ReceiveTransmit = StowPlanMessage.Direction.Transmit;
			originalMessage.EM_LinkedObject = destination1;

			var message = Factory.New<StowPlanMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageNum = "";
			message.EM_MessageText = "UNH+4+CUSRES:D:05B:UN'BGM+294+{0}'TDT+20+VOY444+1++8CAR:172:ZZZ:MAERSK ENFIELD+++9463047:146:11:MAERSK ENFIELD'RFF+AAA'DTM+133:2109210000'LOC+5+CAAAB'RFF+AAA'DTM+132:2109290000'LOC+61+USCHI'ERP+1'ERC+S02'FTX+AAH+++ACCEPTED'UNT+31+4";
			AssertUniversalCustomsMessageProcessorCanHandleMessage(message);
		}
	}
}
