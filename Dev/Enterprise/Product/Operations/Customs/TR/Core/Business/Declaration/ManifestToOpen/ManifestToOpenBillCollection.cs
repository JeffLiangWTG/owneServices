using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenBillCollection : ActiveBusinessObjectCollection<ManifestToOpenBill>
	{
		public ManifestToOpenBillCollection(ManifestToOpenHeader master) : base(master.Factory, master, new ZQuery(), CusTRPreviousDocumentSchema.TPD_CE_EntryNumber)
		{
			Manifest = master;
		}

		ManifestToOpenHeader Manifest { get; }

		protected override void SetDefaultsForNewElementCore(ManifestToOpenBill newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.TPD_ClusterKey = Manifest.ClusterKey;
		}
	}
}
