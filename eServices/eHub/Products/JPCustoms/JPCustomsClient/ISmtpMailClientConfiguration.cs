namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public interface ISmtpMailClientConfiguration : IMailClientConfiguration
	{
		string EmailFrom { get; }
		string EmailTo { get; }
		string DomainName { get; }
	}
}
