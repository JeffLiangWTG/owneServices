using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationSupportingDocSendingObjectParent : BaseMessageSendingObjectParent<SupportingDocSendingObject>, ISupportingDocSendingObjectParent
	{
		public JobDeclarationSupportingDocSendingObjectParent(BaseJobDeclaration declaration) : base(declaration.Factory)
		{
			ParentDeclaration = declaration;
		}

		public readonly BaseJobDeclaration ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<SupportingDocSendingObject> GetSendingObjectsCollectionCore()
		{
			return new JobDeclarationSupportingDocSendingObjectCollection(ParentDeclaration);
		}

		public override BusinessObject TopLevelBusinessObject => ParentDeclaration;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		BusinessObjectFactory ISupportingDocSendingObjectParent.Factory => base.Factory;

		public IEnumerable<ISupportingDocumentMessageDataProvider> SendingObjects => SendingObjectsCollection.Select(x => x).Cast<ISupportingDocumentMessageDataProvider>();
	}
}
