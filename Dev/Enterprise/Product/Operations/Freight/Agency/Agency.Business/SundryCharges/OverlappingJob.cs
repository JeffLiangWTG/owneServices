using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class OverlappingJob
	{
		OverlappingJob(DynamicBusinessObject row)
		{
			JobNumber = new ZString(row[JobSundryChargesSchema.Constants.D4_JobNumber]);
			FromDate = new ZDateTime(row[JobSundryChargesSchema.Constants.D4_FromDate]);
			ToDate = new ZDateTime(row[JobSundryChargesSchema.Constants.D4_ToDate]);
		}

		public static OverlappingJob[] Load(SundryCharges sundry)
		{
			if (sundry == null)
			{
				throw new ArgumentNullException(nameof(sundry));
			}

			var sql = string.Format(CultureInfo.InvariantCulture,
				"select {0}, {1}, {2} " +
				"from {3} " +
				"where " +
					"{4} != @PK and " +
					"{5} = @BillToPartyPK and " +
					"{1} <= @ToDate and " +
					"{2} >= @FromDate" +
				"",
				JobSundryChargesSchema.Constants.D4_JobNumber, // 0
				JobSundryChargesSchema.Constants.D4_FromDate, // 1
				JobSundryChargesSchema.Constants.D4_ToDate, // 2
				JobSundryChargesSchema.Constants.TableName, //3
				JobSundryChargesSchema.Constants.PK, // 4
				JobSundryChargesSchema.Constants.D4_OH_BillToParty); // 5

			if (sundry.D4_OH_BillToParty.IsEmpty ||
				!sundry.D4_FromDate.IsValidSmallDateTime ||
				!sundry.D4_ToDate.IsValidSmallDateTime)
			{
				return null;
			}
			else
			{
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@PK", sundry.PK, JobSundryChargesSchema.PK);
				parameters.Add("@BillToPartyPK", sundry.D4_OH_BillToParty, JobSundryChargesSchema.D4_OH_BillToParty);
				parameters.Add("@FromDate", sundry.D4_FromDate, JobSundryChargesSchema.D4_FromDate);
				parameters.Add("@ToDate", sundry.D4_ToDate, JobSundryChargesSchema.D4_ToDate);

				DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(sundry.Factory);
				collection.Load(sql, parameters);

				OverlappingJob[] result = new OverlappingJob[collection.Count];
				for (int i = 0; i < collection.Count; i++)
				{
					result[i] = new OverlappingJob(collection[i]);
				}

				return result;
			}
		}

		public ZString JobNumber { get; private set; }
		public ZDateTime FromDate { get; private set; }
		public ZDateTime ToDate { get; private set; }
	}
}


