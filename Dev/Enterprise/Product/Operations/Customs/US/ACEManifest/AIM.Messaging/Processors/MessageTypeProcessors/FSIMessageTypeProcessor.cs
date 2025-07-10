using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class FSIMessageTypeProcessor : AIMMessageTypeProcessor<FSIMessage>
	{
		public FSIMessageTypeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override EDIMessage FindMostRecentSentMessage(EDIMessageCollectionNonDependent messages, FSIMessage message, AsycudaManifestHeader manifest, AsycudaBill bill)
		{
			message.CalculatedScheduledArrivalDate = CalculateScheduledArrivalDate(message.CalculatedScheduledArrivalDate, manifest.AMA_E_ARV);
			return base.FindMostRecentSentMessage(messages, message, manifest, bill);
		}

		protected override bool ProcessMessageCore(FSIMessage message, AsycudaManifestHeader manifest, AsycudaBill bill, EDIMessage mostRecentSentMessage)
		{
			return message.ComponentIdentifier == Constants.AIMMessageSubTypes.FSI;
		}

		protected override void AddRowsToPropertiesTable(HtmlTableCreator propertiesTable, FSIMessage message)
		{
			propertiesTable.WriteRow("Message Time", message.EM_SystemCreateTimeUtc);
			propertiesTable.WriteRow("Air Waybill Number", message.MAWBNumber);
			propertiesTable.WriteRow("HAWB Number", message.HAWBNumber);
			propertiesTable.WriteRow("Package Tracking Identifier", message.PackageTrackingIdentifier);
			propertiesTable.WriteRow("Flight Number", message.FlightNumber);
			propertiesTable.WriteRow("Scheduled Arrival Date", message.CalculatedScheduledArrivalDate);
			propertiesTable.WriteRow("Part Arrival Reference", message.PartArrivalReference);
			propertiesTable.WriteRow("Airport Of Arrival", message.AirportOfArrival);
			propertiesTable.WriteRow("Cargo Terminal Operator", message.CargoTerminalOperator);
			propertiesTable.WriteRow("Action Code", message.ActionCode);
			propertiesTable.WriteRow("Remarks", message.Remarks);
		}
	}
}
