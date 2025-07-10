using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.Blazor.Common;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CargoWise.Blazor.SessionBroker.Test
{
	/// <summary>
	/// Will kill any processes that match the file name that are currently running on the machine
	/// </summary>
	public abstract class KillProcessesAttribute : Attribute, ITestAction
	{
		readonly string fileName;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="fileName"></param>
		public KillProcessesAttribute(string fileName)
		{
			this.fileName = fileName;
		}

		public ActionTargets Targets => ActionTargets.Suite | ActionTargets.Test;

		public void AfterTest(ITest test) => KillProcesses();

		public void BeforeTest(ITest test) => KillProcesses();

		[SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		void KillProcesses()
		{
			var processName = Path.GetFileNameWithoutExtension(fileName);
			foreach (var process in Process.GetProcessesByName(processName))
			{
				process.Kill(true);
			}
		}
	}

	/// <summary>
	/// Kills any CargoWise.Winzor.AppServer processes
	/// </summary>
	public sealed class KillBlazorAppProcessesAttribute : KillProcessesAttribute
	{
		public KillBlazorAppProcessesAttribute() : base(BuildFileSystem.AppServerBin.PublishExecutableFileName)
		{
		}
	}

	/// <summary>
	/// Kills any CargoWise.Winzor.AppServer processes
	/// </summary>
	public sealed class KillMockAppProcessesAttribute : KillProcessesAttribute
	{
		public KillMockAppProcessesAttribute() : base("CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe")
		{
		}
	}

	/// <summary>
	/// Kills any SessionBroker processes
	/// </summary>
	public sealed class KillSessionBrokerProcessesAttribute : KillProcessesAttribute
	{
		public KillSessionBrokerProcessesAttribute() : base(BuildFileSystem.SessionBrokerBin.PublishExecutableFileName)
		{
		}
	}
}
