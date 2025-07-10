using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class DataProcessingResultFixture
{
	[Test]
	public void IndexOnDPR_ParentPK()
	{
		var connectionString = TestConnectionString.GetAdmin(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging));
		using var connection = new SqlConnection(connectionString);
		connection.Open();
		var sql = @"SELECT COUNT(1) FROM sys.indexes i JOIN sys.index_columns ic ON ic.index_id=i.index_id AND ic.object_id=i.object_id
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE i.object_id = OBJECT_ID('DataProcessingResult') AND i.name = 'UX_DataProcessingResult_DPR_ParentPK' AND c.name='DPR_ParentPK';";
		using var cmd = connection.CreateCommand();
		cmd.CommandText = sql;
		var result = (int)cmd.ExecuteScalar();
		Assert.AreEqual(1, result, "DPR table should have index [UX_DataProcessingResult_DPR_ParentPK] on [DPR_ParentPK] column only.");
	}
}
