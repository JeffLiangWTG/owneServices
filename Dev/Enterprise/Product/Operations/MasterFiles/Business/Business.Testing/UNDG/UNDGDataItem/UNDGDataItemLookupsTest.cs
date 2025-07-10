using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGDataItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMarinePollutantList()
		{
			UNDGDataItem item = Factory.New<UNDGDataItem>();
			UNDGDataItemLookups lookup = new UNDGDataItemLookups(item);

			AssertEquals("should only have 3", 3, lookup.MarinePollutantList.Count);
			AssertEquals(UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant, lookup.MarinePollutantList.GetDescriptionFromCode(UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code));
			AssertEquals(UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant, lookup.MarinePollutantList.GetDescriptionFromCode(UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code));
			AssertEquals("should not have depends code", null, lookup.MarinePollutantList.GetDescriptionFromCode(UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code));
			AssertEquals("", lookup.MarinePollutantList.GetDescriptionFromCode(""));
		}

		public void TestDGClassList()
		{
			UNDGDataItem item = Factory.New<UNDGDataItem>();
			UNDGDataItemLookups lookup = new UNDGDataItemLookups(item);

			Assert("Contains 1.1", lookup.DGClassList.ContainsCode("1.1A"));
			Assert("Contains 2.1", lookup.DGClassList.ContainsCode("2.1"));
			Assert("Contains 4", lookup.DGClassList.ContainsCode("4"));
			Assert("Contains 6.1", lookup.DGClassList.ContainsCode("6.1"));
			Assert("Contains 6", lookup.DGClassList.ContainsCode("6"));
			Assert("Contains COMB", lookup.DGClassList.ContainsCode("COMB"));
			Assert("Doesn't contain XYZ", !lookup.DGClassList.ContainsCode("XYZ"));
		}

		public void TestEnsureDGClassListContainsSystemDefinedInDatabase()
		{
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load($"SELECT DISTINCT {UNDGSubstance.Schema.DG_Class} FROM {UNDGSubstanceSchema.Constants.SqlSchemaName}.{UNDGSubstanceSchema.Constants.TableName} WHERE {UNDGSubstance.Schema.DG_IsSystem} = 1");

			var dataItem = Factory.New<UNDGDataItem>();
			var lookup = new UNDGDataItemLookups(dataItem);

			foreach (var item in collection.Select(item => item[UNDGSubstance.Schema.DG_Class].ToString()))
			{
				AssertCollectionContains(item, lookup.DGClassList.GetAllCodes());
			}
		}

		public void TestApprovalCertificateTypeList()
		{
			var item = Factory.New<UNDGDataItem>();
			var lookup = new UNDGDataItemLookups(item);

			AssertEquals(10, lookup.ApprovalCertificateTypeList.Count);
			AssertEquals(ApprovalCertificateTypeList.Descriptions.SpecialForm, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.SpecialForm));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.LowDispersibleMaterial, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.LowDispersibleMaterial));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.TypeBPackageDesign, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.TypeBPackageDesign));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.TypeBPackageShipment, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.TypeBPackageShipment));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.TypeCPackageDesignAndShipment, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.TypeCPackageDesignAndShipment));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.FissileMaterialPackageDesign, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.FissileMaterialPackageDesign));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.FissileMaterialPackageShipment, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.FissileMaterialPackageShipment));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.FissileMaterialExcepted, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.FissileMaterialExcepted));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.SpecialArrangement, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.SpecialArrangement));
			AssertEquals(ApprovalCertificateTypeList.Descriptions.Other, lookup.ApprovalCertificateTypeList.GetDescriptionFromCode(ApprovalCertificateTypeList.Codes.Other));
		}
	}
}
