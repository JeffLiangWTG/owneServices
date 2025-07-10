using System;
using System.Globalization;
using System.Reflection;

namespace CargoWise.RefDbRepo.Common.TypeProvider
{
	public static class GenericMethodHelper
	{
		/// <summary>
		/// Can the given string in a certain format be converted to a datetime? If so, Result contains the DateTime resulting from the conversion
		/// </summary>
		/// <param name="value">A string containing a datetime string to convert.</param>
		/// <param name="result">Returned as the datetime equivalent of the given string, else invalid if it cannot be converted.</param>
		/// <param name="format">The format string to apply during parsing</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		public static bool TryParseExact(this string value, out DateTime result, string format)
		{
			Argument.Argument.NotNullOrEmpty(format, nameof(format));
			var success = false;
			result = DateTime.MinValue;

			if (string.IsNullOrEmpty(value))
			{
				success = true;
			}
			else
			{
				DateTime parsedResult;
				if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedResult))
				{
					result = GetParsedTimeOnlyWithAdjustedDate(parsedResult, format);
					success = true;
				}
			}
			return success;
		}

		/// <summary>
		/// DateTime.ParseExact uses the local machine date to complete the DateTime in a Time only format parsing.
		/// This date might be different from the date in the current ZDateTime time zone, Hence it has to be adjusted here.
		/// </summary>
		static DateTime GetParsedTimeOnlyWithAdjustedDate(DateTime parsedResult, string format)
		{
			Argument.Argument.NotNullOrEmpty(format, nameof(format));

			var result = parsedResult;

			if (!format.Contains("y"))
			{
				if (format.Contains("M") || (format.Contains("d") && format.Trim() != "d"))
				{
					int year = DateTime.Now.Year;
					result = result.AddYears(year - result.Year);
				}
				else if (!format.Contains("d"))
				{
					TimeSpan parsedTime = parsedResult.TimeOfDay;
					result = DateTime.Today.Add(parsedTime);
				}
			}

			return result;
		}

		public static object InvokeGenericMethod(this object obj, string methodName, Type genericType, params object[] arguments)
		{
			Argument.Argument.NotNull(obj, nameof(obj));
			Argument.Argument.NotNullOrEmpty(methodName, nameof(methodName));
			Argument.Argument.NotNull(genericType, nameof(genericType));

			return InvokeGenericMethod(obj, methodName, new[] { genericType }, arguments);
		}

		public static object InvokeGenericMethod(this object obj, string methodName, Type[] genericTypes, params object[] arguments)
		{
			Argument.Argument.NotNull(obj, nameof(obj));
			Argument.Argument.NotNullOrEmpty(methodName, nameof(methodName));
			Argument.Argument.NotNull(genericTypes, nameof(genericTypes));

			var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var generic = method.MakeGenericMethod(genericTypes);
			return generic.Invoke(obj, arguments);
		}

		public static object InvokeStaticGenericMethod(this Type type, string methodName, Type[] genericTypes, params object[] arguments)
		{
			Argument.Argument.NotNull(type, nameof(type));
			Argument.Argument.NotNullOrEmpty(methodName, nameof(methodName));
			Argument.Argument.NotNull(genericTypes, nameof(genericTypes));

			var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			var generic = method.MakeGenericMethod(genericTypes);
			return generic.Invoke(null, arguments);
		}

		public static object InvokeStaticGenericMethod(this Type type, string methodName, Type genericType, params object[] arguments)
		{
			Argument.Argument.NotNull(type, nameof(type));
			Argument.Argument.NotNullOrEmpty(methodName, nameof(methodName));
			Argument.Argument.NotNull(genericType, nameof(genericType));

			return InvokeStaticGenericMethod(type, methodName, new[] { genericType }, arguments);
		}
	}
}
