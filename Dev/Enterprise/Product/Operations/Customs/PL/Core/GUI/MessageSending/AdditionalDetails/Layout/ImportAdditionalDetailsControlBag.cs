using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class ImportAdditionalDetailsControlBag : ControlBag
{
	public static ImportAdditionalDetailsControlBag Instance => instance ?? (instance = new ImportAdditionalDetailsControlBag());

	[ThreadStatic]
	static ImportAdditionalDetailsControlBag instance;

	protected override Control CreateTemplate() => new ImportAdditionalDetailsUserControl();
}
