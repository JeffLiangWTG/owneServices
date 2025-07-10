using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class CusPersonCountryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDataAndValueLookups()
		{
			var cpc = Factory.New<CusPersonCountry>();
			cpc.CPC_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			var dataTypes = cpc.Lookups.DataTypes;
			AssertEquals(false, object.ReferenceEquals(Factory.GetCachedValue<ZaDataTypes>(), dataTypes));
			cpc.CPC_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			dataTypes = cpc.Lookups.DataTypes;
			AssertEquals(true, object.ReferenceEquals(Factory.GetCachedValue<ZaDataTypes>(), dataTypes));
			AssertEquals("OCC, RSN, TDT, TRV", dataTypes.CodesAsString);
			cpc.CPC_Type = ZaDataTypes.Codes.Occupation;
			var dataValues1 = cpc.Lookups.DataValues;
			var dataValues2 = cpc.Lookups.DataValues;
			AssertEquals(true, object.ReferenceEquals(dataValues2, dataValues1));
			AssertEquals("H, G", dataValues1.CodesAsString);
			cpc.CPC_Type = ZaDataTypes.Codes.ReasonForMovement;
			dataValues1 = cpc.Lookups.DataValues;
			AssertEquals("type was changed", false, object.ReferenceEquals(dataValues2, dataValues1));
			dataValues2 = cpc.Lookups.DataValues;
			AssertEquals(true, object.ReferenceEquals(dataValues2, dataValues1));
			AssertEquals("E, D", dataValues1.CodesAsString);
			cpc.CPC_Type = ZaDataTypes.Codes.TravellerType;
			dataValues1 = cpc.Lookups.DataValues;
			AssertEquals("type was changed", false, object.ReferenceEquals(dataValues2, dataValues1));
			dataValues2 = cpc.Lookups.DataValues;
			AssertEquals(true, object.ReferenceEquals(dataValues2, dataValues1));
			AssertEquals("VBK, VBV, SVR, VVP", dataValues1.CodesAsString);
			cpc.CPC_Type = ZaDataTypes.Codes.TravelDocumentType;
			var dataValues3 = cpc.Lookups.DataValues;
			AssertEquals("type was changed", false, object.ReferenceEquals(dataValues2, dataValues3));
			dataValues2 = cpc.Lookups.DataValues;
			AssertEquals(true, object.ReferenceEquals(dataValues2, dataValues3));
			AssertEquals("P, L, M, F, A, N, D, E", dataValues3.CodesAsString);
			cpc.CPC_Type = ZaDataTypes.Codes.TravellerType;
			dataValues3 = cpc.Lookups.DataValues;
			AssertEquals(true, object.ReferenceEquals(dataValues1, dataValues3));
		}
	}
}
