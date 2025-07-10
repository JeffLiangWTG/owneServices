using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	sealed class TRCarrierManifestItemWrapperTest : TestCaseWithFactory
	{
		public void TestWrapperAtBillLevel()
		{
			SetData();
			var itemWrapper = new TRCarrierManifestItemWrapper(header.Bills[0]);
			AssertEquals(header.Bills[0].ABL_BillNumber, itemWrapper.BillNumber);
			AssertEquals(header.Bills[0].ABL_SequenceNumber.ToString(), itemWrapper.BillSequenceNumber);
			AssertEquals(header.Bills[0].ABL_ShipperName + "\r\n" + header.Bills[0].ABL_ConsigneeName, itemWrapper.ShipperAndConsigneeNames);
			AssertEquals(itemWrapper.PackLineNo, ZString.Empty);
			AssertEquals(itemWrapper.PackQty, ZString.Empty);
			AssertEquals(itemWrapper.PackUQ, ZString.Empty);
			AssertEquals(itemWrapper.PackContainerNumber, ZString.Empty);
			AssertEquals(itemWrapper.ItemTariff, ZString.Empty);
			AssertEquals(itemWrapper.ItemGoodsDescription, ZString.Empty);
			AssertEquals(itemWrapper.ItemGrossWeight, "0.00");
		}

		public void TestWrapperAtPackLevel()
		{
			SetData();
			var itemWrapper = new TRCarrierManifestItemWrapper(header.Bills[0].Packs[0]);
			AssertEquals(header.Bills[0].Packs[0].APA_LineNo.ToString(), itemWrapper.PackLineNo);
			AssertEquals(header.Bills[0].Packs[0].APA_PackQty.ToString(), itemWrapper.PackQty);
			AssertEquals(header.Bills[0].Packs[0].APA_PackUQ, "BAG");
			AssertEquals(itemWrapper.PackUQ, "BI");
			AssertEquals(header.Bills[0].Packs[0].Container.ACN_ContainerNumber, itemWrapper.PackContainerNumber);
			AssertEquals(header.Bills[0].ABL_BillNumber, itemWrapper.BillNumber);
			AssertEquals(header.Bills[0].ABL_BillNumber, itemWrapper.BillNumber);
			AssertEquals(header.Bills[0].ABL_SequenceNumber.ToString(), itemWrapper.BillSequenceNumber);
			AssertEquals(header.Bills[0].ABL_ShipperName + "\r\n" + header.Bills[0].ABL_ConsigneeName, itemWrapper.ShipperAndConsigneeNames);
			AssertEquals(itemWrapper.ItemTariff, ZString.Empty);
			AssertEquals(itemWrapper.ItemGoodsDescription, ZString.Empty);
			AssertEquals(itemWrapper.ItemGrossWeight, "0.00");
		}

		public void TestWrapperAtItemLevel()
		{
			SetData();
			var itemWrapper = new TRCarrierManifestItemWrapper((AsycudaPackedItem)header.Bills[0].Packs[0].PackedItems[0].PackedItem);
			AssertEquals(header.Bills[0].Packs[0].APA_LineNo.ToString(), itemWrapper.PackLineNo);
			AssertEquals(header.Bills[0].Packs[0].APA_PackQty.ToString(), itemWrapper.PackQty);
			AssertEquals(header.Bills[0].Packs[0].APA_PackUQ, "BAG");
			AssertEquals(itemWrapper.PackUQ, "BI");
			AssertEquals(header.Bills[0].Packs[0].Container.ACN_ContainerNumber, itemWrapper.PackContainerNumber);
			AssertEquals(header.Bills[0].ABL_BillNumber, itemWrapper.BillNumber);
			AssertEquals(header.Bills[0].ABL_BillNumber, itemWrapper.BillNumber);
			AssertEquals(header.Bills[0].ABL_SequenceNumber.ToString(), itemWrapper.BillSequenceNumber);
			AssertEquals(header.Bills[0].ABL_ShipperName + "\r\n" + header.Bills[0].ABL_ConsigneeName, itemWrapper.ShipperAndConsigneeNames);
			AssertEquals(header.Bills[0].Packs[0].PackedItems[0].PackedItem.API_Tariff, itemWrapper.ItemTariff);
			AssertEquals(header.Bills[0].Packs[0].PackedItems[0].PackedItem.API_GoodsDescription, itemWrapper.ItemGoodsDescription);
			AssertEquals(NumericUtil.ToStringWithDecimalPlaces(header.Bills[0].Packs[0].PackedItems[0].PackedItem.API_GrossWeight, 2), itemWrapper.ItemGrossWeight);
			itemWrapper = new TRCarrierManifestItemWrapper((AsycudaPackedItem)header.Bills[0].Packs[0].PackedItems[1].PackedItem);
			AssertEquals(header.Bills[0].Packs[0].PackedItems[1].PackedItem.API_Tariff, itemWrapper.ItemTariff);
			AssertEquals(header.Bills[0].Packs[0].PackedItems[1].PackedItem.API_GoodsDescription, itemWrapper.ItemGoodsDescription);
			AssertEquals(NumericUtil.ToStringWithDecimalPlaces(header.Bills[0].Packs[0].PackedItems[1].PackedItem.API_GrossWeight, 2), itemWrapper.ItemGrossWeight);
		}

		public void TestBillWrapperShouldBeAccesible()
		{
			SetData();
			var bill = header.Bills[0];
			bill.ABL_ShipperName = "Test Shipper Name";
			bill.ABL_ConsigneeName = "Test ConsigneeName";
			var packedItem = (AsycudaPackedItem)bill.Packs[0].PackedItems[0].PackedItem;
			var itemWrapper = new TRCarrierManifestItemWrapper(packedItem);

			AssertEquals("Test Shipper Name", itemWrapper.BillWrapper.ABL_ShipperName);
			AssertEquals("Test ConsigneeName", itemWrapper.BillWrapper.ABL_ConsigneeName);

			bill = header.Bills.AddNew();
			bill.ABL_ShipperName = "Test Shipper Name 2";
			bill.ABL_ConsigneeName = "Test ConsigneeName 2";
			var pack = bill.Packs.AddNew();
			packedItem = (AsycudaPackedItem)pack.PackedItems.AddNewPackedItem();
			itemWrapper = new TRCarrierManifestItemWrapper(packedItem);

			AssertEquals("Test Shipper Name 2", itemWrapper.BillWrapper.ABL_ShipperName);
			AssertEquals("Test ConsigneeName 2", itemWrapper.BillWrapper.ABL_ConsigneeName);

			packedItem = (AsycudaPackedItem)pack.PackedItems.AddNewPackedItem();
			itemWrapper = new TRCarrierManifestItemWrapper(packedItem);

			AssertEquals("Test Shipper Name 2", itemWrapper.BillWrapper.ABL_ShipperName);
			AssertEquals("Test ConsigneeName 2", itemWrapper.BillWrapper.ABL_ConsigneeName);
		}

		public void TestWrapperForCusSupportingInfo()
		{
			SetData();
			var rd1 = header.Bills[0].RelatedDeclarationForExports.AddNew();
			rd1.CSI_ReferenceNumber = "RD1";
			rd1.CSI_SubType = "YES";
			rd1.CSI_Quantity = 1;
			rd1.CSI_Quantity2 = 10;
			rd1.CSI_Procedure = "OZBY";
			var rd2 = header.Bills[0].RelatedDeclarationForExports.AddNew();
			rd2.CSI_ReferenceNumber = "RD2";
			rd2.CSI_SubType = "NO";
			rd2.CSI_Quantity = 2;
			rd2.CSI_Quantity2 = 22;
			rd2.CSI_Procedure = "AKTARMA";
			var bill2 = header.Bills.AddNew();
			var rd3 = bill2.RelatedDeclarationForExports.AddNew();
			rd3.CSI_ReferenceNumber = "RD3";
			rd3.CSI_SubType = "YES";
			rd3.CSI_Quantity = 3;
			rd3.CSI_Quantity2 = 30;
			rd3.CSI_Procedure = "DIGER";
			var bill3 = header.Bills.AddNew();
			var itemWrapper = new TRCarrierManifestItemWrapper(header.Bills[0]);
			var itemWrapper2 = new TRCarrierManifestItemWrapper(bill2);
			var itemWrapper3 = new TRCarrierManifestItemWrapper(bill3);

			CombineAssertions(() =>
			{
				AssertEquals("CusReferenceNumber combined", rd1.CSI_ReferenceNumber + "\r\n" + rd2.CSI_ReferenceNumber, itemWrapper.CusReferenceNumber);
				AssertEquals("CusPartial combined", "E\r\nH", itemWrapper.CusPartial);
				AssertEquals("CusBoxQuantity combined", rd1.CSI_Quantity.ToStringTrimZeros() + "\r\n" + rd2.CSI_Quantity.ToStringTrimZeros(), itemWrapper.CusBoxQuantity);
				AssertEquals("CusGrossWeight combined", rd1.CSI_Quantity2.ToStringTrimZeros() + "\r\n" + rd2.CSI_Quantity2.ToStringTrimZeros(), itemWrapper.CusGrossWeight);
				AssertEquals("CusProcedure combined", rd1.CSI_Procedure + "\r\n" + rd2.CSI_Procedure, itemWrapper.CusProcedure);

				AssertEquals("CusReferenceNumber single", rd3.CSI_ReferenceNumber, itemWrapper2.CusReferenceNumber);
				AssertEquals("CusPartial single", "E", itemWrapper2.CusPartial);
				AssertEquals("CusBoxQuantity single", rd3.CSI_Quantity.ToStringTrimZeros(), itemWrapper2.CusBoxQuantity);
				AssertEquals("CusGrossWeight single", rd3.CSI_Quantity2.ToStringTrimZeros(), itemWrapper2.CusGrossWeight);
				AssertEquals("CusProcedure single", rd3.CSI_Procedure, itemWrapper2.CusProcedure);

				AssertEquals("CusReferenceNumber empty", ZString.Empty, itemWrapper3.CusReferenceNumber);
				AssertEquals("CusPartial empty", ZString.Empty, itemWrapper3.CusPartial);
				AssertEquals("CusBoxQuantity empty", ZString.Empty, itemWrapper3.CusBoxQuantity);
				AssertEquals("CusGrossWeight empty", ZString.Empty, itemWrapper3.CusGrossWeight);
				AssertEquals("CusProcedure empty", ZString.Empty, itemWrapper3.CusProcedure);
			});
		}

		AsycudaManifestHeader header;
		void SetData()
		{
			if (header == null)
			{
				var testWeight = new ZDecimal(5.15);
				header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				company.CompanyName = "TestName";
				company.Address1 = "Test adress 1";
				company.Address2 = "Test adress 2";
				company.Branches.Add(branch);
				var container = Factory.New<AsycudaContainer>();
				container.ACN_ContainerNumber = "ASD-1234-A";
				var bill1 = header.Bills.AddNew();
				var pack11 = bill1.Packs.AddNew();
				var item111 = pack11.PackedItems.AddNewPackedItem();
				var item112 = pack11.PackedItems.AddNewPackedItem();
				pack11.APA_PackQty = 1;
				pack11.APA_PackUQ = "BAG";
				pack11.ContainerPK = container.PK;
				header.Containers.Add(container);
				item111.API_GrossWeight = testWeight;
				item112.API_GrossWeight = testWeight;
				item111.API_GrossWeightUQ = "KG";
				item112.API_GrossWeightUQ = "KG";
				Factory.Save();
			}
		}
	}
}
