using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CreateDeclarationHelperProvider))]
	sealed class CreateDeclarationHelperProviderTest : TestCaseWithFactory
	{
		public void TestNewCreateDeclarationHelper()
		{
			var provider = ObjectFactory.Get<Integration.Customs.Shared.ICreateDeclarationHelperProvider>();
			AssertType<CreateDeclarationHelperProvider>(provider);

			// registered Country
			var auHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.Australia);
			AssertEquals("Enterprise.Customs.AU.Declaration.Business.CreateDeclarationHelper", auHelper.GetType().FullName);

			var caHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.Canada);
			AssertEquals("Enterprise.Customs.CA.Business.CreateDeclarationHelper", caHelper.GetType().FullName);

			var cnHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.China);
			AssertEquals("Enterprise.Customs.CN.Business.CreateDeclarationHelper", cnHelper.GetType().FullName);

			var nzHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.NewZealand);
			AssertEquals("Enterprise.Customs.NZ.Business.CreateDeclarationHelper", nzHelper.GetType().FullName);

			var twHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.Taiwan);
			AssertEquals("Enterprise.Customs.TW.Business.CreateDeclarationHelper", twHelper.GetType().FullName);

			var usHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Enterprise.Customs.US.Business.CreateDeclarationHelper", usHelper.GetType().FullName);

			// GetCustomsCountryOfJurisdictionOrEU
			var frHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.France);
			AssertEquals("Enterprise.Customs.EU.Business.CreateDeclarationHelper", frHelper.GetType().FullName);

			var prHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.PuertoRico);
			AssertEquals("Enterprise.Customs.US.Business.CreateDeclarationHelper", prHelper.GetType().FullName);

			// undefined
			var erHelper = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.Eritrea);
			AssertEquals("Enterprise.Customs.Business.CreateDeclarationHelper", erHelper.GetType().FullName);

			// not singleton
			var erHelper2 = provider.NewCreateDeclarationHelper(Core.Constants.CountryCodes.Eritrea);
			AssertNotSame("CreateDeclarationHelper is not a singleton", erHelper, erHelper2);
		}

		public void TestHelpersCount()
		{
			var providers = ObjectFactory.Get<Hashtable>("CreateDeclarationHelpers");
			AssertEquals("CreateDeclarationHelper entries available through the provider", 8, providers.Keys.OfType<string>().Count(x => !string.IsNullOrWhiteSpace(x)));
		}
	}
}
