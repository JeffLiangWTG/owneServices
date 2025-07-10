using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class PortAuthoritySettings : RegistryBusinessObjectTemplate, IXmlSerializable
	{
		public PortAuthoritySettings(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public PortAuthoritySettings()
		{
		}

		#region Settings

		public PortAuthoritySettingCollection Settings
		{
			get
			{
				if (settings == null)
				{
					settings = NewSettingsCollection();
					ports = null;

					foreach (PortAuthorityPort port in Ports)
					{
						settings.AddNew().Port = port.Port;
					}

					RegisterEditableChildObject(settings);
				}
				return settings;
			}
		}
		PortAuthoritySettingCollection settings;

		PortAuthoritySettingCollection NewSettingsCollection()
		{
			return new PortAuthoritySettingCollection();
		}

		void ClearSettingsCollection()
		{
			ports = null;
			if (settings == null)
			{
				settings = NewSettingsCollection();
				RegisterEditableChildObject(settings);
			}
			else
			{
				settings.RemoveAll();
			}
		}

		#endregion

		#region Read / Write Xml

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ClearSettingsCollection();
			ports = null;

			Dictionary<string, PortAuthoritySetting> portAuthoritySettings = new Dictionary<string, PortAuthoritySetting>();
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(PortAuthoritySetting));

			while (reader.Reader.NodeType == XmlNodeType.Element && reader.Reader.Name == "PortAuthoritySetting")
			{
				PortAuthoritySetting setting = (PortAuthoritySetting)serialiser.Deserialize(reader);
				portAuthoritySettings.Add(setting.Port + setting.PrincipalPK.ToString(), setting);
			}

			Settings.AddRange(portAuthoritySettings.Values);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			ports = null;

			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(PortAuthoritySetting));

			foreach (PortAuthoritySetting setting in Settings)
			{
				serialiser.Serialize(writer, setting);
			}
		}

		#endregion

		#region Implementation

		#region Ports

		PortAuthorityPortCollection Ports
		{
			get
			{
				if (ports == null)
				{
					var retriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.PortAuthorityPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
					ports = (PortAuthorityPortCollection)retriever.GetCurrentValue().Value;
				}

				return ports;
			}
		}
		PortAuthorityPortCollection ports;

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortAuthoritySettings();
		}

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			var newSettings = (PortAuthoritySettings)clone;
			base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);

			var newPortsSettings = settings == null ? null : (PortAuthoritySettingCollection)settings.Clone(
				newSettings.CurrentFallbackLevel, newSettings.CurrentFactory);

			if (newPortsSettings != null)
			{
				newSettings.settings = newPortsSettings;
				newSettings.RegisterEditableChildObject(newSettings.settings);
			}
		}

		#endregion

		#region IXmlSerializable Members

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				reader.Read();
				ReadElements(reader); // NodeType == None, Name == ""
			}
			else
			{
				reader.Read();
				ReadElements(reader);
				reader.ReadEndElement();
			}
		}

		#endregion
	}
}



