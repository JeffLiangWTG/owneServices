using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class SortOptionsControl : ZUserControl
	{
		public SortOptionsControl()
		{
			InitializeComponent();

			BindingSource.DataSourceChanged += BindingSource_DataSourceChanged;
		}

		void BindingSource_DataSourceChanged(object sender, EventArgs e)
		{
			if (CurrentViewModel is null)
			{
				return;
			}

			ConstructButtons();
		}

		void ConstructButtons()
		{
			var newButtons = CurrentViewModel.SupportedSortOptions.Select(NewButton).ToArray();

			buttonsFlowLayoutPanel.SuspendLayout();

			var controlsToRemove = buttonsFlowLayoutPanel.Controls.Cast<Control>().Where(c => c != sortOptionsLabel).ToArray();
			foreach (var control in controlsToRemove)
			{
				buttonsFlowLayoutPanel.Controls.Remove(control);
			}

			buttonsFlowLayoutPanel.Controls.AddRange(newButtons);
			buttonsFlowLayoutPanel.ResumeLayout();
		}

		ZButton NewButton(SortOptionViewModel vm)
		{
			var btn = new ZButton();
			btn.Name = $"{vm.PropertyName}Button";
			btn.AutoSize = true;

			btn.DataBindings.Add(nameof(btn.Text), vm, nameof(vm.DisplayNameWithDirection));

			btn.Click += (sender, e) => ButtonClicked(vm);
			return btn;
		}

		void ButtonClicked(SortOptionViewModel vm)
		{
			foreach (var x in CurrentViewModel.SupportedSortOptions)
			{
				if (x == vm)
				{
					if (x.IsSelected)
					{
						x.Direction = x.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
					}
					else
					{
						x.IsSelected = true;
					}
				}
				else
				{
					x.IsSelected = false;
				}
			}

			CurrentViewModel.ReSort();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (BindingSource != null)
				{
					BindingSource.DataSourceChanged -= BindingSource_DataSourceChanged;
				}

				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		SortableRatesViewModel CurrentViewModel => (SortableRatesViewModel)BindingSource.Current;
	}
}
