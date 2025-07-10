using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class NettingPeriodHelper
	{
		public static Guid GetNettingPeriod(NettingSystem nettingSystem, ZDateTime dateUtc, ZString ledger)
		{
			var sql = "SELECT NSP_PK FROM fn_NettingGetPeriod(@NettingSystem, @DueDate, @CurrentTime, @InvoiceType)";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@NettingSystem", SqlDbType.UniqueIdentifier, nettingSystem != null ? nettingSystem.PK.ToGuid() : Guid.Empty);
				cmd.AddParameter("@DueDate", SqlDbType.SmallDateTime, dateUtc.ToDateTime());
				cmd.AddParameter("@CurrentTime", SqlDbType.SmallDateTime, ZDateTime.UtcToday.ToDateTime());
				cmd.AddParameter("@InvoiceType", SqlDbType.Char, ledger.ToString());

				var cmdResult = cmd.ExecuteScalar();
				return cmdResult != null && cmdResult != DBNull.Value ? (Guid)cmdResult : Guid.Empty;
			}
		}

		public static NettingSystemPeriod GetNextOpenPeriod(NettingSystemPeriod currentPeriod, BusinessObjectFactory factory)
		{
			var nextNettingPeriodQuery = new ZDBOnlyQuery(typeof(NettingSystemPeriod));
			nextNettingPeriodQuery.AddToFilter(NettingSystemPeriodSchema.NSP_IsComplete, false);
			nextNettingPeriodQuery.AddToFilter(NettingSystemPeriodSchema.NSP_NettingExecutionDateUtc, SQLComparisonOperator.GreaterThan, currentPeriod.NSP_NettingExecutionDateUtc);
			nextNettingPeriodQuery.AddToFilter(NettingSystemPeriodSchema.NSP_NS_NettingSystem, currentPeriod.NSP_NS_NettingSystem);
			nextNettingPeriodQuery.OrderBy = NettingSystemPeriodSchema.NSP_NettingExecutionDateUtc.Name;

			return factory.LoadTop1<NettingSystemPeriod>(nextNettingPeriodQuery);
		}

		public static NettingSystemPeriod GetFirstOpenNettingPeriod(ZGuid companyPK, BusinessObjectFactory factory)
		{
			var nettingSystemSubQuery = new ZDBOnlySubQuery(typeof(NettingSystem), NettingSystemSchema.PK);
			nettingSystemSubQuery.AddToFilter(NettingSystemSchema.NS_GC, companyPK);

			var nettingSystemPeriodQuery = new ZDBOnlyQuery(typeof(NettingSystemPeriod));
			nettingSystemPeriodQuery.AddToFilter(NettingSystemPeriodSchema.NSP_IsComplete, false);
			nettingSystemPeriodQuery.OrderBy = NettingSystemPeriodSchema.NSP_NettingExecutionDateUtc.Name;
			nettingSystemPeriodQuery.AddSubQuery(NettingSystemPeriodSchema.NSP_NS_NettingSystem, nettingSystemSubQuery, JoinCondition.And);

			return factory.LoadTop1<NettingSystemPeriod>(nettingSystemPeriodQuery);
		}
	}
}
