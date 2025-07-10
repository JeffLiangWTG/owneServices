using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class DataSetStructureProvider
	{
		public static IEnumerable<string[]> StructuredDataSets
		{
			get
			{
				if (_structuredDataSets == null)
				{
					InitDataSetsStructureOrUserViewFromTTFile();
				}
				return _structuredDataSets;
			}
		}
		static IEnumerable<string[]> _structuredDataSets;

		public static IEnumerable<string> UserViews
		{
			get
			{
				if (_userViews == null)
				{
					InitDataSetsStructureOrUserViewFromTTFile();
				}
				return _userViews;
			}
		}
		static IEnumerable<string> _userViews;

		static void InitDataSetsStructureOrUserViewFromTTFile()
		{
			_structuredDataSets = new List<string[]>();
			_userViews = new List<string>();
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "CargoWise.RefDbRepo.Common.Utils.SafeDataSetStructure.DataSets.ttinclude";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
			{
				var contents = reader.ReadToEnd().Trim();
				var startIndex = contents.IndexOf("return new []", System.StringComparison.Ordinal) + 20;
				var length = contents.LastIndexOf(";", System.StringComparison.Ordinal) - 1 - startIndex;
				contents = contents.Substring(startIndex, length).Trim();
				var dataSetArray = contents.Split(new[] { "new []" }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var dataSet in dataSetArray)
				{
					var dataSetString = dataSet.Replace("{", string.Empty).Replace("}", string.Empty).Trim();
					var tableArray = dataSetString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var table in tableArray.Where(x => !string.IsNullOrEmpty(x)))
					{
						if (!table.TrimStart().StartsWith("\"", System.StringComparison.Ordinal) || !table.TrimEnd().EndsWith("\"", System.StringComparison.Ordinal))
						{
							throw new NotSupportedException("Datasets.ttinclude has an incorrect format, it cannot be read as an string array");
						}
					}
					tableArray = tableArray.Select(x => x.Replace("\"", string.Empty)).ToArray();

					var parentTable = tableArray[0].Trim();
					if (parentTable.EndsWith("View", StringComparison.OrdinalIgnoreCase))
					{
						_userViews = _userViews.Append(parentTable);
						continue;
					}

					tableArray = tableArray.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
					_structuredDataSets = _structuredDataSets.Append(tableArray);
				}
			}
		}
	}
}
