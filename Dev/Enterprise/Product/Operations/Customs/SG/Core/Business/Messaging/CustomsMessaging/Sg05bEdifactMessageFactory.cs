using Enterprise.Customs.SG.Business.CustomsMessaging;
using Enterprise.Edifact;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class Sg05bEdifactMessageFactory : IMessageFactory
	{
		static MessageFactory fSG4MessageFactory;
		public static MessageFactory SG4MessageFactory
		{
			get
			{
				if (fSG4MessageFactory == null)
				{
					fSG4MessageFactory = new MessageFactory(
							new Edifact.D05B.EdifactD05BMessageFactory());
				}
				return fSG4MessageFactory;
			}
		}

		public MessageType MessageType => MessageType.EDIFact;

		ICusMessage IMessageFactory.GetOriginalMessage(ICustomsDec cusEntryHeader)
		{
			ICusMessage result = null;

			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					result = new INPDEC(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.InPayment:
					result = new IPTDEC(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Outward:
					result = new OUTDEC(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					result = new OUTDECWithCO(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Transhipment:
					result = new TNPDEC(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.CertificateOfOrigin:
					result = new TCODEC(cusEntryHeader);
					break;
			}

			return result;
		}

		ICusMessage IMessageFactory.GetAmendmentMessage(ICustomsDec cusEntryHeader)
		{
			ICusMessage result = null;

			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					result = new INPUPD(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.InPayment:
					result = new IPTUPD(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Outward:
					result = new OUTUPD(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					result = new OUTUPDWithCO(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Transhipment:
					result = new TNPUPD(cusEntryHeader);
					break;
			}

			return result;
		}

		ICusMessage IMessageFactory.GetRefundMessage(ICustomsDec cusEntryHeader)
		{
			ICusMessage result = null;

			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InPayment:
					result = new IPTUPDRefund(cusEntryHeader);
					break;
			}

			return result;
		}

		ICusMessage IMessageFactory.GetCancellationMessage(ICustomsDec cusEntryHeader)
		{
			ICusMessage result = null;

			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					result = new INPUPDCancel(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.InPayment:
					result = new IPTUPDCancel(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Outward:
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					result = new OUTUPDCancel(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Transhipment:
					result = new TNPUPDCancel(cusEntryHeader);
					break;
			}

			return result;
		}
	}
}
