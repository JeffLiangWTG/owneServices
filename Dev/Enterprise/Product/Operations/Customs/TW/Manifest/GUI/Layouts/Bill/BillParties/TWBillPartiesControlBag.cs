using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public sealed class TWBillPartiesControlBag : ControlBag
	{
		public static TWBillPartiesControlBag Instance => instance ?? (instance = new TWBillPartiesControlBag());

		[ThreadStatic]
		static TWBillPartiesControlBag instance;

		TWBillPartiesControlBag()
		{
			ShipperAddressUserControl = RegisterControl(nameof(TWBillPartiesUserControl.ShipperAddressUserControl));
			ConsigneeAddressUserControl = RegisterControl(nameof(TWBillPartiesUserControl.ConsigneeAddressUserControl));
			NotifyPartyAddressUserControl = RegisterControl(nameof(TWBillPartiesUserControl.NotifyPartyAddressUserControl));
		}

		protected override Control CreateTemplate()
		{
			return new TWBillPartiesUserControl();
		}

		public ControlReference ShipperAddressUserControl { get; }

		public ControlReference ConsigneeAddressUserControl { get; }

		public ControlReference NotifyPartyAddressUserControl { get; }
	}
}
