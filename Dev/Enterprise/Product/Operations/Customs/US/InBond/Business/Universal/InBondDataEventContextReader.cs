using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class InBondDataEventContextReader
	{
		public InBondDataEventContextReader(CusInBondHeader header)
		{
			this.header = Argument.NotNull(header, "header");
		}

		public void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			var factory = header.Factory;
			var helper = new EventContextValuesHelper(header.IsAir, values);
			helper.AddMasterBillNumberAndPortCodes(ZString.Empty, factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, header.BH_Calc_ImportLoadPortUNLOCO), factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, header.BH_Calc_PortUnladingUNLOCO));
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.VesselName, header.BH_ImportConveyanceName);
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.LloydsNumber, header.BH_LloydsNumber);
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.VoyageNumber, header.BH_VoyageNumber);
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.CarrierCode, header.BH_CarrierSCAC);
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportMode, GetTransportMode(header.BH_ImportTransportMode));
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.DeclarationReference, header.BH_JobReference);
		}

		ZString GetTransportMode(ZString transportCode)
		{
			switch (transportCode)
			{
				case InBondTransportModeCodes.Codes.AirNonContainer:
					transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Air;
					break;
				case InBondTransportModeCodes.Codes.RailNonContainer:
					transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail;
					break;
				case InBondTransportModeCodes.Codes.TruckNonContainer:
					transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Truck;
					break;
				case InBondTransportModeCodes.Codes.VesselContainer:
					transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
					break;
				case InBondTransportModeCodes.Codes.VesselNonContainer:
					transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
					break;
				case InBondTransportModeCodes.Codes.FixedTransportInstallations:
					transportCode = Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations;
					break;
			}
			return transportCode;
		}

		protected readonly CusInBondHeader header;
	}
}
