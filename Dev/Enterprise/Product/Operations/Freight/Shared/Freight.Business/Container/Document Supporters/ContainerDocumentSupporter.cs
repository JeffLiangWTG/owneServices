
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public abstract class ContainerDocumentSupporter : DocumentSupporter
	{
		public ContainerDocumentSupporter(CommonContainer container)
			: base(container)
		{
		}

		protected CommonContainer Container
		{
			get { return (CommonContainer)BusinessObject; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.CartageAdvice, Core.Constants.DataContext.Service, Core.Constants.DataContext.GenericFreightJob, Core.Constants.DataContext.GenericFreightJobServices };
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
