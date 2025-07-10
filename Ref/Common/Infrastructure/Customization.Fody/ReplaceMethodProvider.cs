using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.Common.Customization.Fody
{
	public class ReplaceMethodProvider
	{
		public MethodInfo GetMethodInfo(string methodName)
		{
			return typeof(ReplaceMethodProvider).GetMethod(methodName);
		}

		#region TextWriterErrorWriteMethodReplacement
		public void TextWriterErrorWriteMethodReplaceAddIn(string s)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.Write(result);
		}

		public void TextWriterErrorWriteArg0MethodReplaceAddIn(string s, object arg0)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.Write(result, arg0);
		}

		public void TextWriterErrorWriteArg0Arg1MethodReplaceAddIn(string s, object arg0, object arg1)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.Write(result, arg0, arg1);
		}

		public void TextWriterErrorWriteArg0Arg1Arg2MethodReplaceAddIn(string s, object arg0, object arg1, object arg2)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.Write(result, arg0, arg1, arg2);
		}

		public void TextWriterErrorWriteArgsMethodReplaceAddIn(string s, params object[] arg)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.Write(result, arg);
		}

		public void TextWriterErrorWriteObjectMethodReplaceAddIn(object obj)
		{
			var exception = obj as Exception;
			var result = $"{FodyFlagHelper.GetFlag(FodyFlagHelper.AppException)}:{JsonConvert.SerializeObject(exception)}";
			Console.Error.Write(result);
		}
		#endregion

		#region TextWriterErrorWriteLineMethodReplacement
		public void TextWriterErrorWriteLineMethodReplaceAddIn(string s)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.WriteLine(result);
		}

		public void TextWriterErrorWriteLineArg0MethodReplaceAddIn(string s, object arg0)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.WriteLine(result, arg0);
		}

		public void TextWriterErrorWriteLineArg0Arg1MethodReplaceAddIn(string s, object arg0, object arg1)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.WriteLine(result, arg0, arg1);
		}

		public void TextWriterErrorWriteLineArg0Arg1Arg2MethodReplaceAddIn(string s, object arg0, object arg1, object arg2)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.WriteLine(result, arg0, arg1, arg2);
		}

		public void TextWriterErrorWriteLineArgsMethodReplaceAddIn(string s, params object[] arg)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Error.WriteLine(result, arg);
		}

		public void TextWriterErrorWriteLineObjectMethodReplaceAddIn(object obj)
		{
			var exception = obj as Exception;
			var result = $"{FodyFlagHelper.GetFlag(FodyFlagHelper.AppException)}:{JsonConvert.SerializeObject(exception)}";
			Console.Error.WriteLine(result);
		}
		#endregion

		#region TextWriterOutWriteMethodReplacement
		public void TextWriterOutWriteMethodReplaceAddIn(string s)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.Write(result);
		}

		public void TextWriterOutWriteArg0MethodReplaceAddIn(string s, object arg0)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.Write(result, arg0);
		}

		public void TextWriterOutWriteArg0Arg1MethodReplaceAddIn(string s, object arg0, object arg1)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.Write(result, arg0, arg1);
		}

		public void TextWriterOutWriteArg0Arg1Arg2MethodReplaceAddIn(string s, object arg0, object arg1, object arg2)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.Write(result, arg0, arg1, arg2);
		}

		public void TextWriterOutWriteArgsMethodReplaceAddIn(string s, params object[] arg)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.Write(result, arg);
		}
		#endregion

		#region TextWriterOutWriteLineMethodReplacement
		public void TextWriterOutWriteLineMethodReplaceAddIn(string s)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.WriteLine(result);
		}

		public void TextWriterOutWriteLineArg0MethodReplaceAddIn(string s, object arg0)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.WriteLine(result, arg0);
		}

		public void TextWriterOutWriteLineArg0Arg1MethodReplaceAddIn(string s, object arg0, object arg1)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.WriteLine(result, arg0, arg1);
		}

		public void TextWriterOutWriteLineArg0Arg1Arg2MethodReplaceAddIn(string s, object arg0, object arg1, object arg2)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.WriteLine(result, arg0, arg1, arg2);
		}

		public void TextWriterOutWriteLineArgsMethodReplaceAddIn(string s, params object[] arg)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Out.WriteLine(result, arg);
		}
		#endregion

		#region ConsoleWriteMethodReplacement
		public static void ConsoleWriteMethodReplaceAddIn(string s)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Write(result);
		}

		public static void ConsoleWriteArg0MethodReplaceAddIn(string s, object arg0)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Write(result, arg0);
		}

		public static void ConsoleWriteArg0Arg1MethodReplaceAddIn(string s, object arg0, object arg1)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Write(result, arg0, arg1);
		}

		public static void ConsoleWriteArg0Arg1Arg2MethodReplaceAddIn(string s, object arg0, object arg1, object arg2)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Write(result, arg0, arg1, arg2);
		}

		public static void ConsoleWriteArgsMethodReplaceAddIn(string s, params object[] arg)
		{
			var result = s.Replace("\r\n", "\t");
			Console.Write(result, arg);
		}
		#endregion

		#region ConsoleWriteLineMethodReplacement
		public static void ConsoleWriteLineMethodReplaceAddIn(string s)
		{
			var result = s.Replace("\r\n", "\t");
			Console.WriteLine(result);
		}

		public static void ConsoleWriteLineArg0MethodReplaceAddIn(string s, object arg0)
		{
			var result = s.Replace("\r\n", "\t");
			Console.WriteLine(result, arg0);
		}

		public static void ConsoleWriteLineArg0Arg1MethodReplaceAddIn(string s, object arg0, object arg1)
		{
			var result = s.Replace("\r\n", "\t");
			Console.WriteLine(result, arg0, arg1);
		}

		public static void ConsoleWriteLineArg0Arg1Arg2MethodReplaceAddIn(string s, object arg0, object arg1, object arg2)
		{
			var result = s.Replace("\r\n", "\t");
			Console.WriteLine(result, arg0, arg1, arg2);
		}

		public static void ConsoleWriteLineArgsMethodReplaceAddIn(string s, params object[] arg)
		{
			var result = s.Replace("\r\n", "\t");
			Console.WriteLine(result, arg);
		}
		#endregion

		#region Appsettings Method Replacement

		public string NameValueCollectionGetMethodReplaceAddIn(string name)
		{
			var applicationConfigAssemblyName = "CargoWise.RefDbRepo.Staging.ApplicationConfig";
			var configProviderClassName = "ConfigProvider";
			var getValueMethodName = "GetConfigValueAndUpdateResourceFile";

			var assembly = Assembly.Load(applicationConfigAssemblyName);
			var type = assembly.GetType($"{applicationConfigAssemblyName}.{configProviderClassName}");
			var instance = Activator.CreateInstance(type);
			var methodInfo = type.GetMethod(getValueMethodName);
			return (string)methodInfo.Invoke(instance, new object[] { name });
		}

		#endregion

		#region ConfigurationBuilder Replacement

		public MethodBase RepoConfigurationBuilderCtor => typeof(RepoConfigurationBuilder).GetConstructors().FirstOrDefault();
		public MethodInfo IConfigurationBuilderBuildMethod => typeof(IConfigurationBuilder).GetMethod("Build");

		#endregion
	}
}
