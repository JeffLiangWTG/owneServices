using System.Data;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Edifact;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CUSPMTEDIMessage : SGEDIMessage
	{
		public CUSPMTEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Edifact.Auto.SegmentGroup EdifactMessage
		{
			get
			{
				if (edifactMessage == null)
				{
					var cuspmt09b = (Edifact.D09B.Messages.CUSPMT.CUSPMTMessage)GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
					if (cuspmt09b != null && cuspmt09b.UNH[0].MessageIdentifier.AssociationAssignedCode == SGConstants.TradeNetVersion.AssociationAssignedCodes.FourPointOne)
					{
						edifactMessage = cuspmt09b;
					}
					else
					{
						edifactMessage = (Edifact.D05B.Messages.CUSPMT.CUSPMTMessage)GetAutoEdifactMessageUsingNamedFactory(Sg05bEdifactMessageFactory.SG4MessageFactory, new UNOASGCharacterSet());
					}
				}

				return edifactMessage;
			}
		}
		Edifact.Auto.SegmentGroup edifactMessage;

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (messageInterpretation.IsEmpty)
				{
					if (EdifactMessage is Edifact.D09B.Messages.CUSPMT.CUSPMTMessage cuspmt09b)
					{
						messageInterpretation = InterpretD09bCUSPMT(cuspmt09b);
					}
					else
					{
						messageInterpretation = InterpretD05bCUSPMT((Edifact.D05B.Messages.CUSPMT.CUSPMTMessage)EdifactMessage);
					}
				}
				return messageInterpretation;
			}
		}
		ZString messageInterpretation;

		#region Implementation

		ZString InterpretD05bCUSPMT(Edifact.D05B.Messages.CUSPMT.CUSPMTMessage cUSPMT)
		{
			var builder = new ZStringBuilder("PERMIT APPLICATION APPROVED" + System.Environment.NewLine + System.Environment.NewLine);

			foreach (Edifact.D05B.Messages.CUSPMT.SegmentGroup1 group1 in cUSPMT.Group1)
			{
				foreach (Edifact.D05B.Segments.RFFSegment rFF in group1.RFF)
				{
					if (rFF.Reference.ReferenceCodeQualifier == Enterprise.Edifact.D05B.Elements.ReferenceCodeQualifierList.CustomsDeclarationNumber)
					{
						builder.AppendLine(string.Concat("PERMIT NUMBER : ", rFF.Reference.ReferenceIdentifier, System.Environment.NewLine));

						foreach (Edifact.D05B.Segments.DTMSegment dTM in group1.DTM)
						{
							if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == Enterprise.Edifact.D05B.Elements.DateOrTimeOrPeriodFunctionCodeQualifierList.AuthorizationDate)
							{
								builder.AppendLine("AUTHORISATION DATE : " + dTM.DateTimePeriod.DateOrTimeOrPeriodText);
							}
							else if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == Enterprise.Edifact.D05B.Elements.DateOrTimeOrPeriodFunctionCodeQualifierList.ValidityPeriod)
							{
								ZString validityPeriod = dTM.DateTimePeriod.DateOrTimeOrPeriodText;
								ZString permitValidFrom = validityPeriod.SubstringSafe(0, 8);
								ZString permitValidTo = validityPeriod.SubstringSafe(8, 8);
								builder.Append(string.Concat("VALIDITY FROM ", permitValidFrom, " TO ", permitValidTo));
							}
						}
					}
				}

				foreach (Edifact.D05B.Segments.FTXSegment fTX in group1.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == Enterprise.Edifact.D05B.Elements.TextSubjectCodeQualifierList.CustomsClearanceInstructions
						|| fTX.TextSubjectCodeQualifier == Enterprise.Edifact.D05B.Elements.TextSubjectCodeQualifierList.RegulatoryInformation)
					{
						var textLiteral = string.Join(System.Environment.NewLine, fTX.TextLiteral.FreeText1, fTX.TextLiteral.FreeText2, fTX.TextLiteral.FreeText3, fTX.TextLiteral.FreeText4, fTX.TextLiteral.FreeText5).Trim();
						builder.Append(System.Environment.NewLine + System.Environment.NewLine);
						builder.Append(textLiteral);
					}
				}
			}

			return builder.ToString();
		}

		ZString InterpretD09bCUSPMT(Edifact.D09B.Messages.CUSPMT.CUSPMTMessage cuspmt09b)
		{
			var builder = new ZStringBuilder("PERMIT APPLICATION APPROVED" + System.Environment.NewLine + System.Environment.NewLine);

			foreach (Edifact.D09B.Messages.CUSPMT.SegmentGroup1 group1 in cuspmt09b.Group1)
			{
				foreach (Edifact.D09B.Segments.RFFSegment rFF in group1.RFF)
				{
					if (rFF.Reference.ReferenceCodeQualifier == Enterprise.Edifact.D09B.Elements.ReferenceCodeQualifierList.GoodsDeclarationDocumentIdentifierCustoms)
					{
						builder.AppendLine(string.Concat("PERMIT NUMBER : ", rFF.Reference.ReferenceIdentifier, System.Environment.NewLine));

						foreach (Edifact.D09B.Segments.DTMSegment dTM in group1.DTM)
						{
							if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == Enterprise.Edifact.D09B.Elements.DateOrTimeOrPeriodFunctionCodeQualifierList.GoodsDeclarationDocumentAcceptanceDateTime)
							{
								builder.AppendLine("AUTHORISATION DATE : " + FormattedDateTime(dTM.DateTimePeriod.DateOrTimeOrPeriodText));
							}
							else if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == Enterprise.Edifact.D09B.Elements.DateOrTimeOrPeriodFunctionCodeQualifierList.ValidityPeriod)
							{
								// eg: 2011021420110222
								ZString validityPeriod = dTM.DateTimePeriod.DateOrTimeOrPeriodText;
								var permitValidFrom = string.Concat(validityPeriod.SubstringSafe(6, 2), "-", GetMonthAbbrev(validityPeriod.SubstringSafe(4, 2)), "-", validityPeriod.SubstringSafe(0, 4));
								var permitValidTo = string.Concat(validityPeriod.SubstringSafe(14, 2), "-", GetMonthAbbrev(validityPeriod.SubstringSafe(12, 2)), "-", validityPeriod.SubstringSafe(8, 4));
								builder.AppendLine(string.Concat("VALIDITY FROM ", permitValidFrom, " TO ", permitValidTo));
							}
						}
					}
					else if (rFF.Reference.ReferenceCodeQualifier == Enterprise.Edifact.D09B.Elements.ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber)
					{
						foreach (Edifact.D09B.Segments.DTMSegment dTM in group1.DTM)
						{
							if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == Enterprise.Edifact.D09B.Elements.DateOrTimeOrPeriodFunctionCodeQualifierList.ApprovalDate)
							{
								builder.AppendLine("APPROVAL DATE (CA) : " + FormattedDateTime(dTM.DateTimePeriod.DateOrTimeOrPeriodText));
							}
						}
					}
				}

				foreach (Edifact.D09B.Segments.FTXSegment fTX in group1.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == Enterprise.Edifact.D09B.Elements.TextSubjectCodeQualifierList.CustomsClearanceInstructions
						|| fTX.TextSubjectCodeQualifier == Enterprise.Edifact.D09B.Elements.TextSubjectCodeQualifierList.RegulatoryInformation)
					{
						var code = fTX.TextReference.FreeTextDescriptionCode;
						var textLiteral = Regex.Replace(fTX.TextLiteral.FreeText1, " {2,}", " ").Trim();
						builder.Append(System.Environment.NewLine + System.Environment.NewLine);
						builder.Append(string.IsNullOrEmpty(code) ? textLiteral : string.Concat(code, " - ", textLiteral));
					}
				}
			}

			return builder.ToString();
		}

		ZString FormattedDateTime(ZString messageDate)  // eg: 20110214001550SST
		{
			return string.Concat(messageDate.SubstringSafe(6, 2), "-", GetMonthAbbrev(messageDate.SubstringSafe(4, 2)), "-", messageDate.SubstringSafe(0, 4), " ", messageDate.SubstringSafe(10, 2), ":", messageDate.SubstringSafe(12, 2));
		}

		#endregion
	}
}
