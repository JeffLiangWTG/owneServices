using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public interface ITransportCollection : IBusinessObjectCollection
	{
		new Transport this[int index] { get; }
		void Load();
		void RemoveAndDelete(BusinessObject elementToDelete);
	}
}
