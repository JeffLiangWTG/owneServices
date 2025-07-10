using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CountrySpecificWtihEUTypeDeciderTest : ZArchitecture.Business.Testing.CountrySpecificTypeDeciderTest
	{
		protected CountrySpecificWtihEUTypeDeciderTest() { }

		public void TestEUTypeDeciderIsSetupCorrectly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				AssertNotNull("GetTestCountryPKsAndExpectedTypes should be overriden to return country codes and their decided types", TestCountryPKsAndExpectedTypesForLoad);
				Assert("GetTestCountryPKsAndExpectedTypes should be overriden to return at least one country code and expected types", TestCountryPKsAndExpectedTypesForLoad.Count > 0);
				var newBizO = GetNewBusinessObjectForLoadTest();
				foreach (var countryPK in TestCountryPKsAndExpectedTypesForLoad.Keys)
				{
					var country = Factory.Load<RefCountry>(countryPK);
					if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(country.RN_Code))
					{
						SetBizOCountryForLoadTestCore(newBizO, country.RN_Code);
						Factory.Save();

						var loadedBizO = new BusinessObjectFactory().Load(EUType, newBizO.PK);
						AssertNotNull("Loaded BizO should not be null", loadedBizO);
						AssertEquals(ZString.Format("DecidedType for {0} differs from expected", countryPK), TestCountryPKsAndExpectedTypesForLoad[countryPK], loadedBizO.GetType());
						loadedBizO = null;
					}
				}
			}
		}

		protected abstract Type EUType { get; }
	}
}
