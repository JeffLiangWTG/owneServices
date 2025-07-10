using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class CFSContainerLoadListDocumentSupporter : DocumentSupporter
	{
		public CFSContainerLoadListDocumentSupporter(CFSContainerLoadList header) : base(header)
		{
		}

		protected CFSContainerLoadList header
		{
			get { return (CFSContainerLoadList)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				return BusinessContext.ContainerLoadPlan;
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.None;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, header);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper[] result = System.Array.Empty<DocumentWrapper>();

			switch (dataContext)
			{
				case Core.Constants.DataContext.ContainerLoadPlanHeader:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ContainerLoadPlanHeader, header) };
					break;
				default:
					break;
			}

			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Core.Constants.DataContext.ContainerLoadPlanHeader };
		}
	}
}
