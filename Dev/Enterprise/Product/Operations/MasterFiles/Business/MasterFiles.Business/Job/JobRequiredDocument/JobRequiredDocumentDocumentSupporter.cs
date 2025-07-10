using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocumentDocumentSupporter : DocumentSupporter
	{
		public JobRequiredDocumentDocumentSupporter(IHaveRequiredDocuments parent) : base((BusinessObject)parent)
		{
			OrgHeader = parent as OrgHeader;
		}

		public JobRequiredDocumentDocumentSupporter(JobRequiredDocument jobRequiredDocument) : base(jobRequiredDocument)
		{
			RequiredDocument = jobRequiredDocument;
			OrgHeader = jobRequiredDocument.Parent as OrgHeader;
		}

		protected OrgHeader OrgHeader { get; }
		protected JobRequiredDocument RequiredDocument { get; }

		public override BusinessContext BusinessContext => BusinessContext.JobRequiredDocument;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.OrganisationCustomiseDocuments;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => null;
		protected override DataContext[] GetSupportedDataContexts() => System.Array.Empty<DataContext>();

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;
	}
}
