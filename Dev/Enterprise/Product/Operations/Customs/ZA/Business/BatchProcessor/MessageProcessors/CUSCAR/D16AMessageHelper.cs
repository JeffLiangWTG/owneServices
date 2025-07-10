using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D16A;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class D16AMessageHelper : MessageHelper
	{
		public D16AMessageHelper(ZAMessage ediMessage, CUSCARMessage message)
			: base(ediMessage)
		{
			cuscarMessage = Argument.NotNull(message, "message");
			interchangeTime = ZDateTime.Empty;
		}

		readonly CUSCARMessage cuscarMessage;

		public static D16AMessageHelper New(ZAMessage message)
		{
			D16AMessageHelper result = null;
			if (message != null)
			{
				var messageFactory = new D16AMessageFactory();
				var zaCharSet = new ZACharacterSet();
				var ediMessage = message.GetAutoEdifactMessageUsingNamedFactory(messageFactory, zaCharSet);
				if (ediMessage is CUSCARMessage cuscarMessage)
				{
					result = new D16AMessageHelper(message, cuscarMessage);
					result.interchangeTime = message.EM_DateTimeInterchangeSent;
				}
			}
			return result;
		}

		#region Implementation

		public ZString ValidateMessage()
		{
			var invalidOrMissing = new ZStringBuilder();

			if (BGMSegment?.MessageFunctionCode == null)
			{
				invalidOrMissing.Append("BGM");
			}
			else
			{
				if (ManifestType.IsEmpty)
				{
					invalidOrMissing.Append("BGM.DocumentName");
				}
				if (string.IsNullOrEmpty(MessageFunction))
				{
					invalidOrMissing.Append("BGM.MessageFunctionCode");
				}
			}

			if (TDTSegment?.MeansOfTransportJourneyIdentifier == null)
			{
				invalidOrMissing.Append("TDT");
			}
			else
			{
				if (TransportCode.IsEmpty)
				{
					invalidOrMissing.Append("TDT.TransportModeNameCode");
				}
				if (VoyageFlightNo.IsEmpty)
				{
					invalidOrMissing.Append("TDT.MeansOfTransportJourneyIdentifier");
				}
			}

			if (CNISegment?.DocumentMessageDetails.DocumentIdentifier == null)
			{
				invalidOrMissing.Append("CNI");
			}
			else
			{
				if (DocumentNumber.IsEmpty)
				{
					invalidOrMissing.Append("CNI.DocumentIdentifier");
				}
				if (DocumentDate.IsEmpty)
				{
					invalidOrMissing.Append("CNI.VersionIdentifier");
				}
			}

			// manifest will always have 1 bill.
			if (cuscarMessage.Group7[0].Group8.Count == 0)
			{
				invalidOrMissing.Append("Group 8");
			}
			else
			{
				var allBills = GetBills();

				foreach (var bill in allBills)
				{
					if (bill.BillNumber.IsEmpty)
					{
						invalidOrMissing.Append("RFF.ReferenceIdentifier");
					}

					// bill will always have 1 pack.
					if (bill.Group14.Count == 0)
					{
						invalidOrMissing.AppendFormat("Group 14 in Bill '{0}'", bill.BillNumber);
					}
				}
			}

			return invalidOrMissing.IsEmpty ? "" : invalidOrMissing.ToStringWithDelimiterBetweenAppends(", ") + ".";
		}

		// CNISegment

		public ZString DocumentNumber => CNISegment?.DocumentMessageDetails.DocumentIdentifier;

		public ZDate DocumentDate
		{
			get
			{
				ZDateTime result = ZDateTime.Invalid;

				var dateTimePeriod = CNISegment?.DocumentMessageDetails.VersionIdentifier;
				ZDateTime.TryParseExact(dateTimePeriod, out result, "yyyyMMdd");

				return result.Date;
			}
		}

		CNISegment CNISegment => cniSegment ?? (cniSegment = cuscarMessage.Group7[0].CNI[0]);
		CNISegment cniSegment;

		// BGMSegment

		public ZString UniqueReferenceNumber => BGMSegment?.DocumentMessageIdentification.DocumentIdentifier ?? ZString.Empty;

		public ZString ManifestType => BGMSegment?.DocumentMessageName.DocumentName;

		public MessageFunctionCodeList MessageFunction => BGMSegment?.MessageFunctionCode;

		public ZString MessageType => messageFunctionCode ?? (messageFunctionCode = MessageFunction.ToString());
		string messageFunctionCode;

		public ZString MessageFunctionDescription => Factory.GetCachedValue<Universal.MessageFunctionCodeList>().GetDescriptionFromCode(MessageType);

		BGMSegment BGMSegment => bgmSegment ?? (bgmSegment = cuscarMessage.BGM[0]);
		BGMSegment bgmSegment;

		// TDTSegment

		public ZString VoyageFlightNo => TDTSegment?.MeansOfTransportJourneyIdentifier ?? ZString.Empty;

		public ZString CarrierCode => TDTSegment?.Carrier.CarrierIdentifier ?? ZString.Empty;

		public ZString TransportCode => TDTSegment?.ModeOfTransport.TransportModeNameCode ?? ZString.Empty;

		public ZString RadioCallSign => TDTSegment?.TransportIdentification.TransportMeansIdentificationNameIdentifier ?? ZString.Empty;

		TDTSegment TDTSegment => grp4TDTSegment ?? (grp4TDTSegment = cuscarMessage.Group4[0].TDT[0]);
		TDTSegment grp4TDTSegment;

		public ZString MessageSender => cuscarMessage.Group2.Cast<SegmentGroup2>()
			.SelectMany(x => x.NAD.Cast<NADSegment>())
			.FirstOrDefault(x => x.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.DocumentMessageIssuerSender)
			?.PartyIdentificationDetails.PartyIdentifier;

		public ZString MasterTransportDocumentNumber => cuscarMessage.Group7[0].CNI[0].DocumentMessageDetails.DocumentIdentifier;

		// collections

		public D16AMessageContainerDetail[] GetContainers()
		{
			if (containers == null)
			{
				var qualifierList = new[] { EquipmentTypeCodeQualifierList.Container, EquipmentTypeCodeQualifierList.RailCar };
				containers = cuscarMessage.Group5.Cast<SegmentGroup5>()
												 .Where(s5 => s5.EQD[0].EquipmentTypeCodeQualifier.In(qualifierList))
												 .Select(s5 => new D16AMessageContainerDetail(s5))
												 .ToArray();
			}
			return containers;
		}
		D16AMessageContainerDetail[] containers;

		public D16AMessageHouseBillDetail[] GetBills()
		{
			if (bills == null)
			{
				bills = cuscarMessage.Group7[0].Group8
									 .Cast<SegmentGroup8>()
									 .Where(s8 => s8.RFF.Cast<RFFSegment>().Any(rff => rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.BillOfLadingNumber))
									 .Select(s8 => new D16AMessageHouseBillDetail(s8))
									 .ToArray();
			}
			return bills;
		}
		D16AMessageHouseBillDetail[] bills;

		#endregion
	}
}
