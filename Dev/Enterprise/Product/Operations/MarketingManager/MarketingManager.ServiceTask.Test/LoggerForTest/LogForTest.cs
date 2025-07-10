using System;
using Enterprise.Integration;

namespace Enterprise.MarketingManager.ServiceTask.Testing
{
	public sealed class LogForTest
	{
		public LogForTest(LogType type, string message, Exception ex)
		{
			this.type = type;
			this.message = message;
			this.ex = ex;
		}

		public override bool Equals(object obj)
		{
			LogForTest rhs = obj as LogForTest;
			return rhs != null && type == rhs.type && Message == rhs.Message;
		}

		public override int GetHashCode()
		{
			return type.GetHashCode() ^ Message.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", type, Message);
		}

		readonly LogType type;
		readonly string message;
		readonly Exception ex;

		public LogType Type => type;
		public string Message => message;
		public Exception Ex => ex;
	}
}
