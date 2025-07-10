using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class TransferHeaderMessageChooser : MessageChooser
	{
		public TransferHeaderMessageChooser(ASYCUDA.Business.AsycudaManifestHeader header, IEnumerable<ISelectionItem> items, bool showStatus, bool isArrivalMessage) : base(header, items, showStatus)
		{
			IsArrivalMessage = isArrivalMessage;
		}

		public new TransferHeaderMessageChooserItemCollection ChooserItems => (TransferHeaderMessageChooserItemCollection)base.ChooserItems;

		protected override MessageChooserItemCollection CreateNewMessageChooserItemCollection()
		{
			return new TransferHeaderMessageChooserItemCollection();
		}

		public override ZString SelectedDescription => string.Format(CultureInfo.CurrentCulture, "{0} of {1} Item(s) selected.", SelectedCount, ChooserItems.Count);

		protected override bool ShouldSelectChooserItem(MessageChooserItem chooserItem) => false;

		public bool IsArrivalMessage { get; set; }
	}
}
