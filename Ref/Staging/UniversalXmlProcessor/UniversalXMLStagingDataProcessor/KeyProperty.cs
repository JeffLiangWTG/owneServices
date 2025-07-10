using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class KeyProperty
	{
		public string Name { get; set; }
		public Operations Operation { get; set; } = Operations.Equals;
		public string ConstantValue { get; set; }
		public int Order { get; set; }
	}
}
