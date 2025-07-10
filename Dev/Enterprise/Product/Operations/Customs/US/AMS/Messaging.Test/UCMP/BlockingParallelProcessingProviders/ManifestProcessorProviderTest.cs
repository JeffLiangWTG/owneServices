using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	public class ManifestProcessorProviderTest : BlockingParallelProcessingProviderTest
	{
		[TestDate(2011, 03, 26)]
		public void TestLinkedBusinessObjectMetaData_Linker()
		{
			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			header.BH_ApplicationCode = "AMS";
			header.BH_GB = Env.CurrentBranch.PK;
			header.BH_ImportConveyanceName = "TOWER BRIDGE";
			header.BH_VoyageNumber = "451";
			header.BH_PortUnladingDCode = "2704";
			header.BH_ETA = new ZDateTime(2011, 03, 26);

			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeader = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);
			moveHeader.BM_ManifestSequenceNumber = "000007";

			var msnQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, moveHeader.PK);
			msnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, "MSN");
			var mSNCusEntryNumber = (BusinessObject)Factory.LoadTop1<Integration.Customs.ICusEntryNumber>(msnQuery);
			mSNCusEntryNumber[CusEntryNumSchema.CE_EntryLineReference] = "OTT1";

			Factory.Save();

			incomingMessage = Factory.New<AMSEDIMessage>();
			incomingMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			incomingMessage.EM_MessageText =
				"ACR          KR11032823060900004                                                " +
				"M01OTT111USTOWER BRIDGE           451  00001000007 8505989                      " +
				"M0281                                                                           " +
				"P01270403261100002    0001                                                      " +
				"W02OTT11103282306060100100001000000000000000000000000100019                     " +
				"ZCR          KI                   00004                                         ";
			incomingMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse;
			incomingMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertLinkedBusinessObjectMetaData(incomingMessage, new LinkedBusinessObjectMetaData(CusInBondMoveHeaderSchema.Constants.TableName, moveHeader.PK, header.BH_GB, "C00001000"));
		}

		protected override LinkedBusinessObjectMetaData GenerateExpectedLinkedObject() => new LinkedBusinessObjectMetaData(CusInBondMoveHeaderSchema.Constants.TableName, amsMoveHeader.PK, outgoingMessage.EM_GB, ZString.Empty);

		protected override ProcessingResult<SerializationKeysResult> GenerateExpectedSerializationKeysResult() => ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "AMS0010010" }));

		protected override void GenerateMessageWithNoLinkedObject()
		{
			incomingMessage = Factory.New<AMSEDIMessage>();
			incomingMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			incomingMessage.EM_MessageText = "ACR";
			incomingMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse;
			incomingMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
		}

		Integration.Customs.US.USAMS.ICusInBondMoveHeader amsMoveHeader;

		protected override void PrepareTestingData()
		{
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ApplicationCode = "INB";
			header.BH_GB = Env.CurrentBranch.PK;
			header.BH_JobReference = "AMS0010010";

			amsMoveHeader = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			amsMoveHeader.BM_BH = header.PK;

			outgoingMessage = Factory.New<AMSEDIMessage>();
			outgoingMessage.EM_GB = Env.CurrentBranch.PK;
			outgoingMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			outgoingMessage.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			outgoingMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "DNZ001";
			outgoingMessage.EM_LinkUniqueID = amsMoveHeader.PK;
			outgoingMessage.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;

			incomingMessage = Factory.New<AMSEDIMessage>();
			incomingMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			incomingMessage.EM_MessageText = "ACR";
			incomingMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse;
			incomingMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "DNZ001";
		}
	}
}
