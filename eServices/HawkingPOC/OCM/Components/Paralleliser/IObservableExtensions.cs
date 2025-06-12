using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace OcmPoc.Components.Paralleliser
{
	static class IObservableExtensions
	{
		public static IObservable<TSource> SortContiguousBy<TSource>(this IObservable<TSource> source, Func<TSource, long> indexSelector)
		{
			return Observable.Create<TSource>(observer =>
			{
				var buffer = new ConcurrentDictionary<long, TSource>();
				var nextIndex = 0L;

				var subscription = source.Subscribe(
					onNext: item =>
						{
							buffer.TryAdd(indexSelector(item), item);
							while (buffer.TryRemove(nextIndex, out TSource nextItem))
							{
								observer.OnNext(nextItem);
								nextIndex++;
							}
						},
					onCompleted: () => observer.OnCompleted(),
					onError: ex => observer.OnError(ex));

				return () => subscription.Dispose();
			});
		}
	}
}
