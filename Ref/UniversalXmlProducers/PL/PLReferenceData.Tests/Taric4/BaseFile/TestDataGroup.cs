using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

sealed class TestDataGroup : IDataGroup<TestDataPoint>
{
	public TestDataPoint[] DataPoints { get; set; }
}
