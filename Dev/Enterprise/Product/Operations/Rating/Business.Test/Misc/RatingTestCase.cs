using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;
using WiseRates.Constants;
using static Enterprise.Core.Constants;
using API = WiseRates.Api.Model;
using DTO = WiseRates.Api.Model;
using Rate = WiseRates.Api.Model.Rate;
using RatesSearchResponse = WiseRates.Api.Model.RatesSearchResponse;
using RatesServiceCharge = WiseRates.Api.Model.Charge;
using RefCarrier = WiseRates.Api.Model.RefCarrier;
using RefChargeCode = WiseRates.Api.Model.RefChargeCode;

namespace Enterprise.Rating.Business.Testing
{
	public abstract class RatingTestCase : TestCaseWithFactory
	{
		protected void AssertRatingResults(IEnumerable<SimpleArInfo> expected, AutoRateResult actual)
		{
			AssertRatingResults(expected, actual.RateInfoCollection);
		}

		protected void AssertRatingResults(string message, IEnumerable<SimpleArInfo> expected, AutoRateResult actual)
		{
			AssertRatingResults(message, expected, actual.RateInfoCollection);
		}

		protected void AssertRatingResults(IEnumerable<SimpleArInfo> expected, AutoRateInfoCollection actual)
		{
			AssertRatingResults("", expected, actual);
		}

		protected void AssertRatingResults(string message, IEnumerable<SimpleArInfo> expected, AutoRateInfoCollection actual)
		{
			AssertContainsExactElementsInAnyOrder(
				message,
				SimpleArInfo.Comparer,
				expected,
				actual.Select(x => new SimpleArInfo(x)));
		}

		#region RateEntryFilterStripBusinessObjectForTest

		protected class RateEntryFilterStripBusinessObjectForTest : IRateEntryFilterStripBusinessObject
		{
			public RateEntryFilterStripBusinessObjectForTest(ZQuery filter)
			{
				Filter = filter;
			}

			public ZQuery Filter { get; }

			public void ClearRateEntryFilterStrips()
			{
			}

			public ZQuery RateLineFilter => new ZQuery();
		}

		#endregion

		#region Organisations

		#region New Client

		OrgHeader fNewClient;
		public OrgHeader NewClient
		{
			get
			{
				if (fNewClient == null)
				{
					fNewClient = Factory.New<OrgHeader>();
					fNewClient.OH_Code = "NTC1";
					fNewClient.OH_FullName = "New Test Client";
					fNewClient.OH_RL_NKClosestPort = "AUSYD";

					fNewClient.OH_IsDebtor = true;
					fNewClient.OH_IsConsignor = true;
					fNewClient.OH_IsConsignee = true;

					var address = fNewClient.MainAddress;
					address.OA_Address1 = "123 Fake Street";
					address.OA_City = "Sydney";
					address.OA_State = "NSW";
					address.OA_PostCode = "2000";
				}
				return fNewClient;
			}
		}

		#endregion

		#region New Client 2

		OrgHeader fNewClient2;
		public OrgHeader NewClient2
		{
			get
			{
				if (fNewClient2 == null)
				{
					fNewClient2 = Factory.New<OrgHeader>();
					fNewClient2.OH_Code = "NTC2";
					fNewClient2.OH_FullName = "New Test Client 2";
					fNewClient2.MainAddress.OA_Address1 = "123 Fake Street";
					fNewClient2.MainAddress.OA_City = "Sydney";
					fNewClient2.MainAddress.OA_State = "NSW";
					fNewClient2.MainAddress.OA_PostCode = "2000";
					fNewClient2.OH_RL_NKClosestPort = "AUSYD";
				}
				return fNewClient2;
			}
		}

		#endregion

		#region New Transport Provider 1

		OrgHeader fTransportProvider1;
		public OrgHeader TransportProvider1
		{
			get
			{
				if (fTransportProvider1 == null)
				{
					fTransportProvider1 = Factory.New<OrgHeader>();
					fTransportProvider1.OH_FullName = "Transport Provider 1";
					fTransportProvider1.MainAddress.OA_Address1 = "123 Fake Street";
					fTransportProvider1.MainAddress.OA_City = "Sydney";
					fTransportProvider1.MainAddress.OA_State = "NSW";
					fTransportProvider1.MainAddress.OA_PostCode = "2000";
					fTransportProvider1.OH_RL_NKClosestPort = "AUSYD";
					fTransportProvider1.OH_Code = "TRASPROV1";
					fTransportProvider1.OH_IsShippingProvider = true;

					var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
					shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
					fTransportProvider1.OH_RSL_ShippingLine = shippingLine.PK;
					Factory.Save();
				}

				return fTransportProvider1;
			}
		}

		#endregion

		#region New Transport Provider 2

		OrgHeader fTransportProvider2;
		public OrgHeader TransportProvider2
		{
			get
			{
				if (fTransportProvider2 == null)
				{
					fTransportProvider2 = Factory.New<OrgHeader>();
					fTransportProvider2.OH_FullName = "Transport Provider 2";
					fTransportProvider2.MainAddress.OA_Address1 = "123 Fake Street";
					fTransportProvider2.MainAddress.OA_City = "Sydney";
					fTransportProvider2.MainAddress.OA_State = "NSW";
					fTransportProvider2.MainAddress.OA_PostCode = "2000";
					fTransportProvider2.OH_RL_NKClosestPort = "AUSYD";
					fTransportProvider2.OH_Code = "TRASPROV2";
					Factory.Save();
				}
				return fTransportProvider2;
			}
		}

		#endregion

		#region Emirates Airlines

		OrgHeader emiratesAirlines;
		public OrgHeader EmiratesAirlines
		{
			get
			{
				if (emiratesAirlines == null)
				{
					var airline = Factory.NewWithValidTestData<RefAirline>();
					airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "9XX";
					airline.RM_ThreeLetterCode = "EKK";
					airline.RM_TwoCharacterCode = "EK";

					emiratesAirlines = Factory.NewWithValidTestData<OrgHeader>();
					emiratesAirlines.OH_FullName = "Emirates Airlines";
					emiratesAirlines.MainAddress.OA_City = "Dubai";
					emiratesAirlines.OH_RL_NKClosestPort = "AEDXB";
					emiratesAirlines.OH_IsShippingProvider = true;
					emiratesAirlines.OH_IsCreditor = true;
					emiratesAirlines.MiscServ.OM_RM_Airline = airline.PK;

					Factory.Save();
				}

				return emiratesAirlines;
			}
		}

		#endregion

		#region New Consignee

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

		#endregion

		#region New Consignor

		OrgHeader fConsignor;
		public OrgHeader Consignor
		{
			get
			{
				if (fConsignor == null)
				{
					fConsignor = Factory.NewWithValidTestData<OrgHeader>();
					fConsignor.OH_FullName = "Consignor";
					fConsignor.OH_RL_NKClosestPort = "AUSYD";
					fConsignor.OH_Code = "CONSIGNOR1";
					fConsignor.OH_IsConsignor = true;
				}

				return fConsignor;
			}
		}

		#endregion

		#endregion

		#region Containers

		RefContainer fGP20;
		public RefContainer GP20
		{
			get
			{
				if (fGP20 == null)
				{
					fGP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
					fGP20.RC_Description = "TWENTY FOOT GENERAL PURPOSE";
				}
				return fGP20;
			}
		}

		RefContainer fGP40;
		public RefContainer GP40
		{
			get
			{
				if (fGP40 == null)
				{
					fGP40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
				}
				return fGP40;
			}
		}

		#endregion

