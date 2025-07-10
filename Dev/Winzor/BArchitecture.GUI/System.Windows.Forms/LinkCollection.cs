using System.Collections;
using System.Windows.Forms.Layout;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class LinkLabel
{
	public class LinkCollection : WrappedList<Link>
	{
		public LinkCollection(LinkLabel owner)
		{
			this.owner = owner;
		}

		readonly LinkLabel owner;

		bool linksAdded;   //whether we should serialize the linkCollection

		/// <summary>
		///  whether we have added a non-trivial link to the collection
		/// </summary>
		public bool LinksAdded
		{
			get
			{
				return linksAdded;
			}
		}

		public Link Add(int start, int length)
		{
			return Add(start, length, null);
		}

		public Link Add(int start, int length, object? linkData)
		{
			if (owner.LinkAreaIsDefault)
			{
				inner = inner.Clear();
				owner.FocusLink = null;
			}

			var link = new Link(owner)
			{
				Start = start,
				Length = length,
				LinkData = linkData
			};
			Add(link);

			return link;
		}

		public new int Add(Link value)
		{
			if (value.Length != 0)
			{
				linksAdded = true;
			}
			// check for the special case where the list is in the "magic"
			// state of having only the default link in it. In that case
			// we want to clear the list before adding this link.
			if (owner.Links.Count == 1 && this[0].Start == 0 && this[0].Length == -1)
			{
				owner.Links.Clear();
				owner.FocusLink = null;
			}

			// Set the owner control for this link
			value.Owner = owner;

			base.Add(value);

			if (owner.AutoSize)
			{
				LayoutTransaction.DoLayout(owner.Parent, owner, PropertyNames.Links);
				owner.AdjustSize();
				owner.Invalidate();
			}

			if (owner.Links.Count > 1)
			{
				var sortedList = inner.OrderBy(t => t.Start).ToList();
				inner = inner.Clear();
				inner = inner.AddRange(sortedList);
			}

			owner.ValidateNoOverlappingLinks();
			owner.UpdateSelectability();
			owner.Invalidate();

			if (owner.Links.Count > 1)
			{
				return IndexOf(value);
			}
			return 0;
		}

		protected override void OnAdd(Link item, int index)
		{
			item.Owner = owner;
		}

		/// <summary>
		///  Returns true if the collection contains an item with the specified key, false otherwise.
		/// </summary>
		public virtual bool ContainsKey(string? key)
		{
			return IsValidIndex(IndexOfKey(key));
		}

		///  A caching mechanism for key accessor
		///  We use an index here rather than control so that we don't have lifetime
		///  issues by holding on to extra references.
		///  Note this is not Thread Safe - but WinForms has to be run in a STA anyways.
		int lastAccessedIndex = -1;

		/// <summary>
		///  The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.
		/// </summary>
		public virtual int IndexOfKey(string? key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return -1;
			}

			if (IsValidIndex(lastAccessedIndex))
			{
				if (WindowsFormsUtils.SafeCompareStrings(this[lastAccessedIndex].Name, key, ignoreCase: true))
				{
					return lastAccessedIndex;
				}
			}

			for (var i = 0; i < Count; i++)
			{
				if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, ignoreCase: true))
				{
					lastAccessedIndex = i;
					return i;
				}
			}

			lastAccessedIndex = -1;
			return -1;
		}

		/// <summary>
		///  Remove all links from the linkLabel.
		/// </summary>
		public override void Clear()
		{
			var doLayout = Count > 0 && owner.AutoSize;
			base.Clear();

			if (doLayout)
			{
				LayoutTransaction.DoLayout(owner.Parent, owner, PropertyNames.Links);
				owner.AdjustSize();
				owner.Invalidate();
			}

			owner.UpdateSelectability();
			owner.Invalidate();
		}

		public override void Remove(Link value)
		{
			if (value.Owner != owner)
			{
				return;
			}

			base.Remove(value);

			if (owner.AutoSize)
			{
				LayoutTransaction.DoLayout(owner.Parent, owner, PropertyNames.Links);
				owner.AdjustSize();
				owner.Invalidate();
			}

			if (owner.Links.Count > 1)
			{
				var sortedList = inner.OrderBy(t => t.Start).ToList();
				inner = inner.Clear();
				inner = inner.AddRange(sortedList);
			}

			owner.ValidateNoOverlappingLinks();
			owner.UpdateSelectability();
			owner.Invalidate();

			if (owner.FocusLink is null && owner.Links.Count > 0)
			{
				owner.FocusLink = owner.Links[0];
			}
		}

		public override void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		/// <summary>
		///  Removes the child control with the specified key.
		/// </summary>
		public virtual void RemoveByKey(string? key)
		{
			var index = IndexOfKey(key);
			if (IsValidIndex(index))
			{
				RemoveAt(index);
			}
		}

		/// <summary>
		///  Determines if the index is valid for the collection.
		/// </summary>
		bool IsValidIndex(int index)
		{
			return ((index >= 0) && (index < Count));
		}
	}
}
