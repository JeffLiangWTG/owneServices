using System;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class InputBlockControlGeneratorHelper
	{
		public static void SetupBlockBDetails(this IABIControlMessageBlockB blockB, ICusEntryHeaderMessageAttachee header)
		{
			if (header.IsRemoteLocationFiling)
			{
				blockB.PreparerDistrictPort = header.PreparerDistrictPort;
				blockB.PreparerFilerCode = blockB.EntryFilerCode;
				blockB.PreparerOfficeCode = header.PreparerOfficeCode;
				blockB.PreparerIndicator = "1";
			}
		}

		public static string GetProcessingDistrictPortCode(GlbBranch branch)
		{
			return USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
		}
	}
}
