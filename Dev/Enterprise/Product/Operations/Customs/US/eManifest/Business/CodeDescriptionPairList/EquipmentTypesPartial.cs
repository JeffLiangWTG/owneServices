using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.US.eManifest.Business
{
	public partial class EquipmentTypes : Integration.Customs.US.eManifest.IEquipmentTypesProvider
	{
		internal static bool IsLicensePlateRequired(BusinessObjectFactory factory, string equipmentType)
		{
			Tuple<bool, bool> result;
			GetSealsAndLicensePlatesRequiredDictionary(factory).TryGetValue(equipmentType, out result);
			return result != null && result.Item2;
		}

		static Dictionary<string, Tuple<bool, bool>> GetSealsAndLicensePlatesRequiredDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.SealsAndLicensePlatesRequiredDictionary",
				() => new Dictionary<string, Tuple<bool, bool>>
				{
					{ Codes.Container20FtSeaOpenTop,                        Tuple.Create(false, false) },
					{ Codes.Container20FtSeaClosedTop,                  Tuple.Create(false, false) },
					{ Codes.Container40FtSeaOpenTop,                        Tuple.Create(false, false) },
					{ Codes.Container40FtSeaClosedTop,                  Tuple.Create(false, false) },
					{ Codes.BeverageRackTrailer,                            Tuple.Create(true, false) },
					{ Codes.GooseneckTrailer,                           Tuple.Create(true, false) },
					{ Codes.Chassis,                                        Tuple.Create(true, false) },
					{ Codes.OtherLengthSeaContainerClosedTop,           Tuple.Create(false, false) },
					{ Codes.OtherLengthSeaContainerOpenTop,             Tuple.Create(false, false) },
					{ Codes.RefrigeratedContainer,                      Tuple.Create(false, false) },
					{ Codes.DoubleDropTrailer,                          Tuple.Create(true, false) },
					{ Codes.DropBackTrailer,                                Tuple.Create(true, false) },
					{ Codes.FlatBedTrailerWithHeadboards,               Tuple.Create(false, false) },
					{ Codes.FlatBedTrailerWithNoHeadboards,             Tuple.Create(false, false) },
					{ Codes.FlatRackTrailer,                                Tuple.Create(true, false) },
					{ Codes.FlatbedPlatformTrailer,                     Tuple.Create(true, false) },
					{ Codes.HopperTrailerCovered,                       Tuple.Create(true, false) },
					{ Codes.HorseTrailer,                               Tuple.Create(true, false) },
					{ Codes.HopperTrailerOpen,                          Tuple.Create(true, false) },
					{ Codes.HopperTrailerCoveredPneumaticDischarge,     Tuple.Create(true, false) },
					{ Codes.TankTrailerLiquidsNotHeatedNotInsulated,        Tuple.Create(true, false) },
					{ Codes.TankTrailerLiquidsHeatedNotInsulated,       Tuple.Create(true, false) },
					{ Codes.TankTrailerLiquidsNotHeatedInsulated,       Tuple.Create(true, false) },
					{ Codes.TankTrailerLiquidsHeatedInsulated,          Tuple.Create(true, false) },
					{ Codes.LivestockTrailer,                           Tuple.Create(true, true) },
					{ Codes.NoEquipment,                                    Tuple.Create(false, false) },
					{ Codes.Other,                                      Tuple.Create(false, false) },
					{ Codes.FixedRackDoubleDropTrailer,                 Tuple.Create(true, true) },
					{ Codes.GondolaClosed,                              Tuple.Create(true, true) },
					{ Codes.GondolaOpen,                                    Tuple.Create(true, true) },
					{ Codes.FixedRackSingleDropTrailer,                 Tuple.Create(true, true) },
					{ Codes.ControlledTemperature,                      Tuple.Create(true, true) },
					{ Codes.SingleDropTrailer,                          Tuple.Create(true, true) },
					{ Codes.TankTrailerGasNotHeatedNotInsulated,            Tuple.Create(true, true) },
					{ Codes.TankTrailerGasHeatedNotInsulated,           Tuple.Create(true, true) },
					{ Codes.TankTrailerGasNotHeatedInsulated,           Tuple.Create(true, true) },
					{ Codes.TankTrailerGasHeatedInsulated,              Tuple.Create(true, true) },
					{ Codes.TankTrailerChemicalsNotHeatedNotInsulated,  Tuple.Create(true, true) },
					{ Codes.TankTrailerChemicalsHeatedNotInsulated,     Tuple.Create(true, true) },
					{ Codes.TankTrailerChemicalsNotHeatedInsulated,     Tuple.Create(true, true) },
					{ Codes.TankTrailerChemicalsHeatedInsulated,            Tuple.Create(true, true) },
					{ Codes.AutoCarrierTrailer,                         Tuple.Create(true, true) },
					{ Codes.TrailerDryFreight,                          Tuple.Create(true, true) },
					{ Codes.TankTrailerFoodGradeLiquids,                    Tuple.Create(true, true) },
					{ Codes.SemiTruckTrailer,                           Tuple.Create(true, true) },
					{ Codes.ControlledTemperatureTrailer,               Tuple.Create(true, true) },
				});
		}

		#region Implementation of IEquipmentTypesProvider

		public ICodeDescriptionPairList GetEquipmentTypes()
		{
			var result = new ConveyanceTypes();
			result.AddRange(this);
			return result;
		}

		#endregion
	}
}
