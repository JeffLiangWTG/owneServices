using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateTransportProviderForm : ZTemplateForm
	{
		public RateTransportProviderForm(RateTransportProvider rateTransportProvider)
			: base(rateTransportProvider)
		{
			ZoneItemGrid.TransportProvider = rateTransportProvider;
			SetDeliveryDueDateVisibility();
		}

		public RateTransportProviderForm()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void SetDeliveryDueDateVisibility()
		{
			DefaultDeliveryDueTime.Visible = FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive;
			DefaultHoldForPickupTime.Visible = FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive;
		}

		#region Bind

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				TP_OH_RelatedPartyFindBox.DataBindings.RemoveBinding("IsVisibleForBinding");
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				TP_OH_RelatedPartyFindBox.DataBindings.Add(new KBinding("IsVisibleForBinding", DataSource, "IsZoneSetOwnerFieldVisible"));
			}
		}

		void ZoneItemGrid_AfterBind(object sender, System.EventArgs e)
		{
			ZoneItemGrid.ListManager.PositionChanged += ListManager_PositionChanged;
		}

		protected override void PerformValidation()
		{
			ZoneItemGrid.TransportProvider.Validation.ValidateTP_ZoneMode();
		}

		RateTransportZoneItem lastZoneItem;

		void ListManager_PositionChanged(object sender, System.EventArgs e)
		{
			var currentZoneItem = ZoneItemGrid.ListManager.GetCurrent() as RateTransportZoneItem;
			if (currentZoneItem != lastZoneItem)
			{
				if (currentZoneItem != null)
				{
					if (!currentZoneItem.Lookups.CityTowns.AreEventsSubscribed)
					{
						currentZoneItem.Lookups.CityTowns.Busy += Collection_Busy;
						currentZoneItem.Lookups.CityTowns.Completed += Collection_Completed;
						currentZoneItem.Lookups.CityTowns.SelectionNeeded += Collection_SelectionNeeded;
						currentZoneItem.Lookups.CityTowns.AreEventsSubscribed = true;
					}
					if (!currentZoneItem.Lookups.PostCodes.AreEventsSubscribed)
					{
						currentZoneItem.Lookups.PostCodes.Busy += Collection_Busy;
						currentZoneItem.Lookups.PostCodes.Completed += Collection_Completed;
						currentZoneItem.Lookups.PostCodes.AreEventsSubscribed = true;
					}
				}

				lastZoneItem = currentZoneItem;
			}
		}

		void Collection_SelectionNeeded(object sender, SelectionNeededEventArgs e)
		{
			CurrentZoneItemEdittingFindBox?.SelectPK(e);
		}

		internal CityTownFindBox CurrentZoneItemEdittingFindBox
		{
			get
			{
				CityTownFindBox result = null;
				var columnStyle = ZoneItemGrid.TableStyles[0].GridColumnStyles[ZoneItemGrid.CurrentCell.ColumnNumber];
				if (columnStyle is ZMultiControlColumnStyle multiControlStyle)
				{
					if (multiControlStyle.EditControl.Controls.Count > 0)
					{
						result = multiControlStyle.EditControl.Controls[0] as CityTownFindBox;
					}
				}

				return result;
			}
		}

		void Collection_Completed(object sender, System.EventArgs e)
		{
			ProgressForm.Hide();
		}

		ProgressForm progressForm;

		ProgressForm ProgressForm
		{
			get
			{
				if (progressForm == null)
				{
					progressForm = new ProgressForm();
					progressForm.ShowCancelButton = false;
					progressForm.ShowInTaskbar = false;
					progressForm.ShowProgressBar = false;
				}
				return progressForm;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Collection_Busy(object sender, BusyEventArgs e)
		{
			ProgressForm.Status = e.Message;
			if (!ProgressForm.Visible)
			{
				ProgressForm.ShowModalTo(this);
				Application.DoEvents();
			}
		}
		#endregion

		#region Dispose

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
			if (progressForm != null && !progressForm.IsDisposed)
			{
				progressForm.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

