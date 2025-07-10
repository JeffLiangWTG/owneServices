using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IEmailProcessorLogger
	{
		void Log(LogType logType, bool verboseModeOnly, string format, params object[] args);
	}
}
