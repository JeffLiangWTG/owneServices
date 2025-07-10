namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.Data;

	public static class CmrAuReferenceFileUpdateMutex
	{
		/// <summary>
		/// CMR AU Reference Database can be shared amonst different Enterprise systems in the same server.
		/// So to control concurrency, an application lock is used as a semaphore control.
		/// </summary>
		public static bool AcquireUpdateLock(DbConnection connection, TimeSpan timeout)
		{
			return ReferenceFileUpdateMutex.AcquireUpdateLock(connection, timeout, RefDbTypeEnum.Customs, Core.Constants.CountryCodes.Australia, CMRReferenceFileUpdateLog.ReferenceTableName);
		}
	}
}
