using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class SystemVersionInterceptor : IDbCommandInterceptor
	{
		readonly ISystemVersionContext systemVersionContext;

		public SystemVersionInterceptor(ISystemVersionContext systemVersionContext)
		{
			this.systemVersionContext = systemVersionContext;
		}

		static readonly Regex SystemVersionRegex = new Regex(@"(?<table>FROM\s+\[\w+\])", RegexOptions.Multiline | RegexOptions.IgnoreCase);

		public InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
		{
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
			command.CommandText = SystemVersionRegex.Replace(command.CommandText, match => ReplaceMatch(match, systemVersionContext.SystemVersionUTC));
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
			return result;
		}

		static string ReplaceMatch(Match match, string systemVersion)
		{
			if (!string.IsNullOrEmpty(systemVersion) && IsTemporalTable(match.Value))
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} FOR SYSTEM_TIME AS OF '{1}'", match.Value, systemVersion);
			}

			return match.Value;
		}

		static bool IsTemporalTable(string pattern)
		{
			var startIndex = pattern.LastIndexOf('[');
			var tableName = pattern.Substring(startIndex + 1, pattern.Length - startIndex - 2);
			return TemporalTables.Value.Contains(tableName);
		}

		static readonly Lazy<HashSet<string>> TemporalTables = new Lazy<HashSet<string>>(() => DataSetStructureProvider.StructuredDataSets.SelectMany(o => o).Union(DataSetStructureProvider.UserViews).ToHashSet(StringComparer.OrdinalIgnoreCase));
	}
}
