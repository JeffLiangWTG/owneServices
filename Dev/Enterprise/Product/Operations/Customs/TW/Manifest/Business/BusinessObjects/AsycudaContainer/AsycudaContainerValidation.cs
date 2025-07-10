using System.Linq;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent) : base(parent)
		{
		}

		public new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		protected override void CheckACN_ContainerNumber()
		{
			base.CheckACN_ContainerNumber();

			var parent = Parent;
			var foundBill = parent?.Header?.Bills.Cast<AsycudaBill>().Any(b => b.GetDivot(Parent.PK) != null) ?? false;
			if (!foundBill)
			{
				parent.ACN_ContainerNumberInfo.AddWarning(Res.GetString("40F9F612-8775-455F-98BA-BFC6B834D56E", "This container does not appear on any bills."));
			}
		}

		protected override bool CheckContainerLinkToPack => false;
	}
}
