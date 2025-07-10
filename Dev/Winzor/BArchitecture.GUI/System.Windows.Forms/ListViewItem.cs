using System.Drawing;
using WinzorFramework;

namespace System.Windows.Forms;

public class ListViewItem
{
	public ListViewItem()
	{
	}

	public ListViewItem(string text)
	{
		Text = text;
	}

	public ListView? ListView => listView;

	internal ListView? listView;

	public Font Font
	{
		get
		{
			if (SubItems.Count == 0)
			{
				if (listView is not null)
				{
					return listView.Font;
				}

				return Control.DefaultFont;
			}
			else
			{
				return SubItems[0].Font;
			}
		}
		set => SubItems[0].Font = value;
	}

	public int Index { get; }

	public string Text { get; set; } = string.Empty;

	public object? Tag { get; set; }

	public bool Selected { get; set; }

	public string ImageKey { get; set; } = string.Empty;

	public class ListViewSubItem
	{
		internal ListViewItem? owner;

		public ListViewSubItem(ListViewItem? owner, string? text)
		{
			this.owner = owner;
			this.text = text;
		}

		public string Text
		{
			get => text ?? string.Empty;
			set => text = value;
		}
		string? text;
		SubItemStyle? style;

		public Font Font
		{
			get
			{
				if (style != null && style.font != null)
				{
					return style.font;
				}

				if (owner != null && owner.listView != null)
				{
					return owner.listView.Font;
				}

				return Control.DefaultFont;
			}
			set
			{
				if (style == null)
				{
					style = new SubItemStyle();
				}

				if (style.font != value)
				{
					style.font = value;
				}
			}
		}

		class SubItemStyle
		{
			public Color backColor = Color.Empty;
			public Color foreColor = Color.Empty;
			public Font? font;
		}
	}

	public ListViewSubItemCollection SubItems
	{
		get
		{
			if (subItems != null && subItems.Count == 0)
			{
				subItems.Add(new ListViewSubItem(this, string.Empty));
			}
			return subItems ??= new ListViewSubItemCollection(this);
		}
	}
	ListViewSubItemCollection? subItems;

	internal void SetItemIndex(ListView listView)
	{
		this.listView = listView;
	}

	public class ListViewSubItemCollection : WrappedList<ListViewSubItem>
	{
		readonly ListViewItem owner;

		public ListViewSubItemCollection(ListViewItem owner)
		{
			this.owner = owner.OrThrowIfNull();
		}

		public ListViewSubItem Add(string? text)
		{
			var item = new ListViewSubItem(owner, text);
			Add(item);
			return item;
		}
	}
}
