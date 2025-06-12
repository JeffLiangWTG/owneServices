using System;
using System.IO;
using System.Xml;
using CargoWise.eHub.Adapter;
using Enterprise.Customs.FR.TransportSvc.Messages;
using Enterprise.Customs.FR.TransportSvc.Utilities;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.TransportSvc.Test
{
	[TestFixture]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypesAnalyzer")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA3075:DoNotUseInsecureDtdProcessingAnalyzer")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Class names should begin with uppercase")]
	class eHubMessagesProcessorTest
	{
		[Test]
		public void TestMDLUniversalEventSentWhenMessageDeliveredToPlatform()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			var log = new Logger();
			Logger.DumpLog();

			var messageDocument = new XmlDocument();
			messageDocument.Load(TestHelper.GetEmbeddedResourceFileStream("MessageCDecImpInput.xml"));

			using (var eHubMessage = new IeHubMessageCustom(ToolBox.GetElementTextByTagNameSafely(messageDocument, "SenderID"), ToolBox.GetElementTextByTagNameSafely(messageDocument, "RecipientID"), "GMD", new Guid(), messageDocument.InnerXml))
			{
				var processorMock = new Mock<eHubMessagesProcessor>(log);
				processorMock.Setup(x => x.GetDateTime()).Returns("TestTime");
				processorMock.Setup(m => m.Process(It.IsAny<IeHubMessage>())).CallBase();
				var processor = processorMock.Object;

				processor.Process(eHubMessage);
				Assert.That(processor.UniversalEventContentForTest, Is.EqualTo(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EASYLOG2_EAD</SenderID>
    <RecipientID>CL7101LIO</RecipientID >
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key>SNTE019666</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>147242</Value>
          </Context>
          <Context>
            <Type>CorrelationID</Type>
            <Value>0000068569</Value>
          </Context>
        </ContextCollection>
        <EventTime>TestTime</EventTime>
        <EventType>FRM</EventType>
        <IsEstimate>false</IsEstimate>
        <EventReference>MDL</EventReference>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>
"));
			}
		}

		[Test]
		public void TestIsNetworkException()
		{
			var expectedNetworkExceptionMessageList = new string[]
			{
				"Erreur r�seau inattendue.",
				"Unexpected network error.",
				"La connexion sous-jacente a �t� ferm�e.",
				"The underlying connection",
				"Impossible d'�tablir un canal s�curis�",
				"secure channel",
				"Il n'existait pas de point de terminaison",
				"ending point",
				"There was no endpoint",
				"Client not connected",
				"A socket operation",
				"The HTTP service located at",
				"The request channel timed out",
			};

			foreach (var exceptionMessage in expectedNetworkExceptionMessageList)
			{
				var exception = new Exception(exceptionMessage);
				Assert.That(eHubMessagesProcessor.IsNetworkException(exception));
			}
		}

		[Test]
		public void TestAddPrunedEmbeddedJsonMessage()
		{
			var messageDocument = new XmlDocument();
			messageDocument.Load(TestHelper.GetEmbeddedResourceFileStream("MessageDeltaIEInput.xml"));
			var prunedInputContent = eHubMessagesProcessor.AddPrunedEmbeddedJsonMessage(messageDocument).InnerXml;
			Assert.That(prunedInputContent, Does.Contain(@"<MessageJson xmlns=""http://cargowise.com/ehub/core/genericmessagedelivery"">{""ImportOperation"":{""LRN"":""CL7101LIO0000104776"",""declarationType"":""IM"",""additionalDeclarationType"":""A"",""presentationNotificationEstimatedDateAndTime"":""2023-03-23T00:00:00"",""languageCode"":""FR""},""CustomsOfficeOfPresentation"":{""referenceNumber"":""""},""SupervisingCustomsOffice"":{""referenceNumber"":""FR002300""},""Importer"":{""identificationNumber"":""FR123566654""},""Declarant"":{""identificationNumber"":""FR33159700500064""},""PersonProvidingAGuarantee"":{""identificationNumber"":""""},""PersonPayingCustomsDuty"":{""identificationNumber"":""""},""Representative"":{""identificationNumber"":"""",""status"":""2""},""CurrencyExchange"":{""internalCurrencyUnit"":""EUR""},""GoodsShipment"":[{""sequenceNumber"":""1"",""natureOfTransaction"":""11"",""totalAmountInvoiced"":1500.0,""invoiceCurrency"":""EUR"",""dateOfAcceptance"":""2023-03-23T00:00:00"",""exchangeRate"":1.0,""Exporter"":{""identificationNumber"":""FR6546546546""},""DeliveryTerms"":{""incotermCode"":""FOB"",""UNLOCODE"":""FRPAR"",""location"":"""",""country"":""FR"",""text"":""""},""CountryOfDispatch"":{""countryOfDispatch"":""IL""},""Destination"":{""countryOfDestination"":""FR"",""regionOfDestination"":"""",""ccQualifier"":""""},""SupportingDocument"":[{""sequenceNumber"":""0"",""type"":""N380"",""ccQualifier"":""FR"",""referenceNumber"":""546848"",""documentLineItemNumber"":""0"",""issuingAuthorityName"":"""",""dateOfValidity"":""""}],""Consignment"":{""containerIndicator"":""LCL"",""inlandModeOfTransport"":""ROA"",""modeOfTransportAtTheBorder"":""SEA"",""grossMass"":20.0,""referenceNumberUCR"":""3FR33159700500064-B228185"",""LocationOfGoods"":{""typeOfLocation"":"""",""qualifierOfIdentification"":""""},""ArrivalTransportMeans"":{""typeOfIdentification"":""0"",""identificationNumber"":""""},""ActiveBorderTransportMeans"":{""nationality"":""""}},""GoodsShipmentItem"":[{""sequenceNumber"":""1"",""declarationGoodsItemNumber"":""1"",""statisticalValue"":1500.0,""natureOfTransaction"":""1"",""referenceNumberUCR"":"""",""dateOfAcceptance"":""2023-03-23T00:00:00"",""Procedure"":{""requestedProcedure"":""40"",""previousProcedure"":""00"",""AdditionalProcedure"":[{""sequenceNumber"":"""",""additionalProcedure"":""000"",""ccQualifier"":""FR""}]},""Origin"":{""countryOfOrigin"":""IL"",""countryOfPreferentialOrigin"":""IL""},""CountryOfDispatch"":{""countryOfDispatch"":""ILTLV""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""FR""},""Commodity"":{""descriptionOfGoods"":""MACHINES, APPAREILS ET MATÉRIELS ÉLECTRIQUES ET LEURS PARTIES; APPAREILS D'ENREGISTREMENT OU DE REPRODUCTION DU SON, APPAREILS D'ENREGISTREMENT OU DE REPRODUCTION DES IMAGES ET DU SON EN TÉLÉVISION, ET PARTIES ET ACCESSOIRES DE CES APPAREILS TRANSFORMATEURS ÉLECTRIQUES, CONVERTISSEURS ÉLECTRIQUES STATIQUES (REDRESSEURS, PAR EXEMPLE), BOBINES DE RÉACTANCE ET SELFS CONVERTISSEURS STATIQUES REDRESSEURS AUTRES"",""CUSCode"":"""",""quotaOrderNumber"":"""",""CommodityCode"":{""harmonizedSystemSubheadingCode"":""850440"",""combinedNomenclatureCode"":""83"",""taricCode"":""90"",""TaricAdditionalCode"":[{""sequenceNumber"":"""",""taricAdditionalCode"":""""}],""NationalAdditionalCode"":[{""sequenceNumber"":"""",""nationalAdditionalCode"":"""",""ccQualifier"":""FR""}]},""GoodsMeasure"":{""nationalMeasurementUnitAndQualifier"":""""},""InvoiceLine"":{""itemAmountInvoiced"":1500.0},""CalculationOfTaxes"":{""preference"":""100"",""DutiesAndTaxe"":[{""sequenceNumber"":"""",""taxType"":""B00"",""ccQualifier"":""FR"",""nationalTaxType"":""A445"",""payableTaxAmount"":300.0,""methodOfPayment"":"""",""TaxBase"":[{""sequenceNumber"":"""",""taxRate"":20.0,""measurementUnitAndQualifier"":""%"",""quantity"":20.0,""amount"":1500.0,""taxAmount"":300.0}]}]}},""Packaging"":[{""sequenceNumber"":"""",""typeOfPackages"":""PK"",""numberOfPackages"":""1"",""shippingMarks"":""234324""}],""SupportingDocument"":[{""sequenceNumber"":""0"",""type"":""Y969"",""ccQualifier"":""FR"",""referenceNumber"":""654654"",""documentLineItemNumber"":""0"",""issuingAuthorityName"":"""",""dateOfValidity"":"""",""currency"":""""}],""CustomsValuation"":{""valuationMethod"":""1""},""ValuationAdjustment"":{""valuationIndicators"":""0000""}}]}]}</MessageJson>"));
		}

		[Test]
		[Ignore("Unsuitable for DAT")]
		public void TestProcessFailure()
		{
			var tempDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.WorkingDirectory = tempDirectory;
			ApplicationConfig.Instance.LogPath = tempDirectory + "/Logs";
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			TestHelper.CreateFtpKey(tempDirectory);
			var log = new Logger();
			Logger.DumpLog();

			File.WriteAllText(Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "10000000-0000-0000-0000-000000000000.xml"), TestHelper.GetEmbeddedResourceFileContent("CIN750MessageInput.xml"));

			var processorMock = new Mock<eHubMessagesProcessor>(log);
			processorMock.Setup(x => x.GetLongTime()).Returns("000000000");
			processorMock.Setup(x => x.GetDate()).Returns("20220101");
			processorMock.Setup(x => x.GetDateTime()).Returns("TestTime");

			var ex = new Exception("Some network exception");
			processorMock.Setup(x => x.Process(It.IsAny<IeHubMessage>())).Throws(new Exception("Some network exception"));

			var processor = processorMock.Object;
			processor.ProcessIncomingCW1Messages();
			var logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
			Assert.That(logFileContent, Does.Contain("Some network exception"));
			Assert.That(logFileContent, Does.Contain("Sending a MND notification back to CW1."));
			Assert.That(logFileContent, Does.Contain("MND notification sent to CW1."));
			Assert.That(logFileContent, Does.Contain("eHub message 10000000-0000-0000-0000-000000000000 processed KO"));
			Assert.That(File.Exists(Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "10000000-0000-0000-0000-000000000000.xml")), "In case of a network exception, the input file should still be there to be reprocessed later on.");
			Assert.That(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "10000000-0000-0000-0000-000000000000.xml")), Is.EqualTo(TestHelper.GetEmbeddedResourceFileContent("CIN750MessageInput.xml")), "File to replay should match input file content, not the tranformed one.");
			Assert.That(processor.UniversalEventContentForTest, Is.EqualTo(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EASYLOG2_EAD</SenderID>
    <RecipientID>HYEDFRCMT</RecipientID >
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key></Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>Error</Type>
            <Value>Some network exception</Value>
          </Context>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>147242</Value>
          </Context>
          <Context>
            <Type>CorrelationID</Type>
            <Value>0000068569</Value>
          </Context>
        </ContextCollection>
        <EventTime>TestTime</EventTime>
        <EventType>FRM</EventType>
        <IsEstimate>false</IsEstimate>
        <EventReference>MND</EventReference>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>
"));
		}

		[Test]
		[Ignore("Unsuitable for DAT")]
		public void TestUnknownSchemaId()
		{
			var tempDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.WorkingDirectory = tempDirectory;
			ApplicationConfig.Instance.LogPath = tempDirectory + "/Logs";
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			TestHelper.CreateFtpKey(tempDirectory);
			var log = new Logger();
			Logger.DumpLog();

			File.WriteAllText(Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "10000000-0000-0000-0000-000000000000.xml"), TestHelper.GetEmbeddedResourceFileContent("UnknownSchemaIDInput.xml"));

			var processorMock = new Mock<eHubMessagesProcessor>(log);
			processorMock.Setup(x => x.GetLongTime()).Returns("000000000");
			processorMock.Setup(x => x.GetDate()).Returns("20220101");
			processorMock.Setup(m => m.Process(It.IsAny<IeHubMessage>())).CallBase();

			var processor = processorMock.Object;
			processor.ProcessIncomingCW1Messages();
			var logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
			Assert.That(logFileContent, Does.Contain("MND notification not sent to CW1. The event would not stick to any business object in CW1"));
			Assert.That(logFileContent, Does.Contain("eHub message 10000000-0000-0000-0000-000000000000 processed KO"));
		}

		[Test]
		public void TestEmptyMessage()
		{
			var tempDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.WorkingDirectory = tempDirectory;
			ApplicationConfig.Instance.LogPath = tempDirectory + "/Logs";
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			var log = new Logger();
			Logger.DumpLog();

			File.WriteAllText(Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "10000000-0000-0000-0000-000000000000.xml"), string.Empty);

			using (var processor = new eHubMessagesProcessor(log))
			{
				processor.ProcessIncomingCW1Messages();
			}
			var logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
			Assert.That(logFileContent, Does.Contain($"skipping file {ApplicationConfig.Instance.WorkingDirectory}\\10000000-0000-0000-0000-000000000000.xml because of error : Root element is missing."));
		}

		[Test]
		public void TestPNTSMessage()
		{
			AssertProcessResult("PNTSMessageInput.xml", "PNTSMessageOutput.xml");
		}

		[Test]
		public void TestDeltaIEMessage()
		{
			AssertProcessResult("MessageDeltaIEInput.xml", "MessageDeltaIEOutput.xml");
		}

		[Test]
		public void TestDeltaIE414Message()
		{
			AssertProcessResult("MessageDeltaIE414Input.xml", "MessageDeltaIE414Output.xml");
		}

		[Test]
		public void TestDeltaTPhase5Message()
		{
			AssertProcessResult("DeltaTPhase5MessageInput.xml", "DeltaTPhase5MessageOutput.xml");
		}

		[Test]
		public void TestCIN750Message()
		{
			AssertProcessResult("CIN750MessageInput.xml", "CIN750MessageOutput.xml");
		}

		[Test]
		public void TestDeltaGMessage()
		{
			AssertProcessResult("MessageCDecImpInput.xml", "MessageCDecImpOutput.xml");
		}

		static void AssertProcessResult(string inputFileResource, string expectedOutputResource)
		{
			var tempDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.WorkingDirectory = tempDirectory;
			ApplicationConfig.Instance.LogPath = tempDirectory + "/Logs";
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			var log = new Logger();
			Logger.DumpLog();

			var messageDocument = new XmlDocument();
			messageDocument.Load(TestHelper.GetEmbeddedResourceFileStream(inputFileResource));

			var processorMock = new Mock<eHubMessagesProcessor>(log);
			processorMock.Setup(x => x.GetLongTime()).Returns("000000000");
			processorMock.Setup(x => x.GetDate()).Returns("20220101");

			var processor = processorMock.Object;
			processor.AddEnveloppeEasylogInformation(messageDocument);
			messageDocument = eHubMessagesProcessor.PerformAdditionalChanges(messageDocument);
			messageDocument = eHubMessagesProcessor.RemoveMessageNameSpaces(messageDocument);
			var transformedMessage = processor.GetTransformedMessage(messageDocument, isResponse: false).ToString();

			var expectedResult = TestHelper.GetEmbeddedResourceFileContent(expectedOutputResource);
			Assert.That(transformedMessage, Is.EqualTo(expectedResult));
		}

		[Test]
		[Ignore("Unsuitable for DAT")]
		public void TestProcessOfNewMessagesFromEhub()
		{
			var tempDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.WorkingDirectory = tempDirectory;
			ApplicationConfig.Instance.LogPath = tempDirectory + "/Logs";
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			TestHelper.CreateFtpKey(tempDirectory);
			var log = new Logger();
			Logger.DumpLog();

			File.WriteAllText(Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "10000000-0000-0000-0000-000000000000.xml"), TestHelper.GetEmbeddedResourceFileContent("MessageCDecImpInput.xml"));
			File.WriteAllText(Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "20000000-0000-0000-0000-000000000000.xml"), TestHelper.GetEmbeddedResourceFileContent("MessageCDecImpInput.xml"));

			var processorMock = new Mock<eHubMessagesProcessor>(log);
			processorMock.Setup(x => x.GetLongTime()).Returns("000000000");
			processorMock.Setup(x => x.GetDate()).Returns("20220101");
			processorMock.Setup(m => m.Process(It.IsAny<IeHubMessage>())).CallBase();

			var processor = processorMock.Object;
			processor.ProcessIncomingCW1Messages();
			var logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
			Assert.That(logFileContent, Does.Contain("eHub message 10000000-0000-0000-0000-000000000000 processed OK"));
			Assert.That(logFileContent, Does.Contain("eHub message 20000000-0000-0000-0000-000000000000 processed OK"));
		}
	}
}
