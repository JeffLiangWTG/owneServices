using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.TradeSingleWindow.InwardCargoReport;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	class SendDTRFromMAWBTest : TestCaseWithFactory
	{
		public void TestSendOriginalMessage_NoData()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var type = TSWTransactionTypes.Original;
			var mawb = Factory.New<CusMAWB>();
			var dtr1 = mawb.MasterDTRs.AddNew();
			Factory.Save();
			var messageType = MessageTypeList.Codes.ICR;
			var lastICRMessage = mawb.GetLastSentTSWMessage(messageType);
			var eDocsForSelection = new[] { mawb.DocManagerInfo.AllEDocs };
			var additionalMessageInformation = new AdditionalMessageInformation(lastICRMessage, eDocsForSelection, type, Factory, messageType);
			var sender = new SendDTRFromMAWB(dtr1, additionalMessageInformation, type);
			sender.SendMessage();
			var message = (EDIMessage)dtr1.Messages.Single();
			AssertNotNull(message);
		}

		public void TestSendOriginalMessage()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var declarant = GlbStaff.CurrentUser;
			declarant.GetNZWrapper().NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			var destinationAddress = GetAddress();
			var sendingAgentAddress = GetAddress();
			var messageType = MessageTypeList.Codes.ICR;
			var type = TSWTransactionTypes.Original;
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_CustomsStatus = LowValueManifestStatusList.Codes.Acknowledgement;
			mawb.CM_RL_NKDischargePort = "NZ";
			mawb.CM_MAWB = "12345678";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			var dtr1 = mawb.MasterDTRs.AddNew();
			dtr1.C4_OA_DestinationAddress = destinationAddress.PK;
			dtr1.C4_SendersMessageReference = "12345";
			mawb.SendingAgentAddress.E2_OA_Address = sendingAgentAddress.PK;
			var bill1 = mawb.ChildBills.AddNew();
			bill1.CS_PiecesManifested = 10;
			bill1.CS_PackType = "DN";
			bill1.CS_Weight = 20;
			bill1.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			var bill2 = mawb.ChildBills.AddNew();
			bill2.CS_PiecesManifested = 20;
			bill2.CS_PackType = "DN";
			bill2.CS_Weight = 300;
			bill2.CS_WeightUQ = Core.Constants.Weight.Grams;
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.Addresses.AddNew();
			address.Address1 = "Address";
			dtr1.C4_OA_OriginAddress = address.PK;
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "4331");
			Factory.Save();
			var lastICRMessage = mawb.GetLastSentTSWMessage(messageType);
			var eDocsForSelection = new[] { mawb.DocManagerInfo.AllEDocs };
			var additionalMessageInformation = new AdditionalMessageInformation(lastICRMessage, eDocsForSelection, type, Factory, messageType);
			var sender = new SendDTRFromMAWB(dtr1, additionalMessageInformation, type);
			sender.SendMessage();
			var message = (EDIMessage)dtr1.Messages.Single();
			AssertNotNull(message);

			AssertEquals(CombinedMovementStatus.Codes.STC, dtr1.C4_Status);
			AssertEquals(LowValueManifestStatusList.Codes.Acknowledgement, mawb.CM_CustomsStatus);

			var messageTextNoWhitespace = new string(message.EM_MessageText.ToString().Where(c => !char.IsWhiteSpace(c)).ToArray());
			var expectedMessageNoWhitespace = new string(ExpectedOriginalMessage.Where(c => !char.IsWhiteSpace(c)).ToArray());
			AssertEquals("Message content", messageTextNoWhitespace, expectedMessageNoWhitespace);
		}

		public void TestSendingAgentMessageError()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var sendingAgentMessageError = "Sending Agent: Sending Agent is required for Master DTRs.";
				var arrivalDateMessageError = "Please enter an Arrival Date. (required for Import ECI)";

				var mawb = Factory.New<CusMAWB>();
				mawb.CM_RL_NKLoadPort = "AUSYD";
				mawb.CM_RL_NKDischargePort = "NZAKL";
				var request = mawb.MasterDTRs.AddNew();
				var icrSender = new SendDTRFromMAWB(request, null, TSWTransactionTypes.Original);
				AssertContains(sendingAgentMessageError, icrSender.Errors);
				AssertContains(arrivalDateMessageError, icrSender.Errors);
			}
		}

		public void TestStatusTransactionScopeRollback()
		{
			var declarant = GlbStaff.CurrentUser;
			declarant.GetNZWrapper().NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			var destinationAddress = GetAddress();
			var sendingAgentAddress = GetAddress();
			var messageType = MessageTypeList.Codes.ICR;
			var type = TSWTransactionTypes.Original;
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_CustomsStatus = LowValueManifestStatusList.Codes.Acknowledgement;
			mawb.CM_RL_NKDischargePort = "NZ";
			mawb.CM_MAWB = "12345678";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			var dtr1 = mawb.MasterDTRs.AddNew();
			dtr1.C4_OA_DestinationAddress = destinationAddress.PK;
			dtr1.C4_SendersMessageReference = "12345";
			mawb.SendingAgentAddress.E2_OA_Address = sendingAgentAddress.PK;
			var bill1 = mawb.ChildBills.AddNew();
			bill1.CS_PiecesManifested = 10;
			bill1.CS_PackType = "DN";
			bill1.CS_Weight = 20;
			bill1.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			var bill2 = mawb.ChildBills.AddNew();
			bill2.CS_PiecesManifested = 20;
			bill2.CS_PackType = "DN";
			bill2.CS_Weight = 300;
			bill2.CS_WeightUQ = Core.Constants.Weight.Grams;
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.Addresses.AddNew();
			address.Address1 = "Address";
			dtr1.C4_OA_OriginAddress = address.PK;
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "4331");
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var lastICRMessage = mawb.GetLastSentTSWMessage(messageType);
				var eDocsForSelection = new[] { mawb.DocManagerInfo.AllEDocs };
				var additionalMessageInformation = new AdditionalMessageInformation(lastICRMessage, eDocsForSelection, type, Factory, messageType);
				var dtrSender = new SendDTRFromMAWB(dtr1, additionalMessageInformation, type);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => dtrSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals("C4_Status", "", dtr1.C4_Status);
					AssertEquals("CM_CustomsStatus", LowValueManifestStatusList.Codes.Acknowledgement, mawb.CM_CustomsStatus);
					AssertEquals("Messages.Count", 0, dtr1.Messages.Count);
				});
			}
		}

		OrgAddress GetAddress()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.City = "Sydney";
			address.OA_RN_NKCountryCode = "AU";
			address.State = "NSW";
			address.Address1 = "1 Road";
			address.Address2 = "ABC";
			address.Postcode = "2000";
			address.OA_Phone_Formatted = "0493123456";
			return address;
		}

		const string ExpectedOriginalMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRI</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>ICR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>ICR</TypeCode>
  <FunctionalReferenceID>12345</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <Submitter>
    <ID>00009917B</ID>
  </Submitter>
  <BorderTransportMeans>
    <Name />
    <TypeCode>4</TypeCode>
    <ArrivalDateTime formatCode=""102"" />
    <FirstArrivalLocationID>NZ</FirstArrivalLocationID>
  </BorderTransportMeans>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>CON</StatementTypeCode>
    </AdditionalInformation>
    <Consignee>
      <Name>Header</Name>
      <Address>
        <CityName>Sydney</CityName>
        <CountryCode>AU</CountryCode>
        <CountrySubDivisionName>New South Wales</CountrySubDivisionName>
        <Line>1 Road ABC</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>CONSOL</CargoDescription>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">20.3</GrossMassMeasure>
      </GoodsMeasure>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>30</QuantityQuantity>
        <TypeCode>DN</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>Header</Name>
      <Address>
        <CityName>Sydney</CityName>
        <CountryCode>AU</CountryCode>
        <Line>1 Road ABC</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID>AUSYD</ID>
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>4331</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>AUSYD</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID>12345678</ID>
      <TypeCode>MB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZ</ID>
      <ArrivalDateTime formatCode=""203"" />
    </UnloadingLocation>
  </Consignment>
</Declaration>
</DocumentMetadata>";
	}
}
