using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetInstruction))]
	sealed class DtbConsignmentRunSheetInstructionWorkflowProviderTest : WorkflowProviderTest<DtbConsignmentRunSheetInstruction, DtbConsignmentRunSheetInstructionProcessTaskCollection>
	{
		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria()
		{
			IWorkflowProvider runSheetInstruction = GetNewBusinessObject(Factory);
			AssertEquals(0, ((IColumnValueRankerInternals)runSheetInstruction.GetTemplateSelectionCriteria()).ColumnValues.Count());
		}

		#endregion

		#region TestGetWorkflowInformationProvider

		public void TestGetWorkflowInformationProvider()
		{
			IWorkflowProvider runSheet = GetNewBusinessObject(Factory);
			AssertNull(runSheet.GetWorkflowInformationProvider());
		}

		protected override DtbConsignmentRunSheetInstruction GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			var instruction = runSheet.RunSheetInstructions.AddNew();
			return instruction;
		}

		#endregion

		#region TestProcessTasksCreatedOnSave

		public override void TestProcessTasksCreatedOnSave()
		{
			Assert("RunSheet Instructions doesn't support workflow templates", true);
		}

		#endregion

		#region ExpectedWorkflowType

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.DtbConsignmentRunSheetInstructionWorkflowDescriptorCode; }
		}

		#endregion
	}
}
