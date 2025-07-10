using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class AsycudaContainerFetchStrategy : ManifestBase.AsycudaContainerFetchStrategy
	{
		public AsycudaContainerFetchStrategy(ManifestBase.AsycudaContainer container)
			: base(container)
		{
		}

		protected new AsycudaContainer BusinessObject => (AsycudaContainer)base.BusinessObject;

		protected override void AddFetchHintsForDeleteCore()
		{
			base.AddFetchHintsForDeleteCore();
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
		}
	}
}
