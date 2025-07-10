using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCYDetailDocumentSupporter : DocumentSupporter
	{
		public GateTransportCYDetailDocumentSupporter(GateTransportCYDetail parent)
			: base(parent)
		{
		}

		GateTransportCYDetail GateTransportCYDetail
		{
			get { return (GateTransportCYDetail)BusinessObject; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, GateTransportCYDetail);
		}

		public override BusinessContext BusinessContext => BusinessContext.GateTransportCYDet;

		protected override Constants.DataContext[] GetSupportedDataContexts() => new[] { Constants.DataContext.GenericFreightJob };

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.None;
	}
}
