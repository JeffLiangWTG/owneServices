using CargoWise.Definitions;

namespace Enterprise.ZArchitecture.Modules.DocumentScanning
{
	public interface IAssemblyDataProvider
	{
		string DocManagerCode { get; }
		string TypeName { get; }
		string TypeAssemblyName { get; }
		string Country { get; }
		Clients ClientSpecificCode { get; }
	}
}
