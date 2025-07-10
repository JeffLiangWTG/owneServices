using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.ServiceTasks
{
	class OrphanWIPorACRChecker : DataConsistencyChecker
	{
		public override void Check(ILogger serviceLogger)
		{
			var sqlText = @"
SELECT 
	DISTINCT GC_PK, GC_CODE
FROM
(
	SELECT 
		GC_PK, GC_CODE
	FROM 
		dbo.AccTransactionLines
		INNER JOIN dbo.JobHeader ON AL_JH = JH_PK
		INNER JOIN dbo.GlbCompany on JH_GC = GC_PK
	WHERE
		AL_LineType = @WIP AND
		AL_ReverseDate IS NULL
		AND NOT EXISTS (SELECT JR_AL_ARLine FROM dbo.JobCharge WHERE JR_AL_ARLine = AL_PK)

	UNION ALL

	SELECT 
		GC_PK, GC_CODE
	FROM 
		dbo.AccTransactionLines
		INNER JOIN dbo.JobHeader ON AL_JH = JH_PK
		INNER JOIN dbo.GlbCompany ON JH_GC = GC_PK
	WHERE
		AL_LineType = @ACR AND
		AL_ReverseDate IS NULL
		AND NOT EXISTS (SELECT JR_AL_APLine FROM dbo.JobCharge WHERE JR_AL_APLine = AL_PK)
) INNERTABLE";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@WIP", TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);
			parameters.Add("@ACR", TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType);

			var query = new DynamicBusinessObjectCollection(Factory);
			query.Load(sqlText, parameters);

			var companiesWithOrphanWipsOrAccruals = new List<ZGuid>();
			if (query.Count != 0)
			{
				var builder = new ZStringBuilder();
				foreach (var dynamicBusinessObject in query)
				{
					var companyPk = (ZGuid)dynamicBusinessObject[GlbCompanySchema.PK];
					companiesWithOrphanWipsOrAccruals.Add(companyPk);
					var companyCode = (ZString)dynamicBusinessObject[GlbCompanySchema.GC_Code];
					builder.Append(companyCode);

					AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.SetValue(companyPk.ToGuid(), Guid.Empty, Guid.Empty, true);
				}

				serviceLogger.Log(LogType.Debug, string.Format("Orphan WIPs or Accruals found for the following company(s) - {0}.\r\nPlease use 'Show Only Orphan Transactions' filter option in WIPs or Accruals module to identify the Jobs having these orphan WIPs or Accruals.", builder.ToStringWithDelimiterBetweenAppends(",")));
			}
			else
			{
				serviceLogger.Log(LogType.Debug, "No orphan WIPs or Accruals found for any of the companies.");
			}

			foreach (GlbCompany company in Factory.Load<GlbCompany>(new ZQuery()))
			{
				if (!companiesWithOrphanWipsOrAccruals.Contains(company.PK))
				{
					if (AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
					{
						AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
						serviceLogger.Log(LogType.Debug, string.Format("Orphan WIPs or Accruals resolved for company - {0}", company.GC_Code));
					}
				}
			}
		}

		public override string Description
		{
			get { return Res.GetString("452a098f-6870-46b3-b997-347563cc8823", "Orphan WIPs or Accruals detection"); }
		}
	}
}
