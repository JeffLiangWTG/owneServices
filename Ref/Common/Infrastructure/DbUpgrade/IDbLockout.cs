using System;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public interface IDbLockout
	{
		IDisposable Acquire();
	}
}
