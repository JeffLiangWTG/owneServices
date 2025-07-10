using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrder))]
	public class WhsVASOrderWorkflowProviderTest : WorkflowProviderTest<WhsVASOrder, WhsVASOrderProcessTaskCollection>
	{
		#region TestGetTemplateFilterCriteria

		public void TestGetTemplateFilterCriteria()
		{
			var vasOrder = BusinessObject;
			var whs1 = Helper.CreateWarehouse("Wh1");
			var whs2 = Helper.CreateWarehouse("Wh2");
			var client1 = Helper.CreateClient("Cl1");
			var client2 = Helper.CreateClient("Cl2");

			AssertGetTemplateFilterCriteria((clientPK) => vasOrder.WVO_OH_Client = clientPK, ProcessTaskTemplate.P0_OH_ClientInfo, client1.PK, client2.PK, ZGuid.Empty);
			AssertGetTemplateFilterCriteria((whsPk) =>
			{
				vasOrder.WarehousePK = whsPk;
				vasOrder.WVO_WA_ServiceArea = vasOrder.Warehouse.Areas[0].PK;
			}, ProcessTaskTemplate.P0_WWInfo, whs1.PK, whs2.PK, ZGuid.Empty);
		}

		#endregion

		#region ExpectedWorkflowType

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.WhsVASOrderWorkflowDescriptorCode; }
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
