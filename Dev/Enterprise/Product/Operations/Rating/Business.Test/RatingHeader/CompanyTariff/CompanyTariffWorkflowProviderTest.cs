using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	#region CompanyTariffWorkflowProviderTest

	[TestedType(typeof(CompanyTariff))]
	public class CompanyTariffWorkflowProviderTest : WorkflowProviderTest<CompanyTariff, CompanyTariffProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.CompanyTariffsWorkflowDescriptorCode; }
		}

		protected override CompanyTariff GetNewBusinessObject(BusinessObjectFactory factory)
		{
			CompanyTariff result = base.GetNewBusinessObject(factory);

			return result;
		}
	}

	#endregion

	public class CompanyTariffTest : RatingTestCase
	{
		#region Company Tariff Is...

		public void TestCompanyTariffIs()
		{
			var companyTariff = CreateRandomTariff(1);

			AssertEquals(false, companyTariff.IsClientRate());
			AssertEquals(false, companyTariff.IsClientRateHavingSubsidiaryRelations());

			AssertEquals(true, companyTariff.IsTariff());
			AssertEquals("If this is false db is dirty", true, companyTariff.IsLevelOneTariff());
			AssertEquals(false, companyTariff.IsAdditionalTariff());

			AssertEquals(false, companyTariff.IsQuote());

			AssertEquals(false, companyTariff.IsCosting());
			AssertEquals(false, companyTariff.IsWiseCostRate());
			AssertEquals(false, companyTariff.IsStandardCostRate());

			var companyTariff2 = CreateRandomTariff(2);

			AssertEquals(true, companyTariff2.IsTariff());
			AssertEquals(false, companyTariff2.IsLevelOneTariff());
			AssertEquals(true, companyTariff2.IsAdditionalTariff());
		}

		#endregion

		public void TestTariffLevels()
		{
			var baseTariff = Factory.New<CompanyTariff>();
			var tariff2 = Factory.New<CompanyTariff>();
			var tariff3 = Factory.New<CompanyTariff>();

			CombineAssertions("Should increment company tariff levels sequentially", () =>
			{
				AssertEquals((ZByte)1, baseTariff.TH_GlobalRateLevel);
				AssertEquals((ZByte)2, tariff2.TH_GlobalRateLevel);
				AssertEquals((ZByte)3, tariff3.TH_GlobalRateLevel);
			});

			Factory.Save();
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "NEW";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company1.Branches.Add(branch1);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedCompanyTariff = newFactory.Load<CompanyTariff>(tariff2.PK);

				AssertEquals(baseTariff.PK, reloadedCompanyTariff.LevelOneTariff.PK);

				var newTariff = Factory.New<CompanyTariff>();

				AssertNotEquals("Tariffs are no for the same company", baseTariff.TH_GC, newTariff.TH_GC);
				AssertEquals("First tariff in new company should be the base tariff", (ZByte)1, newTariff.TH_GlobalRateLevel);
			}
		}

		public void TestSelectedFilterCategoryUpdatesDiscountType()
		{
			var tariff = CreateRandomTariff(1);
			tariff.SelectedFilterCategory = "SCO";
			AssertEquals("SCO", tariff.DiscountType);
		}

		OrgRateTariffLevel CreateLevel(OrgHeader org, string tariffType, string rateMode, string direction, ZDate startDate, ZDate expiryDate, ZByte tariffLevel)
		{
			var result = org.CompanyData.RateTariffLevels.AddNew();
			result.P7_OH = org.PK;
			result.P7_GC = org.CompanyData.OB_GC;
			result.P7_TariffType = tariffType;
			result.P7_Mode = rateMode;
			result.P7_TariffLevel = tariffLevel;
			result.P7_Direction = direction;
			result.P7_StartDate = startDate;
			result.P7_ExpiryDate = expiryDate;
			return result;
		}

		CompanyTariff CreateRandomTariff(ZByte level)
		{
			var result = Factory.New<CompanyTariff>();
			result.TH_GlobalRateLevel = level;
			result.TH_GlobalRateDescription = "Random Test Tariff " + level;
			return result;
		}

		public void TestALLDirectionsIsPickedUpForNonDirectionApplicableCategories()
		{
			var org = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();

			org.CompanyData.RateTariffLevels.RemoveAndDeleteAll();
			CreateLevel(org, "DEF", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.EXP), new ZDate(2024, 01, 01), new ZDate(2024, 06, 01), 3);
			CreateLevel(org, "DEF", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.EXP), new ZDate(2024, 06, 02), new ZDate(2024, 12, 31), 4);
			CreateLevel(org, "DEF", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.IMP), new ZDate(2024, 01, 01), new ZDate(2024, 06, 01), 1);
			CreateLevel(org, "DEF", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.ALL), new ZDate(2024, 01, 01), new ZDate(2024, 06, 01), 2);

			AssertSecondLevelIsAlwaysReturnedForNonApplicableCategories(RatingConstants.RateCategory.CST, org);
			AssertSecondLevelIsAlwaysReturnedForNonApplicableCategories(RatingConstants.RateCategory.WHS, org);
			AssertSecondLevelIsAlwaysReturnedForNonApplicableCategories(RatingConstants.RateCategory.TRN, org);
			AssertSecondLevelIsAlwaysReturnedForNonApplicableCategories(RatingConstants.RateCategory.TBC, org);

			AssertLevels([2], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, OrgRateTariffLevel.Directions.ALL, new ZDate(2024, 02, 01), new ZDate(2024, 08, 01)));
			AssertLevels([1], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.FTL, OrgRateTariffLevel.Directions.IMP, new ZDate(2024, 02, 01), new ZDate(2024, 08, 01)));
			AssertLevels([], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.FTL, OrgRateTariffLevel.Directions.IMP, new ZDate(2024, 07, 01), new ZDate(2024, 08, 01)));
			AssertLevels([4], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.SOR, Core.Constants.RateMode.FCL, OrgRateTariffLevel.Directions.EXP, new ZDate(2024, 07, 01), new ZDate(2024, 08, 01)));
			AssertLevels([3, 1], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.FCL, Core.Constants.RateMode.RAI, OrgRateTariffLevel.Directions.EXP | OrgRateTariffLevel.Directions.IMP, new ZDate(2024, 03, 01), new ZDate(2024, 04, 01)));
			AssertLevels([1, 3], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.SDE, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.EXP | OrgRateTariffLevel.Directions.IMP | OrgRateTariffLevel.Directions.ALL, new ZDate(2024, 03, 01), new ZDate(2024, 04, 01)));
		}

		void AssertSecondLevelIsAlwaysReturnedForNonApplicableCategories(string rateCategory, OrgHeader org)
		{
			foreach (CodeDescriptionPair pair in RateEntryLookups.GetTransportModesByRateCategory(rateCategory))
			{
				AssertLevels([2], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, rateCategory, pair.Code, OrgRateTariffLevel.Directions.ALL, new ZDate(2024, 03, 01), new ZDate(2024, 04, 01)));
				AssertLevels([2], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, rateCategory, pair.Code, OrgRateTariffLevel.Directions.IMP, new ZDate(2024, 03, 01), new ZDate(2024, 04, 01)));
				AssertLevels([2], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, rateCategory, pair.Code, OrgRateTariffLevel.Directions.EXP, new ZDate(2024, 03, 01), new ZDate(2024, 04, 01)));
				AssertLevels([2], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, rateCategory, pair.Code, OrgRateTariffLevel.Directions.EXP | OrgRateTariffLevel.Directions.IMP, new ZDate(2024, 03, 01), new ZDate(2024, 04, 01)));
				AssertLevels([2], CompanyTariff.GetLevelsForRequestedDirections(org, GlbCompany.CurrentCompany, rateCategory, pair.Code, OrgRateTariffLevel.Directions.EXP | OrgRateTariffLevel.Directions.IMP | OrgRateTariffLevel.Directions.ALL, new ZDate(2024, 03, 01), new ZDate(2024, 04, 01)));
			}
		}

		void AssertLevels(IEnumerable<int> collection1, IEnumerable<int> collection2)
		{
			AssertContainsExactElementsInAnyOrder(collection1, collection2);
		}

		public void TestGetLevel_DateRange()
		{
			var org = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();

			CreateLevel(org, "DEF", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.ALL), new ZDate(2020, 1, 2), new ZDate(2020, 1, 3), 0);
			CreateLevel(org, "DEF", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.IMP), new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), 1);
			CreateLevel(org, "FRT", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.ALL), new ZDate(2020, 1, 2), ZDate.Empty, 2);
			CreateLevel(org, "FRT", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.ALL), new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), 3);
			CreateLevel(org, "FRT", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.IMP), new ZDate(2020, 1, 1), new ZDate(2020, 1, 2), 4);
			CreateLevel(org, "FRT", Core.Constants.RateMode.LSE, nameof(OrgRateTariffLevel.Directions.IMP), ZDate.Empty, ZDate.Empty, 5);

			AssertEquals(0, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ALL, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.ALL, new ZDate(2020, 1, 1), new ZDate(2020, 1, 2)));
			AssertEquals(0, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ALL, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.ALL, new ZDate(2020, 1, 2), new ZDate(2020, 1, 2)));
			AssertEquals(0, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ALL, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.IMP, new ZDate(2020, 1, 2), new ZDate(2020, 1, 2)));

			AssertEquals(1, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ALL, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.IMP, new ZDate(2020, 1, 1), new ZDate(2020, 1, 1)));

			AssertEquals(2, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.ALL, new ZDate(2020, 1, 1), new ZDate(2020, 1, 2)));
			AssertEquals(2, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.IMP, new ZDate(2020, 1, 1), ZDate.Empty));

			AssertEquals(3, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.ALL, ZDate.Empty, new ZDate(2020, 1, 1)));
			AssertEquals(4, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.IMP, ZDate.Empty, new ZDate(2020, 1, 1)));
			AssertEquals(4, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ALL, OrgRateTariffLevel.Directions.IMP, new ZDate(2020, 1, 1), new ZDate(2020, 1, 2)));

			AssertEquals(5, CompanyTariff.GetLevel(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, OrgRateTariffLevel.Directions.IMP, ZDate.Empty, ZDate.Empty));
		}

		public void TestGetRelevantOrgLevels()
		{
			CreateRandomTariff(1);
			CreateRandomTariff(2);
			var org = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var startDate = new ZDate(2020, 1, 1);
			var expiryDate = new ZDate(2020, 1, 2);
			var pastExpiryDate = new ZDate(2020, 1, 3);

			var levels = new Dictionary<string, OrgRateTariffLevel>
			{
				{ "DEF-ALL-ALL-01-02",	CreateLevel(org, "DEF", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.ALL), startDate, expiryDate, 0) },
				{ "DEF-ALL-ALL-03",		CreateLevel(org, "DEF", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.ALL), pastExpiryDate, ZDate.Empty, 0) },
				{ "FRT-ALL-IMP",		CreateLevel(org, "FRT", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.IMP), ZDate.Empty, ZDate.Empty, 1) },
				{ "ORG-ALL-ALL-01-02",	CreateLevel(org, "ORG", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.ALL), startDate, expiryDate, 2) },
				{ "ORG-AIR-ALL",		CreateLevel(org, "ORG", Core.Constants.RateMode.AIR, nameof(OrgRateTariffLevel.Directions.ALL), ZDate.Empty, ZDate.Empty, 2) },
				{ "ORG-LSE-ALL",		CreateLevel(org, "ORG", Core.Constants.RateMode.LSE, nameof(OrgRateTariffLevel.Directions.ALL), ZDate.Empty, ZDate.Empty, 2) },
				{ "ORG-ALL-IMP",		CreateLevel(org, "ORG", Core.Constants.RateMode.ALL, nameof(OrgRateTariffLevel.Directions.IMP), ZDate.Empty, ZDate.Empty, 2) },
				{ "ORG-AIR-IMP-01-02",	CreateLevel(org, "ORG", Core.Constants.RateMode.AIR, nameof(OrgRateTariffLevel.Directions.IMP), startDate, expiryDate, 2) }
			};

			AssertContainsExactElementsInAnyOrder(
				"No Org -- no levels. In any other situation at least default level should be returned.",
				Array.Empty<OrgRateTariffLevel>(),
				CompanyTariff.GetRelevantOrgLevels(null, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "ALL", OrgRateTariffLevel.Directions.ALL, ZDate.Empty, ZDate.Empty)
			);

			AssertContainsExactElementsInAnyOrder(
				"GetRelevantOrgLevels for ORG, ALL, ALL between any dates",
				[levels["DEF-ALL-ALL-01-02"], levels["DEF-ALL-ALL-03"], levels["ORG-ALL-ALL-01-02"]],
				CompanyTariff.GetRelevantOrgLevels(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "ALL", OrgRateTariffLevel.Directions.ALL, ZDate.Empty, ZDate.Empty)
			);

			AssertContainsExactElementsInAnyOrder(
				"GetRelevantOrgLevels for ORG, ALL, ALL between 2020-1-3 and 2020-1-3",
				[levels["DEF-ALL-ALL-03"]],
				CompanyTariff.GetRelevantOrgLevels(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "ALL", OrgRateTariffLevel.Directions.ALL, pastExpiryDate, pastExpiryDate)
			);

			AssertContainsExactElementsInAnyOrder(
				"GetRelevantOrgLevels for ORG, AIR, ALL between any dates",
				[levels["DEF-ALL-ALL-01-02"], levels["DEF-ALL-ALL-03"], levels["ORG-ALL-ALL-01-02"], levels["ORG-AIR-ALL"]],
				CompanyTariff.GetRelevantOrgLevels(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "AIR", OrgRateTariffLevel.Directions.ALL, ZDate.Empty, ZDate.Empty)
			);

			AssertContainsExactElementsInAnyOrder(
				"GetRelevantOrgLevels for ORG, LSE, ALL between any dates",
				[levels["DEF-ALL-ALL-01-02"], levels["DEF-ALL-ALL-03"], levels["ORG-ALL-ALL-01-02"], levels["ORG-AIR-ALL"], levels["ORG-LSE-ALL"]],
				CompanyTariff.GetRelevantOrgLevels(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "LSE", OrgRateTariffLevel.Directions.ALL, ZDate.Empty, ZDate.Empty)
			);

			AssertContainsExactElementsInAnyOrder(
				"GetRelevantOrgLevels for ORG, LSE, IMP between 2020-1-1 and 2020-1-2",
				[levels["DEF-ALL-ALL-01-02"], levels["ORG-ALL-ALL-01-02"], levels["ORG-AIR-ALL"], levels["ORG-LSE-ALL"], levels["ORG-ALL-IMP"], levels["ORG-AIR-IMP-01-02"]],
				CompanyTariff.GetRelevantOrgLevels(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "LSE", OrgRateTariffLevel.Directions.IMP, startDate, expiryDate)
			);

			AssertContainsExactElementsInAnyOrder(
				"GetRelevantOrgLevels for ORG, LSE, IMP between 2020-1-3 and 2020-1-3",
				[levels["DEF-ALL-ALL-03"], levels["ORG-AIR-ALL"], levels["ORG-LSE-ALL"], levels["ORG-ALL-IMP"]],
				CompanyTariff.GetRelevantOrgLevels(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "LSE", OrgRateTariffLevel.Directions.IMP, pastExpiryDate, pastExpiryDate)
			);

			AssertContainsExactElementsInAnyOrder(
				"No Company -- should then use Current Company",
				[levels["DEF-ALL-ALL-01-02"], levels["DEF-ALL-ALL-03"], levels["ORG-ALL-ALL-01-02"]],
				CompanyTariff.GetRelevantOrgLevels(org, null, RatingConstants.RateCategory.ORG, "ALL", OrgRateTariffLevel.Directions.ALL, ZDate.Empty, ZDate.Empty)
			);

			AssertContainsExactElementsInAnyOrder(
				"Wrong Company -- no levels should be returned",
				Array.Empty<OrgRateTariffLevel>(),
				CompanyTariff.GetRelevantOrgLevels(org, Factory.NewWithValidTestData<GlbCompany>(), RatingConstants.RateCategory.ORG, "ALL", OrgRateTariffLevel.Directions.ALL, ZDate.Empty, ZDate.Empty)
			);

			AssertContainsExactElementsInAnyOrder(
				"Wrong Org -- no levels should be returned",
				Array.Empty<OrgRateTariffLevel>(),
				CompanyTariff.GetRelevantOrgLevels(new BusinessObjectFactory().NewWithValidTestData<OrgHeader>(), GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "ALL", OrgRateTariffLevel.Directions.ALL, ZDate.Empty, ZDate.Empty)
			);

			AssertContainsExactElementsInAnyOrder(
				"Empty rate mode -- should use defaults for tariff type and direction but still filter within date range",
				[levels["DEF-ALL-ALL-01-02"]],
				CompanyTariff.GetRelevantOrgLevels(org, GlbCompany.CurrentCompany, RatingConstants.RateCategory.ORG, "", OrgRateTariffLevel.Directions.EXP, startDate, expiryDate)
			);
		}

		public void TestNextTariffLevelDBHits()
		{
			var companyTariff = Factory.GetNull(typeof(CompanyTariff));
			AssertTableHitCount(0, RatingHeader.Schema.TableName, companyTariff.Factory);

			companyTariff = Factory.New<CompanyTariff>();
			AssertTableHitCount(1, RatingHeader.Schema.TableName, companyTariff.Factory);
		}

		public void TestDiscountType()
		{
			var tariff = Factory.New<CompanyTariff>();
			var rateCategories = RatingConstants.RateCategory.RateCategories;

			for (var i = 0; i < rateCategories.Count; i++)
			{
				tariff.DiscountType = rateCategories[i];
				tariff.Discount = i + 5m;
			}

			for (var i = 0; i < rateCategories.Count; i++)
			{
				tariff.DiscountType = rateCategories[i];
				AssertEquals(i + 5m, tariff.Discount);
			}

			AssertEquals(rateCategories.Count, tariff.DiscountTypes.Count);
			for (var i = 0; i < rateCategories.Count; i++)
			{
				AssertNotNull(tariff.CompanyTariffCodes.GetCompanyTariffDiscountDescription(rateCategories[i]));
			}
		}

		public void TestGetDescriptionFromCode()
		{
			var tariff = Factory.New<CompanyTariff>() as ICompanyTariff;
			AssertEquals(16, tariff.CompanyTariffTypes.Count);
			AssertEquals("Freight", tariff.CompanyTariffTypes.GetDescriptionFromCode("FRT"));
			AssertEquals("Origin", tariff.CompanyTariffTypes.GetDescriptionFromCode("ORG"));
			AssertEquals("Destination", tariff.CompanyTariffTypes.GetDescriptionFromCode("DST"));
			AssertEquals("Shipping Freight", tariff.CompanyTariffTypes.GetDescriptionFromCode("SFR"));
			AssertEquals("Shipping Origin", tariff.CompanyTariffTypes.GetDescriptionFromCode("SOR"));
			AssertEquals("Shipping Destination", tariff.CompanyTariffTypes.GetDescriptionFromCode("SDE"));
			AssertEquals("Shipping Container Detention", tariff.CompanyTariffTypes.GetDescriptionFromCode("SCD"));
			AssertEquals("CFS", tariff.CompanyTariffTypes.GetDescriptionFromCode("CFS"));
			AssertEquals("Product Warehouse", tariff.CompanyTariffTypes.GetDescriptionFromCode("WHS"));
			AssertEquals("Transit Warehouse", tariff.CompanyTariffTypes.GetDescriptionFromCode("TRW"));
			AssertEquals("Transit Warehouse Transportation Unit", tariff.CompanyTariffTypes.GetDescriptionFromCode("TWU"));
			AssertEquals("Transport", tariff.CompanyTariffTypes.GetDescriptionFromCode("TRN"));
			AssertEquals("Land Transport", tariff.CompanyTariffTypes.GetDescriptionFromCode("TBC"));
			AssertEquals("Container Yard", tariff.CompanyTariffTypes.GetDescriptionFromCode("CYD"));
			AssertEquals("Container Yard Transportation Unit", tariff.CompanyTariffTypes.GetDescriptionFromCode("CYU"));
			AssertEquals("Yard Maintenance and Repair Charges", tariff.CompanyTariffTypes.GetDescriptionFromCode("CYM"));
		}

		public void TestCompanyTariffLinesShownCorrectlyForDifferentTariffLevels()
		{
			var orgEntry = Tariff1.AddRateEntry("ORG", "AIR", "AUSYD", "INBOM");
			orgEntry.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 20m;
			orgEntry.AddRateLine("OAWB", FlatCalculator.Code);

			Tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 50M);

			Tariff1.Factory.Save();
			Tariff2.Factory.Save();

			var orgHeader = Helper.NewOrgHeader(0);
			var testQuote = Helper.NewQuote(orgHeader);
			var quoteEntry = (QuoteEntry)testQuote.AddRateEntry("ORG", "AIR", "AUSYD", "INBOM");

			AssertEquals("Company Tariff 1 Lines are not displayed when company tariff level 0 is chosen", 0, quoteEntry.RelatedRateLines.Count);

			orgHeader.CompanyData.RateTariffLevels.RemoveAndDeleteAll();
			orgHeader.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			AssertEquals("Company Tariff 1 Lines are displayed when company tariff level 1 is chosen", 2, quoteEntry.RelatedRateLines.Count);

			RateLine odocLine = null;
			RateLine oawbLine = null;
			if (quoteEntry.RelatedRateLines[0].ChargeCode.AC_Code == "ODOC")
			{
				odocLine = quoteEntry.RelatedRateLines[0];
			}
			else if (quoteEntry.RelatedRateLines[0].ChargeCode.AC_Code == "OAWB")
			{
				oawbLine = quoteEntry.RelatedRateLines[0];
			}
			if (quoteEntry.RelatedRateLines[1].ChargeCode.AC_Code == "ODOC")
			{
				odocLine = quoteEntry.RelatedRateLines[1];
			}
			else if (quoteEntry.RelatedRateLines[1].ChargeCode.AC_Code == "OAWB")
			{
				oawbLine = quoteEntry.RelatedRateLines[1];
			}
			AssertNotNull("ODOC Charge Code Present", odocLine);
			AssertNotNull("OAWB Charge Code Present", oawbLine);
			AssertEquals("ODOC Charge Code Present with tariff 1 amount", 20M, odocLine.GetCalculator<FlatCalculator>().BaseRate);

			testQuote.Header.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			quoteEntry.InvalidateRelatedRateLines();

			AssertEquals("Company Tariff 1 Lines displayed when company tariff 0 is chosen", 2, quoteEntry.RelatedRateLines.Count);
			odocLine = null;
			oawbLine = null;
			if (quoteEntry.RelatedRateLines[0].ChargeCode.AC_Code == "ODOC")
			{
				odocLine = quoteEntry.RelatedRateLines[0];
			}
			else if (quoteEntry.RelatedRateLines[0].ChargeCode.AC_Code == "OAWB")
			{
				oawbLine = quoteEntry.RelatedRateLines[0];
			}
			if (quoteEntry.RelatedRateLines[1].ChargeCode.AC_Code == "ODOC")
			{
				odocLine = quoteEntry.RelatedRateLines[1];
			}
			else if (quoteEntry.RelatedRateLines[1].ChargeCode.AC_Code == "OAWB")
			{
				oawbLine = quoteEntry.RelatedRateLines[1];
			}
			AssertNotNull("ODOC Charge Code Present", odocLine);
			AssertNotNull("OAWB Charge Code Present", oawbLine);
			AssertEquals("ODOC Charge Code Present with tariff 1 amount", 20M, odocLine.GetCalculator<FlatCalculator>().BaseRate);

			testQuote.Header.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);
			quoteEntry.InvalidateRelatedRateLines();

			AssertEquals("Company Tariff 1 Lines displayed when company tariff 0 is chosen", 2, quoteEntry.RelatedRateLines.Count);
			odocLine = null;
			oawbLine = null;
			if (quoteEntry.RelatedRateLines[0].ChargeCode.AC_Code == "ODOC")
			{
				odocLine = quoteEntry.RelatedRateLines[0];
			}
			else if (quoteEntry.RelatedRateLines[0].ChargeCode.AC_Code == "OAWB")
			{
				oawbLine = quoteEntry.RelatedRateLines[0];
			}
			if (quoteEntry.RelatedRateLines[1].ChargeCode.AC_Code == "ODOC")
			{
				odocLine = quoteEntry.RelatedRateLines[1];
			}
			else if (quoteEntry.RelatedRateLines[1].ChargeCode.AC_Code == "OAWB")
			{
				oawbLine = quoteEntry.RelatedRateLines[1];
			}
			AssertNotNull("ODOC Charge Code Present", odocLine);
			AssertNotNull("OAWB Charge Code Present", oawbLine);
			AssertEquals("ODOC Charge Code Present with tariff 2 amount", 10M, odocLine.GetCalculator<FlatCalculator>().BaseRate);
		}

		public void TestLevelOneTariffReference()
		{
			Assert("1st Tariff created is Level One Tariff", Tariff1.IsLevelOneTariff());
			AssertEquals("1st Tariff has correct reference to Level 1 Tariff (itself)", Tariff1.PK, Tariff1.LevelOneTariff.PK);

			Assert("2nd Tariff created is an Additional Tariff", Tariff2.IsAdditionalTariff());
			AssertEquals("2nd Tariff has correct reference to Level 1 Tariff", Tariff1.PK, Tariff2.LevelOneTariff.PK);
		}

		public void TestTariffLevelAndDescription()
		{
			AssertEquals("1st Tariff is Level 1", (ZByte)1, Tariff1.TH_GlobalRateLevel);
			AssertEquals("1st Tariff has correct description", "Base Company Tariff", Tariff1.TH_GlobalRateDescription);

			AssertEquals("2nd Tariff is Level 2", (ZByte)2, Tariff2.TH_GlobalRateLevel);
			AssertEquals("2nd Tariff has correct description", "Company Tariff Level 2", Tariff2.TH_GlobalRateDescription);
		}

		public void TestRelatedEntitiesAreDeletedWithoutLoadingWhenDeletingCompanyTariffsLevel2plus()
		{
			Action<RateLine> setLine = (line) =>
			{
				line.TL_RateCalculator = "FLT";
				line.GetCalculator<FlatCalculator>().BaseRate = 10;
				line.TL_Condition = RateLineConditions.UserDefined;
				line.TL_ConditionalExpression = "MOD=FSA";
			};

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			setLine(rateLine);

			var tariff1Entry = Tariff1.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE");
			tariff1Entry.RateLines.RemoveAndDeleteAll();
			var tariff1Line1 = tariff1Entry.AddRateLine("FRT", FlatCalculator.Code);
			var tariff1Line2 = tariff1Entry.AddRateLine("FRT", FlatCalculator.Code);
			setLine(tariff1Line1);
			setLine(tariff1Line2);

			Factory.Save();

			var tariff2 = Tariff2;
			var newFactory = new BusinessObjectFactory();
			var loadedTariff2 = newFactory.Load<CompanyTariff>(tariff2.PK);

			var tariff2Entry = loadedTariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0];
			var tariff2Line1 = tariff2Entry.RateLines[0];
			var tariff2Line2 = tariff2Entry.RateLines[1];

			tariff2Entry.RateLines.OverrideTariffLines(tariff2Entry.RateLines.Cast<RateLine>().ToArray());

			newFactory.Save();

			loadedTariff2.Delete();
			newFactory.Save();

			var inMemoryQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			inMemoryQuery.AddToFilter(RateEntrySchema.TI_TH, tariff2.PK);

			AssertEquals(0, newFactory.Load<RateEntry>(inMemoryQuery).Length);

			inMemoryQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			inMemoryQuery.AddToFilter(RateEntrySchema.TI_TH, tariff2Line1.PK);

			AssertEquals(0, newFactory.Load<RateEntry>(inMemoryQuery).Length);

			inMemoryQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			inMemoryQuery.AddToFilter(RateEntrySchema.TI_TH, tariff2Line2.PK);

			AssertEquals(0, newFactory.Load<RateEntry>(inMemoryQuery).Length);

			newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<CompanyTariff>(tariff2.PK));
			AssertNull(newFactory.Load<CompanyTariff>(tariff2Line1.PK));
			AssertNull(newFactory.Load<CompanyTariff>(tariff2Line2.PK));

			AssertNotNull(newFactory.Load<CompanyTariff>(tariff1.PK));
			AssertNotNull(newFactory.Load<RateLine>(tariff1Line1.PK));
			AssertNotNull(newFactory.Load<RateLine>(tariff1Line2.PK));
		}

		public void TestCanDelete()
		{
			var companyTariff = Helper.NewCompanyTariff();
			var globalTariff = Helper.NewGlobalTariff();

			companyTariff.Factory.Save();
			globalTariff.Factory.Save();

			AssertEquals("Pre-condition", new ZByte(1), companyTariff.TH_GlobalRateLevel);
			AssertEquals("Pre-condition", new ZByte(1), globalTariff.TH_GlobalRateLevel);

			var additionalCompanyTariff = Helper.NewCompanyTariff();
			var additionalGlobalTariff = Helper.NewGlobalTariff();

			var additionalCompanyTariffFactory = additionalCompanyTariff.Factory;
			var additionalGlobalTariffFactory = additionalGlobalTariff.Factory;
			additionalCompanyTariffFactory.Save();
			additionalGlobalTariffFactory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition", new ZByte(2), additionalCompanyTariff.TH_GlobalRateLevel);
				AssertEquals(true, additionalCompanyTariff.CanDelete);
				AssertEquals(false, companyTariff.CanDelete);
				AssertEquals("You cannot delete Base Company Tariff when additional Local Tariff(s) exist.", companyTariff.ReasonForNotAbleToDelete);

				AssertEquals("Pre-condition", new ZByte(2), additionalGlobalTariff.TH_GlobalRateLevel);
				AssertEquals(true, additionalGlobalTariff.CanDelete);
				AssertEquals(false, globalTariff.CanDelete);
				AssertEquals("You cannot delete Global Base Tariff when additional Global Tariff(s) exist.", globalTariff.ReasonForNotAbleToDelete);
			});

			companyTariff.TH_GlobalRateDescription = "Bobby";
			companyTariff.Factory.Save();
			additionalGlobalTariff.Delete();
			additionalGlobalTariffFactory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(false, companyTariff.CanDelete);
				AssertEquals("You cannot delete Bobby when additional Local Tariff(s) exist.", companyTariff.ReasonForNotAbleToDelete);

				AssertEquals(true, globalTariff.CanDelete);
				AssertEquals(true, globalTariff.ReasonForNotAbleToDelete.IsEmpty);
			});

			additionalCompanyTariff.Delete();
			additionalCompanyTariffFactory.Save();
			var additionalGlobalTariff2 = Helper.NewGlobalTariff();
			additionalGlobalTariff2.Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(true, companyTariff.CanDelete);
				AssertEquals(true, companyTariff.ReasonForNotAbleToDelete.IsEmpty);

				AssertEquals(false, globalTariff.CanDelete);
				AssertEquals("You cannot delete Global Base Tariff when additional Global Tariff(s) exist.", globalTariff.ReasonForNotAbleToDelete);
			});
		}

		#region Implementation

		CompanyTariff Tariff1
		{
			get
			{
				if (tariff1 == null)
				{
					tariff1 = Factory.New<CompanyTariff>();
					Factory.Save();
				}

				return tariff1;
			}
		}
		CompanyTariff tariff1;

		CompanyTariff Tariff2
		{
			get
			{
				AssertNotNull(Tariff1);
				if (tariff2 == null)
				{
					tariff2 = Factory.New<CompanyTariff>();
					Factory.Save();
				}

				return tariff2;
			}
		}
		CompanyTariff tariff2;

		#endregion
	}

	#region Business Object TestCase

	[TestedType(typeof(CompanyTariff))]
	public class BaseGlobalTariffBizObjTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsSuppressedForTestDbHits => false;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<CompanyTariff>();
			var entry = result.AddRateEntry("AIR");
			entry.TI_RateStartDate = ZDate.Today.AddMonths(-6);

			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<Costing>(new ZQuery());
		}
	}

	#endregion
}
