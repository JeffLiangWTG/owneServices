using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class ManifestMessageAttacheeMessageLinkerTest : TestCaseWithFactory
	{
		public void TestLink()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "OTT1";
			var moveHeader = header.MovementHeader;
			moveHeader.BM_ManifestSequenceNumber = "002200";
			moveHeader.MSNCusEntryNumber.CE_EntryLineReference = "OTT1";
			IManifestMessageAttacheeMessageLinker linker = new ManifestMessageAttacheeMessageLinker();
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertEquals(moveHeader, ManifestMessageAttacheeMessageLinker.Link(message, "OTT1", "002200", "", "", "", ZDate.Empty, "", "", "", "", "", null, null));
			AssertEquals(moveHeader, message.EM_LinkedObject);
			AssertEquals(moveHeader.TableName, message.EM_LinkTable);
			message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertEquals(moveHeader, linker.Link(message, "OTT1", "002200", "", "", "", ZDate.Empty, "", "", "", "", "", null, null));
			AssertEquals(moveHeader, message.EM_LinkedObject);
			AssertEquals(moveHeader.TableName, message.EM_LinkTable);
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~123232131";
			var message2 = Factory.New<AMSEDIMessage>();
			message2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message2.EM_MessageNum = "~123232131";
			AssertEquals(moveHeader, ManifestMessageAttacheeMessageLinker.Link(message2, "", "", "", "", "", ZDate.Empty, "", "", "", "", "", null, null));
			AssertEquals(moveHeader, message2.EM_LinkedObject);
			AssertEquals(moveHeader.TableName, message2.EM_LinkTable);
		}

		public void TestLinkToDifferentCarrier()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_CarrierSCAC = "OTT1";
			header1.BH_ImportConveyanceName = "ABC VESSEL";
			header1.BH_VoyageNumber = "V343";
			header1.BH_PortUnladingDCode = "2704";
			header1.BH_ETA = new ZDateTime(2012, 6, 3);
			var moveHeader1 = header1.MovementHeader;
			moveHeader1.BM_ManifestSequenceNumber = "002200";
			moveHeader1.MSNCusEntryNumber.CE_EntryLineReference = "OTT1";
			var bill1A = header1.Bills.AddNew();
			bill1A.B0_IssuerCode = "OTT2";
			bill1A.B0_MasterBillNumber = "MB323423";
			moveHeader1.MovementDetails.AddNew(bill1A.PK);
			var bill1B = header1.Bills.AddNew();
			bill1B.B0_IssuerCode = "OTT2";
			bill1B.B0_MasterBillNumber = "MB895685";
			moveHeader1.MovementDetails.AddNew(bill1B.PK);
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_CarrierSCAC = "OTT2";
			header2.BH_ImportConveyanceName = "ABC VESSEL";
			header2.BH_VoyageNumber = "V343";
			header2.BH_PortUnladingDCode = "2704";
			header2.BH_ETA = new ZDateTime(2012, 6, 3);
			var moveHeader2 = header2.MovementHeader;
			var bill2A = header2.Bills.AddNew();
			bill2A.B0_IssuerCode = "OTT2";
			bill2A.B0_MasterBillNumber = "MB323423";
			var bill2B = header2.Bills.AddNew();
			bill2B.B0_IssuerCode = "OTT2";
			bill2B.B0_MasterBillNumber = "MB493453";
			moveHeader2.MovementDetails.AddNew(bill2B.PK);
			IManifestMessageAttacheeMessageLinker linker = new ManifestMessageAttacheeMessageLinker();
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertEquals(moveHeader1, ManifestMessageAttacheeMessageLinker.Link(message, "OTT1", "002200", "ABC VESSEL", "V343", "2704", new ZDate(2012, 6, 3), "OTT2", "MB323423", "", "", "", new Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>>(x => true), null));
			AssertEquals(moveHeader1, message.EM_LinkedObject);
			AssertEquals(moveHeader1.TableName, message.EM_LinkTable);
			message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertEquals(moveHeader2, ManifestMessageAttacheeMessageLinker.Link(message, "OTT2", "002200", "ABC VESSEL", "V343", "2704", new ZDate(2012, 6, 3), "OTT2", "MB323423", "", "", "", new Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>>(x => true), null));
			AssertEquals(moveHeader2, message.EM_LinkedObject);
			AssertEquals(moveHeader2.TableName, message.EM_LinkTable);
			message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertEquals("Should match even if the carrier is different", moveHeader1, ManifestMessageAttacheeMessageLinker.Link(message, "OTT2", "002200", "ABC VESSEL", "V343", "2704", new ZDate(2012, 6, 3), "OTT2", "MB895685", "", "", "", new Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>>(x => true), null));
			AssertEquals(moveHeader1, message.EM_LinkedObject);
			AssertEquals(moveHeader1.TableName, message.EM_LinkTable);
			message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertEquals(moveHeader2, ManifestMessageAttacheeMessageLinker.Link(message, "OTT1", "002210", "ABC VESSEL", "V343", "2704", new ZDate(2012, 6, 3), "OTT2", "MB493453", "", "", "", new Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>>(x => true), null));
			AssertEquals(moveHeader2, message.EM_LinkedObject);
			AssertEquals(moveHeader2.TableName, message.EM_LinkTable);
		}

		public void TestLinkForInBond()
		{
			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = "INB";
			header.BH_CarrierSCAC = "CARL";
			header.BH_GB = Enterprise.Environment.Env.CurrentBranch.PK;
			header.BH_VoyageNumber = "ST013";
			header.BH_ImportConveyanceName = "HYUNDAI SINGAPORE";
			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			moveHeader.InBondNumber = "333210146";
			var headerBill = Factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			headerBill.B0_BH = header.PK;
			headerBill.B0_IssuerCode = "CARL";
			headerBill.B0_MasterBillNumber = "CBP01307";
			var moveHederDetail = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			moveHederDetail.B9_BM = moveHeader.PK;
			moveHederDetail.B9_B0 = headerBill.PK;
			Factory.Save();
			IManifestMessageAttacheeMessageLinker linker = new ManifestMessageAttacheeMessageLinker();
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertEquals(moveHeader, ManifestMessageAttacheeMessageLinker.Link(message, "CARL", "", "HYUNDAI SINGAPORE", "ST013", "", ZDate.Empty, "CARL", "CBP01307", "", "", "333210146", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.MasterInBond, null));
			AssertEquals(moveHeader, message.EM_LinkedObject);
			message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			AssertEquals(null, linker.Link(message, "OTT1", "002200", "", "", "", ZDate.Empty, "", "", "", "", "", null, null));
			AssertEquals(null, message.EM_LinkedObject);
			AssertEquals(ZString.Empty, message.EM_LinkTable);
		}
	}
}
