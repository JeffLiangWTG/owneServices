using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_WeightUQ()
		{
			invoiceLine.JI_WeightUQ = "~";
			AssertHasMessageError(invoiceLine.JI_WeightUQInfo, JobComInvoiceLineValidation.WeightUQShouldBeInList);
			invoiceLine.JI_WeightUQ = invoiceLine.Lookups.WeightUQList[0].Code;
			AssertNoMessageError(invoiceLine.JI_WeightUQInfo, JobComInvoiceLineValidation.WeightUQShouldBeInList);
		}

		public void TestCheckJI_ParentID()
		{
			invoiceLine.JI_ParentID = invoiceLine.PK;
			AssertHasError(invoiceLine.JI_ParentIDInfo, JobComInvoiceLineValidation.YouHaveSetItselfToParent);

			invoiceLine.JI_ParentID = invoice.JobComInvoiceLines.AddNew().PK;
			AssertNoError(invoiceLine.JI_ParentIDInfo, JobComInvoiceLineValidation.YouHaveSetItselfToParent);
		}

		public void TestParent()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestAutoCreateProductWarnings()
		{
			CusClassification cusClass = Factory.New<CusClassification>();
			invoiceLine.JI_PartNo = "Z!-!";
			AssertNull(invoiceLine.Part);
			invoiceLine.JI_CC = ZGuid.Empty;
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNoWarnings(invoiceLine.JI_CCInfo);
			AssertHasWarning(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.TariffOrLookUpIsRequiredForAutoCreateProduct);
			invoiceLine.JI_CC = cusClass.PK;
			AssertNoWarning(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.TariffOrLookUpIsRequiredForAutoCreateProduct);
			invoiceLine.JI_CC = ZGuid.Empty;
			invoiceLine.JI_Tariff = "1010101010";
			AssertNoWarning(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.TariffOrLookUpIsRequiredForAutoCreateProduct);
		}

		public void TestProductCodeFoundButNotRelatedToSupplierBuyerCombination()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var importerForDeclaration = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_OH_Importer = importerForDeclaration.PK;

			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OH_Supplier = invoiceSupplier.PK;

			var supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "TEST";
			product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			product1.RelatedOrganisations.AddOrganisationIfNotExist(importerForDeclaration.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			invoiceLine.JI_PartNo = product1.OP_PartNum;
			AssertHasWarning(invoiceLine.JI_PartNoInfo, JobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);

			invoice.JZ_OH_Supplier = supplier2.PK;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, JobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
		}

		public virtual void TestCheckJI_TariffIsValid()
		{
			ZString tariff = "0000000000";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Tariff = tariff;
			AssertNull("Tariff '0000.00.00 00' should not exist", invoiceLine.ScheduleBTariff);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_Tariff = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_TariffHasCalculateError()
		{
			invoiceLine.TariffCalculateExceptionMessage = "Calculate error";
			invoiceLine.Validation.ValidateAll();
			AssertHasErrorContaining("InvoiceLine has calculate error", invoiceLine.JI_TariffInfo, "Calculate error");

			invoiceLine.TariffCalculateExceptionMessage = ZString.Empty;
			invoiceLine.Validation.ValidateAll();
			AssertNoErrorContaining("InvoiceLine has no calculate error", invoiceLine.JI_TariffInfo, "Calculate error");
		}

		public void TestMaximumTariffPerLine()
		{
			JobComInvoiceLine additionalInvoiceLine1 = invoiceLine.AddSecondaryInvoiceLine();
			AssertNoMessageErrors(additionalInvoiceLine1.JI_ParentIDInfo);

			JobComInvoiceLine additionalInvoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			AssertNoMessageErrors(additionalInvoiceLine2.JI_ParentIDInfo);

			JobComInvoiceLine additionalInvoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			AssertNoMessageErrors(additionalInvoiceLine3.JI_ParentIDInfo);

			JobComInvoiceLine additionalInvoiceLine4 = invoiceLine.AddSecondaryInvoiceLine();
			AssertNoMessageErrors(additionalInvoiceLine4.JI_ParentIDInfo);

			JobComInvoiceLine additionalInvoiceLine5 = invoiceLine.AddSecondaryInvoiceLine();
			AssertNoMessageErrors(additionalInvoiceLine5.JI_ParentIDInfo);

			JobComInvoiceLine additionalInvoiceLine6 = invoiceLine.AddSecondaryInvoiceLine();
			AssertNoMessageErrors(additionalInvoiceLine6.JI_ParentIDInfo);

			JobComInvoiceLine additionalInvoiceLine7 = invoiceLine.AddSecondaryInvoiceLine();
			AssertNoMessageErrors(additionalInvoiceLine7.JI_ParentIDInfo);

			JobComInvoiceLine additionalInvoiceLine8 = invoiceLine.AddSecondaryInvoiceLine();
			AssertHasMessageError(additionalInvoiceLine8.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);

			declaration.RunPreSaveValidation();
			AssertHasMessageError(additionalInvoiceLine1.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertHasMessageError(additionalInvoiceLine2.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertHasMessageError(additionalInvoiceLine3.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertHasMessageError(additionalInvoiceLine4.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertHasMessageError(additionalInvoiceLine5.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertHasMessageError(additionalInvoiceLine6.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertHasMessageError(additionalInvoiceLine7.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);

			AssertNoExceptionThrown(delegate
			{
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
			});

			additionalInvoiceLine3.Delete();

			AssertNoMessageError(additionalInvoiceLine1.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertNoMessageError(additionalInvoiceLine2.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertNoMessageError(additionalInvoiceLine4.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertNoMessageError(additionalInvoiceLine5.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertNoMessageError(additionalInvoiceLine6.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
			AssertNoMessageError(additionalInvoiceLine7.JI_ParentIDInfo, JobComInvoiceLineValidation.MaxSecondaryLinesCount);
		}

		public void TestSecondarySPIForAnyChangeInTariff()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_OGACodes = "AAAFD4";

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1234567891";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff2.UE_OGACodes = "AAAFD2";

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_Tariff = "1234567890";

			var firstVLine = invoiceLine.AddSecondaryInvoiceLine();
			var secondVLine = invoiceLine.AddSecondaryInvoiceLine();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			firstVLine.JI_Tariff = "1234567891";
			secondVLine.JI_Tariff = "1234567891";
			AssertHasMessageError("There is a error in the first V line due to different tariff number from X line", firstVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertNoMessageError("There is no error in the second V line despite of having different tariff number from X line", secondVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);

			firstVLine.JI_Tariff = "1234567890";
			invoiceLine.US_SupTariff = "99038801";
			firstVLine.Validation.ValidateAll();
			secondVLine.Validation.ValidateAll();

			AssertHasMessageError("There is an error in the first V line cause it has a different prov/prog tariff than X line", firstVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertNoMessageError("There is no error in the V child line despite of having different prov/prog tariff number from X line", secondVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);

			firstVLine.US_SupTariff = "99038801";
			firstVLine.Validation.ValidateAll();
			secondVLine.Validation.ValidateAll();

			AssertNoMessageError("There is no error in the first V line cause it has same prov/prog tariff than X line", firstVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertNoMessageError("There is no error in the V child line despite of having different prov/prog tariff number from X line", secondVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);

			secondVLine.JI_Tariff = "1234567890";
			secondVLine.US_SupTariff = "99038801";
			secondVLine.JI_LineNo = 2;
			firstVLine.JI_LineNo = 3;
			AssertNoMessageError("fist V line now become second V line -> no error despite of having different tariff number from X line", firstVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertNoMessageError("second V line now become first V line -> no error because this V line has the same tariff number as X line's", secondVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);

			secondVLine.JI_ParentID = firstVLine.PK;
			secondVLine.JI_LineNo = 3;
			firstVLine.JI_LineNo = 2;
			firstVLine.US_SupTariff = "99038801";
			firstVLine.JI_Tariff = "";
			secondVLine.JI_Tariff = "1234567890";
			secondVLine.US_SupTariff = "99038801";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNoMessageError("There is no error in the V parent line cause it's child v line has same tariff number with X line", secondVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertNoMessageError("There is no error in the V child line which has same tariff number with X line", firstVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);

			secondVLine.JI_Tariff = "1234567891";
			firstVLine.Validation.ValidateAll();
			AssertHasMessageError("There is a error in the child V line due to different tariff number from X line", secondVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertHasMessageError("There is a error in the parent V line due to child has different tariff number from X line", firstVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
		}

		public void TestCheckJI_PartNo_SerialNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = new ZDate(2000, 1, 2);
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier";
			var supRelation = part.RelatedOrganisations.AddSupplier(supplier);
			invoice.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			invoice.JZ_OH_Buyer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningWithNoAttribValues = $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (),  (),  (), Serial Number () and Effective Date ({invoiceLine.EffectiveDateForDutyRate}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningWithNoAttribValues);
			invoiceLine.JI_PartAttrib1 = "1";
			invoiceLine.JI_PartAttrib2 = "2";
			invoiceLine.JI_PartAttrib3 = "3";
			invoiceLine.JI_SerialNumber = "SN";
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningWithAttribValues = $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (1),  (2),  (3), Serial Number (SN) and Effective Date ({invoiceLine.EffectiveDateForDutyRate}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningWithAttribValues);
		}

		public void TestCheckJI_PartAttrib1()
		{
			TestCheckJI_PartAttribCore(x => x.JI_PartAttrib1 = "1",
				x => $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (1),  (),  (), Serial Number () and Effective Date ({x.EffectiveDateForDutyRate}).");
		}

		public void TestCheckJI_PartAttrib2()
		{
			TestCheckJI_PartAttribCore(x => x.JI_PartAttrib2 = "2",
				x => $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (),  (2),  (), Serial Number () and Effective Date ({x.EffectiveDateForDutyRate}).");
		}

		public void TestCheckJI_PartAttrib3()
		{
			TestCheckJI_PartAttribCore(x => x.JI_PartAttrib3 = "3",
				x => $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (),  (),  (3), Serial Number () and Effective Date ({x.EffectiveDateForDutyRate}).");
		}

		void TestCheckJI_PartAttribCore(Action<JobComInvoiceLine> setAttribAction, Func<JobComInvoiceLine, string> getExpectedWarningWithValues)
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = new ZDate(2000, 1, 2);
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier";
			var supRelation = part.RelatedOrganisations.AddSupplier(supplier);
			invoice.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			invoice.JZ_OH_Buyer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningWithNoAttribValues = $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (),  (),  (), Serial Number () and Effective Date ({invoiceLine.EffectiveDateForDutyRate}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningWithNoAttribValues);

			setAttribAction(invoiceLine);
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, getExpectedWarningWithValues(invoiceLine));
		}

		public void TestCheckJI_SerialNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = new ZDate(2000, 1, 2);
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier";
			var supRelation = part.RelatedOrganisations.AddSupplier(supplier);
			invoice.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			invoice.JZ_OH_Buyer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningWithNoAttribValues = $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (),  (),  (), Serial Number () and Effective Date ({invoiceLine.EffectiveDateForDutyRate}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningWithNoAttribValues);

			invoiceLine.JI_SerialNumber = "SN";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (),  (),  (), Serial Number (SN) and Effective Date ({invoiceLine.EffectiveDateForDutyRate}).");
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
