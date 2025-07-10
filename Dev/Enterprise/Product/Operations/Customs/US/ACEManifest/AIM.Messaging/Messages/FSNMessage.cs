using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class FSNMessage : AIMInboundMessage
	{
		public FSNMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString MessageTypeCode => Constants.AIMMessageSubTypes.FSN;
		public override ZString MessageTypeDescription => "Freight Status Notification";

		#region CargoControlLocation

		public ZString AirportOfArrival => CargoControlLocation.AirportOfArrival.Trim();
		public ZString CargoTerminalOperator => CargoControlLocation.CargoTerminalOperator.Trim();

		AIMCargoControlLocation CargoControlLocation => cargoControlLocation ?? (cargoControlLocation = PopulateMessageBlock(new AIMCargoControlLocation(), lineIndex: 1));
		AIMCargoControlLocation cargoControlLocation;

		#endregion

		#region Arrival

		public ZString FlightNumber => Arrival.FlightNumber;

		public ZString PartArrivalReference => Arrival.PartArrivalReference;

		public ZDate CalculatedScheduledArrivalDate
		{
			get => calculatedScheduledArrivalDate ?? (calculatedScheduledArrivalDate = Arrival.EstimateScheduledArrivalDate).Value;
			set => calculatedScheduledArrivalDate = value;
		}
		ZDate? calculatedScheduledArrivalDate;

		public AIMArrivalWrapper Arrival => arrival ?? (arrival = new AIMArrivalWrapper(PopulateMessageBlock(new AIMArrival())));
		AIMArrivalWrapper arrival;

		#endregion

		#region StatusNotification

		public ZString ActionCode => StatusNotification.ActionCode.Trim();

		public ZDecimal Pieces => statusNotification.NumberOfPieces;

		public ZString Remarks => StatusNotification.Remarks.Trim();

		public ZInt NumberOfPieces => StatusNotification.NumberOfPieces.ToZInt();

		public ZString EntryType => StatusNotification.EntryType.Trim();

		public ZString EntryNumber => StatusNotification.EntryNumber.Trim();

		AIMCBPStatusNotification StatusNotification => statusNotification ?? (statusNotification = PopulateMessageBlock(new AIMCBPStatusNotification()));
		AIMCBPStatusNotification statusNotification;

		#endregion

		#region Message Text Interpretation

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => fEM_MessageInterpretation.IsEmpty ? (fEM_MessageInterpretation = GetMessageInterpretation()) : fEM_MessageInterpretation;
		}
		ZString fEM_MessageInterpretation;

		ZString GetMessageInterpretation()
		{
			var builder = new ZStringBuilder();
			builder.Append(GetMessageInterpretationHeader());
			builder.Append(GetMessageInterpretationBody());

			return builder.ToString();
		}

		protected virtual ZString GetMessageInterpretationHeader()
		{
			var builder = new ZStringBuilder();

			if (EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit)
			{
				builder.Append($"Arrival Message Sent");
				builder.Append(ZString.Empty);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected virtual ZString GetMessageInterpretationBody()
		{
			var builder = new ZStringBuilder();

			builder.Append($"Masterbill: {AirWaybillPrefix}-{AirWaybillSerialNumber}");
			if (!HAWBNumber.IsEmpty)
			{
				builder.Append($"Housebill: {HAWBNumber}");
			}

			if (!PackageTrackingIdentifier.IsEmpty)
			{
				builder.Append($"Package Tracking ID: {PackageTrackingIdentifier}");
			}

			builder.Append(ZString.Empty);
			builder.Append($"Flight No.: {FlightNumber}");
			builder.Append($"Scheduled Arrival Date: {CalculatedScheduledArrivalDate:dd-MMM}");
			builder.Append($"Arrival Airport: {AirportOfArrival}");
			builder.Append(ZString.Empty);
			if (!ActionCode.IsEmpty)
			{
				builder.Append($"CBP Status Notification");
				var dispositionCodeList = AIMDispositionCodesHelper.GetCustomsStatusList(Factory);
				var dispositionActionCode = ActionCode;
				var piecesDescriptor = Pieces > 1 ? "pieces" : "piece";
				builder.Append($"{dispositionActionCode} - {dispositionCodeList.GetDescriptionFromCode(dispositionActionCode)} - for {Pieces} {piecesDescriptor}");
				if (!Remarks.IsEmpty)
				{
					builder.Append($"Remarks: {Remarks}");
				}

				builder.Append(ZString.Empty);
			}

			foreach (var txtLine in Lines.Where(line => line.StartsWith("TXT/") || line.StartsWith("/")))
			{
				builder.Append($"{Regex.Replace(txtLine, @"^(TXT)?/", string.Empty).TrimEnd()}");
			}

			AppendLinesToInterpretation("WBL", builder);
			AppendLinesToInterpretation("ARR", builder);
			AppendLinesToInterpretation("CSN", builder);

			builder.Append(ZString.Empty);
			builder.Append(ZString.Empty);
			builder.Append(EM_MessageText);

			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected void AppendLinesToInterpretation(string lineHeader, ZStringBuilder interpretationBuilder)
		{
			var linesWithHeader = Lines.Where(line => line.StartsWith(lineHeader)).Select(line => line.TrimEnd()).Distinct();
			foreach (var lineWithHeader in linesWithHeader)
			{
				interpretationBuilder.Append(lineWithHeader);
			}
		}

		#endregion
	}
}
