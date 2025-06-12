using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace eServices.Configuration.Schemas
{
	public partial class ConfigurationMessage
	{
		static readonly XmlSerializerNamespaces XmlSerializerNamespaces 
			= new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "http://www.wisetechglobal.com/Schemas/Configuration") });

		public static Stream GetXsd()
		{
			return Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(typeof(ConfigurationMessage).Namespace + ".Configuration.xsd");
		}

		public static XmlDocument SerializeToXmlDocument(ConfigurationMessage configuration)
		{
			var sb = new StringBuilder();
			using (var wrtr = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				var serializer = new XmlSerializer(typeof(ConfigurationMessage));
				serializer.Serialize(wrtr, configuration, XmlSerializerNamespaces);
			}
			var result = new XmlDocument();
			result.LoadXml(sb.ToString());
			return result;
		}

		public static Stream SerializeToStream(ConfigurationMessage configuration)
		{
			var stream = new MemoryStream();
			using (var wrtr = XmlWriter.Create(stream, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				var serializer = new XmlSerializer(typeof(ConfigurationMessage));
				serializer.Serialize(wrtr, configuration, XmlSerializerNamespaces);
			}
			stream.Position = 0;
			return stream;
		}

		public static ConfigurationMessage DeserializeFromXmlDocument(XmlDocument xml)
		{
			using (var xmlStream = new MemoryStream())
			{
				xml.Save(xmlStream);
				xmlStream.Position = 0;

				var serializer = new XmlSerializer(typeof(ConfigurationMessage));
				return (ConfigurationMessage) serializer.Deserialize(xmlStream);
			}
		}

		public static ConfigurationMessage DeserializeFromStream(Stream stream)
		{
			var serializer = new XmlSerializer(typeof(ConfigurationMessage));
			return (ConfigurationMessage)serializer.Deserialize(stream);
		}

		public static ConfigurationMessage Clone(ConfigurationMessage configuration)
		{
			var serializer = new XmlSerializer(typeof(ConfigurationMessage));
			using (var ms = new MemoryStream())
			{
				serializer.Serialize(ms, configuration);
				ms.Position = 0;
				return (ConfigurationMessage)serializer.Deserialize(ms);
			}
		}
	}

	public partial class Group
	{
		public Group[] GroupItems
		{
			get { return this.Items == null ? null : this.Items.OfType<Group>().ToArray(); }
		}

		public Item[] ItemItems
		{
			get { return this.Items == null ? null : this.Items.OfType<Item>().ToArray(); }
		}

		public Credential[] CredentialItems
		{
			get { return this.Items == null ? null : this.Items.OfType<Credential>().ToArray(); }
		}

		public Certificate[] CertificateItems
		{
			get { return this.Items == null ? null : this.Items.OfType<Certificate>().ToArray(); }
		}

		public File[] FileItems
		{
			get { return this.Items == null ? null : this.Items.OfType<File>().ToArray(); }
		}

		public FTP[] FtpItems
		{
			get { return this.Items == null ? null : this.Items.OfType<FTP>().ToArray(); }
		}
	}
}
