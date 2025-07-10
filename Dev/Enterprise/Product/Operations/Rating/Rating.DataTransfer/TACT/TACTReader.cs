using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;

namespace Enterprise.Rating.DataTransfer.TACT
{
	public class TACTReader
	{
		public TACTReader(Stream stream, FixedWidthFlatFileFormat format, INotifications notifications)
		{
			Argument.NotNull(stream, "reader");
			Argument.NotNull(format, "format");
			Argument.NotNull(notifications, "notifications");

			this.stream = stream;
			this.tactFormat = format;
			this.notifications = notifications;

			var tactLineLength = GetTACTLineLength(format);
			TotalRates = stream.Length % tactLineLength == 0
				? stream.Length / tactLineLength
				: stream.Length / tactLineLength + 1;
		}

		public long TotalRates { get; }

		public IEnumerable<TACTDataDataRow> Read(TACTImportResult importResult)
		{
			var rows = new List<string>();

			HashSet<int> chunk;

			lock (syncObject)
			{
				if (chunks == null)
				{
					chunks = GetChunks();
				}

				if (!chunks.Any())
				{
					return Array.Empty<TACTDataDataRow>();
				}

				chunk = chunks.Dequeue();
				var lastLineIndex = chunk.Max();
				var currentLineIndex = 0;

				stream.Seek(0, SeekOrigin.Begin);

				using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
				{
					while (!reader.EndOfStream && currentLineIndex <= lastLineIndex)
					{
						var row = reader.ReadLine();
						if (chunk.Contains(currentLineIndex))
						{
							rows.Add(row);
						}

						currentLineIndex++;
					}
				}
			}

			var parsedRows = rows.Select(Parse).ToList();
			importResult.AddSkippedRates(rows.Count - parsedRows.Count, TACTImportResult.SkipReason.Invalid);
			var excludingDeleteAction = parsedRows.Where(x => x.ActionCode != TACTDataDataRow.Constants.ActionCodes.Delete).ToList();
			var deleteActionCount = parsedRows.Count - excludingDeleteAction.Count;
			importResult.AddSkippedRates(deleteActionCount, TACTImportResult.SkipReason.ActionIsDelete);

			return excludingDeleteAction;
		}

		Queue<HashSet<int>> GetChunks()
		{
			var routes = new Dictionary<string, HashSet<int>>();
			var currentRowIndex = 0;

			stream.Seek(0, SeekOrigin.Begin);

			// Scan the file to see how many lines per route we have
			using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
			{
				while (!reader.EndOfStream)
				{
					var row = reader.ReadLine();

					if (!string.IsNullOrEmpty(row))
					{
						var key = GetKey(row);

						if (!routes.ContainsKey(key))
						{
							routes[key] = new HashSet<int>();
						}

						routes[key].Add(currentRowIndex);
					}

					currentRowIndex++;
				}
			}

			// Group rates by routes in chunks so that rates for the same route go to the same chunk
			var chunks = new Queue<HashSet<int>>();
			var chunk = new HashSet<int>();

			foreach (var route in routes)
			{
				foreach (var rowIndex in route.Value)
				{
					chunk.Add(rowIndex);
				}

				if (chunk.Count > Env.Registry.Rating.TACTRateImportPartitionSize)
				{
					chunks.Enqueue(chunk);
					chunk = new HashSet<int>();
				}
			}

			if (chunk.Any())
			{
				chunks.Enqueue(chunk);
			}

			return chunks;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		TACTDataDataRow Parse(string row)
		{
			try
			{
				return (TACTDataDataRow)tactFormat.ConvertToRow(row);
			}
			catch (Exception ex)
			{
				notifications.AddWarning(Res.GetString("fe8361b0-50bb-11e7-a2b2-fcaa14295823", "An error occurred when parsing row '{0}'. The row will be ignored. Error: {1}", row, ex.Message));
				return null;
			}
		}

		string GetKey(string row)
		{
			if (tactFormat is TACTData80FixedWidthDataFormat tact80)
			{
				return tact80.GetOrigin(row) + tact80.GetDestination(row);
			}

			if (tactFormat is TACTData150FixedWidthDataFormat tact150)
			{
				return tact150.GetOrigin(row) + tact150.GetDestination(row);
			}

			return null;
		}

		static int GetTACTLineLength(FixedWidthFlatFileFormat tactFormat)
		{
			if (tactFormat is TACTData80FixedWidthDataFormat)
			{
				return TACTData80FixedWidthDataFormat.Constants.RowLength + 2;
			}

			if (tactFormat is TACTData150FixedWidthDataFormat)
			{
				return TACTData150FixedWidthDataFormat.Constants.RowLength + 2;
			}

			return -1;
		}

		readonly Stream stream;
		readonly object syncObject = new object();
		readonly INotifications notifications;
		readonly FixedWidthFlatFileFormat tactFormat;
		Queue<HashSet<int>> chunks;
	}
}
