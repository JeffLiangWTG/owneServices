using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ActiveABLEntryNumCollection : ActiveBusinessObjectCollection<ABLEntryNum>
	{
		public ActiveABLEntryNumCollection(AsycudaManifestHeader manifest, ZString country)
			: base(manifest.Factory, new AdhocCollectionRelationship(typeof(ABLEntryNum)))
		{
			this.manifest = manifest;
			this.country = Argument.NotNullOrEmpty(country, nameof(country));
		}

		public void Load()
		{
			((System.Collections.IList)this).Clear();
			if (manifest.AMA_RN_NKCountry == country)
			{
				AddRange(manifest.Bills.OfType<AsycudaBill>().WhereNotNull().SelectMany(x => x.CustomsEntryNumbers.OfType<ABLEntryNum>()).WhereNotNull());
			}
		}

		readonly AsycudaManifestHeader manifest;
		readonly ZString country;
		protected override bool AllowNew => false;
	}
}
