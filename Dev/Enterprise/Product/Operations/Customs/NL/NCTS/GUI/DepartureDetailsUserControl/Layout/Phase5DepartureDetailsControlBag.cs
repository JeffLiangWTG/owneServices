using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI;

public sealed class Phase5DepartureDetailsControlBag : ControlBag
{
	Phase5DepartureDetailsControlBag()
	{
		CalCalculationMethodDropEdit = RegisterControl(nameof(Phase5DepartureDetailsUserControl.CalCalculationMethodDropEdit));
		DateLimitAndCalculationUserControl = RegisterControl(nameof(Phase5DepartureDetailsUserControl.DateLimitAndCalculationUserControl));
	}

	public static Phase5DepartureDetailsControlBag Instance => instance ?? (instance = new Phase5DepartureDetailsControlBag());

	[ThreadStatic]
	static Phase5DepartureDetailsControlBag instance;

	protected override Control CreateTemplate() => new Phase5DepartureDetailsUserControl();

	public ControlReference CalCalculationMethodDropEdit { get; }

	public ControlReference DateLimitAndCalculationUserControl { get; }
}
