using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentContainer))]
	internal class AgencyContainerWorkflowProviderTest : WorkflowProviderTest<AgencyShipmentContainer, AgencyContainerProcessTaskCollection>
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

		public void TestGetTemplateSelectionCriteria_ForLoadPort()
		{
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_NKLoadPortInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUMEL", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForLoadCountry()
		{
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_NKLoadPortInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AU", "AUMEL", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForDischargePort()
		{
			AssertGetTemplateFilterCriteria<ZString>(Shipment.JS_NKDischargePortInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUMEL", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForDischargeCountry()
		{
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

		public void TestGetTemplateSelectionCriteria_ShouldNotSetJobDefaults()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.JS_RL_NKOrigin = "INBOM";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Job.JH_GB = ZGuid.Empty;
			Job.JH_GE = ZGuid.Empty;
			((IWorkflowProvider)Shipment).GetTemplateSelectionCriteria();
			AssertEquals("We should not change the branch PK while GetTemplateSelectionCriteria", ZGuid.Empty, Job.JH_GB);
			AssertEquals("We should not change the department PK while GetTemplateSelectionCriteria", ZGuid.Empty, Job.JH_GE);
		}

		public void TestGetTemplateSelectionCriteria_LoadedJobHasParent()
		{
			Job.Factory.Save();
			Shipment.Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<AgencyShipment>(Shipment.PK);
			var jobInNewFactory = newFactory.Load<JobHeader>(Job.PK);
			((IWorkflowProvider)shipmentInNewFactory).GetTemplateSelectionCriteria();
			AssertNotNull("We should load job with parent in GetTemplateSelectionCriteria", jobInNewFactory.Parent);
		}

		public void TestBookedContainersOnBOLAreNotMatched()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CNS";
			BillOfLading bol = Factory.New<BillOfLading>();
			AgencyShipmentContainer bolBookedContainer = bol.BookedContainers.AddNew();
			AgencyShipmentContainer bolRealContainer = bol.RealContainers.AddNew();
			AgencyBooking booking = Factory.New<AgencyBooking>();
			AgencyShipmentContainer bookingBookedContainer = booking.BookedContainers.AddNew();
			AgencyShipmentContainer bookingRealContainer = booking.RealContainers.AddNew();
			Factory.Save();
			ColumnValueRanker ranker = (ColumnValueRanker)((IWorkflowProvider)bolBookedContainer).GetTemplateSelectionCriteria();
			AssertEquals("No match for BOL Booked container", null, ranker.GetBestMatch<ProcessTaskTemplate>(Factory, new ZQuery()));
			ranker = (ColumnValueRanker)((IWorkflowProvider)bolRealContainer).GetTemplateSelectionCriteria();
			AssertEquals("Template matched for BOL Real Container", template, ranker.GetBestMatch<ProcessTaskTemplate>(Factory, new ZQuery()));
			ranker = (ColumnValueRanker)((IWorkflowProvider)bookingBookedContainer).GetTemplateSelectionCriteria();
			AssertEquals("Template matched for Booking Booked Container", template, ranker.GetBestMatch<ProcessTaskTemplate>(Factory, new ZQuery()));
			ranker = (ColumnValueRanker)((IWorkflowProvider)bookingRealContainer).GetTemplateSelectionCriteria();
			AssertEquals("Template matched for Booking Real Container", template, ranker.GetBestMatch<ProcessTaskTemplate>(Factory, new ZQuery()));
		}

		#endregion
		#region Implementation
		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return WorkflowDescriptors.AgencyContainerWorkflowDescriptorCode;
			}
		}

		AgencyShipmentContainer Container
		{
			get
			{
				return BusinessObject;
			}
		}

		AgencyShipment Shipment
		{
			get
			{
				return Container.Booking;
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
		protected override AgencyShipmentContainer GetNewBusinessObject(BusinessObjectFactory factory)
		{
			AgencyShipmentContainer result = base.GetNewBusinessObject(factory);
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			result.JC_JS_FCLBookingOnlyLink = shipment.PK;
			JobHeader job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return result;
		}
		#endregion
	}
}
