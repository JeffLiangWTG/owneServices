using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingOrderLinesModule))]
	class TrackingOrderLinesModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.BuyerPK = SiteUser.LoggedInOrganisation.PK;
			var result = order.OrderLines.AddNew();
			result.JO_Description = DateTime.Now.Ticks.ToString();
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				Order testOrder = Factory.NewWithValidTestData<Order>();
				testOrder.BuyerPK = SiteUser.LoggedInOrganisation.PK;
				OrderLine testObject = testOrder.OrderLines.AddNew();
				testObject.JO_Description = "Include" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				Order testOrder = Factory.NewWithValidTestData<Order>();
				testOrder.BuyerPK = SiteUser.LoggedInOrganisation.PK;
				OrderLine testObject = testOrder.OrderLines.AddNew();
				testObject.JO_Description = "Other" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(JobOrderLineSchema.JO_Description, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(OrderLine))
			{
				return Factory.NewWithValidTestData<OrderLine>();
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingOrderLines; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutForwardingOrderLines; }
		}

		protected override string ExpectedDefaultLayoutName
		{
			get { return DefaultLayoutNameValue; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingOrderLinesModuleForTest(Factory, TestPage);
		}

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");
			return result;
		}

		#endregion

		#region TestFilterBusinessObjectSetup

		public virtual void TestFilterBusinessObjectSetup()
		{
			AssertNotNull("LoggedInOrg should be set on OrdersFilterBusinessObject creation", ((OrderLineFilterBusinessObject)((TrackingOrderLinesModuleForTest)TestZWebModule).GetNewFilterStripBizOForTest()).LoggedInWebUsersOrg);
		}

		#endregion
		#region Columns and Sorting

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				int i = 0;
				return new[]
					   {
						new ColumnDetailsForTest("Line", i++, typeof(ZButtonColumn)),
						new ColumnDetailsForTest("Order #", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Line #", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Line Split #", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Part #", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Quantity", i++, typeof(ZGroupColumn)),
					   };
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (TrackingOrderLinesModule module = FilterGridModule as TrackingOrderLinesModule)
				{
					result.Add(module.AllColumns["Order #"]);
					result.Add(module.AllColumns["Line #"]);
					result.Add(module.AllColumns["Line Split #"]);
					result.Add(module.AllColumns["Part #"]);
					result.Add(module.AllColumns["Quantity"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingOrderLinesModule;
				return new[] {
					module.AllColumns["Line"]
				};
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobOrderLineSchema.JO_LineNo.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get
			{
				return ListSortDirection.Descending;
			}
		}

		#endregion

		#region TrackingOrdersModuleForTest

		class TrackingOrderLinesModuleForTest : TrackingOrderLinesModule
		{
			public TrackingOrderLinesModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}

		#endregion

	}
}
