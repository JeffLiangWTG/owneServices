using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using Enterprise.Edifact.D96B.Segments;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses
{
	internal class ProdatLoaderForTest
	{
		public TransactionType GetTransactionType(string bgmSegment)
		{
			var bgm = new BGMSegment();
			bgm.Parse(ZACharacterSet.Instance, bgmSegment);

			return ProdatLoader.GetTransactionTypeFromMessageFunction(bgm.MessageFunctionCoded);
		}

		public Rate PopulateRate(string ftxSegment)
		{
			var ftx = new FTXSegment();

			ftx.Parse(ZACharacterSet.Instance, ftxSegment);

			return ProdatLoader.GetRate(ftx);
		}
	}
}
