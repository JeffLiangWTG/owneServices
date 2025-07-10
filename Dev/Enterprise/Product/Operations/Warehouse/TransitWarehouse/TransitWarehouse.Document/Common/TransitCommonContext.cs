using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Document
{
	sealed public class TransitCommonContext : IContext
	{
		public TransitCommonContext(BusinessObjectFactory factory)
		{
			this.Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public BusinessObjectFactory Factory { get; }

		public ICodeDescriptionPairList TransportModes => transportModes ?? (transportModes = GetTransportModes());
		ICodeDescriptionPairList transportModes;

		ICodeDescriptionPairList GetTransportModes()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.TransportModes.Air, Res.GetString("40c47356-94d3-4040-b275-ef76616d0363", "Air"));
			result.AddPair(Core.Constants.TransportModes.AirSea, Res.GetString("e1010103-c747-4e21-809f-bb1910433888", "Air Sea"));
			result.AddPair(Core.Constants.TransportModes.Sea, Res.GetString("d6bc1426-1ead-4cdb-8c2c-827364fceb64", "Sea"));
			result.AddPair(Core.Constants.TransportModes.SeaAir, Res.GetString("7a6aedb1-88a6-4c7b-8018-9592423b35a5", "Sea Air"));
			result.AddPair(Core.Constants.TransportModes.Road, Res.GetString("3cbc27e7-e86b-4ec3-b529-ec461d463d88", "Road"));
			result.AddPair(Core.Constants.TransportModes.Rail, Res.GetString("4178f5cf-d796-4488-8210-a74316a5af54", "Rail"));
			result.AddPair(Core.Constants.TransportModes.Courier, Res.GetString("32c72e58-7cbe-41d1-9922-2ca0b94bb668", "Courier"));
			return result;
		}

		public ICodeDescriptionPairList TransportTypes => transportTypes ?? (transportTypes = new CodeDescriptionPairList());
		ICodeDescriptionPairList transportTypes;

		public IRefContainerCollection ContainerTypes => containerTypes ?? (containerTypes = new RefContainerCollection(Factory));
		IRefContainerCollection containerTypes;

		public IRefUNLOCOCollection Unlocos => unlocos ?? (unlocos = new RefUNLOCOCollection(Factory));
		IRefUNLOCOCollection unlocos;

		public IRefCountryCollection Countries => countries ?? (countries = new RefCountryCollection(Factory));
		IRefCountryCollection countries;

		public IRefCurrencyCollection Currencies => currencies ?? (currencies = new RefCurrencyCollection(Factory));
		IRefCurrencyCollection currencies;

		public ICodeDescriptionPairList WeightUnits => weightUnits ?? (weightUnits = new CodeDescriptionPairList(OLookUpEditType.Weight));
		ICodeDescriptionPairList weightUnits;

		public ICodeDescriptionPairList VolumeUnits => volumeUnits ?? (volumeUnits = new CodeDescriptionPairList(OLookUpEditType.Volume));
		ICodeDescriptionPairList volumeUnits;

		public ICodeDescriptionPairList TemperatureUnits => BindToLists.GetCachedLists(Factory).TemperatureUnits;

		public ICodeDescriptionPairList AirVentFlow => BindToLists.GetCachedLists(Factory).AirVentFlowRateUnits;

		public ICodeDescriptionPairList DimensionUnits => BindToLists.GetCachedLists(Factory).DimensionUnits;

		public ICodeDescriptionPairList RadioactiveUnits => BindToLists.GetCachedLists(Factory).RadioactiveUnits;

		public ICodeDescriptionPairList Humidity
		{
			get
			{
				if (humidity == null)
				{
					humidity = new CodeDescriptionPairList();
					humidity.AddPair("%", (NoResString)"Percent");  // unit constant, currently English only
				}
				return humidity;
			}
		}
		CodeDescriptionPairList humidity;
	}
}
