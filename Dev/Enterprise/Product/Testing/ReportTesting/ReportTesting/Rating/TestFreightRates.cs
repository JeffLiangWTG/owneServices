namespace Enterprise.ReportTesting.Rating
{
	[TemplateName("Freight Rates")]
	public class TestFreightRates : TemplateTestCase
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
{A}-[Name=Freight Rates Report]
{A}-[Data:ReportData=exec XT_Report_FreightRatesReport <CompanyPK>, <Type>, <Mode>, <Origin>, <Destination>, <Service Level>, <Commodity Code>, <Origin Country/Region>, <Destination Country/Region>, <Supplier>, <Consol Carrier>, <Sales Rep.Code>, <Show Global>]
{A}-[HIDECOLUMNIF]   {C}-[""<Show Origin>"" == """"]   {E}-[""<Show Destination>"" == """"]   {N}-[""<ReportData.Header_01>"" == """"]   {O}-[""<ReportData.Header_02>"" == """"]   {P}-[""<ReportData.Header_03>"" == """"]   {Q}-[""<ReportData.Header_04>"" == """"]   {R}-[""<ReportData.Header_05>"" == """"]   {S}-[""<ReportData.Header_06>"" == """"]   {T}-[""<ReportData.Header_07>"" == """"]   {U}-[""<ReportData.Header_08>"" == """"]   {V}-[""<ReportData.Header_09>"" == """"]   {W}-[""<ReportData.Header_10>"" == """"]   {X}-[""<ReportData.Header_11>"" == """"]   {Y}-[""<ReportData.Header_12>"" == """"]   {Z}-[""<ReportData.Header_13>"" == """"]   {AA}-[""<ReportData.Header_14>"" == """"]   {AB}-[""<ReportData.Header_15>"" == """"]   {AC}-[""<ReportData.Header_16>"" == """"]   {AD}-[""<ReportData.Header_17>"" == """"]   {AE}-[""<ReportData.Header_18>"" == """"]   {AF}-[""<ReportData.Header_19>"" == """"]   {AG}-[""<ReportData.Header_20>"" == """"]   {AH}-[""<ReportData.Header_21>"" == """"]   {AL}-[1==1]
{A}-[AutoheightMode=ExpandRow]
{A}-[#DocumentHeader]
{B}-[<Image(CompanyLogo,1,7)>]
{B}-[<Mode> Freight Client Rates]
{B}-[Printed by <Login Full Name> <Now>]

{A}-[#PageHeader]
{B}-[Origin]   {C}-[Origin Port]   {D}-[Dest.]   {E}-[Destination Port]   {F}-[Contract Number]   {G}-[Consignor]   {H}-[Consignee]   {I}-[Organization]   {J}-[Consol Carrier]   {K}-[Srv.Lvl.]   {L}-[Comm.]   {N}-[<ReportData.Header_01>]   {O}-[<ReportData.Header_02>]   {P}-[<ReportData.Header_03>]   {Q}-[<ReportData.Header_04>]   {R}-[<ReportData.Header_05>]   {S}-[<ReportData.Header_06>]   {T}-[<ReportData.Header_07>]   {U}-[<ReportData.Header_08>]   {V}-[<ReportData.Header_09>]   {W}-[<ReportData.Header_10>]   {X}-[<ReportData.Header_11>]   {Y}-[<ReportData.Header_12>]   {Z}-[<ReportData.Header_13>]   {AA}-[<ReportData.Header_14>]   {AB}-[<ReportData.Header_15>]   {AC}-[<ReportData.Header_16>]   {AD}-[<ReportData.Header_17>]   {AE}-[<ReportData.Header_18>]   {AF}-[<ReportData.Header_19>]   {AG}-[<ReportData.Header_20>]   {AH}-[<ReportData.Header_21>]   {AI}-[Start]   {AJ}-[End]   {AK}-[Published]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Origin>]   {C}-[<ReportData.OriginPortName>]   {D}-[<ReportData.Destination>]   {E}-[<ReportData.DestinationPortName>]   {F}-[<AutoHeight><ReportData.ContractNumber>]   {G}-[<AutoHeight><ReportData.Consignor>]   {H}-[<AutoHeight><ReportData.Consignee>]   {I}-[<AutoHeight><ReportData.OrgOrLevel>]   {J}-[<AutoHeight><ReportData.Carrier>]   {K}-[<ReportData.ServiceLevel>]   {L}-[<ReportData.CommodityCode>]   {M}-[<ReportData.Currency>]   {N}-[<Currency(<ReportData.Value_01>,<ReportData.Currency>)>]   {O}-[<Currency(<ReportData.Value_02>,<ReportData.Currency>)>]   {P}-[<Currency(<ReportData.Value_03>,<ReportData.Currency>)>]   {Q}-[<Currency(<ReportData.Value_04>,<ReportData.Currency>)>]   {R}-[<Currency(<ReportData.Value_05>,<ReportData.Currency>)>]   {S}-[<Currency(<ReportData.Value_06>,<ReportData.Currency>)>]   {T}-[<Currency(<ReportData.Value_07>,<ReportData.Currency>)>]   {U}-[<Currency(<ReportData.Value_08>,<ReportData.Currency>)>]   {V}-[<Currency(<ReportData.Value_09>,<ReportData.Currency>)>]   {W}-[<Currency(<ReportData.Value_10>,<ReportData.Currency>)>]   {X}-[<Currency(<ReportData.Value_11>,<ReportData.Currency>)>]   {Y}-[<Currency(<ReportData.Value_12>,<ReportData.Currency>)>]   {Z}-[<Currency(<ReportData.Value_13>,<ReportData.Currency>)>]   {AA}-[<Currency(<ReportData.Value_14>,<ReportData.Currency>)>]   {AB}-[<Currency(<ReportData.Value_15>,<ReportData.Currency>)>]   {AC}-[<Currency(<ReportData.Value_16>,<ReportData.Currency>)>]   {AD}-[<Currency(<ReportData.Value_17>,<ReportData.Currency>)>]   {AE}-[<Currency(<ReportData.Value_18>,<ReportData.Currency>)>]   {AF}-[<Currency(<ReportData.Value_19>,<ReportData.Currency>)>]   {AG}-[<Currency(<ReportData.Value_20>,<ReportData.Currency>)>]   {AH}-[<Currency(<ReportData.Value_21>,<ReportData.Currency>)>]   {AI}-[<ReportData.StartDate>]   {AJ}-[<ReportData.EndDate>]   {AK}-[<ReportData.Published>]
{A}-[#SectionFooter]

{A}-[#EndOfReport]",
				Report.XlInterface.WorkSheets[0].ToString()
			);

			AssertMultilineASCIIEquals
			(
				"Worksheet 2",
				@"{A}-[Type]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Required]
{B}-[Field]   {C}-[Type]
{B}-[Style]   {C}-[DropDown]
{B}-[Option]   {C}-[QTE]   {D}-[Active Quotations]
{C}-[SAL]   {D}-[Client Rates]
{C}-[COS]   {D}-[Costings]
{C}-[GLB]   {D}-[Company Tariffs]

{A}-[Mode]   {B}-[Type]   {C}-[MultipleChoice]
{B}-[Required]
{B}-[Field]   {C}-[Mode]
{B}-[Style]   {C}-[DropDown]
{B}-[Option]   {C}-[AIR]   {D}-[AIR Rates]
{C}-[LCL]   {D}-[LCL Rates]
{C}-[FCL]   {D}-[FCL Rates]

{A}-[Origin]   {B}-[Type]   {C}-[unloco Lookup]
{B}-[Field]   {C}-[OriginPK]

{A}-[Destination]   {B}-[Type]   {C}-[unloco Lookup]
{B}-[Field]   {C}-[DestinationPK]

{A}-[Origin Country/Region]   {B}-[Type]   {C}-[country Lookup]
{B}-[Field]   {C}-[OriginCountryPK]

{A}-[Destination Country/Region]   {B}-[Type]   {C}-[country Lookup]
{B}-[Field]   {C}-[DestinationCountryPK]

{A}-[Service Level]   {B}-[Type]   {C}-[service level Lookup]
{B}-[Field]   {C}-[ServiceLevel]

{A}-[Commodity Code]   {B}-[Type]   {C}-[commodity code Lookup]
{B}-[Field]   {C}-[CommodityCode]

{A}-[Supplier]   {B}-[Type]   {C}-[organisation lookup]
{B}-[Field]   {C}-[SupplierPK]

{A}-[Consol Carrier]   {B}-[Type]   {C}-[organisation lookup]
{B}-[Field]   {C}-[CarrierPK]

{A}-[Sales Rep]   {B}-[Type]   {C}-[staff lookup]
{B}-[Field]   {C}-[SalesRepCode]

{A}-[Show Origin]   {B}-[Type]   {C}-[CheckBox]
{B}-[Options]   {C}-[Y]   {D}-[Show Origin Port Name]

{A}-[Show Destination]   {B}-[Type]   {C}-[CheckBox]
{B}-[Options]   {C}-[Y]   {D}-[Show Destination Port Name]

{A}-[Show Global]   {B}-[Type]   {C}-[checkbox]
{B}-[Options]   {C}-[Y]   {D}-[Show Global Rates]

{A}-[#End]",
				Report.XlInterface.WorkSheets[1].ToString()
			);
		}
	}
}
