using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingImporterSecurityFilingModule))]
	sealed class TrackingImporterSecurityFilingModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override System.Collections.IList GetNewFilterGridCollection() => new List<TrackingCusISFHeader>();

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var header = result as TrackingCusISFHeader;
			if (header != null)
			{
				header.FirstTransport.JW_ETD = ZDateTime.Now;
				header.BF_JobReference = header.PK.ToString().Substring(0, header.BF_JobReferenceInfo.MaxLength);
				header.BF_OH_Importer = SiteUser.LoggedInOrganisation.PK;
			}

			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<TrackingCusISFHeader>();
				testObject.FirstTransport.JW_ETD = ZDateTime.Now;
				testObject.BF_JobReference = "I" + i.ToString();
				testObject.BF_OH_Importer = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<TrackingCusISFHeader>();
				testObject.FirstTransport.JW_ETD = ZDateTime.Now;
				testObject.BF_JobReference = "O" + i.ToString();
				testObject.BF_OH_Importer = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}

			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(CusISFHeaderSchema.BF_JobReference, SQLComparisonOperator.StartsWith, "I");
		}

		#endregion

		protected override bool ExpectCachingOfCollectionKeys => false;

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");

			return result;
		}

		public void TestLoadCollectionReturnsControledOrgsISFs()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			helper.TestSiteUser.Logout();

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject();

			FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("No ISF", 0, FilterGridModule.GridCollection.Count);

			helper.TestSiteUser.Logout();

			var controllingOrgHeader = Factory.New<OrgHeader>();
			controllingOrgHeader.OH_Code = "AAA";
			controllingOrgHeader.OH_IsActive = true;
			controllingOrgHeader.SetRelatedParty(helper.TestOrg, RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Pickup);

			var testISF = Factory.NewWithValidTestData<TrackingCusISFHeader>();
			testISF.BF_OH_Importer = controllingOrgHeader.PK;

			Factory.Save();

			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			var newFilterBizO = FilterGridModule.CreateNewFilterBusinessObject();

			FilterGridModule.LoadCollection(newFilterBizO);
			AssertEquals("testISF should be retreived", 1, FilterGridModule.GridCollection.Count);
		}

		public override void TestLoadCollectionReturnsRowCount()
		{
			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject();

			AssertEquals("FilterGridModule should limit to 1000 rows by default", 1000, FilterGridModule.MaxRows);
			FilterGridModule.MaxRows = 250;
			FilterGridModule.LoadCollection(filterBizO);
			if (!FilterGridModule.GridCollection.TypeOfElements.IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				var filter = ((TrackingISFModuleForTest)FilterGridModule).GetCurrentLoggedInUserFilter_Exposed();
				var query = new ZQuery(filter, JoinCondition.And, filterBizO.Filter);
				var expectedCount = Factory.GetDatabaseCount(FilterGridModule.GridCollection.TypeOfElements, query);

				expectedCount = (expectedCount > FilterGridModule.MaxRows) ? FilterGridModule.MaxRows : expectedCount;
				AssertEquals("LoadCollection should not have returned more than 250 rows", expectedCount, FilterGridModule.GridCollection.Count);
			}
			else
			{
				Assert("LoadCollection should not have returned more than 250 rows", FilterGridModule.GridCollection.Count <= 250);
			}
		}

		protected override WebModuleID TestID => WebModuleIDs.TrackingImporterSecurityFiling;

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutISF;

		protected override ZWebModule GetNewZWebModule() => new TrackingISFModuleForTest(TestPage);

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingCusISFHeader))
			{
				return Factory.New<TrackingCusISFHeader>();
			}
			else
			{
				return base.GetNewBizObjOfType(type);
			}
		}

		#endregion

		#region Columns and Sorting

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingISFModuleForTest)
				{
					result.Add(module.AllColumns["Customs Reference"]);
					result.Add(module.AllColumns["Importer"]);
					result.Add(module.AllColumns["House Bill"]);
					result.Add(module.AllColumns["Status"]);
					result.Add(module.AllColumns["Selling Party"]);
					result.Add(module.AllColumns["Buying Party"]);
					result.Add(module.AllColumns["Main Ship To Party"]);
					result.Add(module.AllColumns["Action Reason Code"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns => new[] { (FilterGridModule as TrackingISFModuleForTest).AllColumns["Job Ref."] };

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var result = new List<ColumnDetailsForTest>();
				var i = 0;

				result.Add(new ColumnDetailsForTest("Job Ref.", i++, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("Customs Reference", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Importer", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("House Bill", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Status", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Selling Party", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Buying Party", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Main Ship To Party", i++, typeof(ZTextEditColumn)));

				result.Add(new ColumnDetailsForTest("Carrier SCAC", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("ID Type", i++, typeof(ZTextEditColumn)));

				result.Add(new ColumnDetailsForTest("Importer Id Type", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Importer Identification", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("DOB", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Issue Ctry/Rgn.", i++, typeof(ZTextEditColumn)));

				result.Add(new ColumnDetailsForTest("Unload Port", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Delivery Port", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Bond Holder", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Surety Code", i++, typeof(ZTextEditColumn)));

				result.Add(new ColumnDetailsForTest("Cnee. ID Type", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Cnee. ID", i++, typeof(ZTextEditColumn)));

				result.Add(new ColumnDetailsForTest("Ocean Bill", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Master Bill", i++, typeof(ZTextEditColumn)));

				result.Add(new ColumnDetailsForTest("1st US Route Vessel", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("1st US Route Voyage/Flight", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("1st US Route Load Port", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("1st US Route Discharge Port", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("1st US Route ETD", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("1st US Route ETA", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("1st US Route ATD", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("1st US Route ATA", i++, typeof(ZDateTimeColumn)));

				result.Add(new ColumnDetailsForTest("Action Reason Code", i++, typeof(ZTextEditColumn)));

				result.Add(new ColumnDetailsForTest("First Accepted Date", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Last Accepted Date", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Created Time", i++, typeof(ZDateTimeColumn)));

				return result.ToArray();
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(CusISFHeaderSchema.BF_JobReference.Name, ListSortDirection.Ascending) };

		#endregion

		#region TrackingISFModuleForTest

		class TrackingISFModuleForTest : TrackingImporterSecurityFilingModule
		{
			public TrackingISFModuleForTest(ZPage page)
				: base(new BusinessObjectFactory(), page)
			{
			}

			public ZQuery GetCurrentLoggedInUserFilter_Exposed() => base.GetCurrentLoggedInUserFilter(null);
		}

		#endregion
	}
}
