using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	class SendDTRFromContainerTest : TestCaseWithFactory
	{
		public void TestSendOriginalMessage_NoData()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var type = TSWTransactionTypes.Original;
				var oceanBill = Factory.New<CusSCAOceanBill>();
				var container = oceanBill.Containers.AddNew();
				var dtr1 = container.ContainerDTRs.AddNew();
				Factory.Save();
				var messageType = MessageTypeList.Codes.ICR;
				var lastICRMessage = oceanBill.Messages.GetLastMessage(EDIMessage.ApplicationCodes.NewZealandCustoms, messageType, EDIMessage.Direction.Transmit);
				var eDocsForSelection = new[] { oceanBill.DocManagerInfo.AllEDocs };
				var additionalMessageInformation = new AdditionalMessageInformation((TSWMessage)lastICRMessage, eDocsForSelection, type, Factory, messageType);
				var sender = new SendDTRFromContainer(dtr1, additionalMessageInformation, type);
				sender.SendMessage();
				var message = (EDIMessage)dtr1.Messages?.Single();
				AssertNotNull(message);
			}
		}

		public void TestSendOriginalMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var declarant = GlbStaff.CurrentUser;
				declarant.GetNZWrapper().NZBPassword.GP_UserID = "40006206E";
				declarant.GS_EmailAddress = "test.user@company.org";
				var destinationAddress = GetAddress();
				var sendingAgentAddress = GetAddress();
				var messageType = MessageTypeList.Codes.ICR;
				var type = TSWTransactionTypes.Original;
				var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
				oceanBill.CB_RL_NKPortOfDischarge = "NZ";
				oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
				oceanBill.CB_OceanBill = "12345";
				var container = oceanBill.Containers.AddNew();
				container.CN_ContainerNumber = "MSKU0049385";
				container.CN_ContainerMode = "FCL";
				container.CN_ContainerSizeOrISOCode = "23";
				container.CN_SealNumber = "Z-52988";
				var dtr1 = container.ContainerDTRs.AddNew();
				dtr1.C4_OA_DestinationAddress = destinationAddress.PK;
				dtr1.C4_SendersMessageReference = "12345";
				oceanBill.SendingAgentAddress.E2_OA_Address = sendingAgentAddress.PK;
				var packingLine1 = container.PackingLines.AddNew();
				packingLine1.CV_PackageCount = 10;
				packingLine1.CV_PackageType = "DN";
				packingLine1.CV_Weight = 20;
				packingLine1.CV_WeightUQ = Core.Constants.Weight.Kilograms;
				var packingLine2 = container.PackingLines.AddNew();
				packingLine2.CV_PackageCount = 20;
				packingLine2.CV_PackageType = "DN";
				packingLine2.CV_Weight = 300;
				packingLine2.CV_WeightUQ = Core.Constants.Weight.Grams;
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var address = header.Addresses.AddNew();
				address.Address1 = "Address";
				dtr1.C4_OA_OriginAddress = address.PK;
				address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "4331");
				Factory.Save();
				var lastICRMessage = oceanBill.Messages.GetLastMessage(EDIMessage.ApplicationCodes.NewZealandCustoms, messageType, EDIMessage.Direction.Transmit);
				var eDocsForSelection = new[] { oceanBill.DocManagerInfo.AllEDocs };
				var additionalMessageInformation = new AdditionalMessageInformation((TSWMessage)lastICRMessage, eDocsForSelection, type, Factory, messageType);
				var sender = new SendDTRFromContainer(dtr1, additionalMessageInformation, type);
				sender.SendMessage();
				var message = (EDIMessage)dtr1.Messages?.Single();
				AssertNotNull(message);
				var messageTextNoWhitespace = new string(message.EM_MessageText.ToString().Where(c => !char.IsWhiteSpace(c)).ToArray());
				var expectedMessageNoWhitespace = new string(ExpectedOriginalMessage.Where(c => !char.IsWhiteSpace(c)).ToArray());
				AssertEquals("Message content", expectedMessageNoWhitespace, messageTextNoWhitespace);
			}
		}

		public void TestSendingAgentMessageError()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var sendingAgentMessageError = "Sending Agent: Sending Agent is required for Container DTRs.";
				var emptyModeErrorMessage = "When this is ticked, the container mode can only choose EMP.";
				var vesselNameMessageError = "The code you have selected is not in the list.";

				var oceanBill = Factory.New<CusSCAOceanBill>();
				oceanBill.CB_RL_NKPortOfLoading = "SGSIN";
				oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
				oceanBill.IsEmptyContainerMode = true;
				oceanBill.ZX_AccountHolder = "";
				oceanBill.CB_VesselName = "NOVESSEL";
				var container = oceanBill.Containers.AddNew();
				container.CN_ContainerMode = ContainerModeList.Codes.Bulk;
				var request = container.ContainerDTRs.AddNew();
				var icrSender = new SendDTRFromContainer(request, null, TSWTransactionTypes.Original);
				AssertContains(sendingAgentMessageError, icrSender.Errors);
				AssertContains(emptyModeErrorMessage, icrSender.Errors);
				AssertContains(vesselNameMessageError, icrSender.Errors);
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
			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfDischarge = "NZ";
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_OceanBill = "12345";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "MSKU0049385";
			container.CN_ContainerMode = "FCL";
			container.CN_ContainerSizeOrISOCode = "23";
			container.CN_SealNumber = "Z-52988";
			var dtr1 = container.ContainerDTRs.AddNew();
			dtr1.C4_OA_DestinationAddress = destinationAddress.PK;
			dtr1.C4_SendersMessageReference = "12345";
			oceanBill.SendingAgentAddress.E2_OA_Address = sendingAgentAddress.PK;
			var packingLine1 = container.PackingLines.AddNew();
			packingLine1.CV_PackageCount = 10;
			packingLine1.CV_PackageType = "DN";
			packingLine1.CV_Weight = 20;
			packingLine1.CV_WeightUQ = Core.Constants.Weight.Kilograms;
			var packingLine2 = container.PackingLines.AddNew();
			packingLine2.CV_PackageCount = 20;
			packingLine2.CV_PackageType = "DN";
			packingLine2.CV_Weight = 300;
			packingLine2.CV_WeightUQ = Core.Constants.Weight.Grams;
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.Addresses.AddNew();
			address.Address1 = "Address";
			dtr1.C4_OA_OriginAddress = address.PK;
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "4331");
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var lastICRMessage = oceanBill.Messages.GetLastMessage(EDIMessage.ApplicationCodes.NewZealandCustoms, messageType, EDIMessage.Direction.Transmit);
				var eDocsForSelection = new[] { oceanBill.DocManagerInfo.AllEDocs };
				var additionalMessageInformation = new AdditionalMessageInformation((TSWMessage)lastICRMessage, eDocsForSelection, type, Factory, messageType);
				var dtrSender = new SendDTRFromContainer(dtr1, additionalMessageInformation, type);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => dtrSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals("C4_Status", "", dtr1.C4_Status);
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

		const string ExpectedOriginalMessage = @"<?xmlversion=""1.0""encoding=""utf-8""?>
