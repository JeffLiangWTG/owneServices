using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CostingRateLineAndItemsControl : BaseRateLinesAndItemsControl
	{
		#region Accept Related Rate Line

		public CostingRateLineAndItemsControl()
		{
			InitializeComponent();
			RateLinesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("e4c1c236-c80d-4385-ab6f-17aa7ff89e8f", "Accept"), new EventHandler(AcceptRelatedLine)));
		}

		void AcceptRelatedLine(object sender, EventArgs e)
		{
			if (RateLinesGrid.SelectedElements.Length > 0)
			{
				RateEntry entry = GetActiveEntry();
				if (entry != null)
				{
					if (entry.Parent.IsAdditionalTariff())
					{
						Globals.Message.Show(Res.GetString("5e32207b-fc51-4e91-ac6f-d2c1f8bca6db", "You cannot accept a costing into a discounted Company Tariff. You should right-click on the charge you wish to change, and click 'Override'."));
						return;
					}

					foreach (var o in RateLinesGrid.SelectedElements)
					{
						var line = (RelatedRateLine)o;
						InsertRelatedRateLine(line, entry);
					}
				}
			}
		}

		RateEntry GetActiveEntry()
		{
			for (Control parentCtrl = Parent; parentCtrl != null; parentCtrl = parentCtrl.Parent)
			{
				var parentPage = parentCtrl as EntryTabPage;
				if (parentPage != null)
				{
					var linesCollection = parentPage.RateLinesAndItemsControl.RateLinesCollection;
					if (linesCollection != null)
					{
						return linesCollection.Master;
					}
					break;
				}
			}

			return null;
		}

		#endregion

		#region Events

		protected override void HookEvents()
		{
			if (Header != null)
			{
				Header.SelectedRateLineChanged += OnSelectedRateLineChanged;
			}

			base.HookEvents();
		}

		protected override void UnhookEvents()
		{
			if (Header != null)
			{
				Header.SelectedRateLineChanged -= OnSelectedRateLineChanged;
			}

			base.UnhookEvents();
		}

		protected override void OnCurrentChanged(object sender, EventArgs e)
		{
		}

		void OnSelectedRateLineChanged(RateEntry sender, int index)
		{
			if (RelatedRateLines != null)
			{
				RateEntry entry = RelatedRateLines.Master;
				if (sender == entry)
				{
					RateLinesGrid.CurrentRowIndex = index;
				}
			}
		}

		#endregion

		#region ColourDeciding

		void RateLinesGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			RelatedRateLine line = e.ObjectAtRow as RelatedRateLine;
			if (line != null)
			{
				e.Colour = line.IsSelectedLineChargeCode ? Color.LightBlue : Color.Empty;
			}
		}

		#endregion

		#region IDisposable Members

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
