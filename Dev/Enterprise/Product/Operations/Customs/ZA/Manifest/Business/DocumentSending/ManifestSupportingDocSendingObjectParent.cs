using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ManifestSupportingDocSendingObjectParent : BaseMessageSendingObjectParent<SupportingDocSendingObject>, ISupportingDocSendingObjectParent
	{
		public ManifestSupportingDocSendingObjectParent(IManifestSupportingDocSendingObject manifest) : base(manifest.Factory)
		{
			ParentManifest = manifest;
		}

		public readonly IManifestSupportingDocSendingObject ParentManifest;

		protected override NonPersistentBusinessObjectCollection<SupportingDocSendingObject> GetSendingObjectsCollectionCore()
		{
			return new ManifestSupportingDocSendingObjectCollection(ParentManifest);
		}

		public override BusinessObject TopLevelBusinessObject => ParentManifest as BusinessObject;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		public IEnumerable<ISupportingDocumentMessageDataProvider> SendingObjects => SendingObjectsCollection.Select(x => x).Cast<ISupportingDocumentMessageDataProvider>();
	}
}
