using System.Data;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using D05B = Enterprise.Edifact.D05B;
using D09B = Enterprise.Edifact.D09B;

namespace Enterprise.Customs.SG.V4.Business
{
	public class COODCIEDIMessage : SGEDIMessage
	{
		public COODCIEDIMessage(BusinessObjectFactory factory, DataRow row)
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
					if (TradeNetMessage is D09B.Messages.TCODEC.TCODECMessage tcodec09b)
					{
						messageInterpretation = InterpretD09bMessage(tcodec09b);
					}
					else
					{
						messageInterpretation = InterpretD05bMessage((D05B.Messages.TCODEC.TCODECMessage)TradeNetMessage);
					}
				}
				return messageInterpretation;
			}
		}
		ZString messageInterpretation;

		#region Implementation

		ZString InterpretD05bMessage(D05B.Messages.TCODEC.TCODECMessage tcodec05b)
		{
			var builder = new ZStringBuilder("CERTIFICATE OF ORIGIN APPROVED" + System.Environment.NewLine + System.Environment.NewLine);

			foreach (D05B.Messages.TCODEC.SegmentGroup1 group1 in tcodec05b.Group1)
			{
				foreach (D05B.Segments.RFFSegment rFF in group1.RFF)
				{
					if (rFF.Reference.ReferenceCodeQualifier == D05B.Elements.ReferenceCodeQualifierList.OriginalCertificateNumber)
					{
						builder.AppendLine(string.Concat("CERTIFICATE NUMBER : ", rFF.Reference.ReferenceIdentifier, System.Environment.NewLine));

						foreach (D05B.Segments.DTMSegment dTM in group1.DTM)
						{
							if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == D05B.Elements.DateOrTimeOrPeriodFunctionCodeQualifierList.AuthorizationDate)
							{
								builder.AppendLine("AUTHORISATION DATE : " + dTM.DateTimePeriod.DateOrTimeOrPeriodText);
							}
						}
					}
				}

				foreach (D05B.Segments.FTXSegment fTX in group1.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == D05B.Elements.TextSubjectCodeQualifierList.CustomsClearanceInstructions)
					{
						var textLiteral = string.Join(System.Environment.NewLine, fTX.TextLiteral.FreeText1, fTX.TextLiteral.FreeText2, fTX.TextLiteral.FreeText3, fTX.TextLiteral.FreeText4, fTX.TextLiteral.FreeText5).Trim();
						builder.Append(System.Environment.NewLine + System.Environment.NewLine + textLiteral);
					}
				}
			}

			return builder.ToString();
		}

		ZString InterpretD09bMessage(D09B.Messages.TCODEC.TCODECMessage tcodec09b)
		{
			var builder = new ZStringBuilder("CERTIFICATE OF ORIGIN APPROVED" + System.Environment.NewLine + System.Environment.NewLine);

			foreach (D09B.Messages.TCODEC.SegmentGroup1 group1 in tcodec09b.Group1)
			{
				foreach (D09B.Segments.RFFSegment rFF in group1.RFF)
				{
					if (rFF.Reference.ReferenceCodeQualifier == D09B.Elements.ReferenceCodeQualifierList.OriginalCertificateNumber)
					{
						builder.AppendLine(string.Concat("CERTIFICATE NUMBER : ", rFF.Reference.ReferenceIdentifier, System.Environment.NewLine));

						foreach (D09B.Segments.DTMSegment dTM in group1.DTM)
						{
							if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == D09B.Elements.DateOrTimeOrPeriodFunctionCodeQualifierList.AuthorizationDate)
							{
								builder.AppendLine("AUTHORISATION DATE : " + dTM.DateTimePeriod.DateOrTimeOrPeriodText);
							}
						}
					}
				}

				foreach (D09B.Segments.FTXSegment fTX in group1.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == D09B.Elements.TextSubjectCodeQualifierList.CustomsClearanceInstructions)
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

		#endregion
	}
}
