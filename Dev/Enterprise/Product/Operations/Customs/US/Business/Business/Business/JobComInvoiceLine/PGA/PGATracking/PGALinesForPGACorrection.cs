using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public static class PGALinesForPGACorrection
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<T> GetPGALines<T>(IACECusEntryLine entryLine, IPGAGovernmentAgenciesCommon line, Func<IPGAGovernmentAgenciesCommon, IEnumerable<T>> getPGALines, Func<IPGAGovernmentAgenciesCommon, IEnumerable<IPGADataCorrection>> getAllPGALinesPerPGA)
		{
			var allPGALines = new List<T>();

			foreach (IPGAGovernmentAgenciesCommon pgaParentLine in GetPGAParentLines(entryLine, line))
			{
				allPGALines.AddRange(getPGALines(pgaParentLine).Where(x => !IsDeleted((IPGADataCorrection)x)));
			}

			IEnumerable<T> result = Array.Empty<T>();
			var pgaLinesWithoutDeletedLines = getPGALines(line).Where(x => !IsDeleted((IPGADataCorrection)x));

			if (allPGALines.Cast<IPGADataCorrection>().AreAllLinesToBeDeleted())
			{
				result = pgaLinesWithoutDeletedLines;
			}
			else
			{
				if (ZZCustomsFunctionality.IsPGADataCorrection2ndPhaseffective)
				{
					result = getPGALines(line).Where(x => NeedToAmend((IPGADataCorrection)x));
				}
				else
				{
					foreach (IPGAGovernmentAgenciesCommon pgaParentLine in GetPGAParentLines(entryLine, line))
					{
						if (getAllPGALinesPerPGA(pgaParentLine).Any(x => NeedToAmend(x)))
						{
							result = pgaLinesWithoutDeletedLines;
							break;
						}
					}
				}
			}
			return result;
		}

		static IEnumerable<IPGAGovernmentAgenciesCommon> GetPGAParentLines(IACECusEntryLine entryLine, IPGAGovernmentAgenciesCommon line)
		{
			if (entryLine != null)
			{
				yield return (IPGAGovernmentAgenciesCommon)entryLine;
				foreach (ISecondaryTariffLine secondaryLine in entryLine.SecondaryTariffLines)
				{
					yield return secondaryLine;
				}
			}
			else
			{
				yield return line;
			}
		}

		static internal IEnumerable<IPGADataCorrection> GetAllAPHISLines(IPGAGovernmentAgenciesCommon line)
		{
			var result = line.APHISHeaders.Cast<IPGADataCorrection>();
			result = result.Concat(line.LaceyActData.Cast<IPGADataCorrection>());
			return result;
		}

		static internal IEnumerable<IPGADataCorrection> GetAllNMFSLines(IPGAGovernmentAgenciesCommon line)
		{
			var result = line.NMFS370Lines.Cast<IPGADataCorrection>();
			result = result.Concat(line.NMFSAMRLines.Cast<IPGADataCorrection>());
			result = result.Concat(line.NMFSCOALines.Cast<IPGADataCorrection>());
			result = result.Concat(line.NMFSHMSLines.Cast<IPGADataCorrection>());
			result = result.Concat(line.NMFSSIMLines.Cast<IPGADataCorrection>());
			return result;
		}

		static internal IEnumerable<IPGADataCorrection> GetAllEPALines(IPGAGovernmentAgenciesCommon line)
		{
			var result = line.EPA_PSTLines.Cast<IPGADataCorrection>();
			result = result.Concat(line.EPA_VNELines.Cast<IPGADataCorrection>());
			result = result.Concat(line.EPA_HFCHeaders.Cast<IPGADataCorrection>());

			if (ShouldAddToAllLines(line.TSCAIndicator, line.TSCADataCorrection))
			{
				result = result.Concat(new IPGADataCorrection[] { line.TSCADataCorrection });
			}

			if (ShouldAddToAllLines(line.ODSIndicator, line.ODSDataCorrection))
			{
				result = result.Concat(new IPGADataCorrection[] { line.ODSDataCorrection });
			}

			return result;
		}

		static internal IEnumerable<IDDTCData> GetDDTCData(IPGAGovernmentAgenciesCommon line)
		{
			var shouldAddDDTCData = ShouldAddToAllLines(line.DDTCIndicator, line.DDTCData);
			return shouldAddDDTCData ? new IDDTCData[] { line.DDTCData } : Array.Empty<IDDTCData>();
		}

		static bool ShouldAddToAllLines(ZString indicator, IPGADataCorrection pgaCorrection)
		{
			return pgaCorrection != null && (OGAIndicatorList.IsToBeDeclared(indicator) || (OGAIndicatorList.IsToBeDisclaimed(indicator) && !pgaCorrection.TrackingStatusInfo.Value.IsEmpty));
		}

		public static bool NeedToAmend(IPGADataCorrection pgaLine)
		{
			var status = (ZString)pgaLine.TrackingStatusInfo.Value;
			return status.IsEmpty
				|| status == PGATrackingStatusList.Codes.ToBeUpdated
				|| status == PGATrackingStatusList.Codes.ToBeDeleted
				|| status == PGATrackingStatusList.Codes.Updating
				|| status == PGATrackingStatusList.Codes.Deleting
				|| status == PGATrackingStatusList.Codes.Adding;
		}

		static bool IsDeleted(IPGADataCorrection pgaLine)
		{
			return (ZString)pgaLine.TrackingStatusInfo.Value == PGATrackingStatusList.Codes.Deleted;
		}
	}
}
