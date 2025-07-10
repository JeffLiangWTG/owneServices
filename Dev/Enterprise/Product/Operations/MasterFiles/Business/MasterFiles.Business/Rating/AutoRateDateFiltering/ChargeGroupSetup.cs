using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public abstract class ChargeGroupSetup<T> : RegistryBusinessObjectTemplate
		where T : RegistryBusinessObjectCollectionTemplate, new()
	{
		#region Schema

		public abstract class Schema
		{
			public const string ChargeGroup = "ChargeGroup";
			public const string ChargeGroupDescription = "ChargeGroupDescription";
		}

		#endregion

		public ChargeGroupSetup()
			: base()
		{
		}

		public ChargeGroupSetup(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public ChargeGroupSetup(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var chargeGroupSetupClone = (ChargeGroupSetup<T>)clone;
			if (chargeGroupSettings != null)
			{
				chargeGroupSetupClone.chargeGroupSettings = (T)chargeGroupSettings.Clone(chargeGroupSetupClone.CurrentFallbackLevel, chargeGroupSetupClone.Factory);
				chargeGroupSetupClone.chargeGroupSettings.CurrentFallbackLevel = chargeGroupSettings.CurrentFallbackLevel;
				chargeGroupSetupClone.RegisterEditableChildObject(chargeGroupSetupClone.chargeGroupSettings);
			}
		}

		#region Bound Properties

		#region Charge Group

		ZString fChargeGroup;

		[MaxLength(3)]
		public ZString ChargeGroup
		{
			get { return fChargeGroup; }
			set
			{
				SetNonPersistentPropertyValue(ChargeGroupInfo, ref fChargeGroup, value);
				ChargeGroupSettings.RefreshBinding();
			}
		}

		public ZPropertyInfo ChargeGroupInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeGroup); }
		}

		#endregion

		#region Charge Group Description

		MultilingualString chargeGroupDescription;

		[MaxLength(256)]
		public MultilingualString ChargeGroupDescription
		{
			get { return chargeGroupDescription ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				SetNonPersistentPropertyValue(ChargeGroupDescriptionInfo, ref chargeGroupDescription, value, false);
			}
		}

		public ZPropertyInfo ChargeGroupDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeGroupDescription); }
		}

		#endregion

		#region ChargeGroupSettings

		public T ChargeGroupSettings
		{
			get
			{
				if (chargeGroupSettings == null)
				{
					chargeGroupSettings = new T();
					RegisterEditableChildObject(chargeGroupSettings);
				}
				chargeGroupSettings.CurrentFallbackLevel = CurrentFallbackLevel;
				return chargeGroupSettings;
			}
		}

		T chargeGroupSettings;

		ZXmlSerializer ChargeGroupSettingsSerialiser
		{
			get
			{
				return chargeGroupSettingsSerialiser ?? (chargeGroupSettingsSerialiser = ZXmlSerializer.New(typeof(T)));
			}
		}

		ZXmlSerializer chargeGroupSettingsSerialiser;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ChargeGroup, ChargeGroup);
			writer.WriteElementString(Schema.ChargeGroupDescription, ChargeGroupDescription);
			ChargeGroupSettingsSerialiser.Serialize(writer, ChargeGroupSettings);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ChargeGroup = reader.ReadElementString(Schema.ChargeGroup);
			ChargeGroupDescription = (NoResString)reader.ReadElementString(Schema.ChargeGroupDescription);
			chargeGroupSettings = (T)ChargeGroupSettingsSerialiser.Deserialize(reader);
			RegisterEditableChildObject(chargeGroupSettings);
		}

		#endregion
	}
}
