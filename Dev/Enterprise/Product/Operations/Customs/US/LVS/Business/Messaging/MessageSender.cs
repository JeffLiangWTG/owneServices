using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.LVS.Business
{
	public class MessageSender
	{
		public delegate ZDialogResult PrepareEventHandler();

		public MessageSender(CusUSLVClearance clearance)
		{
			this.clearance = Argument.NotNull(clearance, "clearance");
		}
		readonly CusUSLVClearance clearance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event PrepareEventHandler OnPrepare;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event Customs.Business.MessageSender.SaveEventHandler OnSave
		{
			add { MessageManager.OnSave += value; }
			remove { MessageManager.OnSave -= value; }
		}

		public bool SendMessage()
		{
			bool result = false;

			if (OnPrepare != null)
			{
				if (OnPrepare() == ZDialogResult.Yes)
				{
					MessageManager.SaveJob();
				}

				result = true;
			}

			return result;
		}

		MessageManager MessageManager
		{
			get
			{
				if (messageManager == null)
				{
					messageManager = new MessageManager(clearance);
				}
				return messageManager;
			}
		}
		MessageManager messageManager;
	}
}
