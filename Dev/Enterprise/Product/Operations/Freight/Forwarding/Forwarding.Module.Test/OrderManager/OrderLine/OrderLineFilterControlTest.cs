using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public class OrderLineFilterControlTest : TestCaseWithFactory
	{
		#region TestWorkflowCustomFields

		[RequiresSTA]
		public void TestWorkflowCustomFields()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrderLineWorkflowDescriptorCode;
			template.P0_Name = "WF Order Line Test Workflow";

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Type = MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Integer;
			column.XC_Name = "WF Custom Field";

			Factory.Save();

			var collection = new OrderLineCollection(Factory);
			var filterBusinessObject = new OrderLineFilterBusinessObject();

			using (var filterControl = new OrderLineFilterControl(collection, filterBusinessObject))
			{
				filterControl.Show();
				var columnStyle = filterControl.Grid.GetColumnStyle("__WF CUSTOM FIELD__prop__ZInt");
				AssertNotNull(columnStyle);
				AssertEquals("Caption", "WF Custom Field", columnStyle.Caption);
				AssertEquals("Visibility", false, columnStyle.IsVisible);
				AssertEquals("ReadOnly", true, columnStyle.IsReadOnly);
			}
		}

		[RequiresSTA]
		public void TestQuantityBookedAndOpenQuantity_WhenEnableAdvOrmFeatureIsOn_ColumnsExist()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var collection = new OrderLineCollection(Factory);

				using (var filterControl = new OrderLineFilterControl(collection, null))
				{
					var qtyBookedColumnStyle = filterControl.Grid.GetColumnStyle("JO_QtyBooked");
					var qtyOpenColumnStyle = filterControl.Grid.GetColumnStyle("JO_OpenQuantity");

					CombineAssertions(() =>
					{
						AssertNotNull(qtyBookedColumnStyle);
						AssertNotNull(qtyOpenColumnStyle);
					});
				}
			});
		}

		[RequiresSTA]
		public void TestQuantityBookedAndOpenQuantity_WhenEnableAdvOrmFeatureIsOff_ColumnsDoNotExist()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var collection = new OrderLineCollection(Factory);

				using (var filterControl = new OrderLineFilterControl(collection, null))
				{
					var qtyBookedColumnStyle = filterControl.Grid.GetColumnStyle("JO_QtyBooked");
					var qtyOpenColumnStyle = filterControl.Grid.GetColumnStyle("JO_OpenQuantity");

					CombineAssertions(() =>
					{
						AssertNull(qtyBookedColumnStyle);
						AssertNull(qtyOpenColumnStyle);
					});
				}
			});
		}

		#endregion

		#region TestSerialNumberColumn

		[RequiresSTA]
		public void TestSerialNumberColumn()
		{
			var collection = new OrderLineCollection(Factory);
			var filterBusinessObject = new OrderLineFilterBusinessObject();
			using (var filterControl = new OrderLineFilterControl(collection, filterBusinessObject))
			{
				filterControl.Show();
				var columnStyle = filterControl.Grid.GetColumnStyle(JobOrderLineSchema.JO_SerialNumber.Name);
				AssertNotNull("Serial Number column should exist", columnStyle);
				AssertEquals("Visibility", false, columnStyle.IsVisible);
			}
		}

		#endregion
	}

	public class OrderLineFilterControlBashFetchTest : FilterControlBashFetchHintTest<OrderLine>
	{
		#region BashFetchTest

		public void TestBashFetchForView_JO_QtyBooked()
		{
			BashFetchForView("JO_QtyBooked", 1);
		}

		public void TestBashFetchForView_JO_OpenQuantity()
		{
			BashFetchForView("JO_OpenQuantity", 1);
		}

		public void TestBashFetchForView_JO_LineReference()
		{
			BashFetchForView("JO_LineReference", 1);
		}

		public void TestBashFetchForView_GoodsAvailableAtNameOrPK()
		{
			BashFetchForView("GoodsAvailableAtNameOrPK", 12);
		}

		public void TestBashFetchForView_GoodsAvailableAtAddress_E2_Address1()
		{
			BashFetchForView("GoodsAvailableAtAddress.E2_Address1", 12);
		}

		public void TestBashFetchForView_GoodsDeliveredToNameOrPK()
		{
			BashFetchForView("GoodsDeliveredToNameOrPK", 12);
		}

		public void TestBashFetchForView_GoodsDeliveredToAddress_E2_Address1()
		{
			BashFetchForView("GoodsDeliveredToAddress.E2_Address1", 12);
		}

		public void TestBashFetchForView_Order_JD_OrderNumberAndSplit()
		{
			// JobOrderHeader: 1

			BashFetchForView("Order+JD_OrderNumberAndSplit", 1);
		}

		public void TestBashFetchForView_JO_LineNo()
		{
			BashFetchForView("JO_LineNo", 0);
		}

		public void TestBashFetchForView_Order_JD_Calc_SupplierCode()
		{
			// JobOrderHeader: 1

			BashFetchForView("Order+JD_Calc_SupplierCode", 1);
		}

		public void TestBashFetchForView_JO_LineSplitNumber()
		{
			BashFetchForView("JO_LineSplitNumber", 0);
		}

		public void TestBashFetchForView_Order_JD_EF_ShipmentPrePlanning()
		{
			// JobOrderHeader: 1

			BashFetchForView("Order+JD_EF_ShipmentPrePlanning", 1);
		}

		public void TestBashFetchForView_JO_ActualWeight()
		{
			BashFetchForView("JO_ActualWeight", 0);
		}

		public void TestBashFetchForView_JO_ActualVolume()
		{
			BashFetchForView("JO_ActualVolume", 0);
		}

		public void TestBashFetchForView_JO_AdditionalInformation()
		{
			BashFetchForView("JO_AdditionalInformation", 0);
		}

		public void TestBashFetchForView_JO_Calc_OrderLineNoAndSubLineNo()
		{
			// JobOrderHeader: 1
			// JobOrderLine: 1

			BashFetchForView("JO_Calc_OrderLineNoAndSubLineNo", 2);
		}

		public void TestBashFetchForView_JO_CommercialInvoiceNo()
		{
			BashFetchForView("JO_CommercialInvoiceNo", 0);
		}

		public void TestBashFetchForView_JO_ContainerNumber()
		{
			BashFetchForView("JO_ContainerNumber", 0);
		}

		public void TestBashFetchForView_JO_ContainerPackingOrder()
		{
			BashFetchForView("JO_ContainerPackingOrder", 0);
		}

		public void TestBashFetchForView_JO_ContainersVisible()
		{
			// JobOrderHeader: 1
			// OrgAddress: 1

			BashFetchForView("JO_ContainersVisible", 2);
		}

		public void TestBashFetchForView_JO_CustomAttrib1()
		{
			BashFetchForView("JO_CustomAttrib1", 0);
		}

		public void TestBashFetchForView_JO_CustomAttrib2()
		{
			BashFetchForView("JO_CustomAttrib2", 0);
		}

		public void TestBashFetchForView_JO_CustomAttrib3()
		{
			BashFetchForView("JO_CustomAttrib3", 0);
		}

		public void TestBashFetchForView_JO_CustomAttrib4()
		{
			BashFetchForView("JO_CustomAttrib4", 0);
		}

		public void TestBashFetchForView_JO_CustomAttrib5()
		{
			BashFetchForView("JO_CustomAttrib5", 0);
		}

		public void TestBashFetchForView_JO_CustomAttrib6()
		{
			BashFetchForView("JO_CustomAttrib6", 0);
		}

		public void TestBashFetchForView_JO_CustomDate1()
		{
			BashFetchForView("JO_CustomDate1", 0);
		}

		public void TestBashFetchForView_JO_CustomDate2()
		{
			BashFetchForView("JO_CustomDate2", 0);
		}

		public void TestBashFetchForView_JO_CustomDate3()
		{
			BashFetchForView("JO_CustomDate3", 0);
		}

		public void TestBashFetchForView_JO_CustomDate4()
		{
			BashFetchForView("JO_CustomDate4", 0);
		}

		public void TestBashFetchForView_JO_CustomDate5()
		{
			BashFetchForView("JO_CustomDate5", 0);
		}

		public void TestBashFetchForView_JO_CustomDecimal1()
		{
			BashFetchForView("JO_CustomDecimal1", 0);
		}

		public void TestBashFetchForView_JO_CustomDecimal2()
		{
			BashFetchForView("JO_CustomDecimal2", 0);
		}

		public void TestBashFetchForView_JO_CustomDecimal3()
		{
			BashFetchForView("JO_CustomDecimal3", 0);
		}

		public void TestBashFetchForView_JO_CustomDecimal4()
		{
			BashFetchForView("JO_CustomDecimal4", 0);
		}

		public void TestBashFetchForView_JO_CustomDecimal5()
		{
			BashFetchForView("JO_CustomDecimal5", 0);
		}

		public void TestBashFetchForView_JO_CustomFlag1()
		{
			BashFetchForView("JO_CustomFlag1", 0);
		}

		public void TestBashFetchForView_JO_CustomFlag2()
		{
			BashFetchForView("JO_CustomFlag2", 0);
		}

		public void TestBashFetchForView_JO_CustomFlag3()
		{
			BashFetchForView("JO_CustomFlag3", 0);
		}

		public void TestBashFetchForView_JO_CustomFlag4()
		{
			BashFetchForView("JO_CustomFlag4", 0);
		}

		public void TestBashFetchForView_JO_CustomFlag5()
		{
			BashFetchForView("JO_CustomFlag5", 0);
		}

		public void TestBashFetchForView_JO_CustomTextBlob1()
		{
			BashFetchForView("JO_CustomTextBlob1", 0);
		}

		public void TestBashFetchForView_JO_Description()
		{
			BashFetchForView("JO_Description", 0);
		}

		public void TestBashFetchForView_JO_InnerPacks()
		{
			BashFetchForView("JO_InnerPacks", 0);
		}

		public void TestBashFetchForView_JO_InnerPacksUQ()
		{
			BashFetchForView("JO_InnerPacksUQ", 0);
		}

		public void TestBashFetchForView_JO_ItemPrice()
		{
			BashFetchForView("JO_ItemPrice", 0);
		}

		public void TestBashFetchForView_JO_LineDropDate()
		{
			BashFetchForView("JO_LineDropDate", 0);
		}

		public void TestBashFetchForView_JO_LinePrice()
		{
			BashFetchForView("JO_LinePrice", 0);
		}

		public void TestBashFetchForView_JO_LineStatus()
		{
			BashFetchForView("JO_LineStatus", 0);
		}

		public void TestBashFetchForView_JO_F3_NKPackType()
		{
			BashFetchForView("JO_F3_NKPackType", 0);
		}

		public void TestBashFetchForView_JO_OuterPacks()
		{
			BashFetchForView("JO_OuterPacks", 0);
		}

		public void TestBashFetchForView_JO_OuterPacksUQ()
		{
			BashFetchForView("JO_OuterPacksUQ", 0);
		}

		public void TestBashFetchForView_JO_PartAttrib1()
		{
			BashFetchForView("JO_PartAttrib1", 0);
		}

		public void TestBashFetchForView_JO_PartAttrib2()
		{
			BashFetchForView("JO_PartAttrib2", 0);
		}

		public void TestBashFetchForView_JO_PartAttrib3()
		{
			BashFetchForView("JO_PartAttrib3", 0);
		}

		public void TestBashFetchForView_JO_SerialNumber()
		{
			BashFetchForView("JO_SerialNumber", 0);
		}

		public void TestBashFetchForView_JO_Partno()
		{
			BashFetchForView("JO_Partno", 0);
		}

		public void TestBashFetchForView_JO_QtyInvoiced()
		{
			BashFetchForView("JO_QtyInvoiced", 0);
		}

		public void TestBashFetchForView_JO_QtyReceived()
		{
			BashFetchForView("JO_QtyReceived", 0);
		}

		public void TestBashFetchForView_JO_Quantity()
		{
			BashFetchForView("JO_Quantity", 0);
		}

		public void TestBashFetchForView_JO_QuantityRemaining()
		{
			BashFetchForView("JO_QuantityRemaining", 0);
		}

		public void TestBashFetchForView_JO_RN_NKCountryOfOrigin()
		{
			BashFetchForView("JO_RN_NKCountryOfOrigin", 0);
		}

		public void TestBashFetchForView_JO_SpecialInstructions()
		{
			BashFetchForView("JO_SpecialInstructions", 0);
		}

		public void TestBashFetchForView_JO_SubLineNo()
		{
			BashFetchForView("JO_SubLineNo", 0);
		}

		public void TestBashFetchForView_JO_TotalInnerPacks()
		{
			BashFetchForView("JO_TotalInnerPacks", 0);
		}

		public void TestBashFetchForView_JO_UnitOfVolume()
		{
			BashFetchForView("JO_UnitOfVolume", 0);
		}

		public void TestBashFetchForView_JO_UnitOfWeight()
		{
			BashFetchForView("JO_UnitOfWeight", 0);
		}

		public void TestBashFetchForView_Order_JD_CalcShipmentBrokerageNumber()
		{
			// JobOrderHeader: 1

			BashFetchForView("Order+JD_CalcShipmentBrokerageNumber", 1);
		}

		public void TestBashFetchForView_JO_INCO()
		{
			BashFetchForView("JO_INCO", 0);
		}

		public void TestBashFetchForView_JO_AdditionalTerms()
		{
			BashFetchForView("JO_AdditionalTerms", 0);
		}

		public void TestBashFetchForView_UNDGs_UNDGSubstanceManager_Value()
		{
			// UNDGDataItem: 1

			BashFetchForView("UNDGs+UNDGSubstanceManagerGuid+Value", 1);
		}

		public void TestBashFetchForView_UNDGs_UNDGFlashPointManager_Value()
		{
			// UNDGDataItem: 1

			BashFetchForView("UNDGs+UNDGFlashPointManager+Value", 1);
		}

		public void TestBashFetchForView_UNDGs_UNDGContactManager_Value()
		{
			// UNDGDataItem: 1

			BashFetchForView("UNDGs+UNDGContactManager+Value", 1);
		}

		public void TestBashFetchForView_JO_ConfirmationNum()
		{
			BashFetchForView("JO_ConfirmationNum", 0);
		}

		public void TestBashFetchForView_JO_ConfirmationDate()
		{
			BashFetchForView("JO_ConfirmationDate", 0);
		}

		public void TestBashFetchForView_JO_ExWorksDate()
		{
			BashFetchForView("JO_ExWorksDate", 0);
		}

		public void TestBashFetchForView_Order_Carrier_OH_Code()
		{
			// JobOrderHeader: 1
			// OrgHeader: 1

			BashFetchForView("Order+Carrier+OH_Code", 2);
		}

		public void TestBashFetchForView_Order_JD_OrderNumberSplit()
		{
			// JobOrderHeader: 1

			BashFetchForView("Order+JD_OrderNumberSplit", 1);
		}

		public void TestBashFetchForView_JO_SystemCreateUser()
		{
			BashFetchForView("JO_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_JO_SystemCreateBranch()
		{
			BashFetchForView("JO_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_JO_SystemCreateDepartment()
		{
			BashFetchForView("JO_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_JO_SystemCreateTimeUtc()
		{
			BashFetchForView("JO_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_JO_SystemLastEditUser()
		{
			BashFetchForView("JO_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_JO_SystemLastEditTimeUtc()
		{
			BashFetchForView("JO_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_JO_ShipmentWindowStart()
		{
			BashFetchForView("JO_ShipmentWindowStart", 0);
		}

		public void TestBashFetchForView_JO_ShipmentWindowEnd()
		{
			BashFetchForView("JO_ShipmentWindowEnd", 0);
		}

		public void TestBashFetchForView_JO_HSCode()
		{
			BashFetchForView("JO_HSCode", 0);
		}

		public void TestBashFetchForView_JO_RH_NKCommodityCode()
		{
			BashFetchForView("JO_RH_NKCommodityCode", 0);
		}

		#endregion

		protected override SchemaPKColumn PkColumn => JobOrderLineSchema.PK;

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			var carrier = factory.NewWithValidTestData<OrgHeader>();
			carrier.FillWithValidTestData();

			Factory.Save();

			for (var i = 0; i < 12; i++)
			{
				var order = factory.New<Order>();
				order.FillWithValidTestData();

				order.JD_OH_Carrier = carrier.PK;

				var line = order.OrderLines.AddNew();
				line.FillWithValidTestData();

				var undg = line.UNDGs.AddNew();
				undg.FillWithValidTestData();

				result.Add(line.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = new OrderLineCollection(Factory);
			var filterBusinessObject = new OrderLineFilterBusinessObject();
			return new OrderLineFilterControl(collection, filterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new OrderLineCollection(Factory);
		}
	}
}
