using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentHeader))]
	sealed class CarrierShipmentHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "CS00001";
			return carrierShipmentHeader;
		}

		#region Creation, Saving and properties

		public void TestCodeDescriptionAttributes()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "REF0001";
			carrierShipmentHeader.CSH_HouseBill = "HBL0001";

			var code = CodePropertyAttribute.CodeFromBusinessObject(carrierShipmentHeader);
			AssertEquals("CodeProperty value", "REF0001", code);

			var description = DescriptionPropertyAttribute.DescriptionFromBusinessObject(carrierShipmentHeader);
			AssertEquals("DescriptionProperty value", "HBL0001", description);
		}

		public void TestHumanReadableName()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();

			AssertEquals($"{nameof(CarrierShipmentHeader.HumanReadableName)} no CSH_HouseBill and no CSH_CarrierShipmentReference",
				"Carrier Shipment", carrierShipmentHeader.HumanReadableName);

			carrierShipmentHeader.CSH_HouseBill = "HBL001";
			AssertEquals($"{nameof(CarrierShipmentHeader.HumanReadableName)} with CSH_HouseBill and no CSH_CarrierShipmentReference",
				"Carrier Shipment (House Bill='HBL001')", carrierShipmentHeader.HumanReadableName);

			carrierShipmentHeader.CSH_HouseBill = ZString.Empty;
			carrierShipmentHeader.CSH_CarrierShipmentReference = "C0001";
			AssertEquals($"{nameof(CarrierShipmentHeader.HumanReadableName)} no CSH_HouseBill and with CSH_CarrierShipmentReference",
				"Carrier Shipment C0001", carrierShipmentHeader.HumanReadableName);

			carrierShipmentHeader.CSH_HouseBill = "HBL001";
			carrierShipmentHeader.CSH_CarrierShipmentReference = "C0001";
			AssertEquals($"{nameof(CarrierShipmentHeader.HumanReadableName)} with CSH_HouseBill and with CSH_CarrierShipmentReference",
				"Carrier Shipment C0001 (House Bill='HBL001')", carrierShipmentHeader.HumanReadableName);
		}

		public void TestCreationShouldFillDefaultValues()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();

			AssertEquals(carrierShipmentHeader.CSH_IsActive, true);
			AssertEquals(carrierShipmentHeader.CSH_CarrierShipmentReference, string.Empty);
			AssertEquals(carrierShipmentHeader.CSH_HouseBill, string.Empty);
			AssertEquals(carrierShipmentHeader.CSH_ScreeningStatus, "NOT");
		}

		public void TestAfterSavingAuditFieldsAreFilled()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "DF00001";

			Factory.Save();

			AssertNotSame(carrierShipmentHeader.CSH_SystemCreateTimeUtc, ZDateTime.Empty);
			AssertNotNullOrEmpty(carrierShipmentHeader.CSH_SystemCreateUser);
			AssertNotSame(carrierShipmentHeader.CSH_SystemLastEditTimeUtc, ZDateTime.Empty);
			AssertNotNullOrEmpty(carrierShipmentHeader.CSH_SystemLastEditUser);
		}

		public void TestSupportedNoteTypes()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();

			AssertContainsExactElementsInAnyOrder("Supported notetypes", new[]
			{
				PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description
			},
			carrierShipmentHeader.NoteTypes.Cast<PredefinedNoteType>().Select(nt => nt.Description));
		}

		public void TestWhenSavingCarrierShipmentWorkflowShouldBeApplied()
		{
			var workflowTemplate = Factory.New<ProcessTaskTemplate>();
			workflowTemplate.P0_Name = "OCS test template";
			workflowTemplate.P0_ProcessType = WorkflowDescriptors.CarrierShipmentHeaderWorkflowDescriptorCode;

			var trigger = workflowTemplate.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "I am a trigger";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;

			Factory.Save();

			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "REF00001";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("workflow template has been applied",
				new[]
				{
					"I am a trigger"
				},
				carrierShipmentHeader.WorkflowItems.Select(wi => wi.P9_Description));
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_ShipmentType = "SHP";
			var selectionCriteria = carrierShipmentHeader.GetTemplateSelectionCriteria() as ColumnValueRanker;
			Assert("CarrierShipmentHeader.GetTemplateSelectionCriteria Entity Type", selectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1).First().ToString() == carrierShipmentHeader.CSH_ShipmentType);
		}

		public void TestGenerateCarrierShipmentReference()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode].Include = true;
			OceanCarrierDataRegistry.Instance.OceanCarrierShipmentReferenceNumberFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();

			Factory.Save();
			AssertEquals("Generated number", $"CA{GlbBranch.CurrentBranch.GB_Code}00000001", carrierShipmentHeader.CSH_CarrierShipmentReference);
		}

		#endregion

		#region JobHeader related tests

		public void TestWhenCreatingCarrierShipmentJobHeaderIsNotCreated()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "DF00001";
			AssertNull("Carrier Shipment JobHeader is not created", carrierShipmentHeader.JobHeader);

			Factory.Save();

			AssertNull("Carrier Shipment JobHeader is not created", carrierShipmentHeader.JobHeader);
		}

		public void TestWhenUnsavedCarrierShipmentIsDeletedThenRelatedJobHeaderIsDeleted()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "DF00001";

			new JobHeader.Loader(carrierShipmentHeader).TryLoadOrCreate();
			AssertNotNull("prerequisite: Carrier Shipment JobHeader is created", carrierShipmentHeader.JobHeader);

			carrierShipmentHeader.Delete();

			Assert("Carrier Shipment is marked as deleted", carrierShipmentHeader.IsDeleted);
			AssertNull("Carrier Shipment JobHeader is deleted", carrierShipmentHeader.JobHeader);
		}

		public void TestWhenSavedCarrierShipmentIsDeleted_ShouldAllowDeletion_WhenJobHeaderDoesNotExistsInDatabase()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "DF00001";
			Factory.Save();

			new JobHeader.Loader(carrierShipmentHeader).TryLoadOrCreate();
			AssertNotNull("prerequisite: Carrier Shipment JobHeader is created", carrierShipmentHeader.JobHeader);

			carrierShipmentHeader.Delete();

			Assert("Carrier Shipment is marked as deleted", carrierShipmentHeader.IsDeleted);
			AssertNull("Carrier Shipment JobHeader is deleted", carrierShipmentHeader.JobHeader);
		}

		#endregion

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			AssertEquals("Should be 0 business objects with related events", 0, carrierShipmentHeader.BusinessObjectsWithRelatedEvents.Length);
			carrierShipmentHeader.Cargoes.AddNew();
			AssertEquals("Business objects with related events", 1, carrierShipmentHeader.BusinessObjectsWithRelatedEvents.Length);
			carrierShipmentHeader.Cargoes.AddNew();
			AssertEquals("Business objects with related events", 2, carrierShipmentHeader.BusinessObjectsWithRelatedEvents.Length);
			var jobHeader = new JobHeader.Loader(carrierShipmentHeader).TryCreate();
			AssertEquals("Business objects with related events", 3, carrierShipmentHeader.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains("Business objects with related events contains the JobHeader", jobHeader, carrierShipmentHeader.BusinessObjectsWithRelatedEvents);
		}
	}
}
