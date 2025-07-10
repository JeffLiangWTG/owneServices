using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PopulateRefDataGroupingTask : DataTransformation, IDataTransformationTask
	{
		public PopulateRefDataGroupingTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RefDataGrouping')
BEGIN
	CREATE TABLE RefDataGrouping
	(
		ZZZ_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefDataGrouping_ZZZ_PK DEFAULT (NEWID()),
		ZZZ_DataGrouping VARCHAR(3) NOT NULL,
		ZZZ_Description VARCHAR(500) NOT NULL,
		ZZZ_ZZZ_NKGrouping VARCHAR(3) NULL,
		CONSTRAINT PK_RefDataGrouping PRIMARY KEY CLUSTERED( ZZZ_PK ASC ),
		CONSTRAINT CK_RefDataGrouping_ZZZ_DataGrouping CHECK (ZZZ_DataGrouping <>''),
		CONSTRAINT CK_RefDataGrouping_ZZZ_Description CHECK (ZZZ_Description <>''),
	)

	INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_NKGrouping)
	VALUES
	(newid(),'BD','Bangladesh','WCO'),
	(newid(),'GB','United Kingdom','EUN'),
	(newid(),'LK','Sri Lanka','WCO'),
	(newid(),'ZZ','Universal Customs',NULL),
	(newid(),'FJ','Fiji','WCO'),
	(newid(),'EUN','European Union',NULL),
	(newid(),'AU','Australia','AU'),
	(newid(),'US','United States','US'),
	(newid(),'CA','Canada','CA'),
	(newid(),'VU','Vanuatu','WCO'),
	(newid(),'SB','Solomon Islands','WCO'),
	(newid(),'WCO','World Trade Organization (WCO)',NULL),
	(newid(),'JP','Japan','WCO'),
	(newid(),'IT','Italy','EUN'),
	(newid(),'DE','Germany','EUN'),
	(newid(),'NZ','New Zealand','NZ'),
	(newid(),'PG','Papua New Guinea','WCO'),
	(newid(),'ZA','South Africa','ZA')

	INSERT RefDbVersionControl (ParentPK, ParentCode, LastUpdatedUTC, Deleted)
	SELECT ZZZ_PK, 'ZZZ', sysutcdatetime(), 0 FROM RefDataGrouping
END";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
