using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortAuthoritySettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PortAuthoritySettingCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public PortAuthoritySettingCollection() : base()
		{
		}

		public new PortAuthoritySetting this[int index]
		{
			get { return (PortAuthoritySetting)Elements[index]; }
		}

		public new PortAuthoritySetting AddNew()
		{
			return (PortAuthoritySetting)base.AddNew();
		}

		public PortAuthoritySetting FindPortSetting(ZString port)
		{
			foreach (PortAuthoritySetting setting in this)
			{
				if (setting.Port == port && setting.Status != PortAuthoritySettingStatus.Codes.Disabled)
				{
					return setting;
				}
			}

			return null;
		}

		public PortAuthoritySetting FindPortSettingWithPrincipal(ZString port, ZGuid principalPK)
		{
			PortAuthoritySetting defaultMatch = null;
			foreach (PortAuthoritySetting setting in this)
			{
				if (setting.Port == port && setting.Status != PortAuthoritySettingStatus.Codes.Disabled)
				{
					if (setting.PrincipalPK == principalPK)
					{
						return setting;
					}
					else if (setting.PrincipalPK.IsEmpty)
					{
						defaultMatch = setting;
					}
				}
			}

			return defaultMatch;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortAuthoritySettingCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PortAuthoritySetting(CurrentFallbackLevel);
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		#endregion
	}
}


