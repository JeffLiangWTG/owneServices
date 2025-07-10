using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CommonContext : IContext
	{
		public CommonContext(BusinessObjectFactory factory)
		{
			this.Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public BusinessObjectFactory Factory { get; }

		public ICodeDescriptionPairList TransportModes => transportModes ?? (transportModes = GetTransportModes());
		ICodeDescriptionPairList transportModes;

		ICodeDescriptionPairList GetTransportModes()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.TransportModes.Air, Res.GetString("8d8f3eef-b5b3-4346-9679-430aad84ab10", "Air"));
			result.AddPair(Core.Constants.TransportModes.Sea, Res.GetString("6357eddd-7cfa-4dfb-81cc-fd2286e6c483", "Sea"));
			result.AddPair(Core.Constants.TransportModes.Road, Res.GetString("7039a471-c684-4d79-b8ac-e5235b89c4a8", "Road"));
			result.AddPair(Core.Constants.TransportModes.Rail, Res.GetString("3dfc15ba-bd48-42b4-9c1c-51dc5abbced2", "Rail"));
			result.AddPair(Core.Constants.TransportModes.InlandWaterwayTransport, Res.GetString("836207e3-29ff-4564-9f7a-f80381d4c901", "Inland Waterways"));
			return result;
		}

		public ICodeDescriptionPairList TransportTypes => transportTypes ?? (transportTypes = new TransportTypes());
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
