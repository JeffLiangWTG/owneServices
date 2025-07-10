using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentCargoWorkflowDescriptor))]
	sealed class CarrierShipmentCargoWorkflowDescriptorTest : WorkflowDescriptorTestCase<CarrierShipmentCargoWorkflowDescriptor>
	{
		public override void TestID() => AssertEquals("Expected Workflow Descriptor Code", "OCC", WorkflowDescriptor.Code);

		public override void TestDescription() => AssertEquals("Expected Workflow Descriptor Description", "Ocean Carrier Cargo", WorkflowDescriptor.Description);

		public override void TestSubTypes()
		{
			AssertEquals("we have 2 selection criterias", 2, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Cargo Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
		}

		public void TestSubType1ForContainer()
		{
			ICodeDescriptionPairList subType2List = new CodeDescriptionPairList(OLookUpEditType.ContainerType);
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptor.Code;
			template.P0_SubType1 = Core.Constants.OceanCarrierCargoTypes.Codes.Container;
			WorkflowDescriptor.LastProcessTaskTemplate = template;

			CodeDescriptionPairList containerTypeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[1].List;
			AssertEquals("Container Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertContainsExactElementsInAnyOrder("Container Type List", subType2List, containerTypeList);
		}

		public void TestSubType1ForRoRo()
		{
			ICodeDescriptionPairList subType2List = new CodeDescriptionPairList(OLookUpEditType.RoRoTypes);
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptor.Code;
			template.P0_SubType1 = Core.Constants.OceanCarrierCargoTypes.Codes.RoRo;
			WorkflowDescriptor.LastProcessTaskTemplate = template;

			CodeDescriptionPairList roRoTypeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[1].List;
			AssertEquals("RoRo Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertContainsExactElementsInAnyOrder("RoRo Type List", subType2List, roRoTypeList);
		}

		public void TestSubType1ForBreakBulk()
		{
			var subType2List = new CodeDescriptionPairList();
			RefPackTypeCollection packTypes = new RefPackTypeCollection(new BusinessObjectFactory(), true);
			packTypes.ApplySort(RefPackTypeSchema.F3_Code.Name, ListSortDirection.Ascending);
			foreach (RefPackType packType in packTypes)
			{
				subType2List.AddPair(packType.F3_Code, packType.F3_DescriptionMultilingual);
			}

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptor.Code;
			template.P0_SubType1 = Core.Constants.OceanCarrierCargoTypes.Codes.BreakBulk;
			WorkflowDescriptor.LastProcessTaskTemplate = template;

			CodeDescriptionPairList breakBulkTypeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[1].List;
			AssertEquals("Break Bulk Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertContainsExactElementsInAnyOrder("Break Bulk Type List", subType2List, breakBulkTypeList);
		}

		public void TestSubType1ForAll()
		{
			var subType2List = new CodeDescriptionPairList();
			subType2List.AddPair("", "All");

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptor.Code;
			template.P0_SubType1 = Core.Constants.OceanCarrierCargoTypes.Codes.All;
			WorkflowDescriptor.LastProcessTaskTemplate = template;

			CodeDescriptionPairList allTypeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[1].List;
			AssertEquals("Sub Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertContainsExactElementsInAnyOrder("Sub Type 'All'", subType2List, allTypeList);
		}

		public override void TestRequiresPorts()
		{
			Assert(nameof(WorkflowDescriptor.RequiresPort1), !WorkflowDescriptor.RequiresPort1);
			Assert(nameof(WorkflowDescriptor.RequiresPort2), !WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient() => Assert(nameof(WorkflowDescriptor.RequiresClient), !WorkflowDescriptor.RequiresClient);

		public override void TestRequiresBranch() => Assert(nameof(WorkflowDescriptor.RequiresBranch), WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresDepartment() => Assert(nameof(WorkflowDescriptor.RequiresDepartment), WorkflowDescriptor.RequiresDepartment);

		public override void TestSupportsEventTracking() => Assert(nameof(WorkflowDescriptor.SupportsEventTracking), WorkflowDescriptor.SupportsEventTracking);

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CS00003";
			var shipmentCargo = Factory.New<CarrierShipmentCargo>();
			shipmentCargo.CSC_CSH_CarrierShipment = shipment.PK;
			shipmentCargo.CSC_CargoMovementTypeOrigin = "FCL";
			shipmentCargo.CSC_CargoMovementTypeDestination = "FCL";
			shipmentCargo.CSC_ReceiptDrayage = "ANY";
			shipmentCargo.CSC_DeliveryDrayage = "ANY";

			return new IWorkflowProvider[] { shipmentCargo };
		}
	}
}
