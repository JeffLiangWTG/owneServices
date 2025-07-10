using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator : IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator
	{
		public void UpdateConsignmentsDestinationDetails(IEnumerable<ZGuid> consignmentPKs)
		{
			if (consignmentPKs != null && consignmentPKs.Any())
			{
				var consignmentsAsDataTable = new DataTable();

				consignmentsAsDataTable.Locale = CultureInfo.InvariantCulture;
				consignmentsAsDataTable.Columns.Add((NoResString)"Value", typeof(Guid)); // Part of SQL code

				foreach (var consignmentPK in consignmentPKs)
				{
					var pk = consignmentPK.ToGuid();
					consignmentsAsDataTable.Rows.Add(pk);
				}

				ExecuteStoredProcedureToUpdateConsignments(consignmentsAsDataTable);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void ExecuteStoredProcedureToUpdateConsignments(DataTable consignmentsAsDataTable)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager()) // invoke Stored Procedure.
			{
				using (var command = Db.Connection.Command("UpdateConsignmentsWithDepotAddressAndCarrierInfo")) // invoke Stored Procedure.
				{
					command.CommandType = CommandType.StoredProcedure;
					command.AddTableValuedParameter("@ConsignmentPks", "dbo.TVP_uniqueidentifier", consignmentsAsDataTable);
					command.AddParameter("@OnlyPopulatesEmptyLMCDepotDetails", SqlDbType.Bit, HVLVDataRegistry.Instance.HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails.Value);
					command.ExecuteNonQuery();
				}

				transactionManager.CommitTransaction();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void UpdateConsignmentsDestinationDetailsByClusterKeys(IEnumerable<int> clusterKeys)
		{
			if (clusterKeys.Any())
			{
				var clusterKeyQuery = new ZQuery(HVLVConsignmentSchema.HVC_ClusterKey, clusterKeys);

				var sqlScript = string.Format(@"
DECLARE @ConsignmentPks dbo.TVP_uniqueidentifier;
INSERT INTO @ConsignmentPks
SELECT HVC_PK FROM dbo.HVLVConsignment Where {0}
EXEC UpdateConsignmentsWithDepotAddressAndCarrierInfo @ConsignmentPks, @OnlyPopulatesEmptyLMCDepotDetails", clusterKeyQuery.LiteralTextSqlFormatted); // SQL Query

				using (var command = Db.Connection.Command(sqlScript)) // invoke Stored Procedure.
				{
					command.CommandType = CommandType.Text;
					command.AddParameter("@OnlyPopulatesEmptyLMCDepotDetails", SqlDbType.Bit, HVLVDataRegistry.Instance.HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails.Value);
					command.ExecuteNonQuery();
				}
			}
		}
	}
}
