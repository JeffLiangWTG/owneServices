using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	public class RatesLoaderTest : RatingTestCase
	{
		public SchemaColumn RefUnlocoSchema { get; private set; }

		public void TestLoadRateEntries_NoDuplicatesAtServiceLocation()
		{
			var helper = Helper;
			var chargeCode1 = helper.ChargeCodes.New("NCLORG", "Non Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.QuarantineInspection);
			var client = NewClient;
			var clientRate = helper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.SEA, "AU", "NZ", chargeCode1.AC_Code, 10m);
			Factory.Save();

			var testObject = new AutoRatingObject("USLAX", "NZAKL", FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0m, 0m, client);
			testObject.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var services = new JobServicesCollection();
			var service = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.QuarantineInspection, "desc", 1,
				locationCode: "AUSYD");
			services.Add(service);
			testObject.JobServices = services;
			var autoRatingProxy = new AutoRatingProxy(testObject);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			var logger = new TestLogger();
			var entries = new RevenueRatesLoader(Factory, logger).Load(criteria);
			AssertEquals(1, entries.Count());
		}

		#region Revenue

		#region LoadCompanyTariffsTest

		public void TestLoadCompanyTariffs()
		{
			var testObject = new AutoRatingObjectWithTariffLevel();
			((IAutoRating)testObject).DebtorOrgs[RatingDebtorOrgTypes.LC] = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "NEWTESSYD");
			var autoRatingInfo = new AutoRatingProxy(testObject);

			InsertClientChargeCodesForAutoRaterTests(Factory);
			Factory.Save();

			var tariff1 = Helper.NewCompanyTariff();
			var tariffEntry1 = tariff1.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			var tariffLine1 = tariffEntry1.AddRateLine("DDOC", FlatCalculator.Code);
			tariffLine1.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)500m;
			tariff1.Factory.Save();

			var tariff2 = Helper.NewCompanyTariff();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Factory.Save();

			var testCriteria = new RatingCriteria(autoRatingInfo, Factory);

			Factory.Save();

			var loader = new RevenueRatesLoader(Factory, new DummyLogger());
			var headers = loader.LoadHeaders(testCriteria).ToArray();

			AssertEquals(1, headers.Length);
			AssertEquals(1, headers.Length);
			AssertEquals(1, testCriteria.TariffLevel);
			AssertEquals(tariff1.PK, RatingHeader.GetBO(headers[0]).PK);
		}

		[ExpectNoExceptions]
		public void TestNoInfiniteCycleWhenLoadingCompanyTariffs()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var testRateClient = Helper.NewClientRate(NewClient);
			var testRateConsignor = Helper.NewClientRate(Consignor);
			var testRateConsignee = Helper.NewClientRate(Consignee);

			NewClient.RelatedManagementSubsidiaryRelations.AddOrganisationWithoutCheckingValid(Consignee);
			Consignee.RelatedManagementSubsidiaryRelations.AddOrganisationWithoutCheckingValid(NewClient);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignee = Consignee;
			testCriteria.Consignor = Consignor;

			Factory.Save();

			var loader = new RevenueRatesLoader(Factory, new DummyLogger());
			var headers = loader.LoadHeaders(testCriteria);
			AssertEquals(3, headers.Count());
		}

		#endregion

		#region Get Intercompany Tariffs

		public void TestGetIntercompanyTariffs_BypassIctWhenReqistryItemHasDefaultValue()
		{
			var serviceProviderCompany = Helper.NewOrgHeader();
			var serviceProviderBranch = Helper.NewOrgHeader();

			using (SetTemporaryCompanyOrgProxy(serviceProviderCompany.PK))
			using (SetTemporaryBranchOrgProxy(serviceProviderBranch.PK))
			using (SetTemporaryUseIntercompanyTariffsRegistryItem(false))
			{
				Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
				Helper.NewIntercompanyTariff(serviceProviderCompany);
				Helper.NewIntercompanyTariff(serviceProviderBranch);

				Factory.Save();

				var loader = new RevenueRatesLoader(Factory, new DummyLogger());
				var ratingCriteria = new TestRatingCriteria();
				ratingCriteria.GatewayBillingSupporter = new GatewayBillingSupporterForTest(serviceProviderBranch, null, true);

				var headers = loader.LoadHeaders(ratingCriteria);
				AssertEquals(0, headers.Count());
			}
		}

		public void TestGetIntercompanyTariffs_BypassIctForNonGatewayBilling()
		{
			var serviceProviderCompany = Helper.NewOrgHeader();
			var serviceProviderBranch = Helper.NewOrgHeader();

			using (SetTemporaryCompanyOrgProxy(serviceProviderCompany.PK))
			using (SetTemporaryBranchOrgProxy(serviceProviderBranch.PK))
			using (SetTemporaryUseIntercompanyTariffsRegistryItem(true))
			{
				Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
				Helper.NewIntercompanyTariff(serviceProviderCompany);
				Helper.NewIntercompanyTariff(serviceProviderBranch);

				Factory.Save();

				var loader = new RevenueRatesLoader(Factory, new DummyLogger());
				var ratingCriteria = new TestRatingCriteria();
				ratingCriteria.GatewayBillingSupporter = new GatewayBillingSupporterForTest(serviceProviderBranch, null, false);

				var headers = loader.LoadHeaders(ratingCriteria);
				AssertEquals(0, headers.Count());
			}
		}

		public void TestGetIntercompanyTariffs_ReturnEmptyListWhenSendingReceivingAgentsAreNull()
		{
			var serviceProviderCompany = Helper.NewOrgHeader();
			var serviceProviderBranch = Helper.NewOrgHeader();

			using (SetTemporaryCompanyOrgProxy(serviceProviderCompany.PK))
			using (SetTemporaryBranchOrgProxy(serviceProviderBranch.PK))
			using (SetTemporaryUseIntercompanyTariffsRegistryItem(true))
			{
				Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
				Helper.NewIntercompanyTariff(serviceProviderCompany);
				Helper.NewIntercompanyTariff(serviceProviderBranch);

				Factory.Save();

				var loader = new RevenueRatesLoader(Factory, new DummyLogger());
				var ratingCriteria = new TestRatingCriteria();
				ratingCriteria.GatewayBillingSupporter = new GatewayBillingSupporterForTest(null, null, true);

				var headers = loader.LoadHeaders(ratingCriteria);
				AssertEquals(0, headers.Count());
			}
		}

		public void TestGetIntercompanyTariffs_ReturnEmptyListWhenNothingFound()
		{
			var sendingAgent = Helper.NewOrgHeader();
			var receivingAgent = Helper.NewOrgHeader();
			var serviceProviderCompany = Helper.NewOrgHeader();
			var serviceProviderBranch = Helper.NewOrgHeader();

			using (SetTemporaryCompanyOrgProxy(serviceProviderCompany.PK))
			using (SetTemporaryBranchOrgProxy(serviceProviderBranch.PK))
			using (SetTemporaryUseIntercompanyTariffsRegistryItem(true))
			{
				Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
				Helper.NewIntercompanyTariff(serviceProviderCompany);
				Helper.NewIntercompanyTariff(serviceProviderBranch);

				Factory.Save();

				var loader = new RevenueRatesLoader(Factory, new DummyLogger());
				var ratingCriteria = new TestRatingCriteria();
				ratingCriteria.GatewayBillingSupporter = new GatewayBillingSupporterForTest(sendingAgent, receivingAgent, true);

				var headers = loader.LoadHeaders(ratingCriteria);
				AssertEquals(0, headers.Count());
			}
		}

		IDisposable SetTemporaryCompanyOrgProxy(ZGuid orgProxyPK)
		{
			var oldOrgProxyPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxyPK;

			return new DisposableAction(() => { GlbCompany.CurrentCompany.GC_OH_OrgProxy = oldOrgProxyPK; });
		}

		IDisposable SetTemporaryBranchOrgProxy(ZGuid orgProxyPK)
		{
			var oldOrgProxyPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxyPK;

			return new DisposableAction(() => { GlbBranch.CurrentBranch.GB_OH_OrgProxy = oldOrgProxyPK; });
		}

		IDisposable SetTemporaryUseIntercompanyTariffsRegistryItem(bool value) =>
			RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

		#endregion

		#region Organisation Types

		public void TestFindBestMatchWhenMatchesExistInMoreThanOneClient()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var testRateClient = Helper.NewClientRate(NewClient);
			var testRateConsignor = Helper.NewClientRate(Consignor);
			var testRateConsignee = Helper.NewClientRate(Consignee);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignee = Consignee;
			testCriteria.Consignor = Consignor;

			var entryClient = testRateClient.AddRateEntry("AIR", "LSE", "AU", "US");
			var line = entryClient.RateLines.AddNew();
			line.TL_AC = TestFRT.PK;

			var entryConsignor = testRateConsignor.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			line = entryConsignor.RateLines.AddNew();
			line.TL_AC = TestFRT.PK;

			var entryConsignee = testRateConsignee.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			line = entryConsignee.RateLines.AddNew();
			line.TL_AC = TestFRT.PK;

			Factory.Save();

			var loader = new RevenueRatesLoader(Factory, new DummyLogger());
			var ratingHeaders = loader.LoadHeaders(testCriteria).Select(RatingHeader.GetBO).ToArray();

			AssertEquals(3, ratingHeaders.Length);
			foreach (var ratingHeader in ratingHeaders)
			{
				if (ratingHeader.PK == entryClient.PK)
				{
					AssertEquals("Entry from the local client is chosen, even though it is less matching than the others - the Local Client is the first option", 1, ratingHeader.SummaryRateEntries[0].RateLines.Count);
				}
				else if (ratingHeader.PK == entryConsignor.PK)
				{
					AssertEquals("Entry from the consignor is chosen for EXPORT, prepaid charge (DDP)", 1, ratingHeader.SummaryRateEntries[0].RateLines.Count);
				}
				else if (ratingHeader.PK == entryConsignee.PK)
				{
					AssertEquals("No match found - Consignee rates are not relevant for EXPORT, prepaid charge (DDP)", 0, ratingHeader.SummaryRateEntries[0].RateLines.Count);
				}
			}
		}

		public void TestFindBestMatchWhenNoRateExistsForNewClient()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var testRateConsignor = Helper.NewClientRate(Consignor);
			var testRateConsignee = Helper.NewClientRate(Consignee);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignee = Consignee;
			testCriteria.Consignor = Consignor;

			var entryConsignor = testRateConsignor.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entryConsignor.RateLines.AddNew();
			line.TL_AC = TestFRT.PK;

			var entryConsignee = testRateConsignee.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			line = entryConsignee.RateLines.AddNew();
			line.TL_AC = TestFRT.PK;
			Factory.Save();

			var loader = new RevenueRatesLoader(Factory, new DummyLogger());
			var headers = loader.LoadHeaders(testCriteria).Select(RatingHeader.GetBO).ToList();
			AssertEquals(2, headers.Count);
			foreach (var header in headers)
			{
				if (header.PK == entryConsignor.PK)
				{
					AssertEquals("Entry from the consignor is chosen for EXPORT, prepaid charge (DDP)", 1, header.SummaryRateEntries[0].RateLines.Count);
				}
				else if (header.PK == entryConsignee.PK)
				{
					AssertEquals("No match found - Consignee rates are not relevant for EXPORT, prepaid charge (DDP)", 0, header.SummaryRateEntries[0].RateLines.Count);
				}
			}
		}

		#endregion

		#endregion

		#region Spot

		public void TestLoadSpotRates_ContainerSpotRates()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", GP20.RC_Code);

			var spotRateMoney = new Money(35m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"));
			var costSpotRateInfo = new SpotRateInfo(spotRateMoney, Constants.FreightRateAutoratingModes.Code.FreightPlusRate, AutoratedValueType.NegotiatedCost);
			var sellSpotRateInfo = new SpotRateInfo(spotRateMoney, Constants.FreightRateAutoratingModes.Code.FreightPlusRate, AutoratedValueType.SpotRate);

			var containerInfo = new MeasureInfo.ContainerInfo();
			containerInfo.SetSpotRates(costSpotRateInfo, sellSpotRateInfo, ZGuid.NewZGuid(), GP20.PK);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, testRate.Header);

			var measures = testCriteria.RateableMeasures;
			measures.AddContainerGroup(GP20.PK, [containerInfo]);

			var loader = new RevenueRatesLoader(Factory, new DummyLogger());
			var results = loader.LoadSpotRates(testCriteria);
			AssertEquals("Container spot rate returned", 1, results.Count(r => r.IsSpotEntry));
		}

		public void TestLoadSpotRates_SpotFreightRate()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 0, null, 15M, "KG", 1M, "M3", testRate.Header, ZString.Empty);

			var loader = new RevenueRatesLoader(Factory, new DummyLogger());
			var results = loader.LoadSpotRates(testCriteria).ToList();
			AssertEquals("No yet spot rates created", 0, results.Count);

			testCriteria.SetSpotRateInfo(5m, "AUD", Constants.FreightRateAutoratingModes.Code.AllInRate);
			results = loader.LoadSpotRates(testCriteria).ToList();
			AssertEquals(1, results.Count);
			AssertCollectionNotContains(entry, results);
			AssertEquals(1, results[0].ChildRateLines.Count());
			AssertEquals(UnitCalculator.Code, results[0].ChildRateLines.First().TL_RateCalculator);
			AssertEquals("LSE", results[0].TI_Mode);

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.ULD, 0, null, 0M, "KG", 0M, "M3", testRate.Header, ZString.Empty);

			testCriteria.SetSpotRateInfo(100m, "AUD", Constants.FreightRateAutoratingModes.Code.AllInRate);
			results = loader.LoadSpotRates(testCriteria).ToList();
			AssertEquals(1, results.Count);
			AssertEquals(1, results[0].ChildRateLines.Count());
			AssertEquals(UnitCalculator.Code, results[0].ChildRateLines.First().TL_RateCalculator);
			AssertEquals("ULD", results[0].TI_Mode);
			AssertEquals(RatingRoundingTypes.Chargeable, results[0].ChildRateLines.First().TL_Rounding);

			Factory.Save();
			Assert("Never saved", !(results[0] as RateEntry).IsInDatabase);
		}

		public void TestLoadSpotRates_SpotFreightRate_RoadUsesWeight()
		{
			var testCriteria = new TestRatingCriteria("USCHI", "USLAX", FreightMode.ROA, 0, null, 15M, "KG", 1M, "M3", null, ZString.Empty);

			testCriteria.SetSpotRateInfo(5m, "AUD", Core.Constants.FreightRateAutoratingModes.Code.AllInRate);
			var loader = new RevenueRatesLoader(Factory, new DummyLogger());
			var results = loader.LoadSpotRates(testCriteria).ToList();
			AssertEquals(1, results.Count);
			AssertEquals(1, results[0].ChildRateLines.Count());
			AssertEquals(UnitCalculator.Code, results[0].ChildRateLines.First().TL_RateCalculator);
			AssertEquals("ROA", results[0].TI_Mode);
		}

		#endregion
	}
}
