using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCFSDetailDocumentSupporter : DocumentSupporter
	{
		public GateTransportCFSDetailDocumentSupporter(GateTransportCFSDetail parent)
			: base(parent)
		{
		}

		GateTransportCFSDetail GateTransportCFSDetail
		{
			get { return (GateTransportCFSDetail)BusinessObject; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, GateTransportCFSDetail);
		}

		public override BusinessContext BusinessContext => BusinessContext.GateTransportCFSDet;

		protected override Constants.DataContext[] GetSupportedDataContexts() => new[] { Constants.DataContext.GenericFreightJob };

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.None;
	}
}
