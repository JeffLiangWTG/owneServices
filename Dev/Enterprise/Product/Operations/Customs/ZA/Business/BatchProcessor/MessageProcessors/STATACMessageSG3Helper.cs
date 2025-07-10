using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.STATAC;
using Enterprise.Edifact.D96B.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class STATACMessageSG3Helper : NonPersistentBusinessObject
	{
		public static class Constants
		{
			public const string HeaderLevelIndicatorForDetailMessage = "H";
			public const string PaymentType = "P";
			public const string OtherType = "O";
		}

		public STATACMessageSG3Helper(SegmentGroup3 sg3)
		{
			group3 = sg3;
		}

		readonly SegmentGroup3 group3;

		#region Implementation

		public ZDateTime TransactionDate => SG3Date(DateTimePeriodQualifierList.AccountingTransactionDate);

		public ZString TransactionDateString => DateString(DateTimePeriodQualifierList.AccountingTransactionDate);

		public ZDateTime DueDate => SG3Date(DateTimePeriodQualifierList.PaymentDueDate);

		ZDateTime SG3Date(DateTimePeriodQualifierList list)
		{
			var result = ZDateTime.Empty;
			ZDateTime.TryParseExact(DateString(list), out result, "yyyyMMdd");
			return result;
		}

		ZString DateString(DateTimePeriodQualifierList list)
		{
			var result = ZString.Empty;
			foreach (DTMSegment dtm in group3.DTM)
			{
				if (dtm.DateTimePeriod.DateTimePeriodQualifier == list
					&&
					dtm.DateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmdd)
				{
					result = dtm.DateTimePeriod.DateTimePeriod.Trim();
					break;
				}
			}
			return result;
		}

		public ZString Level
		{
			get
			{
				var result = ZString.Empty;
				if (DocSegment != null)
				{
					result = new ZString(DocSegment.DocumentMessageName.DocumentMessageName).SubstringSafe(0, 1);
				}
				return result;
			}
		}

		public ZString Type
		{
			get
			{
				var result = ZString.Empty;
				if (DocSegment != null)
				{
					result = new ZString(DocSegment.DocumentMessageName.DocumentMessageName).SubstringSafe(1, 1);
				}
				return result;
			}
		}

		DOCSegment DocSegment
		{
			get
			{
				if (fDocSegment == null)
				{
					fDocSegment = group3.DOC[0].DocumentMessageName.DocumentMessageNameCoded == DocumentMessageNameCodedList.CustomsDeclarationWithCommercialAndItemDetail ? group3.DOC[0] : null;
				}
				return fDocSegment;
			}
		}

		DOCSegment fDocSegment;

		public ZString TransactionReference => DocSegment?.DocumentMessageDetails.DocumentMessageNumber ?? ZString.Empty;

		public ZString TransactionDescription => DocSegment?.DocumentMessageDetails.DocumentMessageSource ?? ZString.Empty;

		public ZDecimal Amount
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (MOASegment moa in group3.MOA)
				{
					if (moa.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.AmountDueAmountPayable)
					{
						result = new ZDecimal(moa.MonetaryAmount.MonetaryAmount);
						break;
					}
				}
				return result;
			}
		}

		#endregion
	}
}
