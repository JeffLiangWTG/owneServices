using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	[TestedType(typeof(USAMSBillModule))]
	sealed class USAMSBillModuleTest : ZModuleBasherTest
	{
		public void TestSupportWorkflow()
		{
			Assert(Module.SupportsWorkflow);
		}

		public void TestDenyNewAndDelete()
		{
			AssertEquals(false, Module.AllowNew);
			AssertEquals(false, Module.AllowDelete);
		}

		USAMSBillModule Module => module ?? (module = (USAMSBillModule)ZModuleFactory.Instance.Create(GetModuleID()));
		USAMSBillModule module;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.US.AMSBill;
		}

		protected override void TearDown()
		{
			if (module != null)
			{
				module.Dispose();
			}

			base.TearDown();
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			var consol = factory.New<ForwardingConsol>();
			var header = factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "ABCD123456789012";
			factory.Save();
		}
	}
}
