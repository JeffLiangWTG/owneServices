using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class CusVehicleEmptyProvider : ICusVehicle
	{
		public CusVehicleEmptyProvider() 
		{
		}

		public string BrandType => ZString.Empty;
		public string BrandRegistrationNo => ZString.Empty;
		public string BrandName => ZString.Empty;
		public decimal BrandAmount => ZDecimal.Zero;
		public string ReferenceNo => ZString.Empty;
		public string ModelYear => CusEntryMessageConstants.VehicleInfo.EmptyModelYear;
		public string Model => ZString.Empty;
		public string EngineVolume => ZInt.Zero.ToString();
		public int NumberOfCylinders => ZInt.Zero;
		public string Color => ZString.Empty;
		public string EngineType => ZString.Empty;
		public string EngineNo => ZString.Empty;
		public string EnginePower => ZInt.Zero.ToString();
		public string GearShift => ZString.Empty;
		public string IMEINo => ZString.Empty;
	}
}
