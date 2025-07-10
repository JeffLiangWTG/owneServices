using System;

namespace Enterprise.MasterData.Common
{
	public interface IDeduplicationGlowObject
	{
		Guid PK { get; }
		string CountryCode { get; }
		string TablePrefix { get; }
		bool IsInDatabase { get; }
		Type BizoType { get; }
	}
}
