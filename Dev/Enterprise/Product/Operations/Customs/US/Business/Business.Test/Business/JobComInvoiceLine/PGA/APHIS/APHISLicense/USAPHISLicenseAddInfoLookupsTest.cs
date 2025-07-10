using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAPHISLicenseAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountries()
		{
			var list = License.AddInfoLookups.Countries;
			AssertEquals("Countries", typeof(RefCountryCollection), list.GetType());
		}

		public void TestOrganisations()
		{
			var list = License.AddInfoLookups.Organisations;
			AssertEquals("Organisations", typeof(OrganisationsFindBoxCollection), list.GetType());
		}

		public void TestStateList()
		{
			License.US_RN_CountryCode = ZString.Empty;
			var list = License.AddInfoLookups.StateList;
			AssertEquals("StateList empty", true, list.CompleteFilter.IsNoResultQuery);

			License.US_RN_CountryCode = Core.Constants.CountryCodes.Canada;
			var country = License.Country;
			list = License.AddInfoLookups.StateList;
			AssertEquals("StateList CanadianProvince", country.States.CompleteFilter, list.CompleteFilter);
		}

		public void TestLicenseTypes()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AAC;
			var list = License.AddInfoLookups.LicenseTypes;
			AssertEquals("LicenseTypes", APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AAC), list);
			Header.US_ProgramType = APHISProgramCodeList.Codes.ABS;
			list = License.AddInfoLookups.LicenseTypes;
			AssertEquals("LicenseTypes", APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.ABS), list);
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			list = License.AddInfoLookups.LicenseTypes;
			AssertEquals("LicenseTypes", APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS), list);

			AssertEquals("Manufacturer’s Statement/Certificate/Declaration", License.AddInfoLookups.LicenseTypes.GetDescriptionFromCode("A25"));
		}

		public void TestDateQualifiers()
		{
			var list = License.AddInfoLookups.DateQualifiers;
			AssertEquals("DateQualifiers", LPCODateQualifierList.GetListForAPHIS(Factory), list);
		}

		public void TestUnitOfMeasureList()
		{
			var list = License.AddInfoLookups.UnitOfMeasureList;
			AssertNotNull(list);
			Assert("LPCO Unit of Measure should allow ACE Appendix B codes", list.ContainsCode(AppendixBUnitsMeasureList.Codes.AST));
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		APHISHeader Header
		{
			get
			{
				if (aphisHeader == null)
				{
					aphisHeader = InvoiceLine.APHISHeaders.AddNew();
					aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
				}
				return aphisHeader;
			}
		}
		APHISHeader aphisHeader;

		APHISLicense License
		{
			get { return license ?? (license = Header.Licenses.AddNew()); }
		}
		APHISLicense license;

		#endregion
	}
}
