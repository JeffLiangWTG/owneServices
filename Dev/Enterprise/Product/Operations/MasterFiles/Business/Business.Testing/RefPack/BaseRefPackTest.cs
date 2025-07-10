using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BaseRefPacks))]
	public class BaseRefPacksTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnlyAndCVanDelete()
		{
			RefPacks.RP_IsSystem = false;
			AssertEquals(false, RefPacks.ReadOnly);
			AssertEquals(true, RefPacks.CanDelete);
			RefPacks.RP_IsSystem = true;
			AssertEquals(true, RefPacks.ReadOnly);
			AssertEquals(false, RefPacks.CanDelete);
		}

		public void TestGetCommercialPackList()
		{
			AssertNotNull("Commercial Pack List", RefPacks.RP_CommercialPack_List);
		}

		public void TestRP_Code()
		{
			OrgHeader supplier = GetValidSupplier();
			RefPacks.RP_OH_Supplier = supplier.PK;
			RefPacks.RP_CustomsPack = "TT";
			RefPacks.RP_CommercialPack = "XX";
			AssertEquals(supplier.OH_Code + "-TT-XX", RefPacks.RP_Code);

			RefPacks.RP_OH_Supplier = ZGuid.Empty;
			AssertEquals("TT-XX", RefPacks.RP_Code);
		}

		[ExpectException(typeof(Exception))]
		public void TestPreventDuplicateRecords()
		{
			RefPacks.RP_CommercialPack = "M2";
			RefPacks.RP_CustomsPack = "SM";
			RefPacks.RP_ConversionFactor = 1m;
			Factory.Save();

			BaseRefPacks newPacks = Factory.New<BaseRefPacks>();
			newPacks.RP_CommercialPack = RefPacks.RP_CommercialPack;
			newPacks.RP_CustomsPack = RefPacks.RP_CustomsPack;
			newPacks.RP_ConversionFactor = 1m;
			Factory.Save();
		}

		public void TestSupplierDifferenciateRecords()
		{
			RefPacks.RP_CommercialPack = "M2";
			RefPacks.RP_CustomsPack = "KG";
			RefPacks.RP_ConversionFactor = 1m;
			RefPacks.RP_OH_Supplier = GetValidSupplier().PK;
			Factory.Save();

			BaseRefPacks newPacks = Factory.New<BaseRefPacks>();
			newPacks.RP_CommercialPack = RefPacks.RP_CommercialPack;
			newPacks.RP_CustomsPack = RefPacks.RP_CustomsPack;
			newPacks.RP_ConversionFactor = 1m;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseRefPacks loadedPacks = newFactory.Load<BaseRefPacks>(newPacks.PK);
			Assert("The new record saved", loadedPacks != null);
		}

		public void TestCustomsCountryCountryCode()
		{
			//1 Do not screw with country when instantiating
			RefPacks.RP_CustomsCountry = "DC";
			Factory.Save();
			var reloaded = new BusinessObjectFactory().Load<BaseRefPacks>(RefPacks.PK);
			AssertEquals("DC", reloaded.RP_CustomsCountry);

			// 2 Default on new records
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("LC"))
			{
				var newRefPacks = Factory.New<BaseRefPacks>();
				AssertEquals("LC", newRefPacks.RP_CustomsCountry);
			}

			// 3 Lookups
			AssertNotNull(RefPacks.RP_CustomsCountry_List.FirstOrDefault(rc => rc.RN_Code == "GB"));
		}

		public void TestRPTypeListCached()
		{
			BaseRefPacks newPacks = Factory.New<BaseRefPacks>();

			var list1 = newPacks.RP_Type_List;
			var list2 = newPacks.RP_Type_List;
			Assert("RP_Type_List should be cached", object.ReferenceEquals(list1, list2));
		}

		public void TestRP_OH_SupplierInfoReadOnly()
		{
			var pack = Factory.New<BaseRefPacks>();
			AssertEquals("Supplier should not be readonly", false, pack.RP_OH_SupplierInfo.ReadOnly);
			pack.RP_Type = RPTypeList.Codes.PackingDeclaration;
			AssertEquals("Supplier should be readonly", true, pack.RP_OH_SupplierInfo.ReadOnly);
		}

		public void TestRP_ConversionFactorInfoReadOnly()
		{
			var pack = Factory.New<BaseRefPacks>();
			AssertEquals("ConversionFactor should not be readonly", false, pack.RP_ConversionFactorInfo.ReadOnly);
			pack.RP_Type = RPTypeList.Codes.PackingDeclaration;
			AssertEquals("ConversionFactor be should readonly", true, pack.RP_ConversionFactorInfo.ReadOnly);
		}

		#region Implementation
		BaseRefPacks RefPacks;
		protected override void SetUp()
		{
			base.SetUp();
			RefPacks = Factory.New<BaseRefPacks>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			RefPacks = factory.New<BaseRefPacks>();
			RefPacks.RP_Type = RPTypeList.Codes.CommercialInvoice;
			return RefPacks;
		}

		OrgHeader GetValidSupplier()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, SQLComparisonOperator.Equal, true);
			return Factory.LoadTop1<OrgHeader>(filter);
		}
		#endregion
	}
}
