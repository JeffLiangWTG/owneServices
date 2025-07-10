using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class RenameRefCusRateCodeLanguageColumns : RenameColumnsDataTransformation
	{
		public RenameRefCusRateCodeLanguageColumns(int version) : base(version) { }

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
			{
				new RenameColumnObject("ZXB_PK","ZXC_PK"),
				new RenameColumnObject("ZXB_ZX6_NKLanguage","ZXC_ZX6_NKLanguage"),
				new RenameColumnObject("ZXB_ZY1_RateCode","ZXC_ZY1_RateCode"),
				new RenameColumnObject("ZXB_Description","ZXC_Description")
			};
		}

		protected override string GetTableName()
		{
			return "RefCusRateCodeLanguage";
		}
	}
}
