namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	class TestConstants
	{
		internal const string AESVersion3_0 = "3.0";
		internal const string AtlasVersion10_0 = "10.0";
		internal const string AtlasVersion10_1 = "10.1";

		internal static readonly string[] ImportCodeListVersions = new[] { TestConstants.AtlasVersion10_0, TestConstants.AtlasVersion10_1 };

		internal static readonly string[] ExportCodeListVersions = new[] { TestConstants.AESVersion3_0 };

		internal static readonly string[] NctsCodeListVersions = new[] { TestConstants.AtlasVersion10_1 };
	}
}
