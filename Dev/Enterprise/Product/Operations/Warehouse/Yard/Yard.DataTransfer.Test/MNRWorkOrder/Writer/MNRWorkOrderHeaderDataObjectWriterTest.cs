using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(MNRWorkOrderHeaderDataObjectWriter))]
	public class MNRWorkOrderHeaderDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			Data.SetupDataForTesting();
		}

		public void TestWriteToDataObject()
		{
			#region Setup test data

			var yardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState.YUS_UnitID = "UN02170101";

			var componentCode = Factory.NewWithValidTestData<RefMRComponentCode>();
			componentCode.RCC_Code = "RCC001";
			componentCode.RCC_Group = "MERC";
			var damage = Factory.NewWithValidTestData<RefDamage>();
			damage.RFM_Code = "RFM001";
			damage.RFM_Group = "MERC";
			var repairCode = Factory.NewWithValidTestData<RefRepairCode>();
			repairCode.RRC_Code = "RRC001";
			repairCode.RRC_Group = "MERC";
			repairCode.RRC_ServiceType = "CLN";
			var unitSection = Factory.NewWithValidTestData<RefUnitSection>();
			unitSection.RUS_Code = "RUS001";
			unitSection.RUS_Group = "MERC";
			var material = Factory.NewWithValidTestData<RefMaterial>();
			material.RMC_Code = "RMC001";
			material.RMC_Group = "MERC";

			var equipmentGrade = Factory.NewWithValidTestData<RefEquipmentGrade>();
			equipmentGrade.REG_Code = "AMO";

			var workOrder = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			workOrder.MWO_JobNumber = "MWO00000001";
			workOrder.MWO_ParentID = yardUnitState.PK;
			workOrder.MWO_ParentTableCode = CYDYardUnitStateSchema.Constants.Prefix;
			workOrder.MWO_REG_StartEquipmentGrade = equipmentGrade.PK;
			workOrder.MWO_REG_EndEquipmentGrade = equipmentGrade.PK;

			var workOrderLine = Factory.NewWithValidTestData<MNRWorkOrderLine>();
			workOrderLine.MWL_MWO_MNRWorkOrderHeader = workOrder.PK;
			workOrderLine.MWL_Description = "Test Description";
			workOrderLine.MWL_RCC_ComponentCode = componentCode.PK;
			workOrderLine.MWL_RFM_Damage = damage.PK;
			workOrderLine.MWL_RRC_RepairCode = repairCode.PK;
			workOrderLine.MWL_RUS_UnitSection = unitSection.PK;
			workOrderLine.MWL_RMC_Material = material.PK;

			Factory.SaveForTesting();

			#endregion

			var dataObject = new MNRWorkOrderHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, workOrder))).GetDataObject(workOrder);

			AssertEquals(nameof(DataContextType.MNRWorkOrder), dataObject.DataContext.DataSourceCollection.FirstOrDefault().Type);
			AssertEquals("MWO00000001", dataObject.DataContext.DataSourceCollection.FirstOrDefault().Key);

			var containerInfo = dataObject.ContainerCollection.FirstOrDefault();
			AssertEquals("Container for UN02170101 should be in Container Collection.", "UN02170101", containerInfo.ContainerNumber);
			AssertEquals("AMO", dataObject.MNRStartEquipmentGrade.Code);
			AssertEquals("AMO", dataObject.MNREndEquipmentGrade.Code);

			var workOrderLineCollection = dataObject.MNRWorkOrderLineCollection;
			AssertEquals(1, workOrderLineCollection.Count);

			var workOrderLineDataObject = workOrderLineCollection.First();
			AssertEquals("RUS001", workOrderLineDataObject.UnitSection.Code);
			AssertEquals("MERC", workOrderLineDataObject.UnitSection.Group);
			AssertEquals("RMC001", workOrderLineDataObject.Material.Code);
			AssertEquals("MERC", workOrderLineDataObject.Material.Group);
			AssertEquals("RFM001", workOrderLineDataObject.Damage.Code);
			AssertEquals("MERC", workOrderLineDataObject.Damage.Group);
			AssertEquals("RCC001", workOrderLineDataObject.ComponentCode.Code);
			AssertEquals("MERC", workOrderLineDataObject.ComponentCode.Group);
			AssertEquals("RRC001", workOrderLineDataObject.RepairCode.Code);
			AssertEquals("MERC", workOrderLineDataObject.RepairCode.Group);
		}
	}
}
