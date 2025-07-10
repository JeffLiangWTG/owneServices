using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.Tests
{
	class UnitsOfMeasurementDetailsTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AISUCC5;

		protected override string ExpectedCode => "CUSUQ";

		protected override string ExpectedNameInFile => "Base measurement";

		protected override string ExpectedTableTitleInFile => "";

		protected override string ExpectedCodeFormattingRegularExpression => "123|X|[A-Z]{3,4}";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new UnitsOfMeasurementDetailsForTesting();
	}

	public class UnitsOfMeasurementDetailsForTesting : UnitsOfMeasurementDetails
	{
		protected override Task<List<RefCusCodeList>> GetEuListTask()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var euListPath = Path.Combine(binPath, @"CodeLists\TestFiles\Input\RefDBRepoUpdate_RefCusCodeListUpdate_CUSUQ_EUN.json");
			return RefDataLoader.GetRefDataAsync<RefCusCodeList>(euListPath);
		}
	}
}
