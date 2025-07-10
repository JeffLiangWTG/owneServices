using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.INReferenceData.Services;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using DimensionConstants = CargoWise.RefDbRepo.INReferenceData.Business.Constants.Tariff.DataExtraction;
using PatternConstants = CargoWise.RefDbRepo.INReferenceData.Business.Constants.Tariff.DataExtraction.Patterns;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	sealed class TariffTableDataExtractionStrategy : ITextExtractionStrategy
	{
		public TariffTableDataExtractionStrategy(ColumnPositionProvider positionProvider, ILogger logger)
		{
			PositionProvider = positionProvider;
			this.logger = logger;
		}

		public void ExtractTableData(List<TariffDataItem> rows, out bool endOfData)
		{
			endOfData = false;
			var lineGroups = GroupTextChunksByLine();

			var tableStarted = false;
			var dataStarted = false;
			foreach (var rawLine in lineGroups)
			{
				var line = rawLine.OrderBy(x => x.GetStartXPosition()).ToList();
				var lineText = GetLineText(line);
				if (!dataStarted && IsDataStart(lineText))
				{
					dataStarted = true;
					continue;
				}

				if (dataStarted || rows.Count != 0)
				{
					if (IsTableHeader(lineText) || IsHeaderLine(lineText))
					{
						tableStarted = true;
						continue;
					}
				}

				if (IsSeperatorLine(lineText) || IsFooterLine(lineText))
				{
					continue;
				}

				if (tableStarted)
				{
					if (IsEndOfData(lineText))
					{
						endOfData = true;
						logger.Log(LogType.ReviewRequired, "End of data reached");
						break;
					}

					HandleTableRowParsing(rows, line);
				}
			}

			if (dataStarted && rows.Count == 0)
			{
				rows.Add(new TariffDataItem(dummyData: true));
				logger.Log(LogType.Info, "Only Header in the page, adding a dummy record");
			}
		}

		void HandleTableRowParsing(List<TariffDataItem> rows, List<TextChunk> line)
		{
			var row = ParseTableRow(line);

			if (HandleDescriptionStartsWithRomanNumbers(rows, row))
			{
				return;
			}

			var previousRow = rows[^1];
			if (HandleOnlyDescriptionIsNotEmpty(previousRow, row))
			{
				return;
			}

			if (HandleTariffHyphensEmpty(previousRow, row))
			{
				return;
			}

			if (HandleRateBrokeAndJoinedUnit(rows, row))
			{
				return;
			}

			rows.Add(row);
		}

		#region Implementation

		#region Handle row cases

		static bool HandleDescriptionStartsWithRomanNumbers(List<TariffDataItem> rows, TariffDataItem row)
		{

			var description = row.Description;
			if (row.AllFieldsOtherThanDescriptionEmpty && Regex.IsMatch(description, PatternConstants.StartWithRomanNumberPattern) || rows.Count == 0)
			{
				rows.Add(row);
				return true;
			}
			return false;
		}

		static bool HandleOnlyDescriptionIsNotEmpty(TariffDataItem previousRow, TariffDataItem row)
		{
			var description = row.Description;
			if (row.AllFieldsOtherThanDescriptionEmpty && !string.IsNullOrEmpty(description))
			{
				if (Regex.IsMatch(description, $@"^\s*({AppConfig.Tariff.WeightUnitsInDescriptionRegEx})\s*$") && string.IsNullOrEmpty(previousRow.Unit))
				{
					previousRow.Unit = description;
				}
				else if (Regex.IsMatch(description, PatternConstants.RatePattern) && string.IsNullOrEmpty(previousRow.StandardRate))
				{
					previousRow.StandardRate = description;
				}
				else if (Regex.IsMatch(description, PatternConstants.PreferentialRatePattern) && string.IsNullOrEmpty(previousRow.PreferentialRate))
				{
					previousRow.PreferentialRate = description;
				}
				else if (previousRow.AllFieldsOtherThanDescriptionEmpty || row.StartXPosition > DimensionConstants.PossibleDescriptionStart)
				{
					previousRow.Description += " " + description;
				}
				return true;
			}
			return false;
		}

		static bool HandleTariffHyphensEmpty(TariffDataItem previousRow, TariffDataItem row)
		{
			if (string.IsNullOrEmpty(row.TariffItem)
				&& string.IsNullOrEmpty(row.Hyphens))
			{
				previousRow.Description = string.Join(" ", previousRow.Description, row.Description).Trim();
				previousRow.Unit = string.Join(" ", previousRow.Unit, row.Unit).Trim();
				previousRow.StandardRate = string.Join(" ", previousRow.StandardRate, row.StandardRate).Trim();
				previousRow.PreferentialRate = string.Join(" ", previousRow.PreferentialRate, row.PreferentialRate).Trim();

				return true;
			}
			return false;
		}

		static bool HandleRateBrokeAndJoinedUnit(List<TariffDataItem> rows, TariffDataItem row)
		{
			var unit = row.Unit;
			var unitLength = unit.Length;
			var rate = row.StandardRate;
			if (Regex.IsMatch(rate, @"^\s*[0]{1,2}%\s*$") && unitLength > 1)
			{
				row.StandardRate = unit.Substring(unitLength - 1) + rate;
				row.Unit = unit.Substring(0, unitLength - 1);
				rows.Add(row);
				return true;
			}
			return false;
		}

		#endregion

		#region Control flow

		static bool IsDataStart(string lineText)
		{
			return lineText.StartsWith("Tariff Item", StringComparison.InvariantCultureIgnoreCase);
		}

		static bool IsHeaderLine(string lineText)
		{
			return lineText.StartsWith("section", StringComparison.InvariantCultureIgnoreCase);
		}

		static bool IsTableHeader(string lineText)
		{
			return Regex.IsMatch(lineText, @"^\(1\)\s*\(2\)\s*\(3\)\s*\(4P?\)\s*\(5\)$");
		}

		static bool IsSeperatorLine(string lineText)
		{
			return Regex.IsMatch(lineText, @"_{10,}");
		}

		static bool IsFooterLine(string lineText)
		{
			return FooterIndicatorPatterns.Any(pattern => Regex.IsMatch(lineText, pattern));
		}

		static bool IsEndOfData(string lineText)
		{
			return DataEndIndicators.Any(x => lineText.StartsWith(x, StringComparison.InvariantCultureIgnoreCase))
			|| DataEndIndicatorsMatchCase.Any(x => lineText.StartsWith(x, StringComparison.InvariantCulture));
		}

		static readonly string[] FooterIndicatorPatterns =
		[
			@"\s*w\s*\.\s*e\s*\.\s*f\.?\s*(\d{1,2}\.\s*\d{1,2}\.\s*\d{2,4})",
			@"^[#*]{0,2}\s*vide\b.*?\b(\d{4})\b.*?\bdt\.\s*(\d{1,2}\.\d{1,2}\.\d{4})$",
			@"^[#*]{0,2}\s*[Aa]mended by Not.*?No\.\s+\d{1,2}/\d{1,2}$"
		];

		static readonly string[] DataEndIndicators = ["exemption", "Safeguard", "[Notfn", "[Notifn", "Note:"];
		static readonly string[] DataEndIndicatorsMatchCase = ["ADDITIONAL DUTY-LEVY", "PROJECT  IMPORTS"];

		#endregion

		#endregion

		List<List<TextChunk>> GroupTextChunksByLine()
		{
			var sortedChunks = textChunks.OrderBy(chunk => -chunk.GetStartYPosition())
										.ThenBy(chunk => chunk.GetStartXPosition())
										.ToList();

			var lineGroups = new List<List<TextChunk>>();
			var currentLine = new List<TextChunk>();
			float? lastY = null;

			foreach (var chunk in sortedChunks)
			{
				if (chunk.GetStartXPosition() < DimensionConstants.LeftMargin)
				{
					continue;
				}

				if (lastY == null || Math.Abs(chunk.GetStartYPosition() - lastY.Value) <= DimensionConstants.SameLineVerticalTolerance)
				{
					currentLine.Add(chunk);
				}
				else
				{
					if (currentLine.Any())
					{
						lineGroups.Add(new List<TextChunk>(currentLine));
					}
					currentLine.Clear();
					currentLine.Add(chunk);
				}
				lastY = chunk.GetStartYPosition();
			}

			if (currentLine.Any())
			{
				lineGroups.Add(currentLine);
			}

			return lineGroups;
		}

		static string GetLineText(List<TextChunk> line)
		{
			return string.Join("", line.Select(c => c.Text));
		}

		TariffDataItem ParseTableRow(List<TextChunk> line)
		{
			var combinedText = GetColumnWiseDataWithFlexibleWidth(line);

			var row = new TariffDataItem();
			row.TariffItem = CombineColumnText(combinedText, 0);
			row.Hyphens = ExtractHyphens(combinedText, 1);
			row.Description = CombineColumnText(combinedText, 2);
			row.Unit = CombineColumnText(combinedText, 3);
			row.StandardRate = CombineColumnText(combinedText, 4);
			row.PreferentialRate = CombineColumnText(combinedText, 5);
			row.StartXPosition = line.Min(x => x.GetStartXPosition());
			return row;
		}

		Dictionary<int, List<string>> GetColumnWiseDataWithFlexibleWidth(List<TextChunk> line)
		{
			var textChunks = GetCombinedChunks(line);
			var combinedChunksColumnWise = textChunks.ToDictionary(x => x.Key, x => x.Value.Select(y => y.Text).ToList());

			var column0Text = CombineColumnText(combinedChunksColumnWise, 0);
			var column1Text = CombineColumnText(combinedChunksColumnWise, 1);



			var context = new AdjustmentContext
			{
				CombinedChunksColumnWise = combinedChunksColumnWise,
				CombinedTextChunksColumnWise = textChunks,
				AdjustedChunksColumnWise = new Dictionary<int, List<string>>(),
				ColumnTexts = new[] { column0Text, column1Text, CombineColumnText(combinedChunksColumnWise, 2), CombineColumnText(combinedChunksColumnWise, 3), CombineColumnText(combinedChunksColumnWise, 4), CombineColumnText(combinedChunksColumnWise, 5) },
				IsTariffAtColumn0 = Regex.IsMatch(column0Text, Constants.Tariff.Pdf.Patterns.TariffItemOptionalRawPattern),
				AreHyphensAtColumn0 = Regex.IsMatch(column0Text, PatternConstants.StartWithHyphenOrDashPattern),
				AreHyphensAtColumn1 = Regex.IsMatch(column1Text, PatternConstants.StartWithHyphenOrDashPattern)
			};

			if (context.IsTariffAtColumn0)
			{
				PositionProvider.AddAndBuild(textChunks);
			}

			UpdateSegmentsFromLine(context);

			return GetAdjustChunks(context);
		}

		readonly List<TextChunk> superScriptLine = [];

		Dictionary<int, List<TextChunk>> GetCombinedChunks(List<TextChunk> line)
		{
			var combinedChunksColumnWise = new Dictionary<int, List<TextChunk>>();
			if (line.All(x => x.Text == "2"))
			{
				superScriptLine.AddRange(line);
				return combinedChunksColumnWise;
			}

			if (superScriptLine.Count != 0)
			{
				line.AddRange(superScriptLine);
				line.Sort((x, y) => x.GetStartXPosition().CompareTo(y.GetStartXPosition()));
				superScriptLine.Clear();
			}

			TextChunk penultimateChunk = null;
			TextChunk previousChunk = null;
			int columnIndex = 0;
			var invalidChunkTexts = new List<string>();

			foreach (var chunk in line.OrderBy(c => c.GetStartXPosition()))
			{
				if (!chunk.IsValid)
				{
					invalidChunkTexts.Add(chunk.Text);
					continue;
				}
				if (previousChunk != null)
				{
					if (columnIndex != 2 && penultimateChunk != null && string.IsNullOrWhiteSpace(penultimateChunk.Text) && string.IsNullOrWhiteSpace(previousChunk.Text) && string.IsNullOrWhiteSpace(chunk.Text))
					{
						continue;
					}
					if (Math.Abs(chunk.GetStartXPosition() - previousChunk.GetEndXPosition()) > DimensionConstants.ChunkSeparationWidth && !combinedChunksColumnWise[columnIndex].All(x => string.IsNullOrWhiteSpace(x.Text)))
					{
						columnIndex++;
					}
				}

				if (!combinedChunksColumnWise.ContainsKey(columnIndex))
				{
					combinedChunksColumnWise[columnIndex] = new List<TextChunk>();
				}
				combinedChunksColumnWise[columnIndex].Add(chunk);
				penultimateChunk = previousChunk;
				previousChunk = chunk;
			}

			if (combinedChunksColumnWise.Count > 1)
			{
				var column0List = combinedChunksColumnWise[0];
				var lineListAfter0 = line.Skip(column0List.Count);
				if (line.All(x => x.GetStartXPosition() > DimensionConstants.PossibleDescriptionStart && x.GetEndXPosition() < DimensionConstants.PossibleDescriptionEnd))
				{
					var lineText = GetLineText(line);
					if (!Regex.IsMatch(lineText, PatternConstants.StartWithTariffItemPattern) && !Regex.IsMatch(lineText, PatternConstants.StartWithHyphenPattern))
					{
						combinedChunksColumnWise.Clear();
						combinedChunksColumnWise[0] = line;
					}
				}
				else if (lineListAfter0.All(x => x.GetStartXPosition() > DimensionConstants.PossibleDescriptionStart && x.GetEndXPosition() < DimensionConstants.PossibleDescriptionEnd))
				{
					combinedChunksColumnWise.Clear();
					combinedChunksColumnWise[0] = column0List;
					combinedChunksColumnWise[1] = lineListAfter0.ToList();
				}
			}

			foreach (var chunks in combinedChunksColumnWise.Values)
			{
				HandleScript(chunks);
			}

			if (invalidChunkTexts.Count != 0)
			{
				logger.Log(LogType.Warning, "Discarded invalid chunk text", string.Join(", ", invalidChunkTexts));
			}

			return combinedChunksColumnWise;
		}

		static void HandleScript(List<TextChunk> chunks)
		{

			var baseYPosition = chunks
				.GroupBy(c => c.GetStartYPosition())
				.OrderByDescending(g => g.Count())
				.First()
				.Key;
			foreach (var chunk in chunks)
			{
				if (IsSuperscript(chunk, baseYPosition))
				{
					UpdateTextToSuperscript(chunk);
				}
				else if (IsSubscript(chunk, baseYPosition))
				{
					UpdateTextToSubscript(chunk);
				}
			}
		}

		static void UpdateTextToSuperscript(TextChunk chunk)
		{
			chunk.Text = chunk.Text switch
			{
				"1" => "\u00B9",
				"2" => "\u00B2",
				"3" => "\u00B3",
				"4" => "\u2074",
				"5" => "\u2075",
				"6" => "\u2076",
				"7" => "\u2077",
				"8" => "\u2078",
				"9" => "\u2079",
				"0" => "\u2070",
				"-" => "\u207b",
				_ => chunk.Text
			};
		}

		static void UpdateTextToSubscript(TextChunk chunk)
		{
			chunk.Text = chunk.Text switch
			{
				"1" => "\u2081",
				"2" => "\u2082",
				"3" => "\u2083",
				"4" => "\u2084",
				"5" => "\u2085",
				"6" => "\u2086",
				"7" => "\u2087",
				"8" => "\u2088",
				"9" => "\u2089",
				"0" => "\u2080",
				_ => chunk.Text
			};
		}
		static bool IsSuperscript(TextChunk chunk, float baseYPosition)
		{
			return chunk.GetStartYPosition() - baseYPosition >= DimensionConstants.SuperscriptVerticalTolerance;
		}

		static bool IsSubscript(TextChunk chunk, float baseYPosition)
		{
			return baseYPosition - chunk.GetStartYPosition() >= DimensionConstants.SubscriptVerticalTolerance;
		}

		static void UpdateSegmentsFromLine(AdjustmentContext context)
		{
			var combinedChunksColumnWise = context.CombinedChunksColumnWise;
			var column0Text = context.ColumnTexts[0];
			var column1Text = context.ColumnTexts[1];
			if (combinedChunksColumnWise.Count == 1 && Regex.IsMatch(column0Text, PatternConstants.PreferentialRatePattern))
			{
				context.PreferentialRateTextAt0 = column0Text;
			}
			else if (context.AreHyphensAtColumn0)
			{
				context.HyphenTextAt0 = column0Text;
			}
			else if (combinedChunksColumnWise.Count == 1 && Regex.IsMatch(column0Text, PatternConstants.DescriptionPattern))
			{
				context.DescriptionTextAt0 = column0Text;
			}
			else if (context.IsTariffAtColumn0 && !context.AreHyphensAtColumn1 && Regex.IsMatch(column1Text, PatternConstants.DescriptionPattern))
			{
				context.DescriptionTextAt1 = column1Text;
			}
			else if (combinedChunksColumnWise.Count == 2 && Regex.IsMatch(column0Text, PatternConstants.RatePattern) && Regex.IsMatch(column1Text, PatternConstants.PreferentialRatePattern))
			{
				context.RateTextAt0 = column0Text;
				context.PreferentialRateTextAt1 = column1Text;
			}
		}

		Dictionary<int, List<string>> GetAdjustChunks(AdjustmentContext context)
		{
			var adjustedChunksColumnWise = context.AdjustedChunksColumnWise;

			if (HandleTariffDataTogetherAtColumn0(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleDescriptionUnitRatesAtColumn2(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleHyphenTextAt0(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleDescriptionStartsWithHyphenAt0(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandledescriptionTouchesHyphenFromStartAt0(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleTariffHyphensDescriptionAt0Together(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleLineWrappingsAfterDescription(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleDescriptionOrPreferentialRateAt0(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleDescriptionEndsWithUnitsAt1(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleDescriptionTextAt1(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleTariffHyphensDescriptionInOrder(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleDescriptionEndsWithUnitsAt2(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleDescriptionStartsWithHyphenAt1(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandledescriptionTouchesHyphenFromStartAt1(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleDescriptionStartsAt0FollowOtherColumns(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleRateAt0AndPreferentialRateAt1(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleUnitTouchesRateAt3(context))
			{
				return adjustedChunksColumnWise;
			}

			if (HandleRateTouchesPreferentialRateAt4(context))
			{
				return adjustedChunksColumnWise;
			}

			return context.CombinedChunksColumnWise;
		}

		#region Handle cases

		static bool HandleTariffDataTogetherAtColumn0(AdjustmentContext context)
		{
			var tariffDataTogetherAtColumn0Match = Regex.Match(context.ColumnTexts[0], PatternConstants.TariffDataTogetherPattern);
			if (tariffDataTogetherAtColumn0Match.Success)
			{
				var groups = tariffDataTogetherAtColumn0Match.Groups;
				for (var i = 0; i <= 5; i++)
				{
					context.AdjustedChunksColumnWise[i] = new List<string>() { groups[i + 1].Value };
				}
				return true;
			}

			return false;
		}

		static bool HandleDescriptionUnitRatesAtColumn2(AdjustmentContext context)
		{

			var DescriptionUnitRatesAtColumn2Match = Regex.Match(context.ColumnTexts[2], PatternConstants.DescriptionUnitRatesTogetherPattern);
			if (DescriptionUnitRatesAtColumn2Match.Success)
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1 }, context.AdjustedChunksColumnWise, new[] { 0, 1 });
				var groups = DescriptionUnitRatesAtColumn2Match.Groups;
				for (var i = 1; i < groups.Count; i++)
				{
					context.AdjustedChunksColumnWise[i + 1] = new List<string>() { groups[i].Value };
				}

				if (string.IsNullOrEmpty(groups[4].Value))
				{
					CopyChunks(context.CombinedChunksColumnWise, new[] { 3 }, context.AdjustedChunksColumnWise, new[] { 5 });
				}
				return true;
			}

			return false;
		}

		static bool HandleHyphenTextAt0(AdjustmentContext context)
		{
			if (!string.IsNullOrEmpty(context.HyphenTextAt0))
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1, 2, 3, 4 }, context.AdjustedChunksColumnWise, new[] { 1, 2, 3, 4, 5 });
				return true;
			}

			return false;
		}

		static bool HandleDescriptionOrPreferentialRateAt0(AdjustmentContext context)
		{
			if (!string.IsNullOrEmpty(context.DescriptionTextAt0) || !string.IsNullOrEmpty(context.PreferentialRateTextAt0))
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0 }, context.AdjustedChunksColumnWise, new[] { 2 });
				return true;
			}

			return false;
		}

		static bool HandleDescriptionTextAt1(AdjustmentContext context)
		{
			if (!string.IsNullOrEmpty(context.DescriptionTextAt1))
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1, 2, 3, 4 }, context.AdjustedChunksColumnWise, new[] { 0, 2, 3, 4, 5 });
				return true;
			}

			return false;
		}

		static bool HandleTariffHyphensDescriptionInOrder(AdjustmentContext context)
		{
			if (context.IsTariffAtColumn0 && context.AreHyphensAtColumn1 && Regex.IsMatch(context.ColumnTexts[2], PatternConstants.DescriptionPattern) && !string.IsNullOrEmpty(context.ColumnTexts[3]) && string.IsNullOrEmpty(context.ColumnTexts[4]) && string.IsNullOrEmpty(context.ColumnTexts[5]))
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1, 2, 3 }, context.AdjustedChunksColumnWise, new[] { 0, 1, 2, 5 });
				return true;
			}

			return false;
		}

		static bool HandleTariffHyphensDescriptionAt0Together(AdjustmentContext context)
		{

			var tariffAndHyphenAtColumn0Match = Regex.Match(context.ColumnTexts[0], PatternConstants.TariffAndHyphenPattern);
			if (tariffAndHyphenAtColumn0Match.Success)
			{
				var tariff = tariffAndHyphenAtColumn0Match.Groups[1].Value;
				var hyphens = tariffAndHyphenAtColumn0Match.Groups[2].Value;
				var description = tariffAndHyphenAtColumn0Match.Groups[3].Value;

				context.AdjustedChunksColumnWise[0] = new List<string> { tariff };
				context.AdjustedChunksColumnWise[1] = context.AreHyphensAtColumn1 ? context.CombinedChunksColumnWise[1].Concat(new[] { hyphens }).ToList() : new List<string> { hyphens };
				if (!string.IsNullOrEmpty(description))
				{
					context.AdjustedChunksColumnWise[2] = new List<string> { description };
					CopyChunks(context.CombinedChunksColumnWise, new[] { 1, 2, 3 }, context.AdjustedChunksColumnWise, new[] { 3, 4, 5 });
				}
				else if (context.AreHyphensAtColumn1)
				{
					CopyChunks(context.CombinedChunksColumnWise, new[] { 2, 3, 4, 5 }, context.AdjustedChunksColumnWise, new[] { 2, 3, 4, 5 });
				}
				else
				{
					CopyChunks(context.CombinedChunksColumnWise, new[] { 1, 2, 3, 4 }, context.AdjustedChunksColumnWise, new[] { 2, 3, 4, 5 });
				}
				return true;
			}

			return false;
		}

		static bool HandleDescriptionEndsWithUnitsAt2(AdjustmentContext context)
		{
			return HandleDescriptionEndsWithUnits(context, 2, new[] { 0, 1, 2, 2, 3, 4 }, new[] { 0, 1, 2, 3, 4, 5 });
		}

		static bool HandleDescriptionEndsWithUnitsAt1(AdjustmentContext context)
		{
			return !string.IsNullOrEmpty(context.DescriptionTextAt1) && HandleDescriptionEndsWithUnits(context, 1, new[] { 0, 1, 1, 2, 3 }, new[] { 0, 2, 3, 4, 5 });
		}

		static bool HandleDescriptionEndsWithUnits(AdjustmentContext context, int columnIndex, int[] sourceKeys, int[] destinationKeys)
		{
			var descriptionEndsWithUnitsMatch = Regex.Match(context.ColumnTexts[columnIndex], PatternConstants.DescriptionEndsWithUnitsPattern);
			if (descriptionEndsWithUnitsMatch.Success && ((Regex.IsMatch(context.ColumnTexts[columnIndex + 1], PatternConstants.RateWithConditionPattern) && Regex.IsMatch(context.ColumnTexts[columnIndex + 2], PatternConstants.PreferentialRatePattern)) || (string.IsNullOrEmpty(context.ColumnTexts[columnIndex + 1]) && string.IsNullOrEmpty(context.ColumnTexts[columnIndex + 2]))))
			{
				var unit = descriptionEndsWithUnitsMatch.Groups[0].Value;

				CopyChunks(context.CombinedChunksColumnWise, sourceKeys, context.AdjustedChunksColumnWise, destinationKeys, mappingInput =>
				{
					switch (mappingInput.DestinationKey)
					{
						case 2:
							return mappingInput.SourceValues.SkipLast(unit.Length).ToList();
						case 3:
							return new List<string> { unit };
						default:
							return mappingInput.SourceValues;
					}
				});
				return true;
			}

			return false;
		}

		static bool HandleDescriptionStartsWithHyphenAt0(AdjustmentContext context)
		{
			var descriptionStartsWithHyphenAtColumn0Match = Regex.Match(context.ColumnTexts[0], PatternConstants.DescriptionStartsWithHyphenPattern);
			if (descriptionStartsWithHyphenAtColumn0Match.Success)
			{
				var hyphens = descriptionStartsWithHyphenAtColumn0Match.Groups[0].Value;

				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 0 }, context.AdjustedChunksColumnWise, new[] { 1, 2 }, mappingInput =>
				{
					switch (mappingInput.DestinationKey)
					{
						case 1:
							return new List<string> { hyphens };
						case 2:
							return mappingInput.SourceValues.Skip(hyphens.Length).ToList();
						default:
							return mappingInput.SourceValues;
					}
				});
				return true;
			}

			return false;
		}

		static bool HandleDescriptionStartsWithHyphenAt1(AdjustmentContext context)
		{
			var descriptionStartsWithHyphenAtColumn1Match = Regex.Match(context.ColumnTexts[1], PatternConstants.DescriptionStartsWithHyphenPattern);
			if (descriptionStartsWithHyphenAtColumn1Match.Success && !context.AreHyphensAtColumn1)
			{
				var hyphens = descriptionStartsWithHyphenAtColumn1Match.Groups[0].Value;

				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1, 1, 2, 3, 4 }, context.AdjustedChunksColumnWise, new[] { 0, 1, 2, 3, 4, 5 }, mappingInput =>
				{
					switch (mappingInput.DestinationKey)
					{
						case 1:
							return new List<string> { hyphens };
						case 2:
							return mappingInput.SourceValues.Skip(hyphens.Length).ToList();
						default:
							return mappingInput.SourceValues;
					}
				});
				return true;
			}

			return false;
		}

		static bool HandledescriptionTouchesHyphenFromStartAt0(AdjustmentContext context)
		{
			var descriptionTouchesHyphenFromStartAtColumn0Match = Regex.Match(context.ColumnTexts[0], PatternConstants.DescriptionTouchesHyphenFromStartPattern);
			if (descriptionTouchesHyphenFromStartAtColumn0Match.Success)
			{
				var hyphens = descriptionTouchesHyphenFromStartAtColumn0Match.Groups[1].Value;

				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 0, 1, 2, 3 }, context.AdjustedChunksColumnWise, new[] { 1, 2, 3, 4, 5 }, mappingInput =>
				{
					switch (mappingInput.DestinationKey)
					{
						case 1:
							return new List<string> { hyphens };
						case 2:
							return mappingInput.SourceValues.Skip(hyphens.Length).ToList();
						default:
							return mappingInput.SourceValues;
					}
				});
				return true;
			}

			return false;
		}

		static bool HandledescriptionTouchesHyphenFromStartAt1(AdjustmentContext context)
		{
			var descriptionTouchesHyphenFromStartAtColumn1Match = Regex.Match(context.ColumnTexts[1], PatternConstants.DescriptionTouchesHyphenFromStartPattern);
			if (descriptionTouchesHyphenFromStartAtColumn1Match.Success)
			{
				var hyphens = descriptionTouchesHyphenFromStartAtColumn1Match.Groups[1].Value;

				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1, 1, 2, 3, 4 }, context.AdjustedChunksColumnWise, new[] { 0, 1, 2, 3, 4, 5 }, mappingInput =>
				{
					switch (mappingInput.DestinationKey)
					{
						case 1:
							return new List<string> { hyphens };
						case 2:
							return mappingInput.SourceValues.Skip(hyphens.Length).ToList();
						default:
							return mappingInput.SourceValues;
					}
				});
				return true;
			}

			return false;
		}

		static bool HandleDescriptionStartsAt0FollowOtherColumns(AdjustmentContext context)
		{
			if (!context.IsTariffAtColumn0 && !context.AreHyphensAtColumn0 && !context.AreHyphensAtColumn1 && context.CombinedChunksColumnWise.Count <= 4)
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1, 2, 3 }, context.AdjustedChunksColumnWise, new[] { 2, 3, 4, 5 });
				return true;
			}

			return false;
		}

		static bool HandleRateAt0AndPreferentialRateAt1(AdjustmentContext context)
		{
			if (!string.IsNullOrEmpty(context.RateTextAt0) && !string.IsNullOrEmpty(context.PreferentialRateTextAt1))
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1 }, context.AdjustedChunksColumnWise, new[] { 4, 5 });
				return true;
			}

			return false;
		}

		static bool HandleUnitTouchesRateAt3(AdjustmentContext context)
		{

			var unitRatesAtColumn3Match = Regex.Match(context.ColumnTexts[3], PatternConstants.UnitRateTogetherPattern);
			if (unitRatesAtColumn3Match.Success)
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1, 2, 3, 3, 4 }, context.AdjustedChunksColumnWise, new[] { 0, 1, 2, 3, 4, 5 }, mappingInput =>
				{
					switch (mappingInput.DestinationKey)
					{
						case 3:
							return new List<string> { unitRatesAtColumn3Match.Groups[1].Value };
						case 4:
							return new List<string> { unitRatesAtColumn3Match.Groups[2].Value };
						default:
							return mappingInput.SourceValues;
					}
				});
				return true;
			}

			return false;
		}

		bool HandleLineWrappingsAfterDescription(AdjustmentContext context)
		{
			if (!context.IsTariffAtColumn0
				&& !context.AreHyphensAtColumn0
				&& !context.AreHyphensAtColumn1
				&& PositionProvider.IsReady)
			{
				foreach (var item in context.CombinedTextChunksColumnWise)
				{
					var center = PdfHelper.GetLeftAlignCenter(item.Value);
					var distanceColumnWise = new Dictionary<int, float>();
					distanceColumnWise[2] = Math.Abs(center - PositionProvider.DescriptionCenter);
					distanceColumnWise[3] = Math.Abs(center - PositionProvider.UnitCenter);
					distanceColumnWise[4] = Math.Abs(center - PositionProvider.RateCenter);
					distanceColumnWise[5] = Math.Abs(center - PositionProvider.PreferentialRateCenter);


					var nearestDistance = distanceColumnWise.Min(x => x.Value);
					var nearestColumnIndex = distanceColumnWise.First(x => x.Value == nearestDistance).Key;

					if (nearestDistance > DimensionConstants.NearstDistanceTolerance && nearestColumnIndex != 2)
					{
						context.AdjustedChunksColumnWise.Clear();
						return false;
					}


					var adjustedChunksColumnWise = context.AdjustedChunksColumnWise;
					if (adjustedChunksColumnWise.ContainsKey(nearestColumnIndex))
					{
						adjustedChunksColumnWise[nearestColumnIndex].AddRange(item.Value.Select(x => x.Text).ToList());
					}
					else
					{
						adjustedChunksColumnWise[nearestColumnIndex] = item.Value.Select(x => x.Text).ToList();
					}
				}
				return true;
			}
			return false;
		}

		static bool HandleRateTouchesPreferentialRateAt4(AdjustmentContext context)
		{
			var rateEndWithPreferentialRateMatch = Regex.Match(context.ColumnTexts[4], PatternConstants.EndWithHyphensPattern);
			if (rateEndWithPreferentialRateMatch.Success)
			{
				CopyChunks(context.CombinedChunksColumnWise, new[] { 0, 1, 2, 3, 4, 4 }, context.AdjustedChunksColumnWise, new[] { 0, 1, 2, 3, 4, 5 }, mappingInput =>
				{
					switch (mappingInput.DestinationKey)
					{
						case 4:
							return new List<string> { rateEndWithPreferentialRateMatch.Groups[1].Value };
						case 5:
							return new List<string> { rateEndWithPreferentialRateMatch.Groups[2].Value };
						default:
							return mappingInput.SourceValues;
					}
				});
				return true;
			}
			return false;
		}

		#endregion

		static void CopyChunks(Dictionary<int, List<string>> source, int[] sourceKeys, Dictionary<int, List<string>> destination, int[] destinationKeys, Func<MappingInput, List<string>> transform = null)
		{
			if (sourceKeys.Length != destinationKeys.Length)
			{
				throw new ArgumentException("Source keys and Destination keys length missmatch");
			}
			for (var i = 0; i < sourceKeys.Length; i++)
			{
				if (source.TryGetValue(sourceKeys[i], out var value))
				{
					destination[destinationKeys[i]] = transform == null ? value : transform(new MappingInput(sourceKeys[i], destinationKeys[i], value));
				}
			}
		}

		static string ExtractHyphens(Dictionary<int, List<TextChunk>> combinedText, int columnIndex)
		{
			return ExtractHyphens(combinedText.ToDictionary(x => x.Key, x => x.Value.Select(y => y.Text).ToList()), columnIndex);
		}

		static string ExtractHyphens(Dictionary<int, List<string>> combinedText, int columnIndex)
		{
			if (!combinedText.ContainsKey(columnIndex))
				return "";

			var hyphenText = string.Join("", combinedText[columnIndex]);
			return new string('-', hyphenText.Count(PatternConstants.HyphenSymbols.Contains));
		}

		static string CombineColumnText(Dictionary<int, List<TextChunk>> combinedText, int columnIndex)
		{
			return CombineColumnText(combinedText.ToDictionary(x => x.Key, x => x.Value.Select(y => y.Text.Trim()).ToList()), columnIndex);
		}

		static string CombineColumnText(Dictionary<int, List<string>> combinedText, int columnIndex)
		{
			if (!combinedText.ContainsKey(columnIndex))
				return "";

			var textList = combinedText[columnIndex].Select(x => string.IsNullOrEmpty(x) ? " " : x);
			return Regex.Replace(string.Join("", textList).Trim(), @"\s{2,}", " ");
		}

		static int GetColumnIndex(float x, List<(float Start, float End)> ranges)
		{
			for (int i = 0; i < ranges.Count; i++)
			{
				if (x >= ranges[i].Start && x < ranges[i].End)
				{
					return i;
				}
			}
			return -1;
		}

		#region ITextExtractionStrategy

		string ITextExtractionStrategy.GetResultantText()
		{
			return ""; // Not used in this implementation
		}

		void IEventListener.EventOccurred(IEventData data, EventType type)
		{
			if (type != EventType.RENDER_TEXT)
				return;

			var renderInfo = (TextRenderInfo)data;
			var chunk = new TextChunk(renderInfo);
			textChunks.Add(chunk);
		}

		ICollection<EventType> IEventListener.GetSupportedEvents()
		{
			return new List<EventType> { EventType.RENDER_TEXT };
		}

		#endregion

		ColumnPositionProvider PositionProvider { get; }

		readonly ILogger logger;

		readonly List<TextChunk> textChunks = new List<TextChunk>();

		class MappingInput
		{
			public MappingInput(int sourceKey, int destinationKey, List<string> sourceValues)
			{
				SourceKey = sourceKey;
				DestinationKey = destinationKey;
				SourceValues = sourceValues;
			}

			public int SourceKey { get; }
			public int DestinationKey { get; }
			public List<string> SourceValues { get; }
		}

		class AdjustmentContext
		{
			public Dictionary<int, List<string>> CombinedChunksColumnWise { get; set; }
			public Dictionary<int, List<TextChunk>> CombinedTextChunksColumnWise { get; set; }
			public Dictionary<int, List<string>> AdjustedChunksColumnWise { get; set; }
			public string[] ColumnTexts { get; set; }
			public bool IsTariffAtColumn0 { get; set; }
			public bool AreHyphensAtColumn0 { get; set; }
			public bool AreHyphensAtColumn1 { get; set; }
			public string HyphenTextAt0 { get; set; }
			public string DescriptionTextAt0 { get; set; }
			public string DescriptionTextAt1 { get; set; }
			public string RateTextAt0 { get; set; }
			public string PreferentialRateTextAt0 { get; set; }
			public string PreferentialRateTextAt1 { get; set; }
		}
	}
}
