using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.GUI;

sealed class SumARegisterDetailsHeaderControlBag : ControlBag
{
	protected override Control CreateTemplate() => new SumARegisterDetailsHeaderUserControl();

	public static SumARegisterDetailsHeaderControlBag Instance => instance ??= new SumARegisterDetailsHeaderControlBag();

	[ThreadSafe]
	static SumARegisterDetailsHeaderControlBag instance;

	SumARegisterDetailsHeaderControlBag()
	{
		GoodsNumberUserControl = RegisterControl(nameof(SumARegisterDetailsHeaderUserControl.GoodsNumberUserControl));
		TrasportMeansUserControl = RegisterControl(nameof(SumARegisterDetailsHeaderUserControl.TrasportMeansUserControl));
		UnloadingRemarksLabel = RegisterControl(nameof(SumARegisterDetailsHeaderUserControl.UnloadingRemarksLabel));
		UnloadingRemarksTextBox = RegisterControl(nameof(SumARegisterDetailsHeaderUserControl.UnloadingRemarksTextBox));
	}

	public ControlReference GoodsNumberUserControl { get; }
	public ControlReference TrasportMeansUserControl { get; }
	public ControlReference UnloadingRemarksLabel { get; }
	public ControlReference UnloadingRemarksTextBox { get; }
}
