using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeLanesControlTest : TestCaseWithFactory
	{
		#region Static / Constructors

		public void TestNew()
		{
			AssertTradeLanesControlType(SystemDefinedSalesProductList.Codes.CustomsBrokerage, typeof(CustomsBrokerageTradeLanesControl));
			AssertTradeLanesControlType(SystemDefinedSalesProductList.Codes.ForwardingShipment, typeof(ForwardingShipmentTradeLanesControl));
			AssertTradeLanesControlType(SystemDefinedSalesProductList.Codes.LinerAgency, typeof(LinerAgencyTradeLanesControl));
			AssertTradeLanesControlType(SystemDefinedSalesProductList.Codes.Transport, typeof(TransportTradeLanesControl));
			AssertTradeLanesControlType(SystemDefinedSalesProductList.Codes.Warehouse, typeof(WarehouseTradeLanesControl));
			AssertTradeLanesControlType("ENT", typeof(GenericTradeLanesControl));
			AssertTradeLanesControlType("", typeof(GenericTradeLanesControl));
		}

		void AssertTradeLanesControlType(ZString productCode, Type expectedControlType)
		{
			var product = Factory.New<OrgSalesProduct>();
			product.MP_Code = productCode;
			using (var control = TradeLanesControl.New(product))
			{
				AssertType(expectedControlType, control);
			}
		}

		#endregion

		#region Sales Matching

		public void TestShowSalesMatchingFormOnOriginDestinationChanged()
		{
			var shipments = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sale1 = org.SalesCollection.AddNew();
			sale1.OW_MP_Product = shipments.PK;
			sale1.OW_OH_Buyer = org.PK;
			sale1.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sale1.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix).PK;
			Factory.Save();

			var header = new SalesHeader(org, shipments);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(shipments))
			{
				form.Controls.Add(control);
				form.Show();

				var sale2 = header.FilterableEntitySalesCollection.AddNew();
				sale2.OW_MP_Product = shipments.PK;
				sale2.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
				sale2.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix).PK;

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertType(typeof(SalesMatchingForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestOnSalesMatchingPropertyChangedWarehouseTradeDetailWithoutProductShouldNotMatch()
		{
			var warehouseSalesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var sydBranch = Factory.NewWithValidTestData<GlbBranch>();
			sydBranch.GB_RL_NKHomePort = "AUSYD";
			var sydWarehouse = Factory.New<IWhsWarehouse>();
			sydWarehouse.WW_GB_RelatedCompanyBranch = sydBranch.PK;
			((BusinessObject)sydWarehouse).FillWithValidTestData();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sale1 = org.SalesCollection.AddNew();
			sale1.OW_MP_Product = warehouseSalesProduct.PK;
			sale1.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			sale1.OW_WW = sydWarehouse.PK;
			sale1.TradeDetails.AddNew();
			Factory.Save();

			var header = new SalesHeader(org, warehouseSalesProduct);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(warehouseSalesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				var sale2 = header.FilterableEntitySalesCollection.AddNew();
				sale2.OW_MP_Product = warehouseSalesProduct.PK;
				sale2.TradeDetails.AddNew();
				sale2.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
				sale2.OW_WW = sydWarehouse.PK;

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestOnSalesMatchingPropertyChangedWarehouseTradeDetailWithProductShouldMatch()
		{
			var warehouseSalesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var sydBranch = Factory.NewWithValidTestData<GlbBranch>();
			sydBranch.GB_RL_NKHomePort = "AUSYD";
			var sydWarehouse = Factory.New<IWhsWarehouse>();
			sydWarehouse.WW_GB_RelatedCompanyBranch = sydBranch.PK;
			((BusinessObject)sydWarehouse).FillWithValidTestData();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sale1 = org.SalesCollection.AddNew();
			sale1.OW_MP_Product = warehouseSalesProduct.PK;
			sale1.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			sale1.OW_WW = sydWarehouse.PK;
			var detail1 = sale1.TradeDetails.AddNew();
			detail1.PA_OP = part.PK;
			Factory.Save();

			var header = new SalesHeader(org, warehouseSalesProduct);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(warehouseSalesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				var sale2 = header.FilterableEntitySalesCollection.AddNew();
				sale2.OW_MP_Product = warehouseSalesProduct.PK;
				var detail2 = sale2.TradeDetails.AddNew();
				detail2.PA_OP = part.PK;
				sale2.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
				sale2.OW_WW = sydWarehouse.PK;

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertType(typeof(SalesMatchingForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#endregion

		#region ShowActualsIfSupported

		public void TestShowActualsIfSupported()
		{
			var shipment = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			AssertEquals("Precondition", true, shipment.IsActualsSupported);
			using (var control = new ForwardingShipmentTradeLanesControl(shipment))
			{
				control.ShowActualsIfSupported();

				using (var template = new TradeLanesControlActualColumnsTemplate())
				{
					foreach (var templateColumn in template.PreColumnsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Union(template.PostColumnsGrid.ColumnStyles.Cast<ZGridColumnInfo>()))
					{
						AssertCollectionContains(string.Format("Should have added {0} column from template", templateColumn.ColumnName),
							templateColumn.ColumnName,
							control.TradeLanesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
					}
				}
			}

			var customProduct = Factory.New<OrgSalesProduct>();
			AssertEquals("Precondition", false, customProduct.IsActualsSupported);
			using (var control = new GenericTradeLanesControl(customProduct))
			{
				control.ShowActualsIfSupported();

				using (var template = new TradeLanesControlActualColumnsTemplate())
				{
					foreach (var templateColumn in template.PreColumnsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Union(template.PostColumnsGrid.ColumnStyles.Cast<ZGridColumnInfo>()))
					{
						AssertCollectionNotContains(string.Format("Should not add {0} column from template as sales product does not support actuals", templateColumn.ColumnName),
							templateColumn.ColumnName,
							control.TradeLanesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
					}
				}
			}
		}

		[TestDate(2011, 1, 1)]
		public void TestMarkAsProspectMenuItem()
		{
			var shipment = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			AssertEquals("Precondition", true, shipment.IsActualsSupported);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(shipment);
			var sales = salesHeader.EntitySalesCollectionProductView.AddNew();

			Factory.Save();

			using (var form = new ZForm(salesHeader))
			using (var control = new ForwardingShipmentTradeLanesControl(shipment))
			{
				form.Controls.Add(control);
				control.ShowActualsIfSupported();
				salesHeader.CompanyFilter = Env.CurrentCompany.PK;
				form.Show();

				var markAsProspectiveMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Mark as PRS (Prospective)");
				AssertNotNull("Menu Item should have been added", markAsProspectiveMenuItem);

				AssertNotEquals("Precondition", OrgSalesActualsStatusList.Codes.Lost, sales.ActualsInformation.Status);
				markAsProspectiveMenuItem.PerformClick();
				AssertEquals("Cannot mark as prospective", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("You can only mark lost trade lanes as prospective.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDate.Empty, sales.OW_LatestProspectDate);

				var veryOldTraded = Factory.New<OrgSales>();
				var veryOldDetail = veryOldTraded.TradeDetails.AddNew();
				var veryOldPeriod = veryOldDetail.TradedPeriods.AddNew();
				veryOldPeriod.PAS_LastTraded = new ZDate(2000, 1, 1);
				veryOldPeriod.PAS_Period = new ZDate(2000, 1, 1);
				sales.ActualsInformation.SetActuals(new[] { veryOldTraded });
				AssertEquals("Precondition", OrgSalesActualsStatusList.Codes.Lost, sales.ActualsInformation.Status);
				UnitTestUserNotification.Instance.ClearMessages();
				markAsProspectiveMenuItem.PerformClick();
				AssertEquals(new ZDate(2011, 1, 1), sales.OW_LatestProspectDate);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region Detach Menu

		public void TestDetachTradeLane()
		{
			var shipments = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = shipments.PK;
			sales.OW_OH_Buyer = org.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix).PK;
			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales);
			Factory.Save();

			var header = new SalesHeader(org, opp1, shipments);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(shipments))
			{
				form.Controls.Add(control);
				form.Show();
				control.TradeLanesGrid.SelectSingleElement(sales);
				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertNotNull("Menu item should have been added", detachMenuItem);
				AssertEquals("Menu item should be enabled if multiple associations exist", true, detachMenuItem.Enabled);
				detachMenuItem.PerformClick();
				header.Factory.Save();
			}

			var anotherFactory = new BusinessObjectFactory();
			var loadedSales = anotherFactory.Load<OrgSales>(sales.PK);
			AssertNotNull(loadedSales);
			AssertEquals(1, loadedSales.SalesAssociationPivotCollectionGlobal.Count);

			header = new SalesHeader(org, opp2, shipments);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(shipments))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertEquals("Menu item should be disabled if only one association exists", false, detachMenuItem.Enabled);
			}
		}

		public void TestDetachTradeLaneShouldDeleteDetails()
		{
			var shipments = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = shipments.PK;
			sales.OW_OH_Buyer = org.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix).PK;
			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var salesWrapper = EntitySalesWrapper.Get(sales, opp1);
			var detail = salesWrapper.EntityTradeDetailsCollection.AddNew();
			salesWrapper.EntityDetailGroupings.AddNew();
			Factory.Save();

			var header = new SalesHeader(org, opp1, shipments);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(shipments))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeLanesGrid.SelectSingleElement(salesWrapper);
				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertNotNull("Menu item should have been added", detachMenuItem);
				AssertEquals("Menu item should be enabled if multiple associations exist", true, detachMenuItem.Enabled);
				detachMenuItem.PerformClick();
				header.Factory.Save();

				AssertEquals("Detaching sales should delete detail", true, detail.IsDeleted);
			}
		}

		public void TestDetachTradeLaneShouldNotRemoveOtherSalesDetails()
		{
			var shipments = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = shipments.PK;
			sales.OW_OH_Buyer = org.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix).PK;
			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = shipments.PK;
			sales2.OW_OH_Buyer = org.PK;
			sales2.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales2.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "USLAX", RefUNLOCOSchema.Constants.Prefix).PK;
			var pivot1 = opp1.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var pivot2 = opp1.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			var pivot1a = opp2.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var salesWrapper = EntitySalesWrapper.Get(sales, opp1);
			var salesWrapper2 = EntitySalesWrapper.Get(sales2, opp1);
			var detail = salesWrapper.EntityTradeDetailsCollection.AddNew();
			var detail2 = salesWrapper2.EntityTradeDetailsCollection.AddNew();
			var grouping = salesWrapper.EntityDetailGroupings.AddNew();
			var grouping2 = salesWrapper2.EntityDetailGroupings.AddNew();
			Factory.Save();

			var header = new SalesHeader(org, opp1, shipments);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(shipments))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeLanesGrid.SelectSingleElement(salesWrapper);
				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertNotNull("Menu item should have been added", detachMenuItem);
				AssertEquals("Menu item should be enabled if multiple associations exist", true, detachMenuItem.Enabled);
				detachMenuItem.PerformClick();
				header.Factory.Save();

				AssertEquals("Detaching sales should delete detail", true, detail.IsDeleted);
				AssertEquals("Detaching sales should delete related pivot", true, pivot1.IsDeleted);
				AssertEquals("Detaching sales should not delete unrelated pivot", false, pivot1a.IsDeleted);
				AssertEquals("Detaching sales should not delete grouping for other sales", false, grouping2.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, detail2.IsDeleted);
				AssertEquals("Detaching sales should not delete pivot for other sales", false, pivot2.IsDeleted);
			}
		}

		[TestDate(2020, 6, 25, 1, 2, 0)]
		public void TestDetachTradeLaneWhenRowNotSelected()
		{
			var shipments = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = shipments.PK;
			sales.OW_OH_Buyer = org.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix).PK;
			Factory.Save();

			TestDateAttribute.AddMinutes(1);
			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = shipments.PK;
			sales2.OW_OH_Buyer = org.PK;
			sales2.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales2.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "USLAX", RefUNLOCOSchema.Constants.Prefix).PK;
			var pivot1 = opp1.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var pivot2 = opp1.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			var pivot1a = opp2.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var salesWrapper = EntitySalesWrapper.Get(sales, opp1);
			var salesWrapper2 = EntitySalesWrapper.Get(sales2, opp1);
			var detail = salesWrapper.EntityTradeDetailsCollection.AddNew();
			var detail2 = salesWrapper2.EntityTradeDetailsCollection.AddNew();
			var grouping = salesWrapper.EntityDetailGroupings.AddNew();
			var grouping2 = salesWrapper2.EntityDetailGroupings.AddNew();
			Factory.Save();

			var header = new SalesHeader(org, opp1, shipments);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(shipments))
			{
				form.Controls.Add(control);
				form.Show();

				var property = TypeDescriptor.GetProperties(typeof(OrgSales))[OrgSalesSchema.Constants.OW_SystemCreateTimeUtc];
				control.TradeLanesGrid.List.ApplySort(property, ListSortDirection.Ascending);
				control.TradeLanesGrid.ListManager.Position = 0;
				AssertEquals(0, control.TradeLanesGrid.GetSelectedRows().Length);
				AssertEquals(sales.PK, ((EntitySalesWrapper)control.TradeLanesGrid.ListManager.Current).PK);
				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertNotNull("Menu item should have been added", detachMenuItem);
				AssertEquals("Menu item should be enabled if multiple associations exist", true, detachMenuItem.Enabled);
				detachMenuItem.PerformClick();
				header.Factory.Save();

				AssertEquals("Detaching sales should delete detail", true, detail.IsDeleted);
				AssertEquals("Detaching sales should delete related pivot", true, pivot1.IsDeleted);
				AssertEquals("Detaching sales should not delete unrelated pivot", false, pivot1a.IsDeleted);
				AssertEquals("Detaching sales should not delete grouping for other sales", false, grouping2.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, detail2.IsDeleted);
				AssertEquals("Detaching sales should not delete pivot for other sales", false, pivot2.IsDeleted);
			}
		}

		public void TestDetachSupersedingTradeLane()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var prospectSales1 = Factory.New<OrgSales>();
			prospectSales1.OW_MP_Product = salesProduct.PK;
			prospectSales1.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
			prospectSales1.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL").PK;
			prospectSales1.OW_IsTraded = false;
			prospectSales1.OW_OH_Primary = client.PK;

			var opp1 = client.SalesOpportunities.AddNew();
			opp1.P8_OpportunityID = "O00005893";
			var opp2 = client.SalesOpportunities.AddNew();
			var salesWrapper = EntitySalesWrapper.Get(prospectSales1, opp2);
			var prospectDetail1 = prospectSales1.TradeDetails.AddNew();
			prospectDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail1.PA_TradeMode = "SEA";
			prospectDetail1.PA_TradeType = "FCL";
			prospectDetail1.ProspectPeriodStart = new ZDate(2018, 1, 1);
			prospectDetail1.ProspectPeriodEnd = new ZDate(2018, 3, 1);
			var pivot1 = opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);
			Factory.Save();

			var prospectDetail2 = prospectSales1.TradeDetails.AddNew();
			prospectDetail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail2.PA_TradeMode = "SEA";
			prospectDetail2.PA_TradeType = "FCL";
			prospectDetail2.ProspectPeriodStart = new ZDate(2018, 2, 1);
			prospectDetail2.ProspectPeriodEnd = new ZDate(2018, 3, 1);
			var detailWrapper = EntityTradeDetailWrapper.Get(prospectDetail2, opp2);
			salesWrapper.EntityTradeDetailsCollection.Add(detailWrapper);
			var pivot2 = opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);

			var superceder = new OrgTradeDetailValueSuperceder(prospectDetail2, new ZDate(2018, 2, 1));
			superceder.SupercedeOverlappedPeriods();
			var periods = prospectDetail1.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Precondition: Should not be superseded", false, periods[0].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[1].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[2].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be expired", true, prospectDetail1.IsExpired);

			Factory.Save();

			var header = new SalesHeader(client, opp2, salesProduct);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(salesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeLanesGrid.SelectSingleElement(salesWrapper);
				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertNotNull("Menu item should have been added", detachMenuItem);
				AssertEquals("Menu item should be enabled if multiple associations exist", true, detachMenuItem.Enabled);
				detachMenuItem.PerformClick();

				//Check message shown

				header.Factory.Save();

				AssertEquals("Detaching sales should delete detail", true, prospectDetail2.IsDeleted);
				AssertEquals("Detaching sales should delete related pivots", true, pivot2.IsDeleted);
				AssertEquals("Detaching sales should not delete pivots for other sales", false, pivot1.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, prospectDetail1.IsDeleted);

				AssertEquals("Should not be superseded", false, periods[0].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods[1].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods[2].PAS_IsSuperseded);
				AssertEquals("Should no longer be expired", false, prospectDetail1.IsExpired);
			}
		}

		public void TestDetachSupersedingTradeLaneWithMultipleSupersessions()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var prospectSales1 = Factory.New<OrgSales>();
			prospectSales1.OW_MP_Product = salesProduct.PK;
			prospectSales1.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
			prospectSales1.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL").PK;
			prospectSales1.OW_IsTraded = false;
			prospectSales1.OW_OH_Primary = client.PK;

			var opp1 = client.SalesOpportunities.AddNew();
			opp1.P8_OpportunityID = "O00005893";
			var opp2 = client.SalesOpportunities.AddNew();
			opp2.P8_OpportunityID = "O00005894";
			var opp3 = client.SalesOpportunities.AddNew();
			var prospectDetail1 = prospectSales1.TradeDetails.AddNew();
			prospectDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail1.PA_TradeMode = "SEA";
			prospectDetail1.PA_TradeType = "FCL";
			prospectDetail1.ProspectPeriodStart = new ZDate(2018, 1, 1);
			prospectDetail1.ProspectPeriodEnd = new ZDate(2018, 5, 1);
			var pivot1 = opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);
			Factory.Save();

			var prospectDetail2 = prospectSales1.TradeDetails.AddNew();
			prospectDetail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail2.PA_TradeMode = "SEA";
			prospectDetail2.PA_TradeType = "FCL";
			prospectDetail2.ProspectPeriodStart = new ZDate(2018, 3, 1);
			prospectDetail2.ProspectPeriodEnd = new ZDate(2018, 6, 1);
			var salesWrapper2 = EntitySalesWrapper.Get(prospectSales1, opp2);
			var detailWrapper2 = EntityTradeDetailWrapper.Get(prospectDetail2, opp2);
			salesWrapper2.EntityTradeDetailsCollection.Add(detailWrapper2);
			var pivot2 = opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);

			var superseder = new OrgTradeDetailValueSuperceder(prospectDetail2, new ZDate(2018, 3, 1));
			superseder.SupercedeOverlappedPeriods();

			var periods = prospectDetail1.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Precondition: Should not be superseded", false, periods[0].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", false, periods[1].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[2].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[3].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[4].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be expired", true, prospectDetail1.IsExpired);
			Factory.Save();

			var prospectDetail3 = prospectSales1.TradeDetails.AddNew();
			prospectDetail3.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail3.PA_TradeMode = "SEA";
			prospectDetail3.PA_TradeType = "FCL";
			prospectDetail3.ProspectPeriodStart = new ZDate(2018, 4, 1);
			prospectDetail3.ProspectPeriodEnd = new ZDate(2018, 8, 1);

			var salesWrapper3 = EntitySalesWrapper.Get(prospectSales1, opp3);
			var detailWrapper3 = EntityTradeDetailWrapper.Get(prospectDetail3, opp3);
			salesWrapper3.EntityTradeDetailsCollection.Add(detailWrapper3);
			var pivot3 = opp3.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);
			salesWrapper3.EntityTradeDetailsCollection.Remove(detailWrapper2);

			var superseder2 = new OrgTradeDetailValueSuperceder(prospectDetail3, new ZDate(2018, 4, 1));
			superseder2.SupercedeOverlappedPeriods();

			var periods2 = prospectDetail2.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Precondition: Should not be superseded", false, periods2[0].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods2[1].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods2[2].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods2[3].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be expired", true, prospectDetail2.IsExpired);
			Factory.Save();

			var header = new SalesHeader(client, opp3, salesProduct);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(salesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeLanesGrid.SelectSingleElement(salesWrapper3);
				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertNotNull("Menu item should have been added", detachMenuItem);
				AssertEquals("Menu item should be enabled if multiple associations exist", true, detachMenuItem.Enabled);
				detachMenuItem.PerformClick();

				//Check message shown

				header.Factory.Save();

				AssertEquals("Detaching sales should delete detail", true, prospectDetail3.IsDeleted);
				AssertEquals("Detaching sales should delete related pivots", true, pivot3.IsDeleted);
				AssertEquals("Detaching sales should not delete pivots for other sales", false, pivot1.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, prospectDetail1.IsDeleted);
				AssertEquals("Detaching sales should not delete pivots for other sales", false, pivot2.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, prospectDetail2.IsDeleted);

				AssertEquals("Supersession should not have changed", false, periods[0].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", false, periods[1].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[2].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[3].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[4].PAS_IsSuperseded);

				AssertEquals("Should not be superseded", false, periods2[0].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods2[1].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods2[2].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods2[3].PAS_IsSuperseded);
				AssertEquals("Should no longer be expired", false, prospectDetail2.IsExpired);
			}
		}

		public void TestDetachSupersedingTradeLaneWithSimilarSupersessions()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var prospectSales1 = Factory.New<OrgSales>();
			prospectSales1.OW_MP_Product = salesProduct.PK;
			prospectSales1.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
			prospectSales1.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL").PK;
			prospectSales1.OW_IsTraded = false;
			prospectSales1.OW_OH_Primary = client.PK;

			var opp1 = client.SalesOpportunities.AddNew();
			opp1.P8_OpportunityID = "O00005893";
			var opp2 = client.SalesOpportunities.AddNew();
			opp2.P8_OpportunityID = "O00005894";
			var opp3 = client.SalesOpportunities.AddNew();
			var prospectDetail1 = prospectSales1.TradeDetails.AddNew();
			prospectDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail1.PA_TradeMode = "SEA";
			prospectDetail1.PA_TradeType = "FCL";
			prospectDetail1.ProspectPeriodStart = new ZDate(2018, 1, 1);
			prospectDetail1.ProspectPeriodEnd = new ZDate(2018, 5, 1);
			var pivot1 = opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);
			Factory.Save();

			var prospectDetail2 = prospectSales1.TradeDetails.AddNew();
			prospectDetail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail2.PA_TradeMode = "SEA";
			prospectDetail2.PA_TradeType = "FCL";
			prospectDetail2.ProspectPeriodStart = new ZDate(2018, 3, 1);
			prospectDetail2.ProspectPeriodEnd = new ZDate(2018, 5, 1);
			var salesWrapper2 = EntitySalesWrapper.Get(prospectSales1, opp2);
			var detailWrapper2 = EntityTradeDetailWrapper.Get(prospectDetail2, opp2);
			salesWrapper2.EntityTradeDetailsCollection.Add(detailWrapper2);
			var pivot2 = opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);

			var superseder = new OrgTradeDetailValueSuperceder(prospectDetail2, new ZDate(2018, 3, 1));
			superseder.SupercedeOverlappedPeriods();

			var periods = prospectDetail1.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Precondition: Should not be superseded", false, periods[0].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", false, periods[1].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[2].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[3].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[4].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be expired", true, prospectDetail1.IsExpired);
			Factory.Save();

			var prospectDetail3 = prospectSales1.TradeDetails.AddNew();
			prospectDetail3.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail3.PA_TradeMode = "SEA";
			prospectDetail3.PA_TradeType = "FCL";
			prospectDetail3.ProspectPeriodStart = new ZDate(2018, 4, 1);
			prospectDetail3.ProspectPeriodEnd = new ZDate(2018, 8, 1);

			var salesWrapper3 = EntitySalesWrapper.Get(prospectSales1, opp3);
			var detailWrapper3 = EntityTradeDetailWrapper.Get(prospectDetail3, opp3);
			salesWrapper3.EntityTradeDetailsCollection.Add(detailWrapper3);
			var pivot3 = opp3.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);
			salesWrapper3.EntityTradeDetailsCollection.Remove(detailWrapper2);

			var superseder2 = new OrgTradeDetailValueSuperceder(prospectDetail3, new ZDate(2018, 4, 1));
			superseder2.SupercedeOverlappedPeriods();

			var periods2 = prospectDetail2.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Precondition: Should not be superseded", false, periods2[0].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods2[1].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods2[2].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be expired", true, prospectDetail2.IsExpired);
			Factory.Save();

			var header = new SalesHeader(client, opp3, salesProduct);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(salesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeLanesGrid.SelectSingleElement(salesWrapper3);
				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertNotNull("Menu item should have been added", detachMenuItem);
				AssertEquals("Menu item should be enabled if multiple associations exist", true, detachMenuItem.Enabled);
				detachMenuItem.PerformClick();

				//Check message shown

				header.Factory.Save();

				AssertEquals("Detaching sales should delete detail", true, prospectDetail3.IsDeleted);
				AssertEquals("Detaching sales should delete related pivots", true, pivot3.IsDeleted);
				AssertEquals("Detaching sales should not delete pivots for other sales", false, pivot1.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, prospectDetail1.IsDeleted);
				AssertEquals("Detaching sales should not delete pivots for other sales", false, pivot2.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, prospectDetail2.IsDeleted);

				AssertEquals("Supersession should not have changed", false, periods[0].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", false, periods[1].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[2].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[3].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[4].PAS_IsSuperseded);

				AssertEquals("Should not be superseded", false, periods2[0].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods2[1].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods2[2].PAS_IsSuperseded);
				AssertEquals("Should no longer be expired", false, prospectDetail2.IsExpired);
			}
		}

		public void TestDetachSupersedingTradeLaneWithIdenticalSupersessions()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var prospectSales1 = Factory.New<OrgSales>();
			prospectSales1.OW_MP_Product = salesProduct.PK;
			prospectSales1.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
			prospectSales1.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL").PK;
			prospectSales1.OW_IsTraded = false;
			prospectSales1.OW_OH_Primary = client.PK;

			var opp1 = client.SalesOpportunities.AddNew();
			opp1.P8_OpportunityID = "O00005893";
			opp1.P8_ClosedDate = ZDate.Empty;
			var opp2 = client.SalesOpportunities.AddNew();
			opp2.P8_OpportunityID = "O00005894";
			opp2.P8_ClosedDate = new ZDate(2018, 2, 1);
			var opp3 = client.SalesOpportunities.AddNew();
			var prospectDetail1 = prospectSales1.TradeDetails.AddNew();
			prospectDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail1.PA_TradeMode = "SEA";
			prospectDetail1.PA_TradeType = "FCL";
			prospectDetail1.ProspectPeriodStart = new ZDate(2018, 1, 1);
			prospectDetail1.ProspectPeriodEnd = new ZDate(2018, 5, 1);
			var pivot1 = opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectDetail1);
			Factory.Save();

			var prospectDetail2 = prospectSales1.TradeDetails.AddNew();
			prospectDetail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail2.PA_TradeMode = "SEA";
			prospectDetail2.PA_TradeType = "FCL";
			prospectDetail2.ProspectPeriodStart = new ZDate(2018, 1, 1);
			prospectDetail2.ProspectPeriodEnd = new ZDate(2018, 5, 1);
			var salesWrapper2 = EntitySalesWrapper.Get(prospectSales1, opp2);
			var detailWrapper2 = EntityTradeDetailWrapper.Get(prospectDetail2, opp2);
			salesWrapper2.EntityTradeDetailsCollection.Add(detailWrapper2);
			var pivot2 = opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectDetail2);

			var superseder = new OrgTradeDetailValueSuperceder(prospectDetail2, new ZDate(2018, 1, 1));
			superseder.SupercedeOverlappedPeriods();

			var periods = prospectDetail1.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Precondition: Should not be superseded", true, periods[0].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[1].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[2].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[3].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods[4].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be expired", true, prospectDetail1.IsExpired);
			Factory.Save();

			var prospectDetail3 = prospectSales1.TradeDetails.AddNew();
			prospectDetail3.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail3.PA_TradeMode = "SEA";
			prospectDetail3.PA_TradeType = "FCL";
			prospectDetail3.ProspectPeriodStart = new ZDate(2018, 4, 1);
			prospectDetail3.ProspectPeriodEnd = new ZDate(2018, 8, 1);

			var salesWrapper3 = EntitySalesWrapper.Get(prospectSales1, opp3);
			var detailWrapper3 = EntityTradeDetailWrapper.Get(prospectDetail3, opp3);
			salesWrapper3.EntityTradeDetailsCollection.Add(detailWrapper3);
			var pivot3 = opp3.AssociatedTradeLanesPivots.AddPivotFor(prospectSales1);
			salesWrapper3.EntityTradeDetailsCollection.Remove(detailWrapper2);

			var superseder2 = new OrgTradeDetailValueSuperceder(prospectDetail3, new ZDate(2018, 4, 1));
			superseder2.SupercedeOverlappedPeriods();

			var periods2 = prospectDetail2.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Precondition: Should not be superseded", false, periods2[0].PAS_IsSuperseded);
			AssertEquals("Precondition: Should not be superseded", false, periods2[1].PAS_IsSuperseded);
			AssertEquals("Precondition: Should not be superseded", false, periods2[2].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods2[3].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be superseded", true, periods2[4].PAS_IsSuperseded);
			AssertEquals("Precondition: Should be expired", true, prospectDetail2.IsExpired);
			Factory.Save();

			var header = new SalesHeader(client, opp3, salesProduct);
			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(salesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				control.TradeLanesGrid.SelectSingleElement(salesWrapper3);
				control.TradeLanesGrid.ContextMenu.ShowPopupMenu();

				var detachMenuItem = control.TradeLanesGrid.ContextMenu.MenuItems.FindByText("Detach");
				AssertNotNull("Menu item should have been added", detachMenuItem);
				AssertEquals("Menu item should be enabled if multiple associations exist", true, detachMenuItem.Enabled);
				detachMenuItem.PerformClick();

				//Check message shown

				header.Factory.Save();

				AssertEquals("Detaching sales should delete detail", true, prospectDetail3.IsDeleted);
				AssertEquals("Detaching sales should delete related pivots", true, pivot3.IsDeleted);
				AssertEquals("Detaching sales should not delete pivots for other sales", false, pivot1.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, prospectDetail1.IsDeleted);
				AssertEquals("Detaching sales should not delete pivots for other sales", false, pivot2.IsDeleted);
				AssertEquals("Detaching sales should not delete detail for other sales", false, prospectDetail2.IsDeleted);

				AssertEquals("Supersession should not have changed", true, periods[0].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[1].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[2].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[3].PAS_IsSuperseded);
				AssertEquals("Supersession should not have changed", true, periods[4].PAS_IsSuperseded);

				AssertEquals("Should not be superseded", false, periods2[0].PAS_IsSuperseded);
				AssertEquals("Should not be superseded", false, periods2[1].PAS_IsSuperseded);
				AssertEquals("Should not be superseded", false, periods2[2].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods2[3].PAS_IsSuperseded);
				AssertEquals("Should no longer be superseded after detach", false, periods2[4].PAS_IsSuperseded);
				AssertEquals("Should no longer be expired", false, prospectDetail2.IsExpired);
			}
		}

		#endregion

		#region Columns

		public void TestProductCustomColumns()
		{
			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			var layout = salesProduct.FormLayout;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			var salesWrapper = EntitySalesWrapper.Get(sales, opp);
			var header = new SalesHeader(org, opp, salesProduct);

			var columnDef1 = layout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			columnDef1.XC_Name = "CustomStr";
			columnDef1.XC_Type = AddOnColumnDataType.Codes.String;

			var columnDef2 = layout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			columnDef2.XC_Name = "CustomInt";
			columnDef2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var columnDef3 = layout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			columnDef3.XC_Name = "AnotherCustomStr";
			columnDef3.XC_Type = AddOnColumnDataType.Codes.String;

			var gridColumn1 = layout.TradeLaneGridColumnDefinitions.AddNew();
			gridColumn1.GenCustomColumnDefinitionFk = columnDef1.PK;

			var gridColumn2 = layout.TradeLaneGridColumnDefinitions.AddNew();
			gridColumn2.GenCustomColumnDefinitionFk = columnDef2.PK;

			var gridColumn3 = layout.TradeLaneGridColumnDefinitions.AddNew();
			gridColumn3.GenCustomColumnDefinitionFk = columnDef3.PK;

			Factory.Save();

			using (var form = new ZForm(header))
			using (var control = new TradeLanesControlForTest(salesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				var resultColumns = control.TradeLanesGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("Custom string column should be added", resultColumns.Any(c => c.Caption == "CustomStr"));
				Assert("Custom int column should be added", resultColumns.Any(c => c.Caption == "CustomInt"));

				var strDescriptor = (IOverridablePropertyDescriptor)resultColumns.First(c => c.Caption == "CustomStr");
				var strRetriever = strDescriptor.PropertyDescriptor as IZPropertyInfoRetriever;
				layout.TradeLaneCustomColumnDefinitionCollection.Remove(columnDef1);
				Factory.Save();
				AssertNoExceptionThrown("Should still be able to use custom property that was deleted after loading control", () => _ = strRetriever.GetZPropertyInfo(salesWrapper));

				var anotherStrDescriptor = (IOverridablePropertyDescriptor)resultColumns.First(c => c.Caption == "AnotherCustomStr");
				var anotherStrRetriever = anotherStrDescriptor.PropertyDescriptor as IZPropertyInfoRetriever;
				columnDef3.XC_Name = "YetAnotherCustomStr";
				Factory.Save();
				AssertNoExceptionThrown("Should still be able to use custom property that was modified after loading control", () => _ = anotherStrRetriever.GetZPropertyInfo(salesWrapper));
			}
		}

		#endregion
	}
}
