using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondCargoDesc))]
	sealed class CusInBondCargoDescCustomLabelsProviderTest : BusinessObjectWithCustomLabelsTestCase
	{
		public void TestCustomLabelsList()
		{
			var header = Factory.New<CusInBondHeader>();
			ICustomLabelsProvider provider = new CusInBondCargoDescCustomLabelsProvider(header);
			var list = provider.GetCustomFields(null, Factory);
			AssertEquals("Should have 0 items", 0, list.Count);
			var org = Factory.New<OrgHeader>();
			list = provider.GetCustomFields(org, Factory);
			AssertEquals("Should have 0 items", 0, list.Count);
			Factory.ClearCachedValue<Dictionary<ZGuid, CustomLabelInfoList>>("USInBondCommodity_CustomLabelsProvider");
			list = provider.GetCustomFields(org, Factory);
			AssertEquals("Should have 0 items", 0, list.Count);
			header.BH_OA_Importer = org.MainAddress.PK;
			Factory.ClearCachedValue<Dictionary<ZGuid, CustomLabelInfoList>>("USInBondCommodity_CustomLabelsProvider");
			list = provider.GetCustomFields(org, Factory);
			AssertEquals("Should have correct number of items", 4, list.Count);
			AssertCustomsLabelInfo(list[0], CusInBondCargoDesc.Schema.BY_PartAttrib1, "Part Attribute 1");
			AssertCustomsLabelInfo(list[1], CusInBondCargoDesc.Schema.BY_PartAttrib2, "Part Attribute 2");
			AssertCustomsLabelInfo(list[2], CusInBondCargoDesc.Schema.BY_PartAttrib3, "Part Attribute 3");
			var serialNumCustomLabelInfo1 = list[3];
			AssertCustomsLabelInfo(serialNumCustomLabelInfo1, CusInBondCargoDesc.Schema.BY_SerialNumber, "Serial Number");
			Assert(!serialNumCustomLabelInfo1.IsEnabled);
			org.MiscServ.OM_IMPartAttrib1Name = "VIN1";
			org.MiscServ.OM_IMPartAttrib2Name = "VIN2";
			org.MiscServ.OM_IMPartAttrib3Name = "VIN3";
			org.MiscServ.OM_IMUseSerialNumber = true;
			Factory.ClearCachedValue<Dictionary<ZGuid, CustomLabelInfoList>>("USInBondCommodity_CustomLabelsProvider");
			list = provider.GetCustomFields(org, Factory);
			AssertEquals("Should have correct number of items", 4, list.Count);
			AssertCustomsLabelInfo(list[0], CusInBondCargoDesc.Schema.BY_PartAttrib1, "VIN1");
			AssertCustomsLabelInfo(list[1], CusInBondCargoDesc.Schema.BY_PartAttrib2, "VIN2");
			AssertCustomsLabelInfo(list[2], CusInBondCargoDesc.Schema.BY_PartAttrib3, "VIN3");
			var serialNumCustomLabelInfo2 = list[3];
			AssertCustomsLabelInfo(serialNumCustomLabelInfo2, CusInBondCargoDesc.Schema.BY_SerialNumber, "Serial Number");
			Assert(serialNumCustomLabelInfo2.IsEnabled);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			return container.Commodities.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			return container.Commodities.AddNew();
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bizObj)
		{
			var header = ((CusInBondCargoDesc)bizObj).Header;
			header.BH_OA_Importer = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var result = new CusInBondCargoDescCustomLabelsProvider(header);
			return result;
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			if (info.Name == CusInBondCargoDesc.Schema.BY_WarehouseEntryNumber || info.Name == CusInBondCargoDesc.Schema.BY_WarehouseEntryLineNo || info.Name == CusInBondCargoDesc.Schema.BY_InvoiceQuantity || info.Name == CusInBondCargoDesc.Schema.BY_PartAttrib1 || info.Name == CusInBondCargoDesc.Schema.BY_PartAttrib3 || info.Name == CusInBondCargoDesc.Schema.BY_PartAttrib3)
			{
				((CusInBondCargoDesc)info.BizObj).BY_PartNumber = "PART323";
			}

			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
		}

		void AssertCustomsLabelInfo(CustomLabelInfoBase info, string propertyName, string caption)
		{
			AssertEquals("PropertyName", propertyName, info.PropertyName);
			AssertEquals("Caption", caption, info.Caption);
		}
	}
}
