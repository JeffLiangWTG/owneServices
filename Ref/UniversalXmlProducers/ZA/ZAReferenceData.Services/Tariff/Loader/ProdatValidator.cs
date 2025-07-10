using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.PRODAT;
using Enterprise.Edifact.D96B.Segments;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader
{
	public static class ProdatValidator
	{
		public static bool ValidateMessage(PRODATMessage prodatMessage, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.Check(errorCollector, prodatMessage == null, "PRODAT message is NULL");
			valid = valid && Validate_Group0(prodatMessage, errorCollector);
			valid = valid && EdifactLoader.Check(errorCollector, prodatMessage.Group8 == null || prodatMessage.Group8.Count == 0, "Expected at least 1 occurrence of 'Group8' segment group");

			if (valid)
			{
				for (int i = 0; i < prodatMessage.Group8.Count && valid; i++)
				{
					valid = Validate_Group8(prodatMessage.Group8[i], errorCollector);
				}
			}

			return valid;
		}

		#region Group0
		internal static bool Validate_Group0(PRODATMessage prodatMessage, StringBuilder errorCollector)
		{
			var valid = Validate_Group0_UNH(prodatMessage.UNH, errorCollector) &&
						Validate_Group0_BGM(prodatMessage.BGM, errorCollector) &&
						Validate_Group0_DTM(prodatMessage.DTM, errorCollector);

			return valid;
		}

		internal static bool Validate_Group0_UNH(UNHSegmentMessageSection segMsgSec, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionUNH, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionUNH, segMsgSec, 1, errorCollector);

			valid = valid && EdifactLoader.Check(errorCollector, !(segMsgSec[0].MessageIdentifier.MessageType == "PRODAT" &&
											segMsgSec[0].MessageIdentifier.MessageVersionNumber == "D" &&
											segMsgSec[0].MessageIdentifier.MessageReleaseNumber == "96B" &&
											segMsgSec[0].MessageIdentifier.ControllingAgency == "UN" &&
											segMsgSec[0].MessageIdentifier.AssociationAssignedCode == "ZZZ01"),
											"Message identifier not supported");

			return valid;
		}

		internal static bool Validate_Group0_BGM(BGMSegmentMessageSection segMsgSec, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionBGM, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionBGM, segMsgSec, 1, errorCollector);

			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].DocumentMessageName.DocumentMessageNameCoded != DocumentMessageNameCodedList.ProductSpecificationReport,
								"Expected DocumentMessageNameCoded to be 6 - Product Specification Report");

			return valid;
		}

		internal static bool Validate_Group0_DTM(DTMSegmentMessageSection segMsgSec, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionDTM, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionDTM, segMsgSec, 1, errorCollector);

			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].DateTimePeriod.DateTimePeriodQualifier != DateTimePeriodQualifierList.PublicationDate, "Expected Publication date in DTM");
			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].DateTimePeriod.DateTimePeriodFormatQualifier != DateTimePeriodFormatQualifierList.Ccyymmdd, "Expected CCYYMMDD format for publication date");

			return valid;
		}
		#endregion

		#region Group8
		internal static bool Validate_Group8(SegmentGroup8 segmentGroup, StringBuilder errorCollector)
		{
			var valid = Validate_Group8_LIN(segmentGroup.LIN, errorCollector, out var lineNumber) &&
						Validate_Group8_PIA(segmentGroup.PIA, errorCollector, lineNumber) &&
						Validate_Group8_DTM(segmentGroup.DTM, errorCollector, lineNumber) &&
						Validate_Group8_MEA(segmentGroup.MEA, errorCollector, lineNumber) &&
						Validate_Group8_FTX(segmentGroup.FTX, errorCollector, lineNumber) &&
						Validate_Group8_PGI(segmentGroup.PGI, errorCollector, lineNumber);

			return valid;
		}

		internal static bool Validate_Group8_LIN(LINSegmentMessageSection segMsgSec, StringBuilder errorCollector, out string lineNumber)
		{
			lineNumber = string.Empty;
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionLIN, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionLIN, segMsgSec, 1, errorCollector);

			if (valid)
			{
				lineNumber = segMsgSec[0].LineItemNumber;
			}

			return valid;
		}

		internal static bool Validate_Group8_PIA(PIASegmentMessageSection segMsgSec, StringBuilder errorCollector, string lineNumber)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionPIA, segMsgSec, errorCollector, lineNumber) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionPIA, segMsgSec, 1, errorCollector, lineNumber);

			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].ProductIdFunctionQualifier != ProductIdFunctionQualifierList.ProductIdentification, "Expected ProductIdFunctionQualifier to be 5 - ProductIdentification", lineNumber);
			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].ItemNumberIdentification1.ItemNumber != "1", "Expected ItemNumberIdentification1.ItemNumber to be 1", lineNumber);

			return valid;
		}

		internal static bool Validate_Group8_DTM(DTMSegmentMessageSection segMsgSec, StringBuilder errorCollector, string lineNumber)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionDTM, segMsgSec, errorCollector, lineNumber) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionDTM, segMsgSec, 2, errorCollector, lineNumber);

			if (valid)
			{
				foreach (DTMSegment dtm in segMsgSec)
				{
					valid = valid && EdifactLoader.Check(errorCollector, dtm.DateTimePeriod.DateTimePeriodFormatQualifier != DateTimePeriodFormatQualifierList.Ccyymmdd &&
														   dtm.DateTimePeriod.DateTimePeriodFormatQualifier != DateTimePeriodFormatQualifierList.Ccyymmddhhmmss,
														   "Expected DateTimePeriodFormat to be 102 - CCYYMMDD or 204 - CCYYMMDDHHMMSS", lineNumber);

					valid = valid && EdifactLoader.Check(errorCollector, dtm.DateTimePeriod.DateTimePeriodQualifier != DateTimePeriodQualifierList.EffectiveDateTime &&
														   dtm.DateTimePeriod.DateTimePeriodQualifier != DateTimePeriodQualifierList.EndDateTime,
														   "Expected DateTimePeriod to be 7 - EffectiveDateTime or 206 - EndDateTime", lineNumber);
				}
			}

			return valid;
		}

		internal static bool Validate_Group8_MEA(MEASegmentMessageSection segMsgSec, StringBuilder errorCollector, string lineNumber)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionMEA, segMsgSec, errorCollector, lineNumber) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionMEA, segMsgSec, 1, errorCollector, lineNumber);

			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].MeasurementApplicationQualifier != MeasurementApplicationQualifierList.Measurement, "Expected MeasurementApplicationQualifier to be AAE - Measurement", lineNumber);

			return valid;
		}

		internal static bool Validate_Group8_FTX(FTXSegmentMessageSection segMsgSec, StringBuilder errorCollector, string lineNumber)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionFTX, segMsgSec, errorCollector, lineNumber) &&
						EdifactLoader.CheckMessageSectionMinOccurrences(EdifactLoader.SectionFTX, segMsgSec, 1, errorCollector, lineNumber);

			return valid;
		}

		internal static bool Validate_Group8_PGI(PGISegmentMessageSection segMsgSec, StringBuilder errorCollector, string lineNumber)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionPGI, segMsgSec, errorCollector, lineNumber) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionPGI, segMsgSec, 1, errorCollector, lineNumber);

			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].ProductGroupTypeCoded != ProductGroupTypeCodedList.ProductGroup, "Expected ProductGroupTypeCoded to be 11 - ProductGroup", lineNumber);

			return valid;
		}
		#endregion
	}
}
