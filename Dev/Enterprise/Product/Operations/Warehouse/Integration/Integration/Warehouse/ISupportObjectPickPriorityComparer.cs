namespace Enterprise.Warehouse.Integration.Warehouse
{
	public interface ISupportObjectPickPriorityComparer
	{
		int Compare(ISupportPickPriority o1, ISupportPickPriority o2);
	}
}
