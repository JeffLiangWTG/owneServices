using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	sealed class CusEntryHeaderWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new CusEntryHeaderWrapper(null, null);
		}

		[TestDate(2013, 08, 01)]
		public void TestIImportDeclaration()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			entryHeader.EntryNumber = "97345213";

			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedImportHeader = (IImportDeclaration)wrapper;
			AssertNotNull("AdditionalInformation", wrappedImportHeader.AdditionalInformation);
			AssertEquals("SubmitterCode - should be padded to 9 characters", "00009908C", wrappedImportHeader.SubmitterCode);
			AssertEquals("BrokerCode - should be padded to 9 characters", "00009908C", wrappedImportHeader.BrokerCode);
			AssertNotNull("Carrier", wrappedImportHeader.Carrier);
			AssertEquals("CraftName", "HYOGO MARU", wrappedImportHeader.CraftName);
			AssertEquals("DateOfImport", new ZDateTime(2013, 08, 01), wrappedImportHeader.DateOfImport);
			AssertNotNull("Declarant", wrappedImportHeader.Declarant);
			AssertEquals("DepartureDate", new ZDateTime(2013, 07, 18), wrappedImportHeader.DepartureDate);
			AssertNotNull("DutyTaxFees", wrappedImportHeader.DutyTaxFees);
			AssertNotNull("ExchangeRates", wrappedImportHeader.ExchangeRates);
			AssertNotNull("GoodsShipment", wrappedImportHeader.GoodsShipment);
			AssertEquals("HandlingInformation", "", wrappedImportHeader.HandlingInformation);
			AssertEquals("HasContainersOrPallets", true, wrappedImportHeader.HasContainersOrPallets);
			AssertNotNull("Importer", wrappedImportHeader.Importer);
			AssertEquals("ImportPeriod", ZDateTime.Empty, wrappedImportHeader.ImportPeriod);
			AssertEquals("IsAir", false, wrappedImportHeader.IsAir);
			AssertEquals("IsCompletionEntry", false, wrappedImportHeader.IsCompletionEntry);
			AssertEquals("IsContainerised", true, wrappedImportHeader.IsContainerised);
			AssertEquals("IsMail", false, wrappedImportHeader.IsMail);
			AssertEquals("IsPeriodicImport", false, wrappedImportHeader.IsPeriodicImport);
			AssertEquals("IsSea", true, wrappedImportHeader.IsSea);
			AssertEquals("LloydsNo", "", wrappedImportHeader.LloydsNo);
			AssertEquals("MessageType", "I10", wrappedImportHeader.MessageType);
			AssertEquals("MPIAccountDetails", "", wrappedImportHeader.MPIAccountDetails);
			AssertNotNull("OtherInfoCodes", wrappedImportHeader.OtherInfoCodes);
			AssertNotNull("Packaging", wrappedImportHeader.Packaging);
			AssertEquals("PaymentType", "B", wrappedImportHeader.PaymentType);
			AssertEquals("PremiseID", "", wrappedImportHeader.PremiseID);
			AssertEquals("PreviousDocumentNo", "", wrappedImportHeader.PreviousDocumentNo);
			AssertEquals("PreviousDocumentType", "", wrappedImportHeader.PreviousDocumentType);
			AssertEquals("SenderReferenceNumber", "BSIS00002309", wrappedImportHeader.SenderReferenceNumber);
			AssertEquals("TotalGrossWeightInKGM", 15000m, wrappedImportHeader.TotalGrossWeightInKGM);
			AssertEquals("TotalGrossWeightUnit", "KGM", wrappedImportHeader.TotalGrossWeightUnit);
			AssertEquals("TransactionType", 9, wrappedImportHeader.TransactionType);
			AssertEquals("TSWReferenceNumber", "97345213", wrappedImportHeader.TSWReferenceNumber);
			AssertEquals("VoyageNo", "227W", wrappedImportHeader.VoyageNo);
		}

		public void TestSubmitterCodeIsUpperCase()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40102601a");
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.PopulateCH_BGMReferenceIfNeeded();

			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedImportHeader = (IImportDeclaration)wrapper;
			AssertEquals("SubmitterCode - should be UpperCase", "40102601A", wrappedImportHeader.SubmitterCode);
		}

		public void TestCreditFees()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			CreateExportDrawbackJob();
			var entryHeader = (FormalEntry.CusEntryHeader)JobDeclaration.CustomsEntryHeaders[0];

			var header = entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedExportHeader = (IImportDeclaration)wrapper;
			AssertNotNull("DutyTaxFees", wrappedExportHeader.DutyTaxFees);

			bool alacCreditFound = false;
			bool accCreditFound = false;
			bool heraCreditFound = false;
			bool pfmlCreditFound = false;
			foreach (IDutyTaxFee fee in wrappedExportHeader.DutyTaxFees)
			{
				if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.AL)
				{
					alacCreditFound = true;
					AssertEquals("ALAC Credit", 100m, fee.Amount);
				}
				else if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.AC)
				{
					accCreditFound = true;
					AssertEquals("ACC credit", 200m, fee.Amount);
				}
				else if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.SL)
				{
					heraCreditFound = true;
					AssertEquals("HERA Credit", 300m, fee.Amount);
				}
				else if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.PF)
				{
					pfmlCreditFound = true;
					AssertEquals("PFML Credit", 400m, fee.Amount);
				}
			}

			AssertEquals("alacCreditFound", true, alacCreditFound);
			AssertEquals("accCreditFound", true, accCreditFound);
			AssertEquals("heraCreditFound", true, heraCreditFound);
			AssertEquals("pfmlCreditFound", true, pfmlCreditFound);
		}

		[TestDate(2013, 07, 23)]
		public void TestIExportDeclaration()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			CreateExportAirJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedExportHeader = (IExportDeclaration)wrapper;
			AssertNotNull("AdditionalInformation", wrappedExportHeader.AdditionalInformation);
			AssertEquals("SubmitterCode - should be padded to 9 characters", "00009908C", wrappedExportHeader.SubmitterCode);
			AssertEquals("BrokerCode - should be padded to 9 characters", "00009908C", wrappedExportHeader.BrokerCode);
			AssertNotNull("Carrier", wrappedExportHeader.Carrier);
			AssertEquals("CraftName", "", wrappedExportHeader.CraftName);
			AssertEquals("DateOfImport", new ZDateTime(2013, 07, 23), wrappedExportHeader.DateOfExport);
			AssertNotNull("Declarant", wrappedExportHeader.Declarant);
			AssertEquals("DepartureDate", new ZDateTime(2013, 07, 23), wrappedExportHeader.DepartureDate);
			AssertNotNull("DutyTaxFees", wrappedExportHeader.DutyTaxFees);
			AssertNotNull("ExchangeRates", wrappedExportHeader.ExchangeRates);
			AssertEquals("FlightNo", "QF108", wrappedExportHeader.FlightNo);
			AssertNotNull("GoodsShipment", wrappedExportHeader.GoodsShipment);
			AssertEquals("HandlingInformation", "", wrappedExportHeader.HandlingInformation);
			AssertEquals("HasContainersOrPallets", false, wrappedExportHeader.HasContainersOrPallets);
			AssertNotNull("Importer", wrappedExportHeader.Importer);
			AssertEquals("IsAir", true, wrappedExportHeader.IsAir);
			AssertEquals("IsContainerised", false, wrappedExportHeader.IsContainerised);
			AssertEquals("IsMail", false, wrappedExportHeader.IsMail);
			AssertEquals("IsSea", false, wrappedExportHeader.IsSea);
			AssertEquals("LloydsNo", "", wrappedExportHeader.LloydsNo);
			AssertEquals("MessageType", "E40", wrappedExportHeader.MessageType);
			AssertEquals("MPIAccountDetails", "", wrappedExportHeader.MPIAccountDetails);
			AssertNotNull("OtherInfoCodes", wrappedExportHeader.OtherInfoCodes);
			AssertNotNull("Packaging", wrappedExportHeader.Packaging);
			AssertEquals("PremiseID", "", wrappedExportHeader.PremiseID);
			AssertEquals("SenderReferenceNumber", "BEX0042709", wrappedExportHeader.SenderReferenceNumber);
			AssertEquals("TotalGrossWeightInKGM", 15m, wrappedExportHeader.TotalGrossWeightInKGM);
			AssertEquals("TotalGrossWeightUnit", "KGM", wrappedExportHeader.TotalGrossWeightUnit);
			AssertEquals("TransactionType", 9, wrappedExportHeader.TransactionType);
			AssertEquals("TSWReferenceNumber", "", wrappedExportHeader.TSWReferenceNumber);
		}

		[TestDate(2013, 08, 01)]
		public void TestIGoodsShipmentImport()
		{
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedImportHeader = (IGoodsShipment)wrapper;
			AssertNotNull("DeliverToParty", wrappedImportHeader.DeliverToParty);
			AssertEquals("FreightApportionmentMethod", "161", wrappedImportHeader.FreightApportionmentMethod);
			AssertEquals("FreightCostsInNZD", 0m, wrappedImportHeader.FreightCostsInNZD);
			AssertNotNull("Invoices", wrappedImportHeader.Invoices);
			AssertNotNull("Items", wrappedImportHeader.Items);
			AssertEquals("LocationOfGoods", "2975W", wrappedImportHeader.LocationOfGoods);
			AssertEquals("MAFContainerDeclaration", true, wrappedImportHeader.MAFContainerDeclaration);
			AssertEquals("NatureOfTransaction", "10", wrappedImportHeader.NatureOfTransaction);
			AssertNotNull("NotifyParties", wrappedImportHeader.NotifyParties);
			AssertEquals("PortOfDischarge", "NZAKL", wrappedImportHeader.PortOfDischarge);
			AssertEquals("PortOfLoading", "SGSIN", wrappedImportHeader.PortOfLoading);
			AssertNotNull("Sellers", wrappedImportHeader.Sellers);
			AssertEquals("ShipmentOrigin", "SG", wrappedImportHeader.ShipmentOrigin);
			AssertNotNull("StuffingEstablishments", wrappedImportHeader.StuffingEstablishments);
			AssertNotNull("Suppliers", wrappedImportHeader.Suppliers);
		}

		public void TestCustomsDeliveryAuthorityNotifyParty()
		{
			CreateImportSeaJob();

			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "PORTS OF AUCKLAND";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "40174293L");    //ControlledPremisesID
			JobDeclaration.JE_OH_NotifyParty = notifyParty.PK;

			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			FormalEntry.CusEntryHeader header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedImportHeader = (IGoodsShipment)wrapper;
			AssertNotNull("NotifyParties", wrappedImportHeader.NotifyParties);
			int notifyPartyCount = 0;
			foreach (IOrganisation shipmentNotifyParty in wrappedImportHeader.NotifyParties)
			{
				AssertNotNull("NotifyParty", shipmentNotifyParty);
				AssertEquals("Notify Party Name", "PORTS OF AUCKLAND", shipmentNotifyParty.Name);
				AssertEquals("Notify Party Premise ID", "40174293L", shipmentNotifyParty.CustomsClientCode);
				notifyPartyCount++;
			}

			AssertEquals("Should be 1 Notify Party org details", 1, notifyPartyCount);
		}

		public void TestNotifyPartyCodes()
		{
			CreateImportSeaJob();

			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "WINTERFELL";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "11111A");
			JobDeclaration.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty.PK;

			JobDeclaration.JE_RL_NKPortOfDeliveryNotify = "DORNE";

			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedImportHeader = (IGoodsShipment)wrapper;

			AssertContainsExactElementsInAnyOrder("NotifyPartyCodes", new ZString[] { "11111A", "DORNE" }, wrappedImportHeader.NotifyPartyCodes);
		}

		[TestDate(2012, 10, 31)]
		public void TestIM1Message()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009908C");
			var expectedresult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BSIS00002309</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">15000</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20121031</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>HYOGO MARU</Name>
    <ID />
    <TypeCode>1</TypeCode>
    <JourneyID>227W</JourneyID>
  </BorderTransportMeans>
  <Carrier>
    <Name>ANL SHIPPING</Name>
  </Carrier>
  <Declarant>
    <ID />
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>SG</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <AdditionalInformation>
        <StatementDescription>MNHU0029382,YNYNN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0149961,YNYNN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0029382,75128</StatementDescription>
        <StatementTypeCode>MAS</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0149961,73492</StatementDescription>
        <StatementTypeCode>MAS</StatementTypeCode>
      </AdditionalInformation>
      <GoodsLocation>
        <ID>2975W</ID>
      </GoodsLocation>
      <LoadingLocation>
        <ID>SGSIN</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>OB293042-24902</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>45</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>MNHU0029382</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>45</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>MNHU0149961</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode>161</FreightChargeApportionmentCode>
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <Warehouse>
      <ID>2975W</ID>
    </Warehouse>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>2</QuantityQuantity>
    <TypeCode>07</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PK</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();

			AssertMultilineASCIIEquals("Generated xml message string", expectedresult.Trim(), im1MessageString);
		}

		[TestDate(2016, 05, 21)]
		public void TestDeliveryNotificationParty()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009908C");
			var expectedresult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BSIS00002309</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">15000</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20160521</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>HYOGO MARU</Name>
    <ID />
    <TypeCode>1</TypeCode>
    <JourneyID>227W</JourneyID>
  </BorderTransportMeans>
  <Carrier>
    <Name>ANL SHIPPING</Name>
  </Carrier>
  <Declarant>
    <ID />
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>SG</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <AdditionalInformation>
        <StatementDescription>MNHU0029382,YNYNN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0149961,YNYNN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0029382,75128</StatementDescription>
        <StatementTypeCode>MAS</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0149961,73492</StatementDescription>
        <StatementTypeCode>MAS</StatementTypeCode>
      </AdditionalInformation>
      <GoodsLocation>
        <ID>2975W</ID>
      </GoodsLocation>
      <LoadingLocation>
        <ID>SGSIN</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>OB293042-24902</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>45</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>MNHU0029382</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>45</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>MNHU0149961</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode>161</FreightChargeApportionmentCode>
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <NotifyParty>
      <Name>PORTS OF AUCKLAND</Name>
      <RoleCode>N2</RoleCode>
      <Communication />
    </NotifyParty>
    <Warehouse>
      <ID>2975W</ID>
    </Warehouse>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>2</QuantityQuantity>
    <TypeCode>07</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PK</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			CreateImportSeaJob();
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "PORTS OF AUCKLAND";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "40174293L");    //ControlledPremisesID
			JobDeclaration.JE_OH_NotifyParty = notifyParty.PK;

			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			FormalEntry.CusEntryHeader header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();

			AssertMultilineASCIIEquals("Generated xml message string with Notify Party", expectedresult.Trim(), im1MessageString);
		}

		public void TestExchangeRates_SameCurreny_NoDuplicateEntries()
		{
			CreateExportAirJob();
			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "HKD";
			invoice1.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "3307.30.00.00E";
			invoiceLine1.JI_LinePrice = 1000m;

			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "HKD";
			invoice2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;

			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3307.30.00.00E";
			invoiceLine2.JI_LinePrice = 1000m;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = entryHeader as FormalEntry.CusEntryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedHeader = (IExportDeclaration)wrapper;

			Assert("Only one exchange rate entry created for one currency", wrappedHeader.ExchangeRates.GroupBy(x => x.CurrencyCode).Select(g => g.Count()).All(r => r == 1));
		}

		public void TestExchangeRates()
		{
			CreateExportAirJob();
			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 1000m;

			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = "HKD";
			invoice2.JZ_InvoiceCurrExRate = 6.33m;
			invoice2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3307.30.00.00E";
			invoiceLine2.JI_LinePrice = 1000m;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedHeader = (IExportDeclaration)wrapper;

			foreach (ICurrency currencyDetails in wrappedHeader.ExchangeRates)
			{
				if (currencyDetails.CurrencyCode == "NZD")
				{
					AssertEquals("NZD ExchangeRate", 1m, currencyDetails.ExchangeRate);
					AssertEquals("NZD ExchangeRateIndicator - New Zealand Dollars", "N", currencyDetails.ExchangeRateIndicator);
				}
				else
				{
					AssertEquals("CurrencyCode", "HKD", currencyDetails.CurrencyCode);
					AssertEquals("HKD ExchangeRate", 6.33m, currencyDetails.ExchangeRate);
					AssertEquals("HKD ExchangeRateIndicator - Forward Cover", "C", currencyDetails.ExchangeRateIndicator);
				}
			}
		}

		public void TestForeignCurrency()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009908C");

			var expectedFloatingCurrencyRateTypeIndicator = @"<AdditionalInformation>
    <StatementCode>F</StatementCode>
    <StatementTypeCode>ERI</StatementTypeCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>";

			var expectedForwardCoverRateTypeIndicator = @"<AdditionalInformation>
    <StatementCode>C</StatementCode>
    <StatementTypeCode>ERI</StatementTypeCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>";

			var expectedAUDLineValue = @"<ValueAmount currencyID=""AUD"">1000</ValueAmount>";
			var expectedUSDLineValue = @"<ValueAmount currencyID=""USD"">2000</ValueAmount>";

			CreateExportAirForeignCurrencyJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			FormalEntry.CusEntryHeader header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();
			AssertEquals("EX1 Message - Floating exchange rate type indicator", true, ex1MessageString.Contains(expectedFloatingCurrencyRateTypeIndicator));
			AssertEquals("EX1 Message - Forward Cover exchange rate type indicator", true, ex1MessageString.Contains(expectedForwardCoverRateTypeIndicator));
			AssertEquals("EX1 Message - GAGI currency details AUD", true, ex1MessageString.Contains(expectedAUDLineValue));
			AssertEquals("EX1 Message - GAGI currency details USD", true, ex1MessageString.Contains(expectedUSDLineValue));
			Assert(true);
		}

		public void TestIM1TariffQtyIsNotSentWhenUQDoesNotExistEvenWithPartsAttribute()
		{
			var expectedPartsAddInfo = @"<AdditionalInformation>
        <StatementCode>PTS</StatementCode>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>";
			var expectedTariffElements = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">75</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">68</NetNetWeightMeasure>
      </GoodsMeasure>";
			var tariffQtyElement = "<TariffQuantity";

			CreateImportAirJob();
			var invoiceLine = JobDeclaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = "8210.00.00.00E";
			invoiceLine.JI_LinePrice = 50m;
			invoiceLine.JI_Weight = 75m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_NetWeight = 68m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.OtherInfos.AddNew(LineOtherInfoList.Codes.Parts, "");
			Factory.Save();

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			FormalEntry.CusEntryHeader header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - Parts OIN code should be present", true, im1MessageString.Contains(expectedPartsAddInfo));
			AssertEquals("IM1 Message - Tariff Quantity element should NOT be present in this case - as Customs UQ is empty", true, im1MessageString.Contains(expectedTariffElements));
			AssertEquals("IM1 Message - Tariff Quantity element does not exist at all", false, im1MessageString.Contains(tariffQtyElement));
		}

		public void TestEX1TariffQtyIsNotSentWhenUQDoesNotExistEvenWithPartsAttribute()
		{
			var expectedPartsAddInfo = @"<AdditionalInformation>
        <StatementCode>PTS</StatementCode>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>";
			var expectedTariffElements = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">75</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">68</NetNetWeightMeasure>
      </GoodsMeasure>";
			var tariffQtyElement = "<TariffQuantity";

			CreateImportAirJob();
			var invoiceLine = JobDeclaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = "8210.00.00.00E";
			invoiceLine.JI_LinePrice = 50m;
			invoiceLine.JI_Weight = 75m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_NetWeight = 68m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.OtherInfos.AddNew(LineOtherInfoList.Codes.Parts, "");
			Factory.Save();

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			FormalEntry.CusEntryHeader header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();
			AssertEquals("EX1 Message - Parts OIN code should be present", true, ex1MessageString.Contains(expectedPartsAddInfo));
			AssertEquals("EX1 Message - Tariff Quantity element should NOT be present in this case - as Customs UQ is empty", true, ex1MessageString.Contains(expectedTariffElements));
			AssertEquals("EX1 Message - Tariff Quantity element does not exist at all", false, ex1MessageString.Contains(tariffQtyElement));
		}

		public void TestMAFContainerStatements_WithData()
		{
			CreateImportSeaJob();
			var header = (FormalEntry.CusEntryHeader)JobDeclaration.CustomsEntryHeaders.AddNew();
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			IImportDeclaration wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			AssertNotNull("MAFContainerStatements", wrapper.GoodsShipment.MAFContainerStatements);

			int mafContainerStatementCount = 0;
			var stmt1 = ZString.Empty;
			var stmt2 = ZString.Empty;
			foreach (var mafStmt in wrapper.GoodsShipment.MAFContainerStatements)
			{
				mafContainerStatementCount++;
				if (mafStmt.StartsWith("MNHU0029382"))
				{
					stmt1 = mafStmt;
				}
				else
				{
					stmt2 = mafStmt;
				}
			}

			AssertEquals("Should be two container MAF statements created in wrapped header", 2, mafContainerStatementCount);
			AssertEquals("MAFContainerStatement", "MNHU0029382,YNYNN", stmt1);
			AssertEquals("MAFContainerStatement", "MNHU0149961,YNYNN", stmt2);
		}

		public void TestMAFContainerStatements_WithoutData()
		{
			CreateImportSeaJob();

			JobDeclaration.JE_HaveMAFContainerDeclaration = false;
			JobDeclaration.JE_SendMCDContainerQuarantineDeclaration = true;

			var header = (FormalEntry.CusEntryHeader)JobDeclaration.CustomsEntryHeaders.AddNew();
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			IImportDeclaration wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			AssertNotNull("MAFContainerStatements", wrapper.GoodsShipment.MAFContainerStatements);

			int mafContainerStatementCount = 0;
			var stmt1 = ZString.Empty;
			var stmt2 = ZString.Empty;
			foreach (var mafStmt in wrapper.GoodsShipment.MAFContainerStatements)
			{
				mafContainerStatementCount++;
				if (mafStmt.StartsWith("MNHU0029382"))
				{
					stmt1 = mafStmt;
				}
				else
				{
					stmt2 = mafStmt;
				}
			}

			AssertEquals("Should be two container MAF statements created in wrapped header", 2, mafContainerStatementCount);
			AssertEquals("MAFContainerStatement", "MNHU0029382,", stmt1);
			AssertEquals("MAFContainerStatement", "MNHU0149961,", stmt2);
		}

		public void TestMAFContainerStatements_NotSending()
		{
			CreateImportSeaJob();
			JobDeclaration.JE_SendMCDContainerQuarantineDeclaration = false;

			var header = (FormalEntry.CusEntryHeader)JobDeclaration.CustomsEntryHeaders.AddNew();
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			IImportDeclaration wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			Assert("MAFContainerDeclaration", !wrapper.GoodsShipment.MAFContainerDeclaration);
			Assert("MAFContainerStatements", !wrapper.GoodsShipment.MAFContainerStatements.Any());
		}

		public void TestVoyageIsTruncated()
		{
			CreateImportSeaJob();
			JobDeclaration.JE_VoyageFlightNo = "J227A-EAST";
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var headerWrapper = (IImportDeclaration)wrapper;
			AssertEquals("Voyage Number should have been truncated to 8 characters", "J227A-EA", headerWrapper.VoyageNo);
		}

		public void TestMISCOrgSendsOverrideName()
		{
			var expectedResult = @"<Importer>
    <Name>Jeremy Jones</Name>
    <Address>
      <CityName>Wollongong</CityName>
      <CountryCode>AU</CountryCode>
      <CountrySubDivisionName>NSW</CountrySubDivisionName>
      <Line>17 Hallop St. Wonoona</Line>
      <PostcodeID>2517</PostcodeID>
    </Address>
    <Contact>
      <Name>Wendy Williams</Name>
      <Communication>
        <ID>wwilliams@gmail.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>61319875627</ID>
        <TypeID>AL</TypeID>
      </Communication>
    </Contact>
  </Importer>";
			var jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobShipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;

			CreateImportAirJob();
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			JobDeclaration.JE_OH_Importer = JobDeclaration.CachedMiscOrgPK;
			JobDeclaration.JE_JS = jobShipment.PK;
			jobShipment.ConsigneeDocumentaryAddress.E2_CompanyName = "Jeremy Jones";
			jobShipment.ConsigneeDocumentaryAddress.E2_Address1 = "17 Hallop St.";
			jobShipment.ConsigneeDocumentaryAddress.E2_Address2 = "Wonoona";
			jobShipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			jobShipment.ConsigneeDocumentaryAddress.E2_City = "Wollongong";
			jobShipment.ConsigneeDocumentaryAddress.E2_Postcode = "2517";
			jobShipment.ConsigneeDocumentaryAddress.E2_State = "NSW";
			jobShipment.ConsigneeDocumentaryAddress.E2_Contact = "Wendy Williams";
			jobShipment.ConsigneeDocumentaryAddress.E2_Email = "wwilliams@gmail.com";
			jobShipment.ConsigneeDocumentaryAddress.E2_Mobile = "61319875627";
			Factory.Save();

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			FormalEntry.CusEntryHeader header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - override details for miscellaneous importer have been included in the message", true, im1MessageString.Contains(expectedResult));
		}

		public void TestMPIApprovedSystemNumbers()
		{
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var headerWrapper = (IImportDeclaration)wrapper;

			var mas1Expected = "MNHU0029382,75128";
			var mas2Expected = "MNHU0149961,73492";
			int masCodesGenerated = 0;
			foreach (ZString mpiApprovedNumber in headerWrapper.GoodsShipment.MPIApprovedSystemNumbers)
			{
				masCodesGenerated++;
				if (mpiApprovedNumber == mas1Expected ||
					mpiApprovedNumber == mas2Expected)
				{
					Assert("MPIApprovedSystemNumber", condition: true);
				}
				else
				{
					Assert(false);
				}
			}

			AssertEquals(2, masCodesGenerated);
		}

		public void TestSendersReference()
		{
			CreateExportAirJob();
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			var entryHeader = (ECIWriteOff.CusEntryHeader)JobDeclaration.CusEntryHeader;
			entryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.WriteOff;

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "75328491";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.ECIWriteOff;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			var wrappedWriteOffHeader = new TSWEntryHeaderWrapper(entryHeader, null);
			ICargoReportExport creHeader = wrappedWriteOffHeader;
			AssertEquals("Write-off entry SenderReferenceNumber", "BEX0042709", creHeader.SenderReferenceNumber);

			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Factory.Save();

			var formalEntryHeader = (FormalEntry.CusEntryHeader)JobDeclaration.CustomsEntryHeaders[1];
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(formalEntryHeader, additionalMessageInformation);
			var wrappedHeader = (IExportDeclaration)wrapper;
			AssertEquals("SenderReferenceNumber when IsWriteOffChangedToFormal cannot be the same as write-off entry, should therefore be set to ref placeholder so new reference is generated on message build", TSWConstants.SendersReferencePlaceHolder, wrappedHeader.SenderReferenceNumber);
		}

		public void TestSellers()
		{
			CreateImportSeaJob();
			var seller1 = Factory.NewWithValidTestData<OrgAddress>();
			seller1.OA_Address1 = "Seller 1";
			var seller2 = Factory.NewWithValidTestData<OrgAddress>();
			seller2.OA_Address1 = "Seller 2";

			var invoice1 = JobDeclaration.Invoices[0];
			invoice1.JZ_OA_SellerAddress = seller1.PK;
			var invoiceLine = invoice1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;

			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice2.JZ_InvoiceDate = new ZDateTime(2014, 2, 25);
			invoice2.JZ_OA_SellerAddress = seller2.PK;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3307.30.00.00E";
			invoiceLine2.JI_LinePrice = 10000m;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedImportHeader = (IGoodsShipment)wrapper;
			AssertNotNull("Sellers", wrappedImportHeader.Sellers);
			int sellerCount = 0;
			foreach (IOrganisation seller in wrappedImportHeader.Sellers)
			{
				sellerCount++;
			}
			AssertEquals("Should be two seller details created in wrapped header", 2, sellerCount);
		}

		public void TestDocumentsAndPermits()
		{
			CreateImportSeaJob();
			var permit1 = JobDeclaration.PermitCodes.AddNew();
			permit1.ZO_Code = "POD";
			permit1.ZO_Data = "M-042384Y";

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedImportHeader = (IImportDeclaration)wrapper;
			AssertNotNull("Permits", wrappedImportHeader.Permits);
			int permitCount = 0;
			var permitDetail = ZString.Empty;
			foreach (ZString permit in wrappedImportHeader.Permits)
			{
				permitCount++;
				permitDetail = permit;
			}
			AssertEquals("Should be one permit details created in wrapped header", 1, permitCount);
		}

		[TestDate(2012, 10, 31)]
		public void TestIM1MessageStuffingEstablishment()
		{
			var packingOrg = Factory.NewWithValidTestData<OrgHeader>();
			packingOrg.OH_FullName = "MAERSK LINES";
			packingOrg.MainAddress.OA_Address1 = "1001 Link Rd";
			packingOrg.MainAddress.OA_City = "Port Botany";
			packingOrg.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			packingOrg.MainAddress.OA_PostCode = "2012";

			var packingOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			packingOrg2.OH_FullName = "CUNNARD";
			packingOrg2.MainAddress.OA_Address1 = "15 Wharf RD";
			packingOrg2.MainAddress.OA_City = "Port Botany";
			packingOrg2.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			packingOrg2.MainAddress.OA_PostCode = "2012";

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009908C");
			var expectedresult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BSIS00002309</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">15000</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20121031</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>HYOGO MARU</Name>
    <ID />
    <TypeCode>1</TypeCode>
    <JourneyID>227W</JourneyID>
  </BorderTransportMeans>
  <Carrier>
    <Name>ANL SHIPPING</Name>
  </Carrier>
  <Declarant>
    <ID />
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>SG</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <AdditionalInformation>
        <StatementDescription>MNHU0029382,YNYNN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0149961,YNYNN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0029382,75128</StatementDescription>
        <StatementTypeCode>MAS</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementDescription>MNHU0149961,73492</StatementDescription>
        <StatementTypeCode>MAS</StatementTypeCode>
      </AdditionalInformation>
      <GoodsLocation>
        <ID>2975W</ID>
      </GoodsLocation>
      <LoadingLocation>
        <ID>SGSIN</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>OB293042-24902</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>45</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>MNHU0029382</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>16B</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>45</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>MNHU0149961</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>16B</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode>161</FreightChargeApportionmentCode>
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <StuffingEstablishment>
      <Name>MAERSK LINES</Name>
      <Address>
        <CityName>Port Botany</CityName>
        <CountryCode>AU</CountryCode>
        <CountrySubDivisionName>NSW</CountrySubDivisionName>
        <Line>1001 Link Rd</Line>
        <PostcodeID>2012</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <StuffingEstablishment>
      <Name>CUNNARD</Name>
      <Address>
        <CityName>Port Botany</CityName>
        <CountryCode>AU</CountryCode>
        <CountrySubDivisionName>NSW</CountrySubDivisionName>
        <Line>15 Wharf RD</Line>
        <PostcodeID>2012</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <Warehouse>
      <ID>2975W</ID>
    </Warehouse>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>2</QuantityQuantity>
    <TypeCode>07</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PK</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			CreateImportSeaJob();
			var container1 = JobDeclaration.CusContainers[0];
			var container2 = JobDeclaration.CusContainers[1];
			container1.CO_OA_PackingLocation = packingOrg.Addresses.MainAddress.PK;
			container2.CO_OA_PackingLocation = packingOrg2.Addresses.MainAddress.PK;

			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();

			AssertMultilineASCIIEquals("Generated xml message string", expectedresult.Trim(), im1MessageString);
		}

		public void TestPreviousDocumentDetails()
		{
			CreateImportSeaJob();
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Temporary;

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "75328491";
			entryNum.CE_EntryType = "FRM";
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			var exportCompletionJob = CreateCompletionJob();
			var completionHeader = (FormalEntry.CompletionCusEntryHeader)exportCompletionJob.CustomsEntryHeaders[1];
			completionHeader.CH_BGMReference = "";
			var additionalMessageInformation = new AdditionalMessageInformation(null, exportCompletionJob.eDocsForSelection, TSWTransactionTypes.Completion, exportCompletionJob.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(completionHeader, additionalMessageInformation);
			var wrappedExportHeader = (IExportDeclaration)wrapper;
			AssertEquals("PreviousDocumentNo", "75328491", wrappedExportHeader.PreviousDocumentNo);
			AssertEquals("PreviousDocumentType", "I51", wrappedExportHeader.PreviousDocumentType);
		}

		public void TestDrawbackCredits()
		{
			CreateExportDrawbackJob();
			var entryHeader = (FormalEntry.CusEntryHeader)JobDeclaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var alacCredit = entryLine.Fees[0];
			alacCredit.CF_ChargeAmount = 100m;
			var dutyCredit = entryLine.Fees[1];
			dutyCredit.CF_ChargeAmount = 150m;
			var gstCredit = entryLine.Fees[2];
			gstCredit.CF_ChargeAmount = 200m;
			Factory.Save();

			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, entryHeader.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(entryHeader, additionalMessageInformation);
			var wrappedExportHeader = (IExportDeclaration)wrapper;
			AssertNotNull("DutyTaxFees", wrappedExportHeader.DutyTaxFees);
			bool alacCreditFound = false;
			bool dutyCreditFound = false;
			bool gstCreditFound = false;
			foreach (IDutyTaxFee fee in wrappedExportHeader.DutyTaxFees)
			{
				if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.CUD)
				{
					dutyCreditFound = true;
					AssertEquals("Duty credit", 150m, fee.Amount);
				}
				else if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.AL)
				{
					alacCreditFound = true;
					AssertEquals("ALAC Credit", 100m, fee.Amount);
				}
				else if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.GST)
				{
					gstCreditFound = true;
					AssertEquals("GST Credit", 200m, fee.Amount);
				}
			}

			AssertEquals("taxCreditFound", true, dutyCreditFound);
			AssertEquals("alacCreditFound", true, alacCreditFound);
			AssertEquals("gstCreditFound", true, gstCreditFound);
		}

		[TestDate(2014, 03, 24)]
		public void TestAddInfoData()
		{
			var expectedresult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>EX</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>EX1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>E40</TypeCode>
  <FunctionalReferenceID>BEX0042709</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">15</TotalGrossMassMeasure>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <Content>Additional information trader wants to provide Customs can be entered here</Content>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>QF108</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>
  <Carrier>
    <Name>QANTAS</Name>
  </Carrier>
  <Declarant>
    <ID />
  </Declarant>
  <Exporter>
    <ID />
  </Exporter>
  <GoodsShipment>
    <ExitDateTime formatCode=""102"">20140324</ExitDateTime>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <GoodsLocation>
        <ID />
      </GoodsLocation>
      <LoadingLocation>
        <ID>NZAKL</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>08100342948</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <UnloadingLocation>
        <ID>AUSYD</ID>
      </UnloadingLocation>
    </Consignment>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <Importer>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </Importer>
  </GoodsShipment>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>15</QuantityQuantity>
    <TypeCode>PKT</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			CreateExportAirJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			additionalMessageInformation.AM_FreeText = "Additional information trader wants to provide Customs can be entered here";
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();

			AssertMultilineASCIIEquals("Generated xml message string has additional information free text", expectedresult.Trim(), ex1MessageString);
		}

		[TestDate(2014, 03, 24)]
		public void TestExchangeRatesInMessage()
		{
			CreateExportAirJob();
			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 1000m;

			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = "HKD";
			invoice2.JZ_InvoiceCurrExRate = 6.33m;
			invoice2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3307.30.00.00E";
			invoiceLine2.JI_LinePrice = 1000m;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedHeader = (IExportDeclaration)wrapper;

			foreach (ICurrency currencyDetails in wrappedHeader.ExchangeRates)
			{
				if (currencyDetails.CurrencyCode == "NZD")
				{
					AssertEquals("NZD ExchangeRate", 1m, currencyDetails.ExchangeRate);
					AssertEquals("NZD ExchangeRateIndicator - New Zealand Dollars", "N", currencyDetails.ExchangeRateIndicator);
				}
				else
				{
					AssertEquals("CurrencyCode", "HKD", currencyDetails.CurrencyCode);
					AssertEquals("HKD ExchangeRate", 6.33m, currencyDetails.ExchangeRate);
					AssertEquals("HKD ExchangeRateIndicator - Forward Cover", "C", currencyDetails.ExchangeRateIndicator);
				}
			}

			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();

			var nzdCurrencyIndicator = @"<AdditionalInformation>
    <StatementCode>N</StatementCode>
    <StatementTypeCode>ERI</StatementTypeCode>";
			var nzdExchangeRate = @"<CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>";
			var hkdCurrencyIndicator = @"<AdditionalInformation>
    <StatementCode>C</StatementCode>
    <StatementTypeCode>ERI</StatementTypeCode>";
			var hkdExchangeRate = @"<CurrencyExchange>
    <RateNumeric>6.33</RateNumeric>
    <CurrencyTypeCode>HKD</CurrencyTypeCode>
  </CurrencyExchange>";

			AssertEquals("NZD currency details from invoice 1", true, ex1MessageString.Contains(nzdCurrencyIndicator));
			AssertEquals("NZD exchange rate", true, ex1MessageString.Contains(nzdExchangeRate));
			AssertEquals("HKD currency details from invoice 2", true, ex1MessageString.Contains(hkdCurrencyIndicator));
			AssertEquals("HKD exchange rate", true, ex1MessageString.Contains(hkdExchangeRate));
		}

		public void TestPointers2HBs1ContainerInEX1Message()
		{
			//<!--This example represents the association between 1 Masterbill, 2 Housebills, 1 Container and 2 Package Types-->
			var expectedBillDetails = "<TransportContractDocument>\r\n        <ID>H458239-1</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>B942042-2</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var alternateSeqBillDetails = "<TransportContractDocument>\r\n        <ID>H458239-1</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>B942042-2</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var expectedContainerElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>7</FullnessCode>\r\n        <ID>YKKU9388747</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>7</FullnessCode>\r\n        <ID>YKKU9388747</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var alternativeSeqContainerElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>7</FullnessCode>\r\n        <ID>YKKU9388747</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>7</FullnessCode>\r\n        <ID>YKKU9388747</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedPackagingElements = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>97</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>2</SequenceNumeric>\r\n    <QuantityQuantity>53</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>";

			Create2HB1ContainerExportJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();

			AssertEquals("expected BillDetails:" + "\r\n\r\n" + expectedBillDetails + "\r\n\r\nmessage generated:\r\n\r\n" + ex1MessageString, true, (ex1MessageString.Contains(expectedBillDetails) || ex1MessageString.Contains(alternateSeqBillDetails)));
			AssertEquals("expected ContainerElements:" + "\r\n\r\n" + expectedContainerElements + "\r\n\r\nmessage generated:\r\n\r\n" + ex1MessageString, true, (ex1MessageString.Contains(expectedContainerElements) || ex1MessageString.Contains(alternativeSeqContainerElements)));
			AssertEquals("expected PackagingElements:" + "\r\n\r\n" + expectedPackagingElements + "\r\n\r\nmessage generated:\r\n\r\n" + ex1MessageString, true, ex1MessageString.Contains(expectedPackagingElements));
		}

		public void TestEX1EquipmentPackagePointers()
		{
			var expectedEquipmentResult = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>HLMU0398470</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>YKKU9348472</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";

			var expectedPackagingResult = @"<Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>2</QuantityQuantity>
    <TypeCode>07</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>07</TypeCode>
  </Packaging>";

			CreateTwoContainerTwoPackageJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();
			Assert("Transport Equipment container package pointers point to correct packages", ex1MessageString.Contains(expectedEquipmentResult));
			Assert("Packaging elements", ex1MessageString.Contains(expectedPackagingResult));
		}

		public void TestEquipmentPackagePointersWhenTwoPivots()
		{
			var expectedEquipmentResult = @"</TransportContractDocument>
      <TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>HLMU0398470</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <UnloadingLocation>
        <ID>AUSYD</ID>
      </UnloadingLocation>";

			var expectedPackagingResult = @"<Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>2</QuantityQuantity>
    <TypeCode>07</TypeCode>
  </Packaging>";

			var equipmentNotExpected = @"<TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>";

			CreateContainerPackageJobWith2PackingPivots();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();
			Assert("Transport Equipment container package pointers only have 1 package pointer", ex1MessageString.Contains(expectedEquipmentResult));
			Assert("Should not be second transport equipment elements", !ex1MessageString.Contains(equipmentNotExpected));
			Assert("Packaging elements", ex1MessageString.Contains(expectedPackagingResult));
		}

		public void TestPointers_Example_a()
		{
			//<!--This example represents the association between 1 Masterbill, 1 Housebill, 1 Container and 1 Package Type-->
			var expectedBillPointerDetails = "TransportContractDocument>\r\n        <ID>61831905231</ID>\r\n        <TypeCode>MB</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>COS12345678</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument";
			var expectedTransportEquipmentElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>CAXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedGoodsItemPackagingElements = "<Packaging>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <MarksNumbersID>N/M</MarksNumbersID>\r\n        <QuantityQuantity>1</QuantityQuantity>\r\n        <TypeCode>BX</TypeCode>\r\n        <VolumeMeasure unitCode=\"MTQ\">1</VolumeMeasure>\r\n      </Packaging>";
			var expectedDeclarationPackagingElements = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>14</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>";
			CreateExampleAJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();

			AssertEquals("expectedBillPointerDetails", true, ex1MessageString.Contains(expectedBillPointerDetails));
			AssertEquals("expectedTransportEquipmentElements", true, ex1MessageString.Contains(expectedTransportEquipmentElements));
			AssertEquals("expectedGoodsItemPackagingElements", true, ex1MessageString.Contains(expectedGoodsItemPackagingElements));
			AssertEquals("expectedDeclarationPackagingElements", true, ex1MessageString.Contains(expectedDeclarationPackagingElements));
		}

		public void TestPointers_Example_d()
		{
			//<!--This example represents the association between 1 Masterbill, 2 Housebills each with 2 Containers and 1 Package Type-->
			var expectedBillElements = "<TransportContractDocument>\r\n        <ID>123456</ID>\r\n        <TypeCode>MB</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>3</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>COS12345678</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>XYZ99887766</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>3</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>4</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var expectedTransportEquipmentElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>CAXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>UUXU99203930</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>3</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>ZZUU9283929</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>3</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>4</SequenceNumeric>\r\n        <CharacteristicCode>40</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>ZZXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>4</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedPackagingElements = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>14</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>2</SequenceNumeric>\r\n    <QuantityQuantity>8</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>3</SequenceNumeric>\r\n    <QuantityQuantity>8</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>4</SequenceNumeric>\r\n    <QuantityQuantity>5</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>";
			CreateExampleDJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();

			AssertEquals("expectedPointerDetails", true, ex1MessageString.Contains(expectedBillElements));
			AssertEquals("expectedTransportEquipmentElements", true, ex1MessageString.Contains(expectedTransportEquipmentElements));
			AssertEquals("expectedPackagingElements", true, ex1MessageString.Contains(expectedPackagingElements));
		}

		public void TestPointers_Example_e()
		{
			//<!--This example represents the association between 1 Housebill, 1 Pallet and 1 Package Type-->
			// Note - Enterprise always requires MasterBill as well
			var expectedBillElements = "<TransportContractDocument>\r\n        <ID>61831905231</ID>\r\n        <TypeCode>HWB</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var expectedTransportEquipmentElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>16</CharacteristicCode>\r\n        <ID>1</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedGAGIPackagingElements = "<Packaging>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <MarksNumbersID>N/M</MarksNumbersID>\r\n        <QuantityQuantity>1</QuantityQuantity>\r\n        <TypeCode>BX</TypeCode>\r\n        <VolumeMeasure unitCode=\"MTQ\">1</VolumeMeasure>\r\n      </Packaging>\r\n      <Packaging>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <MarksNumbersID>U994382/I47, J049428/732</MarksNumbersID>\r\n        <QuantityQuantity>25</QuantityQuantity>\r\n        <TypeCode>CT</TypeCode>\r\n        <VolumeMeasure unitCode=\"MTQ\">4</VolumeMeasure>\r\n      </Packaging>";
			var expectedPackagingElements = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>14</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>";
			CreateExampleEJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();

			AssertEquals("expectedPointerDetails", true, ex1MessageString.Contains(expectedBillElements));
			AssertEquals("expectedTransportEquipmentElements", true, ex1MessageString.Contains(expectedTransportEquipmentElements));
			AssertEquals("expectedGAGIPackagingElements", true, ex1MessageString.Contains(expectedGAGIPackagingElements));
			AssertEquals("expectedPackagingElements", true, ex1MessageString.Contains(expectedPackagingElements));
		}

		public void TestPointers_Example_f()
		{
			//<!-- This example represents the association between a single Transport Contract Document (Bill of Lading) covering both containerised (Transport Equipment – 31B) and non-containerised (Packaging – 93A) Cargo.-->
			var expectedBillElements = "<TransportContractDocument>\r\n        <ID>BILL123456</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var expectedTransportEquipmentElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>21</CharacteristicCode>\r\n        <FullnessCode>5</FullnessCode>\r\n        <ID>ABCU1234560</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedPackagingElements = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>1</QuantityQuantity>\r\n    <TypeCode>VN</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>2</SequenceNumeric>\r\n    <QuantityQuantity>10</QuantityQuantity>\r\n    <TypeCode>PK</TypeCode>\r\n  </Packaging>";
			CreateExampleFJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();

			AssertEquals("expectedBillElements", true, im1MessageString.Contains(expectedBillElements));
			AssertEquals("expectedTransportEquipmentElements", true, im1MessageString.Contains(expectedTransportEquipmentElements));
			AssertEquals("expectedPackagingElements", true, im1MessageString.Contains(expectedPackagingElements));
		}

		public void TestPointers_Example_g()
		{
			//<!--This example represents the association between 1 Masterbill, 2 Housebills, 1 Container (LCL) and 2 Package Types-->
			var expectedBillDetails = "<TransportContractDocument>\r\n        <ID>123456</ID>\r\n        <TypeCode>MB</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>3</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>COS12345678</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>XYZ99887766</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var alternateSeqBillDetails = "<TransportContractDocument>\r\n        <ID>123456</ID>\r\n        <TypeCode>MB</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>3</SequenceNumeric>\r\n          <DocumentSectionCode>30B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>COS12345678</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>\r\n      <TransportContractDocument>\r\n        <ID>XYZ99887766</ID>\r\n        <TypeCode>BM</TypeCode>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>67A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <DocumentSectionCode>28A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>31B</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportContractDocument>";
			var expectedContainerElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>23</CharacteristicCode>\r\n        <FullnessCode>7</FullnessCode>\r\n        <ID>CAXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <CharacteristicCode>23</CharacteristicCode>\r\n        <FullnessCode>7</FullnessCode>\r\n        <ID>CAXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var alternativeSeqContainerElements = "<TransportEquipment>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <CharacteristicCode>23</CharacteristicCode>\r\n        <FullnessCode>7</FullnessCode>\r\n        <ID>CAXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>2</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>\r\n      <TransportEquipment>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <CharacteristicCode>23</CharacteristicCode>\r\n        <FullnessCode>7</FullnessCode>\r\n        <ID>CAXU2968920</ID>\r\n        <Pointer>\r\n          <DocumentSectionCode>42A</DocumentSectionCode>\r\n        </Pointer>\r\n        <Pointer>\r\n          <SequenceNumeric>1</SequenceNumeric>\r\n          <DocumentSectionCode>93A</DocumentSectionCode>\r\n        </Pointer>\r\n      </TransportEquipment>";
			var expectedPackagingElements = "<Packaging>\r\n    <SequenceNumeric>1</SequenceNumeric>\r\n    <QuantityQuantity>14</QuantityQuantity>\r\n    <TypeCode>CT</TypeCode>\r\n  </Packaging>\r\n  <Packaging>\r\n    <SequenceNumeric>2</SequenceNumeric>\r\n    <QuantityQuantity>8</QuantityQuantity>\r\n    <TypeCode>BX</TypeCode>\r\n  </Packaging>";

			CreateExampleGJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "EX1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var ex1Builder = new EX1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var ex1MessageString = ex1Builder.GetXMLMessage();

			AssertEquals("expectedBillDetails:" + "\r\n\r\n" + expectedBillDetails + "\r\n\r\nmessage generated:\r\n\r\n" + ex1MessageString, true, (ex1MessageString.Contains(expectedBillDetails) || ex1MessageString.Contains(alternateSeqBillDetails)));
			AssertEquals("expectedContainerElements:" + "\r\n\r\n" + expectedContainerElements + "\r\n\r\nmessage generated:\r\n\r\n" + ex1MessageString, true, (ex1MessageString.Contains(expectedContainerElements) || ex1MessageString.Contains(alternativeSeqContainerElements)));
			AssertEquals("expectedPackagingElements:" + "\r\n\r\n" + expectedPackagingElements + "\r\n\r\nmessage generated:\r\n\r\n" + ex1MessageString, true, ex1MessageString.Contains(expectedPackagingElements));
		}

		public void TestTSWReferenceNumberForIPIEntry()
		{
			CreateImportSeaJob();
			JobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			var header = (FormalEntry.PrimaryIndustriesCusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var headerWrapper = (IImportDeclaration)wrapper;
			AssertEquals("MessageType should be an IPI entry", MessageTypeList.Codes.IPI, headerWrapper.MessageType);
			AssertEquals("TSWReferenceNumber is empty on original submission", "", headerWrapper.TSWReferenceNumber);

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "75328491";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.PrimaryIndustriesEntry;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Replace, JobDeclaration.Factory, "IM1");
			wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			headerWrapper = wrapper;
			AssertEquals("MessageType should still be an IPI entry", MessageTypeList.Codes.IPI, headerWrapper.MessageType);
			AssertEquals("TSWReferenceNumber should return the entry number from the original submission response", "75328491", headerWrapper.TSWReferenceNumber);
		}

		public void TestTSWReferenceForFormalChangedToIPI()
		{
			CreateImportSeaJob();
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var headerWrapper = (IImportDeclaration)wrapper;
			AssertEquals("MessageType should be a Formal entry", MessageTypeList.Codes.I10, headerWrapper.MessageType);
			AssertEquals("TSWReferenceNumber is empty on original submission", "", headerWrapper.TSWReferenceNumber);

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "61844737";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.FormalEntry;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			header = (FormalEntry.CusEntryHeader)entryHeader;
			additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Replace, JobDeclaration.Factory, "IM1");
			wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			headerWrapper = wrapper;
			AssertEquals("MessageType should still be a Formal entry", MessageTypeList.Codes.I10, headerWrapper.MessageType);
			AssertEquals("TSWReferenceNumber should return the entry number from the original formal submission response", "61844737", headerWrapper.TSWReferenceNumber);

			// now change the current declaration to an IPI declaration
			JobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			var ipiEntryHeader = JobDeclaration.CusEntryHeader;
			var ipiHeader = (FormalEntry.PrimaryIndustriesCusEntryHeader)ipiEntryHeader;
			additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var ipiWrapper = new CusEntryHeaderWrapper(ipiHeader, additionalMessageInformation);
			var ipiHeaderWrapper = (IImportDeclaration)ipiWrapper;
			AssertEquals("MessageType should be an IPI entry", MessageTypeList.Codes.IPI, ipiHeaderWrapper.MessageType);
			AssertEquals("TSWReferenceNumber should be empty for the new IPI submission", "", ipiHeaderWrapper.TSWReferenceNumber);

			var ipiEntryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			ipiEntryNum.CE_EntryNum = "75328491";
			ipiEntryNum.CE_EntryType = CusEntryNumberTypeList.Codes.PrimaryIndustriesEntry;
			ipiEntryNum.CE_ParentID = ipiEntryHeader.PK;
			ipiEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Replace, JobDeclaration.Factory, "IM1");
			wrapper = new CusEntryHeaderWrapper(ipiHeader, additionalMessageInformation);
			ipiHeaderWrapper = wrapper;
			AssertEquals("MessageType should still be an IPI entry", MessageTypeList.Codes.IPI, ipiHeaderWrapper.MessageType);
			AssertEquals("TSWReferenceNumber should return the entry number from the original submission response", "75328491", ipiHeaderWrapper.TSWReferenceNumber);

			// Change the declaration back to a Formal declaration again
			JobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			header = (FormalEntry.CusEntryHeader)entryHeader;
			additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Replace, JobDeclaration.Factory, "IM1");
			wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			headerWrapper = wrapper;
			AssertEquals("MessageType should still be back to a Formal entry", MessageTypeList.Codes.I10, headerWrapper.MessageType);
			AssertEquals("Any further amemdment on this dec now should have the TSWReferenceNumber return the entry number from the original FORMAL submission response", "61844737", headerWrapper.TSWReferenceNumber);

			// Change the declaration back to the Primary Industries declaration again
			JobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			ipiHeader = (FormalEntry.PrimaryIndustriesCusEntryHeader)ipiEntryHeader;
			additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Replace, JobDeclaration.Factory, "IM1");
			ipiWrapper = new CusEntryHeaderWrapper(ipiHeader, additionalMessageInformation);
			ipiHeaderWrapper = ipiWrapper;
			AssertEquals("MessageType should still now be back to an IPI entry", MessageTypeList.Codes.IPI, ipiHeaderWrapper.MessageType);
			AssertEquals("Any further amemdment on this dec now should have the TSWReferenceNumber return the entry number from the IPI submission response", "75328491", ipiHeaderWrapper.TSWReferenceNumber);
		}

		public void TestIM1FromAirJob()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			var expectedResult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>BIS00002309C</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">150</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20130719</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>QF108</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>
  <Carrier>
    <Name>QANTAS AIRFREIGHT</Name>
  </Carrier>
  <CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID />
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">2075.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>AU</ExportationCountryCode>
    <TransactionNatureCode />
    <Consignment>
      <GoodsLocation>
        <ID />
      </GoodsLocation>
      <LoadingLocation>
        <ID>AUSYD</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>0810049584</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HB92027</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">0</FreightChargeAmount>
      <FreightChargeApportionmentCode>161</FreightChargeApportionmentCode>
    </CustomsValuation>
    <DeliveryDestination>
      <Name />
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">10000</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <Commodity>
        <Description>PERFUMED BATH SALTS ETC</Description>
        <ValueAmount currencyID=""NZD"">10000</ValueAmount>
        <Classification>
          <ID>3307300000E</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">500.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">1575.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Source>
          <CountryCode />
        </Source>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">150</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode />
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>Packaging Marks</MarksNumbersID>
        <QuantityQuantity>1</QuantityQuantity>
        <TypeCode>BX</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">1</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">0</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">0</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID />
      <ConditionCode />
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
  </GoodsShipment>
  <Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>15</QuantityQuantity>
    <TypeCode>PCS</TypeCode>
  </Packaging>
  <PreviousDocument>
    <ID />
    <TypeCode />
  </PreviousDocument>
