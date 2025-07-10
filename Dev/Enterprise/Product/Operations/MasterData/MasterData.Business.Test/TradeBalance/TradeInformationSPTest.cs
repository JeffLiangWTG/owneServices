using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class TradeInformationSPTest : ScriptTest
	{
		public void TestContactPhone_Address()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MainAddress.OA_Address1 = "Not override address";
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.MainAddress.OA_Phone = "666777888";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MainAddress.OA_Address1 = "Override Address For Test";
			org2.MainAddress.OA_RL_NKRelatedPortCode = "CN111";
			org2.MainAddress.OA_Phone = "987654321";

			var contact = org1.Contacts.AddNew();
			contact.OC_ContactName = "Contact P";
			contact.OC_Phone = "123456789";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.ToString();
			CreateInvoice(org1, glAccount);
			Factory.Save();

			var resultForOrg = RunScript();

			AssertEquals(1, resultForOrg.Rows.Count);
			AssertEquals("Not override address", resultForOrg.Rows[0]["Address1"]);
			AssertEquals("123456789", resultForOrg.Rows[0]["ContactPhoneNo"]);
			AssertEquals("AU", resultForOrg.Rows[0]["CountryCode"]);

			contact.OC_OH_AddressOverride = org2.PK;
			Factory.Save();

			resultForOrg = RunScript();

			AssertEquals(1, resultForOrg.Rows.Count);
			AssertEquals("Override Address For Test", resultForOrg.Rows[0]["Address1"]);
			AssertEquals("987654321", resultForOrg.Rows[0]["ContactPhoneNo"]);

			var address2 = org2.Addresses.AddNewMainAddress();
			address2.OA_Address1 = "Override Address with same country";
			address2.OA_RL_NKRelatedPortCode = GlbCompany.CurrentCompany.Country.RN_Code + "222";
			Factory.Save();

			resultForOrg = RunScript();
			AssertEquals(1, resultForOrg.Rows.Count);
			AssertEquals("Override Address with same country", resultForOrg.Rows[0]["Address1"]);
			AssertEquals("Use org1 main address phone number when override address phone number is empty", "666777888", resultForOrg.Rows[0]["ContactPhoneNo"]);
		}

		public void TestCalculatePeriods()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var reportDate = new ZDateTime(2021, 5, 1);
			CreateInvoice(org, glAccount, reportDate, 1);
			CreateInvoice(org, glAccount, reportDate.AddDays(1), -3.1M);
			CreateInvoice(org, glAccount, reportDate.AddDays(-30), 2);
			CreateInvoice(org, glAccount, reportDate.AddDays(-60), 3.5M);
			CreateInvoice(org, glAccount, reportDate.AddDays(-90), 5.5M);
			CreateInvoice(org, glAccount, reportDate.AddDays(-120), 6.5M);
			CreateInvoice(org, glAccount, reportDate.AddDays(-120), 1.5M);
			CreateInvoice(org, glAccount, reportDate.AddDays(-121), 8.5M);

			Factory.Save();

			var resultForOrg = RunScript();

			AssertEquals(1, resultForOrg.Rows.Count);

			CombineAssertions("Calculate periods correctly", () =>
			{
				AssertEquals(1M, resultForOrg.Rows[0]["PeriodCurrent"]);
				AssertEquals(2M, resultForOrg.Rows[0]["Period1Total"]);
				AssertEquals(3.5M, resultForOrg.Rows[0]["Period2Total"]);
				AssertEquals(5.5M, resultForOrg.Rows[0]["Period3Total"]);
				AssertEquals(8M, resultForOrg.Rows[0]["Period4Total"]);
				AssertEquals(8.5M, resultForOrg.Rows[0]["Period4PlusTotal"]);
				AssertEquals(25.4M, resultForOrg.Rows[0]["Balance"]);
			});
		}

		public void TestMultiOrganizations()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "DUMMY FOR TEST 1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "DUMMY FOR TEST 2";
			var reportDate = new ZDateTime(2021, 5, 1);
			CreateInvoice(org1, glAccount, reportDate, 1);
			CreateInvoice(org1, glAccount, reportDate.AddDays(1), -3.1M);
			CreateInvoice(org1, glAccount, reportDate.AddDays(-30), 2);
			CreateInvoice(org2, glAccount, reportDate.AddDays(-60), 3.5M);
			CreateInvoice(org2, glAccount, reportDate.AddDays(-90), 5.5M);

			Factory.Save();

			var resultForOrg = RunScript();

			AssertEquals(2, resultForOrg.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK.ToGuid().ToString(), org2.PK.ToGuid().ToString() }, new[] { resultForOrg.Rows[0]["AccountPK"].ToString(), resultForOrg.Rows[1]["AccountPK"].ToString() });
			AssertContainsExactElementsInAnyOrder(new[] { "DUMMY FOR TEST 1", "DUMMY FOR TEST 2" }, new[] { resultForOrg.Rows[0]["AccountName"].ToString(), resultForOrg.Rows[1]["AccountName"].ToString() });
		}

		public void TestRegistrationNumbers()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var reportDate = new ZDateTime(2021, 5, 1);
			CreateInvoice(org, glAccount, reportDate, 1);

			org.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "11111", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia));
			org.CustomsCodes.AddNew(OrgCusCode.MozambiqueCodeTypes.GCR, "22222", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia));
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CompanyNumber, "33333", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.NewZealand));
			org.CustomsCodes.AddNew(OrgCusCode.SamoaCodeTypes.GST, "44444", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.NewZealand));
			var duns = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "55555");
			duns.OK_RN_NKCodeCountry = ZString.Empty;

			Factory.Save();

			var resultForOrg = RunScript();

			AssertEquals(1, resultForOrg.Rows.Count);

			CombineAssertions(() =>
			{
				AssertEquals("11111", resultForOrg.Rows[0]["ABN"]);
				AssertEquals("22222", resultForOrg.Rows[0]["ACN"]);
				AssertEquals("33333", resultForOrg.Rows[0]["NZBN"]);
				AssertEquals("44444", resultForOrg.Rows[0]["NCN"]);
				AssertEquals("55555", resultForOrg.Rows[0]["DUNS"]);
			});
		}

		public void TestInvoiceTerm()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV210", TestObjectCreator.USD, 0.5M, 110M, 0M, 220M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, new ZDateTime(2021, 4, 1), new ZDateTime(2021, 4, 30), new ZDateTime(2021, 4, 30), false);
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = TestObjectCreator.FESDepartment.PK;

			TestObjectCreator.ABIGAS.CompanyData.ARTerms.DeleteAll();

			var arTerm1 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm1, "ALL", ZGuid.Empty, TestObjectCreator.FESDepartment.PK, "ALL", "ALL", "ALL", "COD", 0);

			Factory.Save();

			var resultForOrg = RunScript();

			AssertEquals(1, resultForOrg.Rows.Count);
			AssertEquals("COD", resultForOrg.Rows[0]["InvoiceTerm"]);
		}

		void SetupTermsInfo(OrgARTerms term, string jobType, ZGuid branchPK, ZGuid deptPK, string direction, string transportMode, string invoiceType, string invoiceTerm, int termDays)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
				term.PY_InvoiceTerm = invoiceTerm;
				term.PY_InvoiceDays = (ZByte)termDays;
			}
		}

		public void TestNotThrowArithmeticOverflowErrorWhenBalanceIsOverflowThanMoney()
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var glAccount = TestObjectCreator.GetGLAccountFromDB();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var reportDate = new ZDateTime(2021, 5, 1);
				CreateInvoice(org, glAccount, reportDate.AddDays(1), 3M);
				Factory.Save();

				CreateInvoice(org, glAccount, reportDate.AddDays(-121), 922337203685475M);
				Factory.Save();

				var resultForOrg = RunScript();
				AssertEquals(1, resultForOrg.Rows.Count);
				AssertEquals("The data exceeds the maximum length of Money(922,337,203,685,477.5807), but no error is reported", 922337203685478M, resultForOrg.Rows[0]["Balance"]);
			}
		}

		int invoiceNumber;

		protected override void SetUp()
		{
			base.SetUp();
			invoiceNumber = 0;
		}

		InvoicingBase CreateInvoice(OrgHeader org, AccGLHeader glAccount, ZDateTime? dueDate = null, decimal amount = 10M)
		{
			invoiceNumber++;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), $"INV{invoiceNumber}", TestObjectCreator.AUD, 1M, amount, 0M, amount, 0M, org, glAccount.PK);
			invoice.AH_PostDate = new ZDateTime(2021, 4, 1);
			invoice.AH_DueDate = dueDate ?? new ZDateTime(2021, 5, 1);

			return invoice;
		}

		DataTable RunScript()
		{
			var sql = $"exec TradeInformationSP '2021-05-01', '{GlbCompany.CurrentCompany.PK}'";

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
