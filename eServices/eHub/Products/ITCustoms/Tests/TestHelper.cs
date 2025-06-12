using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Text;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.XmlDiffPatch;

namespace CargoWise.eHub.Products.ITCustoms.Tests
{
	public class TestHelper
	{
		public static eHubTransactionsContext eHubTransactionsContextForTest()
		{
			Type providerService = typeof(System.Data.Entity.SqlServer.SqlProviderServices);

			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			var mockDBTransaction = MockRepository.GenerateStrictMock<IDbTransaction>();
			var ehubRegistrationTypes = new TestDbSet<eHubRegistrationType>() { RegistrationTypeITC };
			var connection = MockRepository.GenerateMock<SqlConnection>();
			connection.Stub(x => x.Open());
			mockeHubTransactionsContext.Stub(x => x.Connection).Return(connection);
			mockeHubTransactionsContext.Stub(x => x.eHubRegistrationTypes).Return(ehubRegistrationTypes);
			mockeHubTransactionsContext.Expect(x => x.BeginTransaction()).Return(mockDBTransaction).Repeat.Any();
			mockDBTransaction.Expect(x => x.Commit());
			mockDBTransaction.Expect(x => x.Rollback()).Repeat.Any();
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose()).Repeat.Any();
			((IDisposable)mockDBTransaction).Expect(x => x.Dispose()).Repeat.Any();

			return mockeHubTransactionsContext;
		}

		public static void CompareEntities(Object real, Object expected, List<string> memberToIgnore = null)
		{
			memberToIgnore = memberToIgnore ?? new List<string>();
			var compareLogic = new CompareLogic()
			{
				Config = new ComparisonConfig()
				{
					MembersToIgnore = memberToIgnore,
					IgnoreObjectTypes = true
				}
			};
			var compareResult = compareLogic.Compare(real, expected);
			var diffs = compareResult.Differences;
			var sb = new StringBuilder();
			if (diffs.Count > 0)
			{
				sb.AppendLine("Comparison Failed:");
				foreach (Difference diff in diffs)
				{
					sb.AppendLine("Property name:" + diff.PropertyName);
					sb.AppendLine("Real value:" + diff.Object1Value);
					sb.AppendLine("Expected value:" + diff.Object2Value + "\n");
				}
				throw new Exception(sb.ToString());
			}
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
		}

		internal static Stream GetResourceStream(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.Products.ITCustoms.Tests." + name);
		}

		internal static string GetResourceText(string name)
		{
			using (var resStream = GetResourceStream(name))
			using (var streamRdr = new StreamReader(resStream))
				return streamRdr.ReadToEnd();
		}

		internal static byte[] GetResourceData(string name)
		{
			using (var resStream = GetResourceStream(name))
			using (var binRdr = new BinaryReader(resStream))
				return binRdr.ReadBytes((int)resStream.Length);
		}

		internal static void AssertXmlContentSameAsExpected(string expacted, string actual)
		{
			using (var actualStream = new MemoryStream(Encoding.UTF8.GetBytes(actual)))
			{
				using (Stream expectedStream = GetResourceStream(expacted))
				{
					MapResult result = (new XmlDiffTool()).Execute(actualStream, expectedStream);
					MapTester.AssertSuccess(result, expacted, actual);
				}
			}
		}

		internal static void AssertNoException<T>(Action action) where T : Exception
		{
			try
			{
				action();
			}
			catch
			{
				Assert.Fail("Unexpected exception thrown.");
				return;
			}
		}

		internal static void AssertXmlAreEqual(Stream expectedXmlStream, XmlDocument actualXmlDocument)
		{
			var xmlDiff = new XmlDiff();
			var xmlDiffgram = new XDocument();
			var expectedXDoc = XDocument.Load(expectedXmlStream);
			foreach (var node in expectedXDoc.Descendants()) node.Attributes("xmlns").Remove();

			using (var expectedRdr = expectedXDoc.CreateReader())
			using (var actualRdr = new XmlNodeReader(actualXmlDocument))
			using (var diffWrtr = xmlDiffgram.CreateWriter())
				if (xmlDiff.Compare(expectedRdr, actualRdr, diffWrtr)) return;

			var tempFile = Path.GetTempFileName();
			actualXmlDocument.Save(tempFile);
			Console.WriteLine("Actual XML: " + tempFile);
			Console.WriteLine("XML Diff:");
			Console.WriteLine(xmlDiffgram.ToString());
			Assert.Fail("AssertXmlAreEqual failed.");
		}

		public static eHubRegistrationType RegistrationTypeITC = new eHubRegistrationType { RT_ID = "ITCustomsAccount", RT_Description = "IT Customs Accounts", RT_RegistrantType = "ClientSystem" };
		public static List<string> MemberToIgnore = new List<string>();
	}
}
