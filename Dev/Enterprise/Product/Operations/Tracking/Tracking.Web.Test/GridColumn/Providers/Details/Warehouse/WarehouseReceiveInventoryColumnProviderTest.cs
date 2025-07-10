using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(WarehouseReceiveInventoryColumnProvider))]
	[HttpContextEnabledTest]
	sealed class WarehouseReceiveInventoryColumnProviderTest : LineColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();

			AddDefaultsColumn(new ZCalcEditColumn("Line No", TrackingWhsReceiveLine.WrapperSchema.WE_LineNo)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.LineNo,
				AutoPostBack = true
			});

			if (AllowEdit)
			{
				AddDefaultsColumn(new ZFindBoxColumn("Product", TrackingWhsReceiveLine.WrapperSchema.WE_OP, "WhsReceiveLine.Lookups.SupplierParts")
				{
					ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Product,
					ValueFieldName = "PK",
					ModuleID = WebModuleIDs.OrgSupplierPartTracking,
					AutoPostBack = true
				});
			}
			else
			{
				ZHyperLinkColumn productColumn = new ZHyperLinkColumn("Product", TrackingWhsReceiveLine.WrapperSchema.ProductCode)
				{
					ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Product,
					DataNavigateUrlFormatString = TrackingConstants.RelativePath.ProductProfileDetailsPage + "?Ref={0}" + ShowInPopupParam,
					DataNavigateUrlFields = new string[1] { TrackingWhsReceiveLine.WrapperSchema.WE_OP }
				};
				if (IsShownInPopup)
				{
					productColumn.Target = "_blank";
					productColumn.WindowStyle = Global.ProductProfilePopupWindowStyle;
				}
				AddDefaultsColumn(productColumn);
			}

			AddDefaultsColumn(new ZTextEditColumn("Description", TrackingWhsReceiveLine.WrapperSchema.ProductDesc)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Description,
				ReadOnly = true
			});

			AddDefaultsColumn(new ZCalcEditColumn("Packs", TrackingWhsReceiveLine.WrapperSchema.WE_PackQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Packs,
				AutoPostBack = true
			});

			AddDefaultsColumn(new ZDropDownListColumn("Packs UQ", TrackingWhsReceiveLine.WrapperSchema.WE_F3_NKPackType, "WhsReceiveLine.Lookups.PackTypes")
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.PackTypes,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				AutoPostBack = true
			});

			AddDefaultsColumn(new ZCalcEditColumn("Expected Quantity", TrackingWhsReceiveLine.WrapperSchema.WE_ClientOrderedUnits)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.OrderedQuantity,
				BindToDecimals = "WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces",
				AutoPostBack = true
			});

			AddDefaultsColumn(new ZCalcEditColumn("Quantity", TrackingWhsReceiveLine.WrapperSchema.WE_TransactionQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Quantity,
				BindToDecimals = "WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces",
				AutoPostBack = true
			});

			AddDefaultsColumn(new ZDropDownListColumn("UQ", TrackingWhsReceiveLine.WrapperSchema.ProductUQ, "WhsReceiveLine.Lookups.PackTypesWithStandardUnits")
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.UQ,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ReadOnly = true
			});

			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				PartAttributeManager partManager = SiteUser.LoggedInOrganisation.PartAttributeManager;
				if (partManager.IsPartAttributeUsedByOrganisation(1))
				{
					AddDefaultsColumn(new ZTextEditColumn(partManager.PartAttributeName1, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib1) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.PartAttribute1 });
				}
				if (partManager.IsPartAttributeUsedByOrganisation(2))
				{
					AddDefaultsColumn(new ZTextEditColumn(partManager.PartAttributeName2, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib2) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.PartAttribute2 });
				}
				if (partManager.IsPartAttributeUsedByOrganisation(3))
				{
					AddDefaultsColumn(new ZTextEditColumn(partManager.PartAttributeName3, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib3) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.PartAttribute3 });
				}
				if (partManager.IsExpiryDateUsedByOrganisation)
				{
					AddDefaultsColumn(new ZDateTimeColumn("Expiry Date", TrackingWhsReceiveLine.WrapperSchema.WE_ExpiryDate) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.ExpiryDate });
				}
			}
			if (TestReceive != null)
			{
				int columnKey = 12000;
				foreach (CustomLabelInfo field in new WhsInventoryView.CustomLabelsProvider(null).GetCustomFields(TestReceive.WhsReceive.Client, Factory))
				{
					if (field.IsEnabled && field.LabelName.StartsWith("WhsDocketLine."))
					{
						var column = ZTemplateColumn.GetNew(field.Caption, field.PropertyType, field.PropertyName);
						column.ColumnKey = columnKey;
						AddDefaultsColumn(column);
						columnKey++;
					}
				}
			}
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
			var provider = new WarehouseReceiveInventoryColumnProvider(TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>()), false, string.Empty, false);
			provider.CustomizeDictionary();
			var serialNumberColumn = new ZTextEditColumn("Serial Number", TrackingWhsReceiveLine.WrapperSchema.WE_SerialNumber) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.SerialNumber };
			AssertEquals(clientUsesSerialNumber, provider.DefaultColumns.Contains(serialNumberColumn.UniqueKey));
		}

		protected override bool SupportsOldLayoutFix => false;

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.WarehouseReceiveLine.Product
		};

		protected override void SetUp()
		{
			IsShownInPopup = false;
			ShowInPopupParam = string.Empty;
			AllowEdit = true;
			base.SetUp();
			TestReceive = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			SetupNewProvider();
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new WarehouseReceiveInventoryColumnProvider(TestReceive, AllowEdit, ShowInPopupParam, IsShownInPopup);
		}

		TrackingWhsReceive TestReceive { get; set; }

		string ShowInPopupParam { get; set; }

		bool IsShownInPopup { get; set; }

		bool AllowEdit { get; set; }
	}
}
