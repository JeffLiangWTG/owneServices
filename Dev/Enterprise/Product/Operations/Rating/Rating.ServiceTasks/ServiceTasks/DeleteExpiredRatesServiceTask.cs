using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static System.FormattableString;

[assembly: HostedService(
	"RED",
	"Permanently Delete Expired Rates from CW Database",
	"RAT",
	typeof(Enterprise.Rating.ServiceTasks.DeleteExpiredRatesServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1week",
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday }
	)]
namespace Enterprise.Rating.ServiceTasks
{
	public class DeleteExpiredRatesServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public override void RunTask(CancellationToken cancellationToken)
		{
			var expiryPeriod = Env.Registry.Rating.PermanentlyDeleteRatesExpiredPeriod.ExpiredRatesPeriodInYears;
			var batchSize = Env.Registry.Rating.PermanentlyDeleteRatesExpiredPeriod.BatchSize;
			if (expiryPeriod < 1)
			{
				ServiceLogger.Log(LogType.Information, Invariant($"The expiry period is not set, terminating"));
				return;
			}

			ServiceLogger.Log(LogType.Information, Invariant($"Permanently Delete Rates Expired in Period Registry Value: {expiryPeriod} year(s)"));

			var dateOfExpiry = ZDate.Today.AddYears(-expiryPeriod);

			var totalCount = GetExpiredRatesCount(dateOfExpiry);

