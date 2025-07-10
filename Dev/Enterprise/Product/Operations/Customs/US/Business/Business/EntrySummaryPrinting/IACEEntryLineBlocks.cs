using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public interface IACEEntryLineBlocks
	{
		AENS40 aens40 { get; }
		AENS41 aens41 { get; }
		List<AENS43> aens43 { get; }
		List<AENS44> aens44 { get; }
		List<AENS47> aens47 { get; }
		List<AENS50> aens50 { get; }
		AENS51 aens51 { get; }
		List<AENS52> aens52 { get; }
		List<AENS53> aens53 { get; }
		List<AENS54> aens54 { get; }
		AENS60 aens60 { get; }
		List<IChargeBlock> aens62 { get; }
	}
}
