using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using HtmlAgilityPack;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CargoWise.eHub.Selenium.IntegrationTests.Core
{
	[TestFixture]
	public abstract class SeleniumTestBase : GenericTestBase
	{
		protected IWebDriver Driver { get; private set; }
		protected WebDriverWait Wait { get; private set; }
		protected SnapshotPageBuilder PageBuilder { get; private set; }

		protected string[] LinkPaths { get; set; } = new string[0];
		protected string[] PreservedNodesXpaths { get; set; } = new string[0];
		protected Dictionary<string, string> SkipComparingAttributes { get; set; } = new Dictionary<string, string>();
		protected string ReferencePath = string.Empty;

		protected List<string> Logs { get; } = new List<string>();

		protected SeleniumTestBase()
		{
			TestAssembly = Assembly.GetCallingAssembly();
		}

		[OneTimeSetUp]
		protected virtual void SetUpField()
		{
			HtmlNode.ElementsFlags["link"] = HtmlElementFlag.Closed;
			HtmlNode.ElementsFlags["meta"] = HtmlElementFlag.Closed;
			ChromeOptions options = new ChromeOptions();
			if (options.BinaryLocation == null && File.Exists(@"C:\Program Files\Google\Chrome\Application\chrome.exe"))
			{
				options.BinaryLocation = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
			}

			options.AddArguments("--no-sandbox");
			if (Deployment.IsRunningInDAT())
			{
				options.AddArguments("--headless");
			}
			Driver = new ChromeDriver(options);
			Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(60));
			PageBuilder = new SnapshotPageBuilder(ReferencePath, LinkPaths, PreservedNodesXpaths);
		}

		[OneTimeTearDown]
		public void Dispose()
		{
			Driver?.Quit();
			Driver?.Dispose();
		}

		protected void WaitUntilFindElement(By locator, int timeoutInSeconds = 20)
		{
			try
			{
				Wait.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
				Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
			}
			catch (Exception ex)
			{
				var jsError = GetJsError();
				throw new Exception($@"{ex.Message}
Logs: {string.Join("\r\n", Logs)}
JSErrors: {jsError}
Body: {Driver.FindElement(By.TagName("body"))?.Text ?? string.Empty}");
			}
		}

		string GetJsError()
		{
			var jsErrors = Driver.Manage().Logs.GetLog(LogType.Browser).Where(x => x.Message.Contains("Error")).Select(x => x.Message);
			return jsErrors.Any()
				? jsErrors.Aggregate("", (s, entry) => s + entry + Environment.NewLine)
				: string.Empty;
		}

		protected HtmlDocument SnapShotPage()
		{
			return PageBuilder.SaveSnapShotOfHtml(Driver.PageSource);
		}

		protected HtmlDocument ReadEmbeddedHtml(string relativePath)
		{
			Assert.IsFalse(string.IsNullOrWhiteSpace(relativePath));
			var stream = GetEmbeddedResource(relativePath);
			HtmlDocument doc = new HtmlDocument();
			doc.OptionFixNestedTags = true;
			doc.Load(stream, Encoding.UTF8);
			return doc;
		}

		Stream GetEmbeddedResource(string relativePath)
		{
			var fullPath = $"{TestAssembly.GetName().Name}.{relativePath}";
			var resource = TestAssembly.GetManifestResourceStream(fullPath);
			if (resource == null)
			{
				throw new FileNotFoundException($"Could not locate embedded resource '{fullPath}'");
			}
			return resource;
		}

		protected void CompareHtmlDocuments(HtmlDocument expected, HtmlDocument actual)
		{
			var error = CompareHtmlNodes(expected.DocumentNode, actual.DocumentNode);
			if (error == string.Empty)
				return;

			var tempFile = Path.GetTempFileName();
			actual.Save(tempFile);
			Assert.Fail(error + "\r\nActual HTML saved in: " + tempFile);
		}

		string CompareHtmlNodes(HtmlNode expected, HtmlNode actual)
		{
			if (expected.NodeType != actual.NodeType || expected.Name != actual.Name)
			{
				return "Different nodes"
					+ $"\r\nExpected: 'Type: {expected.NodeType}, Name: {expected.Name}, Xpath: {expected.XPath}'"
					+ $"\r\nActual: 'Type: {actual.NodeType}, Name: {actual.Name}, Xpath: {actual.XPath}'";
			}

			var error = CompareHtmlAttributes(expected, actual);
			if (error != string.Empty)
				return error;

			if (!expected.HasChildNodes && expected.Name == "#text")
			{
				if (expected.InnerText.Trim() != actual.InnerText.Trim())
				{
					return $"Different inner text at node '{expected.Name}'"
						+ $"\r\nExpected: '{expected.InnerText}, Xpath: {expected.XPath}'"
						+ $"\r\nActual: '{actual.InnerText}, Xpath: {actual.XPath}'";
				}
			}
			else
			{
				var actualChildNodePosition = 0;
				for (; actualChildNodePosition < actual.ChildNodes.Count; actualChildNodePosition++)
				{
					HtmlNode actualNode = actual.ChildNodes[actualChildNodePosition];
					if (IsEmptyTextNode(actualNode) || actualNode.NodeType == HtmlNodeType.Comment) continue;
					var xpath = actualNode.Name != "#text" ? actualNode.XPath : actualNode.XPath.Replace("#text", "text()");
					var expectedNode = expected.NodeType == HtmlNodeType.Document ? expected.SelectSingleNode(xpath) : expected.OwnerDocument.DocumentNode.SelectSingleNode(xpath);
					if (expectedNode == null)
						return $"No matching element in expected document. Xpath: {xpath}";
					error = CompareHtmlNodes(expectedNode, actualNode);
					if (error != string.Empty)
						return error;
				}
			}

			return string.Empty;
		}

		bool IsEmptyTextNode(HtmlNode node) => node.Name == "#text" && node.InnerText.Replace("\r\n", string.Empty).Trim() == string.Empty;

		string CompareHtmlAttributes(HtmlNode expectedNode, HtmlNode actualNode)
		{
			var expected = expectedNode.Attributes;
			var actual = actualNode.Attributes;

			if (expected.Count != actual.Count)
				return $"Attribute counts are different. Node: {actualNode.Name}. \r\nExpected: {string.Join(",", expectedNode.Attributes.Select(x => x.Name))}\r\nActual: {string.Join(",", actualNode.Attributes.Select(x => x.Name))}";

			if (expected.Count == 0)
				return string.Empty;

			foreach (var expectedAttribute in expected)
			{
				if (SkipComparingAttributes.ContainsKey(expectedNode.Name) && SkipComparingAttributes[expectedNode.Name] == expectedAttribute?.Name)
					continue;

				var actualAttribute = actual.FirstOrDefault(a => a?.Name == expectedAttribute?.Name);

				if (actualAttribute == null || expectedAttribute?.Value != actualAttribute?.Value)
				{
					var pixelPattern = @"\d+px";
					if (expectedAttribute?.Name == "style" && actualAttribute?.Name == "style" && Regex.IsMatch(expectedAttribute?.Value, pixelPattern) && Regex.IsMatch(actualAttribute?.Value, pixelPattern))
						continue;

					return "Attributes are different"
						+ $"\r\nExpected: 'Name: {expectedAttribute.Name}, Value: {expectedAttribute.Value}'"
						+ $"\r\nActual: 'Name: {actualAttribute?.Name}, Value: {actualAttribute?.Value}'";
				}
			}

			return string.Empty;
		}

		protected void UpdateMessageDistributionStatusUsingOutboxPK(string pk)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var transaction = connection.BeginTransaction())
			{
				using (var command = connection.CreateCommand())
				{
					command.Transaction = transaction;
					command.CommandType = System.Data.CommandType.StoredProcedure;
					command.CommandText = "UpdateMessageDistributionStatusUsingOutboxPK";
					command.Parameters.AddWithValue("OutboxPK", pk);
					command.ExecuteNonQuery();
				}
				transaction.Commit();
			}
		}

		protected string SelectDateDiffInMinute(string utc)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = "SELECT ISNULL(DATEDIFF(MINUTE,MIN(@Input),GETUTCDATE()),0)";
				command.Parameters.AddWithValue("Input", utc);
				using (var reader = command.ExecuteReader())
				{
					return reader.Read() ? reader[0].ToString() : string.Empty;
				}
			}
		}


		protected void UpdateInboxMessageInsertDateTime(string pk, DateTimeOffset insertDateTime)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var transaction = connection.BeginTransaction())
			{
				using (var command = connection.CreateCommand())
				{
					command.Transaction = transaction;
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = $"UPDATE eHubTransactions.dbo.eHubInboxMessage SET EI_InsertUTC = @insertDateTime WHERE EI_PK = @pk;";
					command.Parameters.AddWithValue("insertDateTime", insertDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
					command.Parameters.AddWithValue("pk", pk);
					command.ExecuteNonQuery();
				}
				transaction.Commit();
			}
		}

		protected void UpdateOutboxMessageInsertDateTime(string pk, DateTimeOffset insertDateTime)
		{
			using (var connection = OpenEHubTransactionsConnection())
			using (var transaction = connection.BeginTransaction())
			{
				using (var command = connection.CreateCommand())
				{
					command.Transaction = transaction;
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = $"UPDATE eHubTransactions.dbo.eHubOutboxMessage SET OI_InsertUTC = @insertDateTime WHERE OI_PK = @pk;";
					command.Parameters.AddWithValue("insertDateTime", insertDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
					command.Parameters.AddWithValue("pk", pk);
					command.ExecuteNonQuery();
				}
				transaction.Commit();
			}
		}

		protected static SqlConnection OpenEHubTransactionsConnection() =>
			SqlServerHelper.OpenAdminSqlConnection("eHubTransactions");
	}
}
