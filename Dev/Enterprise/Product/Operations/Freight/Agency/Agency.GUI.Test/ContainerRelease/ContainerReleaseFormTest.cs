using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class ContainerReleaseFormTest : BaseAgencyTest
	{
		public void TestShow_IsReplacement()
		{
			Booking.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			using (ZForm form = new ZForm(Booking))
			{
				form.Show();
				ShowAndAssertError(form, Booking, true, "Information This booking has not been saved.");
				Factory.Save();
				ShowAndAssertError(form, Booking, true, "Information This booking is not FCL.");
				Booking.JS_CFSReference = "";
				Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				ShowAndAssertError(form, Booking, true, "Information This booking has not been saved.");
				Factory.Save();
				ShowAndAssertError(form, Booking, true, "Information No containers have been entered against this booking.");
				AgencyShipmentContainer container1 = Booking.BookedContainers.AddNew();
				container1.JC_RC = RC_20GP_PK;
				container1.JC_ContainerCount = 2;
				AgencyShipmentContainer container2 = Booking.BookedContainers.AddNew();
				container2.JC_RC = RC_20GP_PK;
				container2.JC_ContainerCount = 3;
				Factory.Save();
				ShowAndAssertError(form, Booking, false, "Information Booking reference not entered yet.");
				Booking.JS_CFSReference = "BOB";
				Factory.Save();
				ShowAndAssertError(form, Booking, true, "Information No containers have been released yet.");
				container1.JC_ReleaseNum = "Ref";
				Factory.Save();
				ShowAndAssertSuccess(form, booking, true);
			}
		}

		public void TestShow_NotReplacement()
		{
			Booking.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			using (ZForm form = new ZForm(Booking))
			{
				form.Show();
				ShowAndAssertError(form, Booking, false, "Information This booking has not been saved.");
				Factory.Save();
				ShowAndAssertError(form, Booking, false, "Information This booking is not FCL.");
				Booking.JS_CFSReference = "";
				Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				ShowAndAssertError(form, Booking, false, "Information This booking has not been saved.");
				Factory.Save();
				ShowAndAssertError(form, Booking, false, "Information No containers have been entered against this booking.");
				AgencyShipmentContainer container1 = Booking.BookedContainers.AddNew();
				container1.JC_RC = RC_20GP_PK;
				container1.JC_ContainerCount = 2;
				container1.JC_ReleaseNum = "Ref";
				AgencyShipmentContainer container2 = Booking.BookedContainers.AddNew();
				container2.JC_RC = RC_20GP_PK;
				container2.JC_ContainerCount = 3;
				container2.JC_ReleaseNum = "Ref";
				Factory.Save();
				ShowAndAssertError(form, Booking, false, "Information Booking reference not entered yet.");
				Booking.JS_CFSReference = "BOB";
				Factory.Save();
				ShowAndAssertError(form, Booking, false, "Information All non-shipper owned containers have been released.");
				container1.JC_ReleaseNum = "";
				Factory.Save();
				ShowAndAssertSuccess(form, booking, false);
			}
		}

		public void TestRelease_Success()
		{
			Booking.JS_CFSReference = "Ref";
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "MMM";
			Booking.JS_OH_DeliveryAgent = principal.PK;
			StmPrintQueue queue = Factory.NewWithValidTestData<StmPrintQueue>();
			OrgAddress address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.Header.OH_FullName = "CY";
			AgencyShipmentContainer container1 = Booking.BookedContainers.AddNew();
			container1.JC_RC = RC_20GP_PK;
			container1.JC_ContainerCount = 4;
			container1.JC_ReleaseNum = "";
			container1.JC_OA_DepartureContainerYardAddress = address1.PK;
			ReleaseHeader header = new ReleaseHeader(Booking, false);
			header.Init();
			header.DoSelectAll();
			header.RunPreSaveValidation();
			AssertNoNotifications("precondition:", header);
			AssertContainers("precondition:", new string[] { "20GP (4) (:CY)" });
			AssertEquals("precondition:", 0, CountPrintJobs(Booking));
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			using (ZFormModaliser.SuspendDispose())
			using (ContainerReleaseForm form = new ContainerReleaseForm(header))
			{
				RegisterPseudoUserAndPreferedPrinter(queue);
				form.Show();
				form.FireReleaseButton();
				AssertEquals("", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertContainers("Containers should be released.", new string[] { "20GP (4) (Ref-1:CY)" });
				AssertEquals("should have delivered the document", 1, CountPrintJobs(Booking));
				AssertEquals("form should have closed", false, form.Visible);
			}
		}

		public void TestRelease_ValidationError()
		{
			Booking.JS_CFSReference = "Ref";
			StmPrintQueue queue = Factory.NewWithValidTestData<StmPrintQueue>();
			OrgAddress address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.Header.OH_FullName = "CY";
			AgencyShipmentContainer container1 = Booking.BookedContainers.AddNew();
			container1.JC_RC = RC_20GP_PK;
			container1.JC_ContainerCount = 4;
			container1.JC_ReleaseNum = "";
			container1.JC_OA_DepartureContainerYardAddress = address1.PK;
			ReleaseHeader header = new ReleaseHeader(Booking, true);
			header.Init();
			header.DoSelectAll();
			header.RunPreSaveValidation();
			AssertHasErrors("precondition:", header.ReleaseNumberInfo);
			AssertContainers("precondition:", new string[] { "20GP (4) (:CY)" });
			AssertEquals("precondition:", 0, CountPrintJobs(Booking));
			using (ContainerReleaseForm form = new ContainerReleaseForm(header))
			{
				RegisterPseudoUserAndPreferedPrinter(queue);
				form.Show();
				form.FireReleaseButton();
				AssertEquals("validationError", "Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertContainers("Containers should not be released.", new string[] { "20GP (4) (:CY)" });
				AssertEquals("should not have delivered the document", 0, CountPrintJobs(Booking));
				AssertEquals("form should not have closed", true, form.Visible);
			}
		}

		#region Implementation
		static int CountPrintJobs(BusinessObject parent)
		{
			ZQuery filter = new ZQuery(StmPrintJobSchema.SP_ParentGuid, parent.PK);
			return parent.Factory.GetDatabaseCount(typeof(StmPrintJob), filter);
		}

		static string FormatContainer(AgencyBookingContainer container)
		{
			return string.Format("{0} ({1}:{2})", container.JC_ContainerCode, container.JC_ReleaseNum, container.DepartureContainerYardAddress == null ? ZString.Empty : container.DepartureContainerYardAddress.Header.OH_FullName);
		}

		static void ShowAndAssertError(Form parentForm, AgencyBooking booking, bool isReplacement, string expectedError)
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ContainerReleaseForm.Show(parentForm, booking, isReplacement);
			AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
		}

		static void ShowAndAssertSuccess(Form parentForm, AgencyBooking booking, bool isReplacement)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ContainerReleaseForm.Show(parentForm, booking, isReplacement);
			using (ZForm formShown = (ZForm)ZFormModaliser.ActiveForm)
			{
				AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(typeof(ContainerReleaseForm), formShown == null ? null : formShown.GetType());
				AssertEquals(typeof(ReleaseHeader), formShown.BusinessEntity == null ? null : formShown.BusinessEntity.GetType());
				ReleaseHeader header = (ReleaseHeader)formShown.BusinessEntity;
				AssertEquals("IsReplacement", isReplacement, header.IsReplacement);
				formShown.Close();
			}
		}

		static void RegisterPseudoUserAndPreferedPrinter(StmPrintQueue queue)
		{
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form)
			{
				ZForm zForm = (ZForm)form;
				zForm.DialogResult = DialogResult.OK;
				DeliveryInstructions instructions = (DeliveryInstructions)zForm.BusinessEntity;
				instructions.PrinterDelivery.PrintQueuePK = queue.PK;
				instructions.Recipients.RemoveAll();
				DocDeliveryContact contact = instructions.Recipients.AddNew();
				contact.Name = "BOB";
				contact.DeliveryMethod = Constants.ContactNotifyModes.Print;
			});
		}

		void AssertContainers(string message, IEnumerable<string> containers)
		{
			AssertContainsExactElementsInAnyOrder(message, containers, Array.ConvertAll(Booking.BookedContainers.ToArray<AgencyBookingContainer>(), FormatContainer));
		}

		AgencyBooking Booking
		{
			get
			{
				return booking ?? (booking = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking booking;
		#endregion
	}
}
