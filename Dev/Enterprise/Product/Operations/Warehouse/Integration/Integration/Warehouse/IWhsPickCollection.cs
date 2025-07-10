using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Integration
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IWhsPickCollection : IBusinessObjectCollection
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		void Load();
	}
}
