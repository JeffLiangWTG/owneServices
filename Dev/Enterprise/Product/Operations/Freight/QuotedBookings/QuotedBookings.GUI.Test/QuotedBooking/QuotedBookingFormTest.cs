using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.GUI.Test;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class QuotedBookingFormTest : BaseFreightTest
	{
		public void TestQuickCalculate()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "CONT001";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			quickBooking.TryLoadOrCreateJob();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = quickBooking.Job.PK;
			charge.JR_AC = Env.Registry.FreightChargeCode;

			Factory.Save();

			quickBooking.Job.LoadCharges_ForTestOnly();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				form.Show();
				Application.DoEvents();

				((ISupportSwitchTabPage)form).SwitchTabPage("BillingTabPage");
				Application.DoEvents();

				AssertEquals("TabPage", "BillingTabPage", form.MainTabControlExposed.SelectedTab.Name);

				var jobChargeBoundGrid = (ZGrid)form.Controls.Find("JobChargeBoundGrid", searchAllChildren: true)[0];
				jobChargeBoundGrid.Select(0);

				var quickCalculateMenuItem = jobChargeBoundGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				quickCalculateMenuItem.PerformClick();

				var jobChargeQuickCalculateForm = (JobChargeQuickCalculateForm)ZFormModaliser.ActiveForm;
				var measurementBasisDropEdit = (ZDropEdit)jobChargeQuickCalculateForm.Controls.Find("MeasurementBasisDropEdit", searchAllChildren: true)[0];
				AssertContainsExactElementsInAnyOrder
				(
					"MeasurementBasisDropEdit List",
					new[]
					{
						"Container Count",
						"Chargeable",
						"Delivery Distance",
						"Inner Packs Package",
						"Inner Packs Unit",
						"Inner Packs Volume",
						"Inner Packs Weight",
						"Loading Meters",
						"Lowest Bill",
						"Package",
						"Pickup Distance",
						"Shipment",
						"Unit",
						"Volume",
						"Weight",
						"Carrier Commission",
						"Charge Percentage",
						"Custom",
						"TEU"
					},
					((CodeDescriptionPairList)measurementBasisDropEdit.List).GetAllCodes()
				);
			}
		}

		#region Print

		public void TestPrintButton()
		{
			var quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Client.CompanyData.OB_AROnCreditHold = true;
			quotedBooking.Client.OH_IsDebtor = true;

			SetupQuotationPack();

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PrintQuoteButton.PerformClick();
				Application.DoEvents();
				AssertMultilineASCIIEquals
				(
					"Should be Error",
					@"Delivery of this document is restricted because:
       The Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this document?",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
			}
		}

		public void TestPrintMenu()
		{
			var oneOffQuote = GetOneOffQuoteForPrinting();
			Factory.Save();

			using (var form = new QuotedBookingFormForTest(oneOffQuote))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				// Load documents menu
				var menu = form.Menu.MenuItems.FindByText("Documents");
				menu.PerformClick();
				Application.DoEvents();

				var menuItems = form.Menu.MenuItems.FindByText("Documents").MenuItems;
				var quotationPackMenuItem = menuItems.FindByText("Quotation Pack");
				quotationPackMenuItem.PerformClick();
				Application.DoEvents();

				AssertMultilineASCIIEquals
				(
					"Should be Error",
					@"Delivery of this document is restricted because:
       The Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this document?",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
			}
		}

		public void TestPrintMenu_AfterPrintingOther()
		{
			var oneOffQuote = GetOneOffQuoteForPrinting();
			Factory.Save();

			using (var form = new QuotedBookingFormForTest(oneOffQuote))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				// Load documents menu
				var menu = form.Menu.MenuItems.FindByText("Documents");
				menu.PerformClick();
				Application.DoEvents();

				var documentsMenuItems = form.Menu.MenuItems.FindByText("Documents").MenuItems;
				var oneOffPricingPageMenuItem = documentsMenuItems.FindByText("Optional Pages").MenuItems.FindByText("One Off Pricing Page");
				oneOffPricingPageMenuItem.PerformClick();

				var quotationPackMenuItem = documentsMenuItems.FindByText("Quotation Pack");
				quotationPackMenuItem.PerformClick();
				Application.DoEvents();

				AssertMultilineASCIIEquals
				(
					"Should be Error",
					@"Delivery of this document is restricted because:
       The Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this document?",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
			}
		}

		QuotedBooking GetOneOffQuoteForPrinting()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);

			var oneOffQuote = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			oneOffQuote.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			oneOffQuote.Mode = Core.Constants.RateMode.FCL;
			oneOffQuote.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			oneOffQuote.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, oneOffQuote.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			oneOffQuote.TryLoadOrCreateJob();
			oneOffQuote.Job.JH_GE = Env.CurrentDepartment.PK;

			oneOffQuote.Client.CompanyData.OB_AROnCreditHold = true;
			oneOffQuote.Client.OH_IsDebtor = true;

			SetupQuotationPack();

			return oneOffQuote;
		}

		void SetupQuotationPack()
		{
			var command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Quotation, "Quotation Pack"));
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);

			var pivot = Factory.New<StmMenuMenuPivotBase>();
			pivot.SF_SU_Inward = command.PK;
			pivot.SF_SU_Outward = command.PK;
			pivot.SF_OverriddenBusinessContext = ZString.Empty;

			command.ChildMenus.Add(pivot);
		}

		#endregion

		public void TestConvertToBookingWithQuote_RecentItems()
		{
#if WINZOR
			using var mainForm = ShowMainFormForTest();
#endif
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;

			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();

				quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true; // cannot convert without approval
				Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertContainsExactElementsInAnyOrder("No recentItems", Array.Empty<string>(), GetRecentItemsUrls());
					AssertNull("No popup form", form.PopupForm_ForTesting);
				});

				var menuItems = form.Menu.MenuItems.FindByText("Actions").MenuItems;
				var convertToBookingWithQuoteMenuItem = menuItems.FindByText("Convert to Booking with Quote");
				convertToBookingWithQuoteMenuItem.PerformClick();
				Application.DoEvents();
				CombineAssertions("ConvertToBookingWithQuote should not create recentItem", () =>
				{
					AssertNullOrEmpty("No error message", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Popup form", $"Edit Booking with Quote - Quote ({quote.TH_QuoteNumber})", form?.PopupForm_ForTesting?.Text);
					AssertContainsExactElementsInAnyOrder("No recentItem", Array.Empty<string>(), GetRecentItemsUrls());
				});

				var popupFormQuotedBooking = ((QuotedBooking)form.PopupForm_ForTesting.BusinessEntity);
				popupFormQuotedBooking.Weight = 10; // avoid Error - Weight: You must specify either a weight or volume if your shipment is LSE or LCL
				popupFormQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK; // avoid Error - OrganisationPK: This Organization has no address entered
				using (RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					form.PopupForm_ForTesting.FireSaveButton();
					Application.DoEvents();
				}
				CombineAssertions("Saving popup form should create recentItem", () =>
				{
					AssertContains($"edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=QuotedBookings&BusinessEntityPK", GetRecentItemsUrls().SingleOrDefault());
				});

				form.PopupForm_ForTesting.Close();
			}
		}

		IEnumerable<string> GetRecentItemsUrls() => RecentItemManager.Instance.GetRecentItems(string.Empty).Select(recentItem => recentItem.STL_ItemUrl.ToString());

		#region Additional Terms

		public void TestAdditionalTermsVisibility_SEAMode_DomesticMode_ShouldBeVisible()
		{
			AssertAdditionalTerms
			(
				transportMode: Constants.TransportModes.Sea,
				mode: ZString.Empty,
				destination: "AUPER",
				expectAdditionalTermsTextBox: true,
				message: "GIVEN domestic location = AUPER THEN AdditionalTermsTextBox should be shown."
			);
		}

		public void TestAdditionalTermsVisibility_SEAMode_NotDomesticMode_ShouldBeVisible()
		{
			AssertAdditionalTerms
			(
				transportMode: Constants.TransportModes.Sea,
				mode: ZString.Empty,
				destination: "USLAX",
				expectAdditionalTermsTextBox: true,
				message: "GIVEN non-domestic location = USLAX THEN AdditionalTermsTextBox should be shown."
			);
		}

		public void TestAdditionalTermsVisibility_FCLMode_DomesticMode_ShouldBeVisible()
		{
			AssertAdditionalTerms
			(
				transportMode: ZString.Empty,
				mode: Core.Constants.RateMode.FCL,
				destination: "AUPER",
				expectAdditionalTermsTextBox: true,
				message: "GIVEN domestic location = AUPER THEN AdditionalTermsTextBox should be shown."
			);
		}

		public void TestAdditionalTermsVisibility_FCLMode_NotDomesticMode_ShouldBeVisible()
		{
			AssertAdditionalTerms
			(
				transportMode: ZString.Empty,
				mode: Core.Constants.RateMode.FCL,
				destination: "USLAX",
				expectAdditionalTermsTextBox: true,
				message: "GIVEN domestic location = AUPER THEN AdditionalTermsTextBox should be shown."
			);
		}

		void AssertAdditionalTerms(ZString transportMode, ZString mode, ZString destination, bool expectAdditionalTermsTextBox, string message)
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			if (!string.IsNullOrEmpty(transportMode))
			{
				quotedBooking.TransportMode = transportMode;
			}

			if (!string.IsNullOrEmpty(mode))
			{
				quotedBooking.Mode = mode;
			}
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = destination;
			quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				var additionalTermsTextBox = GUITestHelper.FindControl<ZTextBox>(form.Controls, "AdditionalTermsTextBox");
				AssertNotNull("AdditionalTermsTextBox should exists", additionalTermsTextBox);
				AssertEquals(message, expectAdditionalTermsTextBox, additionalTermsTextBox.Visible);
			}
		}

		#endregion

		public void TestDisposeAviationSecuritySupport()
		{
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Sea);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Air);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Road);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Rail);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Courier);
		}

		void AssertDisposeAviationSecuritySupportWithTransportMode(string transportMode)
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(transportMode);

			using (var frm = new QuotedBookingForm(quotedBooking))
			{
				frm.Show();
			}

			Assert("Should have no leaked one or more disposable objects that have not been collected by the GC.", true);
		}

		public void TestGuiFactoryServices()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			ICommonShipmentDocumentSupporterQueryProvider shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
			AssertNull("shipment doc supporter", shipmentDocSupporterQueryProvider);

			IServicesSelectionProvider servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("services selection provider", servicesSelectionProvider);

			using (new QuotedBookingForm(quotedBooking))
			{
				shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull("shipment doc supporter", shipmentDocSupporterQueryProvider);
				Assert(shipmentDocSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("services selection provider", servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		public void TestNewQuotedBookingFormCaption()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			using (var form = (QuotedBookingForm)controller.ShowNewForm())
			{
				form.Show();
				Application.DoEvents();

				Assert(!form.FormHeading.StartsWith("Edit"));
				AssertEquals("New", form.FormVerb);

				AssertEquals(false, form.ConvertQuoteToQuotedBookingButton.Visible);
				AssertEquals(false, form.ApproveOneOffButton.Visible);
			}
		}

		public void TestDisposeAndDeleteJob()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "CONT001";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			new JobHeader.Loader(quickBooking).TryCreateWithMutex();
			new JobHeader.Loader(quickBooking.Booking).TryCreateWithMutex();
			JobHeader jobQuoteBooking = null;
			JobHeader jobBooking = null;

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				form.Show();
				Application.DoEvents();

				jobQuoteBooking = form.QuotedBooking.Job;
				jobBooking = form.QuotedBooking.Booking.Job;

				AssertEquals(false, jobQuoteBooking.IsDeleted);
				AssertEquals(false, jobBooking.IsDeleted);
				AssertEquals(true, JobHeader.GetMutex_ForTestOnly(quickBooking.PK).IsLocked);
				AssertEquals(true, JobHeader.GetMutex_ForTestOnly(quickBooking.Booking.PK).IsLocked);
			}

			AssertEquals(true, jobQuoteBooking.IsDeleted);
			AssertEquals(true, jobBooking.IsDeleted);
			AssertEquals(false, JobHeader.GetMutex_ForTestOnly(quickBooking.PK).IsLocked);
			AssertEquals(false, JobHeader.GetMutex_ForTestOnly(quickBooking.Booking.PK).IsLocked);
		}

		#region Null reference tests

		public void TestOneOffQuote_WhenConsolidateToNewConsol_ThenCompanyTariffOverrideLevelIsCopiedFromOOQ()
		{
			var glbTariff = Factory.New<GlobalTariff>();
			var glbTariff2 = Factory.New<GlobalTariff>();
			Factory.Save();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.CompanyTariffLevel = "2";
			quotedBooking.Factory.Save();
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();
				var consolForm = form.PopupForm_ForTesting;
				var createdConsol = consolForm.BusinessEntity as ForwardingConsol;
				var shipment = (ForwardingShipment)createdConsol.Shipments.First();

				Assert("Given One Off Quote's CompanyTariffLevelOverride is 2, When we run action Consolidate, Then the new shipment's CompanyTariffLevelOverride should be copied from OOQ", shipment.JS_CompanyTariffLevelOverride == 2);
			}
		}

		public void TestGivenNewOneOffQuote_WhenConsolidateToNewConsol_ThenHBLDeliveryModeShouldBeCopiedFromOOQ()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.ContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();
				var consolForm = form.PopupForm_ForTesting;
				var createdConsol = consolForm.BusinessEntity as ForwardingConsol;
				var shipment = (ForwardingShipment)createdConsol.Shipments.First();

				AssertEquals
				(
					"Given New One Off Quote, When consolidate to new Consol, Then shipment's HBLDeliveryMode should be copied from OOQ",
					quotedBooking.ContainerPackModeOverride,
					shipment.JS_HBLContainerPackModeOverride
				);
			}
		}

		[ExpectNoExceptions()]
		public void TestConsolidateToNewConsolDoesntThrowException_QBHasNoChanges()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			quotedBooking.Factory.Save();

			using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
			}
		}

		public void TestConsolidateToNewConsolDoesntThrowException_NewConsolNotAllowed()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			quotedBooking.Factory.Save();

			Env.Security.MaintainConsolNew.IsAllowed = false;

			AssertExceptionThrown<TargetInvocationException>("Inner exception of type: SecurityAccessDeniedException", () =>
			{
				using (var form = new QuotedBookingFormForTest(quotedBooking))
				{
					form.ConsolidateToNewConsol();
				}
			});
		}

		void AssertConsolidateToNewConsolDontCopyJobCO2e(string status)
		{
			// Arrange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Weight = status == CO2eStatusList.Codes.NotCalculated ? new ZDecimal(0) : 5m;
			quotedBooking.WeightUnit = Constants.Weight.Tonnes;
			quotedBooking.SetCO2ePerTonneInKg(status == CO2eStatusList.Codes.NotCalculated ? new ZDecimal(0) : 100.1111111m);
			quotedBooking.SetCO2eStatus(status);
			Factory.Save();

			// Act
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var consolForm = form.PopupForm_ForTesting;
				var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;

				// Assert
				AssertEquals("NON", forwardingConsol.GetCO2eStatus());
				AssertEquals(0m, forwardingConsol.GetTotalCO2e());
				AssertEquals(0m, forwardingConsol.Shipments[0].GetTotalCO2e());
			}
		}

		public void TestConsolidateToNewConsol_DoNotPopulateCO2eWithStatusCurrentInShipmentAndConsol()
		{
			AssertConsolidateToNewConsolDontCopyJobCO2e(CO2eStatusList.Codes.Current);
		}

		public void TestConsolidateToNewConsol_DoNotPopulateCO2eWithStatusNotCurrentInShipmentAndConsol()
		{
			AssertConsolidateToNewConsolDontCopyJobCO2e(CO2eStatusList.Codes.NotCurrent);
		}

		public void TestConsolidateToNewConsol_DoNotPopulateCO2eWithStatusPendingInShipmentAndConsol()
		{
			AssertConsolidateToNewConsolDontCopyJobCO2e(CO2eStatusList.Codes.Pending);
		}

		public void TestConsolidateToNewConsol_DoNotPopulateCO2eWithStatusRejectedInShipmentAndConsol()
		{
			AssertConsolidateToNewConsolDontCopyJobCO2e(CO2eStatusList.Codes.Rejected);
		}

		public void TestConsolidateToNewConsol_DoNotPopulateCO2eWithStatusNotCalculatedInShipmentAndConsol()
		{
			AssertConsolidateToNewConsolDontCopyJobCO2e(CO2eStatusList.Codes.NotCalculated);
		}

		public void TestConsolidateToNewConsol_BookingWithQuote()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			quotedBooking.Factory.Save();

			using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				ForwardingShipment sameBooking = Factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
				AssertEquals("One Consol must have been created and attached to booking", 1, sameBooking.Consols.Count);
				Assert(sameBooking.Consols[0].Shipments[0].Job != null);
			}
		}

		public void TestConsolidateToNewConsol_BookingWithQuote_HasSameHBLAndUniqueConsignRef()
		{
			var newCustomisation = new BillOfLadingNumberCustomisation();
			newCustomisation.RemoveFountainPrefix = false;
			newCustomisation.UseShipmentSequenceNumber = true;
			newCustomisation.CheckDigitAlgorithm = "NON";
			newCustomisation.AutoAllocateMasterBillNumbersToConsols = true;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			using (FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation))
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
				var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
				quotedBooking.Origin = "USLAX";
				Factory.Save();

				using (var form = new QuotedBookingFormForTest(quotedBooking))
				{
					form.ConsolidateToNewConsol();
					var consolForm = form.PopupForm_ForTesting;
					var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;
					AssertEquals("HouseBill Generated Automatically", "PENDING ALLOCATION..", forwardingConsol.Shipments[0].JS_HouseBill);
					forwardingConsol.Factory.Save();
					AssertNotNullOrEmpty(forwardingConsol.Shipments[0].JS_HouseBill);
					AssertEquals("HouseBill and UniqueConsignRef are the same", forwardingConsol.Shipments[0].JS_HouseBill, forwardingConsol.Shipments[0].JS_UniqueConsignRef);
				}
			}
		}

		public void TestConvertBookingToConsolidate_WithAutoGenerateHouseBillForOriginAndCompanyAreInSameCountry_NonDirectBooking()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
				quotedBooking.Booking.JS_HouseBill = ZString.Empty;
				quotedBooking.Booking.JS_IsDirectBooking = false;
				quotedBooking.Factory.Save();

				using (var form = new QuotedBookingFormForTest(quotedBooking))
				{
					form.ConsolidateToNewConsol();
					var consolForm = form.PopupForm_ForTesting;
					var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;
					AssertEquals("HouseBill Not Generated Automatically Because Origin and Company Country are not Same", ZString.Empty, forwardingConsol.Shipments[0].JS_HouseBill);
					forwardingConsol.Factory.Save();
					AssertEquals("HouseBill Not Generated Automatically even after save", ZString.Empty, forwardingConsol.Shipments[0].JS_HouseBill);
				}

				quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				quotedBooking.Booking.JS_RL_NKOrigin = "USLAX";
				quotedBooking.Factory.Save();

				quotedBooking.Booking.JS_HouseBill = ZString.Empty;
				quotedBooking.Booking.JS_IsDirectBooking = false;
				quotedBooking.Factory.Save();

				using (var form = new QuotedBookingFormForTest(quotedBooking))
				{
					form.ConsolidateToNewConsol();
					var consolForm = form.PopupForm_ForTesting;
					var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;
					AssertEquals("HouseBill Generated Automatically", "PENDING ALLOCATION..", forwardingConsol.Shipments[0].JS_HouseBill);
					forwardingConsol.Factory.Save();
					AssertNotNullOrEmpty(forwardingConsol.Shipments[0].JS_HouseBill);
				}
			}
		}

		public void TestConvertBookingToConsolidate_DoNotGenerateHouseBill_DirectBooking()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				quotedBooking.Booking.JS_RL_NKOrigin = "USLAX";
				quotedBooking.Booking.JS_HouseBill = ZString.Empty;
				quotedBooking.Booking.JS_IsDirectBooking = true;
				quotedBooking.Factory.Save();

				using (var form = new QuotedBookingFormForTest(quotedBooking))
				{
					form.ConsolidateToNewConsol();
					var consolForm = form.PopupForm_ForTesting;
					var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;
					AssertEquals("HouseBill is not generated automatically because Booking is Direct", ZString.Empty, forwardingConsol.Shipments[0].JS_HouseBill);
					forwardingConsol.Factory.Save();
					AssertEquals("HouseBill is not generated automatically even after save", ZString.Empty, forwardingConsol.Shipments[0].JS_HouseBill);
				}
			}
		}

		public void TestConvertBookingToConsolidate_KeepHouseBillWhenHouseBillIsNotEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
				quotedBooking.Booking.JS_HouseBill = "CHISIN001357";
				quotedBooking.Factory.Save();

				using (var form = new QuotedBookingFormForTest(quotedBooking))
				{
					form.ConsolidateToNewConsol();
					var consolForm = form.PopupForm_ForTesting;
					var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;
					AssertEquals("HouseBill does not change", "CHISIN001357", forwardingConsol.Shipments[0].JS_HouseBill);
					Assert("JS_HouseBillInfo has not changes", !forwardingConsol.Shipments[0].JS_HouseBillInfo.HasChanges);
					forwardingConsol.Factory.Save();
					AssertEquals("HouseBill does not change", "CHISIN001357", forwardingConsol.Shipments[0].JS_HouseBill);
				}

				quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				quotedBooking.Booking.JS_RL_NKOrigin = "USLAX";
				quotedBooking.Booking.JS_HouseBill = "CHISIN001357";
				quotedBooking.Factory.Save();

				using (var form = new QuotedBookingFormForTest(quotedBooking))
				{
					form.ConsolidateToNewConsol();
					var consolForm = form.PopupForm_ForTesting;
					var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;
					AssertEquals("HouseBill does not change", "CHISIN001357", forwardingConsol.Shipments[0].JS_HouseBill);
					Assert("JS_HouseBillInfo has not changes", !forwardingConsol.Shipments[0].JS_HouseBillInfo.HasChanges);
					forwardingConsol.Factory.Save();
					AssertEquals("HouseBill does not change", "CHISIN001357", forwardingConsol.Shipments[0].JS_HouseBill);
				}
			}
		}

		public void TestConvertBookingToConsolidate_CreateDeclaration()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			quotedBooking.Booking.JS_RL_NKDestination = "SGSIN";
			quotedBooking.Booking.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			quotedBooking.Factory.Save();

			using (FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				var consolForm = form.PopupForm_ForTesting;
				var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;
				forwardingConsol.Factory.Save();
				var shipment = forwardingConsol.Shipments[0];
				var declaration = Factory.LoadTop1<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK));
				AssertNull("A declaration should not be created when registry is false", declaration);
			}

			quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			quotedBooking.Booking.JS_RL_NKDestination = "SGSIN";
			quotedBooking.Booking.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			quotedBooking.Factory.Save();

			using (FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				var consolForm = form.PopupForm_ForTesting;
				var forwardingConsol = consolForm.BusinessEntity as ForwardingConsol;
				forwardingConsol.Factory.Save();
				var shipment = forwardingConsol.Shipments[0];
				var declaration = Factory.LoadTop1<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK));
				AssertNotNull("A declaration should be created when registry is true", declaration);
			}
		}

		public void TestConvertBookingAddToExistingConsol_CreateDeclaration()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			quotedBooking.Booking.JS_RL_NKDestination = "SGSIN";
			quotedBooking.Booking.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			quotedBooking.Factory.Save();
			var consol = Factory.New<ForwardingConsol>();
			using (FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Factory.Save();

				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();

				var popup = form.LastSelectHelper_ForTesting.LastPopupForTesting_ForTesting;
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { consol });

				using (ZForm consolForm = (ZForm)form.LastSelectHelper_ForTesting.LastController_ForTesting.LastShownForm)
				{
					var consolLoadedInForm = (ForwardingConsol)consolForm.BusinessEntity;
					AssertEquals("The form should be editing the consol", consol.PK, consolLoadedInForm.PK);
					AssertEquals("The Consol should contain the shipment", true, consolLoadedInForm.Shipments.Contains(quotedBooking.Booking.PK));

					consolLoadedInForm.Factory.Save();
					consolForm.Close();
				}

				var declaration = Factory.LoadTop1<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, quotedBooking.Booking.PK));
				AssertNotNull("A declaration should be created when the registry flag is true.", declaration);
			}
		}
		#endregion

		#region Saving

		public void TestSaveShowsVolumeDialog()
		{
			QuotedBooking book = GetSavedQuotedBooking();
			book.Mode = Core.Constants.RateMode.LCL;

			Factory.Save();

			using (QuotedBookingForm form = new QuotedBookingForm(book))
			{
				form.Show();

				book.Booking.OuterPackLines.AddNew();
				book.Booking.OuterPackLines[0].JL_Height = 2;
				book.Booking.OuterPackLines[0].JL_Width = 3;
				book.Booking.OuterPackLines[0].JL_Length = 4;
				book.Booking.OuterPackLines[0].JL_PackageCount = 1;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				AssertEquals("should not update volume", 0, book.Volume.ToZInt());

				book.Booking.OuterPackLines[0].JL_Height = 6;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();

				AssertEquals("should update volume", 72, book.Volume.ToZInt());
			}
		}

		public void TestSaveShowConfirmationForNewSupplierBuyerRelationshipDialog()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			quotedBooking.Mode = Core.Constants.RateMode.ULD;

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			Factory.Save();

			using (QuotedBookingForm form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();

				ZQuery filter = new ZQuery();
				filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, consignee.PK);
				filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, consignor.PK);

				BusinessObject[] links = Factory.Load<OrgSupplierBuyerLink>(filter);
				AssertEquals("Supplier Buyer Link should saved.", 1, links.Length);
			}
		}

		public void TestSaving_ControllingPartiesDefaulted()
		{
			FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			booking.JS_TransportMode = "SEA";
			booking.JS_PackingMode = "LCL";
			booking.ConsigneePK = consignee.PK;
			booking.ConsignorPK = consignor.PK;

			var controllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent2 = Factory.NewWithValidTestData<OrgHeader>();

			var relatedParty1 = controllingCustomer1.AllRelatedParties.AddNew();
			relatedParty1.PR_OH_RelatedParty = controllingAgent1.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

			var relatedParty2 = controllingCustomer2.AllRelatedParties.AddNew();
			relatedParty2.PR_OH_RelatedParty = controllingAgent2.PK;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

			var supplierLink = consignee.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = consignor.PK;

			var supplierLinkTransportMode = supplierLink.OrgSupBuyLinkTrnModes[0];
			supplierLinkTransportMode.PF_TransportMode = booking.JS_TransportMode;
			supplierLinkTransportMode.PF_ContainerMode = booking.JS_PackingMode;
			supplierLinkTransportMode.PF_OH_ControllingCustomer = controllingCustomer1.PK;

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.FireSaveButton();

				AssertEquals(controllingCustomer1.PK, booking.ControllingCustomer.PK);
				AssertEquals(controllingAgent1.PK, booking.ControllingAgentDocumentaryAddress.OrganisationPK);

				booking.ControllingCustomerNameOrPK = controllingCustomer2.PK.ToString();

				form.FireSaveButton();

				AssertEquals(controllingCustomer2.PK, booking.ControllingCustomer.PK);
				AssertEquals(controllingAgent2.PK, booking.ControllingAgentDocumentaryAddress.OrganisationPK);
			}
		}

		public void TestForceFormCloseOnSaveConcurrencyException()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "CNSHA";

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			quotedBooking.Job.JH_RatingHasBeenRun = true;
			quotedBooking.Mode = Core.Constants.RateMode.ULD;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "CNSHA";

			Factory.Save();
			Factory.RefreshEnabled = false;

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.ForceFormCloseOnSaveConcurrencyException = true;
				form.Show();

				var anotherFactory = new BusinessObjectFactory();
				anotherFactory.RefreshEnabled = false;
				var quotedBookingInAnotherFactory = anotherFactory.Load<QuotedBooking>(quotedBooking.PK);

				quotedBookingInAnotherFactory.Booking.JS_RL_NKOrigin = "USALX";
				anotherFactory.Save();

				booking.JS_RL_NKOrigin = "AUBNE";

				var isFormClosed = false;

				form.Closed += (sender, args) =>
				{
					isFormClosed = true;
				};

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FireSaveButton();

				AssertEquals("While you have been working with this form, another user has made changes which cannot be merged. This form would be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(isFormClosed);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		#endregion

		#region MakingConsolDoesNotAllocateExtraMAWB

		public void TestMakingConsolDoesNotAllocateExtraMAWB()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_MAWB = "111";
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;

			JobMawb anotherMawb = Factory.NewWithValidTestData<JobMawb>();
			anotherMawb.JM_Airline3DigitPrefix = "081";
			anotherMawb.JM_MAWB = "222";
			anotherMawb.JM_ServiceLevel = "STD";
			anotherMawb.JM_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			QuotedBooking quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			quotedBooking.Booking.JS_IsDirectBooking = true;
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.LoadPort = "AUSYD";
			quotedBooking.DischargePort = "NZAKA";
			quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			quotedBooking.Booking.JS_RL_NKDestination = "NZAKA";

			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "081";
			Factory.Save();

			AssertEquals("Preconditions: Booking has taken the mawb job", quotedBooking.Booking.PK, mawb.JM_ParentID);

			using (QuotedBookingFormForTest bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				AssertEquals("Preconditions: No Consol expected to be on the booking", 0, quotedBooking.Booking.Consols.Count);

				bookingForm.ConsolidateToNewConsol();

				bookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				ForwardingShipment sameBooking = Factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
				AssertEquals("One Consol must have been created and attached to booking", 1, sameBooking.Consols.Count);

				JobMawb[] bookingMawbs = newFactory.Load<JobMawb>(new ZQuery(JobMawbSchema.JM_ParentID, quotedBooking.Booking.PK));
				AssertEquals("No Mawb should be allocated for Booking", 0, bookingMawbs.Length);

				JobMawb sameMawb = newFactory.Load<JobMawb>(mawb.PK);
				AssertEquals("Mawb is attached to Consol", sameBooking.Consols[0].PK, sameMawb.JM_ParentID);
			}
		}

		#endregion

		#region PreAllocation

		public void TestReleaseMawbAllocatedToBookingConsolidateToNewConsol()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_MAWB = "10000011";
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			QuotedBooking quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Mode = Constants.RateMode.LSE;
			quotedBooking.Booking.JS_IsDirectBooking = true;
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.LoadPort = "AUSYD";
			quotedBooking.DischargePort = "NZAKA";
			quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			quotedBooking.Booking.JS_RL_NKDestination = "NZAKA";
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "081";

			Factory.Save();

			AssertEquals("mawb allocated", mawb.JM_ParentID, quotedBooking.Booking.PK);
			AssertEquals("mawb allocated", mawb.JM_ParentTableCode, quotedBooking.Booking.TablePrefix);

			using (QuotedBookingFormForTest bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.ConsolidateToNewConsol();

				mawb = bookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Load<JobMawb>(mawb.PK);

				AssertNotEquals("mawb released", quotedBooking.Booking.PK, mawb.JM_ParentID);
				AssertNotEquals("mawb released", quotedBooking.Booking.TablePrefix, mawb.JM_ParentTableCode);
			}
		}

		public void TestReleaseMawbAllocatedToBookingAddToExistingConsol()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_MAWB = "10000011";
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			QuotedBooking quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			quotedBooking.Booking.JS_IsDirectBooking = false;
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.LoadPort = "AUSYD";
			quotedBooking.DischargePort = "NZAKA";
			quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			quotedBooking.Booking.JS_RL_NKDestination = "NZAKA";

			quotedBooking.ScheduleChooser.MAWBAllocation.PerformMAWBAllocation();
			AssertEquals("mawb not allocated because booking is not direct", ZGuid.Empty, mawb.JM_ParentID);
			AssertEquals("mawb not allocated because booking is not direct", ZString.Empty, mawb.JM_ParentTableCode);

			quotedBooking.Booking.JS_IsDirectBooking = true;
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "081";

			Factory.Save();
			AssertEquals("mawb allocated", mawb.JM_ParentID, quotedBooking.Booking.PK);
			AssertEquals("mawb allocated", mawb.JM_ParentTableCode, quotedBooking.Booking.TablePrefix);

			Factory.Save();

			using (QuotedBookingFormForTest bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				var menuItem = bookingForm.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
				menuItem.PerformClick();

				Assert("message was shown to user", !UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("You cannot add a direct booking to an existing Consol. Please choose 'Consolidate' from the menu to create a new Consol.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("AddToExistingConsol method execution skipped", bookingForm.LastSelectHelper_ForTesting);
			}
		}

		public void TestShowPreAllocation()
		{
			AssertShowPreAllocationUnsavedBooking(GetBooking());
			AssertShowPreAllocationUnsavedBooking(GetQuotedBooking());

			AssertShowPreAllocation(GetSavedBooking());
			AssertShowPreAllocation(GetSavedQuotedBooking());
		}

		public void TestCreateNewQuickBookingValidatesConsignee()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();
				AssertEquals("prerequisite", "AUSYD", quotedBooking.Destination);
				AssertNoErrors("Consignee should not be validated when the new booking is created", quotedBooking.ConsigneeDocumentaryAddress);
			}

			GlbDepartment.CurrentDepartment.GE_Export = true;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();
				AssertEquals("prerequisite", "AUSYD", quotedBooking.Origin);
				AssertNoErrors("Consignee should not be validated when the new booking is created", quotedBooking.ConsigneeDocumentaryAddress);
			}
		}

		void AssertShowPreAllocation(QuotedBooking quotedBooking)
		{
			using (QuotedBookingFormForTest bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.ShowPreAllocation();

				Form preallocationForm = FindPreallocationForm();
				AssertNotNull("Preallocation form shown", preallocationForm);
				preallocationForm.Close();
			}
		}

		void AssertShowPreAllocationUnsavedBooking(QuotedBooking quotedBooking)
		{
			using (QuotedBookingFormForTest bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.ShowPreAllocation();

				Form preallocationForm = FindPreallocationForm();
				AssertNull("Preallocation form not shown", preallocationForm);
				AssertEquals("Please save booking before opening the pre-allocation form.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		Form FindPreallocationForm()
		{
			return Application.OpenForms.Cast<Form>()
				.FirstOrDefault(form => form.GetType() == typeof(PreAllocationForm));
		}

		#endregion

		#region AddToConsol

		public void TestAddToExistingConsol_Click()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "23";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(4);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			QuotedBooking quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Booking.JS_JX = sailing.PK;
			quotedBooking.Booking.JS_GoodsDescription = "TestGoods";

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			string noneMessage = "None ";

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");

				var attachRequestWithErrors = new ShipmentConsolAttachRequest(() => "ERROR?", null);
				helper.Setup(m => m.IsAllowedToAttachConsol(It.IsAny<CommonShipment>(), It.IsAny<CommonConsol>())).Returns(attachRequestWithErrors);

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					AssertEquals("Should have errors", "Error ERROR?", UnitTestUserNotification.Instance.LastMessage.ToString());
					helper.VerifyAll();
				}
			}

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				var attachRequest = new ShipmentConsolAttachRequest(null, null);
				helper.Setup(m => m.IsAllowedToAttachConsol(It.IsAny<CommonShipment>(), It.IsAny<CommonConsol>())).Returns(attachRequest);

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("Should have errors", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					helper.VerifyAll();
				}
			}
		}

		public void TestAddToConsol()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "23";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(4);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			var quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Booking.JS_JX = sailing.PK;
			quotedBooking.Booking.JS_GoodsDescription = "TestGoods";

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
				menuItem.PerformClick();
				Application.DoEvents();

				var popup = form.LastSelectHelper_ForTesting.LastPopupForTesting_ForTesting;
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { consol });

				AssertNotNull("Should have created a controller instance.", form.LastSelectHelper_ForTesting.LastController_ForTesting);
				using (ZForm consolForm = (ZForm)form.LastSelectHelper_ForTesting.LastController_ForTesting.LastShownForm)
				{
					var consolLoadedInForm = (ForwardingConsol)consolForm.BusinessEntity;
					AssertEquals("The form should be editing the consol", consol.PK, consolLoadedInForm.PK);
					AssertEquals("The Consol should contain the shipment", true, consolLoadedInForm.Shipments.Contains(quotedBooking.Booking.PK));

					AssertEquals(false, quotedBooking.Quote.TH_IsLocked);
					AssertEquals(true, quotedBooking.Quote.TH_Accepted.IsEmpty);

					consolLoadedInForm.Factory.Save();

					AssertEquals(true, quotedBooking.Quote.TH_IsLocked);
					AssertEquals(ZDateTime.Today, quotedBooking.Quote.TH_Accepted);

					consolForm.Close();
				}
			}
		}

		public void TestAddToConsol_SpotQuote()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "23";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(4);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			var quotedBooking = GetSavedQuote();
			quotedBooking.TransportMode = Core.Constants.RateMode.AIR;
			quotedBooking.ContainerMode = "LSE";
			quotedBooking.Weight = 10;
			quotedBooking.Volume = 20;

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
				menuItem.PerformClick();
				Application.DoEvents();

				var popup = form.LastSelectHelper_ForTesting.LastPopupForTesting_ForTesting;
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { consol });
				Application.DoEvents();

				AssertNotNull("Should have created a controller instance.", form.LastSelectHelper_ForTesting.LastController_ForTesting);
				using (ZForm consolForm = (ZForm)form.LastSelectHelper_ForTesting.LastController_ForTesting.LastShownForm)
				{
					var consolLoadedInForm = (ForwardingConsol)consolForm.BusinessEntity;
					AssertEquals("The form should be editing the consol", consol.PK, consolLoadedInForm.PK);

					AssertEquals("Weight", 10m, consolLoadedInForm.Shipments[0].JS_ActualWeight);
					AssertEquals("Volume", 20m, consolLoadedInForm.Shipments[0].JS_ActualVolume);
					AssertNull(consolLoadedInForm.Shipments[0].Job);
					consolLoadedInForm.Shipments[0].CreateShipmentJobHeaderWithMutex();
					AssertEquals(quotedBooking.Quote.TH_QuoteNumber, consolLoadedInForm.Shipments[0].Job.JH_TH_NKQuoteNumber);

					AssertEquals(false, quotedBooking.Quote.TH_IsLocked);
					AssertEquals(true, quotedBooking.Quote.TH_Accepted.IsEmpty);

					consolLoadedInForm.Factory.Save();

					AssertEquals(true, quotedBooking.Quote.TH_IsLocked);
					AssertEquals(ZDateTime.Today, quotedBooking.Quote.TH_Accepted);

					consolLoadedInForm.Shipments[0].Job.Dispose();
				}
			}
		}

		#region BookingWithQuote_AttachBookingToShipment

		public void TestConsolidateToNewConsol_BookingWithQuote_AttachBookingToShipment()
		{
			var result = CreateQuotedBookingWithTransportBooking();
			var quotedBooking = result.Item1;
			var tBooking = result.Item2;

			using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				// assert the quote does NOT have any bookings (it should have been moved to the forwarding shipment)
				var tbsRelatedToQBAfterConversion = TransportBookingLoader.GetRelatedTransportBookingEDocs(quotedBooking);
				AssertEquals("Should have been moved to the Forwarding shipment", 0, tbsRelatedToQBAfterConversion.Count());

				// assert the forwarding shipment has the transport booking after quoted booking conversion
				var forwardingShipment = quotedBooking.Booking;
				var tbsRelatedToShipment = TransportBookingLoader.GetRelatedTransportBookingEDocs(forwardingShipment);
				var shipmentTransportBooking = (IDtbBooking)tbsRelatedToShipment.Single(d => d is IDtbBooking);
				var shipmentTransportConsolidation = (IDtbBookingConsolidation)tbsRelatedToShipment.Single(d => d is IDtbBookingConsolidation);
				AssertEquals("Should be the booking from quote", tBooking, shipmentTransportBooking);
				AssertEquals("Should be the booking consolidation from quote", shipmentTransportConsolidation.PK, shipmentTransportBooking.KM_KB_Booking);
				AssertEquals(JobShipmentSchema.Constants.Prefix, shipmentTransportConsolidation.KB_ParentTableCode);
				AssertEquals(forwardingShipment.PK, shipmentTransportConsolidation.KB_ParentID);
			}
		}

		public void TestConsolidateAddToConsol_BookingWithQuote_AttachBookingToShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var result = CreateQuotedBookingWithTransportBooking();
			var quotedBooking = result.Item1;
			var tBooking = result.Item2;

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
				menuItem.PerformClick();
				Application.DoEvents();

				var popup = form.LastSelectHelper_ForTesting.LastPopupForTesting_ForTesting;
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { consol });
				Application.DoEvents();

				quotedBooking.Factory.Save();
				Factory.Save();

				AssertNotNull("Should have created a controller instance.", form.LastSelectHelper_ForTesting.LastController_ForTesting);
				using (var consolForm = (ZForm)form.LastSelectHelper_ForTesting.LastController_ForTesting.LastShownForm)
				{
					var consolLoadedInForm = (ForwardingConsol)consolForm.BusinessEntity;
					AssertEquals("The form should be editing the consol", consol.PK, consolLoadedInForm.PK);

					Application.DoEvents();
					Factory.Save();

					// assert the forwarding shipment has the transport booking after quoted booking conversion
					var forwardingShipment = consolLoadedInForm.Shipments[0];
					var tbsRelatedToShipment = TransportBookingLoader.GetRelatedTransportBookingEDocs(forwardingShipment);
					var shipmentTransportBooking = (IDtbBooking)tbsRelatedToShipment.Single(d => d is IDtbBooking);
					var shipmentTransportConsolidation = (IDtbBookingConsolidation)tbsRelatedToShipment.Single(d => d is IDtbBookingConsolidation);
					AssertEquals(JobShipmentSchema.Constants.Prefix, shipmentTransportConsolidation.KB_ParentTableCode);
					AssertEquals(forwardingShipment.PK, shipmentTransportConsolidation.KB_ParentID);
				}
			}
		}

		Tuple<QuotedBooking, IDtbBooking> CreateQuotedBookingWithTransportBooking()
		{
			// create quoted booking
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "NZAKL";
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			// create transport booking for quoted booking
			var tConsolidation = (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			tConsolidation.KB_ParentTableCode = quotedBooking.TablePrefix;
			tConsolidation.KB_ParentID = quotedBooking.PK;
			var tBooking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			tBooking.KM_KB_Booking = tConsolidation.PK;

			var tbsRelatedToQB = TransportBookingLoader.GetRelatedTransportBookingEDocs(quotedBooking);
			var tConsolidationRelatedToQB = (IDtbBookingConsolidation)tbsRelatedToQB.Single(d => d is IDtbBookingConsolidation);
			AssertNotNull(tConsolidationRelatedToQB);
			AssertEquals(ViewQuotedBookingSchema.Constants.Prefix, tConsolidationRelatedToQB.KB_ParentTableCode);
			AssertEquals(quotedBooking.PK, tConsolidationRelatedToQB.KB_ParentID);

			Factory.Save();
			return new Tuple<QuotedBooking, IDtbBooking>(quotedBooking, tBooking);
		}

		#endregion

		public void TestAddToConsolInSpotQuoteNotGenerateErrorIfConsolFormIsOpen()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "23";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(4);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			QuotedBooking quotedBooking = GetSavedQuote();
			quotedBooking.Mode = Core.Constants.RateMode.AIR;
			quotedBooking.Weight = 10;
			quotedBooking.Volume = 20;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var consolInSecond = factory2.Load<ForwardingConsol>(consol.PK);

			ZController consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var consolForm = (ConsolForm)consolController.ShowEditForm(consolInSecond))
			{
				consolForm.Show();
				using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
				{
					form.Show();
					Application.DoEvents();

					var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
					menuItem.PerformClick();

					using (ZFormModaliser.LastFormShownForTest)
					{
						EmbeddedModulePopup popup = form.LastSelectHelper_ForTesting.LastPopupForTesting_ForTesting;
						popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { consol });
						Application.DoEvents();
					}
				}
			}

			Assert(true);
		}

		public void TestMakingConsol_SpotQuote()
		{
			var quotedBooking = GetSavedQuote();
			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			quotedBooking.Weight = 10;
			quotedBooking.Volume = 20;
			Factory.Save();

			using (var bookingForm = new QuotedBookingForm(quotedBooking))
			{
				var addToConsolButtonInfo = typeof(QuotedBookingForm).GetMethod("ConsolidateToNewConsol", BindingFlags.Instance | BindingFlags.NonPublic);
				addToConsolButtonInfo.Invoke(bookingForm, Array.Empty<object>());
				Application.DoEvents();

				AssertNotNull("Should have created a controller instance.", bookingForm.PopupForm_ForTesting);
				using (var consolForm = bookingForm.PopupForm_ForTesting)
				{
					var consolLoadedInForm = (ForwardingConsol)consolForm.BusinessEntity;

					AssertEquals("Weight", 10m, consolLoadedInForm.Shipments[0].JS_ActualWeight);
					AssertEquals("Volume", 20m, consolLoadedInForm.Shipments[0].JS_ActualVolume);
					AssertNull(consolLoadedInForm.Shipments[0].Job);
					consolLoadedInForm.Shipments[0].CreateShipmentJobHeaderWithMutex();
					AssertEquals(quotedBooking.Quote.TH_QuoteNumber, consolLoadedInForm.Shipments[0].Job.JH_TH_NKQuoteNumber);

					AssertEquals(false, quotedBooking.Quote.TH_IsLocked);
					AssertEquals(true, quotedBooking.Quote.TH_Accepted.IsEmpty);

					consolLoadedInForm.Factory.Save();

					AssertEquals(true, quotedBooking.Quote.TH_IsLocked);
					AssertEquals(ZDateTime.Today, quotedBooking.Quote.TH_Accepted);

					consolLoadedInForm.Shipments[0].Job.Dispose();
				}
			}
		}

		#endregion

		#region AddBookingToConsolIsNotAllowedForDirectBookings

		public void TestAddBookingToConsolIsNotAllowedForDirectBookings()
		{
			QuotedBooking quotedBooking = GetSavedQuotedBooking();
			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBooking.Booking.JS_IsDirectBooking = true;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var menuItem = bookingForm.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "You cannot add a direct booking to an existing Consol. Please choose 'Consolidate' from the menu to create a new Consol.", UnitTestUserNotification.Instance.LastMessage.Text);

				quotedBooking.Booking.JS_IsDirectBooking = false;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				Assert("No such Message expected", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region Delivery Due Date

		public void TestCalculateDeliveryDueDateMenuItem()
		{
			var booking = GetBooking();

			void AssertCalculateDDDMenuItem(string message, bool shouldBeAvailable)
			{
				using (var form = new QuotedBookingForm(booking))
				{
					form.Show();
					var calculateDeliveryDueDateMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Calculate Delivery Due Date");
					AssertEquals(message, shouldBeAvailable, calculateDeliveryDueDateMenuItem != null);
				}
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = true;
				AssertCalculateDDDMenuItem("Calculate Delivery Due Date is not available when registry is disabled", false);
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = false;
				AssertCalculateDDDMenuItem("Calculate Delivery Due Date is available when registry is enabled and user does not have security right", true);

				Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = true;
				AssertCalculateDDDMenuItem("Calculate Delivery Due Date is available when registry is enabled and user has security right", true);
			}
		}

		public void TestRecordDeliveryDateUpdatedEvent_ActionMenuItemClick()
		{
			var newDeliveryDueDate = new ZDateTime(2022, 10, 04, 09, 30, 0);
			DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(newDeliveryDueDate);

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var booking = GetBooking();

				Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = true;
				using (var form = new QuotedBookingForm(booking))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("calculate DDD from action menu item");

					var calculateDeliveryDueDateMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Calculate Delivery Due Date");
					AssertNotNull("Pre-condition: Calculate Delivery Due Date menu item is available", calculateDeliveryDueDateMenuItem);
					calculateDeliveryDueDateMenuItem.PerformClick();

					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					var ddeEvent = booking.Logs.MostRecentLogByEventTime(AutoEvents.DeliveryDateUpdated);

					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Change Reason", lastMessage.Caption);
						AssertEquals("Check message text", "Enter the reason for changing the Delivery Due Date.\r\nThe reason will be recorded on the DDE-Delivery Date Updated event for future reference.", lastMessage.Text);
						AssertEquals("DeliveryDueDate should be set after changed", newDeliveryDueDate, booking.DeliveryDueDate);
						AssertEquals("new DDE event created", 1, booking.Booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DeliveryDateUpdatedCode).Count());
						AssertEquals("DDE event Reference", "|ACT=Manual|NEW=04-Oct-22 09:30|RES=calculate DDD from action menu item|TYP=Original", ddeEvent.SL_Reference);
					});
				}

				Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = false;
				using (var form = new QuotedBookingForm(booking))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("calculate DDD from action menu item");

					var calculateDeliveryDueDateMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Calculate Delivery Due Date");
					AssertNotNull("Pre-condition: Calculate Delivery Due Date menu item is available", calculateDeliveryDueDateMenuItem);
					calculateDeliveryDueDateMenuItem.PerformClick();

					AssertEquals("Show error message when user does nor have security right",
						"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:" +
						"\r\n\r\nOperate -> Forwarding -> Bookings -> Edit -> Allow override of Delivery Due Date",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestNotificationIsShownIfCalculatedDateNotChanged_ActionMenuItemClick()
		{
			// Arrange
			var newDeliveryDueDate = new ZDateTime(2022, 11, 21, 12, 0, 0);
			DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(newDeliveryDueDate);

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = true;
				var booking = GetBooking();

				using (var form = new QuotedBookingForm(booking))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("Calculate DDD");

					booking.DeliveryDueDate = newDeliveryDueDate;

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					// Act
					form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Calculate Delivery Due Date").PerformClick();

					// Assert
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					CombineAssertions(() =>
					{
						AssertEquals("Notification caption is not correct", "DDD Calculation Result", lastMessage.Caption);
						AssertEquals("Notification message is not correct", "The Delivery Due Date (DDD) has not been changed based on data entered.", lastMessage.Text);
					});
				}
			}
		}

		public void TestRecordDeliveryDateUpdatedEvent_OverrideDeliveryDueDate()
		{
			var newDeliveryDueDate = new ZDateTime(2022, 10, 04, 09, 30, 0);
			DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(newDeliveryDueDate);

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = true;
				var booking = GetBooking();

				using (var form = new QuotedBookingForm(booking))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("calculate DDD from action menu item");

					booking.DeliveryDueDate = newDeliveryDueDate;

					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					var ddeEvent = booking.Logs.MostRecentLogByEventTime(AutoEvents.DeliveryDateUpdated);

					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Change Reason", lastMessage.Caption);
						AssertEquals("Check message text", "Enter the reason for changing the Delivery Due Date.\r\nThe reason will be recorded on the DDE-Delivery Date Updated event for future reference.", lastMessage.Text);
						AssertEquals("DeliveryDueDate should be set after changed", newDeliveryDueDate, booking.DeliveryDueDate);
						AssertEquals("new DDE event created", 1, booking.Booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DeliveryDateUpdatedCode).Count());
						AssertEquals("DDE event Reference", "|ACT=Override|NEW=04-Oct-22 09:30|RES=calculate DDD from action menu item|TYP=Original", ddeEvent.SL_Reference);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					booking.DeliveryDueDate = newDeliveryDueDate;
					Assert("Should not prompt message for change reason input when new value equals original value", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		#endregion

		#region RecalculateRelatedParties

		public void TestRecalculateRelatedParties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Namibia))
			{
				var booking = GetBooking();
				booking.Booking.JS_RL_NKOrigin = "AUSYD";
				booking.Booking.JS_RL_NKDestination = "CNSHA";

				ChildEditableService.SetState(booking.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new QuotedBookingForm(booking))
				{
					form.Show();
					var calculateDeliveryDueDateMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Recalculate Related Parties for logged in Company");
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Should show company does not match message",
						"You cannot Recalculate Related Parties for this company because the company does not match Pickup or Delivery direction of this job.",
						UnitTestUserNotification.Instance.LastMessage.Text);

					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					booking.Booking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

					booking.Booking.JS_RL_NKOrigin = "NASWP";

					var oldValue = Factory.NewWithValidTestData<OrgHeader>();
					booking.Booking.JS_OH_ExportBroker = oldValue.PK;

					var exportBrokerRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
					consignor.AddRelatedParty(exportBrokerRelatedParty.PK,
						RelatedPartyTypeList.Codes.CustomsAgentBroker,
						RelatedPartyDirectionList.Codes.Pickup,
						Constants.TransportModes.All,
						ZString.Empty, GlbCompany.CurrentCompany);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Should show already has value message",
						"Export Broker has already been entered. Do you wish to update the Export Broker based on your Company Related Party Configuration?",
						UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Response is defaultable.", UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
					AssertEquals("Export Broker should have its old value", oldValue.PK, booking.Booking.JS_OH_ExportBroker);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Should show already has value message",
						"Export Broker has already been entered. Do you wish to update the Export Broker based on your Company Related Party Configuration?",
						UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Response is defaultable.", UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
					AssertEquals("Export Broker should have its old value", exportBrokerRelatedParty.PK, booking.Booking.JS_OH_ExportBroker);

					UnitTestUserNotification.Instance.ClearMessages();
					booking.Booking.JS_OH_ExportBroker = ZGuid.Empty;
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Export Broker should be defaulted by Related Party", exportBrokerRelatedParty.PK, booking.Booking.JS_OH_ExportBroker);
					AssertEquals("Should not show already has value message", null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestRecalculateRelatedParties_RefreshDeliveryAgert()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Namibia))
			{
				var booking = GetBooking();
				booking.Booking.JS_RL_NKOrigin = "AUSYD";
				booking.Booking.JS_RL_NKDestination = "NASWP";

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				booking.Booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				var oldDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
				booking.Booking.JS_OH_DeliveryAgent = oldDeliveryAgent.PK;

				var newDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
				consignee.AddRelatedParty(newDeliveryAgent.PK,
					RelatedPartyTypeList.Codes.DeliveryAgent,
					RelatedPartyDirectionList.Codes.Delivery,
					Constants.TransportModes.All,
					ZString.Empty, GlbCompany.CurrentCompany);

				ChildEditableService.SetState(booking.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new QuotedBookingForm(booking))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.Show();

					var calculateDeliveryDueDateMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Recalculate Related Parties for logged in Company");
					calculateDeliveryDueDateMenuItem.PerformClick();

					var deliveryAgentAddressControl = form.Controls.Find("DeliveryAgentBoundOrganisationControl", true)[1] as ZOrganisationControl;
					var orgFindBox = deliveryAgentAddressControl.Controls.Find("fOrganisationFindBox", true)[0] as ZOrganisationFindBox.Bare;
					var codeBox = orgFindBox.Controls.Find("CodeBox", true)[0] as ZFindBoxUserControl.ZCodeBox;
					AssertEquals(newDeliveryAgent.OH_Code, codeBox.Text);
				}
			}
		}

		public void TestRecalculateRelatedParties_RefreshControllingCustomer()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Namibia))
			{
				var booking = GetBooking();
				booking.Booking.JS_RL_NKOrigin = "AUSYD";
				booking.Booking.JS_RL_NKDestination = "NASWP";

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				booking.Booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				var oldControllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				booking.Booking.ControllingCustomerAddress.E2_OA_Address = oldControllingCustomer.MainAddress.PK;

				booking.Booking.JS_INCO = Constants.IncoTerms.FreeCarrier;

				var newControllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				consignee.AddRelatedParty(newControllingCustomer.PK,
					RelatedPartyTypeList.Codes.ControllingCustomer,
					RelatedPartyDirectionList.Codes.Delivery,
					Constants.TransportModes.All,
					ZString.Empty, GlbCompany.CurrentCompany);

				ChildEditableService.SetState(booking.Factory, ChildEditableServiceStates.Shipment);

				using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new QuotedBookingForm(booking))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.Show();

					var calculateDeliveryDueDateMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Recalculate Related Parties for logged in Company");
					calculateDeliveryDueDateMenuItem.PerformClick();

					var controllingCustomerAddressControl = form.Controls.Find("ControllingCustomerAddressControl", true)[1] as ZDocAddressControl;
					var orgFindBox = controllingCustomerAddressControl.Controls.Find("CutDownSingleLineOrgFindBox", true)[0] as ZOrganisationFindBox.Bare;
					var codeBox = orgFindBox.Controls.Find("CodeBox", true)[0] as ZFindBoxUserControl.ZCodeBox;
					AssertEquals(newControllingCustomer.OH_Code, codeBox.Text);
				}
			}
		}

		public void TestRecalculateRelatedParties_RefreshControllingAgent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Namibia))
			{
				var booking = GetBooking();
				booking.Booking.JS_RL_NKOrigin = "AUSYD";
				booking.Booking.JS_RL_NKDestination = "NASWP";

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				booking.Booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				var oldControllingAgent = Factory.NewWithValidTestData<OrgHeader>();
				booking.Booking.ControllingAgentDocumentaryAddress.E2_OA_Address = oldControllingAgent.MainAddress.PK;

				booking.Booking.JS_INCO = Constants.IncoTerms.FreeCarrier;

				var newControllingAgent = Factory.NewWithValidTestData<OrgHeader>();
				consignee.AddRelatedParty(newControllingAgent.PK,
					RelatedPartyTypeList.Codes.ControllingAgent,
					RelatedPartyDirectionList.Codes.Sales,
					Constants.TransportModes.All,
					ZString.Empty, GlbCompany.CurrentCompany);

				ChildEditableService.SetState(booking.Factory, ChildEditableServiceStates.Shipment);

				using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new QuotedBookingForm(booking))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.Show();

					var calculateDeliveryDueDateMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Recalculate Related Parties for logged in Company");
					calculateDeliveryDueDateMenuItem.PerformClick();

					var controllingCustomerAddressControl = form.Controls.Find("ControllingAgentAddressControl", true)[1] as ZDocAddressControl;
					var orgFindBox = controllingCustomerAddressControl.Controls.Find("CutDownSingleLineOrgFindBox", true)[0] as ZOrganisationFindBox.Bare;
					var codeBox = orgFindBox.Controls.Find("CodeBox", true)[0] as ZFindBoxUserControl.ZCodeBox;
					AssertEquals(newControllingAgent.OH_Code, codeBox.Text);
				}
			}
		}

		public void TestActionMenuItems()
		{
			var booking = GetBooking();

			ChildEditableService.SetState(booking.Factory, ChildEditableServiceStates.Shipment);

			using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(booking))
			{
				form.Show();

				form.Menu.MenuItems.FindByText("Actions").OnPopup(EventArgs.Empty);

				AssertNotNull(form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Selection of Rate Commodity (with FMC Tariff ID)"));
				AssertNotNull(form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Recalculate Related Parties for logged in Company"));
			}
		}

		#endregion

		public void TestAddBookingToConsolForShipmentStatus()
		{
			var quotedBooking = GetSavedQuotedBooking();

			using (var bookingForm = new QuotedBookingForm(quotedBooking))
			{
				var menuItem = bookingForm.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");

				quotedBooking.Booking.JS_IsDirectBooking = false;
				quotedBooking.ShipmentStatus = null;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				Assert("No such Message expected", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestConvertToShipmentForShipmentStatus()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();

			using (var bookingForm = new QuotedBookingForm(quotedBooking))
			{
				var menuItem = bookingForm.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");

				quotedBooking.ShipmentStatus = null;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Shipment Form should not show", bookingForm.PopupForm_ForTesting);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Shipment Form should not show", bookingForm.PopupForm_ForTesting);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Shipment Form should not show", bookingForm.PopupForm_ForTesting);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotNull("Shipment Form should show", bookingForm.PopupForm_ForTesting);
				bookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();
				bookingForm.PopupForm_ForTesting.Close();
			}
		}

		public void TestResynchronizeScreeningStatusOnFormLoad()
		{
			var quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();

			quotedBooking.Client.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			quotedBooking.Consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			quotedBooking.Consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, quotedBooking.Booking.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Unknown, quotedBooking.Booking.JS_ScreeningStatus);
			});

			using (var bookingForm = new QuotedBookingForm(quotedBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.Show();

				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.Clear, quotedBooking.Booking.JS_ScreeningStatus);
					AssertEquals(1, quotedBooking.Booking.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Count());
					AssertContains("|NEW=CLR|OLD=UNK|TYP=SYNC", quotedBooking.Booking.Logs.MostRecentLogByEventTime(Events.DeniedPartyStatusUpdated).SL_Reference);
				});
			}

			quotedBooking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (var bookingForm = new QuotedBookingForm(quotedBooking))
			{
				bookingForm.Show();
				AssertEquals(ScreeningStatusesList.Codes.Matched, quotedBooking.Booking.JS_ScreeningStatus);
				AssertEquals("The number of logs is still 1.", 1, quotedBooking.Booking.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Count());
			}
		}

		public void TestResynchronizeScreeningStatusOnFormLoad_NoDeveloperNotificationExceptionThrown()
		{
			var quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();

			quotedBooking.Client.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			quotedBooking.Consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			quotedBooking.Consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				using (TestingState.SuspendIsRunningTests())
				using (var bookingForm = new QuotedBookingForm(quotedBooking))
				{
					bookingForm.Show();
					AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
					AssertEquals("Booking screening status CLR.", ScreeningStatusesList.Codes.Clear, quotedBooking.Booking.JS_ScreeningStatus);
				}
			});
		}

		public void TestNoNeedToResynchronizeScreeningStatusOnFormLoad()
		{
			var quotedBooking = GetSavedQuotedBooking();
			quotedBooking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			quotedBooking.Client.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			quotedBooking.Consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			quotedBooking.Consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, quotedBooking.Booking.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Clear, quotedBooking.Booking.JS_ScreeningStatus);
			});

			using (var bookingForm = new QuotedBookingForm(quotedBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.Show();

				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.Clear, quotedBooking.Booking.JS_ScreeningStatus);
					AssertEquals(false, quotedBooking.Booking.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Any());
				});
			}
		}

		public void TestConsolidate_Concurrency()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var quotedBooking2 = newFactory.Load<QuotedBooking>(quotedBooking.PK);

			using (var bookingForm = new QuotedBookingForm(quotedBooking))
			using (var reloadBookingForm = new QuotedBookingForm(quotedBooking2))
			{
				var reloadMenuItem = reloadBookingForm.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Consolidate");
				reloadMenuItem.PerformClick();
				AssertNotNull("Consol Form should show", reloadBookingForm.PopupForm_ForTesting);
				reloadBookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();
				reloadBookingForm.PopupForm_ForTesting.Close();

				var menuItem = bookingForm.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Consolidate");
				menuItem.PerformClick();
				var expectedMessage = "Quote has already been converted to a Booking with Quote. Please Close and Reopen.";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidateToNewConsolForShipmentStatus()
		{
			var quotedBooking = GetSavedQuotedBooking();

			using (var bookingForm = new QuotedBookingForm(quotedBooking))
			{
				var menuItem = bookingForm.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Consolidate");

				quotedBooking.ShipmentStatus = null;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Consol Form should not show", bookingForm.PopupForm_ForTesting);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Consol Form should not show", bookingForm.PopupForm_ForTesting);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Consol Form should not show", bookingForm.PopupForm_ForTesting);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotNull("Consol Form should show", bookingForm.PopupForm_ForTesting);
				bookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();
				bookingForm.PopupForm_ForTesting.Close();
			}
		}

		public void TestConsolidateToNewConsolBeforeCreateShipmentFromSameBooking()
		{
			var quotedBooking = GetSavedQuotedBooking();
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			using (var bookingForm1 = new QuotedBookingForm(quotedBooking))
			using (var bookingForm2 = new QuotedBookingForm(quotedBooking))
			{
				var consolidateMenuItem = bookingForm1.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Consolidate");
				consolidateMenuItem.PerformClick();
				using (var consolidateForm = bookingForm1.PopupForm_ForTesting)
				{
					var forwardingConsolidation = consolidateForm.BusinessEntity as ForwardingConsol;
					AssertNotNull(forwardingConsolidation);
					AssertEquals(0, forwardingConsolidation.Shipments.FirstOrDefault().RowErrors.Count());

					var toShipmentMenuItem = bookingForm2.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
					toShipmentMenuItem.PerformClick();
					using (var shipmentForm = bookingForm2.PopupForm_ForTesting)
					{
						shipmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.JobInvoicing);

						var shipment = shipmentForm.DataSource as ForwardingShipment;
						var jobHeader = shipment.InvoicingSupporter.Job;
						AssertNull(jobHeader);
					}
				}
			}
		}

		public void TestCreateShipmentBeforeConsolidateToNewConsolFromSameBooking()
		{
			var quotedBooking = GetSavedQuotedBooking();
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			using (var bookingForm1 = new QuotedBookingForm(quotedBooking))
			using (var bookingForm2 = new QuotedBookingForm(quotedBooking))
			{
				var toShipmentMenuItem = bookingForm2.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				toShipmentMenuItem.PerformClick();
				using (var shipmentForm = bookingForm2.PopupForm_ForTesting)
				{
					shipmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.JobInvoicing);
					var shipment = shipmentForm.DataSource as ForwardingShipment;
					var jobHeader = shipment.InvoicingSupporter.Job;
					AssertNotNull(jobHeader);

					var consolidateMenuItem = bookingForm1.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Consolidate");
					consolidateMenuItem.PerformClick();
					using (var consolidateForm = bookingForm1.PopupForm_ForTesting)
					{
						var forwardingConsolidation = consolidateForm.BusinessEntity as ForwardingConsol;
						var forwardingShipment = forwardingConsolidation.Shipments.FirstOrDefault() as CommonShipment;
						var expectRowError = $@"You have created the job {forwardingShipment.JS_UniqueConsignRef} on another form, but haven't saved it yet.
Please close or save other forms that use job {forwardingShipment.JS_UniqueConsignRef} to continue.";
						AssertCollectionContains(expectRowError, forwardingShipment.RowErrors.Select(x => x.Message));

						quotedBooking.RunPreSaveValidation();
						AssertCollectionContains(expectRowError, forwardingShipment.RowErrors.Select(x => x.Message));
					}
				}
			}
		}

		public void TestConsolidateBookingFromDifferentCompany()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			quotedBooking.Booking.JS_RL_NKDestination = "CNSHA";
			quotedBooking.Booking.JS_TransportMode = "SEA";
			quotedBooking.Booking.JS_ScreeningStatus = "CLR";
			new JobHeader.Loader(quotedBooking).TryCreate();
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_Code = "DMO";
			var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.GB_GC = anotherCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, anotherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory();
				using var bookingForm = new QuotedBookingForm(quotedBooking);
				bookingForm.Show();
				var consolidateMenuItem = bookingForm.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Consolidate");
				consolidateMenuItem.PerformClick();
				Application.DoEvents();

				using var consolidateForm = bookingForm.PopupForm_ForTesting;
				var forwardingConsolidation = consolidateForm.BusinessEntity as ForwardingConsol;
				var forwardingShipment = forwardingConsolidation.Shipments.FirstOrDefault() as CommonShipment;
				AssertEquals(0, forwardingShipment.RowErrors.Count());
			}
		}

		#region TestConvertToShipment

		public void TestConvertToShipment_NotAddedToOneOffQuotes()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quickBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");

				AssertNull("One off Quotes have no booking to convert so this menu is not applicable", menuItem);
			}
		}

		public void TestConvertToShipment_QuickBooking()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var exception = quickBooking.WorkflowItems.Exceptions.AddNew();
			AssertEquals(false, exception.IsExceptionActioned);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);

				AssertEquals(true, exception.IsExceptionActioned);
			}
		}

		public void TestConvertToShipment_WhenDeniedPartyStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(true,
					"You are about to convert a Booking to Shipment while the screen status is not CLR - Clear. Do you want to proceed?",
					"MAT", DPSFreightMovementRestrictionsOptions.Codes.All);
				AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(true,
					"You are about to convert a Booking to Shipment while the screen status is not CLR - Clear. Do you want to proceed?",
					"MAT", DPSFreightMovementRestrictionsOptions.Codes.All, dialogResult: DialogResult.No);
				AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(true,
					"You are about to convert a Booking to Shipment while the screen status is not CLR - Clear. Do you want to proceed?",
					"MAT", DPSFreightMovementRestrictionsOptions.Codes.Exp);
			}

			AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(true,
				null,
				"MAT", DPSFreightMovementRestrictionsOptions.Codes.Exp, expectedIsExported: false, destination: "AUFRE");
			AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(true,
				null,
				"MAT", DPSFreightMovementRestrictionsOptions.Codes.No);
			AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(false,
				null,
				"CLR", DPSFreightMovementRestrictionsOptions.Codes.All);
			AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(false,
				null,
				"JCL", DPSFreightMovementRestrictionsOptions.Codes.All);

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(false,
					string.Format(CultureInfo.InvariantCulture, "Error: You cannot convert a Booking to Shipment while the screen status is not CLR - Clear or JCL - Job Cleared.\r\nDetailed Information: {0}.", Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr.ErrorMessageForNotAllowed),
					"MAT", DPSFreightMovementRestrictionsOptions.Codes.All);

				AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(true,
					"You are about to convert a Booking to Shipment while the screen status is not CLR - Clear or JCL - Job Cleared. Do you want to proceed?",
					"MAT", DPSFreightMovementRestrictionsOptions.Codes.All, dialogResult: DialogResult.No);

				AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(false,
					string.Format(CultureInfo.InvariantCulture, "Error: You cannot convert a Booking to Shipment while the screen status is not CLR - Clear or JCL - Job Cleared.\r\nDetailed Information: {0}.", Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr.ErrorMessageForNotAllowed),
					"MAT", DPSFreightMovementRestrictionsOptions.Codes.All, dialogResult: DialogResult.No);
			}
		}

		public void TestConvertToShipment_RegistryEnableComplianceWiseIsON_WhenOverallComplianceRiskIsPotentialRiskShowMessage()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			AssertEquals("Booking should be Export", true, booking.IsExport());

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				AssertEquals(@"You are about to convert a Booking to Shipment while the Job Compliance Status is not Clear or Override Clear.
Do you want to proceed?"
				, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestConsolidate_RegistryEnableComplianceWiseIsON_WhenOverallComplianceRiskIsPotentialRiskShowMessage()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			AssertEquals("Booking should be Export", true, booking.IsExport());

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Consolidate");
				menuItem.PerformClick();

				AssertEquals(@"You are about to consolidate while the Job Compliance Status is not Clear or Override Clear.
Do you want to proceed?"
				, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertToShipment_RegistryEnableComplianceWiseIsON_WithoutTheOverrideFreightMovementRestrictionsSecurityRight()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			AssertEquals("Booking should be Export", true, booking.IsExport());

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();

			Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed = false;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,ComplianceWiseRegistryHelper.SetValue(true)))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Error: You cannot convert a Booking to Shipment while the Job Compliance Status is not Clear or Override Clear.\r\nDetailed Information: {0}.", Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions.ErrorMessageForNotAllowed), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidate_RegistryEnableComplianceWiseIsON_WithoutTheOverrideFreightMovementRestrictionsSecurityRight()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			AssertEquals("Booking should be Export", true, booking.IsExport());

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();

			Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed = false;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Consolidate");
				menuItem.PerformClick();

				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Error: You cannot consolidate while the Job Compliance Status is not Clear or Override Clear.\r\nDetailed Information: {0}.", Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions.ErrorMessageForNotAllowed), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertToShipment_QuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var customValue1 = AddCustomValueToQuote(quote, "custom1", "AAA");
			var customValue2 = AddCustomValueToQuote(quote, "custom2", "111");

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var exception = quotedBooking.WorkflowItems.Exceptions.AddNew();
			AssertEquals(false, exception.IsExceptionActioned);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				AssertEquals("AAA", quotedBooking.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
				AssertEquals("111", quotedBooking.GetCustomField("custom2", AddOnColumnDataType.Codes.String));

				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);

				var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK);
				query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, shipment.TablePrefix);
				var result = Factory.Load<GenCustomAddOnValue>(query).OrderBy(x => x.XV_Name).ToArray();

				AssertEquals(2, result.Length);
				CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom1"), customValue1);
				CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom2"), customValue2);
				AssertEquals(true, exception.IsExceptionActioned);
			}

			var quotedBookingController = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			using (var form = quotedBookingController.ShowEditForm(quotedBooking))
			{
				AssertEquals("Quoted Booking form should be open in an edit mode, although it has already been consolidated.", ODisplayMode.Edit, form.DisplayMode);

				AssertEquals("AAA", quotedBooking.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
				AssertEquals("111", quotedBooking.GetCustomField("custom2", AddOnColumnDataType.Codes.String));
			}
		}

		public void TestConvertToShipment_WhenSecurityRightsIsDenied_ShouldNoErrorThrown()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "C99";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "B99";
			branch.GB_GC = company.PK;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "S99";
			staff.GS_LoginName = "test99";
			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				AssertNoExceptionThrown(() => menuItem.PerformClick());
			}
		}

		public void TestConvertToShipment_QuickBooking_CFSJobInvoicingSecurityCheckpoint()
		{
			var org = SetupOrgProxy();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			booking.JS_OA_ExportReceivingDepot = org.MainAddress.PK;
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);
				Assert("JS_IsCFSRegistered should be true", shipment.JS_IsCFSRegistered);

				var billingPlugIn = form.PopupForm_ForTesting.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertCFSJobInvoicingSecurityCheckpoint(billingPlugIn);
			}
		}

		public void TestConvertToShipment_QuotedBooking_CFSJobInvoicingSecurityCheckpoint()
		{
			var org = SetupOrgProxy();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			booking.JS_OA_ExportReceivingDepot = org.MainAddress.PK;
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);
				Assert("JS_IsCFSRegistered should be true", shipment.JS_IsCFSRegistered);

				var billingPlugIn = form.PopupForm_ForTesting.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertCFSJobInvoicingSecurityCheckpoint(billingPlugIn);
			}
		}

		public void TestConvertToShipment_QuotedBooking_CustomFieldsAreLoaded()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var exception = quotedBooking.WorkflowItems.Exceptions.AddNew();
			AssertEquals(false, exception.IsExceptionActioned);

			var customValue1 = AddCustomValueToQuote(quote, "custom1", "AAA");
			var customValue2 = AddCustomValueToQuote(quote, "custom2", "111");

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				AssertEquals("AAA", quotedBooking.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
				AssertEquals("111", quotedBooking.GetCustomField("custom2", AddOnColumnDataType.Codes.String));

				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				var shipmentControl = form.PopupForm_ForTesting.Controls.Find("ShipmentCustomFields", true).FirstOrDefault();
				var layoutControl = shipmentControl.Controls.Find("rowLayoutPanel", true).FirstOrDefault();
				var customFields = layoutControl.Controls.ToList<ZTextBox>();

				AssertEquals(2, customFields.Count);
				AssertNotNull(customFields.Find(x => x.Text == customValue1.XV_Data));
				AssertNotNull(customFields.Find(x => x.Text == customValue2.XV_Data));
			}
		}

		public void TestConvertToShipment_QuickBooking_CustomFieldsAreLoaded()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var exception = quickBooking.WorkflowItems.Exceptions.AddNew();
			AssertEquals(false, exception.IsExceptionActioned);

			var customValue1 = AddCustomValueToQuote(booking, "custom1", "AAA");
			var customValue2 = AddCustomValueToQuote(booking, "custom2", "111");

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				var shipmentControl = form.PopupForm_ForTesting.Controls.Find("ShipmentCustomFields", true).FirstOrDefault();
				var layoutControl = shipmentControl.Controls.Find("rowLayoutPanel", true).FirstOrDefault();
				var customFields = layoutControl.Controls.ToList<ZTextBox>();

				AssertEquals(2, customFields.Count);
				AssertNotNull(customFields.Find(x => x.Text == customValue1.XV_Data));
				AssertNotNull(customFields.Find(x => x.Text == customValue2.XV_Data));
			}
		}

		void AssertCFSJobInvoicingSecurityCheckpoint(ZPlugIn billingPlugIn)
		{
			AssertNotNull(billingPlugIn);

			var securityCheckpoint = billingPlugIn.SecurityCheckpoint;
			AssertNotNull(securityCheckpoint);
			AssertEquals("JobInvoicingSecurityCheckpoint should be updated", "MaintainShipmentJobInvoicing", securityCheckpoint.Code);

			var plugInType = billingPlugIn.GetType().BaseType?.BaseType;
			AssertNotNull(plugInType);

			var fieldInfo = plugInType.GetField("SecurityCheckpointEdit", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull(fieldInfo);

			var securityCheckpointEdit = fieldInfo.GetValue(billingPlugIn) as SecurityCheckpoint;
			AssertNotNull(securityCheckpointEdit);
			AssertEquals("JobInvoicingSecurityCheckpoint should be None", "None", securityCheckpointEdit.Code);
		}

		GenCustomAddOnValue AddCustomValueToQuote(BusinessObject quote, ZString customValueName, ZString value)
		{
			var customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = quote.PK;
			customAddOnValue.XV_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			customAddOnValue.XV_Name = customValueName;
			customAddOnValue.XV_Type = AddOnColumnDataType.Codes.String;
			customAddOnValue.XV_Data = value;

			return customAddOnValue;
		}

		public void TestConvertToShipmentIsNotAllowedForDirectBookings()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quickBooking.Booking.JS_IsDirectBooking = true;
			Factory.Save();

			using (var form = new QuotedBookingForm(quickBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				AssertEquals("A message should have popped up", "You cannot convert a direct booking to a shipment. Please choose 'Consolidate' from the menu to create a new Consol.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertToShipment_ChangeBookingConsolidationParent()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);

			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = quotedBooking.ViewPK;
			bookingConsolidation.KB_ParentTableCode = ((BusinessObject)quotedBooking).TablePrefix;

			Factory.Save();

			AssertEquals("Precondition", bookingConsolidation.KB_ParentID, quotedBooking.ViewPK);
			AssertEquals("Precondition", bookingConsolidation.KB_ParentTableCode, ((BusinessObject)quotedBooking).TablePrefix);

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);

				AssertEquals(bookingConsolidation.KB_ParentID, booking.PK);
				AssertEquals(bookingConsolidation.KB_ParentTableCode, booking.TablePrefix);
			}
		}

		public void TestConvertToShipment_UpdateDefaultContainerModeFromRegistry()
		{
			var setupCollection = new DefaultContainerModesCollection();
			var registryItem = new DefaultContainerModes();
			registryItem.TransportMode = Constants.TransportModes.Courier;
			registryItem.ContainerMode = Constants.ContainerModes.Unaccompanied;
			setupCollection.Add(registryItem);

			FreightConfigurationRegistry.Instance.DefaultContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, setupCollection);

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			booking.JS_TransportMode = Constants.TransportModes.Courier;
			booking.JS_PackingMode = Constants.ContainerModes.OnBoardCourier;

			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				AssertEquals("The shipment transport mode should be transferred from the booking", Constants.TransportModes.Courier, shipment.JS_TransportMode);
				AssertEquals("The shipment default packing mode should have been updated from the registry", Constants.ContainerModes.Unaccompanied, shipment.JS_PackingMode);
			}
		}

		public void TestConvertToShipment_ContainerModeOverride()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quickBooking.ContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_DOOR;

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				AssertEquals("This shipment HBL Delivery Type should be transferred from the booking", Constants.HBLDeliveryModes.Codes.CFS_DOOR, shipment.JS_HBLContainerPackModeOverride);
			}
		}

		public void TestConvertToShipment_DisableMilestonesAndTriggersInBooking()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_IsDirectBooking = false;
			booking.JS_ScreeningStatus = "CLR";

			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			Factory.Save();

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);
			var bookingWorkflowProvider = (IWorkflowProvider)quotedBooking;

			var bookingMilestone = bookingWorkflowProvider.WorkflowItems.Milestones.AddNew();
			var bookingTrigger = bookingWorkflowProvider.WorkflowItems.Triggers.AddNew();
			var bookingTask = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			var bookingException = bookingWorkflowProvider.WorkflowItems.Exceptions.AddNew();

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				var shipmentWorkflowProvider = (IWorkflowProvider)form.PopupForm_ForTesting.BusinessEntity;
				var milestones = shipmentWorkflowProvider.WorkflowItems.Milestones.Cast<ProcessTask>();
				Assert(milestones.All(x => x.TriggerConditions.TriggerCondition == EventReferenceConditionList.Codes.ConditionWithMacros));
				Assert(milestones.All(x => x.TriggerConditions.TriggerConditionValue == "false"));

				var triggers = shipmentWorkflowProvider.WorkflowItems.Triggers.Cast<ProcessTask>();
				Assert(triggers.All(x => x.TriggerConditions.TriggerCondition == EventReferenceConditionList.Codes.ConditionWithMacros));
				Assert(triggers.All(x => x.TriggerConditions.TriggerConditionValue == "false"));

				var tasks = shipmentWorkflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>();
				Assert(tasks.All(x => x.TriggerConditions.TriggerCondition == string.Empty));
				Assert(triggers.All(x => x.TriggerConditions.TriggerConditionValue == string.Empty));

				var exceptions = shipmentWorkflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>();
				Assert(exceptions.All(x => x.TriggerConditions.TriggerCondition == string.Empty));
				Assert(exceptions.All(x => x.TriggerConditions.TriggerConditionValue == string.Empty));
			}
		}

		#endregion

		#region TestDeniedPartyScreenMenu

		public void TestAddRemoveOrInsertPartyScreeningLogWhenSavingBookings()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Org1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Org2";

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();

			using (var form = new QuotedBookingForm(quickBooking))
			{
				var shipment = quickBooking.Booking;
				booking.JS_OH_ImportBroker = org1.PK;
				Factory.Save();

				var screeningStatus1 = shipment.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
				CombineAssertions(() =>
				{
					AssertEquals(1, screeningStatus1.Count);
					AssertEquals(true, screeningStatus1.Any(u => u.PJ_Status == "PAA" && u.PJ_ClearedReason == "Parties Info:Org1(Import Broker)"));
				});

				booking.JS_OH_ImportBroker = org2.PK;
				Factory.Save();

				var screeningStatus2 = shipment.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
				CombineAssertions(() =>
				{
					AssertEquals(true, screeningStatus2.Any(u => u.PJ_Status == "PAA" && u.PJ_ClearedReason == "Parties Info:Org2(Import Broker)"));
					AssertEquals(true, screeningStatus2.Any(u => u.PJ_Status == "PAR" && u.PJ_ClearedReason == "Parties Info:Org1(Import Broker)"));
				});
			}
		}

		public void TestDeniedPartyScreenMenu_AddToBookingForm()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				AssertEquals("Precondition", false, quickBooking.IsTemplate);

				var menuScreen = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("View Compliance Status");
				AssertNotNull(menuScreen);
			}
		}

		public void TestDeniedPartyScreenMenu_NotAddedToOneOffQuotes()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quickBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuScreen = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("View Compliance Status");
				AssertNull("One off Quotes have no booking for screen", menuScreen);
			}
		}

		public void TestDeniedPartyScreenMenu_NotAddedToTemplateRecord()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			((ITemplateRecordProvider)quickBooking).IsTemplateRecord = true;

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				AssertEquals("Precondition", true, quickBooking.IsTemplate);

				var menuScreen = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("View Compliance Status");
				AssertNull("Not add menu when is template record", menuScreen);
			}
		}

		public void TestDeniedPartyScreenMenu_CheckQuotedBookingHasChanges()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuScreen = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("View Compliance Status");
				quickBooking.HasChanges = true;

				CombineAssertions(() =>
				{
					AssertEquals("Precondition", false, booking.HasChanges);
					AssertEquals("Precondition", true, booking.IsInDatabase);
					AssertEquals("Precondition", true, quickBooking.HasChanges);
					AssertNotNull("Precondition", menuScreen);
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				menuScreen.PerformClick();
				AssertEquals("Not able to screen when quoted booking has changes", "Please save the form before screening for denied parties.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestMarkAsJobClearMenu

		public void TestMarkAsJobClearMenuItemExist()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var markJobClearTestHelper = new DpsMarkJobScreeningStatusClearTest();

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				AssertEquals("Precondition", false, quickBooking.IsTemplate);
				markJobClearTestHelper.AssertMenuItemAccessibilityCheckpoint(form, false);
			}

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				AssertEquals("Precondition", false, quickBooking.IsTemplate);
				markJobClearTestHelper.AssertMenuItemAccessibilityCheckpoint(form, true);
			}

			booking = QuotedBooking.CreateNewBooking(Factory);
			quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			((ITemplateRecordProvider)quickBooking).IsTemplateRecord = true;

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				AssertEquals("Precondition : Not add menu when is template record", true, quickBooking.IsTemplate);
				markJobClearTestHelper.AssertMenuItemAccessibilityCheckpoint(form, false);
			}
		}

		public void TestMarkAsJobClearWhenSecurityRightsIsDenied_ShouldErrorMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				Factory.Save();

				tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = false;
				new DpsMarkJobScreeningStatusClearTest().AssertSecurityRightsAccessibilityCheckpoint(form, tmpSecurityCore);
			}
		}

		public void TestMarkJobClearWhenScreeningStatusIsJCLorCLR_ShouldShowWarningMessage()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				AssertEquals("Precondition booking screening status", "JCL", booking.JS_ScreeningStatus);
				Factory.Save();

				var markJobClearTestHelper = new DpsMarkJobScreeningStatusClearTest();
				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);

				booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Precondition booking screening status", "CLR", booking.JS_ScreeningStatus);
				Factory.Save();

				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);
			}
		}

		public void TestMarkJobClearWhenJobIsNotSaved_ShouldShowWarningMessage()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertEquals("Precondition booking screening status", "UNK", booking.JS_ScreeningStatus);

				new DpsMarkJobScreeningStatusClearTest().AssertSaveBeforeMarkingClear(form);
			}
		}

		public void TestJobShipmentWhenMarkingJobClear_ShouldUpdateJobScreeningStatusToJCL()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Precondition booking screening status", "MAT", booking.JS_ScreeningStatus);
				Factory.Save();

				new DpsMarkJobScreeningStatusClearTest().AssertScreeenigStatusToJCL(form, booking);
			}
		}

		#endregion

		#region TestCopyCustomFields

		public void TestConsolidateCopyCustomFields()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var customValue1 = AddCustomValueToQuote(quote, "custom1", "AAA");
			var customValue2 = AddCustomValueToQuote(quote, "custom2", "111");

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
				var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK);
				var result = Factory.Load<GenCustomAddOnValue>(query);

				AssertEquals(2, result.Length);
				CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom1"), customValue1);
				CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom2"), customValue2);
			}
		}

		public void TestAddToExistingConsolCopyCustomFields()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var customValue1 = AddCustomValueToQuote(quote, "custom1", "AAA");
			var customValue2 = AddCustomValueToQuote(quote, "custom2", "111");

			var consol = Factory.New<ForwardingConsol>();

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
				menuItem.PerformClick();
				Application.DoEvents();

				var popup = form.LastSelectHelper_ForTesting.LastPopupForTesting_ForTesting;
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { consol });
				Application.DoEvents();

				using (ZForm consolForm = (ZForm)form.LastSelectHelper_ForTesting.LastController_ForTesting.LastShownForm)
				{
					var consolLoadedInForm = (ForwardingConsol)consolForm.BusinessEntity;
					consolLoadedInForm.Factory.Save();
					AssertEquals(1, consolLoadedInForm.Shipments.Count);

					var shipment = consolLoadedInForm.Shipments[0];
					var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK);
					var result = Factory.Load<GenCustomAddOnValue>(query);

					AssertEquals(2, result.Length);
					CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom1"), customValue1);
					CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom2"), customValue2);
				}
			}
		}

		void AssertAddToExistingConsolDoNotCopyCO2eInShipment(string status)
		{
			// Arrange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Weight = status == CO2eStatusList.Codes.NotCalculated ? new ZDecimal(0) : 5m;
			quotedBooking.WeightUnit = Constants.Weight.Tonnes;
			quotedBooking.SetCO2ePerTonneInKg(status == CO2eStatusList.Codes.NotCalculated ? new ZDecimal(0) : 100.1111111m);
			quotedBooking.SetCO2eStatus(status);
			var consol = Factory.New<ForwardingConsol>();
			if (status == CO2eStatusList.Codes.NotCalculated)
			{
				consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			}
			Factory.Save();

			// Act
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Add to Existing Consol");
				menuItem.PerformClick();
				Application.DoEvents();

				var popup = form.LastSelectHelper_ForTesting.LastPopupForTesting_ForTesting;
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { consol });
				Application.DoEvents();

				using (ZForm consolForm = (ZForm)form.LastSelectHelper_ForTesting.LastController_ForTesting.LastShownForm)
				{
					var consolLoadedInForm = (ForwardingConsol)consolForm.BusinessEntity;
					consolLoadedInForm.Factory.Save();

					var shipment = consolLoadedInForm.Shipments[0];

					// Assert
					AssertEquals(quotedBooking.GetTotalCO2e(), shipment.GetTotalCO2e());
					AssertEquals(CO2eStatusList.Codes.NotCalculated, shipment.GetCO2eStatus());
					if (status != CO2eStatusList.Codes.NotCalculated)
					{
						AssertEquals(CO2eStatusList.Codes.NotCalculated, consol.GetCO2eStatus());
					}
					else
					{
						AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
					}
				}
			}
		}

		public void TestAddToExistingConsol_PopulatesCO2eWithStatusCurrentInShipment()
		{
			AssertAddToExistingConsolDoNotCopyCO2eInShipment(CO2eStatusList.Codes.Current);
		}

		public void TestAddToExistingConsol_PopulatesCO2eWithStatusNotCurrentInShipment()
		{
			AssertAddToExistingConsolDoNotCopyCO2eInShipment(CO2eStatusList.Codes.NotCurrent);
		}

		public void TestAddToExistingConsol_PopulatesCO2eWithStatusNotCalculatedInShipment()
		{
			AssertAddToExistingConsolDoNotCopyCO2eInShipment(CO2eStatusList.Codes.NotCalculated);
		}

		public void TestAddToExistingConsol_PopulatesCO2eWithStatusRejectedInShipment()
		{
			AssertAddToExistingConsolDoNotCopyCO2eInShipment(CO2eStatusList.Codes.Rejected);
		}
		public void TestAddToExistingConsol_PopulatesCO2eWithStatusPendingInShipment()
		{
			AssertAddToExistingConsolDoNotCopyCO2eInShipment(CO2eStatusList.Codes.Pending);
		}

		#endregion

		#region Disable Tasks

		public void TestConvertToShipment_QuickBooking_DisableTasks()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var tasks = AddTasks(quickBooking.WorkflowItems);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);

				AssertTasksAreDisabled(tasks);
			}
		}

		public void TestConvertToShipment_QuotedBooking_DisableTasks()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var tasks = AddTasks(quotedBooking.WorkflowItems);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);

				AssertTasksAreDisabled(tasks);
			}
		}

		public void TestConsolidateToNewConsol_QuickBooking_DisableTasks()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var tasks = AddTasks(quickBooking.WorkflowItems);

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				form.ConsolidateToNewConsol();
				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				ForwardingShipment sameBooking = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				AssertEquals("One Consol must have been created and attached to booking", 1, sameBooking.Consols.Count);
				Assert(sameBooking.Consols[0].Shipments[0].Job != null);

				AssertTasksAreDisabled(tasks);
			}
		}

		public void TestConsolidateToNewConsol_QuotedBooking_DisableTasks()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var tasks = AddTasks(quotedBooking.WorkflowItems);

			quotedBooking.Factory.Save();

			using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();
				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				ForwardingShipment sameBooking = Factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
				AssertEquals("One Consol must have been created and attached to booking", 1, sameBooking.Consols.Count);
				Assert(sameBooking.Consols[0].Shipments[0].Job != null);

				AssertTasksAreDisabled(tasks);
			}
		}

		public void TestConvertToShipment_QuickBooking_Milestone_P9_MilestonExceptionAdded_ShouldNotBeEmpty()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var milestone1 = quickBooking.WorkflowItems.Milestones.AddNew();
			var milestone2 = quickBooking.WorkflowItems.Milestones.AddNew();
			var milestone3 = quickBooking.WorkflowItems.Milestones.AddNew();
			milestone3.P9_MilestoneExceptionAdded = ZDateTime.UtcNow;

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);

				Assert(!milestone1.P9_MilestoneExceptionAdded.IsEmpty);
				Assert(!milestone2.P9_MilestoneExceptionAdded.IsEmpty);
				Assert(!milestone3.P9_MilestoneExceptionAdded.IsEmpty);
			}
		}

		public void TestConvertToShipment_QuotedBooking_Milestone_P9_MilestonExceptionAdded_ShouldNotBeEmpty()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ScreeningStatus = "CLR";
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var milestone1 = quotedBooking.WorkflowItems.Milestones.AddNew();
			var milestone2 = quotedBooking.WorkflowItems.Milestones.AddNew();
			var milestone3 = quotedBooking.WorkflowItems.Milestones.AddNew();
			milestone3.P9_MilestoneExceptionAdded = ZDateTime.UtcNow;

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();

				form.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var shipment = Factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);

				Assert(!milestone1.P9_MilestoneExceptionAdded.IsEmpty);
				Assert(!milestone2.P9_MilestoneExceptionAdded.IsEmpty);
				Assert(!milestone3.P9_MilestoneExceptionAdded.IsEmpty);
			}
		}

		ProcessTask CreateTask(ProcessTaskCollection collection, string description, string status)
		{
			var task = collection.Tasks.AddNew();
			task.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task.P9_Description = description;
			task.P9_Status = status;
			return task;
		}

		List<ProcessTask> AddTasks(ProcessTaskCollection collection)
		{
			var tasks = new List<ProcessTask>();
			tasks.Add(CreateTask(collection, "task1", ProcessTaskStatusCodeList.Codes.Assigned));
			tasks.Add(CreateTask(collection, "task2", ProcessTaskStatusCodeList.Codes.Open));
			tasks.Add(CreateTask(collection, "task3", ProcessTaskStatusCodeList.Codes.Working));
			tasks.Add(CreateTask(collection, "task4", ProcessTaskStatusCodeList.Codes.Suspended));
			tasks.Add(CreateTask(collection, "task5", ProcessTaskStatusCodeList.Codes.Cancelled));
			tasks.Add(CreateTask(collection, "task6", ProcessTaskStatusCodeList.Codes.Closed));

			return tasks;
		}

		void AssertTasksAreDisabled(List<ProcessTask> tasks)
		{
			AssertEquals(6, tasks.Count);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, tasks[0].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, tasks[1].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, tasks[2].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, tasks[3].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, tasks[4].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, tasks[5].P9_Status);
		}

		#endregion

		#region Plugins

		public void TestPlugIns()
		{
			QuotedBooking quotedBooking = GetSavedQuotedBooking();
			using (QuotedBookingForm bookForm = new QuotedBookingForm(quotedBooking))
			{
				bookForm.Show();

				bool jobInvoicingPluginExists = false;

				foreach (ZPlugIn plugin in bookForm.PlugIns.Instances)
				{
					if (plugin.GetType().FullName == "Enterprise.Accounting.GUI.JobInvoicing.InvoicingPluginToFreight")
					{
						jobInvoicingPluginExists = true;
						break;
					}
				}
				Assert("Invoicing should be plugged in", jobInvoicingPluginExists);
			}
		}

		public void TestAccessingDocumentPluginDoesNotLockOutOtherUsers()
		{
			var quotedBooking1 = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var quotedBooking2 = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			Factory.Save();

			using (var documentNoteForQuotedBooking1 = DocumentNote.LoadNoteWithExclusiveMutex(quotedBooking1))
			{
				AssertNotNull("should load document note for first quoted booking and aquire mutex", documentNoteForQuotedBooking1);

				using (var newConnection = CargoWise.Data.Db.NewAdminConnection())
				{
					var factory2 = new BusinessObjectFactory(newConnection);

					IStmNoteParent quotedBookingNoteParent = quotedBooking2;

					var dummyNoteParent = new Mock<IStmNoteParentForTesting>();

					dummyNoteParent.Setup(m => m.NotesFactory).Returns(factory2);
					dummyNoteParent.Setup(m => m.NotesParentPK).Returns(quotedBookingNoteParent.NotesParentPK);
					dummyNoteParent.Setup(m => m.NotesParentTableName).Returns(quotedBookingNoteParent.NotesParentTableName);
					dummyNoteParent.Setup(m => m.NoteTypes).Returns(quotedBookingNoteParent.NoteTypes);

					using (var documentNoteForQuotedBooking2 = DocumentNote.LoadNoteWithExclusiveMutex(dummyNoteParent.Object))
					{
						AssertNotNull("should load document note for second quoted booking and aquire mutex", documentNoteForQuotedBooking2);
					}

					dummyNoteParent.VerifyAll();
				}
			}
		}

		public void TestConsolidateToNewConsolAndActionOfExceptionsHaveChanged()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var exception = quotedBooking.WorkflowItems.Exceptions.AddNew();
			AssertEquals(false, exception.IsExceptionActioned);
			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ConsolidateToNewConsol();

				var consolForm = form.PopupForm_ForTesting;
				var newConsol = consolForm.BusinessEntity;
				newConsol.Factory.Save();
				AssertEquals(true, exception.IsExceptionActioned);
			}
		}

		public void TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol()
		{
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Air, ContainerModes.Loose, TransportModes.Air, ContainerModes.Loose, TransportModes.Air, ContainerModes.Loose);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Air, ContainerModes.ULD, TransportModes.Air, ContainerModes.ULD, TransportModes.Air, ContainerModes.ULD);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Air, ContainerModes.BuyersConsol, TransportModes.Air, ContainerModes.BuyersConsol, TransportModes.Air, ContainerModes.BuyersConsol);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Air, ContainerModes.ShippersConsol, TransportModes.Air, ContainerModes.ShippersConsol, TransportModes.Air, ContainerModes.ShippersConsol);

			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, RateMode.SEA, TransportModes.Sea, RateMode.FCL, TransportModes.Sea, RateMode.FCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, ContainerModes.FCL, TransportModes.Sea, ContainerModes.FCL, TransportModes.Sea, ContainerModes.FCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, ContainerModes.LCL, TransportModes.Sea, ContainerModes.LCL, TransportModes.Sea, ContainerModes.LCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, ContainerModes.Bulk, TransportModes.Sea, ContainerModes.Bulk, TransportModes.Sea, ContainerModes.Bulk);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, ContainerModes.Liquid, TransportModes.Sea, ContainerModes.Liquid, TransportModes.Sea, ContainerModes.Liquid);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, ContainerModes.BreakBulk, TransportModes.Sea, ContainerModes.BreakBulk, TransportModes.Sea, ContainerModes.BreakBulk);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, ContainerModes.RollOnRollOff, TransportModes.Sea, ContainerModes.RollOnRollOff, TransportModes.Sea, ContainerModes.RollOnRollOff);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, ContainerModes.BuyersConsol, TransportModes.Sea, ContainerModes.BuyersConsol, TransportModes.Sea, ContainerModes.BuyersConsol);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Sea, ContainerModes.ShippersConsol, TransportModes.Sea, ContainerModes.ShippersConsol, TransportModes.Sea, ContainerModes.ShippersConsol);

			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Road, RateMode.ROA, TransportModes.Road, RateMode.LCL, TransportModes.Road, RateMode.LCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Road, RateMode.LRO, TransportModes.Road, RateMode.LCL, TransportModes.Road, RateMode.LCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Road, ContainerModes.FCL, TransportModes.Road, ContainerModes.FCL, TransportModes.Road, ContainerModes.FCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Road, ContainerModes.LCL, TransportModes.Road, ContainerModes.LCL, TransportModes.Road, ContainerModes.LCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Road, ContainerModes.FTL, TransportModes.Road, ContainerModes.FTL, TransportModes.Road, ContainerModes.FTL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Road, ContainerModes.LTL, TransportModes.Road, ContainerModes.LTL, TransportModes.Road, ContainerModes.LTL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Road, ContainerModes.BuyersConsol, TransportModes.Road, ContainerModes.BuyersConsol, TransportModes.Road, ContainerModes.BuyersConsol);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Road, ContainerModes.ShippersConsol, TransportModes.Road, ContainerModes.ShippersConsol, TransportModes.Road, ContainerModes.ShippersConsol);

			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, RateMode.RAI, TransportModes.Rail, RateMode.LCL, TransportModes.Rail, RateMode.LCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, RateMode.FWL, TransportModes.Rail, RateMode.LCL, TransportModes.Rail, RateMode.LCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, ContainerModes.FCL, TransportModes.Rail, ContainerModes.FCL, TransportModes.Rail, ContainerModes.FCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, ContainerModes.LCL, TransportModes.Rail, ContainerModes.LCL, TransportModes.Rail, ContainerModes.LCL);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, ContainerModes.Bulk, TransportModes.Rail, ContainerModes.Bulk, TransportModes.Rail, ContainerModes.Bulk);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, ContainerModes.Liquid, TransportModes.Rail, ContainerModes.Liquid, TransportModes.Rail, ContainerModes.Liquid);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, ContainerModes.BreakBulk, TransportModes.Rail, ContainerModes.BreakBulk, TransportModes.Rail, ContainerModes.BreakBulk);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, ContainerModes.BuyersConsol, TransportModes.Rail, ContainerModes.BuyersConsol, TransportModes.Rail, ContainerModes.BuyersConsol);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Rail, ContainerModes.ShippersConsol, TransportModes.Rail, ContainerModes.ShippersConsol, TransportModes.Rail, ContainerModes.ShippersConsol);

			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Courier, RateMode.COU, TransportModes.Air, ContainerModes.Other, TransportModes.Courier, ContainerModes.OnBoardCourier);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Courier, ContainerModes.OnBoardCourier, TransportModes.Air, ContainerModes.Other, TransportModes.Courier, ContainerModes.OnBoardCourier);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.Courier, ContainerModes.Unaccompanied, TransportModes.Air, ContainerModes.Other, TransportModes.Courier, ContainerModes.Unaccompanied);

			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.AirSea, ContainerModes.Loose, TransportModes.Air, ContainerModes.Loose, TransportModes.AirSea, ContainerModes.Loose);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.AirSea, ContainerModes.ULD, TransportModes.Air, ContainerModes.ULD, TransportModes.AirSea, ContainerModes.ULD);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.AirSea, ContainerModes.LCL, TransportModes.Air, ContainerModes.Loose, TransportModes.AirSea, ContainerModes.LCL);

			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.SeaAir, ContainerModes.Loose, TransportModes.Sea, ContainerModes.LCL, TransportModes.SeaAir, ContainerModes.Loose);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.SeaAir, ContainerModes.ULD, TransportModes.Sea, ContainerModes.LCL, TransportModes.SeaAir, ContainerModes.ULD);
			TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(TransportModes.SeaAir, ContainerModes.LCL, TransportModes.Sea, ContainerModes.LCL, TransportModes.SeaAir, ContainerModes.LCL);
		}

		void TestTransportModeAndContainerModeAssignedCorrectly_AfterConsolidateToNewConsol(ZString transportMode, ZString containerMode, ZString expectedConsolTransportMode, ZString expectedConsolContainerMode, ZString expectedShipmentTransportMode, ZString expectedShipmentContainerMode)
		{
			var newFactory = new BusinessObjectFactory();
			var quote = QuotedBooking.CreateNewQuote(newFactory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, newFactory);
			quotedBooking.TransportMode = transportMode;
			quotedBooking.ContainerMode = containerMode;
			newFactory.Save();

			ForwardingConsol createdConsol;

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.ConsolidateToNewConsol();

				bookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();
				var consolForm = bookingForm.PopupForm_ForTesting;
				createdConsol = consolForm.BusinessEntity as ForwardingConsol;
			}

			var shipment = (ForwardingShipment)createdConsol.Shipments.First();
			CombineAssertions("After consolidating to new Consol, the transport and container mode should be assigned correctly",
						() =>
						{
							AssertEquals($"The transport mode of Consol shoud be {expectedConsolTransportMode}", expectedConsolTransportMode, createdConsol.JK_TransportMode);
							AssertEquals($"The container mode of Consol shoud be {expectedConsolContainerMode}", expectedConsolContainerMode, createdConsol.JK_ConsolMode);
							AssertEquals($"The transport mode of Shipment shoud be {expectedShipmentTransportMode}", expectedShipmentTransportMode, shipment.JS_TransportMode);
							AssertEquals($"The container mode of Shipment shoud be {expectedShipmentContainerMode}", expectedShipmentContainerMode, shipment.JS_PackingMode);
						});
		}

		public interface IStmNoteParentForTesting : IStmNoteParent, IDocumentSupportable
		{
		}

		#endregion

		#region ISupportSwitchTabPage

		public void TestSwitchTabPage()
		{
			var quotedBooking = GetQuotedBooking();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var bookForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookForm.Show();
				const string tabPageName = "ComplianceRiskTabPage";
				AssertNotEquals(tabPageName, bookForm.MainTabControlExposed.SelectedTab.Name);

				((ISupportSwitchTabPage)bookForm).SwitchTabPage(tabPageName);
				AssertEquals(tabPageName, bookForm.MainTabControlExposed.SelectedTab.Name);
			}
		}

		public void TestComplianceTabShouldNotBeAddedForQuickBookingtemplate()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			((ITemplateRecordProvider)quotedBooking).IsTemplateRecord = true;

			using (var bookForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookForm.Show();
				AssertNull("Compliance tab page should not be added for template",
					bookForm.MainTabControlExposed.TabPages.Cast<ZTabPage>().FirstOrDefault(tab => tab.Name.Equals("ComplianceRiskTabPage")));
			}
		}

		#endregion

		#region NVOCC

		public void TestNVOCC_BookingWithQuote()
		{
			QuotedBooking quotedBooking = GetSavedQuotedBooking();
			Env.Registry.SetFilterCriteria("DisplayBookingInNVOCC", ZBool.False.ToString());
			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				bookingForm.Show();
				ZTemplateTabControl mainTab = (ZTemplateTabControl)bookingForm.AdditionalDetailsTabPage.Parent;

				Assert(bookingForm.NVOCCModeCheckBox.Visible);
				Assert(!bookingForm.NVOCCModeCheckBox.Checked);

				Assert(bookingForm.QuotedBookingDetailsControl.Visible);
				Assert(!bookingForm.NVOCCQuotedBookingDetailsControl.Visible);

				mainTab.SelectTab(1);
				Assert(bookingForm.QuotedBookingAdditionalDetailsControl.Visible);
				Assert(!bookingForm.NVOCCQuotedBookingAdditionalDetailsControl.Visible);

				bookingForm.NVOCCModeCheckBox.Checked = true;
				mainTab.SelectTab(0);
				Assert(!bookingForm.QuotedBookingDetailsControl.Visible);
				Assert(bookingForm.NVOCCQuotedBookingDetailsControl.Visible);
				mainTab.SelectTab(1);
				Assert(!bookingForm.QuotedBookingAdditionalDetailsControl.Visible);
				Assert(bookingForm.NVOCCQuotedBookingAdditionalDetailsControl.Visible);
			}

			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				bookingForm.Show();
				ZTemplateTabControl mainTab = (ZTemplateTabControl)bookingForm.AdditionalDetailsTabPage.Parent;
				Assert(bookingForm.NVOCCModeCheckBox.Visible);
				Assert(bookingForm.NVOCCModeCheckBox.Checked);

				Assert(!bookingForm.QuotedBookingDetailsControl.Visible);
				Assert(bookingForm.NVOCCQuotedBookingDetailsControl.Visible);
				mainTab.SelectTab(1);
				Assert(!bookingForm.QuotedBookingAdditionalDetailsControl.Visible);
				Assert(bookingForm.NVOCCQuotedBookingAdditionalDetailsControl.Visible);
				bookingForm.NVOCCModeCheckBox.Checked = false;
			}

			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				bookingForm.Show();
				ZTemplateTabControl mainTab = (ZTemplateTabControl)bookingForm.AdditionalDetailsTabPage.Parent;
				Assert(bookingForm.NVOCCModeCheckBox.Visible);
				Assert(!bookingForm.NVOCCModeCheckBox.Checked);

				Assert(bookingForm.QuotedBookingDetailsControl.Visible);
				Assert(!bookingForm.NVOCCQuotedBookingDetailsControl.Visible);
				mainTab.SelectTab(1);
				Assert(!bookingForm.NVOCCQuotedBookingAdditionalDetailsControl.Visible);
				Assert(bookingForm.QuotedBookingAdditionalDetailsControl.Visible);
			}
		}

		public void TestNVOCC_SpotQuote()
		{
			QuotedBooking spotQuote = GetSavedQuote();
			Env.Registry.SetFilterCriteria("DisplayBookingInNVOCC", ZBool.False.ToString());
			using (QuotedBookingForm bookingForm = new QuotedBookingForm(spotQuote))
			{
				bookingForm.Show();
				Assert(!bookingForm.NVOCCModeCheckBox.Visible);

				ZTemplateTabControl mainTab = (ZTemplateTabControl)bookingForm.AdditionalDetailsTabPage.Parent;
				Assert(bookingForm.QuotedBookingDetailsControl.Visible);
				Assert(!bookingForm.NVOCCQuotedBookingDetailsControl.Visible);
			}

			Env.Registry.SetFilterCriteria("DisplayBookingInNVOCC", ZBool.True.ToString());
			using (QuotedBookingForm bookingForm = new QuotedBookingForm(spotQuote))
			{
				bookingForm.Show();
				Assert(!bookingForm.NVOCCModeCheckBox.Visible);

				ZTemplateTabControl mainTab = (ZTemplateTabControl)bookingForm.AdditionalDetailsTabPage.Parent;
				Assert(bookingForm.QuotedBookingDetailsControl.Visible);
				Assert(!bookingForm.NVOCCQuotedBookingDetailsControl.Visible);
			}
		}

		public void TestNVOCC_CustomFieldsTabVisiblility()
		{
			QuotedBooking quotedBooking = GetSavedQuotedBooking();

			Env.Registry.SetFilterCriteria("DisplayBookingInNVOCC", ZBool.False.ToString());
			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				bookingForm.Show();
				AssertEquals(true, bookingForm.CustomFieldsTabPage.TabVisible);
				bookingForm.NVOCCModeCheckBox.Checked = true;
				AssertEquals(false, bookingForm.CustomFieldsTabPage.TabVisible);
			}

			Env.Registry.SetFilterCriteria("DisplayBookingInNVOCC", ZBool.True.ToString());
			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				bookingForm.Show();

				AssertEquals(false, bookingForm.CustomFieldsTabPage.TabVisible);
				bookingForm.NVOCCModeCheckBox.Checked = false;
				AssertEquals(true, bookingForm.CustomFieldsTabPage.TabVisible);
			}
		}

		public void TestNVOCC_CustomFieldsTabWillNotBecomeVisibleWhenNewWokflowTemplateKicksIn()
		{
			ProcessTaskTemplate processTaskTemplate = CreateQuotedBookingWorkflow();

			QuotedBooking quotedBooking = GetSavedQuotedBooking();

			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				bookingForm.Show();
				AssertEquals(true, bookingForm.CustomFieldsTabPage.TabVisible);

				bookingForm.NVOCCModeCheckBox.Checked = true;
				AssertEquals(false, bookingForm.CustomFieldsTabPage.TabVisible);

				IProcessTaskTemplateLoader loader = ObjectFactory.New<IProcessTaskTemplateLoader>(Factory);
				ProcessTaskTemplate activeProcessTaskTemplate = (ProcessTaskTemplate)loader.FindTemplateForScreenLayout(quotedBooking);
				AssertNotEquals("loaded some other template", processTaskTemplate, activeProcessTaskTemplate);

				quotedBooking.Mode = "LCL";
				quotedBooking.Origin = "AUSYD";

				activeProcessTaskTemplate = (ProcessTaskTemplate)loader.FindTemplateForScreenLayout(quotedBooking);
				AssertEquals("loaded template that wants to make custom fields tab visible", processTaskTemplate, activeProcessTaskTemplate);
				AssertEquals("custom fields tab should remain invisible", false, bookingForm.CustomFieldsTabPage.TabVisible);
			}
		}

		ProcessTaskTemplate CreateQuotedBookingWorkflow()
		{
			ProcessTaskTemplate processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QBK";
			processTaskTemplate.P0_SubType1 = "SEA";
			processTaskTemplate.P0_SubType2 = "BWQ";
			processTaskTemplate.P0_LoadPortCountry = "AU";

			foreach (FormCustomisableElement tab in processTaskTemplate.FormCustomisationSettings.DisplayTabs)
			{
				tab.Visible = true;
			}

			Factory.Save();

			return processTaskTemplate;
		}

		public void TestConsolidateToNewConsol_OldTriggerInQuickBookingWasNotFired()
		{
			var processTaskTemplate = CreateQuickBookingWorkflowWithTriggers();

			var quickBooking = GetBooking();
			quickBooking.TransportMode = "AIR";
			quickBooking.Factory.Save();

			var loader = ObjectFactory.New<IProcessTaskTemplateLoader>(Factory);
			var activeProcessTaskTemplate = (ProcessTaskTemplate)loader.FindTemplateForScreenLayout(quickBooking);
			AssertEquals("can loaded QuickBooking template", processTaskTemplate, activeProcessTaskTemplate);
			AssertEquals(1, quickBooking.WorkflowItems.Triggers.Count);

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, quickBooking.WorkflowItems.Triggers[0].PK);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

				var logs = Factory.Load<StmALog>(query);
				logs.Cast<StmALog>().ForEach(x => x.IsCancelled = true);
				Factory.Save();

				form.ConsolidateToNewConsol();
				var factoryInConsolForm = form.PopupForm_ForTesting.BusinessEntity.Factory;
				factoryInConsolForm.Save();

				var shipment = factoryInConsolForm.Load<ForwardingShipment>(quickBooking.Booking.PK);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				shipment.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				logs = factoryInConsolForm.Load<StmALog>(query);
				AssertEquals("should not fire the trigger in quickBooking", 0, logs.Length);
			}
		}

		ProcessTaskTemplate CreateQuickBookingWorkflowWithTriggers()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QBK";
			processTaskTemplate.P0_SubType1 = "AIR";
			processTaskTemplate.P0_SubType2 = "QBN";

			var processTask = processTaskTemplate.WorkflowItems.Triggers.AddNew();
			processTask.P9_Description = "test";
			processTask.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;

			var notification = processTaskTemplate.WorkflowItems.Triggers[0].ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "NTF";
			notification.PQ_Calc_TriggerParty = "EML";
			notification.PQ_EmailAddr = "james.wen@wisetechglobal.com";
			notification.PQ_EmailText = "test 123";

			Factory.Save();

			return processTaskTemplate;
		}

		#endregion

		public void TestBookingWithQuoteShouldNotHasChanges_AfterRunningSave()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			Factory.Save();

			var list = new CodeDescriptionPairList();
			list.AddPair("EEE", "DesEEE");

			using (DataRegistryRating.Instance.OneOffQuoteKPISettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteSourceSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				AssertEquals("Pre-condition: Should not have changes", false, quotedBooking.HasChanges);

				quotedBooking.OneOffQuoteStatistics.OneOffQuoteKPI = "EEE";
				AssertEquals("Should have changes", true, quotedBooking.HasChanges);
				Factory.Save();
				AssertEquals("Should not have changes", false, quotedBooking.HasChanges);

				quotedBooking.OneOffQuoteStatistics.OneOffQuoteSource = "EEE";
				AssertEquals("Should have changes", true, quotedBooking.HasChanges);
				Factory.Save();
				AssertEquals("Should not have changes", false, quotedBooking.HasChanges);

				quotedBooking.OneOffQuoteStatistics.OneOffQuoteRevisionReason = "EEE";
				AssertEquals("Should have changes", true, quotedBooking.HasChanges);
				Factory.Save();
				AssertEquals("Should not have changes", false, quotedBooking.HasChanges);
			}
		}

		#region Quote Document

		public void TestSpotQuoteMissingCharges()
		{
			const string NoCharges = "This Spot Quote either has no charges present, or the Incoterm specified dictates that a Freight charge must be present and it is not. Do you want to continue?";
			const string PrintInFinal = "Do you want to print this Spot Quote in Final mode? This will mark this Spot Quote as locked and final.\r\n\r\nClick Yes for final, and No for draft.";

			QuotedBooking quote = GetSavedQuote();
			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quote))
			{
				bookingForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PrintQuoteButton.PerformClick();
				AssertDisplayedDialogMessages("No charges", NoCharges, PrintInFinal);

				JobCharge charge = Factory.New<JobCharge>();
				quote.TryLoadOrCreateJob();
				charge.JR_JH = quote.Job.PK;
				charge.JR_AC = Env.Registry.FreightChargeCode;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PrintQuoteButton.PerformClick();
				AssertDisplayedDialogMessages("With charges", PrintInFinal);
			}
		}

		void AssertDisplayedDialogMessages(string message, params string[] expectedDialogMessages)
		{
			List<string> actual = new List<string>();

			for (int i = 0; i < UnitTestUserNotification.Instance.PreviousMessages.Length; i++)
			{
				if (!UnitTestUserNotification.Instance.PreviousMessages[i].WasNone)
				{
					actual.Add(UnitTestUserNotification.Instance.PreviousMessages[i].Text);
				}
			}

			AssertContainsExactElementsInAnyOrder(message, expectedDialogMessages, actual);
		}

		public void TestValidateQuote()
		{
			string expectedError = string.Format("{0} Please save your quotation before printing.", "Error");
			string expectedNoErrors = "Do you want to print this Spot Quote in Final mode? This will mark this Spot Quote as locked and final.\r\n\r\nClick Yes for final, and No for draft.";

			var today = ZDate.Today;

			var booking = GetQuotedBooking();
			booking.Quote.TH_QuoteEndDate = today;

			var container = booking.QuotedBookingContainers.AddNew();
			container.JC_ContainerCount = 1;

			var charge = Factory.New<JobCharge>();
			charge.JR_GB = Env.CurrentBranch.PK;
			charge.JR_GE = Env.CurrentDepartment.PK;
			booking.TryLoadOrCreateJob();
			charge.JR_JH = booking.Job.PK;
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = 1111m;
			charge.JR_OSSellAmt = 1111m;

			using (QuotedBookingForm bookingForm = new QuotedBookingForm(booking))
			{
				bookingForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PrintQuoteButton.PerformClick();
				AssertEquals("QuoteNumber should be Empty", "", booking.Quote.TH_QuoteNumber);
				AssertEquals("Should be changes", true, booking.HasChanges);
				AssertEquals("Should be Error", expectedError, UnitTestUserNotification.Instance.LastMessage.ToString());

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PrintQuoteButton.PerformClick();
				AssertEquals("QuoteNumber should NOT be Empty", "00001000", booking.Quote.TH_QuoteNumber);
				AssertEquals("Should be NO changes", false, booking.HasChanges);
				AssertEquals("Should be NO Errors", expectedNoErrors, UnitTestUserNotification.Instance.LastMessage.Text);

				container.JC_ContainerCount = 100;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PrintQuoteButton.PerformClick();
				AssertEquals("QuoteNumber should NOT be Empty", "00001000", booking.Quote.TH_QuoteNumber);
				AssertEquals("Should be changes", true, booking.HasChanges);
				AssertEquals("Should be Error", expectedError, UnitTestUserNotification.Instance.LastMessage.ToString());

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PrintQuoteButton.PerformClick();
				AssertEquals("QuoteNumber should NOT be Empty", "00001000", booking.Quote.TH_QuoteNumber);
				AssertEquals("Should be NO changes", false, booking.HasChanges);
				AssertEquals("Should be NO Errors", expectedNoErrors, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		#endregion

		#region TestPrintQuoteWithUseDocumentDoesNotThrowException

		[ExpectNoExceptions]
		public void TestPrintQuoteWithUseDocumentDoesNotThrowException()
		{
			var booking = GetQuotedBooking();

			using (var bookingForm = new QuotedBookingForm(booking))
			{
				bookingForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var command = DocumentCommand.GetDocumentCommand(Factory, booking.Quote, Core.Constants.MenuNameConstantsForPrinting.QuotationPack);

				var pivot = Factory.New<StmMenuMenuPivotBase>();

				var parentMenu = Factory.New<StmMenuItemBase>();
				parentMenu.SU_MenuName = "Parent";

				var childMenu = Factory.New<StmMenuItemBase>();
				childMenu.SU_MenuName = "Child";
				childMenu.SU_BusinessContext = nameof(BusinessContext.Quotation);

				pivot.SF_SU_Inward = parentMenu.PK;
				pivot.SF_SU_Outward = childMenu.PK;
				pivot.SF_OverriddenBusinessContext = ZString.Empty;

				command.ChildMenus.Add(pivot);
				Factory.Save();

				bookingForm.PrintQuoteButton.PerformClick();
			}
		}

		#endregion

		#region Discrepancy Options when saving and printing

		public void TestDiscrepancyFormOptions_WhenUseQuotedPriceIsSelected_ShouldSyncQuoteDataToBooking()
		{
			var viewQuotedBooking = SetupConvertedBookingForDiscrepancyFormOptionsTests(true);

			AssertDiscrepancyFormOptions(
				viewQuotedBooking,
				DialogResult.OK,
				(quotedBooking) =>
				{
					var bookingPackLine = quotedBooking.Booking.OuterPackLines[0];
					CombineAssertions("Quote's pack line should be copied over to Booking's one",
						() =>
						{
							AssertEquals("Booking's pack line package count", 10, bookingPackLine.JL_PackageCount);
							AssertEquals("Booking's pack line weight", 100m, bookingPackLine.JL_ActualWeight);
							AssertEquals("Booking's pack line volume", 1m, bookingPackLine.JL_ActualVolume);
						});
				},
				QuotedBookingState.AcceptedBookingWithQuote);
		}

		public void TestDiscrepancyFormOptions_WhenReRateIsSelected_ShouldSyncBookingDataToQuote()
		{
			var viewQuotedBooking = SetupConvertedBookingForDiscrepancyFormOptionsTests(true);

			AssertDiscrepancyFormOptions(
				viewQuotedBooking,
				DialogResult.Yes,
				(quotedBooking) =>
				{
					var quotePackLine = quotedBooking.Quote.CurrentOneOffQuote.LooseCargo[0];
					CombineAssertions("Booking's pack line should be copied over to Quote's one",
						() =>
						{
							AssertEquals("Quote's pack line container count", 20, (int)quotePackLine.TPL_PackLineCount);
							AssertEquals("Quote's pack line weight", 500m, quotePackLine.TPL_Weight);
							AssertEquals("Quote's pack line volume", 5m, quotePackLine.TPL_Volume);
						});
				},
				QuotedBookingState.AcceptedBookingWithQuote);
		}

		public void TestDiscrepancyFormOptions_WhenHoldForLaterAnalysisIsSelected_ShouldNotSyncData()
		{
			var viewQuotedBooking = SetupConvertedBookingForDiscrepancyFormOptionsTests(true);

			AssertDiscrepancyFormOptions(
				viewQuotedBooking,
				DialogResult.Cancel,
				(quotedBooking) =>
				{
					var quotePackLine = quotedBooking.Quote.CurrentOneOffQuote.LooseCargo[0];
					var bookingPackLine = quotedBooking.Booking.OuterPackLines[0];
					CombineAssertions("Booking's pack line is different from Quote's one",
						() =>
						{
							AssertEquals("Booking's pack line package count", 20, bookingPackLine.JL_PackageCount);
							AssertEquals("Booking's pack line weight", 500m, bookingPackLine.JL_ActualWeight);
							AssertEquals("Booking's pack line volume", 5m, bookingPackLine.JL_ActualVolume);

							AssertEquals("Quote's pack line container count", 10, (int)quotePackLine.TPL_PackLineCount);
							AssertEquals("Quote's pack line weight", 100m, quotePackLine.TPL_Weight);
							AssertEquals("Quote's pack line volume", 1m, quotePackLine.TPL_Volume);
						});
				},
				QuotedBookingState.UnacceptedBookingWithQuote);
		}

		ViewQuotedBooking SetupConvertedBookingForDiscrepancyFormOptionsTests(bool withSyncedData)
		{
			var quotedBooking = GetQuotedBooking(true);
			quotedBooking.Mode = Constants.RateMode.LCL;

			var bookingPackLine = quotedBooking.Booking.OuterPackLines.AddNew();
			bookingPackLine.JL_PackageCount = 10;
			bookingPackLine.JL_ActualWeight = 100;
			bookingPackLine.JL_ActualVolume = 1;
			quotedBooking.Booking.JS_OuterPacks = 10;

			quotedBooking.CopyBookingValuesToQuote();

			if (!withSyncedData)
			{
				var quotePackLine = quotedBooking.Quote.CurrentOneOffQuote.LooseCargo[0];
				quotePackLine.TPL_PackLineCount = 5;
				quotePackLine.TPL_Weight = 50;
				quotePackLine.TPL_Volume = 0.5m;
			}

			Factory.Save();

			AssertEquals("Prerequisite: Ensure Booking with Quote state", QuotedBookingState.UnacceptedBookingWithQuote, quotedBooking.ObjectState);

			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking.Quote.PK));
			return viewQuotedBooking;
		}

		void AssertDiscrepancyFormOptions(ViewQuotedBooking viewQuotedBooking, DialogResult resultToReturnFromShowDialog, Action<QuotedBooking> assertionAction, QuotedBookingState expectedQuotedBookingState)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			using (var form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
			{
				var quotedBookingInNewFactory = form.DataSource as QuotedBooking;

				var bookingPackLineInNewFactory = quotedBookingInNewFactory.Booking.OuterPackLines[0];
				bookingPackLineInNewFactory.JL_PackageCount = 20;
				bookingPackLineInNewFactory.JL_ActualWeight = 500;
				bookingPackLineInNewFactory.JL_ActualVolume = 5;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // agree to update total values on booking

				ZFormModaliser.ResultToReturnFromShowDialog = resultToReturnFromShowDialog;
				form.FireSaveButton();

				assertionAction.Invoke(quotedBookingInNewFactory);

				AssertEquals("Booking with Quote state should be", expectedQuotedBookingState, quotedBookingInNewFactory.ObjectState);
			}
		}

		public void TestPrintBookingWithQuote_WhenBookingAndQuoteAreSync_ShouldNotShowDiscrepancyForm()
		{
			var viewQuotedBooking = SetupConvertedBookingForDiscrepancyFormOptionsTests(true);

			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			using (var form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
			{
				form.PrintQuoteButton.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest as DiscrepancyForm);

				viewQuotedBooking.QuotedBooking.Booking.JS_ActualWeight = 300;
				viewQuotedBooking.Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				form.PrintQuoteButton.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest as DiscrepancyForm);
			}
		}

		public void TestPrintBookingWithQuote_WhenBookingAndQuoteAreNotSync_ShouldShowDiscrepancyForm_UseQuoteClicked()
		{
			var viewQuotedBooking = SetupConvertedBookingForDiscrepancyFormOptionsTests(false);

			AssertDiscrepancyFormOptionsWhenPrinting(
				viewQuotedBooking,
				DialogResult.OK,
				(quotedBooking) =>
				{
					var bookingPackLine = quotedBooking.Booking.OuterPackLines[0];
					CombineAssertions("Quote's pack line should be copied over to Booking's one",
						() =>
						{
							AssertEquals("Booking's pack line package count", 5, bookingPackLine.JL_PackageCount);
							AssertEquals("Booking's pack line weight", 50m, bookingPackLine.JL_ActualWeight);
							AssertEquals("Booking's pack line volume", 0.5m, bookingPackLine.JL_ActualVolume);
						});
				},
				QuotedBookingState.AcceptedBookingWithQuote);
		}

		public void TestPrintBookingWithQuote_WhenBookingAndQuoteAreNotSync_ShouldShowDiscrepancyForm_ReRateClicked()
		{
			var viewQuotedBooking = SetupConvertedBookingForDiscrepancyFormOptionsTests(false);

			AssertDiscrepancyFormOptionsWhenPrinting(
				viewQuotedBooking,
				DialogResult.Yes,
				(quotedBooking) =>
				{
					var quotePackLine = quotedBooking.Quote.CurrentOneOffQuote.LooseCargo[0];
					CombineAssertions("Booking's pack line should be copied over to Quote's one",
						() =>
						{
							AssertEquals("Quote's pack line container count", 10, (int)quotePackLine.TPL_PackLineCount);
							AssertEquals("Quote's pack line weight", 100m, quotePackLine.TPL_Weight);
							AssertEquals("Quote's pack line volume", 1m, quotePackLine.TPL_Volume);
						});
				},
				QuotedBookingState.AcceptedBookingWithQuote);
		}

		public void TestPrintBookingWithQuote_WhenBookingAndQuoteAreNotSync_ShouldShowDiscrepancyForm_HoldValuesClicked()
		{
			var viewQuotedBooking = SetupConvertedBookingForDiscrepancyFormOptionsTests(false);

			AssertDiscrepancyFormOptionsWhenPrinting(
				viewQuotedBooking,
				DialogResult.Cancel,
				(quotedBooking) =>
				{
					var quotePackLine = quotedBooking.Quote.CurrentOneOffQuote.LooseCargo[0];
					var bookingPackLine = quotedBooking.Booking.OuterPackLines[0];
					CombineAssertions("Booking's pack line is different from Quote's one",
						() =>
						{
							AssertEquals("Booking's pack line package count", 10, bookingPackLine.JL_PackageCount);
							AssertEquals("Booking's pack line weight", 100m, bookingPackLine.JL_ActualWeight);
							AssertEquals("Booking's pack line volume", 1m, bookingPackLine.JL_ActualVolume);

							AssertEquals("Quote's pack line container count", 5, (int)quotePackLine.TPL_PackLineCount);
							AssertEquals("Quote's pack line weight", 50m, quotePackLine.TPL_Weight);
							AssertEquals("Quote's pack line volume", 0.5m, quotePackLine.TPL_Volume);
						});
				},
				QuotedBookingState.UnacceptedBookingWithQuote);
		}

		void AssertDiscrepancyFormOptionsWhenPrinting(ViewQuotedBooking viewQuotedBooking, DialogResult resultToReturnFromShowDialog, Action<QuotedBooking> assertionAction, QuotedBookingState expectedQuotedBookingState)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			using (var form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
			{
				var quotedBookingInNewFactory = form.DataSource as QuotedBooking;

				ZFormModaliser.ResultToReturnFromShowDialog = resultToReturnFromShowDialog;
				form.PrintQuoteButton.PerformClick();

				assertionAction.Invoke(quotedBookingInNewFactory);

				AssertEquals("Booking with Quote state should be", expectedQuotedBookingState, quotedBookingInNewFactory.ObjectState);
			}
		}

		#endregion

		#region CancelRequestActionForm

		public void TestShouldNotShowCancelRequestActionForm()
		{
			var viewQuotedBooking = SetupBookingForCancelRequestActionFormPopUpTests(ShipmentStatusList.Codes.Booked);

			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			using (var form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
			{
				AssertNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);

				form.FireSaveButton();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);
			}
		}
		public void TestShouldShowCancelRequestActionForm()
		{
			var viewQuotedBooking = SetupBookingForCancelRequestActionFormPopUpTests(ShipmentStatusList.Codes.EBookingCancellationRequest);

			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var cancelRequestActionForm = form as CancelRequestActionForm;
				if (cancelRequestActionForm != null)
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				}
			});

			using (var form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
			{
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);

				form.FireSaveButton();
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);
			}
		}

		public void TestShowCancelRequestActionFormIfNecessary_AcceptClicked()
		{
			var viewQuotedBooking = SetupBookingForCancelRequestActionFormButtonTests();

			AssertCancelRequestActionForm(
				viewQuotedBooking,
				DialogResult.Yes,
				string.Empty,
				(quotedBooking) =>
				{
					Thread.Sleep(100); // Adding a time gap to ensure "Logs.MostRecentLog" returns the latest
					var log = quotedBooking.Logs.MostRecentLog;
					CombineAssertions("ShipmentStatus should be changed",
						() =>
						{
							AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.BookingCancelled, quotedBooking.ShipmentStatus);
							AssertEquals("Check event free text", ZString.Empty, log.ReferenceFreeText);
							AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
							AssertEquals("Check event new status", "BKX", log.Parameters[Params.New]);
							AssertEquals("Check event old status", "EBC", log.Parameters[Params.Old]);
							AssertEquals("Check event reason", "Booking Cancelled", log.Parameters[Params.Reason]);
							Assert("ShipmentStatus should be readonly", quotedBooking.ShipmentStatusInfo.ReadOnly);
						});
				}
			);
		}

		public void TestShowCancelRequestActionFormIfNecessary_RejectClicked()
		{
			var viewQuotedBooking = SetupBookingForCancelRequestActionFormButtonTests();
			var reason = "Ship already left!";

			AssertCancelRequestActionForm(
				viewQuotedBooking,
				DialogResult.No,
				reason,
				(quotedBooking) =>
				{
					Thread.Sleep(100); // Adding a time gap to ensure "Logs.GetAllLogs" returns the latest
					var log = quotedBooking.Booking.Logs?.GetAllLogs().OfType<StmALog>()
									.OrderBy(l => l.SL_EventTime)
									.Where(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).ToList()[0];
					CombineAssertions("ShipmentStatus should be changed to EBC",
						() =>
						{
							AssertEquals("Check event free text", "Withdrawal", log.ReferenceFreeText);
							AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
							AssertEquals("Check event new status", "EBC", log.Parameters[Params.New]);
							AssertEquals("Check event old status", "BKD", log.Parameters[Params.Old]);
							AssertEquals("Check event reason", "Electronic Booking Cancellation Request Received", log.Parameters[Params.Reason]);
							Assert("ShipmentStatus should be readonly", quotedBooking.ShipmentStatusInfo.ReadOnly);
						});

					log = quotedBooking.Logs?.GetAllLogs().OfType<StmALog>()
									.OrderBy(l => l.SL_EventTime)
									.Where(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).ToList()[0];
					CombineAssertions("ShipmentStatus should be changed to BKD with specified reason",
						() =>
						{
							AssertEquals("Check event free text", ZString.Empty, log.ReferenceFreeText);
							AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
							AssertEquals("Check event new status", "BKD", log.Parameters[Params.New]);
							AssertEquals("Check event old status", "EBC", log.Parameters[Params.Old]);
							AssertEquals("Check event reason", reason, log.Parameters[Params.Reason]);
							Assert("ShipmentStatus should be readonly", quotedBooking.ShipmentStatusInfo.ReadOnly);
						});

					AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);
				}
			);
		}

		public void TestShowCancelRequestActionFormIfNecessary_CancelClicked()
		{
			var viewQuotedBooking = SetupBookingForCancelRequestActionFormButtonTests();

			AssertCancelRequestActionForm(
				viewQuotedBooking,
				DialogResult.Cancel,
				string.Empty,
				(quotedBooking) =>
				{
					CombineAssertions("ShipmentStatus should not be changed",
						() =>
						{
							AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.EBookingCancellationRequest, quotedBooking.ShipmentStatus);
						});
				});
		}

		ViewQuotedBooking SetupBookingForCancelRequestActionFormButtonTests()
		{
			SetupBookingForCancelRequestActionFormTests(ShipmentStatusList.Codes.Booked);

			#region Message

			var withdrawalMessage = Enterprise.UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers.GetQueuedUniversalShipmentMessage(new UniversalObjectFactory(),
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
			<DataVersion>1</DataVersion>
			<Purpose>
				<Code>WTH</Code>
				<Description>Withdrawal</Description>
			</Purpose>
		 </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(withdrawalMessage);

			var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
			AssertEquals("bookings.Count()", 1, bookings.Length);

			return Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, bookings[0].QuotedBooking.Quote.PK));
		}

		ViewQuotedBooking SetupBookingForCancelRequestActionFormPopUpTests(string shipmentStatus)
		{
			var quotedBooking = SetupBookingForCancelRequestActionFormTests(shipmentStatus);

			return Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking.Quote.PK));
		}

		QuotedBooking SetupBookingForCancelRequestActionFormTests(string shipmentStatus)
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);

			var newQuotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			newQuotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			newQuotedBooking.Mode = Constants.RateMode.FCL;
			newQuotedBooking.TryLoadOrCreateJob();
			newQuotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;

			newQuotedBooking.Booking.JS_ShipmentStatus = shipmentStatus;
			newQuotedBooking.Booking.JS_UniqueConsignRef = "S00005001";
			newQuotedBooking.Booking.JS_BookingReference = "CAR";
			newQuotedBooking.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.Save();

			return newQuotedBooking;
		}

		void AssertCancelRequestActionForm(ViewQuotedBooking viewQuotedBooking, DialogResult resultToReturnFromShowDialog, string reasonOfRejection, Action<QuotedBooking> assertionAction)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var cancelRequestActionForm = form as CancelRequestActionForm;
				if (cancelRequestActionForm != null)
				{
					cancelRequestActionForm.Reason = reasonOfRejection;
					ZFormModaliser.ResultToReturnFromShowDialog = resultToReturnFromShowDialog;
				}
			});

			using (var form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
			{
				assertionAction.Invoke(form.DataSource as QuotedBooking);
			}
		}

		#endregion

		#region Convert Quote to Booking

