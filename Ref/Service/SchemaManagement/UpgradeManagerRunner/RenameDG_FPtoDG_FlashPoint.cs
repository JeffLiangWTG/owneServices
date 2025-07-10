using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RenameDG_FPtoDG_FlashPoint : RenameColumnsDataTransformation
	{
		public RenameDG_FPtoDG_FlashPoint(int version)
		: base(version)
		{ }

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
			{
				new RenameColumnObject("DG_FP", "DG_FlashPoint"),
			};
		}

		protected override string GetTableName()
		{
			return "UNDGSubstance";
		}
	}
}
