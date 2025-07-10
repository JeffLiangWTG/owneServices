using Enterprise.ZArchitecture.GUI;
#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Customs.GUI
{
	public partial class NumbersUserControl : ZUserControl
	{
		public NumbersUserControl()
		{
			InitializeComponent();
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(this.DropEditType);
#endif
		}
	}
}
