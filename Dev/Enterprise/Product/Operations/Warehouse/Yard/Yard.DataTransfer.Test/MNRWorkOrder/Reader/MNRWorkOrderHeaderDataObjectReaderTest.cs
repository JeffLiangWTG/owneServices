using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;
using UniversalMNRWorkOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.MNRWorkOrderLine;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(MNRWorkOrderHeaderDataObjectReader))]
	public class MNRWorkOrderHeaderDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			Data.SetupDataForTesting();

			SetUpTestData();
		}

		public void TestReadFromDataObject()
		{
			#region Setup UXML

			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.MNRWorkOrder, null);

			shipment.MNRStartEquipmentGrade = new CodeDescriptionPair { Code = "AMO" };
			shipment.MNREndEquipmentGrade = new CodeDescriptionPair { Code = "AMO" };
			shipment.MNRWorkOrderApprovedTime = DateTimeOffset.Now;
			shipment.MNRType = new CodeDescriptionPair { Code = "STL" };
			shipment.MNRRevision = 0;
			shipment.SetMNRWorkOrderLineCollection(() => new DataObjectList<UniversalMNRWorkOrderLine>()
			{
				new UniversalMNRWorkOrderLine
				{
					ComponentCode = new CodeGroupPair { Code = "RCC001", Group = "MERC" },
					UnitSection = new CodeGroupPair { Code = "RUS001", Group = "MERC" },
					RepairCode = new CodeGroupPair { Code = "RRC001", Group = "MERC" },
					Material = new CodeGroupPair { Code = "RMC001", Group = "MERC" },
					Damage = new CodeGroupPair { Code = "RFM001", Group = "MERC" },
					UnitOfDimension = "CM",
					ResponsibleParty = "OWN",
					Width = 0,
					Length = 0,
					MaterialQuantity = 0,
					LaborHours = 0,
					Description = "TEST Description",
				}
			});

			shipment.SetContainerCollection(() =>
			{
				var container = UniversalTestHelper.CreateContainer("UN02170101", "LD-3", true);
				var containers = new DataObjectList<Container> { container };
				return containers;
			});

			#endregion Setup UXML

			var workOrderHeader = new MNRWorkOrderHeaderDataObjectReader(shipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();

			var workOrders = newFactory.Load<MNRWorkOrderHeader>(new ZQuery());
			AssertEquals("1 WorkOrder should be populated.", 1, workOrders.Length);

			var workOrder = workOrders.First();
			AssertEquals("AMO", workOrder.StartEquipmentGrade.REG_Code);
			AssertEquals("AMO", workOrder.EndEquipmentGrade.REG_Code);
			AssertEquals("STL", workOrder.MWO_Type);
		}

		void SetUpTestData()
		{
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

			Factory.SaveForTesting();
		}
	}
}
