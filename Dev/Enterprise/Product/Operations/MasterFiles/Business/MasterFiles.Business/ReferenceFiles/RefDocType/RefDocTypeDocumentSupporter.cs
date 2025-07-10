using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class RefDocTypeDocumentSupporter : DocumentSupporter
	{
		public RefDocTypeDocumentSupporter(RefDocType refDocType)
			: base(refDocType)
		{
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.RefDocType };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.RefDocType; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper result = null;

			if (dataContext == Constants.DataContext.RefDocType)
			{
				result = DocumentWrapperFactory.CreateWrapper(Constants.DataContext.RefDocType, BusinessObject);
			}

			return new DocumentWrapper[] { result };
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}
	}
}
