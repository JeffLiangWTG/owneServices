using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationAttemptDocumentSupporter : DocumentSupporter
	{
		public GlbAccreditationAttemptDocumentSupporter(BusinessObject parent)
			: base(parent)
		{
		}

		public override BusinessContext BusinessContext => BusinessContext.AccreditationAttempt;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.PersonIntelligenceCustomiseDocuments;

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Constants.DataContext.BusinessObject,
				Constants.DataContext.GenericFreightJob
			};
		}
	}
}
