using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	public class UpdateReturnByActionMethod : OperationalActionMethod
	{
		public UpdateReturnByActionMethod()
			: base(new ZGuid("3b6e6bdd-d49d-4f8f-af34-3a6e3b42eac7")) { }

		public override string Name
		{
			get { return Res.GetString("3b6e6bdd-d49d-4f8f-af34-3a6e3b42eac7", "Update Return Required By Dates"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description
		{
			get
			{
				return Res.GetString(
					"3414d813-3d60-4103-a325-53eeec3d8afe",
					@"Re-calculates the containers return required by date from the final availability date, and the configured detention free days.
If the required by date can be calculated then the detention days for the relevant movements will also be updated.");
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateReturnByApplicator();
		}
	}
}


