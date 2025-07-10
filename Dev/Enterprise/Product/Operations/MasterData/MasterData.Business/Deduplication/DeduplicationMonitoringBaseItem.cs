using System.Drawing;

namespace Enterprise.MasterData.Business
{
	public abstract class DeduplicationMonitoringBaseItem
	{
		public string ItemPath { get; set; }

		public Image Icon { get; set; }

		public string DataType { get; set; }

		public string DataValue { get; set; }

		public object Tag { get; set; }

		public abstract string Name { get; set; }

		public DeduplicationMonitoringBaseItem Parent { get; set; }
	}
}
