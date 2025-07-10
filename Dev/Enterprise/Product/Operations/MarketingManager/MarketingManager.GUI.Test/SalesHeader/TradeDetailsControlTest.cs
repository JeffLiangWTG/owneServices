using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeDetailsControlTest : TestCaseWithFactory
	{
		#region Static / Constructors

		public void TestNew()
		{
			AssertTradeDetailsControlType(SystemDefinedSalesProductList.Codes.CustomsBrokerage, typeof(CustomsBrokerageTradeDetailsControl));
			AssertTradeDetailsControlType(SystemDefinedSalesProductList.Codes.ForwardingShipment, typeof(ForwardingShipmentTradeDetailsControl));
			AssertTradeDetailsControlType(SystemDefinedSalesProductList.Codes.LinerAgency, typeof(JobCommonTradeDetailsControl));
			AssertTradeDetailsControlType(SystemDefinedSalesProductList.Codes.Transport, typeof(TransportTradeDetailsControl));
			AssertTradeDetailsControlType(SystemDefinedSalesProductList.Codes.Warehouse, typeof(WarehouseTradeDetailsControl));
			AssertTradeDetailsControlType("ENT", typeof(GenericTradeDetailsControl));
			AssertTradeDetailsControlType("", typeof(GenericTradeDetailsControl));
		}

		void AssertTradeDetailsControlType(ZString productCode, Type expectedControlType)
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			salesProduct.MP_Code = productCode;
			using (var control = TradeDetailsControl.New(salesProduct))
			{
				AssertType(expectedControlType, control);
			}
		}

		#endregion

		#region Trade Details Grid

		public void TestTradeDetailsGrid_WarehouseDelete()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = warehouseProduct.PK;
			var entitySales = EntitySalesWrapper.Get(sales, opp1);
			var tradeDetailA = entitySales.EntityTradeDetailsCollection.AddNew();
			Factory.Save();

			var pivot1A = tradeDetailA.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => (OrgOpportunity)x.AssociatedEntity == opp1);
			AssertNotNull("Precondition", pivot1A);
			AssertEquals("Precondition", 1, tradeDetailA.SalesAssociationPivotCollectionGlobal.Count);

			using (var form = new TradeDetailsControlFormForTest(entitySales))
			using (var control = new TradeDetailsControlForTest(warehouseProduct))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeDetailsGrid.SelectSingleElement(tradeDetailA);
				control.TradeDetailsGrid.DeleteMenuItem.PerformClick();
				AssertEquals(true, tradeDetailA.IsDeleted);
				AssertEquals(true, pivot1A.IsDeleted);
			}

			var tradeDetailB = entitySales.EntityTradeDetailsCollection.AddNew();
			Factory.Save();

			var pivot1B = tradeDetailB.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_TradeId == tradeDetailB.PK && (OrgOpportunity)x.AssociatedEntity == opp1);
			AssertNotNull("Precondition", pivot1B);
			AssertEquals("Precondition", 1, tradeDetailB.SalesAssociationPivotCollectionGlobal.Count);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opp2, tradeDetailB);
			Factory.Save();

			AssertEquals("Precondition", 2, tradeDetailB.SalesAssociationPivotCollectionGlobal.Count);
			var pivot2B = tradeDetailB.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_TradeId == tradeDetailB.PK && (OrgOpportunity)x.AssociatedEntity == opp2);
			AssertNotNull("Precondition", pivot2B);

			using (var form = new TradeDetailsControlFormForTest(entitySales))
			using (var control = new TradeDetailsControlForTest(warehouseProduct))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeDetailsGrid.SelectSingleElement(tradeDetailB);
				control.TradeDetailsGrid.DeleteMenuItem.PerformClick();
				AssertEquals(true, tradeDetailB.IsDeleted);
				AssertEquals(true, pivot1B.IsDeleted);
				AssertEquals(true, pivot2B.IsDeleted);
			}
		}

		public void TestTradeDetailsGrid_AddNewShouldSetTradeStatus()
		{
			var defaultValue = new OpportunityStatusCollection(defaultBoolForNewChild: false, defaultEffectiveAgreementForNewChild: false);
			defaultValue.Add("CRT", (NoResString)"Current", effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
			defaultValue.Add("WON", (NoResString)"Won", effectiveAgreement: true, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Successful);

			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = warehouseProduct.PK;

			opp1.P8_Status = "WON";

			var entitySales = EntitySalesWrapper.Get(sales, opp1);
			var tradeDetailA = entitySales.EntityTradeDetailsCollection.AddNew();
			Factory.Save();

			var pivot1A = tradeDetailA.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => (OrgOpportunity)x.AssociatedEntity == opp1);
			AssertNotNull("Precondition", pivot1A);
			AssertEquals("Precondition", 1, tradeDetailA.SalesAssociationPivotCollectionGlobal.Count);

			using (var form = new TradeDetailsControlFormForTest(entitySales))
			using (var control = new TradeDetailsControlForTest(warehouseProduct))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeDetailsGrid.SelectSingleElement(tradeDetailA);
				var uncommittedDetail = control.TradeDetailsGrid.List.AddNew();
				AssertEquals(OpportunityTradeStatus.Codes.Successful, ((EntityTradeDetailWrapper)uncommittedDetail).PA_Status);
			}
		}

		public void TestTradeDetailsGrid_NoGrid()
		{
			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			var layout = salesProduct.FormLayout;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			var salesWrapper = EntitySalesWrapper.Get(sales, opp);
			var columnDef = layout.TradeDetailCustomColumnDefinitionCollection.AddNew();
			columnDef.XC_Name = "CustomStr";
			columnDef.XC_Type = AddOnColumnDataType.Codes.String;

			var gridColumn = layout.TradeDetailGridColumnDefinitions.AddNew();
			gridColumn.GenCustomColumnDefinitionFk = columnDef.PK;
			Factory.Save();

			using (var form = new TradeDetailsControlFormForTest(salesWrapper))
			using (var control = new TradeDetailsControlNoGridForTest(salesProduct))
			{
				AssertNoExceptionThrown("", () =>
				{
					form.Controls.Add(control);
					form.Show();
				});
			}
		}
		#endregion

		#region Sales Matching

		public void TestShowSalesMatchingFormOnWarehouseProductChanged()
		{
			var warehouse = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var supplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(warehouse);
			var sales = salesHeader.EntitySalesCollectionProductView.AddNew();
			var tradeDetail = sales.EntityTradeDetailsCollection.AddNew();
			tradeDetail.PA_OP = supplierPart.PK;
			Factory.Save();

			org.SalesCollection.Load();

			using (var form = new ZForm(sales))
			using (var control = new TradeDetailsControlForTest(warehouse))
			{
				form.Controls.Add(control);
				form.Show();

				var tradeDetail2 = sales.EntityTradeDetailsCollection.AddNew();
				tradeDetail2.PA_OP = supplierPart.PK;

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertType(typeof(SalesMatchingForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#endregion

		#region Columns

		public void TestCustomColumns()
		{
			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			var layout = salesProduct.FormLayout;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			var salesWrapper = EntitySalesWrapper.Get(sales, opp);
			var sales2 = org.SalesCollection.AddNew();
			var salesWrapper2 = EntitySalesWrapper.Get(sales2, opp);

			var columnDef1 = layout.TradeDetailCustomColumnDefinitionCollection.AddNew();
			columnDef1.XC_Name = "CustomStr";
			columnDef1.XC_Type = AddOnColumnDataType.Codes.String;

			var columnDef2 = layout.TradeDetailCustomColumnDefinitionCollection.AddNew();
			columnDef2.XC_Name = "CustomInt";
			columnDef2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var columnDef3 = layout.TradeDetailCustomColumnDefinitionCollection.AddNew();
			columnDef3.XC_Name = "AnotherCustomStr";
			columnDef3.XC_Type = AddOnColumnDataType.Codes.String;

			var gridColumn1 = layout.TradeDetailGridColumnDefinitions.AddNew();
			gridColumn1.GenCustomColumnDefinitionFk = columnDef1.PK;

			var gridColumn2 = layout.TradeDetailGridColumnDefinitions.AddNew();
			gridColumn2.GenCustomColumnDefinitionFk = columnDef2.PK;

			var gridColumn3 = layout.TradeDetailGridColumnDefinitions.AddNew();
			gridColumn3.GenCustomColumnDefinitionFk = columnDef3.PK;

			Factory.Save();

			using (var form = new TradeDetailsControlFormForTest(salesWrapper))
			using (var control = new TradeDetailsControlForTest(salesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				var resultColumns = control.TradeDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("Custom string column should be added", resultColumns.Any(c => c.Caption == "CustomStr"));
				Assert("Custom int column should be added", resultColumns.Any(c => c.Caption == "CustomInt"));

				var strDescriptor = (IOverridablePropertyDescriptor)resultColumns.First(c => c.Caption == "CustomStr");
				var strRetriever = strDescriptor.PropertyDescriptor as IZPropertyInfoRetriever;
				layout.TradeDetailCustomColumnDefinitionCollection.Remove(columnDef1);
				Factory.Save();
				AssertNoExceptionThrown("Should still be able to use custom property that was deleted after loading control", () => _ = strRetriever.GetZPropertyInfo(salesWrapper));

				var anotherStrDescriptor = (IOverridablePropertyDescriptor)resultColumns.First(c => c.Caption == "AnotherCustomStr");
				var anotherStrRetriever = anotherStrDescriptor.PropertyDescriptor as IZPropertyInfoRetriever;
				columnDef3.XC_Name = "YetAnotherCustomStr";
				Factory.Save();
				AssertNoExceptionThrown("Should still be able to use custom property that was modified after loading control", () => _ = anotherStrRetriever.GetZPropertyInfo(salesWrapper));

				var columnDef4 = layout.TradeDetailCustomColumnDefinitionCollection.AddNew();
				columnDef4.XC_Name = "DuplicateCustomStr";
				columnDef4.XC_Type = AddOnColumnDataType.Codes.String;
				var gridColumn4 = layout.TradeDetailGridColumnDefinitions.AddNew();
				gridColumn4.GenCustomColumnDefinitionFk = columnDef4.PK;

				Factory.Save();

				control.SetDataBinding(salesWrapper2, null);
				resultColumns = control.TradeDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals("Duplicate Custom string column should not be added", false, resultColumns.Any(c => c.Caption == "DuplicateCustomStr"));
			}
		}

		#endregion

	}
}
