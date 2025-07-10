using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
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
	[TestedType(typeof(TrackingUSCForeignPortModule))]
	sealed class TrackingUSCForeignPortModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override bool AllowActiveStatusFilterTest()
		{
			return false;
		}

		protected override ZQuery GetFilterForExpectedCount(ZFilterGridModule module, FilterBusinessObject filterBizO) => module.GridCollection.CompleteFilter;

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
				var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Include" + i.ToString(), "Include" + i.ToString(), ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
				helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
				result.Add(foreignPort);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
				var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Other" + i.ToString(), "Other" + i.ToString(), ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
				helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
				result.Add(foreignPort);
			}
			return result;
		}

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Other", "Other", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);

			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Other1", "Other1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.AES);

			var foreignPort2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Other2", "Other2", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort2.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Other3", "Other3", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Other4", "Other4", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			return foreignPort;
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
			get { return WebModuleIDs.TrackingUSCForeignPort; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutUSCForeignPort; }
		}

		protected override string ExpectedDefaultLayoutName
		{
			get { return DefaultLayoutNameValue; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingUSCForeignPortModuleForTest(Factory, TestPage);
		}

		#endregion
		#region Columns and Sorting

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingUSCForeignPortModule;
				return new[]
					   {
							module.AllColumns["Code"]
					   };
			}
		}

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				int i = 0;
				return new[]
					   {
						new ColumnDetailsForTest("Code", i++, typeof(ZButtonColumn)),
						new ColumnDetailsForTest("Name", i++, typeof(ZTextEditColumn))
					   };
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (TrackingUSCForeignPortModule module = FilterGridModule as TrackingUSCForeignPortModule)
				{
					result.Add(module.AllColumns["Name"]);
				}

				return result.ToArray();
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(ZZRefCusCodeListCombinedSchema.ZZD_Code.Name, ListSortDirection.Ascending) };

		#endregion

		#region TrackingOrdersModuleForTest

		class TrackingUSCForeignPortModuleForTest : TrackingUSCForeignPortModule
		{
			public TrackingUSCForeignPortModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}

		#endregion

	}
}
