using System.Collections.Generic;
using System.Data;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationMonitoringMethodNameItem : DeduplicationMonitoringBaseItem
	{
		public DeduplicationMonitoringMethodNameItem(string name, string dataType)
		{
			ItemPath = name;
			DataType = dataType;
		}

		public override string Name
		{
			get { return ItemPath; }
			set { ItemPath = value; }
		}

		public string DBCommandText { get; set; }

		public string QueryTitle { get; set; }

		public string DebugLog { get; set; }

		public Dictionary<string, DataTable> Tables { get; set; } = new Dictionary<string, DataTable>();
	}
}
