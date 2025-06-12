using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests
{
	public class LoggerTestHelper
	{
		public static Expression<Action<ILogger>> GetLogSetupExpression<TException>(LogLevel logLevel,
			string expectedMessage, Expression<Func<TException, bool>> exceptionExpression = null)
			where TException : Exception
		{
			if (exceptionExpression == null)
			{
				return _ => _.Log(logLevel, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == expectedMessage), It.IsAny<TException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>());
			}

			return _ => _.Log(logLevel, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == expectedMessage), It.Is(exceptionExpression), It.IsAny<Func<It.IsAnyType, Exception, string>>());
		}

		public static Expression<Action<ILogger>> GetLogSetupExpression(LogLevel logLevel, string expectedMessage) =>
			_ => _.Log(logLevel, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == expectedMessage), null, It.IsAny<Func<It.IsAnyType, Exception, string>>());

		public static Expression<Action<ILogger>> GetLogSetupExpression<TException>(LogLevel logLevel, Expression<Func<TException, bool>> exceptionExpression = null) where TException : Exception
		{
			if (exceptionExpression == null)
			{
				return _ => _.Log(logLevel, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<TException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>());
			}

			return _ => _.Log(logLevel, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.Is(exceptionExpression), It.IsAny<Func<It.IsAnyType, Exception, string>>());
		}

		public static Expression<Action<ILogger>> GetLogSetupExpression(LogLevel logLevel) =>
			_ => _.Log(logLevel, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>());
	}
}
