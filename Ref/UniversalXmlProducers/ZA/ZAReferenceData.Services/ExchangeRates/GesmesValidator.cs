using System;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.GESMES;
using Enterprise.Edifact.D96B.Segments;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates
{
	internal static class GesmesValidator
	{
		public static bool ValidateMessage(GESMESMessage message, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.Check(errorCollector, message == null, "GESMES message is NULL");
			valid = valid && Validate_UNH(message.UNH, errorCollector);
			valid = valid && Validate_BGM(message.BGM, errorCollector);
			valid = valid && Validate_DTM(message.DTM, errorCollector);
			valid = valid && EdifactLoader.Check(errorCollector, message.Group11 == null || message.Group11.Count == 0, "Expected at least 1 occurrence of 'Group11' segment group");
			
			if (valid)
			{
				for (int i = 0; i < message.Group11.Count && valid; i++)
				{
					valid = Validate_Group11(message.Group11[i], errorCollector);
				}
			}

			valid = valid && Validate_UNT(message.UNT, errorCollector, message.Group11[0].ARR.Count);

			return valid;
		}

		internal static bool Validate_UNH(UNHSegmentMessageSection segMsgSec, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionUNH, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionUNH, segMsgSec, 1, errorCollector);

			valid = valid && EdifactLoader.Check(errorCollector, !(segMsgSec[0].MessageIdentifier.MessageType == "GESMES" &&
											segMsgSec[0].MessageIdentifier.MessageVersionNumber == "D" &&
											segMsgSec[0].MessageIdentifier.MessageReleaseNumber == "96B" &&
											segMsgSec[0].MessageIdentifier.ControllingAgency == "UN" &&
											segMsgSec[0].MessageIdentifier.AssociationAssignedCode == "ZZZ01"),
											"Message identifier not supported");

			return valid;
		}

		internal static bool Validate_BGM(BGMSegmentMessageSection segMsgSec, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionBGM, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionBGM, segMsgSec, 1, errorCollector);

			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].DocumentMessageName.DocumentMessageNameCoded != DocumentMessageNameCodedList.StatisticalAndOtherAdministrativeInternalDocuments, "Expected DocumentMessageNameCoded to be 190 - Statistical and other administrative internal documents")
						  && EdifactLoader.Check(errorCollector, segMsgSec[0].MessageFunctionCoded == MessageFunctionCodedList.Deletion, "Exchange rate deletions are not supported");

			return valid;
		}

		internal static bool Validate_DTM(DTMSegmentMessageSection segMsgSec, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionDTM, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionDTM, segMsgSec, 1, errorCollector);

			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].DateTimePeriod.DateTimePeriodQualifier != DateTimePeriodQualifierList.EffectiveDateTime, "Expected Effective date in DTM");
			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].DateTimePeriod.DateTimePeriodFormatQualifier != DateTimePeriodFormatQualifierList.Ccyymmdd, "Expected CCYYMMDD format for effective date");

			return valid;
		}

		internal static bool Validate_Group11(SegmentGroup11 segmentGroup, StringBuilder errorCollector)
		{
			var valid = Validate_Group11_DSI(segmentGroup.DSI, errorCollector) &&
						Validate_Group11_ARR(segmentGroup.ARR, errorCollector);

			return valid;
		}

		internal static bool Validate_Group11_DSI(DSISegmentMessageSection segMsgSec, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionDSI, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionDSI, segMsgSec, 1, errorCollector);

			valid = valid && EdifactLoader.Check(errorCollector, segMsgSec[0].DataSetIdentification.DataSetIdentifier != EdifactLoader.ZAR, "DataSetIdentifier should be 'ZAR'");

			return valid;
		}

		internal static bool Validate_Group11_ARR(ARRSegmentMessageSection segMsgSec, StringBuilder errorCollector)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionARR, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionMinOccurrences(EdifactLoader.SectionARR, segMsgSec, 1, errorCollector);

			if (valid)
			{
				foreach (ARRSegment arr in segMsgSec)
				{
					valid = EdifactLoader.Check(errorCollector, string.IsNullOrEmpty(arr.PositionIdentification.HierarchicalIdNumber), "Exchange rate currency is required") &&
							EdifactLoader.Check(errorCollector, !decimal.TryParse(arr.ArrayCellDetails.ArrayCellInformation, NumberStyles.Any, CultureInfo.InvariantCulture, out var _), $"Could not convert rate '{arr.ArrayCellDetails.ArrayCellInformation}' to decimal for '{arr.PositionIdentification.HierarchicalIdNumber}'");

					if (!valid)
					{
						break;
					}
				}
			}

			return valid;
		}

		internal static bool Validate_UNT(UNTSegmentMessageSection segMsgSec, StringBuilder errorCollector, int group11Count)
		{
			var valid = EdifactLoader.CheckMessageSectionExists(EdifactLoader.SectionUNT, segMsgSec, errorCollector) &&
						EdifactLoader.CheckMessageSectionOccurrences(EdifactLoader.SectionUNT, segMsgSec, 1, errorCollector);

			valid = valid && EdifactLoader.Check(errorCollector, !int.TryParse(segMsgSec[0].NumberOfSegmentsInTheMessage, NumberStyles.Any, CultureInfo.InvariantCulture, out var numSegs), "Number of segments in message could not be read")
						  && EdifactLoader.Check(errorCollector, group11Count != (numSegs-5), $"Message is expected to have {numSegs-5} exchange rates but {group11Count} were identified");

			return valid;
		}
	}
}
