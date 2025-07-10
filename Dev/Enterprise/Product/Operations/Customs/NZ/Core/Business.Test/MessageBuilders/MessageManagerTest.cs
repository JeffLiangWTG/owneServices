using System.Collections.Generic;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	using System;
	using System.Collections;
	using System.Text;
	using Enterprise.Customs.NZ.Registry;
	using NUnit.Framework;

	public abstract class MessageManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableOperationType()
		{
			MessageManager manager = GetNewMessageManager(MessageManager.OperationType.SubmitMessage);
			AssertEquals("HumanReadableOperationType", "Submit Message", manager.HumanReadableOperationType);
		}

		#region Implementation
		protected string GetSortedMessageErrors(IEnumerable<INotification> messageErrors)
		{
			string[] messageErrs = messageErrors.GetUniqueMessageList();
			ArrayList list = new ArrayList(messageErrs);
			list.Sort(new CaseInsensitiveComparer());
			StringBuilder result = new StringBuilder();
			foreach (string line in list)
			{
				result.Append(line + "\r\n");
			}
			return result.ToString().Trim();
		}

		protected abstract MessageManager GetNewMessageManager(MessageManager.OperationType operationType);

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}
		#endregion
	}

	public sealed class MessageManagerConcreteTest : TestCase
	{
		public void TestMessageTypeToBeSent()
		{
			TestMessageManager manager = new TestMessageManager(MessageManager.OperationType.SubmitMessage);
			manager.ExposedIsOkToExecute = false;
			AssertEquals(MessageManager.MessageType.None, manager.MessageTypeToBeSent);

			manager.ExposedMessageTypeForSubmit = MessageManager.MessageType.ReplaceHeaderAndLines;
			manager.ExposedIsOkToExecute = true;
			AssertEquals(MessageManager.MessageType.ReplaceHeaderAndLines, manager.MessageTypeToBeSent);

			manager = new TestMessageManager(MessageManager.OperationType.CancelMessage);
			AssertEquals(MessageManager.MessageType.CancelEntry, manager.MessageTypeToBeSent);

			manager = new TestMessageManager(MessageManager.OperationType.ResetToOriginal);
			AssertEquals(MessageManager.MessageType.ResetToOriginal, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeAndStatus()
		{
			TestMessageManager manager = new TestMessageManager(MessageManager.OperationType.SubmitMessage);
			manager.ExposedIsOkToExecute = false;
			AssertEquals(manager.LastHumanReadableStatus, manager.MessageTypeAndStatus);

			manager.ExposedIsOkToExecute = true;
			manager.ExposedMessageTypeForSubmit = MessageManager.MessageType.Original;
			AssertEquals("Ready to Send Original Message", manager.MessageTypeAndStatus);

			manager.ExposedMessageTypeForSubmit = MessageManager.MessageType.Replacement;
			AssertEquals("Ready to Send Replacement Message", manager.MessageTypeAndStatus);

			manager = new TestMessageManager(MessageManager.OperationType.CancelMessage);
			manager.ExposedIsOkToExecute = true;
			AssertEquals("Ready to Send Cancellation Message", manager.MessageTypeAndStatus);
		}

		class TestMessageManager : MessageManager
		{
			public TestMessageManager(OperationType operationType)
				: base(operationType)
			{
				fLastHumanReadableStatus = "LastHumanReadableStatus";
				ExposedIsOkToExecute = true;
				ExposedMessageTypeForSubmit = MessageType.None;
			}
			public bool ExposedIsOkToExecute;
			public override bool IsOkToExecute
			{
				get
				{
					return ExposedIsOkToExecute;
				}
			}
			public override bool Execute()
			{
				return false;
			}
			public MessageType ExposedMessageTypeForSubmit;
			protected override MessageType GetMessageTypeForSubmit()
			{
				return ExposedMessageTypeForSubmit;
			}
		}
	}
}
