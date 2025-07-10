using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal sealed class RollBookingActionMethod : OperationalActionMethod
	{
		public RollBookingActionMethod()
			: base(new Guid("70101184-fd80-46ce-98a0-f6e33da96ffc"))
		{
		}

		#region Implementation

		public override string Name => Res.GetString("70101184-fd80-46ce-98a0-f6e33da96ffc", "Roll Bookings");

		public override string Description => Res.GetString("acb41ae4-1044-479b-be59-febc7d0207b6", "Roll bookings to another vessel and voyage.");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new RollBookingApplicator(factory);
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new RollBookingApplicatorControl();

		public override LicenceCheckpoint[] GetRequiredLicenceCheckpoints()
		{
			return new LicenceCheckpoint[] { Env.Licence.ShippingManagerBookings };
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[] { Env.Security.AgencyBookingEdit };
		}

		#endregion
	}
}


