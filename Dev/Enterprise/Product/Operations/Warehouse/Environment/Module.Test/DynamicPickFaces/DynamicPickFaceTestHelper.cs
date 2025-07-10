using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	class DynamicPickFaceTestHelper
	{
		public DynamicPickFaceTestHelper(BusinessObjectFactory factory)
		{
			Helper = new WhsTestHelperFunctionsEnv(factory);
			TransactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			Whs = Helper.CreateWarehouse("1");
			Location = Helper.CreateRowAndGenerateLocations(Whs, "A", 1, 1).Locations.Single();
			Client = Helper.CreateClient();
			Part = Helper.CreateProduct(Client, "P1");
		}

		public void CreateAndBindDynamicPickingArea(string areaName = "DYNAMIC")
		{
			var dynamicPickingArea = Helper.CreateDynamicPF(Whs, Location, areaName);
			BindProductToDynamicPickingArea(dynamicPickingArea);
		}

		public void BindProductToDynamicPickingArea(WhsArea dynamicPickingArea)
		{
			BindProductToDynamicPickingArea(Part, Whs, Client, dynamicPickingArea);
		}

		public void BindProductToDynamicPickingArea(OrgSupplierPart product, WhsWarehouse whs, OrgHeader client, WhsArea dynamicPickingArea)
		{
			var w3 = Helper.Factory.New<IWhsProductParamsByWhsAndClient>();
			w3.W3_OP = product.PK;
			w3.W3_WW = whs.PK;
			w3.W3_OH = client.PK;
			w3.W3_WA_DynamicPickFaceArea = dynamicPickingArea.PK;
		}

		public WhsWarehouse Whs { get; }
		public WhsLocation Location { get; }
		public OrgHeader Client { get; }
		public OrgSupplierPart Part { get; }
		public WhsTestHelperFunctionsEnv Helper { get; }
		public IWhsTransactionTestHelper TransactionHelper { get; }
	}
}
