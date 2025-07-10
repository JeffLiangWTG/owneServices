using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderForTestSendingObject : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderForTestSendingObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Action<ZPropertyInfo> MockValidationMessage;

		protected override Type GetBillTypeCore() => typeof(AsycudaBillForTestSendingObject);

		public new AsycudaBillCollectionForTestSendingObject Bills => (AsycudaBillCollectionForTestSendingObject)base.Bills;

		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollectionForTestSendingObject(this);

		protected override void RunPreSaveValidationCore() => Validation.ValidateAll();

		public new AsycudaManifestHeaderValidationForTestSendingObject Validation => GetNewValidation();

		protected new AsycudaManifestHeaderValidationForTestSendingObject GetNewValidation() => new AsycudaManifestHeaderValidationForTestSendingObject(this);
	}
}
