using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.INReferenceData.Services;
using UglyToad.PdfPig;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class CessPdfPigParser : ICessPdfParser
	{
		public CessPdfPigParser(ILogger logger)
		{
			this.logger = logger;
		}

		public string ParseFileAsJson(string pdfFilePath, string outputJsonFolder)
		{
			if (!File.Exists(pdfFilePath))
			{
				logger.Log(LogType.ReviewRequired, $"Invalid file path: {pdfFilePath}");
				return null;
			}

			if (!Directory.Exists(outputJsonFolder))
			{
				Directory.CreateDirectory(outputJsonFolder);
			}

			try
			{
				var data = ParseDataAsJson(pdfFilePath);
				var json = JsonSerializer.Serialize(data, Constants.Tariff.Processing.JsonSerializerOptions);

				var outputPath = Path.Combine(outputJsonFolder, Path.GetFileNameWithoutExtension(pdfFilePath) + ".json");
				File.WriteAllText(outputPath, json);

				logger.Log(LogType.ReviewRequired, $"Cess data written to: {outputPath}");
				return outputPath;
			}
			catch (UnhandledApplicationException ex)
			{
				logger.Log(LogType.ReviewRequired, "Error while parsing PDF using PdfPig.", ex);
				return null;
			}
		}

		List<CessDataItem> ParseDataAsJson(string pdfFilePath)
		{
			try
			{
				return ParseDataCore(pdfFilePath);
			}
			catch (Exception ex)
			{
				throw new UnhandledApplicationException($"Error parsing PDF file {pdfFilePath}", ex);
			}
		}

		List<CessDataItem> ParseDataCore(string filePath)
		{
			var cessData = new List<CessDataItem>();
			var dataStarted = false;
			var dataEnded = false;
			using var pdf = PdfDocument.Open(filePath);

			foreach (var page in pdf.GetPages())
			{
				if (dataEnded)
				{
					break;
				}

				logger.Log(LogType.Info, $"Scanning PDF page: {page.Number}");

				var lines = BuildLineDictionary(page, out var topXs);

				if (topXs.Count < MinRequiredColumns)
				{
					logger.Log(LogType.ReviewRequired, "Less than required column anchors found on page. Skipping.");
					continue;
				}

				CessDataItem lastItem = null;
				var lastItemY = -1m;

				foreach (var eachLine in lines)
				{
					var chunks = CessChunkHelper.ChunkLine(eachLine.Value, eachLine.Key);
					var lineText = string.Join("", eachLine.Value.Select(x => x.Text));
					if (!dataStarted && Regex.IsMatch(lineText, CessTableStartRegEx))
					{
						dataStarted = true;
						logger.Log(LogType.ReviewRequired, "Cess data section started, Page number", page.Number);
					}

					if (Regex.IsMatch(lineText, CessTableEndRegEx, RegexOptions.IgnoreCase))
					{
						dataEnded = true;
						logger.Log(LogType.ReviewRequired, "End of Cess data reached, Page number", page.Number);
						break;
					}

					if (!dataStarted || !chunks.Any() || !chunks.All(c => topXs.Contains(Math.Round(c.StartX))))
					{
						continue;
					}

					if (Math.Round(chunks[0].StartX) == topXs.First() && chunks.Count > 1)
					{
						var item = MapChunksToItem(chunks, topXs);
						cessData.Add(item);
						lastItem = item;
						lastItemY = chunks.Min(c => c.Y);
					}
					else if (lastItem != null)
					{
						var chunkY = chunks.Min(c => c.Y);
						if (Math.Abs(chunkY - lastItemY) > YThreshold)
						{
							continue;
						}

						lastItemY = chunkY;

						if (!chunks.All(c => CessChunkHelper.IsChunkInSingleColumn(c.StartX, c.EndX, topXs)))
						{
							continue;
						}

						AppendChunksToItem(chunks, topXs, lastItem);
					}
				}
			}

			return cessData;
		}

		static SortedDictionary<decimal, List<CessLineChunk>> BuildLineDictionary(
			UglyToad.PdfPig.Content.Page page, out List<decimal> topXs)
		{
			var lines = new SortedDictionary<decimal, List<CessLineChunk>>(
				Comparer<decimal>.Create((a, b) => b.CompareTo(a)));

			var xFrequency = new Dictionary<decimal, int>();

			foreach (var word in page.GetWords())
			{
				var x = Math.Round((decimal)word.BoundingBox.Left);
				xFrequency[x] = xFrequency.GetValueOrDefault(x) + 1;

				var y = Math.Round((decimal)word.BoundingBox.Bottom, 1);
				var xr = Math.Round((decimal)word.BoundingBox.Right, 1);
				x = Math.Round((decimal)word.BoundingBox.Left, 1);

				if (!lines.TryGetValue(y, out var lineChunks))
				{
					lineChunks = new List<CessLineChunk>();
					lines[y] = lineChunks;
				}

				lineChunks.Add(new CessLineChunk(x, xr, word.Text));
			}

			topXs = xFrequency
				.OrderByDescending(kvp => kvp.Value)
				.Take(MinRequiredColumns)
				.Select(kvp => kvp.Key)
				.OrderBy(x => x)
				.ToList();

			return lines;
		}

		static CessDataItem MapChunksToItem(List<CessGroupedChunk> chunks, List<decimal> topXs)
		{
			var item = new CessDataItem();

			foreach (var chunk in chunks)
			{
				var text = string.Join(" ", chunk.Words);
				var col = topXs.IndexOf(Math.Round(chunk.StartX));
				switch (col)
				{
					case 0:
						item.SerialNumber = text;
						break;
					case 1:
						item.HsCode = text;
						break;
					case 2:
						item.Description = text;
						break;
					case 3:
						item.Rate = text;
						break;
				}
			}

			return item;
		}

		static void AppendChunksToItem(List<CessGroupedChunk> chunks, List<decimal> topXs, CessDataItem item)
		{
			foreach (var chunk in chunks)
			{
				var text = string.Join(" ", chunk.Words);
				var col = topXs.IndexOf(Math.Round(chunk.StartX));
				switch (col)
				{
					case 0:
						item.SerialNumber += " " + text;
						break;
					case 1:
						item.HsCode += " " + text;
						break;
					case 2:
						item.Description += " " + text;
						break;
					case 3:
						item.Rate += " " + text;
						break;
				}
			}
		}

		readonly ILogger logger;

		const int MinRequiredColumns = 4;
		const decimal YThreshold = 14m;
		const string CessTableStartRegEx = @"\(\d\)\s*\(\d\)\s*\(\d\)\s*\(\d\)";
		const string CessTableEndRegEx = @"\[\s*Notfn\.?\s*No\.?.*";
	}
}
