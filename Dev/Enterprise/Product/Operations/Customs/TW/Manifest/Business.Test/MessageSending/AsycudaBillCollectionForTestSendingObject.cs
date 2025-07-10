using System;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public class AsycudaBillCollectionForTestSendingObject : AsycudaBillCollection
	{
		public AsycudaBillCollectionForTestSendingObject(AsycudaManifestHeaderForTestSendingObject master)
			: base(master)
		{ }

		public new AsycudaBillForTestSendingObject this[int i] => (AsycudaBillForTestSendingObject)base[i];

		public new AsycudaManifestHeaderForTestSendingObject Master => (AsycudaManifestHeaderForTestSendingObject)base.Master;

		public new AsycudaBillForTestSendingObject AddNew() => (AsycudaBillForTestSendingObject)base.AddNew();

		public new AsycudaBillForTestSendingObject AddNew(Type bizOType) => (AsycudaBillForTestSendingObject)base.AddNew(bizOType);

		public override Type GetTypeOfElementsFromPK(ZGuid pK) => typeof(AsycudaBillForTestSendingObject);
	}
}
