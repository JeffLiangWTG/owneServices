using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class USISFWebMessageSenderTest : TestCaseWithFactory
	{
		public void TestUpsertNoHeader()
		{
			var errorMessage = messageSender.SendUpsertMessage(ZGuid.NewZGuid());
			AssertEquals("Header was not found", errorMessage);
		}

		public void TestDeleteNoHeader()
		{
			var errorMessage = messageSender.SendDeleteMessage(ZGuid.NewZGuid());
			AssertEquals("Header was not found", errorMessage);
		}

		public void TestUpsertNoMessage()
		{
			var header = GetValidHeader();
			Factory.Save();
			mockBuilder.Setup(x => x.PopulateMessage()).Returns<MQEDIMessage>(null);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Add)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendUpsertMessage(header.PK);
			AssertEquals("Message was not created", errorMessage);
		}

		public void TestDeleteNoMessage()
		{
			var header = GetValidHeader();
			header.BF_CustomsReference = "Reference";
			Factory.Save();
			Assert("Precondition", header.CanSendDelete);
			mockBuilder.Setup(x => x.PopulateMessage()).Returns<MQEDIMessage>(null);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Delete)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendDeleteMessage(header.PK);
			AssertEquals("Message was not created", errorMessage);
		}

		public void TestUpsertSuccess()
		{
			var header = GetValidHeader();
			header.BF_CustomsReference = ZString.Empty;
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			message.EM_SendWithMessageErrors = true;
			Factory.Save();
			Assert("Precondition", header.ShouldSendAdd);
			mockBuilder.Setup(x => x.PopulateMessage()).Returns(message);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Add)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendUpsertMessage(header.PK);
			AssertNull(errorMessage);
			Assert(!message.EM_SendWithMessageErrors);
		}

		public void TestUpsertSuccess_Replace()
		{
			var header = GetValidHeader();
			header.BF_CustomsReference = "Reference";
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			message.EM_SendWithMessageErrors = true;
			Factory.Save();
			Assert("Precondition", !header.ShouldSendAdd);
			mockBuilder.Setup(x => x.PopulateMessage()).Returns(message);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Replace)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendUpsertMessage(header.PK);
			Assert(!message.EM_SendWithMessageErrors);
		}

		public void TestUpsert_ValidateHeader()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsReference = "Reference";
			Factory.Save();
			Assert("Precondition", header.CanSendDelete);
			mockBuilder.Setup(x => x.PopulateMessage()).Returns<MQEDIMessage>(null);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Delete)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendUpsertMessage(header.PK);
			AssertStartsWith("Returns validation errors", "Please fix the following errors before continuing: \r\n ", errorMessage);
		}

		public void TestDeleteSuccess()
		{
			var header = GetValidHeader();
			header.BF_CustomsReference = "Reference";
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			message.EM_SendWithMessageErrors = true;
			Factory.Save();
			Assert("Precondition", header.CanSendDelete);
			mockBuilder.Setup(x => x.PopulateMessage()).Returns(message);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Delete)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendDeleteMessage(header.PK);
			AssertNull(errorMessage);
			Assert(!message.EM_SendWithMessageErrors);
		}

		public void TestDelete_AlreadyDeleted()
		{
			var header = GetValidHeader();
			header.BF_CustomsReference = "Reference";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;
			Factory.Save();
			mockBuilder.Setup(x => x.PopulateMessage()).Returns<MQEDIMessage>(null);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Delete)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendDeleteMessage(header.PK);
			AssertEquals("Cannot send further messages as this Customs Reference 'Reference' has been deleted from Customs system", errorMessage);
		}

		public void TestDelete_CantDelete()
		{
			var header = GetValidHeader();
			header.BF_CustomsReference = ZString.Empty;
			Factory.Save();
			Assert("Precondition", !header.CanSendDelete);
			mockBuilder.Setup(x => x.PopulateMessage()).Returns<MQEDIMessage>(null);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Delete)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendDeleteMessage(header.PK);
			AssertEquals("Cannot send 'Delete' message as message hasn't been cleared yet.", errorMessage);
		}

		public void TestZSaveException()
		{
			var header = GetValidHeader();
			header.BF_CustomsReference = ZString.Empty;
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			Factory.Save();
			var listenerMock = new Mock<ITransactionParticipantListener>();
			listenerMock.Setup(x => x.FactorySaveBeginning(It.IsAny<ITransactionParticipant[]>())).Throws(new ZSaveException(new ZDataException(new Exception("ZSaveException message"), null, null), Factory));
			BusinessObjectFactory.RegisterListener(listenerMock.Object);
			mockBuilder.Setup(x => x.PopulateMessage()).Returns(message);
			mockFactory.Setup(x => x.CreateWebMessageBuilder(It.Is<CusISFHeader>(y => y.PK == header.PK), UpdateActionCode.Add)).Returns(mockBuilder.Object);
			var errorMessage = messageSender.SendUpsertMessage(header.PK);
			BusinessObjectFactory.UnRegisterListener(listenerMock.Object);
			AssertEquals("ZSaveException message", errorMessage);
		}

		CusISFHeader GetValidHeader()
		{
			var header = Factory.NewWithValidTestData<CusISFHeader>();
			header.BF_JobReference = "ISF2342343";
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			header.BF_HouseBill = "SCACHB2142";
			var buyingParty = GetValidOrgHeader();
			header.BuyingParty.OrganisationPK = buyingParty.PK;
			var sellingParty = GetValidOrgHeader();
			header.SellingParty.OrganisationPK = sellingParty.PK;
			var stuffing = GetValidOrgHeader();
			header.StuffingLocation.OrganisationPK = stuffing.PK;
			var consolidator = GetValidOrgHeader();
			header.Consolidator.OrganisationPK = consolidator.PK;
			var shipToParty = GetValidOrgHeader();
			header.MainShipToParty.OrganisationPK = shipToParty.PK;
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
			header.BF_BondType = Enterprise.Customs.US.Business.ImporterBondTypeList.Codes.SingleTransactionBond;
			header.BF_SuretyCode = "791";
			header.BF_BondReferenceNumber = "BD323423";
			header.BF_ImporterCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_ImporterCode = "12-3456789XY";
			header.BF_ConsigneeCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_ConsigneeCode = "12-3456789XY";
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Six;
			header.BF_BondNumberOrHolder = "123-12-1234";
			var line = header.Lines.AddNew();
			var manufacturer = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			line.BL_HarmonisedNum = "10.10.8120";
			line.BL_ManufacturerDocAddressPK = manufacturer.PK;
			line.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line.CustomAttribute1 = "LNEATTRIB1";
			line.CustomAttribute2 = "LNEATTRIB2";
			return header;
		}

		OrgHeader GetValidOrgHeader()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTBUYING";
			org.OH_FullName = "CARGOWISE";
			org.MainAddress.FillWithValidTestData();
			org.MainAddress.OA_City = "SYDNEY";
			org.OH_RL_NKClosestPort = "AUSYD";
			return org;
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockFactory = new Mock<IISFMessageBuilderFactory>();
			mockBuilder = new Mock<IMessageBuilder<MQEDIMessage>>();
			messageSender = new USISFWebMessageSender(mockFactory.Object);
		}

		Mock<IISFMessageBuilderFactory> mockFactory;
		Mock<IMessageBuilder<MQEDIMessage>> mockBuilder;
		USISFWebMessageSender messageSender;
	}
}
