using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;

#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface IDisposableLogger : IDisposable, ILogger { }

	public static class Extensions
	{
		public static bool PKEquals<T>(this T bizO, T other) where T : BusinessObject
		{
			return BusinessObjectEqualityComparer<T>.PKOnlyComparer.Equals(bizO, other);
		}

		public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> retriever)
		{
			TValue result;
			if (!dictionary.TryGetValue(key, out result))
			{
				dictionary[key] = result = retriever();
			}

			return result;
		}

		#region Date/minutes conversions

		public static string ToHoursAndMinutesString(this TimeSpan timespan)
		{
			if (timespan.Milliseconds != timespan.Add(TimeSpan.FromTicks(1)).Milliseconds)
			{
				// There could be a datetime precision difference between .NET Framework and .NET
				// When that happens, timespan is 1 Tick less in Winzor than that in CW1, which causes this rounding issue.
				// See WI00709297 for details.
				timespan = timespan.Add(TimeSpan.FromTicks(1));
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", Math.Truncate(timespan.TotalHours), timespan.Minutes.ToString("D2", CultureInfo.InvariantCulture));
		}

		public static string ToStandardDateTimeString(this ZDateTime datetime)
		{
			return datetime.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Default string")]
		public static ZDateTimeOffset ToLocalBranchTimeOffset(this ZDateTime date, BusinessObjectFactory factory = null)
		{
			if (date.IsValid)
			{
				GlbBranch branch = null;
				using (factory?.AddDiagnosisForFactoryQueryCacheWhenLoadingEnvCurrentBranchOrCompany())
				{
					branch = factory != null ? GlbBranch.GetCurrentBranch(factory) : GlbBranch.CurrentBranch;
				}
				if (branch == null)
				{
					var information = "<Unknown>";
					//BaseEnvironment.cs internal readonly IUserContextManager userContextManager;
					var userContextManager = typeof(BaseEnvironment).GetField("userContextManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Env.Instance);
					if (userContextManager != null)
					{
						if (userContextManager.GetType() == typeof(MultiThreadUserContextManager))
						{
							//StackTrace setMasterUserContextNoBranchStackTrace;
							var st1 = typeof(MultiThreadUserContextManager).GetField("setMasterUserContextNoBranchStackTrace", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(userContextManager);
							//StackTrace contextSecurityNoBranchStackTrace;
							var st2 = typeof(MultiThreadUserContextManager).GetField("contextSecurityNoBranchStackTrace", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(userContextManager);
							//Factory information
							var str3 = factory != null ? factory.GetDebugInformation("GlbBranch", Env.CurrentBranch.PK) : string.Empty;
							information = String.Format(
								"\r\nStack Trace where SetMasterUserContext was called and Branch was null: {0}\r\nStack trace where ContextSecurity.get was called and Branch was null in the result: {1}\r\nFactory information: {2}"
								, st1, st2, str3);
						}
					}
					throw new NullReferenceException(string.Format("CurrentBranch is null in ToLocalBranchTimeOffset.\r\nAdded information: {0}", information));
				}
				return ToLocationTime(date, branch.HomePort);
			}
			else
			{
				return new ZDateTimeOffset(date);
			}
		}

		public static ZDateTime ToLocalBranchTime(this ZDateTime date, BusinessObjectFactory factory = null)
		{
			if (date.IsValid)
			{
				return ToLocalBranchTimeOffset(date, factory).ToZDateTime();
			}
			else
			{
				return date;
			}
		}

		public static ZDateTimeOffset ToLocationTime(this ZDateTime utcDateTime, RefUNLOCO location)
		{
			if (!utcDateTime.IsValid)
			{
				return new ZDateTimeOffset(utcDateTime);
			}
			else if (location != null)
			{
				var timeZone = location.TimeZoneSet;
				if (timeZone != null)
				{
					var calculationTimeZone = timeZone.GetCalculationTimeZone();
					var dateTime = utcDateTime.ToDateTime();
					return new ZDateTimeOffset(calculationTimeZone.ToLocalTime(dateTime), calculationTimeZone.GetUtcOffsetBasedOnUtc(dateTime));
				}
				else
				{
					return UtcToDateTimeOffset(utcDateTime);
				}
			}
			else
			{
				// Why is this not ToCurrentLocationTime as when location has no TimeZoneSet?
				return new ZDateTimeOffset(utcDateTime, DateTimeKind.Unspecified);
			}
		}

		public static ZDateTimeOffset UtcToDateTimeOffset(this ZDateTime utcDateTime)
		{
			var dateTime = utcDateTime.ToDateTime();
			return new ZDateTimeOffset(Env.Time.GetLocalTimeFromUtc(dateTime), ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnUtc(dateTime));
		}

		/// <summary>
		/// Converts the ZDateTime parameter into a ZDateTimeOffset in the timezone of the specified UNLOCO.
		/// </summary>
		/// <param name="localDateTime">The ZDateTime to convert into a ZDateTimeOffset.</param>
		/// <param name="locationForTimezone">The UNLOCO whose timezone will be used. If this is null, the timezone of the currently logged-in branch is used.</param>
		public static ZDateTimeOffset ToDateTimeOffset(this ZDateTime localDateTime, RefUNLOCO locationForTimezone)
		{
			if (localDateTime.IsValid)
			{
				var timeZone = locationForTimezone?.TimeZoneSet;

				if (timeZone != null)
				{
					var calculationTimeZone = timeZone.GetCalculationTimeZone();
					var utcOffset = calculationTimeZone.GetUtcOffsetBasedOnLocal(localDateTime.ToDateTime());

					return new ZDateTimeOffset(localDateTime, DateTimeKind.Local, utcOffset);
				}
			}

			return new ZDateTimeOffset(localDateTime);
		}

		public static ZDateTime ToUniversalBranchTime(this ZDateTime date, BusinessObjectFactory factory = null)
		{
			if (date.IsValid)
			{
				var branch = factory != null ? GlbBranch.GetCurrentBranch(factory) : GlbBranch.CurrentBranch;
				return date.ToUniversalBranchTime(branch);
			}
			else
			{
				return date;
			}
		}

		static ZDateTime ToUniversalBranchTime(this ZDateTime date, GlbBranch branch)
		{
			var currentBranchZoneSet = branch?.HomePort?.TimeZoneSet;

			if (currentBranchZoneSet != null)
			{
				var calculationTimeZone = currentBranchZoneSet.GetCalculationTimeZone();
				return calculationTimeZone.ToUniversalTime(date.ToDateTime());
			}
			else
			{
				return Env.Time.GetUtcFromLocalTime(date.ToDateTime());
			}
		}

		/// <summary>
		/// Get the Event Time as local time by using the time zone of the branch that created the event.
		/// If the event has no branch, or if the event has been logged with a different time zone to the actual time of the event well there is not much we can do.
		/// Do not use this for any events unless you personally are guaranteeing that the event you are using this method on was raised assuming local time is equivalent to the currently logged in user.
		/// If you do not understand what this means... Then still don't use this method ;P
		/// </summary>
		public static ZDateTimeOffset GetEventTimeLocal(StmALog log)
		{
			var eventTime = log.SL_EventTime;
			if (!eventTime.IsValid)
			{
				return new ZDateTimeOffset(eventTime);
			}
			else if (log.SL_GB_NKBranch == EnvProxy.Instance.CurrentBranch?.Code)
			{
				return new ZDateTimeOffset(eventTime, ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnLocal(eventTime.ToDateTime()));
			}
			else
			{
				var branch = log.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, log.SL_GB_NKBranch);
				if (branch != null)
				{
					var utcTime = eventTime.ToUniversalBranchTime(branch).ToDateTime();
					return new ZDateTimeOffset(ObjectCache.DateTimeProvider.GetLocalTimeFromUtc(utcTime), ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnUtc(utcTime));
				}
				else
				{
					return new ZDateTimeOffset(eventTime); // We tried our best and couldn't do it. This might cause bad math to happen.
				}
			}
		}

		#endregion

		#region IEnumerable

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "enumeration")]    // To make easier to implicitly pass <T> from 'this IEnumerable<T>'
		public static IComparer<T> CreateFuncComparerForElements<T>(this IEnumerable<T> enumeration, Func<T, T, int> compareFunc)
		{
			return new FuncComparer<T>(compareFunc);
		}

		public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
		{
			batchSize = Math.Max(batchSize, 1);

			return items.Select((item, index) => new { item, index })
				.GroupBy(i => i.index / batchSize)
				.Select(g => g.Select(i => i.item));
		}

		public static X SameOrDefault<T, X>(this IEnumerable<T> items, Func<T, X> selector, IEqualityComparer<X> equalityComparer = null) => SameOrDefault(items.Select(selector), equalityComparer);

		public static T SameOrDefault<T>(this IEnumerable<T> items, IEqualityComparer<T> equalityComparer = null)
		{
			return SameOrDefault(items, equalityComparer, x => x, default(T));
		}

		public static bool AllSame<T, X>(this IEnumerable<T> items, Func<T, X> selector, IEqualityComparer<X> equalityComparer = null) => AllSame(items.Select(selector), equalityComparer);

		public static bool AllSame<T>(this IEnumerable<T> items, IEqualityComparer<T> equalityComparer = null)
		{
			return SameOrDefault(items, equalityComparer, x => true, false);
		}

		static X SameOrDefault<T, X>(this IEnumerable<T> items, IEqualityComparer<T> equalityComparer, Func<T, X> positive, X negative)
		{
			var equals = equalityComparer != null
				? equalityComparer.Equals
				: new Func<T, T, bool>((x, y) => Equals(x, y));

			var first = true;
			var result = default(T);

			foreach (var item in items)
			{
				if (first)
				{
					result = item;
					first = false;
				}
				else if (!equals(result, item))
				{
					return negative;
				}
			}

			return positive(result);
		}

		static string DefaultManyText => Res.GetString("847cb020-27e5-40f0-a4cb-79a4edb85a6f", "Many");

		public static ZString GetSingleValueOrManyText<T>(this IEnumerable<T> items, Func<T, ZString> getValue) => GetSingleValueOrManyText(items, getValue, DefaultManyText);

		public static ZString GetSingleValueOrManyText<T>(this IEnumerable<T> items, Func<T, ZString> getValue, ZString manyReturnValue)
		{
			var values = items.Select(x => getValue(x)).Distinct().Take(2).ToArray();
			return values.Length > 1 ? manyReturnValue : values.FirstOrDefault();
		}

		public static ZString GetSingleValueOrManyText<T, ZGuid>(this IEnumerable<T> items, Func<T, ZString> getValue, Func<T, ZGuid> getDeDuplicationForeignKeySelector) => GetSingleValueOrManyText(items, getValue, getDeDuplicationForeignKeySelector, DefaultManyText);

		public static ZString GetSingleValueOrManyText<T, ZGuid>(this IEnumerable<T> items, Func<T, ZString> getValue, Func<T, ZGuid> getDeDuplicationForeignKeySelector, ZString manyReturnValue)
		{
			var values = items.DistinctBy(x => getDeDuplicationForeignKeySelector(x)).Select(x => getValue(x)).Take(2).ToArray();
			return values.Length > 1 ? manyReturnValue : values.FirstOrDefault();
		}

		public static ZString GetSingleValueOrManyText<T, ZGuid>(this IEnumerable<T> items, Func<T, ZString> getValue, Func<T, ZGuid> getDeDuplicationForeignKeySelector, Func<T, bool> filterAfterDeDuplication) => GetSingleValueOrManyText(items, getValue, getDeDuplicationForeignKeySelector, filterAfterDeDuplication, DefaultManyText);

		public static ZString GetSingleValueOrManyText<T, ZGuid>(this IEnumerable<T> items, Func<T, ZString> getValue, Func<T, ZGuid> getDeDuplicationForeignKeySelector, Func<T, bool> filterAfterDeDuplication, ZString manyReturnValue)
		{
			var values = items.DistinctBy(x => getDeDuplicationForeignKeySelector(x)).Where(x => filterAfterDeDuplication(x)).Select(x => getValue(x)).Take(2).ToArray();
			return values.Length > 1 ? manyReturnValue : values.FirstOrDefault();
		}

		#endregion

		#region IDictionary<TKey, IList<TItem>>

		public static void SafeAdd<TKey, TItem>(this IDictionary<TKey, IList<TItem>> dictionary, TKey key, params TItem[] items) =>
			dictionary.SafeAdd(key, items.ToList());

		public static void SafeAdd<TKey, TItem>(this IDictionary<TKey, IList<TItem>> dictionary, TKey key, IEnumerable<TItem> items)
		{
			if (dictionary.TryGetValue(key, out var list))
			{
				foreach (var item in items)
				{
					list.Add(item);
				}
			}
			else
			{
				dictionary.Add(key, items.ToList());
			}
		}

		#endregion

		[SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public static PropertyInfo GetPropertyInfo<T, U>(Expression<Func<T, U>> lambda)
		{
			if (lambda.Body == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lambda expression ({0}) Body must not be null.", lambda));
			}
			var member = lambda.Body as MemberExpression
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Can't cast lambda expression Body ({0}) to MemberExpression.", lambda.Body));

			if (member.Member == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Lambda expression ({0}) Member must not be null.", member));
			}
			var propertyInfo = member.Member as PropertyInfo
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Can't cast lambda expression Member ({0}) to PropertyInfo.", member.Member));

			return propertyInfo;
		}

		#region ILogger

		public static ILogger WithPrefix(this ILogger logger, string prefix) => new LogAction((type, msg, ex) => logger?.Log(type, prefix + msg, ex));

		public static ILogger DebugOnly(this ILogger logger)
		{
			void Action(LogType type, string msg, Exception ex)
			{
				if (type != LogType.Debug)
				{
					logger?.WithPrefix(type + ":")?.Debug(msg);
				}
				else
				{
					logger?.Debug(msg);
				}
			}

			return new LogAction(Action);
		}

		public static IDisposableLogger SortAndDistinct(this ILogger logger) => new SortedDistinctLogger(logger);

		class SortedDistinctLogger : IDisposableLogger
		{
			public SortedDistinctLogger(ILogger logger)
			{
				this.logger = logger;
			}

			readonly ILogger logger;
			readonly List<(LogType, string, Exception)> logMessages = new List<(LogType, string, Exception)>();

			public void Log(LogType type, string message) => Log(type, message, null);
			public void Log(LogType type, string message, Exception ex)
			{
				logMessages.Add((type, message, ex));
			}

			public void Dispose()
			{
				var duplicatedMessageGroups = logMessages.GroupBy(x => x);

				foreach (var duplicatedGroup in duplicatedMessageGroups)
				{
					var duplicatedMessages = duplicatedGroup.ToList();
					var repeatTimes = string.Empty;

					if (duplicatedMessages.Count > 1)
					{
						repeatTimes = FormattableString.Invariant($" (x{duplicatedMessages.Count})");
						duplicatedMessages.Sort();
					}

					logger.Log(duplicatedMessages[0].Item1, duplicatedMessages[0].Item2 + repeatTimes, duplicatedMessages[0].Item3);
				}
			}
		}

		public static ILogger Distinct(this ILogger logger) => new DistinctLogger(logger);

		class DistinctLogger : ILogger
		{
			public DistinctLogger(ILogger logger)
			{
				this.logger = logger;
			}

			readonly ILogger logger;
			readonly List<(LogType, string)> logMessages = new List<(LogType, string)>();

			public void Log(LogType type, string message) => Log(type, message, null);
			public void Log(LogType type, string message, Exception ex)
			{
				if (!logMessages.Contains((type, message)))
				{
					logMessages.Add((type, message));
					logger.Log(type, message, ex);
				}
			}
		}

		class LogAction : ILogger
		{
			readonly Action<LogType, string, Exception> logAction;

			public LogAction(Action<LogType, string, Exception> logAction)
			{
				this.logAction = logAction;
			}

			public void Log(LogType type, string message) => Log(type, message, null);
			public void Log(LogType type, string message, Exception ex) => logAction(type, message, ex);
		}

		#endregion

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public static void NotifyUserIfRequired(this HttpRequestException ex, string serviceUrl)
		{
			if (ex == null)
			{
				return;
			}

			if (Globals.IsUserInteractive)
			{
				if (ex.InnerException is WebException webEx)
				{
					if (webEx.Status == WebExceptionStatus.SecureChannelFailure)
					{
						var caption = Res.GetString("f2d47549-4d01-439c-8a42-2c6d202243e1", "Connection error");
						var msg = Res.GetString(
							"858e5781-9a5c-456a-a4b8-e817150f5232",
							"Could not establish secure SSL/TLS connection with WiseTech Global service at '{0}'. Please contact your system administrator.", serviceUrl);

						Globals.Message.ShowErrorOnce(msg, caption);
						return;
					}

					if (webEx.Status == WebExceptionStatus.TrustFailure)
					{
						var caption = Res.GetString("f2d47549-4d01-439c-8a42-2c6d202243e1", "Connection error");
						var msg = Res.GetString(
							"91d29c91-8739-4fd0-80c6-e6b7947d170a",
							"Failed to validate the SSL/TLS certificate of WiseTech Global service at '{0}'. Please contact your system administrator.", serviceUrl);

						Globals.Message.ShowErrorOnce(msg, caption);
						return;
					}
				}

				if (ex.InnerException is AuthenticationException authEx)
				{
					if (authEx.Message.Contains((NoResString)"The remote certificate was rejected by the provided RemoteCertificateValidationCallback."))
					{
						var caption = Res.GetString("f2d47549-4d01-439c-8a42-2c6d202243e1", "Connection error");
						var msg = Res.GetString(
							"91d29c91-8739-4fd0-80c6-e6b7947d170a",
							"Failed to validate the SSL/TLS certificate of WiseTech Global service at '{0}'. Please contact your system administrator.", serviceUrl);
						Globals.Message.ShowErrorOnce(msg, caption);
						return;
					}
				}
			}
		}
	}

	public class FuncComparer<T> : IComparer<T>
	{
		public FuncComparer(Func<T, T, int> compare)
		{
			this.compare = compare;
		}

		public int Compare(T x, T y)
		{
			return compare(x, y);
		}

		readonly Func<T, T, int> compare;
	}
}
