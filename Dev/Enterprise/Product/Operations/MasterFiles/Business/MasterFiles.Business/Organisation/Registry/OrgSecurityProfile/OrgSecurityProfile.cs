using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class OrgSecurityProfile : RegistryBusinessObjectTemplate
	{
		public abstract class Schema
		{
			public const string Name = "Name";
			public const string Default = "Default";
			public const string Published = "Published";
		}

		[MaxLength(100)]
		public ZString Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return name; }
			set
			{
				CheckMaximumLength(NameInfo, value);
				SetNonPersistentPropertyValue(NameInfo, ref name, value);

				if (!IsValidationSuspended)
				{
					ValidateName();
				}
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(Schema.Name); }
		}

		ZString name;

		public ZBool Default
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return @default; }
			set
			{
				SetNonPersistentPropertyValue(DefaultInfo, ref @default, value);

				if (value)
				{
					Array.ForEach(ParentCollection, (x) =>
					{
						if (x.PK != this.PK)
						{
							x.Default = false;
						}
					});
				}

				if (!IsValidationSuspended)
				{
					ValidateDefault();
				}
			}
		}

		public ZPropertyInfo DefaultInfo => GetZPropertyInfo(Schema.Default);

		ZBool @default;

		public ZBool Published
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return published; }
			set
			{
				SetNonPersistentPropertyValue(PublishedInfo, ref published, value);
			}
		}

		public ZPropertyInfo PublishedInfo => GetZPropertyInfo(Schema.Published);

		ZBool published;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateName();
			ValidateDefault();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var profile = (OrgSecurityProfile)clone;
			profile.Name = Name;
			profile.Default = Default;
			profile.Published = Published;

			if (orgSecuritySettings != null)
			{
				profile.orgSecuritySettings?.RemoveAndDeleteAll();
				profile.orgSecuritySettings = (OrgSecurityProfileSettingCollection)orgSecuritySettings.Clone(orgSecuritySettings.CurrentFallbackLevel, orgSecuritySettings.Factory);
				profile.orgSecuritySettings.CurrentFallbackLevel = orgSecuritySettings.CurrentFallbackLevel;
				profile.RegisterEditableChildObject(profile.orgSecuritySettings);
			}
		}

		OrgSecurityProfile[] ParentCollection
			=> GetParentCollection(this, typeof(OrgSecurityProfileCollection))?.OfType<OrgSecurityProfile>().ToArray() ?? Array.Empty<OrgSecurityProfile>();

		void ValidateName()
		{
			NameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NameInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(NameInfo);
		}

		void ValidateDefault()
		{
			DefaultInfo.ClearAllNotifications();

			if (ParentCollection.Count(x => x.Default) != 1)
			{
				DefaultInfo.AddError(Res.GetString("84bbe1f7-9443-4c8d-bb94-76dbe9c64342", "There should be one and only one Default Profile."));
			}
		}

		public OrgSecurityProfileSettingCollection OrgSecuritySettings
		{
			get
			{
				if (orgSecuritySettings == null)
				{
					orgSecuritySettings = new OrgSecurityProfileSettingCollection();
					RegisterEditableChildObject(orgSecuritySettings);
				}
				orgSecuritySettings.CurrentFallbackLevel = CurrentFallbackLevel;
				return orgSecuritySettings;
			}
		}

		OrgSecurityProfileSettingCollection orgSecuritySettings;

		readonly ZXmlSerializer OrgSecuritySettingsSerialiser = ZXmlSerializer.New(typeof(OrgSecurityProfileSettingCollection));

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Name, Name);
			writer.WriteElementString(Schema.Default, Default.ToString());
			writer.WriteElementString(Schema.Published, Published.ToString());
			OrgSecuritySettingsSerialiser.Serialize(writer, OrgSecuritySettings);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Name = reader.ReadElementString(Schema.Name);
			Default = new ZBool(reader.ReadElementString(Schema.Default));
			Published = new ZBool(reader.ReadElementString(Schema.Published));

			orgSecuritySettings?.RemoveAndDeleteAll();
			orgSecuritySettings = (OrgSecurityProfileSettingCollection)OrgSecuritySettingsSerialiser.Deserialize(reader);
			orgSecuritySettings.OnDeserialized();
			RegisterEditableChildObject(orgSecuritySettings);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgSecurityProfile();
		}
	}
}
