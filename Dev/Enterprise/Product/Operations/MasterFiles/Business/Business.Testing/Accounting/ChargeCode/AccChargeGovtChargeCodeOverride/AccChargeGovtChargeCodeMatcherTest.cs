using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeGovtChargeCodeMatcherTest : TestCaseWithFactory
	{
		public void TestGetGovtChargeCodeOverride()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE2";

			var auUnlocoA = CreateUNLOCOForTest("TEST1", Constants.CountryCodes.Australia);
			var auUnlocoB = CreateUNLOCOForTest("TEST2", Constants.CountryCodes.Australia);
			var nZUnloco = CreateUNLOCOForTest("NZ000", Constants.CountryCodes.NewZealand);
			var gBUnloco = CreateUNLOCOForTest("GB000", Constants.CountryCodes.UnitedKingdom);
			var dEUnloco = CreateUNLOCOForTest("DE000", Constants.CountryCodes.Germany);

			var override1 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, "SHP", "IMP", "ALL", "govtChargeCode1");
			var override2 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost, "SHP", "EXP", "ALL", "govtChargeCode2");
			var override3 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost, "SHP", "DOM", "ALL", "govtChargeCode3");
			var override4 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost, "SHP", "ALL", "SEA", "govtChargeCode4");
			var override5 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, "BRK", "IMP", "FIX", "govtChargeCode5");
			var override6 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, "BRK", "EXP", "SEA", "govtChargeCode6");
			var override7 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Revenue, "BRK", "DOM", "ALL", "govtChargeCode7");
			var override8 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Revenue, "BRK", "EXP", "AIR", "govtChargeCode8");

			Factory.Save();

			AssertGovtChargeCodeOverride(code, override1, CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Air, nZUnloco, auUnlocoA);
			AssertGovtChargeCodeOverride(code, override3, CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Domestic, Constants.TransportModes.Air, auUnlocoA, auUnlocoA);
			AssertGovtChargeCodeOverride(code, override4, CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.CrossTrade, Constants.TransportModes.Sea, nZUnloco, gBUnloco);
			AssertGovtChargeCodeOverride(code, override2, CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.All, auUnlocoA, gBUnloco);
			AssertGovtChargeCodeOverride(code, override7, CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Domestic, Constants.TransportModes.InlandWaterwayTransport, auUnlocoB, auUnlocoB);
			AssertGovtChargeCodeOverride(code, override6, CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Export, Constants.TransportModes.Sea, auUnlocoB, nZUnloco);
			AssertGovtChargeCodeOverride(code, null, CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.CrossTrade, Constants.TransportModes.All, gBUnloco, nZUnloco);
			AssertGovtChargeCodeOverride(code, null, CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Import, Constants.TransportModes.Sea, auUnlocoA, dEUnloco);
		}

		public void TestGetGovtChargeCodeOverride_EUN()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var code = Factory.NewWithValidTestData<AccChargeCode>();
				code.AC_Code = "CCODE1";

				var auUnloco = CreateUNLOCOForTest("AU000", Constants.CountryCodes.Australia);
				var nZUnloco = CreateUNLOCOForTest("NZ000", Constants.CountryCodes.NewZealand);
				var dEUnloco = CreateUNLOCOForTest("DE000", Constants.CountryCodes.Germany);
				var fRUnloco = CreateUNLOCOForTest("FR000", Constants.CountryCodes.France);

				var override1 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, "SHP", "IMP", "ALL", "govtChargeCode1");
				var override2 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, "SHP", "OTH", "ALL", "govtChargeCode2");
				var override3 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost, "SHP", "EXP", "SEA", "govtChargeCode3");
				var override4 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, "BRK", "EXP", "FIX", "govtChargeCode4");
				var override5 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Revenue, "BRK", "DOM", "ALL", "govtChargeCode5");
				var override6 = CreateGovtChargeCodeOverride(code, AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Revenue, "BRK", "OTH", "AIR", "govtChargeCode6");
				Factory.Save();

				AssertGovtChargeCodeOverride(code, override1, CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Air, auUnloco, dEUnloco);
				AssertGovtChargeCodeOverride(code, override3, CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.Sea, dEUnloco, auUnloco);
				AssertGovtChargeCodeOverride(code, override2, CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.CrossTrade, Constants.TransportModes.Air, fRUnloco, auUnloco);
				AssertGovtChargeCodeOverride(code, override6, CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Import, Constants.TransportModes.Air, auUnloco, dEUnloco);
				AssertGovtChargeCodeOverride(code, override5, CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Domestic, Constants.TransportModes.Air, dEUnloco, dEUnloco);
				AssertGovtChargeCodeOverride(code, null, CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.CrossTrade, Constants.TransportModes.FixedTransportInstallations, nZUnloco, auUnloco);
				AssertGovtChargeCodeOverride(code, override4, CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Export, Constants.TransportModes.FixedTransportInstallations, dEUnloco, fRUnloco);
			}
		}

		void AssertGovtChargeCodeOverride(AccChargeCode code, AccChargeGovtChargeCodeOverride expectedGovtChargeCode, CostSell costOrSell, ZString jobType, Directions direction, ZString transportMode, ILocation origin, ILocation destination)
		{
			var result = code.GovtChargeCodeOverrides.GetGovtChargeCode(Factory, code.PK, ConfigurationMatcherHelper.GetParameters(costOrSell, jobType, direction, transportMode, GlbBranch.CurrentBranch, origin, destination));
			if (expectedGovtChargeCode == null)
			{
				AssertNull(result?.ACG_GovtChargeCode);
			}
			else
			{
				AssertNotNull($"Expected Govt Charge Code'{expectedGovtChargeCode.ACG_GovtChargeCode}', but actual Govt Charge Code was null", result.ACG_GovtChargeCode);
				AssertEquals("Govt Charge Code", expectedGovtChargeCode.ACG_GovtChargeCode, result.ACG_GovtChargeCode);
			}
		}

		AccChargeGovtChargeCodeOverride CreateGovtChargeCodeOverride(AccChargeCode chargeCode, ZString costSellAll, ZString jobType, ZString direction, ZString transportMode, ZString govtChargeCode)
		{
			AccChargeGovtChargeCodeOverride @override = chargeCode.GovtChargeCodeOverrides.AddNew();
			@override.ACG_CostSellAll = costSellAll;
			@override.ACG_Direction = direction;
			@override.ACG_JobType = jobType;
			@override.ACG_TransportMode = transportMode;
			@override.ACG_GovtChargeCode = govtChargeCode;
			return @override;
		}

		RefUNLOCO CreateUNLOCOForTest(ZString uNLOCOCode, ZString countryCode)
		{
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = uNLOCOCode;

			RefCountry country = RefCountry.LoadFromCountryCode(Factory, countryCode);
			uNLOCO.RL_RN_NKCountryCode = country.Code;

			return uNLOCO;
		}
	}
}
