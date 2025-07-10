using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(SingleTariffClassificationModule))]
	sealed class SingleTariffClassificationModuleBaseOnlyTest : SingleTariffClassificationModuleAbstractTest<SingleTariffClassificationModule>
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SriLanka);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.SingleTariffClassification;
	}
}
