using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.VersionReport
{
	public static class VersionReportEHubBuilder
	{
		public static void Send(VersionReport report, bool isCurrent)
		{
			var messageCreator = ObjectFactory.Get<IOutgoingSystemMessage>();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			string reportType = isCurrent
				? SystemMessageList.Descriptions.CurrentVersionReport
				: SystemMessageList.Descriptions.DeliveredVersionReport;

			using (MemoryStream stream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(stream, new UTF8Encoding()))
			{
				report.WriteXMLHeader(writer);
				report.WriteXMLBody(writer);
				report.WriteXMLFooter(writer);
				writer.Flush();
				stream.Position = 0;

				messageCreator.CreateSecure(factory, reportType, stream);
			}
			factory.Save();
		}
	}
}
