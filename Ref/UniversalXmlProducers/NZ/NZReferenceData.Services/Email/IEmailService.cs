namespace CargoWise.RefDbRepo.NZReferenceData.Services
{
	public interface IEmailService
	{
		void SendEmail(string subject, string body, bool isHtmlBody);
	}
}
