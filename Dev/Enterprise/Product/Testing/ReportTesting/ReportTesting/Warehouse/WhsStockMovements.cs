namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using NUnit.Framework;

	[TemplateName("Whs Stock Movements")]
	public class TestWhsStockMovementsReport : WhsTemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestZeroPalletSize()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			OrgPartUnit partUnit = data.Part1.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 0;

			var whsReceipt = WarehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "", allocateLocations: false, finalise: true);

			Factory.Save();

			PrepareReportForRender();
			SelectAllOptionalTemplates();

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = data.Org1.PK.ToString();

			RunReport();
		}

		public void TestTemplate()
		{
			AssertMultilineASCIIEquals("Worksheet 1",
				@"{A}-[#config]
{A}-[DataContext=None]
{A}-[PageStyle=Continuous]
{A}-[AutoHeightMode=ExpandRow]
{A}-[Name=Stock Movements Report]
{A}-[EmailSubject=<CompanyCode> Stock Movements <Now>]
{A}-[DisableCSVExport]
{A}-[Data:ReportData=WhsStockMovementReport(<Product Category>,<Client>,<Warehouse>,<Product>,<Date Range.FromDateForSQLParameter>,<Date Range.ToDateForSQLParameter>,<Display>) Where ((<Adjustments>='Y') OR (DocketType != 'ADJ' OR DocketSubType != 'IWA'))]
{A}-[ColumnHeadings:SavesTo=Client]   {C}-[DisplayLabel=""Product Code"", HeadingText=""Product""]   {D}-[DisplayLabel=""Product Description"", HeadingText=""Description""]   {E}-[DisplayLabel=""Product Category"", HeadingText=""Product Category""]   {F}-[DisplayLabel=""Reference"", HeadingText=""Reference""]   {G}-[DisplayLabel=""Type"", HeadingText=""Type""]   {H}-[DisplayLabel=""Service Level"", HeadingText=""Service Level""]   {I}-[DisplayLabel=""Consignee"", HeadingText=""Consignee""]   {J}-[DisplayLabel=""Distribution Center (DC)"", HeadingText=""Distribution Center (DC)""]   {K}-[DisplayLabel=""Attribute 1"", HeadingText=""Attribute 1""]   {L}-[DisplayLabel=""Attribute 2"", hidden,  HeadingText=""Attribute 2""]   {M}-[DisplayLabel=""Attribute 3"", hidden, HeadingText=""Attribute 3""]   {N}-[DisplayLabel=""Serial Number"", hidden, HeadingText=""Serial Number""]   {O}-[DisplayLabel=""Packing Date"",  HeadingText=""Packing Date""]   {P}-[DisplayLabel=""Expiry Date"", HeadingText=""Expiry Date""]   {Q}-[DisplayLabel=""Finalised Date"", HeadingText=""Date"", Description=""Finalized Date""]   {R}-[DisplayLabel=""Quantity"", HeadingText=""Quantity""]   {S}-[DisplayLabel=""UQ"", HeadingText=""UQ""]   {T}-[DisplayLabel=""PLT Spaces (by Pallet Size)"", hidden, HeadingText=""PLT Spaces (by Pallet Size)""]   {U}-[DisplayLabel=""Location"", HeadingText=""Location""]   {V}-[DisplayLabel=""PLT ID"", HeadingText=""PLT ID""]
{A}-[#DocumentHeader]
{A}-[#SectionPageHeader]
{B}-[<CompanyName>]
{B}-[Stock Movements Report]
{B}-[Client: <ReportData.ClientFullName> (<ReportData.ClientCode>)]
{B}-[Warehouse: <ReportData.WarehouseName>]
{B}-[Filters: <Date Range>]
{A}-[<HideRowIf(""<Product.Code>""=="""")>]   {B}-[Product: <Product.Code>]
{A}-[<HideRowIf(""<Commodity.Code>""=="""")>]   {B}-[Commodity: <Commodity.Code>]
{A}-[<HideRowIf(""<Product Category.Code>""=="""")>]   {B}-[Product Category: <Product Category.Code>]
{A}-[<HideRowIf(""<Consignee.Code>""=="""")>]   {B}-[Consignee: <Consignee.Code>]
{A}-[<HideRowIf(""<Distribution Center (DC).Code>""=="""")>]   {B}-[Distribution Center (DC): <Distribution Center (DC).Code>]
{A}-[<HideRowIf(""<Service Level>""=="""")>]   {B}-[Service Level: <Service Level>]
{A}-[<HideRowIf(""<Attribute 1>""=="""")>]   {B}-[Attribute 1: <Attribute 1>]
{A}-[<HideRowIf(""<Attribute 2>""=="""")>]   {B}-[Attribute 2: <Attribute 2>]
{A}-[<HideRowIf(""<Attribute 3>""=="""")>]   {B}-[Attribute 3: <Attribute 3>]
{A}-[<HideRowIf(""<Serial Number>""=="""")>]   {B}-[Serial Number: <Serial Number>]
{A}-[<HideRowIf(""<Expiry Date>""=="""")>]   {B}-[Expiry Date: <DateTimeAsString('<Expiry Date>', 'dd-MMM-yyyy')>]
{A}-[<HideRowIf(""<Packing Date>""=="""")>]   {B}-[Packing Date: <DateTimeAsString('<Packing Date>', 'dd-MMM-yyyy')>]
{B}-[Sort: <Selected Sort Order>]
{A}-[<HideRowIf(""<Display Totals>""=="""")>]   {B}-[Display Totals]
{A}-[<HideRowIf(""<Display Totals>""==""Y"")>]   {B}-[Hide Totals]
{A}-[<HideRowIf(""<Adjustments>""=="""")>]   {B}-[Include internal warehouse adjustments]
{A}-[<HideRowIf(""<Adjustments>""==""Y"")>]   {B}-[Exclude internal warehouse adjustments]
{B}-[Printed by <Login Full Name>  <DateTimeAsString('<Now>','dd-MMM-yy HH:mm')>]
{C}-[<AutoHeight><CustomisedColumn(Product Code)>]   {D}-[<AutoHeight><CustomisedColumn(Product Description)>]   {E}-[<AutoHeight><CustomisedColumn(Product Category)>]   {F}-[<AutoHeight><CustomisedColumn(Reference)>]   {G}-[<AutoHeight><CustomisedColumn(Type)>]   {H}-[<AutoHeight><CustomisedColumn(Service Level)>]   {I}-[<AutoHeight><CustomisedColumn(Consignee)>]   {J}-[<AutoHeight><CustomisedColumn(Distribution Center (DC))>]   {K}-[<AutoHeight><CustomisedColumn(Attribute 1)>]   {L}-[<AutoHeight><CustomisedColumn(Attribute 2)>]   {M}-[<AutoHeight><CustomisedColumn(Attribute 3)>]   {N}-[<AutoHeight><CustomisedColumn(Serial Number)>]   {O}-[<AutoHeight><CustomisedColumn(Packing Date)>]   {P}-[<AutoHeight><CustomisedColumn(Expiry Date)>]   {Q}-[<AutoHeight><CustomisedColumn(Finalised Date)>]   {R}-[<AutoHeight><CustomisedColumn(Quantity)>]   {S}-[<AutoHeight><CustomisedColumn(UQ)>]   {T}-[<AutoHeight><CustomisedColumn(PLT Spaces (by Pallet Size))>]   {U}-[<AutoHeight><CustomisedColumn(Location)>]   {V}-[<AutoHeight><CustomisedColumn(PLT ID)>]
{A}-[#SectionBody:Data=ReportData]
{C}-[<AutoHeight><ReportData.Product>]   {D}-[<AutoHeight><ReportData.ProductDesc>]   {E}-[<AutoHeight><ReportData.ProductCategoryCode>]   {F}-[<AutoHeight><If(""<ReportData.DocketType>"" == ""HCC"" && ""<ReportData.Reference>"" == """", ""Available"", ""<ReportData.Reference>"")>]   {G}-[<ReportData.DocketType>]   {H}-[<AutoHeight><ReportData.ServiceLevel>]   {I}-[<AutoHeight><ReportData.ConsigneeName>]   {J}-[<AutoHeight><ReportData.DistributionCentreCoName>]   {K}-[<AutoHeight><ReportData.PartAttrib1>]   {L}-[<AutoHeight><ReportData.PartAttrib2>]   {M}-[<AutoHeight><ReportData.PartAttrib3>]   {N}-[<AutoHeight><ReportData.SerialNumber>]   {O}-[<AutoHeight><ReportData.PackingDate>]   {P}-[<AutoHeight><ReportData.ExpiryDate>]   {Q}-[<AutoHeight><DateTimeAsString('<ReportData.FinalisedDateTime>','dd-MMM-yy HH:mm:ss')>]   {R}-[<AutoHeight><ReportData.QuantityActual>]   {S}-[<AutoHeight><ReportData.QuantityUQ>]   {T}-[<AutoHeight><ReportData.TotalPalletSpaces>]   {U}-[<AutoHeight><ReportData.Location>]   {V}-[Unhandled CellContent of Type: FlexCel.Core.TRichString]
{A}-[#if""<Selected Sort Order>""==""Warehouse - Client - Date - Product""]
{A}-[#GroupBy:ReportData.WarehouseName+ReportData.ClientCode+ReportData.FinalisedDateTime+ReportData.Product+ReportData.IsPositiveHCC]
{A}-[#endif]
{A}-[#if ""<Selected Sort Order>""==""Warehouse - Client - Product - Date""]
{A}-[#GroupBy:ReportData.Product+ReportData.FinalisedDateTime+ReportData.IsPositiveHCC]
{A}-[#endif]
{A}-[#if ""<Selected Sort Order>""==""Warehouse - Client - Tran Type - Product - Date""]
{A}-[#GroupBy:ReportData.DocketType+ReportData.Product+ReportData.FinalisedDateTime+ReportData.IsPositiveHCC]
{A}-[#endif]
{A}-[#if ""<Selected Sort Order>""==""Warehouse - Client - Consignee""]
{A}-[#GroupBy:ReportData.ConsigneeName]
{A}-[#endif]
{A}-[#if ""<Selected Sort Order>""==""Warehouse - Client - Product - Location - Date""]
{A}-[#GroupBy:ReportData.Product+ReportData.LocnRow+ReportData.LocationIndexForSort+ReportData.FinalisedDateTime+ReportData.IsPositiveHCC]
{A}-[#endif]
{A}-[<HideRowIf(""<Display Totals>""=="""")>]
{A}-[<HideRowIf(""<Display Totals>""=="""")>]   {R}-[<Total ReportData.QuantityActual>]
{A}-[<HideRowIf(""<Display Totals>""=="""")>]
{A}-[#GroupBy:ReportData.WarehouseName+ReportData.ClientCode:GroupTitle, PageBreak]
{A}-[#DocumentFooter]
{A}-[<HideRowIf(""<Display Totals>""=="""")>]   {R}-[<AccumulativeTotal ReportData.QuantityActual>]
{A}-[#endofreport]",
				Report.XlInterface.WorkSheets[0].ToString());
		}

		WhsTestHelperFunctions WarehouseHelper
		{
			get
			{
				if (fWarehouseHelper == null)
				{
					fWarehouseHelper = new WhsTestHelperFunctions(Factory);
				}
				return fWarehouseHelper;
			}
		}
		WhsTestHelperFunctions fWarehouseHelper;
	}
}
