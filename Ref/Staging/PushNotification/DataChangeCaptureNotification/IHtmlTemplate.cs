namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	public interface IHtmlTemplate
	{
		string GetHeader();
		string GetFooter();
		string GetTitle(string message);
	}
}
