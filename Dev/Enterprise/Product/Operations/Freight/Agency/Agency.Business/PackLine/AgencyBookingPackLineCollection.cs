using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingPackLineCollection : AgencyShipmentPackLineCollection
	{
		public AgencyBookingPackLineCollection(AgencyShipment master) : base(master)
		{
		}

		public new AgencyBookingPackLine AddNew()
		{
			return (AgencyBookingPackLine)base.AddNew();
		}

		public new AgencyBookingPackLine this[int index]
		{
			get { return (AgencyBookingPackLine)Elements[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(AgencyBookingPackLine);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			PackLine packline = (PackLine)child;
			packline.JL_UnitOfDimension = AgencyRegistry.Instance.DefaultBookingDimensionUnit.Value;
		}

		#region Implementation

		#endregion

	}
}
