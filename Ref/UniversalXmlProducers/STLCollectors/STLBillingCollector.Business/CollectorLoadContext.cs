using System.Reflection;
using System.Runtime.Loader;

namespace CargoWise.RefDbRepo.STLBillingCollector.Business;

public class CollectorLoadContext() : AssemblyLoadContext(isCollectible: true)
{
	protected override Assembly Load(AssemblyName assemblyName) => null;
}
