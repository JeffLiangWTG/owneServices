namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public interface IMAFNotification
	{
		bool ShowConfirmation(string message, string caption);
		void ShowError(string message, string caption);
		void ShowInformation(string message, string caption);
	}
}
