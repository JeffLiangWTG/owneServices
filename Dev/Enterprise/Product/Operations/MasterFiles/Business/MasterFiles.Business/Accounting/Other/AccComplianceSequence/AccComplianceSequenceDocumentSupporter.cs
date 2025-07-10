using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceDocumentSupporter : DocumentSupporter
	{
		public AccComplianceSequenceDocumentSupporter(AccComplianceSequence complianceSequence)
			: base(complianceSequence)
		{
		}

		protected AccComplianceSequence ComplianceSequence
		{
			get { return (AccComplianceSequence)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.ReceivablesCustomiseGovernmentInvoice; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ARInvoice; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Enterprise.Core.Constants.DataContext.ARInvoice };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		#endregion
	}
}
