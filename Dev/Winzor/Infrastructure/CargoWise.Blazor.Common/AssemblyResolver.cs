using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Blazor.Common
{
	/// <summary>
	/// Use this class at the start (main method equivalent) of any of the assemblies
	/// that need to reference the CW libs
	/// </summary>
	public static class AssemblyResolver
	{
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static bool isHandlerRegistered;
		static readonly object setupLock = new();

		[SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "The variable is write-once protected by a lock(setupLock), then only used for reads")]
		internal static List<string> AssemblySearchDirectories;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string WinzorPath = "winzor";
		internal static readonly ReadOnlyCollection<string> AssemblyResolvingPaths = new(new List<string>() {
			"SessionBroker",
			"CargoWise.Blazor.SessionBroker.Test-bin",
			"AppServer",
			WinzorPath
		});

		/// <summary>
		/// Setup the resolver to work on this assembly (Common) location
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Baseline")]
		public static void Setup()
		{
			lock (setupLock)
			{
				if (!isHandlerRegistered)
				{
					var runningPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

					var rootBin = GetRootDirectory(runningPath);
					var winzorBin = Path.Combine(rootBin, WinzorPath);

					var searchDirectoriesCandidates = new List<string> { runningPath, winzorBin };

#if USE_NET_CORE_LIBS_IN_WINZOR
					var netCoreBin = Path.Combine(rootBin, CommonAssemblyInfo.CWNetCoreSubfolder);
					searchDirectoriesCandidates.Add(netCoreBin);
#endif

					searchDirectoriesCandidates.Add(rootBin);

#if USE_NET_CORE_LIBS_IN_WINZOR
					searchDirectoriesCandidates.Add(GetNetCoreAppDirectory());
#endif

					searchDirectoriesCandidates.Add(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Microsoft.NET\Framework\v4.0.30319"));

					AssemblySearchDirectories = new List<string>();
					foreach (var dir in searchDirectoriesCandidates)
					{
						if (dir != null && Directory.Exists(dir) && !AssemblySearchDirectories.Contains(dir))
						{
							AssemblySearchDirectories.Add(dir);
						}
					}

					AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
					AppDomain.CurrentDomain.AssemblyLoad += (_, args) => TelemetryService.StopAssemblyLoadActivity(args.LoadedAssembly.GetName().Name);

					isHandlerRegistered = true;
				}
			}
		}

		internal static string GetRootDirectory(string assemblyDirectoryPath)
		{
			var resolvingPath = AssemblyResolvingPaths
				.FirstOrDefault(p => assemblyDirectoryPath.EndsWith($"\\{p}", StringComparison.InvariantCultureIgnoreCase))
					?? throw new InvalidOperationException("The assembly directory path does not contain any of the expected paths for resolving.");

			var rootDirectoryPath = assemblyDirectoryPath.Substring(0, assemblyDirectoryPath.Length - resolvingPath.Length - 1);
			if (!Directory.Exists(Path.Combine(rootDirectoryPath, WinzorPath)))
			{
				throw new InvalidOperationException("The root directory path does not contain the expected Winzor directory.");
			}
			return rootDirectoryPath;
		}

#if USE_NET_CORE_LIBS_IN_WINZOR
		static string GetNetCoreAppDirectory()
		{
			return RuntimeEnvironment.GetRuntimeDirectory();
		}
#endif

		static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyName = new AssemblyName(args.Name);

			if (assemblyName.Name == "System.Windows.Forms")
			{
				return Assembly.Load("WinzorFramework");
			}

			var dllName = assemblyName.Name + ".dll";

			foreach (var searchDirectory in AssemblySearchDirectories)
			{
				var path = Path.Combine(searchDirectory, dllName);
				if (File.Exists(path))
				{
					TelemetryService.StartAssemblyLoadActivity(assemblyName.Name, path);
					return Assembly.LoadFrom(path);
				}
			}

			return null;
		}
	}
}
