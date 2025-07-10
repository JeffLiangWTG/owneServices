using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IRelatedNamedAccountsPivotCollection<out T> : IBusinessObjectCollection
	where T : IPivotBusinessObject, IRatingContractNamedAccountPivot
	{
		public IReadOnlyCollection<IOrgHeader> GetAllNamedAccounts();
		public T AddChild(BusinessObject child);
		public IPivotBusinessObject AddRelatedIfNotExist(BusinessObject related, bool addAlwaysAsParent = false);
		public new T AddNew();
		new T this[int i] { get; }
	}
}
