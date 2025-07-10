using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccChargeCodeModuleForRegistry))]
	sealed class AccChargeCodeModuleForRegistryTest : ZModuleBasherTest
	{
		public void TestCollectionFilter()
		{
			using (var module = new AccChargeCodeModuleForRegistryForTest())
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

				chargeCode.AC_GC = company.PK;
				chargeCode.AC_ChargeType = "CMT";

				Factory.Save();

				var collection = (BusinessObjectCollection)module.GetNewGridCollectionForTesting();

				var filter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, "CMT");
				collection.Load(filter);
				AssertEquals("Contains(ChargeCode)", true, collection.Contains(chargeCode));

				collection = (BusinessObjectCollection)module.GetNewGridCollectionForTesting();

				collection.Load();
				AssertEquals("Contains(ChargeCode)", true, collection.Contains(chargeCode));
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccChargeCodeForRegistry;
		}

		#endregion
	}
}
