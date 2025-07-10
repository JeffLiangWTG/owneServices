using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public static class PortMessagingHelper
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool GetPropertiesReadOnlyState(IPortMessaging portMessaging, PropertyDescriptor property)
		{
			bool isReadOnly = false;

			switch (property.Name)
			{
				case (JobShipmentPortMessagingSchema.Constants.JSM_ATBNumber):
				case (JobPackLinePortMessagingSchema.Constants.JLM_ATBNumber):
					isReadOnly = portMessaging.EntryType != EntryTypeList.Codes.Message
								&& portMessaging.EntryType != EntryTypeList.Codes.EUPortOfDestination
								&& portMessaging.EntryType != EntryTypeList.Codes.ExitSummaryDeclaration
								&& portMessaging.EntryType != EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;
					break;

				case (JobShipmentPortMessagingSchema.Constants.JSM_ExemptionReason):
				case (JobPackLinePortMessagingSchema.Constants.JLM_ExemptionReason):
					isReadOnly = portMessaging.EntryType != EntryTypeList.Codes.Message
								&& portMessaging.EntryType != EntryTypeList.Codes.OtherExemptions;
					break;

				case (JobShipmentPortMessagingSchema.Constants.JSM_Annex30AType):
				case (JobPackLinePortMessagingSchema.Constants.JLM_Annex30AType):
					isReadOnly = portMessaging.EntryType != EntryTypeList.Codes.Message;
					break;

				case (JobShipmentPortMessagingSchema.Constants.JSM_Annex30AFailureProcess):
				case (JobPackLinePortMessagingSchema.Constants.JLM_Annex30AFailureProcess):
					isReadOnly = portMessaging.EntryType != EntryTypeList.Codes.Message || !IsValidAnnex30AType(portMessaging.Annex30AType);
					break;

				case (JobShipmentPortMessagingSchema.Constants.JSM_ExportDeclarationReference):
				case (JobPackLinePortMessagingSchema.Constants.JLM_ExportDeclarationReference):
					isReadOnly = portMessaging.EntryType != EntryTypeList.Codes.EmergencyConcept;
					break;

				case (JobShipmentPortMessagingSchema.Constants.JSM_MovementReferenceNumber):
				case (JobPackLinePortMessagingSchema.Constants.JLM_MovementReferenceNumber):
				case (JobShipmentPortMessagingSchema.Constants.JSM_MovementReferenceNumberComplete):
				case (JobPackLinePortMessagingSchema.Constants.JLM_MovementReferenceNumberComplete):
					isReadOnly = portMessaging.EntryType == EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN
							|| (portMessaging.EntryType == EntryTypeList.Codes.AE1ExportDeclaration && IsEORIAndLRNEffectiveDate());
					break;

				case (JobShipmentPortMessagingSchema.Constants.JSM_LocalReferenceNumber):
				case (JobPackLinePortMessagingSchema.Constants.JLM_LocalReferenceNumber):
				case (JobShipmentPortMessagingSchema.Constants.JSM_LocalReferenceNumberComplete):
				case (JobPackLinePortMessagingSchema.Constants.JLM_LocalReferenceNumberComplete):
					isReadOnly = !IsEORIAndLRNEffectiveDate() || portMessaging.EntryType != EntryTypeList.Codes.AE1ExportDeclaration;
					break;
			}

			return isReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(portMessaging, property);
		}

		static bool IsValidAnnex30AType(ZString type)
		{
			return type == Annex30ATypeList.Codes.AlreadyCompleted
				|| type == Annex30ATypeList.Codes.SeparateEntry
				|| type == Annex30ATypeList.Codes.TransportDeclaration;
		}

		public static bool CheckPortMessagingForConsol(ForwardingConsol consol, Func<IPortMessaging, bool> predicate)
		{
			return consol.TopLevelShipments.Cast<ForwardingShipment>().Any(x => CheckPortMessagingForShipment(x, predicate));
		}

		public static bool CheckPortMessagingForShipment(ForwardingShipment shipment, Func<IPortMessaging, bool> predicate)
		{
			var portMessaging = GetShipmentPortMessaging(shipment);
			return portMessaging != null && (predicate(portMessaging) || shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(x => CheckPortMessagingForPackLine(x, predicate)));
		}

		public static ShipmentPortMessaging GetShipmentPortMessaging(ForwardingShipment shipment)
		{
			return shipment.Factory.GetCachedValue("PortMessagingHelper|GetShipmentPortMessaging|" + shipment.PK.ToString(), () => ShipmentPortMessaging.Load(shipment));
		}

		public static bool CheckPortMessagingForPackLine(ForwardingPackLine packLine, Func<IPortMessaging, bool> predicate)
		{
			var portMessaging = GetPackLinePortMessaging(packLine);
			return portMessaging != null && predicate(portMessaging);
		}

		static PackLinePortMessaging GetPackLinePortMessaging(ForwardingPackLine packLine)
		{
			return packLine.Factory.GetCachedValue("PortMessagingHelper|GetPackLinePortMessaging|" + packLine.PK.ToString(), () => PackLinePortMessaging.Load(packLine));
		}

		public static bool IsEORIAndLRNEffectiveDate() => ZDateTime.UtcNow >= PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.Value;
	}
}
