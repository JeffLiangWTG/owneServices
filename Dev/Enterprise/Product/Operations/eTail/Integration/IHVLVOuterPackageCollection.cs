using CargoWise.EntityFramework;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVOuterPackageCollection : IBusinessObjectCollection
	{
		// Binding uses this to determine element type of a collection
		new IHVLVOuterPackage this[int i] { get; }
	}
}
