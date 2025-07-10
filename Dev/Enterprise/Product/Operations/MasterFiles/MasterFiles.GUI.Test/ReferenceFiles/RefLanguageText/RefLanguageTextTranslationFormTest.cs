using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefLanguageTextTranslationFormTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			testHelper = new RefLanguageTextTranslationTestHelper(Factory);
			base.SetUp();
		}

		RefLanguageTextTranslationTestHelper testHelper;

		public void TestImport()
		{
			using (TempFile file = TempFile.NewWithExtension(".csv"))
			{
				using (var form = new RefLanguageTextTranslationFormForTest(
					new RefLanguageTextPageForTest(testHelper.ParentTableCode, testHelper.ColumnToTest, testHelper.GetMultilingualLanguage("two"), testHelper.GetContextObject(Factory, "two"), testHelper.GetFilterColumns()), Path.GetDirectoryName(file.Filename)))
				{
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					form.Import();

					AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Import has been successful", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestImport_Error()
		{
			using (TempFile file = TempFile.NewWithExtension(".csv"))
			{
				var page = new RefLanguageTextPageForTest(testHelper.ParentTableCode, testHelper.ColumnToTest, testHelper.GetMultilingualLanguage("two"), testHelper.GetContextObject(Factory, "two"), testHelper.GetFilterColumns());
				using (var form = new RefLanguageTextTranslationFormForTest(page, Path.GetDirectoryName(file.Filename)))
				{
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					page.ImportError = "boom";
					form.Import();

					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Import completed with errors: boom", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestExport()
		{
			using (var form = new RefLanguageTextTranslationFormForTest(
				new RefLanguageTextPageForTest(testHelper.ParentTableCode, testHelper.ColumnToTest, testHelper.GetMultilingualLanguage("two"), testHelper.GetContextObject(Factory, "two"), testHelper.GetFilterColumns()), "X:\\"))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.Export();
				Assert(form.SavedFiles.Count >= 33);

				string prefix = @"X:\Test-";

				var contentLines =
("\"Language\",\"Original\",\"Language\",\"Translation\"\r\n"
+ "\"EN-US,\"zero\",\"{0}\"\r\n"
+ "\"EN-US,\"one\",\"{0}\"\r\n"
+ "\"EN-US,\"two\",\"{0}\"\r\n"
+ "\"EN-US,\"three\",\"{0}\"\r\n"
+ "\"EN-US,\"four\",\"{0}\"\r\n"
+ "\"EN-US,\"five\",\"{0}\"\r\n"
+ "\"EN-US,\"six\",\"{0}\"\r\n"
+ "\"EN-US,\"seven\",\"{0}\"\r\n"
+ "\"EN-US,\"eight\",\"{0}\"\r\n"
+ "\"EN-US,\"nine\",\"{0}\"\r\n"
+ "\"EN-US,\"ten\",\"{0}\"")
.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var entry in form.SavedFiles)
				{
					Assert(entry.Key.StartsWith(prefix));
					Assert(entry.Key.EndsWith(@".csv"));

					string language = entry.Key.Substring(prefix.Length, 3);
					var lines = entry.Value.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

					for (var index = 0; index < lines.Length; index++)
					{
						var line = lines[index];
						Assert(string.Format("{0} should start with {1}", line, string.Format(contentLines[index], language)), line.StartsWith(string.Format(contentLines[index], language)));
					}
				}
			}
		}

		class RefLanguageTextPageForTest : RefLanguageTextPage
		{
			public RefLanguageTextPageForTest(string tableCode, string columnName, string initialValue, BusinessObject businessObj, string[] filterColumns)
				: base(tableCode, columnName, initialValue, businessObj, "Test", filterColumns)
			{
			}

			public override string ImportEntries(string csvData)
			{
				return ImportError;
			}

			public string ImportError { get; set; }
		}

		class RefLanguageTextTranslationFormForTest : RefLanguageTextTranslationForm
		{
			readonly string directory;
			public Dictionary<string, string> SavedFiles = new Dictionary<string, string>();

			public RefLanguageTextTranslationFormForTest(RefLanguageTextPage bo, string directory) : base(bo)
			{
				this.directory = directory;
			}

			protected override bool SaveFile(string path, string content)
			{
				SavedFiles.Add(path, content);
				return true;
			}

			protected override string GetDirectory(string description)
			{
				return directory;
			}

			public void Export()
			{
				OnExport(null, EventArgs.Empty);
			}

			public void Import()
			{
				OnImport(null, EventArgs.Empty);
			}
		}
	}
}
