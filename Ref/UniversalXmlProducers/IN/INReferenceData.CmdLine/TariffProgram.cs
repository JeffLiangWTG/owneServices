using CargoWise.RefDbRepo.INReferenceData.Business;

namespace CargoWise.RefDbRepo.INReferenceData.CmdLine
{
	public static class TariffProgram
	{
		public static void Run()
		{
			new TariffXmlProducer(DataSource).ProduceXml();
		}

		const string DataSource = "IN Tariff";
	}
}
