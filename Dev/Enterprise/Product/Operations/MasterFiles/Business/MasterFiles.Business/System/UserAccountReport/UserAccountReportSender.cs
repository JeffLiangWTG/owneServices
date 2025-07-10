using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.UserAccountReport
{
	public interface IUserAccountReportSender
	{
		UserAccountReport SendReport(DateTime lastRunTime);
	}

	public class UserAccountReportSender : IUserAccountReportSender
	{
		public UserAccountReport SendReport(DateTime lastRunTime)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var report = new UserAccountReport(registrationKey, lastRunTime);
			if (!report.StaffList.IsNullOrEmpty())
			{
				Send(report);
			}
			return report;
		}

		public virtual void Send(UserAccountReport report)
		{
			var messageCreator = ObjectFactory.Get<IOutgoingSystemMessage>();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			string reportType = SystemMessageList.Descriptions.UserAccountReport;
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
