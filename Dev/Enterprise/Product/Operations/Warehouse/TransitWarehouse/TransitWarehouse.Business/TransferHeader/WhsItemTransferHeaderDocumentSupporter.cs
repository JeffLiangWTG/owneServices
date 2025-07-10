using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemTransferHeaderDocumentSupporter : DocumentSupporter
	{
		public WhsItemTransferHeaderDocumentSupporter(WhsItemTransferHeader transferHeader) : base(transferHeader)
		{
		}

		#region BusinessContext

		public override BusinessContext BusinessContext => BusinessContext.TransitTransfHeader;

		#endregion

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsItemTransferHeaderCustomizeDocuments;

		#endregion

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, WhsItemTransferHeader);
			}

			return result;
		}

		#endregion

		#region ShowReasonForNotPrinting

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

		#region GetSupportedDataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		#endregion

		#region WhsItemTransferHeader

		protected WhsItemTransferHeader WhsItemTransferHeader
		{
			get { return (WhsItemTransferHeader)BusinessObject; }
		}

		#endregion
	}
}
