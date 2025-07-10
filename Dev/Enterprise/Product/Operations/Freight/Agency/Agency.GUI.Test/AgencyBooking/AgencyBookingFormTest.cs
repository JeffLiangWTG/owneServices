using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class AgencyBookingFormTest : BaseAgencyTest
	{
		public void TestDtbTransportBooking()
		{
			var shipment = Factory.New<AgencyBooking>();
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}
		}

		public void TestGuiFactoryServices()
		{
			var shipment = Factory.New<AgencyBooking>();
			ICommonShipmentDocumentSupporterQueryProvider shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
			AssertNull("shipment doc supporter", shipmentDocSupporterQueryProvider);
			IServicesSelectionProvider servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("services selection provider", servicesSelectionProvider);
			using (new AgencyBookingForm(shipment))
			{
				shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull("shipment doc supporter", shipmentDocSupporterQueryProvider);
				Assert(shipmentDocSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);
				servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("services selection provider", servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		public void TestContainerReleaseWizard()
		{
			var shipment = Factory.New<AgencyBooking>();
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				MenuAssertion.AssertHasMenu(form.Menu, "Actio&ns", "Container Release Wizard");
				MenuAssertion.AssertHasMenu(form.Menu, "Actio&ns", "Container Release Replacement Wizard");
			}
		}

		public void TestShowPreSaveDialogs()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ZDateTime now = ZDateTime.Now;
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "PPPNNNLLL";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();
			OrgHeader bookingParty = Factory.New<OrgHeader>();
			bookingParty.OH_Code = "BBBKKKPPP";
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = "HKHKC";
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.JS_GoodsDescription = "description";
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
				voyage.JV_VoyageFlight = "x42";
				voyage.JV_OH_Line = NewCarrier().PK;
				VoyageOrigin origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = now.AddDays(1);
				VoyageDestination destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "HKHKC";
				destination.JB_E_ARV = now.AddDays(2);
				JobSailing sailing = voyage.Sailings[0];
				shipment.JS_JX = sailing.PK;
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBookingPackingModeChanging()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			booking.RealContainers.AddNew();
			using (var form = new AgencyBookingForm(booking))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				booking.JS_PackingMode = Constants.ContainerModes.BreakBulk;
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Are you sure you want to continue?"));
				AssertEquals("User cancelled mode change", Constants.ContainerModes.FCL, booking.JS_PackingMode);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				booking.JS_PackingMode = Constants.ContainerModes.BreakBulk;
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Are you sure you want to continue?"));
				AssertEquals("User confirmed mode change", Constants.ContainerModes.BreakBulk, booking.JS_PackingMode);
			}
		}

		public void TestUpdateShipmentTotals()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			String messageString = "Total weight and volume do not match the booking total. Would you like to update the booking to match the totals?";
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "PPPPPPPPP";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();
			AgencyBooking booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyBookingContainer container = booking.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 2;
			AgencyBookingPackLine packLine = booking.OuterPackLines.AddNew();
			packLine.JL_Width = 1.2;
			packLine.JL_Height = 1.2;
			packLine.JL_Length = 1.2;
			packLine.JL_ActualVolume = 1.728;
			packLine.JL_PackageCount = 3;
			OrgHeader bookingParty = Factory.New<OrgHeader>();
			bookingParty.OH_Code = "BBBBBBBBB";
			booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			booking.JS_OH_DeliveryAgent = principal.PK;
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "AUBNE";
			booking.JS_GoodsDescription = "description";
			using (AgencyBookingForm form = new AgencyBookingForm(booking))
			{
				form.Show();
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				booking.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
				var vehicle = booking.ShippingContainers.AddNew();
				vehicle.JC_GrossWeight = 10m;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals(messageString, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionsMenuNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new AgencyBookingForm(Factory.New<AgencyBooking>()));
		}

		public void TestValidateAndSave_DeletedShipment()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AgencyBooking shipment = GetShipmentWithoutErrors(null);
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				form.Dispose();
				form.FireSaveButton();
				Assert("This form was disposed, and so cannot be saved.", !form.LastSaveSucceeded);
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestConversionDontCauseConflictWithSelf()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			Factory.Save();
			using (AgencyBookingForm form = new AgencyBookingForm(booking))
			{
				form.Show();
				Application.DoEvents();
				BookingDetailsControl control = GetBookingDetailsControl(form);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ShowDialogsInTest = true;
				control.ConfirmPerformClick(); // showing second form for same shipment causes developer exception.

				form.PopupForm_ForTesting.Close();
			}
		}

		public void TestTheConfirmButton_HasChanges()
		{
			AgencyBooking shipment = (AgencyBooking)NewShipment(ExportSailing, null, false, false);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				BookingDetailsControl control = GetBookingDetailsControl(form);
				control.ConfirmPerformClick();
				AssertLastUserErrorMessage("You must save this booking first.");
				AssertDocumentationFormNotShown(form);
			}
		}

		public void TestTheConfirmButton_Cancel()
		{
			AgencyBooking shipment = (AgencyBooking)NewShipment(ExportSailing, null, false, false);
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				BookingDetailsControl control = GetBookingDetailsControl(form);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				control.ConfirmPerformClick();
				AssertLastUserQuestion("Are you sure you want to confirm this booking?");
				AssertDocumentationFormNotShown(form);
			}
		}

		public void TestTheConfirmButton_Ok()
		{
			AgencyBooking shipment = (AgencyBooking)NewShipment(ExportSailing, null, false, false);
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				BookingDetailsControl control = GetBookingDetailsControl(form);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				control.ConfirmPerformClick();
				AssertContainUserQuestion("Are you sure you want to confirm this booking?");
				AssertDocumentationFormShown(form);
				AssertEquals("Should not have changed the original Shipment", false, shipment.IsBillOfLadingStage);
			}
		}

		public void TestTheConfirmButton_JS_ShipmentStatusIsNotBooked()
		{
			var shipment = (AgencyBooking)NewShipment(ExportSailing, null, false, false);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			Factory.Save();

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var form = new AgencyBookingForm(shipment))
				{
					form.Show();
					var control = GetBookingDetailsControl(form);
					control.ConfirmPerformClick();

					AssertNotEquals("The Booking is not confirmed yet.  Please confirm the booking by changing the Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new AgencyBookingForm(shipment))
				{
					form.Show();
					var control = GetBookingDetailsControl(form);
					control.ConfirmPerformClick();

					AssertEquals("The Booking is not confirmed yet.  Please confirm the booking by changing the Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestTheConfirmButton_ClosesBookingFormBeforeOpeningBOLForm()
		{
			var shipment = (AgencyBooking)NewShipment(ExportSailing, null, false, false);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using var bookingForm = new AgencyBookingForm(shipment);
			bookingForm.FormClosed += (s, e) =>
			{
				AssertNull("Bill of Lading Form", bookingForm.PopupForm_ForTesting);
			};

			bookingForm.Show();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			GetBookingDetailsControl(bookingForm).ConfirmPerformClick();

			using var bolForm = bookingForm.PopupForm_ForTesting;
			AssertNotNull("Bill of Lading Form", bolForm);
		}

		public void TestAddingCusEntryNumberAfterConfirmDoNotThrowException()
		{
			var shipment = (AgencyBooking)NewShipment(ExportSailing, null, false, false);
			var cusEntryNumber1 = shipment.Numbers.AddNew();
			cusEntryNumber1.CE_EntryType = "COC";
			cusEntryNumber1.CE_EntryNum = "1998";
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var controller = ZControllerFactory.Create(ControllerIDs.AgencyBooking);
			using (var form = (AgencyBookingForm)controller.ShowEditForm(shipment))
			{
				form.Show();
				var booking = (AgencyBooking)form.BusinessEntity;
				AssertEquals("Pre-condition", 1, booking.Numbers.Count);
				var control = GetBookingDetailsControl(form);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				control.ConfirmPerformClick();
				using (var billForm = form.PopupForm_ForTesting as BillOfLadingForm)
				{
					var bill = (BillOfLading)billForm.BusinessEntity;
					AssertEquals("Should have confirmed the BillOfLading", true, bill.IsBillOfLadingStage);
					AssertEquals("The booking form should be closed", false, form.Visible);
					bill.Factory.Save();

					var newFactory = new BusinessObjectFactory();
					var billInNewFactory = newFactory.Load<BillOfLading>(bill.PK);

					using (var billFormInNewFactory = new BillOfLadingForm(billInNewFactory))
					{
						billInNewFactory.JS_BookingReference = "ABC";
						var cusEntryNumber2 = billInNewFactory.Numbers.AddNew();
						cusEntryNumber2.CE_EntryType = "CON";
						cusEntryNumber2.CE_EntryNum = "2006";
						AssertNoExceptionThrown(() => billInNewFactory.Factory.Save());
					}
				}
			}
		}

		public void TestDataRefreshManageShouldBeDisabledForBookedContainersAndRealContainersAfterConfirmAgencyBooking()
		{
			var shipment = (AgencyBooking)NewShipment(ExportSailing, null, false, false);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var controller = ZControllerFactory.Create(ControllerIDs.AgencyBooking);
			using (var form = (AgencyBookingForm)controller.ShowEditForm(shipment))
			{
				form.Show();
				var booking = (AgencyBooking)form.BusinessEntity;

				var control = GetBookingDetailsControl(form);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				control.ConfirmPerformClick();

				AssertEquals(false, booking.BookedContainers.IsManagedForDataRefresh);
				AssertEquals(false, booking.RealContainers.IsManagedForDataRefresh);

				using (var billForm = form.PopupForm_ForTesting as BillOfLadingForm)
				{
					var bill = (BillOfLading)billForm.BusinessEntity;
					AssertEquals("Confirmed Bill Of Lading", bill.Factory.NameForDebugging);
				}
			}
		}

		public void TestConfirmedAgencyBookingFactoryShouldNotSave()
		{
			var shipment = (AgencyBooking)NewShipment(ExportSailing, null, false, false);
			var cusEntryNumber1 = shipment.Numbers.AddNew();
			cusEntryNumber1.CE_EntryType = "COC";
			cusEntryNumber1.CE_EntryNum = "1998";
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var controller = ZControllerFactory.Create(ControllerIDs.AgencyBooking);
			using (var form = (AgencyBookingForm)controller.ShowEditForm(shipment))
			{
				form.Show();
				var booking = (AgencyBooking)form.BusinessEntity;
				AssertEquals("Pre-condition", 1, booking.Numbers.Count);

				var control = GetBookingDetailsControl(form);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				control.ConfirmPerformClick();

				AssertEquals(false, booking.Factory.RefreshEnabled);
				AssertEquals("Confirmed Agency Booking", booking.Factory.NameForDebugging);
				AssertEquals(false, ((IBusinessObjectFactoryInternals)booking.Factory).CanSave);

				using (var billForm = form.PopupForm_ForTesting as BillOfLadingForm)
				{
					var bill = (BillOfLading)billForm.BusinessEntity;
					AssertEquals("Confirmed Bill Of Lading", bill.Factory.NameForDebugging);
				}
			}
		}

		public void TestSaving_BSRelationshipExists()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ExportSailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignee = true;
			AgencyBooking shipment = GetShipmentWithoutErrors(ExportSailing1);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			OrgSupplierBuyerLink link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_RN_NKImporterCountry = shipment.JS_RL_NKDestination.Left(2);
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Save should have succedded", false, shipment.HasChanges);
			}
		}

		public void TestSaving_AddBSRelationship()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ExportSailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignee = true;
			AgencyBooking shipment = GetShipmentWithoutErrors(ExportSailing1);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			AssertNull("precondition: link should not exist yet", GetBSRelationshipLink(consignor, consignee));
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals("Question Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Save should have succedded", false, shipment.HasChanges);
				OrgSupplierBuyerLink link = GetBSRelationshipLink(shipment.Consignor, shipment.Consignee);
				AssertNotNull("Should have created a link.", link);
				AssertEquals("Link should be saved.", true, link.IsInDatabase);
			}
		}

		public void TestSaving_DontAddBSRelationship()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ExportSailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignee = true;
			AgencyBooking shipment = GetShipmentWithoutErrors(ExportSailing1);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			AssertNull("precondition: link should not exist yet", GetBSRelationshipLink(consignor, consignee));
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();
				AssertEquals("Question Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Save should have succedded", false, shipment.HasChanges);
				OrgSupplierBuyerLink link = GetBSRelationshipLink(shipment.Consignor, shipment.Consignee);
				AssertNull("Should have created a link.", link);
			}
		}

		public void TestSaving_NoVoyage()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AgencyBooking shipment = GetShipmentWithoutErrors(null);
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("Save should have succedded", false, shipment.HasChanges);
				AssertEquals("Should not have displayed a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSaving_VoyageNotLocked()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ExportSailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			SlotAllocation allocation = ExportSailing.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 20);
			AgencyBooking shipment = GetShipmentWithoutErrors(ExportSailing);
			AssertEquals("Precondition: Shipment should be an export", true, shipment.IsExport());
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("Save should have succedded", false, shipment.HasChanges);
				AssertEquals("Should not have displayed a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSaving_VoyageLocked()
		{
			ExportSailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			ExportSailing.SlotAllocations.GetAllocation(ZGuid.Empty).SetAspect(AllocationAspectTypes.Tonnes, 20);
			var shipment = GetShipmentWithoutErrors(ExportSailing);
			AssertEquals("Precondition: Shipment should be an export", true, shipment.IsExport());
			var mutex = new AgencyAllocationMutex(ExportSailing.Voyage);
			mutex.Lock();
			Assert("Precondition: the test needs to be holding the mutex", mutex.HasLock);
			try
			{
				using (var form = new AgencyBookingForm(shipment))
				{
					form.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					form.FireSaveButton();
					var info = mutex.GetLockInfo();
					AssertEquals("Save should not have succedded", false, shipment.IsInDatabase);
					AssertEquals("Should have displayed a dialog", $"User {info.UserWithLock.GS_LoginName} has this voyage locked since {info.LockStartTime}. Do you want to release the existing lock?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FireSaveButton();
					AssertEquals("Save should not have succeeded", true, shipment.IsInDatabase);
				}
			}
			finally
			{
				mutex.Unlock();
			}
		}

		public void TestDefaultWeightAndVolumeUnits()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Registry.PromptToSaveBuyerSupplier = false;
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			ExportSailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			AgencyBooking shipment = GetShipmentWithoutErrors(ExportSailing);
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_UnitOfWeight = "";
			shipment.JS_UnitOfVolume = "";
			shipment.OuterPackLines.RemoveAndDeleteAll();
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				form.FireSaveButton();
				AssertEquals("Should not have shown a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);
				AssertEquals(Constants.Weight.Tonnes, shipment.JS_UnitOfWeight);
			}
		}

		public void TestSaving_EmptyAllocation()
		{
			ExportSailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			AgencyBooking shipment = GetShipmentWithoutErrors(ExportSailing);
			AssertEquals("precondition: shipment should be an export", true, shipment.IsExport());
			ExportSailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			SlotAllocation allocation = ExportSailing.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 0);
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.ControllerID = ControllerIDs.AgencyBooking;
				form.Show();
				ExposeAllTabPages(form);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("Save should not have succedded", false, shipment.IsInDatabase);
				AssertEquals("Should have displayed the errors dialog", "Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		void ExposeAllTabPages(Control control)
		{
			TabPage tabPage = control as TabPage;
			TabControl tabControl = control.Parent as TabControl;
			if (tabPage != null)
			{
				tabControl.SelectedTab = tabPage;
			}

			foreach (Control child in control.Controls)
			{
				ExposeAllTabPages(child);
			}
		}

		public void TestSaving_InsufficientAllocation()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ExportSailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			SlotAllocation allocation = ExportSailing.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 8);
			AgencyBooking shipment = GetShipmentWithoutErrors(ExportSailing);
			AssertEquals("precondition: shipment should be an export", true, shipment.IsExport());
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("Save should not have succedded", false, shipment.IsInDatabase);
				AssertEquals("Should have displayed the errors dialog", "None ", Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertNotNull("Should have displayed a form", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should have shown the correct form", typeof(AllocationAdjustmentDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSaving_WaitListed()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ExportSailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			SlotAllocation allocation = ExportSailing.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 8);
			AgencyBooking shipment = GetShipmentWithoutErrors(ExportSailing);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			AssertEquals("precondition: shipment should be an export", true, shipment.IsExport());
			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("Save should have succedded", true, shipment.IsInDatabase);
				AssertEquals("Should not have displayed the errors dialog", "None ", Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertNull("Should not have displayed a form", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#region UpdateShipmentStatus

		public void TestUpdateShipmentStatus_NotShow_RegistryIsFalse()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			Factory.Save();

			AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var form = new AgencyBookingForm(booking))
				{
					UnitTestUserNotification.Instance.AddUserResponse("No Reason");

					booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("Message Prompt was not fired", ZDialogResult.None, msg.Answer);
						AssertNullOrEmpty("Check message caption", msg.Caption);
						AssertNullOrEmpty("Check message text", msg.Text);
						AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, booking.JS_ShipmentStatus);
					});
				}
			}
		}

		public void TestUpdateShipmentStatus()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				using (var form = new AgencyBookingForm(booking))
				{
					UnitTestUserNotification.Instance.AddUserResponse("No Reason");

					booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("Message Prompt was fired", ZDialogResult.OK, msg.Answer);
						AssertEquals("Check message caption", "Rejection Reason", msg.Caption);
						AssertEquals("Check message text", "Please enter the reason of rejection.", msg.Text);
						AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, booking.JS_ShipmentStatus);
						AssertEquals("A status changed event should have been logged", "|NEW=BKJ|OLD=EBK|RES=Booking Rejected, No Reason|TYP=Shipment Status", booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
					});
				}
			}
		}

		public void TestUpdateShipmentStatus_WhenUserSelectsCancel()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
				var expectedLog = booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated);

				using (var form = new AgencyBookingForm(booking))
				{
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);

					booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("Message Prompt was fired", ZDialogResult.Cancel, msg.Answer);
						AssertEquals("Check message caption", "Rejection Reason", msg.Caption);
						AssertEquals("Check message text", "Please enter the reason of rejection.", msg.Text);
						AssertEquals("Status should have changed", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
						AssertEquals("No new Status changed event should have been logged", expectedLog, booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated));
					});
				}
			}
		}

		#endregion

		#region CancelRequestActionForm

		public void TestShouldNotShowCancelRequestActionForm()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				var controller = ZControllerFactory.Create(ControllerIDs.AgencyBooking);
				using (var form = (AgencyBookingForm)controller.ShowEditForm(booking))
				{
					AssertNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);

					form.FireSaveButton();
					AssertNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);
				}
			}
		}

		public void TestShowCancelRequestActionForm()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.AgencyBooking);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var cancelRequestActionForm = form as CancelRequestActionForm;
				if (cancelRequestActionForm != null)
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				}
			});

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = (AgencyBookingForm)controller.ShowEditForm(booking))
			{
				AssertNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);

				form.FireSaveButton();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = (AgencyBookingForm)controller.ShowEditForm(booking))
			{
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);

				form.FireSaveButton();
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest as CancelRequestActionForm);
			}
		}

		public void TestShowCancelRequestActionFormIfNecessary_AcceptClicked()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
				AssertCancelRequestActionForm(booking, DialogResult.Yes, string.Empty,
					(agencyBooking) =>
					{
						AssertEquals(3, agencyBooking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

						var log = agencyBooking.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
						CombineAssertions("ShipmentStatus should be changed", () =>
						{
							AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.BookingCancelled, agencyBooking.JS_ShipmentStatus);
							AssertNullOrEmpty("Check event free text", log.ReferenceFreeText);
							AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
							AssertEquals("Check event new status", "BKX", log.Parameters[Params.New]);
							AssertEquals("Check event old status", "EBC", log.Parameters[Params.Old]);
							AssertEquals("Check event reason", "Booking Cancelled by Booking Party", log.Parameters[Params.Reason]);
							Assert("ShipmentStatus should be readonly", agencyBooking.JS_ShipmentStatusInfo.ReadOnly);
						});
					});
			}
		}

		public void TestShowCancelRequestActionFormIfNecessary_RejectClicked()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
				Factory.Save();

				AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				var reason = "Ship already left!";
				AssertEquals(3, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;

				AssertCancelRequestActionForm(booking, DialogResult.No, reason,
					(agencyBooking) =>
					{
						AssertEquals(4, agencyBooking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

						var log = agencyBooking.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
						CombineAssertions("ShipmentStatus should be changed", () =>
						{
							AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.WaitListed, agencyBooking.JS_ShipmentStatus);
							AssertNullOrEmpty("Check event free text", log.ReferenceFreeText);
							AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
							AssertEquals("Check event new status", "WTL", log.Parameters[Params.New]);
							AssertEquals("Check event old status", "EBC", log.Parameters[Params.Old]);
							AssertEquals("Check event reason", "Booking Withdrawal/Cancellation Request Rejected, Ship already left!", log.Parameters[Params.Reason]);
						});

						AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.WaitListed, agencyBooking.JS_ShipmentStatus);
					});
			}
		}

		public void TestShowCancelRequestActionFormIfNecessary_CancelClicked()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
				AssertCancelRequestActionForm(booking, DialogResult.Cancel, string.Empty,
					(agencyBooking) =>
					{
						AssertEquals(2, agencyBooking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("ShipmentStatus should not be changed", ShipmentStatusList.Codes.EBookingCancellationRequest, agencyBooking.JS_ShipmentStatus);
					});
			}
		}

		void AssertCancelRequestActionForm(AgencyBooking booking, DialogResult resultToReturnFromShowDialog, string reasonOfRejection, Action<AgencyBooking> assertionAction)
		{
			Thread.Sleep(100);
			var controller = ZControllerFactory.Create(ControllerIDs.AgencyBooking);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var cancelRequestActionForm = form as CancelRequestActionForm;
				if (cancelRequestActionForm != null)
				{
					cancelRequestActionForm.Reason = reasonOfRejection;
					ZFormModaliser.ResultToReturnFromShowDialog = resultToReturnFromShowDialog;
				}
			});

			using (var form = (AgencyBookingForm)controller.ShowEditForm(booking))
			{
				assertionAction.Invoke(form.DataSource as AgencyBooking);
			}
		}

		#endregion

		#region Send Rejection Message When Setting Inactive

		public void TestSendRejectionMessageWhenSettingInactive_NotShow_RegistryIsFalse()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				using (var form = new AgencyBookingForm(booking))
				{
					var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
					var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
					var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

					Thread.Sleep(100);
					makeInactive.PerformClick();

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("Message Prompt was not fired", ZDialogResult.None, msg.Answer);
						AssertNullOrEmpty("Check message caption", msg.Caption);
						AssertNullOrEmpty("Check message text", msg.Text);
						AssertEquals("Status should not be changed", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
						AssertEquals("A status changed event should have been logged", "|NEW=EBK|RES=Electronic Booking Received|TYP=Shipment Status", booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
					});
				}
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenBookingStatusIsEBK()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				using (var form = new AgencyBookingForm(booking))
				{
					var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
					var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
					var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

					Thread.Sleep(100);
					makeInactive.PerformClick();

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("Message Prompt was fired", ZDialogResult.Yes, msg.Answer);
						AssertEquals("Check message caption", "Booking Rejection Message", msg.Caption);
						AssertEquals("Check message text", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", msg.Text);
						AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, booking.JS_ShipmentStatus);
						AssertEquals("A status changed event should have been logged", "|NEW=BKJ|OLD=EBK|RES=Booking Cancelled|TYP=Shipment Status", booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
					});
				}
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenBookingStatusIsNotEBK()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				using (var form = new AgencyBookingForm(booking))
				{
					var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
					var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
					var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

					Thread.Sleep(100);
					makeInactive.PerformClick();

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("Message Prompt wasn't fired", ZDialogResult.None, msg.Answer);
						AssertNotEquals("Should not display this message caption", "Booking Rejection Message", msg.Caption);
						AssertNotEquals("Should not display this message text", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", msg.Text);
						AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, booking.JS_ShipmentStatus);
						AssertEquals("A status changed event should have been logged", "|NEW=BKJ|OLD=WTL|RES=Booking Cancelled|TYP=Shipment Status", booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
					});
				}
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenUserSelectsNo()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				var expectedLog = booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated);

				using (var form = new AgencyBookingForm(booking))
				{
					booking.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;

					var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
					var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
					var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					Thread.Sleep(100);
					makeInactive.PerformClick();
					Factory.Save();

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("Message Prompt was fired", ZDialogResult.No, msg.Answer);
						AssertEquals("Check message caption", "Booking Rejection Message", msg.Caption);
						AssertEquals("Check message text", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", msg.Text);
						AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, booking.JS_ShipmentStatus);
						AssertEquals("No new status changed event should have been logged", expectedLog, booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated));
					});
				}
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenBookingIsAlreadyRejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				Factory.Save();

				var expectedLog = booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated);

				using (var form = new AgencyBookingForm(booking))
				{
					var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
					var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
					var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

					Thread.Sleep(100);
					makeInactive.PerformClick();

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals("Message Prompt wasn't fired", ZDialogResult.None, msg.Answer);
						AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, booking.JS_ShipmentStatus);
						AssertEquals("No new Status changed event should have been logged", expectedLog, booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated));
					});
				}
			}
		}

		public void TestSendRejectionMessageWhenSettingInactive_WhenBookingStatusHasEBKInHistory()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));

				using (var form = new AgencyBookingForm(booking))
				{
					booking.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;

					var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
					var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
					var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

					Thread.Sleep(100);
					makeInactive.PerformClick();
					Factory.Save();

					var msg = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals(3, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == "Shipment Status"));
						AssertEquals("Message Prompt was fired", ZDialogResult.Yes, msg.Answer);
						AssertEquals("Check message caption", "Booking Rejection Message", msg.Caption);
						AssertEquals("Check message text", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", msg.Text);
						AssertEquals("Status should have changed", ShipmentStatusList.Codes.BookingRejected, booking.JS_ShipmentStatus);
						AssertEquals("A status changed event should have been logged", "|NEW=BKJ|OLD=BKD|RES=Booking Cancelled|TYP=Shipment Status", booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
					});
				}
			}
		}

		#endregion

		#region Implementation

		OrgSupplierBuyerLink GetBSRelationshipLink(OrgHeader consignor, OrgHeader consignee)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, consignor.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, consignee.PK);
			return consignor.Factory.LoadTop1<OrgSupplierBuyerLink>(filter);
		}

		AgencyBooking GetShipmentWithoutErrors(JobSailing sailing)
		{
			OrgHeader bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();
			if (sailing != null && sailing.Voyage != null && sailing.Voyage.JV_OH_Line.IsEmpty)
			{
				sailing.Voyage.JV_OH_Line = NewCarrier().PK;
			}

			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			shipment.JS_OuterPacks = 1;
			shipment.JS_ActualWeight = 10;
			shipment.JS_JX = sailing == null ? ZGuid.Empty : sailing.PK;
			shipment.JS_GoodsDescription = "Blah";
			shipment.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: Should not have any errors. If there are errors then just modify the shipment to remove them.", shipment);
			return shipment;
		}

		BookingDetailsControl GetBookingDetailsControl(AgencyBookingForm form)
		{
			return (BookingDetailsControl)typeof(AgencyBookingForm).GetField("BookingDetails", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
		}

		void AssertDocumentationFormNotShown(ZForm bookingForm)
		{
			AssertNull("Should not have shown the BillOfLadingForm", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("The booking form should not be closed", true, bookingForm.Visible);
		}

		void AssertDocumentationFormShown(AgencyBookingForm bookingForm)
		{
			using (BillOfLadingForm billForm = (BillOfLadingForm)bookingForm.PopupForm_ForTesting)
			{
				AssertNotNull("Should have shown the BillOfLadingForm", billForm);
				AssertEquals(ControllerIDs.AgencyBillOfLading, billForm.ControllerID);
				AssertEquals("The BillOfLadingForm should have a display mode of Edit", ODisplayMode.Edit, billForm.DisplayMode);
				BillOfLading bill = (BillOfLading)billForm.BusinessEntity;
				AssertEquals("Should have confirmed the BillOfLading", true, bill.IsBillOfLadingStage);
				AssertEquals("Should have changes", true, bill.HasChanges);
				AssertEquals("The booking form should be closed", false, bookingForm.Visible);
			}
		}

		#endregion

		#region ComplianceRiskPlugin

		public void TestComplianceRiskPlugin()
		{
			AssertComplianceRiskPluginVisibility(true);
			AssertComplianceRiskPluginVisibility(false);

			void AssertComplianceRiskPluginVisibility(bool registryValue)
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
				var shipment = Factory.New<AgencyBooking>();

				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(registryValue)))
				using (LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(registryValue)))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				using (var form = new AgencyBookingForm(shipment))
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

		public void TestIncidentDefaultModuleOnComplianceRiskTab()
		{
			var shipment = Factory.New<AgencyBooking>();

			using (LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new AgencyBookingForm(shipment))
			{
				form.Show();
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);

				((ZTemplateTabControl)(typeof(AgencyBookingForm).GetField("MainTabControl", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form))).SelectTab("ComplianceRiskTabPage");
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);
			}
		}

		#endregion
	}
}
