using CargoWise.eHub.DataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XmlDiffPatch;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.SymmetricalMessaging.Tests
{
    class TestHelper
    {
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

        public static void AssertXmlAreEqual(Stream expectedXmlStream, XmlDocument actualXmlDocument)
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
    }
}
