namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaPackageContainerLinkValidation : ManifestBase.AsycudaContainerBillOrPackageLinkValidation
	{
		public AsycudaPackageContainerLinkValidation(AsycudaContainerBillOrPackageLink parent)
			: base(parent)
		{
		}

		protected override void CheckAPC_ACN_Container()
		{
		}

		protected override void CheckAPC_ACN_ContainerIsNotEmpty()
		{
		}

		protected override void CheckAPC_ACN_ContainerIsValidZGuid()
		{
		}

		protected new AsycudaContainerBillOrPackageLink Parent => (AsycudaContainerBillOrPackageLink)base.Parent;
	}
}
