using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CFDValidatorTest : TestCaseWithFactory
	{
		string CodeType => MexicoOrgCusCodeInfo.OrgCusCodes.CFD;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.MXCFD;
		string CountryCode => Core.Constants.CountryCodes.Mexico;

		public void TestCFDCodePatternValidation()
		{
			var factory = new BusinessObjectFactory();
			var orgCusCode = factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = CountryCode;

			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPattern, ValidCodesStrings(orgCusCode.OK_CustomsRegNoInfo).Select(x => (ZString)x).ToArray(), InvalidPatternMessage(orgCusCode.OK_CustomsRegNoInfo), OrganisationRegistryCodeType, isOnlyWarning: false);
		}

		readonly ZString[] codesWithInvalidPattern = new ZString[] { "G04", "0G01", "G010", "0G010", "F2G03", "D054!", "AP01HGR", "X6", "P99", "I%%", "0AA01", "P&3", "X10", "9A9SS", "9JKER!", "L", ".I07-1" };
		HashSet<string> ValidCodesStrings(ZPropertyInfo codeInfo)
		{
			var usosCFDI = codeInfo.BizObj.Factory.GetCachedValue("Mexico_UsosCFDI_PairList", () => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetMexicoEInvoicingExtension().GetUsosCFDI().GetAllCodes());

			return new HashSet<string>(usosCFDI);
		}
		string InvalidPatternMessage(ZPropertyInfo codeInfo)
		{
			return (NoResString)@"The 'MX CFD' registration code is invalid.

Valid codes are:
G01, G02, G03, I01, I02, I03, I04, I05, I06, I07, I08, D01, D02, D03, D04, D05, D06, D07, D08, D09, D10, S01, CP01, CN01

Please verify that you are entering a correct registration code.";
		}

		[ExpectNoExceptions]
		public void TestGetUsosCFDICaching()
		{
			var usosCFDI = new CodeDescriptionPairList();
			usosCFDI.AddPair("G03", "Gastos en general");

			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI()).Returns(usosCFDI);

			ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object);

			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode.OK_CodeType = CodeType;

			mockIAccountingMasterFilesDependencyFactory.Verify(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI(), Times.Once);
			mockIAccountingMasterFilesDependencyFactory.Reset();

			orgCusCode.OK_CodeType = CodeType;

			mockIAccountingMasterFilesDependencyFactory.Verify(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI(), Times.Never);
		}
	}
}

