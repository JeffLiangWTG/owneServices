using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class SqlDurationInterceptor : IDbCommandInterceptor
	{
		public InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
		{
			RecordCommandStartTime(command);
			return result;
		}

		public ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
			CancellationToken cancellationToken = new())
		{
			return new ValueTask<InterceptionResult<int>>(NonQueryExecuting(command, eventData, result));
		}

		public InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
		{
			RecordCommandStartTime(command);
			return result;
		}

		public ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
			CancellationToken cancellationToken = new())
		{
			return new ValueTask<InterceptionResult<DbDataReader>>(ReaderExecuting(command, eventData, result));
		}

		public InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
		{
			RecordCommandStartTime(command);
			return result;
		}

		public ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
			CancellationToken cancellationToken = new())
		{
			return new ValueTask<InterceptionResult<object>>(ScalarExecuting(command, eventData, result));
		}

		public int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
		{
			_ = command ?? throw new ArgumentNullException(nameof(command));
			_ = eventData ?? throw new ArgumentNullException(nameof(eventData));

			LogDbOperation(command);
			return result;
		}

		public ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = new())
		{
			return new ValueTask<int>(NonQueryExecuted(command, eventData, result));
		}

		public DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
		{
			_ = command ?? throw new ArgumentNullException(nameof(command));
			_ = eventData ?? throw new ArgumentNullException(nameof(eventData));

			LogDbOperation(command);
			return result;
		}

		public ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
			CancellationToken cancellationToken = new CancellationToken())
		{
			return new ValueTask<DbDataReader>(ReaderExecuted(command, eventData, result));
		}

		public DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result) => result;

		public InterceptionResult<DbCommand> CommandCreating(CommandCorrelatedEventData eventData, InterceptionResult<DbCommand> result) => result;

		public InterceptionResult DataReaderDisposing(DbCommand command, DataReaderDisposingEventData eventData, InterceptionResult result) => result;

		void LogDbOperation(DbCommand command)
		{
			if (!command.CommandText.StartsWith(SqlCommentAdder.PrefixCommentFlag, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (!commandStartTimes.TryRemove(command, out var startTime))
			{
				throw new InvalidOperationException($"Unable to resolve command execution start time for '{command.CommandText}'");
			}

			var id = SqlCommentIdExtractor.ExtractId(command.CommandText);
			var duration = DateTimeOffset.Now - startTime;
			Console.WriteLine($"{FlagHelper.GetFlag(UXMLProducerHelper.SqlPerformance)}:{id}:{duration.TotalMilliseconds}");
		}

		void RecordCommandStartTime(DbCommand command)
		{
			if (!command.CommandText.StartsWith(SqlCommentAdder.PrefixCommentFlag, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			if (!commandStartTimes.TryAdd(command, DateTimeOffset.Now))
			{
				throw new InvalidOperationException("Command execution start time already recorded.");
			}
		}

		readonly ConcurrentDictionary<DbCommand, DateTimeOffset> commandStartTimes = new();
	}
}
