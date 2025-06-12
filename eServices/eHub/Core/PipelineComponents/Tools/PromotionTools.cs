using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	internal static class PromotionTools
	{
		internal static ParameterType GetParameterType(string input)
		{
			if (MethodCallFormatter.Verify(input))
				return ParameterType.MethodCall;
			else if (NameNsFormatter.Verify(input))
				return ParameterType.ContextProperty;
			else
				return ParameterType.PlainText;
		}

		internal static object ReadPropertyValue(IBaseMessage message, string nameNSFormat)
		{
			try
			{
				NameNsFormatter property = new NameNsFormatter(nameNSFormat);
				return message.Context.Read(property.Name, property.Namespace);
			}
			catch (Exception ex)
			{
				throw new FormatException(string.Format("Error when reading property value: {0}\r{1}", nameNSFormat.ToString(), ex.Message));
			}
		}

		internal static object CallMethod(IBaseMessage message, string methodCallformat)
		{
			MethodCallFormatter methodCallSpec = new MethodCallFormatter(methodCallformat);
			return CallMethod(message, methodCallSpec);
		}

		private static object CallMethod(IBaseMessage message, MethodCallFormatter methodCallSpec)
		{
			try
			{
				var parameterValues = new string[] { };
				switch (methodCallSpec.MethodName)
				{
					case "substring":
						parameterValues = GetParameterValues(message, methodCallSpec, 3);
						return parameterValues[0].Substring(int.Parse(parameterValues[1]), int.Parse(parameterValues[2]));

					case "SubstringFrom":
						parameterValues = GetParameterValues(message, methodCallSpec, 2);
						int index = parameterValues[0].IndexOf(parameterValues[1]);
						if (index < 0)
							index = 0;
						return parameterValues[0].Substring(index);

					case "SelectSingleNode":
						parameterValues = GetParameterValues(message, methodCallSpec, 1);
						var stream = message.BodyPart.GetOriginalDataStream();
						var value = new System.IO.StreamReader(stream).ReadToEnd();
						stream.Seek(0, System.IO.SeekOrigin.Begin);
						var xmlDoc = new System.Xml.XmlDocument();
						xmlDoc.LoadXml(value);
						return (xmlDoc.SelectSingleNode(parameterValues[0]).InnerText);

					case "GetCounterValue":
					case "ReadCounterValue":
						string outputParam = methodCallSpec.MethodName == "GetCounterValue" ? "@value" : null;
						parameterValues = GetParameterValues(message, methodCallSpec, 2);
						return new TransformAccessor().CallActionProcedure(methodCallSpec.MethodName, outputParam, "@name", parameterValues[0], "@padlength", parameterValues[1]);

					case "DateTimeNow":
						parameterValues = FindParameterValue(message, methodCallSpec.MethodParameters);
						return DateTime.Now.ToString(parameterValues[0]);

					default:
						throw new FormatException(string.Format("Unrecognized method name to call:({0})", methodCallSpec.MethodName));
				}
			}
			catch (Exception ex)
			{
				throw new FormatException("Error when calling method: " + methodCallSpec.ToString() + "\r" + ex.Message);
			}
		}

		private static string[] GetParameterValues(IBaseMessage message, MethodCallFormatter methodCallSpec, int expectedCount)
		{
			string[] parameterValues = FindParameterValue(message, methodCallSpec.MethodParameters);
			if (methodCallSpec.MethodParameters.Length != expectedCount)
				throw new FormatException(string.Format("Method call is not correct - Expected number of parameters {1} : ({0})", methodCallSpec.ToString(), expectedCount));

			return parameterValues;
		}

		private static string[] FindParameterValue(IBaseMessage message, string[] parameters)
		{
			try
			{
				var result = new List<string>();
				foreach (var parameter in parameters)
				{
					switch (GetParameterType(parameter))
					{
						case ParameterType.PlainText:
							result.Add(parameter);
							break;
						case ParameterType.ContextProperty:
							result.Add(ReadPropertyValue(message, parameter).ToString());
							break;
						case ParameterType.MethodCall:
							result.Add(CallMethod(message, parameter).ToString());
							break;
						default:
							throw new FormatException(string.Format("Unrecognized parameter:({0})", parameter));
					}
				}
				return result.ToArray();
			}
			catch
			{
				throw new FormatException("Error when finding the value of parameter: " + string.Join("@", parameters));
			}
		}
	}

	/// <summary>
	///	Define parameter type, either a context property read, a method call return value or a plain text.
	/// </summary>
	internal enum ParameterType
	{
		PlainText = 0,
		ContextProperty = 1,
		MethodCall = 2
	}

	/// <summary>
	/// Used to format and retrieve message and context property names and namespaces.  The recognised format is namespace#name.
	/// </summary>
	internal struct NameNsFormatter
	{
		private const string OUTPUTFORMAT = "{0}#{1}";
		private const string INPUTFORMAT = "^([^#]+)#([^#]+$)";

		internal static bool Verify(string input)
		{
			if (Regex.IsMatch(input, INPUTFORMAT))
				return true;
			else
				return false;
		}

		public NameNsFormatter(string formattedName)
		{
			var match = Regex.Match(formattedName, INPUTFORMAT);

			if (!match.Success)
				throw new FormatException(String.Format("Unexpected name namespace format ({0}).  Expected format is {1}", formattedName, INPUTFORMAT));

			Namespace = match.Groups[1].Value;
			Name = match.Groups[2].Value;
		}

		public readonly string Name;
		public readonly string Namespace;

		public override string ToString()
		{
			return String.Format(OUTPUTFORMAT, this.Namespace, this.Name);
		}
	}

	/// <summary>
	/// Used to format and retrieve value from a method call.  The recognised format is MethodCall:methodname@param1@param2@....
	/// Supported Methods: MethodCall:substring@inputstring@startindex@length
	/// Supported Methods: MethodCall:ReadCounter@name@padlength
	/// Parameters can be plain string, Namespace for context property and another method call;
	/// </summary>
	internal struct MethodCallFormatter
	{
		private const char DELIMETER = '@';
		private const string INPUTFORMAT = "^MethodCall:([^@]+)(@[^@]+)+$";

		internal static bool Verify(string input)
		{
			if (Regex.IsMatch(input, INPUTFORMAT))
				return true;
			else
				return false;
		}

		public MethodCallFormatter(string input)
		{
			var match = Regex.Match(input, INPUTFORMAT);

			if (!match.Success)
				throw new FormatException(String.Format("Unexpected method call format ({0}).  Expected format is {1}", input, INPUTFORMAT));

			this.MethodName = match.Groups[1].Value;
			var methodParameterList = new List<string>();
			foreach (var methodParameter in match.Groups[2].Captures)
			{
				methodParameterList.Add(methodParameter.ToString().TrimStart(DELIMETER));
			}
			this.MethodParameters = methodParameterList.ToArray<string>();
		}

		public readonly string MethodName;
		public readonly string[] MethodParameters;

		public override string ToString()
		{
			return "MethodCall:" + MethodName + DELIMETER + string.Join(DELIMETER.ToString(), MethodParameters);
		}
	}
}
