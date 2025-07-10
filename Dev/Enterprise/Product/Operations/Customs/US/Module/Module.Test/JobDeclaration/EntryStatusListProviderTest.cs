using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var codePairImport = new ImportEntryStatusList();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.UnitedStates, codePairImport.GetAllCodes(), new string[] { "RT1", "ROK", "CEO" }, Common.Shared.SharedJobMessageTypeList.Codes.Import);
			var codePairExport = ReplaceNotSendCode(new AESDirectCustomsEntryStatus());
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.UnitedStates, codePairExport.GetAllCodes(), new string[] { "RT1", "ROK", "CEO" }, Common.Shared.SharedJobMessageTypeList.Codes.Export);
			var codePairImportExport = new CodeDescriptionPairList(codePairImport);
			codePairImportExport.AddRange(codePairExport);
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.UnitedStates, codePairImportExport.GetAllCodes(), new string[] { "RT1", "ROK", "CEO" });
		}
	}
}
