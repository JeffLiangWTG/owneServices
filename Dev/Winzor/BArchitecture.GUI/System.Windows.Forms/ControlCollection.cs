using System.Windows.Forms.Layout;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class Control
{
	public class ControlCollection : WrappedList<Control>
	{
		public ControlCollection(Control owner)
		{
			Owner = owner;
		}

		public Control[] Find(string? key, bool searchAllChildren)
		{
			var matches = this.Where(t => t.Name != null && t.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
			if (searchAllChildren)
			{
				matches = matches.Concat(this.SelectMany(t => t.Controls.Find(key, searchAllChildren)));
			}
			return matches.ToArray();
		}

		public virtual Control? this[string? key] => Find(key, searchAllChildren: false).SingleOrDefault();

		public virtual bool ContainsKey(string key) => Find(key, searchAllChildren: false).Any();

		public virtual void RemoveByKey(string key) => Remove(this[key]);

		public override void Clear()
		{
			foreach (var item in Owner.Children)
			{
				Remove(item);
			}
			base.Clear();
		}

		public override void Add(Control value)
		{
			if (value is null)
			{
				return;
			}

			if (value.Parent == Owner)
			{
				value.SendToBack();
				return;
			}

			if (value.Parent is not null)
			{
				value.Parent.RemoveControl(value);
			}

			base.Add(value);

			if (value.tabIndex == -1)
			{
				// Find the next highest tab index
				int nextTabIndex = 0;
				for (int c = 0; c < (Count - 1); c++)
				{
					int t = this[c].TabIndex;
					if (nextTabIndex <= t)
					{
						nextTabIndex = t + 1;
					}
				}
				value.tabIndex = nextTabIndex;
			}

			Owner.SuspendLayout();
			try
			{
				var oldParent = value.Parent;
				try
				{
					value.AssignParent(Owner);
				}
				finally
				{
					Form? form;
					if (oldParent != value.Parent && value.Parent is not null && value.Parent.Created && (form = value.FindForm()) != null && form.Visible)
					{
						value.CreateControl();
					}
				}
				value.InitLayout();
			}
			finally
			{
				Owner.ResumeLayout(false);
			}

			LayoutTransaction.DoLayout(Owner, value, PropertyNames.Parent);
			Owner.OnControlAdded(new ControlEventArgs(value));
			Owner.NotifyRenderRequired();
		}

		public override void AddRange(IEnumerable<Control> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			Owner.SuspendLayout();

			try
			{
				base.AddRange(items);
			}
			finally
			{
				Owner.ResumeLayout();
			}
		}

		public override void Remove(Control? value)
		{
			if (value is null)
			{
				return;
			}

			if (value.Parent == Owner)
			{
				if (value.IsHandleCreated && value.Focused)
				{
					// In WinForms, when parking the handle of the control the focus will move to the parent.
					Owner.Focus();
				}

				base.Remove(value);
				value.AssignParent(null);
				LayoutTransaction.DoLayout(Owner, value, PropertyNames.Parent);
				Owner.OnControlRemoved(new ControlEventArgs(value));

				if (Owner.GetContainerControl() is ContainerControl cc)
				{
					cc.AfterControlRemoved(value, Owner);
				}

				Owner.NotifyRenderRequired();
			}
		}

		public virtual void SetChildIndex(Control child, int newIndex)
		{
			if (child == null)
			{
				throw new ArgumentNullException(nameof(child));
			}

			int currentIndex = GetChildIndex(child);

			if (currentIndex == newIndex)
			{
				return;
			}

			if (newIndex >= Count || newIndex == -1)
			{
				newIndex = Count - 1;
			}

			Move(child, currentIndex, newIndex);

			LayoutTransaction.DoLayout(Owner, child, PropertyNames.ChildIndex);
		}

		public int GetChildIndex(Control child)
		{
			return GetChildIndex(child, true);
		}

		public virtual int GetChildIndex(Control child, bool throwException)
		{
			int index = IndexOf(child);
			if (index == -1 && throwException)
			{
				throw new ArgumentException($"{nameof(child)} is not a chld control of this parent");
			}
			return index;
		}

		public Control Owner { get; }
	}
}
