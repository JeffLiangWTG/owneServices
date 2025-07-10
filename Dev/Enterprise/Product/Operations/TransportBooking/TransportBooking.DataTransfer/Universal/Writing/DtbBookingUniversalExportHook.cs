using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public sealed class DtbBookingUniversalExportHook : IUniversalExportHook
	{
		public void OnUniversalXmlExportValidationFailure(BusinessObject businessObject, IEDICommunicationsMode mode)
		{
			if (mode.EK_CommunicationsTransport == EDIInterchangeTransportTypeList.Codes.eHub
				&& businessObject is DtbBooking bookingBO
				&& (bookingBO.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || bookingBO.NotificationBufferForSendingXUSToCTO.Events.HasErrors()))
			{
				var eventParameters = new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceMessageTypes.CarrierBookingAgent),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, LocalCartageJobOrgTypeList.Codes.CTO),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, $"Sending XUS to {DtbAgentBooking.ContainerTransportOptimizationCBA}:")
					};
				bookingBO.Logs.AddNew(Events.MessageValidationFailed, eventParameters);

				bookingBO.KM_Status = TransportStatuses.Codes.ActionRequired;
			}
		}

		void IUniversalExportHook.OnUniversalXmlExport(BusinessObject businessObject, IEDICommunicationsMode mode)
		{
			if (mode.EK_CommunicationsTransport == EDIInterchangeTransportTypeList.Codes.eHub
				&& businessObject is DtbBooking bookingBO)
			{
				if (mode.EK_Destination == DtbAgentBooking.ContainerTransportOptimizationCBA)
				{
					var eventParameters = new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceMessageTypes.CarrierBookingAgent),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, LocalCartageJobOrgTypeList.Codes.CTO),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DtbAgentBooking.ContainerTransportOptimizationCBA)
					};
					bookingBO.Logs.AddNew(Events.MessageSent, eventParameters);
				}
				else if (DtbAgentBooking.IsAuthorisedCarrierBookingAgent(mode.EK_Destination))
				{
					bookingBO.GetLogs().AddNew(AutoEvents.MessageSent, $"|TYP=Booking Request|DEP={mode.EK_Destination}");
				}
			}
		}
	}
}
