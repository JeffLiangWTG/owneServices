using CargoWise.EntityFramework;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVItemCollection : IBusinessObjectCollection
	{
		new IHVLVItem this[int i] { get; }
		void Load();
	}
}
