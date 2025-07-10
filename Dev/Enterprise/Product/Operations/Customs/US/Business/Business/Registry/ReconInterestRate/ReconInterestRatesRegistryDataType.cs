using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.US.DataRegistry.GUI.ReconInterestRatesRegistryItemEditor, Enterprise.Customs.US.GUI")]
	sealed class ReconInterestRatesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ReconInterestRateCollection>
	{
	}
}
