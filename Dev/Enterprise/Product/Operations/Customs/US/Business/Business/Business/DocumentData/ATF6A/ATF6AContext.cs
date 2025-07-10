using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business
{
	internal class ATF6AContext : IContext
	{
		public ATF6AContext(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		BusinessObjectFactory IContext.Factory => factory;

		public ICodeDescriptionPairList TransportModes => new CodeDescriptionPairList();

		public ICodeDescriptionPairList TransportTypes => new CodeDescriptionPairList();

		public IRefContainerCollection ContainerTypes => containerTypes ?? (containerTypes = new RefContainerCollection(factory));
		IRefContainerCollection containerTypes;

		public IRefUNLOCOCollection Unlocos => unlocos ?? (unlocos = new RefUNLOCOCollection(factory));
		IRefUNLOCOCollection unlocos;

		public IRefCountryCollection Countries => countries ?? (countries = new RefCountryCollection(factory));
		IRefCountryCollection countries;

		public IRefCurrencyCollection Currencies => currencies ?? (currencies = new RefCurrencyCollection(factory));
		IRefCurrencyCollection currencies;

		public ICodeDescriptionPairList AirVentFlow => new CodeDescriptionPairList();

		public ICodeDescriptionPairList TemperatureUnits => new CodeDescriptionPairList();

		public ICodeDescriptionPairList Humidity => new CodeDescriptionPairList();

		public ICodeDescriptionPairList WeightUnits => new CodeDescriptionPairList();

		public ICodeDescriptionPairList VolumeUnits => new CodeDescriptionPairList();

		public ICodeDescriptionPairList DimensionUnits => new CodeDescriptionPairList();

		public ICodeDescriptionPairList RadioactiveUnits => new CodeDescriptionPairList();
	}
}
