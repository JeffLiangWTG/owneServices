using System;
using NLog;

namespace Enterprise.Recruiter.Business
{
	public interface IHRLoggerProvider : IDisposable
	{
		ILogger GetLogger();
	}
}
