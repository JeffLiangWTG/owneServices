
namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class DispatchInstructionDocuments
	{
		public static class OperationTypes
		{
			public const string CATP = "CATP";
			public const string DUIMP = "DUIMP";
			public const string LPCO = "LPCO";

			public static string[] ToArray => new[] { CATP, DUIMP, LPCO };
		}
	}
}
