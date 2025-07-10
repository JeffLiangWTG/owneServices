using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class CYContainerLoadListDocumentSupporter : DocumentSupporter
	{
		public CYContainerLoadListDocumentSupporter(CYContainerLoadList header) : base(header)
		{
		}

		protected CYContainerLoadList header
		{
			get { return (CYContainerLoadList)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				return BusinessContext.ContainerLoadList;
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
				case Core.Constants.DataContext.ContainerLoadListHeader:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ContainerLoadListHeader, header) };
					break;
				default:
					break;
			}

			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Core.Constants.DataContext.ContainerLoadListHeader };
		}
	}
}
