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
	[TestedType(typeof(TrackingHAWBModule))]
	class TrackingHAWBModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var testHAWB = result as TrackingMAWBHeader;
			if (testHAWB != null)
			{
				testHAWB.EH_AWBType = AWBTypeList.Codes.House;
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
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

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
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

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(ExportAWBHeaderSchema.EH_AWBType, AWBTypeList.Codes.House);
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
			get { return WebModuleIDs.TrackingHAWB; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutHAWB; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingHAWBModuleForTest(Factory, TestPage);
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
			AssertNotNull("LoggedInOrg should be set on ShipmentExportAWBHeaderFilterBusinessObject creation", ((TrackingHAWBFilterBusinessObject)((TrackingHAWBModuleForTest)TestZWebModule).GetNewFilterStripBizOForTest()).LoggedInWebUsersOrg);
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
						new ColumnDetailsForTest("HAWB", i++, typeof(ZHyperLinkColumn)),
						new ColumnDetailsForTest("Origin", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Destination", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Pieces", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Act. Weight", i++, typeof(ZCalcEditColumn)),
						new ColumnDetailsForTest("Weight UQ", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("SLAC", i++, typeof(ZCalcEditColumn)),
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
						new ColumnDetailsForTest("Consignee Phone", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Status", i++, typeof(ZTextEditColumn))
					   };
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingHAWBModule)
				{
					result.Add(module.AllColumns["Origin"]);
					result.Add(module.AllColumns["Destination"]);
					result.Add(module.AllColumns["Pieces"]);
					result.Add(module.AllColumns["Act. Weight"]);
					result.Add(module.AllColumns["Weight UQ"]);
					result.Add(module.AllColumns["SLAC"]);
					result.Add(module.AllColumns["Shipper Name"]);
					result.Add(module.AllColumns["Shipper Country/Region"]);
					result.Add(module.AllColumns["Consignee Name"]);
					result.Add(module.AllColumns["Consignee Country/Region"]);
					result.Add(module.AllColumns["Status"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingHAWBModule;
				return new[] {
					module.AllColumns["HAWB"]
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

		#region TrackingHAWBModuleForTest

		class TrackingHAWBModuleForTest : TrackingHAWBModule
		{
			public TrackingHAWBModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}

		#endregion

	}
}
