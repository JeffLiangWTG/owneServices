using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCarrierModule))]
	public class ZZRefCarrierModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.Universal.ZZRefCarrier;
		}

		public void TestItemsIsAllowed()
		{
			using (var module = new ZZRefCarrierModule())
			{
				AssertEquals("View is allowed.", true, module.AllowView);
				AssertEquals("New is not allowed.", true, module.AllowNew);
				AssertEquals("Edit is not allowed.", true, module.AllowEdit);
				AssertEquals("Delete is not allowed.", true, module.AllowDelete);
				AssertEquals("UniversalCopy is not allowed.", false, module.AllowUniversalCopy);
			}
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var zzCarrier = collection.Factory.New<ZZRefCarrierCombined>();
			zzCarrier.ZZ4_CountryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			zzCarrier.ZZ4_Code = "~77";
			zzCarrier.ZZ4_Description = "JJ";
			collection.Factory.Save(); // Why is this necessary?  Without it the text fails ("collection has no members"), but surely an active collection doesn't need saving??!?!
		}
	}
}
