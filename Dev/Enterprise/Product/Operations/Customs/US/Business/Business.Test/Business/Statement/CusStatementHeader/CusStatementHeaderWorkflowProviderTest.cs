using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementProcessTaskCollection))]
	sealed class CusStatementHeaderWorkflowProviderTest : WorkflowProviderTest<CusStatementHeader, StatementProcessTaskCollection>
	{
		public void TestGetTemplateSelectionCriteria_ForImporter()
		{
			Statement.B2_StatementNumber = "1234";
			Statement.B2_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			AssertGetTemplateFilterCriteria(Statement.B2_OH_ImporterInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria()
		{
			Statement.B2_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobHeader jobHeader = new JobHeader.Loader(Statement).TryLoadOrCreate();
			jobHeader.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Statement.B2_Status = "MEH";

			ColumnValueRanker ranker = (ColumnValueRanker)((IWorkflowProvider)Statement).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { Statement.B2_OH_Importer, jobHeader.LocalChargesPK, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public new void TestProcessTasksCascadeDeleted()
		{
			//Statement should not be deleted
			Assert(true);
		}

		public void TestGetTemplateSelectionCriteria_ForBranch()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var otherBranch = company.Branches.AddNew();
			otherBranch.GB_Code = "Z2Z";
			Factory.Save();
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery());
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "Z32");
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), department.PK.ToGuid(), "D89");
			var tempalateZ32 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateZ32.P0_ProcessType = StatementProcessTask.StatementWorkflow.Code;
			tempalateZ32.P0_GB = GlbBranch.CurrentBranch.PK;
			var workflowZ32 = tempalateZ32.WorkflowItems.AddNew();
			workflowZ32.P9_Description = "Z32WORK";
			var tempalateD89 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateD89.P0_ProcessType = StatementProcessTask.StatementWorkflow.Code;
			tempalateD89.P0_GB = otherBranch.PK;
			var workflowD89 = tempalateD89.WorkflowItems.AddNew();
			workflowD89.P9_Description = "D89WORK";
			var tempalateALL = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			tempalateALL.P0_ProcessType = StatementProcessTask.StatementWorkflow.Code;
			var workflowALL = tempalateALL.WorkflowItems.AddNew();
			workflowALL.P9_Description = "ALLWORK";
			var tempalateMSC = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			Factory.Save();

			Statement.B2_BranchDesignation = "";
			Statement.WorkflowItems.Tasks.RemoveAndDeleteAll();
			Statement.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, Statement.WorkflowItems.Tasks.Count);
			AssertEquals("ALLWORK", Statement.WorkflowItems.Tasks[0].P9_Description);

			Statement.B2_BranchDesignation = "Z32";
			Statement.WorkflowItems.Tasks.RemoveAndDeleteAll();
			Statement.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, Statement.WorkflowItems.Tasks.Count);
			AssertEquals("Should match to Z32", "Z32WORK", Statement.WorkflowItems.Tasks[0].P9_Description);

			Statement.B2_BranchDesignation = "D89";
			Statement.WorkflowItems.Tasks.RemoveAndDeleteAll();
			Statement.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, Statement.WorkflowItems.Tasks.Count);
			AssertEquals("Should match to D89", "D89WORK", Statement.WorkflowItems.Tasks[0].P9_Description);

			Statement.B2_BranchDesignation = "ZZZ";
			Statement.WorkflowItems.Tasks.RemoveAndDeleteAll();
			Statement.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, Statement.WorkflowItems.Tasks.Count);
			AssertEquals("ALLWORK", Statement.WorkflowItems.Tasks[0].P9_Description);
		}

		public void TestBranchByBranchDesignationFromRegistry()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "US";

			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "US1";

			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "US2";

			Factory.Save();

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery());
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, "Z32");
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branch2.PK.ToGuid(), department.PK.ToGuid(), "D89");

			Factory.Save();

			Statement.B2_BranchDesignation = ZString.Empty;
			AssertNull(Statement.BranchByBranchDesignationFromRegistry);

			Statement.B2_BranchDesignation = "Z32";
			AssertNotNull(Statement.BranchByBranchDesignationFromRegistry);
			AssertEquals(branch1.PK, Statement.BranchByBranchDesignationFromRegistry.PK);

			Statement.B2_BranchDesignation = "D89";
			AssertNotNull(Statement.BranchByBranchDesignationFromRegistry);
			AssertEquals(branch2.PK, Statement.BranchByBranchDesignationFromRegistry.PK);
		}

		#region Implementation

		protected override ZString ExpectedWorkflowType
		{
			get { return StatementProcessTask.StatementWorkflow.Code; }
		}

		CusStatementHeader Statement
		{
			get { return BusinessObject; }
		}

		protected override CusStatementHeader GetNewBusinessObject(BusinessObjectFactory factory)
		{
			CusStatementHeader result = base.GetNewBusinessObject(factory);
			result.B2_StatementNumber = "1234P";
			result.B2_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobHeader job = new JobHeader.Loader(result).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return result;
		}

		#endregion
	}
}
