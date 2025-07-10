using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationImportFormTest : JobDeclarationFormAbstractTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		public void TestSavingCancelled()
		{
			var oldControl = GlbStaff.CurrentUser.GS_IsController;
			bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
			bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
			try
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				Env.Security.SupervisorOverrides.IsAllowed = false;
				Env.Security.AllowMessageErrors.IsAllowed = false;
				using (var registrySetup = new RegistrySetup(TargetInRegistry.ReconIssue))
				{
					var originalAllowed = Env.Security.USReconIssueDefault.IsAllowed;
					try
					{
						Env.Security.USReconIssueDefault.IsAllowed = false;
						var iOR = Factory.NewWithValidTestData<OrgHeader>();
						var iorWrapper = OrgHeaderWrapper.New(iOR);
						iorWrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
						Factory.Save();

						var dec = Factory.New<JobDeclaration>();
						dec.JE_OH_Importer = iOR.PK;
						dec.JE_MessageType = JobMessageTypeList.Codes.Import;
						dec.IOROrgPK = iOR.PK;
						dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
						dec.US_EnableENS = true;
						dec.RecalculateReconIndicators();
						dec.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;

						using (var form = new JobDeclarationFormForTest(dec))
						{
							AssertEquals(ContinueWithSave.No, form.ShowPreSaveDialogsInternal());
						}
					}
					finally
					{
						Env.Security.USReconIssueDefault.IsAllowed = originalAllowed;
					}
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldControl;
				Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
				Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
			}
		}

		public void TestRefreshExchangeRates()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			audCurr.ExchangeRates.DeleteAll();

			audCurr.SetUpExchangeRates(new ZDateTime(2012, 3, 1), 1.05m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 3);
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "CR";
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "OFT";
			invoiceCharge.J7_Amount = 100m;
			invoiceCharge.J7_RX_NKCurrency = "AUD";

			invoice.JobComInvoiceLines.AddNew();
			declaration.ResumeApportionment();

			AssertEquals("Exchange rate is refreshed", 1.05m, invoiceCharge.J7_ExchangeRate);
			AssertEquals(new ZDateTime(2012, 3, 1), declaration.US_LatestRateDate);

			audCurr.SetUpExchangeRates(new ZDateTime(2012, 3, 2), 1.06m);
			Factory.Save();

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();

				AssertEquals("Exrate should be refreshed", 1.06m, invoiceCharge.J7_ExchangeRate);
				AssertEquals("Apportionment Dirty", true, declaration.ApportionmentDirty);
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.CargoReleaseEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			Assert(declaration.ActiveEntryHeaders.CargoReleaseEntry.HasBeenLodgedAtCustoms);

			Factory.Save();

			audCurr.SetUpExchangeRates(new ZDateTime(2012, 3, 3), 1.07m);
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();

				AssertEquals("Exrate should NOT be refreshed", 1.06m, invoiceCharge.J7_ExchangeRate);
				AssertEquals("Apportionment Dirty", false, declaration.ApportionmentDirty);
			}
		}

		public void TestReconIssueCalculatedBeforeSupervisorOverride()
		{
			using (var registrySetup = new RegistrySetup(TargetInRegistry.ReconIssue))
			{
				var iOR = Factory.NewWithValidTestData<OrgHeader>();
				var iorWrapper = OrgHeaderWrapper.New(iOR);
				iorWrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
				Factory.Save();

				var dec = Factory.New<JobDeclaration>();
				dec.JE_OH_Importer = iOR.PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.IOROrgPK = iOR.PK;
				dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				dec.US_EnableENS = true;

				AssertEquals("pre-condition", "", dec.US_OtherReconIndicator);

				using (var form = new JobDeclarationFormForTest(dec))
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogsInternal());
				}
			}
		}

		public void TestNoExceptionThrownWhenCreateTransportBooking()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_HouseBill = "HB11111";
			declaration.JE_TotalWeight = 100m;
			declaration.JE_TotalNoOfPacks = 100;
			declaration.JE_TotalNoOfPacksPackType = "CT";
			Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				var topLevelActionsMenu = form.Menu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				AssertNotNull("Transport Booking menu exists in Actions", transportBookingMenu);

				transportBookingMenu.OnPopup(EventArgs.Empty);
				var createDeliveryTransportBookingMenu = transportBookingMenu.MenuItems.FindByText("Create Delivery Transport Booking");
				AssertNotNull("Create Delivery Transport Booking menu exists in Transport Booking", createDeliveryTransportBookingMenu);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				ZFormModaliser.ShowDialogsInTest = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				createDeliveryTransportBookingMenu.PerformClick();
				AssertEquals(ZString.Empty, (ZString)UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UserIdleWorker.Flush();

			var bookingConsolidation = Factory.Load<Integration.TransportBooking.IDtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, declaration.PK));
			AssertEquals(1, bookingConsolidation.Length);

			foreach (var form in ZApplication.GetOpenForms())
			{
				if (form.Name == "TransportBookingForm")
				{
					form.Dispose();
				}
			}
		}
	}
}
