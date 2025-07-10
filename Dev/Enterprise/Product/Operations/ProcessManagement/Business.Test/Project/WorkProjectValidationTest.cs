using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class WorkProjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestProjectType()
		{
			Project project = Factory.NewWithValidTestData<Project>();
			project.WKP_Type = "XXX";
			AssertHasErrors(project.WKP_TypeInfo);

			project.WKP_Type = project.Lookups.ActiveTypes[0].Code;
			AssertNoErrors(project.WKP_TypeInfo);

			project.WKP_Type = "";
			AssertNoErrors(project.WKP_TypeInfo);

			project.WKP_Type = "1ZZ";
			AssertHasErrors(project.WKP_TypeInfo);

			Factory.Save();
			project.Validation.ValidateWKP_Type();
			AssertNoErrors("inactive codes don't give error", project.WKP_TypeInfo);
			AssertHasWarnings("inactive codes give warning", project.WKP_TypeInfo);
		}

		public void TestProjectSubtype()
		{
			Project project = Factory.NewWithValidTestData<Project>();
			project.WKP_Type = project.Lookups.ActiveTypes[0].Code;

			project.WKP_SubType = "XXX";
			AssertHasErrors(project.WKP_SubTypeInfo);

			project.WKP_SubType = project.Lookups.ActiveSubtypes[0].Code;
			AssertNoErrors(project.WKP_SubTypeInfo);

			project.WKP_SubType = "";
			AssertNoErrors(project.WKP_SubTypeInfo);
		}

		public void TestProjectModule()
		{
			Project project = Factory.NewWithValidTestData<Project>();
			project.WKP_Type = project.Lookups.ActiveTypes[0].Code;
			project.WKP_SubType = project.Lookups.ActiveModules[0].Code;

			project.WKP_Module = "XXX";
			AssertHasErrors(project.WKP_ModuleInfo);

			project.WKP_Module = project.Lookups.ActiveModules[0].Code;
			AssertNoErrors(project.WKP_ModuleInfo);

			project.WKP_Module = "";
			AssertNoErrors(project.WKP_ModuleInfo);
		}

		public void TestPriority()
		{
			Project project = Factory.New<Project>();
			project.WKP_Priority = "ANY";

			AssertNoErrors(project.WKP_PriorityInfo);

			project.WKP_Priority = "XXX";
			AssertHasErrors(project.WKP_PriorityInfo);

			project.WKP_Priority = "OLD";
			AssertHasErrors(project.WKP_PriorityInfo);

			project.WKP_Priority = "";
			AssertNoErrors(project.WKP_PriorityInfo);
		}

		public void TestSummary()
		{
			Project project = Factory.New<Project>();
			project.Validation.ValidateWKP_Summary();
			AssertHasErrors(project.WKP_SummaryInfo);

			project.WKP_Summary = "not blank";
			AssertNoNotifications(project.WKP_SummaryInfo);
		}

		public void TestStatus()
		{
			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes element = collection.AddNew();
			element.Code = "WKP";
			WorkflowTaskType taskType = element.TaskTypes.AddNew();
			taskType.Code = "WRK";
			taskType.Description = (NoResString)"Stuff";
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Project project = Factory.New<Project>();
			project.WKP_Status = "WRK";
			AssertNoNotifications(project.WKP_StatusInfo);

			project.WKP_Status = "XXX";
			AssertHasErrors(project.WKP_StatusInfo);

			project.WKP_Status = "";
			AssertNoNotifications(project.WKP_StatusInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
		}

		public void TestProjectTypeIsMandatory()
		{
			ProcessManagementRegistry.Instance.ProjectTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Project project = Factory.NewWithValidTestData<Project>();
			project.WKP_Type = "";
			project.RunPreSaveValidation();
			AssertNoErrors(project.WKP_TypeInfo);

			ProcessManagementRegistry.Instance.ProjectTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			project.RunPreSaveValidation();
			AssertHasError(project.WKP_TypeInfo, "Please enter a value.");
		}

		public void TestProjectSubtypeIsMandatory()
		{
			ProcessManagementRegistry.Instance.ProjectSubTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Project project = Factory.NewWithValidTestData<Project>();
			project.WKP_SubType = "";
			project.RunPreSaveValidation();
			AssertNoErrors(project.WKP_SubTypeInfo);

			ProcessManagementRegistry.Instance.ProjectSubTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			project.RunPreSaveValidation();
			AssertHasError(project.WKP_SubTypeInfo, "Please enter a value.");
		}

		public void TestProjectModuleIsMandatory()
		{
			ProcessManagementRegistry.Instance.ProjectModuleMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Project project = Factory.NewWithValidTestData<Project>();
			project.WKP_Module = "";
			project.RunPreSaveValidation();
			AssertNoErrors(project.WKP_ModuleInfo);

			ProcessManagementRegistry.Instance.ProjectModuleMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			project.RunPreSaveValidation();
			AssertHasError(project.WKP_ModuleInfo, "Please enter a value.");
		}

		public void TestPriorityIsMandatory()
		{
			ProcessManagementRegistry.Instance.ProjectPriorityMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Project project = Factory.NewWithValidTestData<Project>();
			project.WKP_Priority = "";
			project.RunPreSaveValidation();
			AssertNoErrors(project.WKP_PriorityInfo);

			ProcessManagementRegistry.Instance.ProjectPriorityMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			project.RunPreSaveValidation();
			AssertHasError(project.WKP_PriorityInfo, "Please enter a Priority.");
		}
	}
}
