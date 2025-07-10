using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HROnBoarding))]
	class HROnBoardingCustomFieldsTest : TestICustomFieldProvider
	{
	}

	[TestedType(typeof(HROnBoarding))]
	class HROnBoardingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocManagerSupportImplementation()
		{
			var onBoarding = Factory.New<HROnBoarding>();
			AssertNotNull(onBoarding.DocManagerInfo);
			AssertEquals(onBoarding.DocManagerInfo.DocManagerCode, Core.Constants.DocManagerCodes.OnBoarding);
		}

		public void TestWorkflowProviderImplementation()
		{
			var onBoarding = Factory.New<HROnBoarding>();
			AssertNotNull(onBoarding.WorkflowItems);
			AssertEquals(onBoarding.WorkflowType, WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode);
			AssertNull(onBoarding.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var branchPK = Guid.NewGuid();
			var departmentPK = Guid.NewGuid();

			var onBoarding = Factory.New<HROnBoarding>();
			onBoarding.HOB_GST_NKTeam = "AAA";
			onBoarding.HOB_ContractStatus = "ABC";
			onBoarding.HOB_GB_HomeBranch = branchPK;
			onBoarding.HOB_GE_HomeDepartment = departmentPK;
			var columnValues = ((IColumnValueRankerInternals)onBoarding.GetTemplateSelectionCriteria()).ColumnValues.ToArray();
			AssertEquals(columnValues.Length, 4);
			AssertEquals(columnValues[0].ColumnName, ProcessTaskTemplateSchema.Constants.P0_SubType1);
			AssertEquals(columnValues[0].Values.Length, 2);
			AssertEquals(columnValues[0].Values[0], "AAA");
			AssertEquals(columnValues[0].Values[1], ZString.Empty);

			AssertEquals(columnValues[1].ColumnName, ProcessTaskTemplateSchema.Constants.P0_SubType2);
			AssertEquals(columnValues[1].Values.Length, 2);
			AssertEquals(columnValues[1].Values[0], "ABC");
			AssertEquals(columnValues[1].Values[1], ZString.Empty);

			AssertEquals(columnValues[2].ColumnName, ProcessTaskTemplateSchema.Constants.P0_GB);
			AssertEquals(columnValues[2].Values[0], branchPK);
			AssertEquals(columnValues[2].Values[1], ZGuid.Empty);

			AssertEquals(columnValues[3].ColumnName, ProcessTaskTemplateSchema.Constants.P0_GE);
			AssertEquals(columnValues[3].Values[0], departmentPK);
			AssertEquals(columnValues[3].Values[1], ZGuid.Empty);
		}

		public void TestJobApplicant()
		{
			var onBoarding = Factory.New<HROnBoarding>();
			AssertNull("JobApplicant should not be set", onBoarding.JobApplicant);

			var applicant = Factory.New<HRJobApplicant>();
			onBoarding.HOB_HA_JobApplicant = applicant.PK;

			AssertEquals("JobApplicant should be set", applicant, onBoarding.JobApplicant);
		}

		public void TestGetCustomBusinessObject()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode;

			var customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom1";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom2";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			Factory.Save();

			var onBoarding = Factory.New<HROnBoarding>();

			IDynamicBusinessObject dynamicBusinessObject = ((ICustomFieldProvider)onBoarding).GetCustomBusinessObject();

			AssertContainsExactElementsInAnyOrder("expected workflow property aliases + infos",
					new[] { "__CUSTOM1__prop__ZString", "__CUSTOM1__prop__ZStringInfo", "__CUSTOM2__prop__ZInt", "__CUSTOM2__prop__ZIntInfo" }, dynamicBusinessObject.PropertyNames);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<HROnBoarding>();
	}

	[TestedType(typeof(HROnBoarding))]
	public class HROnBoardingWorkflowProviderTest : WorkflowProviderTest<HROnBoarding, HROnBoardingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode;
	}
}
