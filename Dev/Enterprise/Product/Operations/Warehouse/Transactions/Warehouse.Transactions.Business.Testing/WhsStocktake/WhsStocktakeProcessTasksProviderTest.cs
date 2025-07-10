using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktake))]
	public class WhsStocktakeProcessTasksProviderTest : WorkflowProviderTest<WhsStocktake, ProcessTaskCollection>
	{
		#region TestGetTemplateFilterCriteria

		public void TestGetTemplateFilterCriteria()
		{
			var stocktake = BusinessObject;
			var whs1 = Helper.CreateWarehouse("Wh1");
			var whs2 = Helper.CreateWarehouse("Wh2");
			var client1 = Helper.CreateClient("Cl1");
			var client2 = Helper.CreateClient("Cl2");

			AssertGetTemplateFilterCriteria(stocktake.WS_OH_ClientInfo, ProcessTaskTemplate.P0_OH_ClientInfo,
				client1.PK, client2.PK, ZGuid.Empty);
			AssertGetTemplateFilterCriteria(stocktake.WS_WW_WhsInfo, ProcessTaskTemplate.P0_WWInfo, whs1.PK,
				whs2.PK, ZGuid.Empty);
		}

		#endregion

		#region Overrides

		protected override ZString ExpectedWorkflowType
		{
			get { return "WSC"; }
		}

		protected override WhsStocktake GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return Factory.New<WhsStocktake>();
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
