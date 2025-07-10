using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineEFreightRule : AutoRefAirlineEFreightRule
	{
		public RefAirlineEFreightRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region RME_OriginLocation

		[List("Lookups.Locations")]
		public override ZString RME_OriginLocation
		{
			get { return base.RME_OriginLocation; }
			set { base.RME_OriginLocation = value; }
		}

		#endregion

		#region RME_DestinationLocation

		[List("Lookups.Locations")]
		public override ZString RME_DestinationLocation
		{
			get { return base.RME_DestinationLocation; }
			set { base.RME_DestinationLocation = value; }
		}

		#endregion

		#region RME_EFreightStatus

		[List("Lookups.EFreightStatus_List")]
		public override ZString RME_EFreightStatus
		{
			get { return base.RME_EFreightStatus; }
			set { base.RME_EFreightStatus = value; }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			RemoveRowError(HasDuplicateErrorMessage);

			if (Airline != null
				&& Airline.EFreightStatusCollection
					.Cast<RefAirlineEFreightRule>()
					.Any(c => c.PK != PK && c.RME_DestinationLocation == RME_DestinationLocation && c.RME_OriginLocation == RME_OriginLocation))
			{
				AddRowError(HasDuplicateErrorMessage);
			}
		}

		static string HasDuplicateErrorMessage
		{
			get { return ResString.GetMultilingualString("08d78547-2a21-4aee-8273-45a3420323a2", "You cannot have duplicate rules. Please differentiate the rule origin and destination location."); }
		}
	}
}
