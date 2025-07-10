using WinzorFramework;

#nullable disable

namespace System.Windows.Forms;

public class DataGridViewCellCollection : WrappedList<DataGridViewCell>
{
	public virtual DataGridViewCell this[string key] => null;
}
