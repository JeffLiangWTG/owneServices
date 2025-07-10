using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationLevyRate : AutoNZCClassificationLevyRate
	{
		public NZCClassificationLevyRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static bool ExistsInCurrentDataVersion
		{
			get
			{
				string sqlText = "SELECT object_id('" + Schema.TableName + "')";
				return Db.Connection.ExecuteScalar(sqlText) != DBNull.Value; // This is the most efficient way to find out if the table exists.
			}
		}

		public static ZQuery GetLevyRatesQuery(ZGuid classificationForDutyRatePK, ZDateTime dateForDutyRate)
		{
			var query = new ZQuery(NZCClassificationLevyRateSchema.L0_U0_Classification, classificationForDutyRatePK);
			query.AddToFilter(NZCClassificationLevyRateSchema.L0_DateActiveFrom, SQLComparisonOperator.LessThanOrEqualTo, dateForDutyRate);

			var endDateQuery = new ZQuery(NZCClassificationLevyRateSchema.L0_DateActiveTo, SQLComparisonOperator.GreaterThanOrEqualTo, dateForDutyRate);
			endDateQuery.AddToFilter(JoinCondition.Or, NZCClassificationLevyRateSchema.L0_DateActiveTo, ZDateTime.Empty);
			query.AddToFilter(endDateQuery);
			query.OrderBy = NZCClassificationLevyRateSchema.L0_LevyCode.Name;

			return query;
		}
	}
}
