using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBillLinkAsycudaContainerCollection : NonPersistentBusinessObjectCollection<AsycudaBillLinkAsycudaContainer>
	{
		public AsycudaBillLinkAsycudaContainerCollection(AsycudaBill bill)
			: base(bill.Factory)
		{
			Bill = bill;
		}

		AsycudaBill Bill { get; }

		AsycudaContainerCollection Containers => Bill.Header?.Containers;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override bool AllowSort => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		public override void Load()
		{
			RebuildElements();
		}

		public void RebuildElements()
		{
			RemoveAll();
			BuildElements();
		}

		void BuildElements()
		{
			if (Containers != null)
			{
				foreach (AsycudaContainer container in Containers)
				{
					AddAsycudaBillLinkAsycudaContainer(container);
				}
			}
		}

		void AddAsycudaBillLinkAsycudaContainer(AsycudaContainer container)
		{
			var asycudaBillLinkAsycudaContainer = new AsycudaBillLinkAsycudaContainer(Bill, container);
			if (!this.Cast<AsycudaBillLinkAsycudaContainer>().Any(x => x.PK == container.PK))
			{
				Add(asycudaBillLinkAsycudaContainer);
			}
		}

		public void DeleteAsycudaBillLinkAsycudaContainerByContainer(AsycudaContainer container)
		{
			var linkedItem = this.Cast<AsycudaBillLinkAsycudaContainer>().FirstOrDefault(x => x.PK == container.PK);
			if (linkedItem != null)
			{
				RemoveAndDelete(linkedItem);
			}
		}
	}
}
