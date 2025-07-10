using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module.Testing
{
	class ValueAnalysisFilterControlTest : TestCaseWithFactory
	{
		readonly string[] allColumns =
		{
			"VVA_Status",
			"VVA_IsTraded",
			"Origin+VLO_Code",
			"Destination+VLO_Code",
			"VVA_Warehouse",
			"VVA_LastActivity",
			"VVA_TradeMode",
			"VVA_Service",
			"VVA_TradeType",
			"Buyer+OH_Code",
			"Supplier+OH_Code",
			"Primary+OH_Code",
			"Location"
		};

		public void TestGridColumns()
		{
			var list = new[]
			{
				new { product = SystemDefinedSalesProductList.Codes.CustomsBrokerage, exclude = new[] { "VVA_Service", "VVA_Warehouse" } },
				new { product = SystemDefinedSalesProductList.Codes.ForwardingShipment, exclude = new[] { "VVA_Service", "VVA_Warehouse" , "Location" } },
				new { product = SystemDefinedSalesProductList.Codes.LinerAgency, exclude = new[] { "VVA_Service", "VVA_Warehouse" , "Location", "VVA_TradeMode" } },
				new { product = SystemDefinedSalesProductList.Codes.Transport, exclude = new[] { "VVA_Service", "VVA_Warehouse" , "Location", "VVA_TradeMode" } },
				new { product = SystemDefinedSalesProductList.Codes.Warehouse, exclude = new[] { "VVA_TradeMode", "VVA_TradeType" } }
			};

			foreach (var item in list)
			{
				using (var control = new ValueAnalysisFilterControl(new ViewValueAnalysisCollection(Factory), new ValueAnalysisFilterBusinessObject(), OrgHeaderSchema.PK.Name, item.product))
				{
					AssertEquals(allColumns.Length, control.Grid.ColumnStyles.Count);
					allColumns.All(t =>
					{
						var column = control.Grid.GetColumnStyle(t);
						AssertNotNull(t, column);
						AssertEquals(t, item.exclude.Contains(t), column.IsUnavailable);
						return true;
					});
				}
			}
		}

		public void TestLicenceCheckpoint()
		{
			Env.Licence.SalesValueAnalysis.SetLastConsumptionLogCreatedForTest(ZDateTime.Empty);

			var query = new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.SalesValueAnalysis.Name);
			var initialVALLicenceUsageCount = Factory.GetDatabaseCount(typeof(StmActivityLog), query);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();
			Factory.Save();

			var usageCount = Factory.GetDatabaseCount(typeof(StmActivityLog), query);
			AssertEquals("No new SalesValueAnalysis licence usage", initialVALLicenceUsageCount, usageCount);

			try
			{
				using (var form = new ZForm())
				using (var control = new ValueAnalysisFilterControl(new ViewValueAnalysisCollection(Factory), new ValueAnalysisFilterBusinessObject(), OrgHeaderSchema.PK.Name, SystemDefinedSalesProductList.Codes.ForwardingShipment))
				{
					form.Controls.Add(control);
					form.Show();
					usageCount = Factory.GetDatabaseCount(typeof(StmActivityLog), query);
					AssertEquals("One new SalesValueAnalysis licence usage", initialVALLicenceUsageCount + 1, usageCount);
				}
			}
			finally
			{
				Env.Licence.SalesValueAnalysis.SetLastConsumptionLogCreatedForTest(ZDateTime.Empty);
			}
		}
	}
}
