namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	sealed class CustomsOffice
	{
		public string CustomsName { get; set; }

		public string OfficialSignature { get; set; }

		public string Department { get; set; }

		public override string ToString() => string.Concat(OfficialSignature, "+", Department);
	}
}
