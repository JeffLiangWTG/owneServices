namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using Enterprise.Warehouse.Transactions.Module;
	using NUnit.Framework;

	[TemplateName("Whs Stock On Hand")]
	public class TestWhsStockOnHandReport : WhsTemplateTestCase
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

		public void TestTemplate_StockOnHandByLocation()
		{
			AssertMultilineASCIIEquals("Worksheet - Stock On Hand by Location",
				@"{A}-[#config]
{A}-[PageStyle=Continuous]
{A}-[DataContext=None]
{A}-[Name=Stock On Hand Report]
{A}-[EmailSubject=<CompanyCode> Stock On Hand <Now>]
{A}-[Data:ReportData=WhsStockOnHandReport(<Client>, <Warehouse>, <Product>, <Product Category>)]
{A}-[ColumnHeadings:SavesTo=Client]   {D}-[DisplayLabel=""Client"", HeadingText=""Client""]   {E}-[DisplayLabel=""Product Code"", HeadingText=""Product""]   {F}-[DisplayLabel=""Product Description"", HeadingText=""Description""]   {G}-[DisplayLabel=""Category"", HeadingText=""Category""]   {H}-[DisplayLabel=""Arrival Date"", HeadingText=""Arrival Date""]   {I}-[DisplayLabel=""Part Attribute 1"", HeadingText=""Part Attribute 1""]   {J}-[DisplayLabel=""Part Attribute 2"", hidden, HeadingText=""Part Attribute 2""]   {K}-[DisplayLabel=""Part Attribute 3"", hidden, HeadingText=""Part Attribute 3""]   {L}-[DisplayLabel=""Serial Number"", hidden, HeadingText=""Serial Number""]   {M}-[DisplayLabel=""Packing Date"", HeadingText=""Packing Date""]   {N}-[DisplayLabel=""Expiry Date"", HeadingText=""Expiry Date""]   {O}-[DisplayLabel=""Pallet ID"", HeadingText=""Pallet ID""]   {P}-[DisplayLabel=""Status"", HeadingText=""Status""]   {Q}-[DisplayLabel=""Hold Code"", HeadingText=""Hold Code""]   {R}-[DisplayLabel=""Available Units"", HeadingText=""Available""]   {S}-[DisplayLabel=""Committed Units"", HeadingText=""Committed""]   {T}-[DisplayLabel=""Reserved Units"", HeadingText=""Reserved""]   {U}-[DisplayLabel=""Total Units"", HeadingText=""Total Qty""]   {V}-[DisplayLabel=""Total Units UQ"", HeadingText=""UQ""]   {W}-[DisplayLabel=""Client Total Units"", HeadingText=""Client Total""]   {X}-[DisplayLabel=""Client Total Units UQ"", HeadingText=""UQ""]   {Y}-[DisplayLabel=""Total Weight"", hidden, HeadingText=""Total Weight""]   {Z}-[DisplayLabel=""Total Weight UQ"", hidden, HeadingText=""UQ""]   {AA}-[DisplayLabel=""Total Volume"", hidden, HeadingText=""Total Volume""]   {AB}-[DisplayLabel=""Total Volume UQ"", hidden, HeadingText=""UQ""]   {AC}-[DisplayLabel=""Last Movement Date"", hidden, HeadingText=""Last Move Date""]   {AD}-[DisplayLabel=""Total Pallets"", HeadingText=""Total Pallets""]   {AE}-[DisplayLabel=""Pallet Spaces (by Pallet Size)"", hidden, HeadingText=""PLT Spaces (by Pallet Size)""]   {AF}-[DisplayLabel=""Total Value"", hidden, HeadingText=""Total Value""]   {AG}-[DisplayLabel=""Currency"", hidden, HeadingText=""Currency""]   {AH}-[DisplayLabel=""Last Cost"", hidden, HeadingText=""Last Cost""]   {AI}-[DisplayLabel=""UNDG Number"", hidden, HeadingText=""UNDG Number""]   {AJ}-[DisplayLabel=""UNDG Proper Shipping Name"", hidden, HeadingText=""UNDG Proper Shipping Name""]   {AK}-[DisplayLabel=""UNDG IMO Class"", hidden, HeadingText=""UNDG IMO Class""]   {AL}-[DisplayLabel=""UNDG Sub Risk 1"", hidden, HeadingText=""UNDG Sub Risk 1""]   {AM}-[DisplayLabel=""UNDG Sub Risk 2"", hidden, HeadingText=""UNDG Sub Risk 2""]   {AN}-[DisplayLabel=""UNDG Packing Group"", hidden, HeadingText=""UNDG Packing Group""]
{A}-[#SectionPageHeader]
{B}-[<CompanyName>]
{B}-[Stock On Hand (By Location) Report]
{A}-[<HideRowIf(""<Pick Area>""=="""")>]   {C}-[Pick Area:]   {E}-[<If(""<Pick Area>""=="""", """", ""<ReportData.AreaName>"")>]
{B}-[Printed by <Login Full Name> <Now>]
{C}-[Location]
{D}-[<AutoHeight><CustomisedColumn(Client)>]   {E}-[<AutoHeight><CustomisedColumn(Product Code)>]   {F}-[<AutoHeight><CustomisedColumn(Product Description)>]   {G}-[<AutoHeight><CustomisedColumn(Category)>]   {H}-[<AutoHeight><CustomisedColumn(Arrival Date)>]   {I}-[<AutoHeight><CustomisedColumn(Part Attribute 1)>]   {J}-[<AutoHeight><CustomisedColumn(Part Attribute 2)>]   {K}-[<AutoHeight><CustomisedColumn(Part Attribute 3)>]   {L}-[<AutoHeight><CustomisedColumn(Serial Number)>]   {M}-[<AutoHeight><CustomisedColumn(Packing Date)>]   {N}-[<AutoHeight><CustomisedColumn(Expiry Date)>]   {O}-[<AutoHeight><CustomisedColumn(Pallet ID)>]   {P}-[<AutoHeight><CustomisedColumn(Status)>]   {Q}-[<AutoHeight><CustomisedColumn(Hold Code)>]   {R}-[<AutoHeight><CustomisedColumn(Available Units)>]   {S}-[<AutoHeight><CustomisedColumn(Committed Units)>]   {T}-[<AutoHeight><CustomisedColumn(Reserved Units)>]   {U}-[<AutoHeight><CustomisedColumn(Total Units)>]   {V}-[<AutoHeight><CustomisedColumn(Total Units UQ)>]   {W}-[<AutoHeight><CustomisedColumn(Client Total Units)>]   {X}-[<AutoHeight><CustomisedColumn(Client Total Units UQ)>]   {Y}-[<AutoHeight><CustomisedColumn(Total Weight)>]   {Z}-[<AutoHeight><CustomisedColumn(Total Weight UQ)>]   {AA}-[<AutoHeight><CustomisedColumn(Total Volume)>]   {AB}-[<AutoHeight><CustomisedColumn(Total Volume UQ)>]   {AC}-[<AutoHeight><CustomisedColumn(Last Movement Date)>]   {AD}-[<AutoHeight><CustomisedColumn(Total Pallets)>]   {AE}-[<AutoHeight><CustomisedColumn(Pallet Spaces (by Pallet Size))>]   {AF}-[<AutoHeight><CustomisedColumn(Total Value)>]   {AG}-[<AutoHeight><CustomisedColumn(Currency)>]   {AH}-[<AutoHeight><CustomisedColumn(Last Cost)>]   {AI}-[<AutoHeight><CustomisedColumn(UNDG Number)>]   {AJ}-[<AutoHeight><CustomisedColumn(UNDG Proper Shipping Name)>]   {AK}-[<AutoHeight><CustomisedColumn(UNDG IMO Class)>]   {AL}-[<AutoHeight><CustomisedColumn(UNDG Sub Risk 1)>]   {AM}-[<AutoHeight><CustomisedColumn(UNDG Sub Risk 2)>]   {AN}-[<AutoHeight><CustomisedColumn(UNDG Packing Group)>]
{A}-[#SectionBody:Data=ReportData]
{D}-[<ReportData.ClientCode>]   {E}-[<ReportData.Product>]   {F}-[<AutoHeight><ReportData.ProductDesc>]   {G}-[<ReportData.ProductCategoryCode>]   {H}-[<ReportData.ArrivalDate>]   {I}-[<ReportData.PartAttrib1>]   {J}-[<ReportData.PartAttrib2>]   {K}-[<ReportData.PartAttrib3>]   {L}-[<ReportData.SerialNumber>]   {M}-[<DateTimeAsString('<ReportData.PackingDate>', 'dd-MMM-yy')>]   {N}-[<DateTimeAsString('<ReportData.ExpiryDate>', 'dd-MMM-yy')>]   {O}-[<ReportData.PalletID>]   {P}-[<ReportData.Status>]   {Q}-[<ReportData.WhsDocketLine_HeldCode>]   {R}-[<ReportData.AvailableUnits>]   {S}-[<ReportData.CommittedUnits>]   {T}-[<ReportData.AllocatedUnits>]   {U}-[<ReportData.TotalUnits>]   {V}-[<ReportData.StockKeepingUnit>]   {W}-[<ReportData.TotalClientUnits>]   {X}-[<ReportData.ClientUnit>]   {Y}-[<ReportData.Weight>]   {Z}-[<ReportData.WeightUQ>]   {AA}-[<ReportData.Volume>]   {AB}-[<ReportData.VolumeUQ>]   {AC}-[<ReportData.LastMovementDate>]   {AE}-[<ReportData.TotalPalletSpaces>]   {AF}-[<ReportData.TotalValue>]   {AG}-[<ReportData.Currency>]   {AH}-[<ReportData.LastCost>]   {AI}-[<ReportData.UNDGNumber>]   {AJ}-[<ReportData.UNDGProperShippingName>]   {AK}-[<ReportData.UNDGIMOClass>]   {AL}-[<ReportData.UNDGSubRisk1>]   {AM}-[<ReportData.UNDGSubRisk2>]   {AN}-[<ReportData.UNDGPackingGroup>]
{A}-[#GroupBy:ReportData.LocnRow+ReportData.LocationIndexForSort: GroupTitle]
{C}-[<ReportData.Location>]
{A}-[#GroupBy:ReportData.LocnRow+ReportData.LocationIndexForSort]
{B}-[Location Totals:]   {R}-[<Total ReportData.AvailableUnits>]   {S}-[<Total ReportData.CommittedUnits>]   {T}-[<Total ReportData.AllocatedUnits>]   {U}-[<Total ReportData.TotalUnits>]   {W}-[<Total ReportData.TotalClientUnits>]   {Y}-[<Total ReportData.Weight>]   {AA}-[<Total ReportData.Volume>]   {AD}-[<ReportData.TotalPalletsPerLocation>]   {AE}-[<Total ReportData.TotalPalletSpaces>]   {AF}-[<Total ReportData.TotalValue>]

{A}-[#GroupBy:ReportData.WarehousePK: GroupTitle]
{B}-[Warehouse: <ReportData.WarehouseName>]
{A}-[#GroupBy:ReportData.WarehousePK: PageBreak]

{B}-[Warehouse Totals:]   {R}-[<Total ReportData.AvailableUnits>]   {S}-[<Total ReportData.CommittedUnits>]   {T}-[<Total ReportData.AllocatedUnits>]   {U}-[<Total ReportData.TotalUnits>]   {W}-[<Total ReportData.TotalClientUnits>]   {Y}-[<Total ReportData.Weight>]   {AA}-[<Total ReportData.Volume>]   {AD}-[<Total ReportData.TotalPalletsPerLocation>]   {AE}-[<Total ReportData.TotalPalletSpaces>]   {AF}-[<Total ReportData.TotalValue>]
{A}-[#endofreport]",
				Report.XlInterface.WorkSheets[1].ToString());
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

	public class TestWhsStockOnHandByProductReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Stock On Hand Report"; }
		}

		public override string Hint
		{
			get
			{
				return
					@"This report shows a snapshot of inventory at the time of printing the report. The data is sorted by Warehouse -> Client -> Product -> Location -> Arrival Date. When you need to know how much stock you currently have in your warehouses and where it is located, this is the report to use.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsStockOnHandReport();
		}
	}
}
