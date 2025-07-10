using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class PDFParser
	{
		const float SourceStartPosition = 52.7f;
		const float CodeStartPosition = 146.1f;
		const float ScopeStartPosition = 242.5f;
		const float RemarkStartPosition = 436.9f;
		const float RemarkEndPosition = 540f;
		List<TextChunk> sourceChunks = new List<TextChunk>();
		List<TextChunk> codeChunks = new List<TextChunk>();
		List<TextChunk> scopeChunks = new List<TextChunk>();
		List<TextChunk> remarkChunks = new List<TextChunk>();
		List<string[]> pdfDatas = new List<string[]>();
		int currentZone = 0;
		string previousItemSource = string.Empty;
		const char splitWord = '及';
		bool isNewPage = true;

		public List<(string source, string code, string scope, string remark)> Parse(byte[] downloadedData)
		{
			var rawData = new List<(string source, string code, string scope, string remark)>();
			using (var stream = new MemoryStream(downloadedData))
			using (var pdfReader = new PdfReader(stream))
			using (var pdfDocument = new PdfDocument(pdfReader))
			{
				var pages = new List<string>();
				var wholeList = new List<TextChunk>();

				for (var pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
				{
					var renderListener = new TableExtractionStrategy();
					var pdfPage = pdfDocument.GetPage(pageNumber);
					var pdfContentProcessor = new PdfCanvasProcessor(renderListener);
					pdfContentProcessor.ProcessPageContent(pdfPage);
					wholeList.AddRange(renderListener.locationalResult);
				}
				foreach (var item in wholeList)
				{
					ParseCodeAndScopeChunks(item);
				}
				ClearChunks();
				ReadPdfDatas();
			}
			foreach (var item in records)
			{
				rawData.Add((item.source, item.code, item.scope, item.remark));
			}
			return rawData;
		}

		void ReadPdfDatas()
		{
			var previousSource = string.Empty;
			var waitingUpdatedSourceCode = string.Empty;
			var waitingUpdatedScopeCode = string.Empty;
			var waitingUpdatedRemarkCode = string.Empty;
			var previousScope = string.Empty;
			var previousRemark = string.Empty;
			foreach (var item in pdfDatas)
			{
				var source = item[0];
				var code = item[1];
				var scope = item[2];
				var remark = GetRemark(code, item[3]);

				if (code == string.Empty)
				{
					var updateSourceCodePartsFor = waitingUpdatedSourceCode.TrimStart(splitWord).Split(splitWord);
					for (int i = 0; i < updateSourceCodePartsFor.Length; i++)
					{
						var codePart = updateSourceCodePartsFor[i];
						var curIndex = records.FindIndex(x => x.code == codePart);
						if (curIndex >= 0)
						{
							var record = records[curIndex];
							if (i == 0)
							{
								source = record.source + source;
							}
							record.source = source;
						}
					}

					var updateScopeCodeParts = waitingUpdatedScopeCode.TrimStart(splitWord).Split(splitWord);
					for (int i = 0; i < updateScopeCodeParts.Length; i++)
					{
						var codePart = updateScopeCodeParts[i];
						var curIndex = records.FindIndex(x => x.code == codePart);
						if (curIndex > 0)
						{
							var record = records[curIndex];
							if (i == 0)
							{
								scope = record.scope + scope;
							}
							record.scope = scope;
						}
					}

					var updateRemarkCodeParts = waitingUpdatedRemarkCode.TrimStart(splitWord).Split(splitWord);
					for (int i = 0; i < updateRemarkCodeParts.Length; i++)
					{
						var codePart = updateRemarkCodeParts[i];
						var curIndex = records.FindIndex(x => x.code == codePart);
						if (curIndex > 0)
						{
							var record = records[curIndex];
							if (i == 0)
							{
								remark = record.remark + remark;
							}
							record.remark = remark;
						}
					}
				}
				else
				{
					if (source == string.Empty)
					{
						waitingUpdatedSourceCode += splitWord + code;
					}
					else
					{
						waitingUpdatedSourceCode = code;
					}

					if (scope == string.Empty)
					{
						waitingUpdatedScopeCode += splitWord + code;
					}
					else
					{
						waitingUpdatedScopeCode = code;
					}
					if (remark == string.Empty)
					{
						waitingUpdatedRemarkCode += splitWord + code;
					}
					else
					{
						waitingUpdatedRemarkCode = code;
					}
				}

				if (source == string.Empty)
				{
					source = previousSource;
				}
				else
				{
					previousSource = source;
				}

				if (scope == string.Empty)
				{
					scope = previousScope;
				}
				else
				{
					previousScope = scope;
				}

				if (remark == string.Empty)
				{
					remark = GetRemark(code, previousRemark);
				}
				else
				{
					previousRemark = GetRemark(code, remark);
				}

				var codeParts = code.Split(splitWord);
				foreach (var codePart in codeParts)
				{
					AddRecordWithCode(source, codePart, scope, remark);
				}
			}
		}

		string GetRemark(string code, string remark)
		{
			if (code == "VP999999999999" ||
				code == "DHM00000000504" ||
				code == "DHM99999999506" ||
				code == "ID999999999991" ||
				code == "DHM99999999990" ||
				code == "DHK99999999999" ||
				code == "ML999999999989")
			{
				return string.Empty;
			}
			else if (code == "DH000000000001" ||
				code == "DH000000000002" ||
				code == "DH000000000003" ||
				code == "DH000000000004" ||
				code == "DH000000000005")
			{
				return "一、輸入依食品安全衛生管理法第30條第1項公告應申請查驗之產品，非供販賣，且其金額、數量符合條件者，得免申請輸入查驗，並於輸入時，填報下揭通關代碼於進口報單輸入許可證號碼欄中，並聲明符合食品安全衛生管理法第30條第3項免申請查驗之規定。二、以下項目不適用通關代碼：(一)食品添加物及香料。(二)牛海綿狀腦病發生國家所生產供食用牛隻之屠肉、組織、器官、衍生物或含前揭物品者。(詳見衛生福利部部授食字第1041303340號公告附件)。三、單一項次係指品名、成分、廠牌、製造廠及產地應相同，包括同一品名之所有批號、製造日期(有效日期)或(包裝)規格之總量。";
			}
			else
			{
				return remark;
			}
		}

		void AddRecordWithCode(string source, string code, string scope, string remark)
		{
			if (Regex.IsMatch(code, "[A-Z0-9]{14}"))
			{
				var record = new Record()
				{
					source = source,
					code = code,
					scope = scope,
					remark = remark,
				};
				records.Add(record);
			}
		}

		void ParseCodeAndScopeChunks(TextChunk item)
		{
			const float TableEndPosition = 765f;
			const float TableStartPosition = 75f;

			var startLocation = item.GetLocation().GetStartLocation();
			if (startLocation.Get(1) <= TableEndPosition && startLocation.Get(1) >= TableStartPosition)
			{
				if (isNewPage)
				{
					ClearChunks();
					isNewPage = false;
				}
				var startLocation0 = startLocation.Get(0);
				if (startLocation0 < CodeStartPosition)
				{
					if (currentZone > 1 || Regex.IsMatch(previousItemSource, "\\)$"))
					{
						ClearChunks();
					}
					sourceChunks.Add(item);
					previousItemSource = item.GetText();
					currentZone = 1;
				}
				else if (startLocation0 < ScopeStartPosition)
				{
					if (currentZone > 2)
					{
						ClearChunks();
					}
					codeChunks.Add(item);
					currentZone = 2;
				}
				else if (startLocation0 < RemarkStartPosition)
				{
					if (currentZone > 3)
					{
						ClearChunks();
					}
					scopeChunks.Add(item);
					currentZone = 3;
				}
				else if (startLocation0 < RemarkEndPosition)
				{
					remarkChunks.Add(item);
					currentZone = 4;
				}
			}
			else
			{
				isNewPage = true;
			}
		}

		void ClearChunks()
		{
			if (sourceChunks.Any() || codeChunks.Any() || scopeChunks.Any() || remarkChunks.Any())
			{
				pdfDatas.Add(new string[] { ConvertTextChunksToText(sourceChunks), ConvertTextChunksToText(codeChunks), ConvertTextChunksToText(scopeChunks), ConvertTextChunksToText(remarkChunks) });
				sourceChunks = new List<TextChunk>();
				codeChunks = new List<TextChunk>();
				scopeChunks = new List<TextChunk>();
				remarkChunks = new List<TextChunk>();
			}
		}

		string ConvertTextChunksToText(List<TextChunk> chunks) => string.Join(string.Empty, chunks.Select(m => m.GetText()));

		List<Record> records = new List<Record>();
		class Record
		{
			public string source;
			public string code;
			public string scope;
			public string remark;
		}
	}
}
