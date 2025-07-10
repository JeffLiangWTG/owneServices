using System;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.DataRegistry.GUI.ASNRefreshOptionsConfigRegistryItemEditor, Enterprise.Customs.GUI")]
	sealed class ASNRefreshOptionsConfigRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<ASNRefreshOptionsConfigCollection>
	{
		protected override ASNRefreshOptionsConfigCollection CloneValue(ASNRefreshOptionsConfigCollection value)
		{
			return (ASNRefreshOptionsConfigCollection)value.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), value.Factory);
		}
	}
}
