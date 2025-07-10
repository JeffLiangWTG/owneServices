using System.Windows.Forms.Layout;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class Control
{
	public class WinzorSpecificControlsCollection : WrappedList<Control>
	{
		public event Action<Control>? AfterRemove;

		public WinzorSpecificControlsCollection(Control owner)
		{
			this.owner = owner;
		}

		public override void Add(Control item)
		{
			if (item is null || item.Parent == owner)
			{
				return;
			}

			if (item.Parent is not null)
			{
				item.Parent.RemoveControl(item);
			}

			base.Add(item);
		}

		protected override void OnAdd(Control item, int index)
		{
			item.IsWinzorSpecific = true;
			item.AssignParent(owner);
			owner.NotifyRenderRequired();
		}

		protected override void OnRemove(Control item)
		{
			item.IsWinzorSpecific = false;
			item.AssignParent(null);
			owner.NotifyRenderRequired();
			AfterRemove?.Invoke(item);
		}

		readonly Control owner;
	}
}
