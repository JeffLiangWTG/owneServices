namespace Enterprise.eTail.Integration
{
	public interface IHVLVISFItemInfoProvider : IHVLVItem
	{
		IHVLVISFBillInfoProvider Consignment { get; }
		IHVLVItemLineCollection Lines { get; }
	}
}
