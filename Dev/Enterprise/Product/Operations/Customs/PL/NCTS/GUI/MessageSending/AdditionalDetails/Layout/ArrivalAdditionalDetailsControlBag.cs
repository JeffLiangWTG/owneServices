using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public class ArrivalAdditionalDetailsControlBag : ControlBag
{
	public ArrivalAdditionalDetailsControlBag()
	{
		TirPageNumberDropEdit = RegisterControl(nameof(ArrivalAdditionalDetailsUserControl.TirPageNumberDropEdit));
		TirUnloadingNumberDropEdit = RegisterControl(nameof(ArrivalAdditionalDetailsUserControl.TirUnloadingNumberDropEdit));
	}

	public static ArrivalAdditionalDetailsControlBag Instance => instance ?? (instance = new ArrivalAdditionalDetailsControlBag());

	[ThreadStatic]
	static ArrivalAdditionalDetailsControlBag instance;

	protected override Control CreateTemplate() => new ArrivalAdditionalDetailsUserControl();

	public ControlReference TirPageNumberDropEdit { get; }
	public ControlReference TirUnloadingNumberDropEdit { get; }
}
