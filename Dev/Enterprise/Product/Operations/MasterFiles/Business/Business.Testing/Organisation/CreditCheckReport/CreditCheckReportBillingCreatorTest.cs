using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CreditCheckReportBillingCreatorTest : TestCaseWithFactory
	{
		[TestDate(2020, 05, 20, 10, 20, 15, 100)]
		public void TestCreateBillingTransaction()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";
			registrationKey.SystemIdForTest = "SYD1";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Org";
			orgHeader.OH_RL_NKClosestPort = "USORD";

			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "AU";
			glbCompany.GC_Code = "DUM";

			var glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_Code = "TST";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "TSF";
			staff.GS_GB_HomeBranch = glbBranch.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), glbBranch.PK.ToGuid(), Guid.Empty))
			{
				CreditCheckReportBillingCreator.CreateBillingTransaction(((IDbConnected)Factory).Connection, "ABC", "CRD", "CW1", orgHeader, "123");

				var stmUsageDataCollection = new BillingManager().GetTransactions(Factory, 10).ToList();
				var transactionXml = BillingManager.GetTransactionXml(stmUsageDataCollection.Single());
				var expectedMsg = string.Format(CultureInfo.InvariantCulture,
					@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"">
  <BillableCount>1</BillableCount>
  <Branch>{0}</Branch>
  <Category>CRD</Category>
  <ClientID>{1}</ClientID>
  <ClientNumber>{2}</ClientNumber>
  <ClientStaffCode>{3}</ClientStaffCode>
  <PriceItemCode>ABC</PriceItemCode>
  <Reference1>{4}</Reference1>
  <Reference2>123</Reference2>
  <Reference3>US</Reference3>
  <Reference4>{5}</Reference4>
  <Reference5>Test Org</Reference5>
  <ReportingSource>CW1</ReportingSource>
  <ServiceOccuredUTC>2020-05-20T10:20:15.1Z</ServiceOccuredUTC>
  <Version>1</Version>
  <AdditionalRefs xsi:nil=""true"" />
</BillingTransaction>",
					"TST", "ENTDUMSVR", "SYD1.DUM", "TSF", orgHeader.PK, "AU");
				AssertXMLEquals(expectedMsg, transactionXml);
			}
		}
	}
}
