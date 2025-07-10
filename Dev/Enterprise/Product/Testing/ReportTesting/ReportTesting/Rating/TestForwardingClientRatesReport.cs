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

	[TemplateName("Forwarding Client Rates Report")]
	class TestForwardingClientRatesReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;

		public void TestTemplate()
		{
			AssertEquals("Report should contain 3 worksheets", 3, Report.XlInterface.WorkSheets.Count);

			AssertMultilineASCIIEquals
			(
				"Worksheet 1",
				@"{A}-[#config]
{A}-[PageStyle=Continuous]
{A}-[EmailSubject=<CompanyName> Client Rates Summary Report <Now>]
{A}-[Name=Client Rates Summary Report]
{A}-[Data:ReportData=Select <ReportData.SelectList> From  Report_ClientRatesSummary(<CurrentCompany>,<Rates At>,<Now>,<Show Expired>,<Transport>,<CurrentBranch>,<Show Global>)]
{A}-[Data:ClientData=SELECT RTRIM(OH_Code) As ClientCode, RTRIM(OH_FullName) As ClientFullName FROM OrgHeader WHERE OH_PK = <Client>]
{A}-[Data:SupplierData=SELECT RTRIM(OH_Code) As SupplierCode, OH_FullName As SupplierFullName from OrgHeader WHERE OH_PK = <Supplier>]
{A}-[Data:ChargeCodeData=SELECT AC_CODE As ChargeCode FROM AccChargeCode where  AC_PK = <Charge Code>]
{A}-[DataContext=None]
{A}-[#DocumentHeader]
{B}-[<Image(CompanyLogo, 1, 7)>]
{B}-[<CompanyName>]
{B}-[Client Rates Summary Report]

{B}-[Client : <ClientData.ClientFullName> (<ClientData.ClientCode>)]   {H}-[CFX (Air)  Imp:]   {I}-[<ReportData.RawAirCFX>]   {K}-[CFX (Sea)  Imp:]   {L}-[<ReportData.RawSeaCFX>]   {M}-[Company Tariff:]   {N}-[Level <ReportData.ClientGlobalRateLevel>]
{B}-[Supplier:<SupplierData.SupplierCode>, Trans: <Transport> Calculator: <Calculator>]   {H}-[CFX (Air) Exp:]   {I}-[<ReportData.RawExportAirCFX>]   {K}-[CFX (Sea) Exp:]   {L}-[<ReportData.RawExportSeaCFX>]
{B}-[Rates As Of: <Rates At>, Show Expired: <Show Expired>, Client Override Rates Only, Orig: <Origin>, Dest.: <Destination>, Charge Code: <ChargeCodeData.ChargeCode> ]
{B}-[Printed by: <Login Full Name>  <DateTimeAsString('<Now>','dd-MMM-yy hh:mm tt')>]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Carrier]   {M}-[Supplier]   {N}-[Published]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Curr]   {I}-[Value]   {J}-[Flat]
{A}-[#SectionBody:Data=ReportData]
{F}-[<ReportData.LineItemTypeDescription>]   {G}-[<ReportData.LineItemBreakIsZeroReplacedWithNull>]   {H}-[<ReportData.LineCurrency>]   {I}-[<FormatNumber(<ReportData.LineItemValueOrBreakMinimum>, <IF(""<ReportData.ValueCurrency>"" != """", ""<ReportData.ValueCurrency>"", ""<ReportData.DecimalPlaces>"")>)>]   {J}-[<FormatNumber(<ReportData.LineItemFlatAmountIsZeroReplacedWithNull>, <ReportData.LineCurrency>)>]   {K}-[<ReportData.LineItemText>]   {N}-[<ReportData.Published>]
{A}-[#GroupBy:ReportData.LineChargeCodeCodeAndLineDescription: GroupTitle]
{B}-[<ReportData.LineChargeCodeCode>]   {C}-[<AutoHeight><ReportData.LineDesc>]   {D}-[<ReportData.LineWeightVolume>]   {E}-[<AutoHeight><ReportData.CalculatorDescription>]
{A}-[#GroupBy:ReportData.RateGroup: GroupTitle]
{B}-[<ReportData.FrtMode>]   {C}-[<ReportData.TransportMode>]   {D}-[<ReportData.OriginLRC>]   {E}-[<ReportData.DestinationLRCV>]   {F}-[<ReportData.RateStartDate>]   {G}-[<ReportData.RateEndDate>]   {H}-[<ReportData.ServiceLevel>]   {I}-[<ReportData.CommodityCode>]   {J}-[<ReportData.ContainerCode>]   {K}-[<ReportData.CarrierCode>]   {M}-[<ReportData.SupplierCode>]
{A}-[#GroupBy:ReportData.RateGroup]

{A}-[#SectionFooter]

{A}-[#endofreport]",
				Report.XlInterface.WorkSheets[0].ToString()
			);

			AssertMultilineASCIIEquals
			(
				"Worksheet 2",
				@"{A}-[Client]   {B}-[Type]   {C}-[organisation Lookup]
{B}-[Field]   {C}-[ClientPK]
{B}-[Required]

{A}-[Supplier]   {B}-[Type]   {C}-[organisation Lookup]
{B}-[Field]   {C}-[SupplierPK]

{A}-[Origin]   {B}-[Type]   {C}-[location Lookup code]
{B}-[Field]   {C}-[OriginLRC]

{A}-[Destination]   {B}-[Type]   {C}-[location Lookup code]
{B}-[Field]   {C}-[DestinationLRC]

{A}-[Charge Code]   {B}-[Type]   {C}-[charge code lookup]
{B}-[Field]   {C}-[LocalChargePK]

{A}-[Transport]   {B}-[Type]   {C}-[MultipleChoice]
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

{A}-[Rates At]   {B}-[Type]   {C}-[date]

{A}-[Show Expired]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Expired Rates]

{A}-[Client Override]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Client Override Rates Only]
{B}-[Field]   {C}-[ClientHasOverride]

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
			var client = helper.NewOrgHeader();

			var clientRate = helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 11.12345m, currency: CurrencyCodes.LibyanArabJamahiriya);
			var rateLine = rateEntry.AddRateLine("BAF", PercentageCalculator.Code, currencyCode: CurrencyCodes.LibyanArabJamahiriya);
			rateLine.GetCalculator<PercentageCalculator>().Percent = 12.12345;
			var rateLineItem = rateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rateLineItem.TM_Text = "ALL";

			Factory.Save();

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "1" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				PrepareReportForRender();
				((LookupField)Report.FilterCollection["Client"]).ZValue = client.PK;
				AssertWorkSheet0
				(
					expectedOutput: @"{B}-[Eagle Datamation International]
{B}-[Client Rates Summary Report]

{B}-[Client : Test Client #1 (TESTORG1)]   {H}-[CFX (Air)  Imp:]   {I}-[0]   {K}-[CFX (Sea)  Imp:]   {L}-[0]   {M}-[Company Tariff:]   {N}-[Level ]
{H}-[CFX (Air) Exp:]   {I}-[0]   {K}-[CFX (Sea) Exp:]   {L}-[0]
{B}-[Rates As Of: 1/1/2020 12:00:00 AM, ]
{B}-[Printed by: CargoWise Support  01-Jan-20 12:00 AM]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Carrier]   {M}-[Supplier]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Curr]   {I}-[Value]   {J}-[Flat]
{B}-[AIR]   {C}-[LSE]   {D}-[AUSYD]   {E}-[USLAX]   {F}-[43831]   {G}-[44013]   {I}-[GEN]
{B}-[BAF]   {C}-[Bunker Adjustment Factor]   {E}-[Percentage Calculator]
{F}-[Percentage Applies To]   {H}-[LYD]   {I}-[0.000]   {K}-[ALL]   {N}-[Local]
{F}-[Basic Charge]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[The Greater Charge]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[IGS]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[Maximum]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[Minimum]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[Percent First]   {H}-[LYD]   {I}-[12.1]   {N}-[Local]
{F}-[Part There Of]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[Agency Rate]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[Value or Part Thereof]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{B}-[FRT]   {C}-[International Freight]   {E}-[Flat Calculator]
{F}-[Basic Charge]   {H}-[LYD]   {I}-[11.124]   {N}-[Local]",
					message: "Percentage should be formatted from RateLine currency"
				);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestPercentageBreakCalculator()
		{
			var helper = new TestHelper(Factory);
			var client = helper.NewOrgHeader();

			var clientRate = helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
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

			Factory.Save();

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "1" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				PrepareReportForRender();
				((LookupField)Report.FilterCollection["Client"]).ZValue = client.PK;
				AssertWorkSheet0
				(
					expectedOutput: @"{B}-[Eagle Datamation International]
{B}-[Client Rates Summary Report]

{B}-[Client : Test Client #1 (TESTORG1)]   {H}-[CFX (Air)  Imp:]   {I}-[0]   {K}-[CFX (Sea)  Imp:]   {L}-[0]   {M}-[Company Tariff:]   {N}-[Level ]
{H}-[CFX (Air) Exp:]   {I}-[0]   {K}-[CFX (Sea) Exp:]   {L}-[0]
{B}-[Rates As Of: 1/1/2020 12:00:00 AM, ]
{B}-[Printed by: CargoWise Support  01-Jan-20 12:00 AM]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Carrier]   {M}-[Supplier]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Curr]   {I}-[Value]   {J}-[Flat]
{B}-[AIR]   {C}-[LSE]   {D}-[AUSYD]   {E}-[USLAX]   {F}-[43831]   {G}-[44013]   {I}-[GEN]
{B}-[BAF]   {C}-[Bunker Adjustment Factor]   {D}-[KG]   {E}-[PEB]
{F}-[Use Accumulated]   {H}-[LYD]   {I}-[0.000]   {K}-[N]   {N}-[Local]
{F}-[Percentage Applies To]   {H}-[LYD]   {I}-[0.000]   {K}-[ALL]   {N}-[Local]
{F}-[Basic Charge]   {H}-[LYD]   {I}-[1,200.123]   {N}-[Local]
{F}-[BBV]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[BRP]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[Higher Chargeable, Lower Rate]   {H}-[LYD]   {I}-[0.000]   {K}-[N]   {N}-[Local]
{F}-[IGS]   {H}-[LYD]   {I}-[0.000]   {N}-[Local]
{F}-[Inclusive Breaks]   {H}-[LYD]   {I}-[0.000]   {K}-[N]   {N}-[Local]
{F}-[Maximum]   {H}-[LYD]   {I}-[1,100.123]   {N}-[Local]
{F}-[Minimum]   {H}-[LYD]   {I}-[1,000.123]   {N}-[Local]
{F}-[+  ]   {G}-[1.2]   {H}-[LYD]   {I}-[20.1]   {J}-[200.123]   {N}-[Local]
{F}-[-  ]   {G}-[1.2]   {H}-[LYD]   {I}-[10.1]   {J}-[100.123]   {N}-[Local]",
					message: "PEB should show correct decimals for -/+ break, percentage and flat amount"
				);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestUnitPercentageChangeCostBasedCalculator()
		{
			var helper = new TestHelper(Factory);

			var client = helper.NewOrgHeader();
			var clientRate = helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CostBasedCode, PkgUnit.Unit, CurrencyCodes.Australia);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10.1234m;
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 20.1234M;

			Factory.Save();

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "3" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				PrepareReportForRender();
				((LookupField)Report.FilterCollection["Client"]).ZValue = client.PK;
				AssertWorkSheet0
				(
					expectedOutput: @"{B}-[Eagle Datamation International]
{B}-[Client Rates Summary Report]

{B}-[Client : Test Client #1 (TESTORG1)]   {H}-[CFX (Air)  Imp:]   {I}-[0]   {K}-[CFX (Sea)  Imp:]   {L}-[0]   {M}-[Company Tariff:]   {N}-[Level ]
{H}-[CFX (Air) Exp:]   {I}-[0]   {K}-[CFX (Sea) Exp:]   {L}-[0]
{B}-[Rates As Of: 1/1/2020 12:00:00 AM, ]
{B}-[Printed by: CargoWise Support  01-Jan-20 12:00 AM]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Carrier]   {M}-[Supplier]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Curr]   {I}-[Value]   {J}-[Flat]
{B}-[AIR]   {C}-[LSE]   {D}-[AUSYD]   {E}-[USLAX]   {F}-[43831]   {G}-[44013]   {I}-[GEN]
{B}-[BAF]   {C}-[Bunker Adjustment Factor]   {D}-[UNT]   {E}-[Cost Based Calculator]
{F}-[Basic Charge]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Equipment Type]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Minimum]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Calculation Order]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Percent First]   {H}-[AUD]   {I}-[10.123]   {N}-[Local]
{F}-[Unit Percentage Change]   {H}-[AUD]   {I}-[20.123]   {N}-[Local]
{F}-[Message Sub Type]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Message Type]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Rate Per Unit]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]",
					message: "Unit Percentage Change should show correct decimals like GUI i.e. based on registry"
				);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestUnitPercentageChangeCompanyTariffBasedCalculator()
		{
			var helper = new TestHelper(Factory);

			var client = helper.NewOrgHeader();
			var clientRate = helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, PkgUnit.Unit, CurrencyCodes.Australia);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10.1234m;
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 20.1234M;

			Factory.Save();

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "3" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				PrepareReportForRender();
				((LookupField)Report.FilterCollection["Client"]).ZValue = client.PK;
				AssertWorkSheet0
				(
					expectedOutput: @"{B}-[Eagle Datamation International]
{B}-[Client Rates Summary Report]

{B}-[Client : Test Client #1 (TESTORG1)]   {H}-[CFX (Air)  Imp:]   {I}-[0]   {K}-[CFX (Sea)  Imp:]   {L}-[0]   {M}-[Company Tariff:]   {N}-[Level ]
{H}-[CFX (Air) Exp:]   {I}-[0]   {K}-[CFX (Sea) Exp:]   {L}-[0]
{B}-[Rates As Of: 1/1/2020 12:00:00 AM, ]
{B}-[Printed by: CargoWise Support  01-Jan-20 12:00 AM]
{B}-[Chg Type]   {C}-[Trans]   {D}-[Orig]   {E}-[Dest.]   {F}-[Start Date]   {G}-[Expiry Date]   {H}-[Srv. Lvl.]   {I}-[Comm]   {J}-[Cont]   {K}-[Carrier]   {M}-[Supplier]
{B}-[Chg Code]   {C}-[Desc.]   {D}-[W/V]   {E}-[Calc.]   {F}-[Type]   {G}-[Break]   {H}-[Curr]   {I}-[Value]   {J}-[Flat]
{B}-[AIR]   {C}-[LSE]   {D}-[AUSYD]   {E}-[USLAX]   {F}-[43831]   {G}-[44013]   {I}-[GEN]
{B}-[BAF]   {C}-[Bunker Adjustment Factor]   {D}-[UNT]   {E}-[Company Tariff Based Calculator]
{F}-[Basic Charge]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Equipment Type]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Minimum]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Calculation Order]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Percent First]   {H}-[AUD]   {I}-[10.123]   {N}-[Local]
{F}-[Unit Percentage Change]   {H}-[AUD]   {I}-[20.123]   {N}-[Local]
{F}-[Message Sub Type]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Message Type]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]
{F}-[Rate Per Unit]   {H}-[AUD]   {I}-[0.00]   {N}-[Local]",
					message: "Unit Percentage Change should show correct decimals like GUI i.e. based on registry"
				);
			}
		}
	}
}
