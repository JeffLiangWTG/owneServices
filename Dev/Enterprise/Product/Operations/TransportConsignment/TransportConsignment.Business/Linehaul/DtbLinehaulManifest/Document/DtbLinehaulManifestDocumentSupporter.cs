using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestDocumentSupporter : DocumentSupporter
	{
		public DtbLinehaulManifestDocumentSupporter(DtbLinehaulManifest manifest)
			: base(manifest)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbLinehaulManifest; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
