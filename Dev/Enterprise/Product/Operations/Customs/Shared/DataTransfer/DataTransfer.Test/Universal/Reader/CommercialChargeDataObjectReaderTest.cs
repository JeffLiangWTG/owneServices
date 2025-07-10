using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestImportChargsWithTheSameCode()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var declarationDataObject = SetupDeclaration("OW3234", "HB32342", new WayBillType() { Code = WayBillTypeList.Codes.House });
			var chargeDataObject1 = SetupCommercialCharge(ZBool.False);
			var chargeDataObject2 = SetupCommercialCharge(ZBool.False);
			var invoiceDataObject = SetupCommercialInvoiceHeaderData(commercialInvoiceChargeCollection: new List<CommercialCharge>(new[] { chargeDataObject1, chargeDataObject2 }));
			invoiceDataObject.IncoTerm = new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.CostInsuranceAndFreight };
			declarationDataObject.CommercialInfo = SetupCommercialInfo("GROUP", invoiceCollection: new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceDataObject }));

			var message = (EDIMessage)GetQueuedUniversalShipmentMessage(declarationDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
			var invoice = declaration.Invoices[0];
			AssertEquals(Core.Constants.IncoTerms.CostInsuranceAndFreight, invoice.JZ_IncoTerm);
			AssertEquals("invoice.Charges.Count", 3, invoice.Charges.Count);
			var oftCharges = invoice.Charges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).ToList();
			AssertEquals("Charge from Universal XML, override the one defaulted from INCOTerm", 100.60m, oftCharges[0].J7_Amount);
			AssertEquals("Charge from Universal XML, override the one defaulted from INCOTerm", 100.60m, oftCharges[1].J7_Amount);
			var onsCharge = invoice.Charges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasInsurance).FirstOrDefault();
			AssertNotNull("Default from IncoTerm", onsCharge);
			AssertEquals(0m, onsCharge.J7_Amount);
		}

		public void TestApportionedChargesAreNotImported()
		{
			var declarationDataObject = SetupDeclaration("OW3234", "HB32342", new WayBillType() { Code = WayBillTypeList.Codes.House });
			var masterBillDataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB23443", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
			var houseBillDataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB32342", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB23443" };
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, houseBillDataObject }));
			var apportionedChargeDataObject = SetupCommercialCharge(ZBool.True);
			apportionedChargeDataObject.Amount = 150m;
			var chargeDataObject = SetupCommercialCharge(ZBool.False);
			var invoiceDataObject = SetupCommercialInvoiceHeaderData(commercialInvoiceChargeCollection: new List<CommercialCharge>(new[] { apportionedChargeDataObject, chargeDataObject }));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("GROUP", invoiceCollection: new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceDataObject }));

			var message = (EDIMessage)GetQueuedUniversalShipmentMessage(declarationDataObject);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration (Master Bill='MB23443' House Bill='HB32342') from UniversalShipment.
Successfully saved Declaration B00001000 with 2 x Bill, 1 x BaseInvoiceCharge.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Populating BaseJobDeclaration...
No matching Bill found, creating new Bill.
Populating Bill...
No matching Bill found, creating new Bill.
Populating Bill...
Warning - Matching 'Supplier':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Warning - Matching 'Importer':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
No matching BaseInvoiceCharge found, creating new BaseInvoiceCharge.
Populating BaseInvoiceCharge...
Added Declaration (Master Bill='MB23443' House Bill='HB32342') from UniversalShipment.
Successfully saved Declaration B00001000 with 2 x Bill, 1 x BaseInvoiceCharge.
".Trim(), logNoteText);
				var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
				AssertEquals("declaration.Invoices.Count", 1, declaration.Invoices.Count);
				var invoice = declaration.Invoices[0];
				AssertEquals("invoice.Charges.Count", 1, invoice.Charges.Count);
				AssertContents(invoice.Charges[0], ZBool.False, 100.60m, Common.CustomsChargeTypeList.Codes.OverseasFreight, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Weight, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.5060m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 10m, Core.Constants.PaymentType.Prepaid);
			});
		}

		public void TestBasicCommercialChargeLevelFieldMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeDataObject = SetupCommercialCharge(ZBool.False, ZBool.True);
			var reader = new CommercialChargeDataObjectReader<BaseInvoiceLineCharge>(chargeDataObject, logger, invoiceLine, Factory);
			var chargeBO = reader.ReadIntoBusinessObject();

			AssertNotNull(chargeBO);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				AssertCollectionContains("chargeBO should be part of invoiceLine.Charges", chargeBO, invoiceLine.Charges);
				AssertContents(chargeBO, ZBool.True);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseInvoiceLineCharge found, creating new BaseInvoiceLineCharge.
Information - Populating BaseInvoiceLineCharge...
".Trim(), logger.Logs);
			});

			#endregion
		}
	}
}
