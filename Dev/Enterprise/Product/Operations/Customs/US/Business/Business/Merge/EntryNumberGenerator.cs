using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class EntryNumberGenerator
	{
		public static bool TryGetNextEntryNumber(GlbBranch entryBranch, ZString entryFilerCode, out ZString entryNumber)
		{
			entryNumber = ZString.Empty;
			var result = false;
			if (entryBranch != null)
			{
				var setting = ACEEntryStmNumsSetting.New(entryBranch, entryFilerCode, true);
				result = setting != null && setting.TryGetNextCustomsNumber(entryBranch.Factory, entryBranch, out entryNumber);
			}
			return result;
		}
	}
}
