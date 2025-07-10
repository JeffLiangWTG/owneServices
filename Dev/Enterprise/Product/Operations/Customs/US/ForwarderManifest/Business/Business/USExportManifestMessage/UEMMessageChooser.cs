using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public sealed class UEMMessageChooser : MessageChooser
	{
		public UEMMessageChooser(AsycudaManifestHeader header, IEnumerable<ISelectionItem> items)
			: base(header, items, true)
		{
		}

		protected override MessageChooserItemCollection CreateNewMessageChooserItemCollection() => new UEMMessageChooserItemCollection();

		public new UEMMessageChooserItemCollection ChooserItems => (UEMMessageChooserItemCollection)base.ChooserItems;
	}
}
