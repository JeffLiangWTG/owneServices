using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Rating.ServiceTasks.Test
{
	[TestedType(typeof(DeleteExpiredRatesServiceTask))]
	class DeleteExpiredRatesServiceTaskTest : ServiceTaskTestCase<DeleteExpiredRatesServiceTask>
	{
		public void TestRunTask()
		{
			var entries = GetRateEntries();

			var entriesToBeDeleted = new[] { entries[0], entries[3], entries[6], entries[9], entries[12], entries[15], entries[18], entries[21] };

			var expectedEntries = entries.Except(entriesToBeDeleted).ToList();

			var expectedEntriesPK = expectedEntries.Select(e => e.PK).ToList();

			var expectedLinesPK = expectedEntries.SelectMany(e => e.RateLines).Select(x => x.PK).ToList();

			Env.Registry.Rating.PermanentlyDeleteRatesExpiredPeriod = new DeleteExpiredRates { ExpiredRatesPeriodInYears = 1, BatchSize = 10 };

			var checkFactory = new BusinessObjectFactory();
			var rateLinesTotal = checkFactory.Load<RateLine>(new ZQuery()).OrderBy(x => x.TL_TI).ToList();

			var serviceLogger = new TestServiceLogger();
			var serviceTask = new DeleteExpiredRatesServiceTask() { ServiceLogger = serviceLogger };
			serviceTask.RunTask();

			var deletedfactory = new BusinessObjectFactory();

			var rateEntriesNotDeletedPK = deletedfactory.Load<RateEntry>(new ZQuery()).Select(x => x.PK).ToList();
			var rateLinesNotDeletedPK = deletedfactory.Load<RateLine>(new ZQuery()).Select(x => x.PK).ToList();

			AssertContainsExactElementsInAnyOrder("Entries not deleted", expectedEntriesPK, rateEntriesNotDeletedPK);
			AssertContainsExactElementsInAnyOrder("Lines not deleted", expectedLinesPK, rateLinesNotDeletedPK);
		}

		public void TestRunTask_RegistryZero()
		{
			var entries = GetRateEntries();

			var expectedEntriesPK = entries.Select(e => e.PK).ToList();
			var expectedLinesPK = entries.SelectMany(e => e.RateLines).Select(x => x.PK).ToList();

			Env.Registry.Rating.PermanentlyDeleteRatesExpiredPeriod = new DeleteExpiredRates { ExpiredRatesPeriodInYears = 0, BatchSize = 10 };

			var serviceLogger = new TestServiceLogger();
			var serviceTask = new DeleteExpiredRatesServiceTask() { ServiceLogger = serviceLogger };
			serviceTask.RunTask();

			var deletedFactory = new BusinessObjectFactory();

			var rateEntriesNotDeletedPK = deletedFactory.Load<RateEntry>(new ZQuery()).Select(x => x.PK).ToList();
			var rateLinesNotDeletedPK = deletedFactory.Load<RateLine>(new ZQuery()).Select(x => x.PK).ToList();

			AssertContainsExactElementsInAnyOrder("Entries not deleted", expectedEntriesPK, rateEntriesNotDeletedPK);
			AssertContainsExactElementsInAnyOrder("Lines not deleted", expectedLinesPK, rateLinesNotDeletedPK);
		}

		public void TestRunTask_ByBatchSize()
		{
			var entries = GetRateEntries();

			var entriesToBeDeleted = new[] { entries[0], entries[3], entries[6], entries[9], entries[12], entries[15], entries[18], entries[21] };
			var expectedEntries = entries.Except(entriesToBeDeleted).ToList();
			var expectedEntriesPK = expectedEntries.Select(e => e.PK).ToList();
			var expectedLinesPK = expectedEntries.SelectMany(e => e.RateLines).Select(x => x.PK).ToList();
			var batchSize = 5;

			Env.Registry.Rating.PermanentlyDeleteRatesExpiredPeriod = new DeleteExpiredRates { ExpiredRatesPeriodInYears = 1, BatchSize = batchSize };

			var checkFactory = new BusinessObjectFactory();
			var rateLinesTotal = checkFactory.Load<RateLine>(new ZQuery()).OrderBy(x => x.TL_TI).ToList();

			var serviceLogger = new TestServiceLogger();
			var serviceTask = new DeleteExpiredRatesServiceTask() { ServiceLogger = serviceLogger };
			serviceTask.RunTask();

			var deletedFactory = new BusinessObjectFactory();
			var rateEntriesNotDeletedPK = deletedFactory.Load<RateEntry>(new ZQuery()).Select(x => x.PK).ToList();
			var rateLinesNotDeletedPK = deletedFactory.Load<RateLine>(new ZQuery()).Select(x => x.PK).ToList();

			AssertEquals("No Changes as every expired is already deleted", entries.Length - entriesToBeDeleted.Length, rateEntriesNotDeletedPK.Count);
			AssertContainsExactElementsInAnyOrder("Entries not deleted", expectedEntriesPK, rateEntriesNotDeletedPK);
			AssertContainsExactElementsInAnyOrder("Lines not deleted", expectedLinesPK, rateLinesNotDeletedPK);
			var expiryDate = ZDate.Today.AddYears(-1);
			AssertContains($"Cut-off Expiry Date: {expiryDate.ToShortDateString()} batch 1 - 5 of 8", serviceLogger.ToString());
			AssertContains($"Cut-off Expiry Date: {expiryDate.ToShortDateString()} batch 6 - 8 of 8", serviceLogger.ToString());
		}

		RateEntry[] GetRateEntries()
		{
			var defaultCompany = Factory.NewWithValidTestData<GlbCompany>();
			defaultCompany.GC_Code = "ABC";

			var standardCost = Helper.NewCosting(null);
			var qantasCost = Helper.NewCosting(QantasOrg);
			var costingWithoutCompany = Helper.NewCosting(Consignee);

			var clientRate = Helper.NewClientRate(QantasOrg);
			var clientRateWithoutCompany = Helper.NewClientRate(Consignee);

			var tariff = Helper.NewCompanyTariff();
			var tariffWithoutCompany = Helper.NewCompanyTariff();

			var chargeCode = Helper.ChargeCodes.CreateGlobalCharge("TEST");
			var intercompanyTariffWithoutCompany = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var qantasQuote = Helper.NewQuote(QantasOrg);
			var consigneeQuote = Helper.NewQuote(Consignee);

			var entries = new[]
			{
				#region Costing
				CreateRateEntry(standardCost, null, ZDate.Today.AddYears(-3), null, null), // Should be deleted   
				CreateRateEntry(standardCost, null, ZDate.Today, null, null),
				CreateRateEntry(standardCost, null, null, null, null),
				CreateRateEntry(qantasCost, defaultCompany, ZDate.Today.AddYears(-3), null, null), // Should be deleted   
				CreateRateEntry(qantasCost, defaultCompany, ZDate.Today, null, null),
				CreateRateEntry(qantasCost, defaultCompany, null, null, null),
				CreateRateEntry(costingWithoutCompany, null, ZDate.Today.AddYears(-3), null, null), // Should be deleted   
				CreateRateEntry(costingWithoutCompany, null, ZDate.Today, null, null),
				CreateRateEntry(costingWithoutCompany, null, null, null, null),
				#endregion

				#region Client Rate
				CreateRateEntry(clientRateWithoutCompany, null, ZDate.Today.AddYears(-3), null, null), // Should be deleted   
				CreateRateEntry(clientRateWithoutCompany, null, ZDate.Today, null, null),
				CreateRateEntry(clientRateWithoutCompany, null, null, null, null),
				CreateRateEntry(clientRate, defaultCompany, ZDate.Today.AddYears(-3), null, null), // Should be deleted   
				CreateRateEntry(clientRate, defaultCompany, ZDate.Today, null, null),
				CreateRateEntry(clientRate, defaultCompany, null, null, null),
				#endregion

				#region Tariff
				CreateRateEntry(tariff, defaultCompany, ZDate.Today.AddYears(-3), null, (ZByte)1), // Should be deleted   
				CreateRateEntry(tariff, defaultCompany, ZDate.Today, null, (ZByte)1),
				CreateRateEntry(tariff, defaultCompany, null, null, (ZByte)1),
				CreateRateEntry(tariffWithoutCompany, null, ZDate.Today.AddYears(-3), null, (ZByte)1), // Should be deleted   
				CreateRateEntry(tariffWithoutCompany, null, ZDate.Today, null, (ZByte)1),
				CreateRateEntry(tariffWithoutCompany, null, null, null, (ZByte)1),
				#endregion

				#region Intercompany Tariff
				CreateRateEntry(intercompanyTariffWithoutCompany, null, ZDate.Today.AddYears(-3), chargeCode.PK, null), // Should be deleted   
				CreateRateEntry(intercompanyTariffWithoutCompany, null, ZDate.Today, chargeCode.PK, null),
				CreateRateEntry(intercompanyTariffWithoutCompany, null, null, chargeCode.PK, null),
				#endregion

				#region Quote
				CreateRateEntry(qantasQuote, null, ZDate.Today.AddYears(-3), null, null),
				CreateRateEntry(qantasQuote, null, ZDate.Today, null, null),
				CreateRateEntry(qantasQuote, null, null, null, null),
				CreateRateEntry(consigneeQuote, defaultCompany, ZDate.Today.AddYears(-3), null, null),
				CreateRateEntry(consigneeQuote, defaultCompany, ZDate.Today, null, null),
				CreateRateEntry(consigneeQuote, defaultCompany, null, null, null),
				#endregion
			};

			tariff.Factory.Save();
			tariffWithoutCompany.Factory.Save();
			Factory.Save();

			return entries;
		}

		RateEntry CreateRateEntry(RatingHeader header, GlbCompany company, ZDate? expiryDate, ZGuid? chargeCode, ZByte? globalRateLevel)
		{
			if (company != null)
			{
				header.TH_GC = company.PK;
			}

			if (globalRateLevel.HasValue)
			{
				header.TH_GlobalRateLevel = globalRateLevel.Value;
			}

			var rateEntry = header.AddRateEntry("DST");
			var rateLine = rateEntry.AddFlatRateLine("FRT", 100);
			rateLine.TL_AC = chargeCode ?? rateLine.TL_AC;

			var rateLine2 = rateEntry.AddFlatRateLine("FRT", 200);
			rateLine2.TL_AC = chargeCode ?? rateLine2.TL_AC;

			var rateLine3 = rateEntry.AddFlatRateLine("FRT", 300);
			rateLine3.TL_AC = chargeCode ?? rateLine3.TL_AC;

			if (expiryDate.HasValue)
			{
				rateEntry.TI_RateStartDate = expiryDate.Value.AddYears(-1);
				rateEntry.TI_RateEndDate = expiryDate.Value;
			}
			else
			{
				rateEntry.TI_Mode = "SEA";
				rateEntry.TI_RateStartDate = ZDate.Today.AddYears(-3);
			}
			return rateEntry;
		}

		OrgHeader fConsignee;
		public OrgHeader Consignee
		{
			get
			{
				if (fConsignee == null)
				{
					fConsignee = Factory.NewWithValidTestData<OrgHeader>();
					fConsignee.OH_FullName = "Consignee";
					fConsignee.OH_RL_NKClosestPort = "AUSYD";
					fConsignee.OH_Code = "CONSIGNEE1";
					fConsignee.OH_IsConsignee = true;
				}
				return fConsignee;
			}
		}

		OrgHeader fqantasOrg;
		public OrgHeader QantasOrg
		{
			get
			{
				if (fqantasOrg == null)
				{
					fqantasOrg = Factory.NewWithValidTestData<OrgHeader>();
					fqantasOrg.OH_FullName = "Qantas";
					fqantasOrg.OH_RL_NKClosestPort = "AUSYD";
					fqantasOrg.OH_Code = "QANAUS";
					fqantasOrg.OH_IsConsignee = true;
				}
				return fqantasOrg;
			}
		}

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
