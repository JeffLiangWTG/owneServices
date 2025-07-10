using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaContainerCollection : AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>
	{
		public AsycudaContainerCollection(AsycudaManifestHeader master)
			: base(master)
		{ }

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (!IsLoading && bizOAdded is AsycudaContainer container)
			{
				Master.Bills.Cast<AsycudaBill>().Where(x => x.IsAsycudaBillLinkAsycudaContainersLoaded).ForEach(x => x.AsycudaBillLinkAsycudaContainers.RebuildElements());
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			if (bizO is AsycudaContainer container)
			{
				Master.Bills.Cast<AsycudaBill>().Where(x => x.IsAsycudaBillLinkAsycudaContainersLoaded)
					.Select(x => x.AsycudaBillLinkAsycudaContainers).ForEach(x => x.DeleteAsycudaBillLinkAsycudaContainerByContainer(container));
			}
			base.OnRemoved(bizO);
		}
	}
}
