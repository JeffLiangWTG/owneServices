using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ForwardingOrderLineColumnProvider))]
	[HttpContextEnabledTest]
	sealed class ForwardingOrderLineColumnProviderTest : LineColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZCalcEditColumn("Line #", JobOrderLineSchema.JO_LineNo.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.LineNumber });
			var siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			if (siteUser == null || siteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZTextEditColumn("Part #", JobOrderLineSchema.JO_Partno.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartNumber });
			}
			else
			{
				AddDefaultsColumn(new ZHyperLinkColumn("Part #", JobOrderLineSchema.JO_Partno.Name)
				{
					ColumnKey = WebTracker.Grids.TrackingOrderLines.PartNumber,
					DataNavigateUrlFormatString = TrackingConstants.RelativePath.ProductProfileDetailsPage + "?Ref={0}&OrderRef={1}",
					DataNavigateUrlFields = new[] { "Product.PK", "Order.PK" }
				});
			}

			if (siteUser != null && !siteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZTextEditColumn("Description", JobOrderLineSchema.JO_Description.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.Description });
			}

			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				var partManager = siteUser.LoggedInOrganisation.PartAttributeManager;
				if (partManager != null)
				{
					if (partManager.IsPartAttributeUsedByOrganisation(1))
					{
						AddDefaultsColumn(new ZTextEditColumn(partManager.PartAttributeName1, JobOrderLineSchema.JO_PartAttrib1.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartAttribute1 });
					}
					if (partManager.IsPartAttributeUsedByOrganisation(2))
					{
						AddDefaultsColumn(new ZTextEditColumn(partManager.PartAttributeName2, JobOrderLineSchema.JO_PartAttrib2.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartAttribute2 });
					}
					if (partManager.IsPartAttributeUsedByOrganisation(3))
					{
						AddDefaultsColumn(new ZTextEditColumn(partManager.PartAttributeName3, JobOrderLineSchema.JO_PartAttrib3.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartAttribute3 });
					}
				}
			}

			AddDefaultsColumn(new ZCalcEditColumn("Inner Packs", JobOrderLineSchema.JO_InnerPacks.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.InnerPacks });
			AddDefaultsColumn(new ZDropEditColumn("Inner Package Type", JobOrderLineSchema.JO_InnerPacksUQ.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.InnerPacksUQ });
			AddDefaultsColumn(new ZCalcEditColumn("Outer Packs", JobOrderLineSchema.JO_OuterPacks.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.OuterPacks });
			AddDefaultsColumn(new ZDropEditColumn("Outer Package Type", JobOrderLineSchema.JO_OuterPacksUQ.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.OuterPacksUQ });

			if (siteUser != null && !siteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZCalcEditColumn("Qty Ordered", JobOrderLineSchema.JO_Quantity.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.QuantityOrdered });
				AddDefaultsColumn(new ZCalcEditColumn("Qty Invoiced", JobOrderLineSchema.JO_QtyInvoiced.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.QuantityInvoiced });
				AddDefaultsColumn(new ZCalcEditColumn("Qty Received", JobOrderLineSchema.JO_QtyReceived.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.QuantityReceived });
				AddDefaultsColumn(new ZCalcEditColumn("Qty Remaining", OrderLine.Schema.JO_QuantityRemaining) { ColumnKey = WebTracker.Grids.TrackingOrderLines.QuantityRemaining });
				AddDefaultsColumn(new ZTextEditColumn("Unit of Qty", JobOrderLineSchema.JO_F3_NKPackType.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.UnitOfQuantity });
				AddDefaultsColumn(new ZCalcEditColumn("Item Price", JobOrderLineSchema.JO_ItemPrice.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ItemPrice });
				AddDefaultsColumn(new ZCalcEditColumn("Total Price", JobOrderLineSchema.JO_LinePrice.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.TotalPrice });
				AddColumn(new ZTextEditColumn("Invoice #", JobOrderLineSchema.JO_CommercialInvoiceNo.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.InvoiceNumber });
			}
			AddDefaultsColumn(new ZDropDownListColumn("Line Status", JobOrderLineSchema.JO_LineStatus.Name, "JO_LineStatus_List") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingOrderLines.LineStatus });
			if (TestOrder != null)
			{
				AddLineAttributeColumns(AttributeManager.AttributeModules.Order, TestOrder.Buyer, ZString.Empty);
			}
			AddDefaultsColumn(new ZDateTimeColumn("Required In Store Date", JobOrderLineSchema.JO_LineDropDate.Name, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrderLines.RequiredDate });

			AddColumn(new ZDropEditColumn("Incoterm", JobOrderLineSchema.JO_INCO.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.INCOTerm });
			AddColumn(new ZTextEditColumn("Additional Terms", JobOrderLineSchema.JO_AdditionalTerms.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.AdditionalTerms });

			AddColumn(new ZTextEditColumn("Confirm Number", JobOrderLineSchema.JO_ConfirmationNum.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ConfirmNumber });
			AddColumn(new ZDateTimeColumn("Confirm Date", JobOrderLineSchema.JO_ConfirmationDate.Name, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ConfirmDate });
			AddColumn(new ZDateTimeColumn("Required Ex Works Date", JobOrderLineSchema.JO_ExWorksDate.Name, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrderLines.RequiredExWorksDate });

			AddColumn(new ZTextEditColumn("Container #", JobOrderLineSchema.JO_ContainerNumber.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ContainerNumber });
		}

		public void TestDefaultColumnsWithSerialNumber_SerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestDefaultColumnsWithSerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestDefaultColumnsWithSerialNumber_ClientDoesNotUseSerialNumber()
		{
			TestDefaultColumnsWithSerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestDefaultColumnsWithSerialNumberCore(bool clientUsesSerialNumber)
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			SiteUser.LoggedInOrganisation.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			var provider = new ForwardingOrderLineColumnProvider(Factory.NewWithValidTestData<TrackingOrder>());
			provider.CustomizeDictionary();
			var serialNumberColumn = new ZTextEditColumn("Serial Number", JobOrderLineSchema.JO_SerialNumber.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.SerialNumber };
			AssertEquals(clientUsesSerialNumber, provider.DefaultColumns.Contains(serialNumberColumn.UniqueKey));
		}

		protected override bool SupportsOldLayoutFix => false;

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ForwardingOrderLineColumnProvider(TestOrder);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestOrder = Factory.NewWithValidTestData<TrackingOrder>();
			SetupNewProvider();
		}

		TrackingOrder TestOrder { get; set; }
	}
}
