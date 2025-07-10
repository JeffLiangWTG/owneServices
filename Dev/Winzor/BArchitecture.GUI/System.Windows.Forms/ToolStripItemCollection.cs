using System.Drawing;
using WinzorFramework;

namespace System.Windows.Forms;

public class ToolStripItemCollection : WrappedList<ToolStripItem>, IList<ToolStripItem>
{
	readonly ToolStrip owner;
	readonly bool itemsCollection;
	readonly bool isReadOnly;

	internal ToolStripItemCollection(ToolStrip owner, bool itemsCollection)
		: this(owner, itemsCollection, false)
	{
	}

	internal ToolStripItemCollection(ToolStrip owner, bool itemsCollection, bool isReadOnly)
	{
		this.owner = owner;
		this.itemsCollection = itemsCollection;
		this.isReadOnly = isReadOnly;
	}

	public ToolStripItem[] Find(string key, bool searchAllChildren)
	{
		var matches = this.Where(t => t.Name == key);
		if (searchAllChildren)
		{
			matches = matches.Concat(this.OfType<ToolStripDropDownItem>().SelectMany(t => t.DropDownItems.Find(key, searchAllChildren)));
		}
		return matches.ToArray();
	}

	public new ToolStripItem this[int index] { get => base[index]; set => base[index] = value; }

	public virtual ToolStripItem? this[string? key]
	{
		get
		{
			if (key == null || key.Length == 0)
			{
				return null;
			}

			int index = IndexOfKey(key);
			if (IsValidIndex(index))
			{
				return base[index];
			}

			return null;
		}
	}

	ToolStripItem IList<ToolStripItem>.this[int index] { get => this[index]; set => this[index] = value; }

	public virtual int IndexOfKey(string key)
	{
		var item = this.SingleOrDefault(t => t.Name == key);
		if (item is null)
		{
			return -1;
		}
		return IndexOf(item);
	}

	public virtual bool ContainsKey(string key) => this.Any(t => t.Name == key);

	public virtual void RemoveByKey(string key)
	{
		var item = this.SingleOrDefault(t => t.Name == key);
		if (item is null)
		{
			return;
		}
		Remove(item);
	}

	public ToolStripItem Add(string? text)
	{
		return Add(text, null, null);
	}

	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Align with expected behaviour for testing DocEngineDynamicToolStripMenuItemProviderTest")]
	public ToolStripItem Add(string? text, Image? image, EventHandler? onClick)
	{
		var isOwnerToolStripMenuItem = (owner as ToolStripDropDown)?.OwnerItem is ToolStripMenuItem;
		ToolStripItem item = isOwnerToolStripMenuItem ? new ToolStripMenuItem(text ?? string.Empty, image, onClick, null) : owner.CreateDefaultItem(text, image, onClick);
		Add(item);
		return item;
	}

	protected override void OnAdd(ToolStripItem item, int index)
	{
		SetOwner(item);
		item.SetParent(owner);

		if (itemsCollection && owner is not null)
		{
			owner.OnItemAdded(new ToolStripItemEventArgs(item));
		}
	}

	protected override void OnRemove(ToolStripItem item)
	{
		if (itemsCollection)
		{
			if (item is not null)
			{
				item.SetOwner(null);
			}
		}
	}

	void SetOwner(ToolStripItem item)
	{
		if (itemsCollection)
		{
			if (item is not null)
			{
				if (item.Owner is not null)
				{
					item.Owner.Items.Remove(item);
				}

				item.SetOwner(owner);
			}
		}
	}

	void ICollection<ToolStripItem>.Add(ToolStripItem item) => Add(item);

	bool ICollection<ToolStripItem>.Contains(ToolStripItem item) => Contains(item);

	void ICollection<ToolStripItem>.CopyTo(ToolStripItem[] array, int arrayIndex) => CopyTo(array, arrayIndex);

	IEnumerator<ToolStripItem> IEnumerable<ToolStripItem>.GetEnumerator() => inner.Cast<ToolStripItem>().GetEnumerator();

	int IList<ToolStripItem>.IndexOf(ToolStripItem item) => IndexOf(item);

	void IList<ToolStripItem>.Insert(int index, ToolStripItem item) => Insert(index, item);

	bool ICollection<ToolStripItem>.Remove(ToolStripItem item) => ((ICollection<Control>)this).Remove(item);

	bool IsValidIndex(int index)
	{
		if (index >= 0)
		{
			return index < Count;
		}

		return false;
	}

	public override bool IsReadOnly => isReadOnly;

	public override void Clear()
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException();
		}

		if (Count == 0)
		{
			return;
		}

		ToolStripOverflow? overflow = null;

		if (owner is not null)
		{
			owner.SuspendLayout();
			overflow = owner.GetOverflow();
			overflow?.SuspendLayout();
		}

		try
		{
			while (Count != 0)
			{
				RemoveAt(Count - 1);
			}
		}
		finally
		{
			overflow?.ResumeLayout(false);

			if (owner is not null)
			{
				owner.ResumeLayout();
			}
		}
	}
}
