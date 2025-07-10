using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	[TestedType(typeof(DtbConsignmentForm))]
	public class DtbConsignmentFormTest : ZFormBasherTest
	{
		#region TestControllerID

		public void TestControllerID()
		{
			var consignment = Factory.New<DtbConsignment>();
			using (var form = new DtbConsignmentForm(consignment))
			{
				AssertEquals(ControllerIDs.DtbConsignment, form.ControllerID);
			}
		}

		#endregion

		#region Deactivate

		public void TestDeactivate_WithNoJobHeader_ShouldDeactivate_NoError()
		{
			var consignment = Helper.CreateConsignment();
			AssertNull(consignment.Job);

			Factory.Save();

			ShowFormClickDeactivateAndSave(consignment);

			CombineAssertions("There is no job header to block deactivation.", () =>
			{
				AssertNull("We expect to see no error.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The consignment should have been deactivated.", false, consignment.LTC_IsActive);
			});
		}

		public void TestDeactivate_WithJobHeaderAndNoCharges_ShouldDeactivate_NoError()
		{
			var consignment = Helper.CreateConsignment();
			var job = (Job)Helper.CreateJobHeaderForConsignment(consignment);
			AssertContainsExactElementsInAnyOrder(Array.Empty<Charge>(), job.Charges);

			Factory.Save();

			ShowFormClickDeactivateAndSave(consignment);

			CombineAssertions("The job header has no charges to block deactivation.", () =>
			{
				AssertNull("We expect to see no error.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The consignment should have been deactivated.", false, consignment.LTC_IsActive);
			});
		}

		public void TestDeactivate_WithJobHeaderAndChargesThatAreZero_ShouldDeactivate_NoError()
		{
			var consignment = Helper.CreateConsignment();
			var job = (Job)Helper.CreateJobHeaderForConsignment(consignment);
			AddChargeToJob(job, 0);

			Factory.Save();

			ShowFormClickDeactivateAndSave(consignment);

			CombineAssertions("The charges are all 0.", () =>
			{
				AssertNull("We expect to see no error.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The consignment should have been deactivated.", false, consignment.LTC_IsActive);
			});
		}

		public void TestDeactivate_WithJobHeaderAndChargeGreaterThanZero_ShouldNotDeactivate_WithError()
		{
			var consignment = Helper.CreateConsignment();
			var job = (Job)Helper.CreateJobHeaderForConsignment(consignment);
			AddChargeToJob(job, 1);

			Factory.Save();

			ShowFormClickDeactivateAndSave(consignment);

			CombineAssertions("There is a charge with an amount greater than 0.", () =>
			{
				AssertEquals("We expect to see an error.", $"""
				                                            Land Transport Consignment LTC001 cannot be deactivated.
				                                            Job Invoicing Charge(s) have been saved against this Invoicing Job Header ({job.JH_JobNum}) in the company EDI.
				                                            """, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The consignment should not have been deactivated.", true, consignment.LTC_IsActive);
			});
		}

		void AddChargeToJob(Job job, decimal chargeAmount)
		{
			job.Department.GE_Misc = false;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			job.JH_OA_LocalChargesAddr = org.MainAddress.PK;
			var charge = job.Charges.AddNew();
			charge.SuspendValidation(); // we don't care about all the little fiddly accounting requirements.
			charge.FillWithValidTestData();
			charge.JR_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "WAR")).PK;
			charge.JR_OSCostAmt = chargeAmount;
		}

		static void ShowFormClickDeactivateAndSave(DtbConsignment consignment)
		{
			using var form = new DtbConsignmentForm(consignment);
			form.Show();
			Application.DoEvents();

			var menuItem = form.FindMenuItem_ForTest("Make Inactive");
			menuItem.PerformClick();
			form.FireSaveButton();
			Application.DoEvents();
		}

		#endregion

		public void TestFormCaption()
		{
			using (var form = (DtbConsignmentForm)GetFormToBash())
			{
				AssertEquals("Land Transport Consignment", form.FormCaption);

				((DtbConsignment)form.BusinessEntity).LTC_JobID = "CN1001";
				AssertEquals("Land Transport Consignment CN1001", form.FormCaption);
			}
		}

		public void TestReactivation_WhenBookingIsSeviceCommenced_IsNotAllowed()
		{
			var consignment = SetupConsignmentForReactivation();
			Factory.Save();
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			using (var form = new DtbConsignmentForm(consignment))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, consignment);
				deactivateMenuItem.PerformClick();

				AssertEquals("Consignment should still be inactive as booking is service commenced", false, consignment.LTC_IsActive);
				AssertContains("Message should be shown to inform user why the consignment cannot be reactivated", "The parent booking of this consignment is not available, so this consignment cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestReactivation_WhenBookingIsBooked_IsNotAllowed()
		{
			var consignment = SetupConsignmentForReactivation();
			Factory.Save();
			booking.KM_Status = TransportStatuses.Codes.Booked;

			using (var form = new DtbConsignmentForm(consignment))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, consignment);
				deactivateMenuItem.PerformClick();

				AssertEquals("Consignment should still be inactive as booking is booked", false, consignment.LTC_IsActive);
				AssertContains("Message should be shown to inform user why the consignment cannot be reactivated", "The parent booking of this consignment is not available, so this consignment cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestReactivation_WhenBookingIsInactive_IsNotAllowed()
		{
			var consignment = SetupConsignmentForReactivation();
			Factory.Load<DtbBooking>(consignment.LTC_KM_Booking).KM_IsActive = false;
			Factory.Save();

			using (var form = new DtbConsignmentForm(consignment))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, consignment);
				deactivateMenuItem.PerformClick();

				AssertEquals("Consignment should still be inactive as booking is inactive", false, consignment.LTC_IsActive);
				AssertContains("Message should be shown to inform user why the consignment cannot be reactivated", "The parent booking of this consignment is not available, so this consignment cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestReactivation_WhenBookingIsActiveAndAvailable_IsAllowed()
		{
			var consignment = SetupConsignmentForReactivation();
			Factory.Save();

			using (var form = new DtbConsignmentForm(consignment))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, consignment);
				deactivateMenuItem.PerformClick();

				AssertEquals("Consignment should be active as booking is active and available", true, consignment.LTC_IsActive);
				AssertNotContains("Reactivation message should not be shown", "The parent booking of this consignment is not available, so this consignment cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestReactivation_WhenNoRelatedBooking_IsAllowed()
		{
			var consignment = Helper.CreateConsignment();
			consignment.LTC_IsActive = false;
			Factory.Save();

			using (var form = new DtbConsignmentForm(consignment))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, consignment);
				deactivateMenuItem.PerformClick();

				AssertEquals("Consignment should be active as there is no related booking", true, consignment.LTC_IsActive);
				AssertNotContains("Reactivation message should not be shown", "The parent booking of this consignment is not available, so this consignment cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestDeactivation_WhenConsignmentIsActive_IsAllowed()
		{
			var consignment = SetupConsignmentForReactivation();
			consignment.LTC_IsActive = true;
			Factory.Save();

			using (var form = new DtbConsignmentForm(consignment))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, consignment);
				deactivateMenuItem.PerformClick();

				AssertEquals("Consignment should now be inactive as logic should not affect deactivation", false, consignment.LTC_IsActive);
				AssertNotContains("Reactivation message should not be shown", "The parent booking of this consignment is not available, so this consignment cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		DtbConsignment SetupConsignmentForReactivation()
		{
			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			booking = Factory.New<DtbBooking>();
			booking.KM_KB_Booking = bookingConsolidation.PK;
			var consignment = Helper.CreateConsignment();
			consignment.LTC_KM_Booking = booking.PK;
			consignment.LTC_IsActive = false;

			return consignment;
		}

		MenuItem FindDeactivateActionsMenuItem(DtbConsignmentForm form, DtbConsignment consignment)
		{
			MenuItem result = null;

			if (consignment != null)
			{
				var menuName = (consignment.IsCancelled)
					? "MakeActiveName"
					: "MakeInactiveName";

				result = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(item => item.Name == "ActionsMenuItem").MenuItems.Cast<MenuItem>().FirstOrDefault(item => item.Name == menuName);
			}

			return result;
		}

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (DtbConsignmentForm)GetFormToBash())
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);

				var assertMessage = "Should display error when glow url was not configured in registry.";
				AssertEquals(assertMessage,
					@"This Consignment cannot be opened in a browser.
Glow portal URL has not been configured for this client. Registry: GLOW/Services/GLOW Portals Root URL.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenInGlowPortal()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://Service/"))
			{
				var consignment = Factory.New<DtbConsignment>();

				using (var form = new DtbConsignmentForm(consignment))
				{
					form.Show();

					InvokeClick(form.glowLinkLabel);

					AssertEquals("Should not show error if registry item exists.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				AssertOpenedOnTheWeb(consignment, glowPortalsUri: "address");
			}
		}

		public static void AssertOpenedOnTheWeb(DtbConsignment consignment, string glowPortalsUri)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/Goto/ConsignmentGlow2", uri.AbsolutePath);
			AssertEquals(consignment.PK.ToString(), entityPK);
		}

		#region TestWorkflowTab

		public void TestWorkflowTab()
		{
			var consignment = Factory.New<DtbConsignment>();
			using (var form = new DtbConsignmentForm(consignment))
			{
				AssertNotNull(form.Controls["MainPanel"].Controls["MainTabControl"].Controls["WorkflowTabPage"]);
			}
		}

		#endregion

		public void TestPlugIns()
		{
			var consignment = Factory.New<DtbConsignment>();
			using (var form = new DtbConsignmentForm(consignment))
			{
				AssertPlugin(form, ControllerIDs.DocDataPlugIn);
				AssertPlugin(form, ControllerIDs.JobInvoicing);
				AssertPlugin(form, ControllerIDs.DocumentVisualizer);
			}
		}

		void AssertPlugin(DtbConsignmentForm form, ControllerID controllerID)
		{
			var plugIn = form.PlugIns.GetPlugIn(controllerID);
			AssertNotNull(string.Format("{0} Should be plugged in", controllerID), plugIn);
		}

		protected override Form GetFormToBashCore() => new DtbConsignmentForm(Factory.New<DtbConsignment>());

		void InvokeClick(ZLinkLabel label) => label.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, label, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		TransportConsignmentTestHelper Helper
		{
			get { return helper ??= new TransportConsignmentTestHelper(Factory); }
		}

		TransportConsignmentTestHelper helper;

		DtbBooking booking;
	}
}
