using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class WhsItemDispatchLoadListDTUPivot : AutoWhsItemDispatchLoadListDTUPivot
	{
		public WhsItemDispatchLoadListDTUPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		public WhsItemDispatchTransportationUnit DispatchTransportationUnit => Factory.Load<WhsItemDispatchTransportationUnit>(WLD_WDH_TransitDispatchTransportationUnit);

		public WhsItemDispatchLoadList DispatchLoadList => Factory.Load<WhsItemDispatchLoadList>(WLD_WDL_TransitDispatchLoadList);

		#endregion

		#region Properties

		[RelatedBusinessObject(nameof(DispatchTransportationUnit))]
		public override ZGuid WLD_WDH_TransitDispatchTransportationUnit { get => base.WLD_WDH_TransitDispatchTransportationUnit; }

		[RelatedBusinessObject(nameof(DispatchLoadList))]
		public override ZGuid WLD_WDL_TransitDispatchLoadList { get => base.WLD_WDL_TransitDispatchLoadList; }

		#endregion
	}
}
