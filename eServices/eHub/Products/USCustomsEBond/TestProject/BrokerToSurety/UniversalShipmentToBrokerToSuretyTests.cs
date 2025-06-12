using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.USCustoms.eBond.Helpers;
using CargoWise.eHub.Products.USCustoms.eBond.Transforms.UniversalShipment2BrokerToSurety;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.USCustoms.Tests
{
	[TestClass]
	public class UniversalShipmentToBrokerToSuretyTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDecimalPointForFees()
		{
			var sourceFile = "BrokerToSurety.DecimalPointForFees.xml";
			var outputFile = "BrokerToSurety.OutputDecimalPointForFees.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2BrokerToSuretyTest1()
		{
			var sourceFile = "BrokerToSurety.UniversalShipment1.xml";
			var outputFile = "BrokerToSurety.Output1.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2BrokerToSuretyTest2()
		{
			var sourceFile = "BrokerToSurety.UniversalShipment2.xml";
			var outputFile = "BrokerToSurety.Output2.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMinLengthAndOccurs()
		{
			var sourceFile = "BrokerToSurety.UniversalShipment3.xml";
			var outputFile = "BrokerToSurety.Output3.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEntryLineADDVCD()
		{
			var sourceFile = "BrokerToSurety.UniversalShipment4.xml";
			var outputFile = "BrokerToSurety.Output4.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEntryLineAPHISandLaceySendOnlyOneElement()
		{
			var sourceFile = "BrokerToSurety.UniversalShipment5.xml";
			var outputFile = "BrokerToSurety.Output5.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSSNEINCBNappearInImporterNumber()
		{
			var sourceFile = "BrokerToSurety.UniversalShipment6.xml";
			var outputFile = "BrokerToSurety.Output6.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEntryLine_HarmonisedCodeIsEmpty_Ignore()
		{
			var sourceFile = "BrokerToSurety.UniversalShipment7.xml";
			var outputFile = "BrokerToSurety.Output7.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}

		void AssertUShipment2BrokerToSurety(string sourceFile, string outputFile)
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			string transactionID, jobNumber;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + '.' + outputFile))
			using (var reader = XmlReader.Create(stream))
			{
				var output = new XmlDocument();
				output.Load(reader);
				transactionID = output.SelectSingleNode("//*[local-name()='TransactionID']").InnerText;
				jobNumber = output.SelectSingleNode("//*[local-name()='BrokerFilerReference']").InnerText;
			}

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("USCustomsEBond");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("USCEB", "USCustomsEBond", "TESTSENDER__1", transactionID));
			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2019-05-01T10:30:38");

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor},
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDateMapper}
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UShipment2BrokerToSurety>(sourceFile, outputFile);

			mockContextAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}
	}
}
