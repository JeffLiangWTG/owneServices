using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public interface ISafeRepository
	{
		IQueryable<T> Get<T>() where T : class;
		IQueryable<T> GetLatest<T>() where T : class;
		void Add<T>(T data);
		void Update<T>(T data);
		void Delete<T>(T data);
		Task<bool> ForceDelete<T>(IEnumerable<Guid> ids) where T : class;
		Task<bool> SaveChangesAysnc();
		Task<DateTimeOffset> GetLastestCreatedTimeUTCAsync<T>() where T : class;
		Task<DateTimeOffset> GetLatestUpdatedTimeUTCAsync(string nameOrPrefix);
		IQueryable<T> GetWithOptimizedExpand<T>() where T : class;
		IQueryable<T> GetCreatedBetween<T>(DateTimeOffset? afterCreatedTimeUTC, DateTimeOffset beforeOrEqualCreatedTimeUTC) where T : class;
		Task<bool> BatchExpire<T>(IEnumerable<Guid> ids, DateTimeOffset expiredTime) where T : class;
		Task<bool> BatchInActive<T>(IEnumerable<Guid> ids) where T : class;
		Task<bool> BatchDelete<T>(IEnumerable<Guid> ids) where T : class;
		int AffectedRecords { get; }
		Task<IEnumerable<CloneProcessResult>> CloneExistingRecordChildrenIntoNewRecord<T>(IEnumerable<CloneProcessObject> cloneProcessObjects) where T : class;
		void SavePersistentObjects();
		void TrackNonPersistentFlattenObject(INonPersistentBusinessObjectFlatten flattenObject);
		IEnumerable<object> GetAllPersistentObjects();
	}
}
