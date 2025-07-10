using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class TransferHeaderMessageChooserItemCollection : MessageChooserItemCollection<TransferHeaderMessageChooserItem>
	{
		protected override MessageChooserItem AddNewCore(MessageChooser messageChooser, ISelectionItem item, bool showStatus)
		{
			return new TransferHeaderMessageChooserItem((TransferHeaderMessageChooser)messageChooser, item, showStatus);
		}
	}
}
