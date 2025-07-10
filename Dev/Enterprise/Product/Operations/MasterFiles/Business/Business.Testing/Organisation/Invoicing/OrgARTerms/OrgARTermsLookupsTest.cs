using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgARTermsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAgreedPaymentMethodList()
		{
			AssertEquals(OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList(), lookups.AgreedPaymentMethodList);
		}

		public void TestInvoiceTermListWithoutDefaultValue()
		{
			AssertEquals("InvoiceTermListWithoutDefaultValue.Count", new ARInvoiceTermsList().Count, lookups.InvoiceTermListWithoutDefaultValue.Count);
			AssertCollectionContains("MonthsFromInvoiceCycleDate must be in a list", InvoiceTermsList.MonthsFromInvoiceCycleDate, lookups.InvoiceTermListWithoutDefaultValue);
			AssertCollectionContains("TermDaysAndDebtorPaymentCycle must be in a list", InvoiceTermsList.TermDaysAndDebtorPaymentCycle, lookups.InvoiceTermListWithoutDefaultValue);
		}

		public void TestInvoiceTermList()
		{
			AssertEquals("InvoiceTermList.Count", new ARInvoiceTermsList().Count + 1, lookups.InvoiceTermList.Count);
			AssertCollectionContains("InvoiceTermList contains 'DEF'", OrgARTermsLookups.DefaultInvoiceTerm, lookups.InvoiceTermList);

			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgHeader settlementOrganisation = Factory.New<OrgHeader>();

			OrgARTermsLookups orgTermAllLookups = organisation.CompanyData.LoadARTermForAllInvoiceTypes().Lookups;
			OrgARTermsLookups orgTermDSBLookups = organisation.CompanyData.CreateOrLoadDisbursementARTerm().Lookups;

			AssertEquals("orgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description, orgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("orgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description, orgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));

			organisation.ARSettlementGroupPK = organisation.PK;
			AssertEquals("orgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description, orgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("orgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description, orgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));

			organisation.ARSettlementGroupPK = settlementOrganisation.PK;
			AssertEquals("orgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (COD)", orgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("orgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (COD)", orgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals("orgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description, orgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("orgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description, orgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			settlementOrganisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 1;
			AssertEquals("orgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (INV, 1 day)", orgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("orgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (INV, 1 day)", orgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			settlementOrganisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 0;
			AssertEquals("orgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (INV, 0 days)", orgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("orgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (INV, 0 days)", orgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			AssertEquals("orgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (PIA)", orgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("orgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (PIA)", orgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;
			settlementOrganisation.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 5;
			AssertEquals("orgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (PIA)", orgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("orgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description + " (MTH, 5 days)", orgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));

			OrgARTermsLookups settlementOrgTermAllLookups = settlementOrganisation.CompanyData.LoadARTermForAllInvoiceTypes().Lookups;
			OrgARTermsLookups settlementOrgTermDSBLookups = settlementOrganisation.CompanyData.CreateOrLoadDisbursementARTerm().Lookups;
			AssertEquals("settlementOrgTermAllLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description, settlementOrgTermAllLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
			AssertEquals("settlementOrgTermDSBLookups", OrgARTermsLookups.DefaultInvoiceTerm.Description, settlementOrgTermDSBLookups.InvoiceTermList.GetDescriptionFromCode(OrgARTermsLookups.DefaultInvoiceTerm.Code));
		}

		public void TestInvoiceTypeList()
		{
			arTerms.PY_JobType = ZString.Empty;
			AssertEquals("InvoiceTypeList.Count", /*new InvoiceTypesList().Count*/1 + 1, lookups.InvoiceTypeList.Count);
			AssertEquals("InvoiceTypeList contains type 'ALL'", OrgARTermsLookups.InvoiceTypes.All, lookups.InvoiceTypeList[0]);
			AssertEquals("InvoiceTypeList contains type 'DSB'", OrgARTermsLookups.InvoiceTypes.DSB, lookups.InvoiceTypeList[1]);
			AssertCollectionNotContains("InvoiceTypeList does not contain type 'NON'", InvoiceTypesList.Codes.DoNotPost, lookups.InvoiceTypeList);

			arTerms.PY_JobType = new AllJobsConsumerType().Code;
			var expectedInvoiceTypes = new string[]
							{
											"ALL", "DSB",
											"DES", "DED", "DCU", "DCD",
											"DBT", "DBD", "FIN",
											"FID", "CUR", "CUD", "FRT",
											"FRD", "ITC", "ITD", "SBR",
											"SBD"
							};

			AssertEquals("InvoiceType: No Item Should be found", false, lookups.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedInvoiceTypes).Any());
			AssertEquals("InvoiceType: No Item Should be found", false, expectedInvoiceTypes.Except(lookups.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());

			arTerms.PY_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			expectedInvoiceTypes = new string[]
							{
											"ALL", "DSB",
											"DES", "DED", "DCU", "DCD",
											"DBT", "DBD", "FIN",
											"FID", "CUR", "CUD", "FRT",
											"FRD", "ITC", "ITD", "SBR",
											"SBD"
							};

			AssertEquals("InvoiceType: No Item Should be found", false, lookups.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedInvoiceTypes).Any());
			AssertEquals("InvoiceType: No Item Should be found", false, expectedInvoiceTypes.Except(lookups.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());

			arTerms.PY_JobType = JobInvoicingConsumerTypes.AgencyBooking.Code;
			expectedInvoiceTypes = new string[]
							{
											"ALL", "DSB",
											"FCO", "FCD",
											"FPP", "FPD", "LCO",
											"LCD", "LPP", "LPD",
											"MSC", "MSD"
							};

			AssertEquals("InvoiceType: No Item Should be found", false, lookups.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedInvoiceTypes).Any());
			AssertEquals("InvoiceType: No Item Should be found", false, expectedInvoiceTypes.Except(lookups.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());

			arTerms.PY_JobType = JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code;
			expectedInvoiceTypes = new string[]
							{
											"ALL", "DSB",
											"DES", "DCU", "DCD",
											"DBT", "DBD", "FIN",
											"FID", "CUR", "CUD", "FRT",
											"ITC", "ITD", "SBR",
											"SBD"
							};

			AssertEquals("InvoiceType: No Item Should be found", false, lookups.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedInvoiceTypes).Any());
			AssertEquals("InvoiceType: No Item Should be found", false, expectedInvoiceTypes.Except(lookups.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
		}

		public void TestJobTypeList()
		{
			var expectedJobTypes = new string[]
							{
											"ALL",
											"SHP", "QSH",
											"FCN", "GCN", "BRK", "PCB",
											"AWB", "CSH", "CLL",
											"CST", "TRN", "TBM", "ABK", "ATB",
											"TCW", "LTC", "WIN", "WOU", "WSJ","WVO",
											"WST", "WSC",
											"ACR", "UBR", "CTO",
											"AHW", "AHE", "AGS",
											"AGB", "ACD", "AVA",
											"ASC", "ISF",
											"MAN", "CAE",
											"WKI", "WKP", "WKR",
											"YRA", "YRE", "YTU", "MWO", "YAO", "YPI",
											"TRC", "TDC", "TRU", "TDL", "TDU",
											"NCT", "LPC", "STO"
							};

			AssertEquals("JobType: No Item Should be found", false, lookups.JobTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedJobTypes).Any());
			AssertEquals("JobType: No Item Should be found", false, expectedJobTypes.Except(lookups.JobTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
		}

		public void TestDirectionList()
		{
			var expectedDirections = new string[]
							{
											"ALL", "EXP", "IMP", "DOM", "OTH"
							};

			AssertEquals("Direction: No Item Should be found", false, lookups.DirectionList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedDirections).Any());
			AssertEquals("Direction: No Item Should be found", false, expectedDirections.Except(lookups.DirectionList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
		}

		public void TestTransportModeList()
		{
			var expectedTransportModes = new string[]
							{
											"ALL", "AIR", "SEA", "FSA", "FAS", "ROA", "RAI", "COU"
							};

			AssertEquals("Transport Mode: No Item Should be found", false, lookups.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedTransportModes).Any());
			AssertEquals("Transport Mode: No Item Should be found", false, expectedTransportModes.Except(lookups.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			arTerms = org1.CompanyData.ARTerms[0];
			SetupTermsInfo(arTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");
			lookups = arTerms.Lookups;
			Factory.Save();
		}

		void SetupTermsInfo(OrgARTerms term, ZString jobType, ZGuid branchPK, ZGuid deptPK, ZString direction, ZString transportMode, ZString invoiceType)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
			}
		}

		OrgARTerms arTerms;
		OrgARTermsLookups lookups;
	}
}
