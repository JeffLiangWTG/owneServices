using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.Common.Customization.Fody
{
	public class InsertMethodProvider
	{
		static InsertMethodProvider()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_OnAssemblyResolve;
		}

		static Assembly CurrentDomain_OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyName = new AssemblyName(args.Name);
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			// If the assembly is not found, attempt to load it from the application base directory
			var assemblyPath = $"{baseDirectory}\\{assemblyName.Name}.dll";
			if (File.Exists(assemblyPath))
			{
				return Assembly.LoadFrom(assemblyPath);
			}
			return null;
		}

		#region HandleException Method for ConsoleApp
		public static void HandleException()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception ?? new ApplicationException("Unknown error happens.");
			var exMessage = $"{FodyFlagHelper.GetFlag(FodyFlagHelper.UnhandledException)}:{JsonConvert.SerializeObject(exception)}";
			Console.Error.WriteLine(exMessage);
		}
		#endregion
	}
}
