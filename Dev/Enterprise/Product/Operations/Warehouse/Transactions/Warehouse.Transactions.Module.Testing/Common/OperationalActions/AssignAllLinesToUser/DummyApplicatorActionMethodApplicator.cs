using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class DummyApplicatorActionMethodApplicator : AssignAllLinesOperationalActionMethodApplicator<DummyBusinessObject>
	{
		public DummyApplicatorActionMethodApplicator(BusinessObjectFactory factory) : base("", factory) { }

		protected override string MessageHeader
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override void ApplyCore(Services.OperationalActions.Support.IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			throw new System.NotImplementedException();
		}

		protected override string GetErrorMessage(DummyBusinessObject target)
		{
			throw new System.NotImplementedException();
		}

		protected override ControllerID ControllerID
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override string GetJobNo(DummyBusinessObject target)
		{
			throw new System.NotImplementedException();
		}
	}
}
