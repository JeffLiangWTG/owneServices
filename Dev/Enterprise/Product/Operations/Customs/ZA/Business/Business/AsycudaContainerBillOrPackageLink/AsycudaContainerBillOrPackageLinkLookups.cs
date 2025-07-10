namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaPackageContainerLinkLookups : ManifestBase.AsycudaContainerBillOrPackageLinkLookups
	{
		public AsycudaPackageContainerLinkLookups(AsycudaContainerBillOrPackageLink parent)
			: base(parent)
		{
		}
		protected new AsycudaContainerBillOrPackageLink Parent => (AsycudaContainerBillOrPackageLink)base.Parent;
	}
}
