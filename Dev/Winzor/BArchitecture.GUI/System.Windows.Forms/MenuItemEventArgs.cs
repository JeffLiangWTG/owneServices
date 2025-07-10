namespace System.Windows.Forms;

public class MenuItemEventArgs
{
	public MenuItemEventArgs(MenuItem? item)
	{
		Item = item;
	}

	public MenuItem? Item { get; }
}
