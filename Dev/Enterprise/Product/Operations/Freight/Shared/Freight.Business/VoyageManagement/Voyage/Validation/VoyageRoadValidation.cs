using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class VoyageRoadValidation : BaseJobVoyageValidation
	{
		public VoyageRoadValidation(JobVoyage voyage) : base(voyage)
		{
		}

		#region JV_OH_Line

		protected override void CheckJV_OH_Line()
		{
			base.CheckJV_OH_Line();
			MandatoryValidation.WarnIfNotEntered(Voyage.JV_OH_LineInfo);
		}

		#endregion

		#region JV_VoyageFlight

		protected override void CheckJV_VoyageFlight()
		{
			base.CheckJV_VoyageFlight();
			MandatoryValidation.CheckEntered(Voyage.JV_VoyageFlightInfo, Res.GetString("5f8e55db-59af-495b-a7ec-5b9eafcdd30f", "Truck Ref"));
		}

		#endregion
	}
}
