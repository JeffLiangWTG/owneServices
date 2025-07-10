using System;
using System.Runtime.CompilerServices;

namespace ZAReferenceData.Services
{
	public class ConsoleLogger : ILogger
	{
		public void Succeed(string message, [CallerMemberName] string caller = null)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Time: {DateTime.Now} | Caller: {caller} | Message: {message}");
		}

		public void Info(string message, [CallerMemberName] string caller = null)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"Time: {DateTime.Now} | Caller: {caller} | Message: {message}");
		}

		public void Warn(string message, [CallerMemberName] string caller = null)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"Time: {DateTime.Now} | Caller: {caller} | Message: {message}");
		}

		public void Error(string message, Exception exception = null, [CallerMemberName] string caller = null)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Time: {DateTime.Now} | Caller: {caller} | Message: {message}");
			if (exception == null)
			{
				Console.WriteLine($"Stack Trace: {Environment.StackTrace}");
			}
			else
			{
				Console.WriteLine($"Stack Trace: {exception.StackTrace}");
			}
		}
	}
}
