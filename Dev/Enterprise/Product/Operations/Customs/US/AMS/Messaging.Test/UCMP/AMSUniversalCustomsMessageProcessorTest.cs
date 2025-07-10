using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	[TestedType(typeof(AMSUniversalCustomsMessageProcessor))]
	[TestDate(2025, 3, 3)]
	public sealed class AMSUniversalCustomsMessageProcessorTest : CommonUniversalCustomsMessageProcessorTest<AMSUniversalCustomsMessageProcessor>
	{
		protected override string ApplicationCode => BaseEDIMessage.ApplicationCodes.AMS;

		protected override CommonUniversalCustomsMessageProcessor CreateProcessor() => new AMSUniversalCustomsMessageProcessor();

		public void TestManifestCreateResponseMessage()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "Z!Z";
			branch1.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			var header1 = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header1.BH_GB = branch1.PK;
			var moveHeader1 = header1.MovementHeader;

			var originalMessage = Factory.New<AMSEDIMessage>();
			originalMessage.EM_GB = branch1.PK;
			originalMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "DNZ001";
			originalMessage.EM_LinkedObject = moveHeader1;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = "ACR";
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse;
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_MessageNum = "DNZ002";
			AssertUniversalCustomsMessageProcessorCanHandleMessage(message, EDIMessage.Status.Discarded);

			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "DNZ001";
			AssertUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void TestManifestCreateTransmissionResponseMessage()
		{
			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);

			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			Factory.Save();

			var manifestSequenceNumber = moveHeder.BM_ManifestSequenceNumber;
			var consolRef = consol.JK_UniqueConsignRef;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L 00001" + manifestSequenceNumber.PadLeft(6) + " 7819369                      " +
"M02HB1105031520_OTT14000086                                                     " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     ";
			interchange.EI_FooterText = "ZCR8CWS      MI                   00004";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			AssertUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void TestManifestAmendmentResponseMessage()
		{
			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);

			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
			messageTransmit.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingDelete;
			messageTransmit.EM_MessageOwner = Constants.ACE;
			messageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			var manifestSequenceNumber = moveHeder.BM_ManifestSequenceNumber;
			var consolRef = consol.JK_UniqueConsignRef;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR          AR11050303221800085                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L 00001" + manifestSequenceNumber.PadLeft(6) + " 7819369                      " +
"M02HB1105031520_OTT14000086                                                     " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     ";
			interchange.EI_FooterText = "ZCR          AR                   00004";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			AssertUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void TestManifestEditResponseMessage()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "Z!Z";
			branch1.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			var header1 = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header1.BH_GB = branch1.PK;
			var moveHeader1 = header1.MovementHeader;

			var originalMessage = Factory.New<AMSEDIMessage>();
			originalMessage.EM_GB = branch1.PK;
			originalMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestEdit;
			originalMessage.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "DNZ81";
			originalMessage.EM_LinkedObject = moveHeader1;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText =
				"ACR          KR11032823060900004                                                " +
				"M01OTT111USTOWER BRIDGE           451  00001000007 8505989                      " +
				"M0281                                                                           " +
				"P01270403261100002    0001                                                      " +
				"W02OTT11103282306060100100001000000000000000000000000100019                     " +
				"ZCR          KI                   00004                                         ";
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse;
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_MessageNum = "DNZ81";
			AssertUniversalCustomsMessageProcessorCanHandleMessage(message);
		}
	}
}
