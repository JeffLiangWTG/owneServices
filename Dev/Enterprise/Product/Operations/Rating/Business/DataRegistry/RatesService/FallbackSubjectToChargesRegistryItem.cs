using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class FallbackSubjectToChargesRegistryItem : StronglyTypedRegistryItem<FallbackSubjectToChargesCollection>
	{
		public FallbackSubjectToChargesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new FallbackSubjectToChargesRegistryDataType(), storage))
		{
		}

		public bool GetIsFallbackAllowed(string ratesProviderCode, string transportMode, string containerMode)
		{
			var foundFallbackValue =
				Value
					.Cast<FallbackSubjectToCharges>()
					.FirstOrDefault(v =>
							v.RatesProviderCode == ratesProviderCode &&
							v.TransportMode == transportMode &&
							v.ContainerMode == containerMode
					);

			return foundFallbackValue?.IsFallbackEnabled ?? false;
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.FallbackSubjectToChargesRegistryItemEditor, Enterprise.Rating.GUI")]
	class FallbackSubjectToChargesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FallbackSubjectToChargesCollection>
	{
		public FallbackSubjectToChargesRegistryDataType()
		{
		}
	}
}