			var counter = 0;
			while (counter < totalCount)
			{
				var deleteQuery = GetDeleteQuery(dateOfExpiry, batchSize, counter, totalCount);

				using (var command = Db.Connection.Command(deleteQuery))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var ratingHeaderOrg = reader.IsDBNull(0) ? null : reader.GetString(0);
							var globalCompany = reader.IsDBNull(1) ? null : reader.GetString(1);
							var rateType = reader.GetString(2);
							var globalRateLevel = reader.GetByte(3);
							var entryCount = reader.GetInt32(4);

							LogDetails(ratingHeaderOrg, globalCompany, rateType, globalRateLevel, entryCount);
						}
					}
				}
				counter += batchSize;
			}
		}

		void LogDetails(string ratingHeaderOrg, string globalCompany, string rateType, Byte globalRateLevel, int entryCount)
		{
			var isGlobal = globalCompany is null ? (NoResString)"Global" : (NoResString)"Local";
			var companyCode = globalCompany != null ? (NoResString)"in " + globalCompany : string.Empty;
			var serviceProvider = !string.IsNullOrEmpty(ratingHeaderOrg) ? $" of Service Provider {ratingHeaderOrg}" : string.Empty;

			switch (rateType)
			{
				case RatingConstants.RatingHeaderTypes.ClientRate:
					ServiceLogger.Log(LogType.Information, Invariant($"Client Rates {companyCode} of client {ratingHeaderOrg}: {entryCount} Rate Entries ({isGlobal})"));
					break;
				case RatingConstants.RatingHeaderTypes.Tariff:
					ServiceLogger.Log(LogType.Information, Invariant($"Company Tariffs {companyCode} Levels {globalRateLevel}: {entryCount} Rate Entries ({isGlobal})"));
					break;
				case RatingConstants.RatingHeaderTypes.Costing:
					// there is no Service Provider (these are the Standard Costs)
					if (!string.IsNullOrEmpty(companyCode) && string.IsNullOrEmpty(ratingHeaderOrg))
					{
						ServiceLogger.Log(LogType.Information, Invariant($"Costings {companyCode} of Standard Costs: {entryCount} Rate Entries ({isGlobal})"));
					}
					// there is a Service Provider and the costing is Global
					else if (string.IsNullOrEmpty(companyCode) && !string.IsNullOrEmpty(ratingHeaderOrg))
					{
						ServiceLogger.Log(LogType.Information, Invariant($"Costings {serviceProvider}: {entryCount} Rate Entries ({isGlobal})"));
					}
					// there is a Service Provider and the costing is Local
					else if (!string.IsNullOrEmpty(companyCode) && !string.IsNullOrEmpty(ratingHeaderOrg))
					{
						ServiceLogger.Log(LogType.Information, Invariant($"Costings {companyCode} {serviceProvider}: {entryCount} Rate Entries ({isGlobal})"));
					}

					break;
				case RatingConstants.RatingHeaderTypes.IntercompanyTariff:
					ServiceLogger.Log(LogType.Information, Invariant($"Intercompany Tariffs {serviceProvider}: {entryCount} Rate Entries ({isGlobal})"));
					break;
			}
		}

		int GetExpiredRatesCount(ZDate dateOfExpiry)
		{
			var expiredRatesCount = 0;

			var sql = $@"
SELECT COUNT(*)
FROM dbo.RateEntry as rateEntry
JOIN dbo.RatingHeader as ratingHeader ON rateEntry.{RateEntrySchema.Constants.TI_TH} = ratingHeader.{RatingHeaderSchema.Constants.PK}
LEFT JOIN dbo.OrgHeader as orgHeader ON ratingHeader.{RatingHeaderSchema.Constants.TH_OH} = orgHeader.{OrgHeaderSchema.Constants.PK}
LEFT JOIN dbo.GlbCompany as glbCompany ON ratingHeader.{RatingHeaderSchema.Constants.TH_GC} = glbCompany.{GlbCompanySchema.Constants.PK}
WHERE ratingHeader.{RatingHeaderSchema.Constants.TH_RateType} IN (
    '{RatingConstants.RatingHeaderTypes.ClientRate}',
    '{RatingConstants.RatingHeaderTypes.Costing}',
    '{RatingConstants.RatingHeaderTypes.Tariff}',
    '{RatingConstants.RatingHeaderTypes.IntercompanyTariff}'
)
AND rateEntry.{RateEntrySchema.Constants.TI_RateEndDate} <= '{dateOfExpiry}'
";
			expiredRatesCount = (int)Db.Connection.ExecuteScalar(sql);
			return expiredRatesCount;
		}

		string GetDeleteQuery(ZDate dateOfExpiry, int batchSize, int counter, int totalCount)
		{
			ServiceLogger.Log(LogType.Information, Invariant($"Cut-off Expiry Date: {dateOfExpiry.ToShortDateString()} batch {counter + 1} - {Math.Min((counter + batchSize), totalCount)} of {totalCount}"));

			var sql = $@"
DECLARE @DeletedInfo TABLE
(
    OrgCode nvarchar(12),
    CompanyCode char(3),
    RateType char(3),
    GlobalRateLevel tinyint,
    RateEntryPK uniqueidentifier
)

DELETE TOP({batchSize}) rateEntry
    OUTPUT orgHeader.{OrgHeaderSchema.Constants.OH_Code}, glbCompany.{GlbCompanySchema.Constants.GC_Code},
    ratingHeader.{RatingHeaderSchema.Constants.TH_RateType}, ratingHeader.{RatingHeaderSchema.Constants.TH_GlobalRateLevel},
    DELETED.{RateEntrySchema.Constants.PK}
    INTO @DeletedInfo
FROM dbo.RateEntry as rateEntry
JOIN dbo.RatingHeader as ratingHeader ON rateEntry.{RateEntrySchema.Constants.TI_TH} = ratingHeader.{RatingHeaderSchema.Constants.PK}
LEFT JOIN dbo.OrgHeader as orgHeader ON ratingHeader.{RatingHeaderSchema.Constants.TH_OH} = orgHeader.{OrgHeaderSchema.Constants.PK}
LEFT JOIN dbo.GlbCompany as glbCompany ON ratingHeader.{RatingHeaderSchema.Constants.TH_GC} = glbCompany.{GlbCompanySchema.Constants.PK}
WHERE ratingHeader.{RatingHeaderSchema.Constants.TH_RateType} IN (
    '{RatingConstants.RatingHeaderTypes.ClientRate}',
    '{RatingConstants.RatingHeaderTypes.Costing}',
    '{RatingConstants.RatingHeaderTypes.Tariff}',
    '{RatingConstants.RatingHeaderTypes.IntercompanyTariff}'
)
AND rateEntry.{RateEntrySchema.Constants.TI_RateEndDate} <= '{dateOfExpiry}'

SELECT OrgCode, CompanyCode, RateType, GlobalRateLevel, Count(DISTINCT RateEntryPK)
FROM @DeletedInfo
GROUP BY OrgCode, CompanyCode, RateType, GlobalRateLevel
";
			return sql;
		}
	}
}
