using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusBondDetail))]
	class CusBondDetailTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
			return instruction.Guarantee;
		}

		public void TestLookups()
		{
			var cusBondDetail = Factory.New<CusBondDetail>();
			AssertType<CusBondDetailLookups>(cusBondDetail.Lookups);
		}

		public void TestPW_CPH_Guarantee_Attribute()
		{
			var cusBondDetail = Factory.New<CusBondDetail>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(cusBondDetail.GetType(), nameof(CusBondDetail.PW_CPH_Guarantee), false, attr => attr.Caption == "Guarantee Existing");
		}

		public void TestPW_CPH_Guarantee_Setter()
		{
			var cusBondDetail = Factory.New<CusBondDetail>();
			AssertEquals("PW_BondType", ZString.Empty, cusBondDetail.PW_BondType);
			AssertEquals("PW_BondNumber", ZString.Empty, cusBondDetail.PW_BondNumber);

			var guarantee = Factory.New<Customs.Business.BaseCusGuaranteeHeader>();
			guarantee.CPH_Type = "BANKA";
			guarantee.CPH_Number = "123";
			cusBondDetail.PW_CPH_Guarantee = guarantee.PK;
			AssertEquals("PW_BondType", "BANKA", cusBondDetail.PW_BondType);
			AssertEquals("PW_BondNumber", "123", cusBondDetail.PW_BondNumber);
		}

		public void TestPW_BondType_Attribute()
		{
			var cusBondDetail = Factory.New<CusBondDetail>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(cusBondDetail.GetType(), nameof(CusBondDetail.PW_BondType), false, attr => attr.Caption == "Guarantee Type");
			AssertHasCustomAttribute<ListAttribute>(cusBondDetail.GetType(), nameof(CusBondDetail.PW_BondType), false, attr => attr.ListDataSourceMember == "Lookups.BondTypeList");
		}

		public void TestPW_BondAmount_Attribute()
		{
			var cusBondDetail = Factory.New<CusBondDetail>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(cusBondDetail.GetType(), nameof(CusBondDetail.PW_BondAmount), false, attr => attr.Caption == "Guarantee Amount");
		}

		public void TestPW_BondAmount_Setter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
			instruction.ZG_DedicatedGuaranteeAmount = 111.22m;
			AssertEquals("Before setting", 0m, instruction.ZG_GuaranteeRatio);

			var cusBondDetail = instruction.Guarantee;
			cusBondDetail.PW_BondAmount = 12.34m;
			AssertEquals("After setting", 11.10m, instruction.ZG_GuaranteeRatio);
			AssertEquals("Should not be set back by Ratio", 12.34m, cusBondDetail.PW_BondAmount);
		}

		public void TestPW_GuaranteeDescription_Attribute()
		{
			var cusBondDetail = Factory.New<CusBondDetail>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(cusBondDetail.GetType(), nameof(CusBondDetail.PW_GuaranteeDescription), false, attr => attr.Caption == "Guarantee Description");
		}

		public void TestPW_GuaranteeDescription_Getter()
		{
			var cusBondDetail = Factory.New<CusBondDetail>();
			cusBondDetail.PW_BondType = "BANKA";
			cusBondDetail.PW_BondNumber = "123";
			cusBondDetail.PW_BondAmount = 22.2440m;
			AssertEquals("When empty", "BANKA 123 22.24", cusBondDetail.PW_GuaranteeDescription);

			cusBondDetail.PW_GuaranteeDescription = "Test Description";
			AssertEquals("When not value", "Test Description", cusBondDetail.PW_GuaranteeDescription);
		}
	}
}
