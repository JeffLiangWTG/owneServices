using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.SEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.Nomenclature.Tests
{
	[TestFixture]
	class DownloadGoodsTest : DownloadFileTest<goodsNomenclature>
	{
		[OneTimeSetUp]
		public override void Setup()
		{
			TestFilesPath = @"SE\Nomenclature\TestFiles\";
			base.Setup();
			InputTestFileName = "GoodsNomenclature_GUID_230908";
			ItemsCount = 10;
		}
	}
}
