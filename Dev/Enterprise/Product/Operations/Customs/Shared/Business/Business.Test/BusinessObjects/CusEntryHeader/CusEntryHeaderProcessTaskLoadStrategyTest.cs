using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(CusEntryHeaderProcessTaskLoadStrategy))]
sealed class CusEntryHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
{
	public void TestGetTypeForLoad()
	{
		var cusEntryHeader = Factory.New<Integration.Customs.ICusEntryHeader>();
		AssertEquals(typeof(CusEntryHeaderProcessTask<>).MakeGenericType(cusEntryHeader.GetType()), LoadStrategy.GetTypeForLoad(CusEntryHeaderSchema.Constants.Prefix, cusEntryHeader.PK, Factory));
	}

	public void TestAddAdditionalParentFilters() => CombineAssertions(() =>
	{
		SetupDeclarationAndProcessTaskDataForTest();
		var tasksForDefaultCompanyWithEmmaWorkflowDescriptor = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode);
		AssertEquals("Should return no process tasks for EMMA workflow when data is not created in Norway", 0, tasksForDefaultCompanyWithEmmaWorkflowDescriptor.Length);

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Norway))
		{
			SetupDeclarationAndProcessTaskDataForTest();

			var tasksForEmmaDescriptor = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode);
			AssertEquals("Should contain single task for for EMM Workflow Descriptor in Norway", 1, tasksForEmmaDescriptor.Length);
		}

		void SetupDeclarationAndProcessTaskDataForTest()
		{
			MasterFilesTestHelper.ClearWorkflowTables();
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_ParentID = entryHeader.PK;
			processTask.P9_ParentTableCode = entryHeader.TablePrefix;
			Factory.Save();
		}
	});

	ProcessTask[] GetProcessTasksForTestAddAdditionalParentFilters(string code)
	{
		var query = new ZDBOnlyQuery(typeof(ProcessTask));
		var subQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), ProcessTasksSchema.P9_ParentID);
		var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(code);
		LoadStrategy.AddAdditionalParentFilters(descriptor, subQuery);
		query.AddSubQuery(subQuery, JoinCondition.And);
		return Factory.Load<ProcessTask>(query);
	}

	IProcessTaskLoadStrategy LoadStrategy => loadStrategy ??= new();
	CusEntryHeaderProcessTaskLoadStrategy loadStrategy;
}
