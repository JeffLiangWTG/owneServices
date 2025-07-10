using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingPackLine : AgencyShipmentPackLine
	{
		public AgencyBookingPackLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JL_UnitOfDimension = AgencyRegistry.Instance.DefaultBookingDimensionUnit.Value;
		}

		#region Related Business Objects

		protected override CommonShipment GetParentShipment()
		{
			return Factory.Load<AgencyBooking>(JL_JS);
		}
		public new AgencyBooking Shipment
		{
			get { return (AgencyBooking)base.Shipment; }
		}

		public new AgencyBookingContainer Container
		{
			get { return (AgencyBookingContainer)LoadContainer(JL_JC); }
		}

		protected override CommonContainer LoadContainer(ZGuid containerPK)
		{
			return Factory.Load<AgencyBookingContainer>(containerPK);
		}

		protected override AgencyShipmentContainerManyToManyCollection GetNewContainersCollectionCore()
		{
			return new AgencyBookingContainerManyToManyCollection(this);
		}

		#endregion
	}
}



