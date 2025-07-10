using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.TariffValidation;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using System.Diagnostics;
	using CargoWise.Common.Testing;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
	using Enterprise.Customs.NZ.Business.MasterFiles;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	public class JobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestCheckJI_OH_TreatmentProvider()
		{
			var treatmentProvider = Factory.New<OrgHeader>();
			treatmentProvider.FillWithValidTestData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0203.11.00.02C";
			AssertNoMessageErrors("Export entry should not validate this", invoiceLine.JI_OH_TreatmentProviderInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertNoMessageErrors("Import entry when TSW not activated should not validate this either", invoiceLine.JI_OH_TreatmentProviderInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			invoiceLine.JI_OH_TreatmentProvider = treatmentProvider.PK;
			AssertHasMessageError("TreatmentProvider", invoiceLine.JI_OH_TreatmentProviderInfo, JobComInvoiceLineValidation.TreatmentProviderMissingCCD);

			treatmentProvider.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "12345678A");
			invoiceLine.JI_OH_TreatmentProvider = ZGuid.Empty;
			invoiceLine.JI_OH_TreatmentProvider = treatmentProvider.PK;
			AssertNoMessageError("TreatmentProvider", invoiceLine.JI_OH_TreatmentProviderInfo, JobComInvoiceLineValidation.TreatmentProviderMissingCCD);
		}

		public void TestValidateJI_LineNo()
		{
			AssertNoError("Default position", invoiceLine.JI_LineNoInfo, JobComInvoiceLineValidation.EmptyContainerEntryCannotHaveInvoiceLines);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_LineNo = 1;
			AssertNoError("No Containers entered", invoiceLine.JI_LineNoInfo, JobComInvoiceLineValidation.EmptyContainerEntryCannotHaveInvoiceLines);

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCU0000001";
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			invoiceLine.JI_LineNo = 1;
			AssertNoError("No Containers entered", invoiceLine.JI_LineNoInfo, JobComInvoiceLineValidation.EmptyContainerEntryCannotHaveInvoiceLines);

			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			invoiceLine.JI_LineNo = 1;
			AssertHasError("Is TSW Empty Containers declaration", invoiceLine.JI_LineNoInfo, JobComInvoiceLineValidation.EmptyContainerEntryCannotHaveInvoiceLines);
		}

		public void TestValidateJI_CustomsQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = "NMB";
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, JobComInvoiceLineValidation.MessageErrorMustHaveStatQty);
			invoiceLine.OtherInfos.AddNew(LineOtherInfoList.Codes.Parts, "");
			AssertNoNotifications(invoiceLine.JI_CustomsQuantityInfo);
			invoiceLine.JI_CustomsQuantity = 1.00m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, JobComInvoiceLineValidation.MessageErrorRemoveStatQtyWhenUsingPTSOtherInfo);
			invoiceLine.OtherInfos.RemoveAndDeleteAll();
			AssertNoNotifications(invoiceLine.JI_CustomsQuantityInfo);
			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsQuantity = 1.00m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, JobComInvoiceLineValidation.MessageErrorRemoveStatQtyAsTariffDoesntRequireIt);
			invoiceLine.JI_CustomsQuantity = 0.00m;
			AssertNoNotifications(invoiceLine.JI_CustomsQuantityInfo);
		}

		public void TestInvoiceLineQty()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			invoiceLine.JI_InvoiceQuantity = 0;
			invoiceLine.JI_InvoiceUQ = "CTN";
			AssertNoNotifications("Non TSW declaration validation for Inv Qty should not change", invoiceLine.JI_InvoiceQuantityInfo);
			AssertNoNotifications("Non TSW declaration validation for Inv UQ should not change", invoiceLine.JI_InvoiceUQInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_InvoiceQuantity = 0;
			invoiceLine.JI_InvoiceUQ = "";
			AssertHasWarning("Invoice Qty is Recommended for TSW CRE", invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.NoOfPackagesMissingWarning);
			AssertHasWarning("Invoice UQ is Recommended for TSW CRE", invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.TypeOfPackagesMissingWarning);

			invoiceLine.JI_InvoiceQuantity = 5;
			invoiceLine.JI_InvoiceUQ = "CT";
			AssertNoWarning("Invoice Qty is Recommended for TSW CRE", invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.NoOfPackagesMissingWarning);
			AssertNoWarning("Invoice UQ is Recommended for TSW CRE", invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.TypeOfPackagesMissingWarning);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_InvoiceQuantity = 0;
			invoiceLine.JI_InvoiceUQ = "";
			AssertHasWarning("Invoice Qty is Recommended for TSW", invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.NoOfPackagesMissingWarning);
			AssertNoWarnings(invoiceLine.JI_InvoiceUQInfo);
			AssertNoMessageErrors(invoiceLine.JI_InvoiceUQInfo);
			AssertNoErrors(invoiceLine.JI_InvoiceUQInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_InvoiceQuantity = (ZDecimal)int.MaxValue + 10.0m;
			AssertHasError(invoiceLine.JI_InvoiceQuantityInfo, invoiceLine.Validation.InvoiceQuantityOversizeError);
		}

		public void TestValidateJI_InvoiceUQ()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			Factory.Save();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var invoiceHeader = jobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "NMB";
			AssertHasMessageError("For TSW CRE, list validation applies to required UQ list - previous list values are now message errors", invoiceLine.JI_InvoiceUQInfo, "The code you have selected is not in the list.");
			invoiceLine.JI_InvoiceUQ = "III";
			AssertHasMessageError("Invalid values will also error", invoiceLine.JI_InvoiceUQInfo, "The code you have selected is not in the list.");
			invoiceLine.JI_InvoiceUQ = "BX";
			AssertNoNotifications("Valid uq from TSW list", invoiceLine.JI_InvoiceUQInfo);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			invoiceLine.JI_InvoiceUQ = "III";
			AssertHasMessageError("Invalid values will also error", invoiceLine.JI_InvoiceUQInfo, "The code you have selected is not in the list.");
			invoiceLine.JI_InvoiceUQ = "";
			AssertHasMessageError("Blank values will also error", invoiceLine.JI_InvoiceUQInfo, "You have not entered an Invoice UQ.");
		}

		public void TestInvoiceLineWeight()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			invoiceLine.JI_Weight = 0;
			AssertNoNotifications("Non TSW declaration validation for Volume should not change", invoiceLine.JI_WeightInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_Weight = 0;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertHasWarning("Weight is mandatory for Line Item for TSW declarations", invoiceLine.JI_WeightInfo, JobComInvoiceLineValidation.GrossWeightRequired);

			invoiceLine.JI_Weight = 28.5m;
			AssertNoWarning("Weight is mandatory for Line Item for TSW declarations", invoiceLine.JI_WeightInfo, JobComInvoiceLineValidation.GrossWeightRequired);
		}

		public void TestInvoiceLineNetWeight()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			invoiceLine.JI_NetWeight = 0;
			AssertNoNotifications("Non TSW declaration validation for Volume should not change", invoiceLine.JI_NetWeightInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_NetWeight = 0;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertHasWarning("Weight is mandatory for Line Item for TSW declarations", invoiceLine.JI_NetWeightInfo, JobComInvoiceLineValidation.NetWeightRequired);

			invoiceLine.JI_NetWeight = 32m;
			AssertNoWarning("Weight is mandatory for Line Item for TSW declarations", invoiceLine.JI_NetWeightInfo, JobComInvoiceLineValidation.NetWeightRequired);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_NetWeight = 0;
			AssertNoWarning("Net Weight is not required for TSW CRE declarations", invoiceLine.JI_NetWeightInfo, JobComInvoiceLineValidation.NetWeightRequired);
		}

		public void TestValidateItemPackaging()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_NetWeight = 0;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.RunPreSaveValidation();
			AssertHasRowMessageError(invoiceLine, JobComInvoiceLineValidation.AtLeast1PackagingLineRequired);

			invoiceLine.NumberOfPackages1 = 25;
			invoiceLine.RunPreSaveValidation();
			AssertNoRowMessageErrorContaining(invoiceLine, JobComInvoiceLineValidation.AtLeast1PackagingLineRequired);
		}

		public void TestValidateLevyCreditAmountCode()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("Non drawback declaration should not validate field", false, invoiceLine.JI_LevyCreditAmountCodeInfo.HasMessageErrors());

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("Legacy drawback declaration should not validate field", false, invoiceLine.JI_LevyCreditAmountCodeInfo.HasMessageErrors());

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_LevyCreditAmountCode = "BAD";
			AssertEquals("TSW Drawback declaration should validate field in error", true, invoiceLine.JI_LevyCreditAmountCodeInfo.HasMessageErrors());

			invoiceLine.JI_LevyCreditAmountCode = LevyCodesList.Codes.HERA;
			AssertEquals("TSW Drawback declaration should validate field as valid now", false, invoiceLine.JI_LevyCreditAmountCodeInfo.HasMessageErrors());
		}

		#region TestValidateTariff
		public void TestValidateTariff()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
			invoiceLine.JI_Tariff = "";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeMissing);

			invoiceLine.JI_PartNo = "NEWPART";
			invoiceLine.JI_CC = ZGuid.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);

			invoiceLine.JI_Tariff = "8008";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "NEWCODE";
			classification.CC_IsActive = true;
			Factory.Save();

			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
		}
		#endregion

		#region TestValidateCountryOfOriginWhenDefaultEnteredAgainstInvoiceHeader
		public void TestValidateCountryOfOriginWhenDefaultEnteredAgainstInvoiceHeader()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_CountryOfOrigin = "";
			AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfOrigin);
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);
			invoiceHeader.JZ_RN_NKDefaultOrigin = "";
			AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfOrigin);
			invoiceLine.JI_CountryOfOrigin = "AU";
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_CountryOfOrigin = "";
			AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfOrigin);
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);
			invoiceHeader.JZ_RN_NKDefaultOrigin = "";
			AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfOrigin);
			invoiceLine.JI_CountryOfOrigin = "AU";
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);
		}
		#endregion

		#region TestValidateCountryOfExportWhenDefaultEnteredAgainstInvoiceHeader
		public void TestValidateCountryOfExportWhenDefaultEnteredAgainstInvoiceHeader()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_RN_NKCountryOfExport = "";
			AssertHasMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);
			invoiceHeader.JZ_RN_NKDefaultExport = "AU";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);
			invoiceHeader.JZ_RN_NKDefaultExport = "";
			AssertHasMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_RN_NKCountryOfExport = "";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);
			invoiceHeader.JZ_RN_NKDefaultExport = "AU";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);
			invoiceHeader.JZ_RN_NKDefaultExport = "";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);
		}
		#endregion

		#region TestValidateQualifiesForPrefDutyWhenDefaultEnteredAgainstInvoiceHeader
		public void TestValidateQualifiesForPrefDutyWhenDefaultEnteredAgainstInvoiceHeader()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_QualifiesForPreferentialDuty = "";
			AssertHasMessageError(invoiceLine.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertNoMessageErrors(invoiceLine.JI_QualifiesForPreferentialDutyInfo);
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = "";
			AssertHasMessageError(invoiceLine.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertNoMessageErrors(invoiceLine.JI_QualifiesForPreferentialDutyInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_QualifiesForPreferentialDuty = "";
			AssertNoMessageErrors(invoiceLine.JI_QualifiesForPreferentialDutyInfo);
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertNoMessageErrors(invoiceLine.JI_QualifiesForPreferentialDutyInfo);
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = "";
			AssertNoMessageErrors(invoiceLine.JI_QualifiesForPreferentialDutyInfo);
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertNoMessageErrors(invoiceLine.JI_QualifiesForPreferentialDutyInfo);
		}
		#endregion

		#region TestPerformanceOfHeaderToLineValidationOnCountryOfOriginFor1000LineInvoice
		[SnailTest()]
		public void TestPerformanceOfHeaderToLineValidationOnCountryOfOriginFor1000LineInvoice()
		{
			for (int myCount = 1; myCount < 1000; myCount++)
			{
				JobComInvoiceLine addedLine = declaration.FilteredInvoiceLines.AddNew();
				addedLine.JI_JZ = invoiceHeader.PK;
			}

			AssertEquals("Precondition: Declaration.InvoiceLines.Count", 1000, declaration.FilteredInvoiceLines.Count);
			AssertEquals("Precondition: InvoiceHeader.JobComInvoiceLines.Count", 1000, invoiceHeader.JobComInvoiceLines.Count);

			DisposableLeakListener.Instance.StackTraceEnabled = false;
			var hpc = new Stopwatch();

			hpc.Start();

			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";

			double difference = hpc.Elapsed.TotalSeconds;
			const int AcceptableTime = 2;
			Assert("Test took over acceptable time (" + AcceptableTime.ToString() + " seconds) : Time taken to run test was " + difference.ToString(), difference < AcceptableTime);
		}
		#endregion

		#region TestJI_ParentLineLessThan24
		public void TestJI_ParentLineLessThan24()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();

			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 25;//NZD
			line1.JI_ParentLineNo = line.JI_LineNo.ToString();
			AssertEquals("Parent Line is wrong as > 24", true, line1.JI_ParentLineNoInfo.HasMessageErrors());
		}
		#endregion

		#region TestValidateJI_ParentLineNo
		public void TestValidateJI_ParentLineNo()
		{
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentLineNo = line2.JI_LineNoString;
			AssertEquals("Valid", false, invoiceLine.JI_ParentLineNoInfo.HasErrors());

			invoiceLine.JI_ParentLineNo = "5";
			invoiceLine.Validation.ValidateJI_ParentLine();
			AssertEquals("InValid", true, invoiceLine.JI_ParentLineNoInfo.HasErrors());

			invoiceLine.JI_ParentLineNo = "";
			AssertEquals("Valid", false, invoiceLine.JI_ParentLineNoInfo.HasErrors());
		}
		#endregion

		#region TestValidateCountryOfOrigin
		public void TestValidateCountryOfOrigin()
		{
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Empty is a message error", true, invoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());

			invoiceLine.JI_CountryOfOrigin = "ZZ";
			AssertEquals("Invalid", true, invoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());

			invoiceLine.JI_CountryOfOrigin = "AU";
			AssertEquals("Valid", false, invoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());
		}
		#endregion

		#region TestValidateCountryOfOriginAndConcessionCode

		public void TestValidateCountryOfOriginAndConcessionCode()
		{
			var warning = "There are currently sanctions in place for goods exported from Russian Federation, please enter the applicable concession number";
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var concession = Factory.New<NZCConcession>();
				concession.U2_Code = "LALALAL";
				concession.U2_DateActiveFrom = new ZDateTime(2022, 1, 1);
				concession.U2_Description = "Test concession code";

				invoiceLine.JI_CountryOfOrigin = "RU";
				invoiceLine.JI_ConcessionCode = "LALALAL";
				AssertNoWarning("Should have no warning when country of origin is Russia and concession code is set to a valid code", invoiceLine.JI_CountryOfOriginInfo, warning);
				AssertNoWarning("Should have no warning when country of origin is Russia and concession code is set to a valid code", invoiceLine.JI_ConcessionCodeInfo, warning);

				invoiceLine.JI_CountryOfOrigin = "RU";
				invoiceLine.JI_ConcessionCode = ZString.Empty;
				AssertHasWarning("Should have warning when country of origin is Russia but concession code is blank", invoiceLine.JI_CountryOfOriginInfo, warning);
				AssertHasWarning("Should have warning when country of origin is Russia but concession code is blank", invoiceLine.JI_ConcessionCodeInfo, warning);

				invoiceLine.JI_CountryOfOrigin = "AU";
				invoiceLine.JI_ConcessionCode = ZString.Empty;
				AssertNoWarning("Should have no warning when concession code is blank but country of origin is not Russia", invoiceLine.JI_CountryOfOriginInfo, warning);
				AssertNoWarning("Should have no warning when concession code is blank but country of origin is not Russia", invoiceLine.JI_ConcessionCodeInfo, warning);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);

				var validation = invoiceLine.Validation;
				invoiceLine.JI_CountryOfOrigin = "RU";
				invoiceLine.JI_ConcessionCode = "LALALAL";
				validation.ValidateJI_CountryOfOrigin();
				AssertNoWarning("Should have no warning when country of origin is Russia and concession code is set to a valid code", invoiceLine.JI_CountryOfOriginInfo, warning);
				AssertNoWarning("Should have no warning when country of origin is Russia and concession code is set to a valid code", invoiceLine.JI_ConcessionCodeInfo, warning);

				invoiceLine.JI_CountryOfOrigin = "RU";
				invoiceLine.JI_ConcessionCode = ZString.Empty;
				validation.ValidateJI_CountryOfOrigin();
				AssertHasWarning("Should have warning when country of origin is Russia but concession code is blank", invoiceLine.JI_CountryOfOriginInfo, warning);
				AssertHasWarning("Should have warning when country of origin is Russia but concession code is blank", invoiceLine.JI_ConcessionCodeInfo, warning);

				invoiceLine.JI_CountryOfOrigin = "AU";
				invoiceLine.JI_ConcessionCode = ZString.Empty;
				validation.ValidateJI_CountryOfOrigin();
				AssertNoWarning("Should have no warning when concession code is blank but country of origin is not Russia", invoiceLine.JI_CountryOfOriginInfo, warning);
				AssertNoWarning("Should have no warning when concession code is blank but country of origin is not Russia", invoiceLine.JI_ConcessionCodeInfo, warning);
			}
		}

		#endregion

		#region TestValidateJI_Description
		public void TestValidateJI_Description()
		{
			invoiceLine.JI_Description = ZString.Empty;
			AssertEquals("Description shouldn't be empty", true, invoiceLine.JI_DescriptionInfo.HasMessageErrors());

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_Description = "2007 FORD FPV PURSUIT   VIN  6FPAAAJGCM7K62816  I, THE UNDERSIGNED, BEING THE IMPORTER OF THE VEHICLE DECLARED IN THIS IMPORT ENTRY, UNDERTAKE THAT SHOULD I SELL OR OTHERWISE DISPOSE OF THE VEHICLE WITHIN 2 YEARS FROM THE DATE OF IMPORTATION I WILL IMMEDIATELY PAY NZ CUSTOMS THE SUM OF $1743.15, OR ANY LESSER AMOUNT THAT MAY BE REQUIRED.   .................................................... NAME AND SIGNATURE OF IMPORTER";
			AssertHasWarning("Description should warn user it will be truncated in the TSW message.", invoiceLine.JI_DescriptionInfo, JobComInvoiceLineValidation.GoodsDescTooLong);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "2007 FORD FPV PURSUIT   VIN  6FPAAAJGCM7K62816  I, THE UNDERSIGNED, BEING THE IMPORTER OF THE VEHICLE DECLARED IN THIS IMPORT ENTRY, UNDERTAKE THAT SHOULD I SELL OR OTHERWISE DISPOSE OF THE VEHICLE WITHIN 2 YEARS FROM THE DATE OF IMPORTATION I WILL IMMEDIATELY PAY NZ CUSTOMS THE SUM OF $1743.15, OR ANY LESSER AMOUNT THAT MAY BE REQUIRED.   .................................................... NAME AND SIGNATURE OF IMPORTER";
			AssertNoWarning("Warning should not apply to a legacy declaration.", invoiceLine2.JI_DescriptionInfo, JobComInvoiceLineValidation.GoodsDescTooLong);
		}
		#endregion

		#region TestValidateJI_RN_NKCountryOfExport
		public void TestValidateJI_RN_NKCountryOfExport()
		{
			invoiceLine.Declaration.JE_MessageType = "IMP";
			invoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertEquals("Country/Region of export should be mandatory", true, invoiceLine.JI_RN_NKCountryOfExportInfo.HasMessageErrors());
		}
		#endregion

		#region TestValidateJI_QualifiesForPreferentialDuty
		public void TestValidateJI_QualifiesForPreferentialDuty()
		{
			invoiceLine.Declaration.JE_MessageType = "IMP";
			invoiceLine.JI_QualifiesForPreferentialDuty = "Z";
			AssertEquals("Invalid", true, invoiceLine.JI_QualifiesForPreferentialDutyInfo.HasMessageErrors());
		}
		#endregion

		#region AddInfo Validations
		public void TestValidateJI_ConcessionCode()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				const string warningMessage = "The use of this concession on excisable goods needs to be manually assessed with New Zealand Customs or it may be rejected. When submitting the entry, please use the ‘Manual Processing Request (Override)’ option.";
				const string preferentialCountryGroup = PreferenceClaimedList.Codes.NML;
				const string tariff = "0000.00.00.00N";
				const string concession = "100001C";

				var classification = Factory.New<NZCClassification>();
				classification.U0_Tariff = tariff;
				classification.U0_DateActiveFrom = ZDateTime.MinSmallDateTimeValue;
				classification.U0_DateActiveTo = ZDateTime.MaxSmallDateTimeValue;
				var rate = classification.DutyRates.AddNew();
				rate.U1_DateActiveFrom = ZDateTime.MinSmallDateTimeValue;
				rate.U1_DateActiveTo = ZDateTime.MaxSmallDateTimeValue;
				rate.U1_PreferentialCountryGroup = preferentialCountryGroup;
				rate.U1_DutyRatePerUnit1 = 10;
				invoiceLine.JI_Tariff = tariff;
				invoiceLine.JI_ConcessionCode = concession;

				CombineAssertions(() =>
				{
					AssertHasWarning("DutyRatePerUnit is 10, concession is 100001C", invoiceLine.JI_ConcessionCodeInfo, warningMessage);
					invoiceLine.JI_ConcessionCode = ZString.Empty;
					AssertNoWarning("DutyRatePerUnit is 10, concession is empty", invoiceLine.JI_ConcessionCodeInfo, warningMessage);
					invoiceLine.JI_ConcessionCode = "12345";
					AssertNoWarning("DutyRatePerUnit is 10, concession is different than 100001C", invoiceLine.JI_ConcessionCodeInfo, warningMessage);
					invoiceLine.JI_Tariff = "1234.00.00.00N";
					invoiceLine.JI_ConcessionCode = concession;
					AssertNoWarning("DutyRatePerUnit is 0, concession is different than 100001C", invoiceLine.JI_ConcessionCodeInfo, warningMessage);
				});
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "DTY");
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				helper.CreateTariffUOM(tariff, "CU1", "AAA");
				helper.CreateTariffUOM(tariff, "CU2", "BBB");
				var rate1 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "0");
				var rate2 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "(5.000000*(VFD/100))+(0.700240*[LMS])");
				var tradeGroup1 = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Australia");
				var tradeGroup2 = helper.LoadOrCreateTradeGroup("NZ", "US", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "United States");
				helper.CreateCusApplicability(rate1, tradeGroup1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100001A");
				helper.CreateCusApplicability(rate2, tradeGroup2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100001C");
				invoiceLine.JI_Tariff = "123456789";

				invoiceLine.JI_ConcessionCode = "INVALID";
				AssertHasWarningContaining(invoiceLine.JI_ConcessionCodeInfo, "Concession Code [INVALID] not recognized");
				invoiceLine.JI_ConcessionCode = "100001A";
				AssertNoWarnings(invoiceLine.JI_ConcessionCodeInfo);

				invoiceLine.JI_ConcessionCode = "100001C";
				AssertHasWarning(invoiceLine.JI_ConcessionCodeInfo, "The use of this concession on excisable goods needs to be manually assessed with New Zealand Customs or it may be rejected. When submitting the entry, please use the ‘Manual Processing Request (Override)’ option.");

				rate2.ZZ2_RateFormula = "5.000000*(VFD/100)";
				invoiceLine.Validation.ValidateJI_ConcessionCode();
				AssertNoWarnings(invoiceLine.JI_ConcessionCodeInfo);
			}
		}

		public void TestCheckConcessionCode_UseRefDb()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "123456789";

				invoiceLine.JI_ConcessionCode = "INVALID";
				AssertHasWarningContaining(invoiceLine.JI_ConcessionCodeInfo, "Concession Code [INVALID] not recognized");
				invoiceLine.JI_ConcessionCode = "100001A";
				AssertNoWarnings(invoiceLine.JI_ConcessionCodeInfo);
			}
		}

		public void TestValidateJI_PartsOfClassification()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2002, 12, 12);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			NZCClassification normalClassification = Factory.New<NZCClassification>();
			normalClassification.U0_Tariff = "0000.00.00.00Y";
			normalClassification.U0_DateActiveFrom = new ZDateTime(2002, 1, 1);
			normalClassification.U0_IsManual = false;

			NZCClassification manualClassification = Factory.New<NZCClassification>();
			manualClassification.U0_Tariff = "0000.00.00.00Z";
			manualClassification.U0_DateActiveFrom = new ZDateTime(2002, 1, 1);
			manualClassification.U0_IsManual = true;

			invoiceLine.JI_Tariff = normalClassification.U0_Tariff;
			AssertNoMessageErrors(invoiceLine.JI_PartsOfClassificationInfo);

			invoiceLine.JI_Tariff = manualClassification.U0_Tariff;
			AssertHasMessageError(invoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfPleaseEnterATariffCode);

			invoiceLine.JI_PartsOfClassification = manualClassification.U0_Tariff;
			AssertHasMessageError(invoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);

			invoiceLine.JI_Tariff = normalClassification.U0_Tariff;
			AssertHasMessageError(invoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfDontNeedATariffCodeHere);

			declaration.JE_MessageSubType = MessageTypeList.Codes.IPI;
			invoiceLine.JI_Tariff = manualClassification.U0_Tariff;
			AssertNoMessageError(invoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfPleaseEnterATariffCode);

			invoiceLine.JI_PartsOfClassification = normalClassification.U0_Tariff;
			AssertNoMessageErrors(invoiceLine.JI_PartsOfClassificationInfo);

			invoiceLine.JI_PartsOfClassification = manualClassification.U0_Tariff;
			AssertNoMessageError(invoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);

			invoiceLine.JI_Tariff = normalClassification.U0_Tariff;
			AssertNoMessageError(invoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfDontNeedATariffCodeHere);
		}

		public void TestQualifiesForPreferentialDutyForInvoiceLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_QualifiesForPreferentialDuty = "";
			AssertHasMessageError(invoiceLine.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertNoMessageErrors(invoiceLine.JI_QualifiesForPreferentialDutyInfo);
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			AssertNoMessageErrors(invoiceLine.JI_QualifiesForPreferentialDutyInfo);
			invoiceLine.JI_QualifiesForPreferentialDuty = "";
			AssertHasMessageError(invoiceLine.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);
		}

		public void TestQualifiesForPreferentialDutyForWriteOffDec()
		{
			var writeOffDec = Factory.NewWithValidTestData<JobDeclaration>();
			writeOffDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			writeOffDec.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			writeOffDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var invoiceHeader = writeOffDec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_QualifiesForPreferentialDuty = "";
			AssertHasMessageError(invoiceLine.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);

			writeOffDec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			writeOffDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_QualifiesForPreferentialDuty = "";
			AssertNoMessageError("Validation is not required for write of declaration", invoiceLine.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);
		}

		public void TestValidateCountryOfExportForInvoiceLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertHasMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);

			invoiceLine.JI_RN_NKCountryOfExport = "ZZ";
			AssertHasMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);

			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);
		}

		public void TestValidateCountryOfHeaderForWriteOffDec()
		{
			var writeOffDec = Factory.NewWithValidTestData<JobDeclaration>();
			writeOffDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			writeOffDec.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			writeOffDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var invoiceHeader = writeOffDec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertHasMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);

			writeOffDec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			writeOffDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertNoMessageError("Validation is not required for write of declaration", invoiceLine.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);
		}

		public void TestIntendedUseValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_IntendedUseCode = "";
			invoiceLine.JI_IntendedUse = "";
			AssertNoMessageErrors("Intended use can be blank in this tariff range", invoiceLine.JI_IntendedUseCodeInfo);

			invoiceLine.JI_Tariff = "0203.11.00.02C";
			invoiceLine.JI_IntendedUseCode = "";
			invoiceLine.JI_IntendedUse = "";
			AssertHasMessageError("Intended use is required for this tariff", invoiceLine.JI_IntendedUseCodeInfo, JobComInvoiceLineValidation.IntendedUseCodeRequired);
			AssertHasMessageError("Intended use is required for this tariff", invoiceLine.JI_IntendedUseInfo, JobComInvoiceLineValidation.IntendedUseCodeRequired);

			invoiceLine.JI_IntendedUseCode = IntendedUseCodeList.Codes.TS;
			AssertNoMessageError("Intended use has been provided", invoiceLine.JI_IntendedUseCodeInfo, JobComInvoiceLineValidation.IntendedUseCodeRequired);
			AssertNoMessageError("Intended use has been provided", invoiceLine.JI_IntendedUseInfo, JobComInvoiceLineValidation.IntendedUseCodeRequired);
			AssertNoMessageError("Intended use code is in list", invoiceLine.JI_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_IntendedUseCode = "";
			invoiceLine.JI_IntendedUse = "For use at the Waipapani Trade Show for demonstration purposes only.";
			AssertNoMessageError("Intended use has been provided", invoiceLine.JI_IntendedUseCodeInfo, JobComInvoiceLineValidation.IntendedUseCodeRequired);
			AssertNoMessageError("Intended use has been provided", invoiceLine.JI_IntendedUseInfo, JobComInvoiceLineValidation.IntendedUseCodeRequired);
			AssertNoMessageError("Intended use code is in list", invoiceLine.JI_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_IntendedUseCode = "XX";
			AssertHasMessageError("Intended use code must be in list", invoiceLine.JI_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_PreferentialCountryGroupWithDefaultingOn()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2009, 11, 21);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.PapuaNewGuinea;
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			invoiceLine.JI_Tariff = "0203.11.00.02C";

			AssertEquals("Precondition: Default value for invoiceLine.JI_PreferentialCountryGroup", ZString.Empty, invoiceLine.JI_PreferentialCountryGroup);

			AssertNoMessageErrors("No Group is fine.", invoiceLine.JI_PreferentialCountryGroupInfo);

			invoiceLine.JI_PreferentialCountryGroup = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			AssertHasMessageError(invoiceLine.JI_PreferentialCountryGroupInfo, JobComInvoiceLineValidation.MessageErrorPreferentialCountryGroupNotInList);

			invoiceLine.JI_PreferentialCountryGroup = "LDC"; // Should be valid for PG.
			AssertNoMessageErrors("LDC should be valid for Papua New Guinea", invoiceLine.JI_PreferentialCountryGroupInfo);

			invoiceLine.JI_PreferentialCountryGroup = ZString.Empty;
			AssertNoMessageErrors("No Group is fine.", invoiceLine.JI_PreferentialCountryGroupInfo);
		}

		public void TestCheckJI_PreferentialCountryGroupWithDefaultingOffForCountryWithOneGroupOnly_ie_AU()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2009, 11, 21);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			invoiceLine.JI_Tariff = "0203.11.00.02C";

			AssertEquals("Precondition: Default value for invoiceLine.JI_PreferentialCountryGroup", ZString.Empty, invoiceLine.JI_PreferentialCountryGroup);

			AssertNoMessageErrors("No Group is fine.", invoiceLine.JI_PreferentialCountryGroupInfo);

			invoiceLine.JI_PreferentialCountryGroup = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			AssertHasMessageError(invoiceLine.JI_PreferentialCountryGroupInfo, JobComInvoiceLineValidation.MessageErrorPreferentialCountryGroupNotInList);

			invoiceLine.JI_PreferentialCountryGroup = Enterprise.Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrors("Australia should be in the list, so no errors.", invoiceLine.JI_PreferentialCountryGroupInfo);

			invoiceLine.JI_PreferentialCountryGroup = ZString.Empty;
			AssertNoMessageErrors("No Group is fine.", invoiceLine.JI_PreferentialCountryGroupInfo);
		}

		public void TestCheckJI_PreferentialCountryGroupWithDefaultingOffForCountryWithMultipleGroups_ie_PG()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2009, 11, 21);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.PapuaNewGuinea;
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			invoiceLine.JI_Tariff = "0203.11.00.02C";

			AssertEquals("Precondition: Default value for invoiceLine.JI_PreferentialCountryGroup", ZString.Empty, invoiceLine.JI_PreferentialCountryGroup);

			AssertHasMessageError(invoiceLine.JI_PreferentialCountryGroupInfo, JobComInvoiceLineValidation.MessageErrorPreferentialCountryGroupRequired);

			invoiceLine.JI_PreferentialCountryGroup = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			AssertHasMessageError(invoiceLine.JI_PreferentialCountryGroupInfo, JobComInvoiceLineValidation.MessageErrorPreferentialCountryGroupNotInList);

			invoiceLine.JI_PreferentialCountryGroup = "LDC"; // Should be valid for PG.
			AssertNoMessageErrors("LDC should be valid for Papua New Guinea", invoiceLine.JI_PreferentialCountryGroupInfo);

			invoiceLine.JI_PreferentialCountryGroup = ZString.Empty;
			AssertHasMessageError(invoiceLine.JI_PreferentialCountryGroupInfo, JobComInvoiceLineValidation.MessageErrorPreferentialCountryGroupRequired);

			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			AssertNoMessageErrors("Never Error if Non Qualifying.", invoiceLine.JI_PreferentialCountryGroupInfo);

			invoiceLine.JI_PreferentialCountryGroup = "_O_";
			AssertHasMessageError(invoiceLine.JI_PreferentialCountryGroupInfo, JobComInvoiceLineValidation.MessageErrorPreferentialCountryGroupNotInList);
		}

		public void TestCheckJI_IsZeroRatedDutyForInvoiceLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			string flagMessage = JobComInvoiceLineValidation.MessageErrorMustHaveValidZeroRatedDutyFlag;

			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
			invoiceLine.JI_IsZeroRatedDuty = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
			invoiceLine.JI_IsZeroRatedDuty = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
			invoiceLine.JI_IsZeroRatedDuty = "B";
			AssertHasMessageError(invoiceLine.JI_IsZeroRatedDutyInfo, flagMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
			invoiceLine.JI_IsZeroRatedDuty = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
			invoiceLine.JI_IsZeroRatedDuty = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
			invoiceLine.JI_IsZeroRatedDuty = "B";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
		}

		public void TestCheckJI_IsZeroRatedExciseForInvoiceLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			string flagMessage = JobComInvoiceLineValidation.MessageErrorMustHaveValidZeroRatedExciseFlag;

			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
			invoiceLine.JI_IsZeroRatedExcise = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
			invoiceLine.JI_IsZeroRatedExcise = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
			invoiceLine.JI_IsZeroRatedExcise = "B";
			AssertHasMessageError(invoiceLine.JI_IsZeroRatedExciseInfo, flagMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
			invoiceLine.JI_IsZeroRatedExcise = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
			invoiceLine.JI_IsZeroRatedExcise = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
			invoiceLine.JI_IsZeroRatedExcise = "B";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
		}

		public void TestCheckJI_IsZeroRatedGSTForInvoiceLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			string flagMessage = JobComInvoiceLineValidation.MessageErrorMustHaveValidZeroRatedGSTFlag;

			invoiceLine.JI_IsZeroRatedGST = "";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
			invoiceLine.JI_IsZeroRatedGST = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
			invoiceLine.JI_IsZeroRatedGST = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
			invoiceLine.JI_IsZeroRatedGST = "B";
			AssertHasMessageError(invoiceLine.JI_IsZeroRatedGSTInfo, flagMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
			invoiceLine.JI_IsZeroRatedGST = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
			invoiceLine.JI_IsZeroRatedGST = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
			invoiceLine.JI_IsZeroRatedGST = "B";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
		}

		public void TestCheckJI_IsZeroRatedLeviesForInvoiceLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			string flagMessage = JobComInvoiceLineValidation.MessageErrorMustHaveValidZeroRatedLeviesFlag;

			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);
			invoiceLine.JI_IsZeroRatedLevies = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);
			invoiceLine.JI_IsZeroRatedLevies = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);
			invoiceLine.JI_IsZeroRatedLevies = "B";
			AssertHasMessageError(invoiceLine.JI_IsZeroRatedLeviesInfo, flagMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);
			invoiceLine.JI_IsZeroRatedLevies = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);
			invoiceLine.JI_IsZeroRatedLevies = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);
			invoiceLine.JI_IsZeroRatedLevies = "B";
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);
		}

		public void TestConcessionCodeImportDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZDateTime testDate = new ZDateTime(2006, 10, 23);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = testDate;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			NZCConcession class1 = Factory.New<NZCConcession>();
			class1.U2_Code = "LALALAL";
			class1.U2_DateActiveFrom = new ZDateTime(2006, 10, 26);
			class1.U2_Description = "Gibbiceps";

			NZCConcession class2 = Factory.New<NZCConcession>();
			class2.U2_Code = "NONONO";
			class2.U2_DateActiveFrom = new ZDateTime(2006, 10, 21);
			class2.U2_Description = "CuckooSqueaker";

			NZCConcession class3 = Factory.New<NZCConcession>();
			class3.U2_Code = "HOHOHO";
			class3.U2_DateActiveFrom = new ZDateTime(2006, 10, 21);
			class3.U2_DateActiveTo = new ZDateTime(2006, 10, 22);
			class3.U2_Description = "Test3";

			Factory.Save();

			const string WarningMessageUnknown = "Concession Code [THISIS7] not recognised" + JobComInvoiceLineValidation.ConcessionCodeWarningMessage;
			const string WarningMessageNotActive = "Concession Code [LALALAL] not active yet" + JobComInvoiceLineValidation.ConcessionCodeWarningMessage;
			const string WarningMessageHasExpired = "Concession Code [HOHOHO] has expired" + JobComInvoiceLineValidation.ConcessionCodeWarningMessage;

			invoiceLine.JI_ConcessionCode = class1.U2_Code;
			Assert("Concession Code should have a warning that the Code is not active yet.", invoiceLine.JI_ConcessionCodeInfo.HasWarning(WarningMessageNotActive));
			invoiceLine.JI_ConcessionCode = class2.U2_Code;
			AssertNoWarnings("This is a valid code", invoiceLine.JI_ConcessionCodeInfo);
			invoiceLine.JI_ConcessionCode = "THISIS7";
			Assert(invoiceLine.JI_ConcessionCodeInfo.HasWarning(WarningMessageUnknown));
			invoiceLine.JI_ConcessionCode = ZString.Empty;
			AssertNoWarnings("Empty should not have a warning", invoiceLine.JI_ConcessionCodeInfo);
			invoiceLine.JI_ConcessionCode = class3.U2_Code;
			Assert("Concession Code should have a warning that the Code has expired.", invoiceLine.JI_ConcessionCodeInfo.HasWarning(WarningMessageHasExpired));
		}

		[TestDate(2006, 10, 24)]
		public void TestConcessionCodeExportDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZDateTime testDate = new ZDateTime(2006, 10, 21);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DateOfArrival = testDate;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			NZCConcession class1 = Factory.New<NZCConcession>();
			class1.U2_Code = "OHNOEZ";
			class1.U2_DateActiveFrom = new ZDateTime(2006, 10, 26);
			class1.U2_Description = "Gibbiceps";

			NZCConcession class2 = Factory.New<NZCConcession>();
			class2.U2_Code = "NONONO";
			class2.U2_DateActiveFrom = new ZDateTime(2006, 10, 23);
			class2.U2_Description = "CuckooSqueaker";

			NZCConcession class3 = Factory.New<NZCConcession>();
			class3.U2_Code = "HOHOHO";
			class3.U2_DateActiveFrom = new ZDateTime(2006, 10, 21);
			class3.U2_DateActiveTo = new ZDateTime(2006, 10, 22);
			class3.U2_Description = "Test3";

			Factory.Save();

			const string WarningMessageUnknown = "Concession Code [THISIS7] not recognised" + JobComInvoiceLineValidation.ConcessionCodeWarningMessage;
			const string WarningMessageNotActive = "Concession Code [OHNOEZ] not active yet" + JobComInvoiceLineValidation.ConcessionCodeWarningMessage;
			const string WarningMessageHasExpired = "Concession Code [HOHOHO] has expired" + JobComInvoiceLineValidation.ConcessionCodeWarningMessage;

			invoiceLine.JI_ConcessionCode = class1.U2_Code;
			Assert("Concession Code should have a warning that the Code is not active yet.", invoiceLine.JI_ConcessionCodeInfo.HasWarning(WarningMessageNotActive));
			invoiceLine.JI_ConcessionCode = class2.U2_Code;
			AssertNoWarnings("This is a valid code", invoiceLine.JI_ConcessionCodeInfo);
			invoiceLine.JI_ConcessionCode = "THISIS7";
			Assert(invoiceLine.JI_ConcessionCodeInfo.HasWarning(WarningMessageUnknown));
			invoiceLine.JI_ConcessionCode = ZString.Empty;
			AssertNoWarnings("Empty should not have a warning", invoiceLine.JI_ConcessionCodeInfo);
			invoiceLine.JI_ConcessionCode = class3.U2_Code;
			Assert("Concession Code should have a warning that the Code has expired.", invoiceLine.JI_ConcessionCodeInfo.HasWarning(WarningMessageHasExpired));
		}

		public void TestSupplementaryQuantity()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_SupplementaryUQ = "";
			invoiceLine.JI_SupplementaryQty = 0;
			AssertNoMessageErrors(invoiceLine.JI_SupplementaryQtyInfo);

			invoiceLine.JI_SupplementaryUQ = "LPA";
			invoiceLine.JI_SupplementaryQty = 0;
			AssertHasMessageError(invoiceLine.JI_SupplementaryQtyInfo, JobComInvoiceLineValidation.MessageErrorMustHaveSupplementaryQuantity);

			invoiceLine.JI_SupplementaryUQ = "";
			invoiceLine.JI_SupplementaryQty = 1;
			AssertNoMessageErrors(invoiceLine.JI_SupplementaryQtyInfo);

			invoiceLine.JI_SupplementaryUQ = "LPA";
			invoiceLine.JI_SupplementaryQty = 2;
			AssertNoMessageErrors(invoiceLine.JI_SupplementaryQtyInfo);
		}

		public void TestOriginRegionValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0203.11.00.02C";
			invoiceLine.JI_OriginRegion = "";
			AssertNoMessageErrors("OrginRegion can be blank in this tariff range", invoiceLine.JI_OriginRegionInfo);

			invoiceLine.JI_OriginRegion = "EUR";
			AssertNoMessageErrors("Low range - validation should now pass for this tariff", invoiceLine.JI_OriginRegionInfo);

			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_OriginRegion = "ASIA";
			AssertHasMessageError("Origin Region is not requried for this tariff", invoiceLine.JI_OriginRegionInfo, JobComInvoiceLineValidation.OriginRegionNotRequired);

			//check invalid value does not crash
			invoiceLine.JI_Tariff = "TARIFF";
			invoiceLine.JI_OriginRegion = "AMERICAS";
			AssertHasMessageError("Origin Region is not requried for invalid tariff", invoiceLine.JI_OriginRegionInfo, JobComInvoiceLineValidation.OriginRegionNotRequired);

			invoiceLine.JI_Tariff = "2101.11.00.01C";
			invoiceLine.JI_OriginRegion = "ASIA";
			AssertNoMessageError("In range - Origin Region supplied", invoiceLine.JI_OriginRegionInfo, JobComInvoiceLineValidation.OriginRegionNotRequired);

			invoiceLine.JI_OriginRegion = "";
			AssertNoMessageError("In range - Origin Region left blank", invoiceLine.JI_OriginRegionInfo, JobComInvoiceLineValidation.OriginRegionNotRequired);
		}
		#endregion

		#region Implementation
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}

		#endregion
	}
}
