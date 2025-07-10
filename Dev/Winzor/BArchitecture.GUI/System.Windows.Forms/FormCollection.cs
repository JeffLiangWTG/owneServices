using WinzorFramework;

namespace System.Windows.Forms;

public class FormCollection : WrappedList<Form>
{
	public Form? this[string? name] => this.FirstOrDefault(f => f.Name == name);
}
