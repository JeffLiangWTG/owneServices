using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMMessageChooserItem : MessageChooserItem
	{
		public AIMMessageChooserItem(MessageChooser chooser, ISelectionItem item, bool showStatus)
			: base(chooser, item, showStatus)
		{
		}

		public new AIMMessageChooserItemValidation Validation => (AIMMessageChooserItemValidation)base.Validation;

		protected override MessageChooserItemValidation GetNewValidation() => new AIMMessageChooserItemValidation(this);

		public override ZString Description => BizO.SelectionDescription(false);
	}
}
