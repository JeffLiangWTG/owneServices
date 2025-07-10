using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class OrgSecurityProfileSettingCollection : RegistryBusinessObjectCollectionTemplate<OrgSecurityProfileSetting>
	{
		public OrgSecurityProfileSettingCollection()
			: base(null, null)
		{
		}

		public void OnDeserialized()
		{
			var defaults = DefaultValues;

			if (defaults.Any())
			{
				var securityNamesByKey = defaults.GroupBy(x => x.SecurityKey).ToDictionary(x => x.Key, y => y.First().SecurityName);

				foreach (var setting in this.OfType<OrgSecurityProfileSetting>().ToArray())
				{
					if (securityNamesByKey.TryGetValue(setting.SecurityKey, out var securityName))
					{
						setting.SecurityItemNameForDisplay = securityName;
					}
					else
					{
						RemoveAndDelete(setting);
					}
				}

				var settingsByKey = new HashSet<ZString>(this.OfType<OrgSecurityProfileSetting>().Select(x => x.SecurityKey).Distinct());

				foreach (var @default in defaults)
				{
					if (!settingsByKey.Contains(@default.SecurityKey))
					{
						var newSetting = AddNew();
						newSetting.SecurityKey = @default.SecurityKey;
						newSetting.SecurityItemNameForDisplay = @default.SecurityName;
						newSetting.Granted = false;
					}
				}
			}
			else
			{
				RemoveAndDeleteAll();
			}
		}

		public void PopulateDefaultSettings()
		{
			RemoveAndDeleteAll();

			foreach (var @default in DefaultValues)
			{
				var setting = AddNew();
				setting.SecurityKey = @default.SecurityKey;
				setting.SecurityItemNameForDisplay = @default.SecurityName;
				setting.Granted = @default.Granted;
			}
		}

		internal static void ResetDefaultValues()
		{
			defaultValues = null;
		}

		static IEnumerable<(ZString SecurityKey, ZString SecurityName, ZBool Granted)> DefaultValues
		{
			get
			{
				if (defaultValues == null)
				{
					defaultValues = AllWebSecurityRights.New(new BusinessObjectFactory() { RefreshEnabled = false }).OrderBy(x => x.Description)
						.Select(x => ((!x.SecurityGuid.IsEmpty ? new ZString(x.SecurityGuid.ToString()) : x.SecurityItemName),
										(ZString)x.Description,
										(ZBool)x.IsGrantedByDefault)).ToArray();
				}
				return defaultValues;
			}
		}

		[ThreadStatic]
		static IEnumerable<(ZString SecurityKey, ZString SecurityName, ZBool Granted)> defaultValues;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgSecurityProfileSettingCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new OrgSecurityProfileSetting();

		#endregion Implementation
	}
}
