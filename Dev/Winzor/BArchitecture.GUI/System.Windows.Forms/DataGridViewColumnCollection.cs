using WinzorFramework;

#nullable disable

namespace System.Windows.Forms;

public class DataGridViewColumnCollection : WrappedList<DataGridViewColumn>
{
	public virtual int Add(string columnName, string headerText) => -1;

	public virtual bool Contains(string columnName) => false;

	public DataGridViewColumn this[string columnName] => null;
}
