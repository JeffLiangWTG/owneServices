using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USOMCAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUSCountries()
		{
			Setup();
			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A2;
			var countries7A2 = Header.AddInfoLookups.USCountries;
			countries7A2.Load();
			AssertNotNull(countries7A2.OfType<USCCountry>().FirstOrDefault(x => x.UC_Code == Core.Constants.CountryCodes.Australia));

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A4;
			var countries7A4 = Header.AddInfoLookups.USCountries;
			countries7A4.Load();
			AssertNotNull(countries7A4.OfType<USCCountry>().FirstOrDefault(x => x.UC_Code == Core.Constants.CountryCodes.KoreaSouth));

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7B;
			var countries7B = Header.AddInfoLookups.USCountries;
			countries7B.Load();
			AssertNotNull(countries7B.OfType<USCCountry>().FirstOrDefault(x => x.UC_Code == Core.Constants.CountryCodes.Uruguay));
		}

		public void TestOrganizations()
		{
			AssertNotNull(Header.AddInfoLookups.Organizations);
		}

		public void TestUnitOfMeasureList()
		{
			AssertNotNull(Header.AddInfoLookups.UnitOfMeasureList);
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
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

		OMCHeader Header
		{
			get { return header ?? (header = InvoiceLine.OMCHeaders.AddNew()); }
		}
		OMCHeader header;

		void Setup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ConformanceDeclarationCodeList.Codes._7A2, ConformanceDeclarationCodeList.Codes._7A2);
			helper.CreateNewOrGetExistingCusCodeType(ConformanceDeclarationCodeList.Codes._7A4, ConformanceDeclarationCodeList.Codes._7A4);
			helper.CreateNewOrGetExistingCusCodeType(ConformanceDeclarationCodeList.Codes._7B, ConformanceDeclarationCodeList.Codes._7B);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, ConformanceDeclarationCodeList.Codes._7A2, Core.Constants.CountryCodes.Australia, "Australia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, ConformanceDeclarationCodeList.Codes._7A4, Core.Constants.CountryCodes.KoreaSouth, "Korea, South", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, ConformanceDeclarationCodeList.Codes._7B, Core.Constants.CountryCodes.Uruguay, "Uruguay", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		#endregion
	}
}
