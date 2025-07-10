using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class UNDGPrefixHelper
	{
		static readonly ImmutableArray<string> KnownID_UNNOs = ImmutableArray.Create("8000", "8001");
		static readonly string DefaultUNPrefix = "UN";
		static readonly string IDPrefix = "ID";

		public static ZString GetUnnoPrefix(UNDGSubstance substance)
		{
			if (substance?.StandardSubstance is UNDGSubstanceCFR cfrSubstance)
			{
				return GetUnnoPrefixCFR(cfrSubstance);
			}

			return KnownID_UNNOs.Contains(substance?.DG_UNNO) ? IDPrefix : DefaultUNPrefix;
		}

		public static ZString GetUnnoPrefixCFR(UNDGSubstanceCFR cfrSubstance)
		{
			return cfrSubstance == null || string.IsNullOrEmpty(cfrSubstance.CFR_Prefix) ? (ZString)DefaultUNPrefix : cfrSubstance.CFR_Prefix.ToUpper();
		}
	}
}
