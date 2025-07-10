using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageSendingObjectParent : BaseMessageSendingObjectParent<ControllingMessageSendingObject>, IJobDeclarationMessageSendingObjectParent
	{
		public ControllingMessageSendingObjectParent(JobDeclaration declaration, ZString messageType)
			: this(declaration, messageType, ZString.Empty)
		{
		}

		public ControllingMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, ZString menuCaption)
			: base(declaration.Factory)
		{
			MenuCaption = menuCaption;
			Declaration = declaration;
			MessageType = messageType;
		}

		public ZString MenuCaption { get; }

		public JobDeclaration Declaration { get; }

		public ZString MessageType { get; }

		public override BusinessObject TopLevelBusinessObject => Declaration;

		protected override NonPersistentBusinessObjectCollection<ControllingMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var sendingObjectsCollection = GetNewSendingObjectCollection(Factory);
			if (Declaration.CustomsEntryInstructionProvider?.CustomsEntryInstructions?.Any<CusEntryInstruction>() ?? false)
			{
				foreach (var messageHeader in Declaration.CusEntryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().Where(x => x.TW1_ControllingMessageType == MessageType))
				{
					var message = GetNewSendingObject(messageHeader);
					if (message != null)
					{
						sendingObjectsCollection.Add(message);
					}
				}
			}
			return sendingObjectsCollection;
		}

		protected virtual ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader) => new ControllingMessageSendingObject(messageHeader);

		protected virtual NonPersistentBusinessObjectCollection<ControllingMessageSendingObject> GetNewSendingObjectCollection(BusinessObjectFactory factory) => new ControllingMessageSendingObjectCollection(factory);

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		#region IJobDeclarationMessageSendingObjectParent
		BaseJobDeclaration IJobDeclarationMessageSendingObjectParent.ParentDeclaration => Declaration;

		IEnumerable<MessageSendingObjectProperty> IJobDeclarationMessageSendingObjectParent.MessageSendingObjectProperties => Enumerable.Empty<MessageSendingObjectProperty>();

		IEnumerable<BaseMessageSendingObject> IJobDeclarationMessageSendingObjectParent.SendingObjectsCollection => SendingObjectsCollection.Cast<ControllingMessageSendingObject>();

		ZString IJobDeclarationMessageSendingObjectParent.BizObjValidationMessageErrors => ZString.Empty;

		ZString IJobDeclarationMessageSendingObjectParent.AdditionalWarnings => ZString.Empty;
		#endregion
	}
}
