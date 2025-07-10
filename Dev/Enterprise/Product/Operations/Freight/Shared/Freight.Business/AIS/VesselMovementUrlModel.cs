using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public sealed class VesselMovementsUrlModel : IVesselMovementsUrlModel
	{
		public ZString LloydsNumber { get; set; }
		public ZDateTime DepartureTime { get; set; }
		public ZDateTime ArrivalTime { get; set; }
		public ZString CarrierCode { get; set; }
		public ZString VoyageNumber { get; set; }
		public ZString DeparturePortUnloco { get; set; }
		public ZString ArrivalPortUnloco { get; set; }

		public override bool Equals(object obj)
		{
			var model = (VesselMovementsUrlModel)obj;

			return model != null &&
				LloydsNumber == model.LloydsNumber &&
				DepartureTime == model.DepartureTime &&
				ArrivalTime == model.ArrivalTime &&
				CarrierCode == model.CarrierCode &&
				VoyageNumber == model.VoyageNumber &&
				DeparturePortUnloco == model.DeparturePortUnloco &&
				ArrivalPortUnloco == model.ArrivalPortUnloco;
		}

		public bool IsValidForPortMatching()
		{
			var lloydsNumberValidation = new LloydsNumberValidation();
			lloydsNumberValidation.Validate(LloydsNumber);

			return !CarrierCode.IsEmpty
				&& !LloydsNumber.IsEmpty
				&& !DepartureTime.IsEmpty
				&& DepartureTime.IsValid
				&& !ArrivalTime.IsEmpty
				&& ArrivalTime.IsValid
				&& !DeparturePortUnloco.IsEmpty
				&& !ArrivalPortUnloco.IsEmpty
				&& lloydsNumberValidation.IsValid;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;

				hash = hash * 23 + LloydsNumber.GetHashCode();
				hash = hash * 23 + DepartureTime.GetHashCode();
				hash = hash * 23 + ArrivalTime.GetHashCode();
				hash = hash * 23 + CarrierCode.GetHashCode();
				hash = hash * 23 + VoyageNumber.GetHashCode();
				hash = hash * 23 + DeparturePortUnloco.GetHashCode();
				hash = hash * 23 + ArrivalPortUnloco.GetHashCode();

				return hash;
			}
		}
	}
}
