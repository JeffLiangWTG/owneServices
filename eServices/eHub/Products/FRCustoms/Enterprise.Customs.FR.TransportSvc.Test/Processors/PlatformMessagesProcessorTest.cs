using System.IO;
using Enterprise.Customs.FR.TransportSvc.Messages;
using Enterprise.Customs.FR.TransportSvc.Utilities;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.TransportSvc.Test
{
	[TestFixture]
	class PlatformMessagesProcessorTest
	{
		[Test]
		public void TestTP5Namespaces()
		{
			var inputMessage = TestHelper.GetEmbeddedResourceFileContent("DeltaTPhase5ResponseOutput.xml");
			var expectedMessage = TestHelper.GetEmbeddedResourceFileContent("DeltaTPhase5ResponseOutputWithNamespaces.xml");
			Assert.AreEqual(expectedMessage, ToolBox.UpdateTP5NameSpaces(inputMessage, "CC056C"));
		}

		[Test]
		public void TestGetEhubAddressToUploadTo()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.LogPath = TestHelper.CreateLogDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			ApplicationConfig.Instance.EHubTestEnabled = "0";
			ApplicationConfig.Instance.EHubAddress = "http://ehubAddress.svc";
			ApplicationConfig.Instance.EHubForTestAddress = "http://ehubAddressForTest.svc";
			ApplicationConfig.Instance.ClientsUsingeHubForTest = "HYEDFRCMT, ZZZAAABBB";
			Assert.That(PlatformResponsesProcessor.GetEhubAddressToUploadTo("HYEDFRCMT"), Is.EqualTo("http://ehubAddressForTest.svc"));
			Assert.That(PlatformResponsesProcessor.GetEhubAddressToUploadTo("ZZZAAABBB"), Is.EqualTo("http://ehubAddressForTest.svc"));
			Assert.That(PlatformResponsesProcessor.GetEhubAddressToUploadTo("FRCLIOCL1"), Is.EqualTo("http://ehubAddress.svc"));
		}

		[Test]
		public void TestErrorAcknowledgementTriggersMRJUniversalEvent()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.LogPath = TestHelper.CreateLogDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";

			var inputFilePath = Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "ErrorAcknowledgeFromPlatform.xml");
			File.WriteAllText(inputFilePath, TestHelper.GetEmbeddedResourceFileContent("ErrorAcknowledgeFromPlatform.xml"));

			var log = new Logger();
			Logger.DumpLog();
			var processorMock = new Mock<PlatformResponsesProcessor>(log);
			processorMock.Setup(x => x.GetDateTime()).Returns("TestTime");
			var processor = processorMock.Object;

			Assert.DoesNotThrow(() => processor.ProcessResponses());
			Assert.That(processor.UniversalEventContentForTest, Is.EqualTo(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>CW1SenderID</SenderID>
    <RecipientID>EASYLOG2TEST_EAD</RecipientID >
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key>FA65465166</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>Error</Type>
            <Value>Le numero de sequence est invalide</Value>
          </Context>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>73948932</Value>
          </Context>
          <Context>
            <Type>CorrelationID</Type>
            <Value>999999999</Value>
          </Context>
        </ContextCollection>
        <EventTime>TestTime</EventTime>
        <EventType>FRM</EventType>
        <IsEstimate>false</IsEstimate>
        <EventReference>MRJ</EventReference>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>
"));
		}

		[Test]
		public void TestNegativeAcknowledgementTriggersMNAUniversalEvent()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.LogPath = TestHelper.CreateLogDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";

			var inputFilePath = Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "NegativeAcknowledgeFromPlatform.xml");
			File.WriteAllText(inputFilePath, TestHelper.GetEmbeddedResourceFileContent("NegativeAcknowledgeFromPlatform.xml"));

			var log = new Logger();
			Logger.DumpLog();

			var processorMock = new Mock<PlatformResponsesProcessor>(log);
			processorMock.Setup(x => x.GetDateTime()).Returns("TestTime");
			var processor = processorMock.Object;
			Assert.DoesNotThrow(() => processor.ProcessResponses());
			Assert.That(processor.UniversalEventContentForTest, Is.EqualTo(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>HYEDFRCMT</SenderID>
    <RecipientID>EASYLOG2TEST_EAD</RecipientID >
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key>B230168</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>Error</Type>
            <Value>NACK Erreur MAREVA nï¿½ 7 Le champ partyId n'est pas renseigne ou il contient des caracteres non ASCII  partyId manquant ou invalide.</Value>
          </Context>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>6873</Value>
          </Context>
          <Context>
            <Type>CorrelationID</Type>
            <Value>0000007196</Value>
          </Context>
        </ContextCollection>
        <EventTime>TestTime</EventTime>
        <EventType>FRM</EventType>
        <IsEstimate>false</IsEstimate>
        <EventReference>MNA</EventReference>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>
"));
		}

		[Test]
		public void TestPositiveAcknowledgementTriggersMAKUniversalEvent()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.LogPath = TestHelper.CreateLogDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";

			var inputFilePath = Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "PositiveAcknowledgeFromPlatform.xml");
			File.WriteAllText(inputFilePath, TestHelper.GetEmbeddedResourceFileContent("PositiveAcknowledgeFromPlatform.xml"));

			var log = new Logger();
			Logger.DumpLog();

			var processorMock = new Mock<PlatformResponsesProcessor>(log);
			processorMock.Setup(x => x.GetDateTime()).Returns("TestTime");
			var processor = processorMock.Object;

			Assert.DoesNotThrow(() => processor.ProcessResponses());
			Assert.That(processor.UniversalEventContentForTest, Is.EqualTo(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>CW1SenderID</SenderID>
    <RecipientID>EASYLOG2TEST_EAD</RecipientID >
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key>FA65465166</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>73948932</Value>
          </Context>
          <Context>
            <Type>CorrelationID</Type>
            <Value>999999999</Value>
          </Context>
        </ContextCollection>
        <EventTime>TestTime</EventTime>
        <EventType>FRM</EventType>
        <IsEstimate>false</IsEstimate>
        <EventReference>MAK</EventReference>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>
"));
		}

		[Test]
		public void TestResponseProcessWhenResponseTypeIsUnknown()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.LogPath = TestHelper.CreateLogDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";

			var inputFilePath = Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "UnknownMessageTypeInput.xml");
			File.WriteAllText(inputFilePath, TestHelper.GetEmbeddedResourceFileContent("UnknownMessageTypeInput.xml"));

			var log = new Logger();
			Logger.DumpLog();
			using (var processor = new PlatformResponsesProcessor(log))
			{
				Assert.DoesNotThrow(() => processor.ProcessResponses());

				var logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
				Assert.That(logFileContent, Does.Contain("UnknownMessageTypeInput.xml type (INFO) is unknown."));
				Assert.That(!File.Exists(Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "UnknownMessageTypeInput.xml")), "In case of unknown message type, the input file should be deleted.");
			}
		}

		[Test]
		[Ignore("Unsuitable for DAT")]
		public void TestResponseProcessWhenXMLError()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.LogPath = TestHelper.CreateLogDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			TestHelper.CreateFtpKey(ApplicationConfig.Instance.WorkingDirectory);
			var log = new Logger();
			Logger.DumpLog();

			var testFilePath = Path.Combine(ApplicationConfig.Instance.WorkingDirectory, "bla.xml");
			File.WriteAllText(testFilePath, "bla");

			using (var processor = new PlatformResponsesProcessor(log))
			{
				processor.ProcessResponses();
			}

			var logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
			Assert.That(logFileContent, Does.Contain("Data at the root level is invalid. Line 1, position 1."));
			Assert.That(logFileContent, Does.Contain($"Response file {testFilePath} processed KO."));
		}

		[Test]
		public void TestPNTSResponse()
		{
			AssertProcessResult("PNTSResponseInput.xml", "PNTSResponseOutput.xml");
		}

		[Test]
		public void TestDeltaIEResponse()
		{
			AssertProcessResult("DeltaIEResponseInput.xml", "DeltaIEResponseOutput.xml");
		}

		[Test]
		public void TestDeltaIE917ImportResponse()
		{
			AssertProcessResult("DeltaIE917ImportResponseInput.xml", "DeltaIE917ImportResponseOutput.xml");
		}

		[Test]
		public void TestDeltaIE917ExportResponse()
		{
			AssertProcessResult("DeltaIE917ExportResponseInput.xml", "DeltaIE917ExportResponseOutput.xml");
		}

		[Test]
		public void TestDeltaIEFRA102Response()
		{
			AssertProcessResult("DeltaIEFRA102ResponseInput.xml", "DeltaIEFRA102ResponseOutput.xml");
		}

		[Test]
		public void TestDeltaIEFRA103Response()
		{
			AssertProcessResult("DeltaIEFRA103ResponseInput.xml", "DeltaIEFRA103ResponseOutput.xml");
		}

		[Test]
		public void TestDeltaIE456MultipleResponseInput()
		{
			AssertProcessResult("DeltaIE456MultipleResponseInput.xml", "DeltaIE456MultipleResponseOutput.xml");
		}

		[Test]
		public void TestDeltaGResponse()
		{
			AssertProcessResult("DeltaGResponseInput.xml", "DeltaGResponseOutput.xml");
		}

		[Test]
		public void TestDOAResponse()
		{
			AssertProcessResult("DOAResponseInput.xml", "DOAResponseOutput.xml");
		}

		[Test]

		public void TestDeltaTPhase4ArrivalResponse()
		{
			AssertProcessResult("DeltaTPhase4ArrivalResponseInput.xml", "DeltaTPhase4ArrivalResponseOutput.xml");
		}

		[Test]

		public void TestDeltaTPhase4DepartureResponse()
		{
			AssertProcessResult("DeltaTPhase4DepartureResponseInput.xml", "DeltaTPhase4DepartureResponseOutput.xml");
		}

		[Test]

		public void TestDeltaTPhase5Response()
		{
			AssertProcessResult("DeltaTPhase5ResponseInput.xml", "DeltaTPhase5ResponseOutput.xml");
		}

		public static void AssertProcessResult(string inputFileResource, string expectedOutputResource)
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.LogPath = TestHelper.CreateLogDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";

			var inputFilePath = Path.Combine(ApplicationConfig.Instance.WorkingDirectory, inputFileResource);
			File.WriteAllText(inputFilePath, TestHelper.GetEmbeddedResourceFileContent(inputFileResource));

			var log = new Logger();
			Logger.DumpLog();

			var processorMock = new Mock<PlatformResponsesProcessor>(log);
			processorMock.Setup(x => x.GetTime()).Returns("000000");
			var processor = processorMock.Object;

			var response = PlatformResponsesProcessor.LoadAndTweakResponseXmlFile(inputFilePath);
			response = processor.AddOrUpdateElements(response);
			var transformedResponse = processor.GetTransformedMessage(response, isResponse: true);

			var expectedResult = TestHelper.GetEmbeddedResourceFileContent(expectedOutputResource);
			Assert.That(transformedResponse.ToString(), Is.EqualTo(expectedResult));
		}
	}
}
