using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class VoyageRailValidation : BaseJobVoyageValidation
	{
		public VoyageRailValidation(JobVoyage voyage) : base(voyage)
		{
		}

		#region JV_RV_NKVessel

		protected override void CheckJV_RV_NKVessel()
		{
			base.CheckJV_RV_NKVessel();
			MandatoryValidation.CheckEntered(Voyage.JV_RV_NKVesselInfo);
		}

		#endregion

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
			MandatoryValidation.CheckEntered(Voyage.JV_VoyageFlightInfo);
			if (!Voyage.JV_RV_NKVessel.IsEmpty && !IsJourneyNumberUniqueToJourneyName())
			{
				Voyage.JV_VoyageFlightInfo.AddError(Res.GetString("bd43e686-13d3-4033-a87e-04b07166810b", "Journey number must be unique to a Journey name and cannot be repeated."));
			}
		}

		bool IsJourneyNumberUniqueToJourneyName()
		{
			return FindOtherVoyagesWithSameVesselVoyageCombination().Length == 0;
		}

		#endregion
	}
}
