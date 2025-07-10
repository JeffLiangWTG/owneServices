using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Agency.Documents.DataObjects.Res;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class CommonContext : IContext
	{
		public CommonContext(BusinessObjectFactory factory)
		{
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public BusinessObjectFactory Factory { get; }

		#region TransportModes

		public ICodeDescriptionPairList TransportModes => transportModes ?? (transportModes = GetTransportModes());
		ICodeDescriptionPairList transportModes;

		ICodeDescriptionPairList GetTransportModes()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(Core.Constants.TransportModes.Air, Res.GetString("a7e11d2a-b8a7-4048-8b73-c1f683b512d8", "Air"));
			result.AddPair(Core.Constants.TransportModes.Sea, Res.GetString("531469a7-f38d-4e19-9bf4-d3fecbbcef0b", "Sea"));
			result.AddPair(Core.Constants.TransportModes.Road, Res.GetString("512966cb-de5f-4765-98d4-af33c434f25b", "Road"));
			result.AddPair(Core.Constants.TransportModes.Rail, Res.GetString("52aa6cb5-a5c5-4694-99d0-cdd3ed1ae4d4", "Rail"));
			result.AddPair(Core.Constants.TransportModes.InlandWaterwayTransport, Res.GetString("a56548db-595c-4b8a-860a-ef7afd6f2678", "Inland Waterways"));

			return result;
		}

		#endregion

		#region TransportTypes

		public ICodeDescriptionPairList TransportTypes => transportTypes ?? (transportTypes = new TransportTypes());
		ICodeDescriptionPairList transportTypes;

		#endregion

		#region ContainerTypes

		public IRefContainerCollection ContainerTypes => containerTypes ?? (containerTypes = new RefContainerCollection(Factory));
		IRefContainerCollection containerTypes;

		#endregion

		#region Unlocos

		public IRefUNLOCOCollection Unlocos => unlocos ?? (unlocos = new RefUNLOCOCollection(Factory));
		IRefUNLOCOCollection unlocos;

		#endregion

		#region Countries

		public IRefCountryCollection Countries => countries ?? (countries = new RefCountryCollection(Factory));
		IRefCountryCollection countries;

		#endregion

		#region Currencies

		public IRefCurrencyCollection Currencies => currencies ?? (currencies = new RefCurrencyCollection(Factory));
		IRefCurrencyCollection currencies;

		#endregion

		#region AirVentFlow

		public ICodeDescriptionPairList AirVentFlow => BindToLists.GetCachedLists(Factory).AirVentFlowRateUnits;

		#endregion

		#region TemperatureUnits

		public ICodeDescriptionPairList TemperatureUnits => BindToLists.GetCachedLists(Factory).TemperatureUnits;

		#endregion

		#region Humidity

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

		#endregion

		#region WeightUnits

		public ICodeDescriptionPairList WeightUnits => weightUnits ?? (weightUnits = new CodeDescriptionPairList(OLookUpEditType.Weight));
		ICodeDescriptionPairList weightUnits;

		#endregion

		#region VolumeUnits

		public ICodeDescriptionPairList VolumeUnits => volumeUnits ?? (volumeUnits = new CodeDescriptionPairList(OLookUpEditType.Volume));
		ICodeDescriptionPairList volumeUnits;

		#endregion

		#region DimensionUnits

		public ICodeDescriptionPairList DimensionUnits => BindToLists.GetCachedLists(Factory).DimensionUnits;

		#endregion

		#region RadioactiveUnits

		public ICodeDescriptionPairList RadioactiveUnits => BindToLists.GetCachedLists(Factory).RadioactiveUnits;

		#endregion
	}
}
