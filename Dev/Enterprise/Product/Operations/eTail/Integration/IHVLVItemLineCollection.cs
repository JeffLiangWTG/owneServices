using CargoWise.EntityFramework;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVItemLineCollection : IBusinessObjectCollection
	{
		// Binding uses this to determine element type of a collection
		new IHVLVItemLine this[int i] { get; }
	}
}
