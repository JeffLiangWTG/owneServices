using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSeaManOBLHeaderTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			SetCountryCode(countryCode);
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var tranHead = Factory.New<CusSeaManTranHead>();
			return tranHead.OceanBills.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(CusSeaManOBLHeader);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusSeaManOBLHeader>() }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusSeaManOBLHeader>() }
			};
		}

		#endregion
	}
}
