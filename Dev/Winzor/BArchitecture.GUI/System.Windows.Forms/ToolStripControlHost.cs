namespace System.Windows.Forms;

public class ToolStripControlHost : ToolStripItem
{
	public ToolStripControlHost(Control c)
	{
		Control = c;
	}

	public Control Control { get; set; }

	/// <summary>Raises the <see cref="E:System.Windows.Forms.Control.ParentChanged" /> event.</summary>
	/// <param name="oldParent">The original parent of the item.</param>
	/// <param name="newParent">The new parent of the item.</param>
	protected override void OnParentChanged(ToolStrip? oldParent, ToolStrip? newParent)
	{
		if (oldParent != null && base.Owner == null && newParent == null && Control != null)
		{
			GetControlCollection(Control.Parent as ToolStrip)?.RemoveInternal(Control);
		}
		else
		{
			SyncControlParent();
		}
		base.OnParentChanged(oldParent, newParent);
	}

	static WindowsFormsUtils.ReadOnlyControlCollection? GetControlCollection(ToolStrip? toolStrip)
	{
		return (toolStrip != null) ? ((WindowsFormsUtils.ReadOnlyControlCollection)toolStrip.Controls) : null;
	}

	void SyncControlParent()
	{
		GetControlCollection(base.Parent as ToolStrip)?.AddInternal(Control);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ToolStripControlHost" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">
	///   <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing && Control != null)
		{
			Control.Dispose();
		}
	}
}
