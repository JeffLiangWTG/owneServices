using System.ComponentModel;

namespace System.Windows.Forms;

public class ContextMenuStrip : ToolStripDropDownMenu
{
	public ContextMenuStrip()
	{
	}

	public ContextMenuStrip(IContainer container)
	{
		// this constructor ensures ContextMenuStrip is disposed properly since its not parented to the form.
		if (container == null)
		{
			throw new ArgumentNullException(nameof(container));
		}
		container.Add(this);
	}

	public Control? SourceControl => SourceControlInternal;
}
