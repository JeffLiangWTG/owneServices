using CargoWise.EntityFramework;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVConsignmentCollection : IBusinessObjectCollection
	{
		new IHVLVConsignment this[int i] { get; }
		void Load();
	}
}
