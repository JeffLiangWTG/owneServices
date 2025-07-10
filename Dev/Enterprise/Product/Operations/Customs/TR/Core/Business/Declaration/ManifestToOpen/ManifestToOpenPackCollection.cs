using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenPackCollection : ActiveBusinessObjectCollection<ManifestToOpenPack>
	{
		public ManifestToOpenPackCollection(ManifestToOpenBill master) : base(master)
		{
			Bill = master;
		}

		public ManifestToOpenBill Bill { get; }

		protected override void SetDefaultsForNewElementCore(ManifestToOpenPack newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.TPI_ClusterKey = Bill.TPD_ClusterKey;
		}
	}
}
