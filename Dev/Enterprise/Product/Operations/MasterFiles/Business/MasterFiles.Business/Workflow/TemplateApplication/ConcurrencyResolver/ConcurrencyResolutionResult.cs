using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.ConcurrencyResolver
{
	public class ConcurrencyResolutionResult<T> where T : BusinessObject
	{
		public T ResultingRecord { get; }
		public IEnumerable<T> RecordsMarkedForDeletion { get; }
		public bool Resolved { get; }

		ConcurrencyResolutionResult(T resultingRecord, IEnumerable<T> recordsMarkedForDeletion)
		{
			Resolved = resultingRecord != null;
			ResultingRecord = resultingRecord;
			RecordsMarkedForDeletion = recordsMarkedForDeletion;
		}

		public static ConcurrencyResolutionResultBuilder<T> Builder() => new ConcurrencyResolutionResultBuilder<T>();

		public sealed class ConcurrencyResolutionResultBuilder<P> where P : BusinessObject
		{
			readonly List<P> _recordsMarkedForDeletion = new List<P>();
			P ResultingRecord { get; set; }
			public ConcurrencyResolutionResultBuilder<P> AddForDeletion(P recordToDelete)
			{
				recordToDelete.Delete();
				_recordsMarkedForDeletion.Add(recordToDelete);
				return this;
			}

			public ConcurrencyResolutionResultBuilder<P> ResolvedRecord(P resolvedRecord)
			{
				ResultingRecord = resolvedRecord;
				return this;
			}

			public ConcurrencyResolutionResult<P> Build()
			{
				return new ConcurrencyResolutionResult<P>(ResultingRecord, _recordsMarkedForDeletion);
			}
		}
	}
}
