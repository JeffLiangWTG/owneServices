using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingToShipmentConverter
	{
		public QuotedBookingToShipmentConverter(QuotedBooking booking, BookingToShipmentConversionSource conversionSource)
		{
			Argument.NotNull(booking, "QuotedBooking");

			QuotedBooking = booking;
			ConversionSource = conversionSource;
		}

		public enum BookingToShipmentConversionSource
		{
			Form = 0,
			WorkflowTrigger = 1,
		}

		public bool HasAnyErrors(out string error)
		{
			error = null;

			if (QuotedBooking.Booking == null)
			{
				error = Res.GetString("7b3d0dfc-8f44-46f1-9597-10bae2aa4b25", "Cannot convert a Spot Quote to Shipment without a Booking");
			}
			else
			{
				if (ConversionSource == BookingToShipmentConversionSource.Form && (QuotedBooking.HasChanges || !QuotedBooking.Booking.IsInDatabase))
				{
					error = Res.GetString(
						"dd3f7cfa-5979-44dd-ac5b-fd76c4a261f3",
						"Please save the booking before converting to shipment."
					);
				}
				else if (QuotedBooking.Booking.JS_IsDirectBooking)
				{
					error = Res.GetString(
						"59e53776-299d-466b-bbc1-e30c69dfac5d",
						"You cannot convert a direct booking to a shipment. Please choose 'Consolidate' from the menu to create a new Consol."
					);
				}
				else if (QuotedBooking.ShipmentStatus != ShipmentStatusList.Codes.Booked)
				{
					error = Res.GetString(
						"c3cd2476-b474-429b-b862-ff96aee65589",
						"The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD."
					);
				}
			}

			converterHasCheckedForErrors = true;
			return error != null;
		}

		public void ConvertBookingToShipment(ForwardingShipment targetShipment)
		{
			if (!converterHasCheckedForErrors)
			{
				throw new BookingToShipmentIncorrectUsageException("HasAnyErrors has not been called before conversion.");
			}

			var buildConsolHelper = new BuildConsolHelper();
			buildConsolHelper.TurnBookingIntoShipment(targetShipment, null, QuotedBooking.PK);
			CreateDeclarationIfNecessary(targetShipment);
			targetShipment.DocsAndCartage?.Validation.ValidateAll();

			if (IsDeniedPartyOrComplianceRiskStatusNotClearAndFreightMovementRestricted(QuotedBooking))
			{
				if (ComplianceRiskHelper.IsFreightEnabledComplianceWise)
				{
					targetShipment.GetLogs().AddNew(AutoEvents.StatusUpdated, "Convert Booking to Shipment when Job Compliance Status is not CLR - Clear");
				}
				else
				{
					targetShipment.GetLogs().AddNew(AutoEvents.DeniedPartyStatusUpdated, "Convert Booking to Shipment when screen status is not CLR - Clear");
				}
			}

			if (!targetShipment.JS_IsDirectBooking)
			{
				if (targetShipment.JS_HouseBill.IsEmpty && targetShipment.Origin?.Country != null && targetShipment.Origin.Country.Code.EqualsIgnoringCase(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					targetShipment.RegenerateHouseBillInSaving(true);
				}
			}

			if (!targetShipment.JS_DeliveryDueDate.IsEmpty)
			{
				targetShipment.CreateDeliveryDueDateChangeTracker(ForwardingShipment.ConvertingBookingToShipment);
			}
		}

		public void CreateDeclarationIfNecessary(ForwardingShipment shipment)
		{
			if (FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.Value)
			{
				var broker = ZGuid.Empty;
				if (shipment.IsImport())
				{
					broker = shipment.JS_OH_ImportBroker;
				}
				else if (shipment.IsExport())
				{
					broker = shipment.JS_OH_ExportBroker;
				}

				if (!broker.IsEmpty && broker == (GlbBranch.CurrentBranch?.GB_OH_OrgProxy ?? ZGuid.Empty))
				{
					GetCreateDeclarationHelper().CreateDeclaration(shipment);
				}
			}
		}

		public static bool IsDeniedPartyOrComplianceRiskStatusNotClearAndFreightMovementRestricted(QuotedBooking quotedBooking)
		{
			bool statusNotClear;

			if (ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				var overallRiskStatus = ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus(quotedBooking).JobRisk;
				statusNotClear = overallRiskStatus != ComplianceRiskStatusCodeList.Codes.OverrideClear && overallRiskStatus != ComplianceRiskStatusCodeList.Codes.Clear;
			}
			else
			{
				var screeningStatus = ((IScreeningPartyProvider)quotedBooking.Booking).ScreeningStatus;
				statusNotClear = screeningStatus != ScreeningStatusesList.Codes.Clear && screeningStatus != ScreeningStatusesList.Codes.JobCleared;
			}

			return ((ICreditControlledDocumentDelivery)quotedBooking).IsDPSFreightMovementRestricted && statusNotClear;
		}

		#region Implementation

		internal ICreateDeclarationHelper GetCreateDeclarationHelper()
		{
			return ObjectFactory.Get<ICreateDeclarationHelperProvider>()
				.NewCreateDeclarationHelper(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		[Serializable]
		public class BookingToShipmentIncorrectUsageException : ZException
		{
			public BookingToShipmentIncorrectUsageException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected BookingToShipmentIncorrectUsageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		bool converterHasCheckedForErrors;
		readonly QuotedBooking QuotedBooking;
		readonly BookingToShipmentConversionSource ConversionSource;

		#endregion
	}
}
