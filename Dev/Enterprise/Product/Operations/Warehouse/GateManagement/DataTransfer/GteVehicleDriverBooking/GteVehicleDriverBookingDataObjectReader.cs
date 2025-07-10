using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleDriverBookingDataObjectReader : UniversalItemDataObjectReaderBase<UniversalShipment, GteVehicleDriverBooking>
	{
		public GteVehicleDriverBookingDataObjectReader(Crew crew, UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			this.crew = Argument.NotNull(crew, nameof(crew));
		}

		readonly Crew crew;

		public override DataContextType DataContextType => DataContextType.GateBooking;

		#region Matching

		protected override GteVehicleDriverBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override IMatchingBusinessEntityFinder<GteVehicleDriverBooking> GetCombinedReferenceMatcher() => null;

		#endregion Matching

		#region Populate

		protected override void PopulateBusinessObject(GteVehicleDriverBooking vehicleDriverBooking)
		{
			vehicleDriverBooking.GBD_DriverLicenseNumber = crew.LicenseNumber.Value;
			vehicleDriverBooking.GBD_DriverName = crew.FullName.Value;
		}

		#endregion Populate

		protected sealed override GteVehicleDriverBooking GetExistingBusinessObject()
		{
			return null;
		}
	}
}
