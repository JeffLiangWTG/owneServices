using System;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ZACustoms.Helpers;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ZACustoms.Tests.Helpers
{
	[TestClass]
	public class CertificatesHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CertificatesHelper_SelectMostValidCertificate()
		{
			var eHubCertificates = new TestDbSet<eHubCertificate>
			{
				new eHubCertificate { CE_ValidFromUTC = new DateTime(2001, 1, 1), CE_ValidToUTC = new DateTime(2002, 3, 1), CE_ActiveFromUTC = new DateTime(2001, 2, 1), CE_AddedUTC = new DateTime(2000, 9, 1), CE_PK = new Guid("11111111-1111-1111-1111-111111111111") },
				new eHubCertificate { CE_ValidFromUTC = new DateTime(2002, 1, 1), CE_ValidToUTC = new DateTime(2003, 3, 1), CE_ActiveFromUTC = new DateTime(2002, 2, 1), CE_AddedUTC = new DateTime(2001, 9, 1), CE_PK = new Guid("22222222-2222-2222-2222-222222222222") },
				new eHubCertificate { CE_ValidFromUTC = new DateTime(2003, 1, 1), CE_ValidToUTC = new DateTime(2004, 3, 1), CE_ActiveFromUTC = new DateTime(2003, 2, 1), CE_AddedUTC = new DateTime(2002, 9, 1), CE_PK = new Guid("33333333-3333-3333-3333-333333333333") },
			};

			Assert.AreEqual(new Guid("11111111-1111-1111-1111-111111111111"), CertificatesHelper.SelectMostValidCertificate(eHubCertificates, new DateTime(2000, 1, 1)).CE_PK);
			Assert.AreEqual(new Guid("11111111-1111-1111-1111-111111111111"), CertificatesHelper.SelectMostValidCertificate(eHubCertificates, new DateTime(2001, 1, 1)).CE_PK);
			Assert.AreEqual(new Guid("11111111-1111-1111-1111-111111111111"), CertificatesHelper.SelectMostValidCertificate(eHubCertificates, new DateTime(2002, 1, 1)).CE_PK);
			Assert.AreEqual(new Guid("22222222-2222-2222-2222-222222222222"), CertificatesHelper.SelectMostValidCertificate(eHubCertificates, new DateTime(2002, 2, 1)).CE_PK);
			Assert.AreEqual(new Guid("22222222-2222-2222-2222-222222222222"), CertificatesHelper.SelectMostValidCertificate(eHubCertificates, new DateTime(2003, 1, 1)).CE_PK);
			Assert.AreEqual(new Guid("33333333-3333-3333-3333-333333333333"), CertificatesHelper.SelectMostValidCertificate(eHubCertificates, new DateTime(2003, 2, 1)).CE_PK);
			Assert.AreEqual(new Guid("33333333-3333-3333-3333-333333333333"), CertificatesHelper.SelectMostValidCertificate(eHubCertificates, new DateTime(2005, 1, 1)).CE_PK);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CertificatesHelper_GetZACustomsCertificate()
		{
            var zaCustomsClient = new eHubClient { CC_ID = "ZACustoms" };
            var zaCustomsTestClient = new eHubClient { CC_ID = "ZACustomsTest" };
			var eHubCertificates = new TestDbSet<eHubCertificate>
			{
				new eHubCertificate { CE_Category = "ZACustoms", eHubClient = zaCustomsClient, CE_ID = "CERTID", CE_ValidFromUTC = new DateTime(2001, 1, 1), CE_ValidToUTC = new DateTime(2002, 3, 1), CE_ActiveFromUTC = new DateTime(2001, 2, 1), CE_AddedUTC = new DateTime(2000, 9, 1), CE_PK = new Guid("11111111-1111-1111-1111-111111111111") },
				new eHubCertificate { CE_Category = "ZACustoms", eHubClient = zaCustomsClient, CE_ID = "CERTIDA", CE_ValidFromUTC = new DateTime(2002, 1, 1), CE_ValidToUTC = new DateTime(2003, 3, 1), CE_ActiveFromUTC = new DateTime(2002, 2, 1), CE_AddedUTC = new DateTime(2001, 9, 1), CE_PK = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA") },
				new eHubCertificate { CE_Category = "ZACustomsTest", eHubClient = zaCustomsTestClient, CE_ID = "CERTID", CE_ValidFromUTC = new DateTime(2002, 1, 1), CE_ValidToUTC = new DateTime(2003, 3, 1), CE_ActiveFromUTC = new DateTime(2002, 2, 1), CE_AddedUTC = new DateTime(2001, 9, 1), CE_PK = new Guid("22222222-2222-2222-2222-222222222222") },
				new eHubCertificate { CE_Category = "ZACustomsTest", eHubClient = zaCustomsTestClient, CE_ID = "CERTID", CE_ValidFromUTC = new DateTime(2003, 1, 1), CE_ValidToUTC = new DateTime(2004, 3, 1), CE_ActiveFromUTC = new DateTime(2003, 2, 1), CE_AddedUTC = new DateTime(2002, 9, 1), CE_PK = new Guid("33333333-3333-3333-3333-333333333333") },
			};
			var stubEHubTransactionsContext = MockRepository.GenerateStub<eHubTransactionsContext>();
			stubEHubTransactionsContext.eHubCertificates = eHubCertificates;
			CertificatesHelper.ContextFactory = () => stubEHubTransactionsContext;
			CertificatesHelper.GetDateTimeUtcNow = () => new DateTime(2003, 1, 1);

			Assert.AreEqual(new Guid("22222222-2222-2222-2222-222222222222"), CertificatesHelper.GetZACustomsCertificate("ZACustomsTest", "CERTID", new NoOpLogger()).CE_PK);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CertificatesHelper_GetSenderCertificate()
		{
			var senderClient = new eHubClient { CC_ID = "SENDER" };
			var eHubCertificates = new TestDbSet<eHubCertificate>
			{
				new eHubCertificate { CE_Category = "ZACustoms", eHubClient = senderClient, CE_ID = Constants.AS2ID, CE_ValidFromUTC = new DateTime(2001, 1, 1), CE_ValidToUTC = new DateTime(2002, 3, 1), CE_ActiveFromUTC = new DateTime(2001, 2, 1), CE_AddedUTC = new DateTime(2000, 9, 1), CE_PK = new Guid("11111111-1111-1111-1111-111111111111"), CE_BinaryContainer = new byte[0]},
				new eHubCertificate { CE_Category = "ZACustoms", eHubClient = senderClient, CE_ID = Constants.AS2ID, CE_ValidFromUTC = new DateTime(2002, 1, 1), CE_ValidToUTC = new DateTime(2003, 3, 1), CE_ActiveFromUTC = new DateTime(2002, 2, 1), CE_AddedUTC = new DateTime(2001, 9, 1), CE_PK = new Guid("22222222-2222-2222-2222-222222222222"), CE_BinaryContainer = new byte[0]},
				new eHubCertificate { CE_Category = "ZACustoms", eHubClient = senderClient, CE_ID = Constants.AS2ID, CE_ValidFromUTC = new DateTime(2003, 1, 1), CE_ValidToUTC = new DateTime(2004, 3, 1), CE_ActiveFromUTC = new DateTime(2003, 2, 1), CE_AddedUTC = new DateTime(2002, 9, 1), CE_PK = new Guid("33333333-3333-3333-3333-333333333333"), CE_BinaryContainer = new byte[0]},
			};
			var stubEHubTransactionsContext = MockRepository.GenerateStub<eHubTransactionsContext>();
			stubEHubTransactionsContext.eHubCertificates = eHubCertificates;
			CertificatesHelper.ContextFactory = () => stubEHubTransactionsContext;

            Assert.AreEqual(3, CertificatesHelper.GetSenderCertificateCount("SENDER", "ZACustoms", Constants.AS2ID, new NoOpLogger()));

			CertificatesHelper.GetDateTimeUtcNow = () => new DateTime(2003, 1, 1);
            Assert.AreEqual(new Guid("22222222-2222-2222-2222-222222222222"), CertificatesHelper.GetSenderCertificate("SENDER", "ZACustoms", Constants.AS2ID, false, new NoOpLogger()).CE_PK);
            Assert.AreEqual(new Guid("33333333-3333-3333-3333-333333333333"), CertificatesHelper.GetSenderCertificate("SENDER", "ZACustoms", Constants.AS2ID, true, new NoOpLogger()).CE_PK);

			CertificatesHelper.GetDateTimeUtcNow = () => new DateTime(2003, 2, 1);
            Assert.AreEqual(new Guid("33333333-3333-3333-3333-333333333333"), CertificatesHelper.GetSenderCertificate("SENDER", "ZACustoms", Constants.AS2ID, false, new NoOpLogger()).CE_PK);
            Assert.AreEqual(new Guid("22222222-2222-2222-2222-222222222222"), CertificatesHelper.GetSenderCertificate("SENDER", "ZACustoms", Constants.AS2ID, true, new NoOpLogger()).CE_PK);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CertificatesHelper_CertificateContainerIsNull()
		{
			var senderClient = new eHubClient { CC_ID = "SENDER" };
			var eHubCertificates = new TestDbSet<eHubCertificate>
			{
				new eHubCertificate { CE_Category = "ZACustoms", eHubClient = senderClient, CE_ID = Constants.AS2ID, CE_ValidFromUTC = new DateTime(2001, 1, 1), CE_ValidToUTC = new DateTime(2002, 3, 1), CE_ActiveFromUTC = new DateTime(2001, 2, 1), CE_AddedUTC = new DateTime(2000, 9, 1), CE_PK = new Guid("11111111-1111-1111-1111-111111111111") },
			};
			var stubEHubTransactionsContext = MockRepository.GenerateStub<eHubTransactionsContext>();
			stubEHubTransactionsContext.eHubCertificates = eHubCertificates;
			CertificatesHelper.ContextFactory = () => stubEHubTransactionsContext;

			try
			{
                CertificatesHelper.GetSenderCertificate("SENDER", "ZACustoms", Constants.AS2ID, false, new NoOpLogger());
				Assert.Fail("Exception expected.");
			}
			catch (Exception ex)
			{
				Assert.AreEqual("No certificate registered for South African Customs messaging.", ex.Message);
			}

		}
	}
}
