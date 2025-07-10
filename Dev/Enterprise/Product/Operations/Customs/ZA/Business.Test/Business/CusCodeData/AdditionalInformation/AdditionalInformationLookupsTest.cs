using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AdditionalInformationLookupsTest : TestCaseWithFactory
	{
		public void TestCY_CodeList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Export, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.CountryCodes.SouthAfrica);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Import, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.CountryCodes.SouthAfrica);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.OnlyForLine1, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.CountryCodes.SouthAfrica);
			var codeIMPL1 = testHelper.CreateAdditionalInformationCusCodeEntry("IMPL1");
			codeIMPL1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Import, "");
			codeIMPL1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.OnlyForLine1, "");
			var codeIMPLX = testHelper.CreateAdditionalInformationCusCodeEntry("IMPLX");
			codeIMPLX.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Import, "");
			var codeEXPL1 = testHelper.CreateAdditionalInformationCusCodeEntry("EXPL1");
			codeEXPL1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Export, "");
			codeEXPL1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.OnlyForLine1, "");
			var codeEXPLX = testHelper.CreateAdditionalInformationCusCodeEntry("EXPLX");
			codeEXPLX.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Export, "");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine.JI_CL = entryLine1.PK;
			var addInfo1 = entryLine1.AdditionalInformationCodes.AddNew();
			var addInfo2 = entryLine2.AdditionalInformationCodes.AddNew();
			var list1 = addInfo1.Lookups.CY_CodeList;
			AssertEquals("IsCached", true, object.ReferenceEquals(list1, ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, true, true)));
			var codesAsString = list1.CodesAsString;
			AssertContains("Line 1", "EXPL1", codesAsString);
			AssertContains("Any Line", "EXPLX", codesAsString);
			var list3 = addInfo2.Lookups.CY_CodeList;
			AssertEquals(false, object.ReferenceEquals(list3, list1));
			AssertEquals("IsCached", true, object.ReferenceEquals(list3, ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, true, false)));
			codesAsString = list3.CodesAsString;
			AssertNotContains("Not Line 1", "EXPL1", codesAsString);
			AssertContains("Any Line", "EXPLX", codesAsString);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var list5 = addInfo1.Lookups.CY_CodeList;
			AssertEquals(false, object.ReferenceEquals(list5, list1));
			AssertEquals(false, object.ReferenceEquals(list5, list3));
			AssertEquals("IsCached", true, object.ReferenceEquals(list5, ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, false, true)));
			codesAsString = list5.CodesAsString;
			AssertContains("Line 1", "IMPL1", codesAsString);
			AssertContains("Any Line", "IMPLX", codesAsString);
			var list7 = addInfo2.Lookups.CY_CodeList;
			AssertEquals(false, object.ReferenceEquals(list7, list1));
			AssertEquals(false, object.ReferenceEquals(list7, list3));
			AssertEquals(false, object.ReferenceEquals(list7, list5));
			AssertEquals("IsCached", true, object.ReferenceEquals(list7, ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, false, false)));
			codesAsString = list7.CodesAsString;
			AssertNotContains("NotLine 1", "IMPL1", codesAsString);
			AssertContains("Any Line", "IMPLX", codesAsString);
		}
	}
}
