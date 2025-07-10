using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public class TransportDepartureControlBag : ControlBag
	{
		TransportDepartureControlBag()
		{
			TankerStatusDropEdit = RegisterControl(nameof(TransportDepartureUserControl.TankerStatusDropEdit));
		}

		public static TransportDepartureControlBag Instance => instance ?? (instance = new TransportDepartureControlBag());

		public ControlReference TankerStatusDropEdit { get; }

		protected override Control CreateTemplate() => new TransportDepartureUserControl();

		[ThreadStatic]
		static TransportDepartureControlBag instance;
	}
}
