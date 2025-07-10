using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BaseRefPacks))]
	class RefPacksValidationTest : EnterpriseBusinessObjectTestCase
	{
		#region Check tests

		public void TestCheckRP_Type()
		{
			RefPacks.RP_Type = "XX";
			Assert("Error expected", RefPacks.RP_TypeInfo.HasErrors());
		}

		public void TestCheckRP_CommercialPack()
		{
			RefPacks.RP_CommercialPack = "XX";
			Assert("Error expected", RefPacks.RP_CommercialPackInfo.HasErrors());
		}

		public void TestCheckRP_CustomsPack()
		{
			RefPacks.RP_CustomsPack = "TT";
			Assert("Error Expected", RefPacks.RP_CustomsPackInfo.HasErrors());

			RefPacks.RP_Type = RPTypeList.Codes.AllAreas;
			RefPacks.RP_CustomsPack = "TTTT";
			AssertHasError("Customs Pack Unit cannot be longer than 3 characters.", RefPacks.RP_CustomsPackInfo, "Customs Pack Unit cannot be longer than 3 characters when Pack Conversion Type is different than CIP.");

			RefPacks.RP_Type = RPTypeList.Codes.CommercialInvoice;
			AssertNoError("Customs Pack Unit can be longer than 3 characters when Pack Conversion Type = CIP.", RefPacks.RP_CustomsPackInfo, "Customs Pack Unit cannot be longer than 3 characters when Pack Conversion Type is different than CIP.");
		}

		public void TestCheckRP_ConversionFactor()
		{
			RefPacks.RP_ConversionFactor = 0;
			Assert("Conversion Factor should be filled", RefPacks.RP_ConversionFactorInfo.HasErrors());

			RefPacks.RP_ConversionFactor = -1;
			Assert("Conversion Factor should be greater than zero", RefPacks.RP_ConversionFactorInfo.HasErrors());

			RefPacks.RP_ConversionFactor = 2;
			Assert("No error", !RefPacks.RP_ConversionFactorInfo.HasErrors());
		}

		#endregion

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
		protected OrgHeader GetValidSupplier()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, SQLComparisonOperator.Equal, true);
			return Factory.LoadTop1<OrgHeader>(filter);
		}

		#endregion
	}
}
