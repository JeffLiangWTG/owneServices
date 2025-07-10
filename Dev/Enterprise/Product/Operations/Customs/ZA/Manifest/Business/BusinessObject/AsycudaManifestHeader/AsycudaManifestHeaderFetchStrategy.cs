using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaManifestHeaderFetchStrategy : ASYCUDA.Business.AsycudaManifestHeaderFetchStrategy
	{
		public AsycudaManifestHeaderFetchStrategy(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected new AsycudaManifestHeader BusinessObject => (AsycudaManifestHeader)base.BusinessObject;

		protected override void AddFetchHintsForTreeTableStrategyCore(IExternalFetchHintSupporter externalFetchHintSupporter)
		{
			base.AddFetchHintsForTreeTableStrategyCore(externalFetchHintSupporter);
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
		}

		protected override void AddFetchHintsForLoadChildEditableObjectsCore()
		{
			base.AddFetchHintsForLoadChildEditableObjectsCore();
			if (ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(BusinessObject, Factory))
			{
				Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, BusinessObject.PK);
			}
		}
	}
}
