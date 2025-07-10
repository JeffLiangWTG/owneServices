using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	[TestDate(2012, 06, 19)]
	public class StatusNotificationProcessorProviderTest : BlockingParallelProcessingProviderTest
	{
		protected override LinkedBusinessObjectMetaData GenerateExpectedLinkedObject() => new LinkedBusinessObjectMetaData(CusInBondMoveHeaderSchema.Constants.TableName, moveHeader.PK, Env.CurrentBranch.PK, "AMS0000001");

		protected override ProcessingResult<SerializationKeysResult> GenerateExpectedSerializationKeysResult() => ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "333210146" }));

		protected override void GenerateMessageWithNoLinkedObject()
		{
			incomingMessage = Factory.New<AMSEDIMessage>();
			incomingMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			incomingMessage.EM_MessageText = "ACR";
			incomingMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			incomingMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
		}

		protected override ProcessingResult<LinkedBusinessObjectMetaData> GenerateExpectedMetaData_WithNoLinkedObject() => new LinkedBusinessObjectMetaData(ZString.Empty, ZGuid.Empty, Env.CurrentBranch.PK, ZString.Empty);

		Integration.Customs.US.USAMS.ICusInBondMoveHeader moveHeader;

		protected override void PrepareTestingData()
		{
			var consol = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_TransportMode] = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ApplicationCode = "INB";
			header.BH_CarrierSCAC = "CARL";
			header.BH_GB = Env.CurrentBranch.PK;
			header.BH_VoyageNumber = "ST013";
			header.BH_ImportConveyanceName = "HYUNDAI SINGAPORE";
			header.BH_PortUnladingDCode = "0901";
			header.BH_ETA = new ZDateTime(2012, 06, 19);

			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			moveHeader = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;

			var entryNumber = (BusinessObject)Factory.New<Integration.Customs.ICusEntryNumber>();
			entryNumber[CusEntryNumSchema.CE_ParentID] = moveHeader.PK;
			entryNumber[CusEntryNumSchema.CE_ParentTable] = CusInBondMoveHeaderSchema.Constants.TableName;
			entryNumber[CusEntryNumSchema.CE_EntryNum] = "333210146";
			entryNumber[CusEntryNumSchema.CE_EntryLineReference] = "CARL";
			entryNumber[CusEntryNumSchema.CE_EntryType] = "INB";

			var headerBill = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			headerBill.B0_BH = header.PK;
			headerBill.B0_IssuerCode = "CARL";
			headerBill.B0_MasterBillNumber = "CBP01307";

			var moveHederDetail = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveDetail>();
			moveHederDetail.B9_BM = moveHeader.PK;
			moveHederDetail.B9_B0 = headerBill.PK;
			Factory.Save();

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			interchange.EI_From = "USC";
			interchange.EI_To = "HYEDUKCMT";
			interchange.EI_HeaderText = "ACR          RC12061920375415774                                                ";
			interchange.EI_BodyText = "M01CARL30ITHYUNDAI SINGAPORE      ST013     000001                              R01CARL0901HYUNDAI SINGAPORE      ST013000001120619                             J01CARL                                                                         R02CBP01307    69000000000462333210146      1206192037             1            R020901    390262200                                                            B04SNPOTT1                                                                      R03BILL ON FILE                                                                 R05NC                                                                           ";
			interchange.EI_FooterText = "ZCR          RC                   00008                                         ";

			incomingMessage = Factory.New<AMSEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			incomingMessage.EM_EI = interchange.PK;
			incomingMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			incomingMessage.EM_MessageText = interchange.EI_InterchangeText;

			Factory.Save();
		}
	}
}
