using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class StowPlanInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestProcessInterchange()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var interchange = NewEDIInterchange();
			var logger = new LoggingInformation();
			new StowPlanInterchangeProcessor(logger).ExecuteBatch();
			interchange.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(EDIInterchange.ApplicationCodes.StowPlan, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchange.ApplicationCodes.StowPlan, interchange.EI_InterchangeType);
			AssertEquals(EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
			AssertEquals("USCBPTST", interchange.EI_From);
			AssertEquals("8CAR", interchange.EI_To);
			AssertEquals("UNB+UNOA:4+USCBPTST:ZZZ+8CAR+130128:2239+100000114586++++0'", interchange.EI_HeaderText);
			AssertEquals("UNH+1086431+CUSRES:D:05B:UN'BGM+294+1'TDT+20+1299+1++APLU:172:ZZZ:APL BAHRAIN+++9395927:146:11:APL BAHRAIN'RFF+AAA'DTM+133:20120420'LOC+5+USBAL'RFF+AAA'DTM+132:20120430'LOC+61'ERP+1'ERC+SB4'FTX+AAH+++VESSEL IMO NOT ON FILE'ERP+1'ERC+SBB'FTX+AAH+++VESSEL ARRIVAL DATE IN THE PAST'ERP+1'ERC+SB8'FTX+AAH+++MISSING VESSEL ARRIVAL PORT'ERP+1'ERC+SBT'FTX+AAH+++VESSEL DEPARTURE DATE MORE THAN 30 DAYS IN THE PAST'ERP+1'ERC+S01'FTX+AAH+++REJECTED'UNT+25+1086431'", interchange.EI_BodyText);
			AssertEquals("UNZ+1+100000114586'", interchange.EI_FooterText);
			AssertEquals(1, interchange.ContainedMessages.Count);
			var message = interchange.ContainedMessages[0];
			AssertEquals(EDIInterchange.ApplicationCodes.StowPlan, message.EM_ApplicationCode);
			AssertEquals(EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(interchange.EI_BodyText, message.EM_MessageText);
		}

		public void TestProcessInterchange_FalseArePrerequisiteReceivingConditionsMet()
		{
			var interchange = NewEDIInterchange();
			var logger = new LoggingInformation();
			var processor = new StowPlanInterchangeProcessor(logger);
			processor.ExecuteBatch();
			interchange.Reload();
			AssertEquals("RCV", interchange.EI_Status);
			AssertEquals("One log should be created", 1, logger.UserLogStrings.Count);
			AssertContains("Log message", "Interchange '100000114586' has been processed successfully.", logger.UserLogStrings[0]);
			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.StowPlan);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
			var msgs = Factory.Load<StowPlanMessage>(ediMessageQuery);
			AssertEquals("1 messages created", 1, msgs.Length);
		}

		EDIInterchange NewEDIInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.StowPlan;
			interchange.EI_From = "USCBPTST";
			interchange.EI_To = "8CAR";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_BodyText = "UNB+UNOA:4+USCBPTST:ZZZ+8CAR+130128:2239+100000114586++++0'UNG+CUSRES+USCBPTST+8CAR+130128:2239+1086431+UN+D:05B'UNH+1086431+CUSRES:D:05B:UN'BGM+294+1'TDT+20+1299+1++APLU:172:ZZZ:APL BAHRAIN+++9395927:146:11:APL BAHRAIN'RFF+AAA'DTM+133:20120420'LOC+5+USBAL'RFF+AAA'DTM+132:20120430'LOC+61'ERP+1'ERC+SB4'FTX+AAH+++VESSEL IMO NOT ON FILE'ERP+1'ERC+SBB'FTX+AAH+++VESSEL ARRIVAL DATE IN THE PAST'ERP+1'ERC+SB8'FTX+AAH+++MISSING VESSEL ARRIVAL PORT'ERP+1'ERC+SBT'FTX+AAH+++VESSEL DEPARTURE DATE MORE THAN 30 DAYS IN THE PAST'ERP+1'ERC+S01'FTX+AAH+++REJECTED'UNT+25+1086431'UNE+1+1086431'UNZ+1+100000114586'";
			Factory.Save();
			return interchange;
		}
	}
}
