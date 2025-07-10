namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class HMAC256Test : TestCaseWithFactory
	{
		public void TestMACGeneration()
		{
			var pin = "NN12WW";
			var msg = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>EX</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>EX1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>E40</TypeCode><FunctionalReferenceID>B00001176</FunctionalReferenceID><FunctionCode>9</FunctionCode><TotalGrossMassMeasure unitCode=""KGM"">50</TotalGrossMassMeasure><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>EX1 message test</Content></AdditionalInformation><AdditionalInformation><StatementDescription /><StatementTypeCode>HAN</StatementTypeCode></AdditionalInformation><Agent><ID>00009908C</ID><RoleCode>CB</RoleCode></Agent><BorderTransportMeans><Name>QF108</Name><TypeCode>4</TypeCode></BorderTransportMeans><CurrencyExchange><RateNumeric>1</RateNumeric><CurrencyTypeCode>NZD</CurrencyTypeCode></CurrencyExchange><Declarant><ID>40006206E</ID><Communication><ID>gary.odea@cargowise.com</ID><TypeID>EM</TypeID></Communication><Communication><ID>+61 2 80012200</ID><TypeID>TE</TypeID></Communication></Declarant><DutyTaxFee><Payment><MethodCode>C</MethodCode></Payment></DutyTaxFee><Exporter><ID>24392392B</ID></Exporter><GoodsShipment><ExitDateTime formatCode=""102"">20130527</ExitDateTime><TransactionNatureCode>11</TransactionNatureCode><Consignment><GoodsLocation><ID /></GoodsLocation><LoadingLocation><ID>NZAKL</ID></LoadingLocation><TransportContractDocument><ID>08100293823</ID><TypeCode>MB</TypeCode><Pointer><SequenceNumeric>1</SequenceNumeric><DocumentSectionCode>30B</DocumentSectionCode></Pointer></TransportContractDocument><TransportContractDocument><ID>Q894287</ID><TypeCode>HWB</TypeCode><Pointer><SequenceNumeric>2</SequenceNumeric><DocumentSectionCode>30B</DocumentSectionCode></Pointer></TransportContractDocument><UnloadingLocation><ID>AUSYD</ID></UnloadingLocation></Consignment><Importer /><Invoice><IssueDateTime formatCode=""102"">20130409</IssueDateTime><ID>7342450J</ID><SequenceNumeric>1</SequenceNumeric></Invoice></GoodsShipment><Packaging><SequenceNumeric>1</SequenceNumeric><QuantityQuantity>1</QuantityQuantity><TypeCode>CT</TypeCode></Packaging></Declaration>
</DocumentMetadata>";
			var expectedResult = "KH8RmNT51MUX9Q3LYPaiMjiBYF5GxyE80q5hl7IAGa8=";
			AssertEquals("MAC Generated", expectedResult, HMAC256.GenerateNewHMAC(pin, msg));
		}

		public void TestMAC()
		{
			var pin = "1234567890";
			var msg = "The quick brown fox jumped over the lazy dog";
			var expectedResult = "wtiBiyaNAv5jgAJZ5/j+SqovtJLNATJH/qvSdBrs7a0=";
			AssertEquals("MAC Generated", expectedResult, HMAC256.GenerateNewHMAC(pin, msg));
		}

		public void TestVerifedMAC()
		{
			//I can confirm that your WCO3 message passed the MAC validation - Fleur Savage
			var pin = "NN12WW";
			var msg = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>EX</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>EX1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>E40</TypeCode><FunctionalReferenceID>B00001178</FunctionalReferenceID><FunctionCode>9</FunctionCode><TotalGrossMassMeasure unitCode=""KGM"">50</TotalGrossMassMeasure><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>HMAC 256 generation of MAC</Content></AdditionalInformation><AdditionalInformation><StatementDescription /><StatementTypeCode>HAN</StatementTypeCode></AdditionalInformation><Agent><ID>00009908C</ID><RoleCode>CB</RoleCode></Agent><BorderTransportMeans><Name>QF108</Name><TypeCode>4</TypeCode></BorderTransportMeans><CurrencyExchange><RateNumeric>1</RateNumeric><CurrencyTypeCode>NZD</CurrencyTypeCode></CurrencyExchange><Declarant><ID>40006206E</ID><Communication><ID>gary.odea@cargowise.com</ID><TypeID>EM</TypeID></Communication><Communication><ID>+61 2 80012200</ID><TypeID>TE</TypeID></Communication></Declarant><DutyTaxFee><Payment><MethodCode>C</MethodCode></Payment></DutyTaxFee><Exporter><ID>24392392B</ID></Exporter><GoodsShipment><ExitDateTime formatCode=""102"">20130528</ExitDateTime><TransactionNatureCode>11</TransactionNatureCode><Consignment><GoodsLocation><ID /></GoodsLocation><LoadingLocation><ID>NZAKL</ID></LoadingLocation><TransportContractDocument><ID>08100293425</ID><TypeCode>MB</TypeCode><Pointer><SequenceNumeric>1</SequenceNumeric><DocumentSectionCode>30B</DocumentSectionCode></Pointer></TransportContractDocument><TransportContractDocument><ID>Q432942</ID><TypeCode>HWB</TypeCode><Pointer><SequenceNumeric>2</SequenceNumeric><DocumentSectionCode>30B</DocumentSectionCode></Pointer></TransportContractDocument><UnloadingLocation><ID>AUSYD</ID></UnloadingLocation></Consignment><Importer /><Invoice><IssueDateTime formatCode=""102"">20130409</IssueDateTime><ID>734242Y</ID><SequenceNumeric>1</SequenceNumeric></Invoice></GoodsShipment><Packaging><SequenceNumeric>1</SequenceNumeric><QuantityQuantity>1</QuantityQuantity><TypeCode>CT</TypeCode></Packaging></Declaration>
</DocumentMetadata>";
			var expectedResult = "00T1PWe5ZoRYTjhWO9zsGy4K9osxaTLOHOzGsjPNId4=";
			AssertEquals("MAC Generated", expectedResult, HMAC256.GenerateNewHMAC(pin, msg));
		}
	}
}
