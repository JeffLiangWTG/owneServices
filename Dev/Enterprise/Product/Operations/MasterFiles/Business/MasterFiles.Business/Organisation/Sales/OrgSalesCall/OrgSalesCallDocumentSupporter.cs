using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallDocumentSupporter : DocumentSupporter
	{
		public OrgSalesCallDocumentSupporter(BusinessObject parent)
			: base(parent)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Communication; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CommunicationManagerCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, base.BusinessObject);
			return genericWrappers;
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
				{
					Core.Constants.DataContext.BusinessObject,
					Core.Constants.DataContext.GenericFreightJob
				};
		}

		public new OrgSalesCall BusinessObject
		{
			get { return (OrgSalesCall)base.BusinessObject; }
		}
	}
}