</Declaration>
</DocumentMetadata>";
			CreateImportAirJob(declaration => declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Completion);
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();

			AssertMultilineASCIIEquals("Import Air - Original Message", expectedResult, im1MessageString);
		}

		public void TestItemPackagingQtyIsNotBlank()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			var expectedResult = @"<Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>Packaging Marks</MarksNumbersID>
        <QuantityQuantity>8</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">2</VolumeMeasure>
      </Packaging>";

			CreateImportAirJob();
			var invoiceLine = JobDeclaration.InvoiceLines[0];
			var packaging = invoiceLine.ItemPackages[0];
			packaging.NZ_NumberOfPackages = 8;
			packaging.NZ_PackageUQ = "";
			packaging.NZ_PackageVolume = 1.9m;
			Factory.Save();

			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			FormalEntry.CusEntryHeader header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - Package UQ should not be blank - should defaut package (PK) when empty", true, im1MessageString.Contains(expectedResult));
		}

		[TestDate(2016, 11, 09)]
		public void TestPrefGroupCodeInMessageWhenSinglePreferenceCountry()
		{
			var nzGroup = Factory.New<NZCGroup>();
			nzGroup.Q4_Code = "JP";
			nzGroup.Q4_Name = "JAPAN";
			nzGroup.Q4_IsCountry = true;

			var classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "0101.99.99.99J";
			classification.U0_DateActiveFrom = new ZDateTime(2016, 1, 1);
			classification.U0_Description = "TestClass";
			classification.U0_ComputerDumpDescr = "TestClass";

			var countryGroup = Factory.New<NZCCountryGroup>();
			countryGroup.U4_Country = "JP";
			countryGroup.U4_DateFrom = new ZDateTime(2016, 1, 1);

			var dutyRateFuturePreferential = Factory.New<NZCClassificationDutyRate>();
			dutyRateFuturePreferential.U1_U0_Classification = classification.PK;
			dutyRateFuturePreferential.U1_PreferentialCountryGroup = "JP";
			dutyRateFuturePreferential.U1_DateActiveFrom = new ZDateTime(2016, 1, 1);
			dutyRateFuturePreferential.U1_DutyRatePercent = 0m;

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			var expectedResult = @"<DutyTaxFee>
          <DutyRegimeCode>JP</DutyRegimeCode>
        </DutyTaxFee>";
			CreateImportSeaJob();
			JobDeclaration.InvoiceLines.RemoveAndDeleteAll();
			Factory.Save();

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.JI_Tariff = "0101.99.99.99J";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_QualifiesForPreferentialDuty = "Q";
			invoiceLine.JI_PreferentialCountryGroup = ZString.Empty;
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_RN_NKCountryOfExport = "JP";
			Factory.Save();

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			FormalEntry.CusEntryHeader header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var im1Builder = new IM1MessageBuilder(wrapper, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IM1 Message - when blank preference group but preference chosen & there is only a single preference - value should be that single pref as per system best rate (in this case 'JP') - not Normal rate", true, im1MessageString.Contains(expectedResult));
		}

		public void TestHandlingInformationIsTrimmed()
		{
			var entryHeader = JobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew("GST", 500m);
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			entryHeader.EntryNumber = "97345213";

			var invalidNoteTxt = "awsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPO" +
				"EINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpin" +
				"ewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidf" +
				"owinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoer" +
				"ihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFN";

			JobDeclaration.Notes.AddNew(isCustomDescription: false, "Goods Handling Instructions", invalidNoteTxt);

			var header = (FormalEntry.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, JobDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);
			var wrappedImportHeader = (IImportDeclaration)wrapper;

			var trimmedNoteTxt = "awsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPO" +
				"EINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpin" +
				"ewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidf" +
				"owinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoer";

			AssertEquals("Handling Info should be trimmed", trimmedNoteTxt, wrappedImportHeader.HandlingInformation);
		}

		[TestDate(2013, 08, 01)]
		public void TestAggregateDeclaration()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			CreateImportSeaJob();
			var leadDeclaration = JobDeclaration;
			leadDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			leadDeclaration.Importer.OH_FullName = "imp123";
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "PORTS OF AUCKLAND";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "40174293L");    //ControlledPremisesID
			leadDeclaration.JE_OH_NotifyParty = notifyParty.PK;

			var notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.OH_FullName = "WINTERFELL";
			notifyParty2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "11111A");
			leadDeclaration.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty2.PK;
			leadDeclaration.JE_RL_NKPortOfDeliveryNotify = "DORNE";

			var seller1 = Factory.NewWithValidTestData<OrgAddress>();
			seller1.OA_Address1 = "Seller 1";
			var seller2 = Factory.NewWithValidTestData<OrgAddress>();
			seller2.OA_Address1 = "Seller 2";

			var invoice1 = leadDeclaration.Invoices[0];
			invoice1.JZ_OA_SellerAddress = seller1.PK;
			invoice1.InvoiceLines[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");

			var permit1 = leadDeclaration.PermitCodes.AddNew();
			permit1.ZO_Code = "POD";
			permit1.ZO_Data = "M-042384Y";

			jobDeclaration = null;
			CreateImportSeaJob();
			var anotherDeclaration = JobDeclaration;
			anotherDeclaration.WarehouseAddress.Header.OH_Code = "WAREHOUSE2";
			anotherDeclaration.JE_DeclarationReference = "BSIS/00002310";
			anotherDeclaration.JE_LocationOfGoods = "bbb";
			anotherDeclaration.JE_MasterBill = "11111111";
			anotherDeclaration.JE_HouseBill = "22222222";
			anotherDeclaration.CusContainers[0].CO_ContainerNumber = "container1";
			anotherDeclaration.CusContainers[1].CO_ContainerNumber = "container2";

			var invoice2 = anotherDeclaration.Invoices[0];
			invoice2.JZ_OA_SellerAddress = seller2.PK;
			invoice2.InvoiceLines[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 115m, "NZD");

			Factory.Save();
			leadDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			anotherDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			leadDeclaration.CusEntryHeader.MergedLines[0].GSTAmount = 100m;
			leadDeclaration.CusEntryHeader.EntryNumber = "97345213";
			anotherDeclaration.CusEntryHeader.MergedLines[0].GSTAmount = 200m;
			Factory.Save();

			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			leadDeclaration.JE_EntryStatus = anotherDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			consolidatedDeclaration.JobDeclarations.Add(leadDeclaration);
			consolidatedDeclaration.JobDeclarations.Add(anotherDeclaration);
			consolidatedDeclaration.CRD_JobReferenceNumber = "CRD123";
			leadDeclaration.DeclarationNumber = "Entry123";

			var header = (FormalEntry.CusEntryHeader)consolidatedDeclaration.BuildAggregateJobDeclaration().ActiveEntryHeaders[0];
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, leadDeclaration.Factory, "IM1");
			var wrapper = new CusEntryHeaderWrapper(header, additionalMessageInformation);

			CombineAssertions(() =>
			{
				var wrappedImportHeader = (IImportDeclaration)wrapper;
				AssertEquals("Master bills are exclueded for declarations with different MB", 0, wrappedImportHeader.MasterBills.Count());
				AssertContainsExactElementsInAnyOrder("House bills are aggregated", new[] { "HB92027", "22222222" }, wrappedImportHeader.AllBills.Select(bill => bill.BillNumber));
				AssertContainsExactElementsInAnyOrder("Containers are aggregated", new[] { "MNHU0029382", "MNHU0149961", "CONTAINER1", "CONTAINER2" }, wrappedImportHeader.Equipment.Select(e => e.ContainerNumber));
				// TestIImportDeclaration
				AssertNotNull("AdditionalInformation", wrappedImportHeader.AdditionalInformation);
				AssertEquals("SubmitterCode - should be padded to 9 characters", "00009908C", wrappedImportHeader.SubmitterCode);
				AssertEquals("BrokerCode - should be padded to 9 characters", "00009908C", wrappedImportHeader.BrokerCode);
				AssertNotNull("Carrier", wrappedImportHeader.Carrier);
				AssertEquals("CraftName", "HYOGO MARU", wrappedImportHeader.CraftName);
				AssertEquals("DateOfImport", new ZDateTime(2013, 08, 01), wrappedImportHeader.DateOfImport);
				AssertNotNull("Declarant", wrappedImportHeader.Declarant);
				AssertEquals("DepartureDate", new ZDateTime(2013, 07, 18), wrappedImportHeader.DepartureDate);
				AssertEquals("DutyTaxFees", 300m, wrappedImportHeader.DutyTaxFees.Where(fee => fee.DutyTaxFeeType == "GST").Sum(fee => fee.Amount));
				AssertNotNull("ExchangeRates", wrappedImportHeader.ExchangeRates);
				AssertNotNull("GoodsShipment", wrappedImportHeader.GoodsShipment);
				AssertEquals("HandlingInformation", "", wrappedImportHeader.HandlingInformation);
				AssertEquals("HasContainersOrPallets", true, wrappedImportHeader.HasContainersOrPallets);
				AssertEquals("Importer", "imp123", wrappedImportHeader.Importer.Name);
				AssertEquals("ImportPeriod", ZDateTime.Empty, wrappedImportHeader.ImportPeriod);
				AssertEquals("IsAir", false, wrappedImportHeader.IsAir);
				AssertEquals("IsCompletionEntry", false, wrappedImportHeader.IsCompletionEntry);
				AssertEquals("IsContainerised", true, wrappedImportHeader.IsContainerised);
				AssertEquals("IsMail", false, wrappedImportHeader.IsMail);
				AssertEquals("IsPeriodicImport", false, wrappedImportHeader.IsPeriodicImport);
				AssertEquals("IsSea", true, wrappedImportHeader.IsSea);
				AssertEquals("LloydsNo", "", wrappedImportHeader.LloydsNo);
				AssertEquals("MessageType", "I10", wrappedImportHeader.MessageType);
				AssertEquals("MPIAccountDetails", "", wrappedImportHeader.MPIAccountDetails);
				AssertNotNull("OtherInfoCodes", wrappedImportHeader.OtherInfoCodes);
				AssertEquals("Packaging", 4, wrappedImportHeader.Packaging.Count());
				AssertEquals("PaymentType", "B", wrappedImportHeader.PaymentType);
				AssertEquals("PremiseID", "", wrappedImportHeader.PremiseID);
				AssertEquals("PreviousDocumentNo", "", wrappedImportHeader.PreviousDocumentNo);
				AssertEquals("PreviousDocumentType", "", wrappedImportHeader.PreviousDocumentType);
				AssertEquals("SenderReferenceNumber", "CRD123", wrappedImportHeader.SenderReferenceNumber);
				AssertEquals("TotalGrossWeightInKGM", 30000m, wrappedImportHeader.TotalGrossWeightInKGM);
				AssertEquals("TotalGrossWeightUnit", "KGM", wrappedImportHeader.TotalGrossWeightUnit);
				AssertEquals("TransactionType", 9, wrappedImportHeader.TransactionType);
				AssertEquals("TSWReferenceNumber", "Entry123", wrappedImportHeader.TSWReferenceNumber);
				AssertEquals("LloydsNo", "", wrappedImportHeader.LloydsNo);
				AssertEquals("VoyageNo", "227W", wrappedImportHeader.VoyageNo);

				// TestDocumentsAndPermits
				AssertNotNull("Permits", wrappedImportHeader.Permits);
				int permitCount = 0;
				var permitDetail = ZString.Empty;
				foreach (ZString permit in wrappedImportHeader.Permits)
				{
					permitCount++;
					permitDetail = permit;
				}
				AssertEquals("Should be one permit details created in wrapped header", 1, permitCount);

				var wrappedShipment = (IGoodsShipment)wrapper;

				// TestIGoodsShipmentImport
				AssertNotNull("DeliverToParty", wrappedShipment.DeliverToParty);
				AssertEquals("FreightApportionmentMethod", "161", wrappedShipment.FreightApportionmentMethod);
				AssertEquals("FreightCostsInNZD", 500m, wrappedShipment.FreightCostsInNZD);
				AssertEquals("Invoices", 2, wrappedShipment.Invoices.Count());
				AssertEquals("Items", 2, wrappedShipment.Items.Count());
				AssertEquals("LocationOfGoods", "2975W", wrappedShipment.LocationOfGoods);
				AssertEquals("MAFContainerDeclaration", true, wrappedShipment.MAFContainerDeclaration);
				AssertEquals("NatureOfTransaction", "10", wrappedShipment.NatureOfTransaction);
				AssertNotNull("NotifyParties", wrappedShipment.NotifyParties);
				AssertEquals("PortOfDischarge", "NZAKL", wrappedShipment.PortOfDischarge);
				AssertEquals("PortOfLoading", "SGSIN", wrappedShipment.PortOfLoading);
				AssertNotNull("Sellers", wrappedShipment.Sellers);
				AssertEquals("ShipmentOrigin", "SG", wrappedShipment.ShipmentOrigin);
				AssertNotNull("StuffingEstablishments", wrappedShipment.StuffingEstablishments);
				AssertNotNull("Suppliers", wrappedShipment.Suppliers);
				AssertNotNull("NotifyParties", wrappedShipment.NotifyParties);

				// TestCustomsDeliveryAuthorityNotifyParty
				int notifyPartyCount = 0;
				foreach (IOrganisation shipmentNotifyParty in wrappedShipment.NotifyParties)
				{
					AssertNotNull("NotifyParty", shipmentNotifyParty);
					AssertEquals("Notify Party Name", "PORTS OF AUCKLAND", shipmentNotifyParty.Name);
					AssertEquals("Notify Party Premise ID", "40174293L", shipmentNotifyParty.CustomsClientCode);
					notifyPartyCount++;
				}

				AssertEquals("Should be 1 Notify Party org details", 1, notifyPartyCount);

				// TestNotifyPartyCodes
				AssertContainsExactElementsInAnyOrder("NotifyPartyCodes", new ZString[] { "11111A", "DORNE" }, wrappedShipment.NotifyPartyCodes);

				// TestMAFContainerStatements_WithData
				AssertContainsExactElementsInAnyOrder("MAFContainerStatement", new[] { "MNHU0029382,YNYNN", "MNHU0149961,YNYNN", "CONTAINER1,YNYNN", "CONTAINER2,YNYNN" }, wrappedShipment.MAFContainerStatements);

				// TestSellers
				AssertNotNull("Sellers", wrappedShipment.Sellers);
				int sellerCount = 0;
				foreach (IOrganisation seller in wrappedShipment.Sellers)
				{
					sellerCount++;
				}
				AssertEquals("Should be two seller details created in wrapped header", 2, sellerCount);
			});
		}

		#region Implementation

		JobDeclaration JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				}

				return jobDeclaration;
			}
		}
		JobDeclaration jobDeclaration;

		void CreateImportSeaJob()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "ANL SHIPPING";

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			JobDeclaration.JE_TransactionNature = NatureOfTransactionList.Codes.N10;
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "BSIS/00002309";
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "SGSIN";
			JobDeclaration.JE_VesselName = "Hyogo Maru";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			NZAddInfo addInfo = ((IHaveNZAddInfo)JobDeclaration).AddInfo;
			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
			var wareHouseOrg = Factory.New<OrgHeader>();
			wareHouseOrg.OH_Code = "WAREHOUSE";
			OrgCusCode supplierCode = wareHouseOrg.CustomsCodes.AddNew();
			supplierCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			supplierCode.OK_CountryDefault = true;
			supplierCode.OK_CustomsRegNo = "2975W";
			supplierCode.OK_RN_NKCodeCountry = "NZ";
			JobDeclaration.WarehouseDocAddress.E2_OA_Address = wareHouseOrg.MainAddress.PK;

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "MNHU0029382";
			container1.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc40Ft;
			container1.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C45;
			container1.CO_MPIApprovedSystemNumber = "75128";

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 2;
			packLine1.CW_PackType = "07";

			var container2 = JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "MNHU0149961";
			container2.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc40Ft;
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container2.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C45;
			container2.CO_MPIApprovedSystemNumber = "73492";

			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_HouseBill = packLine1.CW_HouseBill;
			packLine2.CW_ContainerNoOrEquipmentNo = "MNHU0149961";
			packLine2.CW_PackQty = 1;
			packLine2.CW_PackType = "PK";

			JobDeclaration.JE_SendMCDContainerQuarantineDeclaration = true;
			JobDeclaration.JE_HaveMAFContainerDeclaration = true;
			JobDeclaration.JE_IsContainerClean = true;
			JobDeclaration.JE_IsWoodPackagingUsed = true;

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
		}

		void CreateExportAirJob()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS";
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "QF108";
			JobDeclaration.JE_ExportDate = ZDateTime.Today;
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "BEX0042709";
			JobDeclaration.JE_GoodsDescription = "NEWS PAPER";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "08100342948";
			JobDeclaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PKT";
		}

		void CreateImportAirJob(Action<JobDeclaration> setDeclaration = null)
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS AIRFREIGHT";
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_TotalWeight = 150m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "QF108";
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2013, 7, 19);
			JobDeclaration.JE_DeclarationReference = "BIS00002309";
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 7, 19);
			JobDeclaration.JE_GoodsDescription = "NEWS PAPER";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "0810049584";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PCS";
			JobDeclaration.JE_TransactionNature = "";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 6, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			var packaging = invoiceLine.ItemPackages.AddNew();
			packaging.NZ_NumberOfPackages = 1;
			packaging.NZ_PackageUQ = "BX";
			packaging.NZ_PackageVolume = 0.5m;
			packaging.NZ_ShippingMarks = "Packaging Marks";

			setDeclaration?.Invoke(JobDeclaration);

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void Create2HB1ContainerExportJob()
		{
			JobDeclaration.JE_MessageType = "EXP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BS00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "LCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "OB93428378";
			JobDeclaration.JE_HouseBill = "H458239-1";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 150;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PLT";

			var houseBill2 = JobDeclaration.Bills.AddNew();
			houseBill2.CU_BillType = "HB";
			houseBill2.CU_BillNum = "B942042-2";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "YKKU9388747";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "LCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C40;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 97;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_PackQty = 53;
			packLine2.CW_PackType = "BX";
			packLine2.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2015, 1, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTwoContainerTwoPackageJob()
		{
			JobDeclaration.JE_MessageType = "EXP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "B00001573";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "293W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2014, 11, 19);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "J5492";
			JobDeclaration.JE_MasterBill = "OB85349";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "ADMIRALENGRACHT";
			JobDeclaration.JE_TotalNoOfPacks = 3;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PLT";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "HLMU0398470";
			container1.CO_ContainerSize = "23";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 6500m;
			container1.CO_WeightUQ = "KG";
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C23;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 2;
			packLine1.CW_PackType = "07";

			var container2 = JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "YKKU9348472";
			container2.CO_ContainerSize = "23";
			container2.CO_FCL_LCL_AIR = "FCL";
			container2.CO_Weight = 5950m;
			container2.CO_WeightUQ = "KG";
			container2.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C23;

			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_HouseBill = packLine1.CW_HouseBill;
			packLine2.CW_ContainerNoOrEquipmentNo = "YKKU9348472";
			packLine2.CW_PackQty = 1;
			packLine2.CW_PackType = "07";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2014, 10, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4016.99.99.29H";
			invoiceLine.JI_LinePrice = 2500m;

			Factory.Save();
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_VoyageFlightNo = "QF118";
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateContainerPackageJobWith2PackingPivots()
		{
			JobDeclaration.JE_MessageType = "EXP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "B00001573";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "293W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2014, 11, 19);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "J5492";
			JobDeclaration.JE_MasterBill = "OB85349";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "ADMIRALENGRACHT";
			JobDeclaration.JE_TotalNoOfPacks = 3;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PLT";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "HLMU0398470";
			container1.CO_ContainerSize = "23";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 6500m;
			container1.CO_WeightUQ = "KG";
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C23;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 2;
			packLine1.CW_PackType = "07";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2014, 10, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4016.99.99.29H";
			invoiceLine.JI_LinePrice = 2500m;
			Factory.Save();

			var mawbBillQuery = "select CU_PK from dbo.CusDecHouseBill where CU_BillNum = 'OB85349' and CU_BillType = 'MB'";
			Guid mawbCusDecHouseBillPK = (Guid)TestConnection.ExecuteScalar(mawbBillQuery);

			var packingGroup1 = JobDeclaration.PackingGroups[0];
			var packingGroup2 = JobDeclaration.PackingGroups.AddNew();
			packingGroup2.CR_CO_Container = container1.PK;
			packingGroup2.CR_CU_HouseBill = mawbCusDecHouseBillPK;
			Factory.Save();

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateExampleAJob()
		{
			JobDeclaration.JE_MessageType = "EXP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BA00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "COS12345678";
			JobDeclaration.JE_MasterBill = "61831905231";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CAXU2968920";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C40;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			var packaging = invoiceLine.ItemPackages.AddNew();
			packaging.NZ_NumberOfPackages = 1;
			packaging.NZ_PackageUQ = "BX";
			packaging.NZ_PackageVolume = 0.5m;
			packaging.NZ_ShippingMarks = Core.Constants.ContainerMarking.NoMarks;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateExampleDJob()
		{
			JobDeclaration.JE_MessageType = "EXP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BA00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "123456";
			JobDeclaration.JE_HouseBill = "COS12345678";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			var houseBill1 = JobDeclaration.Bills[1];
			var houseBill2 = JobDeclaration.Bills.AddNew();
			houseBill2.CU_BillType = "HB";
			houseBill2.CU_BillNum = "XYZ99887766";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CAXU2968920";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C40;

			var container2 = JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "UUXU99203930";
			container2.CO_ContainerSize = "40";
			container2.CO_FCL_LCL_AIR = "FCL";
			container2.CO_Weight = 1500m;
			container2.CO_WeightUQ = "KG";
			container2.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C40;

			var container3 = JobDeclaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "ZZUU9283929";
			container3.CO_ContainerSize = "40";
			container3.CO_FCL_LCL_AIR = "FCL";
			container3.CO_Weight = 1500m;
			container3.CO_WeightUQ = "KG";
			container3.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C40;

			var container4 = JobDeclaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "ZZXU2968920";
			container4.CO_ContainerSize = "40";
			container4.CO_FCL_LCL_AIR = "FCL";
			container4.CO_Weight = 1500m;
			container4.CO_WeightUQ = "KG";
			container4.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C40;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_PackQty = 8;
			packLine2.CW_PackType = "BX";
			packLine2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;

			var packLine3 = JobDeclaration.Packages.AddNew();
			packLine3.CW_HouseBill = houseBill2.CU_BillNum;
			packLine3.CW_PackQty = 8;
			packLine3.CW_PackType = "BX";
			packLine3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;

			var packLine4 = JobDeclaration.Packages.AddNew();
			packLine4.CW_HouseBill = houseBill2.CU_BillNum;
			packLine4.CW_PackQty = 5;
			packLine4.CW_PackType = "BX";
			packLine4.CW_ContainerNoOrEquipmentNo = container4.CO_ContainerNumber;

			var packingGroup1 = JobDeclaration.PackingGroups[0];
			packingGroup1.CR_CU_HouseBill = houseBill1.PK;

			var packingGroup2 = JobDeclaration.PackingGroups[1];
			packingGroup2.CR_CU_HouseBill = houseBill1.PK;

			var packingGroup3 = JobDeclaration.PackingGroups[2];
			packingGroup3.CR_CU_HouseBill = houseBill2.PK;

			var packingGroup4 = JobDeclaration.PackingGroups[3];
			packingGroup4.CR_CU_HouseBill = houseBill2.PK;

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateExampleEJob()
		{
			JobDeclaration.JE_MessageType = "EXP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BE00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 150m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = ZDateTime.Today;
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "MB1";
			JobDeclaration.JE_HouseBill = "61831905231";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "P1";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C16;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 6, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 1000m;
			var packaging = invoiceLine.ItemPackages.AddNew();
			packaging.NZ_NumberOfPackages = 1;
			packaging.NZ_PackageUQ = "BX";
			packaging.NZ_PackageVolume = 0.5m;
			packaging.NZ_ShippingMarks = Core.Constants.ContainerMarking.NoMarks;

			var packaging2 = invoiceLine.ItemPackages.AddNew();
			packaging2.NZ_NumberOfPackages = 25;
			packaging2.NZ_PackageUQ = "CT";
			packaging2.NZ_PackageVolume = 4m;
			packaging2.NZ_ShippingMarks = "U994382/I47, J049428/732";

			Factory.Save();
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_VoyageFlightNo = "QF118";
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateExampleFJob()
		{
			JobDeclaration.JE_MessageType = "IMP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BIS00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-7);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "MB1";
			JobDeclaration.JE_HouseBill = "BILL123456";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "ABCU1234560";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C21;

			var houseBill = JobDeclaration.PrimaryHouseBill;
			var packLine1 = houseBill.PackingGroups[0].Packages[0];
			packLine1.CW_PackQty = 1;
			packLine1.CW_PackType = "VN";
			packLine1.CW_ContainerNoOrEquipmentNo = "";

			var packLine2 = houseBill.PackingGroups[0].Packages.AddNew();
			packLine2.CW_PackQty = 10;
			packLine2.CW_PackType = "PK";
			packLine2.CW_ContainerNoOrEquipmentNo = "ABCU1234560";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = ZDateTime.Today.AddDays(-20);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			var packaging = invoiceLine.ItemPackages.AddNew();
			packaging.NZ_NumberOfPackages = 3;
			packaging.NZ_PackageUQ = "CT";
			packaging.NZ_PackageVolume = 2m;
			packaging.NZ_ShippingMarks = "Y-399487/TR7";

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateExampleGJob()
		{
			JobDeclaration.JE_MessageType = "EXP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BS00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "LCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "123456";
			JobDeclaration.JE_HouseBill = "COS12345678";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 150;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PLT";

			var houseBill2 = JobDeclaration.Bills.AddNew();
			houseBill2.CU_BillType = "HB";
			houseBill2.CU_BillNum = "XYZ99887766";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CAXU2968920";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "LCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C23;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_PackQty = 8;
			packLine2.CW_PackType = "BX";
			packLine2.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2015, 1, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		JobDeclaration CreateCompletionJob()
		{
			var exportDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS AIRFREIGHT";
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			exportDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			exportDeclaration.JE_OriginalEntryNumber = "75328491";
			exportDeclaration.JE_TransportMode = "AIR";
			exportDeclaration.JE_TotalWeight = 150m;
			exportDeclaration.JE_TotalWeightUnit = "KG";
			exportDeclaration.JE_VoyageFlightNo = "QF108";
			exportDeclaration.JE_DateOfArrival = new ZDateTime(2013, 7, 19);
			exportDeclaration.JE_DeclarationReference = "BIS00002309";
			exportDeclaration.JE_ExportDate = new ZDateTime(2013, 7, 19);
			exportDeclaration.JE_GoodsDescription = "NEWS PAPER";
			exportDeclaration.JE_GS_NKCusAgent = "JKS";
			exportDeclaration.JE_HouseBill = "HB92027";
			exportDeclaration.JE_MasterBill = "0810049584";
			exportDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			exportDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			exportDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			exportDeclaration.JE_RL_NKOrigin = "AUSYD";
			exportDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			exportDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			exportDeclaration.JE_TotalNoOfPacks = 15;
			exportDeclaration.JE_TotalNoOfPacksPackType = "PCS";

			var invoice = exportDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 6, 22);

			var invoiceLine = exportDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;

			Factory.Save();
			exportDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return exportDeclaration;
		}

		void CreateExportDrawbackJob()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BSES002932";
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "SGSIN";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_LevyCreditAmountCode = LevyCodesList.Codes.ALAC;
			invoiceLine.JI_LevyCreditAmount = 100M;

			var invoiceLine2 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0203.11.00.02C";
			invoiceLine2.JI_LinePrice = 900m;
			invoiceLine2.JI_LevyCreditAmountCode = LevyCodesList.Codes.ACC;
			invoiceLine2.JI_LevyCreditAmount = 200M;

			var invoiceLine3 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2301.10.00.01A";
			invoiceLine3.JI_LinePrice = 2000m;
			invoiceLine3.JI_LevyCreditAmountCode = LevyCodesList.Codes.HERA;
			invoiceLine3.JI_LevyCreditAmount = 300M;

			var invoiceLine4 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2701.11.00.00C";
			invoiceLine4.JI_LinePrice = 3000m;
			invoiceLine4.JI_LevyCreditAmountCode = LevyCodesList.Codes.PFML;
			invoiceLine4.JI_LevyCreditAmount = 400M;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateExportAirForeignCurrencyJob()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS";
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BSES002932";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			JobDeclaration.JE_TotalWeight = 15.356m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "QF108";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = ZDateTime.Today;
			JobDeclaration.JE_GoodsDescription = "MAGAZINES";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "08100342948";
			JobDeclaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CT";

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "Supplier Co-Op Pty Ltd";

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_FullName = "Jones Supplier Pty Ltd";

			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_OH_Supplier = supplier1.PK;

			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 2000m;
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			invoice2.JZ_InvoiceCurrExRate = invoice2.JZ_InvoiceCurrExRate + 0.02m;
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_OH_Supplier = supplier2.PK;

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 1000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_Description = "Invoice 1 / Line 1";

			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 2000m;
			line2.JI_Tariff = "4201.00.00.01B";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_Description = "Invoice 2 / Line 1";

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		#endregion
	}
}
