using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module.Testing
{
	sealed class EntryHeaderFilterLookupsBaseOnlyTest : TestCaseWithFactory
	{
		public void TestShipmentTypeList()
		{
			AssertEquals(typeof(JobMessageTypeList), filterBizObj.Lookups.ShipmentTypeList.GetType());
		}

		public void TestShipmentTypeList_DifferentCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var auFilterBizObj = new EntryHeaderFilterBusinessObject();
				AssertEquals("Enterprise.Customs.Common.AU.AUJobMessageTypeList", auFilterBizObj.Lookups.ShipmentTypeList.GetType().FullName);
			}
		}

		public void TestEntryInstructionStyleList()
		{
			var entryInstruction = Factory.GetNull<CusEntryInstruction>();
			AssertEquals(entryInstruction.Lookups.StyleList.GetType(), filterBizObj.Lookups.EntryInstructionStyleList.GetType());
		}

		public void TestMessageTypeList()
		{
			var entryHeader = Factory.GetNull<CusEntryHeader>();
			AssertEquals(entryHeader.Lookups.CH_MessageTypeList.GetType(), filterBizObj.Lookups.MessageTypeList.GetType());
		}

		public void TestWarehouseTransactionStatusList()
		{
			AssertEquals(typeof(WarehouseTransactionStatusList), filterBizObj.Lookups.WarehouseTransactionStatusList.GetType());
		}

		public void TestTransportTypeList()
		{
			var declaration = Factory.GetNull<BaseJobDeclaration>();
			AssertEquals(declaration.Lookups.TransportTypeList.GetType(), filterBizObj.Lookups.TransportTypeList.GetType());
		}

		public void TestDeclarantList()
		{
			AssertEquals(typeof(OrgAddressCollection), filterBizObj.Lookups.DeclarantList.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new EntryHeaderFilterBusinessObject();
		}

		EntryHeaderFilterBusinessObject filterBizObj;
	}
}
