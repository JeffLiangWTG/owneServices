using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	public class ContainerMovementDataContextManager : EventDataContextManager<ContainerMovement>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.ContainerMovement; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.Stock != null ? ParentBO.Stock.R6_ContainerNum : ZString.Empty; }
		}

		delegate IZType GetBookingFieldDelegate(AgencyShipment booking);

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var containers = ParentBO.RelatedInfo.LoadContainers();

				result.AddIfNotEmpty(Event.ContextTypes.VoyageNumber, ParentBO.VoyageNo);
				result.AddIfNotEmpty(Event.ContextTypes.ContainerMovementType, ParentBO.E9_MovementType);
				result.AddIfNotEmpty(Event.ContextTypes.IsEmptyContainer, ParentBO.E9_ContainerIsEmpty);
				result.AddIfNotEmpty(Event.ContextTypes.ContainerDamageCode, ParentBO.E9_ContainerCondition);
				result.AddIfNotEmpty(Event.ContextTypes.ContainerConditionCode, ParentBO.E9_ContainerQuality);
				result.AddIfNotEmpty(Event.ContextTypes.ContainerLeaseNumber, ParentBO.E9_LeaseNumber);
				result.AddIfNotEmpty(Event.ContextTypes.CarriersBookingReference, ParentBO.RelatedInfo.BookingNumbers);
				result.AddIfNotEmpty(Event.ContextTypes.EntryNumber, GetEntryNumbers(containers));
				result.AddIfNotEmpty(Event.ContextTypes.EntryNumberType, GetBookingNumbersTypes(containers));
				result.AddIfNotEmpty(Event.ContextTypes.EntryNumberCountryOfIssue, new ZString(Constants.CountryCodes.Australia));
				result.AddIfNotEmpty(Event.ContextTypes.MBOLOriginUNLOCO, ParentBO.RelatedInfo.LoadPort);
				result.AddIfNotEmpty(Event.ContextTypes.MBOLDestinationUNLOCO, ParentBO.RelatedInfo.DischargePort);
				result.AddIfNotEmpty(Event.ContextTypes.EventActionUNLOCO, ParentBO.DepotPort);
				result.AddIfNotEmpty(Event.ContextTypes.EstimatedTimeOfArrival, GetBookingField(containers, b => b.JS_E_ARV));
				result.AddIfNotEmpty(Event.ContextTypes.EstimatedTimeOfDeparture, GetBookingField(containers, b => b.JS_E_DEP));
				result.AddIfNotEmpty(Event.ContextTypes.MBOLNumber, GetBookingField(containers, b => b.JS_HouseBill));
				result.AddIfNotEmpty(Event.ContextTypes.ContainerDestinationUNLOCO, GetBookingField(containers, b => b.JS_RL_NKDestination));
				result.AddIfNotEmpty(Event.ContextTypes.ContainerFinalDestinationUNLOCO, GetBookingField(containers, b => b.JS_RL_NKDestination));
				result.AddIfNotEmpty(Event.ContextTypes.ContainerConsigneeName, GetBookingField(containers, b => b.Consignee != null ? b.Consignee.OH_FullNameTruncated : ZString.Empty));
				result.AddIfNotEmpty(Event.ContextTypes.ContainerConsignorName, GetBookingField(containers, b => b.Consignor != null ? b.Consignor.OH_FullNameTruncated : ZString.Empty));
				result.AddIfNotEmpty(Event.ContextTypes.ContainerTemperatureSetting, GetTemperatureSetting(containers));
				result.AddIfNotEmpty(Event.ContextTypes.ContainerGoodsDescription, GetBookingField(containers, b => b.JS_GoodsDescription));
				result.AddIfNotEmpty(Event.ContextTypes.ContainerSealNo, GetSealNumbers(containers));
				result.AddIfNotEmpty(Event.ContextTypes.VesselName, ParentBO.VesselName);
				result.AddIfNotEmpty(Event.ContextTypes.LegOriginUNLOCO, ParentBO.RelatedInfo.LoadPort);

				if (ParentBO.Voyage != null && ParentBO.Voyage.Vessel != null)
				{
					result.AddIfNotEmpty(Event.ContextTypes.LloydsNumber, ParentBO.Voyage.Vessel.RV_LloydsNumber);
					result.AddIfNotEmpty(Event.ContextTypes.VesselCallSign, ParentBO.Voyage.Vessel.RV_RadioCallSign);
				}

				if (ParentBO.Stock != null)
				{
					result.AddIfNotEmpty(Event.ContextTypes.ContainerNumber, ParentBO.Stock.R6_ContainerNum);
					result.AddIfNotEmpty(Event.ContextTypes.ContainerOwnershipType, ParentBO.Stock.R6_OwnerType);
					if (ParentBO.Stock.Owner != null)
					{
						result.AddIfNotEmpty(Event.ContextTypes.ContainerOwnerName, ParentBO.Stock.Owner.OH_FullNameTruncated);
					}

					if (ParentBO.Stock.Container != null)
					{
						result.AddIfNotEmpty(Event.ContextTypes.ContainerISOCode, ParentBO.Stock.Container.RC_ISOType);
						if (!ParentBO.Stock.Container.RC_GrossWeight.IsEmpty)
						{
							result.AddIfNotEmpty(Event.ContextTypes.ContainerGrossWeight, new ZString(ParentBO.Stock.Container.RC_GrossWeight + " KG"));
						}
					}
				}
			}

			return result;
		}

		IZType GetTemperatureSetting(AgencyShipmentContainer[] containers)
		{
			if (containers != null && containers.Length > 0)
			{
				return new ZString(string.Concat(containers[0].JC_SetPointTemp.ToString(), " ", containers[0].JC_SetPointTempUnit));
			}

			return ZString.Empty;
		}

		IZType GetSealNumbers(AgencyShipmentContainer[] containers)
		{
			return CombineString(containers, a => a.JC_SealNum);
		}

		IZType GetEntryNumbers(AgencyShipmentContainer[] containers)
		{
			return CombineString(containers, a => a.Booking.CustomsEntryNumber);
		}

		IZType GetBookingField(AgencyShipmentContainer[] containers, GetBookingFieldDelegate action)
		{
			if (containers != null && containers.Length > 0)
			{
				var container = containers.Where(c => c.Booking != null).FirstOrDefault();
				if (container != null)
				{
					return action(container.Booking);
				}
			}

			return ZString.Empty;
		}

		IZType GetBookingNumbersTypes(AgencyShipmentContainer[] containers)
		{
			return CombineString(containers, a => a.Booking.CustomsEntryNumberType);
		}

		ZString CombineString(AgencyShipmentContainer[] containers, Func<AgencyShipmentContainer, string> selector)
		{
			if (containers != null)
			{
				var values = containers.Select(selector);
				var builder = new ZStringBuilder();
				Array.ForEach(values.ToArray(), (string value) => { builder.AppendIfNotEmpty(value); });
				return builder.ToStringWithDelimiterBetweenAppends(",");
			}

			return ZString.Empty;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ContainerMovementEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			throw new NotImplementedException();
		}
	}
}
