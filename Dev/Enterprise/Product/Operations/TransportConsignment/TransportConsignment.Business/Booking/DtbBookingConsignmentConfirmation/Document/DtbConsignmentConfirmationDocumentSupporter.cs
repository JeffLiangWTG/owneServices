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
	public class DtbConsignmentConfirmationDocumentSupporter : DocumentSupporter
	{
		public DtbConsignmentConfirmationDocumentSupporter(DtbConsignmentConfirmation consignmentConfirmation) : base(consignmentConfirmation)
		{
			if (consignmentConfirmation != null)
			{
				ParentBusinessObject = consignmentConfirmation.Transport;
			}
		}

		readonly DtbBookingConsignment ParentBusinessObject;

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbConsignConfirm; }
		}

		#endregion

		#region ShowReasonForNotPrintingCore

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

		#region DocWrappers / DataContext

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null) // Document header
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, ParentBusinessObject, BusinessObject);
			}

			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		#endregion

		#region CustomisationSecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.DtbBookingConsignmentCustomiseDocuments; }
		}

		#endregion
	}
}
