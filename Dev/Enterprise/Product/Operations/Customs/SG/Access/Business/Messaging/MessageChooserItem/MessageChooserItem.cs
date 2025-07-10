using CargoWise.Types;
using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business
{
	public sealed class MessageChooserItem : ASYCUDA.Business.MessageChooserItem
	{
		public MessageChooserItem(MessageChooser chooser, ISelectionItem item, bool showStatus)
			: base(chooser, item, showStatus)
		{
		}

		public ZBool RequiresCycleFields => Chooser.RequiresCycleFields;

		public new MessageChooser Chooser => (MessageChooser)base.Chooser;
		public new MessageChooserItemValidation Validation => (MessageChooserItemValidation)base.Validation;
		protected override ASYCUDA.Business.MessageChooserItemValidation GetNewValidation()
		{
			return new MessageChooserItemValidation(this);
		}
	}
}
