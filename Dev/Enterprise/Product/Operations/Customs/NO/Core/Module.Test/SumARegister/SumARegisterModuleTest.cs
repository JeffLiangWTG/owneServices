using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using CusTempStorageRegHeader = Enterprise.Customs.NO.Business.CusTempStorageRegHeader;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(SumARegisterModule))]
sealed class SumARegisterModuleTest : ZModuleBasherTest
{
	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.NO.TemporaryStorageRegister;

	protected override string CountryCode => Constants.CountryCodes.Norway;

	public void TestGetNewGridCollection()
	{
		using (var module = (SumARegisterModule)GetModule())
		{
			var collection = module.GridCollection;
			AssertType<CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>>("Type", collection);
			var register = Factory.New<CusTempStorageRegHeader>();
			register.SRH_AppCode = TemporaryStorageApplicationCodeList.Codes.SBW;
			Assert(register.MatchesFilter(collection.CompleteFilter));
		}
	}
}
