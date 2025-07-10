using CargoWise.RefDbRepo.INReferenceData.Business;

namespace CargoWise.RefDbRepo.INReferenceData.CmdLine
{
	public static class ErrorCodesProgram
	{
		const string DataSource = "IN Error Codes";

		public static void Run()
		{
			new ErrorCodesXmlProducer(DataSource).ProduceXml();
		}
	}
}