#if WINZOR
		/// <summary>
		/// In Winzor, window service is depend on Form, without Main Form, cannot show form in the context of Closed Form.
		/// </summary>
		IDisposable ShowMainFormForTest()
		{
			var mainForm = new Form() { Name = "MainForm" };
			mainForm.Show();
			return mainForm;
		}
#endif
		public void TestConvertQuoteToBooking()
		{
#if WINZOR
			using var mainForm = ShowMainFormForTest();
#endif
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			JobHeader job = new JobHeader.Loader(quotedBooking).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			ViewQuotedBooking viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quote.PK));

			ZController controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			using (QuotedBookingForm form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
			{
				AssertEquals(true, form.FormCaption.StartsWith("One Off Quote - "));

				form.ConvertQuoteToQuotedBookingButton.PerformClick();
				Application.DoEvents();
				AssertEquals(true, form.IsDisposed);

				ZForm newForm = form.PopupForm_ForTesting;
				AssertEquals(true, newForm.FormCaption.StartsWith("Booking with Quote - "));
				AssertEquals(true, newForm.Visible);
				AssertEquals(false, newForm.IsDisposed);

				newForm.Dispose();
			}
		}

		public void TestConvertQuoteToBooking_ConversionErrors()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.ConvertQuoteToQuotedBookingButton.PerformClick();

				var expectedMessage = "Please Save and Approve Quote before converting into a Quoted Booking";

				AssertEquals("Because there is no booking should skip straight to quote validation", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("AddToExistingConsol method execution skipped", bookingForm.LastSelectHelper_ForTesting);

				Factory.Save();

				using (DataRegistryRating.Instance.QuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					bookingForm.ConvertQuoteToQuotedBookingButton.PerformClick();

					expectedMessage = "Please Approve Quote before converting into a Quoted Booking";
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
					Factory.Save();
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.ConvertQuoteToQuotedBookingButton.PerformClick();

				AssertEquals("Should have no error messages", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPotentialCarriers_TransitTimeFrequencyAndFrequencyUnit()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			var oneOffQuote = quote.CurrentOneOffQuote;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;

			var job = new JobHeader.Loader(quotedBooking).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();
				Application.DoEvents();

				var potentialCarriersGrid = (ZGrid)bookingForm.Controls.Find("PotentialCarriersGrid", true).First();

				AssertContainsExactElementsInAnyOrder
				(
					"Grid should contain transitTime, frequency and frequencyUnit",
					new string[] { "TTC_OH_Carrier", "CarrierName", "TTC_TransitTime", "TTC_Frequency", "TTC_FrequencyUnit", "TTC_OH_Creditor" },
					potentialCarriersGrid.Columns.Select(column => column.ColumnName)
				);
			}
		}

		public void TestConvertQuoteToBooking_SelectingPossibleCarrier()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			var oneOffQuote = quote.CurrentOneOffQuote;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = Carrier1.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = Carrier2.PK;

			AssertConvertQuoteToBooking_SelectingPossibleCarrier
			(
				quotedBooking,
				selectedCarrier: Carrier2,
				expectedPossibleCarriers: new[] { Carrier1, Carrier2 },
				expectedCarrier: Carrier2
			);
		}

		void AssertConvertQuoteToBooking_SelectingPossibleCarrier(QuotedBooking quotedBooking, OrgHeader selectedCarrier, OrgHeader[] expectedPossibleCarriers, OrgHeader expectedCarrier)
		{
			var job = new JobHeader.Loader(quotedBooking).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PromptToSelectSingleCarrierReturn = carrier2.OH_Code;

				bookingForm.ConvertQuoteToQuotedBookingButton.PerformClick();

				AssertEquals("Should have no error messages", null, UnitTestUserNotification.Instance.LastMessage.Text);

				AssertContainsExactElementsInAnyOrder(new string[] { Carrier1.OH_Code, Carrier2.OH_Code }, bookingForm.PromptToSelectSingleCarrierCodes);
				AssertEquals("selected carrier is populated to quote", Carrier2.PK, quotedBooking.Quote.CurrentOneOffQuote.TT_OH_Carrier);
			}
		}

		public void TestConvertQuoteToBooking_SelectingPossibleCarrier_GivenOneOffQuoteHasNoTransitTimeAndFrequency_ThenShouldUsePossibleCarrierTransitTimeAndFrequency()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			var oneOffQuote = quote.CurrentOneOffQuote;

			var possibleCarrier1 = oneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = Carrier1.PK;
			possibleCarrier1.TTC_TransitTime = "1";
			possibleCarrier1.TTC_Frequency = 1;
			possibleCarrier1.TTC_FrequencyUnit = "days";

			var possibleCarrier2 = oneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier2.TTC_TransitTime = "2";
			possibleCarrier2.TTC_Frequency = 2;
			possibleCarrier2.TTC_FrequencyUnit = "forthnight";
			possibleCarrier2.TTC_OH_Carrier = Carrier2.PK;

			AssertConvertQuoteToBooking_SelectingPossibleCarrier
			(
				quotedBooking,
				selectedCarrier: Carrier2,
				expectedPossibleCarriers: new[] { Carrier1, Carrier2 },
				expectedCarrier: Carrier2,
				expectedTransitTime: "2",
				expectedFrequency: 2,
				expectedFrequencyUnit: "forthnight"
			);
		}

		public void TestConvertQuoteToBooking_SelectingPossibleCarrier_GivenOneOffQuoteHasTransitTimeAndFrequency_ThenShouldNotUsePossibleCarrierTransitTimeAndFrequency()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			var oneOffQuote = quote.CurrentOneOffQuote;
			oneOffQuote.TT_TransitTime = "3";
			oneOffQuote.TT_Frequency = 3;
			oneOffQuote.TT_FrequencyUnit = "week";

			var possibleCarrier1 = oneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = Carrier1.PK;
			possibleCarrier1.TTC_TransitTime = "1";
			possibleCarrier1.TTC_Frequency = 1;
			possibleCarrier1.TTC_FrequencyUnit = "days";

			var possibleCarrier2 = oneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier2.TTC_TransitTime = "2";
			possibleCarrier2.TTC_Frequency = 2;
			possibleCarrier2.TTC_FrequencyUnit = "forthnight";
			possibleCarrier2.TTC_OH_Carrier = Carrier2.PK;

			AssertConvertQuoteToBooking_SelectingPossibleCarrier
			(
				quotedBooking,
				selectedCarrier: Carrier2,
				expectedPossibleCarriers: new[] { Carrier1, Carrier2 },
				expectedCarrier: Carrier2,
				expectedTransitTime: "3",
				expectedFrequency: 3,
				expectedFrequencyUnit: "week"
			);
		}

		void AssertConvertQuoteToBooking_SelectingPossibleCarrier(QuotedBooking quotedBooking, OrgHeader selectedCarrier, OrgHeader[] expectedPossibleCarriers, OrgHeader expectedCarrier, string expectedTransitTime, int expectedFrequency, string expectedFrequencyUnit)
		{
			var job = new JobHeader.Loader(quotedBooking).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PromptToSelectSingleCarrierReturn = carrier2.OH_Code;

				bookingForm.ConvertQuoteToQuotedBookingButton.PerformClick();

				AssertEquals("Should have no error messages", null, UnitTestUserNotification.Instance.LastMessage.Text);

				AssertContainsExactElementsInAnyOrder(new string[] { Carrier1.OH_Code, Carrier2.OH_Code }, bookingForm.PromptToSelectSingleCarrierCodes);
				var oneOffQuote = quotedBooking.Quote.CurrentOneOffQuote;
				CombineAssertions("selected possible carrier is populated to quote", () =>
				{
					var actualCarrier = Factory.Load<OrgHeader>(oneOffQuote.TT_OH_Carrier);
					AssertEquals("carrier", expectedCarrier.OH_Code, actualCarrier.OH_Code);

					AssertEquals("transit-time", expectedTransitTime, oneOffQuote.TT_TransitTime);
					AssertEquals("frequency", expectedFrequency, oneOffQuote.TT_Frequency);
					AssertEquals("frequency-unit", expectedFrequencyUnit, oneOffQuote.TT_FrequencyUnit);
				});
			}
		}

		OrgHeader Carrier1
		{
			get
			{
				if (carrier1 == null)
				{
					carrier1 = Factory.NewWithValidTestData<OrgHeader>();
					carrier1.OH_Code = "CARRIER1";
					carrier1.OH_IsShippingLine = true;
				}
				return carrier1;
			}
		}
		OrgHeader carrier1;

		OrgHeader Carrier2
		{
			get
			{
				if (carrier2 == null)
				{
					carrier2 = Factory.NewWithValidTestData<OrgHeader>();
					carrier1.OH_Code = "CARRIER2";
					carrier2.OH_IsShippingLine = true;
				}
				return carrier2;
			}
		}
		OrgHeader carrier2;

		public void TestConvertQuoteToBooking_CarrierNotEmpty_NoPromptToSelectPossibleCarrier()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier2.OH_IsShippingLine = true;
			carrier3.OH_IsShippingLine = true;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			var oneOffQuote = quote.CurrentOneOffQuote;
			oneOffQuote.TT_OH_Carrier = carrier3.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;
			JobHeader job = new JobHeader.Loader(quotedBooking).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bookingForm.PromptToSelectSingleCarrierReturn = carrier2.OH_Code;

				bookingForm.ConvertQuoteToQuotedBookingButton.PerformClick();

				AssertEquals("Should have no error messages", null, UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNull("PromptToSelectSingleCarrierCodes is not called", bookingForm.PromptToSelectSingleCarrierCodes);
				AssertEquals("carrier not changed", carrier3.PK, oneOffQuote.TT_OH_Carrier);
			}
		}

		public void TestConsumedOneOffQuote_ConversionErrors()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				var actionMenuItems = bookingForm.Menu.MenuItems.FindByText("Actions").MenuItems;
				var addToConsolItem = actionMenuItems.FindByText("Add to Existing Consol");
				var consolidateItem = actionMenuItems.FindByText("Consolidate");
				var convertToBookingWithQuoteItem = actionMenuItems.FindByText("Convert to Booking with Quote");

				quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
				quotedBooking.Quote.TH_IsOneOffQuoteConsumed = true;
				Factory.Save();

				var expectedMessage = "One Off Quote has already been used.";
				convertToBookingWithQuoteItem.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				addToConsolItem.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consolidateItem.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				convertToBookingWithQuoteItem.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertQuoteToBooking_JobHeaderMutexLocked()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			Factory.Save();
			ViewQuotedBooking viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quote.PK));

			ZController controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(quote.PK);
			try
			{
				mutex.Lock();

				using (QuotedBookingForm form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
				{
					AssertEquals(true, form.FormCaption.StartsWith("One Off Quote - "));

					form.ConvertQuoteToQuotedBookingButton.PerformClick();
					Application.DoEvents();
					AssertEquals(false, form.IsDisposed);
					AssertEquals("Error converting Quote Charges.\r\n\r\nYou have created the job " + quote.TH_QuoteNumber + " on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job " + quote.TH_QuoteNumber + " to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestSpotQuoteInViewMode()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quoteOnly = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.OneOffQuotes);
			using (var form = (QuotedBookingForm)controller.ShowViewForm(quoteOnly))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);

				AssertEquals(true, form.ConvertQuoteToQuotedBookingButton.Visible);
				AssertEquals(false, form.ConvertQuoteToQuotedBookingButton.Enabled);

				AssertEquals(true, form.ApproveOneOffButton.Visible);
				AssertEquals(false, form.ApproveOneOffButton.Enabled);
			}
		}

		#endregion

		#region Approve Quote

		[GuiTest]
		public void TestShowApprovalDialog()
		{
			QuotedBooking quotedBooking = GetSavedQuote();
			Quote quote = quotedBooking.Quote;

			using (QuotedBookingForm form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				string expected = "Question " +
					"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.\r\n\r\n" +
					"Do you wish to Approve this quote?" +
					"";
				quote.OnShowApprovalDialog(new Quote.ApprovalDialogEventArgs(Quote.ApprovalDialog.WishToApprove));
				AssertEquals("Dialog: WishToApprove", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "None ";
				quote.OnShowApprovalDialog(new Quote.ApprovalDialogEventArgs(Quote.ApprovalDialog.WishToApproveWhenAccepting));
				AssertEquals("Dialog: WishToApproveWhenAccepting", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Question " +
					"This Spot Quotation is not internally approved.\r\n" +
					"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.\r\n\r\n" +
					"Do you wish to Approve this quote for printing in Final mode? Otherwise this quote will be printed in Draft mode." +
					"";
				quote.OnShowApprovalDialog(new Quote.ApprovalDialogEventArgs(Quote.ApprovalDialog.WishToApproveWhenFinalizing));
				AssertEquals("Dialog: WishToApproveWhenFinalizing", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Question " +
					"This Spot Quotation is not internally approved.\r\n" +
					"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.\r\n\r\n" +
					"Do you wish to Approve this quote?" +
					"";
				quote.OnShowApprovalDialog(new Quote.ApprovalDialogEventArgs(Quote.ApprovalDialog.WishToApproveWhenSaving));
				AssertEquals("Dialog: WishToApproveWhenSaving", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestShowApprovalMessage()
		{
			QuotedBooking quotedBooking = GetSavedQuote();
			Quote quote = quotedBooking.Quote;

			using (QuotedBookingForm form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				string expected = "Information " +
					"This Spot Quotation is already approved." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.AlreadyApprovedMessage);
				AssertEquals("Message: AlreadyApprovedMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Information " +
					"The login details entered do not have security rights to approve Spot Quotations." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.HaveNoRightsMessage);
				AssertEquals("Message: HaveNoRightsMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Information " +
					"The login details entered are incorrect." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.LoginFailedMessage);
				AssertEquals("Message: LoginFailedMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Error " +
					"A local client or overseas agent is required to approve this Quotation." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.MissingLocalClientMessage);
				AssertEquals("Message: MissingLocalClientMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Error " +
					"An overseas agent is required to approve this Quotation." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.MissingOverseasAgentMessage);
				AssertEquals("Message: MissingOverseasAgentMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestQuoteApprovalSecurityNotGranted()
		{
			QuotedBooking quotedBooking = GetSavedQuote();
			Quote quote = quotedBooking.Quote;

			using (QuotedBookingForm form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				string expected = "Question " +
					"You do not have the appropriate security rights to mark this Spot Quotation as Approved.\r\n" +
					"If you do not mark this quotation as approved you cannot use it for Auto Rating Purposes and it can only be printed in Draft Mode.\r\n\r\n" +
					"You can either continue, or have a user with higher security rights enter their credentials.\r\n\r\n" +
					"Do you wish to have a user with higher rights enter their credentials?" +
					"";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				quote.OnQuoteApprovalSecurityNotGranted(new Quote.QuoteApprovalSecurityEventArgs());
				AssertEquals("Dialog: WishToSwitchSecurity", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				quote.OnQuoteApprovalSecurityNotGranted(new Quote.QuoteApprovalSecurityEventArgs());
				AssertEquals("Message: HaveNoRightsMessage", typeof(Rating.GUI.LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		[GuiTest]
		public void TestUpdateQuoteLockedReadOnlyState()
		{
			QuotedBooking quotedBooking = GetSavedQuote();
			Quote quote = quotedBooking.Quote;

			using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Assert(!form.NotesTabPage_Exposed.ReadOnly);

				quote.TH_IsLocked = true;
				AssertEquals("Precondition", null, quotedBooking.Booking);
				AssertEquals("Precondition", QuotedBookingState.QuoteOnly, quotedBooking.ObjectState);

				form.UpdateQuoteLockedReadOnlyState_Exposed();

				form.SelectNotesTab();
				Application.DoEvents();

				Assert(form.NotesTabPage_Exposed.ReadOnly);

				quotedBooking.Factory.Save();
			}

			using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				form.SelectNotesTab();
				Application.DoEvents();
				Assert(form.NotesTabPage_Exposed.ReadOnly);
			}
		}

		#endregion

		[GuiTest]
		public void TestStaffAssignmentForClient_ShouldAddErrorOnlyIfClientHasChanged()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.LCL;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "NZAKL";
			quotedBooking.Weight = 100m;

			var clientA = Factory.NewWithValidTestData<OrgHeader>();
			clientA.OH_IsConsignor = true;
			var salesRepresentativeA = Factory.NewWithValidTestData<GlbStaff>();

			var assignmentA = clientA.StaffAssignments.AddNew();
			assignmentA.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignmentA.O8_GS_NKPersonResponsible = salesRepresentativeA.GS_Code;
			salesRepresentativeA.GS_IsActive = false;

			quotedBooking.ClientPK = clientA.PK;

			var clientB = Factory.NewWithValidTestData<OrgHeader>();
			clientB.OH_IsConsignor = true;
			var salesRepresentativeB = Factory.NewWithValidTestData<GlbStaff>();

			var assignmentB = clientB.StaffAssignments.AddNew();
			assignmentB.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignmentB.O8_GS_NKPersonResponsible = salesRepresentativeB.GS_Code;
			salesRepresentativeB.GS_IsActive = false;

			Factory.Save();
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				AssertEquals(false, quotedBooking.HasChanges);
				quotedBooking.RunPreSaveValidation();
				AssertEquals("For existing records there should NOT be any error when there is an inactive staff assigned to client", false, quotedBooking.HasErrors);

				quotedBooking.ClientPK = clientA.PK;
				AssertEquals("assigning the same client should not be considered as a change", false, quotedBooking.ClientPKInfo.HasChanges);

				quotedBooking.Weight = 200m;
				AssertEquals("Weight has changed and so there is a change in quoted booking", true, quotedBooking.HasChanges);
				AssertEquals("still there is no change in client", false, quotedBooking.ClientPKInfo.HasChanges);
				quotedBooking.RunPreSaveValidation();
				AssertEquals("No error should be added to client for inactive staff assignment as there is no change for client", false, quotedBooking.HasErrors);

				quotedBooking.ClientPK = clientB.PK;
				AssertEquals(true, quotedBooking.Quote.QuotationClientAddress.E2_OA_AddressInfo.HasChanges);

				form.FireSaveButton();

				AssertEquals(true, quotedBooking.HasErrors);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				var errorMesssage = "The First Signatory defaults from the Sales Representative for the nominated Local Client. Delete or Replace the inactive record from Organizations>Details>Staff Assignments>Role>Sales Representative";
				AssertHasError(quotedBooking.ClientDocAddress.OrganisationPKInfo, errorMesssage);

				salesRepresentativeB.GS_IsActive = true;
				salesRepresentativeB.Factory.Save();
				form.FireSaveButton();

				AssertEquals("there should not be any error since staff status is active", false, quotedBooking.HasErrors);
				form.Dispose();
			}
		}

		[GuiTest]
		public void TestPrintingExistingQuoteWithInactiveSalesRep_NoErrorShouldBeDisplayed()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.LCL;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "NZAKL";
			quotedBooking.Weight = 100m;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_IsConsignor = true;
			var salesRepresentative = Factory.NewWithValidTestData<GlbStaff>();

			var assignment = client.StaffAssignments.AddNew();
			assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignment.O8_GS_NKPersonResponsible = salesRepresentative.GS_Code;
			salesRepresentative.GS_IsActive = false;

			quotedBooking.ClientPK = client.PK;

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Application.DoEvents();
				Factory.Save();

				quotedBooking.RunPreSaveValidation();
				AssertEquals(false, quotedBooking.HasErrors);

				form.PrintQuoteButton.PerformClick();

				var expectedMessage = "Question " +
					"This Spot Quotation is not internally approved.\r\n" +
					"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.\r\n\r\n" +
					"Do you wish to Approve this quote for printing in Final mode? Otherwise this quote will be printed in Draft mode." +
					"";

				AssertEquals("User can print the quote without any error", expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

				form.Dispose();
			}
		}

		#region Billing Security

		public void TestQuoteChargeSecurityForOneOffQuote()
		{
			var originalValue = Env.Security.OneOffQuoteJobInvoicing.IsAllowed;

			try
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
				var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
				var job = new JobHeader.Loader(quotedBooking).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;

				Env.Security.OneOffQuoteJobInvoicing.IsAllowed = false;

				Factory.Save();

				var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quote.PK));
				var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);

				using (var form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
				{
					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
					AssertNotNull("Should be plugged in", plugIn);
					plugIn.SelectTabPage();
					AssertMultilineASCIIEquals("User should not have access to Charge Quote tab page", @"You do not have the appropriate security rights to run this function.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
Manage -> Tariffs & Rates -> One Off Quotes -> Billing", plugIn.TabPage.Controls[0].Text);
				}

				Env.Security.OneOffQuoteJobInvoicing.IsAllowed = true;

				using (var form = (QuotedBookingForm)controller.ShowEditForm(viewQuotedBooking))
				{
					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
					AssertNotNull("Should be plugged in", plugIn);
					plugIn.SelectTabPage();
					AssertEquals("User should have access to Charge Quote tab page", "", plugIn.TabPage.Controls[0].Text);
				}
			}
			finally
			{
				Env.Security.OneOffQuoteJobInvoicing.IsAllowed = originalValue;
			}
		}

		public void TestBillingSecurityForBooking()
		{
			var originalValue = Env.Security.QuickBookingJobInvoicing.IsAllowed;

			try
			{
				Env.Security.QuickBookingJobInvoicing.IsAllowed = false;

				using (QuotedBookingForm form = new QuotedBookingForm(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory)))
				{
					form.Show();
					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
					AssertNotNull("Should be plugged in", plugIn);
					plugIn.SelectTabPage();
					AssertMultilineASCIIEquals("User should not have access to Charge Quote tab page", @"You do not have the appropriate security rights to run this function.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
Operate -> Forwarding -> Bookings -> Billing", plugIn.TabPage.Controls[0].Text);
				}

				Env.Security.QuickBookingJobInvoicing.IsAllowed = true;

				using (QuotedBookingForm form = new QuotedBookingForm(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory)))
				{
					form.Show();
					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
					AssertNotNull("Should be plugged in", plugIn);
					plugIn.SelectTabPage();
					AssertMultilineASCIIEquals("", plugIn.TabPage.Controls[0].Text);
				}
			}
			finally
			{
				Env.Security.QuickBookingJobInvoicing.IsAllowed = originalValue;
			}
		}

		#endregion

		#region TestSetupJob

		public void TestSetupJob()
		{
			ZGuid quoteOnlyPK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK;
			QuotedBooking quotedBooking = QuotedBooking.New(quoteOnlyPK, ZGuid.Empty, Factory);

			using (QuotedBookingForm form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				quotedBooking.Mode = Core.Constants.RateMode.FCL;
				quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
				quotedBooking.Origin = "AUSYD";
				quotedBooking.Destination = "NZAKL";
				Assert(quotedBooking.Job.JH_GE.IsEmpty);
				form.FireSaveButton();
				Assert(!quotedBooking.Job.JH_GE.IsEmpty);
				Assert(!quotedBooking.Job.JH_GEInfo.HasErrors());
			}
		}

		#endregion

		#region AllowManualShipmentEntry

		[ExpectNoExceptions]
		public void TestSaveAllowManualShipmentEntry()
		{
			var old = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = true;
			try
			{
				var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				AssertShipmentNumberEntryFormShown(quotedBooking, true);

				quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				quotedBooking.Booking.JS_UniqueConsignRef = "ABC";
				AssertShipmentNumberEntryFormShown(quotedBooking, false);

				quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				AssertShipmentNumberEntryFormShown(quotedBooking, false);

				quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				AssertShipmentNumberEntryFormShown(quotedBooking, true);

				quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				quotedBooking.Booking.JS_UniqueConsignRef = "ABC";
				AssertShipmentNumberEntryFormShown(quotedBooking, false);
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = old;
			}
		}

		[ExpectNoExceptions]
		public void TestSaveNotAllowManualShipmentEntry()
		{
			var old = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = false;
			try
			{
				var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				AssertShipmentNumberEntryFormShown(quotedBooking, false);

				quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				quotedBooking.Booking.JS_UniqueConsignRef = "ABC";
				AssertShipmentNumberEntryFormShown(quotedBooking, false);

				quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				AssertShipmentNumberEntryFormShown(quotedBooking, false);

				quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				AssertShipmentNumberEntryFormShown(quotedBooking, false);

				quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				quotedBooking.Booking.JS_UniqueConsignRef = "ABC";
				AssertShipmentNumberEntryFormShown(quotedBooking, false);
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = old;
			}
		}

		void AssertShipmentNumberEntryFormShown(QuotedBooking quotedBooking, bool expectedShipmentNumberEntryFormShown)
		{
			using (QuotedBookingForm form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				form.FireSaveButton();
				if (expectedShipmentNumberEntryFormShown)
				{
					AssertEquals("Should have shown the ShipmentNumberEntryForm", true,
						 ZFormModaliser.LastFormShownDialogForTest != null &&
						 ZFormModaliser.LastFormShownDialogForTest.GetType() == typeof(ShipmentNumberEntryForm));
					Assert("Should have not shown validation error message", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
				else
				{
					AssertNull("Should have not shown the ShipmentNumberEntryForm", ZFormModaliser.LastFormShownDialogForTest);
					Assert("Should have shown validation error message",
					!UnitTestUserNotification.Instance.LastMessage.WasNone && UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are errors - can't save"));
				}

				AssertEquals(false, quotedBooking.IsInDatabase);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		#endregion

		public void TestHiddenTabsDoNotAppearWhenSavingSpotQuote()
		{
			using (QuotedBookingForm form = new QuotedBookingForm(QuotedBooking.New(QuoteBookingType.SpotQuote, Factory)))
			{
				form.Show();

				string[] tabsBeforeSave = GetTabNames(form);

				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder(tabsBeforeSave, GetTabNames(form));
			}
		}

		public void TestNoteGrid_WhenDeleteSaveAndReAddNote_ShouldShowNewNoteInNoteGrid()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var note0 = quotedBooking.Notes.AddNew();
			note0.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
			note0.ST_GC_RelatedCompany = Env.CurrentCompany.PK;
			note0.ST_IsCustomDescription = false;

			Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				form.SelectNotesTab();
				Application.DoEvents();

				var noteTabPage = form.NotesTabPage_Exposed;
				var noteGrid = (ZGrid)noteTabPage.Controls.Find("NoteGrid", true).First();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("NoteGrid should show 1 note", 1, noteGrid.ListManager.Count);
					AssertEquals("NoteGrid should show ! note with correct description", PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description, ((StmNote)noteGrid.List[0]).ST_Description);
				});

				note0.Delete();
				AssertEquals("WHEN delete note THEN NoteGrid should show 0 note", 0, noteGrid.ListManager.Count);
				Factory.Save();
				AssertEquals("WHEN delete note and save THEN NoteGrid should show 0 note", 0, noteGrid.ListManager.Count);

				var note1 = quotedBooking.Notes.AddNew();
				note1.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
				note1.ST_GC_RelatedCompany = Env.CurrentCompany.PK;
				note1.ST_IsCustomDescription = false;
				AssertEquals("WHEN adding new note THEN NoteGrid should show 1 note", 1, noteGrid.ListManager.Count);
			}
		}

		public void TestCustomNoteTypes()
		{
			CustomNoteModuleAndCountry quotedBookingCustomNotes = new CustomNoteModuleAndCountry();
			quotedBookingCustomNotes.ModuleIDName = ModuleIDs.QuotedBookings.Name;
			quotedBookingCustomNotes.CountryCode = "ALL";
			quotedBookingCustomNotes.CustomNoteTypesList.AddNew();

			CustomNoteTypeItem customNote = quotedBookingCustomNotes.CustomNoteTypesList[0];
			customNote.NoteName = "Custom Quoted Booking Note";
			customNote.IsTextOnly = ZBool.True;
			customNote.IsAppendingNote = ZBool.True;
			customNote.IsReadOnlyAfterAdd = ZBool.False;
			customNote.ForceRead = ZBool.False;
			customNote.DefaultVisibility = nameof(StmNoteVisibility.PRV);

			CustomNoteTypes customNoteTypes = new CustomNoteTypes();
			customNoteTypes.NoteModuleAndCountryList.Add(quotedBookingCustomNotes);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customNoteTypes);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ControllerID = ControllerIDs.QuotedBookings;

				form.Show();
				AssertNull("CustomNoteTypesDelegate hasn't been set up yet", quotedBooking.CustomNoteTypesDelegate);

				form.SelectNotesTab();
				AssertNotNull("CustomNoteTypesDelegate has been set up by the Notes tab page", quotedBooking.CustomNoteTypesDelegate);

				Assert("Expected custom note type for quoted bookings",
					quotedBooking.NoteTypes.Cast<PredefinedNoteType>()
					.Any(noteType => noteType.Description == "Custom Quoted Booking Note"));
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new QuotedBookingForm(quotedBooking));
		}

		public void TestPlugIns_TransportBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking);
				AssertNotNull("Should be plugged in", plugIn);
			}

			var quoteOnly = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			using (var form = new QuotedBookingForm(quoteOnly))
			{
				form.Show();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking);
				AssertNull("Not applicable for spot quotes", plugIn);
			}
		}

		public void TestImportGlobalSailingSchedules()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Mode = Core.Constants.RateMode.FCL;

			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new QuotedBookingForm(booking))
			{
				form.Show();

				var tabControl = (ZTemplateTabControl)form.AdditionalDetailsTabPage.Parent;
				tabControl.SelectTab(form.AdditionalDetailsTabPage);

				var importButton = form.QuotedBookingAdditionalDetailsControl.Controls.Find("ImportGlobalSchedulesButton", true)
					.OfType<ZButton>()
					.SingleOrDefault();

				AssertEquals("Button should be hidden when functionality is disabled in registry", false, importButton.Visible);
			}

			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(booking))
			{
				form.Show();

				var tabControl = (ZTemplateTabControl)form.AdditionalDetailsTabPage.Parent;
				tabControl.SelectTab(form.AdditionalDetailsTabPage);

				var importButton = form.QuotedBookingAdditionalDetailsControl.Controls.Find("ImportGlobalSchedulesButton", true)
					.OfType<ZButton>()
					.SingleOrDefault();

				AssertEquals("Button should be visible", true, importButton.Visible);

				importButton.PerformClick();
				AssertEquals(true, ZFormModaliser.LastFormShownForTest is Freight.GUI.OnlineSailingSchedules.OnlineSchedulesForm);
			}
		}

		public void TestMenuItemsAsControlButtonAlternates_Visibility()
		{
#if WINZOR
			using var mainForm = ShowMainFormForTest();
#endif
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			quotedBooking.TransportMode = Constants.TransportModes.Sea;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "NZAKL";
			quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
			Factory.Save();

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				AssertEquals("1.1 Print menu item should be visible", true, bookingForm.PrintMenuItem.Visible);
				AssertEquals("1.2 Convert Quote to Booking menu item should be visible", true, bookingForm.ConvertQuoteToBookingMenuItem.Visible);
				AssertEquals("1.3 Approve menu item should be visible", true, bookingForm.ApproveOneOffMenuItem.Visible);

				bookingForm.ConvertQuoteToQuotedBookingButton.PerformClick();
				Factory.Save();
				Application.DoEvents();
				var newForm = bookingForm.PopupForm_ForTesting as QuotedBookingForm;
				AssertEquals("2.1 Print menu item should be visible", true, newForm.PrintMenuItem.Visible);
				AssertEquals("2.2 Convert Quote to Booking menu item should not be visible", false, newForm.ConvertQuoteToBookingMenuItem.Visible);
				AssertEquals("2.3 Approve menu item should not be visible", false, newForm.ApproveOneOffMenuItem.Visible);
			}
		}

		public void TestMenuItemsAsControlButtonAlternates_QuickBooking_Visibility_None()
		{
			var quickBooking = GetBooking();
			quickBooking.TransportMode = "AIR";
			quickBooking.Factory.Save();

			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Print menu item should not be visible", false, form.PrintMenuItem.Visible);
				AssertEquals("Convert Quote to Booking menu item should not be visible", false, form.ConvertQuoteToBookingMenuItem.Visible);
				AssertEquals("Approve menu item should not be visible", false, form.ApproveOneOffMenuItem.Visible);
			}
		}

		public void TestSpliContainerExistsForPanelsBookingServicesAndReferenceNumbers()
		{
			QuotedBooking quotedBooking = GetSavedQuotedBooking();

			using (QuotedBookingForm bookingForm = new QuotedBookingForm(quotedBooking))
			{
				bookingForm.Show();

				var quotedBookingAdditionalDetailsControl = bookingForm.AdditionalDetailsTabPage.Controls.Find("QuotedBookingAdditionalDetailsControl", true)[0];
				var bottomPanel = quotedBookingAdditionalDetailsControl.Controls.Find("BottomFillPanel", true)[0];
				var splitContainer = (KSplitContainer)bottomPanel.Controls.Find("ServicesAndReferenceNumberDynamicControlSplitContainer", true)[0];
				AssertNotNull(splitContainer);

				var servicesControl = splitContainer.Panel1.Controls.Find("servicesControl", true)[0];
				var referenceNumbersControl = splitContainer.Panel2.Controls.Find("referenceNumbersGroupBoxControl", true)[0];

				AssertNotNull(servicesControl);
				AssertNotNull(referenceNumbersControl);
			}
		}

		#region Finalising One Off Quote Quote

		[GuiTest]
		public void TestFactorySave_QuoteFinalisedByPrinting_CancelsTaskAndActionsExceptions()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "CNSHA";
			quote.CurrentOneOffQuote.TT_ActualWeight = 100m;

			var workflowProvider = quotedBooking as IWorkflowProvider;

			var quoteOpenTask = workflowProvider.WorkflowItems.Tasks.AddNew();
			quoteOpenTask.P9_Type = "UDF";
			quoteOpenTask.P9_Status = "OPN";

			var quoteWorkingTask = workflowProvider.WorkflowItems.Tasks.AddNew();
			quoteWorkingTask.P9_Type = "UDF";
			quoteWorkingTask.P9_Status = "WRK";

			var quoteException = workflowProvider.WorkflowItems.Exceptions.AddNew();
			quoteException.P9_SE_NKExceptionEvent = "ATH";

			Factory.Save();

			using (QuotedBookingFormForTest form = new QuotedBookingFormForTest(quotedBooking))
			{
				quote.TH_IsLocked = true;
				form.UpdateQuoteLockedReadOnlyState_Exposed();
				quotedBooking.Factory.Save();
			}

			CombineAssertions(() =>
			{
				Assert("Quote should be locked", quote.TH_IsLocked);
				Assert("Quote should be read only", quote.ReadOnly);
				AssertEquals("CAN", quoteOpenTask.P9_Status);
				AssertEquals("CLS", quoteWorkingTask.P9_Status);
				AssertEquals("Exception should be actioned upon Finalizing Quote", true, quoteException.IsExceptionActioned);
			});
		}

		#endregion

		#region Controlling Agent authorize request during saving

		public void TestQuotedBookingFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsNotEmpty()
		{
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_Code = "CAG";
			var controllingAgentAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingAgentAddress.OA_OH = controllingAgent.PK;
			controllingAgentAddress.OA_Code = "CAG";

			var quotedBooking = GetNewQuoteBookingWithShipment(Constants.TransportModes.Courier);
			quotedBooking.Booking.ControllingAgentDocumentaryAddress.E2_OA_Address = controllingAgentAddress.PK;

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();
				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, quotedBooking.Booking.IsInDatabase);
		}

		public void TestQuotedBookingNotSavedWhenControllingAgentIsEmptyAndAuthorizeRequestCancelled()
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(Constants.TransportModes.Courier);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, quotedBooking.IsInDatabase);
		}

		public void TestQuotedBookingNotSavedWhenControllingAgentIsEmptyAndUserEnteredToAthorizationFormDoesNotExists()
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(Constants.TransportModes.Courier);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();

				int loginFormShownCount = 0;

				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest("aaa", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not exist, password is invalid or password is expired.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, quotedBooking.IsInDatabase);
		}

		public void TestQuotedBookingNotSavedWhenControllingAgentIsEmptyAndSupervisorDoesNotHaveSecurityRightsToOverrideSecurityPolicy()
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(Constants.TransportModes.Courier);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();

				int loginFormShownCount = 0;
				var newUserLogin = "User1";
				var newUserPassword = "pass";

				SecurityTestObject.CreateTestUser(false, Env.Security.QuickBookingAllowSaveWithoutControllingAgent.Code, "US1", newUserLogin, newUserPassword);
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not have security rights to override this security policy.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, quotedBooking.IsInDatabase);
		}

		public void TestQuotedBookingFormSavedAndNoControllingAgentAuthorizeRequestPriorToRegistryDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsDebtor = true;
			var consignorAddress = Factory.NewWithValidTestData<OrgAddress>();
			consignorAddress.OA_OH = consignor.PK;
			consignorAddress.OA_Code = "CNR";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_OH = consignee.PK;
			consigneeAddress.OA_Code = "CNE";

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			booking.JS_ActualWeight = 200m;
			booking.JS_ActualVolume = 1m;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.ClientPK = consignor.PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = department.PK;

			quotedBooking.GetControllingAgentSecurityCheckPoint().IsAllowed = false;

			using (booking.GetMandatoryControllingAgentEffectiveDateRegistry().SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(3).ToDateTime()))
			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();
				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, quotedBooking.Booking.IsInDatabase);
		}

		public void TestQuotedBookingFormNotSavedAndShowControllingAgentAuthorizeRequestAfterRegistryDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsDebtor = true;
			var consignorAddress = Factory.NewWithValidTestData<OrgAddress>();
			consignorAddress.OA_OH = consignor.PK;
			consignorAddress.OA_Code = "CNR";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_OH = consignee.PK;
			consigneeAddress.OA_Code = "CNE";

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			booking.JS_ActualWeight = 200m;
			booking.JS_ActualVolume = 1m;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.ClientPK = consignor.PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = department.PK;

			quotedBooking.GetControllingAgentSecurityCheckPoint().IsAllowed = false;

			using (booking.GetMandatoryControllingAgentEffectiveDateRegistry().SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-2).ToDateTime()))
			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, quotedBooking.IsInDatabase);
		}

		public void TestQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions()
		{
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.RateMode.LSE, Env.Security.QuickBookingAllowSaveWithoutControllingAgentAir);
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.RateMode.FCL, Env.Security.QuickBookingAllowSaveWithoutControllingAgentSea);
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.RateMode.FRO, Env.Security.QuickBookingAllowSaveWithoutControllingAgentRoad);
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.RateMode.FRA, Env.Security.QuickBookingAllowSaveWithoutControllingAgentRail);
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.RateMode.COU, Env.Security.QuickBookingAllowSaveWithoutControllingAgent);
		}

		public void TestQuotedBookingNotSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm()
		{
			AssertQuotedBookingNotSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.LSE, Env.Security.QuickBookingAllowSaveWithoutControllingAgentAir);
			AssertQuotedBookingNotSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.FCL, Env.Security.QuickBookingAllowSaveWithoutControllingAgentSea);
			AssertQuotedBookingNotSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.FRO, Env.Security.QuickBookingAllowSaveWithoutControllingAgentRoad);
			AssertQuotedBookingNotSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.FRA, Env.Security.QuickBookingAllowSaveWithoutControllingAgentRail);
			AssertQuotedBookingNotSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.COU, Env.Security.QuickBookingAllowSaveWithoutControllingAgent);
		}

		void AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(string transportMode, SecurityCheckpoint securityCheckpoint)
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(transportMode);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = false;
			securityCheckpoint.IsAllowed = true;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();
				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, quotedBooking.IsInDatabase);
		}

		void AssertQuotedBookingNotSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(string transportMode, SecurityCheckpoint securityCheckpoint)
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(transportMode);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = false;
			securityCheckpoint.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();
				int loginFormShownCount = 0;

				var newUserLogin = "User1" + transportMode;
				var newUserPassword = "pass";

				SecurityTestObject.CreateTestUser(true, securityCheckpoint.Code, transportMode, newUserLogin, newUserPassword);
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(true, quotedBooking.IsInDatabase);
		}

		#endregion

		#region Controlling Customer authorize request during saving

		public void TestQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsNotEmpty()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CAG";
			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CAG";

			var quotedBooking = GetNewQuoteBookingWithShipment(Constants.TransportModes.Courier);
			quotedBooking.Booking.ControllingCustomerAddress.E2_OA_Address = controllingCustomerAddress.PK;

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.QuickBookingAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();
				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, quotedBooking.Booking.IsInDatabase);
		}

		public void TestQuotedBookingNotSavedWhenTransportModeCourierAndControllingCustomerIsEmptyAndAuthorizeRequestCancelled()
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(Constants.TransportModes.Courier);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.QuickBookingAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, quotedBooking.IsInDatabase);
		}

		public void TestQuotedBookingNotSavedWhenTransportModeCourierAndControllingCustomerIsEmptyAndUserEnteredToAthorizationFormDoesNotExists()
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(Constants.TransportModes.Courier);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.QuickBookingAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();

				int loginFormShownCount = 0;

				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest("aaa", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not exist, password is invalid or password is expired.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, quotedBooking.IsInDatabase);
		}

		public void TestQuotedBookingNotSavedWhenTransportModeCourierAndControllingCustomerIsEmptyAndSupervisorDoesNotHaveSecurityRightsToOverrideSecurityPolicy()
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(Constants.TransportModes.Courier);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.QuickBookingAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();

				int loginFormShownCount = 0;
				var newUserLogin = "User1";
				var newUserPassword = "pass";

				SecurityTestObject.CreateTestUser(false, Env.Security.QuickBookingAllowSaveWithoutControllingAgent.Code, "US1", newUserLogin, newUserPassword);
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not have security rights to override this security policy.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, quotedBooking.IsInDatabase);
		}

		public void TestQuotedBookingFormSavedAndNoControllingCustomerAuthorizeRequestPriorToRegistryDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsDebtor = true;
			var consignorAddress = Factory.NewWithValidTestData<OrgAddress>();
			consignorAddress.OA_OH = consignor.PK;
			consignorAddress.OA_Code = "CNR";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_OH = consignee.PK;
			consigneeAddress.OA_Code = "CNE";

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			booking.JS_ActualWeight = 200m;
			booking.JS_ActualVolume = 1m;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.ClientPK = consignor.PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = department.PK;

			quotedBooking.GetControllingCustomerSecurityCheckPoint().IsAllowed = false;

			using (booking.GetMandatoryControllingCustomerEffectiveDateRegistry().SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(3).ToDateTime()))
			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();
				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, quotedBooking.Booking.IsInDatabase);
		}

		public void TestQuotedBookingFormNotSavedAndShowControllingCustomerAuthorizeRequestAfterRegistryDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsDebtor = true;
			var consignorAddress = Factory.NewWithValidTestData<OrgAddress>();
			consignorAddress.OA_OH = consignor.PK;
			consignorAddress.OA_Code = "CNR";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_OH = consignee.PK;
			consigneeAddress.OA_Code = "CNE";

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			booking.JS_ActualWeight = 200m;
			booking.JS_ActualVolume = 1m;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.ClientPK = consignor.PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = department.PK;

			quotedBooking.GetControllingCustomerSecurityCheckPoint().IsAllowed = false;

			using (booking.GetMandatoryControllingCustomerEffectiveDateRegistry().SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-2).ToDateTime()))
			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, quotedBooking.IsInDatabase);
		}

		public void TestQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions()
		{
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.RateMode.LSE, Env.Security.QuickBookingAllowSaveWithoutControllingCustomerAir);
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.RateMode.FCL, Env.Security.QuickBookingAllowSaveWithoutControllingCustomerSea);
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.RateMode.FRO, Env.Security.QuickBookingAllowSaveWithoutControllingCustomerRoad);
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.RateMode.FRA, Env.Security.QuickBookingAllowSaveWithoutControllingCustomerRail);
			AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.RateMode.COU, Env.Security.QuickBookingAllowSaveWithoutControllingCustomer);
		}

		public void TestQuotedBookingSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm()
		{
			AssertQuotedBookingSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.LSE, Env.Security.QuickBookingAllowSaveWithoutControllingCustomerAir);
			AssertQuotedBookingSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.FCL, Env.Security.QuickBookingAllowSaveWithoutControllingCustomerSea);
			AssertQuotedBookingSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.FRO, Env.Security.QuickBookingAllowSaveWithoutControllingCustomerRoad);
			AssertQuotedBookingSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.FRA, Env.Security.QuickBookingAllowSaveWithoutControllingCustomerRail);
			AssertQuotedBookingSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.RateMode.COU, Env.Security.QuickBookingAllowSaveWithoutControllingCustomer);
		}

		void AssertQuotedBookingSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(string transportMode, SecurityCheckpoint securityCheckpoint)
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(transportMode);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.QuickBookingAllowSaveWithoutControllingCustomer.IsAllowed = false;
			securityCheckpoint.IsAllowed = true;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();
				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, quotedBooking.IsInDatabase);
		}

		void AssertQuotedBookingSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(string transportMode, SecurityCheckpoint securityCheckpoint)
		{
			var quotedBooking = GetNewQuoteBookingWithShipment(transportMode);

			Env.Security.QuickBookingAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.QuickBookingAllowSaveWithoutControllingCustomer.IsAllowed = false;
			securityCheckpoint.IsAllowed = false;

			using (var quotedBookingForm = new QuotedBookingForm(quotedBooking))
			{
				quotedBookingForm.Show();
				int loginFormShownCount = 0;

				var newUserLogin = "User1" + transportMode;
				var newUserPassword = "pass";

				SecurityTestObject.CreateTestUser(true, securityCheckpoint.Code, transportMode, newUserLogin, newUserPassword);
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = quotedBookingForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(true, quotedBooking.IsInDatabase);
		}

		#endregion

		#region Update HBL Booking Status

		public void TestPromptRejectionReasonWhenStatusUpdatedToBKJ()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			Factory.Save();

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;

				var msg = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					Assert("No message prompted", msg.WasNone);
					Assert("Status should be read-only", quotedBooking.ShipmentStatusInfo.ReadOnly);
					AssertEquals("Status should be set", ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);
				});

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				AssertEquals(ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);

				UnitTestUserNotification.Instance.AddUserResponse(ZString.Empty);
				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;

				msg = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertNotNull("Should prompt message for rejection reason input", msg);
					AssertEquals("Check message caption", "Rejection Reason", msg.Caption);
					AssertEquals("Check message text", "Please enter the reason of rejection.", msg.Text);
					AssertEquals("Status should not be set", ShipmentStatusList.Codes.ElectronicBooking, quotedBooking.ShipmentStatus);
				});

				UnitTestUserNotification.Instance.AddUserResponse("Non empty user response.");
				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;

				CombineAssertions(() =>
				{
					AssertNotNull("Should prompt message for rejection reason input", msg);
					AssertEquals("Check message caption", "Rejection Reason", msg.Caption);
					AssertEquals("Check message text", "Please enter the reason of rejection.", msg.Text);
					AssertEquals("Status should be set", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);
				});
			}
		}

		#endregion

		#region Send Rejection Message When Setting Inactive

		public void TestSendRejectionMessageWhenSettingInactive_WhenBookingStatusIsEBK()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			Factory.Save();

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				makeInactive.PerformClick();

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Message Prompt was fired", ZDialogResult.Yes, msg.Answer);
					AssertEquals("Check message caption", "Booking Rejection Message", msg.Caption);
					AssertEquals("Check message text", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", msg.Text);
					AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);
					AssertContains("A status changed event should have been logged", "|RES=Booking Cancelled|", quotedBooking.Logs.MostRecentLog.SL_Reference);
				});
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenBookingStatusIsNotEBK()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Factory.Save();

			var expectedLog = GetSTULogs(quotedBooking).FirstOrDefault();
			AssertNull(expectedLog);

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				makeInactive.PerformClick();

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Message Prompt wasn't fired", ZDialogResult.None, msg.Answer);
					AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);

					expectedLog = GetSTULogs(quotedBooking).FirstOrDefault();
					AssertNull("No new status changed event should have been logged", expectedLog);
				});
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenUserSelectsNo()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			Factory.Save();

			var stuLogsCount = GetSTULogs(quotedBooking).Count();

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				makeInactive.PerformClick();

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Message Prompt was fired", ZDialogResult.No, msg.Answer);
					AssertEquals("Check message caption", "Booking Rejection Message", msg.Caption);
					AssertEquals("Check message text", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", msg.Text);
					AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);
					AssertEquals("No new status changed event should have been logged", stuLogsCount, GetSTULogs(quotedBooking).Count());
				});
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenBookingIsAlreadyRejected()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			Factory.Save();

			var stuLogsCount = GetSTULogs(quotedBooking).Count();

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				makeInactive.PerformClick();

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Message Prompt wasn't fired", ZDialogResult.None, msg.Answer);
					AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);
					AssertEquals("No new Status changed event should have been logged", stuLogsCount, GetSTULogs(quotedBooking).Count());
				});
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenBookingStatusHasEBKInHistory()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking.LogStatusChangedEvent(ShipmentStatusList.Codes.Confirmed, "Reason");
			quotedBooking.LogStatusChangedEvent(ShipmentStatusList.Codes.ElectronicBooking, "Reason");
			quotedBooking.LogStatusChangedEvent(ShipmentStatusList.Codes.ContainerTransportArranged, "Reason");

			Factory.Save();

			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				makeInactive.PerformClick();

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Message Prompt was fired", ZDialogResult.Yes, msg.Answer);
					AssertEquals("Check message caption", "Booking Rejection Message", msg.Caption);
					AssertEquals("Check message text", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", msg.Text);
					AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);
					AssertContains("A status changed event should have been logged", "|RES=Booking Cancelled|", quotedBooking.Logs.MostRecentLog.SL_Reference);
				});
			}
		}

		#endregion

		#region ExportBroker/ImportBroker

		public void TestQuotedBookingForm_ConsigneePK_Set_ImportBroker()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "SGSIN";
			booking.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Factory.Save();

			booking.ConsigneePK = ZGuid.NewZGuid();
			Assert(booking.JS_OH_ImportBroker.IsEmpty);

			var testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();
			booking.JS_TransportMode = Constants.TransportModes.Air;

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				booking.ConsigneePK = testPKs.OrgHeader;
				AssertEquals("Booking should have AIR customs broker", testPKs.AirImportCustomsBroker, booking.JS_OH_ImportBroker);
				AssertEquals("Consignee has been changed. Do you wish to update the Import Broker?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingForm_ConsignorPK_Set_ExportBroker()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			booking.JS_RL_NKDestination = "";
			Factory.Save();

			booking.JS_OH_ExportBroker = ZGuid.Empty;
			booking.JS_RX_NKGoodsValueCurr = ZString.Empty;
			booking.ConsignorPK = ZGuid.Empty;
			Assert(booking.JS_OH_ExportBroker.IsEmpty);
			Assert(booking.JS_RX_NKGoodsValueCurr.IsEmpty);
			Assert(booking.ConsignorPK.IsEmpty);

			booking.ConsignorPK = Factory.New<OrgHeader>().PK;
			Assert(booking.JS_OH_ExportBroker.IsEmpty);
			Assert(booking.DocsAndCartage.PickupCartageCoPK.IsEmpty);

			var testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.Show();

				booking.ConsignorPK = testPKs.OrgHeader;
				AssertEquals("Booking should have an Exporter", testPKs.OrgHeader, booking.ConsignorPK);
				AssertEquals("Booking should have SEA customs broker", testPKs.SeaExportCustomsBroker, booking.JS_OH_ExportBroker);
				AssertEquals("Consignor has been changed. Do you wish to update the Export Broker?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Universal Copy

		public void TestScheduledCopyIsDisabled()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Factory.Save();

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.ControllerID = ControllerIDs.QuotedBookings;
				bookingForm.Show();

				var actionsMenu = bookingForm.Menu.MenuItems.FindByName("ActionsMenuItem");
				actionsMenu.ShowPopupMenu();

				var copySchedulesMenu = actionsMenu.MenuItems.FindByName("CopySchedules", true);
				Assert("Menu is disabled", !copySchedulesMenu.Enabled);
				Assert("Menu is hidden", !copySchedulesMenu.Visible);
			}
		}

		#endregion

		#region Template record

		public void TestQuotedBookingNotDirtiedByLoadingTemplateRecord()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Quote.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			quotedBooking.Booking.JS_TransportMode = "AIR";
			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider1 = quotedBooking as ITemplateRecordProvider;
			templateRecordProvider1.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			AssertEquals("Template Record is not in the database yet", false, templateRecord.IsInDatabase);

			Factory.Save();

			AssertEquals("Template Record is already in the database", true, templateRecord.IsInDatabase);

			var quotedBooking2 = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking2.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var templateRecordProvider2 = (ITemplateRecordProvider)quotedBooking2;
			templateRecordProvider2.LoadFromTemplateRecord(templateRecord);
			AssertEquals("HasChanges should be false after loading the template record", false, quotedBooking2.HasChanges);

			using (var form = new QuotedBookingForm(quotedBooking2))
			{
				form.Show();
				Application.DoEvents();
				TabPageNotificationsExposer.ExposeTabPageNotifications(form, form.BusinessEntity);
				AssertEquals("Shipment must not be dirtied by viewing", false, quotedBooking2.HasChanges);
			}
		}

		public void TestTemplateRecordValidationStandard()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.StandardValidation);
			AssertTemplateRecordValidation(true, false);
		}

		public void TestTemplateRecordValidationNone()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.NoValidation);
			AssertTemplateRecordValidation(false, true);
		}

		public void TestTemplateRecordValidationIgnoreAndSave()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.IgnoreAndSave);
			AssertTemplateRecordValidation(true, true, DialogResult.Ignore);
		}

		public void TestTemplateRecordValidationIgnoreAndSaveAbort()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.IgnoreAndSave);
			AssertTemplateRecordValidation(true, false, DialogResult.Abort);
		}

		void AssertTemplateRecordValidation(bool expectValidation, bool expectSave, DialogResult errorDialogResult = DialogResult.OK)
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, factory);
			quotedBooking.Quote.TH_OH = factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			quotedBooking.Booking.JS_TransportMode = "AIR";
			factory.TemplateRecordProvider = quotedBooking;

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			var templateRecordProvider = (ITemplateRecordProvider)quotedBooking;
			templateRecordProvider.TemplateRecord = templateRecord;
			templateRecordProvider.IsTemplateRecord = true;

			quotedBooking.Booking.JS_TransportMode = "XYZ";
			AssertEquals(expectValidation, quotedBooking.Booking.JS_TransportModeInfo.HasErrors());

			ChildEditableService.SetState(quotedBooking.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.ShowErrorsDialogCloseDialogResultForTest = errorDialogResult;

				AssertEquals(true, quotedBooking.HasChanges);
				AssertEquals(false, templateRecord.IsInDatabase);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);

				form.FireSaveButton();

				AssertEquals(!expectSave, quotedBooking.HasChanges);
				AssertEquals(expectSave, templateRecord.IsInDatabase);
				AssertEquals(expectSave ? ODisplayMode.Browse : ODisplayMode.Edit, form.DisplayMode);
			}
		}

		public void TestTemplateNameTextBoxExists()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, factory);
			factory.TemplateRecordProvider = quotedBooking;

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			var templateRecordProvider = (ITemplateRecordProvider)quotedBooking;
			templateRecordProvider.TemplateRecord = templateRecord;
			templateRecordProvider.IsTemplateRecord = true;

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				var templateNameTextBox = form.Controls.Find("templateNameTextBox", true).FirstOrDefault();
				AssertNotNull("templateNameTextBox should exist when QuotedBooking is a template", templateNameTextBox);

				AssertEquals("templateNameTextBox should be visible", true, templateNameTextBox.Visible);
			}
		}
		#endregion

		#region TestScreeningStatusDropEditVisibilityWhenNVOCC

		public void TestScreeningStatusDropEditVisibilityWhenNVOCC()
		{
			AssertScreeningStatusDropEditVisibilityWhenNVOCC(false, true);
			AssertScreeningStatusDropEditVisibilityWhenNVOCC(true, false);

			void AssertScreeningStatusDropEditVisibilityWhenNVOCC(bool isTemplate, bool expectedVisible)
			{
				QuotedBooking quotedBooking;

				if (isTemplate)
				{
					var factory = new TemplateRecordBusinessObjectFactory();
					var booking = QuotedBooking.CreateNewBooking(factory);
					quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, factory);
					factory.TemplateRecordProvider = quotedBooking;
					factory.TemplateRecordProvider.IsTemplateRecord = true;
					AssertNotNull(quotedBooking.Booking);
				}
				else
				{
					var booking = QuotedBooking.CreateNewBooking(Factory);
					booking.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
					AssertNotNull(quotedBooking.Booking);
				}

				AssertEquals(isTemplate, quotedBooking.IsTemplate);

				using (var bookingForm = new QuotedBookingForm(quotedBooking))
				{
					bookingForm.Show();
					var mainTab = (ZTemplateTabControl)bookingForm.AdditionalDetailsTabPage.Parent;

					bookingForm.NVOCCModeCheckBox.Checked = true;
					mainTab.SelectTab(0);

					var screeningStatusDropEdit = mainTab.SelectedTab.Controls.Find("ScreeningStatusDropEditNVOCC", true).Single();
					AssertEquals(expectedVisible, screeningStatusDropEdit.Visible);
				}
			}
		}

		#endregion

		#region TestAddScreeningLogsTabPage

		public void TestAddScreeningLogsTabPage()
		{
			AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(true, true, false, "Denied Party Screening Logs");
		}

		public void TestAddScreeningLogsTabPage_BookingIsNullOrNot()
		{
			AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(false, false, false, "Denied Party Screening Logs");
			AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(true, true, false, "Denied Party Screening Logs");
		}

		public void TestAddScreeningLogsTabPage_QuotedBookingIsTemplate()
		{
			AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(true, true, false, "Denied Party Screening Logs");
			AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(true, false, true, "Denied Party Screening Logs");
		}

		void AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(bool hasBooking, bool isRegistryEnabled, bool isTemplateQuoteBooking, string textTabPage)
		{
			QuotedBooking quotedBooking;

			if (hasBooking)
			{
				if (isTemplateQuoteBooking)
				{
					var factory = new TemplateRecordBusinessObjectFactory();
					var booking = QuotedBooking.CreateNewBooking(factory);
					quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, factory);
					factory.TemplateRecordProvider = quotedBooking;
					factory.TemplateRecordProvider.IsTemplateRecord = true;
					AssertNotNull(quotedBooking.Booking);
					Assert(quotedBooking.IsTemplate);
				}
				else
				{
					var booking = QuotedBooking.CreateNewBooking(Factory);
					booking.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
					AssertNotNull(quotedBooking.Booking);
					Assert(!quotedBooking.IsTemplate);
				}
			}
			else
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
				quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
				AssertNull(quotedBooking.Booking);
				Assert(!quotedBooking.IsTemplate);
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(isRegistryEnabled)))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();

				var tabControl = (ZTabControl)form.LogsTabPageForTest.Controls[0].Controls[0];
				var specifiedTabPage = tabControl.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Text == textTabPage);

				if (isRegistryEnabled)
				{
					AssertNotNull(specifiedTabPage);

					if (textTabPage == "Denied Party Screening Logs")
					{
						AssertEquals(typeof(RelatedDeniedPartyScreeningStatusControl), specifiedTabPage.Controls[0].GetType());
						AssertEquals(2, tabControl.TabPages.Count);
						AssertEquals("Denied Party Screening Logs", tabControl.TabPages[1].Text);
					}
					else
					{
						AssertEquals(typeof(ComplianceLogUserControl), specifiedTabPage.Controls[0].GetType());
						AssertEquals(2, tabControl.TabPages.Count);
						AssertEquals("Compliance Logs", tabControl.TabPages[1].Text);

						var tabControl2 = (ZTabControl)tabControl.TabPages[1].Controls[0].Controls[0];
						AssertEquals(3, tabControl2.TabPages.Count);
						AssertEquals("Compliance Risk Status Changes", tabControl2.TabPages[0].Text);
						AssertEquals("Removed Commodities", tabControl2.TabPages[1].Text);
						AssertEquals("Denied Party Screening Logs", tabControl2.TabPages[2].Text);
					}
				}
				else
				{
					AssertNull(specifiedTabPage);
				}
			}
		}

		#endregion

		#region TestAddComplianceLogsTabPage

		public void TestAddComplianceTabPage()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(true, true, false, "Compliance Logs");
			}
		}

		public void TestAddComplianceTabPage_BookingIsNull()
		{
			AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(false, false, false, "Compliance Logs");
		}

		public void TestAddComplianceTabPage_QuotedBookingIsTemplate()
		{
			AssertDeniedPartyScreeningLogsOrComplianceLogsTabPage(true, false, true, "Compliance Logs");
		}

		#endregion

		#region CO2 Emission

		public void TestCO2EmissionForOneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			AssertCO2Emission(quotedBooking);
		}

		public void TestCO2EmissionForBookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			AssertCO2Emission(quotedBooking);
		}

		void AssertCO2Emission(QuotedBooking quotedBooking)
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
				AssertNull("Registry is false, should not have added CO2e Plugin", co2ePlugin);
				AssertNull("Registry is false, should not show Calculate Greenhouse Gas Emissions (CO2e) menu", menuItem);
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
				AssertNotNull("Registry is true, should have added CO2e Plugin", co2ePlugin);
				AssertNotNull("Registry is true, should show Calculate Greenhouse Gas Emissions (CO2e) menu", menuItem);
			}
		}

		public void TestNoCO2ePluginInBookingTemplate()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			((ITemplateRecordProvider)quotedBooking).IsTemplateRecord = true;

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
				AssertNull(co2ePlugin);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_OOQ_EHub()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var ooq = quote.CurrentOneOffQuote;
			ooq.TT_TransportMode = Constants.TransportModes.Air;
			ooq.TT_ContainerMode = Constants.ContainerModes.Loose;
			ooq.TT_RL_NKReceivalLocation = "AUSYD";
			ooq.TT_RL_NKDeliveryLocation = "USLAX";
			ooq.TT_ActualWeight = 1;
			ooq.TT_UnitOfWeight = "T";

			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();

				var calculateCO2EmissionMenuItem = FindCalculateCO2EmissionMenu(form);
				calculateCO2EmissionMenuItem.PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});

				AssertEquals("CO2eStatus should be set", CO2eStatusList.Codes.Pending, quotedBooking.GetCO2eStatus());
				AssertEquals("TotalCO2eForBinding should be set", "Pending", quotedBooking.TotalCO2eForBinding);
				AssertEquals("TotalCO2eForSorting should be set", 0m, quotedBooking.TotalCO2eForSorting);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_OOQ_Api()
		{
			// Arrange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var ooq = quote.CurrentOneOffQuote;
			ooq.TT_TransportMode = TransportModes.Air;
			ooq.TT_ContainerMode = ContainerModes.Loose;
			ooq.TT_RL_NKReceivalLocation = "BDAKH";
			ooq.TT_RL_NKDeliveryLocation = "USLAX";
			ooq.TT_ActualWeight = 1;
			ooq.TT_UnitOfWeight = "T";

			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			var client = new Mock<IApiClient>();
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(() => GenerateEmissionResponse("Enterprise.Freight.QuotedBookings.GUI.Test.QuotedBooking.TestFiles.CO2eUniversalShipment_Response_Booking.xml"));

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			{
				form.Show();
				Factory.Save();

				// Act
				var calculateCO2EmissionMenuItem = FindCalculateCO2EmissionMenu(form);
				calculateCO2EmissionMenuItem.PerformClick();

				// Assert
				AssertEquals("Quoted Booking CO2eStatus should be set", CO2eStatusList.Codes.Current, quotedBooking.GetCO2eStatus());
				AssertEquals("CO2ePerTonneInKg should be set", 9959m, quotedBooking.GetTotalCO2e());
				AssertEquals("TotalCO2eForBinding should be set", "9,959", quotedBooking.TotalCO2eForBinding);
				AssertEquals("TotalCO2eForSorting should be set", 9959m, quotedBooking.TotalCO2eForSorting);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_BWQ_EHub()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "VNVNH";
			quotedBooking.Mode = "LSE";
			quotedBooking.Booking.JS_ActualWeight = 1m;

			var sailing = CreateSailing(HomePort, OverseasPort);
			quotedBooking.Booking.JS_JX = sailing.PK;

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();

				var calculateCO2EmissionMenuItem = FindCalculateCO2EmissionMenu(form);
				calculateCO2EmissionMenuItem.PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});

				AssertEquals("Booking CO2eStatus should be set", CO2eStatusList.Codes.Pending, quotedBooking.GetCO2eStatus());
				AssertEquals("Sailing CO2eStatus should be set", CO2eStatusList.Codes.Pending, quotedBooking.Booking.Sailing.GetCO2eStatus());
				AssertEquals("TotalCO2eForBinding should be set", "Pending", quotedBooking.TotalCO2eForBinding);
				AssertEquals("TotalCO2eForSorting should be set", 0m, quotedBooking.TotalCO2eForSorting);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_BWQ_Api()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Origin = "BDAKH";
			quotedBooking.Destination = "USLAX";
			quotedBooking.Mode = ContainerModes.Loose;
			quotedBooking.Booking.JS_ActualWeight = 1m;

			var sailing = CreateSailing("BDAKH", "USLAX");
			quotedBooking.Booking.JS_JX = sailing.PK;

			var client = new Mock<IApiClient>();
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(() => GenerateEmissionResponse("Enterprise.Freight.QuotedBookings.GUI.Test.QuotedBooking.TestFiles.CO2eUniversalShipment_Response_BookingWithLeg.xml"));

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			{
				form.Show();
				Factory.Save();

				// Act
				var calculateCO2EmissionMenuItem = FindCalculateCO2EmissionMenu(form);
				calculateCO2EmissionMenuItem.PerformClick();

				// Assert
				AssertEquals("Booking CO2eStatus should be set", CO2eStatusList.Codes.Current, quotedBooking.GetCO2eStatus());
				AssertEquals("Sailing CO2eStatus should be set", CO2eStatusList.Codes.Current, quotedBooking.Booking.Sailing.GetCO2eStatus());
				AssertEquals("TotalCO2eForBinding should be set", "9.959", quotedBooking.TotalCO2eForBinding);
				AssertEquals("Sailing CO2eStatus should be set", 9959.2m, quotedBooking.Booking.Sailing.GetCO2ePerTonneInKg());
				AssertEquals("TotalCO2eForSorting should be set", 9.9592m, quotedBooking.TotalCO2eForSorting);
			}
		}

		JobSailing CreateSailing(string load, string discharge)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "23";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(4);

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		IApiResponse<EmissionResult> GenerateEmissionResponse(string path, string mediaType = "application/xml")
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var content = new StringContent(resourceRetriever.GetString(path));
				content.Headers.ContentType.MediaType = mediaType;
				var serializer = new EmissionSerializer();
				var result = serializer.FromHttpContentAsync<EmissionResult>(content).GetAwaiter().GetResult();
				var responseMock = new Mock<IApiResponse<EmissionResult>>();
				responseMock.SetupGet(x => x.Content).Returns(result);
				return responseMock.Object;
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_ClearValidation()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			var ooq = quote.CurrentOneOffQuote;
			ooq.TT_TransportMode = Constants.TransportModes.Air;
			ooq.TT_ContainerMode = Constants.ContainerModes.Loose;
			ooq.TT_RL_NKReceivalLocation = "AUSYD";
			ooq.TT_RL_NKDeliveryLocation = "USLAX";
			quotedBooking.SetCO2ePerTonneInKg(5m);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			quotedBooking.Weight = 1;
			quotedBooking.WeightUnit = "T";

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

				var calculateCO2EmissionMenuItem = FindCalculateCO2EmissionMenu(form);
				calculateCO2EmissionMenuItem.PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});

				AssertEquals("CO2eStatus should be set", CO2eStatusList.Codes.Pending, quotedBooking.GetCO2eStatus());
				AssertEquals("TotalCO2eForBinding should be set", "Pending", quotedBooking.TotalCO2eForBinding);
				AssertNoWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenMandatoryDataMissing()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			AsertShowErrorMessage_WhenMandatoryDataMissing(quotedBooking, "One Off Quote", isQuote: true);

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking2 = QuotedBooking.New(quote.PK, booking.PK, Factory);
			AsertShowErrorMessage_WhenMandatoryDataMissing(quotedBooking2, "Booking with Quote");

			var quotedBooking3 = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			AsertShowErrorMessage_WhenMandatoryDataMissing(quotedBooking3, "Quick Booking");
		}

		void AsertShowErrorMessage_WhenMandatoryDataMissing(QuotedBooking quotedBookingObject, string businessObjectName, bool isQuote = false)
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new QuotedBookingFormForTest(quotedBookingObject))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();

				var calculateCO2EmissionMenuItem = FindCalculateCO2EmissionMenu(form);

				void TestMissingField(Action change, string text)
				{
					change.Invoke();
					Factory.Save();
					calculateCO2EmissionMenuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
						AssertContains("Check massege text", "The greenhouse gas emissions calculation cannot be requested because following mandatory input is missing or invalid:", lastMessage.Text);
						AssertContains("Check mandatory field", text, lastMessage.Text);
					});
				}

				TestMissingField(() => quotedBookingObject.TransportMode = ZString.Empty, $"{businessObjectName} > Transport Mode");
				TestMissingField(() => quotedBookingObject.Origin = ZString.Empty, $"{businessObjectName} > Origin");
				TestMissingField(() => quotedBookingObject.Destination = ZString.Empty, $"{businessObjectName} > Destination");
				TestMissingField(() => quotedBookingObject.Weight = 0, $"{businessObjectName} > Goods Details > Weight");
			}
		}

		public void TestCalculateCO2EmissionMenuItem_SentSuccessfullyRequireTEU_Booking() => SentSuccessfullyUsingTEU_Booking(TransportModes.Sea);

		public void TestCalculateCO2EmissionMenuItem_SentSuccessfullyIncludeTEU_Booking() => SentSuccessfullyUsingTEU_Booking(TransportModes.Road);

		void SentSuccessfullyUsingTEU_Booking(string transportMode)
		{
			// Arrange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var containers = quotedBooking.QuotedBookingContainers;
			quotedBooking.TransportMode = transportMode;
			quotedBooking.ContainerMode = RateMode.FCL;
			quotedBooking.Weight = 0m;
			quotedBooking.WeightUnit = Weight.Tonnes;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "CNSHA";

			containers.RemoveAll();
			var container1 = containers.AddNew();
			container1.JC_ContainerCount = 2;

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();

				// Act
				FindCalculateCO2EmissionMenu(form).PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2EmissionMenuItem_SentSuccessfullyWhenNoWeightAndRequireTEU_OOQ()
		{
			// Arrange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var ooq = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			Factory.Save();
			ooq.Mode = RateMode.FCL;
			ooq.Weight = 0;
			ooq.WeightUnit = Weight.Kilograms;
			var containers = ooq.Quote.CurrentOneOffQuote.Containers;
			ooq.TransportMode = TransportModes.Sea;
			ooq.ContainerMode = RateMode.FCL;
			ooq.Origin = "AUSYD";
			ooq.Destination = "CNSHA";

			containers.RemoveAll();
			var container1 = containers.AddNew();
			container1.TC_ContainerCount = 2;
			container1.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new QuotedBookingFormForTest(ooq))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();

				// Act
				FindCalculateCO2EmissionMenu(form).PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenQuoteNotSaved()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			var ooq = quote.CurrentOneOffQuote;

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				FindCalculateCO2EmissionMenu(form).PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
					AssertEquals("Check message text", "Please save before calculating greenhouse gas emissions.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorIfExceptionIsThrown()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var ooq = quote.CurrentOneOffQuote;
			ooq.TT_TransportMode = Constants.TransportModes.Air;
			ooq.TT_ContainerMode = Constants.ContainerModes.Loose;
			ooq.TT_RL_NKReceivalLocation = "AUSYD";
			ooq.TT_RL_NKDeliveryLocation = "USLAX";
			ooq.TT_ActualWeight = 1;
			ooq.TT_UnitOfWeight = "T";

			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
				{
					if (factory.NameForDebugging == "CO2e Calculation Request Sender")
					{
						throw new Exception("Houston, we have a problem");
					}
				});

				AssertNoExceptionThrown(() => FindCalculateCO2EmissionMenu(form).PerformClick());
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Check message text", "Sending greenhouse gas emissions calculation request failed due to an error. Please try again.", lastMessage.Text);
			}
		}

		MenuItem FindCalculateCO2EmissionMenu(QuotedBookingFormForTest form)
		{
			var menuItems = form.Menu.MenuItems.FindByText("Actions").MenuItems;
			var calculateCO2EmissionMenuItem = menuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
			return calculateCO2EmissionMenuItem;
		}

		#endregion

		#region TestTransportBookingMenuItem_WhenQuotedBookingIsConvertedToShipment

		public void TestTransportBookingMenuItem_WhenQuotedBookingIsConvertedToShipment()
		{
			var booking = GetBooking();
			booking.Booking.JS_IsForwardRegistered = true;
			booking.Factory.Save();

			using (var form = new QuotedBookingFormForTest(booking))
			{
				form.Show();
				Application.DoEvents();

				var actionMenuItems = form.Menu.MenuItems.FindByName("ActionsMenuItem");
				actionMenuItems.ShowPopupMenu();

				var transportBookingMenuItem = actionMenuItems.MenuItems.FindByName("TransportBooking");
				AssertEquals(true, transportBookingMenuItem.Enabled);
				transportBookingMenuItem.ShowPopupMenu();

				AssertEquals("Menu should have one item", 1, transportBookingMenuItem.MenuItems.Count);
				AssertEquals("Menu should have the first item displaying No Actions Available", "No Actions Available", transportBookingMenuItem.MenuItems[0].Text);
			}
		}

		#endregion

		#region eConversations

		public void TestEConversationsPlugIn_Visible()
		{
			AssertEConversationPlugInVisibility(true);
		}

		public void TestEConversationsPlugIn_Hidden()
		{
			AssertEConversationPlugInVisibility(false);
		}

		void AssertEConversationPlugInVisibility(bool registryValue)
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			Factory.Save();

			using (Registry.Business.GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				var eConverationPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);

				if (registryValue)
				{
					AssertNotNull(eConverationPlugIn);
				}
				else
				{
					AssertNull(eConverationPlugIn);
				}
			}
		}

		public void TestEConversationsPlugIn_RequiresBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quickBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			Factory.Save();

			using (Registry.Business.GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTest(quickBooking))
			{
				AssertNull("OneOffQuotes should not have eConversations", form.PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn));
			}
		}

		#endregion

		#region ICancellable

		public void TestMessageWhenReactivatingNotAllowed()
		{
			var quotedBooking = GetBooking();
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			((ICancellable)quotedBooking).IsCancelled = true;
			quotedBooking.Factory.Save();

			var cannotReactivateMessage = "A booking created via a Booking Request EDI Message cannot be re-activated.";

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Make Active");
				menuItem.PerformClick();

				AssertEquals(cannotReactivateMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestSetBookingPartyDocumentaryAddressReadonly

		public void TestSetBookingPartyDocumentaryAddressReadonly_NewQuickBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				Assert("BookingPartyDocumentaryAddress is not readonly for new QuickBooking",
					!quotedBooking.Booking.BookingPartyDocumentaryAddress.ReadOnly);
			}
		}

		public void TestSetBookingPartyDocumentaryAddressReadonly_NewBookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				Assert("BookingPartyDocumentaryAddress is not readonly for new BookingWithQuote",
					!quotedBooking.Booking.BookingPartyDocumentaryAddress.ReadOnly);
			}
		}

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingQuickBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				Assert("BookingPartyDocumentaryAddress is not readonly for saved QuickBooking which is not a part of NVOCC electroning message exchange",
					!quotedBooking.Booking.BookingPartyDocumentaryAddress.ReadOnly);
			}
		}

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingBookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				Assert("BookingPartyDocumentaryAddress is not readonly for saved BookingWithQuote which is not a part of NVOCC electroning message exchange",
					!quotedBooking.Booking.BookingPartyDocumentaryAddress.ReadOnly);
			}
		}

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingBookingWithQuoteWithSTULog_ElectronicShippingInstruction() =>
			AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSBookingWithQuoteWithSTULog(true, new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.ElectronicShippingInstruction));

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingBookingWithQuoteWithSTULog_WebBooking() =>
			AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSBookingWithQuoteWithSTULog(true, new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.WebBooking));

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingBookingWithQuoteWithSTULog_ElectronicBooking() =>
			AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSBookingWithQuoteWithSTULog(true, new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.ElectronicBooking));

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingBookingWithQuoteWithSTULog_NonNVOCCLog() =>
			AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSBookingWithQuoteWithSTULog(false, new KeyValuePair<string, string>("ZZZ", "hello"));

		void AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSBookingWithQuoteWithSTULog(bool expectedBookingPartyAddressReadOnly, params KeyValuePair<string, string>[] stuLogParameters)
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.TransportMode = Core.Constants.TransportModes.Sea;
			quotedBooking.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var log = quotedBooking.Booking.Logs.AddNew(Events.StatusUpdated, stuLogParameters);
			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();

				AssertEquals($"Booking BookingPartyDocumentaryAddress readonly for STU {log.SL_Reference}",
					expectedBookingPartyAddressReadOnly,
					quotedBooking.Booking.BookingPartyDocumentaryAddress.ReadOnly);

				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				menuItem.PerformClick();
				AssertNotNull("Shipment Form should show", form.PopupForm_ForTesting);

				var convertedShipment = form.PopupForm_ForTesting.BusinessEntity as ForwardingShipment;
				var bookingPartyDocumentaryAddress =
					convertedShipment.DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
				AssertNotNull(bookingPartyDocumentaryAddress);
				AssertEquals($"Converted Shipment BookingPartyDocumentaryAddress readonly for STU {log.SL_Reference}",
					expectedBookingPartyAddressReadOnly,
					bookingPartyDocumentaryAddress.ReadOnly);

				form.PopupForm_ForTesting.Close();
			}
		}

		#endregion

		#region ComplianceRiskPlugin

		public void TestIncidentDefaultModuleOnComplianceRiskTab()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);

				((ZTemplateTabControl)(typeof(QuotedBookingForm).GetField("MainTabControl", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form))).SelectTab("ComplianceRiskTabPage");
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);
			}
		}

		public void TestComplianceRiskPlugin_Visible()
		{
			AssertComplianceRiskPluginVisibility(false);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				AssertComplianceRiskPluginVisibility(true);
			}

			void AssertComplianceRiskPluginVisibility(bool registryValue)
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				using (var form = new QuotedBookingForm(quotedBooking))
				{
					var complianceRiskPlugin = form.PlugIns.GetPlugIn(ControllerIDs.ComplianceRiskPlugin);
					if (registryValue)
					{
						AssertNotNull(complianceRiskPlugin);
					}
					else
					{
						AssertNull(complianceRiskPlugin);
					}
				}
			}
		}

		#endregion

		#region TestSelectRateCommodity_Click

		[ExpectNoExceptions]
		public void TestSelectRateCommodity_ClickDoesnotThrowExceptionWithOneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var notRegistryCommodityQuery = new ZQuery(RefCommodityCodeSchema.PK, SQLComparisonOperator.NotEqual, Env.Registry.CommodityCode);
			var commodity = Factory.LoadTop1<RefCommodityCode>(notRegistryCommodityQuery);
			quotedBooking.Commodity = commodity.RH_Code;

			quotedBooking.Factory.Save();
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				var menuItems = form.Menu.MenuItems.FindByText("Actions").MenuItems;
				var selectRateCommodityAction = menuItems.FindByText("Selection of Rate Commodity (with FMC Tariff ID)");
				selectRateCommodityAction.PerformClick();
				Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		public void TestSelectRateCommodity_ClickDoesnotThrowExceptionWithQuotedBooking()
		{
			var qb = GetQuotedBooking();
			var notRegistryCommodityQuery = new ZQuery(RefCommodityCodeSchema.PK, SQLComparisonOperator.NotEqual, Env.Registry.CommodityCode);
			var commodity = Factory.LoadTop1<RefCommodityCode>(notRegistryCommodityQuery);
			qb.Commodity = commodity.RH_Code;

			qb.Factory.Save();
			using (var form = new QuotedBookingFormForTest(qb))
			{
				form.Show();
				Application.DoEvents();

				var menuItems = form.Menu.MenuItems.FindByText("Actions").MenuItems;
				var selectRateCommodityAction = menuItems.FindByText("Selection of Rate Commodity (with FMC Tariff ID)");
				selectRateCommodityAction.PerformClick();
				Application.DoEvents();
			}
		}

		#endregion

		#region Implementation

		SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		IEnumerable<StmALog> GetSTULogs(QuotedBooking quotedBooking) => quotedBooking.Logs.Find(log => log.SL_SE_NKEvent == Events.StatusUpdatedCode);

		void AssertDeniedPartyScreeningStatusWithSecurityRightAndFreightMovementRestrictionsConditionsValid(bool hasSecurityRight, string expectedMessage, string screenStatus, string dpsFreightMovementRestrictedRegistry, bool expectedIsExported = true, string destination = "CNSHA", DialogResult dialogResult = DialogResult.Yes)
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = destination;
			AssertEquals(expectedIsExported, booking.IsExport());

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			((IScreeningPartyProvider)quotedBooking.Booking).ScreeningStatus = screenStatus;
			Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr.IsAllowed = hasSecurityRight;
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dpsFreightMovementRestrictedRegistry))
			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Convert to Shipment");
				UnitTestUserNotification.Instance.AddAnswer(dialogResult);
				menuItem.PerformClick();

				var filter = (form.PopupForm_ForTesting?.BusinessEntity as ForwardingShipment)?.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdated.Code));
				if (hasSecurityRight && IsDPSFreightMovementRestricted(expectedIsExported, dpsFreightMovementRestrictedRegistry) && screenStatus != ScreeningStatusesList.Codes.Clear && dialogResult == DialogResult.Yes)
				{
					AssertEquals(1, filter.Length);
				}
				else if (!IsDPSFreightMovementRestricted(expectedIsExported, dpsFreightMovementRestrictedRegistry) || screenStatus == ScreeningStatusesList.Codes.Clear || screenStatus == ScreeningStatusesList.Codes.JobCleared)
				{
					AssertEquals(0, filter.Length);
				}
				else
				{
					AssertNull(form.PopupForm_ForTesting);
				}
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		bool IsDPSFreightMovementRestricted(bool expectedIsExported, string dpsFreightMovementRestricted_Registry)
		{
			bool result;

			switch (dpsFreightMovementRestricted_Registry)
			{
				case DPSFreightMovementRestrictionsOptions.Codes.All:
					result = true;
					break;
				case DPSFreightMovementRestrictionsOptions.Codes.No:
					result = false;
					break;
				default:
					result = expectedIsExported;
					break;
			}

			return result;
		}

		OrgHeader SetupOrgProxy()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DXZENTBNE";
			org.OH_FullName = "DXZ Enterprises";
			org.OH_RL_NKClosestPort = "AUBNE";
			org.OH_IsForwarder = true;
			org.MainAddress.OA_Address1 = "1 Street St";

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "DXZ";
			newCompany.GC_Name = "DXZ Enterprises";
			newCompany.GC_RN_NKCountryCode = "AU";
			newCompany.GC_RX_NKLocalCurrency = "AUD";
			newCompany.GC_OH_OrgProxy = org.PK;

			return org;
		}

		QuotedBooking GetSavedQuote()
		{
			QuotedBooking result = GetQuote();

			Factory.Save();

			return result;
		}

		QuotedBooking GetQuote()
		{
			ZGuid quoteOnlyPK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK;
			QuotedBooking result = QuotedBooking.New(quoteOnlyPK, ZGuid.Empty, Factory);
			result.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.Mode = Core.Constants.RateMode.FCL;
			result.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			result.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, result.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			result.TryLoadOrCreateJob();
			result.Job.JH_GE = Env.CurrentDepartment.PK;
			return result;
		}

		QuotedBooking GetSavedBooking()
		{
			QuotedBooking result = GetBooking();

			Factory.Save();

			return result;
		}

		QuotedBooking GetBooking()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);

			QuotedBooking result = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			result.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.Mode = Core.Constants.RateMode.FCL;
			result.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			result.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, result.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			result.TryLoadOrCreateJob();
			result.Job.JH_GE = Env.CurrentDepartment.PK;

			return result;
		}

		QuotedBooking GetSavedQuotedBooking()
		{
			QuotedBooking result = GetQuotedBooking();

			Factory.Save();

			return result;
		}

		QuotedBooking GetQuotedBooking(bool withLocalConsignorAndOverseasConsignee = false)
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);

			QuotedBooking result = QuotedBooking.New(quote.PK, booking.PK, Factory);
			result.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.Mode = Core.Constants.RateMode.FCL;
			if (withLocalConsignorAndOverseasConsignee)
			{
				result.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, LocalConsignor.PK)).PK;
				result.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, OverseasConsignee.PK)).PK;
			}
			else
			{
				result.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				result.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, result.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			}
			result.TryLoadOrCreateJob();
			result.Job.JH_GE = Env.CurrentDepartment.PK;

			return result;
		}

		QuotedBooking GetNewQuoteBookingWithShipment(string transportMode)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR" + transportMode;
			consignor.OH_IsConsignor = true;
			consignor.OH_IsDebtor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE" + transportMode;
			consignee.OH_IsConsignee = true;

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			booking.JS_ActualWeight = 200m;
			booking.JS_ActualVolume = 1m;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.ClientPK = consignor.PK;
			quotedBooking.Mode = transportMode;
			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = department.PK;

			return quotedBooking;
		}

		public class QuotedBookingFormForTest : QuotedBookingForm, IDisposable
		{
			public QuotedBookingFormForTest(QuotedBooking quotedBooking)
				: base(quotedBooking)
			{
			}

			public void ConsolidateToNewConsol()
			{
				MethodInfo method = typeof(QuotedBookingForm).GetMethod("ConsolidateToNewConsol", BindingFlags.Instance | BindingFlags.NonPublic);
				method.Invoke(this, Array.Empty<object>());
			}

			public void ShowPreAllocation()
			{
				MethodInfo method = typeof(QuotedBookingForm).GetMethod("ShowPreAllocation", BindingFlags.Instance | BindingFlags.NonPublic);
				method.Invoke(this, Array.Empty<object>());
			}

			public ZStmNoteTabPage NotesTabPage_Exposed
			{
				get
				{
					return NotesTabPage;
				}
			}

			public void SelectNotesTab()
			{
				ZStmNoteTabPage noteTabPage = TopLevelTabControl.TabPages
					.Cast<ZTabPage>()
					.OfType<ZStmNoteTabPage>()
					.FirstOrDefault()
						?? throw new Exception("Notes tab page not found");

				TopLevelTabControl.SelectTab(noteTabPage);
			}

			public void UpdateQuoteLockedReadOnlyState_Exposed()
			{
				UpdateQuoteLockedReadOnlyState();
			}

			public string PromptToSelectSingleCarrierReturn { get; set; }

			internal override string PromptToSelectSingleCarrier(IEnumerable<string> carrierCodes)
			{
				PromptToSelectSingleCarrierCodes = carrierCodes.ToArray();
				return PromptToSelectSingleCarrierReturn;
			}

			internal string[] PromptToSelectSingleCarrierCodes { get; private set; }

			internal ZTemplateTabControl MainTabControlExposed => MainTabControl;

			void IDisposable.Dispose()
			{
				base.Dispose();

				if (PopupForm_ForTesting != null)
				{
					PopupForm_ForTesting.Dispose();
				}
			}

			public ZTabPage LogsTabPageForTest => LogsTabPage;
		}

		string[] GetTabNames(QuotedBookingForm form)
		{
			return ((IFormPlugInsProvider)form).TopLevelTabControl.TabPages.Cast<ZTabPage>().Select((tabPage) => tabPage.Name).ToArray();
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}

		#endregion

		public class QuoteTabControlTest : QuotedBookingFormTest
		{
			public void TestZStmALogFilterControl_ShowForAll()
			{
				var quotedBooking = GetSavedQuote();
				using (var module = new ZStmALogModule())
				{
					module.InitData(quotedBooking.Quote);
					var control = module.EmbeddedControl as ZStmALogFilterControl;
					(control.FilterBusinessObject as ZStmALogFilterBusinessObject).LogsToShow = LogsToShow.All;
					module.PerformSearch_ForTest();

					var collection = (control.GridCollection as StmALogCollection);
					Assert("should contain logs belong to RateOneOffShipment", collection.Cast<StmALog>().Any(x => x.SL_Parent == quotedBooking.Quote.CurrentOneOffQuote.PK));
				}
			}

			public void TestZStmALogFilterControl_IncludesInvoicingJob()
			{
				var quotedBooking = GetSavedQuote();
				var jobEvent = quotedBooking.Job.Logs.AddNew(Events.Arrival);
				Factory.Save();
				using (var module = new ZStmALogModule())
				{
					module.InitData(quotedBooking);
					var control = module.EmbeddedControl as ZStmALogFilterControl;
					control.Show();
					(control.FilterBusinessObject as ZStmALogFilterBusinessObject).LogsToShow = LogsToShow.All;
					module.PerformSearch_ForTest();

					var collection = (control.GridCollection as StmALogCollection);
					Assert("should contain logs belong to Invoicing Job", collection.Cast<StmALog>().Any(x => x.PK == jobEvent.PK));
					Assert("should contain logs belong to Invoicing Job", collection.Cast<StmALog>().Any(x => x.SL_Parent == quotedBooking.Job.PK));
				}
			}
		}
	}
}
