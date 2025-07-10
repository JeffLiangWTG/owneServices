using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccInvMsgModule))]
	sealed class AccInvMsgModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccInvMsg;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			AccInvMsg message1 = Factory.NewWithValidTestData<AccInvMsg>();
			message1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccInvMsg message2 = Factory.NewWithValidTestData<AccInvMsg>();
			message2.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();
		}
	}
}
