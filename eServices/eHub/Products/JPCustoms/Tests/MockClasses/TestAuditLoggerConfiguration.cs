using System.IO;
using CargoWise.eHub.Products.JPCustoms.Common;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	public class TestAuditLoggerConfiguration: IAuditLoggerConfiguration
	{
		public TestAuditLoggerConfiguration(IDateTimeProvider dateTimeProvider, DirectoryInfo logFolder, string fileNamePattern)
		{
			DateTimeProvider = dateTimeProvider;
			this.AuditLogFolder = logFolder;
			this.FileNamePattern = fileNamePattern;
		}

		public DirectoryInfo AuditLogFolder { get; private set; }
		public string FileNamePattern { get; private set; }
		public IDateTimeProvider DateTimeProvider { get; private set; }
	}
}