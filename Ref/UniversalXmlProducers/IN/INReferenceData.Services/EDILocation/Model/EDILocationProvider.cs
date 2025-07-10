namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public class EDILocationProvider : IEDILocation
	{
		public EDILocationProvider(string name, string code, string mailId)
		{
			Name = name;
			Code = code;
			MailId = mailId;
		}

		public string Name { get; }

		public string Code { get; }

		public string MailId { get; }
	}
}
