using System.Data;
using System.Text;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class PopulateUnitCodeTransformation : DataTransformation, IDataTransformationTask
	{
		public PopulateUnitCodeTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"SELECT COUNT(*)
FROM NamedEntityClassification
WHERE NEC_Class = 'CACUSTOMUOM'
AND NEC_Language = 'EN'";
			var count = DbHelper.ExecuteScalar(trans, sql);
			if ((int)count == 0)
			{
				var dataSql = new StringBuilder(@"INSERT NamedEntityClassification(NEC_PK, NEC_Name, NEC_Class, NEC_Language, NEC_Code)
VALUES");

				dataSql.Append(@"
(newid(), 'unit', 'CACUSTOMUOM', 'EN', 'NMB'),
(newid(), 'kilogram', 'CACUSTOMUOM', 'EN', 'KGM'),
(newid(), 'kg', 'CACUSTOMUOM', 'EN', 'KGM'),
(newid(), 'square meter', 'CACUSTOMUOM', 'EN', 'MTK'),
(newid(), 'square metre', 'CACUSTOMUOM', 'EN', 'MTK'),
(newid(), 'metric ton', 'CACUSTOMUOM', 'EN', 'TNE'),
(newid(), 'metric tonne', 'CACUSTOMUOM', 'EN', 'TNE'),
(newid(), '100kg', 'CACUSTOMUOM', 'EN', 'DTN'),
(newid(), 'watt', 'CACUSTOMUOM', 'EN', 'WTT'),
(newid(), 'piece', 'CACUSTOMUOM', 'EN', 'PCE')");
				var insertSql = dataSql.ToString();
				DbHelper.ExecuteNonQuery(trans, insertSql);
			}
		}
	}
}
