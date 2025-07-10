using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentCargo))]
	sealed class CarrierShipmentCargoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCarrierShipment()
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			AssertNull(nameof(cargo.CarrierShipmentHeader), cargo.CarrierShipmentHeader);

			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			cargo.CSC_CSH_CarrierShipment = carrierShipmentHeader.PK;

			AssertEquals(nameof(cargo.CarrierShipmentHeader), carrierShipmentHeader, cargo.CarrierShipmentHeader);
		}

		public void TestGetTemplateSelectionCriteriaForContainer()
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			cargo.CSC_CargoType = "CNT";
			var referenceContainer = Factory.NewWithValidTestData<RefContainer>();
			referenceContainer.RC_Code = "20GP";
			referenceContainer.RC_ContainerType = "FLT";
			cargo.CSC_RC_ChargeableEquipmentType = referenceContainer.PK;
			var selectionCriteria = cargo.GetTemplateSelectionCriteria() as ColumnValueRanker;
			Assert("CarrierShipmentCargo.GetTemplateSelectionCriteria Cargotype", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1).First().ToString() == cargo.CSC_CargoType);
			Assert("CarrierShipmentCargo.GetTemplateSelectionCriteria Container Type", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType2).First().ToString() == cargo.ChargeableEquipmentType.RC_ContainerType);
		}

		public void TestGetTemplateSelectionCriteriaForRoRo()
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			cargo.CSC_CargoType = "ROR";
			cargo.CSC_RoRoType = "CAR";
			var selectionCriteria = cargo.GetTemplateSelectionCriteria() as ColumnValueRanker;
			Assert("CarrierShipmentCargo.GetTemplateSelectionCriteria Cargotype", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1).First().ToString() == cargo.CSC_CargoType);
			Assert("CarrierShipmentCargo.GetTemplateSelectionCriteria RoRo Type", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType2).First().ToString() == cargo.CSC_RoRoType);
		}

		public void TestGetTemplateSelectionCriteriaForBreakBulk()
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			cargo.CSC_CargoType = "BBK";
			cargo.CSC_F3_NKPackType = "PKG";
			var selectionCriteria = cargo.GetTemplateSelectionCriteria() as ColumnValueRanker;
			Assert("CarrierShipmentCargo.GetTemplateSelectionCriteria Cargotype", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1).First().ToString() == cargo.CSC_CargoType);
			Assert("CarrierShipmentCargo.GetTemplateSelectionCriteria Break Bulk Type", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType2).First().ToString() == cargo.CSC_F3_NKPackType);
		}

		public void TestGetTemplateSelectionCriteriaForAll()
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			cargo.CSC_CargoType = "BBK";
			cargo.CSC_F3_NKPackType = ZString.Empty;
			var selectionCriteria = cargo.GetTemplateSelectionCriteria() as ColumnValueRanker;
			Assert("CarrierShipmentCargo.GetTemplateSelectionCriteria Cargotype", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1).First().ToString() == cargo.CSC_CargoType);
			Assert("CarrierShipmentCargo.GetTemplateSelectionCriteria All", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType2).First().ToString() == cargo.CSC_F3_NKPackType);
		}

		public void TestGetTotalCargoGrossWeightMeasure()
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			var referenceContainer = Factory.NewWithValidTestData<RefContainer>();
			referenceContainer.RC_Code = "20GP";
			referenceContainer.RC_ContainerType = "FLT";
			cargo.CSC_RC_ChargeableEquipmentType = referenceContainer.PK;
			cargo.CSC_DunnageWeight = 1000;
			cargo.CSC_PieceCount = 1;
			cargo.CSC_CargoWeight = 1939;
			cargo.CSC_EquipmentTareWeight = 1941;
			AssertEquals("GetTotalCargoGrossWeightMeasure", (ZDecimal)4880, cargo.GetTotalCargoGrossWeightMeasure());
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var cargo = Factory.NewWithValidTestData<CarrierShipmentCargo>();
			return cargo;
		}

		#endregion

		#region IWorkflowTriggerEventSource

		public void TestIWorkflowTriggerEventSource_JobHeaderCompany()
		{
			var item = Factory.NewWithValidTestData<CarrierShipmentCargo>();
			AssertEquals(GlbCompany.CurrentCompany.PK, ((IWorkflowTriggerEventSource)item).JobHeaderCompany.PK);
		}

		public void TestIWorkflowTriggerEventSource_ParentWorkflowProviders()
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			AssertEquals(0, ((IWorkflowTriggerEventSource)cargo).ParentWorkflowProviders.Count);

			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			cargo.CSC_CSH_CarrierShipment = carrierShipmentHeader.PK;

			AssertEquals(1, ((IWorkflowTriggerEventSource)cargo).ParentWorkflowProviders.Count);
			AssertEquals(carrierShipmentHeader.PK, ((IWorkflowTriggerEventSource)cargo).ParentWorkflowProviders[0].PK);
		}

		#endregion
	}
}
