using System;
using Common.Logging;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public interface ILogWrapper
	{
		ILog GetLog<T>();
		ILog GetLog(Type type);
		ILog GetLog(string key);
	}
}
