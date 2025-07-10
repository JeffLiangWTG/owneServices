using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public static class DataServiceQueryExtensions
	{
		public static async Task<IEnumerable<T>> ExecuteAsync<T>(this IQueryable<T> query, int timeoutInSeconds = 300)
		{
			Argument.Argument.NotNull(query, nameof(query));

			if (typeof(DataServiceQuery<T>).IsAssignableFrom(query.GetType()))
			{
				return await ((DataServiceQuery<T>)query).ExecuteAsync().TimeoutAfter(new TimeSpan(0, 0, timeoutInSeconds));
			}
			else
			{
				return query.AsEnumerable();
			}
		}

		public static IQueryable<T> Expand<T, TElement>(this IQueryable<T> query, Expression<Func<T, TElement>> navigationPropertyAccessor)
		{
			Argument.Argument.NotNull(query, nameof(query));

			var result = query;
			if (typeof(DataServiceQuery<T>).IsAssignableFrom(query.GetType()))
			{
				result = ((DataServiceQuery<T>)query).Expand(navigationPropertyAccessor);
			}
			return result;
		}

		public static IQueryable<T> Expand<T>(this IQueryable<T> query, string path)
		{
			Argument.Argument.NotNull(query, nameof(query));
			Argument.Argument.NotNullOrEmpty(path, nameof(path));

			var result = query;
			if (typeof(DataServiceQuery<T>).IsAssignableFrom(query.GetType()))
			{
				result = ((DataServiceQuery<T>)query).Expand(path);
			}
			return result;
		}

		public static IQueryable<T> AddQueryOption<T>(this IQueryable<T> query, string name, object value)
		{
			Argument.Argument.NotNull(query, nameof(query));

			var result = query;
			if (typeof(DataServiceQuery<T>).IsAssignableFrom(query.GetType()))
			{
				result = ((DataServiceQuery<T>)query).AddQueryOption(name, value);
			}
			return result;
		}

		static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
			using (var timeoutCancellationTokenSource = new CancellationTokenSource())
			{
				var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
				if (completedTask == task)
				{
					timeoutCancellationTokenSource.Cancel();
					return await task;
				}
				else
				{
					throw new TimeoutException("The operation has timed out.");
				}
			}
		}
	}
}
