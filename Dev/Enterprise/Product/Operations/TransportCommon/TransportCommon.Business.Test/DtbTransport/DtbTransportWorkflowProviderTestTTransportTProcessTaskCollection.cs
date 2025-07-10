using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportWorkflowProviderTest<TTransport, TProcessTaskCollection> : WorkflowProviderTest<TTransport, TProcessTaskCollection>
			where TTransport : DtbTransport
			where TProcessTaskCollection : ProcessTaskCollection
	{
		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria()
		{
			TestGetTemplateSelectionCriteriaCore();
		}

		protected virtual void TestGetTemplateSelectionCriteriaCore()
		{
			IWorkflowProvider transport = GetNewBusinessObject(Factory);
			AssertEquals(0, ((IColumnValueRankerInternals)transport.GetTemplateSelectionCriteria()).ColumnValues.Count());
		}

		#endregion

		#region TestGetWorkflowInformationProvider

		public void TestGetWorkflowInformationProvider()
		{
			IWorkflowProvider transport = GetNewBusinessObject(Factory);
			AssertNull(transport.GetWorkflowInformationProvider());
		}

		#endregion
	}
}
