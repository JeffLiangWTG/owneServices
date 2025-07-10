using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class SalesHeaderListControlTest : TestCaseWithFactory
	{
		#region Refresh

		public void TestRefreshStrips()
		{
			var salesProductAaa = Factory.NewWithValidTestData<OrgSalesProduct>();
			salesProductAaa.MP_Code = "AAA";
			salesProductAaa.MP_Name = "AAA";
			var salesProductBbb = Factory.NewWithValidTestData<OrgSalesProduct>();
			salesProductBbb.MP_Code = "BBB";
			salesProductBbb.MP_Name = "BBB";
			var salesProductCcc = Factory.NewWithValidTestData<OrgSalesProduct>();
			salesProductCcc.MP_Code = "CCC";
			salesProductCcc.MP_Name = "CCC";
			var salesProductDdd = Factory.NewWithValidTestData<OrgSalesProduct>();
			salesProductDdd.MP_Code = "DDD";
			salesProductDdd.MP_Name = "DDD";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();

			var salesAaa1 = org.SalesCollection.AddNew();
			salesAaa1.OW_MP_Product = salesProductAaa.PK;
			var detailAaa1 = salesAaa1.TradeDetails.AddNew();

			var salesAaa2 = org.SalesCollection.AddNew();
			salesAaa2.OW_MP_Product = salesProductAaa.PK;
			var detailAaa2 = salesAaa2.TradeDetails.AddNew();

			var salesBbb = org.SalesCollection.AddNew();
			salesBbb.OW_MP_Product = salesProductBbb.PK;
			var detailBbb = salesBbb.TradeDetails.AddNew();

			var salesCcc = org.SalesCollection.AddNew();
			salesCcc.OW_MP_Product = salesProductCcc.PK;
			var detailCcc = salesCcc.TradeDetails.AddNew();

			var tradedSalesDdd = org.SalesCollection.AddNew();
			tradedSalesDdd.OW_MP_Product = salesProductDdd.PK;
			tradedSalesDdd.OW_IsTraded = true;
			tradedSalesDdd.TradeDetails.AddNew();

			opp.AssociatedTradeLanesPivots.AddPivotFor(salesAaa1);
			opp.AssociatedTradeLanesPivots.AddPivotFor(detailAaa1);
			opp.AssociatedTradeLanesPivots.AddPivotFor(salesAaa2);
			opp.AssociatedTradeLanesPivots.AddPivotFor(detailAaa2);
			opp.AssociatedTradeLanesPivots.AddPivotFor(salesBbb);
			opp.AssociatedTradeLanesPivots.AddPivotFor(detailBbb);
			opp.AssociatedTradeLanesPivots.AddPivotFor(salesCcc);
			opp.AssociatedTradeLanesPivots.AddPivotFor(detailCcc);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);

			using (var form = new ZForm(orgInOtherFactory))
			using (var listControl = new SalesHeaderListControl())
			{
				form.Controls.Add(listControl);

				form.Show();

				AssertMultilineASCIIEquals("Strip Product Codes - Should not included DDD as it only has traded data (no prospect)",
@"CCC
BBB
AAA",
				string.Join(System.Environment.NewLine, listControl.StripsPanel.Controls.OfType<SalesHeaderStripControl>().Select(x => x.CurrentDataItem.SalesProductCode)));
			}

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgOpportunitySchema.Constants.TableName, 1 },
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgSalesProductSchema.Constants.TableName, 2 },
				{ OrgTradeDetailSchema.Constants.TableName, 3 },
				{ OrgTradeProspectSchema.Constants.TableName, 1 },
				{ OrgTradePeriodSchema.Constants.TableName, 1 },
				{ OrgSalesValueAssociationPivotSchema.Constants.TableName, 2 } // 1 for loading associated activities for UI binding (company view), 1 for readonly status (global view)
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		#endregion

		#region ReadOnly

		public void TestReadOnly()
		{
			using (var control = new SalesHeaderListControlForTest())
			{
				control.ReadOnly = true;
				AssertEquals(false, control.TopToolStrip_Exposed.Visible);

				control.ReadOnly = false;
				AssertEquals(true, control.TopToolStrip_Exposed.Visible);
			}
		}

		#endregion

		#region Buttons

		public void TestAddButton()
		{
			foreach (var product in Factory.Load<OrgSalesProduct>(new ZQuery()))
			{
				product.Delete();
			}

			var xxxProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			xxxProduct.MP_Code = "XXX";
			xxxProduct.MP_Name = "XXX Product";
			var yyyProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			yyyProduct.MP_Code = "YYY";
			yyyProduct.MP_Name = "YYY Product";
			var zzzProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			zzzProduct.MP_Code = "ZZZ";
			zzzProduct.MP_Name = "ZZZ Product & Hah";

			Factory.Save();

			var org = Factory.New<OrgHeader>();
			using (var form = new ZForm(org))
			using (var listControl = new SalesHeaderListControlForTest())
			{
				form.Controls.Add(listControl);
				form.Show();

				AssertMultilineASCIIEquals("Strip Product Codes", string.Empty, string.Join(System.Environment.NewLine, listControl.StripsPanel.Controls.OfType<SalesHeaderStripControl>().Select(x => x.CurrentDataItem.SalesProductCode)));

				listControl.AddToolStripDropDownButton_Exposed.PerformClick();
				AssertMultilineASCIIEquals("Should have a menu item for each product",
@"XXX Product
YYY Product
ZZZ Product && Hah",
					string.Join(System.Environment.NewLine, listControl.AddToolStripDropDownButton_Exposed.DropDownItems.Cast<ZToolStripMenuItem>().Select(x => x.Text)));

				var menuItem = listControl.AddToolStripDropDownButton_Exposed.DropDownItems.Cast<ZToolStripMenuItem>().First();
				menuItem.PerformClick();

				AssertMultilineASCIIEquals("Should have added XXX product as a strip",
@"XXX",
				string.Join(System.Environment.NewLine, listControl.StripsPanel.Controls.OfType<SalesHeaderStripControl>().Select(x => x.CurrentDataItem.SalesProductCode)));

				AssertEquals(true, listControl.StripsPanel.Controls.OfType<SalesHeaderStripControl>().Single().CurrentDataItem.HasChanges);
				AssertMultilineASCIIEquals("Should still have a menu item for XXX product",
@"XXX Product
YYY Product
ZZZ Product && Hah",
					string.Join(System.Environment.NewLine, listControl.AddToolStripDropDownButton_Exposed.DropDownItems.Cast<ZToolStripMenuItem>().Select(x => x.Text)));
			}
		}

		public void TestCreateQuotationToolStripButton()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			Factory.Save();

			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var salesHeaderCollection = (SalesHeaderCollection)opportunity.ProspectiveSalesHeaderCollection;
			var shipmentHeader = salesHeaderCollection.AddNew(shipmentProduct);

			using (var form = new ZForm(opportunity))
			using (var control = new SalesHeaderListControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var generateQuoteController = new MockGenerateQuoteForSalesValueAssociatedEntityControllerForTest();
				using (ObjectFactory.Substitute<IGenerateQuoteForSalesValueAssociatedEntityController>(generateQuoteController))
				{
					var createQuotationToolStripButton = (ZToolStripButton)control.TopToolStrip_Exposed.Items.Find("CreateQuotationToolStripButton", true).Single();
					createQuotationToolStripButton.PerformClick();
					AssertEquals("Please enter trade lanes details before creating a quotation.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("EntityCalled", generateQuoteController.EntityCalled);
					AssertNull("TradeDetailsCalled", generateQuoteController.TradeDetailsCalled);

					var prospectiveTradeLane1 = shipmentHeader.EntitySalesCollectionProductView.AddNew();
					var prospectiveTradeDetail1 = prospectiveTradeLane1.EntityTradeDetailsCollection.AddNew();
					var prospectiveTradeLane2 = shipmentHeader.EntitySalesCollectionProductView.AddNew();
					var prospectiveTradeDetail2 = prospectiveTradeLane2.EntityTradeDetailsCollection.AddNew();

					UnitTestUserNotification.Instance.ClearMessages();
					createQuotationToolStripButton.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

					AssertEquals("EntityCalled", opportunity, generateQuoteController.EntityCalled);
					AssertContainsExactElementsInAnyOrder("TradeDetailsCalled",
						new[] { prospectiveTradeDetail1, prospectiveTradeDetail2 },
						generateQuoteController.TradeDetailsCalled);
				}
			}
		}

		#endregion

		class SalesHeaderListControlForTest : SalesHeaderListControl
		{
			public ZToolStrip TopToolStrip_Exposed
			{
				get { return TopToolStrip; }
			}

			public ZToolStripDropDownButton AddToolStripDropDownButton_Exposed
			{
				get { return AddToolStripDropDownButton; }
			}
		}
	}
}
