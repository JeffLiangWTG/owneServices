using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used in future Work Items")]
	public class DtbConsignmentLeg : AutoDtbConsignmentLeg
	{
		public DtbConsignmentLeg(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		public DtbConsignmentAction PickupConsignmentAction
		{
			get { return Factory.Load<DtbConsignmentAction>(LTG_LTA_Pickup); }
		}

		public DtbConsignmentAction DeliveryConsignmentAction
		{
			get { return Factory.Load<DtbConsignmentAction>(LTG_LTA_Delivery); }
		}

		public DtbConsignment Consignment
		{
			get { return Factory.Load<DtbConsignment>(LTG_LTC_Consignment); }
		}

		#endregion
	}
}
