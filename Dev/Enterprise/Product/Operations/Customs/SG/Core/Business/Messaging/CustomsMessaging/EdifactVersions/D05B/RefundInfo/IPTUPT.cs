using System;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSPMT;
using Enterprise.Edifact.D05B.Segments;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo
{
	public class IPTUPT : CUSPMT, IRefundInfo
	{
		public IPTUPT()
			: base()
		{
		}

		#region IRefundInfo Members

		public ZString ReplacementNumber
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup1 gR1 in Group1)
				{
					if (gR1.RFF[0].Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.GoodsDeclarationNumber)
					{
						result = gR1.RFF[0].Reference.ReferenceIdentifier;
					}
				}
				return result;
			}
		}

		public new ZString NameOfCompany
		{
			get
			{
				ZString result;
				DeclarationTypeCodeList declarationTypeList = new DeclarationTypeCodeList();
				result = base.NameOfCompany;
				if (result == "" && DeclarationType == declarationTypeList.GetDescriptionFromCode(DeclarationTypeCodeList.Codes.BKO).ToUpper())
				{
					result = Exporter;
				}
				if (result == "" && DeclarationType != declarationTypeList.GetDescriptionFromCode(DeclarationTypeCodeList.Codes.BKO).ToUpper())
				{
					result = Importer;
				}
				return result;
			}
		}

		public ZString EntityIdentifier
		{
			get
			{
				ZString result;
				DeclarationTypeCodeList declarationTypeList = new DeclarationTypeCodeList();
				result = base.EntityIdentOfCompany;
				if (result == "" && DeclarationType == declarationTypeList.GetDescriptionFromCode(DeclarationTypeCodeList.Codes.BKO).ToUpper())
				{
					foreach (SegmentGroup6 gR6 in Group6)
					{
						foreach (NADSegment nad in gR6.NAD)
						{
							if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Exporter)
							{
								result = nad.PartyIdentificationDetails.PartyIdentifier;
							}
						}
					}
				}
				if (result == "" && DeclarationType != declarationTypeList.GetDescriptionFromCode(DeclarationTypeCodeList.Codes.BKO).ToUpper())
				{
					foreach (SegmentGroup6 gR6 in Group6)
					{
						foreach (NADSegment nad in gR6.NAD)
						{
							if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Importer)
							{
								result = nad.PartyIdentificationDetails.PartyIdentifier;
							}
						}
					}
				}
				return result;
			}
		}

		public ZString TelNo
		{
			get { return base.TelNb; }
		}

		public ZDate DateOfApproval
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup1 gR1 in Group1)
				{
					foreach (DTMSegment dtm in gR1.DTM)
					{
						if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.AuthorizationDate)
						{
							result = dtm.DateTimePeriod.DateOrTimeOrPeriodText;
						}
					}
				}
				if (result != "")
				{
					return new ZDate(Convert.ToInt32(result.Substring(0, 4)), Convert.ToInt32(result.Substring(4, 2)), Convert.ToInt32(result.Substring(6, 2)));
				}
				else
				{
					return new ZDate();
				}
			}
		}

		public ICConditions[] ReasonForRefund
		{
			get
			{
				int total = 0;
				IPTUPTReasonForRefund[] result = null;
				foreach (FTXSegment ftx in FTX)
				{
					if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.DiscrepancyInformation || ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.Reason)
					{
						total++;
					}
				}
				if (total > 0)
				{
					result = new IPTUPTReasonForRefund[total];
					for (int i = 0; i < FTX.Count; i++)
					{
						if (FTX[i].TextSubjectCodeQualifier == TextSubjectCodeQualifierList.DiscrepancyInformation)
						{
							result.SetValue(new IPTUPTReasonForRefund(FTX[i], true), i);
							//result = Ftx.TextLiteral.FreeText1 + " " + Ftx.TextLiteral.FreeText2 + " " + Ftx.TextLiteral.FreeText3 + " " + Ftx.TextLiteral.FreeText4 + "\n";
						}
						if (FTX[i].TextSubjectCodeQualifier == TextSubjectCodeQualifierList.Reason)
						{
							result.SetValue(new IPTUPTReasonForRefund(FTX[i], false), i);
						}
					}
				}
				return result;
			}
		}

		public ICConditions[] RefundMessage
		{
			get
			{
				int total = 0;
				IPTUPTRefundMessage[] result = null;
				foreach (SegmentGroup1 gR1 in Group1)
				{
					foreach (FTXSegment ftx in gR1.FTX)
					{
						if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.ReimbursementInstructions)
						{
							total++;
						}
					}
				}
				if (total > 0)
				{
					result = new IPTUPTRefundMessage[total];
					foreach (SegmentGroup1 gR1 in Group1)
					{
						for (int i = 0; i < gR1.FTX.Count; i++)
						{
							if (gR1.FTX[i].TextSubjectCodeQualifier == TextSubjectCodeQualifierList.ReimbursementInstructions)
							{
								result.SetValue(new IPTUPTRefundMessage(gR1.FTX[i]), i);
							}
						}
					}
				}
				return result;
			}
		}

		public new IRefundInfoConsignment[] ConsignmentDetails
		{
			get
			{
				IPTUPTRefundInfoConsignment[] result = new IPTUPTRefundInfoConsignment[Group30.Count];
				for (int i = 0; i < Group30.Count; i++)
				{
					result.SetValue(new IPTUPTRefundInfoConsignment(Group30[i]), i);
				}
				return result;
			}
		}

		#endregion

		public ZDecimal TotalGoodsAndServicesTaxRefundAmount => 0m;

		public ZDecimal TotalExciseDutyRefundAmount => 0m;

		public ZDecimal TotalCustomsDutyRefundAmount => 0m;

		public ZDecimal TotalOtherTaxRefundAmount => 0m;
	}

	public class IPTUPTRefundInfoConsignment : IRefundInfoConsignment
	{
		public IPTUPTRefundInfoConsignment(SegmentGroup30 gr30)
		{
			gR30 = gr30;
		}

		protected SegmentGroup30 gR30;

		#region IRefundInfoConsignment Members

		public ZString SerialNb
		{
			get { return gR30.CST[0].GoodsItemNumber; }
		}

		public ZString HSCode
		{
			get { return gR30.CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier; }
		}

		public ZDecimal DutyAmountPayable
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup41 gR41 in gR30.Group41)
				{
					if (gR41.TAX[0].DutyOrTaxOrFeeFunctionCodeQualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty && gR41.TAX[0].DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode == DutyOrTaxOrFeeTypeNameCodeList.CustomsDuty)
					{
						foreach (MOASegment moa in gR41.MOA)
						{
							if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.Refund)
							{
								result = moa.MonetaryAmount.MonetaryAmount;
							}
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal ExciseAmountPayable
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup41 gR41 in gR30.Group41)
				{
					if (gR41.TAX[0].DutyOrTaxOrFeeFunctionCodeQualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty && gR41.TAX[0].DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode == DutyOrTaxOrFeeTypeNameCodeList.ExciseDuty)
					{
						foreach (MOASegment moa in gR41.MOA)
						{
							if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.Refund)
							{
								result = moa.MonetaryAmount.MonetaryAmount;
							}
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal GstAmount
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup41 gR41 in gR30.Group41)
				{
					if (gR41.TAX[0].DutyOrTaxOrFeeFunctionCodeQualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty && gR41.TAX[0].DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode == DutyOrTaxOrFeeTypeNameCodeList.GoodsAndServicesTax)
					{
						foreach (MOASegment moa in gR41.MOA)
						{
							if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.Refund)
							{
								result = moa.MonetaryAmount.MonetaryAmount;
							}
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		#endregion
	}

	public class IPTUPTReasonForRefund : ICConditions
	{
		public IPTUPTReasonForRefund(FTXSegment message, ZBool isOtherReason)
		{
			msg = message;
			this.isOtherReason = isOtherReason;
		}

		readonly FTXSegment msg;
		readonly ZBool isOtherReason;

		#region ICConditions Members

		public ZString Code
		{
			get
			{
				ZString result;
				if (!isOtherReason)
				{
					result = msg.TextLiteral.FreeText1;
				}
				else
				{
					result = ReasonForRefundCodeList.Codes.RF35;
				}
				return result;
			}
		}

		public ZString Message
		{
			get
			{
				ZString result;
				if (!isOtherReason)
				{
					ReasonForRefundCodeList list = new ReasonForRefundCodeList();
					result = list.GetDescriptionFromCode(Code);
				}
				else
				{
					result = msg.TextLiteral.FreeText1 + " " + msg.TextLiteral.FreeText2 + " " + msg.TextLiteral.FreeText3 + " " + msg.TextLiteral.FreeText4;
				}
				return result;
			}
		}

		#endregion
	}

	public class IPTUPTRefundMessage : ICConditions
	{
		public IPTUPTRefundMessage(FTXSegment message)
		{
			msg = message;
		}

		readonly FTXSegment msg;

		#region ICConditions Members

		public ZString Code
		{
			get { return msg.TextLiteral.FreeText1.Substring(0, 4); }
		}

		public ZString Message
		{
			get { return msg.TextLiteral.FreeText1.Substring(4); }
		}

		#endregion
	}
}
