using System;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	sealed class ABIApplicationControlGenerator : ABIApplicationControlGenerator<APLA, APLZ>
	{
		public ABIApplicationControlGenerator(GlbBranch branch)
			: base(branch)
		{
			A.BatchNumber = 1;
			A.NationalImporterLiquidationNILIndicator = USCustomsDataRegistry.Instance.NationalImporterLiquidationIndicator.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty) ? "NIL" : "";
			Z.BatchNumber = 1;
		}
	}
}
