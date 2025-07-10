using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Testing.GUI
{
	internal class TariffLevelIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestAutoRatingTariffLevels_MultipleLevels()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			shipment.JS_E_DEP = ZDate.Today;
			Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, "LSE", "USLAX", "AUBNE", "FRT", 100m);
			Helper.NewNonLevel1CompanyTariff(2, RatingConstants.RateCategory.AIR, 10m);

			var level1 = localClient.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", ZDate.Empty, ZDate.Empty, 1);
			var level2 = localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Empty, ZDate.Empty, 2);
			Factory.Save();

			TestCase("DEF", "ALL", "ALL", "FRT", "ALL", "ALL", ExpectedCharge("FRT", 90m, 2));
			TestCase("FRT", "ALL", "ALL", "DEF", "ALL", "ALL", ExpectedCharge("FRT", 100m, 1));
			TestCase("FRT", "ALL", "ALL", "FRT", "IMP", "ALL", ExpectedCharge("FRT", 90m, 2));
			TestCase("FRT", "ALL", "ALL", "FRT", "ALL", "LSE", ExpectedCharge("FRT", 90m, 2));
			TestCase("FRT", "IMP", "ALL", "FRT", "ALL", "LSE", ExpectedCharge("FRT", 90m, 2));
			TestCase("FRT", "ALL", "AIR", "FRT", "ALL", "LSE", ExpectedCharge("FRT", 90m, 2));
			TestCase("FRT", "ALL", "LSE", "FRT", "IMP", "LSE", ExpectedCharge("FRT", 90m, 2));

			// Second level not relevant
			TestCase("DEF", "ALL", "ALL", "ORG", "ALL", "ALL", ExpectedCharge("FRT", 100m, 1));
			TestCase("DEF", "ALL", "ALL", "FRT", "EXP", "ALL", ExpectedCharge("FRT", 100m, 1));
			TestCase("DEF", "ALL", "ALL", "FRT", "ALL", "SEA", ExpectedCharge("FRT", 100m, 1));

			void TestCase(string type, string direction, string mode, string type2, string direction2, string mode2, AssertionCharge[] expectedCharges)
			{
				level1.P7_TariffType = type;
				level1.P7_Direction = direction;
				level1.P7_Mode = mode;

				level2.P7_TariffType = type2;
				level2.P7_Direction = direction2;
				level2.P7_Mode = mode2;
				Factory.Save();

				AutorateAndAssert("Should load the correct rate per level", expectedCharges, shipment, localClient);
			}
		}

		public void TestAutoRatingTariffLevels_UsingDepartureDate()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, "LSE", "USLAX", "AUBNE", "FRT", 100m);
			Helper.NewNonLevel1CompanyTariff(2, RatingConstants.RateCategory.AIR, 10m);

			localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Today.AddDays(2), ZDate.Empty, 1);
			localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Empty, ZDate.Today.AddDays(1), 2);
			Factory.Save();

			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			AutorateAndAssert("Should load the correct rate based on shipments departure date", ExpectedCharge("FRT", 90m, 2), shipment, localClient);

			shipment.JS_E_DEP = ZDate.Today.AddDays(2);
			AutorateAndAssert("Should load the correct rate based on shipments departure date", ExpectedCharge("FRT", 100m, 1), shipment, localClient);
		}

		public void TestAutoRatingTariffLevels_NoDate_DefaultsToToday()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);

			Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, "LSE", "USLAX", "AUBNE", "FRT", 100m);
			Helper.NewNonLevel1CompanyTariff(2, RatingConstants.RateCategory.AIR, 10m);

			localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Today.AddDays(1), ZDate.Empty, 1);
			localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Empty, ZDate.Today, 2);
			Factory.Save();

			AutorateAndAssert("Should use todays date when no date provided in shipment", ExpectedCharge("FRT", 90m, 2), shipment, localClient);
		}

		public void TestAutoRatingTariffLevels_UsingArrivalDate()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			shipment.JS_E_DEP = ZDate.Today.AddDays(-5);

			Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.DST, "LSE", "USLAX", "AUBNE", "DPCH", 100m);
			Helper.NewNonLevel1CompanyTariff(2, RatingConstants.RateCategory.DST, 10m);

			localClient.CompanyData.RateTariffLevels.AddLevel("DST", "ALL", "ALL", ZDate.Today.AddDays(2), ZDate.Empty, 1);
			localClient.CompanyData.RateTariffLevels.AddLevel("DST", "ALL", "ALL", ZDate.Empty, ZDate.Today.AddDays(1), 2);
			Factory.Save();

			shipment.JS_E_ARV = ZDate.Today.AddDays(1);
			AutorateAndAssert("Should load the correct rate based on shipments arrival date", ExpectedCharge("DPCH", 90m, 2), shipment, localClient);

			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			AutorateAndAssert("Should load the correct rate based on shipments arrival date", ExpectedCharge("DPCH", 100m, 1), shipment, localClient);
		}

		public void TestAutoRatingTariffLevels_ConsolAttached()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);

			Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, "LSE", "USLAX", "AUBNE", "FRT", 100m);
			Helper.NewNonLevel1CompanyTariff(2, RatingConstants.RateCategory.AIR, 10m);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";

			localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Today.AddDays(1), ZDate.Empty, 1);
			localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Empty, ZDate.Today, 2);
			Factory.Save();

			consol.Transports[0].JW_ETD = ZDate.Today;
			AutorateAndAssert("Should use the consols departure date to find relevant levels", ExpectedCharge("FRT", 90m, 2), shipment, localClient);

			consol.Transports[0].JW_ETD = ZDate.Today.AddDays(1);
			AutorateAndAssert("Should use the consols departure date to find relevant levels", ExpectedCharge("FRT", 100m, 1), shipment, localClient);
		}

		public void TestAutoRatingTariffLevels_RegistrySetToUseArrivalDate()
		{
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Arrival,
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "USLAX", "AUBNE", 100);
			Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, "LSE", "USLAX", "AUBNE", "FRT", 100m);
			Helper.NewNonLevel1CompanyTariff(2, RatingConstants.RateCategory.AIR, 10m);

			localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Today.AddDays(2), ZDate.Empty, 1);
			localClient.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Empty, ZDate.Today.AddDays(1), 2);
			Factory.Save();

			shipment.JS_E_ARV = ZDate.Today.AddDays(1);
			AutorateAndAssert("Should load the correct rate based on shipments departure date", ExpectedCharge("FRT", 90m, 2), shipment, localClient);

			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			AutorateAndAssert("Should load the correct rate based on shipments departure date", ExpectedCharge("FRT", 100m, 1), shipment, localClient);
		}

		#region Helpers

		static AssertionCharge[] ExpectedCharge(string chargeCode, decimal amount, int level)
		{
			return new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode,
					JR_OSSellAmt = amount,
					RevenueCalculationDescription = ZString.Format("Charge located in Company Tariff Level {0} (Linked to: TESTORG1) with the following details:", level),
				},
			};
		}

		#endregion
	}
}
