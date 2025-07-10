using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class PickableDocketAttacher : ZRecordAttacher
	{
		public PickableDocketAttacher(WhsPick pick, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			: base(destinationCollection, findBoxList, moduleID)
		{
			Pick = pick;
		}

		WhsPick Pick { get; }

		protected override void AttachItemsCore(IBusinessObjectCollection destinationCollection, IEnumerable<BusinessObject> list)
		{
			Pick.AddOrders(list.Cast<WhsPickableDocket>());
		}
	}
}
