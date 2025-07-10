using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefComplianceList))]
	sealed class RefComplianceListTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetReadOnlySecurityForRefComplianceListEdit()
		{
			var refComplianceList = Factory.NewWithValidTestData<RefComplianceListForTest>();
			var configPropertyIsExcluded = new PropertyDescriptorForTest(RefComplianceListSchema.RCL_IsExcluded.Name, Array.Empty<Attribute>());

			Env.Security.RefComplianceListEdit.IsAllowed = false;
			var shouldConfigPropertyIsExcludedBeReadOnly = refComplianceList.GetReadOnlySecurityForTest(configPropertyIsExcluded);
			AssertEquals("Property should be read only as RefComplianceListEdit security is not allowed", true, shouldConfigPropertyIsExcludedBeReadOnly);

			Env.Security.RefComplianceListEdit.IsAllowed = true;
			shouldConfigPropertyIsExcludedBeReadOnly = refComplianceList.GetReadOnlySecurityForTest(configPropertyIsExcluded);
			AssertEquals("Property should be editable as RefComplianceListEdit security is allowed", false, shouldConfigPropertyIsExcludedBeReadOnly);
		}

		public void TestPreventCancel()
		{
			var refComplianceList = Factory.NewWithValidTestData<RefComplianceListForTest>();
			var canCancel = refComplianceList.CanCancel();
			AssertEquals("Should not be able to cancel", false, string.IsNullOrEmpty(canCancel));
		}

		public void TestPreventReactivate()
		{
			var refComplianceList = Factory.NewWithValidTestData<RefComplianceListForTest>();
			var canReactivate = refComplianceList.CanReactivate();
			AssertEquals("Should not be able to reactivate", false, string.IsNullOrEmpty(canReactivate));
		}

		public void TestDeleteWorkFlowItems()
		{
			var header = Factory.New<RefComplianceList>();
			header.WorkflowItems.AddNew();

			AssertEquals("Precondition: ", 1, header.WorkflowItems.Count);

			header.Delete();
			AssertEquals(false, header.WorkflowItems.Any());
		}

		public void TestIWorkflowProviderMembers()
		{
			var header = Factory.New<RefComplianceList>();
			var newWorkflowItem = header.WorkflowItems.AddNew();
			var workflowProvider = (IWorkflowProvider)header;

			AssertEquals(WorkflowDescriptors.RefComplianceListWorkflowDescriptorCode, workflowProvider.WorkflowType);
			AssertContainsExactElementsInAnyOrder(new[] { newWorkflowItem }, workflowProvider.WorkflowItems);
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestCodeAndDescriptionPropertyAttribute()
		{
			AssertEquals("DescriptionProperty", AutoRefComplianceList.Schema.RCL_ListName, ((DescriptionPropertyAttribute)typeof(RefComplianceList).GetCustomAttributes(typeof(DescriptionPropertyAttribute), false)[0]).PropertyName);
			AssertEquals("CodeProperty", AutoRefComplianceList.Schema.RCL_ListCode, ((CodePropertyAttribute)typeof(RefComplianceList).GetCustomAttributes(typeof(CodePropertyAttribute), false)[0]).PropertyName);
		}

		public void TestNoAuditLog()
		{
			var refComplianceList = Factory.NewWithValidTestData<RefComplianceList>();
			Factory.Save();

			refComplianceList.RCL_ListDescription = "Test Update";
			Factory.Save();

			CombineAssertions("No audit log for Ref Compliance List", () =>
			{
				Assert("No ADD log when created", !refComplianceList.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode));
				Assert("No EDT log when edited", !refComplianceList.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode));
			});
		}
	}
}
