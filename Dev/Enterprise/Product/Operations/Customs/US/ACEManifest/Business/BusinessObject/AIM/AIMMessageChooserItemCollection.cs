using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMMessageChooserItemCollection : MessageChooserItemCollection<AIMMessageChooserItem>
	{
		protected override MessageChooserItem AddNewCore(MessageChooser messageChooser, ISelectionItem item, bool showStatus)
		{
			return new AIMMessageChooserItem((AIMMessageChooser)messageChooser, item, showStatus);
		}
	}
}
