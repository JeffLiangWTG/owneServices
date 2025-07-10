using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UniversalPlaceOfSupplyHelperTest : TestCaseWithFactory
	{
		public void TestGetUniversalPlaceOfSupply()
		{
			var placeOfSupplyHelper = new UniversalPlaceOfSupplyHelper(GlbCompany.CurrentCompany);

			Assert(!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
			AssertNull(placeOfSupplyHelper.GetPlaceOfSupply("", ""));
			AssertNull(placeOfSupplyHelper.GetPlaceOfSupply("NSW", ""));
			AssertNull(placeOfSupplyHelper.GetPlaceOfSupply("NSW", PlaceOfSupplyTypes.State.Code));
			AssertNull(placeOfSupplyHelper.GetPlaceOfSupply("", PlaceOfSupplyTypes.State.Code));

			var newFactory = new BusinessObjectFactory();
			var indianCompany = newFactory.NewWithValidTestData<GlbCompany>();
			indianCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.India;
			var indianBranch = newFactory.NewWithValidTestData<GlbBranch>();
			indianBranch.GB_GC = indianCompany.PK;
			newFactory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, indianBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				using (AssertDbHitsForAllFactories("Expected Db Hits", new Dictionary<string, int>
					{
						{ GlbCompanySchema.Constants.TableName, 1 },
						{ RefCountryStatesSchema.Constants.TableName, 1 }
					}, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true, tablesToIgnore: new List<string>() { RefLanguageTextSchema.Constants.TableName })) // Ignore table which gives us 36 multilingual strings
				{
					placeOfSupplyHelper = new UniversalPlaceOfSupplyHelper(GlbCompany.CurrentCompany);

					Assert(PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
					AssertNull("empty FPOS should return null", placeOfSupplyHelper.GetPlaceOfSupply("", PlaceOfSupplyTypes.State.Code));
					AssertNull("empty FPOS type should return null", placeOfSupplyHelper.GetPlaceOfSupply("JH", ""));
					AssertNull("wrong FPOS type should return null", placeOfSupplyHelper.GetPlaceOfSupply("JH", "XXX"));
					AssertNull("wrong FPOS should return null", placeOfSupplyHelper.GetPlaceOfSupply("XX", PlaceOfSupplyTypes.State.Code));

					AssertPlaceOfSupply(placeOfSupplyHelper.GetPlaceOfSupply("DL", PlaceOfSupplyTypes.State.Code),
						"DL", "Delhi", PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.State.Description);

					AssertPlaceOfSupply(placeOfSupplyHelper.GetPlaceOfSupply(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, PlaceOfSupplyTypes.PredefinedRule.Code),
						PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, PlaceOfSupplyListProvider.Descriptions.OutsideTheLoginCountry, PlaceOfSupplyTypes.PredefinedRule.Code, PlaceOfSupplyTypes.PredefinedRule.Description);
				}

				placeOfSupplyHelper = new UniversalPlaceOfSupplyHelper(GlbCompany.CurrentCompany.GC_Code, newFactory);
				AssertPlaceOfSupply(placeOfSupplyHelper.GetPlaceOfSupply("DL", PlaceOfSupplyTypes.State.Code),
						"DL", "Delhi", PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.State.Description);

				placeOfSupplyHelper = new UniversalPlaceOfSupplyHelper(GlbCompany.CurrentCompany.GC_Code, null);
				AssertPlaceOfSupply(placeOfSupplyHelper.GetPlaceOfSupply("DL", PlaceOfSupplyTypes.State.Code),
						"DL", "Delhi", PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.State.Description);
			}
		}

		void AssertPlaceOfSupply(PlaceOfSupply placeOfSupply, string expectedLocationCode, string expectedLocationDesc, string expectedLocationTypeCode, string expectedLocationTypeDesc)
		{
			AssertEquals("Location.Code", expectedLocationCode, placeOfSupply.Location.Code);
			AssertEquals("Location.Description", expectedLocationDesc, placeOfSupply.Location.Description);
			AssertEquals("LocationType.Code", expectedLocationTypeCode, placeOfSupply.LocationType.Code);
			AssertEquals("LocationType.Description", expectedLocationTypeDesc, placeOfSupply.LocationType.Description);
		}
	}
}
