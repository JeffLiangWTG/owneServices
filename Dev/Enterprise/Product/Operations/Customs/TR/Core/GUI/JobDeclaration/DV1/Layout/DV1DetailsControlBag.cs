using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class DV1DetailsControlBag : ControlBag
	{
		DV1DetailsControlBag()
		{
			PlaceTextBox = RegisterControl(nameof(DV1DetailsUserControl.PlaceTextBox));
			CustomsDecisionDateDateEdit = RegisterControl(nameof(DV1DetailsUserControl.CustomsDecisionDateDateEdit));
		}

		public static DV1DetailsControlBag Instance => instance ?? (instance = new DV1DetailsControlBag());

		[ThreadStatic]
		static DV1DetailsControlBag instance;

		protected override Control CreateTemplate() => new DV1DetailsUserControl();

		public ControlReference PlaceTextBox { get; }
		public ControlReference CustomsDecisionDateDateEdit { get; }
	}
}
