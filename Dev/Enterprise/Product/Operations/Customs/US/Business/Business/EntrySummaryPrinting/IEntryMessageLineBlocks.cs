using System.Collections.Generic;

using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public interface IEntryMessageLineBlocks
	{
		ENS40 ens40 { get; }
		List<ENS43> ens43 { get; }
		ENS50 ens50 { get; }
		ENS51 ens51 { get; }
		ENS52 ens52 { get; }
		ENS57 ens57 { get; }
		ENS60 ens60 { get; }
		List<ENS62> ens62 { get; }
		ENS70 ens70 { get; }
		ENS80 ens80 { get; }
		List<ENS81> ens81 { get; }
	}
}
