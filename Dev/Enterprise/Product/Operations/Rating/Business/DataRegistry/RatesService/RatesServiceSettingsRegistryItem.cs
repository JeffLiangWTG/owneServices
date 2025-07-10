using System;
using System.Linq;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class RatesServiceSettingsRegistryItem : StronglyTypedRegistryItem<RatesServiceRegistrySettingsCollection>
	{
		public RatesServiceSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RatesServiceSettingsRegistryItemRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

		public bool IsEnabled(string transportMode, string containerMode)
		{
			var foundEnableSubscriptionValue =
				Value
					.Cast<RatesServiceRegistrySettings>()
					.FirstOrDefault(v => v.TransportMode == transportMode && v.ContainerMode == containerMode);

			return foundEnableSubscriptionValue?.IsSubscriptionEnabled ?? false;
		}

		public bool IsEnabled(string transportMode)
		{
			return Value
				.Cast<RatesServiceRegistrySettings>()
				.Any(v => v.TransportMode == transportMode && v.IsSubscriptionEnabled);
		}

		public bool IsAllowed()
		{
			return Value.Cast<RatesServiceRegistrySettings>().Any(v => v.IsSubscriptionEnabled);
		}

		public class RatesServiceSettingsRegistryItemRegistryItemImpl : RegistryItemImpl
		{
			public RatesServiceSettingsRegistryItemRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new RatesServiceSettingsRegistryDataType(), storage)
			{
				this.name = name;
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				bool isSubscriptionEnabled = name != nameof(DataRegistryRating.Instance.RatesServiceRateSelector);

				var registryValue = new RatesServiceRegistrySettingsCollection
				{
					new RatesServiceRegistrySettings
					{
						TransportMode = Constants.TransportModes.Sea,
						ContainerMode = Constants.ContainerModes.FCL,
						IsSubscriptionEnabled = isSubscriptionEnabled
					},
					new RatesServiceRegistrySettings
					{
						TransportMode = Constants.TransportModes.Air,
						ContainerMode = Constants.ContainerModes.Loose,
						IsSubscriptionEnabled = isSubscriptionEnabled
					}
				};

				return registryValue;
			}

			readonly string name;
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.RatesServiceSettingsRegistryItemEditor, Enterprise.Rating.GUI")]
	class RatesServiceSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RatesServiceRegistrySettingsCollection>
	{
		public RatesServiceSettingsRegistryDataType()
		{
		}
	}
}
