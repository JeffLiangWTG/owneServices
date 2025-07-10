using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class CusVehicleProvider : ICusVehicle
	{
		public CusVehicleProvider(CusVehicle vehicle)
		{
			Vehicle = Argument.NotNull(vehicle, nameof(vehicle));
			HasRegistrationNumber = !Vehicle.CVH_RegistrationNumber.IsEmpty;
		}
		CusVehicle Vehicle { get; }
		ZBool HasRegistrationNumber { get; }

		public string BrandType => HasRegistrationNumber ? CusEntryMessageConstants.VehicleInfo.BrandType : CusEntryMessageConstants.VehicleInfo.NonBrandType;
		public string BrandRegistrationNo => HasRegistrationNumber ? Vehicle.CVH_RegistrationNumber : ZString.Empty;
		public string BrandName => !HasRegistrationNumber ? Vehicle.CVH_BrandName : ZString.Empty;
		public decimal BrandAmount => Vehicle.BrandValueInTRY.RoundAmount();
		public string ReferenceNo => Vehicle.CVH_SerialNumber;
		public string ModelYear => Vehicle.CVH_ModelYear.IsEmpty ? CusEntryMessageConstants.VehicleInfo.EmptyModelYear : Vehicle.CVH_ModelYear;
		public string Model => Vehicle.CVH_ModelName;
		public string EngineVolume => Vehicle.Engine?.CEG_CapacityCC > 0 ? Vehicle.Engine.CEG_CapacityCC.ToString() : ZInt.Zero.ToString();
		public int NumberOfCylinders => Vehicle.Engine?.CEG_Cylinders > 0 ? (int)(Vehicle.Engine.CEG_Cylinders) : ZInt.Zero;
		public string Color => Vehicle.CVH_Color;
		public string EngineType => Vehicle.Engine?.CEG_EngineType;
		public string EngineNo => Vehicle.CVH_VehicleIdentificationNumber;
		public string EnginePower => Vehicle.Engine?.CEG_CapacityHP > 0 ? Vehicle.Engine.CEG_CapacityHP.ToString() : ZInt.Zero.ToString();
		public string GearShift => Vehicle.CVH_Gears.ToString();
		public string IMEINo => Vehicle.CVH_IMEINo;
	}
}
