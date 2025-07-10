using System.Collections.Generic;
using System.Text;

namespace Enterprise.MasterData.Common
{
	public class DeduplicationExclusion
	{
		readonly SortedDictionary<string, string> items = new SortedDictionary<string, string>();
		public int maxRecordToBuild = 20;

		public string this[string key]
		{
			get { return items[key]; }
			set { items[key] = value; }
		}

		public int Count => items.Count;

		public string BuildDisplay()
		{
			var builder = new StringBuilder();

			builder.AppendLine();
			var counter = 0;

			if (items.Count > maxRecordToBuild)
			{
				builder.Append("  ");
				builder.AppendLine(Res.GetString("235a270d-2ed6-4d7f-b650-a0c2e4d6cdda", "Showing details of the first {0} records", maxRecordToBuild));
				builder.AppendLine();
			}

			foreach (var item in items)
			{
				if (counter < maxRecordToBuild)
				{
					builder.AppendLine("   " + item.Key + ": " + item.Value + "    ");
					counter++;
				}
				else
				{
					break;
				}
			}

			builder.AppendLine(" ");

			return builder.ToString();
		}
	}
}
