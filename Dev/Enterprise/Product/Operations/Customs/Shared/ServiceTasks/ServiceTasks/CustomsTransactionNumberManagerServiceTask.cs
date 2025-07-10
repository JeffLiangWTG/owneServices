using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(CustomsTransactionNumberManagerServiceTask.ServiceTaskCode,
	CustomsTransactionNumberManagerServiceTask.ServiceTaskDescription,
	CustomsTransactionNumberManagerServiceTask.ServiceTaskCategory,
	typeof(CustomsTransactionNumberManagerServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.ServiceTasks
{
	public class CustomsTransactionNumberManagerServiceTask : ServiceProviderImpl
	{
		public const string ServiceTaskCategory = "CUS";
		public const string ServiceTaskCode = "TNM";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "service task description")]
		public const string ServiceTaskDescription = "Customs Transaction Number Manager Service Task";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			using (DisposableEnvironment.ForBranch(Env.CurrentBranchPK))
			{
				CleanUpOldData();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		void CleanUpOldData()
		{
			var factory = new BusinessObjectFactory()
			{
				NameForDebugging = "Cleanup Transaction Number Data",
				RefreshEnabled = false
			};
			var parameters = new ZSqlParameterCollection();
			var sqlText = new ZStringBuilder("TN_SystemCreateTimeUtc < (CASE ");
			foreach ((string type, ZDateTime date) in GetCleanUpDetails(factory))
			{
				var typeVariable = "@" + type + "Type";
				var dateVariable = "@" + type + "Date";
				sqlText.Append($"WHEN TN_Type = {typeVariable} THEN {dateVariable}");
				parameters.Add(typeVariable, type, CusTransactionNumberSchema.TN_Type);
				parameters.Add(dateVariable, date, CusTransactionNumberSchema.TN_SystemCreateTimeUtc);
			}
			parameters.Add("@DefaultDate", ZDateTime.UtcToday.AddMonths(-1).AddDays(1), CusTransactionNumberSchema.TN_SystemCreateTimeUtc);
			sqlText.Append("ELSE @DefaultDate END)");
			var query = new ZDBOnlyQuery(typeof(CusTransactionNumber));
			query.AddFilterAndZSQLParameterCollection(sqlText.ToStringWithDelimiterBetweenAppends("\r\n\t"), parameters);
			Db.Connection.ExecuteNonQuery("DELETE dbo.CusTransactionNumber WHERE " + query.LiteralTextSqlFormatted);
		}

		IEnumerable<(string type, ZDateTime date)> GetCleanUpDetails(BusinessObjectFactory factory)
		{
			var ieNoOfDays = new RefSysConfig.Loader(factory).GetDecimalValue(Customs.Universal.Constants.RefSysConfig.ConfigCodes.NoOfDaysIETransactionIDShouldBeKept, 3m, ZDateTime.UtcToday).ToZInt() + IETransactionNumberMinimumNoOfDaysToKeep - 1;
			yield return (CusTransactionNumberTypeList.Codes.IECustoms, ZDateTime.UtcToday.AddDays(-ieNoOfDays));
			yield return (CusTransactionNumberTypeList.Codes.IECustomsEMCS, ZDateTime.UtcToday.AddDays(-ieNoOfDays));
		}

		const int IETransactionNumberMinimumNoOfDaysToKeep = 7;
	}
}
