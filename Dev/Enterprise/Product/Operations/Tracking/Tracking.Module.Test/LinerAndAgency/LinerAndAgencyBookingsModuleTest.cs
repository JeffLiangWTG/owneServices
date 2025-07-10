using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Module;
using Enterprise.MasterFiles.Business;
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
	[TestedType(typeof(LinerAndAgencyBookingsModule))]
	sealed class LinerAndAgencyBookingsModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			BusinessObject result = null;
			if (!type.IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				if (type.Equals(typeof(JobHeader)) || type.IsSubclassOf(typeof(JobHeader)))
				{
					result = Factory.NewJobForTesting<JobHeader>();
				}
				else
				{
					result = Factory.New(type);
				}
			}
			return result;
		}

		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<AgencyBooking>();
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var booking = result as AgencyBooking;
			if (booking != null)
			{
				booking.JS_BookingReference = string.Format("A{0}", DateTime.Now.Ticks);
				booking.BookingPartyDocumentaryAddress.E2_OA_Address = SiteUser.LoggedInOrganisation.MainAddress.PK;
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				AgencyBooking testObject = Factory.NewWithValidTestData<AgencyBooking>();
				testObject.JS_BookingReference = "Include" + i.ToString();
				testObject.BookingPartyDocumentaryAddress.E2_OA_Address = SiteUser.LoggedInOrganisation.MainAddress.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				AgencyBooking testObject = Factory.NewWithValidTestData<AgencyBooking>();
				testObject.JS_BookingReference = "Other" + i.ToString();
				testObject.BookingPartyDocumentaryAddress.E2_OA_Address = SiteUser.LoggedInOrganisation.MainAddress.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(JobShipmentSchema.JS_BookingReference, SQLComparisonOperator.StartsWith, "Include");
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
			get { return WebModuleIDs.LinerAndAgencyBookings; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new LinerAndAgencyBookingsModuleForTest(Factory, TestPage);
		}

		#endregion

		#region TestFilterBusinessObjectSetup

		public void TestFilterBusinessObjectSetup()
		{
			var bizO = ((LinerAndAgencyBookingsModuleForTest)TestZWebModule).GetNewFilterStripBizOForTest();
			var clientFilter = bizO[AgencyShipmentFilterStrip.Descriptions.BookingParty] as ModuleGuidFilter;

			AssertEquals("LoggedInOrg was set", ZGuid.Empty, clientFilter.Property);
			Assert("Client filter was made active", clientFilter.IsActive);
			AssertEquals("Client filter was made AlwaysAppliedAndHidden", FilterVisibility.AlwaysAppliedAndHidden, clientFilter.Visibility);
		}

		#endregion

		#region Columns and Sorting

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				var module = FilterGridModule as LinerAndAgencyBookingsModule;
				return new[]
				{
					module.AllColumns["Ocean Bill"],
					module.AllColumns["Status"],
					module.AllColumns["Vessel"],
					module.AllColumns["Voyage No."],
					module.AllColumns["Load"],
					module.AllColumns["Disch."],
					module.AllColumns["Dest."],
					module.AllColumns["Cargo Desc."]
				};
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				return new DataGridColumn[]
				{
					(FilterGridModule as LinerAndAgencyBookingsModule).AllColumns["Shipment #"]
				};
			}
		}

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				List<ColumnDetailsForTest> result = new List<ColumnDetailsForTest>();
				int i = 0;

				result.Add(new ColumnDetailsForTest("Shipment #", i++, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("Ocean Bill", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Consignor", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Consignee", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Status", i++, typeof(ZDropDownListColumn)));
				result.Add(new ColumnDetailsForTest("Vessel", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Voyage No.", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Origin", i++, typeof(ZCodeFindBoxColumn)));
				result.Add(new ColumnDetailsForTest("Load", i++, typeof(ZCodeFindBoxColumn)));
				result.Add(new ColumnDetailsForTest("Disch.", i++, typeof(ZCodeFindBoxColumn)));
				result.Add(new ColumnDetailsForTest("Dest.", i++, typeof(ZCodeFindBoxColumn)));
				result.Add(new ColumnDetailsForTest("Cargo Desc.", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Packs", i++, typeof(ZGroupColumn)));
				result.Add(new ColumnDetailsForTest("Weight", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Volume", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Cargo Type", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("ETD", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("ETA", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Cnr. Contact", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Shipper's Ref#", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Booked", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CFS Recv.", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CFS Cut", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CTO Recv.", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CTO Cut", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Job Status", i++, typeof(ZTextEditColumn)));

				return result.ToArray();
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobShipmentSchema.JS_A_BKD.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutLinerAndAgencyBookings; }
		}

		#endregion

		#region LinerAndAgencyBookingsModuleForTest

		class LinerAndAgencyBookingsModuleForTest : LinerAndAgencyBookingsModule
		{
			public LinerAndAgencyBookingsModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}

		#endregion
	}
}
