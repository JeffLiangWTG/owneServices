using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class ExceptionExtensions
	{
		public static string GetUnWrappedMessage(this Exception ex)
		{
			if (ex == null)
			{
				return string.Empty;
			}

			if (ex is AggregateException aggregateEx)
			{
				return GetUnWrappedMessage(aggregateEx);
			}
			else
			{
				return $@"{ex.GetType().FullName}: {ex.Message}
{ex.InnerException.GetUnWrappedMessage()}";
			}
		}

		static string GetUnWrappedMessage(AggregateException aggregateEx)
		{
			var builder = new StringBuilder();
			builder.AppendLine(CultureInfo.InvariantCulture, $"{aggregateEx.GetType().FullName}: {aggregateEx.Message}-->");

			foreach (var childEx in aggregateEx.InnerExceptions)
			{
				builder.Append(childEx.GetUnWrappedMessage());
			}

			return builder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
		public static Exception CreateException(string exceptionJson)
		{
			Exception rootException;
			try
			{
				dynamic exceptionObj = JsonConvert.DeserializeObject(exceptionJson);
				rootException = new Exception($"[{exceptionObj.ClassName ?? "UnknownExceptionType"}] {exceptionObj.Message}");
				SetExceptionStackTrace(rootException, (string)(exceptionObj.StackTraceString ?? exceptionObj.StackTrace));
				var exception = rootException;
				while (exceptionObj.InnerException != null)
				{
					exceptionObj = exceptionObj.InnerException;
					var innerException = new Exception($"[{exceptionObj.ClassName ?? "UnknownExceptionType"}] {exceptionObj.Message}");
					SetExceptionStackTrace(innerException, (string)(exceptionObj.StackTraceString ?? exceptionObj.StackTrace));
					SetInnerException(exception, innerException);
					exception = exception.InnerException;
				}
			}
			catch
			{
				rootException = new Exception("Exception with details in the StackTrace.");
				SetExceptionStackTrace(rootException, exceptionJson);
			}
			return rootException;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public static bool TryDeserialiseException(string exceptionJson, ref Exception exception)
		{
			try
			{
				dynamic exceptionObj = JsonConvert.DeserializeObject(exceptionJson);
				var rootException = DeserialiseException(exceptionJson, exceptionObj);
				var tempException = rootException;
				while (tempException?.InnerException != null)
				{
					exceptionObj = exceptionObj.InnerException;
					exceptionJson = JsonConvert.SerializeObject(exceptionObj);
					var innerException = DeserialiseException(exceptionJson, exceptionObj);
					SetInnerException(tempException, innerException);
					tempException = tempException.InnerException;
				}
				exception = rootException;
				return true;
			}
			catch
			{
				exception = null;
				return false;
			}
		}

		static Exception DeserialiseException(string exceptionJson, dynamic exceptionObj)
		{
			var typeName = (string)exceptionObj.ClassName;
			var exceptionType = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);
			if (exceptionType == null)
			{
				var exception = JsonConvert.DeserializeObject<Exception>(exceptionJson);
				SetExceptionMessage(exception, $"[{typeName}] {exception.Message}");
				return exception;
			}
			else
			{
				return JsonConvert.DeserializeObject(exceptionJson, exceptionType) as Exception;
			}
		}

		static void SetInnerException(Exception exception, Exception innerException)
		{
			var filedInfo = typeof(Exception).GetField("_innerException", BindingFlags.NonPublic | BindingFlags.Instance);
			filedInfo.SetValue(exception, innerException);
		}

		static void SetExceptionMessage(Exception exception, string message)
		{
			var filedInfo = typeof(Exception).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
			filedInfo.SetValue(exception, message);
		}

		static void SetExceptionStackTrace(Exception exception, string stackTrace)
		{
			var filedInfo = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
			filedInfo.SetValue(exception, stackTrace);
		}
	}
}
