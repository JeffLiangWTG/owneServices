using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCountryDataTypeDeciderTest : TestCaseWithFactory
	{
		#region GetTypeForNew

		public void TestGetTypeForNew()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				OrgCountryData auCountryData = Factory.New<OrgCountryData>();
				AssertEquals(typeof(OrgCountryDataAU), auCountryData.GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				OrgCountryData jpCountryData = Factory.New<OrgCountryData>();
				AssertEquals(typeof(OrgCountryDataJP), jpCountryData.GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				OrgCountryData hkCountryData = Factory.New<OrgCountryData>();
				AssertEquals(typeof(OrgCountryDataHK), hkCountryData.GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Norway))
			{
				OrgCountryData euCountryData = Factory.New<OrgCountryData>();
				AssertEquals(typeof(OrgCountryDataEU), euCountryData.GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Jamaica))
			{
				OrgCountryData countryData = Factory.New<OrgCountryData>();
				AssertEquals(typeof(OrgCountryData), countryData.GetType());
			}
		}

		#endregion

		#region GetTypeForLoad

		public void TestGetTypeForLoad()
		{
			OrgCountryData countryData1 = Factory.New<OrgCountryData>();
			countryData1.OV_RN_NKClientCountryRelation = Constants.CountryCodes.Japan;
			countryData1.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			OrgCountryData countryData2 = Factory.New<OrgCountryData>();
			countryData2.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
			countryData2.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			OrgCountryData countryData3 = Factory.New<OrgCountryData>();
			countryData3.OV_RN_NKClientCountryRelation = Constants.CountryCodes.EuropeanUnion;
			countryData3.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			Factory.Save();

			OrgCountryDataTypeDecider typeDecider = new OrgCountryDataTypeDecider();
			DataRow row = ((INeedRow)countryData1).Row;
			AssertEquals(typeof(OrgCountryDataJP), typeDecider.GetTypeForLoad(row, Factory));

			row = ((INeedRow)countryData2).Row;
			AssertEquals(typeof(OrgCountryData), typeDecider.GetTypeForLoad(row, Factory));

			row = ((INeedRow)countryData3).Row;
			AssertEquals(typeof(OrgCountryDataEU), typeDecider.GetTypeForLoad(row, Factory));
		}

		#endregion

		#region GetTypeForBinding

		public void TestGetTypeForBinding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				Type jpCountryDataType = new OrgCountryDataTypeDecider().GetTypeForBinding();
				AssertEquals(typeof(OrgCountryDataJP), jpCountryDataType);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Jamaica))
			{
				Type countryDataType = new OrgCountryDataTypeDecider().GetTypeForBinding();
				AssertEquals(typeof(OrgCountryData), countryDataType);
			}
		}

		#endregion
	}
}
