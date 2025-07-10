using System.Collections.Concurrent;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public interface IRateEntryFilterValueCache
	{
		RateEntryFilterValue this[ZGuid key] { get; set; }
	}

	/// <summary>
	/// Allows storage of the rate entry filter values temporarily.
	/// It stores them in between the Costing controller preparing them and the
	/// costing form displaying the filters.
	///
	/// This is a read-once cache. Once read, the value is removed.
	/// </summary>
	public class RateEntryFilterValueCache : IRateEntryFilterValueCache
	{
		public static IRateEntryFilterValueCache Instance
		{
			get
			{
				return ObjectFactory.Get<IRateEntryFilterValueCache>();
			}
		}

		public RateEntryFilterValueCache()
		{
		}

		readonly ReadOnceCache<ZGuid, RateEntryFilterValue> cache = new ReadOnceCache<ZGuid, RateEntryFilterValue>();

		public RateEntryFilterValue this[ZGuid guid]
		{
			get
			{
				return cache[guid];
			}
			set
			{
				cache[guid] = value;
			}
		}

		// The functionality of a cache that only lets things be read once is
		// kept separate in case anyone wants to resuse something like this in
		// the future.
		class ReadOnceCache<TIn, TOut>
		{
			readonly ConcurrentDictionary<TIn, TOut> store = new ConcurrentDictionary<TIn, TOut>();
			public TOut this[TIn guid]
			{
				get
				{
					TOut result = default;
					store.TryRemove(guid, out result);

					return result;
				}
				set
				{
					store[guid] = value;
				}
			}
		}
	}
}
