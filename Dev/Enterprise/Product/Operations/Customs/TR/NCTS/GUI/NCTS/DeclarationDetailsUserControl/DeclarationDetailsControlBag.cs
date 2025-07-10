using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public sealed class DeclarationDetailsControlBag : ControlBag
	{
		public DeclarationDetailsControlBag()
		{
			LrnTextBox = RegisterControl(nameof(DeclarationDetailsUserControl.LrnTextBox));
		}

		public static DeclarationDetailsControlBag Instance => instance ?? (instance = new DeclarationDetailsControlBag());

		[ThreadStatic]
		static DeclarationDetailsControlBag instance;

		public ControlReference LrnTextBox { get; }

		protected override Control CreateTemplate() => new DeclarationDetailsUserControl();
	}
}
