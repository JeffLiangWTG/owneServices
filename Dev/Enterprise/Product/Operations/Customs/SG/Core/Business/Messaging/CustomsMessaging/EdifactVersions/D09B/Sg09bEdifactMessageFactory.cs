using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	class Sg09bEdifactMessageFactory : IMessageFactory
	{
		static MessageFactory fSG41MessageFactory;
		public static MessageFactory SG41MessageFactory
		{
			get
			{
				if (fSG41MessageFactory == null)
				{
					fSG41MessageFactory = new MessageFactory(
							new Edifact.D09B.EdifactD09BMessageFactory());
				}
				return fSG41MessageFactory;
			}
		}

		public MessageType MessageType => MessageType.EDIFact;

		ICusMessage IMessageFactory.GetOriginalMessage(ICustomsDec cusEntryHeader)
		{
			ICusMessage result = null;

			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					result = new Inpdec09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.InPayment:
					result = new Iptdec09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Outward:
					result = new Outdec09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					result = new OutdecWithCo09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Transhipment:
					result = new Tnpdec09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.CertificateOfOrigin:
					result = new Tcodec09b(cusEntryHeader);
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
					result = new Inpupd09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.InPayment:
					result = new Iptupd09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Outward:
					result = new Outupd09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					result = new OutupdWithCo09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Transhipment:
					result = new Tnpupd09b(cusEntryHeader);
					break;
			}

			return result;
		}

		ICusMessage IMessageFactory.GetRefundMessage(ICustomsDec cusEntryHeader)
		{
			return cusEntryHeader.EntryType == CusEntryHeader.EntryTypes.InPayment ? new Iptupd09bRefund(cusEntryHeader) : null;
		}

		ICusMessage IMessageFactory.GetCancellationMessage(ICustomsDec cusEntryHeader)
		{
			ICusMessage result = null;

			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					result = new Inpupd09bCancel(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.InPayment:
					result = new Iptupd09bCancel(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Outward:
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					result = new OutupdCancel09b(cusEntryHeader);
					break;
				case CusEntryHeader.EntryTypes.Transhipment:
					result = new Tnpupd09bCancel(cusEntryHeader);
					break;
			}

			return result;
		}
	}
}
