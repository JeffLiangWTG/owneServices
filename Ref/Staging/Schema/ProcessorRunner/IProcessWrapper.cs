namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	public interface IProcessWrapper
	{
		int Id { get; }
		string ProcessName { get; }
		bool HasExited { get; }
		long WorkingSet64 { get; }
		bool IsActive { get; }
		IntPtr Handle { get; }
		void Refresh();
		void Kill();
	}
}
