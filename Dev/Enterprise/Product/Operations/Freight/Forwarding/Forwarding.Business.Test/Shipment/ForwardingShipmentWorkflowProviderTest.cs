using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment))]
	sealed class ForwardingShipmentWorkflowProviderTest : WorkflowProviderTest<ForwardingShipment, ForwardingShipmentProcessTaskCollection>
	{
		public void TestTemplateTasksAreCreatedOnlyForForwardRegisteredShipments()
		{
			var (shipment1, shipment2) = CreateTestShipment();
			Factory.Save();

			AssertEquals("Template tasks added", 1, ((IWorkflowProvider)shipment1).WorkflowItems.Milestones.Count);
			AssertEquals("Template tasks NOT added when shipment is not forward registered", 0, ((IWorkflowProvider)shipment2).WorkflowItems.Milestones.Count);
		}

		(ForwardingShipment shipment1, ForwardingShipment shipment2) CreateTestShipment()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExpectedWorkflowType;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "XXX";

			Factory.Save();

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_IsForwardRegistered = true;
			shipment1.HasChanges = true;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_IsForwardRegistered = false;
			shipment2.HasChanges = true;

			return (shipment1, shipment2);
		}

		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria_ForConsignee()
		{
			Shipment.JS_RL_NKOrigin = "MYPKG";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals(true, Shipment.IsImport());

			Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ConsigneePKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);

			var clientInTemplateSelectionCriteriaCollection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode("SHP");
			clientInTemplateSelectionCriteria.SelectedItems.Remove(clientInTemplateSelectionCriteria.SelectedItems.GetValueByCode("CON"));

			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);

			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ShipmentJobHeader.LocalChargesPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForConsignor()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(true, Shipment.IsExport());

			Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ConsignorPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);

			var clientInTemplateSelectionCriteriaCollection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode("SHP");
			clientInTemplateSelectionCriteria.SelectedItems.MoveItem(0, 1);

			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);

			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ShipmentJobHeader.LocalChargesPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
			AssertGetTemplateFilterCriteria(Shipment.ConsigneePKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForLocalClient()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(true, Shipment.IsExport());

			Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ConsignorPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);

			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ConsigneePKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
			AssertGetTemplateFilterCriteria(Shipment.ShipmentJobHeader.LocalChargesPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);

			var clientInTemplateSelectionCriteriaCollection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode("SHP");
			clientInTemplateSelectionCriteria.SelectedItems.Remove(clientInTemplateSelectionCriteria.SelectedItems.GetValueByCode("CON"));

			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);

			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ShipmentJobHeader.LocalChargesPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForControllingCustomer()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(true, Shipment.IsExport());

			Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ConsignorPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);

			JobDocAddress address = ((IDocAddresses)Shipment).DocAddresses.AddNew(DocAddressType.ControllingCustomer);
			address.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ConsigneePKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
			AssertGetTemplateFilterCriteria(Shipment.ShipmentJobHeader.LocalChargesPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);

			var clientInTemplateSelectionCriteriaCollection = (ClientInTemplateSelectionCriteriaCollection)WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value.Clone(null, null);
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode("SHP");
			clientInTemplateSelectionCriteria.SelectedItems.Remove(clientInTemplateSelectionCriteria.SelectedItems.GetValueByCode("CON"));
			clientInTemplateSelectionCriteria.SelectedItems.Remove(clientInTemplateSelectionCriteria.SelectedItems.GetValueByCode("LOC"));
			clientInTemplateSelectionCriteria.SelectedItems.Add(clientInTemplateSelectionCriteria.AvailableItems.GetValueByCode("CPY"));

			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);

			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer).OrganisationPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForTransportMode()
		{
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_TransportModeInfo, ProcessTaskTemplate.P0_SubType1Info, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForOrigin()
		{
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_RL_NKOriginInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForDestination()
		{
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_RL_NKDestinationInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForBranch()
		{
			Job.Factory.Save();
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Job.JH_GBInfo, ProcessTaskTemplate.P0_GBInfo, GlbBranch.CurrentBranch.PK, Branch.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForDepartment()
		{
			Job.Factory.Save();
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Job.JH_GEInfo, ProcessTaskTemplate.P0_GEInfo, GlbDepartment.CurrentDepartment.PK, Department.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteriaUsesCurrentBranchAndDepartmentIfSetBlank()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.JS_RL_NKOrigin = "INBOM";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var branchPK = ZGuid.NewZGuid();
			var departmentPK = ZGuid.NewZGuid();
			Job.JH_GB = branchPK;
			Job.JH_GE = departmentPK;
			ColumnValueRanker columnValueRanker = (ColumnValueRanker)(((IWorkflowProvider)Shipment).GetTemplateSelectionCriteria());
			AssertEquals(Job.JH_GB, columnValueRanker.GetValues(ProcessTaskTemplateSchema.P0_GB)[0]);
			AssertEquals(Job.JH_GE, columnValueRanker.GetValues(ProcessTaskTemplateSchema.P0_GE)[0]);
			AssertEquals("We should not change the branch PK while GetTemplateSelectionCriteria", branchPK, Job.JH_GB);
			AssertEquals("We should not change the department PK while GetTemplateSelectionCriteria", departmentPK, Job.JH_GE);

			Job.JH_GB = ZGuid.Empty;
			Job.JH_GE = ZGuid.Empty;
			columnValueRanker = (ColumnValueRanker)(((IWorkflowProvider)Shipment).GetTemplateSelectionCriteria());
			AssertEquals(GlbBranch.CurrentBranch.PK, columnValueRanker.GetValues(ProcessTaskTemplateSchema.P0_GB)[0]);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, columnValueRanker.GetValues(ProcessTaskTemplateSchema.P0_GE)[0]);
			AssertEquals("We should not change the branch PK while GetTemplateSelectionCriteria", ZGuid.Empty, Job.JH_GB);
			AssertEquals("We should not change the department PK while GetTemplateSelectionCriteria", ZGuid.Empty, Job.JH_GE);
		}

		public void TestGetTemplateSelectionCriteria_LoadedJobHasParent()
		{
			Shipment.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(Shipment.PK);
			var jobInNewFactory = newFactory.Load<JobHeader>(Job.PK);

			((IWorkflowProvider)shipmentInNewFactory).GetTemplateSelectionCriteria();
			AssertNotNull("We should load job with parent in GetTemplateSelectionCriteria", jobInNewFactory.Parent);
		}

		public void TestGetTemplateSelectionCriteriaDoesNotEnableHasChanges()
		{
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			var jobLoader = new JobHeader.Loader(shipment);
			var job = jobLoader.TryLoadOrCreate();
			Factory.Save();

			var consol = new BusinessObjectFactory().New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			consol.Shipments.Add(shipment);
			consol.Factory.Save();

			var shipmentInDifferentFactory = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			var x = shipmentInDifferentFactory.ShipmentJobHeader; // ShipmentJobHeader getter needs to be hit to set PlugInData on Job
			((IWorkflowProvider)shipmentInDifferentFactory).GetTemplateSelectionCriteria();
			AssertEquals("GetTemplateSelectionCriteria should not enable HasChanges", false, shipmentInDifferentFactory.HasChanges);
		}

		#endregion

		#region Implementation

		ForwardingShipment Shipment
		{
			get { return BusinessObject; }
		}

		JobHeader Job
		{
			get
			{
				if (job == null)
				{
					job = new JobHeader.Loader(Shipment).TryLoadOrCreate();
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				}
				return job;
			}
		}

		JobHeader job;

		GlbDepartment Department
		{
			get { return department ?? (department = Factory.NewWithValidTestData<GlbDepartment>()); }
		}
		GlbDepartment department;

		protected override ZString ExpectedWorkflowType
		{
			get { return JobInvoicingConsumerTypes.Shipment.Code; }
		}

		protected override ForwardingShipment GetNewBusinessObject(BusinessObjectFactory factory)
		{
			ForwardingShipment result = base.GetNewBusinessObject(factory);
			JobHeader job = new JobHeader.Loader(result).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return result;
		}

		protected override void SetupBusinessObjectForDeletion()
		{
			Shipment.Job?.Delete();
		}

		#endregion
	}
}
