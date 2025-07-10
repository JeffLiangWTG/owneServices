using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class AgencyWorkflowProviderTest<ShipmentT, ProcessTaskT> : WorkflowProviderTest<ShipmentT, ProcessTaskT> where ShipmentT : AgencyShipment where ProcessTaskT : AgencyShipmentProcessTaskCollection
	{
		#region TestGetTemplateSelectionCriteria
		public void TestGetTemplateSelectionCriteria_ForConsignee()
		{
			Shipment.JS_RL_NKOrigin = "MYPKG";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals(true, Shipment.IsImport());
			Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ConsigneePKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForConsignor()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(true, Shipment.IsExport());
			Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria(Shipment.ConsignorPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForContainerMode()
		{
			Shipment.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_PackingModeInfo, ProcessTaskTemplate.P0_SubType1Info, Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.RollOnRollOff, ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForLoadOrigin()
		{
			Shipment.JS_NKLoadPort = "";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_RL_NKOriginInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
			Shipment.JS_NKLoadPort = "AUMEL";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_RL_NKOriginInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
			Shipment.JS_RL_NKOrigin = "AUSYD";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_NKLoadPortInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUMEL", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForLoadOriginCountry()
		{
			Shipment.JS_NKLoadPort = "NZAKL";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_RL_NKOriginInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AU", "AUSYD", "MYPKG", ZString.Empty);
			Shipment.JS_RL_NKOrigin = "NZAKL";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_NKLoadPortInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AU", "AUMEL", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForDischargeDestination()
		{
			Shipment.JS_NKDischargePort = "";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_RL_NKDestinationInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
			Shipment.JS_NKDischargePort = "AUMEL";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_RL_NKDestinationInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
			Shipment.JS_RL_NKDestination = "AUSYD";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_NKDischargePortInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUMEL", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForDischargeDestinationCountry()
		{
			Shipment.JS_NKDischargePort = "NZAKL";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_RL_NKDestinationInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AU", "AUSYD", "MYPKG", ZString.Empty);
			Shipment.JS_RL_NKDestination = "NZAKL";
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_NKDischargePortInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AU", "AUMEL", "MYPKG", ZString.Empty);
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

		public void TestGetTemplateSelectionCriteriaDoesNotEnableHasChanges()
		{
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			var jobLoader = new JobHeader.Loader(shipment);
			var job = jobLoader.TryLoadOrCreate();
			Factory.Save();
			var consol = new BusinessObjectFactory().New<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			consol.Shipments.Add(shipment);
			consol.Factory.Save();
			var shipmentInDifferentFactory = new BusinessObjectFactory().Load<AgencyShipment>(shipment.PK);
			var x = shipmentInDifferentFactory.ShipmentJobHeader; // ShipmentJobHeader getter needs to be hit to set PlugInData on Job
			((IWorkflowProvider)shipmentInDifferentFactory).GetTemplateSelectionCriteria();
			AssertEquals("GetTemplateSelectionCriteria should not enable HasChanges", false, shipmentInDifferentFactory.HasChanges);
		}

		public void TestGetTemplateSelectionCriteria_ShouldNotSetJobDefaults()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			var jobLoader = new JobHeader.Loader(shipment);
			var job = jobLoader.TryLoadOrCreate();
			job.JH_GB = ZGuid.Empty;
			job.JH_GE = ZGuid.Empty;
			((IWorkflowProvider)shipment).GetTemplateSelectionCriteria();
			AssertEquals("We should not change the branch PK while GetTemplateSelectionCriteria", ZGuid.Empty, job.JH_GB);
			AssertEquals("We should not change the department PK while GetTemplateSelectionCriteria", ZGuid.Empty, job.JH_GE);
		}

		public void TestGetTemplateSelectionCriteria_LoadedJobHasParent()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			var jobLoader = new JobHeader.Loader(shipment);
			var job = jobLoader.TryLoadOrCreate();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<AgencyShipment>(shipment.PK);
			var jobInNewFactory = newFactory.Load<JobHeader>(job.PK);
			((IWorkflowProvider)shipmentInNewFactory).GetTemplateSelectionCriteria();
			AssertNotNull("We should load job with parent in GetTemplateSelectionCriteria", jobInNewFactory.Parent);
		}

		#endregion
		#region Implementation
		AgencyShipment Shipment
		{
			get
			{
				return BusinessObject;
			}
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
			get
			{
				return department ?? (department = Factory.NewWithValidTestData<GlbDepartment>());
			}
		}

		GlbDepartment department;
		protected override ShipmentT GetNewBusinessObject(BusinessObjectFactory factory)
		{
			ShipmentT result = base.GetNewBusinessObject(factory);
			JobHeader job = new JobHeader.Loader(result).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return result;
		}
		#endregion
	}
}
