using System;

namespace CargoWise.RefDbRepo.CNReferenceData.Services
{
	public interface ILog : IDisposable
	{
		string ProgramName { get; set; }

		void Debug(object message);
		void Info(object message);
		void Warning(object message);
		void Error(object message);
		void Error(string message, Exception ex);

		string Errors { get; }
		string All { get; }
	}
}
