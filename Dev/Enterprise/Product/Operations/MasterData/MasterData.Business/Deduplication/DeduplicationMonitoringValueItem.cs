namespace Enterprise.MasterData.Business
{
	public class DeduplicationMonitoringValueItem : DeduplicationMonitoringBaseItem
	{
		public DeduplicationMonitoringValueItem(string name, string dataValue, DeduplicationMonitoringBaseItem parent)
		{
			ItemPath = name;
			DataValue = dataValue;
			Parent = parent;
		}

		public override string Name
		{
			get { return ItemPath; }
			set { ItemPath = value; }
		}
	}
}
