using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class PersonDocumentSupporter : DocumentSupporter
	{
		public PersonDocumentSupporter(BusinessObject parent)
			: base(parent)
		{
		}
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.GlbPerson; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.PersonIntelligenceCustomiseDocuments;

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
	}
}