<DocumentMetadata
    xmlns=""urn:wco:datamodel:WCO:DM:1"">
    <WCODataModelVersion>3.2</WCODataModelVersion>
    <WCODocumentName>CRI</WCODocumentName>
    <CountryCode>NZ</CountryCode>
    <AgencyAssignedCustomizedDocumentName>ICR</AgencyAssignedCustomizedDocumentName>
    <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
    <Declaration
        xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
        <TypeCode>ICR</TypeCode>
        <FunctionalReferenceID>12345</FunctionalReferenceID>
        <FunctionCode>9</FunctionCode>
        <Submitter>
            <ID>00009917B</ID>
        </Submitter>
        <BorderTransportMeans>
            <Name/>
            <ID/>
            <TypeCode>1</TypeCode>
            <ArrivalDateTimeformatCode=""102""/>
            <FirstArrivalLocationID>NZ</FirstArrivalLocationID>
            <JourneyID/>
        </BorderTransportMeans>
        <Consignment>
            <SequenceNumeric>1</SequenceNumeric>
            <AdditionalInformation>
                <StatementCode>Y</StatementCode>
                <StatementTypeCode>CON</StatementTypeCode>
            </AdditionalInformation>
            <Consignee>
                <Name>EagleDatamationInternational</Name>
                <Address>
                    <CityName>Alexandria</CityName>
                    <CountryCode>NZ</CountryCode>
                    <CountrySubDivisionName/>
                    <Line>184BourkeRoad</Line>
                    <PostcodeID>2015</PostcodeID>
                </Address>
            </Consignee>
            <ConsignmentItem>
                <SequenceNumeric>1</SequenceNumeric>
                <Commodity>
                    <CargoDescription>CONSOL</CargoDescription>
                </Commodity>
                <GoodsMeasure>
                    <GrossMassMeasureunitCode=""KGM"">20.3
                    </GrossMassMeasure>
                </GoodsMeasure>
                <Packaging>
                    <SequenceNumeric>1</SequenceNumeric>
                    <QuantityQuantity>30</QuantityQuantity>
                    <TypeCode>DN</TypeCode>
                </Packaging>
                <TransportEquipment>
                    <ID>MSKU0049385</ID>
                </TransportEquipment>
            </ConsignmentItem>
            <Consignor>
                <Name>Header</Name>
                <Address>
                    <CityName>Sydney</CityName>
                    <CountryCode>AU</CountryCode>
                    <Line>1RoadABC</Line>
                    <PostcodeID>2000</PostcodeID>
                </Address>
            </Consignor>
            <Freight>
                <PaymentMethodCode/>
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
                <ID>12345</ID>
                <TypeCode>MB</TypeCode>
            </TransportContractDocument>
            <TransportEquipment>
                <SequenceNumeric>1</SequenceNumeric>
                <CharacteristicCode/>
                <FullnessCode>5</FullnessCode>
                <ID>MSKU0049385</ID>
                <Seal>
                    <SequenceNumeric>1</SequenceNumeric>
                    <ID>Z-52988</ID>
                </Seal>
			</TransportEquipment>
            <UnloadingLocation>
                <ID>NZ</ID>
                <ArrivalDateTimeformatCode=""203""/>
            </UnloadingLocation>
        </Consignment>
    </Declaration>
</DocumentMetadata>";
	}
}
