using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobShipmentPrePlanningProcessTaskCollection : RoutingSupportProcessTaskCollection
	{
		public JobShipmentPrePlanningProcessTaskCollection(JobShipmentPreplanning preAdvice)
			: base(preAdvice)
		{
		}

		#region Implementation

		public new JobShipmentPreplanningProcessTask this[int index]
		{
			get { return (JobShipmentPreplanningProcessTask)Elements[index]; }
		}

		public virtual new JobShipmentPreplanningProcessTask AddNew()
		{
			return (JobShipmentPreplanningProcessTask)base.AddNew();
		}

		new JobShipmentPreplanning Parent
		{
			get { return (JobShipmentPreplanning)base.Parent; }
		}

		#endregion

		public override ZString OriginCountry
		{
			get { return Parent.PortLoad == null ? ZString.Empty : Parent.PortLoad.RL_RN_NKCountryCode; }
		}

		public override ZString DestinationCountry
		{
			get { return Parent.PortDisch == null ? ZString.Empty : Parent.PortDisch.RL_RN_NKCountryCode; }
		}
	}
}
