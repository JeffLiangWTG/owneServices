using System.Collections;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class Menu
{
	public class MenuItemCollection : WrappedList<MenuItem>
	{
		readonly Menu owner;

		///  A caching mechanism for key accessor
		///  We use an index here rather than control so that we don't have lifetime
		///  issues by holding on to extra references.
		int lastAccessedIndex = -1;

		public MenuItemCollection(Menu owner)
		{
			this.owner = owner;
		}

		/// <summary>
		///  Searches for Controls by their Name property, builds up an array
		///  of all the controls that match.
		/// </summary>
		public MenuItem[] Find(string key, bool searchAllChildren)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			var foundMenuItems = FindInternal(key, searchAllChildren, this, new ArrayList());

			// Make this a strongly typed collection.
			var stronglyTypedfoundMenuItems = new MenuItem[foundMenuItems.Count];
			foundMenuItems.CopyTo(stronglyTypedfoundMenuItems, 0);

			return stronglyTypedfoundMenuItems;
		}

		/// <summary>
		///  Searches for Controls by their Name property, builds up an array list
		///  of all the controls that match.
		/// </summary>
		ArrayList FindInternal(string key, bool searchAllChildren, MenuItemCollection menuItemsToLookIn, ArrayList foundMenuItems)
		{
			if ((menuItemsToLookIn == null) || (foundMenuItems == null))
			{
				return new ArrayList();
			}

			// Perform breadth first search - as it's likely people will want controls belonging
			// to the same parent close to each other.

			for (var i = 0; i < menuItemsToLookIn.Count; i++)
			{
				if (menuItemsToLookIn[i] == null)
				{
					continue;
				}

				if (WindowsFormsUtils.SafeCompareStrings(menuItemsToLookIn[i].Name, key, /* ignoreCase = */ true))
				{
					foundMenuItems.Add(menuItemsToLookIn[i]);
				}
			}

			// Optional recursive search for controls in child collections.

			if (searchAllChildren)
			{
				for (var i = 0; i < menuItemsToLookIn.Count; i++)
				{
					if (menuItemsToLookIn[i] == null)
					{
						continue;
					}
					if ((menuItemsToLookIn[i].MenuItems != null) && menuItemsToLookIn[i].MenuItems.Count > 0)
					{
						// if it has a valid child collecion, append those results to our collection
						foundMenuItems = FindInternal(key, searchAllChildren, menuItemsToLookIn[i].MenuItems, foundMenuItems);
					}
				}
			}
			return foundMenuItems;
		}

		public virtual MenuItem? this[string key] => this.Find(key, searchAllChildren: false).SingleOrDefault();

		/// <summary>
		///  Returns true if the collection contains an item with the specified key, false otherwise.
		/// </summary>
		public virtual bool ContainsKey(string key) => IsValidIndex(IndexOfKey(key));

		/// <summary>
		///  The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.
		/// </summary>
		public virtual int IndexOfKey(string key)
		{
			// Step 0 - Arg validation
			if (string.IsNullOrEmpty(key))
			{
				return -1; // we dont support empty or null keys.
			}

			// step 1 - check the last cached item
			if (IsValidIndex(lastAccessedIndex))
			{
				if (WindowsFormsUtils.SafeCompareStrings(this[lastAccessedIndex].Name, key, /* ignoreCase = */ true))
				{
					return lastAccessedIndex;
				}
			}

			// step 2 - search for the item
			for (var i = 0; i < Count; i++)
			{
				if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, /* ignoreCase = */ true))
				{
					lastAccessedIndex = i;
					return i;
				}
			}

			// step 3 - we didn't find it.  Invalidate the last accessed index and return -1.
			lastAccessedIndex = -1;
			return -1;
		}

		protected override void OnAdd(MenuItem menuItem, int index)
		{
			if (menuItem.Parent != this.owner)
			{
				menuItem.Parent?.MenuItems.Remove(menuItem);
				menuItem.Parent = this.owner;
			}

			this.owner.OnItemAdded(new MenuItemEventArgs(menuItem));
		}

		protected override void OnRemove(MenuItem menuItem)
		{
			this.owner.OnItemRemoved(new MenuItemEventArgs(menuItem));
			menuItem.Parent = null;
		}

		public virtual int Add(int index, MenuItem item)
		{
			if (item is null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			// MenuItems can only belong to one menu at a time
			if (item.Parent is not null)
			{
				if (owner is MenuItem parent)
				{
					while (parent is MenuItem)
					{
						if (parent.Equals(item))
						{
							throw new ArgumentException(string.Format(SR.MenuItemAlreadyExists, item.Text), nameof(item));
						}

						if (parent.Parent is MenuItem)
						{
							parent = (MenuItem)parent.Parent;
						}
						else
						{
							break;
						}
					}
				}

				if (item.Parent.Equals(owner) && index > 0)
				{
					index--;
				}

				item.Parent.MenuItems.Remove(item);
			}

			Insert(index, item);
			return index;
		}

		public new int Add(MenuItem menuItem)
		{
			return Add(Count, menuItem);
		}

		[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1100:DoNotUseMenuItemOrKMenuItem", Justification = "Baseline")]
		public MenuItem Add(string caption)
		{
			var menuItem = new MenuItem(caption);
			Add(menuItem);
			return menuItem;
		}

		[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1100:DoNotUseMenuItemOrKMenuItem", Justification = "Baseline")]
		public MenuItem Add(string caption, EventHandler? onClick)
		{
			var menuItem = new MenuItem(caption, onClick);
			Add(menuItem);
			return menuItem;
		}

		[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1100:DoNotUseMenuItemOrKMenuItem", Justification = "Baseline")]
		public MenuItem Add(string caption, MenuItem[] items)
		{
			var menuItem = new MenuItem(caption);
			foreach (var item in items)
			{
				menuItem.MenuItems.Add(item);
			}
			Add(menuItem);
			return menuItem;
		}

		public virtual void AddRange(MenuItem[] items)
		{
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}
			foreach (MenuItem item in items)
			{
				Add(item);
			}
		}

		public virtual void RemoveByKey(string key)
		{
			var item = this[key];
			if (item is null)
			{
				return;
			}
			Remove(item);
		}

		/// <summary>
		///  Determines if the index is valid for the collection.
		/// </summary>
		bool IsValidIndex(int index)
		{
			return (index >= 0) && (index < Count);
		}
	}
}
