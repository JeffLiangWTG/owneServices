using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public sealed class TransportDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new TransportDetailsLayoutsUserControl();

		public static TransportDetailsControlBag Instance => instance ?? (instance = new TransportDetailsControlBag());

		[ThreadStatic]
		static TransportDetailsControlBag instance;

		TransportDetailsControlBag()
		{
			TransportDetailsFlightUserControl = RegisterControl(nameof(TransportDetailsLayoutsUserControl.TransportDetailsFlightUserControl));
			TransportDetailsNationalityUserControl = RegisterControl(nameof(TransportDetailsLayoutsUserControl.TransportDetailsNationalityUserControl));
			TransportDetailsVoyageUserControl = RegisterControl(nameof(TransportDetailsLayoutsUserControl.TransportDetailsVoyageUserControl));
		}

		public ControlReference TransportDetailsFlightUserControl { get; }
		public ControlReference TransportDetailsNationalityUserControl { get; }
		public ControlReference TransportDetailsVoyageUserControl { get; }
	}
}
