using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.NZReferenceData.Business;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	class TestConstants
	{
		public const string ConcessionToTariffHeader = "Ctac Concession Code~Ctac Tariff Level 1~Ctac Tariff Level 2~Ctac Tariff Level 3~Ctac Tariff Level 4~Ctac Tariff Level 5~Ctac Tariff Section";
		public const string ConcessionDetailsHeader = "Cc Concession Code~Cc Start Date~Cc Expiry Date";
		public const string ConcessionRatesHeader = "Cdrc Concession Code~Cdrc Rate Group~Cdrc Expiry Date~Cdrc Excise Factor~Cdrc Rate Formula~Cdrc Factor A~Cdrc Factor B~Cdrc Factor C~Cdrc Factor D~Cdrc Factor E~Cdrc Factor F";

		public static BuildersFilePath[] ConcessionFilePaths { get; } = new[]{
			new BuildersFilePath("Concession_Details.txt", BuilderFilePathSymbol.ConcessionDetail),
			new BuildersFilePath("Concession_Rates.txt", BuilderFilePathSymbol.ConcessionRate),
			new BuildersFilePath("Concession_to_Tariff.txt", BuilderFilePathSymbol.ConcessionToTariff),
			new BuildersFilePath("nz_en_concessions_paper.json", BuilderFilePathSymbol.ConsolidatedListOfApprovalsJson),
		};
	}
}
