using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingCartageModule))]
	sealed class TrackingCartageModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<TrackingCartage>();
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var cartage = result as TrackingCartage;
			if (cartage != null)
			{
				cartage.JJ_DropMode = "TST";
				cartage.JJ_OH_ClientID = SiteUser.LoggedInOrganisation.PK;
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingCartage testObject = Factory.NewWithValidTestData<TrackingCartage>();
				testObject.JJ_DropMode = "I" + i.ToString();
				testObject.JJ_OH_ClientID = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingCartage testObject = Factory.NewWithValidTestData<TrackingCartage>();
				testObject.JJ_DropMode = "O" + i.ToString();
				testObject.JJ_OH_ClientID = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(JobCartageSchema.JJ_DropMode, SQLComparisonOperator.StartsWith, "I");
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

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingCartage; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get
			{
				return WebDataRegistry.Instance.DefaultFilterLayoutTrackingCartage;
			}
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingCartageModuleForTest(TestPage);
		}

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingMilestone))
			{
				return new TrackingMilestone(string.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			}
			if (type == typeof(TrackingCartage))
			{
				return Factory.New<TrackingCartage>();
			}
			if (type.Equals(typeof(JobHeader)) || type.IsSubclassOf(typeof(JobHeader)))
			{
				return Factory.NewJobForTesting<JobHeader>();
			}
			else
			{
				return base.GetNewBizObjOfType(type);
			}
		}

		#endregion

		#region Columns and Sorting

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (TrackingCartageModuleForTest module = FilterGridModule as TrackingCartageModuleForTest)
				{
					result.Add(module.AllColumns["Job ID"]);
				}
				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (TrackingCartageModuleForTest module = FilterGridModule as TrackingCartageModuleForTest)
				{
					result.Add(module.AllColumns["Type"]);
					result.Add(module.AllColumns["Ref #"]);
					result.Add(module.AllColumns["Quote #"]);
					result.Add(module.AllColumns["Waybill #"]);
					result.Add(module.AllColumns["Description"]);
					result.Add(module.AllColumns["Comp. Date"]);
					result.Add(module.AllColumns["Vessel"]);
					result.Add(module.AllColumns["Voyage/Flight"]);
					result.Add(module.AllColumns["Stor."]);
					result.Add(module.AllColumns["Avail."]);
					result.Add(module.AllColumns["Rec. Comm."]);
					result.Add(module.AllColumns["Cut Off"]);
					result.Add(module.AllColumns["Drop Mode"]);
					result.Add(module.AllColumns["Container"]);
				}

				return result.ToArray();
			}
		}

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				List<ColumnDetailsForTest> result = new List<ColumnDetailsForTest>();
				int i = 0;

				result.Add(new ColumnDetailsForTest("Job ID", i++, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("Type", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("First Address", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Second Address", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Third Address", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Fourth Address", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Local Client", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Ref #", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Quote #", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Waybill #", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Description", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Comp. Date", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Vessel", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Voyage/Flight", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Stor.", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Avail.", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Rec. Comm.", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Cut Off", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Drop Mode", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Container", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Service Level", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Job Status", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Sailing ATA", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Sailing ATD", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CFS Storage Date", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CFS Availability Date", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CFS Receival Start", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CFS Cutoff", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Estimated Time of Arrival", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Estimated Time of Departure", i++, typeof(ZDateTimeColumn)));

				return result.ToArray();
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobCartageSchema.JJ_ConsignmentID.Name, ListSortDirection.Ascending) };

		#endregion

		#region TrackingISFModuleForTest

		class TrackingCartageModuleForTest : TrackingCartageModule
		{
			public TrackingCartageModuleForTest(ZPage page)
				: base(new BusinessObjectFactory(), page)
			{
			}

			public DataGridColumn[] GetNewGridColumnFieldsForTest()
			{
				return GetNewGridColumnFields();
			}
		}

		#endregion

		#region DummyPage

		class DummyPage : ZPage
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}

		#endregion
	}
}
