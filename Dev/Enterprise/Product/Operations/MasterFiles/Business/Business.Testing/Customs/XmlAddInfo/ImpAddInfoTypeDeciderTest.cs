using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ImpAddInfoTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetCountrySpecificTypeForNewOrgAddInfo_AU()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Australia;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_CA()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Canada;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_CN()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.China;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction()
		{
			foreach (var countryCode in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				countryData.OV_RN_NKClientCountryRelation = countryCode;
				AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
			}
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_DE()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Germany;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_KR()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_NL()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Netherlands;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.NL.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_ES()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Spain;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_IN()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.India;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.IN.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_TW()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Taiwan;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_TR()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Turkey;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_GB()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.GB.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_USAndPR()
		{
			CombineAssertions(() =>
			{
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("United States", ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.PuertoRico;
				AssertEquals("PuertoRico", ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
			});
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_NotSupported()
		{
			CombineAssertions(() =>
			{
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Italy;
				AssertExceptionThrown<NotSupportedException>("Italy", () => new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));

				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Switzerland;
				AssertExceptionThrown<NotSupportedException>("Switzerland", () => new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
			});
		}

		public void TestGetCountrySpecificTypeForNewOrgAddInfo_BR()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Brazil;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetRegionSpecificTypeForNewOrgAddInfo_FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction()
		{
			foreach (var countryCode in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.France;
				AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IRegionOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(countryCode));
			}
		}

		public void TestGetRegionSpecificTypeForNewOrgAddInfo_NL()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Netherlands;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.NL.IRegionOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetRegionSpecificTypeForNewOrgAddInfo_TR()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Turkey;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetRegionSpecificTypeForNewOrgAddInfo_GB()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetRegionSpecificTypeForNewOrgAddInfo_EUMember()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Germany;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetRegionSpecificTypeForNewOrgAddInfo_Unsupported()
		{
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
			AssertExceptionThrown<NotSupportedException>(() => new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(countryData));
		}

		public void TestGetRegionSpecificTypeForNewOrgAddInfo_UKHasLeftEU()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			countryData.OV_OH_OrgHeader = organisation.PK;

			RemoveGbFromEu();

			CombineAssertions(() =>
			{
				AssertEquals("GB not EU Memeber", false, ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.UnitedKingdom));

				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.France;
				AssertEquals("FR still works", ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IRegionOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(countryData));

				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("Should return EU, even though GB is no longer in EU. (As it should still inherit the properties)", ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgImpAddInfo>(), new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(countryData));
			});

			void RemoveGbFromEu()
			{
				var sql = $@"
--RefDataGrouping //Adding EUN and GB
DECLARE @RefDataGroupingParentPK uniqueidentifier = NEWID();
INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (@RefDataGroupingParentPK, 'EUN', 'Europe', NULL)
INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), 'GB', 'United Kingdom', @RefDataGroupingParentPK)

--RefCusTradeGroup //Adding EUC
DECLARE @RefCusTradeGroupParentPK uniqueidentifier = NEWID();
INSERT RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping) VALUES (@RefCusTradeGroupParentPK, 'EUC', 'European Trade Group', '1900-01-01 12:00:00', '2079-06-06 23:59:00', 'EUN')

--RefCusTradeGroupCountry //Adding GB and FR
INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK, 'GB', '1900-01-01 12:00:00', '2019-05-13 00:00:00')
INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK, 'FR', '1900-01-01 12:00:00', '2079-06-06 23:59:00')";

				((CargoWise.Data.IDbConnected)Factory).Connection.ExecuteNonQuery(sql);
				Factory.Save();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			countryData = Factory.New<OrgCountryData>();
		}
		OrgCountryData countryData;
	}
}
