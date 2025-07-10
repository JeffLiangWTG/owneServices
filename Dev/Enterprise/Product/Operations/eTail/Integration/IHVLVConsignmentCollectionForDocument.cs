
namespace Enterprise.eTail.Integration
{
	public interface IHVLVConsignmentCollectionForDocument : IHVLVConsignmentCollection
	{
		new IHVLVConsignmentForDocument this[int i] { get; }
	}
}
