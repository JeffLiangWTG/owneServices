using System;
using System.Linq;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	public class IncotermsRankerTest : RatingTestCase
	{
		#region JobType x OrgType tests

		public void TestJobNonCUS_PrioritiesAll()
		{
			var priorities = new[]
			{
				(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL),
				(RatingDebtorOrgTypes.CNR, RatingJobTypes.ALL), // <== try to match
				(RatingDebtorOrgTypes.AG, RatingJobTypes.ALL),
			};
			var jobRateType = RateType.Forwarding;

			using (SetStandaloneCustomJobRegistry(false))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertContainsExactElementsInAnyOrder(
						"Job has only 1 debtor",
						new[] { RatingDebtorOrgTypes.CNR },
						filteredOrgs
					);

					AssertEquals(
						"Priority index 1 matches the criteria debtor type",
						1,
						ranker.OrgImportance
					);
				}
			}

			using (SetStandaloneCustomJobRegistry(true))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertContainsExactElementsInAnyOrder(
						"Job has only 1 debtor",
						new[] { RatingDebtorOrgTypes.CNR },
						filteredOrgs
					);

					AssertEquals(
						"Priority index 1 matches the criteria debtor type",
						1,
						ranker.OrgImportance
					);
				}
			}
		}

		public void TestJobNonCUS_PrioritiesCUS()
		{
			var priorities = new[]
			{
				(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS),
				(RatingDebtorOrgTypes.CNR, RatingJobTypes.CUS),
				(RatingDebtorOrgTypes.AG, RatingJobTypes.CUS),
			};
			var jobRateType = RateType.Forwarding;

			using (SetStandaloneCustomJobRegistry(false))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertEquals("No 'ALL' priorities are present that match CNR/ALL.", 0, filteredOrgs.Count());
					AssertEquals("OrgImportance should be NotApplicable.", IncotermsRanker.NotApplicable, ranker.OrgImportance);
				}
			}

			using (SetStandaloneCustomJobRegistry(true))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertEquals("No 'ALL' priorities are present that match CNR/ALL.", 0, filteredOrgs.Count());
					AssertEquals("OrgImportance should be NotApplicable.", IncotermsRanker.NotApplicable, ranker.OrgImportance);
				}
			}
		}

		public void TestJobCUS_PrioritiesAll()
		{
			var priorities = new[]
			{
				(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL),
				(RatingDebtorOrgTypes.CNR, RatingJobTypes.ALL),
				(RatingDebtorOrgTypes.AG, RatingJobTypes.ALL),
			};
			var jobRateType = RateType.Customs;

			using (SetStandaloneCustomJobRegistry(false))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertEquals(
						"Custom rate type only matches CUS priorities.",
						0,
						filteredOrgs.Count()
					);
					AssertEquals(IncotermsRanker.NotApplicable, ranker.OrgImportance);
				}
			}

			using (SetStandaloneCustomJobRegistry(true))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertEquals(
						"Custom rate type only matches CUS priorities.",
						0,
						filteredOrgs.Count()
					);
					AssertEquals(IncotermsRanker.NotApplicable, ranker.OrgImportance);
				}
			}
		}

		public void TestJobCUS_PrioritiesCUS()
		{
			var priorities = new[]
			{
				(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS),
				(RatingDebtorOrgTypes.CNR, RatingJobTypes.CUS), // <== try to match
				(RatingDebtorOrgTypes.AG, RatingJobTypes.CUS),
			};
			var jobRateType = RateType.Customs;

			using (SetStandaloneCustomJobRegistry(false))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertEquals(
						"Custom rate type only matches CUS priorities when registry is enabled.",
						0,
						filteredOrgs.Count()
					);
					AssertEquals(IncotermsRanker.NotApplicable, ranker.OrgImportance);
				}
			}

			using (SetStandaloneCustomJobRegistry(true))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertContainsExactElementsInAnyOrder(
						"Job has only 1 debtor",
						new[] { RatingDebtorOrgTypes.CNR },
						filteredOrgs
					);
					AssertEquals(
						"Priority index 1 matches the criteria debtor type",
						1,
						ranker.OrgImportance
					);
				}
			}
		}

		public void TestRank_RateLineFromCompanyTariffLevelOverrideHasHighestImportance()
		{
			var autoRatingMock = new Mock<IAutoRating>();
			var mockTariffLevelProvider = autoRatingMock.As<IAutoRatingCompanyTariffLevelProvider>();
			mockTariffLevelProvider.Setup(a => a.TariffLevel).Returns(1);
			var ratingCriteria = new TestRatingCriteria(autoRatingMock.Object);
			var line = CreateCompanyTariffFastLine(ratingCriteria);

			AssertEquals("RateLine from Company Tariff Level Override has highest importance regardless of OrgHeader", 0, new IncotermsRanker().Rank(line).OrgImportance);
		}

		public void TestJobALL_PrioritiesMixed()
		{
			var priorities = new[]
			{
				(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS),
				(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL),
				(RatingDebtorOrgTypes.CNR, RatingJobTypes.ALL),
				(RatingDebtorOrgTypes.CNR, RatingJobTypes.CUS),
				(RatingDebtorOrgTypes.AG, RatingJobTypes.CUS),
				(RatingDebtorOrgTypes.AG, RatingJobTypes.ALL), // <== try to match
			};
			var jobRateType = RateType.Forwarding;

			using (SetStandaloneCustomJobRegistry(false))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.AG);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertContainsExactElementsInAnyOrder(
						"Job has only 1 debtor",
						new[] { RatingDebtorOrgTypes.AG },
						filteredOrgs
					);

					AssertEquals(
						"Priority index 5 matches the criteria debtor type",
						5,
						ranker.OrgImportance
					);
				}
			}

			using (SetStandaloneCustomJobRegistry(true))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.AG);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertContainsExactElementsInAnyOrder(
						"Job has only 1 debtor",
						new[] { RatingDebtorOrgTypes.AG },
						filteredOrgs
					);

					AssertEquals(
						"Priority index 5 matches the criteria debtor type",
						5,
						ranker.OrgImportance
					);
				}
			}
		}

		public void TestJobCUS_PrioritiesMixed()
		{
			var priorities = new[]
			{
				(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS),
				(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL),
				(RatingDebtorOrgTypes.CNR, RatingJobTypes.ALL),
				(RatingDebtorOrgTypes.CNR, RatingJobTypes.CUS), // <== try to match
				(RatingDebtorOrgTypes.AG, RatingJobTypes.CUS),
				(RatingDebtorOrgTypes.AG, RatingJobTypes.ALL),
			};
			var jobRateType = RateType.Customs;

			using (SetStandaloneCustomJobRegistry(false))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					AssertEquals(
						"Custom rate type only matches CUS priorities when registry is enabled.",
						0,
						filteredOrgs.Count()
					);
					AssertEquals(IncotermsRanker.NotApplicable, ranker.OrgImportance);
				}
			}

			using (SetStandaloneCustomJobRegistry(true))
			{
				using (PopulateRatesPriorities(priorities, RatingDataRegistry.Instance.ImportCollectPriorities))
				{
					var ranker = CreateRanker(jobRateType, Directions.Import, RatingDebtorOrgTypes.CNR);
					var filteredOrgs = ranker.OrgTypesSellRatePriorityRegistryFiltered;

					var expectedFilteredOrgs = new[] { RatingDebtorOrgTypes.CNR };
					AssertContainsExactElementsInAnyOrder(
						"Job has only 1 debtor",
						expectedFilteredOrgs,
						filteredOrgs
					);

					AssertEquals(
						"Priority index 3 matches the criteria debtor type",
						3,
						ranker.OrgImportance
					);
				}
			}
		}

		#endregion

		#region Helpers

		IncotermsRanker CreateRanker(RateType jobRateType, Directions jobDirection, RatingDebtorOrgTypes orgTypeInJob)
		{
			var criteria = new TestRatingCriteria();
			criteria.RateTypeToUse = jobRateType;
			criteria.DebtorOrgs.Clear();
			criteria.JobDirection = jobDirection;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var debtorOrg = new DebtorOrg(org, orgTypeInJob);
			criteria.DebtorOrgs.Add(debtorOrg);

			var rateLine = new Mock<IRateLine>();
			rateLine.Setup(x => x.TL_FeeChargeType).Returns("X");

			var ranker = new IncotermsRanker();
			ranker.IsApplicableToOrg = new Func<IRateLine, OrgHeader, RatingCriteria, bool>((a, b, c) =>
			{
				return true;
			});

			return ranker.Rank(criteria, rateLine.Object, isCostRate: false, isCollect: true);
		}

		IDisposable PopulateRatesPriorities((RatingDebtorOrgTypes OrgType, RatingJobTypes JobType)[] prioritiesInOrder, RatesPrioritiesRegistryItem registryItem)
		{
			var collection = new RatesPrioritiesCollection();
			foreach (var priorities in prioritiesInOrder)
			{
				collection.AddNew(priorities.OrgType, priorities.JobType);
			}
			return registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		IDisposable SetStandaloneCustomJobRegistry(bool value) =>
			RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

		FastLine CreateCompanyTariffFastLine(RatingCriteria criteria, string rateCategory = "AIR", string origin = "AUSYD", string destination = "CNSHA")
		{
			return criteria.Cache.GetOrCreateFastLine(CreateRateLineFromCompanyTariff(rateCategory, origin, destination));
		}

		RateLine CreateRateLineFromCompanyTariff(string rateCategory, string origin, string destination)
		{
			var companyTariff = Helper.NewCompanyTariff();
			return companyTariff.AddRateEntryWithFlatRateLine(rateCategory, "LCL", origin, destination, "FRT", 50).RateLines[0];
		}

		#endregion
	}
}
