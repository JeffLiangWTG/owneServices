using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class CompanyProviderTest : TestCaseWithFactory
	{
		public void TestObjectFactory()
		{
			var companyProvider = ObjectFactory.Get<ICompanyProvider>();
			AssertNotNull("ObjectFactory always returns a valid company provider", companyProvider);

			var companyProvider2 = ObjectFactory.Get<ICompanyProvider>();
			AssertNotSame("ICompanyProvider should not be not a singleton", companyProvider, companyProvider2);
		}

		public void TestGet_ReturnsNone_WhenInvalidCompanyPK()
		{
			var companyProvider = ObjectFactory.Get<ICompanyProvider>().WithFactory(Factory);

			var emptyResult = companyProvider.Get(ZGuid.Empty);
			AssertEquals("Empty guid should return None", false, emptyResult.TryGet(out var _));

			var invalidResult = companyProvider.Get(ZGuid.Invalid);
			AssertEquals("Invalid guid should return None", false, invalidResult.TryGet(out var _));

			var missingResult = companyProvider.Get(ZGuid.Missing);
			AssertEquals("Missing guid should return None", false, missingResult.TryGet(out var _));
		}

		public void TestGet_ReturnsNone_WhenUnknownCompanyPK()
		{
			var companyProvider = ObjectFactory.Get<ICompanyProvider>().WithFactory(Factory);

			var unknownResult = companyProvider.Get(ZGuid.NewZGuid());
			AssertEquals("Unknown company guid should return None", false, unknownResult.TryGet(out var _));
		}

		public void TestGet_ReturnsCompany_WhenCompanyInDatabase()
		{
			var pkForEdiCompany = new ZGuid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			var companyProvider = ObjectFactory.Get<ICompanyProvider>().WithFactory(Factory);

			var ediResult = companyProvider.Get(pkForEdiCompany);
			AssertEquals("EDI company guid should return Some", true, ediResult.TryGet(out var ediCompany));
			AssertEquals("EDI company object should be returned", "EDI", ediCompany.Code);
		}

		public void TestGet_IsProtectedAgainstDeepNulls()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AAA";
			company.GC_Name = "AAA Pty Ltd";
			// Badly configured company has invalid country.
			company.GC_RN_NKCountryCode = "__";
			Factory.Save();

			var companyProvider = ObjectFactory.Get<ICompanyProvider>().WithFactory(Factory);

			var aaaResult = companyProvider.Get(company.PK);
			AssertEquals("AAA company should return Some", true, aaaResult.TryGet(out var aaaCompany));
			AssertEquals("AAA company object should be returned", "AAA", aaaCompany.Code);

			AssertNull("Null country should be returned because GC_RN_NKCountryCode is empty", aaaCompany.Country);
			var emptyCountryCode = aaaResult.Map(c => c.Country).Map(c => c.Code).SomeOrDefault("");
			AssertEquals("Using Option.Map() allows us to safely traverse any object graph, even when there are nulls.", "", emptyCountryCode);
			var emptyCountryCode2 = aaaResult.GetCountryCode().SomeOrDefault("");
			AssertEquals("Using Option.GetCountryCode() also allows us to safely traverse the object graph, but with a bit less typing.", "", emptyCountryCode2);

			var auCountryCode = aaaResult.GetCountryCode().SomeOrDefault("AU");
			AssertEquals("Using Option.GetCountryCode() allows us to safely return a default value, even when there are nulls in the object graph.", "AU", auCountryCode);

			var auCountryCode2 = aaaResult.Map(c => c.Country?.Code).SomeOrDefault("AU");
			AssertEquals("Using Option.SomeOrDefault() allows us to safely return a default value, even when there are nulls in the object graph.", "AU", auCountryCode2);
		}

		public void TestGet_IsProtectedAgainstNullCompany()
		{
			var companyProvider = ObjectFactory.Get<ICompanyProvider>().WithFactory(Factory);

			var unknownResult = companyProvider.Get(ZGuid.NewZGuid());
			AssertEquals("Unknown guid should return None", false, unknownResult.TryGet(out var _));

			var currencyCode = unknownResult.Map(c => c.LocalCurrency).Map(c => c.Code).SomeOrDefault("ABC");
			AssertEquals("Using Option.Map() allows us to safely traverse any object graph, even when ICompany is null.", "ABC", currencyCode);
		}

		public void TestProviderFactory_MustBeSet()
		{
			var companyProvider = ObjectFactory.Get<ICompanyProvider>();
			AssertExceptionThrown<InvalidOperationException>("Factory property must be set", "Factory has not been initialized. You must set the Factory property or use WithFactory() extension method. Sorry, using constructors is rather painful with ObjectFactory; please pass a factory.", () => companyProvider.Get(ZGuid.NewZGuid()));
		}

		public void TestProviderFactory_IsUsedByGet()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AAA";
			company.GC_Name = "AAA Pty Ltd";
			company.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadsBefore = newFactory.DatabaseLoadCount;
			var companyProvider = ObjectFactory.Get<ICompanyProvider>().WithFactory(newFactory);
			var companyFromProvider = companyProvider.Get(company.PK);
			var loadsAfter = newFactory.DatabaseLoadCount;
			var totalLoads = loadsAfter - loadsBefore;

			Assert("Company is loaded", companyFromProvider.TryGet(out var _));
			AssertEquals("An external factory was used by the provider, so one load is expected", 1, totalLoads);
		}

		public void TestProviderFactory_CanOnlyBeSetOnce()
		{
			var companyProvider = ObjectFactory.Get<ICompanyProvider>().WithFactory(Factory);
			AssertNotNull("Helper method can be used to set factory", companyProvider);

			AssertExceptionThrown<InvalidOperationException>(
				"The Factory property can only be set once",
				"Factory has already been initialized and cannot be overridden. If you want to provide an external Factory, it must be done before any other methods are called.",
				() => companyProvider.WithFactory(Factory)
			);
		}
	}
}
