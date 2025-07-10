using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class FSCMessageTypeProcessor : AIMMessageTypeProcessor<FSCMessage>
	{
		public FSCMessageTypeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool ProcessMessageCore(FSCMessage message, AsycudaManifestHeader manifest, AsycudaBill bill, EDIMessage mostRecentSentMessage)
		{
			if (message.ComponentIdentifier == AIMMessageSubTypes.FSC)
			{
				switch (message.StatusAnswerCode)
				{
					case AIMFreightStatusCodes.Codes.RecordNotOnFile:
						{
							ProcessRecordNotOnFile(bill);
							break;
						}
					case AIMFreightStatusCodes.Codes.BillIsSplit:
						{
							ProcessBillIsSplit(message, manifest, bill, mostRecentSentMessage);
							break;
						}
					case AIMFreightStatusCodes.Codes.RoutingInformationFollows:
						{
							ProcessRoutingInformation(message, manifest, bill, mostRecentSentMessage);
							break;
						}
					case AIMFreightStatusCodes.Codes.CurrentAirWaybillInformationOnFileFollows:
						{
							ProcessCurrentAirWaybillInformationOnFile(message, manifest, bill, mostRecentSentMessage);
							break;
						}
				}

				return true;
			}
			return false;
		}

		void ProcessRecordNotOnFile(AsycudaBill bill)
		{
			bill.ABL_BillStatus = ZString.Empty;
			bill.ABL_MessageStatus = ZString.Empty;
		}

		void ProcessBillIsSplit(FSCMessage message, AsycudaManifestHeader manifest, AsycudaBill bill, EDIMessage mostRecentSentMessage)
		{
			foreach (var arrival in message.SplitBillArrivals)
			{
				var arrivalLine = CreateUpdateArrivalLine(manifest, bill, arrival);
				UpdateTransferBill(bill, arrivalLine, mostRecentSentMessage);
			}
		}

		void ProcessRoutingInformation(FSCMessage message, AsycudaManifestHeader manifest, AsycudaBill bill, EDIMessage mostRecentSentMessage)
		{
			if (message.PartArrivalReference.IsEmpty)
			{
				UpdateManifestFlightDetails(manifest, message);

				var arrivalPort = UNLOCOFromIATA(message.Factory, message.RoutingInformation.AirportOfArrival);
				if (!arrivalPort.IsEmpty)
				{
					var dischargePort = UNLOCOFromIATA(message.Factory, message.RoutingInformation.PermitToProceedDestinationAirport);
					if (!dischargePort.IsEmpty || !manifest.AMA_RL_NKPortOfFirstArrival.IsEmpty)
					{
						manifest.AMA_RL_NKPortOfFirstArrival = arrivalPort;
					}

					manifest.AMA_RL_NKPortOfDischarge = !dischargePort.IsEmpty ? dischargePort : arrivalPort;
				}
			}
			else
			{
				var arrivalLine = CreateUpdateArrivalLine(manifest, bill, message.Arrival);
				UpdateTransferBill(bill, arrivalLine, mostRecentSentMessage);
			}
		}

		void ProcessCurrentAirWaybillInformationOnFile(FSCMessage message, AsycudaManifestHeader manifest, AsycudaBill bill, EDIMessage mostRecentSentMessage)
		{
			if (message.PartArrivalReference.IsEmpty)
			{
				UpdateManifestFlightDetails(manifest, message);

				var dischargePort = UNLOCOFromIATA(message.Factory, message.WayBill.PermitToProceedDestinationAirport);
				if (!dischargePort.IsEmpty)
				{
					manifest.AMA_RL_NKPortOfDischarge = dischargePort;
				}
			}

			if (message.AirWayBillArrival is AIMArrivalWrapper airWayBill)
			{
				var arrivalLine = CreateUpdateArrivalLine(manifest, bill, airWayBill);
				if (arrivalLine.ATL_Reference.IsEmpty)   // indicates this is not a split shipment where the arrival.BoardedPieces is used for Quantity. Therefore Total Qty of the Waybill is required for pieces.
				{
					arrivalLine.ATL_Quantity = (ZInt)message.WayBill.NumberOfPieces;
				}

				UpdateTransferBill(bill, arrivalLine, mostRecentSentMessage);
			}

			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Registered;
		}

		void UpdateManifestFlightDetails(AsycudaManifestHeader manifest, FSCMessage message)
		{
			var flightNumber = message.FlightNumber;
			var arrivalDate = message.CalculatedScheduledArrivalDate;
			if (manifest.AMA_Voyage != flightNumber || manifest.AMA_E_ARV.Date != arrivalDate)
			{
				if (!flightNumber.IsEmpty)
				{
					manifest.AMA_Voyage = flightNumber;
				}

				if (!arrivalDate.IsEmpty)
				{
					manifest.AMA_E_ARV = arrivalDate;
				}
			}
		}

		ZString UNLOCOFromIATA(BusinessObjectFactory factory, ZString iataCode)
		{
			var unLoco = !iataCode.IsEmpty ? RefUNLOCO.LoadFromIATA(factory, iataCode) : null;
			return unLoco?.RL_Code ?? ZString.Empty;
		}

		protected override EDIMessage FindMostRecentSentMessage(EDIMessageCollectionNonDependent messages, FSCMessage message, AsycudaManifestHeader manifest, AsycudaBill bill)
		{
			message.CalculatedScheduledArrivalDate = CalculateScheduledArrivalDate(message.CalculatedScheduledArrivalDate, manifest.AMA_E_ARV);
			return base.FindMostRecentSentMessage(messages, message, manifest, bill);
		}

		#region eMailings

		protected override void AddRowsToPropertiesTable(HtmlTableCreator propertiesTable, FSCMessage message)
		{
			propertiesTable.WriteRow("Message Time", message.EM_SystemCreateTimeUtc);
			propertiesTable.WriteRow("Air Waybill Number", message.MAWBNumber);
			propertiesTable.WriteRow("HAWB Number", message.HAWBNumber);
			propertiesTable.WriteRow("Package Tracking Identifier", message.PackageTrackingIdentifier);
			propertiesTable.WriteRow("Flight Number", message.FlightNumber);
			propertiesTable.WriteRow("Part Arrival Reference", message.PartArrivalReference);
			propertiesTable.WriteRow("Scheduled Arrival Date", message.CalculatedScheduledArrivalDate);
			propertiesTable.WriteRow("Status Answer Code", message.StatusAnswerCode + " - " + (new AIMFreightStatusCodes().GetDescriptionFromCode(message.StatusAnswerCode)));
			propertiesTable.WriteRow("Information", message.Information);
		}

		protected override ZString GetEmailContentPart2(FSCMessage message, EmailDefBuilder emailBuilder, AsycudaBill bill)
		{
			switch (message.StatusAnswerCode)
			{
				case AIMFreightStatusCodes.Codes.BillIsSplit:
					return GetEmailContent_BillIsSplit(message, bill);
				case AIMFreightStatusCodes.Codes.BillStatusInformationFollows:
					return GetEmailContent_BillStatusInformation(message);
				case AIMFreightStatusCodes.Codes.RoutingInformationFollows:
					return GetEmailContent_RoutingInformation(message);
				case AIMFreightStatusCodes.Codes.CurrentAirWaybillInformationOnFileFollows:
					return GetEmailContent_CurrentAirWaybillInformationOnFile(message);
				default:
					return ZString.Empty;
			}
		}

		ZString GetEmailContent_BillIsSplit(FSCMessage message, AsycudaBill bill)
		{
			ZString tableHtml = "<P><B>FSC Code 02 - Split Bill</B></P>";

			var referenceDate = bill.Header?.AMA_E_ARV ?? ZDate.Empty;

			var table = new HtmlTableCreator(new string[] { "Part Arrival Reference", "Flight Number", "Scheduled Arrival Date", "Boarded Pieces" });
			foreach (var arrival in message.SplitBillArrivals)
			{
				table.WriteRow(arrival.PartArrivalReference, arrival.FlightNumber, CalculateScheduledArrivalDate(arrival.EstimateScheduledArrivalDate, referenceDate), arrival.BoardedPieces);
			}

			return tableHtml += table.ToHtml();
		}

		ZString GetEmailContent_BillStatusInformation(FSCMessage message)
		{
			ZString tableHtml = "<P><B>FSC Code 07 - Bill Status Information</B></P>";
			var table = new HtmlTableCreator(new string[] { "Conditions" });
			foreach (var line in message.Conditions)
			{
				table.WriteRow(line.TrimEnd());
			}

			foreach (var line in message.TextContinuation)
			{
				table.WriteRow(line.SubstringSafe(1).TrimEnd());
			}

			return tableHtml += table.ToHtml();
		}

		ZString GetEmailContent_RoutingInformation(FSCMessage message)
		{
			var routingInformation = message.RoutingInformation;
			ZString tableHtml = "<P><B>FSC Code 08 - Routing Information</B></P>";

			var table = new HtmlTableCreator(new string[] { "Arrival Port" });
			table.WriteRow(routingInformation.AirportOfArrival);

			return tableHtml += table.ToHtml();
		}

		ZString GetEmailContent_CurrentAirWaybillInformationOnFile(FSCMessage message)
		{
			var waybill = message.WayBill;
			ZString tableHtml = "<P><B>FSC Code 10 - Air Waybill</B></P>";

			var table = new HtmlTableCreator(new string[] { "Port of Origin", "Destination Port" });
			var destinationPort = waybill.PermitToProceedDestinationAirport;
			if (destinationPort.IsEmpty)
			{
				table = new HtmlTableCreator(new string[] { "Port of Origin" });
				table.WriteRow(waybill.AirportOfOrigin);
			}
			else
			{
				table.WriteRow(waybill.AirportOfOrigin, destinationPort);
			}

			return tableHtml += table.ToHtml();
		}

		#endregion
	}
}
