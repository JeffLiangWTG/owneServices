using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestTaxOrFeeDeletionAndAddition()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.GB.IJobDeclaration>();
				declaration.JE_MasterBill = "MYMASTER";
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var originalTaxBizO = Factory.New<JobComInvoiceLineTax>();
				originalTaxBizO.JLT_JI = invoiceLine.PK;
				originalTaxBizO.JLT_Type = "A30";

				var taxOrFeeDataObject = JobDeclarationDataObjectReaderTest.CreateTaxOrFee("A30", "G");
				var universalInvoiceLineData = new UniversalCustoms.CommercialInvoiceLine();
				universalInvoiceLineData.TaxOrFeeCollection = new List<UniversalCustoms.TaxOrFee>() { taxOrFeeDataObject };
				var reader = new TaxOrFeeCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom));
				var jlts = reader.ReadIntoDataRows(invoiceLine.PK, invoiceLine.TablePrefix, invoiceLine.IsInDatabase, universalInvoiceLineData);

				AssertNotNull(jlts);

				#region Check Contents of Business Object

				CombineAssertions(delegate
				{
					AssertEquals(true, originalTaxBizO.IsDeleted);
					AssertEquals(1, jlts.Length);
					var reloadedJlt = jlts[0];
					AssertEquals("A30", reloadedJlt.GetValue(JobComInvoiceLineTaxSchema.JLT_Type));
					AssertEquals("G", reloadedJlt.GetValue(JobComInvoiceLineTaxSchema.JLT_MethodOfPayment));
					AssertEquals(123.45m, reloadedJlt.GetValue(JobComInvoiceLineTaxSchema.JLT_Amount));
					AssertEquals(1234.56m, reloadedJlt.GetValue(JobComInvoiceLineTaxSchema.JLT_BaseValue));
					AssertEquals(456m, reloadedJlt.GetValue(JobComInvoiceLineTaxSchema.JLT_BaseQuantity));
					AssertEquals("ABCD", reloadedJlt.GetValue(JobComInvoiceLineTaxSchema.JLT_MethodOfCalculation));
					AssertEquals("X", reloadedJlt.GetValue(JobComInvoiceLineTaxSchema.JLT_RateOverrideReasonCode));
				});

				#endregion
			}
		}

		public void TestTaxOrFeeReadFromOldAddInfoSchemaIfRegistrySaysSo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.CommercialInfo = new UniversalCustoms.CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>() { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) } };
				var fakeTaxAddINfoGroup = new UniversalCustoms.AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = "GTX" },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key =  "Type", Value = "A00" },
						new AddInfo() { Key =  "MethodOfPayment", Value = "F" },
						new AddInfo() { Key =  "Amount", Value = "123.45" },
					}
				};
				var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine() { AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>() { fakeTaxAddINfoGroup } };
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>() { invoiceLineData });

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				manager.Process(message);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				var declarationBO = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "MYMASTER"));
				var createdInvoiceLineBO = declarationBO.Invoices[0].InvoiceLines[0];
				AssertEquals(0, Factory.Load<JobComInvoiceLineTax>(new ZQuery(JobComInvoiceLineTaxSchema.JLT_JI, createdInvoiceLineBO.PK)).Length);
				declarationBO.Delete();
				Factory.SaveForTesting();

				using (ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					message = GetQueuedUniversalShipmentMessage(declarationDataObject);
					manager.Process(message);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					declarationBO = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "MYMASTER"));
					createdInvoiceLineBO = declarationBO.Invoices[0].InvoiceLines[0];
					var taxBosSaved = Factory.Load<JobComInvoiceLineTax>(new ZQuery(JobComInvoiceLineTaxSchema.JLT_JI, createdInvoiceLineBO.PK));
					AssertEquals(1, taxBosSaved.Length);
					AssertEquals("A00", taxBosSaved[0].JLT_Type);
					AssertEquals("F", taxBosSaved[0].JLT_MethodOfPayment);
					AssertEquals(123.45m, taxBosSaved[0].JLT_Amount);
				}
			}
		}

		public void TestTaxOrFeeWriteToOldAddInfoSchemaIfRegistrySaysSo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var declarationBO = Factory.New<BaseJobDeclaration>();
				var invoice = declarationBO.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var tax = Factory.New<JobComInvoiceLineTax>();
				tax.JLT_JI = invoiceLine.PK;
				tax.JLT_Type = "A12";
				tax.JLT_Amount = 123.45m;
				tax.JLT_MethodOfPayment = "G";
				var declarationDataObject = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declarationBO);
				var taxOrFees = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].TaxOrFeeCollection;
				AssertEquals(1, taxOrFees.Count);
				AssertEquals("A12", taxOrFees[0].Type.Code);
				var addInfoForTax = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoGroupCollection;
				AssertEquals(null, addInfoForTax);

				using (ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					declarationDataObject = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declarationBO);
					taxOrFees = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].TaxOrFeeCollection;
					AssertEquals(1, taxOrFees.Count);
					AssertEquals("A12", taxOrFees[0].Type.Code);
					var addInfoGroupForTax = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoGroupCollection.First(c => c.Type.GetCodeAsUpperCase() == "GTX");
					AssertEquals("A12", addInfoGroupForTax.AddInfoCollection.First(c => c.Key.GetValueOrDefault() == "Type").Value);
				}
			}
		}

		public void TestTaxOrFeeWriteForNonEuCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			using (ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationBO = Factory.New<BaseJobDeclaration>();
				var invoice = declarationBO.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var tax = Factory.New<JobComInvoiceLineTax>();  // An Aussie JI won't have any taxes, but let's add some anyway to prove that they're not written for Aussie decs. 
				tax.JLT_JI = invoiceLine.PK;
				tax.JLT_Type = "A12";
				tax.JLT_Amount = 123.45m;
				tax.JLT_MethodOfPayment = "G";

				var declarationDataObject = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declarationBO);
				var taxOrFees = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].TaxOrFeeCollection;
				AssertEquals("Even though the Aussie dec's JI has some taxes in the DB, they're not written to TaxOrFee", null, taxOrFees);
				var addInfoForTax = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoGroupCollection;
				AssertEquals("Even though the Aussie dec's JI has some taxes in the DB and the rego is true, they're not written to AddInfo", null, addInfoForTax);
			}
		}

		public void TestTaxOrFeeReadForNonEuCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.CommercialInfo = new UniversalCustoms.CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>() { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) } };
				var fakeTaxAddINfoGroup = new UniversalCustoms.AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = "GTX" },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key =  "Tty", Value = "A00" },
						new AddInfo() { Key =  "MethodOfPayment", Value = "F" },
						new AddInfo() { Key =  "Amount", Value = "123.45" },
					}
				};
				var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine() { AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>() { fakeTaxAddINfoGroup } };
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>() { invoiceLineData });

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				manager.Process(message);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				var declarationBO = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "MYMASTER"));
				var createdInvoiceLineBO = declarationBO.Invoices[0].InvoiceLines[0];
				AssertEquals(0, Factory.Load<JobComInvoiceLineTax>(new ZQuery(JobComInvoiceLineTaxSchema.JLT_JI, createdInvoiceLineBO.PK)).Length);
				declarationBO.Delete();
				Factory.SaveForTesting();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.CommercialInfo = new UniversalCustoms.CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>() { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) } };
				var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine() { TaxOrFeeCollection = new List<UniversalCustoms.TaxOrFee>() { new UniversalCustoms.TaxOrFee { Type = new CodeDescriptionPair6Char() { Code = "DJC" }, Amount = 123m } } };
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0].SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>() { invoiceLineData });
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				manager.Process(message);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				var declarationBO = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "MYMASTER"));
				var createdInvoiceLineBO = declarationBO.Invoices[0].InvoiceLines[0];
				AssertEquals(0, Factory.Load<JobComInvoiceLineTax>(new ZQuery(JobComInvoiceLineTaxSchema.JLT_JI, createdInvoiceLineBO.PK)).Length);
			}
		}
	}
}
