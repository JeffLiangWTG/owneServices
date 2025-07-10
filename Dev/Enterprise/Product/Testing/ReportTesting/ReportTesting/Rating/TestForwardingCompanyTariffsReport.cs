namespace Enterprise.ReportTesting.Rating
{
	using System;
	using CargoWise.Types;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;
	using Enterprise.Rating.Business.Testing;
	using NUnit.Framework;
	using static Enterprise.Core.Constants;

	[TemplateName("Forwarding Company Tariffs Report")]
	public class TestForwardingCompanyTariffsReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;

		public void TestTemplate()
		{
			AssertEquals("Report should contain 3 worksheets", 3, Report.XlInterface.WorkSheets.Count);

			AssertMultilineASCIIEquals
			(
				"Worksheet 1",
				@"{A}-[#config]
{A}-[PageStyle=Landscape]
{A}-[EmailSubject=<CompanyName> Company Tariffs Summary Report <Now>]
{A}-[Name=Company Tariffs Summary Report]
{A}-[Data:ReportData=SELECT (FrtMode + TransportMode + OrigDestPair + ISNULL(ServiceLevel, '-') + ISNULL(CommodityCode, '-') + ISNULL(ContainerCode, '-')) As RateGroup, (LineChargeCodeCode + LineDesc) As LineChargeCodeCodeAndLineDescription, Report_ClientRates.[GLOBALRATEDESCRIPTION], [LINEITEMTYPEDESCRIPTION], [LINEITEMBREAKISZEROREPLACEDWITHNULL], [LINEITEMVALUE], [LINEITEMTEXT], [LINECHARGECODECODE], [LINEDESC], [LINEWEIGHTVOLUME], [CALCULATORDESCRIPTION], [FRTMODE], [TRANSPORTMODE], [ORIGINLRC], [DESTINATIONLRC], [RATESTARTDATE], [RATEENDDATE], [SERVICELEVEL], [COMMODITYCODE], [CONTAINERCODE], [CURRENCY], [LINECURRENCY], [CARRIERCODE], [SUPPLIERCODE], [VALUECURRENCY], [DECIMALPLACES], [LINEITEMVALUEORBREAKMINIMUM], [LINEITEMFLATAMOUNTISZEROREPLACEDWITHNULL], [COMPANYPK], [PUBLISHED] FROM Report_ClientRates(<Transport>, null, <CurrentCompany>, <Show Global>) AS Report_ClientRates|>WHERE RateType = 'GLB' AND (CompanyPK = <CurrentCompany> OR (CompanyPK is null and <Show Global> = 'Y'))|>AND (((CASE WHEN LEN(<Rates At>) = 0 THEN GetDate() ELSE <Rates At> END) BETWEEN ISNULL(RateStartDate, '19000101') AND ISNULL(RateEndDate, '20500101')) OR ((CASE WHEN <Show Expired> = 'Y' THEN (CASE WHEN LEN(<Rates At>) = 0 THEN GetDate() ELSE <Rates At> END) END) > RateEndDate))]
{A}-[Data:OriginPortData=SELECT RL_Code As OriginPort FROM RefUNLOCO WHERE RL_PK = <Origin>]
{A}-[Data:DestinationPortData=SELECT RL_Code As DestinationPort FROM RefUNLOCO WHERE RL_PK = <Destination>]
{A}-[Data:SupplierData=SELECT RTRIM(OH_Code) As SupplierCode, OH_FullName As SupplierFullName from OrgHeader WHERE OH_PK = <Supplier>]
{A}-[Data:ChargeCodeData=SELECT AC_CODE As ChargeCode FROM AccChargeCode where  AC_PK = <Charge Code>]
{A}-[DataContext=None]

{A}-[#DocumentHeader]
{B}-[<Image(CompanyLogo, 8, 6)>]







{B}-[<CompanyName>]
{B}-[Company Tariffs Summary Report]
{B}-[Company Tariff Level : 1 Level Description: <ReportData.GlobalRateDescription>]
{B}-[Rates As Of: <Rates At>, Show Expired: <Show Expired>, Orig: <OriginPortData.OriginPort>, Dest.: <DestinationPortData.DestinationPort>, Charge Code: <ChargeCodeData.ChargeCode>, Supplier: <SupplierData.SupplierCode>, Trans: <Transport>, Calculator: <Calculator>]
{B}-[Printed by: <Login Full Name>  <DateTimeAsString('<Now>','dd-MMM-yy hh:mm tt')>]   {N}-[Page:  <CurrentPage> of <TotalPages>]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Curr]   {L}-[Carrier]   {M}-[Supplier]   {N}-[Published]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Value]   {I}-[Flat]
{A}-[#PageHeader:StartFromSecondPage]
{B}-[<CompanyName> - Company Tariffs Summary Report]
{B}-[Company Tariff Level : 1 Level Description: <ReportData.GlobalRateDescription>]
{B}-[Rates As Of: <Rates At>, Show Expired: <Show Expired>, Orig: <OriginPortData.OriginPort>, Dest.: <DestinationPortData.DestinationPort>, Charge Code: <ChargeCodeData.ChargeCode>, Supplier: <SupplierData.SupplierCode>, Trans: <Transport>, Calculator: <Calculator>]
{B}-[Printed by: <Login Full Name>  <DateTimeAsString('<Now>','dd-MMM-yy hh:mm tt')>]   {N}-[Page:  <CurrentPage> of <TotalPages>]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Curr]   {L}-[Carrier]   {M}-[Supplier]   {N}-[Published]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Value]   {I}-[Flat]
{A}-[#SectionBody:Data=ReportData]
{F}-[<ReportData.LineItemTypeDescription>]   {G}-[<ReportData.LineItemBreakIsZeroReplacedWithNull>]   {H}-[<FormatNumber(<ReportData.LineItemValueOrBreakMinimum>, <IF(""<ReportData.ValueCurrency>"" != """", ""<ReportData.ValueCurrency>"", ""<ReportData.DecimalPlaces>"")>)>]   {I}-[<FormatNumber(<ReportData.LineItemFlatAmountIsZeroReplacedWithNull>, <ReportData.LineCurrency>)>]   {J}-[<ReportData.LineItemText>]   {N}-[<ReportData.Published>]
{A}-[#GroupBy:ReportData.LineChargeCodeCodeAndLineDescription: GroupTitle]
{B}-[<ReportData.LineChargeCodeCode>]   {C}-[<AutoHeight><ReportData.LineDesc>]   {D}-[<ReportData.LineWeightVolume>]   {E}-[<AutoHeight><ReportData.CalculatorDescription>]
{A}-[#GroupBy:ReportData.RateGroup: GroupTitle]
{B}-[<ReportData.FrtMode>]   {C}-[<ReportData.TransportMode>]   {D}-[<ReportData.OriginLRC>]   {E}-[<ReportData.DestinationLRC>]   {F}-[<ReportData.RateStartDate>]   {G}-[<ReportData.RateEndDate>]   {H}-[<ReportData.ServiceLevel>]   {I}-[<ReportData.CommodityCode>]   {J}-[<ReportData.ContainerCode>]   {K}-[<ReportData.Currency>]   {L}-[<ReportData.CarrierCode>]   {M}-[<ReportData.SupplierCode>]
{A}-[#GroupBy:ReportData.RateGroup]

{A}-[#SectionFooter]

{A}-[#endofreport]",
				Report.XlInterface.WorkSheets[0].ToString()
			);

			AssertMultilineASCIIEquals
			(
				"Worksheet 2",
				@"{A}-[Transport]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Style]   {C}-[DropDown]
{B}-[Option]   {C}-[ALL]   {D}-[Transport Mode ALL]
{C}-[AIR]   {D}-[Air Freight (LSE and ULD)]
{C}-[LSE]   {D}-[Air Freight (LSE)]
{C}-[ULD]   {D}-[Air Freight (ULD)]
{C}-[SEA]   {D}-[Sea Freight (LCL and FCL)]
{C}-[LCL]   {D}-[Sea Freight (LCL)]
{C}-[FCL]   {D}-[Sea Freight (FCL)]
{C}-[SHP]   {D}-[Sea Shipping (Containerized and Non-Containerized)]
{C}-[SNC]   {D}-[Sea Shipping (Non-Containerized)]
{C}-[SCO]   {D}-[Sea Shipping (Containerized)]
{C}-[RAI]   {D}-[Rail Freight (LCL, FCL and FWL)]
{C}-[FRA]   {D}-[Rail Freight (FCL)]
{C}-[LWL]   {D}-[Rail Freight (LCL)]
{C}-[FWL]   {D}-[Rail Freight (FWL)]
{C}-[ROA]   {D}-[Road Freight (LCL, FCL and FTL)]
{C}-[FRO]   {D}-[Road Freight (FCL)]
{C}-[LTL]   {D}-[Road Freight (LCL)]
{C}-[FTL]   {D}-[Road Freight (FTL)]
{C}-[MAI]   {D}-[Post]
{C}-[WHS]   {D}-[Warehouse]
{A}-[Calculator]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Field]   {C}-[LineCalculator]
{B}-[Style]   {C}-[DropDown]
{B}-[Option]   {C}-[AGY]   {D}-[Agency Calculator]
{C}-[CMB]   {D}-[Sliding or Per Unit with Base, Minimum, Maximum Calculator]
{C}-[CST]   {D}-[Cost Based Calculator]
{C}-[CTB]   {D}-[Company Tariff Based Calculator]
{C}-[CTG]   {D}-[Cartage Calculator]
{C}-[FPA]   {D}-[First plus Additional Calculator]
{C}-[FLT]   {D}-[Flat Calculator]
{C}-[FPU]   {D}-[Flat plus Per Unit Calculator]
{C}-[IAT]   {D}-[Italian Airport Tax Calculator]
{C}-[INV]   {D}-[Invoice Value Calculator]
{C}-[IXC]   {D}-[Value of Goods Range Calculator]
{C}-[MPU]   {D}-[Minimum Or Per Unit Calculator]
{C}-[NTE]   {D}-[Note Calculator]
{C}-[PER]   {D}-[Percentage Calculator]
{C}-[UNT]   {D}-[Unit Calculator]
{C}-[WTR]   {D}-[Weight Range Calculator]
{C}-[WRU]   {D}-[Weight Range then Per Unit Calculator]
{C}-[TME]   {D}-[Time Calculator]
{C}-[HRT]   {D}-[House bill Release Type Calculator]
{C}-[WPK]   {D}-[Warehouse Pack Type Calculator]
{C}-[WLT]   {D}-[Warehouse Location Type Calculator]
{A}-[Supplier]   {B}-[Type]   {C}-[organisation Lookup]
{B}-[Field]   {C}-[SupplierPK]
{A}-[Origin]   {B}-[Type]   {C}-[unloco Lookup]
{B}-[Field]   {C}-[OriginLRCPK]

{A}-[Destination]   {B}-[Type]   {C}-[unloco Lookup]
{B}-[Field]   {C}-[DestinationLRCPK]
{A}-[Charge Code]   {B}-[Type]   {C}-[charge code lookup]
{B}-[Field]   {C}-[LocalChargePK]

{A}-[Rates At]   {B}-[Type]   {C}-[date]

{A}-[Show Expired]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Expired Rates]

{A}-[Show Global]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Global Rates]

{A}-[#End]",
				Report.XlInterface.WorkSheets[1].ToString()
			);

			AssertMultilineASCIIEquals
			(
				"Worksheet 3",
				@"{A}-[Start Date, Expiry Date]   {B}-[RateStartDate, RateEndDate, FrtMode, TransportMode,LineItemBreak,LineItemType]

{A}-[#end]",
				Report.XlInterface.WorkSheets[2].ToString()
			);
		}

		[TestDate(2020, 01, 01)]
		public void TestPercentageCalculatorPercentageFormatting()
		{
			var helper = new TestHelper(Factory);

			var tariff = helper.NewCompanyTariff();
			var rateEntry = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", "FRT", 11.12345m, currency: CurrencyCodes.LibyanArabJamahiriya);
			var rateLine = rateEntry.AddRateLine("BAF", PercentageCalculator.Code, currencyCode: "LYD");
			rateLine.GetCalculator<PercentageCalculator>().Percent = 12.12345;
			var rateLineItem = rateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rateLineItem.TM_Text = "ALL";

			tariff.Factory.Save();

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "1" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				PrepareReportForRender();
				((IValueAsStringProviderForUnitTests)Report.FilterCollection["Transport"]).ValueAsStringForSerialisation = "ALL";
				((DateField)Report.FilterCollection["Rates At"]).Value = ZDateTime.Today;
				AssertWorkSheet0
				(
					expectedOutput: @"{B}-[Eagle Datamation International]
{B}-[Company Tariffs Summary Report]
{B}-[Company Tariff Level : 1 Level Description: Base Company Tariff]
{B}-[Rates As Of: 1/1/2020 12:00:00 AM, Trans: ALL, ]
{B}-[Printed by: CargoWise Support  01-Jan-20 12:00 AM]   {N}-[Page:  1 of 1]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Curr]   {L}-[Carrier]   {M}-[Supplier]   {N}-[Published]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Value]   {I}-[Flat]
{B}-[AIR]   {C}-[LSE]   {D}-[AUSYD]   {E}-[USLAX]   {F}-[43831]   {G}-[44013]   {I}-[GEN]   {K}-[AUD]
{B}-[BAF]   {C}-[Bunker Adjustment Factor]   {E}-[Percentage Calculator]
{F}-[Percentage Applies To]   {H}-[0.000]   {J}-[ALL]   {N}-[Local]
{F}-[Basic Charge]   {H}-[0.000]   {N}-[Local]
{F}-[The Greater Charge]   {H}-[0.000]   {N}-[Local]
{F}-[IGS]   {H}-[0.000]   {N}-[Local]
{F}-[Maximum]   {H}-[0.000]   {N}-[Local]
{F}-[Minimum]   {H}-[0.000]   {N}-[Local]
{F}-[Percent First]   {H}-[12.1]   {N}-[Local]
{F}-[Part There Of]   {H}-[0.000]   {N}-[Local]
{F}-[Agency Rate]   {H}-[0.000]   {N}-[Local]
{F}-[Value or Part Thereof]   {H}-[0.000]   {N}-[Local]
{B}-[FRT]   {C}-[International Freight]   {E}-[Flat Calculator]
{F}-[Basic Charge]   {H}-[11.124]   {N}-[Local]",
					message: "Percentage should be formatted from RateLine currency"
				);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestPercentageBreakCalculator()
		{
			var helper = new TestHelper(Factory);

			var tariff = helper.NewCompanyTariff();

			var rateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("BAF", PercentageBreaksCalculator.Code, "KG", CurrencyCodes.LibyanArabJamahiriya);
			var rateLineCalculator = rateLine.GetCalculator<PercentageBreaksCalculator>();
			rateLineCalculator["MIN"] = (ZDecimal)1000.1234;
			rateLineCalculator["MAX"] = (ZDecimal)1100.1234;
			rateLineCalculator["BAS"] = (ZDecimal)1200.1234;
			rateLineCalculator.AddApplyToItem(CalculatorConstants.Text.AllCharges);

			rateLineCalculator["-1.2"] = (ZDecimal)0m; // PEB doesn't use rate column hence the set value is not used.
			var rateLineItemMinus = rateLine.RateLineItems[rateLine.RateLineItems.Count - 1];
			rateLineItemMinus.TM_BreakMinimum = 10.1234m;
			rateLineItemMinus.TM_FlatAmount = 100.1234m;

			rateLineCalculator["+1.2"] = (ZDecimal)0m; // PEB doesn't use rate column hence the set value is not used.
			var rateLineItemPlus = rateLine.RateLineItems[rateLine.RateLineItems.Count - 1];
			rateLineItemPlus.TM_BreakMinimum = 20.1234m;
			rateLineItemPlus.TM_FlatAmount = 200.1234m;

			tariff.Factory.Save();

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "1" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				PrepareReportForRender();
				((IValueAsStringProviderForUnitTests)Report.FilterCollection["Transport"]).ValueAsStringForSerialisation = "ALL";
				((DateField)Report.FilterCollection["Rates At"]).Value = ZDateTime.Today;
				AssertWorkSheet0
				(
					expectedOutput: @"{B}-[Eagle Datamation International]
{B}-[Company Tariffs Summary Report]
{B}-[Company Tariff Level : 1 Level Description: Base Company Tariff]
{B}-[Rates As Of: 1/1/2020 12:00:00 AM, Trans: ALL, ]
{B}-[Printed by: CargoWise Support  01-Jan-20 12:00 AM]   {N}-[Page:  1 of 1]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Curr]   {L}-[Carrier]   {M}-[Supplier]   {N}-[Published]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Value]   {I}-[Flat]
{B}-[AIR]   {C}-[LSE]   {D}-[AUSYD]   {E}-[USLAX]   {F}-[43831]   {G}-[44013]   {I}-[GEN]   {K}-[AUD]
{B}-[BAF]   {C}-[Bunker Adjustment Factor]   {D}-[KG]   {E}-[PEB]
{F}-[Use Accumulated]   {H}-[0.000]   {J}-[N]   {N}-[Local]
{F}-[Percentage Applies To]   {H}-[0.000]   {J}-[ALL]   {N}-[Local]
{F}-[Basic Charge]   {H}-[1,200.123]   {N}-[Local]
{F}-[BBV]   {H}-[0.000]   {N}-[Local]
{F}-[BRP]   {H}-[0.000]   {N}-[Local]
{F}-[Higher Chargeable, Lower Rate]   {H}-[0.000]   {J}-[N]   {N}-[Local]
{F}-[IGS]   {H}-[0.000]   {N}-[Local]
{F}-[Inclusive Breaks]   {H}-[0.000]   {J}-[N]   {N}-[Local]
{F}-[Maximum]   {H}-[1,100.123]   {N}-[Local]
{F}-[Minimum]   {H}-[1,000.123]   {N}-[Local]
{F}-[+  ]   {G}-[1.2]   {H}-[20.1]   {I}-[200.123]   {N}-[Local]
{F}-[-  ]   {G}-[1.2]   {H}-[10.1]   {I}-[100.123]   {N}-[Local]",
					message: "Percentage should be formatted from RateLine currency"
				);
			}
		}
	}
}
