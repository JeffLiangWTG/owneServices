using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AES;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AESTIREDIMessage))]
	sealed class AESTIRMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOriginalAndResponseMessagesAreLoadedCorrectly()
		{
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "C2!";
			company2.GC_Name = "COMPANY 2";
			company2.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company2.Branches.AddNew();
			branch1.GB_Code = "B1!";
			branch1.GB_BranchName = "BRANCH 1";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "B2!";
			branch2.GB_BranchName = "BRANCH 2";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var message1OtherCompanyTrx = CreateMessage<AESTIREDIMessage>(branch2.PK, EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.AES.CommodityShipment);
			var message2CurrentBranchTrx = CreateMessage<AESTIREDIMessage>(GlbBranch.CurrentBranch.PK, EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.AES.CommodityShipment);
			Factory.Save();
			message1OtherCompanyTrx.EM_MessageNum = "1A";
			message2CurrentBranchTrx.EM_MessageNum = "1B";
			var message1OtherCompanyRcv = CreateMessage<AESTIREDIMessage>(branch1.PK, EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, message1OtherCompanyTrx.EM_MessageNum);
			var message1CurrentBranchRcv = CreateMessage<AESTIREDIMessage>(GlbBranch.CurrentBranch.PK, EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, message1OtherCompanyTrx.EM_MessageNum);
			var message2OtherCompanyRcv = CreateMessage<AESTIREDIMessage>(branch1.PK, EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, message2CurrentBranchTrx.EM_MessageNum);
			var message2CurrentBranchRcv = CreateMessage<AESTIREDIMessage>(GlbBranch.CurrentBranch.PK, EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, message2CurrentBranchTrx.EM_MessageNum);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var message1OtherCompanyTrxInOtherFactory = newFactory.Load<EDIMessage>(message1OtherCompanyTrx.PK);
			var message1OtherCompanyTrxInOtherFactoryResponse = message1OtherCompanyTrxInOtherFactory.ResponseMessage;
			AssertEquals("Should match to other company response message 1", message1OtherCompanyRcv.PK, message1OtherCompanyTrxInOtherFactoryResponse.PK);
			AssertEquals(message1OtherCompanyTrxInOtherFactory, message1OtherCompanyTrxInOtherFactoryResponse.OriginalMessage);
			var message2CurrentBranchTrxInOtherFactory = newFactory.Load<EDIMessage>(message2CurrentBranchTrx.PK);
			var message2CurrentBranchTrxInOtherFactoryResponse = message2CurrentBranchTrxInOtherFactory.ResponseMessage;
			AssertEquals("Should match to current company response message 2", message2CurrentBranchRcv.PK, message2CurrentBranchTrxInOtherFactoryResponse.PK);
			AssertEquals(message2CurrentBranchTrxInOtherFactory, message2CurrentBranchTrxInOtherFactoryResponse.OriginalMessage);
		}

		public void TestSetDefaultValues()
		{
			var message = (AESTIREDIMessage)GetNewBusinessObject();
			AssertEquals("EM_ApplicationCode", AESTIREDIMessage.ApplicationCodes.USCustomsExport, message.EM_ApplicationCode);
		}

		public void TestMessageNumber()
		{
			var message = (AESTIREDIMessage)GetNewBusinessObject();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var expectedNumber = Env.NumberFountains.EDIFACTNumberFountain("M", GlbCompany.CurrentCompany.GC_Code, ApplicationIdentifierCodeList.AES.CommodityShipment).PeekPreliminaryFormatted(Factory);
			AssertNotEquals(expectedNumber, message.EM_MessageNum);
			Factory.Save();
			AssertEquals(expectedNumber, message.EM_MessageNum);
		}

		public void TestIAESMessagePrintMembers()
		{
			var message = (AESTIREDIMessage)GetNewBusinessObject();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var iAESMessagePrintObject = (IAESMessagePrint)message;
			AssertEquals("Message Text", message.EM_FormattedMessageText, iAESMessagePrintObject.MessageText);
			AssertEquals("Message Type", message.EM_MessageType, iAESMessagePrintObject.MessageType);
			AssertEquals("Link Unique ID", message.EM_LinkUniqueID, iAESMessagePrintObject.LinkUniqueID);
			AssertEquals("Factory", message.Factory, iAESMessagePrintObject.Factory);
		}

		T CreateMessage<T>(ZGuid branchPK, ZString direction, ZString messageType, string messageNum = null)
			where T : EDIMessage
		{
			var message = Factory.New<T>();
			message.EM_GB = branchPK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			message.EM_ReceiveTransmit = direction;
			message.EM_MessageType = messageType;
			message.EM_MessageNum = messageNum;
			return message;
		}
	}
}
