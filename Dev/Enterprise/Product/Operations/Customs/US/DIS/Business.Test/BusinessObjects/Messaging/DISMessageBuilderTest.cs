using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISMessageBuilderTest : TestCaseWithFactory
	{
		public void TestExportDIS()
		{
			var obj = GetExportMessage();
			var messageBuilder = new DISMessageBuilder(obj);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();
			AssertContains("Export messages", @"      <DIS:TradeTransaction>
        <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
        <DIS:ITN>
          <DIS:ITN>7984328</DIS:ITN>
          <DIS:ReferenceNumber>123456789</DIS:ReferenceNumber>
        </DIS:ITN>
      </DIS:TradeTransaction>", ediMessage.EM_MessageText);
		}

		public void TestFTZAdmissionNumberWithoutPipelineDIS()
		{
			var tradeTransction = new Mock<IDISTradeTransaction>();
			tradeTransction.Setup(m => m.Number).Returns(new ZString("4011231A1|21|TST00270"));
			tradeTransction.Setup(m => m.Type).Returns(TradeTransactionType.FTZAdmission);

			var document = new Mock<IDISDocument>();
			document.Setup(m => m.DocumentID).Returns(new ZString("DocumentID-1"));
			document.Setup(m => m.DocumentLabelUSDISDocCode).Returns(new ZString("FTZDesc"));
			document.Setup(m => m.CompleteFileName).Returns(new ZString("FTZ.docx"));
			document.Setup(m => m.DocumentDescription).Returns(new ZString("DocumentDescription-1"));
			document.Setup(m => m.PreviouslySubmitted).Returns(ZBool.False);
			document.Setup(m => m.PGAs).Returns(new ZString[4] { PGAList.Codes.CBP, PGAList.Codes.AMS, PGAList.Codes.APH, "XXX" });
			document.Setup(m => m.Comment).Returns(new ZString("commentÉ-1"));
			document.Setup(m => m.CommodityData).Returns(System.Array.Empty<IDISCommodityLine>());
			document.Setup(m => m.AdditionalData).Returns(System.Array.Empty<IDISAdditionalData>());
			document.Setup(m => m.eDocsDocumentPK).Returns(new ZGuid("95E977EA-863B-4D09-B981-258A50B14C19"));
			document.Setup(m => m.PortCode).Returns(new ZString("3901"));
			document.Setup(m => m.PreparerID).Returns(new ZString("XJ4"));
			document.Setup(m => m.PreparerSiteCode).Returns(new ZString("3902"));
			document.Setup(m => m.TradeTransactions).Returns(new IDISTradeTransaction[] { tradeTransction.Object });
			document.Setup(m => m.TransactionCategory).Returns(TransactionCategory.SingleTransaction);
			document.Setup(m => m.DocumentSentDateEST).Returns(ZDateTime.BrettsBirthday);
			document.Setup(m => m.ActionCodeForSubmission).Returns(new ZString(ActionCodeList.Codes.Add));
			document.Setup(m => m.CreateMessage(It.IsAny<string>())).Returns(Factory.New<EDIMessage>());

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();
			AssertContains(ExpectedResultForTradeTransaction, ediMessage.EM_MessageText);
			tradeTransction.VerifyAll();
			document.VerifyAll();
		}

		[TestTimeZoneUNLOCO("USLAX")] // https://www.timeanddate.com/time/zone/usa/los-angeles
		[TestDate(2012, 01, 11)]
		public void TestDocumentWithdrawal()
		{
			TestDateAttribute.UseUNLOCO = true;
			AssertEquals("Precondition to verify timezone is being applied", "2012-01-10T16:00:00", ZDateTime.Now.ToISO8601String());

			var obj = GetMessageDataForWithdrawal();

			var messageBuilder = new DISMessageBuilder(obj);
			var ediMessage = messageBuilder.BuildDocumentWithdrawalUsingDocSubmissionPackage();
			AssertContains("Withdrawal messages", @"<DIS:MessageBody>
    <DIS:DocumentSubmissionPackage>
      <DIS:SubmittedToPortCode>3901</DIS:SubmittedToPortCode>
      <DIS:ActionCode>DELETE</DIS:ActionCode>
      <DIS:TradeTransaction>
        <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
        <DIS:Entry>
          <DIS:EntryNumber>7984328</DIS:EntryNumber>
          <DIS:Filer>XJ5</DIS:Filer>
          <DIS:ReferenceNumber>123456789</DIS:ReferenceNumber>
        </DIS:Entry>
      </DIS:TradeTransaction>
      <DIS:DocumentData>
        <DIS:DocumentHeader>
          <DIS:DocumentID>DOCUMENTID-1</DIS:DocumentID>
          <DIS:DocumentLabel>LABELDESC</DIS:DocumentLabel>
          <DIS:CompleteFileName>SSS.DOCX</DIS:CompleteFileName>
          <DIS:FileExtensionType>DOCX</DIS:FileExtensionType>
          <DIS:DocumentDescription>DOCUMENTDESCRIPTION-1</DIS:DocumentDescription>
          <DIS:DocPreviouslySubmitted>N</DIS:DocPreviouslySubmitted>
        </DIS:DocumentHeader>
        <DIS:GovtAgencyList>
          <DIS:GovtAgency>CBP</DIS:GovtAgency>
          <DIS:GovtAgency>AMS</DIS:GovtAgency>
          <DIS:GovtAgency>APH</DIS:GovtAgency>
          <DIS:GovtAgency>XXX</DIS:GovtAgency>
        </DIS:GovtAgencyList>
      </DIS:DocumentData>
    </DIS:DocumentSubmissionPackage>
  </DIS:MessageBody>", ediMessage.EM_MessageText);

			AssertContains("MessageNumberPlaceHolder in EM_MessageText", EDIMessage.DISMessageNumberPlaceHolder, ediMessage.EM_MessageText);

			Factory.Save();

			AssertContains("MessageNumberPlaceHolder in EM_MessageText is replaced", ediMessage.EM_MessageNum, ediMessage.EM_MessageText);
			AssertNotContains("MessageNumberPlaceHolder in EM_MessageText is replaced", EDIMessage.DISMessageNumberPlaceHolder, ediMessage.EM_MessageText);
		}

		[TestTimeZoneUNLOCO("USLAX")] // https://www.timeanddate.com/time/zone/usa/los-angeles
		[TestDate(2012, 01, 11)]
		public void TestDocumentSubmissionPackage()
		{
			TestDateAttribute.UseUNLOCO = true;
			AssertEquals("Precondition to verify timezone is being applied", "2012-01-10T16:00:00", ZDateTime.Now.ToISO8601String());

			var obj = GetMessageData();
			var messageBuilder = new DISMessageBuilder(obj);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("SubmittedPortCode", "<DIS:SubmittedToPortCode>123456</DIS:SubmittedToPortCode>", ediMessage.EM_MessageText);
			AssertContains("ActionCode", "<DIS:ActionCode>ADD</DIS:ActionCode>", ediMessage.EM_MessageText);

			AssertContains("TradeTransactions", @"      <DIS:TradeTransaction>
        <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
        <DIS:Entry>
          <DIS:EntryNumber>7984328</DIS:EntryNumber>
          <DIS:Filer>XJ5</DIS:Filer>
          <DIS:ReferenceNumber>123456789</DIS:ReferenceNumber>
        </DIS:Entry>
      </DIS:TradeTransaction>", ediMessage.EM_MessageText);

			AssertContains("CBP Request", @"<DIS:CBPRequest>
        <DIS:CBPRequestID>8D5228EE-B9C1-4681-AB7F-A3B0D8E582B7</DIS:CBPRequestID>
        <DIS:CBPRequestType>OTHERCBPREQUEST</DIS:CBPRequestType>
      </DIS:CBPRequest>", ediMessage.EM_MessageText);

			AssertContains("DocumentHeader", @"<DIS:DocumentHeader>
          <DIS:DocumentID>DOCUMENTID-1</DIS:DocumentID>
          <DIS:DocumentLabel>LABELDESC</DIS:DocumentLabel>
          <DIS:CompleteFileName>SSS.DOCX</DIS:CompleteFileName>
          <DIS:FileExtensionType>DOCX</DIS:FileExtensionType>
          <DIS:DocumentDescription>DOCUMENTDESCRIPTION-1</DIS:DocumentDescription>
          <DIS:DocPreviouslySubmitted>N</DIS:DocPreviouslySubmitted>
        </DIS:DocumentHeader>", ediMessage.EM_MessageText);

			AssertContains("GovtAgencyList", @"<DIS:GovtAgencyList>
          <DIS:GovtAgency>CBP</DIS:GovtAgency>
          <DIS:GovtAgency>AMS</DIS:GovtAgency>
          <DIS:GovtAgency>APH</DIS:GovtAgency>
          <DIS:GovtAgency>XXX</DIS:GovtAgency>
        </DIS:GovtAgencyList>", ediMessage.EM_MessageText);

			AssertContains("comment", @"<DIS:Comment>COMMENT*-1</DIS:Comment>", ediMessage.EM_MessageText);

			AssertContains("invoiceData", @"<DIS:InvoiceData>
            <DIS:InvoiceNbr>INVOICENO1</DIS:InvoiceNbr>
            <DIS:InvoiceType>COMMERCIAL_INVOICE</DIS:InvoiceType>
            <DIS:InvoiceLineItemData>
              <DIS:InvoiceLineNbr>12</DIS:InvoiceLineNbr>
              <DIS:CommodityData>
                <DIS:EntryLineNumber>1</DIS:EntryLineNumber>
                <DIS:HTSNumber>HTSNO-1</DIS:HTSNumber>
                <DIS:CommodityDescription>COMMLINEDESC</DIS:CommodityDescription>
                <DIS:CountryOfOrigin>AUS</DIS:CountryOfOrigin>
                <DIS:ContainerNbr>CONNUM1</DIS:ContainerNbr>
                <DIS:PortOfLading>PORTOFLOADING-1</DIS:PortOfLading>
                <DIS:PortOfUnlading>PORTOFUNLADING1</DIS:PortOfUnlading>
                <DIS:PortOfEntry>PORTENTRY-1</DIS:PortOfEntry>
                <DIS:SealNumbers>SELANO-1</DIS:SealNumbers>
                <DIS:TradeParties>
                  <DIS:TradeParty>
                    <DIS:TradePartyID>ID1</DIS:TradePartyID>
                    <DIS:TradePartyType>EXPORTER</DIS:TradePartyType>
                    <DIS:TradePartyName>NAME1</DIS:TradePartyName>
                    <DIS:TradePartyAddress>ADD1</DIS:TradePartyAddress>
                  </DIS:TradeParty>
                  <DIS:TradeParty>
                    <DIS:TradePartyID>ID2</DIS:TradePartyID>
                    <DIS:TradePartyType>IMPORTER</DIS:TradePartyType>
                    <DIS:TradePartyName>NAME2</DIS:TradePartyName>
                    <DIS:TradePartyAddress>ADD2</DIS:TradePartyAddress>
                  </DIS:TradeParty>
                </DIS:TradeParties>
                <DIS:VehicleAndEngineData>
                  <DIS:VIN>VIN1</DIS:VIN>
                  <DIS:VehicleManufacturer>MANUFACTURER1</DIS:VehicleManufacturer>
                  <DIS:VehicleModel>MODEL1</DIS:VehicleModel>
                  <DIS:VehicleSerialNumber>SERIALNU1</DIS:VehicleSerialNumber>
                  <DIS:EngineManufacturer>ENGINEMANUFACTURER1</DIS:EngineManufacturer>
                  <DIS:EngineModel>ENGINEMODEL1</DIS:EngineModel>
                  <DIS:EngineSerialNumber>ENGINESERIALNUMBER1</DIS:EngineSerialNumber>
                </DIS:VehicleAndEngineData>
              </DIS:CommodityData>
            </DIS:InvoiceLineItemData>
            <DIS:InvoiceLineItemData>
              <DIS:InvoiceLineNbr>13</DIS:InvoiceLineNbr>
              <DIS:CommodityData>
                <DIS:EntryLineNumber>1</DIS:EntryLineNumber>
                <DIS:HTSNumber>HTSNO-1</DIS:HTSNumber>
                <DIS:CommodityDescription>COMMLINEDESC</DIS:CommodityDescription>
                <DIS:CountryOfOrigin>AUS</DIS:CountryOfOrigin>
                <DIS:ContainerNbr>CONNUM1</DIS:ContainerNbr>
                <DIS:PortOfLading>PORTOFLOADING-1</DIS:PortOfLading>
                <DIS:PortOfUnlading>PORTOFUNLADING1</DIS:PortOfUnlading>
                <DIS:PortOfEntry>PORTENTRY-1</DIS:PortOfEntry>
                <DIS:SealNumbers>SELANO-1</DIS:SealNumbers>
                <DIS:TradeParties>
                  <DIS:TradeParty>
                    <DIS:TradePartyID>ID1</DIS:TradePartyID>
                    <DIS:TradePartyType>EXPORTER</DIS:TradePartyType>
                    <DIS:TradePartyName>NAME1</DIS:TradePartyName>
                    <DIS:TradePartyAddress>ADD1</DIS:TradePartyAddress>
                  </DIS:TradeParty>
                  <DIS:TradeParty>
                    <DIS:TradePartyID>ID2</DIS:TradePartyID>
                    <DIS:TradePartyType>IMPORTER</DIS:TradePartyType>
                    <DIS:TradePartyName>NAME2</DIS:TradePartyName>
                    <DIS:TradePartyAddress>ADD2</DIS:TradePartyAddress>
                  </DIS:TradeParty>
                </DIS:TradeParties>
                <DIS:VehicleAndEngineData>
                  <DIS:VIN>VIN1</DIS:VIN>
                  <DIS:VehicleManufacturer>MANUFACTURER1</DIS:VehicleManufacturer>
                  <DIS:VehicleModel>MODEL1</DIS:VehicleModel>
                  <DIS:VehicleSerialNumber>SERIALNU1</DIS:VehicleSerialNumber>
                  <DIS:EngineManufacturer>ENGINEMANUFACTURER1</DIS:EngineManufacturer>
                  <DIS:EngineModel>ENGINEMODEL1</DIS:EngineModel>
                  <DIS:EngineSerialNumber>ENGINESERIALNUMBER1</DIS:EngineSerialNumber>
                </DIS:VehicleAndEngineData>
              </DIS:CommodityData>
            </DIS:InvoiceLineItemData>
          </DIS:InvoiceData>", ediMessage.EM_MessageText);

			AssertContains("BondData", @"<DIS:BondData>
            <DIS:BondName>SINGLE_TXN_BOND</DIS:BondName>
            <DIS:BondNumber>BOND NO-1</DIS:BondNumber>
            <DIS:BondType>BOND-TYPE</DIS:BondType>
            <DIS:SuretyCode>SURETY CODE</DIS:SuretyCode>
            <DIS:AgentIDNbr>0F1968EF-CD68-45B3-9D47-DC4AA3B8B88B</DIS:AgentIDNbr>
            <DIS:Filer>FILER1</DIS:Filer>
            <DIS:BondAmount>45.00</DIS:BondAmount>
          </DIS:BondData>", ediMessage.EM_MessageText);

			AssertContains("PackingList", @"<DIS:PackingListData>
            <DIS:PackingListNbr>PACKINGLISTNUMBER1</DIS:PackingListNbr>
            <DIS:InvoiceNumber>INVOICENUMBER1</DIS:InvoiceNumber>
            <DIS:PurchaseOrderNbr>PURCHASEORDERNUMBER1</DIS:PurchaseOrderNbr>
          </DIS:PackingListData>", ediMessage.EM_MessageText);

			AssertContains("CertificateData (before time zone)", @"<DIS:CertificateData>
            <DIS:CertificateNumber>NUM-1</DIS:CertificateNumber>
            <DIS:CertificateType>TYPE</DIS:CertificateType>
            <DIS:Statement>STATEMENT-1</DIS:Statement>
            <DIS:ExpirationDate>2012-12-12T00:00:00", ediMessage.EM_MessageText);

			AssertContains("CertificateData (after time zone)", @"</DIS:ExpirationDate>
            <DIS:ImporterOfRecord>IMPORTER</DIS:ImporterOfRecord>
            <DIS:InspectionLocation>INSPECTION LOCATION</DIS:InspectionLocation>
          </DIS:CertificateData>", ediMessage.EM_MessageText);

			AssertContains("PermitData", @"<DIS:PermitData>
            <DIS:PermitNumber>NUMBER 1</DIS:PermitNumber>
            <DIS:PermitType>TYPE</DIS:PermitType>
            <DIS:ApprovalNumber>APP NUM</DIS:ApprovalNumber>
            <DIS:Statement>STATE</DIS:Statement>
            <DIS:ImporterOfRecord>IMPORTER RECORD</DIS:ImporterOfRecord>
          </DIS:PermitData>", ediMessage.EM_MessageText);

			AssertContains("ToxicSubstance", @"<DIS:ToxicSubstancesData>
            <DIS:CASNbr>CASNUM</DIS:CASNbr>
            <DIS:EPARegistrationNbr>EPAREGONUMBER</DIS:EPARegistrationNbr>
            <DIS:EPAProducerEstNbr>EPAPRODUCERESTNUMBER</DIS:EPAProducerEstNbr>
          </DIS:ToxicSubstancesData>", ediMessage.EM_MessageText);

			AssertContains("AdditionalData", @"<DIS:AdditionalData>
            <DIS:NameValuePair>
              <DIS:Name>ISSUE_DATE</DIS:Name>
              <DIS:Value>2011-11-11T00:00:00</DIS:Value>
            </DIS:NameValuePair>
            <DIS:NameValuePair>
              <DIS:Name>GROSS_TONNAGE</DIS:Name>
              <DIS:Value>100</DIS:Value>
            </DIS:NameValuePair>
            <DIS:NameValuePair>
              <DIS:Name>NET_TONNAGE</DIS:Name>
              <DIS:Value>90</DIS:Value>
            </DIS:NameValuePair>
            <DIS:NameValuePair>
              <DIS:Name>FIELDNA*ME1</DIS:Name>
              <DIS:Value>VA*LUE1</DIS:Value>
            </DIS:NameValuePair>
            <DIS:NameValuePair>
              <DIS:Name>F*IELDNAME2</DIS:Name>
              <DIS:Value>V*ALUE2</DIS:Value>
            </DIS:NameValuePair>
          </DIS:AdditionalData>", ediMessage.EM_MessageText);

			AssertContains("PackageIdentifierData", @"      <DIS:PackageIdentifier>
        <DIS:PackageCategory>CBMA</DIS:PackageCategory>
        <DIS:ImporterOfRecordNbr>0123456789</DIS:ImporterOfRecordNbr>
      </DIS:PackageIdentifier>", ediMessage.EM_MessageText);

			AssertContains("DocumentObject", @"<DIS:DocumentObject>!dOcUmEnTiMaGePlAcEHoLdEr:{0}!</DIS:DocumentObject>", ediMessage.EM_MessageText);
			AssertContains("MessageNumberPlaceHolder in EM_MessageText", EDIMessage.DISMessageNumberPlaceHolder, ediMessage.EM_MessageText);

			Factory.Save();

			AssertContains("MessageNumberPlaceHolder in EM_MessageText is replaced", ediMessage.EM_MessageNum, ediMessage.EM_MessageText);
			AssertNotContains("MessageNumberPlaceHolder in EM_MessageText is replaced", EDIMessage.DISMessageNumberPlaceHolder, ediMessage.EM_MessageText);
		}

		[TestTimeZoneUNLOCO("USLAX")] // https://www.timeanddate.com/time/zone/usa/los-angeles
		[TestDate(2012, 01, 11)]
		public void TestDocumentSubmissionPackageOptionalData()
		{
			TestDateAttribute.UseUNLOCO = true;
			AssertEquals("Precondition to verify timezone is being applied", "2012-01-10T16:00:00", ZDateTime.Now.ToISO8601String());

			CombineAssertions(() =>
			{
				AssertOptionalInvoiceData();
				AssertOptionalBondData();
				AssertOptionalPackingListData();
				AssertOptionalCertificateData();
				AssertOptionalPermitData();
				AssertOptionalToxicSubstancesData();
				AssertOptionalCommodityData();
				AssertOptionalAdditionalData();
				AssertOptionalPackageIdentifier();
			});
		}

		public void TestDocumentObjectShouldNotBeSentIfNoDocIsSelected()
		{
			var mockObject = GetMessageDataMock();
			mockObject.Setup(m => m.eDocsDocumentPK).Returns(ZGuid.Empty);

			var messageBuilder = new DISMessageBuilder(mockObject.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();
			AssertNotContains("DocumentObject should not exist to get a rejection from Customs. Users have ignored a message error", @"<DIS:DocumentObject>!dOcUmEnTiMaGePlAcEHoLdEr:{0}!</DIS:DocumentObject>", ediMessage.EM_MessageText);
		}

		IDISDocument GetMessageData()
		{
			return GetMessageDataMock().Object;
		}

		Mock<IDISDocument> GetMessageDataMock()
		{
			var tradeParty1 = new Mock<IDISTradeParty>();
			tradeParty1.Setup(m => m.Address).Returns(new ZString("add1"));
			tradeParty1.Setup(m => m.ID).Returns(new ZString("ID1"));
			tradeParty1.Setup(m => m.Name).Returns(new ZString("name1"));
			tradeParty1.Setup(m => m.Type).Returns(DISTradePartyType.Exporter);

			var tradeParty2 = new Mock<IDISTradeParty>();
			tradeParty2.Setup(m => m.Address).Returns(new ZString("add2"));
			tradeParty2.Setup(m => m.ID).Returns(new ZString("ID2"));
			tradeParty2.Setup(m => m.Name).Returns(new ZString("name2"));
			tradeParty2.Setup(m => m.Type).Returns(DISTradePartyType.Importer);

			var tradeParties = (IEnumerable<IDISTradeParty>)new IDISTradeParty[] { tradeParty1.Object, tradeParty2.Object };

			var vehicleAndEngineData = new Mock<IDISVehicleAndEngineData>();
			vehicleAndEngineData.Setup(m => m.EngineManufactureDate).Returns(new ZDateTime(10, 11, 12));
			vehicleAndEngineData.Setup(m => m.EngineManufacturer).Returns(new ZString("engineManufacturer1"));
			vehicleAndEngineData.Setup(m => m.EngineModel).Returns(new ZString("engineModel1"));
			vehicleAndEngineData.Setup(m => m.EngineSerialNumber).Returns(new ZString("engineSerialNumber1"));
			vehicleAndEngineData.Setup(m => m.Manufacturer).Returns(new ZString("manufacturer1"));
			vehicleAndEngineData.Setup(m => m.Model).Returns(new ZString("model1"));
			vehicleAndEngineData.Setup(m => m.SerialNumber).Returns(new ZString("serialNu1"));
			vehicleAndEngineData.Setup(m => m.VIN).Returns(new ZString("vin1"));
			vehicleAndEngineData.Setup(m => m.VNELineNumber).Returns(new ZInt(1));
			vehicleAndEngineData.Setup(m => m.ManufactureYear).Returns(new ZString("1998"));
			vehicleAndEngineData.Setup(m => m.ManufactureMonth).Returns(new ZString("1"));

			var commodityLine = new Mock<IDISCommodityLine>();
			commodityLine.Setup(m => m.ArrivalDate).Returns(new ZDateTime(12, 10, 10));
			commodityLine.Setup(m => m.CommodityDescription).Returns(new ZString("commLineDesc"));
			commodityLine.Setup(m => m.ContainerNumber).Returns(new ZString("conNum1"));
			commodityLine.Setup(m => m.CountryOfOrigin).Returns(new ZString("Aus"));
			commodityLine.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			commodityLine.Setup(m => m.HTSNumber).Returns(new ZString("HTSNo-1"));
			commodityLine.Setup(m => m.PortOfEntry).Returns(new ZString("PortEntry-1"));
			commodityLine.Setup(m => m.PortOfLoading).Returns(new ZString("PortOfLoading-1"));
			commodityLine.Setup(m => m.PortOfUnlading).Returns(new ZString("PortOfUnlading1"));
			commodityLine.Setup(m => m.SealNumber).Returns(new ZString("SelaNo-1"));
			commodityLine.Setup(m => m.TradeParties).Returns(tradeParties);
			commodityLine.Setup(m => m.VehicleData).Returns(vehicleAndEngineData.Object);

			var commodityLine2 = new Mock<IDISCommodityLine>();
			commodityLine2.Setup(m => m.ArrivalDate).Returns(new ZDateTime(12, 10, 10));
			commodityLine2.Setup(m => m.CommodityDescription).Returns(new ZString("commLineDesc2"));
			commodityLine2.Setup(m => m.ContainerNumber).Returns(new ZString("conNum2"));
			commodityLine2.Setup(m => m.CountryOfOrigin).Returns(new ZString("Aus"));
			commodityLine2.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			commodityLine2.Setup(m => m.HTSNumber).Returns(new ZString("HTSNo-2"));
			commodityLine2.Setup(m => m.PortOfEntry).Returns(new ZString("PortEntry-2"));
			commodityLine2.Setup(m => m.PortOfLoading).Returns(new ZString("portoflanding-2"));
			commodityLine2.Setup(m => m.PortOfUnlading).Returns(new ZString("PortOfUnlading2"));
			commodityLine2.Setup(m => m.SealNumber).Returns(new ZString("SelaNo-2"));
			commodityLine2.Setup(m => m.TradeParties).Returns(tradeParties);
			commodityLine2.Setup(m => m.VehicleData).Returns(vehicleAndEngineData.Object);

			var invoiceLine1 = new Mock<IDISInvoiceLine>();
			invoiceLine1.Setup(m => m.CommodityDetails).Returns(commodityLine.Object);
			invoiceLine1.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(12));

			var invoiceLine2 = new Mock<IDISInvoiceLine>();
			invoiceLine2.Setup(m => m.CommodityDetails).Returns(commodityLine.Object);
			invoiceLine2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(13));

			var invoice = new Mock<IDISInvoice>();
			invoice.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLine[] { invoiceLine1.Object, invoiceLine2.Object });
			invoice.Setup(m => m.InvoiceNumber).Returns(new ZString("invoiceNo1"));
			invoice.Setup(m => m.InvoiceType).Returns(DISInvoiceType.CommercialInvoice);

			ZString portCode = "123456";
			ZString actionCode = "DEL";
			ZString tradeTransactionCategory = "SINGLE_TXN";

			var cBPRequest = new Mock<IDISCBPRequest>();
			cBPRequest.Setup(m => m.ID).Returns(new ZString("8D5228EE-B9C1-4681-AB7F-A3B0D8E582B7"));
			cBPRequest.Setup(m => m.RequestDate).Returns(new ZDateTime(13, 10, 01));
			cBPRequest.Setup(m => m.Type).Returns(new ZString("ACEActionNumber"));

			var bondData = new Mock<IDISBondData>();
			bondData.Setup(m => m.AgentIDNumber).Returns(new ZString("0F1968EF-CD68-45B3-9D47-DC4AA3B8B88B"));
			bondData.Setup(m => m.BondAmount).Returns(new ZDecimal(45));
			bondData.Setup(m => m.BondName).Returns(BondNameType.Single);
			bondData.Setup(m => m.BondNumber).Returns(new ZString("bond no-1"));
			bondData.Setup(m => m.BondType).Returns(new ZString("bond-type"));
			bondData.Setup(m => m.Filer).Returns(new ZString("filer1"));
			bondData.Setup(m => m.SuretyCode).Returns(new ZString("surety code"));

			var packingList = new Mock<IDISPackingList>();
			packingList.Setup(m => m.PackingListNumber).Returns(new ZString("PackingListNumber1"));
			packingList.Setup(m => m.InvoiceNumber).Returns(new ZString("InvoiceNumber1"));
			packingList.Setup(m => m.PurchaseOrderNumber).Returns(new ZString("PurchaseOrderNumber1"));

			var certificateData = new Mock<IDISCertificate>();
			certificateData.Setup(m => m.Number).Returns(new ZString("num-1"));
			certificateData.Setup(m => m.Type).Returns(new ZString("type"));
			certificateData.Setup(m => m.Statement).Returns(new ZString("statement-1"));
			certificateData.Setup(m => m.IssueDate).Returns(new ZDateTime(2011, 11, 11));
			certificateData.Setup(m => m.ExpiryDate).Returns(new ZDateTime(2012, 12, 12));
			certificateData.Setup(m => m.ImporterOfRecord).Returns(new ZString("importer"));
			certificateData.Setup(m => m.InspectionLocation).Returns(new ZString("Inspection Location"));
			certificateData.Setup(m => m.GrossTonnage).Returns(new ZDecimal(100m));
			certificateData.Setup(m => m.NetTonnage).Returns(new ZDecimal(90m));

			var permitData = new Mock<IDISPermit>();
			permitData.Setup(m => m.Number).Returns(new ZString("number 1"));
			permitData.Setup(m => m.Type).Returns(new ZString("type"));
			permitData.Setup(m => m.ApprovalNumber).Returns(new ZString("app num"));
			permitData.Setup(m => m.Statement).Returns(new ZString("state"));
			permitData.Setup(m => m.StartDate).Returns(new ZDateTime(12, 12, 12));
			permitData.Setup(m => m.EndDate).Returns(new ZDateTime(11, 11, 11));
			permitData.Setup(m => m.ImporterOfRecord).Returns(new ZString("importer record"));

			var toxicSubstanceData = new Mock<IDISToxicSubstanceData>();
			toxicSubstanceData.Setup(m => m.CASNumber).Returns(new ZString("casNum"));
			toxicSubstanceData.Setup(m => m.EPARegoNumber).Returns(new ZString("ePARegoNumber"));
			toxicSubstanceData.Setup(m => m.EPAProducerEstNumber).Returns(new ZString("ePAProducerEstNumber"));

			var additionalData = new Mock<IDISAdditionalData>();
			additionalData.Setup(m => m.FieldName).Returns(new ZString("fieldnaÉme1"));
			additionalData.Setup(m => m.Value).Returns(new ZString("vaÉlue1"));
			var additionalData2 = new Mock<IDISAdditionalData>();
			additionalData2.Setup(m => m.FieldName).Returns(new ZString("fÉieldname2"));
			additionalData2.Setup(m => m.Value).Returns(new ZString("vÉalue2"));

			var packageIdentifierData = new Mock<IDISPackageIdentifier>();
			packageIdentifierData.Setup(m => m.PackageCategory).Returns(new ZString("CBMA"));
			packageIdentifierData.Setup(m => m.ImporterOfRecordNumber).Returns(new ZString("0123456789"));

			var document = new Mock<IDISDocument>();
			document.Setup(m => m.DocumentID).Returns(new ZString("DocumentID-1"));
			document.Setup(m => m.DocumentLabel).Returns(new ZString("GenericDocument"));
			document.Setup(m => m.CompleteFileName).Returns(new ZString("sss.docx"));
			document.Setup(m => m.DocumentDescription).Returns(new ZString("DocumentDescription-1"));
			document.Setup(m => m.PreviouslySubmitted).Returns(ZBool.False);
			document.Setup(m => m.PGAs).Returns(new ZString[4] { PGAList.Codes.CBP, PGAList.Codes.AMS, PGAList.Codes.APH, "XXX" });
			document.Setup(m => m.Comment).Returns(new ZString("commentÉ-1"));
			document.Setup(m => m.Invoice).Returns(invoice.Object);
			document.Setup(m => m.BondData).Returns(bondData.Object);
			document.Setup(m => m.PackingList).Returns(packingList.Object);
			document.Setup(m => m.CertificateData).Returns(certificateData.Object);
			document.Setup(m => m.PermitData).Returns(permitData.Object);
			document.Setup(m => m.ToxicSubstanceData).Returns(toxicSubstanceData.Object);
			document.Setup(m => m.PackageIdentifierData).Returns(packageIdentifierData.Object);
			document.Setup(m => m.CommodityData).Returns(new IDISCommodityLine[] { commodityLine.Object, commodityLine2.Object });
			document.Setup(m => m.AdditionalData).Returns(new IDISAdditionalData[] { additionalData.Object, additionalData2.Object });
			document.Setup(m => m.eDocsDocumentPK).Returns(new ZGuid("95E977EA-863B-4D09-B981-258A50B14C19"));
			document.Setup(m => m.PortCode).Returns(portCode);
			document.Setup(m => m.PreparerID).Returns(new ZString("XJ4"));
			document.Setup(m => m.PreparerSiteCode).Returns(new ZString("3902"));
			document.Setup(m => m.TradeTransactions).Returns(GetTradeTransactions());
			document.Setup(m => m.CBPRequest).Returns(cBPRequest.Object);
			document.Setup(m => m.TransactionCategory).Returns(TransactionCategory.SingleTransaction);
			document.Setup(m => m.DocumentSentDateEST).Returns(ZDateTime.BrettsBirthday);
			document.Setup(m => m.ActionCodeForSubmission).Returns(new ZString(ActionCodeList.Codes.Add));
			document.Setup(m => m.CreateMessage(It.IsAny<string>())).Returns(Factory.New<EDIMessage>());
			document.Setup(m => m.DocumentLabelUSDISDocCode).Returns("LabelDesc");
			return document;
		}

		IDISDocument GetMessageDataForWithdrawal()
		{
			var invoice = new Mock<IDISInvoice>();
			invoice.Setup(m => m.InvoiceLines).Returns(System.Array.Empty<IDISInvoiceLine>());
			invoice.Setup(m => m.InvoiceNumber).Returns(ZString.Empty);
			invoice.Setup(m => m.InvoiceType).Returns(DISInvoiceType.CommercialInvoice);

			var cBPRequest = new Mock<IDISCBPRequest>();
			cBPRequest.Setup(m => m.ID).Returns(ZString.Empty);
			cBPRequest.Setup(m => m.RequestDate).Returns(ZDateTime.Empty);
			cBPRequest.Setup(m => m.Type).Returns(ZString.Empty);

			var bondData = new Mock<IDISBondData>();
			bondData.Setup(m => m.AgentIDNumber).Returns(ZString.Empty);
			bondData.Setup(m => m.BondAmount).Returns(ZDecimal.Zero);
			bondData.Setup(m => m.BondName).Returns(BondNameType.Single);
			bondData.Setup(m => m.BondNumber).Returns(ZString.Empty);
			bondData.Setup(m => m.BondType).Returns(ZString.Empty);
			bondData.Setup(m => m.Filer).Returns(ZString.Empty);
			bondData.Setup(m => m.SuretyCode).Returns(ZString.Empty);

			var packingList = new Mock<IDISPackingList>();
			packingList.Setup(m => m.PackingListNumber).Returns(ZString.Empty);
			packingList.Setup(m => m.InvoiceNumber).Returns(ZString.Empty);
			packingList.Setup(m => m.PurchaseOrderNumber).Returns(ZString.Empty);

			var certificateData = new Mock<IDISCertificate>();
			certificateData.Setup(m => m.Number).Returns(ZString.Empty);
			certificateData.Setup(m => m.Type).Returns(ZString.Empty);
			certificateData.Setup(m => m.Statement).Returns(ZString.Empty);
			certificateData.Setup(m => m.IssueDate).Returns(ZDateTime.Empty);
			certificateData.Setup(m => m.ExpiryDate).Returns(ZDateTime.Empty);
			certificateData.Setup(m => m.ImporterOfRecord).Returns(ZString.Empty);
			certificateData.Setup(m => m.InspectionLocation).Returns(ZString.Empty);
			certificateData.Setup(m => m.GrossTonnage).Returns(ZDecimal.Zero);
			certificateData.Setup(m => m.NetTonnage).Returns(ZDecimal.Zero);

			var permitData = new Mock<IDISPermit>();
			permitData.Setup(m => m.Number).Returns(ZString.Empty);
			permitData.Setup(m => m.Type).Returns(ZString.Empty);
			permitData.Setup(m => m.ApprovalNumber).Returns(ZString.Empty);
			permitData.Setup(m => m.Statement).Returns(ZString.Empty);
			permitData.Setup(m => m.StartDate).Returns(ZDateTime.Empty);
			permitData.Setup(m => m.EndDate).Returns(ZDateTime.Empty);
			permitData.Setup(m => m.ImporterOfRecord).Returns(ZString.Empty);

			var toxicSubstanceData = new Mock<IDISToxicSubstanceData>();
			toxicSubstanceData.Setup(m => m.CASNumber).Returns(ZString.Empty);
			toxicSubstanceData.Setup(m => m.EPARegoNumber).Returns(ZString.Empty);
			toxicSubstanceData.Setup(m => m.EPAProducerEstNumber).Returns(ZString.Empty);

			var packageIdentifierData = new Mock<IDISPackageIdentifier>();
			packageIdentifierData.Setup(m => m.PackageCategory).Returns(ZString.Empty);
			packageIdentifierData.Setup(m => m.ImporterOfRecordNumber).Returns(ZString.Empty);

			var document = new Mock<IDISDocument>();
			document.Setup(m => m.DocumentID).Returns(new ZString("DocumentID-1"));
			document.Setup(m => m.DocumentLabel).Returns(new ZString("GenericDocument"));
			document.Setup(m => m.CompleteFileName).Returns(new ZString("sss.docx"));
			document.Setup(m => m.DocumentDescription).Returns(new ZString("DocumentDescription-1"));
			document.Setup(m => m.PreviouslySubmitted).Returns(ZBool.False);
			document.Setup(m => m.PGAs).Returns(new ZString[4] { PGAList.Codes.CBP, PGAList.Codes.AMS, PGAList.Codes.APH, "XXX" });
			document.Setup(m => m.Comment).Returns(new ZString("comment-É"));
			document.Setup(m => m.Invoice).Returns(invoice.Object);
			document.Setup(m => m.BondData).Returns(bondData.Object);
			document.Setup(m => m.PackingList).Returns(packingList.Object);
			document.Setup(m => m.CertificateData).Returns(certificateData.Object);
			document.Setup(m => m.PermitData).Returns(permitData.Object);
			document.Setup(m => m.ToxicSubstanceData).Returns(toxicSubstanceData.Object);
			document.Setup(m => m.PackageIdentifierData).Returns(packageIdentifierData.Object);
			document.Setup(m => m.CommodityData).Returns(System.Array.Empty<IDISCommodityLine>());
			document.Setup(m => m.AdditionalData).Returns(System.Array.Empty<IDISAdditionalData>());
			document.Setup(m => m.eDocsDocumentPK).Returns(new ZGuid("95E977EA-863B-4D09-B981-258A50B14C19"));
			document.Setup(m => m.PortCode).Returns(new ZString("3901"));
			document.Setup(m => m.PreparerID).Returns(new ZString("XJ4"));
			document.Setup(m => m.PreparerSiteCode).Returns(new ZString("3902"));
			document.Setup(m => m.TradeTransactions).Returns(GetTradeTransactions());
			document.Setup(m => m.TransactionCategory).Returns(TransactionCategory.SingleTransaction);
			document.Setup(m => m.DocumentSentDateEST).Returns(ZDateTime.BrettsBirthday);
			document.Setup(m => m.CreateMessage(It.IsAny<string>())).Returns(Factory.New<EDIMessage>());
			document.Setup(m => m.DocumentLabelUSDISDocCode).Returns("LabelDesc");
			return document.Object;
		}

		Mock<IDISDocument> GetMessageDataMockNonOptionalParts()
		{
			ZString portCode = "123456";
			ZString actionCode = "DEL";
			ZString tradeTransactionCategory = "SINGLE_TXN";

			var cBPRequest = new Mock<IDISCBPRequest>();
			cBPRequest.Setup(m => m.ID).Returns(new ZString("8D5228EE-B9C1-4681-AB7F-A3B0D8E582B7"));
			cBPRequest.Setup(m => m.RequestDate).Returns(new ZDateTime(13, 10, 01));
			cBPRequest.Setup(m => m.Type).Returns(new ZString("ACEActionNumber"));

			var document = new Mock<IDISDocument>();
			document.Setup(m => m.DocumentID).Returns(new ZString("DocumentID-1"));
			document.Setup(m => m.DocumentLabel).Returns(new ZString("GenericDocument"));
			document.Setup(m => m.CompleteFileName).Returns(new ZString("sss.docx"));
			document.Setup(m => m.DocumentDescription).Returns(new ZString("DocumentDescription-1"));
			document.Setup(m => m.PreviouslySubmitted).Returns(ZBool.False);
			document.Setup(m => m.PGAs).Returns(new ZString[4] { PGAList.Codes.CBP, PGAList.Codes.AMS, PGAList.Codes.APH, "XXX" });
			document.Setup(m => m.Comment).Returns(new ZString("commentÉ-1"));

			// dummy non-null values
			document.Setup(m => m.CommodityData).Returns(System.Array.Empty<IDISCommodityLine>());
			document.Setup(m => m.AdditionalData).Returns(System.Array.Empty<IDISAdditionalData>());

			document.Setup(m => m.eDocsDocumentPK).Returns(new ZGuid("95E977EA-863B-4D09-B981-258A50B14C19"));
			document.Setup(m => m.PortCode).Returns(portCode);
			document.Setup(m => m.PreparerID).Returns(new ZString("XJ4"));
			document.Setup(m => m.PreparerSiteCode).Returns(new ZString("3902"));
			document.Setup(m => m.TradeTransactions).Returns(GetTradeTransactions());
			document.Setup(m => m.CBPRequest).Returns(cBPRequest.Object);
			document.Setup(m => m.TransactionCategory).Returns(TransactionCategory.SingleTransaction);
			document.Setup(m => m.DocumentSentDateEST).Returns(ZDateTime.BrettsBirthday);
			document.Setup(m => m.ActionCodeForSubmission).Returns(new ZString(ActionCodeList.Codes.Add));
			document.Setup(m => m.CreateMessage(It.IsAny<string>())).Returns(Factory.New<EDIMessage>());
			document.Setup(m => m.DocumentLabelUSDISDocCode).Returns("LabelDesc");
			return document;
		}

		IEnumerable<IDISTradeTransaction> GetTradeTransactions()
		{
			var tradeTransction = new Mock<IDISTradeTransaction>();
			tradeTransction.Setup(m => m.AdditionalNumbers).Returns(new ZString[] { new ZString("1"), new ZString("2") });
			tradeTransction.Setup(m => m.FilerOrSCAC).Returns(new ZString("XJ5"));
			tradeTransction.Setup(m => m.Number).Returns(new ZString("7984328"));
			tradeTransction.Setup(m => m.Type).Returns(TradeTransactionType.Entry);
			tradeTransction.Setup(m => m.ReferenceNumber).Returns(new ZString("123456789"));

			return new IDISTradeTransaction[] { tradeTransction.Object };
		}

		IEnumerable<IDISTradeTransaction> GetTradeTransactionsExport()
		{
			var tradeTransction = new Mock<IDISTradeTransaction>();
			tradeTransction.Setup(m => m.Number).Returns(new ZString("7984328"));
			tradeTransction.Setup(m => m.Type).Returns(TradeTransactionType.Export);
			tradeTransction.Setup(m => m.ShipmentNo).Returns(new ZString("123456789"));

			return new IDISTradeTransaction[] { tradeTransction.Object };
		}

		IDISDocument GetExportMessage()
		{
			var invoice = new Mock<IDISInvoice>();
			invoice.Setup(m => m.InvoiceLines).Returns(System.Array.Empty<IDISInvoiceLine>());
			invoice.Setup(m => m.InvoiceNumber).Returns(ZString.Empty);
			invoice.Setup(m => m.InvoiceType).Returns(DISInvoiceType.CommercialInvoice);

			var cBPRequest = new Mock<IDISCBPRequest>();
			cBPRequest.Setup(m => m.ID).Returns(ZString.Empty);
			cBPRequest.Setup(m => m.RequestDate).Returns(ZDateTime.Empty);
			cBPRequest.Setup(m => m.Type).Returns(ZString.Empty);

			var bondData = new Mock<IDISBondData>();
			bondData.Setup(m => m.AgentIDNumber).Returns(ZString.Empty);
			bondData.Setup(m => m.BondAmount).Returns(ZDecimal.Zero);
			bondData.Setup(m => m.BondName).Returns(BondNameType.Single);
			bondData.Setup(m => m.BondNumber).Returns(ZString.Empty);
			bondData.Setup(m => m.BondType).Returns(ZString.Empty);
			bondData.Setup(m => m.Filer).Returns(ZString.Empty);
			bondData.Setup(m => m.SuretyCode).Returns(ZString.Empty);

			var packingList = new Mock<IDISPackingList>();
			packingList.Setup(m => m.PackingListNumber).Returns(ZString.Empty);
			packingList.Setup(m => m.InvoiceNumber).Returns(ZString.Empty);
			packingList.Setup(m => m.PurchaseOrderNumber).Returns(ZString.Empty);

			var certificateData = new Mock<IDISCertificate>();
			certificateData.Setup(m => m.Number).Returns(ZString.Empty);
			certificateData.Setup(m => m.Type).Returns(ZString.Empty);
			certificateData.Setup(m => m.Statement).Returns(ZString.Empty);
			certificateData.Setup(m => m.IssueDate).Returns(ZDateTime.Empty);
			certificateData.Setup(m => m.ExpiryDate).Returns(ZDateTime.Empty);
			certificateData.Setup(m => m.ImporterOfRecord).Returns(ZString.Empty);
			certificateData.Setup(m => m.InspectionLocation).Returns(ZString.Empty);
			certificateData.Setup(m => m.GrossTonnage).Returns(ZDecimal.Zero);
			certificateData.Setup(m => m.NetTonnage).Returns(ZDecimal.Zero);

			var permitData = new Mock<IDISPermit>();
			permitData.Setup(m => m.Number).Returns(ZString.Empty);
			permitData.Setup(m => m.Type).Returns(ZString.Empty);
			permitData.Setup(m => m.ApprovalNumber).Returns(ZString.Empty);
			permitData.Setup(m => m.Statement).Returns(ZString.Empty);
			permitData.Setup(m => m.StartDate).Returns(ZDateTime.Empty);
			permitData.Setup(m => m.EndDate).Returns(ZDateTime.Empty);
			permitData.Setup(m => m.ImporterOfRecord).Returns(ZString.Empty);

			var toxicSubstanceData = new Mock<IDISToxicSubstanceData>();
			toxicSubstanceData.Setup(m => m.CASNumber).Returns(ZString.Empty);
			toxicSubstanceData.Setup(m => m.EPARegoNumber).Returns(ZString.Empty);
			toxicSubstanceData.Setup(m => m.EPAProducerEstNumber).Returns(ZString.Empty);

			var packageIdentifierData = new Mock<IDISPackageIdentifier>();
			packageIdentifierData.Setup(m => m.PackageCategory).Returns(ZString.Empty);
			packageIdentifierData.Setup(m => m.ImporterOfRecordNumber).Returns(ZString.Empty);

			var document = new Mock<IDISDocument>();
			document.Setup(m => m.DocumentID).Returns(new ZString("DocumentID-1"));
			document.Setup(m => m.DocumentLabel).Returns(new ZString("GenericDocument"));
			document.Setup(m => m.DocumentLabelUSDISDocCode).Returns(new ZString("LabelDesc"));
			document.Setup(m => m.CompleteFileName).Returns(new ZString("sss.docx"));
			document.Setup(m => m.DocumentDescription).Returns(new ZString("DocumentDescription-1"));
			document.Setup(m => m.PreviouslySubmitted).Returns(ZBool.False);
			document.Setup(m => m.PGAs).Returns(new ZString[4] { PGAList.Codes.CBP, PGAList.Codes.AMS, PGAList.Codes.APH, "XXX" });
			document.Setup(m => m.Comment).Returns(new ZString("commentÉ-1"));
			document.Setup(m => m.Invoice).Returns(invoice.Object);
			document.Setup(m => m.BondData).Returns(bondData.Object);
			document.Setup(m => m.PackingList).Returns(packingList.Object);
			document.Setup(m => m.CertificateData).Returns(certificateData.Object);
			document.Setup(m => m.PermitData).Returns(permitData.Object);
			document.Setup(m => m.ToxicSubstanceData).Returns(toxicSubstanceData.Object);
			document.Setup(m => m.PackageIdentifierData).Returns(packageIdentifierData.Object);
			document.Setup(m => m.CommodityData).Returns(System.Array.Empty<IDISCommodityLine>());
			document.Setup(m => m.AdditionalData).Returns(System.Array.Empty<IDISAdditionalData>());
			document.Setup(m => m.eDocsDocumentPK).Returns(new ZGuid("95E977EA-863B-4D09-B981-258A50B14C19"));
			document.Setup(m => m.PortCode).Returns(new ZString("3901"));
			document.Setup(m => m.PreparerID).Returns(new ZString("XJ4"));
			document.Setup(m => m.PreparerSiteCode).Returns(new ZString("3902"));
			document.Setup(m => m.TradeTransactions).Returns(GetTradeTransactionsExport());
			document.Setup(m => m.TransactionCategory).Returns(TransactionCategory.SingleTransaction);
			document.Setup(m => m.DocumentSentDateEST).Returns(ZDateTime.BrettsBirthday);
			document.Setup(m => m.ActionCodeForSubmission).Returns(new ZString(ActionCodeList.Codes.Add));
			document.Setup(m => m.CreateMessage(It.IsAny<string>())).Returns(Factory.New<EDIMessage>());
			return document.Object;
		}

		void AssertOptionalInvoiceData()
		{
			var invoiceLine1 = new Mock<IDISInvoiceLine>();
			invoiceLine1.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(12));

			var invoiceLine2 = new Mock<IDISInvoiceLine>();
			invoiceLine2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(13));

			var invoice = new Mock<IDISInvoice>();
			invoice.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLine[] { invoiceLine1.Object, invoiceLine2.Object });
			invoice.Setup(m => m.InvoiceNumber).Returns(new ZString("invoiceNo1"));
			invoice.Setup(m => m.InvoiceType).Returns(DISInvoiceType.CommercialInvoice);

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.Invoice).Returns(invoice.Object);

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("invoiceData", @"<DIS:InvoiceData>
            <DIS:InvoiceNbr>INVOICENO1</DIS:InvoiceNbr>
            <DIS:InvoiceType>COMMERCIAL_INVOICE</DIS:InvoiceType>
            <DIS:InvoiceLineItemData>
              <DIS:InvoiceLineNbr>12</DIS:InvoiceLineNbr>
            </DIS:InvoiceLineItemData>
            <DIS:InvoiceLineItemData>
              <DIS:InvoiceLineNbr>13</DIS:InvoiceLineNbr>
            </DIS:InvoiceLineItemData>
          </DIS:InvoiceData>", ediMessage.EM_MessageText);

			invoiceLine1.VerifyAll();
			invoiceLine2.VerifyAll();
			invoice.VerifyAll();
		}

		void AssertOptionalBondData()
		{
			var bondData = new Mock<IDISBondData>();
			bondData.Setup(m => m.AgentIDNumber).Returns(new ZString("0F1968EF-CD68-45B3-9D47-DC4AA3B8B88B"));
			bondData.Setup(m => m.BondAmount).Returns(new ZDecimal(45));
			bondData.Setup(m => m.BondName).Returns(BondNameType.Single);
			bondData.Setup(m => m.BondNumber).Returns(new ZString("bond no-1"));
			bondData.Setup(m => m.BondType).Returns(new ZString("bond-type"));
			bondData.Setup(m => m.Filer).Returns(new ZString("filer1"));
			bondData.Setup(m => m.SuretyCode).Returns(new ZString("surety code"));

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.BondData).Returns(bondData.Object);

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("BondData", @"<DIS:BondData>
            <DIS:BondName>SINGLE_TXN_BOND</DIS:BondName>
            <DIS:BondNumber>BOND NO-1</DIS:BondNumber>
            <DIS:BondType>BOND-TYPE</DIS:BondType>
            <DIS:SuretyCode>SURETY CODE</DIS:SuretyCode>
            <DIS:AgentIDNbr>0F1968EF-CD68-45B3-9D47-DC4AA3B8B88B</DIS:AgentIDNbr>
            <DIS:Filer>FILER1</DIS:Filer>
            <DIS:BondAmount>45.00</DIS:BondAmount>
          </DIS:BondData>", ediMessage.EM_MessageText);

			bondData.VerifyAll();
		}

		void AssertOptionalPackingListData()
		{
			var packingList = new Mock<IDISPackingList>();
			packingList.Setup(m => m.PackingListNumber).Returns(new ZString("PackingListÉNumber1"));
			packingList.Setup(m => m.InvoiceNumber).Returns(new ZString("InvoiceÉNumber1"));
			packingList.Setup(m => m.PurchaseOrderNumber).Returns(new ZString("PurchaseOrderÉNumber1"));

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.PackingList).Returns(packingList.Object);

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("PackingList", @"<DIS:PackingListData>
            <DIS:PackingListNbr>PACKINGLIST*NUMBER1</DIS:PackingListNbr>
            <DIS:InvoiceNumber>INVOICE*NUMBER1</DIS:InvoiceNumber>
            <DIS:PurchaseOrderNbr>PURCHASEORDER*NUMBER1</DIS:PurchaseOrderNbr>
          </DIS:PackingListData>", ediMessage.EM_MessageText);

			packingList.VerifyAll();
		}

		void AssertOptionalCertificateData()
		{
			var certificateData = new Mock<IDISCertificate>();
			certificateData.Setup(m => m.Number).Returns(new ZString("num-1"));
			certificateData.Setup(m => m.Type).Returns(new ZString("type"));
			certificateData.Setup(m => m.Statement).Returns(new ZString("statement-1"));
			certificateData.Setup(m => m.IssueDate).Returns(new ZDateTime(2011, 11, 11));
			certificateData.Setup(m => m.ExpiryDate).Returns(new ZDateTime(2012, 12, 12));
			certificateData.Setup(m => m.ImporterOfRecord).Returns(new ZString("importer"));
			certificateData.Setup(m => m.InspectionLocation).Returns(new ZString("Inspection Location"));
			certificateData.Setup(m => m.GrossTonnage).Returns(new ZDecimal(100m));
			certificateData.Setup(m => m.NetTonnage).Returns(new ZDecimal(90m));

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.CertificateData).Returns(certificateData.Object);

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("CertificateData (before time zone)", @"<DIS:CertificateData>
            <DIS:CertificateNumber>NUM-1</DIS:CertificateNumber>
            <DIS:CertificateType>TYPE</DIS:CertificateType>
            <DIS:Statement>STATEMENT-1</DIS:Statement>
            <DIS:ExpirationDate>2012-12-12T00:00:00+11:00", ediMessage.EM_MessageText);

			AssertContains("CertificateData (after time zone)", @"</DIS:ExpirationDate>
            <DIS:ImporterOfRecord>IMPORTER</DIS:ImporterOfRecord>
            <DIS:InspectionLocation>INSPECTION LOCATION</DIS:InspectionLocation>
          </DIS:CertificateData>", ediMessage.EM_MessageText);

			AssertContains("AdditionalData from Certificate", @"<DIS:AdditionalData>
            <DIS:NameValuePair>
              <DIS:Name>ISSUE_DATE</DIS:Name>
              <DIS:Value>2011-11-11T00:00:00</DIS:Value>
            </DIS:NameValuePair>
            <DIS:NameValuePair>
              <DIS:Name>GROSS_TONNAGE</DIS:Name>
              <DIS:Value>100</DIS:Value>
            </DIS:NameValuePair>
            <DIS:NameValuePair>
              <DIS:Name>NET_TONNAGE</DIS:Name>
              <DIS:Value>90</DIS:Value>
            </DIS:NameValuePair>
          </DIS:AdditionalData>", ediMessage.EM_MessageText);

			certificateData.VerifyAll();
		}

		void AssertOptionalPermitData()
		{
			var permitData = new Mock<IDISPermit>();
			permitData.Setup(m => m.Number).Returns(new ZString("number 1"));
			permitData.Setup(m => m.Type).Returns(new ZString("type"));
			permitData.Setup(m => m.ApprovalNumber).Returns(new ZString("app num"));
			permitData.Setup(m => m.Statement).Returns(new ZString("state"));
			permitData.Setup(m => m.StartDate).Returns(new ZDateTime(12, 12, 12));
			permitData.Setup(m => m.EndDate).Returns(new ZDateTime(11, 11, 11));
			permitData.Setup(m => m.ImporterOfRecord).Returns(new ZString("importer record"));

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.PermitData).Returns(permitData.Object);

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("PermitData", @"<DIS:PermitData>
            <DIS:PermitNumber>NUMBER 1</DIS:PermitNumber>
            <DIS:PermitType>TYPE</DIS:PermitType>
            <DIS:ApprovalNumber>APP NUM</DIS:ApprovalNumber>
            <DIS:Statement>STATE</DIS:Statement>
            <DIS:ImporterOfRecord>IMPORTER RECORD</DIS:ImporterOfRecord>
          </DIS:PermitData>", ediMessage.EM_MessageText);

			permitData.VerifyAll();
		}

		void AssertOptionalToxicSubstancesData()
		{
			var toxicSubstanceData = new Mock<IDISToxicSubstanceData>();
			toxicSubstanceData.Setup(m => m.CASNumber).Returns(new ZString("casNum"));
			toxicSubstanceData.Setup(m => m.EPARegoNumber).Returns(new ZString("ePARegoNumber"));
			toxicSubstanceData.Setup(m => m.EPAProducerEstNumber).Returns(new ZString("ePAProducerEstNumber"));

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.ToxicSubstanceData).Returns(toxicSubstanceData.Object);

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("ToxicSubstance", @"<DIS:ToxicSubstancesData>
            <DIS:CASNbr>CASNUM</DIS:CASNbr>
            <DIS:EPARegistrationNbr>EPAREGONUMBER</DIS:EPARegistrationNbr>
            <DIS:EPAProducerEstNbr>EPAPRODUCERESTNUMBER</DIS:EPAProducerEstNbr>
          </DIS:ToxicSubstancesData>", ediMessage.EM_MessageText);

			toxicSubstanceData.VerifyAll();
		}

		void AssertOptionalCommodityData()
		{
			var tradeParty1 = new Mock<IDISTradeParty>();
			tradeParty1.Setup(m => m.Address).Returns(new ZString("add1"));
			tradeParty1.Setup(m => m.ID).Returns(new ZString("ID1"));
			tradeParty1.Setup(m => m.Name).Returns(new ZString("name1"));
			tradeParty1.Setup(m => m.Type).Returns(DISTradePartyType.Exporter);

			var tradeParty2 = new Mock<IDISTradeParty>();
			tradeParty2.Setup(m => m.Address).Returns(new ZString("add2"));
			tradeParty2.Setup(m => m.ID).Returns(new ZString("ID2"));
			tradeParty2.Setup(m => m.Name).Returns(new ZString("name2"));
			tradeParty2.Setup(m => m.Type).Returns(DISTradePartyType.Importer);

			var tradeParties = (IEnumerable<IDISTradeParty>)new IDISTradeParty[] { tradeParty1.Object, tradeParty2.Object };

			var vehicleAndEngineData = new Mock<IDISVehicleAndEngineData>();
			vehicleAndEngineData.Setup(m => m.EngineManufactureDate).Returns(new ZDateTime(10, 11, 12));
			vehicleAndEngineData.Setup(m => m.EngineManufacturer).Returns(new ZString("engineManufacturer1"));
			vehicleAndEngineData.Setup(m => m.EngineModel).Returns(new ZString("engineModel1"));
			vehicleAndEngineData.Setup(m => m.EngineSerialNumber).Returns(new ZString("engineSerialNumber1"));
			vehicleAndEngineData.Setup(m => m.Manufacturer).Returns(new ZString("manufacturer1"));
			vehicleAndEngineData.Setup(m => m.Model).Returns(new ZString("model1"));
			vehicleAndEngineData.Setup(m => m.SerialNumber).Returns(new ZString("serialNu1"));
			vehicleAndEngineData.Setup(m => m.VIN).Returns(new ZString("vin1"));
			vehicleAndEngineData.Setup(m => m.VNELineNumber).Returns(new ZInt(1));
			vehicleAndEngineData.Setup(m => m.ManufactureYear).Returns(new ZString("1998"));
			vehicleAndEngineData.Setup(m => m.ManufactureMonth).Returns(new ZString("1"));

			var commodityLine = new Mock<IDISCommodityLine>();
			commodityLine.Setup(m => m.ArrivalDate).Returns(new ZDateTime(12, 10, 10));
			commodityLine.Setup(m => m.CommodityDescription).Returns(new ZString("commLineDesc"));
			commodityLine.Setup(m => m.ContainerNumber).Returns(new ZString("conNum1"));
			commodityLine.Setup(m => m.CountryOfOrigin).Returns(new ZString("Aus"));
			commodityLine.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			commodityLine.Setup(m => m.HTSNumber).Returns(new ZString("HTSNo-1"));
			commodityLine.Setup(m => m.PortOfEntry).Returns(new ZString("PortEntry-1"));
			commodityLine.Setup(m => m.PortOfLoading).Returns(new ZString("PortOfLoading-1"));
			commodityLine.Setup(m => m.PortOfUnlading).Returns(new ZString("PortOfUnlading1"));
			commodityLine.Setup(m => m.SealNumber).Returns(new ZString("SelaNo-1"));
			commodityLine.Setup(m => m.TradeParties).Returns(tradeParties);
			commodityLine.Setup(m => m.VehicleData).Returns(vehicleAndEngineData.Object);

			var commodityLine2 = new Mock<IDISCommodityLine>();
			commodityLine2.Setup(m => m.ArrivalDate).Returns(new ZDateTime(12, 10, 10));
			commodityLine2.Setup(m => m.CommodityDescription).Returns(new ZString("commLineDesc2"));
			commodityLine2.Setup(m => m.ContainerNumber).Returns(new ZString("conNum2"));
			commodityLine2.Setup(m => m.CountryOfOrigin).Returns(new ZString("Aus"));
			commodityLine2.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			commodityLine2.Setup(m => m.HTSNumber).Returns(new ZString("HTSNo-2"));
			commodityLine2.Setup(m => m.PortOfEntry).Returns(new ZString("PortEntry-2"));
			commodityLine2.Setup(m => m.PortOfLoading).Returns(new ZString("portoflanding-2"));
			commodityLine2.Setup(m => m.PortOfUnlading).Returns(new ZString("PortOfUnlading2"));
			commodityLine2.Setup(m => m.SealNumber).Returns(new ZString("SelaNo-2"));
			commodityLine2.Setup(m => m.TradeParties).Returns(tradeParties);
			commodityLine2.Setup(m => m.VehicleData).Returns(vehicleAndEngineData.Object);

			var invoiceLine1 = new Mock<IDISInvoiceLine>();
			invoiceLine1.Setup(m => m.CommodityDetails).Returns(commodityLine.Object);
			invoiceLine1.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(12));

			var invoiceLine2 = new Mock<IDISInvoiceLine>();
			invoiceLine2.Setup(m => m.CommodityDetails).Returns(commodityLine.Object);
			invoiceLine2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(13));

			var invoice = new Mock<IDISInvoice>();
			invoice.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLine[] { invoiceLine1.Object, invoiceLine2.Object });
			invoice.Setup(m => m.InvoiceNumber).Returns(new ZString("invoiceNo1"));
			invoice.Setup(m => m.InvoiceType).Returns(DISInvoiceType.CommercialInvoice);

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.Invoice).Returns(invoice.Object);

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("invoiceData", @"          <DIS:InvoiceData>
            <DIS:InvoiceNbr>INVOICENO1</DIS:InvoiceNbr>
            <DIS:InvoiceType>COMMERCIAL_INVOICE</DIS:InvoiceType>
            <DIS:InvoiceLineItemData>
              <DIS:InvoiceLineNbr>12</DIS:InvoiceLineNbr>
              <DIS:CommodityData>
                <DIS:EntryLineNumber>1</DIS:EntryLineNumber>
                <DIS:HTSNumber>HTSNO-1</DIS:HTSNumber>
                <DIS:CommodityDescription>COMMLINEDESC</DIS:CommodityDescription>
                <DIS:CountryOfOrigin>AUS</DIS:CountryOfOrigin>
                <DIS:ContainerNbr>CONNUM1</DIS:ContainerNbr>
                <DIS:PortOfLading>PORTOFLOADING-1</DIS:PortOfLading>
                <DIS:PortOfUnlading>PORTOFUNLADING1</DIS:PortOfUnlading>
                <DIS:PortOfEntry>PORTENTRY-1</DIS:PortOfEntry>
                <DIS:SealNumbers>SELANO-1</DIS:SealNumbers>
                <DIS:TradeParties>
                  <DIS:TradeParty>
                    <DIS:TradePartyID>ID1</DIS:TradePartyID>
                    <DIS:TradePartyType>EXPORTER</DIS:TradePartyType>
                    <DIS:TradePartyName>NAME1</DIS:TradePartyName>
                    <DIS:TradePartyAddress>ADD1</DIS:TradePartyAddress>
                  </DIS:TradeParty>
                  <DIS:TradeParty>
                    <DIS:TradePartyID>ID2</DIS:TradePartyID>
                    <DIS:TradePartyType>IMPORTER</DIS:TradePartyType>
                    <DIS:TradePartyName>NAME2</DIS:TradePartyName>
                    <DIS:TradePartyAddress>ADD2</DIS:TradePartyAddress>
                  </DIS:TradeParty>
                </DIS:TradeParties>
                <DIS:VehicleAndEngineData>
                  <DIS:VIN>VIN1</DIS:VIN>
                  <DIS:VehicleManufacturer>MANUFACTURER1</DIS:VehicleManufacturer>
                  <DIS:VehicleModel>MODEL1</DIS:VehicleModel>
                  <DIS:VehicleSerialNumber>SERIALNU1</DIS:VehicleSerialNumber>
                  <DIS:EngineManufacturer>ENGINEMANUFACTURER1</DIS:EngineManufacturer>
                  <DIS:EngineModel>ENGINEMODEL1</DIS:EngineModel>
                  <DIS:EngineSerialNumber>ENGINESERIALNUMBER1</DIS:EngineSerialNumber>
                </DIS:VehicleAndEngineData>
              </DIS:CommodityData>
            </DIS:InvoiceLineItemData>
            <DIS:InvoiceLineItemData>
              <DIS:InvoiceLineNbr>13</DIS:InvoiceLineNbr>
              <DIS:CommodityData>
                <DIS:EntryLineNumber>1</DIS:EntryLineNumber>
                <DIS:HTSNumber>HTSNO-1</DIS:HTSNumber>
                <DIS:CommodityDescription>COMMLINEDESC</DIS:CommodityDescription>
                <DIS:CountryOfOrigin>AUS</DIS:CountryOfOrigin>
                <DIS:ContainerNbr>CONNUM1</DIS:ContainerNbr>
                <DIS:PortOfLading>PORTOFLOADING-1</DIS:PortOfLading>
                <DIS:PortOfUnlading>PORTOFUNLADING1</DIS:PortOfUnlading>
                <DIS:PortOfEntry>PORTENTRY-1</DIS:PortOfEntry>
                <DIS:SealNumbers>SELANO-1</DIS:SealNumbers>
                <DIS:TradeParties>
                  <DIS:TradeParty>
                    <DIS:TradePartyID>ID1</DIS:TradePartyID>
                    <DIS:TradePartyType>EXPORTER</DIS:TradePartyType>
                    <DIS:TradePartyName>NAME1</DIS:TradePartyName>
                    <DIS:TradePartyAddress>ADD1</DIS:TradePartyAddress>
                  </DIS:TradeParty>
                  <DIS:TradeParty>
                    <DIS:TradePartyID>ID2</DIS:TradePartyID>
                    <DIS:TradePartyType>IMPORTER</DIS:TradePartyType>
                    <DIS:TradePartyName>NAME2</DIS:TradePartyName>
                    <DIS:TradePartyAddress>ADD2</DIS:TradePartyAddress>
                  </DIS:TradeParty>
                </DIS:TradeParties>
                <DIS:VehicleAndEngineData>
                  <DIS:VIN>VIN1</DIS:VIN>
                  <DIS:VehicleManufacturer>MANUFACTURER1</DIS:VehicleManufacturer>
                  <DIS:VehicleModel>MODEL1</DIS:VehicleModel>
                  <DIS:VehicleSerialNumber>SERIALNU1</DIS:VehicleSerialNumber>
                  <DIS:EngineManufacturer>ENGINEMANUFACTURER1</DIS:EngineManufacturer>
                  <DIS:EngineModel>ENGINEMODEL1</DIS:EngineModel>
                  <DIS:EngineSerialNumber>ENGINESERIALNUMBER1</DIS:EngineSerialNumber>
                </DIS:VehicleAndEngineData>
              </DIS:CommodityData>
            </DIS:InvoiceLineItemData>
          </DIS:InvoiceData>", ediMessage.EM_MessageText);
		}

		void AssertOptionalAdditionalData()
		{
			var additionalData = new Mock<IDISAdditionalData>();
			additionalData.Setup(m => m.FieldName).Returns(new ZString("fieldÉname1"));
			additionalData.Setup(m => m.Value).Returns(new ZString("valÉue1"));
			var additionalData2 = new Mock<IDISAdditionalData>();
			additionalData2.Setup(m => m.FieldName).Returns(new ZString("fielÉdname2"));
			additionalData2.Setup(m => m.Value).Returns(new ZString("valÉue2"));

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.AdditionalData).Returns(new IDISAdditionalData[] { additionalData.Object, additionalData2.Object });

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();

			AssertContains("AdditionalData", @"          <DIS:AdditionalData>
            <DIS:NameValuePair>
              <DIS:Name>FIELD*NAME1</DIS:Name>
              <DIS:Value>VAL*UE1</DIS:Value>
            </DIS:NameValuePair>
            <DIS:NameValuePair>
              <DIS:Name>FIEL*DNAME2</DIS:Name>
              <DIS:Value>VAL*UE2</DIS:Value>
            </DIS:NameValuePair>", ediMessage.EM_MessageText);

			additionalData.VerifyAll();
			additionalData2.VerifyAll();
		}

		void AssertOptionalPackageIdentifier()
		{
			var packageIdentifierData = new Mock<IDISPackageIdentifier>();
			packageIdentifierData.Setup(m => m.PackageCategory).Returns(new ZString("GEN"));
			packageIdentifierData.Setup(m => m.ImporterOfRecordNumber).Returns(new ZString("0123456789"));

			var document = GetMessageDataMockNonOptionalParts();
			document.Setup(m => m.PackageIdentifierData).Returns(packageIdentifierData.Object);

			var messageBuilder = new DISMessageBuilder(document.Object);
			var ediMessage = messageBuilder.BuildDocumentSubmissionPackage();
			AssertContains("PermitData", @"<DIS:PackageIdentifier>
        <DIS:PackageCategory>GEN</DIS:PackageCategory>
        <DIS:ImporterOfRecordNbr>0123456789</DIS:ImporterOfRecordNbr>
      </DIS:PackageIdentifier>", ediMessage.EM_MessageText);

			packageIdentifierData.VerifyAll();
		}

		const string ExpectedResultForTradeTransaction = @"<DIS:TradeTransaction>
        <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
        <DIS:FTZAdmissionNbr>4011231A121TST00270</DIS:FTZAdmissionNbr>
      </DIS:TradeTransaction>";
	}
}
