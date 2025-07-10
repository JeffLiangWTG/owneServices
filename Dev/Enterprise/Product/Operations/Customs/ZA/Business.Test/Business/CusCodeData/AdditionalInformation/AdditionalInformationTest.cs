using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AdditionalInformation))]
	sealed class AdditionalInformationTest : Customs.Business.Testing.CusCodeDataTest<AdditionalInformation>
	{
		public void TestAdditionalInfoBusinessObjectFieldType()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
			{
				var job = Factory.New<JobDeclaration>();
				var entryLine = job.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
				var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.SafeguardDutyItem);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.CountervailingDutyItem);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignHaulier);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.DiamondLevyValue);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.DutyCreditValue);
				helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue);
				Factory.Save();
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
				AssertEquals("Text", additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount;
				AssertEquals("Integer", additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("0", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "";
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.SafeguardDutyItem;
				AssertEquals(new ZString("Text"), additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("0", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "";
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.CountervailingDutyItem;
				AssertEquals(new ZString("Text"), additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety;
				AssertEquals(new ZString("Text"), additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter;
				AssertEquals(new ZString("Text"), additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignHaulier;
				AssertEquals(new ZString("Text"), additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.DiamondLevyValue;
				AssertEquals("Integer", additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("0", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "";
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.DutyCreditValue;
				AssertEquals("Integer", additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("0", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "";
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditValue;
				AssertEquals("Integer", additionalInfo.CY_FormattedData_FieldType);
				AssertEquals("0", additionalInfo.CY_Data);
			}
		}

		public void TestAdditionalInfoAmountsData()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
			{
				SetupData();
				var declaration = Factory.New<JobDeclaration>();
				var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
				additionalInfo.CY_Data = "111 222";
				AssertEquals("111 222", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditValue;
				AssertEquals("111222", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
				additionalInfo.CY_Data = "111 222.333";
				AssertEquals("111 222.333", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditValue;
				AssertEquals("0", additionalInfo.CY_Data);
				additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
				AssertEquals("0", additionalInfo.CY_Data);
			}
		}

		public void TestCY_Data()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
			{
				SetupData();
				var declaration = Factory.New<JobDeclaration>();
				var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
				additionalInfo.CY_Code = "DT2";
				additionalInfo.CY_Data = "";
				AssertEquals("0.00", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "0";
				AssertEquals("0.00", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "ABC";
				AssertEquals("0.00", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "123.1";
				AssertEquals("123.10", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "123,123";
				AssertEquals("123.12", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "123.123";
				AssertEquals("123.12", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "-123.123";
				AssertEquals("-123.12", additionalInfo.CY_Data);
				additionalInfo.CY_Data = "123 123";
				AssertEquals("123123.00", additionalInfo.CY_Data);
			}
		}

		public void TestCY_FormattedData()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
			{
				SetupData();
				var declaration = Factory.New<JobDeclaration>();
				var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
				additionalInfo.CY_Code = "DT2";
				AssertEquals("0,00", additionalInfo.CY_FormattedData);
				additionalInfo.CY_Data = "0";
				AssertEquals("0,00", additionalInfo.CY_FormattedData);
				additionalInfo.CY_Data = "ABC";
				AssertEquals("0,00", additionalInfo.CY_FormattedData);
				additionalInfo.CY_Data = "123.1";
				AssertEquals("123,10", additionalInfo.CY_FormattedData);
				additionalInfo.CY_Data = "123.123";
				AssertEquals("123,12", additionalInfo.CY_FormattedData);
				additionalInfo.CY_Data = "123.123";
				AssertEquals("123,12", additionalInfo.CY_FormattedData);
				additionalInfo.CY_Data = "-123.123";
				AssertEquals("-123,12", additionalInfo.CY_FormattedData);
				additionalInfo.CY_Data = "123 123";
				AssertEquals("123123,00", additionalInfo.CY_FormattedData);
			}
		}

		public void TestDescription()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
			addInfo1.CY_Order = 1;
			addInfo1.CY_Data = "RCC1";
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditValue;
			addInfo2.CY_Order = 1;
			addInfo2.CY_Data = "123";
			AssertEquals("Rebate Credit Certificate - 123", addInfo1.Description);
			AssertEquals("Credit Rebate Value - RCC1", addInfo2.Description);
		}

		public void TestGrouping()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
			addInfo1.CY_Order = 1;
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditValue;
			addInfo2.CY_Order = 2;
			AssertEquals(ZString.Empty, addInfo1.Grouping);
			AssertEquals(ZString.Empty, addInfo2.Grouping);
			var addInfo3 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo3.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
			addInfo3.CY_Order = 3;
			AssertEquals("1", addInfo1.Grouping);
			AssertEquals("2", addInfo2.Grouping);
		}

		public void TestHasPair()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo0 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo0.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
			Assert(addInfo0.HasPair);
		}

		public void TestHasMultiplePairs()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.ApprovedExporter);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo0 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo0.CY_Code = UniversalReferenceConstants.AdditionalInformation.ApprovedExporter;
			addInfo0.CY_Order = 1;
			var addInfo3 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo3.CY_Code = UniversalReferenceConstants.AdditionalInformation.ApprovedExporter;
			addInfo3.CY_Order = 1;
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
			addInfo1.CY_Order = 1;
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditValue;
			addInfo2.CY_Order = 2;
			CombineAssertions(() =>
			{
				Assert("ApprovedExporter should not be HasMultiplePairs", !addInfo0.HasMultiplePairs);
				Assert("ApprovedExporter should not be HasMultiplePairs", !addInfo3.HasMultiplePairs);
				Assert("RebateCreditCertificate should not be HasMultiplePairs if only one exists", !addInfo1.HasMultiplePairs);
				Assert("RebateCreditValue should not be HasMultiplePairs if only one exists", !addInfo2.HasMultiplePairs);
				var addInfo4 = entryLine.AdditionalInformationCodes.AddNew();
				addInfo4.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
				addInfo4.CY_Order = 1;
				Assert("RebateCreditCertificate should be HasMultiplePairs if multipe exist", addInfo1.HasMultiplePairs);
				Assert("RebateCreditValue should be HasMultiplePairs if multipe exist", addInfo2.HasMultiplePairs);
			});
		}

		public void TestCloneReturnsCorrectType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo = entryLine.AdditionalInformationCodes.AddNew();
			addInfo.CY_Code = "CC";
			addInfo.CY_Data = "Data";
			var clonedAddInfo = (AdditionalInformation)addInfo.Clone();
			AssertEquals("clonedAddInfo.CY_Code", "CC", clonedAddInfo.CY_Code);
			AssertEquals("clonedAddInfo.CY_Data", "Data", clonedAddInfo.CY_Data);
		}

		public void TestCaseForcedUpperForCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo = entryLine.AdditionalInformationCodes.AddNew();
			addInfo.CY_Code = "bnd";
			AssertEquals("BND", addInfo.CY_Code);
		}

		public void TestVTEAllowEmpty()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("VTE");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo = entryLine.AdditionalInformationCodes.AddNew();
			addInfo.CY_Code = "VTE";
			AssertNoMessageErrors(addInfo.CY_DataInfo);
			addInfo.CY_Data = "111";
			AssertNoMessageErrors(addInfo.CY_DataInfo);
			addInfo.CY_Data = "";
			AssertNoMessageErrors(addInfo.CY_DataInfo);
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var info = entryLine.AdditionalInformationCodes.AddNew();
			AssertEquals(3, info.CY_CodeInfo.MaxLength);
			AssertEquals(32, info.CY_DataInfo.MaxLength);
		}

		public void TestIAdditionalInformationMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = "ZZZ";
			addInfo1.CY_Data = "ZZZData";
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = "AAA";
			addInfo2.CY_Data = "AAAData";
			var addInfo3 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo3.CY_Code = "MMM";
			addInfo3.CY_Data = "MMMData";
			var testInterface = addInfo1 as IAdditionalInformation;
			AssertEquals("ZZZ", testInterface.Code);
			AssertEquals("ZZZData", testInterface.Value);
			testInterface = addInfo2;
			AssertEquals("AAA", testInterface.Code);
			AssertEquals("AAAData", testInterface.Value);
			testInterface = addInfo3;
			AssertEquals("MMM", testInterface.Code);
			AssertEquals("MMMData", testInterface.Value);
		}

		public void TestAmount()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
			{
				SetupData();
				var declaration = Factory.New<JobDeclaration>();
				var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
				additionalInfo.CY_Code = "DT2";
				additionalInfo.CY_Data = "";
				AssertEquals(0m, additionalInfo.Amount);
				additionalInfo.CY_Data = "0";
				AssertEquals(0m, additionalInfo.Amount);
				additionalInfo.CY_Data = "ABC";
				AssertEquals(0m, additionalInfo.Amount);
				additionalInfo.CY_Data = "123.1";
				AssertEquals(123.10m, additionalInfo.Amount);
				additionalInfo.CY_Data = "123,123";
				AssertEquals(123.12m, additionalInfo.Amount);
				additionalInfo.CY_Data = "123.123";
				AssertEquals(123.12m, additionalInfo.Amount);
				additionalInfo.CY_Data = "-123.123";
				AssertEquals(-123.12m, additionalInfo.Amount);
				additionalInfo.CY_Data = "123 123";
				AssertEquals(123123m, additionalInfo.Amount);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return entryLine.AdditionalInformationCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<AdditionalInformation>();

		protected override IEnumerable<AdditionalInformation> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<AdditionalInformation>();
			var declaration = factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.AdditionalInformationCodes.Add(result);
			yield return result;
		}

		void SetupData()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue);
			helper.CreateAdditionalInformationCusCodeEntry("DT1");
			helper.CreateAdditionalInformationCusCodeEntry("DT2");
			Factory.Save();
		}
	}
}
