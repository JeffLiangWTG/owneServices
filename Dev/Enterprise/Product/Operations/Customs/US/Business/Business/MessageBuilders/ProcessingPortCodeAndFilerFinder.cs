using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public static class ProcessingPortCodeAndFilerFinder
	{
		public static ZString GetProcessingPortCodeFromRegistry(GlbBranch branch)
		{
			var result = ZString.Empty;

			if (branch != null)
			{
				result = USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			}

			return result;
		}

		public static ZString GetPrcessingPortCodeFromRegistryForCompany(GlbCompany company)
		{
			var result = ZString.Empty;
			if (company != null)
			{
				foreach (var branch in company.Branches)
				{
					result = USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
					if (result != ZString.Empty)
					{
						break;
					}
				}
			}

			return result;
		}

		public static ZString GetEntryFilerCodeFromRegistry(GlbBranch branch)
		{
			var result = ZString.Empty;

			if (branch != null)
			{
				var filer = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
				result = filer.EntryFilerCode;
			}

			return result;
		}
	}
}
