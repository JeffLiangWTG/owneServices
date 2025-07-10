using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DummyDocumentSupporter : DocumentSupporter
	{
		public DummyDocumentSupporter(BusinessObject parent)
			: base(parent)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { throw new System.NotImplementedException(); }
		}

		public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, MasterFiles.Integration.IStmMenuItem commandBeingRun)
		{
			throw new System.NotImplementedException();
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			throw new System.NotImplementedException();
		}
	}
}
