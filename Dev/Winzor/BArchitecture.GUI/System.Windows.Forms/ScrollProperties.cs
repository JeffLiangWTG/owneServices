namespace System.Windows.Forms;

public class ScrollProperties
{
	readonly ScrollableControl? _parent;

	internal bool _visible;

	public bool Enabled { get; set; }

	public int Maximum { get; set; }

	public int Value { get; set; }

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_parent != null && _parent.AutoScroll)
			{
				return;
			}

			if (value != _visible)
			{
				_visible = value;
				_parent?.NotifyRenderRequired();
			}
		}
	}

	public int SmallChange { get; set; }

	protected ScrollProperties(ScrollableControl? container)
	{
		_parent = container;
	}
}
