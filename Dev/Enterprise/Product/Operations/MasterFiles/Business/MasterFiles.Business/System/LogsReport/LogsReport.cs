using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.LogsReport
{
	public class LogsReport
	{
		public LogsReport(ZString incidentNumber, ZString serviceTaskCode, byte[] logFilesZip)
		{
			this.IncidentNumber = incidentNumber;
			this.ServiceTaskCode = serviceTaskCode;
			this.LogFilesZip = logFilesZip;
		}

		public LogsReport(ZString xML)
		{
			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xML)))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				reader.ReadToFollowing("IncidentNumber");
				IncidentNumber = reader.ReadElementContentAsString("IncidentNumber", "");
				reader.ReadToFollowing("ServiceTaskCode");
				ServiceTaskCode = reader.ReadElementContentAsString("ServiceTaskCode", "");
				reader.ReadToFollowing("LogFilesZip");
				LogFilesZip = Convert.FromBase64String(reader.ReadElementContentAsString("LogFilesZip", ""));
			}
		}

		public ZString XML
		{
			get
			{
				using (MemoryStream stream = new MemoryStream())
				using (XmlTextWriter writer = new XmlTextWriter(stream, new UTF8Encoding()))
				{
					writer.Formatting = Formatting.Indented;
					writer.WriteStartDocument();
					writer.WriteStartElement("LogsReport");
					writer.WriteElementString("IncidentNumber", IncidentNumber.ToString().ToUpper());
					writer.WriteElementString("ServiceTaskCode", ServiceTaskCode.ToString().ToUpper());
					writer.WriteElementString("LogFilesZip", Convert.ToBase64String(LogFilesZip));
					writer.WriteEndElement();
					writer.WriteEndDocument();
					writer.Flush();
					stream.Position = 0;
					return Encoding.UTF8.GetString(stream.ToArray());
				}
			}
		}

		public ZString IncidentNumber { get; set; }
		public ZString ServiceTaskCode { get; set; }
		public byte[] LogFilesZip { get; set; }
	}
}
