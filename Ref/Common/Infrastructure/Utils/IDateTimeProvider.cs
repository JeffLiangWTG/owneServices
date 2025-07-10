using System;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public interface IDateTimeProvider
	{
		DateTime GetUTCNow();
	}
}
