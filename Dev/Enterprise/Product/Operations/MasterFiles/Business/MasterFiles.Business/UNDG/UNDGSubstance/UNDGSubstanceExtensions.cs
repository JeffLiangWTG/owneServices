using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class UNDGSubstanceExtensions
	{
		public static ZString GetUnnoPrefix(this UNDGSubstance substance) => UNDGPrefixHelper.GetUnnoPrefix(substance);
	}
}
