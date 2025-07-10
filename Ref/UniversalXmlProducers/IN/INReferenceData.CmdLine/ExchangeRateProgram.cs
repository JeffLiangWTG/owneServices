using System;
using CargoWise.RefDbRepo.INReferenceData.Business;

namespace CargoWise.RefDbRepo.INReferenceData.CmdLine
{
	public static class ExchangeRateProgram
	{
		const string DataSource = "IN Exchange Rate";

		public static void Run()
		{
			var errors = new ExchangeRateXmlProducer(DataSource).ProduceXml();
			if (errors.Count > 0)
			{
				errors.ForEach(Console.Error.WriteLine);
			}
		}

	}
}
