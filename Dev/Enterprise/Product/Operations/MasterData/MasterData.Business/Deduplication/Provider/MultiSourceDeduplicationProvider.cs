using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.Business
{
	public class MultiSourceDeduplicationProvider : IDeduplicationMultiSourceProvider
	{
		readonly DeduplicationProvider provider;

		public MultiSourceDeduplicationProvider()
		{
		}

		public MultiSourceDeduplicationProvider(DeduplicationProvider provider, Type glowType, string groupNameForType)
		{
			this.provider = provider;
			GlowType = glowType;
			GroupNameForType = groupNameForType;
		}

		public Type GlowType { get; }

		public DeduplicationDisplayMode DisplayModeForType { get; } = DeduplicationDisplayMode.List;

		public string GroupNameForType { get; }

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			var prefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(columnNames.First());
			var bizoProvider = provider.GetBizOProvider(prefix);

			return bizoProvider?.GetComparisonSource(master, pk, columnNames, standardizeDomains);
		}
	}
}
