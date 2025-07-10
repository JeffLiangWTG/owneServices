using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCommodityCodeModule))]
	sealed class RefCommodityCodeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefCommodityCode;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			refCommodityCode = new RefCommodityCodeModuleForTest();
			IFilterControl controlForTest = refCommodityCode.GetNewFilterControlForTest();
			Assert(controlForTest is RefCommodityCodeFilterControl);
			controlForTest.Dispose();
			refCommodityCode.Dispose();
		}

		public void TestGridCollection()
		{
			refCommodityCode = new RefCommodityCodeModuleForTest();
			IBusinessObjectCollection collectionForTest = refCommodityCode.GetNewGridCollectionForTest();
			Assert(collectionForTest is ActiveBusinessObjectCollection<RefCommodityCode>);
			refCommodityCode.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			refCommodityCode = new RefCommodityCodeModuleForTest();
			FilterBusinessObject businessForTest = refCommodityCode.GetNewFilterBusinessObjectForTest();
			Assert(businessForTest is FilterBusinessObject);
			refCommodityCode.Dispose();
		}

		[RequiresSTA]
		public void TestColumnNMFCShouldOnlyBeVisibleWhenLogInCountryIsUS_CA_MX()
		{
			var list = new List<GlbBranch>
			{
				GetGlbBranch("US"),
				GetGlbBranch("CA"),
				GetGlbBranch("MX")
			};

			foreach (var glbBranch in list)
			{
				using var userContext = Env.SetTemporaryUserContext(Env.CurrentUser.PK, glbBranch.PK.ToGuid(), Guid.Empty);
				using var module = new RefCommodityCodeModuleForTest();
				using var filterControl = module.GetNewFilterControlForTest();

				AssertCollectionContains
				(
					$"Column NMFC should be available in {glbBranch.Company.GC_RN_NKCountryCode}",
					filterControl.FilteredGrid.ColumnStyles.OfType<ZGridColumnInfo>(),
					col => col.ColumnName == "RH_FN_NKNMFC"
				);
			}

			using var userContextAU = Env.SetTemporaryUserContext(Env.CurrentUser.PK, GetGlbBranch("AU").PK.ToGuid(), Guid.Empty);
			using var moduleAU = new RefCommodityCodeModuleForTest();
			using var filterControlAU = moduleAU.GetNewFilterControlForTest();

			AssertCollectionNotContains
			(
				"Column NMFC should not be available in AU",
				filterControlAU.FilteredGrid.ColumnStyles.OfType<ZGridColumnInfo>(),
				col => col.ColumnName == "RH_FN_NKNMFC"
			);
		}

		[RequiresSTA]
		public void TestColumnsExistAndInvisible()
		{
			var columnsToTest = new HashSet<string>
			{
				"RH_UniversalCommodityGroup",
				"RH_ExpiryDate",
				"RH_ReeferMinTemperature",
				"RH_ReeferMaxTemperature",
				"RH_ContainerVentRequired",
				"LocalCodesAsString",
				"RatingCodesAsString"
			};
			using var module = new RefCommodityCodeModuleForTest();
			using var filterControl = module.GetNewFilterControlForTest();

			var columns = filterControl.FilteredGrid.ColumnStyles.OfType<ZGridColumnInfo>()
				.Where(col => columnsToTest.Contains(col.ColumnName) && !col.IsVisible)
				.Select(col => col.ColumnName);

			AssertContainsExactElementsInAnyOrder(columnsToTest, columns);
		}

		GlbBranch GetGlbBranch(string countryCode)
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = countryCode;

			var glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;

			Factory.Save();

			return glbBranch;
		}

		#region Implementation

		RefCommodityCodeModuleForTest refCommodityCode;

		#endregion
	}
}
