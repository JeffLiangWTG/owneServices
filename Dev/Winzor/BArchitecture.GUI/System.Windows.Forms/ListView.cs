using System.Drawing;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

public partial class ListView : Control
{
	BorderStyle _borderStyle = BorderStyle.Fixed3D;

	public ListView()
	{
		dotNetObjectReference = DotNetObjectReference.Create(this);
		Items = new ListViewItemCollection(this);
		SelectedItems = new ListViewItemCollection(this);
		BackColor = SystemColors.Window;
		ForeColor = DefaultForeColor;

		PerformLayout();
	}

	/// <summary>
	///  Removes all items and columns from the ListView.
	/// </summary>
	public void Clear()
	{
		Items.Clear();
		Columns.Clear();
	}

	readonly DotNetObjectReference<ListView> dotNetObjectReference;

	protected override Size DefaultSize => new Size(121, 97);

	public ColumnHeaderStyle HeaderStyle { get; set; }

	public ColumnHeaderCollection Columns { get; } = new ColumnHeaderCollection();

	public BorderStyle BorderStyle
	{
		get => _borderStyle;
		set
		{
			if (UpdateProperty(ref _borderStyle, value))
			{
				UpdateStyles();
				UpdateBounds(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
			}
		}
	}

	internal override void AdjustWindowRectEx(ref Interop.RECT rect)
	{
		base.AdjustWindowRectEx(ref rect);

		if (BorderStyle == BorderStyle.FixedSingle)
		{
			rect = new Interop.RECT(rect.left - 1, rect.top - 1, rect.right + 1, rect.bottom + 1);
		}
		else if (BorderStyle == BorderStyle.Fixed3D)
		{
			rect = new Interop.RECT(rect.left - 2, rect.top - 2, rect.right + 2, rect.bottom + 2);
		}
	}

	bool gridLines;

	public bool GridLines
	{
		get => gridLines;
		set
		{
			UpdateProperty(ref gridLines, value);
		}
	}

	public bool FullRowSelect { get; set; }

	public bool MultiSelect { get; set; }

	public bool VirtualMode { get; set; }

	public int VirtualListSize { get; set; }

	public bool HideSelection { get; set; }

	public ImageList? LargeImageList { get; set; }
	public ImageList? SmallImageList { get; set; }

	public View View { get; set; }

	public bool UseCompatibleStateImageBehavior { get; set; }

	public ListViewItemCollection Items { get; }

	public ListViewItemCollection SelectedItems { get; }

	public void RedrawItems(int startIndex, int endIndex, bool invalidateOnly)
	{
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}

	protected internal virtual async Task OnResizerMouseDownAsync(ListViewDetailsColumnHeader.ResizerEventArgs args)
	{
		WebMouseEventArgs e = args.Args;
		ElementReference elementReference = args.Column;

		if (e.GetMouseButtons() == MouseButtons.Left && e.Detail == 1)
		{
			await (GetJSInterop<IListViewJSInterop>()?.ResizeColumnAsync(dotNetObjectReference, e, elementReference) ?? Task.CompletedTask);
		}
	}

	public event ItemDragEventHandler? ItemDrag;

	public event RetrieveVirtualItemEventHandler? RetrieveVirtualItem;

	// Raised when index of the focused selected item has been changed.
	// This event is useful when only single item can be selected (i.e. MultiSelect is set to false).
	public event EventHandler? SelectedIndexChanged;

	protected virtual void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
	{
		RetrieveVirtualItem?.Invoke(this, e);
	}

	public void OnSelectedIndexChanged(EventArgs e)
	{
		SelectedIndexChanged?.Invoke(this, e);
	}

	int ItemCount
	{
		get
		{
			return _itemCount;
		}
		set
		{
			_itemCount = value;
			NotifyRenderRequired();
		}
	}
	int _itemCount;

	public class ColumnHeaderCollection : WrappedList<ColumnHeader>
	{
		public virtual ColumnHeader Add(string? text)
		{
			var columnHeader = new ColumnHeader() { Text = text };
			Add(columnHeader);
			return columnHeader;
		}

		public virtual ColumnHeader Add(string? text, int width)
		{
			var columnHeader = new ColumnHeader() { Text = text, Width = width };
			Add(columnHeader);
			return columnHeader;
		}
	}

	public class ListViewItemCollection : WrappedList<ListViewItem>
	{
		readonly ListView owner;

		public ListViewItemCollection(ListView owner)
		{
			this.owner = owner;
		}

		public override ListViewItem this[int displayIndex]
		{
			get
			{
				if (owner.VirtualMode)
				{
					var rVI = new RetrieveVirtualItemEventArgs(displayIndex);
					owner.OnRetrieveVirtualItem(rVI);
					rVI.Item.SetItemIndex(owner);
					return rVI.Item;
				}
				else
				{
					if (displayIndex < 0 || displayIndex >= owner.ItemCount)
					{
						throw new ArgumentOutOfRangeException(nameof(displayIndex), displayIndex, string.Format(SR.InvalidArgument, nameof(displayIndex), displayIndex));
					}

					return inner[displayIndex];
				}
			}
			set
			{
				if (owner.VirtualMode)
				{
					throw new InvalidOperationException(SR.ListViewCantModifyTheItemCollInAVirtualListView);
				}

				if (displayIndex < 0 || displayIndex >= owner.ItemCount)
				{
					throw new ArgumentOutOfRangeException(nameof(displayIndex), displayIndex, string.Format(SR.InvalidArgument, nameof(displayIndex), displayIndex));
				}

				RemoveAt(displayIndex);
				Insert(displayIndex, value);
			}
		}

		public override int Count
		{
			get
			{
				if (owner.VirtualMode)
				{
					return owner.VirtualListSize;
				}
				else
				{
					return owner.ItemCount;
				}
			}
		}

		public override void Add(ListViewItem item)
		{
			base.Add(item);
			owner.ItemCount++;
		}

		public virtual ListViewItem Add(string text)
		{
			if (owner.VirtualMode)
			{
				throw new InvalidOperationException(SR.ListViewCantAddItemsToAVirtualListView);
			}
			else
			{
				var item = new ListViewItem(text);
				Add(item);
				return item;
			}
		}

		public override void RemoveAt(int index)
		{
			if (owner.VirtualMode)
			{
				throw new InvalidOperationException(SR.ListViewCantRemoveItemsFromAVirtualListView);
			}

			if (index < 0 || index >= owner.ItemCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
			}
			base.RemoveAt(index);
			owner.ItemCount--;
		}

		public override void Insert(int index, ListViewItem item)
		{
			base.Insert(index, item);
			owner.ItemCount++;
		}

		public override void Clear()
		{
			base.Clear();
			owner.ItemCount = 0;
		}
	}

	public sealed class HitTestInfo
	{
		public HitTestType type = HitTestType.None;

		public int row;
		public int col;

		/// <summary>
		///  Allows the <see cref='HitTestInfo'/> object to inform you the
		///  extent of the grid.
		/// </summary>
		public static readonly HitTestInfo Nowhere = new HitTestInfo();

		public HitTestInfo()
		{
			row = col = -1;
		}

		internal HitTestInfo(HitTestType type)
		{
			this.type = type;
			row = col = -1;
		}

		/// <summary>
		///  Gets the number of the clicked column.
		/// </summary>
		public int Column => col;

		/// <summary>
		///  Gets the
		///  number of the clicked row.
		/// </summary>
		public int Row => row;

		/// <summary>
		///  Gets the part of the <see cref='DataGrid'/> control, other than the row or column, that was
		///  clicked.
		/// </summary>
		public HitTestType Type => type;

		/// <summary>
		///  Indicates whether two objects are identical.
		/// </summary>
		public override bool Equals(object? value)
		{
			if (value != null && value is HitTestInfo ci)
			{
				return (type == ci.type &&
					   row == ci.row &&
					   col == ci.col);
			}
			return false;
		}

		/// <summary>
		///  Gets the hash code for the <see cref='HitTestInfo'/> instance.
		/// </summary>
		public override int GetHashCode() => HashCode.Combine(type, row, col);

		/// <summary>
		///  Gets the type, row number, and column number.
		/// </summary>
		public override string ToString()
		{
			return "{ " + ((type).ToString()) + "," + row.ToString(CultureInfo.InvariantCulture) + "," + col.ToString(CultureInfo.InvariantCulture) + "}";
		}
	}

	[Flags]
	public enum HitTestType
	{
		None = 0x00000000,
		Cell = 0x00000001,
		ColumnHeader = 0x00000002,
		RowHeader = 0x00000004,
		ColumnResize = 0x00000008,
		RowResize = 0x00000010,
		Caption = 0x00000020,
		ParentRows = 0x00000040
	}
}
