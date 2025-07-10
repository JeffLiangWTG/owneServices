using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRWorkOrderLine))]
	class MNRWorkOrderLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var machineryMake = Factory.NewWithValidTestData<RefMachineryMake>();

			var componentCode = Factory.NewWithValidTestData<RefMRComponentCode>();
			componentCode.RCC_Group = "CEDEX";

			var machineryPart = Factory.NewWithValidTestData<RefMachineryPart>();
			machineryPart.RMP_RMM_MachineryMake = machineryMake.PK;
			machineryPart.RMP_RCC_ComponentCode = componentCode.PK;

			var workOrderLine = Factory.NewWithValidTestData<MNRWorkOrderLine>();
			workOrderLine.MWL_RMP_PartNumber = machineryPart.PK;
			workOrderLine.MWL_RMM_MachineryMake = machineryMake.PK;

			return workOrderLine;
		}

		public void TestWorkOrderHeader()
		{
			var header = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			var line = (MNRWorkOrderLine)GetNewBusinessObject();
			line.MWL_MWO_MNRWorkOrderHeader = header.PK;
			AssertEquals(header, line.WorkOrderHeader);
		}
	}
}
