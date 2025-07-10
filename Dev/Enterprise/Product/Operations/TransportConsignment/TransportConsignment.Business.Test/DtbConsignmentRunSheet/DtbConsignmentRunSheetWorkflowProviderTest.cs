using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheet))]
	sealed class DtbConsignmentRunSheetWorkflowProviderTest : WorkflowProviderTest<DtbConsignmentRunSheet, DtbConsignmentRunSheetProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode; }
		}

		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria()
		{
			IWorkflowProvider runSheet = GetNewBusinessObject(Factory);
			AssertEquals(0, ((IColumnValueRankerInternals)runSheet.GetTemplateSelectionCriteria()).ColumnValues.Count());
		}

		#endregion

		#region TestGetWorkflowInformationProvider

		public void TestGetWorkflowInformationProvider()
		{
			IWorkflowProvider runSheet = GetNewBusinessObject(Factory);
			AssertNull(runSheet.GetWorkflowInformationProvider());
		}

		protected override DtbConsignmentRunSheet GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
		}

		#endregion
	}
}
