using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EventsBannerControl : ZUserControl
	{
		public EventsBannerControl()
		{
			InitializeComponent();
			Load += Control_Load;
			EventsTableLayout.ControlRemoved += EventItemsRemoved;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.PropertyChanged -= EventBannerModel_PropertyChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				DataSource.PropertyChanged += EventBannerModel_PropertyChanged;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (EventsTableLayout != null)
				{
					EventsTableLayout.Controls.RemoveAndDisposeAll();
					EventsTableLayout.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected new EventsBannerModel DataSource => BindingSource.Current as EventsBannerModel;

		async void Control_Load(object sender, EventArgs e)
		{
			await DataSource.LoadCreditEventsOrSilentCompanyLookup();
		}

		void EventItemsRemoved(object sender, ControlEventArgs e)
		{
			var sourceControl = e.Control;
			if (sourceControl != null && !sourceControl.IsDisposed)
			{
				sourceControl.Controls.RemoveAndDisposeAll();
				sourceControl.Dispose();
			}
		}

		void LoadEventsItems(EventsBannerModel eventBannerModel)
		{
			if (!eventBannerModel.ErrorOccur)
			{
				if (EventsTableLayout.Controls.Count > 1)
				{
					EventsTableLayout.Visible = false;
					var events = eventBannerModel.Events.ToList();
					var currentEventsBannerItemVisible = false;
					foreach (var control in EventsTableLayout.Controls.Cast<ZUserControl>())
					{
						if (control is EventsBannerItemControl bannerItemControl)
						{
							var date = bannerItemControl.eventDateLabel.Text;
							var type = bannerItemControl.eventTypeLabel.Text;
							if (!events.Any(o => o.EventDate.Equals(date) && o.EventDescription.Equals(type)))
							{
								control.Visible = false;
								currentEventsBannerItemVisible = false;
							}
							else
							{
								control.Visible = true;
								currentEventsBannerItemVisible = true;
							}
						}
						else if (control is ReportItemSplitter splitter)
						{
							splitter.Visible = currentEventsBannerItemVisible;
						}
					}

					EventsTableLayout.Visible = true;
				}
				else
				{
					foreach (var eventBannerItem in eventBannerModel.Events)
					{
						var eventsBannerItem = new EventsBannerItemControl();
						eventsBannerItem.SetDataBinding(eventBannerItem, string.Empty);
						eventsBannerItem.eventTipPictureBox.Image = eventBannerItem.EventIcon;
						var splitter = new ReportItemSplitter();
						EventsTableLayout.Controls.Add(eventsBannerItem);
						EventsTableLayout.Controls.Add(splitter);
					}

					if (!IsDisposed)
					{
						eventListPanel.Controls.Add(EventsTableLayout);
					}
					else
					{
						EventsTableLayout.Controls.RemoveAndDisposeAll();
						EventsTableLayout.Dispose();
					}
				}
			}
		}

		void EventBannerModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(DataSource.IsLoading))
			{
				eventListUnavailableMessageControl.SetLoading(DataSource.IsLoading);
			}

			if (e.PropertyName == nameof(DataSource.Events))
			{
				LoadEventsItems(DataSource);
			}
		}
	}
}
