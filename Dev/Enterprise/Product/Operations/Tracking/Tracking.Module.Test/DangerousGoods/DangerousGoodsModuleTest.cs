using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(DangerousGoodsModule))]
	sealed class DangerousGoodsModuleTest : ZFilterStripGridModuleTestCase
	{
		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<UNDGSubstance>();
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			newElementCount++;
			var substance = result as UNDGSubstance;
			if (substance != null)
			{
				substance.DG_UNNO = "90" + newElementCount;
				substance.DG_Variant = newElementCount.ToString();
				substance.DG_Class = string.Format("AF{0}", newElementCount);
			}
			return result;
		}

		int newElementCount;

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(UNDGSubstanceSchema.DG_Class, SQLComparisonOperator.StartsWith, "AF");
		}

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();
			newElementCount = 0;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection)
		{
			return false;
		}

		#endregion

		[StressTest]
		public override void TestAllBindablePropertiesHavePropertyInfo()
		{
			base.TestAllBindablePropertiesHavePropertyInfo();
		}

		[StressTest]
		public override void TestAllGridColumnsAreExportableToExcel()
		{
			base.TestAllGridColumnsAreExportableToExcel();
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new DangerousGoodsModule(Factory, TestPage);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.DangerousGoods; }
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(UNDGSubstanceSchema.DG_Code.Name, ListSortDirection.Ascending) };

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				List<ColumnDetailsForTest> result = new List<ColumnDetailsForTest>();
				int i = 0;

				result.Add(new ColumnDetailsForTest("Code", i++, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("UN Number", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Variant", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Variation", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Proper Shipping Name", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("US DOT Name", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("EMS", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("IMO Class", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Stowage Requirements", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Active", i++, typeof(ZCheckBoxColumn)));
				result.Add(new ColumnDetailsForTest("System", i++, typeof(ZCheckBoxColumn)));
				result.Add(new ColumnDetailsForTest("State", i++, typeof(ZCodeFindBoxColumn)));
				result.Add(new ColumnDetailsForTest("Flash Point", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Packing Group", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Stowage Category", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Tank Provisions", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Packing Provisions", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Treat As", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Technical Name", i++, typeof(ZTextEditColumn)));

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (DangerousGoodsModule module = FilterGridModule as DangerousGoodsModule)
				{
					result.Add(module.AllColumns["Variation"]);
					result.Add(module.AllColumns["Proper Shipping Name"]);
					result.Add(module.AllColumns["US DOT Name"]);
					result.Add(module.AllColumns["EMS"]);
					result.Add(module.AllColumns["IMO Class"]);
					result.Add(module.AllColumns["Stowage Requirements"]);
					result.Add(module.AllColumns["Active"]);
					result.Add(module.AllColumns["System"]);
					result.Add(module.AllColumns["State"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (DangerousGoodsModule module = FilterGridModule as DangerousGoodsModule)
				{
					result.Add(module.AllColumns["Code"]);
				}
				return result.ToArray();
			}
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutLinerAndAgencyBookings; }
		}
	}
}
