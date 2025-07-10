using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class FERMessageTypeProcessor : AIMMessageTypeProcessor<FERMessage>
	{
		public FERMessageTypeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool ProcessMessageCore(FERMessage ferMessage, AsycudaManifestHeader manifest, AsycudaBill bill, EDIMessage mostRecentSentMessage)
		{
			if (ferMessage.ComponentIdentifier == Constants.AIMMessageSubTypes.FER)
			{
				var flightNumber = ferMessage.FlightNumber;
				var arrivalDate = ferMessage.CalculatedArrivalDate;

				var isArrivalResponse = manifest.AMA_Voyage != flightNumber
					|| manifest.AMA_E_ARV != arrivalDate
					|| (mostRecentSentMessage != null && mostRecentSentMessage.EM_MessageSubType == Constants.AIMMessageSubTypes.FSN);

				var arrival = isArrivalResponse ? FindArrival(bill, flightNumber, arrivalDate) : null;

				if (arrival != null)
				{
					arrival.ATL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Error;
				}
				else
				{
					if (ferMessage.Errors.Any(err => err.ErrorCode == AIMErrorCodes.Codes.ExpressRecordIncomplete))
					{
						bill.ABL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Warning;
					}
					else
					{
						bill.ABL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Error;
					}
				}

				var transferBill = FindActiveTransferBill(bill, FindArrivalHeader(manifest, flightNumber, arrivalDate));
				if (transferBill != null)
				{
					if (mostRecentSentMessage != null && mostRecentSentMessage.EM_MessageSubType == Constants.AIMMessageSubTypes.FSN)
					{
						transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.ArrivalError;
					}
					else if (ferMessage.Errors.Count > 0)
					{
						transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;
					}
				}

				return true;
			}
			return false;
		}

		protected override EDIMessage FindMostRecentSentMessage(EDIMessageCollectionNonDependent messages, FERMessage ferMessage, AsycudaManifestHeader manifest, AsycudaBill bill)
		{
			EDIMessage result = null;

			ferMessage.CalculatedArrivalDate = CalculateScheduledArrivalDate(ferMessage.CalculatedArrivalDate, manifest.AMA_E_ARV);

			if (!bill.IsChildMasterBill)
			{
				var arrivalLine = FindArrival(bill, ferMessage.FlightNumber, ferMessage.CalculatedArrivalDate);
				if (arrivalLine != null)
				{
					var outgoingMessageQuery = new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
					outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, AsycudaArrivalHeaderSchema.Constants.TableName);
					outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, arrivalLine.ArrivalHeader.PK);
					outgoingMessageQuery.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + OrderByClause.Descending;

					result = messages.Find(outgoingMessageQuery).FirstOrDefault() as EDIMessage;
				}
			}

			if (result == null)
			{
				result = base.FindMostRecentSentMessage(messages, ferMessage, manifest, bill);
			}

			return result;
		}

		ManifestBase.AsycudaArrivalLine FindArrival(AsycudaBill bill, ZString flightNumber, ZDate arrivalDate)
		{
			if (!flightNumber.IsEmpty && !arrivalDate.IsEmpty)
			{
				var arrivalQuery = new ZDBOnlyQuery(typeof(ManifestBase.AsycudaArrivalLine));
				arrivalQuery.AddToFilter(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK);
				arrivalQuery.AddToFilter(AsycudaArrivalLineSchema.ATL_MessageStatus, ASYCUDA.Business.MessageStatusCodeList.Codes.Sent);
				var arrivalHeaderSubQuery = new ZDBOnlySubQuery(typeof(ManifestBase.AsycudaArrivalHeader), AsycudaArrivalLineSchema.ATL_ATH);
				arrivalHeaderSubQuery.AddToFilter(AsycudaArrivalHeaderSchema.ATH_VoyageFlightNo, flightNumber);
				arrivalHeaderSubQuery.AddToFilter(AsycudaArrivalHeaderSchema.ATH_ETAAtDischargePort, SQLComparisonOperator.EqualToDatePartOnly, arrivalDate);
				arrivalQuery.AddSubQuery(arrivalHeaderSubQuery, JoinCondition.And);

				return bill.Factory.LoadTop1<ManifestBase.AsycudaArrivalLine>(arrivalQuery);
			}

			return null;
		}

		protected override void AddRowsToPropertiesTable(HtmlTableCreator propertiesTable, FERMessage ferMessage)
		{
			propertiesTable.WriteRow("Message Time", ferMessage.EM_SystemCreateTimeUtc);
			propertiesTable.WriteRow("Air Waybill Number", ferMessage.MAWBNumber);
			propertiesTable.WriteRow("HAWB Number", ferMessage.HAWBNumber);
			propertiesTable.WriteRow("Flight Number", ferMessage.FlightNumber);
			propertiesTable.WriteRow("Arrival Date", ferMessage.CalculatedArrivalDate);
			propertiesTable.WriteRow("Package Tracking Identifier", ferMessage.PackageTrackingIdentifier);
		}

		protected override ZString GetEmailContentPart2(FERMessage ferMessage, EmailDefBuilder emailBuilder, AsycudaBill bill)
		{
			ZString errorsTableHtml = "<P><B>Error List</B></P>";

			var messageErrorsTable = new HtmlTableCreator(new string[] { "Code", "Description" });
			foreach (var error in ferMessage.Errors)
			{
				messageErrorsTable.WriteRow(error.ErrorCode, error.ErrorMessageText);
			}
			errorsTableHtml += messageErrorsTable.ToHtml();

			return errorsTableHtml;
		}
	}
}
