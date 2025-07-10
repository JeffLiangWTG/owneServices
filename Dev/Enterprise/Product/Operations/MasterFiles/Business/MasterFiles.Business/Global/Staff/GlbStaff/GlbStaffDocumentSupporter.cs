using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffDocumentSupporter : DocumentSupporter
	{
		public GlbStaffDocumentSupporter(GlbStaff staff)
			: base(staff)
		{
		}

		protected GlbStaff Staff
		{
			get { return (GlbStaff)BusinessObject; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.StaffCustomiseDocuments; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.GlbStaff; }
		}

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.GlbStaff };
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}
	}
}
