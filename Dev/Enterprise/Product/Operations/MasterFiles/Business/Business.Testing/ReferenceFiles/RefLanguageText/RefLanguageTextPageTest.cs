using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefLanguageTextPage))]
	sealed class RefLanguageTextPageTest : NonPersistentBusinessObjectTestCase
	{
		RefLanguageTextPage GetPageInstance(ZString caption)
		{
			return new RefLanguageTextPage(testHelper.ParentTableCode, testHelper.ColumnToTest, testHelper.GetMultilingualLanguage(caption), testHelper.GetContextObject(Factory, caption), "", testHelper.GetFilterColumns());
		}

		public void TestImportEntries()
		{
			var pageRef = GetPageInstance("two");
			var page = GetPageInstance("two");
			var edited =
				"\"Language\",\"Original\",\"Language\",\"Translation\"\r\n"
				+ "\"EN-US\",\"zero\",\"RU-RU\",\"0\"\r\n"
				+ "\"EN-US\",\"one\",\"AR-AE\",\"1\"\r\n"
				+ "\"EN-US\",\"two\",\"FR-FR\",\"2\"\r\n"
				+ "\"EN-US\",\"three\",\"ZH-CN\",\"3\"\r\n"
				+ "\"EN-US\",\"bamboozle\",\"ZZZ\",\":-)\"\r\n";

			using (var tempfile = TempFile.NewWithExtension("csv"))
			{
				File.WriteAllText(tempfile.Filename, edited);
				var error = page.ImportEntries(tempfile.Filename);

				AssertEquals(string.Empty, error);
				AssertEquals("should not add additional entries", pageRef.All.Count, page.All.Count);
			}

			foreach (RefLanguageTextPageEntry entry in page.All)
			{
				var refItem = pageRef.All.Cast<RefLanguageTextPageEntry>().FirstOrDefault(e => e.English.Equals(entry.English) && e.Language.Equals(entry.Language));
				AssertNotNull("should not add additional entries", refItem);

				if (entry.Language == SharedConstants.Languages.Russian && entry.English == "zero")
				{
					AssertEquals("0", entry.Translation);
				}
				else if (entry.Language == SharedConstants.Languages.Arabic && entry.English == "one")
				{
					AssertEquals("1", entry.Translation);
				}
				else if (entry.Language == SharedConstants.Languages.French && entry.English == "two")
				{
					AssertEquals("2", entry.Translation);
				}
				else if (entry.Language == SharedConstants.Languages.ChineseSimplified && entry.English == "three")
				{
					AssertEquals("3", entry.Translation);
				}
				else
				{
					AssertEquals(refItem.Translation, entry.Translation);
				}
			}
		}

		public void TestImportEntries_ReadOnly()
		{
			var pageRef = GetPageInstance("two");
			var page = GetPageInstance("two");
			page.ReadOnly = true;

			var edited =
				@"Language|Original|Language|Translation
EN-US|zero|RU-RU|0
EN-US|one|AR-AE|1
EN-US|two|FR-FR|2
EN-US|three|ZH-CN|3
EN-US|bamboozle|ZZZ|:-)";

			var error = page.ImportEntries(edited);

			AssertEquals("Editing is not allowed", error);
			AssertEquals("should not add additional entries", pageRef.All.Count, page.All.Count);

			foreach (RefLanguageTextPageEntry entry in page.All)
			{
				var refItem = pageRef.All.Cast<RefLanguageTextPageEntry>().FirstOrDefault(e => e.English.Equals(entry.English) && e.Language.Equals(entry.Language));
				AssertNotNull("should not add additional entries", refItem);
				AssertEquals(refItem.Translation, entry.Translation);
			}
		}

		public void TestExportEntries()
		{
			var page = GetPageInstance("two");
			var result = page.ExportEntries();

			var contentLines =
				("\"Language\",\"Original\",\"Language\",\"Translation\"\r\n"
				+ "\"EN\",\"zero\"\r\n"
				+ "\"EN\",\"one\"\r\n"
				+ "\"EN\",\"two\"\r\n"
				+ "\"EN\",\"three\"\r\n"
				+ "\"EN\",\"four\"\r\n"
				+ "\"EN\",\"five\"\r\n"
				+ "\"EN\",\"six\"\r\n"
				+ "\"EN\",\"seven\"\r\n"
				+ "\"EN\",\"eight\"\r\n"
				+ "\"EN\",\"nine\"\r\n"
				+ "\"EN\",\"ten\"")
					.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var entry in result)
			{
				var lines = entry.Value.ToStringWithNewLineBetweenAppends().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var content in contentLines)
				{
					Assert(FormattableString.Invariant($"{content} was not found in the content lines to export."), lines.Any(x => x.StartsWith(content)));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestStringLengthOutOfRange()
		{
			const string english = @"Prompt payment is appreciated or a late fee of 1.5% will be incurred. 
							In accordance with 19 CFR 111.29(b)(1), we are obliged to advise you of the following: 
							If you are the importer of record, payment to the broker will not relieve you of liability for Customs charges 
							(duties, taxes, or other debts owed Customs) in the event these charges are note paid by the broker. 
							Therefore, if you pay by check, Customs charges may be paid with a separate check payable to U.S. 
							Bureau of Customs and Border Protection which shall be delivered to Customs by the broker";

			var page = GetPageInstance("zero");
			AssertEquals(false, page.HasChanges);
			var test = (RefLanguageTextPageEntry)page.AllTranslationsOfCurrentValue.First(entry => ((RefLanguageTextPageEntry)(entry)).Language == Core.SharedConstants.Languages.Bulgarian);
			AssertEquals("zero", test.Translation);
			test.Translation = "zero";
			test.English = english;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetPageInstance("one");
		}

		protected override void SetUp()
		{
			testHelper = new RefLanguageTextTranslationTestHelper(Factory);
			base.SetUp();
		}

		RefLanguageTextTranslationTestHelper testHelper;
	}
}
