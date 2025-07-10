using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class NVOCCModeAndPartyControlTest : TestCaseWithFactory
	{
		public void TestNVOCCReceiverDeliveryLabel()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			using (var form = new QuotedBookingForm(quotedBooking))
			using (var control = new NVOCCAdditionalDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(quotedBooking, "");
				var receivalPointGroupBox = control.Controls.Find("ReceivalPointGroupBox", true).FirstOrDefault() as ZGroupBox;
				var deliveryPointGroupBox = control.Controls.Find("DeliveryPointGroupBox", true).FirstOrDefault() as ZGroupBox;
				AssertEquals("Pickup CFS", receivalPointGroupBox.Text);
				AssertEquals("Delivery CFS", deliveryPointGroupBox.Text);
				quotedBooking.Mode = "LSE";
				AssertEquals("Pickup CFS", receivalPointGroupBox.Text);
				AssertEquals("Delivery CFS", deliveryPointGroupBox.Text);
				quotedBooking.Mode = "FCL";
				AssertEquals("Pickup CTO", receivalPointGroupBox.Text);
				AssertEquals("Delivery CTO", deliveryPointGroupBox.Text);
			}
		}

		public void TestControllingAgentAndCustomerControls()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			using (var form = new QuotedBookingForm(quotedBooking))
			using (var control = new NVOCCAdditionalDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(quotedBooking, "");

				var controllingAgent = control.Controls.Find("ControllingAgentAddressControl", true).FirstOrDefault() as MasterFiles.GUI.ZDocAddressControl;
				AssertNotNull(controllingAgent);

				var controllingCustomer = control.Controls.Find("ControllingCustomerAddressControl", true).FirstOrDefault() as MasterFiles.GUI.ZDocAddressControl;
				AssertNotNull(controllingCustomer);
			}
		}

		public void TestControlsVisibility()
		{
			var quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			using (var form = new QuotedBookingForm(quotedBooking))
			using (var control = new NVOCCAdditionalDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(quotedBooking, "");
				quotedBooking.TryLoadOrCreateJob();
				var jobHeader = quotedBooking.Job;
				AssertNotNull(jobHeader);
				AssertEquals(true, control.ClientOrgControl.Visible);
				AssertEquals(false, control.JobHeaderClientCoveringLabel.Visible);
				jobHeader.Delete();
				AssertEquals(false, control.ClientOrgControl.Visible);
				AssertEquals(true, control.JobHeaderClientCoveringLabel.Visible);
				AssertEquals("Billing job is being deleted.", control.JobHeaderClientCoveringLabel.Text);
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
				var quickBooking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
				using (var form = new QuotedBookingForm(quickBooking))
				using (var control = new NVOCCAdditionalDetailsControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(quickBooking, "");
					AssertNull("Job Header is not created for Quick Booking", quickBooking.Job);
					AssertEquals("Covering label is visible", true, control.JobHeaderClientCoveringLabel.Visible);
					AssertEquals("Billing jobs will only be created upon entry to the Billing tab. Check registry for defaults.", control.JobHeaderClientCoveringLabel.Text);
				}

				var bookingWithQuote = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
				using (var form = new QuotedBookingForm(bookingWithQuote))
				using (var control = new NVOCCAdditionalDetailsControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(bookingWithQuote, "");
					AssertNotNull("Job Header is created for Booking with Quote", bookingWithQuote.Job);
					AssertEquals("Covering label is not visible", false, control.JobHeaderClientCoveringLabel.Visible);
				}
			}
			mockAccounting.Verify(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>()), Times.AtLeastOnce());
			mockAccounting.Verify(m => m.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation, Times.AtLeastOnce());
		}

		public void TestAccountingJob_AutoCreateRegistryOn()
		{
			var quickBooking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
			using (var form = new QuotedBookingForm(quickBooking))
			using (var control = new NVOCCAdditionalDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(quickBooking, "");
				AssertNotNull("Job Header is created for Quick Booking", quickBooking.Job);
				AssertEquals("Covering label is not visible", false, control.JobHeaderClientCoveringLabel.Visible);
			}

			var bookingWithQuote = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			using (var form = new QuotedBookingForm(bookingWithQuote))
			using (var control = new NVOCCAdditionalDetailsControl())
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
			var repository = new MockRepository(MockBehavior.Default);
			var mockAccounting = repository.Create<IAccounting>(MockBehavior.Strict);
			mockAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			mockAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			mockAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				var quickBooking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
				var job = new JobHeader.Loader(quickBooking).TryCreate();
				using (var form = new QuotedBookingForm(quickBooking))
				using (var control = new NVOCCAdditionalDetailsControl())
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
			mockAccounting.Verify(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>()), Times.AtLeastOnce());
		}

		public void TestAccountingJob_AutoCreateRegistryOff_JobInactive()
		{
			var repository = new MockRepository(MockBehavior.Default);
			var mockAccounting = repository.Create<IAccounting>(MockBehavior.Strict);
			mockAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			mockAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			mockAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				var bookingWithInactiveJob = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
				var job = new JobHeader.Loader(bookingWithInactiveJob).TryCreate();
				Factory.Save();
				job.MarkAsInactive();
				Factory.Save();
				using (var form = new QuotedBookingForm(bookingWithInactiveJob))
				using (var control = new NVOCCAdditionalDetailsControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(bookingWithInactiveJob, "");
					Assert("Job is cancelled", job.IsCancelled);
					AssertEquals("Covering label is visible", true, control.JobHeaderClientCoveringLabel.Visible);
					AssertEquals("This job has been created but is currently inactive, please click on the Billing tab or run Job Invoicing menu to re-activate it.", control.JobHeaderClientCoveringLabel.Text);
				}
			}
			mockAccounting.Verify(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>()), Times.AtLeastOnce());
		}

		public void TestQuotedBookingForm_ChangingTransportMode_ShouldNotThrowNullReferenceException()
		{
			var quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			Env.Registry.SetFilterCriteria("DisplayBookingInNVOCC", ZBool.True.ToString());

			using (var form = new QuotedBookingForm(quotedBooking))
			using (var control = new NVOCCAdditionalDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();
				form.NVOCCModeCheckBox.Checked = false;
				form.Controls.Remove(control);
				form.NVOCCModeCheckBox.Checked = true;

				AssertNoExceptionThrown(() => { control.TransportContainerMode = "FCL"; });
			}
		}
	}
}
