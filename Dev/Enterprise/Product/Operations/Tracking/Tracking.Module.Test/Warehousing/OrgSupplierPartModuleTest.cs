using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
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
	[TestedType(typeof(OrgSupplierPartModule))]
	class OrgSupplierPartModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				OrgSupplierPart testObject = Factory.NewWithValidTestData<OrgSupplierPart>();
				testObject.OP_CustomAttrib1 = "Include" + i.ToString();
				testObject.RelatedOrganisations.AddOwner(SiteUser.LoggedInOrganisation);
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				OrgSupplierPart testObject = Factory.NewWithValidTestData<OrgSupplierPart>();
				testObject.OP_CustomAttrib1 = "Other" + i.ToString();
				testObject.RelatedOrganisations.AddOwner(SiteUser.LoggedInOrganisation);
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(OrgSupplierPartSchema.OP_CustomAttrib1, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var element = base.GetNewElement(elementType, isCancelled);
			var part = element as OrgSupplierPart;
			if (part != null)
			{
				part.RelatedOrganisations.AddOwner(SiteUser.LoggedInOrganisation);
			}
			return element;
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.OrgSupplierPartTracking; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutWarehouseProducts; }
		}

		protected override string ExpectedDefaultLayoutName
		{
			get { return DefaultLayoutNameValue; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new OrgSupplierPartModuleForTest(Factory, TestPage);
		}

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			return new Dictionary<string, string>()
			{
				{ "Created Time", "Created Time" },
				{ "Last Edit Time", "Last Edit Time" },
				{ "Created On Web/Internal", "Created On Web/Internal" },
			};
		}

		#endregion

		#region Columns and Sorting

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				List<ColumnDetailsForTest> result = new List<ColumnDetailsForTest>();

				result.Add(new ColumnDetailsForTest("Product#", 0, typeof(ZButtonColumn)));
				result.Add(new ColumnDetailsForTest("Description", 1, typeof(ZTextEditColumn)));

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				return new DataGridColumn[]
				{
					(FilterGridModule as OrgSupplierPartModule).AllColumns["Description"]
				};
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				return new DataGridColumn[]
				{
					(FilterGridModule as OrgSupplierPartModule).AllColumns["Product#"]
				};
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(OrgSupplierPartSchema.OP_PartNum.Name, ListSortDirection.Ascending) };

		#endregion

		#region OrgSupplierPartModule For Test

		protected class OrgSupplierPartModuleForTest : OrgSupplierPartModule
		{
			public OrgSupplierPartModuleForTest(BusinessObjectFactory factory, ZPage page)
				: base(factory, page)
			{
			}

			protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => OrgRestrictionFilterFactory.Instance.GetSubQuery(typeof(OrgSupplierPart), typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
		}

		#endregion
	}
}
