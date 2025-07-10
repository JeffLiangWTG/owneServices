using System.Data;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"SSR", 
	"Sailing Schedule Recycling", 
	"FRT",
	typeof(Enterprise.Freight.Business.ServiceTasks.SailingScheduleRecyclingServiceTask), 
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	CanRunInAnyBranch = true
	)]

// Can't apply HostedServiceBusinessObjectBinding
// The service task uses 'overdue' logic, so it does the processing when an event didn't happen before current time.
namespace Enterprise.Freight.Business.ServiceTasks
{
	class SailingScheduleRecyclingServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service logger")]
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			const string sql = @"
DECLARE @Changes TABLE 
(
	JV_PK				uniqueidentifier,
	JV_RV_NKVessel		varchar(35),
	JV_VoyageFlight		varchar(10)
);

WITH VoyageDetails
AS
(
	SELECT JV_PK AS VoyagePK,
	CASE
		WHEN (OM_CRVoyageRecyclingPeriodInMonths IS NULL OR OM_CRVoyageRecyclingPeriodInMonths = -1)
		THEN @period
		ELSE OM_CRVoyageRecyclingPeriodInMonths
	END AS RecyclingPeriod,
	(
		SELECT MAX(VoyageDate)
		FROM
		(
			VALUES (JA_A_DEP), (JA_E_DEP), (JB_A_ARV), (JB_E_ARV)
		) AS AllDates(VoyageDate)
	) AS LatestDate
	
	FROM dbo.JobVoyage
	LEFT JOIN dbo.OrgHeader ON JV_OH_Line = OH_PK
	LEFT JOIN dbo.OrgMiscServ ON OM_OH = OH_PK
	LEFT JOIN dbo.JobVoyOrigin ON JA_JV = JV_PK
	LEFT JOIN dbo.JobVoyDestination ON JB_JV = JV_PK

	WHERE JV_IsActive = 1
	AND JV_AirSeaRoad = 'SEA'
),
VoyageDetailsGrouped
AS
(
	SELECT VoyagePK, RecyclingPeriod, MAX(LatestDate) AS LatestDate
	FROM VoyageDetails
	GROUP BY VoyagePK, RecyclingPeriod
)

UPDATE dbo.JobVoyage
SET JV_IsActive = 0, JV_SystemLastEditTimeUtc = GETUTCDATE(), JV_SystemLastEditUser = @currentUser
OUTPUT INSERTED.JV_PK, INSERTED.JV_RV_NKVessel, INSERTED.JV_VoyageFlight
INTO @Changes
FROM dbo.JobVoyage
JOIN VoyageDetailsGrouped ON JV_PK = VoyagePK
WHERE RecyclingPeriod IS NOT NULL AND RecyclingPeriod != 0
AND DATEADD(month, RecyclingPeriod, CAST(LatestDate AS datetime)) < GETDATE()

SELECT
	JV_PK,
	JV_RV_NKVessel,
	JV_VoyageFlight
FROM
	@Changes
";

			var period = VoyageRecyclingPeriodList.GetAmountFromCode(FreightConfigurationRegistry.Instance.VoyageRecyclingPeriod.Value);
			var currentUser = GlbStaff.CurrentUser.GS_Code;
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddParameterBasedOnDbColumn("@currentUser", currentUser.ToString(), JobVoyageSchema.JV_SystemLastEditUser);
				sqlCommand.AddParameter("@period", SqlDbType.VarChar, period.ToString());

				using (var reader = sqlCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = reader.GetGuid(0);
						var vessel = reader.GetString(1);
						var voyage = reader.GetString(2);

						var log = string.Format(CultureInfo.InvariantCulture, (NoResString)@"Archived Sailing: {0}
{1} - {2}", pk, vessel, voyage);

						ServiceLogger.Log(LogType.Information, log);
					}
				}
			}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
		}
	}
}
