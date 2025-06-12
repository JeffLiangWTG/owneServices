using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.Accessors;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;
using System.Linq;

namespace CargoWise.eHub.Core.Tests.OrchestrationsHelper
{
    [TestClass]
    public class ReporteHubErrorTest
    {
        [TestMethod]
        public void TestClientIDMandatory()
        {
            var result1 = ReporteHubError.InserteHubError(null, "ABC", "ABC", "ABC");
            Assert.AreEqual("clientID is required.", result1);

            var result2 = ReporteHubError.InserteHubError("", "ABC", "ABC", "ABC");
            Assert.AreEqual("clientID is required.", result2);
        }

        [TestMethod]
        public void TestErrorTypeMandatory()
        {
            var result1 = ReporteHubError.InserteHubError("ABC", null, "ABC", "ABC");
            Assert.AreEqual("errorType is required.", result1);

            var result2 = ReporteHubError.InserteHubError("ABC", "", "ABC", "ABC");
            Assert.AreEqual("errorType is required.", result2);
        }


        [TestMethod]
        public void TestDescriptionMandatory()
        {
            var result1 = ReporteHubError.InserteHubError("ABC", "ABC", null, "ABC");
            Assert.AreEqual("description is required.", result1);

            var result2 = ReporteHubError.InserteHubError("ABC", "ABC", "", "ABC");
            Assert.AreEqual("description is required.", result2);
        }

