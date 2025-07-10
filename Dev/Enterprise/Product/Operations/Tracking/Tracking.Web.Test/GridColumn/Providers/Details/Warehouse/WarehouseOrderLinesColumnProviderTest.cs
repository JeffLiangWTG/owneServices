using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(WarehouseOrderLinesColumnProvider))]
	[HttpContextEnabledTest]
	class WarehouseOrderLinesColumnProviderTest : LineColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			if (SiteUser == null || SiteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZTextEditColumn("Product", TrackingWhsOrderLine.WrapperSchema.ProductCode) { ColumnKey = WebTracker.Grids.WarehouseDocketLine.Product });
			}
			else
			{
				AddDefaultsColumn(new ZHyperLinkColumn("Product", TrackingWhsOrderLine.WrapperSchema.ProductCode)
				{
					ColumnKey = WebTracker.Grids.WarehouseDocketLine.Product,
					DataNavigateUrlFormatString = TrackingConstants.RelativePath.ProductProfileDetailsPage + "?Ref={0}",
					DataNavigateUrlFields = new string[1] { TrackingWhsOrderLine.WrapperSchema.WE_OP }
				});
				AddDefaultsColumn(new ZTextEditColumn("Description", TrackingWhsOrderLine.WrapperSchema.ProductDescription));
			}
			AddDefaultsColumn(new ZCalcEditColumn("Packs", TrackingWhsOrderLine.WrapperSchema.WE_PackQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.Packs,
				AutoPostBack = true
			});
			AddDefaultsColumn(new ZDropDownListColumn("Packs UQ", TrackingWhsOrderLine.WrapperSchema.WE_F3_NKPackType, TrackingWhsOrderLine.GetSchemaPath("Lookups.PackTypes"))
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.PackTypes,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddDefaultsColumn(new ZCalcEditColumn("Qty Ordered", TrackingWhsOrderLine.WrapperSchema.WE_TransactionQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.OrderedQuantity,
				BindToDecimals = TrackingWhsOrderLine.GetSchemaPath("SupplierPart+OP_CountDecimalPlaces")
			});
			AddDefaultsColumn(new ZDropDownListColumn("UQ", TrackingWhsOrderLine.WrapperSchema.ProductUQ, TrackingWhsOrderLine.GetSchemaPath("Lookups.PackTypes"))
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.UQ,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddDefaultsColumn(new ZHyperLinkColumn("Reserved", "")
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.Reserved,
				DataTextFormatString = "{0} View",
				DataTextFields = new string[] { TrackingWhsOrderLine.WrapperSchema.AllocatedQuantityAsString },
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.WarehouseOrderLineAllocationPage + "?Ref={0}" + "TEST",
				DataNavigateUrlFields = new string[] { TrackingWhsOrderLine.WrapperSchema.WE_PK },
				Target = "_blank",
				WindowStyle = Global.WarehouseOrderLineAllocationPopupWindowStyle
			});
			if (TestOrder != null)
			{
				AddLineAttributeColumns(AttributeManager.AttributeModules.Warehouse, TestOrder.WhsOrder.Client, ZString.Empty);
			}
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.WarehouseDocketLine.Reserved
		};

		protected override void SetUp()
		{
			base.SetUp();
			TestOrder = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			SetupNewProvider();
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new WarehouseOrderLinesColumnProvider(TestOrder, "TEST");
		}

		TrackingWhsOrder TestOrder
		{
			get { return testOrder; }
			set { testOrder = value; }
		}

		TrackingWhsOrder testOrder;
	}
}
