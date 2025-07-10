using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class ACEAMSMessageBuilderTest : TestCaseWithFactory
	{
		public void TestMessageBranchShouldBeBasedOnAMSBranch()
		{
			var company1 = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MainAddress.OA_Address1 = "address 1";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_OH_OrgProxy = org2.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_OH_OrgProxy = org2.PK;
			company2.Branches.Add(branch2);
			var messages = new AMSEDIMessageCollection(Factory.New<DummyBusinessObject>());
			var manifestMock1 = AMSInterfaceTestHelper.GetManifestForACE(false);
			manifestMock1.Setup(m => m.Factory).Returns(Factory);
			manifestMock1.Setup(m => m.Messages).Returns(messages);
			manifestMock1.Setup(m => m.Branch).Returns((GlbBranch)null);
			var builder = new ACEAMSMessageBuilder(manifestMock1.Object, ActionCode.Creating);
			var message1 = builder.PopulateMessage();
			var manifestMock2 = AMSInterfaceTestHelper.GetManifestForACE(false);
			manifestMock2.Setup(m => m.Factory).Returns(Factory);
			manifestMock2.Setup(m => m.Messages).Returns(messages);
			manifestMock2.Setup(m => m.Branch).Returns(branch2);
			var builder2 = new ACEAMSMessageBuilder(manifestMock2.Object, ActionCode.Creating);
			var message2 = builder2.PopulateMessage();
			AssertEquals("Should be the current logged in branch", GlbBranch.CurrentBranch.PK, message1.EM_GB);
			AssertEquals("Should be the AMS Branch since it's specified", branch2.PK, message2.EM_GB);

			manifestMock1.VerifyAll();
			manifestMock2.VerifyAll();
		}

		public void TestPopulateMessage()
		{
			var billPK = "0A60F818-CB95-4606-BC95-3EBD3745801C";
			var dummy = Factory.New<DummyBusinessObject>();
			var messages = new AMSEDIMessageCollection(dummy);
			var billMessages = new AMSBillEDIMessageCollection(dummy.Factory, billPK, null);
			var billMessageCount = billMessages.Count;
			var manifestMock = AMSInterfaceTestHelper.GetManifestForACE(true);
			manifestMock.Setup(m => m.Factory).Returns(Factory);
			manifestMock.Setup(m => m.Messages).Returns(messages);
			manifestMock.Setup(m => m.BillPKAsString).Returns(billPK);
			manifestMock.Setup(m => m.BillMessages).Returns(billMessages);
			var builder = new ACEAMSMessageBuilder(manifestMock.Object, ActionCode.Creating);
			var message = builder.PopulateMessage();
			AssertEquals(AMSEDIMessage.ApplicationCodes.AMS, message.EM_ApplicationCode);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, message.EM_MessageType);
			AssertEquals(AMSMessageSubTypeList.Codes.Creating, message.EM_MessageSubType);
			AssertCollectionContains(message, messages);
			AssertCollectionContains(message, billMessages);
			AssertEquals(billMessageCount + 1, billMessages.Count);
			AssertNotEquals("", message.EM_FormattedMessageText);
			AssertNotEquals("", message.EM_MessageInterpretation);
			AssertEquals(message.EM_ApplicationReference, billPK);
			builder = new ACEAMSMessageBuilder(manifestMock.Object, ActionCode.AmendingAdd);
			message = builder.PopulateMessage();
			AssertEquals(AMSEDIMessage.ApplicationCodes.AMS, message.EM_ApplicationCode);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, message.EM_MessageType);
			AssertEquals(AMSMessageSubTypeList.Codes.AmendingAdd, message.EM_MessageSubType);
			AssertCollectionContains(message, messages);
			AssertNotEquals("", message.EM_FormattedMessageText);
			AssertNotEquals("", message.EM_MessageInterpretation);
			AssertEquals(message.EM_ApplicationReference, billPK);
			builder = new ACEAMSMessageBuilder(manifestMock.Object, ActionCode.Equipment);
			message = builder.PopulateMessage();
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.EquipmentInventory, message.EM_MessageType);
			AssertEquals(AMSMessageSubTypeList.Codes.Equipment, message.EM_MessageSubType);
			AssertEquals(message.EM_ApplicationReference, billPK);
			builder = new ACEAMSMessageBuilder(manifestMock.Object, ActionCode.GeneralOrderStatus);
			message = builder.PopulateMessage();
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.GeneralOrderStatus, message.EM_MessageType);
			AssertEquals(AMSMessageSubTypeList.Codes.GeneralOrderStatus, message.EM_MessageSubType);
			AssertEquals(message.EM_ApplicationReference, billPK);
			builder = new ACEAMSMessageBuilder(manifestMock.Object, ActionCode.InBondArrival);
			message = builder.PopulateMessage();
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, message.EM_MessageType);
			AssertEquals(AMSMessageSubTypeList.Codes.InBondArrival, message.EM_MessageSubType);
			AssertEquals(message.EM_ApplicationReference, billPK);

			manifestMock.VerifyAll();
		}

		public void TestPopulatePTTMessage()
		{
			var messages = new AMSEDIMessageCollection(Factory.New<DummyBusinessObject>());

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");
			billMock1.Setup(m => m.ManifestQuantity).Returns(245m);
			billMock1.Setup(m => m.FIRMS).Returns("Z456");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.InBondQuantity).Returns(245);
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			manifestMock.Setup(m => m.Factory).Returns(Factory);
			manifestMock.Setup(m => m.Messages).Returns(messages);
			var builder = new ACEAMSMessageBuilder(manifestMock.Object, ActionCode.PermitToTransfer);
			var message = builder.PopulateMessage();
			AssertEquals(AMSEDIMessage.ApplicationCodes.AMS, message.EM_ApplicationCode);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, message.EM_MessageType);
			AssertEquals(AMSMessageSubTypeList.Codes.PermitToTransfer, message.EM_MessageSubType);

			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestPopulateCancelPTTMessage()
		{
			var messages = new AMSEDIMessageCollection(Factory.New<DummyBusinessObject>());

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.BillActionCode).Returns(AMSBillSendingActionCodeList.Codes.AddBill);
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.TOLInBondCarrierCode).Returns("TOL1");
			inBOndMock.Setup(m => m.TOLBondedCarrierID).Returns("TOL-INBCARID");
			inBOndMock.Setup(m => m.TOLDateTime).Returns(new ZDateTime(2012, 7, 9, 10, 22, 8));
			inBOndMock.Setup(m => m.TOLCityName).Returns("BOB'S CITY");
			inBOndMock.Setup(m => m.TOLStateCode).Returns("CA");
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			manifestMock.Setup(m => m.Factory).Returns(Factory);
			manifestMock.Setup(m => m.Messages).Returns(messages);
			manifestMock.Setup(m => m.ModeOfTransportationCode).Returns(new ZString("S"));
			var builder = new ACEAMSMessageBuilder(manifestMock.Object, ActionCode.CancelPermitToTransfer);
			var message = builder.PopulateMessage();
			AssertEquals(AMSEDIMessage.ApplicationCodes.AMS, message.EM_ApplicationCode);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, message.EM_MessageType);
			AssertEquals(AMSMessageSubTypeList.Codes.CancelPermitToTransfer, message.EM_MessageSubType);

			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestPopulateInBondArrivalMessage()
		{
			var messages = new AMSEDIMessageCollection(Factory.New<DummyBusinessObject>());

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.BillActionCode).Returns(InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability);
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.TOLInBondCarrierCode).Returns("TOL1");
			inBOndMock.Setup(m => m.TOLBondedCarrierID).Returns("TOL-INBCARID");
			inBOndMock.Setup(m => m.TOLDateTime).Returns(new ZDateTime(2012, 7, 9, 10, 22, 8));
			inBOndMock.Setup(m => m.TOLCityName).Returns("BOB'S CITY");
			inBOndMock.Setup(m => m.TOLStateCode).Returns("CA");
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			manifestMock.Setup(m => m.Factory).Returns(Factory);
			manifestMock.Setup(m => m.Messages).Returns(messages);
			var builder = new ACEAMSMessageBuilder(manifestMock.Object, ActionCode.InBondTransferOfLiability);
			var message = builder.PopulateMessage();
			AssertEquals(AMSEDIMessage.ApplicationCodes.AMS, message.EM_ApplicationCode);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, message.EM_MessageType);
			AssertEquals(AMSMessageSubTypeList.Codes.InBondTransferOfLiability, message.EM_MessageSubType);
			AssertMultilineASCIIEquals("InBondArrival Message", @"ACR          HI                                                                 
M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
P015946091871                                                                   
H01AVINB53264     120709        1022  TOL1TOL-INBCARIDBOB'S CITY         CA     
ZCR                               00000", message.EM_FormattedMessageText);

			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}
	}
}
