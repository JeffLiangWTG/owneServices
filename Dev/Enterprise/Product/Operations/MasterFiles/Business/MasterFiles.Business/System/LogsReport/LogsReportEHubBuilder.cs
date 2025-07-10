using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.LogsReport
{
	public static class LogsReportEHubBuilder
	{
		public static void Send(LogsReport report)
		{
			var messageCreator = ObjectFactory.Get<IOutgoingSystemMessage>();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			string reportType = SystemMessageList.Descriptions.LogsReport;
			using (MemoryStream stream = new MemoryStream())
			using (StreamWriter writer = new StreamWriter(stream))
			{
				writer.Write(report.XML);
				writer.Flush();
				stream.Position = 0;
				messageCreator.CreateSecure(factory, reportType, stream);
			}
			factory.Save();
		}
	}
}
