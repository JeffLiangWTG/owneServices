using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.StabilityChecker;

[assembly: StabilityChecker("Customs Number Range RunOut Stability Checker", "NRC", typeof(Enterprise.Customs.ServiceTasks.NumberRangeThresholdRunOutChecker))]

namespace Enterprise.Customs.ServiceTasks
{
	public class NumberRangeThresholdRunOutChecker : IStabilityChecker
	{
		public StabilityResult[] Check()
		{
			var results = new List<StabilityResult>();
			var warning = GetNumberRangeThatHasReachWarning();
			if (!warning.IsEmpty)
			{
				results.Add(new StabilityResult(StabilityResultLevel.Warning, FormattableString.Invariant($"The following number range has reach the thresh hold warning mark:\r\n{warning}"), isUserRelatedNotification: true));
			}

			return results.ToArray();
		}

		ZString GetNumberRangeThatHasReachWarning()
		{
			var result = new ZStringBuilder();
			var sqlText = @"
SELECT *
FROM
(
	SELECT CASE
				WHEN GB_PK IS NOT NULL THEN GB_Code + ' - ' + GB_BranchName
				WHEN GC_PK IS NOT NULL THEN GC_Code + ' - ' + GC_Name
				ELSE 'UNKNOWN'
			END AS Owner,
			RTRIM(SUBSTRING(SNR_Name, 6, UnderscoreIndex-6)) AS Type,
			SUBSTRING(SNR_Name, 3, 2) AS CountryCode,
			SUBSTRING(SNR_Name, UnderscoreIndex+1, NameLength) AS FountainName,
			SNR_ThresholdRunOutWarning AS ThresholdRunOutWarning,
			Available
	FROM dbo.StmNumberRange
	INNER JOIN
	(
		SELECT SUM(CASE WHEN SN_MaximumValue >= FirstAvailable THEN SN_MaximumValue - FirstAvailable + 1 ELSE 0 END) AS Available, SN_Name, SN_Owner, CHARINDEX('_', SN_Name) AS UnderscoreIndex, LEN(SN_Name) AS NameLength
		FROM
		(
			SELECT SN_Owner, CASE WHEN SequenceSeparator = 0 THEN SN_Name ELSE SUBSTRING(SN_Name, 1, SequenceSeparator - 1) END AS SN_Name, SN_MaximumValue, FirstAvailable
			FROM
			(
				SELECT SN_Owner, SN_Name, CHARINDEX('|', SN_Name) AS SequenceSeparator, SN_MaximumValue, ISNULL(FirstAvailable, SN_Value) AS FirstAvailable
				FROM dbo.StmNums
				LEFT JOIN
				(
					SELECT SG_SN, MIN(SG_Value) AS FirstAvailable
					FROM dbo.StmNumberCache
					WHERE SG_IsUsed = 0
					GROUP BY SG_SN
				) AS NumberCache ON SN_Id = NumberCache.SG_SN
				WHERE SN_Name LIKE 'C#__-%~_%|%' escape '~'
			) AS Data
		) AS Data
		GROUP BY SN_Name, SN_Owner
	) AS AvailableData ON AvailableData.SN_Name LIKE SNR_Name + '%' AND AvailableData.SN_Owner = SNR_Owner AND AvailableData.Available <= SNR_ThresholdRunOutWarning
	LEFT JOIN dbo.GlbCompany ON GC_PK = SNR_Owner
	LEFT JOIN dbo.GlbBranch ON GB_PK = SNR_Owner
	WHERE 1=1
	AND (GC_PK IS NULL OR GC_IsActive =1)
	AND (GB_PK IS NULL OR GB_IsActive =1)
) AS Data
ORDER BY Data.CountryCode, Data.Type, Data.FountainName, Data.Owner
";
			using (var reader = Db.Connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					result.Append(FormattableString.Invariant($"Owner='{reader["Owner"]}', Type='{reader["Type"]}', Country='{reader["CountryCode"]}', FountainName='{reader["FountainName"]}', ThresholdRunOutWarning='{reader["ThresholdRunOutWarning"]}', Available='{reader["Available"]}'"));
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}
	}
}
