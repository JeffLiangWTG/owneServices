using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITagLinkCollection : IBusinessObjectCollection
	{
		new ITagLink this[int index] { get; }
	}
}
