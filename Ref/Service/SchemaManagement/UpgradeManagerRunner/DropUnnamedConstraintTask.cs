using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DropUnnamedConstraintTask : DataTransformation, IDataTransformationTask
	{
		public DropUnnamedConstraintTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			DropDefaultConstraint(trans, "RefCusProcedure", "ZZ6_Group");
			DropDefaultConstraint(trans, "RefCusTariffRule", "ZZ1_PK");
			DropDefaultConstraint(trans, "RefCusTariffRule", "ZZ1_Applied");
			DropDefaultConstraint(trans, "RefCusConditionValueType", "ZX4_IsFormula");
			DropDefaultConstraint(trans, "RefCusNomenclatureGroupType", "ZZ9_PK");
			DropDefaultConstraint(trans, "RefCusTradeGroup", "ZZA_StartDate");
			DropDefaultConstraint(trans, "RefCusTradeGroup", "ZZA_EndDate");
			DropDefaultConstraint(trans, "RefCusTradeGroupCountry", "ZZB_StartDate");
			DropDefaultConstraint(trans, "RefCusTradeGroupCountry", "ZZB_EndDate");
			DropDefaultConstraint(trans, "RefDbVersionControl", "Deleted");
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
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
