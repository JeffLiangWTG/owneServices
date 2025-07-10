using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonDeclarationDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new CommonDeclarationDetailsUserControl();

		public static CommonDeclarationDetailsControlBag Instance => instance ??= new CommonDeclarationDetailsControlBag();

		[ThreadSafe]
		static CommonDeclarationDetailsControlBag instance;

		CommonDeclarationDetailsControlBag()
		{
			DeclarationNumberTextBox = RegisterControl(nameof(CommonDeclarationDetailsUserControl.DeclarationNumberTextBox));
			StatusTextBox = RegisterControl(nameof(CommonDeclarationDetailsUserControl.StatusTextBox));
		}

		public ControlReference DeclarationNumberTextBox { get; }
		public ControlReference StatusTextBox { get; }
	}
}
