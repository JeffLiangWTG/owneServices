using System;
using Enterprise.Rating.GUI.RateSelection.BaseControls;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class ExpandCollapseToggle : ViewModelBasedControl
	{
		public ExpandCollapseToggle()
		{
			InitializeComponent();
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();
			RefreshExpandCollapse();
		}

		public void SetStates<T>(Func<T, bool> getState, Action<T, bool> setState)
		{
			GetState = o => getState((T)o);
			SetState = (o, val) => setState((T)o, val);

			RefreshExpandCollapse();
		}

		Func<object, bool> GetState;

		Action<object, bool> SetState;

		void RefreshExpandCollapse()
		{
			if (BindingSource.Current is null)
			{
				return;
			}

			var isExpanded = GetState(BindingSource.Current);

			if (isExpanded)
			{
				iconPictureBox.Image = Properties.Resources.CollapseDrawing;
			}
			else
			{
				iconPictureBox.Image = Properties.Resources.ExpandDrawing;
			}
		}

		void zPictureBox1_Click(object sender, EventArgs e)
		{
			var newVal = !GetState(BindingSource.Current);
			SetState(BindingSource.Current, newVal);
		}
	}
}
