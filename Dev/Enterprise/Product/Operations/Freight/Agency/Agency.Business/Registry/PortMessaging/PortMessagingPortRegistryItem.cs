using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortMessagingPortRegistryItem : StronglyTypedRegistryItem<PortMessagingPortCollection>
	{
		public PortMessagingPortRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, PortMessagingPortCollection.NewWithDefaultValues(Array.Empty<string>())) { }

		public PortMessagingPortRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, PortMessagingPortCollection defaultValue)
			: base(new PortMessagingPortRegistryItemImpl(name, category, caption, hint, new PortMessagingPortRegistryDataType(defaultValue), storage)) { }

		public PortMessagingPort FindByPortAndPrincipalWithFallback(string port, ZGuid principalPK)
		{
			var setting = FindByPortAndPrincipal(port, principalPK, GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).OfType<PortMessagingPort>());

			if (setting == null || setting.PrincipalPK.IsEmpty && !setting.Enabled)
			{
				setting = FindByPortAndPrincipal(port, principalPK, GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty).OfType<PortMessagingPort>());
			}

			return setting;
		}

		public PortMessagingPort FindByPortWithFallback(string port) => FindByPortAndPrincipal(port, ZGuid.Empty, GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).OfType<PortMessagingPort>(), true) ??
				FindByPortAndPrincipal(port, ZGuid.Empty, GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty).OfType<PortMessagingPort>(), true);

		PortMessagingPort FindByPortAndPrincipal(string port, ZGuid principalPK, IEnumerable<PortMessagingPort> settings, bool ignorePrincipal = false)
		{
			PortMessagingPort settingWithPrincipal = null;
			PortMessagingPort settingWithEmptyPrincipal = null;
			PortMessagingPort settingWithoutPrincipal = null;

			foreach (var setting in settings)
			{
				if (setting.Port.StartsWith(Environment.Env.CurrentCompany.Country.Code) && setting.Port == port)
				{
					if (ignorePrincipal)
					{
						settingWithoutPrincipal = setting;
						break;
					}
					else if (setting.PrincipalPK == principalPK)
					{
						settingWithPrincipal = setting;
						break;
					}
					else if (setting.PrincipalPK.IsEmpty)
					{
						settingWithEmptyPrincipal = setting;
					}
				}
			}

			return ignorePrincipal ? settingWithoutPrincipal : (settingWithPrincipal ?? settingWithEmptyPrincipal);
		}

		class PortMessagingPortRegistryItemImpl : RegistryItemImpl
		{
			public PortMessagingPortRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, category, caption, hint, dataType, storage)
			{
			}
		}
	}
}
