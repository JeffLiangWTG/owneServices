using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.GUI;

sealed class DeclarationDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new DeclarationDetailsUserControl();

	public static DeclarationDetailsControlBag Instance => instance ??= new DeclarationDetailsControlBag();

	[ThreadSafe]
	static DeclarationDetailsControlBag instance;

	DeclarationDetailsControlBag()
	{
		PhaseStatusTextBox = RegisterControl(nameof(DeclarationDetailsUserControl.PhaseStatusTextBox));
		MessageStatusTextBox = RegisterControl(nameof(DeclarationDetailsUserControl.MessageStatusTextBox));
	}

	public ControlReference PhaseStatusTextBox { get; }
	public ControlReference MessageStatusTextBox { get; }
}
