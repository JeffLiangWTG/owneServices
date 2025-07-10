using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Module.Testing;

[TestedType(typeof(TemporaryStorageModule))]
class TemporaryStorageModuleTest : EU.TemporaryStorage.Module.Testing.TemporaryStorageModuleAbstractTest
{
	public override void TestModuleShowsAndCanSearch()
	{
		var dec = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
		dec.SJH_GB = GlbBranch.CurrentBranch.PK;

		CusTempStorageDec.New(dec);
		Factory.Save();

		base.TestModuleShowsAndCanSearch();
	}

	protected BusinessObject GetBusinessObjectHeader()
	{
		var header = CusTempStorageJobHeader.New(Factory);
		Factory.Save();
		return header;
	}

	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.TemporaryStorage;

	protected override string CountryCode => Core.Constants.CountryCodes.Poland;

	protected override bool HasController() => true;
}
