using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclaration))]
	sealed class BaseJobDeclarationWorkflowProviderTest : WorkflowProviderTest<BaseJobDeclaration, JobDeclarationProcessTaskCollection<BaseJobDeclaration>>
	{
		public void TestGetTemplateSelectionCriteria_ForImporter()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.Job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertEquals(true, Declaration.IsImport);
			Factory.Save();
			AssertGetTemplateFilterCriteria(Declaration.JE_OH_ImporterInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForSupplier()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.Job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertEquals(true, Declaration.IsExport);
			Factory.Save();
			AssertGetTemplateFilterCriteria(Declaration.JE_OH_SupplierInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForTransportMode()
		{
			Declaration.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_TransportModeInfo, ProcessTaskTemplate.P0_SubType1Info, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForJobType()
		{
			Declaration.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_MessageTypeInfo, ProcessTaskTemplate.P0_SubType2Info, ImportExportCodeList.Codes.Import, ImportExportCodeList.Codes.Export, ZString.Empty);
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_MessageTypeInfo, ProcessTaskTemplate.P0_SubType2Info, JobMessageTypeList.Codes.MiscellaneousCustoms, "", ZString.Empty);
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_MessageTypeInfo, ProcessTaskTemplate.P0_SubType2Info, JobMessageTypeList.Codes.Refund, "", ZString.Empty);
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_MessageTypeInfo, ProcessTaskTemplate.P0_SubType2Info, JobMessageTypeList.Codes.ExWarehouse, "", ZString.Empty);
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_MessageTypeInfo, ProcessTaskTemplate.P0_SubType2Info, JobMessageTypeList.Codes.Drawback, "", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForJobTypeFallBack()
		{
			var decMock = Factory.NewMoq<BaseJobDeclaration>();
			decMock.Setup(m => m.IsImport).Returns(true);
			decMock.Setup(m => m.IsExWarehouse).Returns(true);
			var dec = decMock.Object;
			var tempalateEXP = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateEXP.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateEXP.P0_SubType2 = ImportExportCodeList.Codes.Export;
			var workflowEXP = tempalateEXP.WorkflowItems.AddNew();
			workflowEXP.P9_Description = "EXPWORK";
			var tempalateIMP = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateIMP.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateIMP.P0_SubType2 = ImportExportCodeList.Codes.Import;
			var workflowIMP = tempalateIMP.WorkflowItems.AddNew();
			workflowIMP.P9_Description = "IMPWORK";
			var tempalateEXW = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateEXW.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateEXW.P0_SubType2 = JobMessageTypeList.Codes.ExWarehouse;
			var workflowEXW = tempalateEXW.WorkflowItems.AddNew();
			workflowEXW.P9_Description = "EXWWORK";
			var tempalateMSC = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateMSC.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateMSC.P0_SubType2 = JobMessageTypeList.Codes.MiscellaneousCustoms;
			var workflowMSC = tempalateMSC.WorkflowItems.AddNew();
			workflowMSC.P9_Description = "MSCWORK";
			var tempalateOTH = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateOTH.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateOTH.P0_SubType2 = "OTH";
			var workflowOTH = tempalateOTH.WorkflowItems.AddNew();
			workflowOTH.P9_Description = "OTHWORK";
			Factory.Save();

			dec.JE_MessageType = "OTH";
			dec.WorkflowItems.Tasks.RemoveAndDeleteAll();
			dec.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, dec.WorkflowItems.Tasks.Count);
			AssertEquals("OTHWORK", dec.WorkflowItems.Tasks[0].P9_Description);

			dec.JE_MessageType = "ZZZ";
			dec.WorkflowItems.Tasks.RemoveAndDeleteAll();
			dec.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, dec.WorkflowItems.Tasks.Count);
			AssertEquals("Should match to EXW as IsExWarehouse", "EXWWORK", dec.WorkflowItems.Tasks[0].P9_Description);

			dec.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			dec.WorkflowItems.Tasks.RemoveAndDeleteAll();
			dec.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, dec.WorkflowItems.Tasks.Count);
			AssertEquals("Should match to MSC as IsMiscellaneous", "MSCWORK", dec.WorkflowItems.Tasks[0].P9_Description);

			decMock.Setup(m => m.IsExWarehouse).Returns(false);
			dec.JE_MessageType = "ZZZ";
			dec.WorkflowItems.Tasks.RemoveAndDeleteAll();
			dec.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, dec.WorkflowItems.Tasks.Count);
			AssertEquals("Should match to IMP", "IMPWORK", dec.WorkflowItems.Tasks[0].P9_Description);
		}

		public void TestGetTemplateSelectionCriteria_ForContainerMode()
		{
			Declaration.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_ContainerModeInfo, ProcessTaskTemplate.P0_SubType3Info, Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.LCL, ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForLoadPort()
		{
			Declaration.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_RL_NKPortOfLoadingInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForFinalDischargePort()
		{
			Declaration.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Declaration.JE_RL_NKPortOfArrivalInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForBranch()
		{
			Job.Factory.Save();
			Declaration.Factory.Save();
			AssertGetTemplateFilterCriteria(Job.JH_GBInfo, ProcessTaskTemplate.P0_GBInfo, Declaration.JE_GB, Branch.PK, ZGuid.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForDepartment()
		{
			Job.Factory.Save();
			Declaration.Factory.Save();
			AssertGetTemplateFilterCriteria(Job.JH_GEInfo, ProcessTaskTemplate.P0_GEInfo, GlbDepartment.CurrentDepartment.PK, Department.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ShouldNotSetJobDefaults()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_RL_NKOrigin = "INBOM";
			Declaration.JE_RL_NKFinalDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			Job.JH_GB = ZGuid.Empty;
			Job.JH_GE = ZGuid.Empty;
			((IWorkflowProvider)Declaration).GetTemplateSelectionCriteria();
			AssertEquals("We should not change the branch PK while GetTemplateSelectionCriteria", ZGuid.Empty, Job.JH_GB);
			AssertEquals("We should not change the department PK while GetTemplateSelectionCriteria", ZGuid.Empty, Job.JH_GE);
		}

		public void TestGetTemplateSelectionCriteria_LoadedJobHasParent()
		{
			Declaration.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<BaseJobDeclaration>(Declaration.PK);
			var jobInNewFactory = newFactory.Load<JobHeader>(Job.PK);

			((IWorkflowProvider)shipmentInNewFactory).GetTemplateSelectionCriteria();
			AssertNotNull("We should load job with parent in GetTemplateSelectionCriteria", jobInNewFactory.Parent);
		}

		public void TestDoNotCreateTasksAndMilestonesForOldJobs()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Factory.Save();

			var tempalateIMP = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateIMP.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateIMP.P0_SubType2 = ImportExportCodeList.Codes.Import;

			var workflowIMP = tempalateIMP.WorkflowItems.AddNew();
			workflowIMP.P9_Description = "IMPWORK";

			var tempalateMSC = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateMSC.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateMSC.P0_SubType2 = JobMessageTypeList.Codes.MiscellaneousCustoms;
			var workflowMSC = tempalateMSC.WorkflowItems.AddNew();
			workflowMSC.P9_Description = "MSCWORK";

			var tempalateOTH = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateOTH.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			tempalateOTH.P0_SubType2 = "OTH";
			var workflowOTH = tempalateOTH.WorkflowItems.AddNew();
			workflowOTH.P9_Description = "OTHWORK";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration1.PK);
			loadedDeclaration.SuspendAddingWorkflow = true;
			loadedDeclaration.JE_TransportMode = loadedDeclaration.TransportModeSeaCodeForTesting;
			newFactory.Save();
			AssertEquals(0, loadedDeclaration.WorkflowItems.Tasks.Count);
			AssertEquals(0, loadedDeclaration.WorkflowItems.Milestones.Count);

			loadedDeclaration.SuspendAddingWorkflow = false;
			loadedDeclaration.JE_TransportMode = loadedDeclaration.TransportModeAirCodeForTesting;
			newFactory.Save();
			AssertEquals(1, loadedDeclaration.WorkflowItems.Tasks.Count);
			AssertEquals("Should match to IMP", "IMPWORK", loadedDeclaration.WorkflowItems.Tasks[0].P9_Description);
			AssertEquals(0, loadedDeclaration.WorkflowItems.Milestones.Count);
		}

		protected override ZString ExpectedWorkflowType => JobInvoicingConsumerTypes.Brokerage.Code;

		protected override BaseJobDeclaration GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObject(factory);
			var job = new JobHeader.Loader(result).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return result;
		}

		protected override void SetupBusinessObjectForDeletion()
		{
			Declaration.Job?.Delete();
		}

		BaseJobDeclaration Declaration
		{
			get { return BusinessObject; }
		}

		JobHeader Job
		{
			get
			{
				if (job == null)
				{
					job = new JobHeader.Loader(Declaration).TryLoadOrCreate();
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
	}
}
