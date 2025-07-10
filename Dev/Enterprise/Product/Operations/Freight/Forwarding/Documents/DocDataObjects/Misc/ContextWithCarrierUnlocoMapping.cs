using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ContextWithCarrierUnlocoMapping : IContext
	{
		public ContextWithCarrierUnlocoMapping(BusinessObjectFactory factory, ZGuid carrierPK, bool showForeignCode = true)
		{
			this.Factory = factory;
			this.context = new CommonContext(factory);
			this.carrierPK = carrierPK;
			this.showForeignCode = showForeignCode;
		}

		public BusinessObjectFactory Factory { get; }
		readonly CommonContext context;
		readonly ZGuid carrierPK;
		readonly bool showForeignCode;

		public IRefUNLOCOCollection Unlocos => unlocos ?? (unlocos = new RefUNLOCOCollectionWithCarrierMapping(Factory, carrierPK, showForeignCode));
		IRefUNLOCOCollection unlocos;

		public ICodeDescriptionPairList TransportModes => context.TransportModes;
		public ICodeDescriptionPairList TransportTypes => context.TransportTypes;
		public IRefContainerCollection ContainerTypes => context.ContainerTypes;
		public IRefCountryCollection Countries => context.Countries;
		public IRefCurrencyCollection Currencies => context.Currencies;
		public ICodeDescriptionPairList AirVentFlow => context.AirVentFlow;
		public ICodeDescriptionPairList TemperatureUnits => context.TemperatureUnits;
		public ICodeDescriptionPairList Humidity => context.Humidity;
		public ICodeDescriptionPairList WeightUnits => context.WeightUnits;
		public ICodeDescriptionPairList VolumeUnits => context.VolumeUnits;
		public ICodeDescriptionPairList DimensionUnits => context.DimensionUnits;
		public ICodeDescriptionPairList RadioactiveUnits => context.RadioactiveUnits;
	}
}
