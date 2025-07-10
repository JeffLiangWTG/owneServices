using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.MultiLineAddInfos.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Business.Extensions.Testing
{
	sealed class BusinessObjectExtensionsTest : TestCaseWithFactory
	{
		public void TestMostRecentLogByEventTimeExcludingEstimated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var logs = declaration.Logs;
			AssertNull(logs.MostRecentLogByEventTimeExcludingEstimated(null));
			AssertNull(logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised));
			var log1 = declaration.Logs.AddNew(Events.Authorised, new ZDateTimeOffset(2013, 6, 1), true);
			AssertNull(logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised));
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_IsEstimate = false;
			}
			AssertEquals(log1, logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised));
			var log2 = declaration.Logs.AddNew(Events.Authorised, new ZDateTimeOffset(2013, 6, 2), false);
			AssertEquals(log2, logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised));
			log2.Cancel();
			AssertEquals(log1, logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised));
		}

		public void TestDeleteAllCusAddInfoChildrenIfSupportedHandleUnknownType()
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			var declaration = Factory.New<UnknownCusAddInfoTest.JobDeclarationWithCusAddInfoTypeSupporter>();
			var info1 = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			info1.B7_ParentID = declaration.PK;
			info1.B7_ParentTableCode = declaration.TablePrefix;
			info1.Data.UZ_String = "Fred";
			var info2 = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			info2.B7_Type = "#@$";
			info2.B7_ParentID = declaration.PK;
			info2.B7_ParentTableCode = declaration.TablePrefix;
			info2.Data.UZ_String = "Fred";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var decInDiffFactory = newFactory.Load<UnknownCusAddInfoTest.JobDeclarationWithCusAddInfoTypeSupporter>(declaration.PK);
			decInDiffFactory.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			AssertContains("does not support B7_Type '#@$'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			using (newFactory.EnableUnknown<CusAddInfo>())
			{
				var infos = newFactory.Load<CusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, declaration.PK));
				AssertEquals("infos", 0, infos.Length);
			}
		}

		public void TestDeleteAllCusCodeDataChildrenIfSupportedHandleUnknownType()
		{
			var declaration = Factory.New<UnknownCusCodeDataTest.JobDeclarationWithCusCodeDataTypeSupporter>();
			var info1 = Factory.New<DummyCusCodeData>();
			info1.CY_ParentID = declaration.PK;
			info1.CY_ParentTableCode = declaration.TablePrefix;
			info1.CY_Data = "Fred";
			var info2 = Factory.New<DummyCusCodeData>();
			info2.CY_Type = "#@$";
			info2.CY_ParentID = declaration.PK;
			info2.CY_ParentTableCode = declaration.TablePrefix;
			info2.CY_Data = "Fred";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var decInDiffFactory = newFactory.Load<UnknownCusCodeDataTest.JobDeclarationWithCusCodeDataTypeSupporter>(declaration.PK);
			decInDiffFactory.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			AssertContains("ICusCodeDataTypeSupporter or is missing a support for CY_Type '#@$'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			using (newFactory.EnableUnknown<CusCodeData>())
			{
				var infos = newFactory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, declaration.PK));
				AssertEquals("infos", 0, infos.Length);
			}
		}

		public void TestDeleteAllCusSupportingInfoChildrenIfSupportedHandleUnknownType()
		{
			var declaration = Factory.New<JobDeclarationWithCusSupportingInfoTypeSupporter>();
			var info1 = Factory.New<CusSupportingInfo>();
			info1.CSI_Type = "COO";
			info1.CSI_ParentID = declaration.PK;
			info1.CSI_ParentTableCode = declaration.TablePrefix;
			info1.CSI_ReferenceNumber = "Fred";
			var info2 = Factory.New<CusSupportingInfo>();
			info2.CSI_Type = "PQD";
			info2.CSI_ParentID = declaration.PK;
			info2.CSI_ParentTableCode = declaration.TablePrefix;
			info2.CSI_ReferenceNumber = "Fred";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var decInDiffFactory = newFactory.Load<JobDeclarationWithCusSupportingInfoTypeSupporter>(declaration.PK);
			decInDiffFactory.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			AssertContains("has not implement Enterprise.Integration.Customs.ICusSupportingInfoTypeSupporter or is missing a support for CSI_Type 'PQD'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			using (newFactory.EnableUnknown<CusSupportingInfo>())
			{
				var infos = newFactory.Load<CusSupportingInfo>(new ZQuery(CusSupportingInfoSchema.CSI_ParentID, declaration.PK));
				AssertEquals("infos", 0, infos.Length);
			}
		}

		public void TestMostRecentLogByEventTimeExcludingEstimated_Reference()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var logs = declaration.Logs;
			AssertNull(logs.MostRecentLogByEventTimeExcludingEstimated(null, "HELLO"));
			AssertNull(logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised, "HELLO"));
			var log1 = declaration.Logs.AddNew(Events.Authorised, "HELLO", new ZDateTimeOffset(2013, 6, 1), true);
			var log2 = declaration.Logs.AddNew(Events.Authorised, "BYE", new ZDateTimeOffset(2013, 6, 1), false);
			AssertNull(logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised, "HELLO"));
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_IsEstimate = false;
			}
			AssertEquals(log1, logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised, "HELLO"));
			var log3 = declaration.Logs.AddNew(Events.Authorised, "HELLO", new ZDateTimeOffset(2013, 6, 2), false);
			AssertEquals(log3, logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised, "HELLO"));
			log3.Cancel();
			AssertEquals(log1, logs.MostRecentLogByEventTimeExcludingEstimated(Events.Authorised, "HELLO"));
		}

		public void TestGetEuropeanUnionForCustomsMembers()
		{
			var expectedMemebers = new[] { Core.Constants.CountryCodes.Austria, Core.Constants.CountryCodes.Belgium, Core.Constants.CountryCodes.Bulgaria, Core.Constants.CountryCodes.Croatia, Core.Constants.CountryCodes.Cyprus,
				Core.Constants.CountryCodes.CzechRepublic, Core.Constants.CountryCodes.Denmark, Core.Constants.CountryCodes.Estonia, Core.Constants.CountryCodes.Finland, Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Germany,
				Core.Constants.CountryCodes.Greece, Core.Constants.CountryCodes.Hungary, Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Latvia, Core.Constants.CountryCodes.Lithuania,
				Core.Constants.CountryCodes.Luxembourg, Core.Constants.CountryCodes.Malta, Core.Constants.CountryCodes.Netherlands, Core.Constants.CountryCodes.Poland, Core.Constants.CountryCodes.Portugal, Core.Constants.CountryCodes.Romania,
				Core.Constants.CountryCodes.Slovakia, Core.Constants.CountryCodes.Slovenia, Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.Sweden };

			AssertContainsExactElementsInAnyOrder(expectedMemebers, Factory.GetEuropeanUnionForCustomsMembers());
		}

		[TestDate(2019, 3, 17)]
		public void TestGetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers_NotFromZZRefDb()
		{
			CombineAssertions(() =>
			{
				var list = new List<string>();
				list.AddRange(Factory.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers());

				AssertEquals("Not ZZ Count", 28, list.Count);
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Austria, true, list.Contains(Core.Constants.CountryCodes.Austria));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Belgium, true, list.Contains(Core.Constants.CountryCodes.Belgium));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Bulgaria, true, list.Contains(Core.Constants.CountryCodes.Bulgaria));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Croatia, true, list.Contains(Core.Constants.CountryCodes.Croatia));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Cyprus, true, list.Contains(Core.Constants.CountryCodes.Cyprus));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.CzechRepublic, true, list.Contains(Core.Constants.CountryCodes.CzechRepublic));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Denmark, true, list.Contains(Core.Constants.CountryCodes.Denmark));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Estonia, true, list.Contains(Core.Constants.CountryCodes.Estonia));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Finland, true, list.Contains(Core.Constants.CountryCodes.Finland));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.France, true, list.Contains(Core.Constants.CountryCodes.France));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Germany, true, list.Contains(Core.Constants.CountryCodes.Germany));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Greece, true, list.Contains(Core.Constants.CountryCodes.Greece));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Hungary, true, list.Contains(Core.Constants.CountryCodes.Hungary));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Ireland, true, list.Contains(Core.Constants.CountryCodes.Ireland));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Italy, true, list.Contains(Core.Constants.CountryCodes.Italy));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Latvia, true, list.Contains(Core.Constants.CountryCodes.Latvia));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Lithuania, true, list.Contains(Core.Constants.CountryCodes.Lithuania));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Luxembourg, true, list.Contains(Core.Constants.CountryCodes.Luxembourg));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Malta, true, list.Contains(Core.Constants.CountryCodes.Malta));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Netherlands, true, list.Contains(Core.Constants.CountryCodes.Netherlands));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Norway, false, list.Contains(Core.Constants.CountryCodes.Norway));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Poland, true, list.Contains(Core.Constants.CountryCodes.Poland));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Portugal, true, list.Contains(Core.Constants.CountryCodes.Portugal));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Romania, true, list.Contains(Core.Constants.CountryCodes.Romania));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Slovakia, true, list.Contains(Core.Constants.CountryCodes.Slovakia));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Slovenia, true, list.Contains(Core.Constants.CountryCodes.Slovenia));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Spain, true, list.Contains(Core.Constants.CountryCodes.Spain));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.Sweden, true, list.Contains(Core.Constants.CountryCodes.Sweden));
				AssertEquals("Not ZZ, " + Core.Constants.CountryCodes.UnitedKingdom, true, list.Contains(Core.Constants.CountryCodes.UnitedKingdom));
			});
		}

		[TestDate(2019, 3, 17)]
		public void TestGetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers_FromZZRefDb()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupCUAM = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Andorra, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Andorra, Core.Constants.CountryCodes.UnitedKingdom }, factory.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers());
		}

		[TestDate(2019, 3, 17)]
		public void TestIsInEuropeanCustomsUnion()
		{
			CombineAssertions(() =>
			{
				Assert("GR (not ZZ)", Factory.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Greece));

				var factory = new BusinessObjectFactory();
				var helper = new UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
				var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
				helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
				helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
				var tradeGroupCUAM = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
				helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Switzerland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
				helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Andorra, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

				Assert("GR", !factory.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Greece));
				Assert("IT", factory.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Italy));
				Assert("UK", factory.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.UnitedKingdom));
				Assert("AD", factory.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Andorra));
				Assert("DE", !factory.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Germany));
				Assert("MC", !factory.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Monaco));
			});
		}

		[TestDate(2019, 3, 17)]
		public void TestIsCountryEuOrCtCountry_NoZZData()
		{
			CombineAssertions("Use default list", () =>
			{
				AssertEquals("Greece", true, Factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Greece));
				AssertEquals("Switzerland", true, Factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Switzerland));
				AssertEquals("Norway", false, Factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Norway));
				AssertEquals("Turkey", false, Factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Turkey));
			});
		}

		[TestDate(2019, 3, 17)]
		public void TestIsCountryEuOrCtCountry()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupCUAM = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Iceland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupEUCTP = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EUCommonTransitProcedure, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupEUCTP, Core.Constants.CountryCodes.Turkey, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			CombineAssertions("Use ZZ list", () =>
			{
				AssertEquals("Greece", false, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Greece));
				AssertEquals("Italy in EUC", true, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Italy));
				AssertEquals("United Kingdom in EUC", true, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.UnitedKingdom));
				AssertEquals("Iceland in CUAM", true, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Iceland));
				AssertEquals("Turkey in EUCTP", true, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Turkey));
				AssertEquals("Switzerland", false, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Switzerland));
				AssertEquals("Germany no ZZ", false, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Germany));
				AssertEquals("Monaco", false, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Monaco));
				AssertEquals("Norway", false, factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Norway));
			});
		}

		public void TestIsCountryConsideredInEuForSafetyAndSecurity_NoZZData()
		{
			CombineAssertions("Use default list", () =>
			{
				AssertEquals("Greece", true, Factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Greece));
				AssertEquals("Switzerland", true, Factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Switzerland));
				AssertEquals("Norway", false, Factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Norway));
				AssertEquals("Turkey", false, Factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Turkey));
			});
		}

		[TestDate(2019, 3, 17)]
		public void TestIsCountryConsideredInEuForSafetyAndSecurity()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupCUAM = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Iceland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupEUSEC = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EUForSafetyAndSecurity, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupEUSEC, Core.Constants.CountryCodes.Turkey, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			CombineAssertions("Use ZZ list", () =>
			{
				AssertEquals("Greece", false, factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Greece));
				AssertEquals("Italy in EUC", true, factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Italy));
				AssertEquals("United Kingdom in EUC", true, factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.UnitedKingdom));
				AssertEquals("Iceland in CUAM", true, factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Iceland));
				AssertEquals("Turkey in EUSEC", true, factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Turkey));
				AssertEquals("Switzerland", false, factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Switzerland));
				AssertEquals("Norway", false, factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Norway));
				AssertEquals("Monaco", false, factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Monaco));
			});
		}

		[TestDate(2020, 12, 31)]
		public void TestCommonTransitButNotEUCountries()
		{
			var list = new List<string>();
			list.AddRange(Factory.CommonTransitButNotEUCountries());
			Assert("Use default list: CH", list.Contains(Core.Constants.CountryCodes.Switzerland));

			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			var tradeGroup1 = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroup2 = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EUCommonTransitProcedure, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Vanuatu, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31)); // still fresh
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.SierraLeone, new ZDate(2019, 1, 1), new ZDate(2020, 06, 30)); // expired
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			factory.Save();

			list.Clear();
			list.AddRange(factory.CommonTransitButNotEUCountries());

			Assert("Use ZZ list: CN", !list.Contains(Core.Constants.CountryCodes.China));
			Assert("Use ZZ list: IT", !list.Contains(Core.Constants.CountryCodes.Italy));
			Assert("Use ZZ list: CH", !list.Contains(Core.Constants.CountryCodes.Switzerland));
			Assert("Use ZZ list: VU", list.Contains(Core.Constants.CountryCodes.Vanuatu));
			Assert("Use ZZ list: SL", !list.Contains(Core.Constants.CountryCodes.SierraLeone));
		}

		[TestDate(2019, 3, 17)]
		public void TestIsMemberOfEU()
		{
			Assert(!Factory.IsMemberOfEU(Core.Constants.CountryCodes.Monaco));
			Assert(Factory.IsMemberOfEU(Core.Constants.CountryCodes.Italy));
			Assert(!Factory.IsMemberOfEU(Core.Constants.CountryCodes.UnitedKingdom));
			Assert(!Factory.IsMemberOfEU(Core.Constants.CountryCodes.Switzerland));
			Assert(Factory.IsMemberOfEU(Core.Constants.CountryCodes.Germany));

			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Monaco, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Vanuatu, new ZDate(2019, 1, 1), new ZDate(2019, 1, 31));  // expired
			var tradeGroupCUAM = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Switzerland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			Assert(factory.IsMemberOfEU(Core.Constants.CountryCodes.Monaco));
			Assert(factory.IsMemberOfEU(Core.Constants.CountryCodes.Italy));
			Assert(factory.IsMemberOfEU(Core.Constants.CountryCodes.UnitedKingdom));
			Assert(!factory.IsMemberOfEU(Core.Constants.CountryCodes.Switzerland));
			Assert(!factory.IsMemberOfEU(Core.Constants.CountryCodes.Germany));
			Assert(!factory.IsMemberOfEU(Core.Constants.CountryCodes.Vanuatu));
		}

		public void TestIsMemberOfICS2()
		{
			Assert(Factory.IsMemberOfICS2(Core.Constants.CountryCodes.Italy));
			Assert(Factory.IsMemberOfICS2(Core.Constants.CountryCodes.Germany));
			Assert(!Factory.IsMemberOfICS2(Core.Constants.CountryCodes.Monaco));
			Assert(!Factory.IsMemberOfICS2(Core.Constants.CountryCodes.UnitedKingdom));
			Assert(Factory.IsMemberOfICS2(Core.Constants.CountryCodes.Switzerland));
			Assert(Factory.IsMemberOfICS2(Core.Constants.CountryCodes.Norway));
		}

		public void TestGetEuropeanUnionAndCtCountries_NoZZData()
		{
			AssertCollectionContains(Core.Constants.CountryCodes.Switzerland, Factory.GetEuropeanUnionAndCtCountries());
		}

		[TestDate(2019, 3, 17)]
		public void TestGetEuropeanUnionAndCtCountries()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupCUAM = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Iceland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupCTP = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EUCommonTransitProcedure, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCTP, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Iceland, Core.Constants.CountryCodes.UnitedKingdom }, Factory.GetEuropeanUnionAndCtCountries());
		}

		public void TestEuropeanUnionCountryList()
		{
			var factory = new BusinessObjectFactory();
			var europeanCountryList = factory.GetEuropeanUnionCountryList();

			AssertEquals("All EU Countries and GB", "AT, BE, BG, CY, CZ, DE, DK, EE, EL, ES, FI, FR, GB, HR, HU, IE, IT, LT, LU, LV, MT, NL, PL, PT, RO, SE, SI, SK", europeanCountryList.CodesAsString);
		}

		[TestDate(2019, 3, 17)]
		public void TestGetEuropeanUnionForSafetyAndSecurityCountries()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EUForSafetyAndSecurity, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Switzerland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2019, 1, 31));
			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Switzerland }, Factory.GetEuropeanUnionForSafetyAndSecurityCountries());
		}

		[TestDate(2019, 3, 17)]
		public void TestIsEuropeanUnionForSafetyAndSecurityCountry()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EUForSafetyAndSecurity, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Switzerland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2019, 1, 31));
			AssertEquals("IT", true, Factory.IsEuropeanUnionForSafetyAndSecurityCountry(Core.Constants.CountryCodes.Italy));
			AssertEquals("CH", true, Factory.IsEuropeanUnionForSafetyAndSecurityCountry(Core.Constants.CountryCodes.Switzerland));
			AssertEquals("GB", false, Factory.IsEuropeanUnionForSafetyAndSecurityCountry(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("AU", false, Factory.IsEuropeanUnionForSafetyAndSecurityCountry(Core.Constants.CountryCodes.Australia));
		}

		class JobDeclarationWithCusSupportingInfoTypeSupporter : BaseJobDeclaration, Integration.Customs.ICusSupportingInfoTypeSupporter
		{
			public JobDeclarationWithCusSupportingInfoTypeSupporter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
			{
				yield return new FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
			}

			IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
			{
				var result = new Dictionary<ZString, Type>();
				result.Add("COO", typeof(CusSupportingInfo));
				return result;
			}
		}
	}
}
