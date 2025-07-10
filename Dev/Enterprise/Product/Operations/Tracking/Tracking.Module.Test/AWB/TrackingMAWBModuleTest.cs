using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingMAWBModule))]
	class TrackingMAWBModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<TrackingMAWBHeader>();
				testObject.EH_AWBType = AWBTypeList.Codes.AgentMaster;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<TrackingMAWBHeader>();
				testObject.EH_AWBType = AWBTypeList.Codes.House;
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(ExportAWBHeaderSchema.EH_AWBType, AWBTypeList.Codes.AgentMaster);
		}

		#endregion

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = base.GetExpectedAuditFilters();
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
			get { return WebModuleIDs.TrackingMAWB; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutMAWB; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingMAWBModuleForTest(Factory, TestPage);
		}

		protected override List<string> ExcludeFromExcelExportColumnsBoundTo
		{
			get
			{
				List<string> columns = new List<string>();
				return columns;
			}
		}

		#endregion

		#region TestFilterBusinessObjectSetup

		public virtual void TestFilterBusinessObjectSetup()
		{
			AssertNotNull("LoggedInOrg should be set on ConsolExportAWBHeaderFilterBusinessObject creation", ((TrackingMAWBFilterBusinessObject)((TrackingMAWBModuleForTest)TestZWebModule).GetNewFilterStripBizOForTest()).LoggedInWebUsersOrg);
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
						new ColumnDetailsForTest("MAWB", i++, typeof(ZHyperLinkColumn)),
						new ColumnDetailsForTest("Origin", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Departure and Routing", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Destination", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Destination Name", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Issue Date", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Issue Place", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("To 1st", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("By 1st", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("To 2nd", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("By 2nd", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("To 3rd", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("By 3rd", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("1st Carrier", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("1st Flight", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("1st Flight Date", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("2nd Carrier", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("2nd Flight", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("2nd Flight Date", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Currency", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Charge Code", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("WT/VAL", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Declared Value", i++, typeof(ZCalcEditColumn)),
						new ColumnDetailsForTest("Customs Value", i++, typeof(ZCalcEditColumn)),
						new ColumnDetailsForTest("Status", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Name", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Address", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Address 2", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Place", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper State", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Postal Code", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Country/Region", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Phone", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Name", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Address", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Address 2", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Place", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee State", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Postal Code", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Country/Region", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Phone", i++, typeof(ZTextEditColumn))
					   };
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingMAWBModule)
				{
					result.Add(module.AllColumns["Origin"]);
					result.Add(module.AllColumns["Destination"]);
					result.Add(module.AllColumns["Issue Date"]);
					result.Add(module.AllColumns["To 1st"]);
					result.Add(module.AllColumns["By 1st"]);
					result.Add(module.AllColumns["1st Carrier"]);
					result.Add(module.AllColumns["1st Flight"]);
					result.Add(module.AllColumns["1st Flight Date"]);
					result.Add(module.AllColumns["Currency"]);
					result.Add(module.AllColumns["Charge Code"]);
					result.Add(module.AllColumns["WT/VAL"]);
					result.Add(module.AllColumns["Declared Value"]);
					result.Add(module.AllColumns["Customs Value"]);
					result.Add(module.AllColumns["Status"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingMAWBModule;
				return new[] {
					module.AllColumns["MAWB"]
				};
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(ExportAWBHeaderSchema.EH_AWBIssueDate.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get
			{
				return ListSortDirection.Descending;
			}
		}

		#endregion

		#region TrackingMAWBModuleForTest

		class TrackingMAWBModuleForTest : TrackingMAWBModule
		{
			public TrackingMAWBModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}

		#endregion

	}
}
