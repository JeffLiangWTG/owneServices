using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Edifact;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DEBADVEDIMessage : SGEDIMessage
	{
		public DEBADVEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (messageInterpretation.IsEmpty)
				{
					messageInterpretation = GetAppropriateTradeNetVersionForDebadv;
				}
				return messageInterpretation;
			}
		}
		ZString messageInterpretation;

		#region Implementation

		ZString GetAppropriateTradeNetVersionForDebadv
		{
			get
			{
				var result = ZString.Empty;

				var debadv09b = (Edifact.D09B.Messages.DEBADV.DEBADVMessage)GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
				if (debadv09b != null && IsTradeNet4_1Message(debadv09b.UNH[0].MessageIdentifier.AssociationAssignedCode))
				{
					result = D09bDebadv(debadv09b);
				}
				else
				{
					var dEBADV = (Edifact.D05B.Messages.DEBADV.DEBADVMessage)GetAutoEdifactMessageUsingNamedFactory(Sg05bEdifactMessageFactory.SG4MessageFactory, new UNOASGCharacterSet());
					if (dEBADV != null)
					{
						result = D05bDebadv(dEBADV);
					}
				}

				return result;
			}
		}

		ZString D05bDebadv(Edifact.D05B.Messages.DEBADV.DEBADVMessage dEBADV)
		{
			var result = "DEBIT ADVICE MESSAGE\r\n";
			ZString issuedDate = dEBADV.DTM[0].DateTimePeriod.DateOrTimeOrPeriodText;
			result += "ISSUED ON " + issuedDate.Left(8) + " AT " + issuedDate.Right(4) + "\r\n";

			foreach (Edifact.D05B.Messages.DEBADV.SegmentGroup1 sg1 in dEBADV.Group1)
			{
				if (sg1.RFF[0].Reference.ReferenceCodeQualifier == "DM")
				{
					result += "LICENCE NO. : " + sg1.RFF[0].Reference.ReferenceIdentifier + "\r\n";
				}
				else if (sg1.RFF[0].Reference.ReferenceCodeQualifier == "ABT")
				{
					result += "PERMIT NO.  : " + sg1.RFF[0].Reference.ReferenceIdentifier + "\r\n";
				}
			}

			result += "FEE         : " + dEBADV.Group3[0].MOA[0].MonetaryAmount.MonetaryAmount + " " + dEBADV.Group3[0].MOA[0].MonetaryAmount.CurrencyIdentificationCode + "\r\n";
			result += "DATE        : " + dEBADV.Group3[0].DTM[0].DateTimePeriod.DateOrTimeOrPeriodText + "\r\n";
			result += "REMARKS     : ";

			if (dEBADV.FTX.Count > 0)
			{
				result += (dEBADV.FTX[0].TextLiteral.FreeText1.Trim() + " " + dEBADV.FTX[0].TextLiteral.FreeText2).Trim();
			}

			return result;
		}

		ZString D09bDebadv(Edifact.D09B.Messages.DEBADV.DEBADVMessage dEBADV)
		{
			var result = "DEBIT ADVICE: FEE MESSAGE\r\n";
			ZString issuedDate = dEBADV.DTM[0].DateTimePeriod.DateOrTimeOrPeriodText;
			result += "ISSUED ON " + FormattedDate(issuedDate.Left(8)) + " AT " + issuedDate.Right(4).SubstringSafe(0, 2) + ":" + issuedDate.Right(4).SubstringSafe(2, 2) + "\r\n";

			foreach (Edifact.D09B.Messages.DEBADV.SegmentGroup1 sg1 in dEBADV.Group1)
			{
				if (sg1.RFF[0].Reference.ReferenceCodeQualifier == "DM")
				{
					result += "LICENCE NO.     : " + sg1.RFF[0].Reference.ReferenceIdentifier + "\r\n";
				}
				if (sg1.RFF[0].Reference.ReferenceCodeQualifier == "ABT")
				{
					result += "PERMIT NO.      : " + sg1.RFF[0].Reference.ReferenceIdentifier + "\r\n";
				}
			}

			result += "FEE             : " + dEBADV.Group3[0].MOA[0].MonetaryAmount.MonetaryAmount + " " + dEBADV.Group3[0].MOA[0].MonetaryAmount.CurrencyIdentificationCode + "\r\n";
			result += "SETTLEMENT DATE : " + FormattedDate(dEBADV.Group3[0].DTM[0].DateTimePeriod.DateOrTimeOrPeriodText) + "\r\n";
			result += "REMARKS         : ";

			if (dEBADV.FTX.Count > 0)
			{
				result += (dEBADV.FTX[0].TextLiteral.FreeText1.Trim() + " " + dEBADV.FTX[0].TextLiteral.FreeText2).Trim();
			}
			return result;
		}

		ZString FormattedDate(ZString messageDate)
		{
			return messageDate.SubstringSafe(6, 2) + "-" + GetMonthAbbrev(messageDate.SubstringSafe(4, 2)) + "-" + messageDate.SubstringSafe(0, 4);
		}

		#endregion
	}
}
