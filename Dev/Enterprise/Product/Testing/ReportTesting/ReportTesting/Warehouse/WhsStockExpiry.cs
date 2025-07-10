namespace Enterprise.ReportTesting.Warehouse
{
	using CargoWise.Types;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using NUnit.Framework;

	[TemplateName("Whs Stock Expiry")]
	public class TestWhsStockExpiryTemplate : WhsTemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestZeroPalletSize()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			OrgPartUnit partUnit = data.Part1.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 0;

			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			orgPartRelation.OU_UseExpiryDate = true;
			orgPartRelation.OU_UsePartAttrib3 = true;
			orgPartRelation.OU_LocalPartNumber = "DONTUSETHIS";
			orgPartRelation.OU_LocalPartDescription = "DONTUSETHIS";

			var productParam = WarehouseHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 10;

			var receive = WarehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			var inventory = receive.Inventory[0];

			inventory.WI_ExpiryDate = new ZDate(1900, 1, 1);

			Factory.Save();

			PrepareReportForRender();
			SelectAllOptionalTemplates();

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = data.Org1.PK.ToString();

			RunReport();
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

	public class TestWhsStockExpiryReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Warehouse.Transactions.Module.ReportModule(); }
		}

		public override string MenuName
		{
			get { return @"Stock Expiry Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report is used to identify Stock coming up to its expiry date. The report shows stock-lines where the current stock has expired, or is about to expire.
Only stock flagged to track expiry dates are included in this report. Run this report regularly to monitor the expiry dates on client stock.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsStockExpiryTemplate();
		}
	}
}
