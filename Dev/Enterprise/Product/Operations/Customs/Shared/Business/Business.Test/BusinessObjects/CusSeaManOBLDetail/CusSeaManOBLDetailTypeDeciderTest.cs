using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSeaManOBLDetailTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			SetCountryCode(countryCode);
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var tranHead = Factory.New<CusSeaManTranHead>();
			var cusSeaManOBLHeader = tranHead.OceanBills.AddNew();
			return cusSeaManOBLHeader.Details.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(CusSeaManOBLDetail);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusSeaManOBLDetail>() }
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
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusSeaManOBLDetail>() }
			};
		}

		#endregion
	}
}
