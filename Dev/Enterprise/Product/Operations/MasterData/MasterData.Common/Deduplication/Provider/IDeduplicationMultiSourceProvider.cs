using System;
using System.Collections.Generic;

namespace Enterprise.MasterData.Common
{
	public interface IDeduplicationMultiSourceProvider
	{
		Type GlowType { get; }
		DeduplicationDisplayMode DisplayModeForType { get; }
		string GroupNameForType { get; }
		IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false);
		IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false);
	}

	public interface IDeduplicationProvider
	{
		string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short);
	}
}
