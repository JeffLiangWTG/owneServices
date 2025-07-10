using System.Diagnostics;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	class ProcessWrapper : IProcessWrapper
	{
		public ProcessWrapper(Process process)
		{
			Argument.NotNull(process, nameof(process));
			this.process = process;
		}

		readonly Process process;

		public int Id => IsActive ? process.Id : int.MinValue;
		public string ProcessName => process.ProcessName;
		public bool HasExited => process.HasExited;
		public long WorkingSet64 => process.WorkingSet64;
		public bool IsActive => !HasExited && Process.GetProcessesByName(process.ProcessName).Length > 0;
		public IntPtr Handle => process.Handle;

		public void Refresh() => process.Refresh();
		public void Kill() => process.Kill();
	}
}
