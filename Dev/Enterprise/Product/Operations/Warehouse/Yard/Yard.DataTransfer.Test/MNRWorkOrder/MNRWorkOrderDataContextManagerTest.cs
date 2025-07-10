using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using MNRWorkOrderLine = Enterprise.Warehouse.Yard.Business.MNRWorkOrderLine;
using UniversalMNRWorkOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.MNRWorkOrderLine;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(MNRWorkOrderDataContextManager))]
	public class MNRWorkOrderDataContextManagerTest : ShipmentDataContextManagerTestCase<MNRWorkOrderDataContextManager, MNRWorkOrderHeader>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.MRNWorkOrder.xml");

		UniversalTestData Data;
		Lazy<EmbeddedResourceRetriever> ResourceRetriever;
		CYDYardUnitState yardUnitState;
		MNRWorkOrderHeader workOrder;
		List<MNRWorkOrderLine> workOrderLines;

		protected override void SetUp()
		{
			base.SetUp();
			ResourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			Data.SetupDataForTesting();
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			(yardUnitState, workOrder, workOrderLines) = SetUpTestData();
		}

		public void TestMatchingByDataContextKey()
		{
			(yardUnitState, workOrder, workOrderLines) = SetUpTestData();
			var workOrderLine = workOrderLines.First();

			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.MNRWorkOrder, "MWO00000001");

			shipment.MNRStartEquipmentGrade = new CodeDescriptionPair { Code = workOrder.StartEquipmentGrade.REG_Code };
			shipment.MNREndEquipmentGrade = new CodeDescriptionPair { Code = workOrder.EndEquipmentGrade.REG_Code };
			shipment.MNRWorkOrderApprovedTime = workOrder.MWO_WorkOrderApprovedTime;
			shipment.MNRType = new CodeDescriptionPair { Code = workOrder.MWO_Type };
			shipment.MNRRevision = workOrder.MWO_Revision;
			shipment.SetMNRWorkOrderLineCollection(() => new DataObjectList<UniversalMNRWorkOrderLine>
			{
				new UniversalMNRWorkOrderLine
				{
					ComponentCode = new CodeGroupPair { Code = workOrderLine.ComponentCode.RCC_Code, Group = workOrderLine.ComponentCode.RCC_Group },
					UnitSection = new CodeGroupPair { Code = workOrderLine.UnitSection.RUS_Code, Group = workOrderLine.UnitSection.RUS_Group },
					RepairCode = new CodeGroupPair { Code = workOrderLine.RepairCode.RRC_Code, Group = workOrderLine.RepairCode.RRC_Group },
					Material = new CodeGroupPair { Code = workOrderLine.Material.RMC_Code, Group = workOrderLine.Material.RMC_Group },
					Damage = new CodeGroupPair { Code = workOrderLine.Damage.RFM_Code, Group = workOrderLine.Damage.RFM_Group },
					UnitOfDimension = workOrderLine.MWL_UnitOfDimension,
					ResponsibleParty = workOrderLine.MWL_ResponsibleParty,
					Width = workOrderLine.MWL_Width,
					Length = workOrderLine.MWL_Length,
					MaterialQuantity = workOrderLine.MWL_MaterialQuantity,
					LaborHours = workOrderLine.MWL_LaborHours,
					Description = workOrderLine.MWL_Description
				}
			});

			shipment.SetContainerCollection(() =>
			{
				var container = UniversalTestHelper.CreateContainer("CON1", "LD-3", true);
				container.ContainerNumber = yardUnitState.YUS_UnitID;
				container.ContainerJobID = workOrder.MWO_JobNumber;
				var containers = new DataObjectList<Container> { container };
				return containers;
			});

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipment);
			manager.Process(message);

			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("Updated Work order MWO00000001 from UniversalShipment.", serviceTaskLog.Logs.First().Message);
			AssertEquals("Successfully saved Work order MWO00000001 with 1 x MNRWorkOrderLine.", serviceTaskLog.Logs.Last().Message);
		}

		public void TestMNRWorkOrderHeaderDataObjectReader()
		{
			(yardUnitState, workOrder, workOrderLines) = SetUpTestData(2);

			var dataObject = new MNRWorkOrderHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, workOrder))).GetDataObject(workOrder);
			dataObject.DataContext.DataSourceCollection.Single().Key = null;

			workOrder.Delete();
			Factory.SaveForTesting();

			var workOrderHeader = Factory.LoadTop1<MNRWorkOrderHeader>(new ZQuery(MNRWorkOrderHeaderSchema.MWO_ParentID, yardUnitState.PK));
			AssertNull("Precondition: No work orders created for this Yard Unit:", workOrderHeader);

			workOrderHeader = new MNRWorkOrderHeaderDataObjectReader(dataObject, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();

			workOrderHeader = newFactory.LoadTop1<MNRWorkOrderHeader>(new ZQuery(MNRWorkOrderHeaderSchema.MWO_ParentID, yardUnitState.PK));
			AssertNotNull("WorkOrderHeader should be populated.", workOrderHeader);
			AssertEquals("2 WorkOrderLine should be populated.", 2, workOrderHeader.WorkOrderLines.Count);
		}

		(CYDYardUnitState yardUnitState, MNRWorkOrderHeader workOrder, List<MNRWorkOrderLine> workOrderLines) SetUpTestData(int lineCount = 1)
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

			var workOrder = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			workOrder.MWO_JobNumber = "MWO00000001";
			workOrder.MWO_ParentID = yardUnitState.PK;
			workOrder.MWO_ParentTableCode = CYDYardUnitStateSchema.Constants.Prefix;
			workOrder.MWO_REG_StartEquipmentGrade = equipmentGrade.PK;
			workOrder.MWO_REG_EndEquipmentGrade = equipmentGrade.PK;

			var workOrderLines = new List<MNRWorkOrderLine>();
			for (var i = 0; i < lineCount; i++)
			{
				var workOrderLine = Factory.NewWithValidTestData<MNRWorkOrderLine>();
				workOrderLine.MWL_MWO_MNRWorkOrderHeader = workOrder.PK;
				workOrderLine.MWL_Description = $"Test Description {i}";
				workOrderLine.MWL_RCC_ComponentCode = componentCode.PK;
				workOrderLine.MWL_RFM_Damage = damage.PK;
				workOrderLine.MWL_RRC_RepairCode = repairCode.PK;
				workOrderLine.MWL_RUS_UnitSection = unitSection.PK;
				workOrderLine.MWL_RMC_Material = material.PK;

				workOrderLines.Add(workOrderLine);
			}

			Factory.SaveForTesting();

			return (yardUnitState, workOrder, workOrderLines);
		}
	}
}
