using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Warehouse.Environment.CodeLists.US;

namespace Enterprise.Warehouse.Environment.Business.US
{
	public class WhsLocationValidation : WhsLocationViewValidation
	{
		public WhsLocationValidation(WhsLocation parent)
			: base(parent)
		{
		}

		protected override void CheckWLV_ApprovedKnownLocation()
		{
			base.CheckWLV_ApprovedKnownLocation();

			MandatoryValidation.CheckEntered(Parent.WLV_ApprovedKnownLocationInfo, ResString.GetMultilingualString("29225bb1-6fe9-4c81-b3ca-216d39a651be", "TSA Status") as IMultilingualString);
			ListValidation.ErrorIfInvalidCode(Parent.WLV_ApprovedKnownLocationInfo, new TSAStatus(), ResString.GetMultilingualString("29225bb1-6fe9-4c81-b3ca-216d39a651be", "TSA Status"));

			CheckWLV_ApprovedKnownLocation_CannotChangeIfStockExists();
		}

		void CheckWLV_ApprovedKnownLocation_CannotChangeIfStockExists()
		{
			var location = Parent;
			if (!location.WLV_ApprovedKnownLocationInfo.HasErrors()
				&& !location.WLV_ApprovedKnownLocationInfo.OriginalValue.Equals(location.WLV_ApprovedKnownLocation)
				&& !location.IsEmpty)
			{
				location.WLV_ApprovedKnownLocationInfo.AddError(Res.GetString("c0c1db6c-fc51-4912-aa82-7b8f3a146e99", "Cannot change TSA status as stock exists in this location."));
			}
		}
	}
}
