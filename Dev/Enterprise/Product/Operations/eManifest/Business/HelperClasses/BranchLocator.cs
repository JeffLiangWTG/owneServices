using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.eManifest.Business
{
	static class BranchLocator
	{
		public static GlbBranch GetMatchingBranch(string origin, string destination)
		{
			GlbBranch result = null;
			var branches = GlbCompany.CurrentCompany.Branches.Where(x => x.GB_IsActive);

			foreach (var searchRule in SystemDataRegistry.Instance.ShipmentImportBranchRules.Value.GetBranchSearchRules())
			{
				switch (searchRule)
				{
					case ImportBranchRule.BranchSearchRules.FromOriginLoadPort:
						result = GetBranchFromUnloco(branches, origin);
						break;
					case ImportBranchRule.BranchSearchRules.FromOriginLoadPortCountry:
						result = GetBranchFromCountry(branches, origin);
						break;
					case ImportBranchRule.BranchSearchRules.FromDestinationDischargePort:
						result = GetBranchFromUnloco(branches, destination);
						break;
					case ImportBranchRule.BranchSearchRules.FromDestinationDischargePortCountry:
						result = GetBranchFromCountry(branches, destination);
						break;
					case ImportBranchRule.BranchSearchRules.Any:
						result = GlbBranch.CurrentBranch;
						break;
					case ImportBranchRule.BranchSearchRules.Cancel:
						return null;
				}

				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		static GlbBranch GetBranchFromUnloco(IEnumerable<GlbBranch> branches, ZString unloco)
		{
			return branches.FirstOrDefault(branch => branch.GB_RL_NKHomePort == unloco ||
				branch.ExtraPorts.Cast<GlbBranchExtraPorts>()
					.Any(extraPort => extraPort.GY_RL_NKAdditionalBranchRelatedPort == unloco));
		}

		static GlbBranch GetBranchFromCountry(IEnumerable<GlbBranch> branches, ZString unloco)
		{
			return branches.FirstOrDefault(branch => branch.GB_RL_NKHomePort.SubstringSafe(0, 2) == unloco.SubstringSafe(0, 2) ||
				branch.ExtraPorts.Cast<GlbBranchExtraPorts>()
					.Any(extraPort => extraPort.GY_RL_NKAdditionalBranchRelatedPort.SubstringSafe(0, 2) == unloco.SubstringSafe(0, 2)));
		}
	}
}
