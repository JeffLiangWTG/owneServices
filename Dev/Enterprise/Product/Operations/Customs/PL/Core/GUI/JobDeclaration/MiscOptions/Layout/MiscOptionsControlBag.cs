using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class MiscOptionsControlBag : ControlBag
{
	MiscOptionsControlBag()
	{
		SupportingInformationUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.SupportingInformationUserControl));
		ExciseZDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.ExciseZDropEdit));
		VATZDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.VATZDropEdit));
	}

	public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

	[ThreadStatic]
	static MiscOptionsControlBag instance;

	protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();

	public ControlReference SupportingInformationUserControl { get; }
	public ControlReference ExciseZDropEdit { get; }
	public ControlReference VATZDropEdit { get; }
}
