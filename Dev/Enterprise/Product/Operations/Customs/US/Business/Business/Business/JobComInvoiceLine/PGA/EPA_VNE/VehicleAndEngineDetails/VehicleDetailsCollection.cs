using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class VehicleDetailsCollection : DependentCusAddInfoCollection<VehicleDetails, BusinessObject>
	{
		public VehicleDetailsCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USPGAVehicleDetails)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (VehicleDetails)child;

			if (Count > 0)
			{
				var previousLine = this[Count - 1];
				previousLine.UpdateAddInfoProperties();
				newElement.US_BuildMonth = previousLine.US_BuildMonth;
				newElement.US_BuildYear = previousLine.US_BuildYear;
				newElement.US_IdentityNumberQualifier = previousLine.US_IdentityNumberQualifier;
				newElement.US_VehicleManufacturer = previousLine.US_VehicleManufacturer;
				newElement.US_EngineBuildDate = previousLine.US_EngineBuildDate;
				newElement.US_EngineModel = previousLine.US_EngineModel;
				newElement.US_EngineManufacturer = previousLine.US_EngineManufacturer;
			}
		}
	}
}
