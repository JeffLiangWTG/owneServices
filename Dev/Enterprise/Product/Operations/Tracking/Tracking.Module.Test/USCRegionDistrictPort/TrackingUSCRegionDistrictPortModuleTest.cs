using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
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
	[TestedType(typeof(TrackingUSCRegionDistrictPortModule))]
	sealed class TrackingUSCRegionDistrictPortModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override bool AllowActiveStatusFilterTest()
		{
			return false;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
				var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Include" + i.ToString(), "Include" + i.ToString(), ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
				result.Add(port);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
				var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Other" + i.ToString(), "Other" + i.ToString(), ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
				result.Add(port);
			}
			return result;
		}

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var port = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Other", "Other", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Other1", "Other1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Other2", "Other2", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Other3", "Other3", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Other4", "Other4", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			return port;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Description, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(ZZRefCusCodeListCombined))
			{
				return Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingUSCRegionDistrictPort; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutUSCRegionDistrictPort; }
		}

		protected override string ExpectedDefaultLayoutName
		{
			get { return DefaultLayoutNameValue; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingUSCRegionDistrictPortModuleForTest(Factory, TestPage);
		}

		#endregion
		#region Columns and Sorting

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var i = 0;
				return new[]
					   {
						new ColumnDetailsForTest("Code", i++, typeof(ZButtonColumn)),
						new ColumnDetailsForTest("Name", i++, typeof(ZTextEditColumn))
					   };
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingUSCRegionDistrictPortModule;
				return new[]
				{
					module.AllColumns["Code"]
				};
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingUSCRegionDistrictPortModule)
				{
					result.Add(module.AllColumns["Name"]);
				}

				return result.ToArray();
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(ZZRefCusCodeListCombinedSchema.ZZD_Code.Name, ListSortDirection.Ascending) };

		#endregion

		#region TrackingOrdersModuleForTest

		class TrackingUSCRegionDistrictPortModuleForTest : TrackingUSCRegionDistrictPortModule
		{
			public TrackingUSCRegionDistrictPortModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}

		#endregion

	}
}
