namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;
	using Enterprise.Rating.Business.Testing;
	using Enterprise.ReportTesting;
	using NUnit.Framework;
	using static Enterprise.Core.Constants;

	[TemplateName("Forwarding Client Rates by Client Report")]
	public class TestForwardingClientRatesbyClientTemplate : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "ForwardingClientRatesSummary" }; }
		}

		public void TestTemplate()
		{
			AssertEquals("Report should contain 3 worksheets", 3, Report.XlInterface.WorkSheets.Count);

			AssertMultilineASCIIEquals
			(
				"Worksheet 1",
				@"{A}-[#config]
{A}-[PageStyle=Continuous]
{A}-[EmailSubject=<CompanyName> Client Rates by Client Summary Report <Now>]
{A}-[Name= Client Rates by Client Summary Report]
{A}-[SheetNameOverride=Forwarding Client Rates by Client]
{A}-[Data:ReportData=SELECT <ReportData.SelectList> FROM Report_ClientRatesByClientSummary(<CurrentCompany>, <Rates At>, <Rate Category>, <Transport Mode>, <Show Expired>, <CurrentBranch>, <Show Global>)]
{A}-[Data:ClientData=SELECT RTRIM(OH_Code) As ClientCode, RTRIM(OH_FullName) As ClientFullName FROM OrgHeader WHERE OH_PK = <Client>]
{A}-[Data:OriginPortData=SELECT RL_Code As OriginPort FROM RefUNLOCO WHERE RL_PK = <Origin>]
{A}-[Data:DestinationPortData=SELECT RL_Code As DestinationPort FROM RefUNLOCO WHERE RL_PK = <Destination>]
{A}-[Data:SupplierData=SELECT RTRIM(OH_Code) As SupplierCode, OH_FullName As SupplierFullName from OrgHeader WHERE OH_PK = <Supplier>]
{A}-[Data:ChargeCodeData=SELECT AC_CODE As ChargeCode FROM AccChargeCode where  AC_PK = <Charge Code>]
{A}-[DataContext=None]
{A}-[Auotheight=ExpandRow]
{A}-[#DocumentHeader]
{B}-[<CompanyName>]
{B}-[ Client Rates by Client Summary Report]
{B}-[Client : <ClientData.ClientFullName> (<ClientData.ClientCode>), Supplier: <SupplierData.SupplierCode>, Calculator: <Calculator>]
{B}-[Rate Category: <Rate Category>, Transport Mode: <Transport Mode>, Pack Mode: <Pack Mode>, Rates As Of: <DateTimeAsString('<Rates At>','dd-MMM-yy')>Show Expired: <Show Expired>, Client Override Rates Only, Charge Code: <ChargeCodeData.ChargeCode> Orig.: <OriginPortData.OriginPort>, Dest.: <DestinationPortData.DestinationPort>, ]
{B}-[Printed by: <Login Full Name>  <DateTimeAsString('<Now>','dd-MMM-yy hh:mm tt')>]
{A}-[<HideRowIf(<Count(ReportData)> &lt; 3000)>]   {B}-[The search filters have rendered results that exceed the capacity of this report. Only the first 3000 records are shown.]
{C}-[Mode]   {D}-[Transport]   {F}-[Orig]   {G}-[Dest.]   {H}-[Start Date]   {I}-[Expiry Date]   {K}-[Srv. Lvl.]   {L}-[Comm]   {M}-[Cont]   {O}-[Carrier]   {P}-[Supplier]   {Q}-[Published]
{D}-[Chg Code]   {E}-[Desc.]   {F}-[W/V]   {G}-[Calc.]   {H}-[Type]   {I}-[Break]   {K}-[Value]   {L}-[Flat]

{A}-[#SectionBody:Data=ReportData:MaximumNumberOfRowsToShow=3000]
{H}-[<ReportData.LineItemTypeDescription>]   {I}-[<ReportData.LineItemBreakIsZeroReplacedWithNull>]   {J}-[<ReportData.LineCurrency>]   {K}-[<FormatNumber(<ReportData.LineItemValueOrBreakMinimum>, <IF(""<ReportData.ValueCurrency>"" != """", ""<ReportData.ValueCurrency>"", ""<ReportData.DecimalPlaces>"")>)>]   {L}-[<FormatNumber(<ReportData.LineItemFlatAmountIsZeroReplacedWithNull>, <ReportData.LineCurrency>)>]   {M}-[<ReportData.LineItemText>]   {Q}-[<ReportData.Published>]
{A}-[#GroupBy:ReportData.LineChargeCodeCodeAndLineDescription: GroupTitle]
{D}-[<ReportData.LineChargeCodeCode>]   {E}-[<AutoHeight><ReportData.LineDesc>]   {F}-[<ReportData.LineWeightVolume>]   {G}-[<ReportData.CalculatorDescription>]
{A}-[#GroupBy:ReportData.RateGroup: GroupTitle]
{C}-[<ReportData.FrtMode>]   {D}-[<ReportData.TransportMode>]   {F}-[<ReportData.OriginLRC>]   {G}-[<ReportData.DestinationLRC>]   {H}-[<ReportData.RateStartDate>]   {I}-[<ReportData.RateEndDate>]   {K}-[<ReportData.ServiceLevel>]   {L}-[<ReportData.CommodityCode>]   {M}-[<ReportData.ContainerCode>]   {O}-[<Autoheight><ReportData.CarrierCode>]   {P}-[<Autoheight><ReportData.SupplierCode>]
{A}-[#GroupBy:ReportData.RateGroup]

{A}-[#GroupBy:ReportData.ClientCode]

{A}-[#GroupBy:ReportData.Client: GroupTitle]
{B}-[Client : <ReportData.ClientFullName> (<ReportData.ClientCode>)]   {J}-[CFX (Air) Import:]   {K}-[<ReportData.RawAirCFX>]   {M}-[CFX (Sea) Import:]   {N}-[<ReportData.RawSeaCFX>]   {O}-[Company Tariff:]   {P}-[<ReportData.ClientGlobalRateLevel>]
{J}-[CFX (Air) Export:]   {K}-[<ReportData.RawExportAirCFX>]   {M}-[CFX (Sea) Export:]   {N}-[<ReportData.RawExportSeaCFX>]
{A}-[#SectionFooter]

{A}-[#endofreport]",
				Report.XlInterface.WorkSheets[0].ToString()
			);

			AssertMultilineASCIIEquals
			(
				"Worksheet 2",
				@"{A}-[Rates At]   {B}-[Type]   {C}-[date]
{B}-[Required]

{A}-[Client]   {B}-[Type]   {C}-[organisation Lookup]
{B}-[Field]   {C}-[ClientPK]

{A}-[Supplier]   {B}-[Type]   {C}-[organisation Lookup]
{B}-[Field]   {C}-[SupplierPK]

{A}-[Rate Category]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Style]   {C}-[DropDown]
{B}-[Option]   {C}-[All]   {D}-[All Rate Categories]
{C}-[Air ]   {D}-[Air Freight Rates Only]
{C}-[FCL]   {D}-[FCL Freight Rates Only]
{C}-[LCL]   {D}-[LCL Freight Rates Only]
{C}-[Origin]   {D}-[Origin Charge Rates Only]
{C}-[Dest]   {D}-[Destination Charge Rates Only]
{B}-[Default]   {C}-[All]

{A}-[Transport Mode]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Style]   {C}-[DropDown]
{B}-[Option]   {C}-[ALL]   {D}-[Only Rates for ALL transport modes]
{C}-[AIR]   {D}-[Only Rates for AIR transport modes]
{C}-[RAIL]   {D}-[Only Rates for RAIL transport modes]
{C}-[ROAD]   {D}-[Only Rates for ROAD transport modes]
{C}-[SEA]   {D}-[Only Rates for SEA transport modes]
{C}-[MAIL]   {D}-[Only Rates for MAIL transport modes]

{A}-[Pack Mode]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Style]   {C}-[DropDown]
{B}-[Option]   {C}-[LSE]   {D}-[Only Rates for LSE pack modes]
{C}-[ULD]   {D}-[Only Rates for ULD pack modes]
{C}-[LCL]   {D}-[Only Rates for LCL pack modes]
{C}-[FCL]   {D}-[Only Rates for FCL pack modes]
{C}-[FTL]   {D}-[Only Rates for FTL pack modes]
{C}-[FWL]   {D}-[Only Rates for FWL pack modes]
{C}-[LRO]   {D}-[Only Rates for LRO pack modes]
{C}-[FRO]   {D}-[Only Rates for FRO pack modes]
{C}-[LRA]   {D}-[Only Rates for LRA pack modes]
{C}-[FRA]   {D}-[Only Rates for FRA pack modes]
{C}-[ALL]   {D}-[Only Rates for ALL pack modes]
{C}-[AIR]   {D}-[Only Rates for AIR pack modes]
{C}-[RAI]   {D}-[Only Rates for RAI pack modes]
{C}-[ROA]   {D}-[Only Rates for ROA pack modes]
{C}-[SEA]   {D}-[Only Rates for SEA pack modes]
{C}-[MAI]   {D}-[Only Rates for MAI pack modes]
{B}-[Field]   {C}-[TransportMode]

{A}-[Show Expired]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Expired Rates]

{A}-[Client Override]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Client Override Rates Only]
{B}-[Field]   {C}-[ClientHasOverride]

{A}-[Show Global]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Global Rates]

{A}-[Charge Code]   {B}-[Type]   {C}-[charge code lookup]
{B}-[Field]   {C}-[LocalChargePK]

{A}-[Calculator]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Field]   {C}-[LineCalculator]
{B}-[Style]   {C}-[DropDown]
{B}-[Option]   {C}-[AGY]   {D}-[Agency Calculator]
{C}-[CMB]   {D}-[Sliding or Per Unit with Base, Minimum, Maximum Calculator]
{C}-[CST]   {D}-[Cost Based Calculator]
{C}-[CTB]   {D}-[Company Tariff Based Calculator]
{C}-[CTG]   {D}-[Cartage Calculator]
{C}-[CTZ]   {D}-[Cartage Zone Distance Calculator]
{C}-[DIN]   {D}-[Disbursement Interest Calculator]
{C}-[EQH]   {D}-[Equipment Hire Calculator]
{C}-[FPA]   {D}-[First plus Additional Calculator]
{C}-[FLT]   {D}-[Flat Calculator]
{C}-[FPU]   {D}-[Flat plus Per Unit Calculator]
{C}-[HRT]   {D}-[House-bill Release Type Calculator]
{C}-[IAT]   {D}-[Pack Count Calculator]
{C}-[INV]   {D}-[Value Range Calculator]
{C}-[IXC]   {D}-[Value of Goods Range Calculator]
{C}-[MPU]   {D}-[Minimum Or Per Unit Calculator]
{C}-[NTE]   {D}-[Note Calculator]
{C}-[PER]   {D}-[Percentage Calculator]
{C}-[UNT]   {D}-[Unit Calculator]
{C}-[WTR]   {D}-[Weight Range Calculator]
{C}-[WRU]   {D}-[Weight Range then Per Unit Calculator]
{C}-[TME]   {D}-[Time Calculator]

{A}-[Orig. Ctry/Region]   {B}-[Type]   {C}-[country Lookup Code]
{B}-[Field]   {C}-[OriginCountry]

{A}-[Origin]   {B}-[Type]   {C}-[unloco Lookup]
{B}-[Field]   {C}-[OriginLRCPK]

{A}-[Dest. Ctry/Region]   {B}-[Type]   {C}-[country Lookup Code]
{B}-[Field]   {C}-[DestinationCountry]

{A}-[Destination]   {B}-[Type]   {C}-[unloco Lookup]
{B}-[Field]   {C}-[DestinationLRCPK]

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
				((DateField)Report.FilterCollection["Rates At"]).Value = ZDateTime.Today;
				AssertWorkSheet0
				(
					expectedOutput: @"{B}-[Eagle Datamation International]
{B}-[ Client Rates by Client Summary Report]
{B}-[Client : Test Client #1 (TESTORG1), ]
{B}-[Rate Category: All, Rates As Of: 01-Jan-20]
{B}-[Printed by: CargoWise Support  01-Jan-20 12:00 AM]
{C}-[Mode]   {D}-[Transport]   {F}-[Orig]   {G}-[Dest.]   {H}-[Start Date]   {I}-[Expiry Date]   {K}-[Srv. Lvl.]   {L}-[Comm]   {M}-[Cont]   {O}-[Carrier]   {P}-[Supplier]   {Q}-[Published]
{D}-[Chg Code]   {E}-[Desc.]   {F}-[W/V]   {G}-[Calc.]   {H}-[Type]   {I}-[Break]   {K}-[Value]   {L}-[Flat]

{B}-[Client : Test Client #1 (TESTORG1)]   {J}-[CFX (Air) Import:]   {K}-[0]   {M}-[CFX (Sea) Import:]   {N}-[0]   {O}-[Company Tariff:]
{J}-[CFX (Air) Export:]   {K}-[0]   {M}-[CFX (Sea) Export:]   {N}-[0]
{C}-[AIR]   {D}-[LSE]   {F}-[AUSYD]   {G}-[USLAX]   {H}-[43831]   {I}-[44013]   {L}-[GEN]
{D}-[BAF]   {E}-[Bunker Adjustment Factor]   {F}-[KG]   {G}-[PEB]
{H}-[Use Accumulated]   {J}-[LYD]   {K}-[0.000]   {M}-[N]   {Q}-[Local]
{H}-[Percentage Applies To]   {J}-[LYD]   {K}-[0.000]   {M}-[ALL]   {Q}-[Local]
{H}-[Basic Charge]   {J}-[LYD]   {K}-[1,200.123]   {Q}-[Local]
{H}-[BBV]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[BRP]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[Higher Chargeable, Lower Rate]   {J}-[LYD]   {K}-[0.000]   {M}-[N]   {Q}-[Local]
{H}-[IGS]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[Inclusive Breaks]   {J}-[LYD]   {K}-[0.000]   {M}-[N]   {Q}-[Local]
{H}-[Maximum]   {J}-[LYD]   {K}-[1,100.123]   {Q}-[Local]
{H}-[Minimum]   {J}-[LYD]   {K}-[1,000.123]   {Q}-[Local]
{H}-[+  ]   {I}-[1.2]   {J}-[LYD]   {K}-[20.1]   {L}-[200.123]   {Q}-[Local]
{H}-[-  ]   {I}-[1.2]   {J}-[LYD]   {K}-[10.1]   {L}-[100.123]   {Q}-[Local]",
					message: "PEB should show correct decimals for -/+ break, percentage and flat amount"
				);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestPercentageCalculatorPercentageFormatting()
		{
			var helper = new TestHelper(Factory);
			var client = helper.NewOrgHeader();

			var clientRate = helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", "FRT", 11.12345m, currency: CurrencyCodes.LibyanArabJamahiriya);
			var rateLine = rateEntry.AddRateLine("BAF", PercentageCalculator.Code, currencyCode: "LYD");
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
				((IValueAsStringProviderForUnitTests)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = client.OH_Code;
				((DateField)Report.FilterCollection["Rates At"]).Value = ZDateTime.Today;
				AssertWorkSheet0
				(
					expectedOutput: @"{B}-[Eagle Datamation International]
{B}-[ Client Rates by Client Summary Report]

{B}-[Rate Category: All, Rates As Of: 01-Jan-20]
{B}-[Printed by: CargoWise Support  01-Jan-20 12:00 AM]
{C}-[Mode]   {D}-[Transport]   {F}-[Orig]   {G}-[Dest.]   {H}-[Start Date]   {I}-[Expiry Date]   {K}-[Srv. Lvl.]   {L}-[Comm]   {M}-[Cont]   {O}-[Carrier]   {P}-[Supplier]   {Q}-[Published]
{D}-[Chg Code]   {E}-[Desc.]   {F}-[W/V]   {G}-[Calc.]   {H}-[Type]   {I}-[Break]   {K}-[Value]   {L}-[Flat]

{B}-[Client : Test Client #1 (TESTORG1)]   {J}-[CFX (Air) Import:]   {K}-[0]   {M}-[CFX (Sea) Import:]   {N}-[0]   {O}-[Company Tariff:]
{J}-[CFX (Air) Export:]   {K}-[0]   {M}-[CFX (Sea) Export:]   {N}-[0]
{C}-[AIR]   {D}-[LSE]   {F}-[AUSYD]   {G}-[USLAX]   {H}-[43831]   {I}-[44013]   {L}-[GEN]
{D}-[BAF]   {E}-[Bunker Adjustment Factor]   {G}-[Percentage Calculator]
{H}-[Percentage Applies To]   {J}-[LYD]   {K}-[0.000]   {M}-[ALL]   {Q}-[Local]
{H}-[Basic Charge]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[The Greater Charge]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[IGS]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[Maximum]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[Minimum]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[Percent First]   {J}-[LYD]   {K}-[12.1]   {Q}-[Local]
{H}-[Part There Of]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[Agency Rate]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{H}-[Value or Part Thereof]   {J}-[LYD]   {K}-[0.000]   {Q}-[Local]
{D}-[FRT]   {E}-[International Freight]   {G}-[Flat Calculator]
{H}-[Basic Charge]   {J}-[LYD]   {K}-[11.124]   {Q}-[Local]",
					message: "Percentage should be formatted from RateLine currency"
				);
			}
		}
	}

	public class TestForwardingClientRatesbyClientReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return @"Client Rates by Client Summary"; }
		}

		public override string Hint
		{
			get
			{
				return @"Rates by Client Summary Report shows a summary listing of rates entered against your client base. 
This report can be filtered by any mix of clients, selected suppliers, origin, destination, freight mode, transport type, rate calculator type, charge codes and rate dates.

The Freight Mode filter restricts the report to rates entered on the selected tab, - AIR, FCL, LCL, Origin, Destination.
The Transport Mode filter selects all rates for the chosen Transport Mode - Air, Rail, Road, Sea, Mail. Use this filter to select all rates with transport pack modes associated with the chosen Transport journey Mode. E.g. SEA will select FCL-SEA, LCL-SEA and Original / Destination rates using sea freight. 
The Transport Pack mode filter restricts the report to rates with the specified Transport Pack mode - use this filter to select a specific transport pack mode, E.g. FCL, LCL, ULD etc.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestForwardingClientRatesbyClientTemplate();
		}
	}
}
