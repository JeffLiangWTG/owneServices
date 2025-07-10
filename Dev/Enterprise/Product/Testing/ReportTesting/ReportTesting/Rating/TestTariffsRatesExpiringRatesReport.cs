namespace Enterprise.ReportTesting.Rating
{
	[TemplateName("Tariffs & Rates Expiring Rates Report")]
	public class TestTariffsRatesExpiringRatesReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;

		public void TestTemplate()
		{
			AssertEquals("Report should contain 2 worksheets", 2, Report.XlInterface.WorkSheets.Count);

			AssertMultilineASCIIEquals
			(
				"Worksheet 1",
				@"{A}-[#config]
{A}-[PageStyle=Continuous]
{A}-[DataContext=None]
{A}-[Name=Tariffs & Rates Expiring Rates Report]
{A}-[Data:ReportData=exec Report_TariffsRatesExpiringRates <CompanyPK>, <Expiring Days>, <Rate>, <Type>, <Mode>, <Show Global>]
{A}-[#DocumentHeader]
{C}-[<Image(CompanyLogo,8,10)>]








{B}-[<CompanyName>]
{B}-[Unhandled CellContent of Type: FlexCel.Core.TFlxFormulaErrorValue]

{B}-[Printed by <Login Full Name> <Now>]

{A}-[#PageHeader]
{D}-[Type]   {E}-[Mode]   {F}-[Orig]   {G}-[Dest.]   {H}-[Cont.]   {I}-[Srv.Lvl.]   {J}-[Comm.]   {K}-[Srv.Prov.]   {L}-[Carrier]   {M}-[Start]   {N}-[End]   {O}-[Published]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Name>]   {C}-[<ReportData.OrgOrLevel>]   {D}-[<ReportData.Type>]   {E}-[<ReportData.Mode>]   {F}-[<ReportData.Origin>]   {G}-[<ReportData.Destination>]   {H}-[<ReportData.Container>]   {I}-[<ReportData.ServiceLevel>]   {J}-[<ReportData.CommodityCode>]   {K}-[<ReportData.Supplier>]   {L}-[<ReportData.Carrier>]   {M}-[<ReportData.StartDate>]   {N}-[<ReportData.EndDate>]   {O}-[<ReportData.Published>]
{A}-[#SectionFooter]
{A}-[#EndOfReport]",
				Report.XlInterface.WorkSheets[0].ToString()
			);

			AssertMultilineASCIIEquals
			(
				"Worksheet 2",
				@"{A}-[Expiring Days]   {B}-[Type]   {C}-[Number]
{B}-[DefaultValue]   {C}-[30]
{B}-[Decimal Places]   {C}-[0]

{A}-[Rate]   {B}-[Type]   {C}-[Checkbox]
{B}-[Options]   {C}-[Client Rates]   {D}-[Client Rates]
{C}-[Company Tariff]   {D}-[Company Tariff]
{C}-[Costings]   {D}-[Costings]
{B}-[DefaultValue]   {C}-[Client Rates, Company Tariff, Costings]

{A}-[Type]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Required]
{B}-[Option]   {C}-[ALL]   {D}-[All Types]
{C}-[FOR]   {D}-[Forwarding]
{C}-[ORG]   {D}-[Forwarding Origin]
{C}-[DST]   {D}-[Forwarding Destination]
{C}-[LRA]   {D}-[Liner and Agency]
{C}-[SOR]   {D}-[Liner and Agency Origin]
{C}-[SDE]   {D}-[Liner and Agency Destination]
{C}-[CFS]   {D}-[CFS]
{C}-[WHS]   {D}-[Warehouse]
{C}-[TRN]   {D}-[Transport]
{B}-[Default]   {C}-[ALL]

{A}-[Mode]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Required]
{B}-[Option]   {C}-[ALL]   {D}-[All Freight Modes]
{C}-[AIR]   {D}-[Air Freight]
{C}-[LSE]   {D}-[Air Freight (Loose)]
{C}-[ULD]   {D}-[Air Freight (ULD)]
{C}-[SEA]   {D}-[Sea Freight (LCL and FCL)]
{C}-[LCL]   {D}-[Sea Freight (LCL)]
{C}-[FCL]   {D}-[Sea Freight (FCL)]
{C}-[ROA]   {D}-[Road Freight (LCL, FCL and FTL)]
{C}-[LRO]   {D}-[Road Freight (LCL)]
{C}-[FRO]   {D}-[Road Freight (FCL)]
{C}-[FTL]   {D}-[Road Freight (Full Truck Load)]
{C}-[RAI]   {D}-[Rail Freight (LCL, FCL and FWL)]
{C}-[LRA]   {D}-[Rail Freight (LCL)]
{C}-[FRA]   {D}-[Rail Freight (FCL)]
{C}-[FWL]   {D}-[Rail Freight (Full Wagon Load)]
{B}-[Default]   {C}-[ALL]

{A}-[Show Global]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Global Rates]

{A}-[#End]",
				Report.XlInterface.WorkSheets[1].ToString()
			);
		}
	}
}
