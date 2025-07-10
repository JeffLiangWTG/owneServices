using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class SameChargeCodeDifferentProviderRegistryItem : StronglyTypedRegistryItem<SameChargeCodeDifferentProviderCollection>
	{
		public SameChargeCodeDifferentProviderRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new SameChargeCodeDifferentProviderRegistryDataType(), storage))
		{
		}

		public bool GetIsEnabled(string jobType, string transportMode, string direction)
		{
			var foundFallbackValue =
				Value
					.Cast<SameChargeCodeDifferentProvider>()
					.FirstOrDefault(v =>
						(v.JobType == jobType || v.JobType == "ALL") &&
						(v.TransportMode == transportMode || v.TransportMode == Core.Constants.TransportModes.All) &&
						(v.Direction == direction || v.Direction == Core.Constants.FreightShipmentDirection.Code.All)
					);

			return foundFallbackValue?.IsEnabled ?? false;
		}

		public bool GetIsEnabled(string jobType, string transportMode, Directions direction)
		{
			string directionCode;

			switch (direction)
			{
				case Directions.Import:
					directionCode = Core.Constants.FreightShipmentDirection.Code.Import;
					break;
				case Directions.Export:
					directionCode = Core.Constants.FreightShipmentDirection.Code.Export;
					break;
				case Directions.Domestic:
					directionCode = Core.Constants.FreightShipmentDirection.Code.Domestic;
					break;
				default:
					directionCode = Core.Constants.FreightShipmentDirection.Code.All;
					break;
			}

			return GetIsEnabled(jobType, transportMode, directionCode);
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.SameChargeCodeDifferentProviderRegistryItemEditor, Enterprise.Rating.GUI")]
	class SameChargeCodeDifferentProviderRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SameChargeCodeDifferentProviderCollection>
	{
		public SameChargeCodeDifferentProviderRegistryDataType()
		{
		}
	}
}
