using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI;

public sealed class UnloadingDetailsControlBag : ControlBag
{
	public UnloadingDetailsControlBag()
	{
		UnloadingRemarksFreeTextTextBox = RegisterControl(nameof(UnloadingDetailsUserControl.UnloadingRemarksFreeTextTextBox));
		UnloadingRemarksGrid = RegisterControl(nameof(UnloadingDetailsUserControl.UnloadingRemarksGrid));
	}

	public static UnloadingDetailsControlBag Instance => instance ?? (instance = new UnloadingDetailsControlBag());

	[ThreadStatic]
	static UnloadingDetailsControlBag instance;

	public ControlReference UnloadingRemarksFreeTextTextBox { get; }

	public ControlReference UnloadingRemarksGrid { get; }

	protected override Control CreateTemplate() => new UnloadingDetailsUserControl();
}
