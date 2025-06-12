namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public interface IPop3MailClientConfiguration : IMailClientConfiguration
	{
		string UserName { get; }
		string Password { get; }
		int HeaderLineNumberToSkip { get; }
	}
}
