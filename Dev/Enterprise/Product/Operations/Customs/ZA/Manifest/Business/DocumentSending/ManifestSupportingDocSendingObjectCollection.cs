using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ManifestSupportingDocSendingObjectCollection : NonPersistentBusinessObjectCollection<SupportingDocSendingObject>
	{
		public ManifestSupportingDocSendingObjectCollection(IManifestSupportingDocSendingObject manifest) : base(manifest.Factory)
		{
			this.manifest = manifest;
		}

		protected IManifestSupportingDocSendingObject manifest;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return CreateSupportingDocSendingObject();
		}

		protected virtual BusinessObject CreateSupportingDocSendingObject()
		{
			return SupportingDocSendingObject.New(manifest);
		}
	}
}