		public bool AssertContainsErrorMesg(ZString errorMesg, NotificationBuffer buffer)
		{
			var result = false;

			foreach (var notification in buffer.Events)
			{
				if (notification.Message == errorMesg) // test code only so error string ok
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#region Charge Codes

		protected void InsertClientChargeCodesForAutoRaterTests(BusinessObjectFactory factory)
		{
			TestFRT = InsertChargeCode(factory, "TESTFRT", "Test Freight", CombinedCalculator.Code, "FRT");
			TestBAF = InsertChargeCode(factory, "TESTBAF", "Test BAF", PercentageCalculator.Code, "FRT");
			TestCAF = InsertChargeCode(factory, "TESTCAF", "Test CAF", PercentageCalculator.Code, "FRT");
			TestWAR = InsertChargeCode(factory, "TESTWAR", "Test WAR", PercentageCalculator.Code, "FRT");
			TestFSC = InsertChargeCode(factory, "TESTFSC", "Test FSC", PercentageCalculator.Code, "FRT");

			TestAWB = InsertChargeCode(factory, "TESTAWB", "Test Airway Bill Fee", FlatCalculator.Code, "ORG");
			TestBBK = InsertChargeCode(factory, "TESTBBK", "Test Breakbulk", CombinedCalculator.Code, "ORG");
			TestTHC = InsertChargeCode(factory, "TESTTHC", "Test Terminal Handling Charge", FlatCalculator.Code, "ORG");
			TestANY = InsertChargeCode(factory, "TESTANY", "Test ANY Charge Code", FlatCalculator.Code, "ORG");
			TestSEA = InsertChargeCode(factory, "TESTSEA", "Test SEA Charge Code", UnitCalculator.Code, "ORG");

			TestADF = InsertChargeCode(factory, "TESTADF", "Test Airline Doc Fee", FlatCalculator.Code, "DST");
			TestITF = InsertChargeCode(factory, "TESTITF", "Test Intl Terminal Fee", MinimumOrPerUnitCalculator.Code, "DST");
			TestLOL = InsertChargeCode(factory, "TESTLOL", "Test Lift On/Lift Off", FlatPlusPerUnitCalculator.Code, "DST");
			TestPSC = InsertChargeCode(factory, "TESTPSC", "Test Port Services Charge", UnitCalculator.Code, "DST");
			TestCTG = InsertChargeCode(factory, "TESTCTG", "Test Cartage Charge Code", CartageCalculator.Code, "DST");
		}

		void InsertGlobalChargeCodesForAutoRaterTests(BusinessObjectFactory factory)
		{
			TestGL1 = InsertChargeCode(factory, "TESTGL1", "Test Global 1", FlatCalculator.Code, "ORG");
			TestGL2 = InsertChargeCode(factory, "TESTGL2", "Test Global 2", FlatCalculator.Code, "ORG");
			TestGL3 = InsertChargeCode(factory, "TESTGL3", "Test Global 3", UnitCalculator.Code, "DST");
			TestGL4 = InsertChargeCode(factory, "TESTGL4", "Test Global 4", UnitCalculator.Code, "DST");
			TestGL5 = InsertChargeCode(factory, "TESTGL5", "Test Global 5", FlatCalculator.Code, "DST");
		}

		protected AccChargeCode InsertChargeCode(BusinessObjectFactory factory, ZString code, ZString description, ZString calculatorCode, ZString chargeGroup, ZString universalChargeCode = default, bool isGlobal = false, bool isConsoleCharge = true)
		{
			AccChargeCode newChargeCode;
			if (isGlobal)
			{
				newChargeCode = Helper.ChargeCodes.CreateGlobalCharge(code, calculatorCode, chargeGroup);
			}
			else
			{
				newChargeCode = factory.New<AccChargeCode>();
				newChargeCode.AC_Code = code;
				newChargeCode.AC_ChargeGroup = chargeGroup;
				newChargeCode.AC_RateCalculator = calculatorCode;
				newChargeCode.AC_GC = Env.CurrentCompanyPK;
				newChargeCode.AC_IsGroupageCharge = isConsoleCharge;
			}

			newChargeCode.AC_Desc = description;
			newChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			if (!string.IsNullOrWhiteSpace(universalChargeCode))
			{
				var mapping = newChargeCode.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Code = universalChargeCode;
			}

			return newChargeCode;
		}

		protected AccChargeCode TestFRT;
		protected AccChargeCode TestBAF;
		protected AccChargeCode TestCAF;
		protected AccChargeCode TestWAR;
		protected AccChargeCode TestFSC;

		protected AccChargeCode TestADF;
		protected AccChargeCode TestAWB;
		protected AccChargeCode TestBBK;
		protected AccChargeCode TestITF;
		protected AccChargeCode TestLOL;
		protected AccChargeCode TestPSC;
		protected AccChargeCode TestTHC;
		protected AccChargeCode TestANY;
		protected AccChargeCode TestSEA;
		protected AccChargeCode TestGL1;
		protected AccChargeCode TestGL2;
		protected AccChargeCode TestGL3;
		protected AccChargeCode TestGL4;
		protected AccChargeCode TestGL5;

		protected AccChargeCode TestCTG;

		#endregion

		#region Exchange Rates

		public void AddParityExchangeRate(RefCurrency currency)
		{
			if (currency != null && currency.Code != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				Helper.NewExchangeRate(currency, ExchangeRateTypes.Code.BuyRate, 1);
				Helper.NewExchangeRate(currency, ExchangeRateTypes.Code.SellRate, 1);
			}
		}

		#endregion

		#region Export Client Rates

		protected ClientRate SetupExportClientRatesForAutoRater(bool setupFreight = true)
		{
			var testRate = Factory.New<ClientRate>();
			InsertClientChargeCodesForAutoRaterTests(Factory);
			testRate.TH_OH = NewClient.PK;
			testRate.Header.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			if (setupFreight)
			{
				#region AIR Entries

				var aIRRateEntry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
				aIRRateEntry1.RateLines.RemoveAndDeleteAll();
				var aIRRateLine1a = aIRRateEntry1.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "KG");
				aIRRateLine1a.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
				aIRRateLine1a.Calculator["-45"] = (ZDecimal)4.5m;
				aIRRateLine1a.Calculator["+45"] = (ZDecimal)4.0m;
				aIRRateLine1a.Calculator["+100"] = (ZDecimal)3.5m;
				aIRRateLine1a.Calculator["+250"] = (ZDecimal)3.0m;
				aIRRateLine1a.Calculator["+500"] = (ZDecimal)2.5m;
				aIRRateLine1a.Calculator["+1000"] = (ZDecimal)2.0m;

				var aIRRateLine1b = aIRRateEntry1.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
				aIRRateLine1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				aIRRateLine1b.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 13.62M;

				var aIRRateLine1c = aIRRateEntry1.AddRateLine(TestCAF.AC_Code, PercentageCalculator.Code);
				aIRRateLine1c.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				aIRRateLine1c.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 8.75M;

				var aIRRateEntry2 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "D2D", "");
				aIRRateEntry2.RateLines.RemoveAndDeleteAll();
				aIRRateEntry2.TI_OH_TransportProvider = TransportProvider1.PK;

				var aIRRateLine2a = aIRRateEntry2.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "KG");
				aIRRateLine2a.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
				aIRRateLine2a.Calculator["-45"] = (ZDecimal)4.5m;
				aIRRateLine2a.Calculator["+45"] = (ZDecimal)4.0m;
				aIRRateLine2a.Calculator["+100"] = (ZDecimal)3.5m;
				aIRRateLine2a.Calculator["+250"] = (ZDecimal)3.0m;
				aIRRateLine2a.Calculator["+500"] = (ZDecimal)2.5m;
				aIRRateLine2a.Calculator["+1000"] = (ZDecimal)2.0m;

				var aIRRateLine2b = aIRRateEntry2.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
				aIRRateLine2b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				aIRRateLine2b.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 13.62M;

				var aIRRateLine2c = aIRRateEntry2.AddRateLine(TestCAF.AC_Code, PercentageCalculator.Code);
				aIRRateLine2c.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				aIRRateLine2c.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 8.75M;

				// Entry Not Used in Testing - Setup to test ClientRate search in the scope of the AutoRater
				var aIRRateEntry3 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");

				var aIRRateEntry4 = testRate.AddRateEntry("AIR", "ULD", "AUSYD", "USLAX", "STD", "20GP");
				aIRRateEntry4.RateLines.RemoveAndDeleteAll();
				var aIRRateLine4a = aIRRateEntry4.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "KG");
				aIRRateLine4a.TL_RateCalculator = "UNT";
				aIRRateLine4a.TL_WeightVolume = "CN";
				aIRRateLine4a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)150m;

				#endregion

				#region LCL Entries

