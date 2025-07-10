namespace Enterprise.Packing.Business
{
	public enum ParentJobType
	{
		None,
#if DEBUG
		Dummy,
#endif
		WarehouseOrder
	}
}
