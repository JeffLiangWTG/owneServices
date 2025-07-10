using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public abstract class FZMessageBuilder
	{
		protected FZMessageBuilder(IFZEventHeader header)
		{
			this.Header = header;
		}
		readonly protected IFZEventHeader Header;

		public MQEDIMessage PopulateMessage()
		{
			var block = new ACEInputBlockControlGenerator(Header);
			block.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			block.AddMessageBlocks(Build());

			return block.CreateMessage<MQEDIMessage>(((Messaging.Business.IMessageAttachee)Header).Factory);
		}

		protected abstract IEnumerable<MessageBlock> Build();

		protected FTZFZ10 GenerateFZ10WithMandatoryFields(ZInt actionQualifier, ZString identificationNumber, ZString actionCode)
		{
			var fz10 = new FTZFZ10();
			fz10.ActionQualifier = actionQualifier;
			fz10.IdentificationNumber = identificationNumber;
			fz10.ActionCode = actionCode;
			fz10.AirportCode = Header.AirportCode;
			return fz10;
		}

		protected virtual FTZFZ10 GenerateFZ10ForSplitDetails(Func<FTZFZ10> generateFZ10, IConveyanceOrSplitDetails splitDetails) => generateFZ10();

		protected IEnumerable<MessageBlock> GenerateFZ10AndRelatedBlocks(IFZEventBill bill, ZString admissionNumber, Func<FTZFZ10> generateFZ10, Func<FTZFZ12> generateF12Block, ZString remarks)
		{
			if (bill.AirShipmentDetails.Any())
			{
				foreach (IConveyanceOrSplitDetails splitDetails in bill.AirShipmentDetails)
				{
					yield return GenerateFZ10ForSplitDetails(generateFZ10, splitDetails);
					yield return GenerateFZ11(admissionNumber, splitDetails.CarrierCode, splitDetails.FlightNumber, splitDetails.ArrivalDate.Date);

					var fz12Block = generateF12Block?.Invoke();
					if (fz12Block != null)
					{
						yield return fz12Block;
					}

					foreach (var fz20 in GenerateFZ20s(remarks))
					{
						yield return fz20;
					}
				}
			}
			else
			{
				yield return generateFZ10();
				if (!admissionNumber.IsEmpty)
				{
					yield return GenerateFZ11(admissionNumber, ZString.Empty, ZString.Empty, ZDate.Empty);
				}

				var fz12Block = generateF12Block?.Invoke();
				if (fz12Block != null)
				{
					yield return fz12Block;
				}

				foreach (var fz20 in GenerateFZ20s(remarks))
				{
					yield return fz20;
				}
			}
		}

		FTZFZ11 GenerateFZ11(ZString admissionNumber, ZString carrierCode, ZString flightNumber, ZDate arrivalDate)
		{
			var fz11 = new FTZFZ11();
			fz11.AdmissionNumber = admissionNumber;
			fz11.AirlineCarrierCodeOfImportingCarrier = carrierCode;
			fz11.FlightNumber = flightNumber;
			fz11.ScheduledDateOfArrival = arrivalDate;
			return fz11;
		}

		protected FTZFZ13 GenerateFZ13(ZString contactName, ZString contactPhone, ZString reasonCode)
		{
			var fz13 = new FTZFZ13();
			fz13.ContactName = contactName;
			fz13.ContactPhone = contactPhone;
			fz13.ReasonCode = reasonCode;
			return fz13;
		}

		protected FTZFZ14 GenerateFZ14(ZString referenceQualifier, ZString referenceID)
		{
			var fz14 = new FTZFZ14();
			fz14.ReferenceQualifier = referenceQualifier;
			fz14.ReferenceID = referenceID;
			return fz14;
		}

		protected IEnumerable<FTZFZ20> GenerateFZ20s(ZString remarks)
		{
			var description = remarks.Trim();
			int countOf20 = 0;
			foreach (var comment in description.Split(60))
			{
				if (countOf20 > 10)
				{
					break;
				}

				countOf20++;
				var fz20 = new FTZFZ20();
				fz20.Remarks = comment.SubstringSafe(0, 30);
				fz20.Remarks1 = comment.SubstringSafe(30);
				yield return fz20;
			}
		}
	}
}
