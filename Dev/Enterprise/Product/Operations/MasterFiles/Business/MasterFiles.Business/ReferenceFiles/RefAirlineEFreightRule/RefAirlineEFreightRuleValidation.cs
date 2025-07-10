using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineEFreightRuleValidation : AutoRefAirlineEFreightRuleValidation
	{
		public RefAirlineEFreightRuleValidation(AutoRefAirlineEFreightRule parent) : base(parent)
		{
		}

		new RefAirlineEFreightRule Parent
		{
			get { return base.Parent as RefAirlineEFreightRule; }
		}

		protected override void CheckRME_OriginLocation()
		{
			base.CheckRME_OriginLocation();
			ListValidation.ErrorIfInvalidCode(Parent.RME_OriginLocationInfo, Parent.Lookups.Locations);

			if (Parent.RME_DestinationLocation.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.RME_OriginLocationInfo);
			}
		}

		protected override void CheckRME_DestinationLocation()
		{
			base.CheckRME_DestinationLocation();
			ListValidation.ErrorIfInvalidCode(Parent.RME_DestinationLocationInfo, Parent.Lookups.Locations);

			if (Parent.RME_OriginLocation.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.RME_DestinationLocationInfo);
			}
		}

		protected override void CheckRME_EFreightStatus()
		{
			base.CheckRME_EFreightStatus();
			ListValidation.ErrorIfInvalidCode(Parent.RME_EFreightStatusInfo, Parent.Lookups.EFreightStatus_List);
		}
	}
}
