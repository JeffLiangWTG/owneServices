using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Views;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class PenaltiesControl : TemplateBasedControl
	{
		public PenaltiesControl()
		{
			InitializeComponent();
			lblTitle.Text = ResStrings.Penalties;
			lblProcess.Text = ResStrings.Process;
			lblType.Text = ResStrings.Type;
			lblName.Text = ResStrings.Name;
			lblFreeTime.Text = ResStrings.FreeTime;
			lblUnit.Text = ResStrings.Unit;
			lblCostPUnit.Text = ResStrings.CostPerUnit;
			lblCurrency.Text = ResStrings.Currency;

#if DEBUG
			TypeDescriptor.AddAttributes(lblFreeTime, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCostPUnit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCurrency, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblProcess, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTitle, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblType, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblUnit, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new PenaltiesItemTemplate();
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return Data?.Penalties;
		}

		BookingInfoViewModel Data => CurrentDataItem as BookingInfoViewModel;
	}
}
