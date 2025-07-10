using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMMessageChooserItemCollection : MessageChooserItemCollection<UEMMessageChooserItem>
	{
		protected override MessageChooserItem AddNewCore(MessageChooser messageChooser, ISelectionItem item, bool showStatus)
		{
			return new UEMMessageChooserItem((UEMMessageChooser)messageChooser, item, showStatus);
		}
	}
}
