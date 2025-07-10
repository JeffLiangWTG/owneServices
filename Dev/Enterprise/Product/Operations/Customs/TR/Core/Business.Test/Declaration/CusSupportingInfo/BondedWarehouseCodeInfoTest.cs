using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(BondedWarehouseCodeInfo))]
	public class BondedWarehouseCodeInfoTest : Customs.Business.Testing.CusSupportingInfoTest<BondedWarehouseCodeInfo>
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CusSupportingInfoList.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var bondedWarehouseList = declaration.CusSupportingInfoList.AddNew();
			bondedWarehouseList.CSI_CustomsOffice = "ABC";
			return bondedWarehouseList;
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<BondedWarehouseCodeInfo>();
			AssertEquals(CusSupportingInfoTypeList.Codes.BondedWarehouse, supporting.CSI_Type);
			AssertEquals(JobDeclarationSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<BondedWarehouseCodeInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "Bonded Warehouse Codes List", "TR");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "ABC", "ABC Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			var cusSupportingInfo = factory.New<JobDeclaration>().CusSupportingInfoList.AddNew();
			cusSupportingInfo.CSI_CustomsOffice = "ABC";
			yield return cusSupportingInfo;
		}

		public void TestCusSupportingInfoSettingValues()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "Bonded Warehouse Codes List", "TR");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "ABC", "ABC Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			var declaration = Factory.New<JobDeclaration>();
			var cusSupportingInfo = declaration.CusSupportingInfoList.AddNew();
			cusSupportingInfo.CSI_CustomsOffice = "ABC";

			CombineAssertions(() =>
			{
				AssertEquals("ABC", cusSupportingInfo.CSI_CustomsOffice);
			});
		}

		public void TestSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bondedWarehouseList = declaration.CusSupportingInfoList.AddNew();

			bondedWarehouseList.CSI_CustomsOffice = ZString.Empty;
			Factory.Save();
			Assert("BondedWarehouseCodeInfo should be deleted", bondedWarehouseList.IsDeleted);

			bondedWarehouseList = declaration.CusSupportingInfoList.AddNew();
			bondedWarehouseList.CSI_CustomsOffice = "ABC";
			Factory.Save();
			Assert("BondedWarehouseCodeInfo should NOT be deleted", !bondedWarehouseList.IsDeleted);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "Bonded Warehouse Codes List", "TR");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "ABC", "ABC Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		}
	}
}
