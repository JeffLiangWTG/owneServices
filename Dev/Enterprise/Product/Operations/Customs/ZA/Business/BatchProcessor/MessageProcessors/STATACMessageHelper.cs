using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.STATAC;
using D96BMessageFactory = Enterprise.Edifact.D96B.EdifactD96BMessageFactory;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class STATACMessageHelper : MessageHelper
	{
		public static class Constants
		{
			public const string Detail = "DETAIL";
		}

		public STATACMessageHelper(ZAMessage ediMessage, STATACMessage message)
			: base(ediMessage)
		{
			statacMessage = Argument.NotNull(message, "message");
		}

		public readonly STATACMessage statacMessage;

		public static STATACMessageHelper New(ZAMessage message)
		{
			STATACMessageHelper result = null;
			if (message != null)
			{
				var d96bMessageFactory = new D96BMessageFactory();
				var zaCharSet = new ZACharacterSet();
				var statacMessage = message.GetAutoEdifactMessageUsingNamedFactory(d96bMessageFactory, zaCharSet) as STATACMessage;
				if (statacMessage != null)
				{
					result = new STATACMessageHelper(message, statacMessage);
					result.interchangeTime = message.EM_DateTimeInterchangeSent;
				}
			}
			return result;
		}

		#region Implementation

		public ZString DailyOrDetail
		{
			get
			{
				if (dailyOrDetail == null)
				{
					dailyOrDetail = statacMessage.BGM[0].DocumentMessageName.DocumentMessageName;
				}
				return dailyOrDetail;
			}
		}
		string dailyOrDetail;

		public ZString FinancialAccountNumber
		{
			get
			{
				if (financialAccountNumber == null)
				{
					var rff = statacMessage.RFF[0];

					if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.AccountNumber)
					{
						financialAccountNumber = rff.Reference.ReferenceNumber;
					}
				}
				return financialAccountNumber;
			}
		}
		string financialAccountNumber;

		public bool IsDetailMessage
		{
			get { return DailyOrDetail == Constants.Detail; }
		}

		public ZString CustomsOffice
		{
			get
			{
				if (customsOffice == null)
				{
					customsOffice = PartyIdentification(PartyQualifierList.Customs);
				}

				return customsOffice;
			}
		}
		string customsOffice;

		public ZString AgentCode
		{
			get
			{
				if (agentCode == null)
				{
					agentCode = PartyIdentification(PartyQualifierList.AgentRepresentative);
					if (agentCode?.Length > 8)
					{
						agentCode = agentCode.Substring(0, 8);
					}
				}

				return agentCode;
			}
		}
		string agentCode;

		string PartyIdentification(PartyQualifierList partyQualifier)
		{
			foreach (SegmentGroup1 grp1 in statacMessage.Group1)
			{
				if (grp1.NAD[0].PartyQualifier == partyQualifier)
				{
					return grp1.NAD[0].PartyIdentificationDetails.PartyIdIdentification;
				}
			}

			return null;
		}

		#endregion
	}
}