        [TestMethod]
        public void TestReporteHubErrorWithClientIDProd()
        {
            var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            var localDate = DateTime.UtcNow;
            ReporteHubError.GetContext = () => mockContext;
            ReporteHubError.NewGuid = () => Guid.Parse("786fc722-f52e-40cc-acac-a8230c34be9a");
            ReporteHubError.CurrentUtcDate = () => localDate;
            eHubTransactionsAccessor.GetDbContext = () => mockContext;

            var clientID = "ABCDEFGHI";
            mockContext.Stub(x => x.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", clientID.Substring(0, 3), clientID.Substring(6, 3)))
                .Return(new string[] { "PRD" });

            var mockeHubErrors = new TestDbSet<eHubError>();
            mockContext.Stub(x => x.eHubErrors).Return(mockeHubErrors);
            
            var result1 = ReporteHubError.InserteHubError(clientID, "ABC", "ABC", "ABC");

            Assert.AreEqual(string.Empty, result1);

            mockContext.AssertWasCalled(x => x.eHubErrors);
            mockContext.AssertWasCalled(x => x.SaveChanges());

            Assert.IsNotNull(mockeHubErrors.FirstOrDefault(), "eHubError cannot be null");
            Assert.AreEqual(mockeHubErrors.Count(), 1, "only one entry in eHubError");

            var eHubError = mockeHubErrors.FirstOrDefault();

            Assert.AreEqual("786fc722-f52e-40cc-acac-a8230c34be9a", eHubError.EE_PK.ToString(), "PK should match");
            Assert.AreEqual("ABC", eHubError.EE_ErrorType, "Error type should be ABC");
            Assert.AreEqual("ABC", eHubError.EE_Description, "Description should be ABC");
            Assert.AreEqual("<ErrorDetail>ABC</ErrorDetail>", eHubError.EE_ErrorDetail, "Error Detail should be ABC");
            Assert.AreEqual(localDate, eHubError.EE_DateTimeUTC, $"Date should be {localDate}");
            Assert.AreEqual("BTS", eHubError.EE_Source, "Source should be BTS");
            Assert.AreEqual(false, eHubError.EE_Alerted, "Alerted should be false");        
        }

        [TestMethod]
        public void TestReporteHubErrorWithClientIDTest()
        {
            var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            ReporteHubError.GetContext = () => mockContext;
            eHubTransactionsAccessor.GetDbContext = () => mockContext;

            var clientID = "ABCDEFGHI";
            mockContext.Stub(x => x.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", clientID.Substring(0, 3), clientID.Substring(6, 3)))
                .Return(new string[] { "XYZ" });

            var mockeHubErrors = new TestDbSet<eHubError>();
            mockContext.Stub(x => x.eHubErrors).Return(mockeHubErrors);

            var result1 = ReporteHubError.InserteHubError(clientID, "ABC", "ABC", "ABC");

            mockContext.AssertWasNotCalled(x => x.eHubErrors);
            mockContext.AssertWasNotCalled(x => x.SaveChanges());

            Assert.IsNull(mockeHubErrors.FirstOrDefault(), "eHubError should be null");
            Assert.AreEqual(mockeHubErrors.Count(), 0, "no entry in eHubError");
        }

        [TestMethod]
        public void TestReporteHubErrorAllFields()
        {
            var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            var localDate = DateTime.UtcNow;
            ReporteHubError.GetContext = () => mockContext;
            ReporteHubError.NewGuid = () => Guid.Parse("786fc722-f52e-40cc-acac-a8230c34be9a");
            ReporteHubError.CurrentUtcDate = () => localDate;
            eHubTransactionsAccessor.GetDbContext = () => mockContext;

            var clientID = "ABCDEFGHI";
            mockContext.Stub(x => x.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", clientID.Substring(0, 3), clientID.Substring(6, 3)))
                .Return(new string[] { "PRD" });

            var mockeHubErrors = new TestDbSet<eHubError>();
            mockContext.Stub(x => x.eHubErrors).Return(mockeHubErrors);

            var result1 = ReporteHubError.InserteHubError(clientID, "ABC1", "ABC2", "ABC3", "5bdd3968-16d0-45d0-b70a-e311696eb456", "e90f5cea-c83c-4aa7-99f1-a5fd22741477");

            Assert.AreEqual(string.Empty, result1);

            mockContext.AssertWasCalled(x => x.eHubErrors);
            mockContext.AssertWasCalled(x => x.SaveChanges());

            Assert.IsNotNull(mockeHubErrors.FirstOrDefault(), "eHubError cannot be null");
            Assert.AreEqual(mockeHubErrors.Count(), 1, "only one entry in eHubError");

            var eHubError = mockeHubErrors.FirstOrDefault();

            Assert.AreEqual("786fc722-f52e-40cc-acac-a8230c34be9a", eHubError.EE_PK.ToString(), "PK should match");
            Assert.AreEqual("ABC1", eHubError.EE_ErrorType, "Error type should be ABC1");
            Assert.AreEqual("ABC2", eHubError.EE_Description, "Description should be ABC2");
            Assert.AreEqual("<ErrorDetail>ABC3</ErrorDetail>", eHubError.EE_ErrorDetail, "Error Detail should be ABC3");
            Assert.AreEqual(localDate, eHubError.EE_DateTimeUTC, $"Date should be {localDate}");
            Assert.AreEqual("5bdd3968-16d0-45d0-b70a-e311696eb456", eHubError.EE_EI_Inbox.ToString());
            Assert.AreEqual("e90f5cea-c83c-4aa7-99f1-a5fd22741477", eHubError.EE_OI_Outbox.ToString());
            Assert.AreEqual("BTS", eHubError.EE_Source, "Source should be BTS");
            Assert.AreEqual(false, eHubError.EE_Alerted, "Alerted should be false");
        }

        [TestMethod]
        public void TestReporteHubErrorInvalidXMLinErrorDetails()
        {
            var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            var localDate = DateTime.UtcNow;
            ReporteHubError.GetContext = () => mockContext;
            ReporteHubError.NewGuid = () => Guid.Parse("786fc722-f52e-40cc-acac-a8230c34be9a");
            ReporteHubError.CurrentUtcDate = () => localDate;
            eHubTransactionsAccessor.GetDbContext = () => mockContext;

            var clientID = "ABCDEFGHI";
            mockContext.Stub(x => x.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", clientID.Substring(0, 3), clientID.Substring(6, 3)))
                .Return(new string[] { "PRD" });

            var mockeHubErrors = new TestDbSet<eHubError>();
            mockContext.Stub(x => x.eHubErrors).Return(mockeHubErrors);

            var result1 = ReporteHubError.InserteHubError(clientID, "ABC1", "ABC2", "some text with invalid xml chars & < > \" '", "5bdd3968-16d0-45d0-b70a-e311696eb456", "e90f5cea-c83c-4aa7-99f1-a5fd22741477");

            Assert.AreEqual(string.Empty, result1);

            mockContext.AssertWasCalled(x => x.eHubErrors);
            mockContext.AssertWasCalled(x => x.SaveChanges());

            Assert.IsNotNull(mockeHubErrors.FirstOrDefault(), "eHubError cannot be null");
            Assert.AreEqual(mockeHubErrors.Count(), 1, "only one entry in eHubError");

            var eHubError = mockeHubErrors.FirstOrDefault();

            Assert.AreEqual("786fc722-f52e-40cc-acac-a8230c34be9a", eHubError.EE_PK.ToString(), "PK should match");
            Assert.AreEqual("ABC1", eHubError.EE_ErrorType, "Error type should be ABC1");
            Assert.AreEqual("ABC2", eHubError.EE_Description, "Description should be ABC2");
            Assert.AreEqual("<ErrorDetail>some text with invalid xml chars &amp; &lt; &gt; \" '</ErrorDetail>", eHubError.EE_ErrorDetail, "Error Detail should be formated xml chars");
            Assert.AreEqual(localDate, eHubError.EE_DateTimeUTC, $"Date should be {localDate}");
            Assert.AreEqual("5bdd3968-16d0-45d0-b70a-e311696eb456", eHubError.EE_EI_Inbox.ToString());
            Assert.AreEqual("e90f5cea-c83c-4aa7-99f1-a5fd22741477", eHubError.EE_OI_Outbox.ToString());
            Assert.AreEqual("BTS", eHubError.EE_Source, "Source should be BTS");
            Assert.AreEqual(false, eHubError.EE_Alerted, "Alerted should be false");
        }

        [TestMethod]
        public void TestReporteHubErrorInternalError()
        {
            var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            ReporteHubError.GetContext = () => mockContext;
            eHubTransactionsAccessor.GetDbContext = () => mockContext;

            var clientID = "ABCDEFGHI";
            mockContext.Stub(x => x.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", clientID.Substring(0, 3), clientID.Substring(6, 3)))
                .Return(new string[] { "PRD" });

            mockContext.Stub(x => x.eHubErrors).Return(null);
            var result1 = ReporteHubError.InserteHubError(clientID, "ABC1", "ABC2", "ABC3");

            Assert.AreEqual(true, result1.Contains("Object reference not set to an instance of an object."));

            var mockeHubErrors = new TestDbSet<eHubError>();
            ReporteHubError.GetContext = () => null;
            mockContext.Stub(x => x.eHubErrors).Return(mockeHubErrors);
            var result2 = ReporteHubError.InserteHubError(clientID, "ABC1", "ABC2", "ABC3");

            Assert.AreEqual(true, result2.Contains("Object reference not set to an instance of an object."));
        }

        [TestMethod]
        public void TestReporteHubErrorInvalidInboxPK()
        {
            var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            var localDate = DateTime.UtcNow;
            ReporteHubError.GetContext = () => mockContext;
            ReporteHubError.NewGuid = () => Guid.Parse("786fc722-f52e-40cc-acac-a8230c34be9a");
            ReporteHubError.CurrentUtcDate = () => localDate;
            eHubTransactionsAccessor.GetDbContext = () => mockContext;

            var clientID = "ABCDEFGHI";
            mockContext.Stub(x => x.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", clientID.Substring(0, 3), clientID.Substring(6, 3)))
                .Return(new string[] { "PRD" });

            var mockeHubErrors = new TestDbSet<eHubError>();
            mockContext.Stub(x => x.eHubErrors).Return(mockeHubErrors);

            var result1 = ReporteHubError.InserteHubError(clientID, "ABC1", "ABC2", "ABC3", "XYZ", "e90f5cea-c83c-4aa7-99f1-a5fd22741477");

            Assert.AreEqual("inboxPK is not valid.", result1);
        }

        [TestMethod]
        public void TestReporteHubErrorInvalidOutboxPK()
        {
            var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            var localDate = DateTime.UtcNow;
            ReporteHubError.GetContext = () => mockContext;
            ReporteHubError.NewGuid = () => Guid.Parse("786fc722-f52e-40cc-acac-a8230c34be9a");
            ReporteHubError.CurrentUtcDate = () => localDate;
            eHubTransactionsAccessor.GetDbContext = () => mockContext;

            var clientID = "ABCDEFGHI";
            mockContext.Stub(x => x.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", clientID.Substring(0, 3), clientID.Substring(6, 3)))
                .Return(new string[] { "PRD" });

            var mockeHubErrors = new TestDbSet<eHubError>();
            mockContext.Stub(x => x.eHubErrors).Return(mockeHubErrors);

            var result1 = ReporteHubError.InserteHubError(clientID, "ABC1", "ABC2", "ABC3", "5bdd3968-16d0-45d0-b70a-e311696eb456", "XYZ");

            Assert.AreEqual("outboxPK is not valid.", result1);
        }
    }
}
