using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	sealed class TradenetDeclarationMessageFactory : TradeNetMessageFactory<TradenetDeclaration>
	{
		public override TradenetDeclaration CreateTradeNetMessageParent()
		{
			return new TradenetDeclaration()
			{
				SenderID = SGXmlEDIMessage.SendersReferencePlaceHolderXml,
				RecipientID = SGXmlEDIMessage.RecipientReferencePlaceHolderXml,
				MessageVersion = MessageVersion,
				TotalNumberOfDeclaration = 1,
				TotalNumberOfDeclarationSpecified = true,
				dateTime = ZDateTime.Now.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
				instanceIdentifier = SGXmlEDIMessage.InterchangeNumberPlaceHolderXml
			};
		}

		#region IMessageFactory

		protected override ICusMessage GetOriginalMessageCore(ICustomsDec cusEntryHeader)
		{
			ITradeNetMessage message;
			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					{
						message = new INPDEC(cusEntryHeader);
						break;
					}
				case CusEntryHeader.EntryTypes.InPayment:
					{
						message = new IPTDEC(cusEntryHeader);
						break;
					}

				case CusEntryHeader.EntryTypes.Transhipment:
					{
						message = new TNPDEC(cusEntryHeader);
						break;
					}

				case CusEntryHeader.EntryTypes.Outward:
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					{
						message = new OUTDEC(cusEntryHeader);
						break;
					}

				case CusEntryHeader.EntryTypes.CertificateOfOrigin:
					{
						message = new COODEC(cusEntryHeader);
						break;
					}

				default:
					{
						throw new NotSupportedException(GetNotSupportedMessage(cusEntryHeader));
					}
			}

			BuildMessage(message);
			return message;
		}

		protected override ICusMessage GetAmendmentMessageCore(ICustomsDec cusEntryHeader)
		{
			ITradeNetMessage message;
			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					{
						message = new INPUPD(cusEntryHeader);
						break;
					}
				case CusEntryHeader.EntryTypes.InPayment:
					{
						message = new IPTUPD(cusEntryHeader);
						break;
					}

				case CusEntryHeader.EntryTypes.Transhipment:
					{
						message = new TNPUPD(cusEntryHeader);
						break;
					}

				case CusEntryHeader.EntryTypes.Outward:
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					{
						message = new OUTUPD(cusEntryHeader);
						break;
					}

				default:
					{
						throw new NotSupportedException(GetNotSupportedMessage(cusEntryHeader));
					}
			}

			BuildMessage(message);
			return message;
		}

		protected override ICusMessage GetRefundMessageCore(ICustomsDec cusEntryHeader)
		{
			ITradeNetMessage message;

			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					{
						message = null;
						break;
					}
				case CusEntryHeader.EntryTypes.InPayment:
					{
						message = new IPTUPDRefund(cusEntryHeader);
						break;
					}

				default:
					{
						throw new NotSupportedException(GetNotSupportedMessage(cusEntryHeader));
					}
			}

			BuildMessage(message);
			return message;
		}

		protected override ICusMessage GetCancellationMessageCore(ICustomsDec cusEntryHeader)
		{
			ITradeNetMessage message;
			switch (cusEntryHeader.EntryType)
			{
				case CusEntryHeader.EntryTypes.InNonPayment:
					{
						message = new INPUPDCancel(cusEntryHeader);
						break;
					}
				case CusEntryHeader.EntryTypes.InPayment:
					{
						message = new IPTUPDCancel(cusEntryHeader);
						break;
					}

				case CusEntryHeader.EntryTypes.Transhipment:
					{
						message = new TNPUPDCancel(cusEntryHeader);
						break;
					}

				case CusEntryHeader.EntryTypes.Outward:
				case CusEntryHeader.EntryTypes.OutwardWithCO:
					{
						message = new OUTUPDCancel(cusEntryHeader);
						break;
					}

				default:
					{
						throw new NotSupportedException(GetNotSupportedMessage(cusEntryHeader));
					}
			}

			BuildMessage(message);
			return message;
		}

		string GetNotSupportedMessage(ICustomsDec cusEntryHeader) => Res.GetString("393A66BB-85FC-4D2E-BB3D-051DE47972CF", "{0} is not supported yet.", cusEntryHeader.EntryType);

		void BuildMessage(ITradeNetMessage message)
		{
			if (message != null)
			{
				var messageParent = CreateTradeNetMessageParent();
				message.Build(messageParent);

				var messageText = GetMessageContent(messageParent);
				message.SetMessageText(messageText);
			}
		}

		#endregion
	}
}
