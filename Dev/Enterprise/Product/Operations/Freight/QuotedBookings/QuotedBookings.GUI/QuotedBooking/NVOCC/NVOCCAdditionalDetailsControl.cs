using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class NVOCCAdditionalDetailsControl : ZUserControl
	{
		public NVOCCAdditionalDetailsControl()
		{
			InitializeComponent();
			SetDataSourceBinding("TransportContainerMode", QuotedBooking.Schema.Mode);
			SetDataSourceBinding("IsDomesticFreight", QuotedBooking.Schema.IsDomesticFreight);

			OnBoardDropEdit.AllowOverlap(ChargesApplyDropEdit);
			ReleaseTypeDropEdit.AllowOverlap(ChargesApplyDropEdit);
			BookingPartyDocAddressControl.AllowOutsideOfParent();

			if (!Enterprise.ZArchitecture.Core.DesignModeFinder.IsDesigning)
			{
				ConsigneeDocAddressControl.Enter += new EventHandler(ConsigneeDocAddressControl_Enter);
			}
		}

		#region Binding

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (QuotedBooking != null)
			{
				LoadOrCreateJob();

				QuotedBooking.JobCreating += new EventHandler(OnJobCreating);
				QuotedBooking.JobDeleting += new EventHandler(OnJobDeleting);

				SetupLayout();
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (QuotedBooking != null)
			{
				QuotedBooking.JobCreating -= new EventHandler(OnJobCreating);
				QuotedBooking.JobDeleting -= new EventHandler(OnJobDeleting);
			}
		}

		#region TransportContainerMode

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZString TransportContainerMode
		{
			get { return transportContainerMode; }
			set
			{
				if (transportContainerMode != value)
				{
					transportContainerMode = value;
					SetupLayout();
				}
			}
		}
		ZString transportContainerMode;

		#endregion

		#region IsDomesticFreight

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZString IsDomesticFreight
		{
			get { return isDomesticFreight; }
			set
			{
				if (isDomesticFreight != value)
				{
					isDomesticFreight = value;
					SetupLayout();
				}
			}
		}
		ZString isDomesticFreight = "";

		#endregion

		#endregion

		#region SetupLayout

		const int GroupBoxHeight_1 = 40; //1x controls
		const int GroupBoxHeight_2 = 60; //2x controls
		const int GroupBoxHeight_3 = 80; //3x controls
		const int GroupBoxMargin = 5;

		void SetupLayout()
		{
			if (ParentQuotedBookingForm == null)
			{
				return;
			}

			//Main Fields
			//Quote
			ViaCodeFindBox.Visible = ParentQuotedBookingForm.ShowQuoteControls;

			//Goods Details
			//Quote
			CommodityFindBox.Visible = ParentQuotedBookingForm.ShowQuoteControls;

			//Bokerage Details
			BrokerageDetailsGroupBox.Visible = !QuotedBooking.IsDomesticFreight;
			//Quote
			EntriesPanel.Visible = ParentQuotedBookingForm.ShowQuoteControls;
			//Booking
			EntryNumberPanel.Visible = ParentQuotedBookingForm.ShowBookingControls;
			//Sizing
			if (ParentQuotedBookingForm.ShowBookingControls && ParentQuotedBookingForm.ShowQuoteControls)
			{
				ControlDpiScalingHelper.SetHeight(ref BrokerageDetailsGroupBox, GroupBoxHeight_3 + GroupBoxMargin, true);
			}
			else if (ParentQuotedBookingForm.ShowQuoteControls)
			{
				ControlDpiScalingHelper.SetHeight(ref BrokerageDetailsGroupBox, GroupBoxHeight_2 + GroupBoxMargin, true);
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(ref BrokerageDetailsGroupBox, GroupBoxHeight_1 + GroupBoxMargin, true);
			}

			//Monetary Details
			JP_InsuranceRequiredCheckBox.Visible = ParentQuotedBookingForm.ShowBookingControls;
			if (ParentQuotedBookingForm.ShowBookingControls)
			{
				ControlDpiScalingHelper.SetHeight(ref MonetaryGroupBox, GroupBoxHeight_3 + GroupBoxMargin, true);
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(ref MonetaryGroupBox, GroupBoxHeight_2 + GroupBoxMargin, true);
			}
		}

		#endregion

		#region OrderItemsEditButton

		void OrderItemsEditButton_Click(object sender, EventArgs e)
		{
			OrderItemCollectionForm.ShowDialog(QuotedBooking.Booking.DocsAndCartage.OrderItems);
		}

		#endregion

		#region Implementaion

		QuotedBookingForm ParentQuotedBookingForm
		{
			get { return (QuotedBookingForm)ParentForm; }
		}

		QuotedBooking QuotedBooking
		{
			get { return ParentQuotedBookingForm == null ? null : ParentQuotedBookingForm.QuotedBooking; }
		}

		void ConsigneeDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (!QuotedBooking.ConsigneeDocumentaryAddress.ReadOnly && QuotedBooking.BuyerSupplierLinksHelper.ShouldShowRelatedConsignees)
			{
				ConsigneeDocAddressControl.SelectFromPopupForm();
			}
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
						jobCreationLoadError = Res.GetString("DBA78162-352C-46E2-BC16-F42441315BF9", "This job has been created but is currently inactive, please click on the Billing tab or run Job Invoicing menu to re-activate it.");
					}
					else
					{
						jobCreationLoadError = Res.GetString("0b64dcf8-3ebf-4080-a5ed-da9065643778", "Billing jobs will only be created upon entry to the Billing tab. Check registry for defaults.",
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

			ShowHideJobCreationError(jobCreationLoadError);
		}

		JobHeader Job;

		void OnJobCreating(object sender, EventArgs e)
		{
			if (QuotedBooking != null)
			{
				QuotedBooking.ClientAddrPKInfo.RefreshBinding();
			}

			ShowHideJobCreationError("");
		}

		void OnJobDeleting(object sender, EventArgs e)
		{
			if (ParentQuotedBookingForm != null && ParentQuotedBookingForm.ShowQuoteOnlyControls)
			{
				return;
			}

			ShowHideJobCreationError(Res.GetString("06be73c3-c225-4ce5-9815-0ae7d54524f4", "Billing job is being deleted."));
		}

		void ShowHideJobCreationError(ZString jobCreationLoadError)
		{
			if (jobCreationLoadError.IsEmpty)
			{
				JobHeaderClientCoveringLabel.Visible = false;

				if (ParentQuotedBookingForm != null && ParentQuotedBookingForm.ShowQuoteOnlyControls)
				{
					ClientOrgControl.Visible = false;
				}
				else
				{
					ClientOrgControl.Visible = true;
				}
			}
			else
			{
				JobHeaderClientCoveringLabel.Visible = true;
				JobHeaderClientCoveringLabel.Text = jobCreationLoadError;

				ClientOrgControl.Visible = false;
			}
		}

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

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<NVOCCAdditionalDetailsControl>()
			.Property("IsDomesticFreight", ZString.Empty, false)
			.Property("TransportContainerMode", ZString.Empty, false)
			.Result;
		}

		#endregion
	}
}
