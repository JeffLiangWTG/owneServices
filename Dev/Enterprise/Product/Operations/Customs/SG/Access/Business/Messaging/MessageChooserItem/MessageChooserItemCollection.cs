using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business
{
	public class MessageChooserItemCollection : ASYCUDA.Business.MessageChooserItemCollection<MessageChooserItem>
	{
		protected override ASYCUDA.Business.MessageChooserItem AddNewCore(ASYCUDA.Business.MessageChooser messageChooser, ISelectionItem item, bool showStatus)
		{
			return new MessageChooserItem((MessageChooser)messageChooser, item, showStatus);
		}
	}
}
