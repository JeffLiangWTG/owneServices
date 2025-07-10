using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportBookings.Business
{
	public class ViewTransportBookingParentLegs : AutoViewTransportBookingParentLegs
	{
		public ViewTransportBookingParentLegs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.RefUNLOCOs")]
		public override ZString VL_RL_NKDischarge
		{
			get { return base.VL_RL_NKDischarge; }
			set { base.VL_RL_NKDischarge = value; }
		}

		[List("Lookups.RefUNLOCOs")]
		public override ZString VL_RL_NKLoad
		{
			get { return base.VL_RL_NKLoad; }
			set { base.VL_RL_NKLoad = value; }
		}

		[List("Lookups.RefVessels")]
		public override ZString VL_Vessel
		{
			get { return base.VL_Vessel; }
			set { base.VL_Vessel = value; }
		}

		public override void Delete()
		{
			throw new NotSupportedException("Deletion of the ViewTransportBookingParentLegs is not supported.");
		}

		public override bool ReadOnly
		{
			get { return true; }
			set { base.ReadOnly = value; }
		}
	}
}
