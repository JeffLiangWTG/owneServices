using System;

namespace Enterprise.MasterData.Common
{
	public interface IDeduplicationStrategy
	{
		Func<int> MaximumPKs { get; }
	}
}
