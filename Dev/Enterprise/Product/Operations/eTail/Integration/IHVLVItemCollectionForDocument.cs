
namespace Enterprise.eTail.Integration
{
	public interface IHVLVItemCollectionForDocument : IHVLVItemCollection
	{
		new IHVLVItemForDocument this[int i] { get; }
	}
}
