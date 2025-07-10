#if NETFRAMEWORK
#error This file must be included only in NetCore projects
#endif

#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable CS0436 // The type 'CommonAssemblyInfo' in '...\Dev\CommonAssemblyInfo.cs' conflicts with the imported type 'CommonAssemblyInfo' in '...'

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CargoWise;

/// <summary>
/// Use this class at the start (main method equivalent) of the assemblies that need to reference the CW libs
/// </summary>
public static class NetCoreAssemblyResolver
{
	// make a Lazy AssemblyResolve method to avoid unsafe static fields
	static readonly Lazy<ResolveEventHandler> EventHandler = new (BuildAssemblyResolver);

	/// <summary>
	/// Set up the resolver to work on this assembly (Common) location
	/// </summary>
	public static void Setup()
	{
		AppDomain.CurrentDomain.AssemblyResolve += EventHandler.Value;
	}

	static ResolveEventHandler BuildAssemblyResolver()
	{
		var directories = BuildSearchDirectories();
		var extensions = new List<string> { ".dll", ".exe" };

		return (sender, args) => AssemblyResolve(sender, args, directories, extensions);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file", Justification = "Does not appear to cause issues with some publish as single file projects (TestProcessAnyCpu).")]
	static IReadOnlyList<string> BuildSearchDirectories()
	{
		List<string> directories = new ();

		var executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NullReferenceException("Assembly location not found");
		var binDirectory = FindBinDirectory(executingLocation);

		AddSearchDirectory(directories, executingLocation);

		if (binDirectory is not null)
		{
			AddSearchDirectory(directories, Path.Combine(binDirectory, CommonAssemblyInfo.CWNetCoreSubfolder));
			AddSearchDirectory(directories, binDirectory);
		}

		AddSearchDirectory(directories, GetNetCoreAppDirectory());
		AddSearchDirectory(directories, GetNetFrameworkDirectory());

		return directories;
	}

	static string GetNetCoreAppDirectory()
	{
		return RuntimeEnvironment.GetRuntimeDirectory();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "framework string")]
	static string GetNetFrameworkDirectory()
	{
		try
		{
			using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework");
			if (key?.GetValue("InstallRoot") is string installRoot)
			{
				return Path.Combine(installRoot, typeof(object).Assembly.ImageRuntimeVersion);
			}
		}
		catch (Exception ex)
		{
			WriteOutput($"Error accessing the registry: {ex.Message}");
		}

		return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows), "Microsoft.NET", "Framework" + typeof(object).Assembly.ImageRuntimeVersion);
	}

	static void AddSearchDirectory(ICollection<string> collection, string? directory)
	{
		if (string.IsNullOrEmpty(directory) || collection.Contains(directory))
		{
			return;
		}
		WriteOutput($"Add search directory: {directory}");
		collection.Add(directory);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "bin inside targetDirectory")]
	static string? FindBinDirectory(string executingLocation)
	{
		var targetDirectory = new DirectoryInfo(executingLocation);
		if (executingLocation.EndsWith(CommonAssemblyInfo.CWNetCoreSubfolder))
		{
			targetDirectory = targetDirectory.Parent;
			if (targetDirectory is null)
			{
				return null;
			}
		}

		if (Directory.Exists(Path.Combine(targetDirectory.FullName, "bin")))
		{
			targetDirectory = new DirectoryInfo(Path.Combine(targetDirectory.FullName, "bin"));
		}

		while (!targetDirectory.EnumerateFiles().Any(f => f.Name.Equals("CargoWise.WindowsDesktop.exe", StringComparison.OrdinalIgnoreCase)))
		{
			targetDirectory = targetDirectory.Parent;
			if (targetDirectory is null)
			{
				return null;
			}
		}

		var binDirectory = targetDirectory.FullName;
		WriteOutput($"Found bin directory: {binDirectory}");
		return binDirectory;
	}

	static Assembly? AssemblyResolve(object? sender, ResolveEventArgs args, IReadOnlyList<string> assemblySearchDirectories, IReadOnlyList<string> fileExtensions)
	{
		var dllName = new AssemblyName(args.Name).Name;

		foreach (var searchDirectory in assemblySearchDirectories)
		{
			foreach (var fileExt in fileExtensions)
			{
				var path = Path.Combine(searchDirectory, dllName + fileExt);
				if (File.Exists(path))
				{
					try
					{
						var resolved = Assembly.LoadFrom(path);
						WriteOutput($"Assembly Resolved {dllName}: {path}");
						return resolved;
					}
					catch (Exception ex)
					{
						WriteOutput($"Assembly resolved error {dllName}: {path}, {ex}");
						return null;
					}
				}
			}
		}

		WriteOutput($"Assembly NOT resolved: {dllName}");
		return null;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "Logging may not be available")]
	static void WriteOutput(string message)
	{
		Debug.WriteLine(message);
		Console.WriteLine(message);
	}
}
#nullable restore
#pragma warning restore CS0436
#pragma warning restore IDE0005
