using System.IO;

namespace CargoWise.Blazor.Common
{
	/// <summary>
	/// Strings identifying output/publish paths and repo paths which some integration tests (and the converter) need to know about.
	///
	/// Not for use in deployed Blazor applications (CargoWise.Winzor.AppServer,SessionBroker,etc) - they shouldn't know about the build process
	///
	/// </summary>
	public static class BuildFileSystem
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static readonly string InfrastructureSourcePath = Path.Combine("Winzor", "Infrastructure");

		/// <summary>
		/// CargoWise.Winzor.AppServer outputs (relative to bin\)
		/// </summary>
		public static class AppServerBin
		{
			const string PublishDirectoryName = @"AppServer";
			public static readonly string PublishDirectoryPath = PublishDirectoryName + @"\";
			public const string PublishExecutableFileName = @"CargoWise.Winzor.AppServer.exe";
			public static readonly string PublishExePath = Path.Combine(PublishDirectoryPath, PublishExecutableFileName);
		}

		/// <summary>
		/// CargoWise.Winzor.AppServer project (relative to root of source repo)
		/// </summary>
		public static class AppServerProject
		{
			const string AppName = @"CargoWise.Winzor.AppServer";
			public static readonly string ProjectDirectoryPath = Path.Combine(InfrastructureSourcePath, @"CargoWise.Winzor.AppServer");
			public static readonly string ProjectFilePath = Path.Combine(ProjectDirectoryPath, @"CargoWise.Winzor.AppServer.csproj");
			public static readonly string ContentRootName = FormatContentRootName(AppName);
		}

		/// <summary>
		/// CargoWise.Winzor.AppServer project (relative to root of source repo)
		/// </summary>
		public static class AppServerTestProject
		{
			public const string Namespace = @"CargoWise.Winzor.AppServer.Test";
			public static readonly string ProjectDirectoryPath = Path.Combine(InfrastructureSourcePath, Namespace);
			public static readonly string ProjectFilePath = Path.Combine(InfrastructureSourcePath, Namespace, @"CargoWise.Winzor.AppServer.Test.csproj");
		}

		/// <summary>
		/// Blazor.Controls.Test project (relative to root of source repo)
		/// </summary>
		public static class ControlsTestProject
		{
			public const string Namespace = @"CargoWise.Blazor.Controls.Tests";
			public static readonly string ProjectDirectoryPath = Path.Combine(InfrastructureSourcePath, Namespace);
			public static readonly string ProjectFilePath = Path.Combine(InfrastructureSourcePath, Namespace, @"CargoWise.Blazor.Controls.Test.csproj");
		}

		/// <summary>
		/// SessionBroker outputs (relative to bin\)
		/// </summary>
		public static class SessionBrokerBin
		{
			const string PublishDirectoryName = @"SessionBroker";
			public static readonly string PublishDirectoryPath = PublishDirectoryName + @"\";
			public const string PublishExecutableFileName = @"CargoWise.Blazor.SessionBroker.exe";
			public static readonly string PublishExePath = Path.Combine(PublishDirectoryPath, PublishExecutableFileName);
		}

		/// <summary>
		/// SessionBroker project (relative to root of source repo)
		/// </summary>
		public static class SessionBrokerProject
		{
			const string AppName = @"CargoWise.Blazor.SessionBroker";
			public static readonly string ProjectDirectoryPath = Path.Combine(InfrastructureSourcePath, @"CargoWise.Blazor.SessionBroker");
			public static readonly string ProjectFilePath = Path.Combine(ProjectDirectoryPath, @"CargoWise.Blazor.SessionBroker.csproj");
			public static readonly string ContentRootName = FormatContentRootName(AppName);
		}

		static string FormatContentRootName(string appName) => appName.Replace(".", "_", System.StringComparison.OrdinalIgnoreCase).ToUpperInvariant();
	}
}
