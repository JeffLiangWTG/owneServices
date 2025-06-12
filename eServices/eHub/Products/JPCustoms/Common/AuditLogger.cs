
using System;
using System.IO;

namespace CargoWise.eHub.Products.JPCustoms.Common
{
	public interface IAuditLogger
	{
		void MessageSubmitted(string message, string audit);
		void ReplyReceived(string message);
	}

	public class AuditLogger : IAuditLogger
	{
		readonly IAuditLoggerConfiguration configuration;

		public AuditLogger(IAuditLoggerConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			this.configuration = configuration;

			CreateLogFolder();
		}

		public void MessageSubmitted(string message, string audit)
		{
			var jpCustomsMessage = JPCustomsMessage.Create(message);

			if (string.IsNullOrEmpty(jpCustomsMessage.ReporterId)) throw new ArgumentException("Invalid JP Customs message format: ReporterId is empty");
			if (string.IsNullOrEmpty(jpCustomsMessage.ProcedureCode)) throw new ArgumentException("Invalid JP Customs message format: ProcedureCode is empty");

			AddLogWithFallback(jpCustomsMessage.ReporterId, jpCustomsMessage.ProcedureCode, audit);
		}

		public void ReplyReceived(string message)
		{
			var jpCustomsMessage = JPCustomsMessage.Create(message);

			if (string.IsNullOrEmpty(jpCustomsMessage.ReporterId)) throw new ArgumentException("Invalid JP Customs message format: ReporterId is empty");
			if (string.IsNullOrEmpty(jpCustomsMessage.ProcedureCode)) throw new ArgumentException("Invalid JP Customs message format: ProcedureCode is empty");

			AddLogWithFallback(jpCustomsMessage.ReporterId, jpCustomsMessage.ProcedureCode, null);
		}

		protected string LogFilePath
		{
			get
			{
				return GetLogFilePath(DateTimeNow.ToString("yyyyMMdd"));
			}
		}

		protected string LogFilePathBackup
		{
			get
			{
				string fileNameAddon = string.Format("{0}_{1}", DateTimeNow.ToString("yyyyMMdd"),
					Guid.NewGuid().ToString().Replace("-", ""));
				return GetLogFilePath(fileNameAddon);
			}
		}

		#region Implementation

		public void AddLogWithFallback(string reporterId, string procedureCode, string clientAndGatewayIPAddress)
		{
			try
			{
				AddLog(LogFilePath, reporterId, procedureCode, clientAndGatewayIPAddress);
			}
			catch (IOException)
			{
				AddLog(LogFilePathBackup, reporterId, procedureCode, clientAndGatewayIPAddress);
			}
		}

		void AddLog(string logFilePath, string reporterId, string procedureCode, string clientAndGatewayIPAddress)
		{
			using (var writer = new StreamWriter(logFilePath, true))
			{
				string dateTimeString = configuration.DateTimeProvider.DateTimeNow.ToString("yyyyMMddHHmmss");
				if (string.IsNullOrEmpty(clientAndGatewayIPAddress))
				{
					writer.WriteLine("{0}	{1}	{2}", reporterId, procedureCode, dateTimeString);
				}
				else
				{
					var ipAddresses = clientAndGatewayIPAddress.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
					if (ipAddresses.Length != 2) throw new FormatException(string.Format("Invalid format of clientAndGatewayIPAddress('{0}'). Correct format should be: clientIPAddress;GatewayIPAddress. An example is 213.209.119.114;193.168.0.1", clientAndGatewayIPAddress));
					var gatewayIPAddress = ipAddresses[1];
					writer.WriteLine("{0}	{1}	{2}	{3}", reporterId, procedureCode, dateTimeString, gatewayIPAddress);
				}
			}
		}

		string GetLogFilePath(string fileNameAddon)
		{
			string fileName = string.Format(configuration.FileNamePattern, fileNameAddon);
			return Path.Combine(configuration.AuditLogFolder.FullName, fileName);
		}

		void CreateLogFolder()
		{
			configuration.AuditLogFolder.Create();
		}

		protected DateTime DateTimeNow
		{
			get
			{
				return configuration.DateTimeProvider.DateTimeNow;
			}
		}

		#endregion
	}
}
