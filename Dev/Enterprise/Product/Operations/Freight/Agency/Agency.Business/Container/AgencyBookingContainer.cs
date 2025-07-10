using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingContainer : AgencyShipmentContainer, Integration.Agency.IAgencyBookingContainer
	{
		public AgencyBookingContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Related BusinessObjects

		public new AgencyBooking Booking
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyBooking)base.Booking; }
		}

		protected override Type ShipmentType
		{
			get { return typeof(AgencyBooking); }
		}

		[ChildEditable(true)]
		public new AgencyBookingPackLineManyToManyCollection PackLines
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyBookingPackLineManyToManyCollection)base.PackLines; }
		}

		protected override PackLineManyToManyCollection GetNewPackLineCollection()
		{
			return new AgencyBookingPackLineManyToManyCollection(this);
		}

		#endregion

		#region Implementation

		protected override ZString DefaultWeightUnit
		{
			get { return AgencyRegistry.Instance.DefaultBookingWeightUnit.Value; }
		}

		protected override ZString DefaultVolumeUnit
		{
			get { return AgencyRegistry.Instance.DefaultBookingVolumeUnit.Value; }
		}

		protected override AgencyShipmentContainerValidation GetNewContainerValidation()
		{
			return new AgencyBookingContainerValidation(this);
		}

		#endregion
	}
}
