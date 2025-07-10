using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS
{
	class VehicleAndEngineDataWrapper : IDISVehicleAndEngineData
	{
		public VehicleAndEngineDataWrapper(VehicleDetails source)
		{
			this.source = source;
		}

		readonly VehicleDetails source;

		public ZInt VNELineNumber
		{
			get { return source.Vehicle.US_LineNo; }
		}

		public ZDateTime EngineManufactureDate
		{
			get { return source.US_EngineBuildDate; }
		}

		public ZString EngineManufacturer
		{
			get { return source.US_EngineManufacturer; }
		}

		public ZString EngineModel
		{
			get { return source.US_EngineModel; }
		}

		public ZString EngineSerialNumber
		{
			get { return source.US_EngineNumber; }
		}

		public ZString ManufactureMonth
		{
			get { return source.US_BuildMonth; }
		}

		public ZString ManufactureYear
		{
			get { return source.US_BuildYear; }
		}

		public ZString Manufacturer
		{
			get { return source.US_EngineNumber; }
		}

		public ZString Model
		{
			get { return source.Vehicle.US_VehicleModel; }
		}

		public ZString SerialNumber
		{
			get { return source.US_IdentityNumberQualifier == ItemIdentityNumberQualifierList.Codes.SerialNumber ? source.US_IdentityNumber : ZString.Empty; }
		}

		public ZString VIN
		{
			get { return source.US_IdentityNumberQualifier == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN ? source.US_IdentityNumber : ZString.Empty; }
		}
	}
}
