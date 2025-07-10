namespace Enterprise.MasterData.Business
{
	public class DeduplicationMonitoringTypeItem : DeduplicationMonitoringBaseItem
	{
		public DeduplicationMonitoringTypeItem(string name, string dataType, object tag, DeduplicationMonitoringBaseItem parent)
		{
			ItemPath = name;
			DataType = dataType;
			Tag = tag;
			Parent = parent;
		}

		public override string Name
		{
			get { return ItemPath; }
			set { ItemPath = value; }
		}
	}
}
