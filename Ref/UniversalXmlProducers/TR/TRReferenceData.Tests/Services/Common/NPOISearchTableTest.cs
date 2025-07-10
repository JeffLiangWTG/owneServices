using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using NPOI.SS.UserModel;
using NUnit.Framework;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Tests
{
	class NPOISearchTableTest
	{
		[TestCaseSource(nameof(FindTestData))]
		public void TestFind(string[] headers, (string Header, string Value)[] filters, string[][] expected, string message)
		{
			Assert.That(searchTable.Find(headers, filters), Is.EquivalentTo(expected), message);
		}

		[Test]
		public void Constructor_ValidInput_InitializesCorrectly()
		{
			var normalizedTableHeaders = tableHeaders.Select(StringExtensions.ToSearchKey);
			var normalizedSearchHeaders = searchHeaders.Select(StringExtensions.ToSearchKey);

			Assert.IsNotNull(searchTable);
			Assert.AreEqual(101, searchTable.Length, "Should skip the rows that does not have data in all tableHeaders");

			Assert.IsNotNull(searchTable.SearchDictionary_Exposed);
			Assert.That(searchTable.SearchDictionary_Exposed.Count, Is.EqualTo(5), "SearchDictionary should reflect supplied searchHeaders");
			Assert.That(normalizedSearchHeaders.Except(searchTable.SearchDictionary_Exposed.Keys), Is.Empty, "SearchDictionary should reflect supplied searchHeaders");

			Assert.IsNotNull(searchTable.HeadersDictionary_Exposed);
			Assert.That(searchTable.HeadersDictionary_Exposed.Count, Is.EqualTo(7), "HeadersDictionary should reflect supplied tableHeaders");
			Assert.That(normalizedTableHeaders.Except(searchTable.HeadersDictionary_Exposed.Keys), Is.Empty, "HeadersDictionary should reflect supplied tableHeaders");
		}

		[Test]
		public void Constructor_NonNullableParameters_ThrowsArgumentNullException()
		{
			var sheet = Mock.Of<ISheet>();

			Assert.Throws<ArgumentNullException>(() => _ = new NPOISearchTableForTesting(null, logger, tableHeaders, searchHeaders));
			Assert.Throws<ArgumentNullException>(() => _ = new NPOISearchTableForTesting(sheet, null, tableHeaders, searchHeaders));
			Assert.Throws<ArgumentNullException>(() => _ = new NPOISearchTableForTesting(sheet, logger, null, searchHeaders));
			Assert.Throws<ArgumentNullException>(() => _ = new NPOISearchTableForTesting(sheet, logger, tableHeaders, null));
		}

		[Test]
		public void Constructor_SearchHeadersNotInTableHeaders_ThrowsArgumentException()
		{
			var wrongSearchHeaders = new[] { "Header1", "NonExistingHeader" };

			var mockRow1 = new Mock<IRow>();
			mockRow1.Setup(row => row.PhysicalNumberOfCells).Returns(2);

			var mockSheet = new Mock<ISheet>();
			mockSheet.Setup(sheet => sheet.GetRow(0)).Returns(mockRow1.Object);
			mockSheet.Setup(sheet => sheet.PhysicalNumberOfRows).Returns(1);

			Assert.Throws<ArgumentException>(() => _ = new NPOISearchTableForTesting(mockSheet.Object, logger, tableHeaders, wrongSearchHeaders.ToHashSet()));
		}

		[Test]
		public void Find_NoFilters_ReturnsAllRows()
		{
			var result = searchTable.Find(tableHeaders.ToArray<string>());

			Assert.IsNotNull(result);
			Assert.AreEqual(101, result.Length);
		}

		[Test]
		public void Find_NonExistingHeaders_LogsError()
		{
			var result = searchTable.Find(headers: new[] { "Header", "NonExistentHeader2", "NonExistentHeader" }, filters: new[] { ("NonExistentHeader2", "NonExistentValue") });

			Assert.That(result, Is.Empty);
			loggerMock.Verify(l => l.Log(
					It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Unrecognized Headers: NONEXISTENTHEADER2, NONEXISTENTHEADER" && @type.Name == "FormattedLogValues"),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Test]
		public void Find_NonExistingFilters_LogsError()
		{
			var result = searchTable.Find(headers: tableHeaders.ToArray<string>(), filters: new[]
			{
				("Header1", "Value11"),
				("NonExistentHeader2", "NonExistentValue"),
				("NonExistentHeader", "NonExistentValue"),
			});

			Assert.That(result, Is.Empty);
			loggerMock.Verify(l => l.Log(
					It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Unrecognized Search Headers: NONEXISTENTHEADER2, NONEXISTENTHEADER" && @type.Name == "FormattedLogValues"),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Test]
		public void Find_NoMatchingFilters_LogsError()
		{
			var result = searchTable.Find(headers: tableHeaders.ToArray<string>(), filters: new[] { ("Header2", "Value99") });

			Assert.That(result, Is.Empty);
			loggerMock.Verify(l => l.Log(
					It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Unable to find result, sheet:Sheet2 filters: (Header2, Value99)" && @type.Name == "FormattedLogValues"),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Test]
		public void IsValid_ReturnsTrue()
		{
			var searchTable = new NPOISearchTableForTesting(Mock.Of<ISheet>(), logger, new HashSet<string>(), new HashSet<string>());
			var mockRow = new Mock<IRow>();

			Assert.That(searchTable.IsValid_Exposed(null));
			Assert.That(searchTable.IsValid_Exposed(mockRow.Object));
		}

		static IEnumerable<TestCaseData> FindTestData
		{
			get
			{
				yield return new TestCaseData(
					new[] { "Number", "Date" },
					new[] { ("Header", "Value") },
					new[] {
						new[] { "1234.56", "2024-04-01T14:15:16" },
						new[] { "2345.68", "2024-04-02T16:17:18" }
					},
					"Should filter correctly"
				);
				yield return new TestCaseData(
					new[] { "Number", "Date", "Header4" },
					new[] { ("Header", "Khaki") },
					new[] {
						new[] { "31.87", "2022-10-18T00:00:00" , "NULL"},
						new[] { "32.95", "2021-12-10T00:00:00" , "和製漢語"},
						new[] { "20.27", "2023-07-26T00:00:00" , "𠜎𠜱𠝹𠱓𠱸𠲖𠳏"},
						new[] { "83.85", "2022-11-07T00:00:00" , "｀ｨ(´∀｀∩"},
					},
					"Should support non-unicode character sets."
				);
				yield return new TestCaseData(
					new[] { "Number", "Date", "Header4" },
					new[] { ("Header", "Khaki"), ("Header2", "donec") },
					new[] {
						new[] { "20.27", "2023-07-26T00:00:00" , "𠜎𠜱𠝹𠱓𠱸𠲖𠳏"},
					},
					"Should filter based on all filters."
				);
				yield return new TestCaseData(
					new[] { "Number", "Date" },
					new[] { ("Header", "Puce"), ("Header1", "tellus nisi") },
					new[] {
						new[] { "75.61", "2021-12-29T00:00:00"},
					},
					"[Precondition]: Should return result set if filter criteria exists."
				);
				yield return new TestCaseData(
					new[] { "Number", "Date" },
					new[] { ("Header", "Puce"), ("Header1", "Non Existent Value") },
					Array.Empty<string[]>(),
					"Should return empty result set if no value with filter criteria exists."
				);
				yield return new TestCaseData(
					new[] { "Number", "Date" },
					new[] { ("Header", "Green"), ("Header1", "pretium iaculis") },
					new[] {
						new[] { "5.65", string.Empty},
					},
					"Should return value even if it's empty."
				);
			}
		}

		[OneTimeSetUp]
		public void LoadSheet()
		{
			loggerMock = new Mock<ILogger>();
			logger = loggerMock.Object;
			tableHeaders = new HashSet<string> { "Header", "Header1", "Header2", "Header3", "Header4", "Number", "Date" };
			searchHeaders = new HashSet<string> { "Header", "Header1", "Header2", "Header3", "Header4" };

			var testSheet = WorkbookFactory.Create(
				Assembly.GetExecutingAssembly().GetManifestResourceStream(NPOIExtensionsTest.NPOITestFileName)
			).GetSheetAt(1);
			searchTable = new NPOISearchTableForTesting(
				testSheet,
				logger,
				tableHeaders,
				searchHeaders
			);
		}
		Mock<ILogger> loggerMock;
		ILogger logger;
		HashSet<string> tableHeaders;
		HashSet<string> searchHeaders;
		NPOISearchTableForTesting searchTable;
	}

	class NPOISearchTableForTesting : NPOISearchTable
	{
		public NPOISearchTableForTesting(ISheet sheet, ILogger logger, HashSet<string> tableHeaders, HashSet<string> searchHeaders) : base(sheet, logger, tableHeaders, searchHeaders, GetCellFormat)
		{
		}

		static string GetCellFormat(string headerKey)
		{
			switch (headerKey)
			{
				case "DATE":
					return "s";
				case "NUMBER":
					return "0.##";
				default:
					return null;
			}
		}

		internal Dictionary<string, (int ColumnIndex, Dictionary<string, HashSet<int>> SheetRows)> SearchDictionary_Exposed => SearchDictionary;

		internal Dictionary<string, int> HeadersDictionary_Exposed => HeadersDictionary;

		internal bool IsValid_Exposed(IRow row) => base.IsValid(row);
	}
}
