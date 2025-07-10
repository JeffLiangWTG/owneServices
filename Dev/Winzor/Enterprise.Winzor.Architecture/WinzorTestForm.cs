#if DEBUG
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Winzor.Architecture;

public class WinzorTestForm : ZForm
{
	public new KBindingSource BindingSource => base.BindingSource;
}
#endif
