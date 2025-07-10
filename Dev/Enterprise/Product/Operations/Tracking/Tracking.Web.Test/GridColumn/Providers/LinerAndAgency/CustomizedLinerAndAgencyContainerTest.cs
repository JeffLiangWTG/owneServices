using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.GridColumn.Providers.LinerAndAgency;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CustomizedLinerAndAgencyContainerColumnProvider))]
	[HttpContextEnabledTest]
	class CustomizedLinerAndAgencyContainerTest : GridColumnProviderTest
	{
		public void TestShipmentQuickViewWhenContainerQuickViewEnabled()
		{
			using (WebDataRegistry.Instance.WebTrackerContainerQuickView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				LoginTestUser(true);
				TestProvider = GetNewTestProvider();
				TestProvider.CustomizeDictionary();
				SetupColumns();
				TestDefaultColumns();
				TestRequiredColumns();

				var column = TestProvider.RequiredGridColumnFields.First() as ZHyperLinkColumn;
				AssertEquals("ContainerQuickViewNumber is in the query string", column.DataNavigateUrlFormatString, "/LinerAndAgency/LinerAndAgencyContainers/LinerAndAgencyContainerDetails.aspx?Ref={0}&ContainerQuickViewNumber={1}");
			}
		}

		public void TestShipmentQuickViewWhenContainerQuickViewDisabled()
		{
			using (WebDataRegistry.Instance.WebTrackerContainerQuickView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				LoginTestUser(true);
				TestProvider = GetNewTestProvider();
				TestProvider.CustomizeDictionary();
				SetupColumns();
				TestDefaultColumns();
				TestRequiredColumns();

				AssertNotNull("Container # column is a text column", TestProvider.DefaultGridColumnFields.First() as ZTextEditColumn);
			}
		}

		public void TestNonQuickViewWhenContainerQuickViewEnabled()
		{
			using (WebDataRegistry.Instance.WebTrackerContainerQuickView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				LoginTestUser(false);
				TestProvider = GetNewTestProvider();
				TestProvider.CustomizeDictionary();
				SetupColumns();
				TestDefaultColumns();
				TestRequiredColumns();

				var column = TestProvider.RequiredGridColumnFields.First() as ZHyperLinkColumn;
				AssertEquals("ContainerQuickViewNumber is NOT in the query string", column.DataNavigateUrlFormatString, "/LinerAndAgency/LinerAndAgencyContainers/LinerAndAgencyContainerDetails.aspx?Ref={0}");
			}
		}

		public void TestNonQuickViewWhenContainerQuickViewDisabled()
		{
			using (WebDataRegistry.Instance.WebTrackerContainerQuickView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				LoginTestUser(false);
				TestProvider = GetNewTestProvider();
				TestProvider.CustomizeDictionary();
				SetupColumns();
				TestDefaultColumns();
				TestRequiredColumns();

				var column = TestProvider.RequiredGridColumnFields.First() as ZHyperLinkColumn;
				AssertEquals("ContainerQuickViewNumber is NOT in the query string", column.DataNavigateUrlFormatString, "/LinerAndAgency/LinerAndAgencyContainers/LinerAndAgencyContainerDetails.aspx?Ref={0}");
			}
		}

		public virtual void TestContainerColumnKeys()
		{
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			TestColumnKeys();
			TestUniqueColumns();
			TestDefaultColumns();
			TestRequiredColumns();
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			if ((testHelper?.TestSiteUser?.IsShipmentQuickViewUser ?? false) && !WebDataRegistry.Instance.WebTrackerContainerQuickView.Value)
			{
				AddDefaultsColumn(new ZTextEditColumn("Container #", LinerAndAgencyContainer.Schema.JC_ContainerNum)
				{
					ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber,
				});
			}
			else
			{
				AddRequiredColumn(new ZHyperLinkColumn("Container #", LinerAndAgencyContainer.Schema.JC_ContainerNum)
				{
					ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber,
				});
			}

			AddDefaultsColumn(new ZGuidDropDownListColumn("Type", LinerAndAgencyContainer.Schema.JC_RC) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.TypeDescription });
			AddDefaultsColumn(new ZCalcEditColumn("Count", LinerAndAgencyContainer.Schema.JC_ContainerCount) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainersCount });
			AddDefaultsColumn(new ZCalcEditColumn("Net Wt.", LinerAndAgencyContainer.Schema.JC_Calc_NetWeight) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.NetWeight });
			AddDefaultsColumn(new ZCalcEditColumn("Tare Wt.", LinerAndAgencyContainer.Schema.JC_TareWeight) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.TareWeight });
			AddDefaultsColumn(new ZCalcEditColumn("Gross Wt.", LinerAndAgencyContainer.Schema.JC_GrossWeight) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.GrossWeight });
			AddDefaultsColumn(new ZDropDownListColumn("WQ", LinerAndAgencyContainer.Schema.JC_GrossWeightUQ) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.WQ });

			AddDefaultsColumn(new ZCodeFindBoxColumn("Commodity", LinerAndAgencyContainer.Schema.JC_RH_NKContainerCommodityCode) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Commodity });

			AddDefaultsColumn(new ZCheckBoxColumn("Is Shipper Owned", LinerAndAgencyContainer.Schema.JC_IsShipperOwned) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.IsShipperOwned });
			AddDefaultsColumn(new ZTextEditColumn("Seal #", LinerAndAgencyContainer.Schema.JC_SealNum) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.SealNumber });

			AddDefaultsColumn(new ZDateTimeColumn("Verified Date", LinerAndAgencyContainer.Schema.JC_GrossWeightVerificationDateTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedDate });
			AddDefaultsColumn(new ZTextEditColumn("Verified Method", LinerAndAgencyContainer.Schema.VerifiedMethod) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedMethod });
			AddDefaultsColumn(new ZTextEditColumn("Verified Company", LinerAndAgencyContainer.Schema.VerifiedByCompany) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedCompany });
			AddDefaultsColumn(new ZTextEditColumn("Verified Contact", LinerAndAgencyContainer.Schema.VerifiedByPerson) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedContact });
			AddDefaultsColumn(new ZTextEditColumn("Verified Phone", LinerAndAgencyContainer.Schema.VerifiedByPhone) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedPhone });
			AddDefaultsColumn(new ZTextEditColumn("Verified Email", LinerAndAgencyContainer.Schema.VerifiedByEmail) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedEmail });

			AddColumn(new ZTextEditColumn("Empty Pickup From", "DepartureContainerYardAddress.AddressDescription") { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyPickupFrom });
			AddColumn(new ZDateTimeColumn("Empty Released", LinerAndAgencyContainer.Schema.JC_ContainerYardEmptyPickupGateOut) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReleased });
			AddColumn(new ZDateTimeColumn("Wharf Gate In", LinerAndAgencyContainer.Schema.JC_FCLWharfGateIn) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.WharfGateIn });
			AddColumn(new ZDateTimeColumn("Loaded", LinerAndAgencyContainer.Schema.JC_FCLOnBoardVessel) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Loaded });

			AddColumn(new ZTextEditColumn("Empty Return To", "ArrivalContainerYardAddress.AddressDescription") { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReturnTo });
			AddColumn(new ZDateTimeColumn("Empty Return By", LinerAndAgencyContainer.Schema.JC_EmptyReturnedBy) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReturnBy });
			AddColumn(new ZDateTimeColumn("Unloaded", LinerAndAgencyContainer.Schema.JC_FCLUnloadFromVessel) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Unloaded });
			AddColumn(new ZDateTimeColumn("Wharf Gate Out", LinerAndAgencyContainer.Schema.JC_FCLWharfGateOut) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.WharfGateOut });
			AddColumn(new ZDateTimeColumn("Empty Returned", LinerAndAgencyContainer.Schema.JC_ContainerYardEmptyReturnGateIn) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReturned });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.LinerAndAgencyContainers.Commodity
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CustomizedLinerAndAgencyContainerColumnProvider();
		}

		protected override void SetUp()
		{
			testHelper = new TestHelper(Factory);

			base.SetUp();
		}

		void LoginTestUser(bool isShipmentQuickViewUser)
		{
			testHelper.TestSiteUser.LoginSupportForTest(isShipmentQuickViewUser ? GlbCompany.CurrentCompany.OrgProxy.OH_Code : ZString.Empty);
		}

		TestHelper testHelper;
	}
}
