using System.IO;

namespace CargoWise.eHub.Products.JPCustoms.Common
{
	public interface IAuditLoggerConfiguration
	{
		DirectoryInfo AuditLogFolder { get; }
		string FileNamePattern { get; }
		IDateTimeProvider DateTimeProvider { get; }
	}
}
