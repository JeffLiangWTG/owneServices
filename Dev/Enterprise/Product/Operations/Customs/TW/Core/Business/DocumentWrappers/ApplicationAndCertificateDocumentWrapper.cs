using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class ApplicationAndCertificateDocumentWrapper : DocumentWrapper, IDocumentWrapper
	{
		public ApplicationAndCertificateDocumentWrapper(CusEntryHeader entryHeader, ZString chassisNumber, string baseGoodsDescription, ZDecimal netWeightInKG, ZDecimal unitCommodityTax, List<string> additionalDocuments) : base(entryHeader, entryHeader.Factory)
		{
			this.entryHeader = entryHeader;
			cusDataHeader = new NX5105MessageSendingObject(entryHeader);
			ChassisNumber = chassisNumber;
			NetWeightInKG = netWeightInKG;
			UnitCommodityTax = unitCommodityTax;
			AdditionalDocuments = additionalDocuments;
			Lines = new BusinessObjectCollectionWrapper<GoodsDescriptionLine>();
			SetGoodsDescriptionLines(baseGoodsDescription);
		}

		#region Lines
		readonly string chassisNoTag = (NoResString)"CHASSIS NO";
		void SetGoodsDescriptionLines(string baseGoodsDescription)
		{
			var originalGoodsDescription = GetGoodsDescription(baseGoodsDescription);

			var lines = originalGoodsDescription.Split(new string[] { "\n\r", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
			GoodsDescriptionLine goodsDescription = null;
			int num = 0;
			foreach (var line in lines)
			{
				num++;
				var newLines = CutString(line);
				for (var i = 0; i < newLines.Count; i++)
				{
					var newLine = newLines[i];
					if (goodsDescription == null)
					{
						goodsDescription = new GoodsDescriptionLine();
						Lines.Add(goodsDescription);
					}
					if (i == 0)
					{
						goodsDescription.AddLine(newLine, num);
					}
					else
					{
						goodsDescription.AddLine(newLine);
					}
					if (goodsDescription.Count >= 12)
					{
						goodsDescription = null;
					}
				}
			}
			if (Lines.Count > 0)
			{
				var line = Lines[Lines.Count - 1];
				if (line.Count < 12)
				{
					line.AddLine((NoResString)"“         以下空白         ”");
				}
			}
		}

		string GetGoodsDescription(string baseGoodsDescription)
		{
			var regex = new Regex(string.Format(CultureInfo.InvariantCulture, "{0}[ ]*[:：][ ]*(?<chassis>[\\w ,;，；]*)", chassisNoTag), RegexOptions.IgnoreCase);
			var chassisNoLine = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", chassisNoTag, ChassisNumber);
			var originalGoodsDescription = regex.IsMatch(baseGoodsDescription) ? regex.Replace(baseGoodsDescription, chassisNoLine) : string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", baseGoodsDescription, System.Environment.NewLine, chassisNoLine);
			return originalGoodsDescription;
		}

		internal static List<string> CutString(string line, int length = 64)
		{
			var regex = new Regex("^[(（]*[0-9]+[)）.]*[ ]*");
			var matches = regex.Matches(line);
			if (matches.Count > 0)
			{
				line = regex.Replace(line, string.Empty);
			}

			var newLines = new List<string>();
			var words = line.Split(new string[] { " " }, System.StringSplitOptions.None);
			var newLineList = new List<string>();
			foreach (var word in words)
			{
				newLineList.Add(word);
				AddStringIfNeed(length, newLines, newLineList, word);
			}
			AddLastString(newLineList, length, newLines);
			return newLines;
		}

		static void AddStringIfNeed(int length, List<string> newLines, List<string> newLineList, string word)
		{
			var newLine = newLineList.Count > 0 ? string.Join(" ", newLineList) : string.Empty;
			if (newLine.Length >= length)
			{
				if (newLine.Length == length)
				{
					newLines.Add(newLine);
					newLineList.Clear();
					newLineList.Add("");
				}
				else if (newLineList.Count > 1)
				{
					newLineList.RemoveAt(newLineList.Count - 1);
					newLine = string.Join(" ", newLineList);
					newLines.Add(newLine);
					newLineList.Clear();
					if (newLine.Length == length)
					{
						newLineList.Add("");
					}
					newLineList.Add(word);
				}
				else
				{
					newLines.Add(newLine.Substring(0, length));
					newLineList.Clear();
					newLineList.Add(newLine.Substring(length, newLine.Length - length));
				}
			}
		}

		static void AddLastString(List<string> newLineList, int length, List<string> newLines)
		{
			var newLine = newLineList.Count > 0 ? string.Join(" ", newLineList) : string.Empty;
			if (!string.IsNullOrEmpty(newLine.Trim()))
			{
				if (newLine.Length <= length)
				{
					newLines.Add(newLine);
				}
				else
				{
					newLines.Add(newLine.Substring(0, length));
					newLines.Add(newLine.Substring(length, newLine.Length - length));
				}
			}
		}
		#endregion

		public BusinessObjectCollectionWrapper<GoodsDescriptionLine> Lines { get; }

		readonly INX5105Declaration cusDataHeader;

		readonly CusEntryHeader entryHeader;

		public ZDecimal NetWeightInKG { get; }

		public ZDecimal UnitCommodityTax { get; }

		List<string> AdditionalDocuments { get; }

		#region Implementation of INX5105Declaration
		public ZString ImporterChineseName => cusDataHeader?.Importer?.ChineseName ?? ZString.Empty;

		public ZString BorderTransportMeans => entryHeader.Declaration?.JE_VesselName ?? ZString.Empty;

		ZDateTime GoodsShipmentExitDateTime => cusDataHeader?.GoodsShipment?.ExitDateTime ?? ZDateTime.Empty;

		public ZString GoodsShipmentExitDateTimeYear => GoodsShipmentExitDateTime.IsValid ? (GoodsShipmentExitDateTime.Year - 1911).ToString(CultureInfo.InvariantCulture) : string.Empty;

		public ZString GoodsShipmentExitDateTimeMonth => GoodsShipmentExitDateTime.Month.ToString(CultureInfo.InvariantCulture);

		public ZString GoodsShipmentExitDateTimeDay => GoodsShipmentExitDateTime.Day.ToString(CultureInfo.InvariantCulture);

		ZDateTime BorderTransportMeansArrivalDateTime => cusDataHeader?.BorderTransportMeans?.ArrivalDateTime ?? ZDateTime.Empty;

		public ZString BorderTransportMeansArrivalDateTimeYear => BorderTransportMeansArrivalDateTime.IsValid ? (BorderTransportMeansArrivalDateTime.Year - 1911).ToString(CultureInfo.InvariantCulture) : string.Empty;

		public ZString BorderTransportMeansArrivalDateTimeMonth => BorderTransportMeansArrivalDateTime.Month.ToString(CultureInfo.InvariantCulture);

		public ZString BorderTransportMeansArrivalDateTimeDay => BorderTransportMeansArrivalDateTime.Day.ToString(CultureInfo.InvariantCulture);

		public ZString DeclarationID => cusDataHeader?.ID ?? ZString.Empty;

		public ZString DeclarationIDFormatted
		{
			get
			{
				var id = DeclarationID;
				return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}/{3}/{4}", CutString(id, 0, 2), CutString(id, 2, 2), CutString(id, 4, 2), CutString(id, 6, 3), CutString(id, 9, 5));
			}
		}

		public ZString FirstAdditionalDocumentID => AdditionalDocuments?.FirstOrDefault() ?? ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		public ZString DocumentRemark
		{
			get
			{
				var result = "此欄空白";
				var list = AdditionalDocuments;
				if (list?.Count > 1)
				{
					result = string.Join(System.Environment.NewLine, list.Skip(1));
				}
				return result;
			}
		}
		#endregion

		#region Implementation of EntryHeader
		public ZString PortOfLoadingName => entryHeader?.Declaration?.PortOfLoading?.RL_PortName ?? ZString.Empty;

		public ZString PortOfArrivalName => entryHeader?.Declaration?.PortOfArrival?.RL_PortName ?? ZString.Empty;
		#endregion

		#region Implementation
		public ZString ChassisNumber { get; }

		public ZString CustomsName => SharedHelper.GetCustomsOfficeName(entryHeader.EntryNumber);

		ZString CutString(ZString id, int startIndex, int cutLength)
		{
			return id.Length >= (startIndex + cutLength) ? id.Substring(startIndex, cutLength) : ZString.Empty;
		}

		#endregion

		public class GoodsDescriptionLine : DocumentWrapper
		{
			readonly List<ZString> lines = new List<ZString>();
			readonly List<ZString> numbers = new List<ZString>();

			public void AddLine(ZString line, int num = 0)
			{
				lines.Add(line);
				numbers.Add(num > 0 ? ZString.Format("{0})", num) : ZString.Empty);
			}

			public int Count => lines.Count;

			public ZString Line1 => lines.Count > 0 ? lines[0] : ZString.Empty;

			public ZString Line2 => lines.Count > 1 ? lines[1] : ZString.Empty;

			public ZString Line3 => lines.Count > 2 ? lines[2] : ZString.Empty;

			public ZString Line4 => lines.Count > 3 ? lines[3] : ZString.Empty;

			public ZString Line5 => lines.Count > 4 ? lines[4] : ZString.Empty;

			public ZString Line6 => lines.Count > 5 ? lines[5] : ZString.Empty;

			public ZString Line7 => lines.Count > 6 ? lines[6] : ZString.Empty;

			public ZString Line8 => lines.Count > 7 ? lines[7] : ZString.Empty;

			public ZString Line9 => lines.Count > 8 ? lines[8] : ZString.Empty;

			public ZString Line10 => lines.Count > 9 ? lines[9] : ZString.Empty;

			public ZString Line11 => lines.Count > 10 ? lines[10] : ZString.Empty;

			public ZString Line12 => lines.Count > 11 ? lines[11] : ZString.Empty;

			public ZString Number1 => numbers.Count > 0 ? numbers[0] : ZString.Empty;

			public ZString Number2 => numbers.Count > 1 ? numbers[1] : ZString.Empty;

			public ZString Number3 => numbers.Count > 2 ? numbers[2] : ZString.Empty;

			public ZString Number4 => numbers.Count > 3 ? numbers[3] : ZString.Empty;

			public ZString Number5 => numbers.Count > 4 ? numbers[4] : ZString.Empty;

			public ZString Number6 => numbers.Count > 5 ? numbers[5] : ZString.Empty;

			public ZString Number7 => numbers.Count > 6 ? numbers[6] : ZString.Empty;

			public ZString Number8 => numbers.Count > 7 ? numbers[7] : ZString.Empty;

			public ZString Number9 => numbers.Count > 8 ? numbers[8] : ZString.Empty;

			public ZString Number10 => numbers.Count > 9 ? numbers[9] : ZString.Empty;

			public ZString Number11 => numbers.Count > 10 ? numbers[10] : ZString.Empty;

			public ZString Number12 => numbers.Count > 11 ? numbers[11] : ZString.Empty;
		}
	}
}
