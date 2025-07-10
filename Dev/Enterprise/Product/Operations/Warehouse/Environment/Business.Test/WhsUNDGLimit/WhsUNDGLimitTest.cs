using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsUNDGLimit))]
	class WhsUNDGLimitTest : WhsEnvBusinessObjectTestCase
	{
		public void TestUNDGLimitDescription()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a");

			undgLimit.WWD_DG = ZGuid.Empty;
			AssertEquals("Precondition:", ZString.Empty, undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 0m;
			undgLimit.WWD_TotalVolumeLimit = 0;

			undgLimit.WWD_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("When weight or volume threshold is zero, storage is not permitted of this UNDG", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 10m;
			undgLimit.WWD_TotalWeightLimitUQ = Constants.Weight.Kilograms;
			AssertEquals("0004a is allowed to store up to 10 KG in the warehouse", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 0m;
			undgLimit.WWD_TotalVolumeLimit = 10m;
			undgLimit.WWD_TotalVolumeLimitUQ = Constants.Volume.CubicMetres;
			AssertEquals("0004a is allowed to store up to 10 M3 in the warehouse", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 10m;
			undgLimit.WWD_TotalWeightLimitUQ = Constants.Weight.Kilograms;
			AssertEquals("0004a is allowed to store up to 10 KG and 10 M3 in the warehouse", undgLimit.UNDGLimitDescription);
		}

		public void TestUNDGLimitDescription_CountryReference()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var countryReference = Helper.CreateCountryReference("4567");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference);

			undgLimit.WWD_DCR_UNDGCountryReference = ZGuid.Empty;
			AssertEquals("Precondition:", ZString.Empty, undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 0m;
			undgLimit.WWD_TotalVolumeLimit = 0;

			undgLimit.WWD_DCR_UNDGCountryReference = countryReference.PK;
			AssertEquals("When weight or volume threshold is zero, storage is not permitted of this UNDG", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 10m;
			undgLimit.WWD_TotalWeightLimitUQ = Constants.Weight.Kilograms;
			AssertEquals("4567 is allowed to store up to 10 KG in the warehouse", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 0m;
			undgLimit.WWD_TotalVolumeLimit = 10m;
			undgLimit.WWD_TotalVolumeLimitUQ = Constants.Volume.CubicMetres;
			AssertEquals("4567 is allowed to store up to 10 M3 in the warehouse", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 10m;
			undgLimit.WWD_TotalWeightLimitUQ = Constants.Weight.Kilograms;
			AssertEquals("4567 is allowed to store up to 10 KG and 10 M3 in the warehouse", undgLimit.UNDGLimitDescription);
		}

		public void TestUNDGLimitDescription_Class()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1");

			undgLimit.WWD_UNDGClass = ZString.Empty;
			AssertEquals("Precondition:", ZString.Empty, undgLimit.WWD_UNDGClass);

			undgLimit.WWD_TotalWeightLimit = 0m;
			undgLimit.WWD_TotalVolumeLimit = 0;

			undgLimit.WWD_UNDGClass = "1";
			AssertEquals("When weight or volume threshold is zero, storage is not permitted of this UNDG", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 10m;
			undgLimit.WWD_TotalWeightLimitUQ = Constants.Weight.Kilograms;
			AssertEquals("Class 1 is allowed to store up to 10 KG in the warehouse", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 0m;
			undgLimit.WWD_TotalVolumeLimit = 10m;
			undgLimit.WWD_TotalVolumeLimitUQ = Constants.Volume.CubicMetres;
			AssertEquals("Class 1 is allowed to store up to 10 M3 in the warehouse", undgLimit.UNDGLimitDescription);

			undgLimit.WWD_TotalWeightLimit = 10m;
			undgLimit.WWD_TotalWeightLimitUQ = Constants.Weight.Kilograms;
			AssertEquals("Class 1 is allowed to store up to 10 KG and 10 M3 in the warehouse", undgLimit.UNDGLimitDescription);
		}

		public void TestIsDGAllowed()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			AssertEquals("Precondition:", true, undgLimit.IsDGAllowed);

			undgLimit.WWD_TotalVolumeLimit = 0m;
			AssertEquals(true, undgLimit.IsDGAllowed);

			undgLimit.WWD_TotalWeightLimit = 0m;
			AssertEquals(false, undgLimit.IsDGAllowed);
		}

		public void TestDGSubstance_ReadOnly()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var countryReference = Helper.CreateCountryReference();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference);
			Factory.Save();

			AssertNotNull(undgLimit.CountryReference);
			AssertEquals("When UNDG Country Reference is set, DG Substance shall be readonly", true, undgLimit.WWD_DGInfo.ReadOnly);

			undgLimit.WWD_DCR_UNDGCountryReference = ZGuid.Empty;
			undgLimit.WWD_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			Factory.Save();
			AssertNull(undgLimit.CountryReference);
			AssertEquals("When UNDG Country Reference is null, DG Substance shall be editable", false, undgLimit.WWD_DGInfo.ReadOnly);
		}

		public void TestDGCountryReference_ReadOnly()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			Factory.Save();

			AssertNotNull(undgLimit.UNDGSubstance);
			AssertEquals("When UNDG Substance is set, DG Country Reference shall be readonly", true, undgLimit.WWD_DCR_UNDGCountryReferenceInfo.ReadOnly);

			undgLimit.WWD_DG = ZGuid.Empty;
			var countryReference = Helper.CreateCountryReference();
			undgLimit.WWD_DCR_UNDGCountryReference = countryReference.PK;
			Factory.Save();
			AssertNull(undgLimit.UNDGSubstance);
			AssertEquals("When UNDG Substance is null, DG Country Reference shall be editable", false, undgLimit.WWD_DCR_UNDGCountryReferenceInfo.ReadOnly);
		}

		public void TestUNDGSubstancePivotAddAndModify()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			Factory.Save();

			var pivot = Factory.Load<UNDGSubstancePivot>(new ZQuery(UNDGSubstancePivotSchema.DP_ParentId, undgLimit.PK)).Single();
			AssertEquals("Pivot.DP_ParentId Should be equal to undgLimit.PK", undgLimit.PK, pivot.DP_ParentId);
			AssertEquals("Pivot.DP_ParentTableCode Should be equal to WWD", WhsUNDGLimitSchema.Constants.Prefix, pivot.DP_ParentTableCode);
			AssertEquals("Pivot.DP_UNNO Should be equal to '0004", "0004", pivot.DP_UNNO);
			AssertEquals("Pivot.DP_Variant Should be equal to 'a'", "a", pivot.DP_Variant);
			AssertEquals("Pivot.DP_Standard Should be equal to 'IMO'", UNDGSubstanceStandardTypes.IMO, pivot.DP_Standard);

			undgLimit.WWD_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", UNDGSubstanceStandardTypes.IMO).First().PK;
			Factory.Save();

			pivot = Factory.Load<UNDGSubstancePivot>(new ZQuery(UNDGSubstancePivotSchema.DP_ParentId, undgLimit.PK)).Single();

			AssertEquals("Pivot.DP_ParentId Should be equal to undgLimit.PK", undgLimit.PK, pivot.DP_ParentId);
			AssertEquals("Pivot.DP_ParentTableCode Should be equal to WWD", WhsUNDGLimitSchema.Constants.Prefix, pivot.DP_ParentTableCode);
			AssertEquals("Pivot.DP_UNNO Should be equal to '0004", "0004", pivot.DP_UNNO);
			AssertEquals("Pivot.DP_Variant Should be equal to 'b'", "b", pivot.DP_Variant);
			AssertEquals("Pivot.DP_Standard Should be equal to 'IMO'", UNDGSubstanceStandardTypes.IMO, pivot.DP_Standard);
		}

		public void TestUNDGSubstancePivotCollectionIsChildEditable()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			Assert("Should be registered as editable child", undgLimit.IsRegisteredEditableChildObject(undgLimit.UNDGSubstancePivotCollection));
		}

		public void TestDeleteUNDGLimitAndUNDGSubstancePivot()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, "0004b");

			Factory.Save();

			AssertEquals(1, undgLimit1.UNDGSubstancePivotCollection.Count);
			AssertEquals(1, undgLimit2.UNDGSubstancePivotCollection.Count);

			undgLimit1.Delete();

			undgLimit2.WWD_DG = ZGuid.Empty;
			undgLimit2.Delete();

			AssertEquals(0, undgLimit1.UNDGSubstancePivotCollection.Count);
			AssertEquals(0, undgLimit2.UNDGSubstancePivotCollection.Count);
		}

		public void TestDGClass_ReadOnly()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1");
			Factory.Save();

			AssertNotNull(undgLimit.WWD_UNDGClass);
			AssertEquals("When UNDG Class is set, DG Country Reference and DG Substance shall be readonly", true, undgLimit.WWD_DCR_UNDGCountryReferenceInfo.ReadOnly, undgLimit.WWD_DGInfo.ReadOnly);

			undgLimit.WWD_UNDGClass = ZString.Empty;
			var countryReference = Helper.CreateCountryReference();
			undgLimit.WWD_DCR_UNDGCountryReference = countryReference.PK;
			Factory.Save();
			AssertNullOrEmpty(undgLimit.WWD_UNDGClass);
			AssertEquals("When UNDG Class is null, UNDG Class shall be editable", false, undgLimit.WWD_DCR_UNDGCountryReferenceInfo.ReadOnly, undgLimit.WWD_UNDGClassInfo.ReadOnly);

			undgLimit.WWD_DCR_UNDGCountryReference = ZGuid.Empty;
			undgLimit.WWD_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			Factory.Save();
			AssertNull(undgLimit.CountryReference);
			AssertEquals("When UNDG Class is null, UNDG Class shall be editable", false, undgLimit.WWD_DGInfo.ReadOnly, undgLimit.WWD_UNDGClassInfo.ReadOnly);
		}

		public void TestDangerousGoodsLogs()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var subs1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1");
			undgLimit.WWD_DG = subs1.PK;
			undgLimit.WWD_UNDGClass = ZString.Empty;
			Factory.Save();
			var log1 = undgLimit.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			AssertNotNull("DangerousGoodsChanged log created", log1);

			var subs2 = Factory.NewWithValidTestData<UNDGSubstance>();
			undgLimit.WWD_DG = subs2.PK;
			Factory.Save();
			var log2 = undgLimit.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			AssertNotNull("DangerousGoodsChanged log created", log2);
			AssertNotEquals(log1, log2);

			undgLimit.WWD_DG = ZGuid.Empty;
			undgLimit.WWD_UNDGClass = "1";
			Factory.Save();
			var log3 = undgLimit.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			AssertNotNull("DangerousGoodsChanged log created", log3);
			AssertNotEquals(log2, log3);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var newHelper = new WhsTestHelperFunctionsEnv(factory);
			var warehouse = factory.Load<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "XXX")).SingleOrDefault() ?? newHelper.CreateWarehouse("XXX");
			factory.Save();

			return Helper.CreateWhsUNDGLimit(warehouse, "0004a");
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion
	}
}
