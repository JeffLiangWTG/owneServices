using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Web.WebService;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Web.Testing
{
	[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
	sealed class AccountingServiceTest : BaseWebServiceTest<AccountingService>, IHttpContextEnabledTestWithAppInstance
	{
		#region TestGetInvoiceListInternalEmpty

		public void TestGetInvoiceListInternalEmpty()
		{
			Xsd.FinancialInvoices result = WebService.GetInvoiceListInternal(null, GlbCompany.CurrentCompany, ZGuid.Empty);
			AssertNotNull("even though there are no invoice, it should return something", result);
			AssertEquals("empty content", 0, result.TxnHeader.Count);
		}

		#endregion

		#region TestGetInvoiceListWithFilter

		public void TestGetInvoiceListWithFilter()
		{
			var filter = new Xsd.WebInvoiceFilter();

			filter.Company = TestCompany.GC_Code;
			var result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 1, "Invoice4");

			filter.Company = CurrentTestCompany.GC_Code;
			result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 3, "Invoice1, Invoice2, Invoice3");

			//Ledger Type Filter
			filter.LedgerTypes.Add(Xsd.TxnLedgerType.AR);
			result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 2, "Invoice1, Invoice2");

			//Transaction Type Filter
			filter.TxnTypes.Add(Xsd.TxnType.INV);
			result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 1, "Invoice1");

			//postdateFrom filter
			filter.PostDateFrom = new DateTime(2006, 12, 12, 12, 12, 12);
			result = WebService.GetInvoiceList(filter);
			AssertNotNull("even though there are no invoice, it should return something", result);
			AssertEquals("empty content", 0, result.TxnHeader.Count);

			filter.PostDateFrom = new DateTime(2006, 5, 5, 23, 12, 12);
			filter.TxnTypes.Add(Xsd.TxnType.ADJ);
			result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 2, "Invoice1, Invoice2");

			//make sure the filter compare the date only instead of datetime
			filter.PostDateTo = new DateTime(2006, 6, 6, 0, 0, 0);
			result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 2, "Invoice1, Invoice2");

			filter.PostDateTo = new DateTime(2006, 5, 10, 0, 0, 0);
			result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 1, "Invoice2");
		}

		#region AssertFilterMatches

		void AssertFilterMatches(Xsd.WebInvoiceFilter filter, Xsd.FinancialInvoices invoices, int numOfInvoice, string invoiceNumbers)
		{
			AssertEquals("Number of Invoices", numOfInvoice, invoices.TxnHeader.Count);

			foreach (Xsd.TxnHeader invoice in invoices.TxnHeader)
			{
				if (filter.LedgerTypes.Count > 0)
				{
					AssertCollectionContains("LedgerType", invoice.Ledger, filter.LedgerTypes);
				}

				if (filter.TxnTypes.Count > 0)
				{
					AssertCollectionContains("TransactionType", invoice.TxnType, filter.TxnTypes);
				}

				if (filter.PostDateFrom.IsValid)
				{
					AssertEquals("Invoice PostDate >= Filter Post Date From", true, invoice.PostDate.Date >= filter.PostDateFrom.Date);
				}

				if (filter.PostDateTo.IsValid)
				{
					AssertEquals("Invoice PostDate <= Filter Post Date To", true, invoice.PostDate.Date < filter.PostDateTo.Date.AddDays(1));
				}

				AssertContains("Invoice Number", invoice.TxnNumber, invoiceNumbers);
			}
		}

		#endregion

		#endregion

		#region TestFilterExcludeChargeCodeOnRegistry

		public void TestFilterExcludeChargeCodeOnRegistry()
		{
			var filter = new Xsd.WebInvoiceFilter();
			filter.Company = CurrentTestCompany.GC_Code;
			filter.LedgerTypes.Add(Xsd.TxnLedgerType.AR);

			var result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertEquals("Should contain 2 invoices", 2, result.TxnHeader.Count);

			WebDataRegistry.Instance.WebServiceChargeCodes.SetValue(CurrentTestCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CurrentTestCompanyChargeCode.PK.ToGuid().ToString());

			result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertEquals("Should contain 1 invoice", 1, result.TxnHeader.Count);

			WebDataRegistry.Instance.WebServiceChargeCodes.SetValue(TestCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestCompanyChargeCode.PK.ToGuid().ToString());

			result = WebService.GetInvoiceList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertEquals("Should contain 1 invoice", 1, result.TxnHeader.Count);

			filter.Company = TestCompany.GC_Code;
			result = WebService.GetInvoiceList(filter);
			AssertEquals("empty content", 0, result.TxnHeader.Count);
		}

		#endregion

		#region TestGetInvoicePKListInternalEmpty

		public void TestGetInvoicePKListInternalEmpty()
		{
			Xsd.BusinessObjectPKCollection result = WebService.GetInvoicePKListInternal(null, GlbCompany.CurrentCompany, ZGuid.Empty);
			AssertNotNull("even though there are no invoice, it should return something", result);
			AssertEquals("empty content", 0, result.Count);
		}

		#endregion

		#region TestGetInvoicePKListWithFilter

		public void TestGetInvoicePKListWithFilter()
		{
			var filter = new Xsd.WebInvoiceFilter();

			filter.Company = TestCompany.GC_Code;
			var result = WebService.GetInvoicePKList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 1, Invoice4.PK);

			filter.Company = CurrentTestCompany.GC_Code;
			result = WebService.GetInvoicePKList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 3, Invoice1.PK, Invoice2.PK, Invoice3.PK);

			//Ledger Type Filter
			filter.LedgerTypes.Add(Xsd.TxnLedgerType.AR);
			result = WebService.GetInvoicePKList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 2, Invoice1.PK, Invoice2.PK);

			//Transaction Type Filter
			filter.TxnTypes.Add(Xsd.TxnType.INV);
			result = WebService.GetInvoicePKList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 1, Invoice1.PK);

			//postdateFrom filter
			filter.PostDateFrom = new DateTime(2006, 12, 12, 12, 12, 12);
			result = WebService.GetInvoicePKList(filter);
			AssertNotNull("even though there are no invoice, it should return something", result);
			AssertEquals("empty content", 0, result.Count);

			filter.PostDateFrom = new DateTime(2006, 5, 5, 23, 12, 12);
			filter.TxnTypes.Add(Xsd.TxnType.ADJ);
			result = WebService.GetInvoicePKList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 2, Invoice1.PK, Invoice2.PK);

			//make sure the filter compare the date only instead of datetime
			filter.PostDateTo = new DateTime(2006, 6, 6, 0, 0, 0);
			result = WebService.GetInvoicePKList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 2, Invoice1.PK, Invoice2.PK);

			filter.PostDateTo = new DateTime(2006, 5, 10, 0, 0, 0);
			result = WebService.GetInvoicePKList(filter);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(filter, result, 1, Invoice2.PK);
		}

		#region AssertFilterMatches

		void AssertFilterMatches(Xsd.WebInvoiceFilter filter, Xsd.BusinessObjectPKCollection invoicePKs, int numOfInvoice, params ZGuid[] pKs)
		{
			AssertEquals("Number of Invoices", numOfInvoice, invoicePKs.Count);

			foreach (Xsd.BusinessObjectPK invoicePK in invoicePKs)
			{
				InvoicingBase invoice = Factory.Load<InvoicingBase>(new ZGuid(invoicePK.Pk));

				if (filter.LedgerTypes.Count > 0)
				{
					ZString[] ledgerTypes = new ZString[filter.LedgerTypes.Count];
					for (int i = 0; i < ledgerTypes.Length; i++)
					{
						ledgerTypes[i] = filter.LedgerTypes[i].ToString();
					}

					AssertCollectionContains("LedgerType", invoice.AH_Ledger, ledgerTypes);
				}

				if (filter.TxnTypes.Count > 0)
				{
					ZString[] txnTypes = new ZString[filter.TxnTypes.Count];
					for (int i = 0; i < txnTypes.Length; i++)
					{
						txnTypes[i] = filter.TxnTypes[i].ToString();
					}

					AssertCollectionContains("TransactionType", invoice.AH_TransactionType, txnTypes);
				}

				if (filter.PostDateFrom.IsValid)
				{
					AssertEquals("Invoice PostDate >= Filter Post Date From", true, invoice.AH_PostDate.Date >= filter.PostDateFrom.Date);
				}

				if (filter.PostDateTo.IsValid)
				{
					AssertEquals("Invoice PostDate <= Filter Post Date To", true, invoice.AH_PostDate.Date < filter.PostDateTo.Date.AddDays(1));
				}

				AssertCollectionContains("Invoice PKs", invoice.PK, pKs);
			}
		}

		#endregion

		#endregion

		#region TestGetInvoiceListForPKs

		public void TestGetInvoiceListForPKs()
		{
			var pKs = new Xsd.BusinessObjectPKCollection();

			var result = WebService.GetInvoiceListForPKs(pKs);
			AssertNotNull("even though there are no invoice, it should return something", result);
			AssertEquals("empty content", 0, result.TxnHeader.Count);

			var pk = pKs.AddNew();
			pk.Pk = Invoice1.PK.ToString();
			result = WebService.GetInvoiceListForPKs(pKs);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(result, 1, "Invoice1");

			pk = pKs.AddNew();
			pk.Pk = Invoice2.PK.ToString();
			pk = pKs.AddNew();
			pk.Pk = Invoice3.PK.ToString();
			result = WebService.GetInvoiceListForPKs(pKs);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(result, 3, "Invoice1, Invoice2, Invoice3");
		}

		[ExpectExceptionMessage(typeof(Exception), "ERROR: Invalid Guid value: 'Some Crap'")]
		public void TestGetInvoiceListForPKsException()
		{
			var pKs = new Xsd.BusinessObjectPKCollection();

			var result = WebService.GetInvoiceListForPKs(pKs);
			AssertNotNull("even though there are no invoice, it should return something", result);
			AssertEquals("empty content", 0, result.TxnHeader.Count);

			Xsd.BusinessObjectPK pk = pKs.AddNew();
			pk.Pk = Invoice1.PK.ToString();
			result = WebService.GetInvoiceListForPKs(pKs);
			AssertNotNull("there are invoices, it should return something", result);
			AssertFilterMatches(result, 1, "Invoice1");

			pk = pKs.AddNew();
			pk.Pk = "Some Crap";
			result = WebService.GetInvoiceListForPKs(pKs);
			Fail("Exception should be thrown");
		}

		#region AssertFilterMatches

		void AssertFilterMatches(Xsd.FinancialInvoices invoices, int numOfInvoice, string invoiceNumbers)
		{
			AssertEquals("Number of Invoices", numOfInvoice, invoices.TxnHeader.Count);

			foreach (Xsd.TxnHeader invoice in invoices.TxnHeader)
			{
				AssertContains("Invoice Number", invoice.TxnNumber, invoiceNumbers);
			}
		}

		#endregion

		#endregion

		public void TestGetInvoiceListIsRestricted()
		{
			var filter = new Xsd.WebInvoiceFilter();
			filter.Company = CurrentTestCompany.GC_Code;

			var result = WebService.GetInvoiceList(filter);
			AssertFilterMatches(filter, result, 3, "Invoice1, Invoice2, Invoice3");
		}

		public void TestGetInvoicePKListIsRestricted()
		{
			var filter = new Xsd.WebInvoiceFilter();
			filter.Company = CurrentTestCompany.GC_Code;

			var result = WebService.GetInvoicePKList(filter);
			AssertFilterMatches(filter, result, 3, Invoice1.PK, Invoice2.PK, Invoice3.PK);
		}

		public void TestGetInvoiceListForPKsIsRestricted()
		{
			var filter = new Xsd.WebInvoiceFilter();
			filter.Company = CurrentTestCompany.GC_Code;

			var pkList = new Xsd.BusinessObjectPKCollection();
			Xsd.BusinessObjectPK pk;

			pk = pkList.AddNew();
			pk.Pk = Invoice1.PK.ToString();
			pk = pkList.AddNew();
			pk.Pk = Invoice2.PK.ToString();
			pk = pkList.AddNew();
			pk.Pk = Invoice3.PK.ToString();
			pk = pkList.AddNew();
			pk.Pk = Invoice4.PK.ToString();
			pk = pkList.AddNew();
			pk.Pk = Invoice5.PK.ToString();

			var result = WebService.GetInvoiceListForPKs(pkList);
			AssertFilterMatches(result, 4, "Invoice1, Invoice2, Invoice3, Invoice4");
		}

		public void TestSoapBodyElementNames()
		{
			// Callers expect to provide XML bodies beginning with capital letters.
			var getInvoicePKListParameters = typeof(AccountingService).GetMethod(nameof(AccountingService.GetInvoicePKList)).GetParameters();
			var getInvoicePKListXmlAttribute = getInvoicePKListParameters[0].GetCustomAttributes(true).OfType<XmlElementAttribute>().FirstOrDefault();

			AssertEquals("Filter", getInvoicePKListXmlAttribute.ElementName);

			var getInvoiceListParameters = typeof(AccountingService).GetMethod(nameof(AccountingService.GetInvoiceList)).GetParameters();
			var getInvoiceListXmlAttribute = getInvoiceListParameters[0].GetCustomAttributes(true).OfType<XmlElementAttribute>().FirstOrDefault();

			AssertEquals("Filter", getInvoiceListXmlAttribute.ElementName);

			var getInvoiceListForPKsParameters = typeof(AccountingService).GetMethod(nameof(AccountingService.GetInvoiceListForPKs)).GetParameters();
			var getInvoiceListForPksXmlAttribute = getInvoiceListForPKsParameters[0].GetCustomAttributes(true).OfType<XmlElementAttribute>().FirstOrDefault();

			AssertEquals("PKs", getInvoiceListForPksXmlAttribute.ElementName);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			SetupOrgAndContact();
			SetupCompanies();
			SetupInvoices();
			Factory.Save();
		}

		GlbCompany TestCompany;
		GlbCompany CurrentTestCompany;
		AccChargeCode TestCompanyChargeCode;
		AccChargeCode CurrentTestCompanyChargeCode;
		OrgContact TestContact;

		void SetupOrgAndContact()
		{
			CurrentOrg.OH_IsDebtor = true;
			CurrentOrg.OH_IsCreditor = true;

			TestContact = CurrentOrg.Contacts.AddNew();
			TestContact.OC_Email = "test@edi.com.au";
			var password = "abc123";
			TestContact.SetHashedPassword(password);
			TestContact.OC_WebAccessEnabled = true;

			Factory.Save();

			WebService.MessageHeader.UserName = TestContact.OC_Email;
			WebService.MessageHeader.Password = password;
		}

		#region SetupCompany

		void SetupCompanies()
		{
			TestCompany = Factory.NewWithValidTestData<GlbCompany>();
			TestCompany.GC_Code = "ABC";

			var testBranch = TestCompany.Branches.AddNew();
			testBranch.GB_Code = "BR1";

			CurrentTestCompany = Factory.NewWithValidTestData<GlbCompany>();
			CurrentTestCompany.GC_Code = "DEF";

			var currentTestBranch = CurrentTestCompany.Branches.AddNew();
			currentTestBranch.GB_Code = "BR2";
		}

		#endregion

		#region SetupInvoices

		void SetupInvoices()
		{
			var otherDebtorCreditor = Factory.New<OrgHeader>();
			otherDebtorCreditor.OH_IsDebtor = true;
			otherDebtorCreditor.OH_IsCreditor = true;
			otherDebtorCreditor.OH_Code = "OtherDebtor";

			CurrentTestCompanyChargeCode = Factory.New<AccChargeCode>();
			CurrentTestCompanyChargeCode.AC_Code = "DEF";
			CurrentTestCompanyChargeCode.AC_GC = CurrentTestCompany.PK;
			CurrentTestCompanyChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			CurrentTestCompanyChargeCode.AC_IsActive = true;

			Invoice1 = Factory.New<ARInvoice>();
			Invoice1.AH_OH = CurrentOrg.PK;
			Invoice1.AH_GB = CurrentTestCompany.Branches[0].PK;
			Invoice1.AH_PostDate = new DateTime(2006, 6, 6, 6, 6, 6);

			var line1 = (ARInvoiceLine)Invoice1.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100;
			line1.AL_AC = CurrentTestCompanyChargeCode.PK;
			line1.AL_GB = CurrentTestCompany.Branches[0].PK;

			Invoice2 = Factory.New<ARAdjustmentNote>();
			Invoice2.AH_GB = CurrentTestCompany.Branches[0].PK;
			Invoice2.AH_PostDate = new DateTime(2006, 5, 5, 5, 5, 5);
			Invoice2.AH_OH = CurrentOrg.PK;

			var line2 = (ARAdjustmentNoteLine)Invoice2.Lines.AddNew();
			line2.AL_OSExTaxAmount = 200;
			line2.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			line2.AL_GB = CurrentTestCompany.Branches[0].PK;

			Invoice3 = Factory.New<APInvoice>();
			Invoice3.AH_TransactionNum = "APINV3";
			Invoice3.AH_GB = CurrentTestCompany.Branches[0].PK;
			Invoice3.AH_OH = CurrentOrg.PK;

			var line3 = (APInvoiceLine)Invoice3.Lines.AddNew();
			line3.AL_OSExTaxAmount = 999;
			line3.AL_AC = Env.Registry.FreightChargeCode;
			line3.AL_GB = CurrentTestCompany.Branches[0].PK;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GB = CurrentTestCompany.Branches[0].PK;
			line3.AL_JH = job.PK;

			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge.JR_AL_APLine = line3.PK;
			jobCharge.JR_GB = CurrentTestCompany.Branches[0].PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.SetAmountsFromLinkedLinesForTests();

			Invoice4 = Factory.New<ARInvoice>();
			Invoice4.AH_GB = TestCompany.Branches[0].PK;
			Invoice4.AH_OH = CurrentOrg.PK;

			var chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, TestCompany.PK);
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Disbursement);
			TestCompanyChargeCode = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);

			if (TestCompanyChargeCode == null)
			{
				TestCompanyChargeCode = Factory.New<AccChargeCode>();
				TestCompanyChargeCode.AC_Code = "ABC";
				TestCompanyChargeCode.AC_GC = TestCompany.PK;
				TestCompanyChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				TestCompanyChargeCode.AC_IsActive = true;
			}

			var line4 = (ARInvoiceLine)Invoice4.Lines.AddNew();
			line4.AL_OSExTaxAmount = 400;
			line4.AL_AC = TestCompanyChargeCode.PK;
			line4.AL_GB = TestCompany.Branches[0].PK;

			Invoice5 = Factory.New<ARAdjustmentNote>();
			Invoice5.AH_GB = CurrentTestCompany.Branches[0].PK;
			Invoice5.AH_PostDate = new DateTime(2006, 5, 5, 5, 5, 5);
			Invoice5.AH_OH = otherDebtorCreditor.PK;

			var line5 = (ARAdjustmentNoteLine)Invoice5.Lines.AddNew();
			line5.AL_OSExTaxAmount = 200;
			line5.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			line5.AL_GB = CurrentTestCompany.Branches[0].PK;

			Invoice1.IsManuallySetTransactionNumber_ForTestOnly = true;
			Invoice2.IsManuallySetTransactionNumber_ForTestOnly = true;
			Invoice3.IsManuallySetTransactionNumber_ForTestOnly = true;
			Invoice4.IsManuallySetTransactionNumber_ForTestOnly = true;
			Invoice5.IsManuallySetTransactionNumber_ForTestOnly = true;

			Invoice1.AH_TransactionNum = "Invoice1";
			Invoice2.AH_TransactionNum = "Invoice2";
			Invoice3.AH_TransactionNum = "Invoice3";
			Invoice4.AH_TransactionNum = "Invoice4";
			Invoice5.AH_TransactionNum = "Invoice5";

			Factory.Save();
		}

		ARInvoice Invoice1;
		ARAdjustmentNote Invoice2;
		APInvoice Invoice3;
		ARInvoice Invoice4;
		ARAdjustmentNote Invoice5;

		#endregion

		#endregion

		ZEnterpriseGlobalBase IHttpContextEnabledTestWithAppInstance.AppInstance => new Global();
	}
}
