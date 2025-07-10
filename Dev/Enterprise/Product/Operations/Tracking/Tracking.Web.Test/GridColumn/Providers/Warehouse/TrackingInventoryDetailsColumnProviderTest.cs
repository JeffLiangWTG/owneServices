using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingInventoryDetailsColumnProvider))]
	[HttpContextEnabledTest]
	class TrackingInventoryDetailsColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override void BeforeLayoutsWithFewDynamicColumns()
		{
			base.BeforeLayoutsWithFewDynamicColumns();
			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMPartAttrib2Type = "TST";
			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMUseExpiryDate = true;
			SetupNewProvider();
		}

		protected override void BeforeLayoutsWithAllDynamicColumns()
		{
			base.BeforeLayoutsWithAllDynamicColumns();
			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMPartAttrib1Type = "TST";
			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMPartAttrib2Type = "TST";
			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMPartAttrib3Type = "TST";
			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMUseExpiryDate = true;
			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMUsePackingDate = true;
			SetupNewProvider();
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.Currency],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.TotalValue],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.AvailableToPickQuantity],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.Status],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.ReceiptReference]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.Currency],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.TotalValue],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.PartAttribute2],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.AvailableToPickQuantity],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.Status],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.ExpiryDate],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.ReceiptReference]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.Currency],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.TotalValue],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.PartAttribute2],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.PartAttribute1],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.PartAttribute3],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.AvailableToPickQuantity],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.Status],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.ExpiryDate],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.PackingDate],
				TestProvider[WebTracker.Grids.TrackingInventoryDetails.ReceiptReference]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();

			var receiptRefColumn = new ZHyperLinkColumn("Receipt Ref", "ReceiptReference")
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.ReceiptReference
			};

			if (HttpContext.Current != null && HttpContext.Current.Request != null)
			{
				receiptRefColumn.DataNavigateUrlFormatString = @"javascript: parent." + HttpContext.Current.Request.QueryString[ZIFramePage.OKFunctionQuery] + "('{0}','{1}');"; // non-semantic text
			}

			receiptRefColumn.DataNavigateUrlFields = new string[] { "ReceiptReference", "PK" };
			AddRequiredColumn(receiptRefColumn);

			var productColumn = new ZTextEditColumn("Product", WhsInventoryView.Schema.WI_OP_PartNum)
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.Product
			};
			AddDefaultsColumn(productColumn);

			var arrivalDateColumn = new ZDateTimeColumn("ETA/Arrival", WhsInventoryView.Schema.WI_ArrivalDateOrETA)
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.ETA
			};
			AddDefaultsColumn(arrivalDateColumn);

			var statusColumn = new ZTextEditColumn("Status", "StatusDesc")
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.Status
			};
			AddColumn(statusColumn);

			var availableQtyColumn = new ZCalcEditColumn("Available Pick Qty", WhsInventoryView.Schema.WI_AvailableToPickQuantity)
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.AvailableToPickQuantity
			};
			AddColumn(availableQtyColumn);

			var committedQtyColumn = new ZCalcEditColumn("Committed Qty", "InternalsProxy+" + WhsInventoryView.Schema.CommittedToTransactionQuantity)
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.CommittedQuantity
			};
			AddColumn(committedQtyColumn);

			var allocatedQtyColumn = new ZCalcEditColumn("Reserved Qty", WhsInventoryView.Schema.WI_CrossDockQuantity)
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.ReservedQuantity
			};
			AddColumn(allocatedQtyColumn);

			var totalQtyColumn = new ZCalcEditColumn("Total Qty", WhsInventoryView.Schema.WI_TotalUnits)
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.TotalQuantity
			};
			AddColumn(totalQtyColumn);

			var totalValueColumn = new ZCalcEditColumn("Total Value", WhsInventoryView.Schema.WI_TotalValue)
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.TotalValue
			};
			AddColumn(totalValueColumn);

			var totalValueCurrencyColumn = new ZTextEditColumn("Currency", WhsInventoryView.Schema.WI_Currency)
			{
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.Currency
			};
			AddColumn(totalValueCurrencyColumn);

			if (AttributeManager != null)
			{
				var partAttr1Column = new ZTextEditColumn(AttributeManager.PartAttributeName1, WhsInventoryViewSchema.WI_PartAttrib1.Name)
				{
					ColumnKey = WebTracker.Grids.TrackingInventoryDetails.PartAttribute1
				};
				var partAttr2Column = new ZTextEditColumn(AttributeManager.PartAttributeName2, WhsInventoryViewSchema.WI_PartAttrib2.Name)
				{
					ColumnKey = WebTracker.Grids.TrackingInventoryDetails.PartAttribute2
				};
				var partAttr3Column = new ZTextEditColumn(AttributeManager.PartAttributeName3, WhsInventoryViewSchema.WI_PartAttrib3.Name)
				{
					ColumnKey = WebTracker.Grids.TrackingInventoryDetails.PartAttribute3
				};

				if (AttributeManager.IsPartAttributeUsedByOrganisation(1))
				{
					AddColumn(partAttr1Column);
				}
				if (AttributeManager.IsPartAttributeUsedByOrganisation(2))
				{
					AddColumn(partAttr2Column);
				}
				if (AttributeManager.IsPartAttributeUsedByOrganisation(3))
				{
					AddColumn(partAttr3Column);
				}

				var expiryDateColumn = new ZDateTimeColumn("Expiry Date", WhsInventoryViewSchema.WI_ExpiryDate.Name, ZDateTimePickerFormat.Short)
				{
					ColumnKey = WebTracker.Grids.TrackingInventoryDetails.ExpiryDate
				};
				var packingDateColumn = new ZDateTimeColumn("Packing Date", WhsInventoryViewSchema.WI_PackingDate.Name, ZDateTimePickerFormat.Short)
				{
					ColumnKey = WebTracker.Grids.TrackingInventoryDetails.PackingDate
				};

				if (AttributeManager.IsExpiryDateUsedByOrganisation)
				{
					AddColumn(expiryDateColumn);
				}
				if (AttributeManager.IsPackingDateUsedByOrganisation)
				{
					AddColumn(packingDateColumn);
				}
			}

			var lastCostColumn = new ZCalcEditColumn("Last Cost", "SupplierPart+" + Enterprise.MasterFiles.Business.OrgSupplierPart.Schema.OP_LastCost)
			{
				Decimals = 2,
				ColumnKey = WebTracker.Grids.TrackingInventoryDetails.LastCost
			};
			AddColumn(lastCostColumn);
		}

		protected virtual PartAttributeManager AttributeManager
		{
			get
			{
				return LoggedSiteUser.LoggedInOrganisation.PartAttributeManager;
			}
		}

		OrgContactWebUser LoggedSiteUser
		{
			get
			{
				return (OrgContactWebUser)WebEnv.AppInstance.SiteUser;
			}
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingInventoryDetailsColumnProvider(AttributeManager);
		}

		protected override void SetUp()
		{
			LoginWebUser();
			base.SetUp();
		}

		void LoginWebUser()
		{
			OrgContact contact = Factory.Load<OrgContact>(new ZGuid("f960e868-fef4-4cab-a1f5-3ace433c04e9"));
			AssertNotNull("Fixed Contact", contact);
			AssertEquals("Contact Name", "TONY MORAN - SALES", contact.OC_ContactName);
			contact.OC_WebAccessEnabled = true;
			var password = "test";
			contact.SetHashedPassword(password);

			Factory.Save();

			OrgContactWebUser user = (OrgContactWebUser)WebEnv.AppInstance.GetNewSiteUser();
			user.Login(contact.ParentOrg.OH_Code, contact.OC_Email, password);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);
			AssertEquals("Logged in Contact Name", "TONY MORAN - SALES", WebEnv.CurrentUser.Name);
			AssertEquals("IsLogged In", true, user.IsLoggedIn);
		}

		#endregion

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

			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			var provider = new TrackingInventoryDetailsColumnProvider(LoggedSiteUser.LoggedInOrganisation.PartAttributeManager);
			provider.CustomizeDictionary();
			var serialNumberColumn = new ZTextEditColumn("Serial Number", WhsInventoryViewSchema.WI_SerialNumber.Name) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.SerialNumber };
			AssertEquals(clientUsesSerialNumber, provider.AllColumns.Cast<IUniqueKeyColumn>().Any(col => col.UniqueKey == serialNumberColumn.UniqueKey));
		}

		public void TestOldColumnsOrderWithSerialNumber_SerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestOldColumnsOrderCore(clientUsesSerialNumber: true);
		}

		public void TestOldColumnsOrderWithSerialNumber_ClientDoesNotUseSerialNumber()
		{
			TestOldColumnsOrderCore(clientUsesSerialNumber: false);
		}

		void TestOldColumnsOrderCore(bool clientUsesSerialNumber)
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			LoggedSiteUser.LoggedInOrganisation.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			var provider = new TrackingInventoryDetailsColumnProvider(LoggedSiteUser.LoggedInOrganisation.PartAttributeManager);
			AssertEquals(clientUsesSerialNumber, provider.OldColumnsOrder.Contains((int)WebTracker.Grids.TrackingInventoryDetails.SerialNumber));
		}
	}
}
