using System.Collections;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestsSubclassesOf(typeof(GlbCompanyWrapperProvider))]
	public abstract class GlbCompanyWrapperProviderTest<T> : TestCaseWithFactory
		where T : GlbCompanyWrapperProvider, new()
	{
		public void TestGetWrapper()
		{
			var provider = new T();
			var company = Factory.New<GlbCompany>();

			var wrapper1 = provider.GetWrapper(company);
			var wrapper2 = provider.GetWrapper(company);

			AssertSame("Wrapper should be cached.", wrapper1, wrapper2);
		}

		public virtual void TestProviderIsCorrectlySetup()
		{
			AssertType<T>(GlbCompanyWrapperProvider.GetProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		}
	}

	[TestedType(typeof(GlbCompanyWrapperProviderForTesting))]
	class GlbCompanyWrapperProviderBaseOnlyTest : GlbCompanyWrapperProviderTest<GlbCompanyWrapperProviderForTesting>
	{
		public void TestGetProvider()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda + "DESC");
			Factory.Save();
			var za = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, parent: za);
			Factory.Save();
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, Core.Constants.CountryCodes.Botswana, Core.Constants.CountryCodes.Botswana, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var usProvider = GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.UnitedStates);
				AssertEquals(true, typeof(Integration.Customs.US.IUSGlbCompanyWrapperProvider).IsAssignableFrom(usProvider.GetType()));
				AssertEquals(usProvider.GetType(), GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.UnitedStates).GetType());
				AssertNotNull(GlbCompanyWrapperProvider.GetProvider("!@").GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Botswana))
			{
				var acProvider = GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Botswana);
				AssertEquals(true, typeof(Integration.Customs.AsycudaCustoms.IAsycudaGlbCompanyWrapperProvider).IsAssignableFrom(acProvider.GetType()));
				AssertEquals(acProvider.GetType(), GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Botswana).GetType());
			}
		}

		public override void TestProviderIsCorrectlySetup()
		{
			var companyWrapperProvidersHash = new Hashtable();
			companyWrapperProvidersHash.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new TestObjectHandle(new GlbCompanyWrapperProviderForTesting()));
			var companyWrappersHash = new Hashtable();
			companyWrappersHash.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new TestObjectHandle(new GlbCompanyWrapperForTesting(GlbCompany.CurrentCompany)));
			using (ObjectFactory.Substitute("GlbCompanyWrappers", companyWrappersHash))
			using (ObjectFactory.Substitute("GlbCompanyWrapperProviders", companyWrapperProvidersHash))
			{
				base.TestProviderIsCorrectlySetup();
			}
		}
	}

	class GlbCompanyWrapperProviderForTesting : GlbCompanyWrapperProvider
	{
	}
}
