using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Dat.Integration;

namespace CargoWise.RefDbRepo.Deployment
{
	public class PowerShellScriptCreator
	{
		readonly string _binPath;
		readonly string _fileName;
		readonly IErrorBuilder _errorBuilder;
		readonly ITaskLogger _logger;

		public PowerShellScriptCreator(string binPath, string fileName, IErrorBuilder errorBuilder, ITaskLogger logger)
		{
			_binPath = binPath;
			_fileName = fileName;
			_errorBuilder = errorBuilder;
			_logger = logger;
		}

		string PSScriptPath => Path.Combine(_binPath, @"Tools\net8.0", _fileName);

		public PowerShell Create(Runspace runspace)
		{
			var ps = PowerShell.Create(runspace);
			ps.AddCommand(PSScriptPath);
			return ps;
		}

		public void Invoke(PowerShell powershell)
		{
			using (var output = new PSDataCollection<PSObject>())
			{
				output.DataAdded += (sender, args) =>
				{
					var data = ((PSDataCollection<PSObject>)sender)[args.Index];
					if (data != null)
					{
						Log(data.ToString());
					}
				};
				var errorStream = powershell.Streams?.Error;
				errorStream.DataAdded += (sender, args) =>
				{
					var errorRecord = ((PSDataCollection<ErrorRecord>)sender)[args.Index];
					if (errorRecord != null)
					{
						Log(errorRecord.ToString());
						Log(errorRecord.ScriptStackTrace);
						_errorBuilder.Append(errorRecord);
					}
				};

				var result = powershell.BeginInvoke<PSObject, PSObject>(null, output);
				result.AsyncWaitHandle.WaitOne(600000);
				powershell.EndInvoke(result);
				var errorString = _errorBuilder.Build();
				if (!string.IsNullOrEmpty(errorString))
				{
					throw new ApplicationFailedException("Error: " + errorString);
				}
			}
		}

		void Log(string message)
		{
			_logger?.RecordInfo($"{DateTime.Now}: {message}");
		}
	}
}
