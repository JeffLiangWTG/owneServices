using System;
using System.Runtime.CompilerServices;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business
{
	public interface ILogger
	{
		void Succeed(string message, [CallerMemberName] string caller = null);

		void Info(string message, [CallerMemberName] string caller = null);

		void Warn(string message, [CallerMemberName] string caller = null);

		void Error(string message, Exception exception = null, [CallerMemberName] string caller = null);
	}
}
