using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public class MessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<MessageSendingObject>
	{
		public MessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(MessageSendingObject.Schema.DeclarationType, true, 100),
			new MessageSendingObjectProperty(MessageSendingObject.Schema.Description, true, 100),
			new MessageSendingObjectProperty(MessageSendingObject.Schema.Procedure, true, 100),
			new MessageSendingObjectProperty(MessageSendingObject.Schema.MessageType, true, 100),
			new MessageSendingObjectProperty(MessageSendingObject.Schema.CustomsOffice, true, 200),
			new MessageSendingObjectProperty(MessageSendingObject.Schema.EntryStatus, true, 100),
			new MessageSendingObjectProperty(MessageSendingObject.Schema.EntryNumber, true, 100),
			new MessageSendingObjectProperty(MessageSendingObject.Schema.PaymentMethod, true, 100),
		};

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
			=> new MessageSendingObject((CusEntryHeader)header);

		protected override ZString GetAdditionalWarningsCore()
		{
			var result = new ZStringBuilder(base.GetAdditionalWarningsCore());

			foreach (JobDeclarationMessageSendingObject sendingObject in SelectedSendingObjects)
			{
				var entry = (CusEntryHeader)sendingObject.Header;
				if (entry.CH_EntryStatus == MessageSendingStatusCodes.Codes.RefusalOfDeclaration)
				{
					result.AppendLine((NoResString)"This declaration has been rejected by customs (IU). Cannot be resent.");
				}
			}
			return result.ToString();
		}

		public ZDate RequestProcessingDate
		{
			get; set;
		}
	}
}
