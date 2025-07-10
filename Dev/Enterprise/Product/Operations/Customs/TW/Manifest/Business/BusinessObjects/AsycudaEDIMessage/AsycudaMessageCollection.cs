using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaMessageCollection : EDIMessageCollectionNonDependent
	{
		public AsycudaMessageCollection(BusinessObjectFactory factory, AsycudaManifestHeader manifest)
			: base(factory, manifest)
		{
		}

		public new AsycudaMessage AddNew()
		{
			return (AsycudaMessage)base.AddNew();
		}

		public new AsycudaMessage this[int index]
		{
			get { return (AsycudaMessage)(base[index]); }
		}
	}
}
