using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	sealed class DummyWithWorkflowAndDataContext : DummyWithWorkflow
	{
		public DummyWithWorkflowAndDataContext(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			UseRealShipmentForInternalUniversalXMLSending = true;
		}
	}
}
