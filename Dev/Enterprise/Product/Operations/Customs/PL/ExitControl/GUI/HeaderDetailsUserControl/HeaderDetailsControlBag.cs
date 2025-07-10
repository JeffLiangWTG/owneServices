using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.ExitControl.GUI;

public sealed class HeaderDetailsControlBag : ControlBag
{
	HeaderDetailsControlBag()
	{
		BrokerCodeFindBox = RegisterControl(nameof(HeaderDetailsUserControl.BrokerCodeFindBox));
		CertificateDropEdit = RegisterControl(nameof(HeaderDetailsUserControl.CertificateDropEdit));
		TrainingCheckBox = RegisterControl(nameof(HeaderDetailsUserControl.TrainingCheckBox));
		StoringFlagCheckBox = RegisterControl(nameof(HeaderDetailsUserControl.StoringFlagCheckBox));
	}

	public static HeaderDetailsControlBag Instance => instance ??= new HeaderDetailsControlBag();

	[ThreadStatic]
	static HeaderDetailsControlBag instance;

	protected override Control CreateTemplate() => new HeaderDetailsUserControl();

	public ControlReference BrokerCodeFindBox { get; }

	public ControlReference CertificateDropEdit { get; }

	public ControlReference TrainingCheckBox { get; }

	public ControlReference StoringFlagCheckBox { get; }
}
