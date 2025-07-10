using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingConfirmationDataContextManager : EventDataContextManager<DtbBookingConfirmation>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportBookingConfirmation; }
		}

		public override ZString DataContextKey
		{
			get { return ""; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery { IsNoResultQuery = true };
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();
			var confirmation = ParentBO;
			if (confirmation != null && confirmation.HasPackages)
			{
				var packages = confirmation.PackageDivot != null ? new[] { confirmation.PackageDivot.Package } : confirmation.Instruction.DivotsWithPackages.Packages.ToArray();
				foreach (var container in packages.Where(p => p.KP_PackageID.IsValid && p.IsContainer))
				{
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.ContainerNumber, container.KP_PackageID);
				}
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DtbBookingConfirmationEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);
			SetTimeFromEvent(eventAdded);
			SetReferenceFromEvent(eventAdded);
		}

		void SetTimeFromEvent(UniversalEvent eventAdded)
		{
			var value = eventAdded.EventType.GetValueOrDefault();

			switch (value)
			{
				case AutoEvents.PickedUpCode:
				case AutoEvents.DeliveredCode:
					if (eventAdded.IsEstimate.GetValueOrDefault())
					{
						SetEstimated(eventAdded.EventTime);
					}
					else
					{
						SetActual(eventAdded.EventTime);
					}
					break;
				case AutoEvents.PickupCartageCompleteFinalisedCode:
				case AutoEvents.DeliveryCartageCompleteFinalisedCode:
				case AutoEvents.CartageCompleteFinalisedCode:
				case AutoEvents.GateInCode when eventAdded.EventParameters != null && eventAdded.EventParameters.Facility.GetValueOrDefault() == CargoWise.EventReference.Constants.Facilities.Code.ContainerYard:
					SetActual(eventAdded.EventTime);
					break;
				case AutoEvents.SlotConfirmedCode:
				case AutoEvents.SlotRequestedCode:
					SetSlot(eventAdded.EventTime);
					break;
				case AutoEvents.SlotCancelledCode:
					SetSlot(ZDateTimeOffset.Empty);
					break;
				default:
					break;
			}
		}

		void SetReferenceFromEvent(UniversalEvent eventAdded)
		{
			var value = eventAdded.EventType.GetValueOrDefault();

			switch (value)
			{
				case AutoEvents.SlotConfirmedCode:
				case AutoEvents.SlotRequestedCode:
					SetReference(eventAdded.EventReference, ParentBO.KK_ReferenceNumInfo);
					break;
				case AutoEvents.SlotCancelledCode:
					SetReference(string.Empty, ParentBO.KK_ReferenceNumInfo);
					break;
				case AutoEvents.ServiceCommencedCode:
					SetReference(eventAdded.EventReference, ParentBO.KK_ReferenceNumInfo);
					SetBookingReferenceIfRequired(eventAdded.EventReference);
					ParentBO.Booking.UpdateStatus();
					break;
				default:
					break;
			}
		}

		void SetEstimated(ZDateTimeOffset? eventTime)
		{
			if (eventTime.HasValue)
			{
				ParentBO.KK_Estimated = eventTime.GetValueOrDefault().ToZDateTime();
			}
		}

		void SetActual(ZDateTimeOffset? eventTime)
		{
			if (eventTime.HasValue)
			{
				ParentBO.KK_Actual = eventTime.GetValueOrDefault().ToZDateTime();
			}
		}

		void SetSlot(ZDateTimeOffset? eventTime)
		{
			if (eventTime.HasValue)
			{
				ParentBO.KK_SlotDateTime = eventTime.GetValueOrDefault().ToZDateTime();
			}
		}

		void SetReference(ZString? eventReference, ZPropertyInfo info)
		{
			if (eventReference.HasValue)
			{
				var referenceParameters = StmALog.GetParametersFromReference(eventReference.Value);

				if (!referenceParameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber)
					|| string.IsNullOrEmpty(referenceParameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber]))
				{
					info.Value = StmALog.GetFreeTextFromReference(eventReference.Value).Left(info.MaxLength);
				}
				else
				{
					info.Value = (ZString)referenceParameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber];
				}
			}
		}

		void SetBookingReferenceIfRequired(ZString? eventReference)
		{
			if (eventReference.HasValue)
			{
				var booking = ParentBO.Booking;
				var deliveryInstructions = booking.Instructions.Where(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery);
				if (deliveryInstructions.Count() < 2)
				{
					SetReference(eventReference, booking.KM_TransportReferenceInfo);
				}
			}
		}
	}
}
