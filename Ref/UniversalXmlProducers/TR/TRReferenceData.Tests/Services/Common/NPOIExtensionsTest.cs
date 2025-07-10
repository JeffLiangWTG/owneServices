using System;
using System.Collections.Generic;
using System.Reflection;
using NPOI.SS.UserModel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Tests
{
	class NPOIExtensionsTest
	{
		[TestCaseSource(nameof(GetTextOrMergedTestCases))]
		public void TestGetTextOrMerged(int rowIndex, int columnIndex, string expected)
		{
			var cell = testSheet.GetRow(rowIndex).GetCell(columnIndex);
			Assert.That(NPOIExtensions.GetTextOrMerged(cell), Is.EqualTo(expected));
		}

		static IEnumerable<TestCaseData> GetTextOrMergedTestCases
		{
			get
			{
				yield return new TestCaseData(0, 0, "A1");
				yield return new TestCaseData(1, 0, null);
				yield return new TestCaseData(0, 1, null);
				for (int row = 1; row <= 4; row++)
				{
					for (int column = 1; column <= 5; column++)
					{
						yield return new TestCaseData(row, column, "Merged from B2 to F5");
					}
				}
				yield return new TestCaseData(5, 0, null);
				yield return new TestCaseData(5, 5, null);
				yield return new TestCaseData(5, 6, new DateTime(2024, 4, 1, 16, 0, 0).ToString("o"));
			}
		}

		[TestCaseSource(nameof(ValueToStringTestCases))]
		public void TestValueToString(int row, int column, string format, string expected)
		{
			var cell = testSheet.GetRow(row).GetCell(column);
			Assert.That(NPOIExtensions.ValueToString(cell, format), Is.EqualTo(expected));
		}

		static IEnumerable<TestCaseData> ValueToStringTestCases
		{
			get
			{
				yield return new TestCaseData(0, 0, null, "A1");
				yield return new TestCaseData(5, 6, "o", new DateTime(2024, 4, 1, 16, 0, 0).ToString("o"));
				yield return new TestCaseData(5, 6, "s", new DateTime(2024, 4, 1, 16, 0, 0).ToString("s"));
			}
		}

		[TestCaseSource(nameof(ToSearchKeyTestCases))]
		public void TestToSearchKey(int row, int column, string expected)
		{
			var cell = testSheet.GetRow(row).GetCell(column);
			Assert.That(cell.ToSearchKey(), Is.EqualTo(expected));
		}

		static IEnumerable<TestCaseData> ToSearchKeyTestCases
		{
			get
			{
				yield return new TestCaseData(0, 0, "A1");
				yield return new TestCaseData(1, 1, "MERGEDFROMB2TOF5");
				yield return new TestCaseData(6, 7, "H2");
				yield return new TestCaseData(7, 8, "STRINGTOTRIM");
			}
		}

		[TestCaseSource(nameof(GetMergedTextTestCases))]
		public void TestGetMergedText(int row, int column, string expected)
		{
			var cell = testSheet.GetRow(row)?.GetCell(column);
			var result = NPOIExtensions.GetMergedText(cell);

			Assert.That(result, Is.EqualTo(expected));
		}

		static IEnumerable<TestCaseData> GetMergedTextTestCases
		{
			get
			{
				yield return new TestCaseData(0, 0, string.Empty);
				for (int row = 1; row <= 4; row++)
				{
					for (int column = 1; column <= 5; column++)
					{
						yield return new TestCaseData(row, column, "Merged from B2 to F5");
					}
				}
				yield return new TestCaseData(5, 6, string.Empty);
			}
		}

		[TestCaseSource(nameof(IsEmptyTestCases))]
		public void TestIsEmpty(int row, int column, bool expected)
		{
			var cell = testSheet.GetRow(row)?.GetCell(column);
			var result = NPOIExtensions.IsEmpty(cell);

			Assert.That(result, Is.EqualTo(expected));
		}

		static IEnumerable<TestCaseData> IsEmptyTestCases
		{
			get
			{
				yield return new TestCaseData(0, 0, false);
				yield return new TestCaseData(2, 2, true);
				yield return new TestCaseData(4, 4, true);
			}
		}


		[OneTimeSetUp]
		public void LoadSheet()
		{
			testSheet = WorkbookFactory.Create(
				Assembly.GetExecutingAssembly().GetManifestResourceStream(NPOITestFileName)
			).GetSheetAt(0);
		}
		ISheet testSheet;

		public const string NPOITestFileName = "CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.NPOITest.xlsx";
	}
}
