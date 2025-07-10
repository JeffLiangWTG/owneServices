using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateChooser.Converters;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class ChargesChargeGroupTemplate : TemplateBasedControl, IItemTemplateControl
	{
		public ChargesChargeGroupTemplate()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(lblChargeGroup, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotal, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return Data?.Charges;
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (Data != null)
			{
				var totalConverter = new TotalConverter();
				lblTotal.Text = totalConverter.Convert(Data.Charges, Data.Parent);
			}
		}

		protected override Panel GetContainerPanel()
		{
			return pnlContainer;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new ChargesItemTemplate();
		}

		#region IItemTemplateControl
		public void DataBind(object data)
		{
			if (data is ChargesViewModel.ChargeGroupView viewModel)
			{
				BindingSource.DataSource = viewModel;
			}
		}

		public void ClearSelection()
		{
		}

		public bool IsSelected { get; set; }
		public EventHandler SelectionChanged { get; set; }
		#endregion

		ChargesViewModel.ChargeGroupView Data => CurrentDataItem as ChargesViewModel.ChargeGroupView;
	}
}
