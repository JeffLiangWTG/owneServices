using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CsvHelper;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	[TestFixture]
	abstract class NexDocParserTests<T, TConverter>
		where T : RefDataRepoModelEntityType
		where TConverter : CsvToItemCodeSetsConverter
	{
		[Test]
		public void TestEmptyCodeErrorMessage()
		{
			if (this is ITestEmptyCodeErrorMessage parserTester)
			{
				AssertCorrectErrorMessage(new ListCodeSet[] { parserTester.EmptyCodeTestData }, parserTester.ExpectedEmptyCodeErrorMessage);
			}
		}

		[Test]
		public void TestEmptyDescriptionErrorMessage()
		{
			if (this is ITestEmptyDescriptionErrorMessage parserTester)
			{
				AssertCorrectErrorMessage(new ListCodeSet[] { parserTester.EmptyDescriptionTestData }, parserTester.ExpectedEmptyDescriptionErrorMessage);
			}
		}

		[Test]
		public void TestInvalidStartDateErrorMessage()
		{
			if (this is ITestInvalidStartDateErrorMessage parserTester)
			{
				AssertCorrectErrorMessage(new ListCodeSet[] { parserTester.InvalidStartDateTestData }, parserTester.InvalidStartDataErrorMessage);
			}
		}

		[Test]
		public void TestEmptySecondaryCodeErrorMessage()
		{
			if (this is ITestEmptySecondaryCodeErrorMessage parserTester)
			{
				AssertCorrectErrorMessage(new ListCodeSet[] { parserTester.InvalidSecondaryCodeTestData }, parserTester.InvalidSecondaryCodeErrorMessage);
			}
		}

		[Test]
		public void TestDuplicateCodeErrorMessage()
		{
			var duplicateCodeTestData = DuplicateCodeTestData;
			AssertCorrectErrorMessage(new ListCodeSet[] { duplicateCodeTestData, duplicateCodeTestData }, DuplicateCodeErrorMessage);
		}

		protected abstract ListCodeSet DuplicateCodeTestData { get; }
		protected abstract string DuplicateCodeErrorMessage { get; }

		[Test]
		public void TestRESTGenericCodeResultToNativeXML()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream($"CargoWise.RefDbRepo.AUReferenceData.Tests.NexDocs.TestFiles.REST.Input.{CodeSetName}.csv"))
			using (var expectedTestStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.NexDocs.TestFiles.Output." + ExpectedTestFileName))
			using (var reader = new StreamReader(webServiceMockStream))
			using (var csv = new CsvReader(reader))
			{
				var converter = new CsvToListCodeSetsConverter<TConverter>();
				var mockList = converter.Convert(csv).ToArray();
				AssertDownloadAndConvertCodesToXMLFileIsValid(mockList);
			}
		}

		[Test]
		public void TestGenericCodeResultToNativeXML()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.NexDocs.TestFiles.Input." + WebServiceMockResultFileName))
			{
				using (var reader = XmlReader.Create(webServiceMockStream))
				{
					var serializer = new XmlSerializer(typeof(ListCodeSet[]), new XmlRootAttribute("Results"));
					var mockList = (ListCodeSet[])serializer.Deserialize(reader);
					AssertDownloadAndConvertCodesToXMLFileIsValid(mockList);
				}
			}
		}

		[Test]
		public void TestDontOutputXmlWhenThereIsNoFilteredData()
		{
			var parser = ParserToRun(Array.Empty<IListCodeSet>());
			var error = parser.ConvertCodesToXMLFile(TestHelperClass.TestFilesPath, nexDocDateTimeProviderMock.Object);

			var path = Path.Combine(TestHelperClass.TestFilesPath, ExpectedTestFileName);

			if (File.Exists(path))
			{
				File.Delete(path);
			}

			Assert.AreEqual(error, NoDataForImportingErrorMessage);
			Assert.IsFalse(File.Exists(path), "Should not output any xml files when there is no filtered data for importing.");
		}

		void AssertDownloadAndConvertCodesToXMLFileIsValid(IListCodeSet[] mockList)
		{
			var codeSet = mockList[0];
			var items = codeSet.Items.ToList();
			var endDateItem = items.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.EndDate);
			if (endDateItem != null)
			{
				var list = mockList.ToList();
				list.Remove(codeSet);
				var codeSetWithEndDate = new Mock<IListCodeSet>();
				codeSetWithEndDate.Setup(x => x.Position).Returns(codeSet.Position);
				var endDateItemIndex = items.IndexOf(endDateItem);
				items.RemoveAt(endDateItemIndex);
				var endDateItemMock = new Mock<IItemCodeSet>();
				endDateItemMock.Setup(x => x.Key).Returns(endDateItem.Key);
				endDateItemMock.Setup(x => x.ValueType).Returns(endDateItem.ValueType);
				endDateItemMock.Setup(x => x.Value).Returns("2030-12-31T23:59:49.000");
				items.Insert(endDateItemIndex, endDateItemMock.Object);
				codeSetWithEndDate.Setup(x => x.Items).Returns(items.ToArray());
				list.Insert(0, codeSetWithEndDate.Object);
				mockList = list.ToArray();
			}

			Assert.That(mockList.Length, Is.EqualTo(WebServiceMockResultCount));
			ParserToRun(mockList).ConvertCodesToXMLFile(TestHelperClass.TestFilesPath, nexDocDateTimeProviderMock.Object);
			using (var expectedTestStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.NexDocs.TestFiles.Output." + ExpectedTestFileName))
			using (var converterResultStream = new FileStream(Path.Combine(TestHelperClass.TestFilesPath, ExpectedTestFileName), FileMode.Open))
			using (var expectedReader = new StreamReader(expectedTestStream))
			using (var converterReader = new StreamReader(converterResultStream))
			{
				Assert.AreEqual(expectedReader.ReadToEnd(), converterReader.ReadToEnd());
			}
		}

		protected void AssertCorrectErrorMessage(ListCodeSet[] mockListToReturn, string expectedErrorMessage)
		{
			var errors = ParserToRun(mockListToReturn).ConvertCodesToXMLFile(TestHelperClass.TestFilesPath, nexDocDateTimeProviderMock.Object);
			Assert.IsTrue(errors.Contains(expectedErrorMessage), $@"Expected: {expectedErrorMessage}{System.Environment.NewLine}Actual: {errors}");
		}

		protected abstract string CodeSetName { get; }

		protected abstract string WebServiceMockResultFileName { get; }

		protected abstract int WebServiceMockResultCount { get; }

		protected virtual string ExpectedTestFileName => "RefCusCodeListZZ_AU_" + CodeSetName + ".xml";

		protected abstract Func<IListCodeSet[], BaseNexDocCodeParser<T>> ParserToRun { get; }

		protected const string NoDataForImportingErrorMessage = "There is no filtered data for importing.\r\n";

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			nexDocDateTimeProviderMock = new Mock<IDateTimeProvider>();
			nexDocDateTimeProviderMock.Setup(x => x.CurrentLocalDateTime).Returns(new DateTime(2019, 2, 26, 18, 51, 45));
		}
		Assembly assembly;
		Mock<IDateTimeProvider> nexDocDateTimeProviderMock;
	}
}
