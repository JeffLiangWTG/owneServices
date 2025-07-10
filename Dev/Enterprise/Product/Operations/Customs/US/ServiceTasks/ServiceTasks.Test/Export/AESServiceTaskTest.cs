using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using EDIMessage = Enterprise.Customs.US.Business.EDIMessage;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(AESServiceTask))]
	sealed class AESServiceTaskTest : ServiceTaskTestCase<AESServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("USE", "United States Export Customs Messaging", "USC");
		}

		[TestDate(2009, 12, 23)]
		public void TestPackagingAndProcessingAESTEDIMessage()
		{
			using (var dir1 = new TempDirectory())
			{
				var company1PK = GlbCompany.CurrentCompany.PK.ToGuid();
				var filer = new ExportEntryFilerID();
				filer.EntryFilerID = "861161674";
				filer.EntryFilerIDType = "D";
				USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(company1PK, Guid.Empty, Guid.Empty, filer);
				USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(company1PK, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
				var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
				currentUser.GS_EmailAddress = "dummy@where.com";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_DeclarationReference = "B44000001";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
				entry.CH_BGMReference = "B44000001";
				var outgoingMessage = (AESTIREDIMessage)entry.Messages.AddNew(typeof(AESTIREDIMessage));
				outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
				outgoingMessage.EM_Status = EDIMessage.Status.Queued;
				outgoingMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Transmit;
				outgoingMessage.EM_SystemCreateUser = currentUser.GS_Code;
				outgoingMessage.EM_MessageText = "B  60612456712E          US EXPORTER NAME                                       Y  60612456712E          US EXPORTER NAME";
				outgoingMessage.EM_SendWithMessageErrors = true;
				Factory.Save();
				var encodedMessageNum = AESTIRMessageNumberEncoder.Encode(new ZInt(outgoingMessage.EM_MessageNum));
				var incomingInterchange = Factory.New<CBPEDIInterchange>();
				incomingInterchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsExport;
				incomingInterchange.EI_InterchangeType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
				incomingInterchange.EI_From = "USC";
				incomingInterchange.EI_To = "DUS";
				incomingInterchange.EI_HeaderText = "A    861161674CAREDIEXT20091207" + encodedMessageNum + "N861161674";
				incomingInterchange.EI_BodyText = "B  60612456712E          US EXPORTER NAME                                       " +
					"SC1N11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 N    " +
					"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
					"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
					"Y  60612456712E          US EXPORTER NAME";
				incomingInterchange.EI_FooterText = "Z    861161674      EXT20091207" + encodedMessageNum + " 861161674";
				var incomingMessage = (AESTIREDIMessage)incomingInterchange.ContainedMessages.AddNew(typeof(AESTIREDIMessage));
				incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				incomingMessage.EM_Status = EDIMessage.Status.Queued;
				incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
				incomingMessage.EM_MessageNum = outgoingMessage.EM_MessageNum;
				incomingMessage.EM_MessageText = incomingInterchange.EI_BodyText;
				Factory.Save();
				AssertEquals(EDIMessage.Status.Queued, outgoingMessage.EM_Status);
				AssertEquals(ZGuid.Empty, outgoingMessage.EM_EI);
				AssertEquals(EDIMessage.Status.Queued, incomingMessage.EM_Status);
				var serviceTask = new AESServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
				incomingMessage.Reload();
				outgoingMessage.Reload();
				AssertEquals(EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				var outgoingInterchange = (CBPEDIInterchange)outgoingMessage.Interchange;
				AssertNotNull(outgoingInterchange);
				AssertEquals(CBPEDIInterchange.ApplicationCodes.USCustomsExport, outgoingInterchange.EI_ApplicationCode);
				AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipment, outgoingInterchange.EI_InterchangeType);
				AssertEquals(CBPEDIInterchange.Status.eHubQueued, outgoingInterchange.EI_Status);
				AssertEquals(EDIInterchange.TransportType.eHub, outgoingInterchange.EI_TransportType);
				AssertEquals("A    861161674      DXP20091223" + encodedMessageNum.PadRight(6) + " 861161674", outgoingInterchange.EI_HeaderText);
				AssertEquals(outgoingMessage.EM_MessageText, outgoingInterchange.EI_BodyText);
				AssertEquals("Z    861161674      DXP20091223" + encodedMessageNum.PadRight(6) + " 861161674", outgoingInterchange.EI_FooterText);
				AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals(entry, incomingMessage.EM_LinkedObject);
			}
		}

		public void TestCompanyBranchBecomeInactiveDuringProcessing() => new USServiceTaskTestCommon().AssertCompanyBranchBecomeInactiveDuringProcessing<AESServiceTask>();

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"US Customs AES export interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.USCustomsExport),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs AES export messages outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsExport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.AES.CommodityShipment,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs AES export messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsExport,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
