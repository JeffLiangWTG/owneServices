using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingOrdersModule))]
	class TrackingOrdersModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override IList GetNewFilterGridCollection()
		{
			return new List<TrackingOrder>();
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var order = result as TrackingOrder;
			if (order != null)
			{
				order.JD_OrderNumber = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
				order.BuyerPK = SiteUser.LoggedInOrganisation.PK;
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingOrder testObject = Factory.NewWithValidTestData<TrackingOrder>();
				testObject.JD_OrderNumber = "Include" + i.ToString();
				testObject.BuyerPK = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingOrder testObject = Factory.NewWithValidTestData<TrackingOrder>();
				testObject.JD_OrderNumber = "Other" + i.ToString();
				testObject.BuyerPK = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			Dictionary<string, string> result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");
			return result;
		}

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingMilestone))
			{
				return new TrackingMilestone(string.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingOrders; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutForwardingOrders; }
		}

		protected override string ExpectedDefaultLayoutName
		{
			get { return DefaultLayoutNameValue; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingOrdersModuleForTest(Factory, TestPage);
		}

		protected override List<string> ExcludeFromExcelExportColumnsBoundTo
		{
			get
			{
				List<string> columns = new List<string>();
				columns.Add("Products");
				columns.Add("Containers");
				return columns;
			}
		}

		#endregion

		#region TestFilterBusinessObjectSetup

		public virtual void TestFilterBusinessObjectSetup()
		{
			AssertNotNull("LoggedInOrg should be set on OrdersFilterBusinessObject creation", ((OrdersFilterBusinessObject)((TrackingOrdersModuleForTest)TestZWebModule).GetNewFilterStripBizOForTest()).LoggedInWebUsersOrg);
		}

		#endregion

		#region Columns and Sorting

		public void TestAdditionalInformationColumns()
		{
			Globals.IsWeb = true;
			var testLoggedInOrg = ((OrgContactWebUser)TestPage.SiteUser).LoggedInOrganisation;
			try
			{
				GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
				testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();

				OrgCustomLabels attribute1 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
				attribute1.OT_FieldName = Constants.CustomLabels.Order.CustomAttribute3;
				attribute1.OT_Caption = "OrgProxy'sCA3";

				OrgCustomLabels attribute2 = testLoggedInOrg.CustomLabels.AddNew();
				attribute2.OT_FieldName = Constants.CustomLabels.Order.CustomAttribute3;
				attribute2.OT_Caption = "LoggedInOrg'sCA3";

				OrgCustomLabels attribute3 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
				attribute3.OT_FieldName = Constants.CustomLabels.Order.CustomFlag4;
				attribute3.OT_Caption = "OrgProxy'sCF4";

				OrgCustomLabels attribute4 = testLoggedInOrg.CustomLabels.AddNew();
				attribute4.OT_FieldName = Constants.CustomLabels.Order.UserTrackDate1;
				attribute4.OT_Caption = "LoggedInOrg'sUTD1";

				OrgCustomLabels attribute5 = testLoggedInOrg.CustomLabels.AddNew();
				attribute5.OT_FieldName = Constants.CustomLabels.Order.UserTrackDate2;
				attribute5.OT_Caption = "LoggedInOrg'sUTD1";

				AssertEquals("Should be 5 more columns", ExpectedColumnDetails.Length + 5, ((TrackingOrdersModule)TestZWebModule).GridColumnFields.Length);
				AssertHeaderExistence("OrgProxy'sCA3", ((TrackingOrdersModule)TestZWebModule).GridColumnFields, false);
				AssertHeaderExistence("LoggedInOrg'sCA3", ((TrackingOrdersModule)TestZWebModule).GridColumnFields, true);
				AssertHeaderExistence("OrgProxy'sCF4. If company has its own settings, OrgProxy ones should not be used at all", ((TrackingOrdersModule)TestZWebModule).GridColumnFields, false);
				AssertHeaderExistence("Est. LoggedInOrg'sUTD1", ((TrackingOrdersModule)TestZWebModule).GridColumnFields, true);
				AssertHeaderExistence("Act. LoggedInOrg'sUTD1", ((TrackingOrdersModule)TestZWebModule).GridColumnFields, true);
			}
			finally
			{
				GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
				testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();
			}
		}

		void AssertHeaderExistence(ZString header, DataGridColumn[] columns, ZBool shouldBeContained)
		{
			ZBool result = !shouldBeContained;
			foreach (var column in columns)
			{
				if (shouldBeContained ? column.HeaderText == header : column.HeaderText != header)
				{
					result = shouldBeContained;
					break;
				}
			}
			AssertEquals("Checking existence of " + header, shouldBeContained, result);
		}

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				int i = 0;
				return new[]
					   {
							new ColumnDetailsForTest("Order #", i++, typeof(ZButtonColumn)),
							new ColumnDetailsForTest("Split Number", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Transport Mode", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Supplier", i++, typeof(ZFindBoxColumn)),
							new ColumnDetailsForTest("Buyer", i++, typeof(ZFindBoxColumn)),
							new ColumnDetailsForTest("Controlling Customer", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Status", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Order Date", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Origin", i++, typeof(ZCodeFindBoxColumn)),
							new ColumnDetailsForTest("Destination", i++, typeof(ZCodeFindBoxColumn)),
							new ColumnDetailsForTest("Current Vessel", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Current Voyage/Flight", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Packs", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Volume", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Weight", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Last Milestone Desc.", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Last Milestone Date", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Next Milestone Desc.", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Next Milestone Date", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Req. Ex Works", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Req. In Store", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Ex-Factory", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("Origin Receival", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("Departure", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("Arrival", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("Clearance Commenced", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("Clearance Finalized", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("Unpacked", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("Port Transport Advised", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("Delivered", i++, typeof(ZTimelineColumn)),
							new ColumnDetailsForTest("House Bill", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Master Bill", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Load", i++, typeof(ZCodeFindBoxColumn)),
							new ColumnDetailsForTest("Discharge", i++, typeof(ZCodeFindBoxColumn)),
							new ColumnDetailsForTest("Pickup Address", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Delivery Address", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Consol #", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Booking Conf. Ref. #", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Container #", i++, typeof(ZHyperLinksColumn)),
							new ColumnDetailsForTest("Invoice #", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Product #", i++, typeof(ZHyperLinksColumn)),
							new ColumnDetailsForTest("Shipment #", i++, typeof(ZHyperLinkColumn)),
							new ColumnDetailsForTest("Confirmed Date", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Follow Up Date", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Sending Agent", i++, typeof(ZFindBoxColumn)),
							new ColumnDetailsForTest("Receiving Agent", i++, typeof(ZFindBoxColumn)),
							new ColumnDetailsForTest("Service Level", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Container Mode", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Created On", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Created Time", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Last Edit Time", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Main Vessel", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Main Voyage/Flight", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Planned Container #", i++, typeof(ZHyperLinksColumn)),
							new ColumnDetailsForTest("Incoterm", i++, typeof(ZDropEditColumn)),
							new ColumnDetailsForTest("Additional Terms", i++, typeof(ZTextEditColumn)),
					   };
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (TrackingOrdersModule module = FilterGridModule as TrackingOrdersModule)
				{
					result.Add(module.AllColumns["Split Number"]);
					result.Add(module.AllColumns["Transport Mode"]);
					result.Add(module.AllColumns["Supplier"]);
					result.Add(module.AllColumns["Buyer"]);
					result.Add(module.AllColumns["Controlling Customer"]);
					result.Add(module.AllColumns["Status"]);
					result.Add(module.AllColumns["Order Date"]);
					result.Add(module.AllColumns["Origin"]);
					result.Add(module.AllColumns["Destination"]);
					result.Add(module.AllColumns["Packs"]);
					result.Add(module.AllColumns["Volume"]);
					result.Add(module.AllColumns["Weight"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingOrdersModule;
				return new[] {
					module.AllColumns["Order #"]
				};
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobOrderHeaderSchema.JD_OrderDate.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get
			{
				return ListSortDirection.Descending;
			}
		}

		#endregion

		#region TrackingOrdersModuleForTest

		class TrackingOrdersModuleForTest : TrackingOrdersModule
		{
			public TrackingOrdersModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}

		#endregion

	}
}
