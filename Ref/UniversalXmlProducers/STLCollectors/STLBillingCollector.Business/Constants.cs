
namespace CargoWise.RefDbRepo.STLBillingCollector.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string STLCollector = "STLCollector";
			public const string CollectorInterfaceName = ".IRefStlScript";
			public const string SourceNamespace = "CargoWise.Billing.Collectors";
		}

		public static class Dll
		{
			public const string DllName = "CargoWise.Billing.Collectors.dll";
			public const string DllExtractOutputDir = "STLNuGetPackage";
			public const string DllLocation = "lib";
		}
	}
}