				var lCLRateEntry1 = testRate.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON", "STD", "");
				lCLRateEntry1.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
				lCLRateEntry1.RateLines.RemoveAndDeleteAll();
				var lCLRateLine1a = lCLRateEntry1.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "M3");
				lCLRateLine1a.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)150m;
				lCLRateLine1a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)125m;

				var lCLRateLine1b = lCLRateEntry1.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
				lCLRateLine1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				lCLRateLine1b.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 13.62M;

				var lCLRateLine1c = lCLRateEntry1.AddRateLine(TestCAF.AC_Code);
				lCLRateLine1c.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				lCLRateLine1c.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 8.75M;

				// Entry Not Used in Testing - Setup to test ClientRate search in the scope of the AutoRater
				var lCLRateEntry2 = testRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX", "STD", "");
				lCLRateEntry2.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
				lCLRateEntry2.RateLines.RemoveAndDeleteAll();
				var lCLRateLine2a = lCLRateEntry2.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "M3");
				lCLRateLine2a.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)150m;
				lCLRateLine2a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)110m;

				#endregion

				#region FCL Entries

				var fCLRateEntry1 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
				fCLRateEntry1.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
				fCLRateEntry1.RateLines.RemoveAndDeleteAll();
				var fCLRateLine1a = fCLRateEntry1.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, QuantityUnit.CN);
				fCLRateLine1a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2500m;

				var fCLRateLine1b = fCLRateEntry1.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
				fCLRateLine1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				fCLRateLine1b.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 13.62M;

				var fCLRateLine1c = fCLRateEntry1.AddRateLine(TestCAF.AC_Code, PercentageCalculator.Code);
				fCLRateLine1c.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				fCLRateLine1c.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 8.75M;

				var fCLRateEntry2 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
				fCLRateEntry2.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
				fCLRateEntry2.RateLines.RemoveAndDeleteAll();
				var fCLRateLine2a = fCLRateEntry2.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, QuantityUnit.CN);
				fCLRateLine2a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4000m;

				var fCLRateLine2b = fCLRateEntry2.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
				fCLRateLine2b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				fCLRateLine2b.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 13.62M;

				var fCLRateLine2c = fCLRateEntry2.AddRateLine(TestCAF.AC_Code, PercentageCalculator.Code);
				fCLRateLine2c.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
				fCLRateLine2c.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 8.75M;

				// Entry Not Used in Testing - Setup to test ClientRate search in the scope of the AutoRater
				var fCLRateEntry3 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "40GP");

				#endregion
			}

			#region ORG Entries

			var oRGRateEntry1 = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			var oRGRateLine1a = oRGRateEntry1.AddRateLine(TestAWB.AC_Code, FlatCalculator.Code);
			oRGRateLine1a.GetCalculator<FlatCalculator>().BaseRate = 50M;

			var oRGRateLine1b = oRGRateEntry1.AddRateLine(TestBBK.AC_Code, CombinedCalculator.Code, "KG");
			oRGRateLine1b.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)10m;
			oRGRateLine1b.Calculator["-45"] = (ZDecimal).50m;
			oRGRateLine1b.Calculator["+45"] = (ZDecimal).45m;
			oRGRateLine1b.Calculator["+100"] = (ZDecimal).40m;
			oRGRateLine1b.Calculator["+250"] = (ZDecimal).35m;

			var oRGRateLine1c = oRGRateEntry1.AddRateLine(TestTHC.AC_Code, FlatCalculator.Code);
			oRGRateLine1c.GetCalculator<FlatCalculator>().BaseRate = 18.50M;

			var oRGRateEntry2 = testRate.AddRateEntry("ORG", "LCL", "AUSYD", "", "", "");
			var oRGRateLine2a = oRGRateEntry2.AddRateLine(TestAWB.AC_Code, FlatCalculator.Code);
			oRGRateLine2a.GetCalculator<FlatCalculator>().BaseRate = 50M;

			var oRGRateLine2b = oRGRateEntry2.AddRateLine(TestBBK.AC_Code, CombinedCalculator.Code, "M3");
			oRGRateLine2b.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			oRGRateLine2b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)15m;

			var oRGRateLine2c = oRGRateEntry2.AddRateLine(TestTHC.AC_Code, FlatCalculator.Code);
			oRGRateLine2c.GetCalculator<FlatCalculator>().BaseRate = 18.50M;

			var oRGRateEntry3 = testRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			var oRGRateLine3a = oRGRateEntry3.AddRateLine(TestAWB.AC_Code, FlatCalculator.Code, "CN");
			oRGRateLine3a.GetCalculator<FlatCalculator>().BaseRate = 50M;

			var oRGRateLine3b = oRGRateEntry3.AddRateLine(TestBBK.AC_Code, CombinedCalculator.Code, "CN");
			oRGRateLine3b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var oRGRateLine3c = oRGRateEntry3.AddRateLine(TestTHC.AC_Code, FlatCalculator.Code, "CN");
			oRGRateLine3c.GetCalculator<FlatCalculator>().BaseRate = 18.50M;

			var oRGRateEntry4 = testRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "40GP");
			var oRGRateLine4a = oRGRateEntry4.AddRateLine(TestAWB.AC_Code, FlatCalculator.Code, "CN");
			oRGRateLine4a.GetCalculator<FlatCalculator>().BaseRate = 50M;

			var oRGRateLine4b = oRGRateEntry4.AddRateLine(TestBBK.AC_Code, CombinedCalculator.Code, "CN");
			oRGRateLine4b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)175m;

			var oRGRateLine4c = oRGRateEntry4.AddRateLine(TestTHC.AC_Code, FlatCalculator.Code, "CN");
			oRGRateLine4c.GetCalculator<FlatCalculator>().BaseRate = 18.50M;

			// Entry Not Used in Testing - Setup to test ClientRate search in the scope of the AutoRater
			var oRGRateEntry5 = testRate.AddRateEntry("ORG", "FCL", "GBLON", "", "", "40GP");

			var oRGRateEntry6 = testRate.AddRateEntry("ORG", "ULD", "AUSYD", "", "", "20GP");
			var oRGRateLine6a = oRGRateEntry6.AddRateLine(TestAWB.AC_Code, FlatCalculator.Code, RatingConstants.Units.CN);
			oRGRateLine6a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)350M;

			#endregion

			#region DST Entries

			var dSTRateEntry1 = testRate.AddRateEntry("DST", "AIR", "", "USLAX", "", "");
			var dSTRateLine1a = dSTRateEntry1.AddRateLine(TestADF.AC_Code, FlatCalculator.Code);
			dSTRateLine1a.GetCalculator<FlatCalculator>().BaseRate = 50M;

			var dSTRateLine1b = dSTRateEntry1.AddRateLine(TestITF.AC_Code, MinimumOrPerUnitCalculator.Code, "KG");
			dSTRateLine1b.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value = 25M;
			dSTRateLine1b.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 1.5M;

			var dSTRateLine1c = dSTRateEntry1.AddRateLine(TestLOL.AC_Code, FlatPlusPerUnitCalculator.Code, "KG");
			dSTRateLine1c.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value = 5.0M;
			dSTRateLine1c.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = .4M;

			var dSTRateEntry2 = testRate.AddRateEntry("DST", "LCL", "", "GBLON", "", "");
			var dSTRateLine2a = dSTRateEntry2.AddRateLine(TestPSC.AC_Code, UnitCalculator.Code, "M3");
			dSTRateLine2a.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 2.5M;

			var dSTRateLine2b = dSTRateEntry2.AddRateLine(TestITF.AC_Code, MinimumOrPerUnitCalculator.Code, "M3");
			dSTRateLine2b.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value = 100M;
			dSTRateLine2b.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 45M;

			var dSTRateLine2c = dSTRateEntry2.AddRateLine(TestLOL.AC_Code, FlatPlusPerUnitCalculator.Code, "M3");
			dSTRateLine2c.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value = 25M;
			dSTRateLine2c.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 35M;

			var dSTRateEntry3 = testRate.AddRateEntry("DST", "FCL", "", "USLAX", "", "20GP");
			var dSTRateLine3a = dSTRateEntry3.AddRateLine(TestPSC.AC_Code, UnitCalculator.Code, "CN");
			dSTRateLine3a.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 150M;

			var dSTRateEntry4 = testRate.AddRateEntry("DST", "FCL", "", "USLAX", "", "40GP");
			var dSTRateLine4a = dSTRateEntry4.AddRateLine(TestPSC.AC_Code, UnitCalculator.Code, "CN");
			dSTRateLine4a.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 250M;

			// Entry Not Used in Testing - Setup to test ClientRate search in the scope of the AutoRater
			var dSTRateEntry5 = testRate.AddRateEntry("DST", "FCL", "", "GBLON", "", "40GP");

			#endregion

			Factory.Save();
			return testRate;
		}

		protected ClientRate SetupExportClientRatesForAutoRaterWithPercentageOfAllCharges()
		{
			var testRate = Factory.New<ClientRate>();
			InsertClientChargeCodesForAutoRaterTests(testRate.SummaryRateEntries.Factory);
			testRate.TH_OH = NewClient.PK;
			testRate.Header.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			#region LCL Entries

			var lCLRateEntry1 = testRate.AddRateEntry("LCL", "LCL", "AUPER", "GBLON", "STD", "");
			lCLRateEntry1.RateLines.RemoveAndDeleteAll();
			lCLRateEntry1.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			var lCLRateLine1a = lCLRateEntry1.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "M3");
			lCLRateLine1a.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)150m;
			lCLRateLine1a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)125m;

			var lCLRateLine1b = lCLRateEntry1.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
			lCLRateLine1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.AllCharges);
			lCLRateLine1b.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 1M;

			#endregion

			#region ORG Entries

			var oRGRateEntry2 = testRate.AddRateEntry("ORG", "LCL", "AUPER", "", "", "");
			var oRGRateLine2a = oRGRateEntry2.AddRateLine(TestAWB.AC_Code, FlatCalculator.Code);
			oRGRateLine2a.GetCalculator<FlatCalculator>().BaseRate = 50M;

			#endregion

			Factory.Save();
			return testRate;
		}

		#endregion

		#region Company Tariff

		protected void SetupGlobalTariffForAutoRater()
		{
			var testGlobal = Factory.New<CompanyTariff>();
			InsertGlobalChargeCodesForAutoRaterTests(Factory);

			#region AIR Entries

			var aIRRateEntry1 = testGlobal.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			aIRRateEntry1.RateLines.RemoveAndDeleteAll();

			var aIRRateLine1a = aIRRateEntry1.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "KG");
			aIRRateLine1a.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			aIRRateLine1a.Calculator["-45"] = (ZDecimal)4.5m;
			aIRRateLine1a.Calculator["+45"] = (ZDecimal)4.0m;
			aIRRateLine1a.Calculator["+100"] = (ZDecimal)3.5m;
			aIRRateLine1a.Calculator["+250"] = (ZDecimal)3.0m;
			aIRRateLine1a.Calculator["+500"] = (ZDecimal)2.5m;
			aIRRateLine1a.Calculator["+1000"] = (ZDecimal)2.0m;

			var aIRRateLine1b = aIRRateEntry1.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
			aIRRateLine1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
			aIRRateLine1b.GetCalculator<PercentageCalculator>().Percent = 13.62m;

			var aIRRateLine1c = aIRRateEntry1.AddRateLine(TestCAF.AC_Code, PercentageCalculator.Code);
			aIRRateLine1c.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
			aIRRateLine1c.GetCalculator<PercentageCalculator>().Percent = 8.75m;

			var aIRRateEntry2 = testGlobal.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "D2D", "");
			aIRRateEntry2.RateLines.RemoveAndDeleteAll();

			var aIRRateLine2a = aIRRateEntry2.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "KG");
			aIRRateLine2a.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			aIRRateLine2a.Calculator["-45"] = (ZDecimal)4.5m;
			aIRRateLine2a.Calculator["+45"] = (ZDecimal)4.0m;
			aIRRateLine2a.Calculator["+100"] = (ZDecimal)3.5m;
			aIRRateLine2a.Calculator["+250"] = (ZDecimal)3.0m;
			aIRRateLine2a.Calculator["+500"] = (ZDecimal)2.5m;
			aIRRateLine2a.Calculator["+1000"] = (ZDecimal)2.0m;

			var aIRRateLine2b = aIRRateEntry2.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
			aIRRateLine2b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
			aIRRateLine2b.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 13.62M;

			var aIRRateLine2c = aIRRateEntry2.AddRateLine(TestCAF.AC_Code, PercentageCalculator.Code);
			aIRRateLine2c.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = TestFRT.PK;
			aIRRateLine2c.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 8.75M;

			// Entry Not Used in Testing - Setup to test ClientRate search in the scope of the AutoRater
			var aIRRateEntry3 = testGlobal.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");

			#endregion

			#region ORG Entries

			var oRGRateEntry1 = testGlobal.AddRateEntry("ORG", "ALL", "AUSYD", "", "STD", "");
			var oRGRateLine1a = oRGRateEntry1.AddRateLine(TestGL1.AC_Code, FlatCalculator.Code);
			oRGRateLine1a.GetCalculator<FlatCalculator>().BaseRate = 65.43M;

			var oRGRateEntry2 = testGlobal.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			var oRGRateLine2a = oRGRateEntry2.AddRateLine(TestGL2.AC_Code, FlatCalculator.Code);
			oRGRateLine2a.GetCalculator<FlatCalculator>().BaseRate = 78.12M;

			#endregion

			#region DST Entries

			var dSTRateEntry1 = testGlobal.AddRateEntry("DST", "AIR", "", "USLAX", "STD", "");
			var dSTRateLine1a = dSTRateEntry1.AddRateLine(TestGL3.AC_Code, UnitCalculator.Code, "KG");
			dSTRateLine1a.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = .24M;

			var dSTRateEntry2 = testGlobal.AddRateEntry("DST", "AIR", "", "USLAX", "", "");
			var dSTRateLine2a = dSTRateEntry2.AddRateLine(TestGL4.AC_Code, UnitCalculator.Code, "KG");
			dSTRateLine2a.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = .65M;

			var dSTRateEntry3 = testGlobal.AddRateEntry("DST", "FCL", "", "USLAX", "", "20GP");
			var dSTRateLine3a = dSTRateEntry3.AddRateLine(TestGL5.AC_Code, FlatCalculator.Code);
			dSTRateLine3a.GetCalculator<FlatCalculator>().BaseRate = 5M;

			#endregion

			Factory.Save();
		}

		#endregion

		#region One Off Quotation

		protected Quote SetupSampleOneOffQuotation()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			var testRate = Helper.NewQuote(NewClient);

			#region AIR Entries

			var aIRRateEntry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			aIRRateEntry1.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			aIRRateEntry1.RateLines.RemoveAndDeleteAll();
			var aIRRateLine1a = aIRRateEntry1.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, QuantityUnit.KG);
			aIRRateLine1a.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			aIRRateLine1a.Calculator["-45"] = (ZDecimal)4.5m;
			aIRRateLine1a.Calculator["+45"] = (ZDecimal)4.0m;
			aIRRateLine1a.Calculator["+100"] = (ZDecimal)3.5m;
			aIRRateLine1a.Calculator["+250"] = (ZDecimal)3.0m;
			aIRRateLine1a.Calculator["+500"] = (ZDecimal)2.5m;
			aIRRateLine1a.Calculator["+1000"] = (ZDecimal)2.0m;

			#endregion

			#region FCL Entries

			var fCLRateEntry1 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			fCLRateEntry1.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			fCLRateEntry1.RateLines.RemoveAndDeleteAll();
			var fCLRateLine1a = fCLRateEntry1.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, "CN");
			fCLRateLine1a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1800m;

			#endregion

			#region ORG Entries

			var oRGRateEntry1 = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			var oRGRateLine1a = oRGRateEntry1.AddRateLine(TestAWB.AC_Code, FlatCalculator.Code);
			oRGRateLine1a.GetCalculator<FlatCalculator>().BaseRate = 50M;

			#endregion

			Factory.Save();

			return testRate;
		}

		#endregion

		#region Helpers

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		#endregion
	}

	#region TestCurrencyConverter

	public class TestCurrencyConverter : CurrencyConverter, IJobExRateCurrencyConverter
	{
		public TestCurrencyConverter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override ZDecimal GetExchangeRate(ICurrency currency)
		{
			switch (currency.Code)
			{
				case "AUD":
					return 1m;

				case "NZD":
					return 1.2m;

				case "USD":
					return 0.7m;

				case "EUR":
					return 0.5m;

				default:
					return 0m;
			}
		}

		public override ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate)
		{
			foundRateDate = ZDateTime.Today;
			return GetExchangeRate(currency);
		}

		public ZDecimal GetExchangeRate(ICurrency currency, ZGuid orgPK, CostSell costOrSell)
		{
			var rate = GetExchangeRate(currency, out _);

			if (!orgPK.IsEmpty)
			{
				rate += costOrSell == CostSell.Cost ? (-0.1m) : (+0.1m);
			}

			return rate;
		}

		public Money ConvertExact(Money monetaryAmount, ICurrency destinationCurrency, ZGuid orgPK, CostSell costOrSell, bool roundToDestinationCurrencyDecimals = true)
			=> ConvertExact(monetaryAmount, destinationCurrency, currency => GetExchangeRate(currency, orgPK, costOrSell), roundToDestinationCurrencyDecimals);
	}

	public class TestCurrencyConverterTest : TestCaseWithFactory
	{
		public void TestConverter()
		{
			var aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var eUR = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");

			var converter = new TestCurrencyConverter(Factory);

			AssertEquals(700m, converter.ConvertExact(new Money(1000m, aUD), uSD).Amount);
			AssertEquals(200m, converter.ConvertExact(new Money(140m, uSD), aUD).Amount);
			AssertEquals(500m, converter.ConvertExact(new Money(1000m, aUD), eUR).Amount);
			AssertEquals(400m, converter.ConvertExact(new Money(200m, eUR), aUD).Amount);
			AssertEquals(1000m, converter.ConvertExact(new Money(1400m, uSD), eUR).Amount);
			AssertEquals(196m, converter.ConvertExact(new Money(140m, eUR), uSD).Amount);
		}
	}

	#endregion

	public class TestHelper
	{
		public TestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public BusinessObjectFactory Factory;

		public static IDisposable MockUniversalChargeCodes(API.ChargeCodeWithMappingInfo[] universalChargeCodes)
		{
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.GetAllChargeCodesWithMappingInfo(It.IsAny<string>()))
				.Returns(universalChargeCodes);

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, null));

			return ObjectFactory.Substitute(wiseRatesClientFactoryMock.Object);
		}

		#region Organisations

		public OrgHeader NewOrgHeader(int companyTariffDefault)
		{
			var result = NewOrgHeader();
			result.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, companyTariffDefault);
			return result;
		}

		public OrgHeader NewOrgHeader(int companyTariffDefault, int companyTariffOrigin, int companyTariffFreight, int companyTariffDestination)
		{
			var result = NewOrgHeader(companyTariffDefault);
			result.CompanyData.RateTariffLevels.SetLevel("ORG", companyTariffOrigin);
			result.CompanyData.RateTariffLevels.SetLevel("FRT", companyTariffFreight);
			result.CompanyData.RateTariffLevels.SetLevel("DST", companyTariffDestination);

			return result;
		}

		public OrgHeader NewOrgHeader(string code = null, string closestPort = null)
		{
			var result = NewOrgHeaderWithoutSettingDebtorCreditor();
			result.OH_IsDebtor = true;
			result.OH_IsCreditor = true;

			if (!string.IsNullOrWhiteSpace(code))
			{
				result.OH_Code = code;
			}

			if (!string.IsNullOrWhiteSpace(closestPort))
			{
				result.OH_RL_NKClosestPort = closestPort;
			}

			return result;
		}

		public OrgHeader NewOrgHeaderWithoutSettingDebtorCreditor()
		{
			var result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = ZString.Format("Test Client #{0}", ++OrgIndex);
			result.MainAddress.OA_Address1 = ZString.Format("{0} Fake Street", 100 + OrgIndex);
			result.MainAddress.OA_City = "Sydney";
			result.MainAddress.OA_State = "NSW";
			result.MainAddress.OA_PostCode = "2000";
			result.OH_RL_NKClosestPort = "AUSYD";
			result.OH_Code = ZString.Format("TESTORG{0}", OrgIndex);

			return result;
		}

		int OrgIndex;

		public static OrgRateTariffLevel NewLevel(OrgHeader orgHeader, ZString tariffType, ZString mode, ZString direction, ZByte level)
		{
			var orgRateTariffLevel = orgHeader.CompanyData.RateTariffLevels.AddNew();
			orgRateTariffLevel.P7_TariffType = tariffType;
			orgRateTariffLevel.P7_Mode = mode;
			orgRateTariffLevel.P7_Direction = direction;
			orgRateTariffLevel.P7_TariffLevel = level;
			return orgRateTariffLevel;
		}

		public OrgHeader NewAirlineOrg(string iataCode)
		{
			var airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, iataCode));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = iataCode + "_AIRLINE";
			org.OH_FullName = iataCode + " Airline";
			org.OH_IsAirLine = true;
			org.OH_IsActive = true;
			org.MiscServ.OM_RM_Airline = airline.PK;

			return org;
		}

		#endregion

		#region Rating Date Config

		public RatingDateConfig CreateOrganizationRatingDateConfig(OrgHeader carrier, string chargeGroup, string jobType, string direction, string transportMode, string rateType, string containerMode, string location, string autoratingDate)
			=> CreateRatingDateConfig(OrgHeaderSchema.Constants.Prefix, carrier, chargeGroup, jobType, direction, transportMode, rateType, containerMode, location, autoratingDate);

		public RatingDateConfig CreateRatingDateConfig(string parentTableCode, OrgHeader carrier, string chargeGroup, string jobType, string direction, string transportMode, string rateType, string containerMode, string location, string autoratingDate)
		{
			var ratingDateConfig = Factory.New<RatingDateConfig>();

			ratingDateConfig.RDT_ParentTableCode = parentTableCode;
			ratingDateConfig.RDT_ParentID = carrier.PK;

			ratingDateConfig.RDT_ChargeGroup = chargeGroup;
			ratingDateConfig.RDT_JobType = jobType;
			ratingDateConfig.RDT_Direction = direction;
			ratingDateConfig.RDT_TransportMode = transportMode;
			ratingDateConfig.RDT_RateType = rateType;
			ratingDateConfig.RDT_ContainerMode = containerMode;
			ratingDateConfig.RDT_Location = location;
			ratingDateConfig.RDT_AutoratingDate = autoratingDate;

			return ratingDateConfig;
		}

		#endregion

		#region Rating Contracts

		/// <param name="contractType">A value such as RatingContractTypes.Provider or RatingContractTypes.Client</param>
		/// <param name="transportMode">Either Core.Constants.TransportModes.Sea or Core.Constants.TransportModes.Air</param>
		public IRatingContract NewRatingContract(OrgHeader org, string contractNumber, string contractType = RatingContractTypes.Provider, ZDate? startDate = null, ZDate? endDate = null, ZGuid? companyPK = null, string transportMode = Core.Constants.TransportModes.Sea, string containerType = "", bool allowHazardousCommodities = true)
		{
			var contract = (IRatingContract)Factory.NewWithValidTestData(ObjectFactory.GetType<IRatingContract>());

			contract.RCT_ContractType = contractType;
			contract.RCT_ContractNumber = contractNumber;
			contract.RCT_StartDate = startDate ?? ZDate.Today.AddMonths(-1);
			contract.RCT_EndDate = endDate ?? contract.RCT_StartDate.AddMonths(2);
			contract.RCT_TransportMode = transportMode;
			contract.RCT_IsActive = true;
			contract.RCT_OH = org.PK;
			contract.RCT_GC = companyPK ?? ZGuid.Empty;
			contract.RCT_ContainerType = containerType;
			contract.RCT_AllowHazardousCommodities = allowHazardousCommodities;

			return contract;
		}

		public IRatingContractAllocationLine NewRatingContractAllocation(IRatingContract ratingContract, string originUNLOCO, string destinationUNLOCO, ZDate? startDate = null, ZDate? endDate = null, ZGuid container = default(ZGuid))
		{
			var allocation = (IRatingContractAllocationLine)Factory.NewWithValidTestData(ObjectFactory.GetType<IRatingContractAllocationLine>());

			allocation.RCA_RCT_RatingContract = ratingContract.PK;
			allocation.RCA_StartDate = startDate ?? ZDate.Today.AddMonths(-1);
			allocation.RCA_ExpiryDate = endDate ?? ZDate.Today.AddMonths(1);
			allocation.RCA_LoadLocation = originUNLOCO;
			allocation.RCA_DischargeLocation = destinationUNLOCO;
			allocation.RCA_RC_ContainerType = container;

			return allocation;
		}

		#endregion

		#region Rating Headers

		#region Quote

		public Quote NewQuote(OrgHeader client)
		{
			var result = Factory.New<Quote>();
			if (client != null)
			{
				result.QuotationClientAddress.OrganisationPK = client.PK;
			}
			result.TH_QuoteNumber = (1000 - ++QuoteIndex).ToString().PadLeft(8, '0');

			return result;
		}

		public Quote NewFullyPopulatedQuote()
		{
			var result = NewQuote(NewOrgHeader());
			var transportProvider = NewOrgHeader();

			var entry = result.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", "STD", Containers["20GP"].RC_Code);
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "INBOM", "SGSIN", "STD", Containers["40GP"].RC_Code);
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "INBOM", "SGSIN");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.DST, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "INBOM", "SGSIN");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.PAC, "FCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.UNP, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CST, "SEA", "", "");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.AllWarehouses = true;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.TRN, "LRO", "AUSYD", "");
			AddMultipleLinesToEntry(entry);

			return result;
		}

		int QuoteIndex;

		#endregion

		#region Company Tariff

		/// <summary>
		/// Warning! New Company/Global Tariffs are created in
		/// a NEW Factory!. You need to save the Tariff.Factory !
		/// </summary>
		public CompanyTariff NewCompanyTariff()
		{
			return new BusinessObjectFactory().New<CompanyTariff>();
		}

		/// <summary>
		/// Warning! New Company/Global Tariffs are created in
		/// a NEW Factory!. You need to save the Tariff.Factory !
		/// </summary>
		public CompanyTariff NewGlobalTariff()
		{
			return new BusinessObjectFactory().New<GlobalTariff>();
		}

		public CompanyTariff NewFullyPopulatedCompanyTariff()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", "STD", Containers["20GP"].RC_Code);
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "INBOM", "SGSIN", "STD", Containers["40GP"].RC_Code);
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "FCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.UNP, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.CST, "SEA", "", "");
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.AllWarehouses = true;
			AddMultipleLinesToEntry(entry);

			entry = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, "LRO", "AUSYD", "");
			AddMultipleLinesToEntry(entry);

			return tariff;
		}

		public CompanyTariff NewLevel1CompanyTariffWithSingleRateLine(ZString category, ZString mode, ZString origin, ZString destination, ZString chargeCode, ZDecimal baseRate)
		{
			var companyTariff = NewCompanyTariff();
			companyTariff.TH_GlobalRateLevel = 1;

			AddRateEntryWithSingleRateLine(companyTariff, category, mode, origin, destination, chargeCode, baseRate);

			companyTariff.Factory.Save();

			return companyTariff;
		}

		public CompanyTariff AddRateEntryWithSingleRateLine(CompanyTariff companyTariff, ZString category, ZString mode, ZString origin, ZString destination, ZString chargeCode, ZDecimal baseRate)
		{
			var tariffRateEntry = companyTariff.AddRateEntry(category, mode, origin, destination);
			tariffRateEntry.RateLines.RemoveAndDeleteAll();

			AddRateLineWithFlatCalculatorToRateEntry(tariffRateEntry, chargeCode, baseRate);

			companyTariff.Factory.Save();
			return companyTariff;
		}

		public CompanyTariff NewNonLevel1CompanyTariff(ZByte level, ZString discountType, ZDecimal discount)
		{
			if (level <= 1)
			{
				throw new NotSupportedException("This method only support level > 1. Please use NewLevel1CompanyTariffWithSingleRateLine for level 1.");
			}

			var companyTariff = NewCompanyTariff();
			companyTariff.TH_GlobalRateLevel = level;

			SetNonLevel1CompanyTariff(companyTariff, discountType, discount);

			companyTariff.Factory.Save();

			return companyTariff;
		}

		public void SetNonLevel1CompanyTariff(CompanyTariff companyTariff, ZString discountType, ZDecimal discount)
		{
			companyTariff.DiscountType = discountType;
			companyTariff.Discount = discount;

			companyTariff.Factory.Save();
		}

		public RateLine AddRateLineWithFlatCalculatorToRateEntry(RateEntry rateEntry, ZString chargeCode, ZDecimal baseRate)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code, "", CurrencyCodes.Australia);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = baseRate;

			return rateLine;
		}

		#endregion

		#region Client Rate

		public ClientRate NewClientRate(OrgHeader client)
		{
			var result = Factory.NewWithValidTestData<ClientRate>();
			if (client != null)
			{
				result.TH_OH = client.PK;
				client.OH_IsDebtor = true;
			}

			return result;
		}

		public ClientRate NewGlobalClientRate(OrgHeader client)
		{
			var result = NewClientRate(client);
			result.TH_GC = ZGuid.Empty;

			return result;
		}

		public ClientRate NewFullyPopulatedClientRate()
		{
			var result = Factory.NewWithValidTestData<ClientRate>();
			result.Header.OH_Code = ZString.Format("TESTORG{0}", ++OrgIndex);
			var companyData = result.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany); //required for ActiveRatesForm form basher.
			var transportProvider = NewOrgHeader();

			var entry = result.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", "STD", Containers["20GP"].RC_Code);
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "INBOM", "SGSIN", "STD", Containers["40GP"].RC_Code);
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "INBOM", "SGSIN");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.DST, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "INBOM", "SGSIN");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			#region Customs Rates

			entry = result.AddRateEntry(RatingConstants.RateCategory.CAI, "LSE", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CAI, "LSE", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CFC, "SEA", "AUSYD", "USLAX", "STD", Containers["20GP"].RC_Code);
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CFC, "SEA", "INBOM", "SGSIN", "STD", Containers["40GP"].RC_Code);
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CLC, "LCL", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CLC, "LCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.COR, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.COR, "FCL", "INBOM", "SGSIN");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CDS, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CDS, "FCL", "INBOM", "SGSIN");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			#endregion

			entry = result.AddRateEntry(RatingConstants.RateCategory.PAC, "FCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.UNP, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.CST, "SEA", "", "");
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.AllWarehouses = true;
			AddMultipleLinesToEntry(entry);

			entry = result.AddRateEntry(RatingConstants.RateCategory.TRN, "LRO", "AUSYD", "");
			AddMultipleLinesToEntry(entry);

			return result;
		}

		public ClientRate NewClientRateWithSingleRateLine(OrgHeader client, ZString category, ZString mode, ZString origin, ZString destination, ZString chargeCode, ZDecimal baseRate)
		{
			var clientRate = NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(category, mode, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code, "", CurrencyCodes.Australia);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = baseRate;
			Factory.Save();

			return clientRate;
		}

		#endregion

		#region Costing

		public Costing NewCosting(OrgHeader supplier)
		{
			var result = Factory.New<Costing>();

			if (supplier != null)
			{
				result.TH_OH = supplier.PK;
				supplier.OH_IsCreditor = true;
			}

			return result;
		}

		public Costing NewGlobalCosting(OrgHeader supplier)
		{
			var result = NewCosting(supplier);
			result.TH_GC = ZGuid.Empty;

			return result;
		}

		public Costing NewFullyPopulatedCosting()
		{
			var testCosting = Factory.New<Costing>();

			testCosting.TH_OH = NewOrgHeader().PK;
			var transportProvider = NewOrgHeader();

			var entry = testCosting.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", "STD", Containers["20GP"].RC_Code);
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "INBOM", "SGSIN", "STD", Containers["40GP"].RC_Code);
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUSYD", "USLAX");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "INBOM", "SGSIN");
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "INBOM", "SGSIN");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.DST, "AIR", "AUSYD", "USLAX");
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "INBOM", "SGSIN");
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.CST, "SEA", "", "");
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.TI_WW_Warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Warehouse.Integration.IWhsWarehouse))).PK;
			AddMultipleLinesToEntry(entry);

			entry = testCosting.AddRateEntry(RatingConstants.RateCategory.TRN, "LRO", "AUSYD", "");
			AddMultipleLinesToEntry(entry);

			return testCosting;
		}

		#endregion

		#region Intercompany Tariff

		public IntercompanyTariff NewIntercompanyTariff(OrgHeader client = null)
		{
			var result = Factory.New<IntercompanyTariff>();
			result.TH_OH = client == null ? GlbCompany.CurrentCompany.GC_OH_OrgProxy : client.PK;

			return result;
		}

		public IntercompanyTariff NewFullyPopulatedIntercompanyTariff(OrgHeader client = null)
		{
			var intercompanyTariff = NewIntercompanyTariff(client ?? GlbCompany.CurrentCompany.OrgProxy);
			var transportProvider = NewOrgHeader();

			var entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "INBOM", "SGSIN");
			entry.RateLines.RemoveAndDeleteAll();
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", "STD", Containers["20GP"].RC_Code);
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "INBOM", "SGSIN", "STD", Containers["40GP"].RC_Code);
			entry.RateLines.RemoveAndDeleteAll();
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "INBOM", "SGSIN");
			entry.RateLines.RemoveAndDeleteAll();
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "INBOM", "SGSIN");
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.DST, "AIR", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "INBOM", "SGSIN");
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_OH_TransportProvider = transportProvider.PK;
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.CST, "SEA", "", "");
			entry.RateLines.RemoveAndDeleteAll();
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.RateLines.RemoveAndDeleteAll();
			AddMultipleLinesToEntry(entry);

			entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.TRN, "LRO", "AUSYD", "");
			entry.RateLines.RemoveAndDeleteAll();
			AddMultipleLinesToEntry(entry);

			return intercompanyTariff;
		}

		#endregion

		internal RatingHeader NewRatingHeader(string rateType, OrgHeader orgHeader)
		{
			switch (rateType)
			{
				case RatingConstants.RatingHeaderTypes.ClientRate:
					return NewClientRate(orgHeader);
				case RatingConstants.RatingHeaderTypes.Costing:
					return NewCosting(orgHeader);
				case RatingConstants.RatingHeaderTypes.Tariff:
					return NewCompanyTariff();
				case RatingConstants.RatingHeaderTypes.Quote:
					return NewQuote(orgHeader);
				default:
					throw new NotImplementedException($"{rateType} has not been implemented yet");
			}
		}

		#endregion

		#region Charge Codes

		public ChargeCodeFactory ChargeCodes
		{
			get { return fChargeCodes ?? (fChargeCodes = new ChargeCodeFactory(Factory)); }
		}

		ChargeCodeFactory fChargeCodes;

		public class ChargeCodeFactory
		{
			public ChargeCodeFactory(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public AccChargeCode this[string chargeCode] =>
				GetExisting(chargeCode) ?? New(chargeCode, chargeCode + " Desc", FlatCalculator.Code);

			public AccChargeCode GetExisting(string chargeCode)
			{
				var filter = new ZQuery(
					new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK),
					new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode));

				return factory.Load<AccChargeCode>(filter).FirstOrDefault();
			}

			public AccChargeCode New(ZString code, ZString description, ZString calculator, string chargeGroup = "", string chargeSubGroup = "", bool showOnQuotation = true, bool suppressIfZero = false, string localLanguageDescription = null)
			{
				var chargeCode = factory.New<AccChargeCode>();
				chargeCode.AC_ChargeType = ChargeType.Margin;
				chargeCode.AC_MarginPercentage = 100m;
				chargeCode.AC_Code = code;
				chargeCode.AC_Desc = description;
				chargeCode.AC_RateCalculator = calculator;
				chargeCode.AC_ChargeGroup = string.IsNullOrEmpty(chargeGroup) ? ChargeCodeGroupList.Codes.Freight : chargeGroup;
				chargeCode.AC_ChargeSubGroup = chargeSubGroup;
				chargeCode.AC_ShowOnQuotation = showOnQuotation;
				chargeCode.AC_SuppressOnQuoteIfZero = suppressIfZero;
				chargeCode.SetGLAccountDataForTesting();
				SetTemporaryDepartmentValueOnChargeCode(chargeCode);

				if (localLanguageDescription != null)
				{
					chargeCode.AC_LocalLanguageDescription = localLanguageDescription;
				}

				return chargeCode;
			}

			public void SetTemporaryDepartmentValueOnChargeCode(ZString code)
			{
				var query = new ZQuery();
				query.AddToFilter(AccChargeCodeSchema.AC_Code, code);
				query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

				var accChargeCode = factory.LoadTop1<AccChargeCode>(query);
				SetTemporaryDepartmentValueOnChargeCode(accChargeCode);
			}

			public void SetTemporaryDepartmentValueOnChargeCode(AccChargeCode accChargeCode)
			{
				accChargeCode.AC_DepartmentFilterList = "ALL";
			}

			/// <summary>
			/// Creates a GST/VAT tax for current company country.
			/// </summary>
			public AccTaxRate CreateTaxRate(ZInt taxRateAmount)
			{
				var taxRate = new BusinessObjectFactory().NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.AT_Code = "TAX" + ++taxRateCounter;
				taxRate.AT_Description = ZString.Format("TAX #{0}", taxRateCounter);
				taxRate.SetRateNumerator_ForTestOnly(taxRateAmount);
				taxRate.Factory.Save();

				return factory.Load<AccTaxRate>(taxRate.PK);
			}

			int taxRateCounter;

			public AccChargeCode NewConsolChargeCode(ZString code, ZString description, ZString calculator, string chargeGroup = "", string chargeSubGroup = "", bool showOnQuotation = true, bool suppressIfZero = false, ZGuid companyPK = default)
			{
				var tax = factory.NewWithValidTestData<AccTaxRate>();
				tax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				tax.SetRateNumerator_ForTestOnly(0);

				var consolChargeCode = New(code, description, calculator, chargeGroup, chargeSubGroup, showOnQuotation, suppressIfZero);
				consolChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
				consolChargeCode.AC_AT_GSTRate = tax.PK;
				consolChargeCode.AC_IsGroupageCharge = true;

				if (!companyPK.IsEmpty)
				{
					consolChargeCode.AC_GC = companyPK;
				}

				return consolChargeCode;
			}

			public AccChargeCode CreateGlobalCharge(ZString code, string calculator = FlatCalculator.Code, string chargeGroup = ChargeCodeGroupList.Codes.Freight)
			{
				FixSingaporeCompanyData();
				AccTaxRate.LoadExistingOrCreateNewTaxRate(factory, "GST", AccTaxRate.Types.Rated, 10);

				var globalChargeCode = factory.NewWithValidTestData<AccChargeCode>();
				globalChargeCode.AC_GC = ZGuid.Empty;
				globalChargeCode.AC_Code = code;
				globalChargeCode.AC_Desc = code + " Global Charge";
				globalChargeCode.AC_ChargeType = ChargeType.Margin;
				globalChargeCode.AC_ChargeGroup = chargeGroup;
				globalChargeCode.AC_RateCalculator = calculator;
				SetTemporaryDepartmentValueOnChargeCode(globalChargeCode);

				return globalChargeCode;
			}

			void FixSingaporeCompanyData()
			{
				var singaporeCompany = SingaporeCompany;

				if (singaporeCompany.GC_IsGSTRegistered)
				{
					singaporeCompany.GC_IsGSTRegistered = false;

					if (!IsMarkAsNeedingValidationSuspended)
					{
						factory.Save();
					}
				}
			}

			public AccChargeTypeOverride NewChargeTypeOverride(AccChargeCode chargeCode, string jobType, string chargeType = "REV", string invoiceType = "FCO")
			{
				var typeOverride = chargeCode.ChargeTypeOverrides.AddNew();
				typeOverride.AN_JobDirection = "ALL";
				typeOverride.AN_JobType = jobType;
				typeOverride.AN_ChargeType = chargeType;
				typeOverride.AN_InvoiceType = invoiceType;

				return typeOverride;
			}

			GlbCompany SingaporeCompany
			{
				get { return factory.GetCachedValue("SingaporeCompany", () => factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"))); }
			}

			public bool IsMarkAsNeedingValidationSuspended { get; set; }

			readonly BusinessObjectFactory factory;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Test Code")]
		public void SetChargeCodeMultilingualDescription(AccChargeCode chargeCode, IMockResourceStringCache mockRes, string multilingualDescription)
		{
			var resKey = chargeCode.AC_DescInfo.CustomizableDataResourceStrings.GetMultilingualString(chargeCode, chargeCode.AC_Desc).ResourceKey;
			mockRes.Put(resKey, new ResourceStringData(resKey, multilingualDescription));
		}

		#endregion

		#region Registry Charges Collections

		public void AddLocationCharge(LocationsChargesCollection collection, ZString location, ZGuid chargeCode)
		{
			var group = collection.Cast<LocationsChargesGroup>().FirstOrDefault(x => x.Location == location);
			if (group == null)
			{
				group = collection.AddNew();
				group.Location = location;
			}

			group.Charges.AddNew().ChargeCodePK = chargeCode;
		}

		#endregion

		#region Currencies

		public RefExchangeRate NewExchangeRate(RefCurrency currency, ZString rateType, ZDecimal rate)
		{
			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = rateType;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			return exchangeRate;
		}

		public RefExchangeRate NewExchangeRate(ZString currency, ZString rateType, ZDecimal rate)
		{
			return NewExchangeRate(Currencies[currency], rateType, rate);
		}

		public CurrencyFactory Currencies
		{
			get { return fCurrencies ?? (fCurrencies = new CurrencyFactory(Factory)); }
		}

		CurrencyFactory fCurrencies;

		public class CurrencyFactory
		{
			public CurrencyFactory(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}

			public RefCurrency this[string currency]
			{
				get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency); }
			}

			readonly BusinessObjectFactory Factory;
		}

		#endregion

		#region Containers

		public ContainerFactory Containers
		{
			get { return fContainers ?? (fContainers = new ContainerFactory(Factory)); }
		}

		ContainerFactory fContainers;

		public class ContainerFactory
		{
			public ContainerFactory(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}

			public RefContainer this[string container]
			{
				get { return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container); }
			}

			readonly BusinessObjectFactory Factory;
		}

		#endregion

		#region Commodities

		public RefCommodityCode NewCommodity(string code, string universalGroup)
		{
			var commodity = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, code);

			if (commodity == null)
			{
				commodity = Factory.NewWithValidTestData<RefCommodityCode>();
				commodity.RH_Code = code;
				commodity.RH_UniversalCommodityGroup = universalGroup;
			}

			return commodity;
		}

		/// <summary>
		/// Creates a relationship between a parent commodity and a child commodity.
		/// It assumes both the parent and the child to already exist when this is called
		/// </summary>
		public RefCommodityRatingCodeMap NewCommodityRatingCode(string parentCommodity, string childCommodity)
		{
			var mapping = Factory.New<RefCommodityRatingCodeMap>();
			mapping.RI_RH_NKCommodityParent = parentCommodity;
			mapping.RI_RH_NKCommodityChild = childCommodity;

			return mapping;
		}

		#endregion

		#region OrgSupplierParts

		public OrgSupplierPart NewOrgSupplierPart(OrgHeader owner)
		{
			return NewOrgSupplierPart(owner, Constants.PkgUnit.Unit);
		}

		public OrgSupplierPart NewOrgSupplierPart(OrgHeader owner, ZString stockKeepingUnit)
		{
			var result = Factory.New<OrgSupplierPart>();
			result.OP_PartNum = ZString.Format("PROD{0}", ++OrgSupplierPartIndex);
			result.OP_Desc = ZString.Format("###{0}", OrgSupplierPartIndex);
			result.OP_StockKeepingUnit = stockKeepingUnit;

			var rel = result.RelatedOrganisations.AddNew();
			rel.OU_OH = owner.PK;
			rel.OU_OP = result.PK;
			rel.OU_Relationship = "OWN";

			return result;
		}

		int OrgSupplierPartIndex;

		public void AddPartUnit(OrgSupplierPart part, ZString package, ZString parentPackage, ZDecimal quantityInParent)
		{
			var unit = part.PartUnits.AddNew();
			unit.OF_OP = part.PK;
			unit.OF_PackType = package;
			unit.OF_ParentPackType = parentPackage;
			unit.OF_QuantityInParent = quantityInParent;
		}

		public void SetProductWeightAndVolume(OrgSupplierPart part, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ)
		{
			part.OP_Weight = weight;
			part.OP_WeightUQ = weightUQ;
			part.OP_Cubic = volume;
			part.OP_CubicUQ = volumeUQ;
		}

		#endregion

		#region Warehouses

		public BusinessObject NewWarehouse()
		{
			var result = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Warehouse.Integration.IWhsWarehouse)));
			result[WhsWarehouseSchema.WW_WarehouseName.Name] = ZString.Format("WHS{0}", ++WarehouseIndex);

			return result;
		}

		int WarehouseIndex;

		#endregion

		#region International Zone Sets

		public RefZoneHeader NewInternationalZone(string zoneCode, OrgHeader carrier = null, params string[] iLocations)
		{
			var internationalZone = Factory.New<RefZoneHeader>();
			internationalZone.FZ_Code = zoneCode;
			internationalZone.FZ_Description = zoneCode + " Zone";
			internationalZone.FZ_OH_RelatedParty = carrier?.PK ?? ZGuid.Empty;
			internationalZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			internationalZone.FZ_ZoneMode = RateMode.ALL;

			foreach (var iLocation in iLocations)
			{
				if (iLocation.Length == 5)
				{
					var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, iLocation);
					internationalZone.UNLOCOs.Add(unloco);
				}
				else if (iLocation.Length == 2)
				{
					var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, iLocation);
					internationalZone.Countries.Add(country);
				}
			}

			return internationalZone;
		}

		#endregion

		#region Transport Zone Sets

		public RateTransportProvider CreateRateTransportZoneSet(OrgHeader supplier, ZString countryCode, RefCityTown cityTown = null, params ZString[] zoneNames)
		{
			var result = CreateRateTransportZoneSet(supplier, countryCode);

			if (cityTown != null)
			{
				result.TP_R9_ZoneHubLocation = cityTown.PK;
			}

			foreach (var zoneName in zoneNames)
			{
				result.Zones.AddNew().TZ_ZoneName = zoneName;
			}

			return result;
		}

		public RateTransportProvider CreateRateTransportZoneSet(OrgHeader supplier, ZString countryCode, params int[] distances)
		{
			var zoneSet = CreateRateTransportZoneSet(supplier, countryCode);

			for (var i = 1; i < distances.Length; i++)
			{
				var zoneName = ZString.Format("{0} to {1}", distances[i - 1], distances[i] - 1);
				var zone = zoneSet.CreateRateTransportZoneForTest(zoneName);
				zone.CreateRateTransportZoneItemForTest(distances[i - 1], distances[i] - 1);
			}

			return zoneSet;
		}

		public RateTransportProvider CreateRateTransportZoneSet(OrgHeader supplier, ZString countryCode, string zoneType = RatingConstants.RatingZoneTypes.Rating, string zoneMode = RateMode.ALL)
		{
			var zoneSet = Factory.New<RateTransportProvider>();
			zoneSet.TP_ZoneType = zoneType;
			zoneSet.TP_ZoneMode = zoneMode;
			zoneSet.TP_RN_NKCountry = countryCode;
			if (supplier != null)
			{
				zoneSet.TP_OH_RelatedParty = supplier.PK;
			}

			return zoneSet;
		}

		#endregion

		#region PostCode

		public RefPostCode CreateRefPostCode(ZString cityTownPostCode, string countryCode = CountryCodes.Australia)
		{
			var result = Factory.New<RefPostCode>();
			result.RK_CityTownPostCode = cityTownPostCode;
			result.RK_RN_NKCountry = countryCode;
			return result;
		}

		#endregion

		#region Get City/Town

		public RefCityTown GetCityTown(ZString internationalName, ZString stateCode, string countryCode = "AU")
		{
			var query = new ZQuery(RefCityTownSchema.R9_InternationalName, internationalName);
			query.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, countryCode);
			if (!stateCode.IsEmpty)
			{
				query.AddToFilter(RefCityTownSchema.R9_RW_NKState, stateCode);
			}

			var cityTown = Factory.LoadTop1<RefCityTown>(query);
			if (cityTown == null)
			{
				cityTown = Factory.New<RefCityTown>();
				cityTown.R9_RN_NKCountry = countryCode;
				cityTown.R9_InternationalName = internationalName;
				cityTown.R9_RW_NKState = stateCode;
			}

			return cityTown;
		}

		#endregion

		#region Ad-Hoc Job Services

		public JobService CreateAdHocJobService(IHaveServices parent, ZString code, ZDecimal rate, ZString measureBasis, ZGuid? contractorPK = null)
		{
			var service = parent.Services.AddNew();
			service.ES_Completed = ZDateTime.Today;
			service.ES_ServiceCount = 1;
			service.ES_OA_Location = NewOrgHeader().MainAddress.PK;
			service.ES_ServiceCode = code;
			service.ES_ServiceRate = rate;
			service.ES_MeasurementBasis = measureBasis;
			service.ES_References = "REF" + code + rate;

			if (contractorPK != null)
			{
				service.ES_OH_Contractor = contractorPK.Value;
			}

			return service;
		}

		#endregion

		#region Random Charge Codes / Calculators

		void CreateChargeCodes(bool isGlobal)
		{
			if ((isGlobal && isGlobalChargesCreated) || (!isGlobal && isChargeCodesCreated))
			{
				return;
			}

			foreach (var calcCode in CalcCodeList)
			{
				if (calcCode == ExcludeCompanyTariffsCalculator.Code)
				{
					continue;
				}

				(string code, string description, string calculator, string group)[] chargeInfoList =
				{
					("TESTFRT" + calcCode, "Test Freight", calcCode, ChargeCodeGroupList.Codes.Freight),
					("TESTORG" + calcCode, "Test Origin", calcCode, ChargeCodeGroupList.Codes.Origin),
					("TESTDST" + calcCode, "Test Destination", calcCode, ChargeCodeGroupList.Codes.Destination),
					("TESTTRN" + calcCode, "Test Transport", calcCode, ChargeCodeGroupList.Codes.Transport),
					("TESTWHS" + calcCode, "Test Warehouse", calcCode, ChargeCodeGroupList.Codes.WHSInwards),
					("TESTCFS" + calcCode, "Test CFS", calcCode, ChargeCodeGroupList.Codes.CFSLoadList),
					("TESTCST" + calcCode, "Test Container Storage", calcCode, ChargeCodeGroupList.Codes.ContainerStorage)
				};

				foreach (var chargeInfo in chargeInfoList)
				{
					if (isGlobal)
					{
						ChargeCodes.CreateGlobalCharge(chargeInfo.code, chargeInfo.calculator, chargeInfo.group);
					}
					else
					{
						ChargeCodes.New(chargeInfo.code, chargeInfo.description, chargeInfo.calculator, chargeInfo.group);
					}
				}
			}

			isChargeCodesCreated |= !isGlobal;
			isGlobalChargesCreated |= isGlobal;

			if (!IsMarkAsNeedingValidationSuspended)
			{
				Factory.Save();
			}
		}

		bool isChargeCodesCreated;
		bool isGlobalChargesCreated;

		#region Suspend Validation For Basher Test

		public bool IsMarkAsNeedingValidationSuspended
		{
			get { return fIsMarkAsNeedingValidationSuspended; }
			set
			{
				fIsMarkAsNeedingValidationSuspended = value;
				ChargeCodes.IsMarkAsNeedingValidationSuspended = value;
			}
		}
		bool fIsMarkAsNeedingValidationSuspended;

		#endregion

		AccChargeCode GetChargeCode(RateEntry rateEntry, AccChargeCodeCollection chargeCodes)
		{
			AccChargeCode code = null;
			while (EntryAlreadyContainsChargeCode(rateEntry, code) || code == null)
			{
				code = chargeCodes[new Random().Next(0, chargeCodes.Count - 1)];
			}

			return code;
		}

		bool EntryAlreadyContainsChargeCode(RateEntry entry, AccChargeCode chargeCode)
		{
			var result = false;

			foreach (RateLine line in entry.RateLines)
			{
				if (line.ChargeCode != null && chargeCode != null && line.ChargeCode.PK == chargeCode.PK)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		void AddMultipleLinesToEntry(RateEntry entry)
		{
			CreateChargeCodes(entry.IsGlobal());

			var chargeCodesAdded = 0;
			foreach (var calcCode in CalcCodeList)
			{
				if (chargeCodesAdded == 5)
				{
					break;
				}

				if (calcCode == CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode ||
					calcCode == CompanyTariffOrCostBasedCalculator.CostBasedCode)
				{
					continue;
				}

				if (entry.IsFreightEntry() && (calcCode == CartageCalculator.Code ||
					calcCode == CartageZoneDistanceCalculator.Code ||
					calcCode == AgencyCalculator.Code))
				{
					continue;
				}

				var rateLine = entry.RateLines.AddNew();
				rateLine.Lookups.ChargeCodes.Load();
				var chargeCode = GetChargeCode(entry, rateLine.Lookups.ChargeCodes);
				rateLine.TL_AC = chargeCode.PK;
				rateLine.TL_WeightVolume = "KG";
				rateLine.TL_RX_NKCurrency = "AUD";
				rateLine.TL_RateCalculator = calcCode;

				if (rateLine.RateCalculatorType == CalculatorType.Combined)
				{
					var line1Item1 = rateLine.RateLineItems.AddNew();
					line1Item1.TM_Type = "MIN";
					line1Item1.TM_Value = 100m;

					var line1Item2 = rateLine.RateLineItems.AddNew();
					line1Item2.TM_Type = "-";
					line1Item2.TM_Value = 100m;
					line1Item2.TM_Break = 100m;

					var line1Item3 = rateLine.RateLineItems.AddNew();
					line1Item3.TM_Type = "+";
					line1Item3.TM_Value = 100m;
					line1Item3.TM_Break = 100m;

					var line1Item4 = rateLine.RateLineItems.AddNew();
					line1Item4.TM_Type = "+";
					line1Item4.TM_Value = 400m;
					line1Item4.TM_Break = 400m;

					var line1Item5 = rateLine.RateLineItems.AddNew();
					line1Item5.TM_Type = "ACC";
					line1Item5.TM_Text = "Y";
				}
				else
				{
					if (rateLine.RateLineItems.Count > 0)
					{
						var line1Item1 = rateLine.RateLineItems[0];
						line1Item1.TM_Value = 100m;
					}
				}

				if (rateLine.RateCalculatorType == CalculatorType.Cartage ||
					rateLine.RateCalculatorType == CalculatorType.CartageZoneDistance)
				{
					if (rateLine.Lookups.EquipmentTypes.Count > 0)
					{
						((CartageCalculator)rateLine.Calculator).EquipmentType = rateLine.Lookups.EquipmentTypes[0].Code;
					}
				}

				if (rateLine.RateCalculatorType == CalculatorType.Percentage)
				{
					var item1 = rateLine.RateLineItems.AddNew();
					item1.TM_Type = CalculatorConstants.Type.ApplyTo;
					item1.TM_Text = CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods;
				}

				rateLine.RateCalculatorChanged = false;
				chargeCodesAdded++;
			}
		}

		internal string[] CalcCodeList => calcCodeList ?? (calcCodeList = new CodeDescriptionPairList(OLookUpEditType.RateCalculators).GetAllCodes());
		string[] calcCodeList;

		#endregion

		#region Create Creditor

		public OrgHeader CreateCreditor(string code = "CREDITOR")
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = code;
			creditor.OH_IsCreditor = true;
			creditor.CompanyData.SetAPTaxApplicable(false);

			return creditor;
		}

		#endregion

		#region TestRatingCriteria

		public TestRatingCriteria CreateRatingCriteria(string origin, string destination = "", FreightMode freightMode = FreightMode.LSE, decimal weight = 500m, decimal volume = 1m, OrgHeader localClient = null, OrgHeader carrier = null, JobInvoicingConsumerType consumerType = null, string containerMode = null, Directions? directions = null)
		{
			var testRatingCriteria = new TestRatingCriteria(origin, destination, freightMode, weight, volume, localClient);

			if (carrier != null)
			{
				testRatingCriteria.Carrier = carrier;
			}

			if (consumerType != null)
			{
				testRatingCriteria.ConsumerType = consumerType;
			}

			if (containerMode != null)
			{
				testRatingCriteria.ContainerMode = containerMode;
			}

			if (directions != null)
			{
				testRatingCriteria.JobDirection = directions.Value;
			}

			return testRatingCriteria;
		}

		public TestRatingCriteria CreateCriteriaForFCL(string origin, string destination = "", int containerCount = 1, RefContainer containerType = null, OrgHeader localClient = null)
		{
			return new TestRatingCriteria(origin, destination, containerCount, containerType ?? Containers["20GP"], localClient);
		}

		#endregion

		#region CurrencyConverter

		public IJobExRateCurrencyConverter CurrencyConverter =>
			currencyConverter ?? (currencyConverter = NonOrgSpecificExRateCurrencyConverter.Default(Factory));

		IJobExRateCurrencyConverter currencyConverter;

		#endregion

		#region Rate Services

		public RatesSearchResponse CreateSEARatesSearchResponse(Rate[] rates, Rate[] ratesForGettingChargeCodes, string carrierCode = "Emirates", string carrierFullName = "Transport Provider 1", string scacCode = "SCAC")
			=> new RatesSearchResponse
			{
				Rates = rates,
				Carriers = new[]
				{
					new RefCarrier { Code = carrierCode, SCACCode = scacCode, Name = carrierFullName }
				},
				ChargeCodes = GetChargeCodesFromRates(ratesForGettingChargeCodes)
			};

		public RefChargeCode[] GetChargeCodesFromRates(params Rate[] rates)
		{
			List<RefChargeCode> chargeCodes = new List<RefChargeCode>();
			foreach (var chargeCode in rates.SelectMany(r => r.Charges.Select(x => x.ChargeCode)).Distinct())
			{
				var accChargeCode = ChargeCodes[chargeCode];
				if (string.IsNullOrEmpty(accChargeCode.AC_ChargeGroup))
				{
					accChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
				}
				chargeCodes.Add(new RefChargeCode { Code = chargeCode, Group = accChargeCode.AC_ChargeGroup });
			}

			return chargeCodes.ToArray();
		}

		public Rate CreateTestRate(ZString category, ZString mode, string origin, string destination, ZString serviceLevel, ZString commodityCode, ZString containerType, ZString carrier, ZString client, ZString controllingCustomer)
		{
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerType));

			var result = new Rate
			{
				Client = client,
				Carrier = carrier,
				ControllingCustomer = controllingCustomer,
				Charges = new List<RatesServiceCharge>(),
				Commodity = commodityCode,
				Container = refContainer != null
					? new DTO.RefContainer
					{
						Code = category == WRConstants.TransportModes.AIR ? refContainer.RC_Code : refContainer.RC_ISOType,
						ISOType = refContainer.RC_ISOType
					}
					: null,
				ContainerMode = mode,
				Destination = destination,
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Origin = origin,
				ServiceLevel = serviceLevel,
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				TransportMode = category,
				Provider =
					category == WRConstants.TransportModes.AIR ? WRConstants.RateProviders.CargoGuide :
					category == WRConstants.TransportModes.SEA ? WRConstants.RateProviders.CargoSphere :
					WRConstants.RateProviders.WTG
			};

			return result;
		}

		public void SetContainerQuality(Rate wiseRate, string quality)
		{
			var customFields =
				wiseRate.ProviderCustomFields?.Where(x => x.Code != Rate.CustomFields.CargoSphere.ContainerQuality).ToList()
				?? new List<API.CustomField>();

			if (quality != null)
			{
				customFields.Add(new API.CustomField
				{
					Code = Rate.CustomFields.CargoSphere.ContainerQuality,
					Value = quality
				});
			}

			wiseRate.ProviderCustomFields = customFields;
		}

		public RatesServiceCharge CreatePerUnitCharge(string code, string unit, string currency, decimal perUnit)
		{
			var charge = CreateCharge(code, unit, currency);
			charge.PerUnitRate = perUnit;
			charge.FlatRate = null;
			return charge;
		}

		public RatesServiceCharge CreateCharge(string code, string unit = null, string currency = null)
			=> new RatesServiceCharge
			{
				RateLineID = 1,
				ChargeCode = code,
				Unit = unit,
				Currency = currency
			};

		#endregion

		#region Buyers Consol

		public ForwardingShipment CreateColoadShipment(ForwardingConsol consol, ForwardingShipment leadShipment, OrgHeader consignor, OrgHeader consignee, decimal weight = 0m, decimal volume = 0m)
		{
			var coloadShipment = consol.Shipments.AddNew();
			coloadShipment.JS_TransportMode = leadShipment.JS_TransportMode;
			coloadShipment.JS_PackingMode = leadShipment.JS_PackingMode;
			coloadShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			coloadShipment.JS_INCO = IncoTerms.FreeOnBoard;
			coloadShipment.ConsignorPK = consignor.PK;
			coloadShipment.ConsigneePK = consignee.PK;
			coloadShipment.JS_RL_NKOrigin = leadShipment.JS_RL_NKOrigin;
			coloadShipment.JS_RL_NKDestination = leadShipment.JS_RL_NKDestination;
			coloadShipment.JS_ActualWeight = weight;
			coloadShipment.JS_ActualVolume = volume;
			coloadShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;

			return coloadShipment;
		}

		public ForwardingShipment CreateBuyerConsolLeadShipment(ForwardingConsol consol, OrgHeader consignor, OrgHeader consignee, decimal weight = 0m, decimal volume = 0m, string transportMode = "SEA", string contractNumberType = null)
		{
			var leadShipment = consol.Shipments.AddNew();
			leadShipment.JS_TransportMode = transportMode;
			leadShipment.JS_PackingMode = ContainerModes.BuyersConsol;
			leadShipment.JS_ShipmentType = ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_INCO = IncoTerms.FreeOnBoard;
			leadShipment.ConsignorPK = consignor.PK;
			leadShipment.ConsigneePK = consignee.PK;
			leadShipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			leadShipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
			leadShipment.JS_ActualWeight = weight;
			leadShipment.JS_ActualVolume = volume;

			if (!string.IsNullOrEmpty(contractNumberType))
			{
				leadShipment.Numbers.RemoveAndDeleteAll();
				var number = leadShipment.Numbers.AddNew();
				number.CE_EntryType = contractNumberType;
				number.CE_EntryNum = "Test";
			}

			return leadShipment;
		}

		public ForwardingConsol CreateBuyersConsolConsol(string origin, string destination, OrgHeader creditor = null, string transportMode = "SEA", string prepaidCollect = "")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = ContainerModes.BuyersConsol;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;

			if (creditor != null)
			{
				consol.CreditorPK = creditor.PK;
			}

			consol.JK_PrepaidCollect = prepaidCollect;

			return consol;
		}

		#endregion

		public static ForwardingShipment CreateForwardingShipment(BusinessObjectFactory factory, string transportMode, string packingMode, ZGuid consignorPK, ZGuid consigneePK, string origin, string destination, decimal weight, decimal volume = 0m, CommonConsol consol = null)
		{
			var shipment = factory.New<ForwardingShipment>();

			if (consol != null)
			{
				consol.Shipments.Add(shipment);
			}

			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = packingMode;
			shipment.ConsignorPK = consignorPK;
			shipment.ConsigneePK = consigneePK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_ActualWeight = weight;
			shipment.JS_ActualVolume = volume;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			return shipment;
		}

		internal static ForwardingConsol CreateForwardingConsol(string transportMode, string origin, string destination, OrgHeader transportProvider, ForwardingShipment shipment, string prepaidCollect = "")
		{
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00023421";
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = transportProvider.PK;
			consol.JK_OA_ShippingLineAddress = transportProvider.MainAddress.PK;
			consol.JK_PrepaidCollect = prepaidCollect;

			return consol;
		}

		static public IRegistryItem FindRegistryItemByName(string registryItemSetName, string registryItemName)
		{
			var registryItemSet = (RegistryItemSet)ObjectFactory.Get(registryItemSetName);
			return registryItemSet.FindByName(registryItemName);
		}

		public T LoadInNewFactory<T>(T bizO) where T : BusinessObject
		{
			return new BusinessObjectFactory().Load<T>(bizO.PK);
		}

		static string RateEntryDisplay(IRateEntry rateEntry) => $"{rateEntry.TI_RateCategory}-{rateEntry.TI_Mode}";

		public static void AssertRateEntries(IEnumerable<IRateEntry> expectedRateEntries, IEnumerable<IRateEntry> actualRateEntries, string message = default)
		{
			Converter<IRateEntry, string> displayTextProvider = (rateEntry) => TestHelper.RateEntryDisplay(rateEntry);
			Assertion.AssertContainsExactElementsInAnyOrder
			(
				message,
				new RateEntryComparer(),
				displayTextProvider,
				expectedRateEntries,
				actualRateEntries
			);
		}

		static string RateLineDisplay(IRateLine rateLine) => $"Category: {rateLine.ParentRateEntry.TI_RateCategory}, Mode: {rateLine.ParentRateEntry.TI_Mode}, Charge: {rateLine.ChargeCode.AC_Code}";

		public static void AssertRateLines(IEnumerable<IRateLine> expectedRateLines, IEnumerable<IRateLine> actualRateLines, string message = default)
		{
			Converter<IRateLine, string> displayTextProvider = (rateLine) => TestHelper.RateLineDisplay(rateLine);
			Assertion.AssertContainsExactElementsInAnyOrder
			(
				message,
				new RateLineComparer(),
				displayTextProvider,
				expectedRateLines,
				actualRateLines
			);
		}

		static string RatingHeaderDisplay(IRatingHeader ratingHeader) => $"Client: {ratingHeader.Header.OH_Code}";

		public static void AssertRatingHeaders(IEnumerable<IRatingHeader> expectedRatingHeaders, IEnumerable<IRatingHeader> actualRatingHeaders, string message = default)
		{
			Converter<IRatingHeader, string> displayTextProvider = (ratingHeader) => TestHelper.RatingHeaderDisplay(ratingHeader);
			Assertion.AssertContainsExactElementsInAnyOrder
			(
				message,
				new RatingHeaderComparer(),
				displayTextProvider,
				expectedRatingHeaders,
				actualRatingHeaders
			);
		}

		public static void SetChargeableFactorRegistry(ChargeableFactorRegistryItem chargeableFactorRegistryItem, decimal metricFactor, string metricNumeratorUnit, string metricDenominatorUnit, decimal imperialFactor, string imperialNumeratorUnit, string imperialDenominatorUnit)
		{
			chargeableFactorRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(metricFactor, metricNumeratorUnit, metricDenominatorUnit),
				new ConversionFactor(imperialFactor, imperialNumeratorUnit, imperialDenominatorUnit)));
		}
	}

	class RateEntryComparer : IEqualityComparer<IRateEntry>
	{
		public bool Equals(IRateEntry b1, IRateEntry b2) => b1.PK == b2.PK;

		public int GetHashCode(IRateEntry entry) => entry.PK.GetHashCode();
	}

	class RateLineComparer : IEqualityComparer<IRateLine>
	{
		public bool Equals(IRateLine b1, IRateLine b2) => b1.PK == b2.PK;

		public int GetHashCode(IRateLine rateLine) => rateLine.PK.GetHashCode();
	}

	class RatingHeaderComparer : IEqualityComparer<IRatingHeader>
	{
		public bool Equals(IRatingHeader b1, IRatingHeader b2) => b1.PK == b2.PK;

		public int GetHashCode(IRatingHeader ratingHeader) => ratingHeader.PK.GetHashCode();
	}

	public class TestLogger : ElementaryLogger
	{
		protected override void LogCore(LogType type, string message, Exception ex)
		{
			string prefix;

			switch (type)
			{
				case LogType.Error:
					prefix = "Error:";
					break;
				case LogType.Warning:
					prefix = "Warning:";
					break;
				case LogType.Information:
					prefix = "Info:";
					break;
				case LogType.Debug:
					prefix = "Debug:";
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			base.LogCore(type, prefix + message, ex);
		}

		public override string ToString()
		{
			var errors = string.Join("\r\n", Debugs);
			var warnings = string.Join("\r\n", Warnings);
			var infos = string.Join("\r\n", Infos);
			var debugs = string.Join("\r\n", Debugs);

			return string.Join("\r\n", errors, warnings, infos, debugs);
		}
	}

	#region TestContainers

	public class TestContainers
	{
		public TestContainers(BusinessObjectFactory factory, params object[] @params)
		{
			for (var i = 0; i < @params.Length; i += 2)
			{
				ZGuid refContainerPK;
				if (@params[i] is ZString)
				{
					refContainerPK = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, (ZString)@params[i]).PK;
				}
				else if (@params[i] is string)
				{
					refContainerPK = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, (string)@params[i]).PK;
				}
				else if (@params[i] is ZGuid)
				{
					refContainerPK = (ZGuid)@params[i];
				}
				else if (@params[i] is Guid)
				{
					refContainerPK = (Guid)@params[i];
				}
				else
				{
					throw new NotSupportedException("TestContainers received currently-unsupported parameters");
				}

				MeasureInfo.ContainerInfo[] containers;
				if (refContainerPK != MeasureInfo.ContainerInfo.LCL)
				{
					var count = (int)@params[i + 1];
					containers = new[] { new MeasureInfo.ContainerInfo(containerCount: count) };
				}
				else
				{
					containers = new[] { new MeasureInfo.ContainerInfo((decimal)@params[i + 1], Constants.Weight.Kilograms, (decimal)@params[i + 2], Constants.Volume.CubicMetres, 0, 0, "CONT0000" + i, containerCount: 0) };
					++i;
				}

				containerList.Add((refContainerPK, containers));
			}
		}

		public TestContainers(BusinessObjectFactory factory, string containerType, MeasureInfo.ContainerInfo[] containerInfos)
		{
			var refContainerPK = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			containerList.Add((refContainerPK, containerInfos));
		}

		public void PopulateContainerList(RateableMeasureSet measures)
		{
			measures.RemoveContainerList();
			foreach (var containerGroup in containerList)
			{
				measures.AddContainerGroup(containerGroup.refContainerPK, containerGroup.containerInfos);
			}
		}

		readonly List<(ZGuid refContainerPK, MeasureInfo.ContainerInfo[] containerInfos)> containerList = new List<(ZGuid refContainerPK, MeasureInfo.ContainerInfo[] containerInfos)>();
	}

	#endregion

	#region Rate Transport

	public static class RateTransportExtensionForTest
	{
		#region TransportZones

		public static RateTransportZone CreateRateTransportZoneForTest(this RateTransportProvider zoneSet, ZString zoneName)
		{
			var zone = zoneSet.Zones.AddNew();
			zone.TZ_ZoneName = zoneName;

			return zone;
		}

		#endregion

		#region Zone Items

		public static RateTransportZoneItem CreateRateTransportZoneItemForTest(this RateTransportZone zone, RefCityTown cityTown)
		{
			var zoneItem = zone.Items.AddNew();

			zoneItem.TQ_RN_NKCountry = cityTown.R9_RN_NKCountry;
			zoneItem.TQ_R9_CityTown = cityTown.PK;

			return zoneItem;
		}

		public static RateTransportZoneItem CreateRateTransportZoneItemForTest(this RateTransportZone zone, RefPostCode fromPostCode)
		{
			var zoneItem = zone.Items.AddNew();

			zoneItem.TQ_RN_NKCountry = fromPostCode.RK_RN_NKCountry;
			zoneItem.TQ_FromPostCode = fromPostCode.RK_CityTownPostCode;

			return zoneItem;
		}

		public static RateTransportZoneItem CreateRateTransportZoneItemForTest(this RateTransportZone zone, int fromDistance, int toDistance)
		{
			var zoneItem = zone.Items.AddNew();

			zoneItem.TQ_FromDistance = fromDistance;
			zoneItem.TQ_ToDistance = toDistance;

			return zoneItem;
		}

		public static RateTransportZoneItem CreateRateTransportZoneItemForTest(this RateTransportZone zone, RefPostCode fromPostCode, RefPostCode toPostCode)
		{
			var zoneItem = zone.Items.AddNew();

			zoneItem.TQ_FromPostCode = fromPostCode.RK_CityTownPostCode;
			zoneItem.TQ_ToPostCode = toPostCode.RK_CityTownPostCode;

			return zoneItem;
		}

		#endregion
	}

	#endregion
}
