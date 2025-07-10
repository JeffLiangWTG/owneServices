using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class ModeAndPartyControlTest : TestCaseWithFactory
	{
		public void TestControlsVisibility()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			using (ZForm form = new ZForm(quotedBooking))
			using (ClientControl control = new ClientControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(quotedBooking, "");
				quotedBooking.TryLoadOrCreateJob();
				JobHeader jobHeader = quotedBooking.Job;
				AssertNotNull(jobHeader);
				AssertEquals(true, control.ClientOrgControl.Visible);
				AssertEquals(false, control.ClientDocAddresssControl.Visible);
				AssertEquals(false, control.JobHeaderClientCoveringLabel.Visible);
				jobHeader.Delete();
				AssertEquals(false, control.ClientOrgControl.Visible);
				AssertEquals(false, control.ClientDocAddresssControl.Visible);
				AssertEquals(true, control.JobHeaderClientCoveringLabel.Visible);
				AssertEquals("Billing job is being deleted.", control.JobHeaderClientCoveringLabel.Text);
			}
		}

		public void TestControlsVisibilityQuoteOnly()
		{
			QuotedBooking spotQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			using (QuotedBookingForm form = new QuotedBookingForm(spotQuote))
			using (ClientControl control = new ClientControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(spotQuote, "");
				AssertEquals("Precondition", true, form.ShowQuoteOnlyControls);
				spotQuote.TryLoadOrCreateJob();
				JobHeader jobHeader = spotQuote.Job;
				AssertNotNull(jobHeader);
				AssertEquals(false, control.ClientOrgControl.Visible);
				AssertEquals(true, control.ClientDocAddresssControl.Visible);
				AssertEquals(false, control.JobHeaderClientCoveringLabel.Visible);
				jobHeader.Delete();
				AssertEquals(false, control.ClientOrgControl.Visible);
				AssertEquals(true, control.ClientDocAddresssControl.Visible);
				AssertEquals(false, control.JobHeaderClientCoveringLabel.Visible);
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOff()
		{
			var mockAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			mockAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			mockAccounting.Setup(m => m.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation).Returns("TESTLOCATION");
			mockAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			mockAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				QuotedBooking quickBooking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
				using (ZForm form = new ZForm(quickBooking))
				using (ClientControl control = new ClientControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(quickBooking, "");
					AssertNull("Job Header is not created for Quick Booking", quickBooking.Job);
					AssertEquals("Covering label is visible", true, control.JobHeaderClientCoveringLabel.Visible);
					AssertEquals("The registry item [TESTLOCATION] has been set so that billing jobs will only be created upon entry to the Billing tab.", control.JobHeaderClientCoveringLabel.Text);
				}

				QuotedBooking spotQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
				using (ZForm form = new ZForm(spotQuote))
				using (ClientControl control = new ClientControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(spotQuote, "");
					AssertNotNull("Job Header is created for Spot Quote", spotQuote.Job);
					AssertEquals("Covering label is not visible", false, control.JobHeaderClientCoveringLabel.Visible);
				}

				QuotedBooking bookingWithQuote = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
				using (ZForm form = new ZForm(bookingWithQuote))
				using (ClientControl control = new ClientControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(bookingWithQuote, "");
					AssertNotNull("Job Header is created for Booking with Quote", spotQuote.Job);
					AssertEquals("Covering label is not visible", false, control.JobHeaderClientCoveringLabel.Visible);
				}
			}
			mockAccounting.Verify(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>()), Times.Exactly(1));
			mockAccounting.Verify(m => m.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation, Times.Exactly(1));
		}

		public void TestAccountingJob_AutoCreateRegistryOn()
		{
			QuotedBooking quickBooking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
			using (ZForm form = new ZForm(quickBooking))
			using (ClientControl control = new ClientControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(quickBooking, "");
				AssertNotNull("Job Header is created for Quick Booking", quickBooking.Job);
				AssertEquals("Covering label is not visible", false, control.JobHeaderClientCoveringLabel.Visible);
			}

			QuotedBooking spotQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			using (ZForm form = new ZForm(spotQuote))
			using (ClientControl control = new ClientControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(spotQuote, "");
				AssertNotNull("Job Header is created for Spot Quote", spotQuote.Job);
				AssertEquals("Covering label is not visible", false, control.JobHeaderClientCoveringLabel.Visible);
			}

			QuotedBooking bookingWithQuote = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			using (ZForm form = new ZForm(bookingWithQuote))
			using (ClientControl control = new ClientControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(bookingWithQuote, "");
				AssertNotNull("Job Header is created for Booking with Quote", bookingWithQuote.Job);
				AssertEquals("Covering label is not visible", false, control.JobHeaderClientCoveringLabel.Visible);
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOff_JobAlreadyExists()
		{
			var mockAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			mockAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			var mockRegistry = new Mock<IRegistry>();
			mockAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);
			mockAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);

			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				QuotedBooking quickBooking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
				JobHeader job = new JobHeader.Loader(quickBooking).TryCreate();
				using (ZForm form = new ZForm(quickBooking))
				using (ClientControl control = new ClientControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(quickBooking, "");
					AssertNotNull(quickBooking.Job);
					AssertEquals(job.PK, quickBooking.Job.PK);
					AssertEquals(true, control.ClientOrgControl.Visible);
					AssertEquals(false, control.JobHeaderClientCoveringLabel.Visible);
				}
			}
			mockAccounting.Verify(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>()), Times.Exactly(1));
		}

		public void TestAccountingJob_AutoCreateRegistryOff_JobInactive()
		{
			var mockAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			mockAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			mockAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			mockAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				var quickBooking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
				var job = new JobHeader.Loader(quickBooking).TryCreate();
				Factory.Save();
				job.MarkAsInactive();
				Factory.Save();
				using (var form = new ZForm(quickBooking))
				using (var control = new ClientControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(quickBooking, "");
					Assert("Job Header is cancelled", job.IsCancelled);
					AssertEquals("Covering label is visible", true, control.JobHeaderClientCoveringLabel.Visible);
					AssertEquals(@"The Job Invoicing Record has been created but is currently not active. 
Please click on ‘Billing’ tab or ‘Job Invoicing’ menu to activate the job.", control.JobHeaderClientCoveringLabel.Text);
				}
			}
			mockAccounting.Verify(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>()), Times.Exactly(1));
		}
		public void TestCaptionWithCrossTradeAndRegistry()
		{
			var quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Origin = "ES22E";
			quotedBooking.Destination = "FRLYO";

			using (var form = new ZForm(quotedBooking))
			using (var control = new ClientControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(quotedBooking, "");
				quotedBooking.TryLoadOrCreateJob();
				control.ClientOrgControl.Text = "Client";
				AssertEquals("Booking is cross trade but without registry, caption is Client", "Client", control.ClientOrgControl.Text);
			}

			using (var form = new ZForm(quotedBooking))
			using (var control = new ClientControl())
			using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(quotedBooking, "");
				quotedBooking.TryLoadOrCreateJob();
				AssertEquals("With registry, when open a saved cross trade booking, caption is Prepaid Bill-To Party", "Prepaid Bill-To Party", control.ClientOrgControl.Text);
				AssertEquals("With registry, when open a saved cross trade booking, help bubble is Prepaid Bill-To Party", quotedBooking.PrepaidBillToPartyCaption, control.ClientOrgControl.CaptionResourceString);

				quotedBooking.Origin = "AUSYD";
				AssertEquals("With registry, when change origin to home port, caption is Client", "Client", control.ClientOrgControl.Text);
				AssertEquals("With registry, when change origin to home port, help bubble is Client", quotedBooking.ClientCaption, control.ClientOrgControl.CaptionResourceString);

				quotedBooking.Origin = "FRLYO";
				quotedBooking.Destination = "ES22E";
				AssertEquals("With registry, when change to cross trade shipment, caption is Prepaid Bill-To Party", "Prepaid Bill-To Party", control.ClientOrgControl.Text);
				AssertEquals("With registry, when change to cross trade shipment, help bubble is Prepaid Bill-To Party", quotedBooking.PrepaidBillToPartyCaption, control.ClientOrgControl.CaptionResourceString);
			}
		}

		public void TestClientFieldHaveCorrectValueOnJobCreated()
		{
			var quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignor = false;

			using (var form = new ZForm(quotedBooking))
			using (var control = new ClientControl())
			{
				form.Controls.Add(control);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);

				var firstVisibleTabPage = new ZTabPage();
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);

				form.Show();

				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				tabControl.SelectedTab = plugin.TabPage;
				tabControl.SelectedTab = firstVisibleTabPage;

				AssertEquals("Client field should have value of consignor", consignor.PK, quotedBooking.ClientAddrPK_ZAddress.OrgPK);
			}
		}
	}
}
