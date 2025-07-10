using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class AMSInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestProcessManifestResponseMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			ZString interchangeHeader =
				"ACR8CAR      MR11032301200500101                                                ";
			ZString interchangeBody =
				"M01OTT111USTOWER BRIDGE           451  00001000007 8505989                      " +
				"M02123456789012_79                                                              " +
				"P01270403261100002    0001                                                      " +
				"W02OTT11103230120050100100001000000000000000000000000100017                     ";
			ZString interchangeFooter = "ZCR8CAR      MR                   00004";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.AMS, "USC", "8CAR",
				interchangeHeader, interchangeBody, interchangeFooter);
			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);

			IInboundInterchangeProcessor processor = new AMSInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, interchange.EI_InterchangeType);

			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CBPEDIInterchange.ApplicationCodes.AMS);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, "79");
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			var msgs = Factory.Load<AMSEDIMessage>(ediMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];
			AssertEquals("EM_MessageText", interchangeHeader + interchangeBody + interchangeFooter, msg.EM_MessageText);
		}

		public void TestPreProcessMessagesForInterchangeAMS()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "B$1";
			branch.GB_BranchName = "111 NAME";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "B$2";
			branch2.GB_BranchName = "222 NAME";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			ZString interchangeHeader =
				"ACR8CAR      MR11032301200500101                                                ";
			ZString interchangeBody =
				"M01OTT111USTOWER BRIDGE           451  00001000007 8505989                      " +
				"P01270403261100002    0001                                                      " +
				"W02OTT11103230120050100100001000000000000000000000000100017                     ";
			ZString interchangeFooter = "ZCR8CAR      MR                   00004";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.AMS, "USC", "8CAR",
				interchangeHeader, interchangeBody, interchangeFooter);
			interchange.EI_GB = branch2.PK;
			Factory.Save();

			IInboundInterchangeProcessor processor = new AMSInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, interchange.EI_InterchangeType);

			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CBPEDIInterchange.ApplicationCodes.AMS);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, "8CAR0323012005101");
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			var msgs = Factory.Load<AMSEDIMessage>(ediMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];
			AssertEquals("EM_GB", branch2.PK, msg.EM_GB);
		}

		public void TestProcessManifestResponseMessage_WithNoCarrierAssignedBatchNumber()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			ZString interchangeHeader =
				"ACR8CAR      MR11032301200500101                                                ";
			ZString interchangeBody =
				"M01OTT111USTOWER BRIDGE           451  00001000007 8505989                      " +
				"P01270403261100002    0001                                                      " +
				"W02OTT11103230120050100100001000000000000000000000000100017                     ";
			ZString interchangeFooter = "ZCR8CAR      MR                   00004";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.AMS, "USC", "8CAR",
				interchangeHeader, interchangeBody, interchangeFooter);
			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);

			IInboundInterchangeProcessor processor = new AMSInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, interchange.EI_InterchangeType);

			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CBPEDIInterchange.ApplicationCodes.AMS);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, "8CAR0323012005101");
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			var msgs = Factory.Load<AMSEDIMessage>(ediMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];
			AssertEquals("EM_MessageText", interchangeHeader + interchangeBody + interchangeFooter, msg.EM_MessageText);
		}

		public void TestProcessManifestResponseMessage_FalseArePrerequisiteReceivingConditionsMet()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			ZString interchangeHeader =
				"ACR8CAR      MR11032301200500101                                                ";
			ZString interchangeBody =
				"M01OTT111USTOWER BRIDGE           451  00001000007 8505989                      " +
				"M02123456789012_79                                                              " +
				"P01270403261100002    0001                                                      " +
				"W02OTT11103230120050100100001000000000000000000000000100017                     ";
			ZString interchangeFooter = "ZCR8CAR      MR                   00004";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.AMS, "USC", "8CAR",
				interchangeHeader, interchangeBody, interchangeFooter);
			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);

			var logger = new LoggingInformation();
			IInboundInterchangeProcessor processor = new AMSInboundInterchangeProcessor(logger);
			processor.Execute();

			interchange.Reload();
			AssertEquals("RCV", interchange.EI_Status);
			AssertEquals("One log should be created", 1, logger.UserLogStrings.Count);

			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CBPEDIInterchange.ApplicationCodes.AMS);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, "79");
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			var msgs = Factory.Load<AMSEDIMessage>(ediMessageQuery);
			AssertEquals("messages created.", 1, msgs.Length);
		}
	}
}
