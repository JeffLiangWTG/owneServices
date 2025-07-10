using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class ClientControl : ZUserControl
	{
		public ClientControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (QuotedBooking != null)
			{
				QuotedBooking.JobCreating -= new EventHandler(OnJobCreating);
				QuotedBooking.JobDeleting -= new EventHandler(OnJobDeleting);
				QuotedBooking.JobCreated -= new EventHandler(OnJobCreated);
				QuotedBooking.CrossTradeChanged -= new EventHandler(OnCrossTradeChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (QuotedBooking != null)
			{
				LoadOrCreateJob();

				QuotedBooking.JobCreating += new EventHandler(OnJobCreating);
				QuotedBooking.JobDeleting += new EventHandler(OnJobDeleting);
				QuotedBooking.JobCreated += new EventHandler(OnJobCreated);
				QuotedBooking.CrossTradeChanged += new EventHandler(OnCrossTradeChanged);

				if (ShouldChangeCaptionOnCrossTrade)
				{
					ClientOrgControl.Text = QuotedBooking.PrepaidBillToPartyText;
					ClientOrgControl.CaptionResourceString = QuotedBooking.PrepaidBillToPartyCaption;
					QuotedBooking.PrevIsCrossTrade = true;
				}
			}
		}

		QuotedBooking QuotedBooking
		{
			get { return (QuotedBooking)CurrentDataItem; }
		}

		QuotedBookingForm ParentQuotedBookingForm
		{
			get { return ParentForm as QuotedBookingForm; }
		}

		#endregion

		#region Job Related

		void LoadOrCreateJob()
		{
			ZString jobCreationLoadError = "";

			IAccounting accounting = ObjectFactory.Get<IAccounting>();

			if (QuotedBooking.QuotedBookingTypeCode == QuotedBooking.QuickBookingCode && !accounting.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(QuotedBooking))
			{
				if (QuotedBooking.Job == null)
				{
					var jobLoader = new JobHeader.Loader(QuotedBooking);
					var hasInactiveJobHeader = jobLoader.HasInactiveJobHeader(GlbCompany.CurrentCompany);
					if (hasInactiveJobHeader)
					{
						jobCreationLoadError = Res.GetString("A8E7901E-214B-48D3-A215-75B28698530F", @"The Job Invoicing Record has been created but is currently not active. 
Please click on ‘Billing’ tab or ‘Job Invoicing’ menu to activate the job.");
					}
					else
					{
						jobCreationLoadError = Res.GetString("17fb6cff-4e7e-46d3-a80a-32efcef42cb2", "The registry item [{0}] has been set so that billing jobs will only be created upon entry to the Billing tab.",
							accounting.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation);
					}
				}
			}
			else
			{
				jobCreationLoadError = QuotedBooking.TryLoadOrCreateJob();
				if (string.IsNullOrEmpty(jobCreationLoadError))
				{
					Job = QuotedBooking.Job;
				}
			}

			SetupLayout(jobCreationLoadError);
		}

		JobHeader Job;

		void OnJobCreating(object sender, EventArgs e)
		{
			if (QuotedBooking != null)
			{
				QuotedBooking.ClientAddrPKInfo.RefreshBinding();
			}

			SetupLayout("");
		}

		void OnJobCreated(object sender, EventArgs e)
		{
			if (QuotedBooking != null)
			{
				QuotedBooking.ClientAddrPKInfo.RefreshBinding();
			}

			SetupLayout("");
		}

		void OnJobDeleting(object sender, EventArgs e)
		{
			if (ParentQuotedBookingForm != null && ParentQuotedBookingForm.ShowQuoteOnlyControls)
			{
				return;
			}

			SetupLayout(Res.GetString("06be73c3-c225-4ce5-9815-0ae7d54524f4", "Billing job is being deleted."));
		}

		void OnCrossTradeChanged(object sender, EventArgs e)
		{
			if (QuotedBooking != null)
			{
				if (ShouldChangeCaptionOnCrossTrade)
				{
					ClientOrgControl.Text = QuotedBooking.PrepaidBillToPartyText;
					ClientOrgControl.CaptionResourceString = QuotedBooking.PrepaidBillToPartyCaption;
				}
				else
				{
					ClientOrgControl.Text = QuotedBooking.ClientText;
					ClientOrgControl.CaptionResourceString = QuotedBooking.ClientCaption;
				}
			}
		}

		void SetupLayout(ZString jobCreationLoadError)
		{
			if (jobCreationLoadError.IsEmpty)
			{
				JobHeaderClientCoveringLabel.Visible = false;

				if (ParentQuotedBookingForm != null && ParentQuotedBookingForm.ShowQuoteOnlyControls)
				{
					ClientDocAddresssControl.Visible = true;
					ClientDocAddresssControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
					this.Size = ClientDocAddresssControl.Size;
					ClientDocAddresssControl.Visible = true;

					ClientOrgControl.Visible = false;
				}
				else
				{
					if (QuotedBooking.QuotedBookingTypeCode == QuotedBooking.QuickBookingCode)
					{
						ClientOrgControl.CaptionResourceString = Res.GetData("ClientControl|ae3ac2c9-c409-41da-9c7f-5c861ec9a19d", "Client");
					}

					ClientOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
					this.Size = ClientOrgControl.Size;
					ClientOrgControl.Visible = true;

					ClientDocAddresssControl.Visible = false;
				}
			}
			else
			{
				JobHeaderClientCoveringLabel.Visible = true;
				JobHeaderClientCoveringLabel.Text = jobCreationLoadError;

				ClientOrgControl.Visible = false;
				ClientDocAddresssControl.Visible = false;
			}
		}

		bool ShouldChangeCaptionOnCrossTrade => AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.Value && QuotedBooking.IsCrossTrade();

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Job != null)
				{
					Job.Dispose();
				}

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
