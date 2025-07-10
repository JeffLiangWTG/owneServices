using CargoWise.Common;

namespace Enterprise.Customs.US.Business
{
	public class AESMessageSender
	{
		public delegate void PrepareEventHandler();

		public AESMessageSender(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}

		readonly JobDeclaration declaration;

		public event PrepareEventHandler OnPrepare;

		public event Customs.Business.MessageSender.SaveEventHandler OnSave
		{
			add { MessageManager.OnSave += value; }
			remove { MessageManager.OnSave -= value; }
		}

		public bool SendMessage()
		{
			var errorMsgs = MessageManager.GetAnyReasonsWeCantSendToAESTIR();
			if (errorMsgs.Count > 0)
			{
				var errorMsgsString = "";
				foreach (string s in errorMsgs)
				{
					errorMsgsString += s;
				}
				declaration.MessageInitiator.WarnUserAboutSomething(errorMsgsString, "Send Message");
				return false;
			}
			bool result = false;
			if (MessageManager.MergeAndCheck())
			{
				foreach (CusEntryHeader entry in declaration.CustomsEntryHeaders)
				{
					entry.US_SendWithdrawn = !entry.IsActive && entry.HasBeenLodgedAtCustoms && !entry.HasBeenWithdrawn;
				}

				if (OnPrepare != null)
				{
					OnPrepare();
					result = true;
				}
			}
			return result;
		}

		AESMessageManager MessageManager
		{
			get
			{
				if (messageManager == null)
				{
					messageManager = new AESMessageManager(declaration);
				}
				return messageManager;
			}
		}
		AESMessageManager messageManager;
	}
}
