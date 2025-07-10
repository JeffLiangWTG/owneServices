using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class FERMessage : AIMInboundMessage
	{
		public FERMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString MessageTypeCode => Constants.AIMMessageSubTypes.FER;
		public override ZString MessageTypeDescription => "Freight Error Report";

		#region ErrorReportFlight

		public ZString FlightNumber => Arrival.FlightNumber;

		public ZDate CalculatedArrivalDate
		{
			get => calculatedArrivalDate ?? (calculatedArrivalDate = Arrival.EstimateScheduledArrivalDate).Value;
			set => calculatedArrivalDate = value;
		}
		ZDate? calculatedArrivalDate;

		AIMArrivalWrapper Arrival => arrival ?? (arrival = new AIMArrivalWrapper(PopulateMessageBlock(new AIMErrorReportFlight(), lineIndex: 1)));
		AIMArrivalWrapper arrival;

		#endregion

		public List<AIMError> Errors
		{
			get
			{
				if (errors == null)
				{
					errors = new List<AIMError>();

					for (var idx = 3; idx < Lines.Count; idx++)
					{
						var error = new AIMError();
						error.Deserialise(Lines[idx]);
						errors.Add(error);
					}
				}
				return errors;
			}
		}
		List<AIMError> errors;

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
			builder.Append($"Masterbill: {AirWaybillPrefix}-{AirWaybillSerialNumber}");
			if (!HAWBNumber.IsEmpty)
			{
				builder.Append($"Housebill: {HAWBNumber}");
			}

			if (!PackageTrackingIdentifier.IsEmpty)
			{
				builder.Append($"Tracking ID: {PackageTrackingIdentifier}");
			}

			if (!HAWBNumber.IsEmpty || !PackageTrackingIdentifier.IsEmpty)
			{
				builder.Append(ZString.Empty);
			}

			foreach (var error in Errors)
			{
				builder.Append($"{error.ErrorCode} - {error.ErrorMessageText}");
			}

			builder.Append(ZString.Empty);
			builder.Append(ZString.Empty);
			builder.Append(EM_MessageText);

			return builder.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}
