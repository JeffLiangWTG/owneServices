using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI
{
	public sealed class DeclarationDetailsControlBag : ControlBag
	{
		public DeclarationDetailsControlBag()
		{
			FallbackProcedureCheckBox = RegisterControl(nameof(DeclarationDetailsUserControl.FallbackProcedureCheckBox));
			FallbackUserControl = RegisterControl(nameof(DeclarationDetailsUserControl.FallbackUserControl));
		}

		public static DeclarationDetailsControlBag Instance => instance ?? (instance = new DeclarationDetailsControlBag());

		[ThreadStatic]
		static DeclarationDetailsControlBag instance;

		public ControlReference FallbackProcedureCheckBox { get; }
		public ControlReference FallbackUserControl { get; }

		protected override Control CreateTemplate() => new DeclarationDetailsUserControl();
	}
}
