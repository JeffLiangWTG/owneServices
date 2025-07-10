using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public class CustomsMessengerImplForTest : ICustomsMessenger
	{
		public CustomsMessengerImplForTest(IEDIMessageCollectionOwner owner) : this(owner, new CustomsMessageGeneratorImplForTest(new CargoWise.EntityFramework.BusinessObjectFactory())) { }
		public CustomsMessengerImplForTest(IEDIMessageCollectionOwner owner, ICustomsMessageGenerator generator)
		{
			this.owner = Argument.NotNull(owner, nameof(owner));
			this.generator = Argument.NotNull(generator, nameof(generator));
		}
		readonly IEDIMessageCollectionOwner owner;
		readonly ICustomsMessageGenerator generator;

		IEDIMessageCollectionOwner ICustomsMessenger.Owner => owner;
		ICustomsMessageGenerator ICustomsMessenger.MessageGenerator => generator;

		public bool ShouldCreateMessageForTest { get; set; } = true;
		bool ICustomsMessenger.ShouldCreateMessage(ActionResult previousResult) => ShouldCreateMessageForTest;
		public EDIMessage CreateMessageForTest
		{
			get
			{
				if (generator is CustomsMessageGeneratorImplForTest gen)
				{
					return gen.GenerateMessageForTest;
				}
				return null;
			}
			set
			{
				if (generator is CustomsMessageGeneratorImplForTest gen)
				{
					gen.UseMessageForTest = true;
					gen.GenerateMessageForTest = value;
				}
			}
		}

		public bool ProcessUpdatesForTest { get; set; } = true;
		bool ICustomsMessenger.ProcessUpdates(ActionResult previousResult) => ProcessUpdatesForTest;
	}

	public sealed class CustomsMessengerWithPermitSupportImplForTest : CustomsMessengerImplForTest, ISupportPermitProcessing
	{
		public CustomsMessengerWithPermitSupportImplForTest(IEDIMessageCollectionOwner owner, ICusPermitCusDecProcessor<EDIMessage> processor) : base(owner)
		{
			this.processor = processor;
		}
		readonly ICusPermitCusDecProcessor<EDIMessage> processor;

		ICusPermitCusDecProcessor<EDIMessage> ISupportPermitProcessing.PermitProcessor => processor;

		ZString ISupportPermitProcessing.GetPermitAppIdForMessage(EDIMessage message) => "PERM0001";
	}

	public sealed class CustomsMessengerWithISupportMessageSigning : CustomsMessengerImplForTest, ISupportMessageSigning
	{
		public CustomsMessengerWithISupportMessageSigning(IEDIMessageCollectionOwner owner) : base(owner)
		{
		}

		public Func<IEnumerable<EDIMessage>, ZString> SignMessagesForTest { get; set; }
		string ISupportMessageSigning.SignMessages(IReadOnlyCollection<EDIMessage> messages, ActionResult previousResult) => SignMessagesForTest?.Invoke(messages) ?? string.Empty;
	}
}
