using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class DetentionContainerRatingJobDatesProvider : JobDatesProvider<ContainerMovement>
	{
		public DetentionContainerRatingJobDatesProvider(ContainerMovement movement)
			: base(movement) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			return Parent.E9_MovementDate;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return Parent.E9_MovementDate;
		}

		protected override ZDateTime GetJobOpenDateCore()
		{
			return Parent.Detention?.Job?.JH_A_JOP ?? ZDateTime.Empty;
		}
	}
}
