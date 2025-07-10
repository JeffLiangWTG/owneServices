using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestCusCodeDataDeletionAndAddition()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceCusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)invoice;
			var relatedDocumentString = "RLD";
			var relatedDocumentAirWaybillNumber = "AW";
			Type relatedDocumentType = null;
			invoiceCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(relatedDocumentString, out relatedDocumentType);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCusCodeDataSupporter = (ICusCodeDataTypeSupporter)invoiceLine;
			Type type = null;
			AssertEquals("No support for related document on invoiceline", false, invoiceLineCusCodeDataSupporter.GetCusCodeDataTypes().TryGetValue(relatedDocumentString, out type));
			var relatedDocument = (CusCodeData)Factory.New(relatedDocumentType);
			relatedDocument.CY_Code = relatedDocumentAirWaybillNumber;
			relatedDocument.CY_Data = "AW123";
			relatedDocument.CY_ParentID = invoiceLine.PK;
			relatedDocument.CY_ParentTableCode = invoiceLine.TablePrefix;
			var feeString = "FEE";
			var licenceAndPermitString = "LNP";
			var feeMerchandiseProcessing = "499";
			var licenceAndPermitSteelImportLicense = "01";
			Type feeType = null;
			invoiceLineCusCodeDataSupporter.GetCusCodeDataTypes().TryGetValue(feeString, out feeType);
			var fee = (CusCodeData)Factory.New(feeType);
			fee.CY_Code = feeMerchandiseProcessing;
			fee.CY_Data = "12";
			fee.CY_ParentID = invoiceLine.PK;
			fee.CY_ParentTableCode = invoiceLine.TablePrefix;
			Type licenceAndPermitType = null;
			invoiceLineCusCodeDataSupporter.GetCusCodeDataTypes().TryGetValue(licenceAndPermitString, out licenceAndPermitType);
			var licenceAndPermit = (CusCodeData)Factory.New(licenceAndPermitType);
			licenceAndPermit.CY_Code = licenceAndPermitSteelImportLicense;
			licenceAndPermit.CY_Data = "12";
			licenceAndPermit.CY_ParentID = invoiceLine.PK;
			licenceAndPermit.CY_ParentTableCode = invoiceLine.TablePrefix;
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine()
			{
				CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[]
				{
					SetupCustomsReference2(feeString, feeMerchandiseProcessing),
					new UniversalCustoms.CustomsReference() { Type = new CodeDescriptionPair() { Code = licenceAndPermitString } }
				})
			};
			var reader = new CustomsReferenceCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates));
			var cusCodeDataBOs = reader.ReadIntoDataRows(invoiceLine.PK, invoiceLine.TablePrefix, invoiceLine.IsInDatabase, invoiceLineData);
			AssertNotNull(cusCodeDataBOs);

			CombineAssertions(delegate
			{
				AssertEquals("cusCodeDataBOs", 1, cusCodeDataBOs.Length);
				var feeBO = cusCodeDataBOs[0];
				AssertCusCodeDataContents2(feeBO, invoiceLine.TablePrefix, invoiceLine.PK, feeString, feeMerchandiseProcessing);

				AssertEquals(true, licenceAndPermit.IsDeleted);
				AssertEquals(true, fee.IsDeleted);
				AssertEquals(false, relatedDocument.IsDeleted);
				AssertCusCodeDataContents(relatedDocument, invoiceLine.TablePrefix, invoiceLine.PK, relatedDocumentString, relatedDocumentAirWaybillNumber, "AW123", ZBool.False, ZShort.Zero);

				var invoiceLineCusCodeDataBOs = LoadCusCodeData(invoiceLine.TablePrefix, invoiceLine.PK);
				AssertEquals("invoiceLineCusCodeDataBOs.Length", 2, invoiceLineCusCodeDataBOs.Length);
				AssertEquals("relatedDocument matched", ((IBusinessObjectInternals)relatedDocument).Row, invoiceLineCusCodeDataBOs.FirstOrDefault(x => x.GetValue(CusCodeDataSchema.PK) == relatedDocument.PK));
				AssertEquals("feeBO matched", feeBO, invoiceLineCusCodeDataBOs.FirstOrDefault(x => x.GetValue(CusCodeDataSchema.PK) == feeBO.GetValue(CusCodeDataSchema.PK)));
			});
		}

		public void TestCusCodeDataFieldMappingsForUS()
		{
			var provider = Factory.BOFactory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.UnitedStates);
			var list = provider.TableSpecificCusCodeDataTypeList(JobDeclarationSchema.Constants.Prefix, "");
			Assert("TableSpecificCusCodeDataTypeList for US Declaration should exist", list.Count > 0);
			var type = "CNN";
			AssertEquals("CNN should be a valid code for US Declaration TableSpecificCusCodeDataTypeList; if not then please update the test with a valid code", true, list.ContainsCode(type));
			var unknownType = "S!D";
			AssertEquals(false, list.ContainsCode(unknownType));
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var customsReference1 = SetupCustomsReference(new CodeDescriptionPair() { Code = unknownType }, null, (ZString)"GDF", ZBool.False, ZShort.Zero);
			var customsReference2 = SetupCustomsReference(new CodeDescriptionPair() { Code = type }, null, (ZString)"ABC", ZBool.False, ZShort.Zero);
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetCustomsReferenceCollection(() => new List<UniversalCustoms.CustomsReference>(new[] { customsReference1, customsReference2 }));
			var reader = new CustomsReferenceCollectionDataObjectReader(logger, helper);
			var cusCodeDataBOs = reader.ReadIntoDataRows(declaration.PK, JobDeclarationSchema.Constants.Prefix, declaration.IsInDatabase, shipmentData);

			AssertNotNull(cusCodeDataBOs);

			CombineAssertions(delegate
			{
				AssertEquals("Child CusCusCodeData", 1, cusCodeDataBOs.Length);
				var cusCodeDataBO = cusCodeDataBOs[0];
				AssertCusCodeDataContents(cusCodeDataBO, JobDeclarationSchema.Constants.Prefix, declaration.PK, type, type, "ABC", ZBool.False, ZShort.Zero);
			});
		}

		public void TestParentTableNotSupportedWarning_Unsupported()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetCustomsReferenceCollection(() => new List<UniversalCustoms.CustomsReference>(new[] { new UniversalCustoms.CustomsReference() }));
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany);
			new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(entryHeader.PK, entryHeader.TablePrefix, entryHeader.IsInDatabase, shipmentData);
			AssertContains("Unsupported Type 'CH'", "Customs Reference was not processed as there is no support for Table with code 'CH'.", logger.GetWarnings());
		}

		public void TestParentTableNotSupportedWarning_SupportedForCusReference()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var universalEntryInstructionData = new UniversalCustoms.EntryInstruction()
			{
				CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>() { new UniversalCustoms.CustomsReference() }
			};
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany);
			new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(entryInstruction.PK, entryInstruction.TablePrefix, entryInstruction.IsInDatabase, universalEntryInstructionData);
			AssertEquals(false, logger.HasWarnings);
		}

		public void TestParentTableNotSupportedWarning_SupportedForCusCodeData()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetCustomsReferenceCollection(() => new List<UniversalCustoms.CustomsReference>(new[] { new UniversalCustoms.CustomsReference() }));
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany);
			new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(jobDeclaration.PK, jobDeclaration.TablePrefix, jobDeclaration.IsInDatabase, shipmentData);
			AssertEquals(false, logger.HasWarnings);
		}
	}
}
