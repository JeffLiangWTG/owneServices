using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Customs.NO.Business;

public sealed class EmmaMessageEntryHeaderWrapper(CusEntryHeader entryHeader)
{
	readonly CusEntryHeader entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));

	public IEnumerable<CusEntryHeader> EntryHeaders => [entryHeader];
}
