using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestGuarantee()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
			AssertNotNull("Created if not exists", instruction.Guarantee);
			AssertEquals("PW_ParentID", instruction.PK, instruction.Guarantee.PW_ParentID);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var instruction2 = newFactory.Load<CusEntryInstruction>(instruction.PK);
			AssertEquals("Same Guarantee", instruction.Guarantee.PK, instruction2.Guarantee.PK);
		}

		public void TestZG_DedicatedGuaranteeAmount()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(instruction.GetType(), nameof(CusEntryInstruction.ZG_DedicatedGuaranteeAmount), false, attr => attr.Caption == "Dedicated Guarantee Amount" && attr.ShortCaption == "Dedi.Guar.Amount");
		}

		public void TestZG_GuaranteeRatio_Attribute()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(instruction.GetType(), nameof(CusEntryInstruction.ZG_GuaranteeRatio), false, attr => attr.Caption == "Guarantee Ratio %");
		}

		public void TestZG_GuaranteeRatio_Setter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
			instruction.ZG_DedicatedGuaranteeAmount = 111.22m;
			var cusBondDetail = instruction.Guarantee;
			AssertEquals("Before setting", 0m, cusBondDetail.PW_BondAmount);

			instruction.ZG_GuaranteeRatio = 20m;
			AssertEquals("After setting", 22.24m, cusBondDetail.PW_BondAmount);
			AssertEquals("Should not be set back by Amount", 20m, instruction.ZG_GuaranteeRatio);
		}

		public void TestZG_ExportUnionSecretaryCode_Attribute()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(instruction.GetType(), nameof(CusEntryInstruction.ZG_ExportUnionSecretaryCode), false, attr => attr.Caption == "Union Secretary Code" && attr.ShortCaption == "Union Sec.Code");
		}

		public void TestZG_ExportUnionCode_Attribute()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(instruction.GetType(), nameof(CusEntryInstruction.ZG_ExportUnionCode), false, attr => attr.Caption == "Union Code");
		}

		public void TestZG_ExportUnionCountryCode_Attribute()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(instruction.GetType(), nameof(CusEntryInstruction.ZG_ExportUnionCountryCode), false, attr => attr.Caption == "Export Union Country Code" && attr.ShortCaption == "Exp. Un. Country");
		}

		public void TestZG_InlandTransportType_Attribute()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(instruction.GetType(), nameof(CusEntryInstruction.ZG_InlandTransportType), false, attr => attr.Caption == "Inland Transport Type" && attr.ShortCaption == "Inland Trans.Type");
		}
	}
}
