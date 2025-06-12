using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.Products.NZCustoms.Common;

namespace CargoWise.eHub.Products.NZCustoms.PullService
{
	public interface IFileManager
	{
		string SaveReceivedMessage(NZCustomsReply message, string messageReference);
	}

	public class FileManager : IFileManager
	{
		IConfigurationProvider configurationProvider;

		public FileManager(IConfigurationProvider configurationProvider)
		{
			if (configurationProvider == null) throw new ArgumentNullException("IConfigurationProvider");
			this.configurationProvider = configurationProvider;
		}

		public string SaveReceivedMessage(NZCustomsReply message, string messageReference)
		{
			string fileName = String.Format("CRN_{0}_{1}_{2}", message.Reference, messageReference, NewID);
			string tempFilePath = Path.Combine(configurationProvider.FailedToDeliverMessageFolder.FullName, fileName + ".tmp");
			string outputFilePath = Path.Combine(configurationProvider.FailedToDeliverMessageFolder.FullName, fileName + ".xml");

			var writerSettings = new XmlWriterSettings { OmitXmlDeclaration = true };
			var serializer = new XmlSerializer(typeof(NZCustomsReply));
			using (var writer = XmlWriter.Create(tempFilePath, writerSettings))
			{
				var ns = new XmlSerializerNamespaces();
				ns.Add("ns0", "http://cargowise.com/ehub/products/");
				serializer.Serialize(writer, message, ns);
			}

			File.Move(tempFilePath, outputFilePath);
			return outputFilePath;
		}

		protected virtual string NewID
		{
			get
			{
				return Guid.NewGuid().ToString().Replace("-", "");
			}
		}
	}
}
