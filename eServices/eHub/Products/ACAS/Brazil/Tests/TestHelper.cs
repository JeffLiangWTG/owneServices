using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XmlDiffPatch;
using CargoWise.eHub.DataModel.eHubTransactions;
using Rhino.Mocks;
using System.Data;
using System.Data.SqlClient;

namespace CargoWise.eHub.Products.ACAS.BR.Tests
{
	class TestHelper
	{
		public static void AssertXmlAreEqual(Stream expectedXmlStream, XmlDocument actualXmlDocument)
		{
			var xmlDiff = new XmlDiff();
			var xmlDiffgram = new XDocument();
			var expectedXDoc = XDocument.Load(expectedXmlStream);
			var actualXDoc = XDocument.Parse(actualXmlDocument.OuterXml);

			using (var expectedRdr = expectedXDoc.CreateReader())
			using (var actualRdr = actualXDoc.CreateReader())
			using (var diffWrtr = xmlDiffgram.CreateWriter())
			{
				if (xmlDiff.Compare(expectedRdr, actualRdr, diffWrtr)) return;
			}

			var tempFile = Path.GetTempFileName();
			actualXmlDocument.Save(tempFile);
			Console.WriteLine("Actual XML: " + tempFile);
			Console.WriteLine("XML Diff:");
			Console.WriteLine(xmlDiffgram.ToString());
			Assert.Fail("AssertXmlAreEqual failed.");
		}

        public static eHubTransactionsContext eHubTransactionsContextForTest()
        {
            Type providerService = typeof(System.Data.Entity.SqlServer.SqlProviderServices);

            var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
            var mockDBTransaction = MockRepository.GenerateStrictMock<IDbTransaction>();
            var connection = MockRepository.GenerateMock<SqlConnection>();
            connection.Stub(x => x.Open());
            mockeHubTransactionsContext.Stub(x => x.Connection).Return(connection);
            mockeHubTransactionsContext.Expect(x => x.BeginTransaction()).Return(mockDBTransaction).Repeat.Any();
            mockDBTransaction.Expect(x => x.Commit());
            mockDBTransaction.Expect(x => x.Rollback()).Repeat.Any();
            ((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose()).Repeat.Any();
            ((IDisposable)mockDBTransaction).Expect(x => x.Dispose()).Repeat.Any();

            return mockeHubTransactionsContext;
        }
	}
}