using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Tests.Controllers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
	[TestClass]
	public class CodeMappingControllerCodeMapsPostTests : CodeMappingController_TestBase
	{
		[TestMethod()]
		public void ShouldSaveCodeMapsFromJsonData()
		{
			var logger = new TestLogger();
			formData.Clear();
			responseText.Clear();
			formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
			formData["codeMapsData"] = "[" +
				"[\"XXX\",\"AA\",\"111*\",\"AAA\",\"BBB\"]," +
				"[\"XXX\",\"AA\",\"000\",\"AAA\",\"BBB\"]," +
				"[\"XXX\",\"AA\",\"*222\",\"AAA\",\"BBB\"]," +
				"[\"XXX\",\"AA\",\"999\",\"AAA\",\"BBB\"]," +
				"[\"*\",\"*\",\"*\",\"#INPUTFIELD2#\",\"#INPUTFIELD3#\"]" +
				"]";
			string expected = "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
				"XXX,AA,111*,AAA,BBB" + Environment.NewLine +
				"XXX,AA,000,AAA,BBB" + Environment.NewLine +
				"XXX,AA,*222,AAA,BBB" + Environment.NewLine +
				"XXX,AA,999,AAA,BBB" + Environment.NewLine +
				"*,*,*,#INPUTFIELD2#,#INPUTFIELD3#" + Environment.NewLine;

			SaveCodeMapsTest(logger);
			Assert.IsTrue(((CargoWise.eHub.Portal.Tests.Fakes.TestContext)controller.Context).GetSaveChangesCount() == 2,
				"The SaveChanges in SaveCodeMaps must only be called twice. Once for clearing old key/values that are not in the current dataset and once for clearing and inserting current records.");

			string actual = Encoding.Default.GetString(controller.DownloadCsv(new Guid(formData["codeset"])).FileContents);

			Trace.WriteLine(actual);
			Assert.AreEqual(expected, actual);
			Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapValue:"));
			Assert.IsTrue(logger.Log.Contains("CV_CR=00000000-dddd-3333-2222-000000000000, CV_OutputCode=, CV_PassThroughKey=3"));
		}

		[TestMethod]
		public void InsertDefaultRowOrderLast()
		{
			var logger = new TestLogger();
			formData.Clear();
			responseText.Clear();
			formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
			formData["codeMapsData"] = "[" +
									   "[\"XXX\",\"AA\",\"111*\",\"AAA\",\"BBB\"]," +
									   "[\"*\",\"*\",\"*\",\"#INPUTFIELD2#\",\"#INPUTFIELD3#\"]," +
									   "[\"XXX\",\"AA\",\"000\",\"AAA\",\"BBB\"]," +
									   "[\"XXX\",\"AA\",\"*222\",\"AAA\",\"BBB\"]," +
									   "[\"XXX\",\"AA\",\"999\",\"AAA\",\"BBB\"]" +
									   "]";
			string expected = "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
							  "XXX,AA,111*,AAA,BBB" + Environment.NewLine +
							  "XXX,AA,000,AAA,BBB" + Environment.NewLine +
							  "XXX,AA,*222,AAA,BBB" + Environment.NewLine +
							  "XXX,AA,999,AAA,BBB" + Environment.NewLine +
							  "*,*,*,#INPUTFIELD2#,#INPUTFIELD3#" + Environment.NewLine;

			SaveCodeMapsTest(logger);
			Assert.IsTrue(((CargoWise.eHub.Portal.Tests.Fakes.TestContext)controller.Context).GetSaveChangesCount() == 2,
				"The SaveChanges in SaveCodeMaps must only be called twice. Once for clearing old key/values that are not in the current dataset and once for clearing and inserting current records.");

			string actual = Encoding.Default.GetString(controller.DownloadCsv(new Guid(formData["codeset"])).FileContents);

			Trace.WriteLine(actual);
			Assert.AreEqual(expected, actual);
			Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapValue:"));
			Assert.IsTrue(logger.Log.Contains("CV_CR=00000000-dddd-3333-2222-000000000000, CV_OutputCode=, CV_PassThroughKey=3"));
		}

		[TestMethod()]
		public void ShouldReplaceCodeMapsFromJsonData()
		{
			var logger = new TestLogger();
			formData.Clear();
			responseText.Clear();
			formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
			formData["codeMapsData"] = "[" +
				"[\"XXX\",\"AA\",\"111*\",\"AAA\",\"BBB\"]," +
				"[\"XXX\",\"AA\",\"000\",\"AAA\",\"BBB\"]," +
				"[\"XXX\",\"AA\",\"*222\",\"AAA\",\"BBB\"]," +
				"[\"XXX\",\"AA\",\"999\",\"AAA\",\"BBB\"]," +
				"[\"*\",\"*\",\"*\",\"#INPUTFIELD2#\",\"#INPUTFIELD3#\"]" +
				"]";
			SaveCodeMapsTest(logger);
			Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapValue:"));
			Assert.IsTrue(logger.Log.Contains("CV_CR=00000000-dddd-3333-2222-000000000000, CV_OutputCode=, CV_PassThroughKey=3"));
			string actualTemp = Encoding.Default.GetString(controller.DownloadCsv(new Guid(formData["codeset"])).FileContents);
			formData["codeMapsData"] = "[" +
				"[\"YYY\",\"BB\",\"333\",\"CCC\",\"DDD\"]," +
				"[\"*\",\"*\",\"*\",\"#INPUTFIELD2#\",\"#INPUTFIELD3#\"]" +
				"]";
			SaveCodeMapsTest(logger);
			Assert.IsTrue(((CargoWise.eHub.Portal.Tests.Fakes.TestContext)controller.Context).GetSaveChangesCount() == 4,
				"The SaveChanges in SaveCodeMaps must only be called twice per call. Once for clearing old key/values that are not in the current dataset and once for clearing and inserting current records. It was called twice so expecting 4 calls to SaveChanges().");

			string expected = "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
				"YYY,BB,333,CCC,DDD" + Environment.NewLine +
				"*,*,*,#INPUTFIELD2#,#INPUTFIELD3#" + Environment.NewLine;

			string actual = Encoding.Default.GetString(controller.DownloadCsv(new Guid(formData["codeset"])).FileContents);

			Trace.WriteLine(actual);
			Assert.AreEqual(expected, actual);
			Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapValue:"));
			Assert.IsTrue(logger.Log.Contains("CV_CR=00000000-dddd-3333-2222-000000000000, CV_OutputCode=, CV_PassThroughKey=3"));
		}

		protected JsonResult SaveCodeMapsTest(ILog logger)
		{
			controller.logger = logger;
			return controller.SaveCodeMaps();
		}

		[TestMethod()]
		public void ExistingCodeMapIsSelectedWhileReplacingItDuringBulkSave()
		{
			var logger = new TestLogger();
			var csvContent = "";
			var expectedResult = "";
			formData.Clear();
			responseText.Clear();
			formData["codeset"] = "{00000000-CCCC-4444-0000-000000000000}";
			formData["merge"] = "merge";

			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + ".Controllers.CodeMappingControllerTests.TestFiles.LargeSampleCodeMapData.json";
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			using (var stream = resource)
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					csvContent = reader.ReadToEnd();
				}
			}

			fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + ".Controllers.CodeMappingControllerTests.TestFiles.LargeSampleActualResult.txt";
			resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			using (var stream = resource)
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					expectedResult = reader.ReadToEnd();
				}
			}

			formData["codeMapsData"] = csvContent;

			SaveCodeMapsTest(logger);
			logger = new TestLogger();

			formData["codeMapsData"] = csvContent.Replace("[\r\n [\r\n   \"XXX\",\r\n   \"001\",\r\n   \"FHL\",\r\n   \"__AEJ\",\r\n   \"*\",\r\n   \"AMSHQCR\"\r\n ]",
				"[\r\n [\r\n   \"XXX\",\r\n   \"001\",\r\n   \"FHL\",\r\n   \"__LBG\",\r\n   \"*\",\r\n   \"AMSHQCR\"\r\n ]");

			SaveCodeMapsTest(logger);
			Assert.IsTrue(((CargoWise.eHub.Portal.Tests.Fakes.TestContext)controller.Context).GetSaveChangesCount() == 4,
				"The SaveChanges in SaveCodeMaps must only be called twice per call. Once for clearing old key/values that are not in the current dataset and once for clearing and inserting current records. It was called twice so expecting 4 calls to SaveChanges().");

			string actual = Encoding.Default.GetString(controller.DownloadCsv(new Guid(formData["codeset"])).FileContents);
			Assert.AreEqual(expectedResult, actual);

			var logEntries = logger.Log.Split(Environment.NewLine.ToCharArray()).Where(x => x.Contains("eHubCodeMapKey"));
			var count = 0;
			var delEntry = logEntries.LastOrDefault(x => x.Contains("[del]") && x.Contains("CK_CS=00000000-cccc-4444-0000-000000000000") && x.Contains("CK_Key1Value=XXX, CK_Key2Value=001, CK_Key3Value=FHL, CK_Key4Value=, CK_Key5Value="));
			var delIndex = 0;
			bool IsChronologicallyExecuted = false;

			Assert.IsTrue(delEntry != null);

			foreach (var logEntry in logEntries)
			{
				count++;
				if (logEntry == delEntry)
				{
					delIndex = count;
					continue;
				}
				if (delIndex == 0) continue;

				if (logEntry.Contains("CK_CS=00000000-cccc-4444-0000-000000000000") && logEntry.Contains("CK_Key1Value=XXX, CK_Key2Value=001, CK_Key3Value=FHL, CK_Key4Value=, CK_Key5Value="))
				{
					if (logEntry.Contains("[add]"))
					{
						IsChronologicallyExecuted = true;
						break;
					}
				}
				else break;
			}
			Assert.IsTrue(IsChronologicallyExecuted, "The deleting/clearing of a key was not chronologically executed before the adding of it");
		}

		[TestMethod()]
		public void ExistingCodeMapIsSavedWithNonEqualChangedKeysAndValues()
		{
			var logger = new TestLogger();
			formData.Clear();
			responseText.Clear();
			formData["codeset"] = "{00000000-CCCC-5555-0000-000000000000}";
			formData["codeMapsData"] = "[" +
				"[\"Provider 1\",\"SI\",\"ZZZ\"]," +
				"[\"*\",\"SI\",\"#BLANK#\"]" +
				"]";
			var jsonResult = SaveCodeMapsTest(logger);
			Assert.IsFalse(jsonResult.Data.ToString().Contains("Save Failed.<br>System.ArgumentOutOfRangeException:<br>Index was out of range."), "The Save failed with the following error: Index was out of range.");
			formData["codeMapsData"] = "[" +
				"[\"Provider 1\",\"SI\",\"LBG\"]," +
				"[\"*\",\"SI\",\"#BLANK#\"]" +
				"]";
			jsonResult = SaveCodeMapsTest(logger);
			Assert.IsFalse(jsonResult.Data.ToString().Contains("Save Failed.<br>System.ArgumentOutOfRangeException:<br>Index was out of range."), "The Save failed with the following error: Index was out of range.");
		}

	}
}
