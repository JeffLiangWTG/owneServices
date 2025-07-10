using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class WhsItemReceiveASNRTUPivot : AutoWhsItemReceiveASNRTUPivot
	{
		public WhsItemReceiveASNRTUPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		public WhsItemReceiveASN ReceiveASN => Factory.Load<WhsItemReceiveASN>(WAR_WRP_TransitReceiveASN);

		public WhsItemReceiveTransportationUnit ReceiveTransportationUnit => Factory.Load<WhsItemReceiveTransportationUnit>(WAR_WRH_TransitReceiveTransportationUnit);

		#endregion

		#region Properties

		[RelatedBusinessObject(nameof(ReceiveASN))]
		public override ZGuid WAR_WRP_TransitReceiveASN { get => base.WAR_WRP_TransitReceiveASN; }

		[RelatedBusinessObject(nameof(ReceiveTransportationUnit))]
		public override ZGuid WAR_WRH_TransitReceiveTransportationUnit { get => base.WAR_WRH_TransitReceiveTransportationUnit; }

		#endregion
	}
}
