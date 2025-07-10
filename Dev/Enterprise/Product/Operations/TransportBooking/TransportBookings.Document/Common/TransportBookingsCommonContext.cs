using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Document
{
	public sealed class TransportBookingsCommonContext : IContext
	{
		public TransportBookingsCommonContext(BusinessObjectFactory factory)
		{
			this.Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}
		public BusinessObjectFactory Factory { get; }

		public ICodeDescriptionPairList TransportModes => transportModes ?? (transportModes = new CodeDescriptionPairList());
		ICodeDescriptionPairList transportModes;

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
					humidity.AddPair("%", (NoResString)"Percent");
				}
				return humidity;
			}
		}
		CodeDescriptionPairList humidity;
	}
}
