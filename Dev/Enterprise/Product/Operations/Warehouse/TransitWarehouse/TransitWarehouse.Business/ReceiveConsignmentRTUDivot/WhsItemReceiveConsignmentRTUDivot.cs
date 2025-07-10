using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class WhsItemReceiveConsignmentRTUDivot : AutoWhsItemReceiveConsignmentRTUDivot
	{
		public WhsItemReceiveConsignmentRTUDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		public WhsItemReceiveConsignment ReceiveConsignment => Factory.Load<WhsItemReceiveConsignment>(WRD_WRC_TransitReceiveConsignment);

		public WhsItemReceiveTransportationUnit ReceiveTransportationUnit => Factory.Load<WhsItemReceiveTransportationUnit>(WRD_WRH_TransitReceiveHeader);

		#endregion

		#region Properties

		[RelatedBusinessObject(nameof(ReceiveConsignment))]
		public override ZGuid WRD_WRC_TransitReceiveConsignment { get => base.WRD_WRC_TransitReceiveConsignment; }

		[RelatedBusinessObject(nameof(ReceiveTransportationUnit))]
		public override ZGuid WRD_WRH_TransitReceiveHeader { get => base.WRD_WRH_TransitReceiveHeader; }

		#endregion
	}
}
