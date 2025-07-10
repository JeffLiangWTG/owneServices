using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class UCRHelperTest : TestCaseWithFactory
	{
		[TestDate(2019, 12, 26)]
		[ExpectNoExceptions]
		public void TestCalculateUCR()
		{
			var helper = new UCRHelper();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			NUnit.Framework.Assert.That(helper.CalculateUCR(entryInstruction), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 12, 22);
			NUnit.Framework.Assert.That(helper.CalculateUCR(entryInstruction), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			declaration.JE_OH_Supplier = org.PK;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 12, 22);
			NUnit.Framework.Assert.That(helper.CalculateUCR(entryInstruction), NUnit.Framework.Is.EqualTo("9TWVAT00120191226000000000").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			TestDateAttribute.AddDays(1);
			NUnit.Framework.Assert.That(helper.CalculateUCR(entryInstruction), NUnit.Framework.Is.EqualTo("9TWVAT00120191227000000000").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2019, 12, 26)]
		[ExpectNoExceptions]
		public void TestGetUCRPrefix()
		{
			var helper = new UCRHelper();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			NUnit.Framework.Assert.That(helper.GetUCRPrefix(entryInstruction), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 12, 22);
			NUnit.Framework.Assert.That(helper.GetUCRPrefix(entryInstruction), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			declaration.JE_OH_Supplier = org.PK;
			NUnit.Framework.Assert.That(helper.GetUCRPrefix(entryInstruction), NUnit.Framework.Is.EqualTo("9TWVAT001").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 12, 22);
			NUnit.Framework.Assert.That(helper.GetUCRPrefix(entryInstruction), NUnit.Framework.Is.EqualTo("0TWVAT001").Using(CustomComparers.TypeComparison));
		}
	}
}
