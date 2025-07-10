using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class DropUnnamedConstraintTask : DataTransformation, IDataTransformationTask
	{
		public DropUnnamedConstraintTask(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			DropDefaultConstraint(trans, "DataProcessingInformation", "DPI_ID");
			DropDefaultConstraint(trans, "RefCusConditionValueType", "ZX4_IsFormula");
			DropDefaultConstraint(trans, "RefCusNomenclatureGroupType", "ZZ9_PK");
			DropDefaultConstraint(trans, "RefCusTradeGroup", "ZZA_StartDate");
			DropDefaultConstraint(trans, "RefCusTradeGroup", "ZZA_EndDate");
			DropDefaultConstraint(trans, "RefCusTradeGroupCountry", "ZZB_StartDate");
			DropDefaultConstraint(trans, "RefCusTradeGroupCountry", "ZZB_EndDate");
			DropDefaultConstraint(trans, "RefTariffSource", "SRC_PK");
			DropDefaultConstraint(trans, "RefTariffSource", "SRC_SourceDate");
			DropDefaultConstraint(trans, "SourceData", "ID");
			DropDefaultConstraint(trans, "SourceData", "ContentText");
			DropDefaultConstraint(trans, "SourceData", "Status");
			DropDefaultConstraint(trans, "SourceData", "CreatedTime");
		}

		static void DropDefaultConstraint(IDbTransaction trans, string tableName, string columnName)
		{
			Argument.NotNull(trans, nameof(trans));
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = SQLHelper.GetDropUnnamedConstraint(tableName, columnName);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
