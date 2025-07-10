using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRHiringRequest))]
	sealed class HRHiringRequestTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocManagerSupportImplementation()
		{
			var hiringRequest = Factory.New<HRHiringRequest>();
			AssertNotNull(hiringRequest.DocManagerInfo);
			AssertEquals(hiringRequest.DocManagerInfo.DocManagerCode, Core.Constants.DocManagerCodes.HiringRequest);
		}

		public void TestWorkflowProviderImplementation()
		{
			var hiringRequest = Factory.New<HRHiringRequest>();
			AssertNotNull(hiringRequest.WorkflowItems);
			AssertEquals(hiringRequest.WorkflowType, WorkflowDescriptors.HRHiringRequestDescriptorCode);
			AssertNull(hiringRequest.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var branchPK = Guid.NewGuid();
			var departmentPK = Guid.NewGuid();

			var hiringRequest = Factory.New<HRHiringRequest>();
			hiringRequest.HRR_GST_NKTeam = "AAA";
			var columnValues = ((IColumnValueRankerInternals)hiringRequest.GetTemplateSelectionCriteria()).ColumnValues.ToArray();
			AssertEquals(columnValues.Length, 1);
			AssertEquals(columnValues[0].ColumnName, ProcessTaskTemplateSchema.Constants.P0_SubType1);
			AssertEquals(columnValues[0].Values.Length, 2);
			AssertEquals(columnValues[0].Values[0], "AAA");
			AssertEquals(columnValues[0].Values[1], ZString.Empty);
		}

		public void TestJobApplicant()
		{
			HRHiringRequest hiringRequest = Factory.New<HRHiringRequest>();
			AssertNull("JobApplicant should not be set", hiringRequest.JobApplicant);

			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			hiringRequest.HRR_HA_JobApplicant = applicant.PK;

			AssertEquals("JobApplicant should be set", applicant, hiringRequest.JobApplicant);
		}

		public void TestGetCustomBusinessObject()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.HRHiringRequestDescriptorCode;

			var customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom1";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom2";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			Factory.Save();

			var hiringRequest = Factory.New<HRHiringRequest>();

			IDynamicBusinessObject dynamicBusinessObject = ((ICustomFieldProvider)hiringRequest).GetCustomBusinessObject();

			AssertContainsExactElementsInAnyOrder("expected workflow property aliases + infos",
					new[] { "__CUSTOM1__prop__ZString", "__CUSTOM1__prop__ZStringInfo", "__CUSTOM2__prop__ZInt", "__CUSTOM2__prop__ZIntInfo" }, dynamicBusinessObject.PropertyNames);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<HRHiringRequest>();
		}
	}
}
