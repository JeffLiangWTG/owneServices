using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportBookings.GUI.Testing
{
	[TestedType(typeof(TransportBookingMultiForm))]
	public class TransportBookingFormSingleJobConsolidationTest : TransportBookingMultiFormTest
	{
		public void TestCancellingAndReactivatingBookingWithParentJobDoesNotFail()
		{
			// need to make a parent invoicing Job to later ensure it is not deleted
			var shipment = Helper.CreateForwardingShipment("S001", "HSB1", "SEA", "FCL");

			var consignor = Helper.CreateOrganisation("CNR");
			var consignee = Helper.CreateOrganisation("CNE");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var billingOrg = Helper.CreateOrganisation("BILLC");
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = billingOrg.MainAddress.PK;

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CNR", consignor.MainAddress);
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, "CNE", consignee.MainAddress);

			var container = booking.PackageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT1");
			container.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK;
			Helper.CreateAndAssignPackageDivots(booking, container);

			Factory.Save();

			// open and save in single form to save any auto-created data to prevent concurrency exceptions
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				form.FireSaveButton();
			}

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			using (var form = new TransportBookingMultiForm(booking.ConsolidationSingleJob))
			{
				form.Show();
				form.transportBookingsControl.SelectBooking(booking);

				// call Deactivate Booking
				TransportBookingForm ClickMenuItem(string menuItemName)
				{
					var contextMenu = form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.ContextMenu;
					contextMenu.DoPopup();
					var deactivateMenu = contextMenu.MenuItems.FindByText(menuItemName);
					deactivateMenu.PerformClick();

					Application.DoEvents();
					return (TransportBookingForm)ZFormModaliser.ActiveForm;
				}

				using (var bookingForm = ClickMenuItem("Deactivate Booking"))
				{
					// make the save throw a 'FK Violation exception'
					void ThrowException(BusinessObjectFactory f)
					{
						f.ServiceContainer.AddAfterOnSavingService(new ServiceThatThrowsException(f));
					}

					var otherFactory = bookingForm.BusinessEntity.Factory;
					otherFactory.Saving += ThrowException;

					bookingForm.FireSaveButton();
					AssertEquals("Precondition: Save failed. Reload form failed",
"This record is in use by other records in the system. Would you like to mark this record as Inactive?", UnitTestUserNotification.Instance.LastMessage.Text);

					otherFactory.Saving -= ThrowException;
					ErrorReporter.Clear();
				}

				using (var reattemptForm = ClickMenuItem("Deactivate Booking"))
				{
					reattemptForm.FireSaveButton(); // save again, should work now
				}

				AssertEquals("Precondition: Booking is Deactivated in original Factory.", TransportStatuses.Codes.Deactivated, booking.KM_Status);
				AssertEquals("Should not have Deleted Billing Job on Shipment.", false, job.IsDeleted);
				booking.Instructions[0].KN_ServiceInstruction = "11"; // make random change and save
				form.FireSaveButton();

				using (var bookingForm = ClickMenuItem("Activate Booking"))
				{
					AssertNoExceptionThrown(() => bookingForm.FireSaveButton());
					AssertEquals("Booking should be available again.", TransportStatuses.Codes.Available, booking.KM_Status);
				}
			}
		}

		class ServiceThatThrowsException : IAfterOnSavingBOProcessingService
		{
			public ServiceThatThrowsException(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			BusinessObjectFactory Factory { get; }

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				Factory.ServiceContainer.RemoveAfterOnSavingService<ServiceThatThrowsException>();
				// Throwing an exception with the following Message makes the ZForm think that a Foreign Key was violated.
				var inner = new InvalidOperationException("The DELETE statement conflicted with the REFERENCE constraint");
				throw new ZDataException(inner, null, ((IDbConnected)Factory).Connection);
			}
		}

		protected override DtbBookingConsolidation GetNewConsolidation()
		{
			return Helper.CreateConsolidation();
		}

		protected override bool NewActionEnabled
		{
			get { return false; }
		}

		protected override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbBookingConsolidation; }
		}
	}
}
