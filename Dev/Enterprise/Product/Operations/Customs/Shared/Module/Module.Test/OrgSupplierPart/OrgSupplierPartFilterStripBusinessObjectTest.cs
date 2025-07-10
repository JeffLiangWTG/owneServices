using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
	public class OrgSupplierPartFilterStripBusinessObjectTest : MasterFiles.Module.Testing.OrgSupplierPartFilterStripBusinessObjectTest
	{
		public void TestTariffCodeIsBlankFilter()
		{
			var stripBusinessObj = GetNewFilterStripBusinessObject() as OrgSupplierPartFilterStripBusinessObject;

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "TEST0003";
			product1.OP_Desc = "Part with blank tariff and non-blank classification lookup";
			var lookup1 = Factory.New<BaseCusClassification>();
			lookup1.CC_LookupCode = "TestLookup";
			lookup1.CC_TariffNum = "9999.88.00.00";
			lookup1.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			var pivot1 = Factory.New<BaseCusClassPartPivot>();
			pivot1.CI_TariffNum = "";
			pivot1.CI_CC = lookup1.PK;
			product1.PivotsForBinding.Add(pivot1);

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "TEST0004";
			product2.OP_Desc = "Part with blank tariff and blank classification lookup";
			var pivot2 = Factory.New<BaseCusClassPartPivot>();
			pivot2.CI_TariffNum = "";
			product2.PivotsForBinding.Add(pivot2);

			Factory.Save();

			var filter = (ModuleTextFilter)stripBusinessObj[OrgSupplierPartFilterConstants.TariffCode];
			if (filter != null)
			{
				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				var result = Factory.Load<OrgSupplierPart>(filter.Query);
				AssertEquals(1, result.Length);
				AssertEquals(result[0].OP_PartNum, product2.OP_PartNum);
			}
			else
			{
				Assert(!stripBusinessObj.ModuleFilters.Filter_List.ContainsCode(OrgSupplierPartFilterConstants.TariffCode));
			}
		}

		public void TestTariffCodeAndCustomsTypeFilterCreated()
		{
			OrgSupplierPartFilterStripBusinessObject stripBusinessObj = GetNewFilterStripBusinessObject() as OrgSupplierPartFilterStripBusinessObject;
			var type = stripBusinessObj.GetType();
			var tariffCodeFilterExists = (bool)(type.GetProperty("NeedTariffCodeFilter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(stripBusinessObj) ?? false);
			AssertEquals(tariffCodeFilterExists, stripBusinessObj.ModuleFilters.Filter_List.ContainsCode(OrgSupplierPartFilterConstants.TariffCode));
			var cusTypeFilterExists = (bool)(type.GetProperty("NeedCustomsTypeFilter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(stripBusinessObj) ?? false);
			AssertEquals(cusTypeFilterExists, stripBusinessObj.ModuleFilters.Filter_List.ContainsCode(OrgSupplierPartFilterConstants.CustomsType));
			if (tariffCodeFilterExists || cusTypeFilterExists)
			{
				Assert("Customs header should be there", stripBusinessObj.ModuleFilters.Filter_List.ContainsCode(OrgSupplierPartFilterConstants.CustomsHeader));
			}
		}

		public void TestTariffCodeFilterForProduct()
		{
			OrgSupplierPartFilterStripBusinessObject stripBusinessObj = GetNewFilterStripBusinessObject() as OrgSupplierPartFilterStripBusinessObject;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TEST0001";
			var pivotWithTariff = Factory.New<BaseCusClassPartPivot>();
			pivotWithTariff.CI_TariffNum = Guid.NewGuid().ToString();
			product.PivotsForBinding.Add(pivotWithTariff);
			var pivotWithClassification = Factory.New<BaseCusClassPartPivot>();
			var lookup = Factory.New<BaseCusClassification>();
			lookup.CC_LookupCode = "TestLookup";
			lookup.CC_TariffNum = "0999.12.344321";
			lookup.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			pivotWithClassification.CI_CC = lookup.PK;
			product.PivotsForBinding.Add(pivotWithClassification);
			Factory.Save();
			var filter = (ModuleTextFilter)stripBusinessObj[OrgSupplierPartFilterConstants.TariffCode];
			if (filter != null)
			{
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = pivotWithTariff.CI_TariffNum;
				var result = Factory.Load<OrgSupplierPart>(filter.Query);
				AssertEquals(1, result.Length);
				AssertEquals(result[0].OP_PartNum, product.OP_PartNum);
				AssertNotNullOrEmpty(lookup.CC_TariffNum);
				filter.Property = lookup.CC_TariffNum;
				result = Factory.Load<OrgSupplierPart>(filter.Query);
				AssertEquals(1, result.Length);
				AssertEquals(result[0].OP_PartNum, product.OP_PartNum);
			}
			else
			{
				Assert(!stripBusinessObj.ModuleFilters.Filter_List.ContainsCode(OrgSupplierPartFilterConstants.TariffCode));
			}
		}

		public void TestCustomsTypeFilterForProduct()
		{
			OrgSupplierPartFilterStripBusinessObject stripBusinessObj = GetNewFilterStripBusinessObject() as OrgSupplierPartFilterStripBusinessObject;
			var filter = (ModuleTextFilter)stripBusinessObj[OrgSupplierPartFilterConstants.CustomsType];
			if (filter != null)
			{
				var product = Factory.New<OrgSupplierPart>();
				product.OP_PartNum = "TEST0002";
				var pivot = Factory.New<BaseCusClassPartPivot>();
				pivot.CI_OP = product.PK;
				pivot.CI_ChildType = filter.List[0].ToString();
				product.PivotsForBinding.Add(pivot);
				Factory.Save();
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = pivot.CI_ChildType;
				var results = new List<OrgSupplierPart>(Factory.Load<OrgSupplierPart>(filter.Query));
				Assert(results.Exists(p => p.OP_PartNum == "TEST0002"));
			}
			else
			{
				Assert(!stripBusinessObj.ModuleFilters.Filter_List.ContainsCode(OrgSupplierPartFilterConstants.CustomsType));
			}
		}

		public void TestImporterSupplierFilterDefault()
		{
			OrgSupplierPartFilterStripBusinessObject orgSupplierPartFilter = new OrgSupplierPartFilterStripBusinessObject();
			MasterFiles.Module.ImporterSupplierModuleGuidsWithListFilter filter = (MasterFiles.Module.ImporterSupplierModuleGuidsWithListFilter)orgSupplierPartFilter["Importer/Supplier"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);
			AssertEquals("Default Filter Condition should be 'Both' for mathing Products for the Declaration > Invoice Line.", Enterprise.MasterFiles.Business.ImporterSuplierFilterConditions.Codes.Both, filter.FilterCondition);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = new List<Tuple<string, string>>();
			result.Add(TableFilter(CusClassPartPivotSchema.Constants.TableName, "Classification"));
			result.Add(TableFilter(CusClassificationSchema.Constants.TableName, "Classification"));
			// Inherited form Masterfiles:
			result.Add(TableFilter(OrgPartRelationSchema.Constants.TableName, "Importer/Supplier"));
			result.Add(TableFilter(OrgSupplierPartSchema.Constants.TableName, "Importer/Supplier"));
			result.Add(TableFilter(WhsABCCategorySchema.Constants.TableName, "ABC Category / Warehouse"));
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();
			result.Add(TableFilter(OrgPartRelationSchema.Constants.TableName, "Importer/Supplier"));
			return result;
		}
	}
}
