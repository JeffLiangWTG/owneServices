using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsJobServiceLookupsTest : JobServiceLookupsTest
	{
		protected override CodeDescriptionPairList GetValidJobServiceTypes()
		{
			CodeDescriptionPairList result = base.GetValidJobServiceTypes();
			result.AddPair("WHS", "Random Warehouse Service");

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			SystemDefinableCodeDescriptionBoolCollection warehouseJobServices = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServices.Add("WHS", (NoResString)"Random Warehouse Service", false);
			warehouseJobServices.SetDefaultCode("WHS", true);

			WarehouseDataRegistry.Instance.JobServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, warehouseJobServices);
		}

		protected override JobService GetNewJobService()
		{
			return Factory.New<WhsOrder>().Services.AddNew();
		}
	}
}
