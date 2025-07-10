using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public sealed class ShipmentDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentDetailsLayoutsUserControl();

		public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

		[ThreadStatic]
		static ShipmentDetailsControlBag instance;

		ShipmentDetailsControlBag()
		{
			ShipmentDetailsFinalDestinationUserControl = RegisterControl(nameof(ShipmentDetailsLayoutsUserControl.ShipmentDetailsFinalDestinationUserControl));
			ShipmentDetailsOriginUserControl = RegisterControl(nameof(ShipmentDetailsLayoutsUserControl.ShipmentDetailsOriginUserControl));
			ShipmentDetailsGoodsLocationUserControl = RegisterControl(nameof(ShipmentDetailsLayoutsUserControl.ShipmentDetailsGoodsLocationUserControl));
			ShipmentDetailsWeightAndVolumeUserControl = RegisterControl(nameof(ShipmentDetailsLayoutsUserControl.ShipmentDetailsWeightAndVolumeUserControl));
		}

		public ControlReference ShipmentDetailsFinalDestinationUserControl { get; }
		public ControlReference ShipmentDetailsOriginUserControl { get; }
		public ControlReference ShipmentDetailsGoodsLocationUserControl { get; }
		public ControlReference ShipmentDetailsWeightAndVolumeUserControl { get; }
	}
}
