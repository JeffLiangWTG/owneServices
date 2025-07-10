using System;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.DataRegistry.GUI.DeclarationLockConfigRegistryItemEditor, Enterprise.Customs.GUI")]
	sealed class DeclarationLockConfigRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<DeclarationLockConfigCollection>
	{
		protected override DeclarationLockConfigCollection CloneValue(DeclarationLockConfigCollection value)
		{
			return (DeclarationLockConfigCollection)value.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), value.Factory);
		}
	}
}
