using System;
using System.Collections;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public abstract class DeclarationValueObjectDataAdapterAbstractTest : ValueObjectDataAdapterTest<BaseJobDeclaration, Xsd.ConsolAndShipment>
	{
		public void TestFindBusinessObjects()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "HouseBillToFind";

			var duplicateDeclaration = Factory.New<BaseJobDeclaration>();
			duplicateDeclaration.JE_MasterBill = "MasterBillToFind";
			duplicateDeclaration.JE_HouseBill = "HouseBillToFind";

			var cancelledDeclaration = Factory.New<BaseJobDeclaration>();
			cancelledDeclaration.JE_MasterBill = "MasterBillToFind";
			cancelledDeclaration.JE_HouseBill = "HouseBillToFind";
			cancelledDeclaration.JE_IsCancelled = true;

			var declarationValue = new Xsd.ConsolAndShipment();
			var masterBill = declarationValue.Consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MasterBillToFind";
			var houseBill = declarationValue.Shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HouseBillToFind";
			var houseBill2 = declarationValue.Shipment.ShipmentIdentifier.AddNew();
			houseBill2.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill2.Value = "HouseBillToFind2";

			var context = new ValueObjectImportContext(Factory, Notify);

			var foundDeclarations = adapter.FindJobDeclarations(declarationValue, context);
			AssertEquals("There should be no errors", false, Notify.HasErrors);
			AssertEquals("Both declarations should be found", 2, foundDeclarations.Length);
			AssertEquals("Both declarations should be found", true, ((IList)foundDeclarations).Contains(declaration));
			AssertEquals("Both declarations should be found", true, ((IList)foundDeclarations).Contains(duplicateDeclaration));
		}

		public virtual void TestFindBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var declarationValue = new Xsd.ConsolAndShipment();

			var masterBillValue = declarationValue.Consol.ConsolIdentifier.AddNew();
			masterBillValue.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			var houseBillValue = declarationValue.Shipment.ShipmentIdentifier.AddNew();
			houseBillValue.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			masterBillValue.Value = "MasterBillToFind";
			houseBillValue.Value = "HouseBillToFind";
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "HouseBillToFind";
			Factory.Save();
			AssertBusinessObjectFound("Should find the correct declaration by master bill and house bill", declarationValue, declaration, true);

			masterBillValue.Value = "MasterBillToFind";
			houseBillValue.Value = "";
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "";
			Factory.Save();
			AssertBusinessObjectFound("Should find the correct declaration by master bill and blank house bill", declarationValue, declaration, true);

			masterBillValue.Value = "MasterBillToFind";
			houseBillValue.Value = "";
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "x";
			Factory.Save();
			AssertBusinessObjectFound("Should not find a match as the house bill criteria is blank while the dec has a house bill", declarationValue, declaration, false);

			masterBillValue.Value = "MasterBillToFind";
			houseBillValue.Value = "x";
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "";
			Factory.Save();
			AssertBusinessObjectFound("Should not find a match as the match candidate has a blank house bill and the criteria doesn't", declarationValue, declaration, false);

			masterBillValue.Value = "MasterBillToFind";
			houseBillValue.Value = "x";
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "y";
			Factory.Save();
			AssertBusinessObjectFound("Should not find a match as the house bill is different", declarationValue, declaration, false);

			masterBillValue.Value = "x";
			houseBillValue.Value = "HouseBillToFind";
			declaration.JE_MasterBill = "y";
			declaration.JE_HouseBill = "HouseBillToFind";
			Factory.Save();
			AssertBusinessObjectFound("Should not find a match as the master bill is different", declarationValue, declaration, false);

			masterBillValue.Value = "";
			houseBillValue.Value = "";
			declaration.JE_MasterBill = "";
			declaration.JE_HouseBill = "";
			Factory.Save();
			AssertBusinessObjectFound("Should not match on blank master and house bills", declarationValue, declaration, false);
		}

		public void TestFindBusinessObject_WithNoMasterOrHouseBillCriteria()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var declarationValue = new Xsd.ConsolAndShipment();

			declaration.JE_MasterBill = "";
			declaration.JE_HouseBill = "";
			Factory.Save();
			AssertBusinessObjectFound("Should not match when there are no master or house bills", declarationValue, declaration, false);
		}

		public void TestFindBusinessObject_WithMultipleHouseMasterBillsOnOneDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MasterBillToFind";

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MasterBillToFind2";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_MasterBill = "MasterBillToFind2";
			houseBill2.CU_HouseBill = "HouseBillToFind2";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "MasterBillToFind";
			houseBill.CU_HouseBill = "HouseBillToFind";

			var declarationValue = new Xsd.ConsolAndShipment();
			var masterBillValue = declarationValue.Consol.ConsolIdentifier.AddNew();
			masterBillValue.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBillValue.Value = "MasterBillToFind";
			var houseBillValue = declarationValue.Shipment.ShipmentIdentifier.AddNew();
			houseBillValue.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBillValue.Value = "HouseBillToFind";

			Factory.Save();

			AssertBusinessObjectFound("Should find a match even on a declaration with multiple master/house bills", declarationValue, declaration, true);
		}

		public void TestFindBusinessObject_WithDuplicateHouseMasterBill()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "HouseBillToFind";

			var duplicateDeclaration = Factory.New<BaseJobDeclaration>();
			duplicateDeclaration.JE_MasterBill = "MasterBillToFind";
			duplicateDeclaration.JE_HouseBill = "HouseBillToFind";

			var declarationValue = new Xsd.ConsolAndShipment();
			var masterBill = declarationValue.Consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MasterBillToFind";
			var houseBill = declarationValue.Shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HouseBillToFind";
			var houseBill2 = declarationValue.Shipment.ShipmentIdentifier.AddNew();
			houseBill2.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill2.Value = "HouseBillToFind2";

			Factory.Save();

			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var foundDeclaration = adapter.FindBusinessObject(declarationValue, context);
			AssertEquals("There should be errors resulting from importing when there are duplicate matches", true, notify.HasErrors);
			Assert("Should not be saved with duplicate declarations", notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));
		}

		public void TestAllowBillingImportIntoDeclaration()
		{
			var xmlDec = new Xsd.ConsolAndShipment();
			xmlDec.Shipment.Billing = new Xsd.Billing();
			xmlDec.Shipment.Billing.ChargeLines.AddNew().ChargeCode = "FRT";

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var jobHeader = new JobHeader.Loader(declaration).TryCreate();

			CustomsDataRegistry.Instance.AllowBillingImportIntoDeclaration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(declaration, xmlDec, context);
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));

			AssertEquals("Should not import billing charges", 0, charges.Length);

			CustomsDataRegistry.Instance.AllowBillingImportIntoDeclaration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			adapter.ImportFromValueObject(declaration, xmlDec, context);
			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));

			AssertEquals("Should import billing charges", 1, charges.Length);
			AssertEquals("Should import billing charges", "FRT", charges[0].ChargeCode.AC_Code);
		}

		public void TestImportingPaymentMethod()
		{
			var xmlDec = new Xsd.ConsolAndShipment();
			xmlDec.Shipment.Declaration.IsSpecified = true;
			xmlDec.Shipment.Declaration.PaymentTerms = "B!D";
			xmlDec.Shipment.Declaration.PaymentTermsSpecified = true;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var declaration = Factory.New<BaseJobDeclaration>();
			adapter.ImportFromValueObject(declaration, xmlDec, context);
			if (declaration.SupportJE_PaymentMethodUsage)
			{
				AssertEquals("B!D", declaration.JE_PaymentMethod);
			}
			else
			{
				AssertNotEquals("B!D", declaration.JE_PaymentMethod);
			}
		}

		public void TestImportingSupplierImporterDefaultsLocalPartyInformation()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.Country.RN_Code;
			consignee.MiscServ.OM_RS_NKIMDefaultServiceLevel = "YYY";
			consignee.MiscServ.OM_IMDefaultINCOTerm = "321";
			consignee.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "ZZZ";
			Factory.Save();

			var xmlDec = new Xsd.ConsolAndShipment();
			xmlDec.Shipment.ShipmentDetails.ShipmentType = Xsd.ShipmentType.IMP;
			xmlDec.Shipment.ShipmentDetails.Consignee.EDICode = consignee.OH_Code;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RS_NKServiceLevel = "STD";
			adapter.ImportFromValueObject(declaration, xmlDec, context);

			AssertEquals("321", declaration.JE_ShipmentIncoTerm);
			AssertEquals("YYY", declaration.JE_RS_NKServiceLevel);
			AssertEquals("ZZZ", declaration.JE_MergeBy);
		}

		public void TestAddReferenceIfNessesary()
		{
			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			AssertEquals(jobDec.CustomsEntryHeaders.Count, 0);

			var toUpdateHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			toUpdateHeader.CH_BGMReference = "CHBGM!";
			AssertEquals(jobDeclaration.CustomsEntryHeaders.Count, 1);
			var result = adapter.AddReferenceIfNessesary(jobDeclaration, "CHBGM!");
			AssertEquals(jobDeclaration.CustomsEntryHeaders.Count, 1);
			AssertEquals(toUpdateHeader.PK, result.PK);
			result = adapter.AddReferenceIfNessesary(jobDeclaration, "newCHBGM");
			AssertEquals(jobDeclaration.CustomsEntryHeaders.Count, 2);
		}

		public void TestImportEntryReferencesWithoutCustomsInfo()
		{
			var xsdShipment = new Xsd.Shipment();
			var xsdInvHeader = xsdShipment.Invoices.AddNew();
			xsdInvHeader.InvoiceNumber = "INVNO1";
			var xsdInvLine = xsdInvHeader.InvoiceLines.AddNew();
			xsdInvLine.EntryReference = "entryref1";
			xsdInvLine = xsdInvHeader.InvoiceLines.AddNew();
			xsdInvLine.EntryReference = "entryref2";
			xsdInvHeader = xsdShipment.Invoices.AddNew();
			xsdInvHeader.InvoiceNumber = "INVNO2";
			xsdInvLine = xsdInvHeader.InvoiceLines.AddNew();
			xsdInvLine.EntryReference = "entryref3";
			xsdInvLine = xsdInvHeader.InvoiceLines.AddNew();
			xsdInvLine.EntryReference = "entryref3";

			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			AssertEquals(jobDeclaration.CustomsEntryHeaders.Count, 0);
			adapter.ImportEntryReferences(xsdShipment, jobDeclaration, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(jobDeclaration.CustomsEntryHeaders.Count, 3);
			AssertEquals(jobDeclaration.CustomsEntryHeaders[0].CH_BGMReference, "entryref1");
			AssertEquals(jobDeclaration.CustomsEntryHeaders[1].CH_BGMReference, "entryref2");
			AssertEquals(jobDeclaration.CustomsEntryHeaders[2].CH_BGMReference, "entryref3");
		}

		public void TestGetInvoiceNumbers()
		{
			var invoceCollection = new Xsd.InvoiceHeaderCollection();
			var header = invoceCollection.AddNew();
			header.InvoiceNumber = "InvHeadNo";
			var line = header.InvoiceLines.AddNew();

			line.InvoiceLineNumber = "3";
			line.EntryReference = "EntryRef";
			line.EntryLineNumber = "1";

			var entryLineNumber = "2";
			var entryReference = "";

			var headerNo = ZString.Empty;
			var lineNo = ZShort.Zero;
			adapter.GetInvoiceNumbers(invoceCollection, entryLineNumber, entryReference, out headerNo, out lineNo);
			AssertEquals(headerNo, ZString.Empty);
			AssertEquals(lineNo, ZShort.Zero);

			entryReference = "EntryRef";
			entryLineNumber = "1";

			adapter.GetInvoiceNumbers(invoceCollection, entryLineNumber, entryReference, out headerNo, out lineNo);
			AssertEquals(headerNo, "InvHeadNo");
			AssertEquals(lineNo, (ZShort)3);
		}

		public void TestImportEntryReferences()
		{
			var xsdShipment = new Xsd.Shipment();
			var jobDec = Factory.New<BaseJobDeclaration>();

			#region BusinessObjectToUpdate

			CusEntryHeader cusEntryHeader = jobDec.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "entryRef2";

			var invheader = jobDec.Invoices.AddNew();
			invheader.JZ_InvoiceNumber = "INVHEADERN";
			var invLine1 = invheader.JobComInvoiceLines.AddNew();
			invLine1.JI_LineNo = 1;
			var invLine2 = invheader.JobComInvoiceLines.AddNew();
			invLine2.JI_LineNo = 2;

			var invheader2 = jobDec.Invoices.AddNew();
			invheader2.JZ_InvoiceNumber = "INVHEADER2";
			var invLine3 = invheader2.JobComInvoiceLines.AddNew();
			invLine3.JI_LineNo = 1;
			#endregion

			#region Xsd

			var xsdInvHeader = xsdShipment.Invoices.AddNew();
			xsdInvHeader.InvoiceNumber = "INVHEADERN";
			var xsdInvLine1 = xsdInvHeader.InvoiceLines.AddNew();
			xsdInvLine1.InvoiceLineNumber = "1";
			xsdInvLine1.EntryReference = "entryRef";
			xsdInvLine1.EntryLineNumber = "1";
			var xsdInvLine2 = xsdInvHeader.InvoiceLines.AddNew();
			xsdInvLine2.InvoiceLineNumber = "2";
			xsdInvLine2.EntryReference = "entryRef";
			xsdInvLine2.EntryLineNumber = "2";
			var xsdInvHeader2 = xsdShipment.Invoices.AddNew();
			xsdInvHeader2.InvoiceNumber = "INVHEADER2";
			var xsdInvLine3 = xsdInvHeader2.InvoiceLines.AddNew();
			xsdInvLine3.InvoiceLineNumber = "1";
			xsdInvLine3.EntryReference = "entryRef2";
			xsdInvLine3.EntryLineNumber = "1";

			Xsd.EntryHeader xsdHeader = xsdShipment.Declaration.EntryHeader.AddNew();
			xsdHeader.EntryReference = "entryRef";
			Xsd.EntryLine xsdLine1 = xsdHeader.EntryLines.AddNew();
			xsdLine1.EntryLineNumber = "1";
			var charge1 = xsdLine1.EntryLineCharges.AddNew();
			charge1.ChargeType = "01";
			charge1.ChargeValue.Value = 1.1;
			charge1.ChargeValue.CurrencyCode = "USD";
			var charge2 = xsdLine1.EntryLineCharges.AddNew();
			charge2.ChargeType = "02";
			charge2.ChargeValue.Value = 2.2;
			charge2.ChargeValue.CurrencyCode = "USD";

			Xsd.EntryLine xsdLine2 = xsdHeader.EntryLines.AddNew();
			xsdLine2.EntryLineNumber = "2";
			var charge3 = xsdLine2.EntryLineCharges.AddNew();
			charge3.ChargeType = "03";
			charge3.ChargeValue.Value = 3.3;
			charge3.ChargeValue.CurrencyCode = "AUD";

			Xsd.EntryHeader xsdHeader2 = xsdShipment.Declaration.EntryHeader.AddNew();
			xsdHeader2.EntryReference = "entryRef2";
			var entryLine3 = xsdHeader2.EntryLines.AddNew();
			entryLine3.EntryLineNumber = "1";
			var charge4 = entryLine3.EntryLineCharges.AddNew();
			charge4.ChargeType = "04";
			charge4.ChargeValue.Value = 4.4;
			charge4.ChargeValue.CurrencyCode = "AUD";

			#endregion

			AssertEquals(jobDec.CustomsEntryHeaders.Count, 1);
			AssertEquals("Originally only 1 invHeader", jobDec.Invoices.Count, 2);
			AssertEquals("2 Lines in Header", invheader.JobComInvoiceLines.Count, 2);
			adapter.ImportEntryReferences(xsdShipment, jobDec, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals(jobDec.CustomsEntryHeaders.Count, 2);
			AssertEquals("Should add new InvoiceHeader", jobDec.Invoices.Count, 2);

			AssertEquals("Same number of InvLines for 1st header", jobDec.Invoices[0].JobComInvoiceLines.Count, 2);
			AssertEquals("1 line for second header", jobDec.Invoices[1].JobComInvoiceLines.Count, 1);

			AssertEquals("Number of fees", jobDec.Invoices[0].JobComInvoiceLines[0].CusEntryLine.Fees.Count, 2);
			AssertEquals(jobDec.Invoices[0].JobComInvoiceLines[0].CusEntryLine.Fees[0].CF_ChargeType, "01");
			AssertEquals(jobDec.Invoices[0].JobComInvoiceLines[0].CusEntryLine.Fees[0].CF_ChargeAmount, (ZDecimal)1.1);
			AssertEquals(jobDec.Invoices[0].JobComInvoiceLines[0].CusEntryLine.Fees[1].CF_ChargeType, "02");
			AssertEquals(jobDec.Invoices[0].JobComInvoiceLines[0].CusEntryLine.Fees[1].CF_ChargeAmount, (ZDecimal)2.2);

			AssertEquals("Number of fees", jobDec.Invoices[0].JobComInvoiceLines[1].CusEntryLine.Fees.Count, 1);
			AssertEquals(jobDec.Invoices[0].JobComInvoiceLines[1].CusEntryLine.Fees[0].CF_ChargeType, "03");
			AssertEquals(jobDec.Invoices[0].JobComInvoiceLines[1].CusEntryLine.Fees[0].CF_ChargeAmount, (ZDecimal)3.3);

			AssertEquals("Number of fees", jobDec.Invoices[1].JobComInvoiceLines[0].CusEntryLine.Fees.Count, 1);
			AssertEquals(jobDec.Invoices[1].JobComInvoiceLines[0].CusEntryLine.Fees[0].CF_ChargeType, "04");
			AssertEquals(jobDec.Invoices[1].JobComInvoiceLines[0].CusEntryLine.Fees[0].CF_ChargeAmount, (ZDecimal)4.4);
		}

		public void TestImportDocAddresses()
		{
			var notify = new NotificationBuffer();

			var consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Consol = new Xsd.Consol();
			consolAndShipment.Consol.ConsolDetail.AgentReference = "AgentReference";

			consolAndShipment.Shipment = new Xsd.Shipment();
			consolAndShipment.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			consolAndShipment.Shipment.ShipmentIdentifier.AddNew().Value = "BLAH";
			consolAndShipment.Shipment.ShipmentDetails.Consignor.EDICode = "CONNSINGOR1";
			consolAndShipment.Shipment.ShipmentDetails.Consignor.OrganisationDetails.Name = "CONNSIGNOR ORG";
			consolAndShipment.Shipment.ShipmentDetails.Consignee.EDICode = "CONNSINGEE1";
			consolAndShipment.Shipment.ShipmentDetails.Consignee.OrganisationDetails.Name = "CONNSIGNEE ORG";
			consolAndShipment.Shipment.Declaration = new Xsd.Declaration();

			var docAddress1 = consolAndShipment.Shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew();
			docAddress1.AddressType = Xsd.DocAddressAddressType.IMD;
			docAddress1.AddressLine1 = "line 1";
			docAddress1.AddressLine2 = "line 2";
			docAddress1.CityOrSuburb = "CITY";

			var docAddress2 = consolAndShipment.Shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew();
			docAddress2.AddressType = Xsd.DocAddressAddressType.CRD;
			docAddress2.AddressLine1 = "line 1";
			docAddress2.AddressLine2 = "line 2";
			docAddress2.CityOrSuburb = "CITY";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(declaration, consolAndShipment, context);

			AssertNotNull(declaration.JE_OH_Importer);
			AssertNotNull(declaration.ImporterDocumentaryAddress);
			AssertEquals("line 1", declaration.ImporterDocumentaryAddress.E2_Address1);
			AssertEquals("line 2", declaration.ImporterDocumentaryAddress.E2_Address2);
			AssertEquals("CITY", declaration.ImporterDocumentaryAddress.E2_City);

			AssertNotNull(declaration.SupplierDocumentaryAddress);
			AssertEquals("line 1", declaration.SupplierDocumentaryAddress.E2_Address1);
			AssertEquals("line 2", declaration.SupplierDocumentaryAddress.E2_Address2);
			AssertEquals("CITY", declaration.SupplierDocumentaryAddress.E2_City);
		}

		public void TestImportDocAddresses_IMDandSUDAddressNotSpecified()
		{
			var notify = new NotificationBuffer();

			var consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Consol = new Xsd.Consol();
			consolAndShipment.Consol.ConsolDetail.AgentReference = "AgentReference";

			consolAndShipment.Shipment = new Xsd.Shipment();
			consolAndShipment.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			consolAndShipment.Shipment.ShipmentIdentifier.AddNew().Value = "BLAH";
			consolAndShipment.Shipment.ShipmentDetails.Consignor.EDICode = "CONNSINGOR1";
			consolAndShipment.Shipment.ShipmentDetails.Consignor.OrganisationDetails.Name = "CONNSIGNOR ORG";
			consolAndShipment.Shipment.ShipmentDetails.Consignee.EDICode = "CONNSINGEE1";
			consolAndShipment.Shipment.ShipmentDetails.Consignee.OrganisationDetails.Name = "CONNSIGNEE ORG";
			consolAndShipment.Shipment.Declaration = new Xsd.Declaration();

			var docAddress1 = consolAndShipment.Shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew();
			docAddress1.AddressType = Xsd.DocAddressAddressType.OQP;
			docAddress1.AddressLine1 = "line 1";
			docAddress1.AddressLine2 = "line 2";
			docAddress1.CityOrSuburb = "CITY";

			var docAddress2 = consolAndShipment.Shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew();
			docAddress2.AddressType = Xsd.DocAddressAddressType.NPP;
			docAddress2.AddressLine1 = "line 1";
			docAddress2.AddressLine2 = "line 2";
			docAddress2.CityOrSuburb = "CITY";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(declaration, consolAndShipment, context);

			AssertNotNull(declaration.JE_OH_Importer);

			AssertNotNull(declaration.ImporterDocumentaryAddress);
			AssertEquals("", declaration.ImporterDocumentaryAddress.E2_Address1);
			AssertEquals("", declaration.ImporterDocumentaryAddress.E2_Address2);
			AssertEquals("", declaration.ImporterDocumentaryAddress.E2_City);

			AssertNotNull(declaration.SupplierDocumentaryAddress);
			AssertEquals("", declaration.SupplierDocumentaryAddress.E2_Address1);
			AssertEquals("", declaration.SupplierDocumentaryAddress.E2_Address2);
			AssertEquals("", declaration.SupplierDocumentaryAddress.E2_City);
		}

		public void TestImportAddressesToShipment()
		{
			NotificationBuffer notify = new NotificationBuffer();

			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Consol = new Xsd.Consol();
			consolAndShipment.Consol.ConsolDetail.AgentReference = "AgentReference";

			consolAndShipment.Shipment = new Xsd.Shipment();
			consolAndShipment.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			consolAndShipment.Shipment.ShipmentIdentifier.AddNew().Value = "BLAH";
			consolAndShipment.Shipment.Declaration = new Xsd.Declaration();

			Xsd.DocAddress docAddress1 = consolAndShipment.Shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew();
			docAddress1.AddressType = Xsd.DocAddressAddressType.CRD;
			docAddress1.AddressTypeSpecified = true;
			docAddress1.AddressLine1 = "line 1";
			docAddress1.AddressLine2 = "line 2";
			docAddress1.CityOrSuburb = "CITY";

			Xsd.DocAddress docAddress2 = consolAndShipment.Shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew();
			docAddress2.AddressType = Xsd.DocAddressAddressType.CED;
			docAddress2.AddressTypeSpecified = true;
			docAddress2.AddressLine1 = "line 3";
			docAddress2.AddressLine2 = "line 4";
			docAddress2.CityOrSuburb = "CITY2";

			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "OLD CON";
			consignor.OH_FullName = "OLD CONSIGNOR";
			shipment.ConsignorPK = consignor.PK;

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "OLD CONE";
			consignee.OH_FullName = "OLD CONSIGNEE";
			shipment.ConsigneePK = consignee.PK;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(declaration, consolAndShipment, context);

			AssertEquals(consignor.PK, shipment.ConsignorPK);
			AssertEquals(consignee.PK, shipment.ConsigneePK);

			consolAndShipment.Shipment.ShipmentDetails.DocAddresses.DocAddress.Clear();
			adapter.ImportFromValueObject(declaration, consolAndShipment, context);

			AssertEquals(consignor.PK, shipment.ConsignorPK);
			AssertEquals(consignee.PK, shipment.ConsigneePK);

			consolAndShipment.Shipment.ShipmentDetails.Consignor.EDICode = "CONNSINGOR1";
			consolAndShipment.Shipment.ShipmentDetails.Consignor.OrganisationDetails.Name = "CONNSIGNOR ORG";
			consolAndShipment.Shipment.ShipmentDetails.Consignee.EDICode = "CONNSINGEE1";
			consolAndShipment.Shipment.ShipmentDetails.Consignee.OrganisationDetails.Name = "CONNSIGNEE ORG";
			adapter.ImportFromValueObject(declaration, consolAndShipment, context);

			AssertNotNull("Consignor", shipment.ConsignorDocumentaryAddress.Organisation);
			AssertEquals("CONNSIGNOR ORG", shipment.ConsignorDocumentaryAddress.Organisation.OH_FullName);
			AssertNotNull("Consignee", shipment.ConsigneeDocumentaryAddress.Organisation);
			AssertEquals("CONNSIGNEE ORG", shipment.ConsigneeDocumentaryAddress.Organisation.OH_FullName);
		}

		public void TestImportContainersFromISOOrContainerCode()
		{
			NotificationBuffer notify = new NotificationBuffer();

			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Consol = new Xsd.Consol();
			consolAndShipment.Consol.ConsolDetail.AgentReference = "AgentReference";

			consolAndShipment.Shipment = new Xsd.Shipment();
			consolAndShipment.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			consolAndShipment.Shipment.Declaration = new Xsd.Declaration();

			AddContainerValue(consolAndShipment.Consol.ConsolDetail.Containers, "CONT1", "20GP", "22G0");
			AddContainerValue(consolAndShipment.Consol.ConsolDetail.Containers, "CONT2", "wrong", "22P0");
			AddContainerValue(consolAndShipment.Consol.ConsolDetail.Containers, "CONT3", "40GP", "wrong");

			consolAndShipment.Consol.ConsolDetail.ContainerMode = Xsd.ContainerMode.LCL;
			consolAndShipment.Consol.ConsolDetail.ContainerModeSpecified = true;

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(declaration, consolAndShipment, context);

			AssertEquals("3 containers expected", 3, declaration.CusContainers.Count);
			AssertHasContainer(declaration, "CONT1", "20GP");
			AssertHasContainer(declaration, "CONT2", "20PL");
			AssertHasContainer(declaration, "CONT3", "40GP");
		}

		public void TestImportAgentReferencePutInDeclarationRef()
		{
			SystemDataRegistry.Instance.ImportDeclarationNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.ConsolAndShipment xmlConsolAndShipment = new Xsd.ConsolAndShipment();
			xmlConsolAndShipment.Consol.ConsolDetail.AgentReference = "AgentReference";

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(declaration, xmlConsolAndShipment, context);

			AssertEquals("Should populate declaration ref with agent ref", "AgentReference", declaration.JE_DeclarationReference);
		}

		public void TestDelarationIsNotCreatedIfExists()
		{
			SystemDataRegistry.Instance.ImportDeclarationNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			BaseJobDeclaration existstingDeclaration = Factory.New<BaseJobDeclaration>();
			existstingDeclaration.JE_DeclarationReference = "B00001002";

			Factory.Save();

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.ConsolAndShipment xmlConsolAndShipment = new Xsd.ConsolAndShipment();
			xmlConsolAndShipment.Consol.ConsolDetail.AgentReference = "B00001002";

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			int numberOfDecsBeforeImport = Factory.GetDatabaseCount(typeof(BaseJobDeclaration));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			adapter.ImportFromValueObject(declaration, xmlConsolAndShipment, context);
			AssertEquals("Should have Data Prevent Save Error", true, notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));

			AssertEquals("No declaration is created", numberOfDecsBeforeImport, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
		}

		public void TestImportMasterBill()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL1";

			masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL2";

			masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL3";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportMasterbillDetails(declaration, consol, importContext);
			AssertEquals(3, declaration.Bills.Count);
			AssertEquals("MBL1", declaration.JE_MasterBill);
			AssertEquals("MBL1", declaration.Bills[0].CU_MasterBill);
			AssertEquals("MBL2", declaration.Bills[1].CU_MasterBill);
			AssertEquals("MBL3", declaration.Bills[2].CU_MasterBill);
		}

		public void TestImportHouseBill()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL1";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL2";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL3";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportHousebillDetails(declaration, null, shipment, context);
			AssertEquals(3, declaration.Bills.Count);
			AssertEquals("HBL1", declaration.JE_HouseBill);
			AssertEquals("HBL1", declaration.Bills[0].CU_HouseBill);
			AssertEquals("HBL2", declaration.Bills[1].CU_HouseBill);
			AssertEquals("HBL3", declaration.Bills[2].CU_HouseBill);
		}

		public void TestImportHouseBillWhere1HousebillAlreadyExists()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_HouseBill = "HBL2";

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL1";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL2";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL3";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportHousebillDetails(declaration, null, shipment, context);
			AssertEquals(3, declaration.Bills.Count);
			AssertEquals("HBL2", declaration.JE_HouseBill);
			AssertEquals("HBL2", declaration.Bills[0].CU_HouseBill);
			AssertEquals("HBL1", declaration.Bills[1].CU_HouseBill);
			AssertEquals("HBL3", declaration.Bills[2].CU_HouseBill);
		}

		public void TestImportHouseBillAndMasterBill()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			Xsd.ConsolIdentifier consolID;

			Xsd.Consol consol = new Xsd.Consol();
			consolID = consol.ConsolIdentifier.AddNew();
			consolID.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolID.Value = "MBL1";

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL1";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL2";
			houseBill.Masterbill = "MBL2";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL3";
			houseBill.Masterbill = "MBL3";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportConsolDetails(declaration, consol, context);
			adapter.ImportHousebillDetails(declaration, consol, shipment, context);

			AssertEquals(6, declaration.Bills.Count);
			AssertEquals("HBL1", declaration.JE_HouseBill);
			AssertEquals("MBL1", declaration.JE_MasterBill);
			AssertEquals("MBL1", declaration.Bills[0].CU_MasterBill);
			AssertEquals("HBL1", declaration.Bills[1].CU_HouseBill);
			AssertEquals("MBL2", declaration.Bills[2].CU_MasterBill);
			AssertEquals("HBL2", declaration.Bills[3].CU_HouseBill);
			AssertEquals("MBL3", declaration.Bills[4].CU_MasterBill);
			AssertEquals("HBL3", declaration.Bills[5].CU_HouseBill);
		}

		public void TestImportHouseBillAndMasterBillWithExistingMaster()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier consolID = consol.ConsolIdentifier.AddNew();
			consolID.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolID.Value = "MBL1";

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL2";
			houseBill.Masterbill = "MBL2";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL3";
			houseBill.Masterbill = "MBL3";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL4";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportConsolDetails(declaration, consol, context);
			adapter.ImportHousebillDetails(declaration, consol, shipment, context);

			AssertEquals(6, declaration.Bills.Count);
			AssertEquals("HBL4", declaration.JE_HouseBill);
			AssertEquals("MBL1", declaration.JE_MasterBill);
			Assert("HBL4", DoesDeclarationHaveThisBill(declaration, "HBL4", BillTypeList.Codes.HouseBill));
			Assert("MBL1", DoesDeclarationHaveThisBill(declaration, "MBL1", BillTypeList.Codes.MasterBill));
			Assert("HBL2", DoesDeclarationHaveThisBill(declaration, "HBL2", BillTypeList.Codes.HouseBill));
			Assert("MBL2", DoesDeclarationHaveThisBill(declaration, "MBL2", BillTypeList.Codes.MasterBill));
			Assert("HBL3", DoesDeclarationHaveThisBill(declaration, "HBL3", BillTypeList.Codes.HouseBill));
			Assert("MBL3", DoesDeclarationHaveThisBill(declaration, "MBL3", BillTypeList.Codes.MasterBill));
		}

		public void TestImportHouseBillWithEmptyMasterBillWhere1HouseBillAlreadyExists()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			declaration.JE_HouseBill = "HBL2";
			declaration.JE_MasterBill = "MBL2";

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL1";
			houseBill.Masterbill = "MBL1";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL3";
			houseBill.Masterbill = "MBL3";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportHousebillDetails(declaration, null, shipment, context);
			AssertEquals(6, declaration.Bills.Count);
			AssertEquals("HBL2", declaration.JE_HouseBill);
			AssertEquals("MBL2", declaration.JE_MasterBill);
			Assert("HBL2", DoesDeclarationHaveThisBill(declaration, "HBL2", BillTypeList.Codes.HouseBill));
			Assert("MBL2", DoesDeclarationHaveThisBill(declaration, "MBL2", BillTypeList.Codes.MasterBill));
			Assert("MBL1", DoesDeclarationHaveThisBill(declaration, "MBL1", BillTypeList.Codes.MasterBill));
			Assert("HBL1", DoesDeclarationHaveThisBill(declaration, "HBL1", BillTypeList.Codes.HouseBill));
			Assert("HBL3", DoesDeclarationHaveThisBill(declaration, "HBL3", BillTypeList.Codes.HouseBill));
			Assert("MBL3", DoesDeclarationHaveThisBill(declaration, "MBL3", BillTypeList.Codes.MasterBill));
		}

		public void TestImportHouseBillWithMasterAndNonMatchingMaster()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier consolID = consol.ConsolIdentifier.AddNew();
			consolID.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolID.Value = "MBL1";

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier houseBill1 = shipment.ShipmentIdentifier.AddNew();
			houseBill1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill1.Value = "HBL1";
			houseBill1.Masterbill = "MBL1A";

			houseBill1 = shipment.ShipmentIdentifier.AddNew();
			houseBill1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill1.Value = "HBL2";
			houseBill1.Masterbill = "MBL2";

			houseBill1 = shipment.ShipmentIdentifier.AddNew();
			houseBill1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill1.Value = "HBL3";
			houseBill1.Masterbill = "MBL3";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportConsolDetails(declaration, consol, context);

			adapter.ImportHousebillDetails(declaration, consol, shipment, context);

			AssertEquals(7, declaration.Bills.Count);
			AssertEquals("", declaration.JE_HouseBill);
			AssertEquals("MBL1", declaration.JE_MasterBill);

			Bill houseBill = declaration.Bills.FindByBillNumberAndType("HBL1", BillTypeList.Codes.HouseBill);
			AssertNotNull(houseBill);
			AssertEquals("MBL1A", houseBill.CU_MasterBill);

			houseBill = declaration.Bills.FindByBillNumberAndType("HBL2", BillTypeList.Codes.HouseBill);
			AssertNotNull(houseBill);
			AssertEquals("MBL2", houseBill.CU_MasterBill);

			houseBill = declaration.Bills.FindByBillNumberAndType("HBL3", BillTypeList.Codes.HouseBill);
			AssertNotNull(houseBill);
			AssertEquals("MBL3", houseBill.CU_MasterBill);
		}

		public void TestImportHouseBillAndMasterWithMultipleExistingConsol()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier consolID = consol.ConsolIdentifier.AddNew();
			consolID.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolID.Value = "MBL1";

			consolID = consol.ConsolIdentifier.AddNew();
			consolID.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolID.Value = "MBL2";

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "";
			houseBill.Masterbill = "MBL1";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL1";
			houseBill.Masterbill = "MBL1";

			houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL2";
			houseBill.Masterbill = "MBL2";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportConsolDetails(declaration, consol, context);
			adapter.ImportHousebillDetails(declaration, null, shipment, context);

			AssertEquals(5, declaration.Bills.Count);
			Assert("HBL1", DoesDeclarationHaveThisBill(declaration, "HBL1", BillTypeList.Codes.HouseBill));
			Assert("MBL1", DoesDeclarationHaveThisBill(declaration, "MBL1", BillTypeList.Codes.MasterBill));
			Assert("HBL2", DoesDeclarationHaveThisBill(declaration, "HBL2", BillTypeList.Codes.HouseBill));
			Assert("MBL2", DoesDeclarationHaveThisBill(declaration, "MBL2", BillTypeList.Codes.MasterBill));
			Assert("Empty house bill", DoesDeclarationHaveThisBill(declaration, "", BillTypeList.Codes.HouseBill));
		}

		public void TestImportMasterBillWhere1MasterBillAlreadyExists()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MasterBill = "MBL2";

			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL1";

			masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL2";

			masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL3";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportMasterbillDetails(declaration, consol, importContext);
			AssertEquals(3, declaration.Bills.Count);
			AssertEquals("MBL2", declaration.JE_MasterBill);
			AssertEquals("MBL2", declaration.Bills[0].CU_MasterBill);
			AssertEquals("MBL1", declaration.Bills[1].CU_MasterBill);
			AssertEquals("MBL3", declaration.Bills[2].CU_MasterBill);
		}

		public void TestImportMasterBillWhereEmptyMasterBillAlreadyExists()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_HouseBill = "HBL1";

			Bill emptyMasterBill = declaration.Bills.AddNew();
			emptyMasterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			emptyMasterBill.CU_BillNum = "";

			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL1";

			masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL2";

			masterBill = consol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL3";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportMasterbillDetails(declaration, consol, importContext);
			AssertEquals(4, declaration.Bills.Count);
			AssertEquals("MBL1", declaration.JE_MasterBill);

			AssertNotNull(declaration.Bills.FindByBillNumberAndType("MBL1", BillTypeList.Codes.MasterBill));
			AssertNotNull(declaration.Bills.FindByBillNumberAndType("MBL2", BillTypeList.Codes.MasterBill));
			AssertNotNull(declaration.Bills.FindByBillNumberAndType("MBL3", BillTypeList.Codes.MasterBill));
		}

		public virtual void TestImportHouseBillWhereEmptyHousebillAlreadyExists()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MasterBill = "MBL1";

			Xsd.ConsolAndShipment declarationValue = new Xsd.ConsolAndShipment();

			Xsd.Shipment shipment = declarationValue.Shipment;
			Xsd.ShipmentIdentifier houseBill1 = shipment.ShipmentIdentifier.AddNew();
			houseBill1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill1.Value = "HBL1";

			houseBill1 = shipment.ShipmentIdentifier.AddNew();
			houseBill1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill1.Value = "HBL2";

			houseBill1 = shipment.ShipmentIdentifier.AddNew();
			houseBill1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill1.Value = "HBL3";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(declaration, declarationValue, context);
			AssertEquals(4, declaration.Bills.Count);
			AssertEquals("", declaration.JE_HouseBill);
			AssertEquals("MBL1", declaration.JE_MasterBill);
			Assert("MBL1", DoesDeclarationHaveThisBill(declaration, "MBL1", BillTypeList.Codes.MasterBill));

			Bill houseBill = declaration.Bills.FindByBillNumberAndType("HBL1", BillTypeList.Codes.HouseBill);
			AssertNotNull(houseBill);
			AssertNull("no parent", houseBill.ParentBill);

			houseBill = declaration.Bills.FindByBillNumberAndType("HBL2", BillTypeList.Codes.HouseBill);
			AssertNotNull(houseBill);
			AssertNull("no parent", houseBill.ParentBill);

			houseBill = declaration.Bills.FindByBillNumberAndType("HBL3", BillTypeList.Codes.HouseBill);
			AssertNotNull(houseBill);
			AssertNull("no parent", houseBill.ParentBill);
		}

		public void TestOnImportCreateDefaultPackingGroupIfNoPackingGroupSpecified()
		{
			Xsd.ConsolAndShipment declarationValue = new Xsd.ConsolAndShipment();
			declarationValue.Consol.ConsolDetail.PortFirstArrival.ActualDateTime = ZDateTime.Now;
			Xsd.Container container = declarationValue.Consol.ConsolDetail.Containers.AddNew();
			container.ContainerNumber = "C12345";

			Xsd.ShipmentIdentifier shipmentIdentifier = declarationValue.Shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "HouseBill";
			declarationValue.Shipment.ShipmentDetails.TotalOuterPacksQty.Value = 5;
			declarationValue.Shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = Core.Constants.PkgUnit.Unit;

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(declaration, declarationValue, context);

			AssertEquals("A default packing group should be created if none were explicitly specified in the value object", 1, declaration.PackingGroups.Count);
			AssertEquals("Packing group should be attached to the house bill", declaration.Bills[0].PK, declaration.PackingGroups[0].Bill.PK);
			AssertEquals("NumberOfPackages", 5, declaration.PackingGroups[0].TotalPackageCount());
			AssertEquals("Container", "C12345", declaration.PackingGroups[0].Container.CO_ContainerNumber);
		}

		public void TestImportMergeBy()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "AUSYD";
			importer.OH_Code = "MERGE";
			importer.MainAddress.OA_Address1 = "BLAH ST";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Tariff;

			Xsd.Shipment shipment = new Xsd.Shipment();
			shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment.ShipmentDetails.Consignee = new Xsd.Organisation();
			shipment.ShipmentDetails.Consignee.OrganisationDetails.Name = importer.OH_FullName;
			shipment.ShipmentDetails.Consignee.EDICode = importer.OH_Code;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportShipmentDetails(jobDec, null, shipment, context);
			AssertEquals("Importer should be picked up", importer.PK, jobDec.JE_OH_Importer);
			AssertEquals("MergeBy should be defaulted from Importer", OrgConstants.MergeInvoiceLines.Tariff, jobDec.JE_MergeBy);

			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Factory.Save();
			adapter.ImportShipmentDetails(jobDec, null, shipment, context);
			AssertEquals("MergeBy should be defaulted from Importer", OrgConstants.MergeInvoiceLines.NotMerge, jobDec.JE_MergeBy);
		}

		public void TestOnImportEnsureEmptyMarksAndNumbersDoesNotGetAdded()
		{
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Shipment = new Xsd.Shipment();
			consolAndShipment.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;

			consolAndShipment.Consol = new Xsd.Consol();
			consolAndShipment.Consol.ConsolDetail = new Xsd.ConsolConsolDetail();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);

			AssertEquals(false, jobDec.Notes.HasNotes);
		}

		public void TestImport_LCLDates()
		{
			Xsd.SailingWithVesselVoyage xmlVessel = new Xsd.SailingWithVesselVoyage();
			xmlVessel.LCLDates.AvailableDate = new DateTime(2005, 05, 25);

			BaseJobDeclaration jobDec = GetJobDeclaration();

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportVesselInformationDates(xmlVessel, jobDec, importContext);

			AssertEquals(new ZDateTime(2005, 05, 25), jobDec.DocsAndCartage.JP_LCLAvailable);
			AssertEquals(ZDateTime.Empty, jobDec.DocsAndCartage.JP_FCLAvailable);
		}

		public void TestImport_FCLDates()
		{
			Xsd.SailingWithVesselVoyage xmlVessel = new Xsd.SailingWithVesselVoyage();
			xmlVessel.FCLDates.AvailableDate = new DateTime(2005, 05, 25);

			BaseJobDeclaration jobDec = GetJobDeclaration();

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportVesselInformationDates(xmlVessel, jobDec, importContext);

			AssertEquals(new ZDateTime(2005, 05, 25), jobDec.DocsAndCartage.JP_FCLAvailable);
			AssertEquals(ZDateTime.Empty, jobDec.DocsAndCartage.JP_LCLAvailable);
		}

		public void TestFromXmlValueObject()
		{
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Shipment = new Xsd.Shipment();
			consolAndShipment.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;
			consolAndShipment.Shipment.Declaration = new Xsd.Declaration();
			consolAndShipment.Shipment.Invoices = new Xsd.InvoiceHeaderCollection();

			consolAndShipment.Consol = new Xsd.Consol();
			consolAndShipment.Consol.ConsolDetail = new Xsd.ConsolConsolDetail();
			consolAndShipment.Consol.ConsolDetail.Containers = new Xsd.ContainerCollection();
			consolAndShipment.Consol.ConsolDetail.ContainerMode = Xsd.ContainerMode.LCL;
			consolAndShipment.Consol.ConsolDetail.ContainerModeSpecified = true;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);

			AssertEquals("TransportMode:", Core.Constants.TransportModes.Air, jobDec.JE_TransportMode);
			AssertEquals("ContainerMode:", Core.Constants.ContainerModes.LCL, jobDec.JE_ContainerMode);
		}

		public void TestImportConsolDetails_SEA()
		{
			Xsd.Consol consol = new Xsd.Consol();

			jobDec.JE_TransportMode = "SEA";
			consol.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();

			Xsd.ConsolIdentifier identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = "IDENT1";

			identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = "IDENT2";

			identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			identifier.Value = "IDENT3";

			consol.ConsolDetail = new Xsd.ConsolConsolDetail();
			consol.ConsolDetail.PortOfLoading = new Xsd.Movement();
			consol.ConsolDetail.PortOfLoading.ActualDateTime = new DateTime(2005, 3, 13);
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = new DateTime(2005, 3, 14);
			consol.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUMEL");
			consol.ConsolDetail.PortOfDischarge = new Xsd.Movement();
			consol.ConsolDetail.PortOfDischarge.ActualDateTime = new DateTime(2005, 3, 17);
			consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = new DateTime(2005, 3, 18);
			consol.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			consol.ConsolDetail.PortFirstArrival = new Xsd.Movement();
			consol.ConsolDetail.PortFirstArrival.ActualDateTime = new DateTime(2005, 3, 17);
			consol.ConsolDetail.PortFirstArrival.EstimatedDateTime = new DateTime(2005, 3, 18);
			consol.ConsolDetail.PortFirstArrival.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");

			consol.ConsolDetail.Item = new Xsd.SailingWithVesselVoyage();
			((Xsd.SailingWithVesselVoyage)consol.ConsolDetail.Item).LloydsNo = "LloydsNo";
			((Xsd.SailingWithVesselVoyage)consol.ConsolDetail.Item).VesselName = "Name";
			((Xsd.SailingWithVesselVoyage)consol.ConsolDetail.Item).VoyageNo = "Voyage";

			consol.ConsolDetail.Carrier = new Xsd.Organisation();
			consol.ConsolDetail.Carrier.EDICode = "DOODAD";
			consol.ConsolDetail.Carrier.OrganisationDetails = new Xsd.OrganisationDetail();
			consol.ConsolDetail.Carrier.OrganisationDetails.Name = "DOODAD";
			consol.ConsolDetail.Carrier.IsSpecified = true;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportConsolDetails(jobDec, consol, context);

			AssertEquals("IDENT1", jobDec.JE_MasterBill);
			AssertEquals("IDENT1", jobDec.Bills[0].CU_MasterBill);
			AssertEquals("IDENT2", jobDec.Bills[1].CU_MasterBill);
			AssertEquals("AUMEL", jobDec.JE_RL_NKPortOfLoading);
			AssertEquals(new ZDateTime(2005, 3, 13), jobDec.JE_ExportDate);
			AssertEquals("AUSYD", jobDec.JE_RL_NKPortOfArrival);
			AssertEquals(new ZDateTime(2005, 3, 17), jobDec.JE_DateOfArrival);
			if (ShouldTestonFirstArrivalInfo)
			{
				AssertEquals("AUSYD", jobDec.JE_RL_NKPortOfFirstArrival);
				AssertEquals(new ZDateTime(2005, 3, 17), jobDec.JE_DateOfFirstArrival);
			}
			AssertEquals("Voyage", jobDec.JE_VoyageFlightNo);
			AssertNotNull("Must import", jobDec.ShippingLine);

			TestImportConsolValues_ReceivingSendingRecievingAgent(consol, context);
		}

		public void TestImportConsolDetails_AIR()
		{
			Xsd.Consol consol = new Xsd.Consol();

			jobDec.JE_TransportMode = "AIR";

			consol.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = "IDENT1";

			identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = "IDENT2";

			identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			identifier.Value = "IDENT3";

			consol.ConsolDetail = new Xsd.ConsolConsolDetail();
			consol.ConsolDetail.PortOfLoading = new Xsd.Movement();
			consol.ConsolDetail.PortOfLoading.ActualDateTime = new DateTime(2005, 3, 13);
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = new DateTime(2005, 3, 14);
			consol.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUMEL");
			consol.ConsolDetail.PortOfDischarge = new Xsd.Movement();
			consol.ConsolDetail.PortOfDischarge.ActualDateTime = new DateTime(2005, 3, 17);
			consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = new DateTime(2005, 3, 18);
			consol.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			consol.ConsolDetail.PortFirstArrival = new Xsd.Movement();
			consol.ConsolDetail.PortFirstArrival.ActualDateTime = new DateTime(2005, 3, 17);
			consol.ConsolDetail.PortFirstArrival.EstimatedDateTime = new DateTime(2005, 3, 18);
			consol.ConsolDetail.PortFirstArrival.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			consol.ConsolDetail.Item = new Xsd.FlightWithFlightNumber();
			((Xsd.FlightWithFlightNumber)consol.ConsolDetail.Item).FlightNoJourneyNoTruckRegNo = "Flight";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportConsolDetails(jobDec, consol, context);

			AssertEquals("IDENT1", jobDec.JE_MasterBill);
			AssertEquals("IDENT1", jobDec.Bills[0].CU_MasterBill);
			AssertEquals("IDENT2", jobDec.Bills[1].CU_MasterBill);
			AssertEquals("AUMEL", jobDec.JE_RL_NKPortOfLoading);
			AssertEquals(new ZDateTime(2005, 3, 13), jobDec.JE_ExportDate);
			AssertEquals("AUSYD", jobDec.JE_RL_NKPortOfArrival);
			AssertEquals(new ZDateTime(2005, 3, 17), jobDec.JE_DateOfArrival);
			if (ShouldTestonFirstArrivalInfo)
			{
				AssertEquals("AUSYD", jobDec.JE_RL_NKPortOfFirstArrival);
				AssertEquals(new ZDateTime(2005, 3, 17), jobDec.JE_DateOfFirstArrival);
			}
			AssertEquals("Flight", jobDec.JE_VoyageFlightNo);

			TestImportConsolValues_ReceivingSendingRecievingAgent(consol, context);
		}

		public void TestImportVesselInformationIfValidLloydsNo()
		{
			BaseJobDeclaration jobDec = GetEmptyJobDeclaration();
			Xsd.SailingWithVesselVoyage vesselInfo = new Xsd.SailingWithVesselVoyage();
			vesselInfo.LloydsNo = "8811924";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportVesselInformation(jobDec, vesselInfo, importContext);
			AssertEquals("Should have valid vessel", "ADMIRALENGRACHT", jobDec.JE_VesselName);
		}

		public void TestImportVesselInformationIfProvideOnlyName()
		{
			BaseJobDeclaration jobDec = GetEmptyJobDeclaration();
			Xsd.SailingWithVesselVoyage vesselInfo = new Xsd.SailingWithVesselVoyage();
			vesselInfo.VesselName = "ADMIRALENGRACHT";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportVesselInformation(jobDec, vesselInfo, importContext);
			AssertEquals("Should have valid vessel", "ADMIRALENGRACHT", jobDec.JE_VesselName);
		}

		public void TestImportVesselInformationIfNull()
		{
			BaseJobDeclaration jobDec = GetEmptyJobDeclaration();
			Xsd.SailingWithVesselVoyage vesselInfo = new Xsd.SailingWithVesselVoyage();
			bool exceptionOccured = false;
			try
			{
				ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
				adapter.ImportVesselInformation(jobDec, vesselInfo, importContext);
			}
			catch (NullReferenceException)
			{
				exceptionOccured = true;
			}

			AssertEquals("NullReferenceException occured because VesselInfo Was Null", false, exceptionOccured);
		}

		public void TestImportVesselInformation_MatchByLloydsNumber()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			var jobDec = GetEmptyJobDeclaration();
			var vesselInfo = new Xsd.SailingWithVesselVoyage();
			vesselInfo.LloydsNo = vessel.RV_LloydsNumber;
			vesselInfo.VesselName = "BLAH";

			adapter.ImportVesselInformation(jobDec, vesselInfo, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(vessel.RV_Code, jobDec.JE_VesselName);
			AssertEquals(vessel.PK, jobDec.Vessel.PK);
		}

		public void TestImportVesselInformation_MatchByVesselNameWhenLloydsNumberInvalidOrEmpty()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var jobDec = GetEmptyJobDeclaration();
			var vesselInfo = new Xsd.SailingWithVesselVoyage();
			vesselInfo.LloydsNo = "9999999";
			vesselInfo.VesselName = existingVessel.RV_Code;

			adapter.ImportVesselInformation(jobDec, vesselInfo, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(existingVessel.RV_Code, jobDec.JE_VesselName);
			AssertEquals(existingVessel.PK, jobDec.Vessel.PK);
		}

		public void TestImportVesselInformation_MatchingNotFoundImportVesselNameAsVessel()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var jobDec = GetEmptyJobDeclaration();
			var vesselInfo = new Xsd.SailingWithVesselVoyage();
			vesselInfo.LloydsNo = "9999999";
			vesselInfo.VesselName = "BLAH";

			adapter.ImportVesselInformation(jobDec, vesselInfo, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("BLAH", jobDec.JE_VesselName);
			AssertNull(jobDec.Vessel);
		}

		public void TestImportVesselInformation_MatchingNotFoundImportLooydsNoAsVessel()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var jobDec = GetEmptyJobDeclaration();
			var vesselInfo = new Xsd.SailingWithVesselVoyage();
			vesselInfo.LloydsNo = "9999999";

			adapter.ImportVesselInformation(jobDec, vesselInfo, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("9999999", jobDec.JE_VesselName);
			AssertNull(jobDec.Vessel);
		}

		public void TestImportContainers()
		{
			Xsd.ContainerCollection xsdContainers = new Xsd.ContainerCollection();

			Xsd.Container xsdContainer = xsdContainers.AddNew();
			xsdContainer.ContainerNumber = "CONTAINERNUM";
			xsdContainer.ContainerType = new Xsd.ContainerType();
			xsdContainer.ContainerType.Height = 10;
			xsdContainer.ContainerType.HeightSpecified = true;
			xsdContainer.ContainerType.ISOCode = "ISOCode";
			xsdContainer.ContainerType.Length = 11;
			xsdContainer.ContainerType.LengthSpecified = true;
			xsdContainer.ContainerType.USContainerCode = "USCode";
			xsdContainer.ContainerType.Width = 12;
			xsdContainer.ContainerType.WidthSpecified = true;
			xsdContainer.PackingMode = Xsd.ContainerMode.AIR;
			xsdContainer.Seal = "SEAL";
			xsdContainer.Seal2 = "SEAL2";
			xsdContainer.Weight = 12.33m;

			#region Export Process
			xsdContainer.ExportProcess.EmptyRequiredBy = testDate.AddDays(1);
			xsdContainer.ExportProcess.EstimatedFullPickup = testDate.AddDays(2);
			xsdContainer.ExportProcess.CartageAdvised = testDate.AddDays(3);
			xsdContainer.ExportProcess.SlotBookingRef = "export book ref";
			xsdContainer.ExportProcess.SlotDate = testDate.AddDays(4);
			xsdContainer.ExportProcess.CartageRef = "export cart ref";
			xsdContainer.ExportProcess.ContainerYardGateOut = testDate.AddDays(5);
			xsdContainer.ExportProcess.WharfGateIn = testDate.AddDays(6);
			xsdContainer.ExportProcess.CartageComplete = testDate.AddDays(7);
			xsdContainer.ExportProcess.ShippedOnboard = testDate.AddDays(8);
			xsdContainer.ExportProcess.DemurrageTime = testDate.AddDays(9).AddYears(ZDateTime.DefaultDurationEpoch.Year - testDate.Year);
			xsdContainer.ExportProcess.DemurrageCharge = 10M;
			xsdContainer.ExportProcess.DemurrageChargeSpecified = true;
			xsdContainer.ExportProcess.IsArrivingAtCTOByRail = false;
			xsdContainer.ExportProcess.IsArrivingAtCTOByRailSpecified = true;

			Xsd.Organisation xsdPickupFromOrg = xsdContainer.ExportProcess.PickupEmptyFrom.Organisation;
			xsdPickupFromOrg.OrganisationDetails.Name = "pickup from org";
			Xsd.OrgAddress xsdPickupFromOrgAddress = xsdPickupFromOrg.OrganisationDetails.Addresses.AddNew();
			xsdPickupFromOrgAddress.AddressLine1 = "pickup from org addr 1";
			xsdContainer.ExportProcess.PickupEmptyFrom.AddressSequenceRef = 1;
			#endregion

			#region Import Process
			xsdContainer.ImportProcess.FCLAvailable = testDate.AddDays(11);
			xsdContainer.ImportProcess.FCLStorage = testDate.AddDays(12);
			xsdContainer.ImportProcess.LCLAvailable = testDate.AddDays(13);
			xsdContainer.ImportProcess.LCLStorage = testDate.AddDays(14);
			xsdContainer.ImportProcess.WharfUnload = testDate.AddDays(15);
			xsdContainer.ImportProcess.SlotBookingRef = "import book ref";
			xsdContainer.ImportProcess.SlotDate = testDate.AddDays(16);
			xsdContainer.ImportProcess.CartageRef = "import cart ref";
			xsdContainer.ImportProcess.WharfGateOut = testDate.AddDays(17);
			xsdContainer.ImportProcess.EstimatedDelivery = testDate.AddDays(18);
			xsdContainer.ImportProcess.CartageAdvised = testDate.AddDays(19);
			xsdContainer.ImportProcess.CartageComplete = testDate.AddDays(20);

			xsdContainer.ImportProcess.EmptyReady = testDate.AddDays(21);
			xsdContainer.ImportProcess.EmptyReturnRequiredBy = testDate.AddDays(22);
			xsdContainer.ImportProcess.EmptyReturnedOn = testDate.AddDays(23);

			xsdContainer.ImportProcess.PickupByRail = false;
			xsdContainer.ImportProcess.PickupByRailSpecified = true;
			xsdContainer.ImportProcess.HeldForFCLTransitStaging = true;
			xsdContainer.ImportProcess.HeldForFCLTransitStagingSpecified = true;

			xsdContainer.ImportProcess.StorageDays = "24";
			xsdContainer.ImportProcess.StorageCharge = 25M;
			xsdContainer.ImportProcess.StorageChargeSpecified = true;
			xsdContainer.ImportProcess.DemurrageTime = testDate.AddDays(26).AddYears(ZDateTime.DefaultDurationEpoch.Year - testDate.Year);
			xsdContainer.ImportProcess.DemurrageCharge = 27M;
			xsdContainer.ImportProcess.DemurrageChargeSpecified = true;
			xsdContainer.ImportProcess.DetentionDays = "28";
			xsdContainer.ImportProcess.DetentionCharge = 29M;
			xsdContainer.ImportProcess.DetentionChargeSpecified = true;

			Xsd.Organisation xsdDeliverEmptyToOrg = xsdContainer.ImportProcess.DeliverEmptyTo.Organisation;
			xsdDeliverEmptyToOrg.OrganisationDetails.Name = "deliver empty to org";
			Xsd.OrgAddress xsdDeliverEmptyToOrgAddress = xsdDeliverEmptyToOrg.OrganisationDetails.Addresses.AddNew();
			xsdDeliverEmptyToOrgAddress.AddressLine1 = "deliver empty to addr 1";
			xsdContainer.ImportProcess.DeliverEmptyTo.AddressSequenceRef = 1;
			#endregion

			#region Custom Fields
			xsdContainer.Custom.CustomAttribute1 = "blah";
			xsdContainer.Custom.Date1 = testDate;
			xsdContainer.Custom.Decimal1 = 111.00M;
			xsdContainer.Custom.Decimal1Specified = true;
			xsdContainer.Custom.Flag1 = true;
			xsdContainer.Custom.Flag1Specified = true;
			#endregion

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportContainers(jobDec, xsdContainers, context);

			BaseCusContainer cusContainer = jobDec.CusContainers[0];

			AssertEquals("CONTAINERNUM", cusContainer.CO_ContainerNumber);
			AssertEquals("AIR", cusContainer.CO_FCL_LCL_AIR);
			AssertEquals("SEAL", cusContainer.CO_Seal);
			AssertEquals("SEAL2", cusContainer.CO_SecondSeal);
			AssertEquals(12.33m, cusContainer.CO_Weight);

			AssertNotNull(cusContainer.JobContainer);
			CommonContainer freightContainer = cusContainer.JobContainer;

			#region Export Process
			AssertEquals("EmptyRequiredBy", xsdContainer.ExportProcess.EmptyRequiredBy, freightContainer.JC_EmptyRequired);
			AssertEquals("EstimatedFullPickup", xsdContainer.ExportProcess.EstimatedFullPickup, freightContainer.JC_DepartureEstimatedPickup);
			AssertEquals("CartageAdvised", xsdContainer.ExportProcess.CartageAdvised, freightContainer.JC_DepartureCartageAdvised);
			AssertEquals("SlotBookingRef", xsdContainer.ExportProcess.SlotBookingRef, freightContainer.JC_DepartureSlotReference);
			AssertEquals("SlotDate", xsdContainer.ExportProcess.SlotDate, freightContainer.JC_DepartureSlotDateTime);
			AssertEquals("CartageRef", xsdContainer.ExportProcess.CartageRef, freightContainer.JC_DepartureCartageRef);
			AssertEquals("ContainerYardGateOut", xsdContainer.ExportProcess.ContainerYardGateOut, freightContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals("WharfGateIn", xsdContainer.ExportProcess.WharfGateIn, freightContainer.JC_FCLWharfGateIn);
			AssertEquals("CartageComplete", xsdContainer.ExportProcess.CartageComplete, freightContainer.JC_DepartureCartageComplete);
			AssertEquals("ShippedOnboard", xsdContainer.ExportProcess.ShippedOnboard, freightContainer.JC_FCLOnBoardVessel);
			AssertEquals("DemurrageTime", xsdContainer.ExportProcess.DemurrageTime, freightContainer.DepartureTruckWaitTime);
			AssertEquals("DemurrageCharge", xsdContainer.ExportProcess.DemurrageCharge, freightContainer.DepartureTruckWaitCost);

			AssertEquals("IsArrivingAtCTOByRail", xsdContainer.ExportProcess.IsArrivingAtCTOByRail, freightContainer.JC_DepartureDeliveryByRail);

			AssertNotNull(freightContainer.DepartureContainerYardAddress);
			OrgHeader pickupFromOrg = Factory.Load<OrgHeader>(freightContainer.DepartureContainerYardAddress.OA_OH);
			AssertEquals("pick up from org", xsdPickupFromOrg.OrganisationDetails.Name, pickupFromOrg.OH_FullName);
			AssertEquals("pick up from org addr 1", xsdPickupFromOrgAddress.AddressLine1, freightContainer.DepartureContainerYardAddress.OA_Address1);
			#endregion

			#region Import Process
			AssertEquals("FCLAvailable", xsdContainer.ImportProcess.FCLAvailable, freightContainer.JC_FCLAvailable);
			AssertEquals("FCLStorage", xsdContainer.ImportProcess.FCLStorage, freightContainer.JC_ArrivalCTOStorageStartDate);
			AssertEquals("LCLAvailable", xsdContainer.ImportProcess.LCLAvailable, freightContainer.JC_LCLAvailable);
			AssertEquals("LCLStorage", xsdContainer.ImportProcess.LCLStorage, freightContainer.JC_LCLStorageCommences);
			AssertEquals("WharfUnload", xsdContainer.ImportProcess.WharfUnload, freightContainer.JC_FCLUnloadFromVessel);
			AssertEquals("SlotBookingRef", xsdContainer.ImportProcess.SlotBookingRef, freightContainer.JC_ArrivalSlotReference);
			AssertEquals("SlotDate", xsdContainer.ImportProcess.SlotDate, freightContainer.JC_ArrivalSlotDateTime);
			AssertEquals("CartageRef", xsdContainer.ImportProcess.CartageRef, freightContainer.JC_ArrivalCartageRef);
			AssertEquals("WharfGateOut", xsdContainer.ImportProcess.WharfGateOut, freightContainer.JC_FCLWharfGateOut);
			AssertEquals("EstimatedDelivery", xsdContainer.ImportProcess.EstimatedDelivery, freightContainer.JC_ArrivalEstimatedDelivery);
			AssertEquals("CartageAdvised", xsdContainer.ImportProcess.CartageAdvised, freightContainer.JC_ArrivalCartageAdvised);
			AssertEquals("CartageComplete", xsdContainer.ImportProcess.CartageComplete, freightContainer.JC_ArrivalCartageComplete);

			AssertEquals("EmptyReady", xsdContainer.ImportProcess.EmptyReady, freightContainer.JC_EmptyReadyForReturn);
			AssertEquals("EmptyReturnRequiredBy", xsdContainer.ImportProcess.EmptyReturnRequiredBy, freightContainer.JC_EmptyReturnedBy);
			AssertEquals("EmptyReturnedOn", xsdContainer.ImportProcess.EmptyReturnedOn, freightContainer.JC_ContainerYardEmptyReturnGateIn);

			AssertEquals("PickupByRail", xsdContainer.ImportProcess.PickupByRail, freightContainer.JC_ArrivalPickupByRail);
			AssertEquals("HeldForFCLTransitStaging", xsdContainer.ImportProcess.HeldForFCLTransitStaging, freightContainer.JC_FCLHeldInTransitStaging);

			AssertEquals("StorageDays", xsdContainer.ImportProcess.StorageDays, freightContainer.ArrivalCTOStorageDays.ToString());
			AssertEquals("StorageCharge", xsdContainer.ImportProcess.StorageCharge, freightContainer.ArrivalCTOStorageCost);
			AssertEquals("DemurrageTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, xsdContainer.ImportProcess.DemurrageTime.Month, xsdContainer.ImportProcess.DemurrageTime.Day), freightContainer.ArrivalTruckWaitTime);
			AssertEquals("DemurrageCharge", xsdContainer.ImportProcess.DemurrageCharge, freightContainer.ArrivalTruckWaitCost);
			AssertEquals("DetentionDays", xsdContainer.ImportProcess.DetentionDays, freightContainer.ArrivalCarrierDetentionDays.ToString());
			AssertEquals("DetentionCharge", xsdContainer.ImportProcess.DetentionCharge, freightContainer.ArrivalCarrierDetentionCost);

			AssertNotNull(freightContainer.ArrivalContainerYardAddress);
			OrgHeader deliverEmptyToOrg = Factory.Load<OrgHeader>(freightContainer.ArrivalContainerYardAddress.OA_OH);
			AssertEquals("deliver empty to org", xsdDeliverEmptyToOrg.OrganisationDetails.Name, deliverEmptyToOrg.OH_FullName);
			AssertEquals("deliver empty to addr 1", xsdDeliverEmptyToOrgAddress.AddressLine1, freightContainer.ArrivalContainerYardAddress.OA_Address1);
			#endregion

			#region Custom Fields
			AssertEquals(xsdContainer.Custom.CustomAttribute1, cusContainer.CO_CustomAttrib1);
			AssertEquals(xsdContainer.Custom.Date1, cusContainer.CO_CustomDate1);
			AssertEquals(xsdContainer.Custom.Decimal1, cusContainer.CO_CustomDecimal1);
			AssertEquals(xsdContainer.Custom.Flag1, cusContainer.CO_CustomFlag1);
			#endregion
		}

		public void TestImportShipmentDetails()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			shipment.Declaration = new Xsd.Declaration();
			shipment.Declaration.ManifestID = "Manifest";
			shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment.ShipmentIdentifier = new Xsd.ShipmentIdentifierCollection();

			Xsd.ShipmentIdentifier identifier = shipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "IDENT1";
			identifier = shipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;
			identifier.Value = "IDENT2";

			shipment.Notes = new Xsd.NotesNoteCollection();
			Xsd.NotesNote note = shipment.Notes.AddNew();
			note.NoteType = Enterprise.DataTransfer.Xml.XsdVersion1.NotesNoteNoteType.CustomsDeliveryInstructions;
			note.NoteData = "NOTETEXT";

			shipment.ShipmentDetails.AgentReference = "S12345678";
			shipment.ShipmentDetails.DeclarationStyle = "DEC";
			shipment.ShipmentDetails.PortOfOrigin = new Xsd.Movement();
			shipment.ShipmentDetails.PortOfOrigin.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUMEL");
			shipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = new DateTime(2004, 1, 2);
			shipment.ShipmentDetails.ServiceLevel = "SRV";
			shipment.ShipmentDetails.MarksAndNumbers = "MarksAndNumbersShort";
			shipment.ShipmentDetails.PortofDestination = new Xsd.Movement();
			shipment.ShipmentDetails.PortofDestination.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			shipment.ShipmentDetails.PortofDestination.EstimatedDateTime = new DateTime(2004, 1, 3);
			shipment.ShipmentDetails.GoodsDescription = "GoodsDescription";
			shipment.ShipmentDetails.OwnerReference = "OwnerRef";
			shipment.Declaration.Broker.Value = "BOB";
			shipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(240), "KG");
			shipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(229), "KI");
			shipment.ShipmentDetails.TotalOuterPacksQty = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(101), "PK");
			shipment.ShipmentDetails.TotalInnerPacksQty = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(102), "IP");
			shipment.ShipmentDetails.Incoterm = "Incoterm";

			BaseJobDeclaration jobDec = GetJobDeclaration();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportShipmentDetails(jobDec, null, shipment, context);
			adapter.ImportShipmentDetails(jobDec, null, shipment, context);

			StmNote stmNote = jobDec.Notes.FindByDescription(PredefinedNoteTypes.Instance.CustomsDeliveryInstructions.Description)[0];
			AssertEquals("NOTETEXT", stmNote.ST_NoteText);

			AssertEquals(1, jobDec.Bills.Count);
			AssertEquals("IDENT1", jobDec.JE_HouseBill);
			AssertEquals("", jobDec.JE_DeclarationReference);
			AssertEquals("AUMEL", jobDec.JE_RL_NKOrigin);
			AssertEquals("SRV", jobDec.JE_RS_NKServiceLevel);

			StmNote[] notes = jobDec.Notes.FindByDescription("Marks & Numbers");
			AssertEquals("PreCondition: Note should be found", 1, notes.Length);
			AssertEquals("MarksAndNumbersShort", notes[0].ST_NoteText);

			AssertEquals(new ZDateTime(2004, 1, 2), jobDec.JE_DateAtOrigin);
			AssertEquals("AUSYD", jobDec.JE_RL_NKFinalDestination);
			AssertEquals(new ZDateTime(2004, 1, 3), jobDec.JE_DateAtFinalDestination);
			AssertEquals("GOODSDESCRIPTION", jobDec.JE_GoodsDescription.ToUpper());
			AssertEquals("OwnerRef", jobDec.JE_OwnerRef);
			AssertEquals("BOB", jobDec.JE_GS_NKCusAgent);
			AssertEquals((ZDecimal)240, jobDec.JE_TotalWeight);
			AssertEquals("KG", jobDec.JE_TotalWeightUnit);
			AssertEquals((ZDecimal)229, jobDec.JE_TotalVolume);
			AssertEquals("KI", jobDec.JE_TotalVolumeUnit);
			AssertEquals((ZInt)101, jobDec.JE_TotalNoOfPacks);
			AssertEquals("PK", jobDec.JE_TotalNoOfPacksPackType);
			AssertEquals((ZInt)102, jobDec.JE_TotalNoOfPieces);
			AssertEquals("Should have the first three characters of the ShipmentDetails.Incoterm", "Inc", jobDec.JE_ShipmentIncoTerm);
			AssertEquals("Agent Reference", "S12345678", jobDec.JE_AgentsReference);
			AssertEquals("Declaration Style", "DEC", jobDec.JE_MessageSubType);

			CustomsDataRegistry.Instance.AllowBillingImportIntoDeclaration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			shipment.ShipmentDetails.LocalClient = new Xsd.Organisation();
			shipment.ShipmentDetails.LocalClient.EDICode = "DOODAD";
			shipment.ShipmentDetails.LocalClient.OrganisationDetails = new Xsd.OrganisationDetail();
			shipment.ShipmentDetails.LocalClient.OrganisationDetails.Name = "DOODAD";
			shipment.ShipmentDetails.LocalClient.IsSpecified = true;
			adapter.ImportShipmentDetails(jobDec, null, shipment, context);
			Assert("Must import", !jobDec.Job.LocalChargesPK.IsEmpty);
		}

		public void TestImportFromInterchangeDetails()
		{
			var interchange = new Xsd.XmlInterchange();
			var declaration = Factory.New<BaseJobDeclaration>();
			var notify = new NotificationBuffer();

			var context = new ValueObjectImportContext(Factory, interchange, notify);
			adapter.ImportFromValueObject(declaration, new Xsd.ConsolAndShipment(), context);
			AssertEquals("Defaults to the current branch", Env.CurrentBranch.PK, declaration.JE_GB);
			AssertEquals("Notification should have warnings", false, notify.HasWarnings);

			interchange.InterchangeInfo.Target.BranchCode = "005";
			interchange.InterchangeInfo.Target.CountryCode = Env.CurrentCompany.Country.Code;
			notify.Clear();
			var consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Shipment.Declaration.Branch.Value = ZString.Empty;

			adapter.ImportFromValueObject(declaration, consolAndShipment, context);
			AssertEquals("Defaults to the current branch", Env.CurrentBranch.PK, declaration.JE_GB);
			AssertEquals("Notification should have warnings", true, notify.HasWarnings);
			AssertEquals("Notification error message", "Warning: Branch Code not found: 005", notify.Events[0].Message.Trim());

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "LEP";
			company.GC_Name = "Lep International";
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "005";
			branch1.GB_BranchName = "PAN ORIENT";

			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "006";
			branch2.GB_BranchName = "BOB BRANCH";

			notify.Clear();
			adapter.ImportFromValueObject(declaration, consolAndShipment, context);
			AssertEquals("Correct Branch is set", branch1.PK, declaration.JE_GB);
			AssertEquals("Notification should NOT have errors", false, notify.HasWarnings);

			consolAndShipment.Shipment.Declaration.Branch.Value = "006";
			notify.Clear();
			adapter.ImportFromValueObject(declaration, consolAndShipment, context);
			AssertEquals("Correct Branch is set", branch2.PK, declaration.JE_GB);
			AssertEquals("Notification should NOT have errors", false, notify.HasWarnings);
		}

		public virtual void TestImportBillContainerPacks()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.DisableDefaultPackingInformation = true;
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();

			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "MB1", "CONT 1", 11);
			consolAndShipment.Consol.ConsolDetail.Containers.Clear();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;

			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);
			Assert(notify.AsString.IndexOf("Could not import bill container packs because the container does not exist") < 0);

			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);
			Assert(notify.AsString.IndexOf("Could not find container number 'CONT 1' while importing bill container packs") >= 0);

			consolAndShipment.Shipment.Declaration.BillContainerPacks.Clear();
			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "MB1", "CONT 1", 11);

			AddBillsToDeclarationForPackTesting(jobDec, consolAndShipment);

			jobDec.PackingGroups.RemoveAndDeleteAll();
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);

			AssertPackageDetails(jobDec);
			BasePackingGroup package = jobDec.PackingGroups[0];
			AssertEquals("CONT 1", package.Container.CO_ContainerNumber);
		}

		public virtual void TestImportBillContainerPacksForEmptyContainerNo()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.DisableDefaultPackingInformation = true;
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();

			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "MB1", "", 11);
			consolAndShipment.Consol.ConsolDetail.Containers.Clear();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;

			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);
			Assert(notify.AsString.IndexOf("Could not find container number") <= 0);

			AddBillsToDeclarationForPackTesting(jobDec, consolAndShipment);

			jobDec.PackingGroups.RemoveAndDeleteAll();
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);

			Assert(notify.AsString.IndexOf("Could not find container number") <= 0);

			AssertPackageDetails(jobDec);
			BasePackingGroup package = jobDec.PackingGroups[0];
			AssertNull(package.Container);
		}

		public void TestImportMultiPackagesForOneBill()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.DisableDefaultPackingInformation = true;
			BaseCusContainer container1 = jobDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT 1";

			BaseCusContainer container2 = jobDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT 2";

			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();

			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "MB1", "CONT 1", 11);
			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "MB1", "CONT 2", 22);

			Bill bill = jobDec.Bills.AddNew();
			bill.CU_BillNum = "MB1";

			Bill houseBill = jobDec.Bills.AddNew();
			houseBill.CU_HouseBill = "HOUSEBILL 1";
			houseBill.CU_MasterBill = "MB1";

			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);

			AssertEquals("There should be only two packing groups", 2, jobDec.PackingGroups.Count);

			BasePackage package = jobDec.Packages[0];
			AssertEquals("HOUSEBILL 1", package.PackingGroup.Bill.CU_HouseBill);
			AssertEquals("MB1", package.PackingGroup.Bill.CU_MasterBill);
			AssertEquals("CONT 1", package.PackingGroup.Container.CO_ContainerNumber);
			AssertEquals(11, package.CW_PackQty);

			BasePackage package2 = jobDec.Packages[1];
			AssertEquals("HOUSEBILL 1", package2.PackingGroup.Bill.CU_HouseBill);
			AssertEquals("MB1", package2.PackingGroup.Bill.CU_MasterBill);
			AssertEquals("CONT 2", package2.PackingGroup.Container.CO_ContainerNumber);
			AssertEquals(22, package2.CW_PackQty);
		}

		public void TestImportBillContainerPacks_MasterBillShouldDefaultFromXsdConsol()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();

			Xsd.ConsolIdentifier consolIdentifier = consolAndShipment.Consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "MB1";
			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "", "CONT 1", 11);
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);
			BasePackingGroup package = jobDec.PackingGroups[0];
			AssertEquals("House bill should import", "HOUSEBILL 1", package.Bill.CU_HouseBill);
			AssertEquals("Masterbill should come from the xsd consol when it is blank on the Xsd.BillContainerPack", "MB1", package.Bill.CU_MasterBill);
		}

		public void TestImportBillContainerPacksWithMasterBillOnly()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();

			Xsd.ConsolIdentifier consolIdentifier = consolAndShipment.Consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "MB1";
			AddBillContainerPackToXml(consolAndShipment, "", "MB1", "CONT 1", 11);
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);
			BasePackingGroup package = jobDec.PackingGroups[0];
			AssertEquals("package is linked up to MasterBill", true, package.Bill.IsMasterBill);
			AssertEquals("PackingGroup should have been linked to master bill", "MB1", package.Bill.CU_BillNum);
			AssertEquals("HasErrors", false, buffer.HasErrors);
		}

		public void TestImportBondedWarehouseAddress()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			Xsd.ConsolAndShipment xsdDec = new Xsd.ConsolAndShipment();

			Xsd.Shipment xsdShipment = xsdDec.Shipment;
			xsdShipment.Declaration.BondedWarehouse.IsSpecified = true;
			xsdShipment.Declaration.BondedWarehouse.AddressSequenceRef = 1;
			Xsd.Organisation xsdOrg = new Xsd.Organisation();
			xsdOrg.OrganisationDetails.Name = "ORGNAME";
			Xsd.OrgAddress xsdAddress = xsdOrg.OrganisationDetails.Addresses.AddNew();
			xsdAddress.Sequence = 1;
			xsdAddress.AddressLine1 = "blah";
			xsdShipment.Declaration.BondedWarehouse.Organisation = xsdOrg;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(jobDec, xsdDec, context);
			AssertNotNull("WarehouseAddress", jobDec.WarehouseDocAddress.Address);
			AssertEquals("OA_Address1", "blah", jobDec.WarehouseDocAddress.Address.OA_Address1);
			var org = Factory.Load<OrgHeader>(jobDec.WarehouseDocAddress.Address.OA_OH);
			AssertNotNull("Org", org);
			AssertEquals("Org", "ORGNAM", org.OH_Code);
		}

		public void TestAddImportEvent()
		{
			Factory.Save();

			Xsd.ConsolAndShipment xsdDeclaration = new Xsd.ConsolAndShipment();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			BaseJobDeclaration importedDeclaration = adapter.CreateOrUpdateFromValueObject(xsdDeclaration, context);
			StmALog[] dataImportEvents = importedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to new declaration", 1, dataImportEvents.Length);

			Factory.Save();

			importedDeclaration = adapter.CreateOrUpdateFromValueObject(xsdDeclaration, context);
			dataImportEvents = importedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to NEW declaration", 1, dataImportEvents.Length);
		}

		public void TestAddImportEventWithReference()
		{
			Xsd.ConsolAndShipment xsdDeclaration = new Xsd.ConsolAndShipment();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			adapter.DIMReference = ZString.Empty;

			BaseJobDeclaration importedDeclaration = adapter.CreateOrUpdateFromValueObject(xsdDeclaration, context);

			StmALog[] dataImportEvents = importedDeclaration.Logs.Find(DIMEventQuery(adapter.DIMReference));

			AssertEquals("DIM event should be added to new declaration", 1, dataImportEvents.Length);

			Factory.Save();

			adapter.DIMReference = "ABC";

			importedDeclaration = adapter.CreateOrUpdateFromValueObject(xsdDeclaration, context);
			dataImportEvents = importedDeclaration.Logs.Find(DIMEventQuery(adapter.DIMReference));
			AssertEquals("DIM event should be added to NEW declaration", 1, dataImportEvents.Length);
		}

		public virtual void TestImportInvoiceHeader_PopulationOfJZ_CU_Housebill()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();

			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();

			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "MB1", "CONT 1", 11);
			consolAndShipment.Consol.ConsolDetail.Containers.Clear();
			consolAndShipment.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;

			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "MB1", "CONT 1", 11);
			Bill masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB1";

			Bill houseBill = jobDec.Bills.AddNew();
			houseBill.CU_HouseBill = "HOUSEBILL 1";
			houseBill.CU_MasterBill = "MB1";

			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 1", "MB2", "CONT 2", 12);
			masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB2";

			houseBill = jobDec.Bills.AddNew();
			houseBill.CU_HouseBill = "HOUSEBILL 1";
			houseBill.CU_MasterBill = "MB2";

			Xsd.InvoiceHeader xmlInvoiceHeader = consolAndShipment.Shipment.Invoices.AddNew();
			xmlInvoiceHeader.InvoiceNumber = "1";
			xmlInvoiceHeader.PackingDetails.Housebill = "HOUSEBILL 1";
			xmlInvoiceHeader.PackingDetails.Masterbill = "MB2";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);

			AssertEquals(1, jobDec.Invoices.Count);
			AssertEquals("1", jobDec.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("HOUSEBILL 1", jobDec.Invoices[0].Bill.CU_HouseBill);
			AssertEquals("MB2", jobDec.Invoices[0].Bill.CU_MasterBill);
		}

		public void TestImportCustomAttrubutes()
		{
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Shipment.ShipmentDetails.Custom.CustomAttribute1 = "CustAttrib1";
			consolAndShipment.Shipment.ShipmentDetails.Custom.CustomAttribute2 = "CustAttrib2";
			consolAndShipment.Shipment.ShipmentDetails.Custom.Date1 = new ZDateTime(2005, 11, 30, 15, 0, 0);
			consolAndShipment.Shipment.ShipmentDetails.Custom.Date2 = ZDateTime.Empty;

			consolAndShipment.Shipment.ShipmentDetails.Custom.Decimal1 = 21m;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Decimal2 = 86.45m;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Flag1 = Xsd.TrueFalse.@true;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Flag2 = Xsd.TrueFalse.@false;

			consolAndShipment.Shipment.ShipmentDetails.Custom.Decimal1Specified = true;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Decimal2Specified = true;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Flag1Specified = true;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Flag2Specified = true;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			BaseJobDeclaration jobDec = adapter.CreateOrUpdateFromValueObject(consolAndShipment, context);
			AssertEquals("Custom Attribute 1", "CustAttrib1", jobDec.DocsAndCartage.JP_CustomAttrib1);
			AssertEquals("Custom Attribute 2", "CustAttrib2", jobDec.DocsAndCartage.JP_CustomAttrib2);
			AssertEquals("Date 1", new ZDateTime(2005, 11, 30, 15, 0, 0), jobDec.DocsAndCartage.JP_CustomDate1);
			AssertEquals("Date 1", ZDateTime.Empty, jobDec.DocsAndCartage.JP_CustomDate2);
			AssertEquals("Decimal 1", 21m, jobDec.DocsAndCartage.JP_CustomDecimal1);
			AssertEquals("Decimal 2", 86.45m, jobDec.DocsAndCartage.JP_CustomDecimal2);
			AssertEquals("Flag 1", true, jobDec.DocsAndCartage.JP_CustomFlag1);
			AssertEquals("Flag 1", false, jobDec.DocsAndCartage.JP_CustomFlag2);

			consolAndShipment.Shipment.ShipmentDetails.Custom.Decimal1Specified = false;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Decimal2Specified = false;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Flag1Specified = false;
			consolAndShipment.Shipment.ShipmentDetails.Custom.Flag2Specified = false;

			jobDec = adapter.CreateOrUpdateFromValueObject(consolAndShipment, context);
			AssertEquals("Decimal 1", 0m, jobDec.DocsAndCartage.JP_CustomDecimal1);
			AssertEquals("Decimal 2", 0m, jobDec.DocsAndCartage.JP_CustomDecimal2);
			AssertEquals("Flag 1", false, jobDec.DocsAndCartage.JP_CustomFlag1);
			AssertEquals("Flag 1", false, jobDec.DocsAndCartage.JP_CustomFlag2);
		}

		public void TestImportShipmentType()
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Shipment.ShipmentDetails.ShipmentType = Xsd.ShipmentType.EXW;
			consolAndShipment.Shipment.ShipmentDetails.ShipmentTypeSpecified = true;
			jobDec = adapter.CreateOrUpdateFromValueObject(consolAndShipment, context);
			AssertEquals("MessageType should be ExWarehouse", JobMessageTypeList.Codes.ExWarehouse, jobDec.JE_MessageType);
			Assert("SetShipmentDetailsCoreWasExecuted should NOT have been executed", !adapter.SetShipmentDetailsCoreWasExecuted);

			consolAndShipment.Shipment.ShipmentDetails.ShipmentTypeSpecified = false;
			jobDec.JE_MessageType = ZString.Empty;
			jobDec = adapter.CreateOrUpdateFromValueObject(consolAndShipment, context);
			AssertEquals("MessageType should be MiscellaneousCustoms", JobMessageTypeList.Codes.MiscellaneousCustoms, jobDec.JE_MessageType);
			Assert("SetShipmentDetailsCoreWasExecuted should have been executed", adapter.SetShipmentDetailsCoreWasExecuted);
		}

		public void TestImportBillingImportWithShipment()
		{
			Xsd.ConsolAndShipment xmlDec = new Xsd.ConsolAndShipment();

			xmlDec.Shipment = new Xsd.Shipment();
			xmlDec.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			xmlDec.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			Xsd.ShipmentIdentifier shipmentIdentifier = xmlDec.Shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "ZA HBL3";

			xmlDec.Shipment.Billing = new Xsd.Billing();
			xmlDec.Shipment.ShipmentDetails.LocalClient = new Xsd.Organisation();
			xmlDec.Shipment.ShipmentDetails.LocalClient.OrganisationDetails.Name = "Local Client";
			xmlDec.Shipment.ShipmentDetails.LocalClient.IsSpecified = true;

			Xsd.ChargeLine chargeLine = xmlDec.Shipment.Billing.ChargeLines.AddNew();
			chargeLine.ChargeCode = "FRT";
			chargeLine.OSSellAmount.CurrencyCode = "ZAR";
			chargeLine.OSSellAmount.Value = 100M;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "ZA HBL3";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			JobHeader shipmentJobHeader = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			shipmentJobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentJobHeader.JH_ParentID = shipment.PK;
			shipmentJobHeader.JH_JobNum = shipment.JS_UniqueConsignRef;

			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			Factory.Save();

			CustomsDataRegistry.Instance.AllowBillingImportIntoDeclaration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);//this registry should be used
			adapter.ImportFromValueObject(declaration, xmlDec, context);

			Factory.Save();

			AssertNull(Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK)));

			var factory2 = new BusinessObjectFactory();
			var jobHeaderLoaded = factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull("Job Header", jobHeaderLoaded);

			var charges = factory2.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeaderLoaded.PK));
			AssertEquals("Should import billing charges", 1, charges.Length);

			CustomsDataRegistry.Instance.AllowBillingImportIntoDeclaration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);//this registry should be used
			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			xmlDec = new Xsd.ConsolAndShipment();

			xmlDec.Shipment = new Xsd.Shipment();
			xmlDec.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			xmlDec.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;

			xmlDec.Shipment.Billing = new Xsd.Billing();
			chargeLine = xmlDec.Shipment.Billing.ChargeLines.AddNew();
			chargeLine.ChargeCode = "FRT";
			chargeLine.OSSellAmount.CurrencyCode = "ZAR";
			chargeLine.OSSellAmount.Value = 100M;

			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			Factory.Save();

			adapter.ImportFromValueObject(declaration, xmlDec, context);

			Factory.Save();

			jobHeaderLoaded = factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull("Job Header", jobHeaderLoaded);
			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeaderLoaded.PK));
			AssertEquals("Should import billing charges", 1, charges.Length);
		}

		public void TestDoNotImportBillingInfoIfRegistrySaysSo()
		{
			CustomsDataRegistry.Instance.AllowBillingImportIntoDeclaration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);//this registry should be used

			var xmlDec = new Xsd.ConsolAndShipment();

			xmlDec.Shipment = new Xsd.Shipment();
			xmlDec.Shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			xmlDec.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;

			xmlDec.Shipment.Billing = new Xsd.Billing();
			xmlDec.Shipment.ShipmentDetails.LocalClient = new Xsd.Organisation();
			xmlDec.Shipment.ShipmentDetails.LocalClient.OrganisationDetails.Name = "Local Client";
			xmlDec.Shipment.ShipmentDetails.LocalClient.IsSpecified = true;

			var chargeLine = xmlDec.Shipment.Billing.ChargeLines.AddNew();
			chargeLine.ChargeCode = "FRT";
			chargeLine.OSSellAmount.CurrencyCode = "ZAR";
			chargeLine.OSSellAmount.Value = 100M;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			adapter.ImportFromValueObject(declaration, xmlDec, context);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var jobHeaderLoaded = factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNull("Should not create a job when the registry says so", jobHeaderLoaded);
		}

		public void TestExportingPaymentMethod()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_PaymentMethod = "B!D";
			Factory.Save();

			var xmlDec = adapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));
			if (declaration.SupportJE_PaymentMethodUsage)
			{
				AssertEquals("B!D", xmlDec.Shipment.Declaration.PaymentTerms);
			}
			else
			{
				AssertNotEquals("B!D", xmlDec.Shipment.Declaration.PaymentTerms);
			}
		}

		public void TestIncludeBillingInformationInXMLFile()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			JobHeader.Loader loader = new JobHeader.Loader(Factory, declaration);
			JobHeader header = loader.TryCreate();
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = header.PK;
			charge.JR_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery()).PK;

			Factory.Save();

			CustomsDataRegistry.Instance.IncludeBillingInformationInXMLFile.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Xsd.ConsolAndShipment xmlDec = adapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));

			Assert(!xmlDec.Shipment.Billing.IsSpecified);

			CustomsDataRegistry.Instance.IncludeBillingInformationInXMLFile.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			xmlDec = adapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));

			Assert(xmlDec.Shipment.Billing.IsSpecified);
			AssertEquals(1, xmlDec.Shipment.Billing.ChargeLines.Count);
		}

		public void TestExportDocAddresses_DeclarationWithoutShipment()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.Addresses.MainAddress.OA_Code = "importer address line1";
			importer.Addresses.MainAddress.OA_Address1 = "importer address line1";
			importer.Addresses.MainAddress.OA_Address2 = "importer address line2";
			importer.Addresses.MainAddress.OA_City = "Sydney";
			declaration.JE_OH_Importer = importer.PK;

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.Addresses.MainAddress.OA_Code = "supplier address line1";
			supplier.Addresses.MainAddress.OA_Address1 = "supplier address line1";
			supplier.Addresses.MainAddress.OA_Address2 = "supplier address line2";
			supplier.Addresses.MainAddress.OA_City = "Melburn";
			declaration.JE_OH_Supplier = supplier.PK;

			Factory.Save();

			var importerDeliveryAddress = declaration.DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress);
			importerDeliveryAddress.E2_AddressOverride = true;
			importerDeliveryAddress.E2_Address1 = "importer address line1";
			importerDeliveryAddress.E2_Address2 = "importer address line2";
			importerDeliveryAddress.E2_City = "Sydney";
			importerDeliveryAddress.E2_RN_NKCountryCode = ZString.Empty;

			Xsd.ConsolAndShipment value = new Xsd.ConsolAndShipment();
			adapter.ExportToValueObject(declaration, value, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(4, value.Shipment.ShipmentDetails.DocAddresses.DocAddress.Count);
			AssertEquals(1, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[0].AddressReference.AddressSequenceRef);
			AssertEquals(importer.OH_Code, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[0].AddressReference.Organisation.EDICode);
			AssertEquals(Xsd.DocAddressAddressType.IMD, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[0].AddressType);

			AssertEquals("importer address line1", value.Shipment.ShipmentDetails.DocAddresses.DocAddress[1].AddressLine1);
			AssertEquals("importer address line2", value.Shipment.ShipmentDetails.DocAddresses.DocAddress[1].AddressLine2);
			AssertEquals(Xsd.DocAddressAddressType.IMG, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[1].AddressType);
			AssertEquals("Sydney", value.Shipment.ShipmentDetails.DocAddresses.DocAddress[1].CityOrSuburb);

			AssertEquals(1, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[2].AddressReference.AddressSequenceRef);
			AssertEquals(supplier.OH_Code, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[2].AddressReference.Organisation.EDICode);
			AssertEquals(Xsd.DocAddressAddressType.SUD, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[2].AddressType);

			AssertEquals(1, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[3].AddressReference.AddressSequenceRef);
			AssertEquals(supplier.OH_Code, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[3].AddressReference.Organisation.EDICode);
			AssertEquals(Xsd.DocAddressAddressType.SUG, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[3].AddressType);

			declaration.JE_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			ErrorReporter.Clear();
			declaration.Shipment.DocAddresses.FindOrCreateWithRequirement(new JobDocAddressRequirement(DocAddressType.ConsigneeDocumentaryAddress, AddressType.OFC, ContactType.Consignee));
		}

		public void TestExportDocAddresses_DeclarationWithShipment()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.Addresses.MainAddress.OA_Code = "importer address line1";
			importer.Addresses.MainAddress.OA_Address1 = "importer address line1";
			importer.Addresses.MainAddress.OA_Address2 = "importer address line2";
			importer.Addresses.MainAddress.OA_City = "Sydney";

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.Addresses.MainAddress.OA_Code = "supplier address line1";
			supplier.Addresses.MainAddress.OA_Address1 = "supplier address line1";
			supplier.Addresses.MainAddress.OA_Address2 = "supplier address line2";
			supplier.Addresses.MainAddress.OA_City = "Melburn";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			declaration.Shipment.InsuredByDocAddress.E2_AddressOverride = true;
			declaration.Shipment.InsuredByDocAddress.E2_Address1 = "INSURER ADDRESSES";
			declaration.Shipment.InsuredByDocAddress.E2_RN_NKCountryCode = ZString.Empty;

			Factory.Save();

			Xsd.ConsolAndShipment value = new Xsd.ConsolAndShipment();
			adapter.ExportToValueObject(declaration, value, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(3, value.Shipment.ShipmentDetails.DocAddresses.DocAddress.Count);

			AssertEquals("importer address line1", value.Shipment.ShipmentDetails.DocAddresses.DocAddress[0].AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals(importer.OH_Code, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[0].AddressReference.Organisation.EDICode);
			AssertEquals("supplier address line1", value.Shipment.ShipmentDetails.DocAddresses.DocAddress[1].AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals(supplier.OH_Code, value.Shipment.ShipmentDetails.DocAddresses.DocAddress[1].AddressReference.Organisation.EDICode);
			AssertEquals("INSURER ADDRESSES", value.Shipment.ShipmentDetails.DocAddresses.DocAddress[2].AddressLine1);
		}

		public void TestExportMasterBill()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			Bill bill = jobDec.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB1";

			bill = jobDec.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB2";

			Xsd.Consol consol = new Xsd.Consol();
			adapter.ExportMasterbillDetails(consol, jobDec);
			Xsd.ConsolIdentifierCollection consolIdentifierCollection = consol.ConsolIdentifier.Find(Xsd.ConsolIdentifierType.MasterWaybill);
			AssertEquals(2, consolIdentifierCollection.Count);
			AssertEquals("MB1", consolIdentifierCollection[0].Value);
			AssertEquals("MB2", consolIdentifierCollection[1].Value);
		}

		public void TestExportMasterbillDetailsDoesNotExportDuplicates()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();

			Bill masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB1";

			masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB2";

			Bill houseBill = jobDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "MB1";
			houseBill.CU_HouseBill = "HB1";

			houseBill = jobDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "MB1";
			houseBill.CU_HouseBill = "HB1";

			houseBill = jobDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "MB2";
			houseBill.CU_HouseBill = "HB2";

			Xsd.Consol consol = new Xsd.Consol();
			adapter.ExportMasterbillDetails(consol, jobDec);
			Xsd.ConsolIdentifierCollection consolIdentifierCollection = consol.ConsolIdentifier.Find(Xsd.ConsolIdentifierType.MasterWaybill);
			AssertEquals(2, consolIdentifierCollection.Count);
			AssertEquals("MB1", consolIdentifierCollection[0].Value);
			AssertEquals("MB2", consolIdentifierCollection[1].Value);
		}

		public void TestExportHousebillDetails()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();

			Bill masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB1";

			masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB2";

			Bill houseBill = jobDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HB1";
			houseBill.CU_MasterBill = "MB1";

			houseBill = jobDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HB2";
			houseBill.CU_MasterBill = "MB2";

			Xsd.Shipment shipment = new Xsd.Shipment();
			adapter.ExportHousebillDetails(shipment, jobDec);
			Xsd.ShipmentIdentifierCollection shipmentIdentifierCollection = shipment.ShipmentIdentifier.Find(Xsd.ShipmentIdentifierType.Housebill);
			AssertEquals(2, shipmentIdentifierCollection.Count);
			AssertEquals("HB1", shipmentIdentifierCollection[0].Value);
			AssertEquals("MB1", shipmentIdentifierCollection[0].Masterbill);
			AssertEquals("HB2", shipmentIdentifierCollection[1].Value);
			AssertEquals("MB2", shipmentIdentifierCollection[1].Masterbill);
		}

		public void TestExportToValueObject()
		{
			Xsd.ConsolAndShipment consolAndShipment = adapter.ExportToValueObject(TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));

			AssertNotNull(consolAndShipment.Consol);
			AssertNotNull(consolAndShipment.Shipment);
		}

		public void TestExportDeclaration_LCLDates()
		{
			TestJobDec.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2005, 05, 25);
			TestJobDec.JE_TransportMode = "SEA";
			TestJobDec.DocsAndCartage.JP_FCLAvailable = ZDateTime.Empty;

			Xsd.Consol consol = new Xsd.Consol();

			adapter.ExportConsolValues(consol, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));

			Xsd.SailingWithVesselVoyage xmlVessel = consol.ConsolDetail.Item as Xsd.SailingWithVesselVoyage;

			AssertNotNull("Prereq", xmlVessel);

			AssertEquals(DateTime.MinValue, xmlVessel.FCLDates.AvailableDate);
			AssertEquals(false, xmlVessel.FCLDates.AvailableDate.IsValid);

			AssertEquals(new DateTime(2005, 05, 25), xmlVessel.LCLDates.AvailableDate);
			AssertEquals(true, xmlVessel.LCLDates.AvailableDate.IsValid);
		}

		public void TestExportConsolsValueObject()
		{
			Xsd.Consols consol = adapter.ExportConsolsValueObject(TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			AssertNotNull(consol.Consol);
		}

		public void TestExportInvoiceHeaders()
		{
			Xsd.Consols consol = adapter.ExportConsolsValueObject(TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.InvoiceHeaderCollection invoiceHeaders = consol.Consol[0].Shipments[0].Invoices;
			AssertEquals(2, invoiceHeaders.Count);

			Assert(invoiceHeaders[0].IsGroupInvoiceSpecified);
			AssertEquals(Xsd.TrueFalse.@true, invoiceHeaders[0].IsGroupInvoice);
			Assert(invoiceHeaders[1].IsGroupInvoiceSpecified);
			AssertEquals(Xsd.TrueFalse.@false, invoiceHeaders[1].IsGroupInvoice);
			AssertEquals("HOUSEBILL", invoiceHeaders[1].PackingDetails.Housebill);
			AssertEquals("MASTERBILL", invoiceHeaders[1].PackingDetails.Masterbill);
		}

		public void TestExportConsolValues()
		{
			TestJobDec.JE_MasterBill = "MASTERBILL";
			TestJobDec.JE_TransportMode = "SEA";
			TestJobDec.JE_VoyageFlightNo = "VoyageFlig";
			TestJobDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			TestJobDec.JE_RL_NKPortOfArrival = "MYPKG";
			TestJobDec.JE_RL_NKPortOfLoading = "AUMEL";
			TestJobDec.JE_AgentsReference = "JE098324098";
			Xsd.Consol toConsol = new Xsd.Consol();

			adapter.ExportConsolValues(toConsol, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(Xsd.ConsolIdentifierType.MasterWaybill, toConsol.ConsolIdentifier[0].ConsolIdentifierType);
			Assert(toConsol.ConsolIdentifier[0].ConsolIdentifierTypeSpecified);
			AssertEquals("MASTERBILL", toConsol.ConsolIdentifier[0].Value);

			AssertEquals(Xsd.ConsolTransportMode.SEA, toConsol.ConsolDetail.TransportMode);
			Assert(toConsol.ConsolDetail.TransportModeSpecified);
			AssertEquals("VoyageFlig", ((Xsd.SailingWithVesselVoyage)toConsol.ConsolDetail.Item).VoyageNo);
			AssertDTAF(toConsol);
			AssertEquals("MYPKG", toConsol.ConsolDetail.PortOfDischarge.Port.Value);
			AssertEquals("AUMEL", toConsol.ConsolDetail.PortOfLoading.Port.Value);
			AssertEquals("External AgentReference", "JE098324098", toConsol.ConsolDetail.ExternalAgentReference);
		}

		public void TestExportConsolValues_RAI()
		{
			TestJobDec.JE_MasterBill = "MASTERBILL";
			TestJobDec.JE_TransportMode = "RAI";
			TestJobDec.JE_VesselName = "JAYS";
			TestJobDec.JE_VoyageFlightNo = "0413";
			TestJobDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			TestJobDec.JE_RL_NKPortOfArrival = "MYPKG";
			TestJobDec.JE_RL_NKPortOfLoading = "AUMEL";
			TestJobDec.JE_AgentsReference = "JE098324098";
			Xsd.Consol toConsol = new Xsd.Consol();

			adapter.ExportConsolValues(toConsol, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(Xsd.ConsolTransportMode.RAI, toConsol.ConsolDetail.TransportMode);
			AssertEquals("0413", ((Xsd.FlightWithFlightNumber)toConsol.ConsolDetail.Item).FlightNoJourneyNoTruckRegNo);
		}

		public void TestExportDeclaration_FCLDates()
		{
			TestJobDec.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2005, 05, 25);
			TestJobDec.JE_TransportMode = "SEA";
			TestJobDec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			TestJobDec.DocsAndCartage.JP_LCLAvailable = ZDateTime.Empty;

			Xsd.Consol consol = new Xsd.Consol();

			adapter.ExportConsolValues(consol, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));

			Xsd.SailingWithVesselVoyage xmlVessel = consol.ConsolDetail.Item as Xsd.SailingWithVesselVoyage;

			AssertNotNull("Prereq", xmlVessel);

			AssertEquals(DateTime.MinValue, xmlVessel.LCLDates.AvailableDate);
			AssertEquals(false, xmlVessel.LCLDates.AvailableDate.IsValid);

			AssertEquals(new DateTime(2005, 05, 25), xmlVessel.FCLDates.AvailableDate);
			AssertEquals(true, xmlVessel.FCLDates.AvailableDate.IsValid);
			Assert("Container mode should have been specified in xsd", consol.ConsolDetail.ContainerModeSpecified);
			AssertEquals("Container mode in Xsd:", Xsd.ContainerMode.LCL, consol.ConsolDetail.ContainerMode);
		}

		public void TestExportConsolValues_InvalidTransportMode()
		{
			TestJobDec.JE_TransportMode = "BAD";

			Xsd.Consol toConsol = new Xsd.Consol();

			adapter.ExportConsolValues(toConsol, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Should default to SEA when transport mode not in Xsd.ConsolTransportMode", Xsd.ConsolTransportMode.SEA, toConsol.ConsolDetail.TransportMode);
			Assert("Should default to SEA when transport mode not in Xsd.ConsolTransportMode", toConsol.ConsolDetail.TransportModeSpecified);
		}

		public void TestExportConsolValues_SendingRecievingAgent()
		{
			Xsd.Consol toConsol = new Xsd.Consol();
			TestJobDec.JE_MessageType = ZString.Empty;
			adapter.ExportConsolValues(toConsol, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			Assert("SendingAgent not set", !toConsol.ConsolDetail.SendingAgent.IsSpecified);
			Assert("ReceivingAgent not set", !toConsol.ConsolDetail.ReceivingAgent.IsSpecified);

			toConsol = new Xsd.Consol();
			adapter.ExportConsolValues(toConsol, ImportJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			Assert("SendingAgent is set", toConsol.ConsolDetail.SendingAgent.IsSpecified);
			Assert("ReceivingAgent not set", !toConsol.ConsolDetail.ReceivingAgent.IsSpecified);

			toConsol = new Xsd.Consol();
			ExportJobDec.ResumeApportionment();
			adapter.ExportConsolValues(toConsol, ExportJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			Assert("SendingAgent not set", !toConsol.ConsolDetail.SendingAgent.IsSpecified);
			Assert("ReceivingAgent is set", toConsol.ConsolDetail.ReceivingAgent.IsSpecified);
		}

		public void TestExportConsolValues_Carrier()
		{
			Xsd.Consol toConsol = new Xsd.Consol();
			adapter.ExportConsolValues(toConsol, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			Assert("No carrier", !toConsol.ConsolDetail.Carrier.IsSpecified);

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			TestJobDec.JE_OH_ShippingLine = carrier.PK;
			adapter.ExportConsolValues(toConsol, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("CARRIER", toConsol.ConsolDetail.Carrier.EDICode);
		}

		public void TestExportShipmentValues()
		{
			TestJobDec.JE_DateAtOrigin = new ZDateTime(2005, 3, 17, 15, 57, 0);
			TestJobDec.JE_DateAtFinalDestination = new ZDateTime(2005, 3, 17, 15, 59, 0);
			StmNote note = TestJobDec.Notes.AddNew();
			note.ST_NoteText = "NOTETEXT";
			note.ST_Description = PredefinedNoteTypes.Instance.CustomsDeliveryInstructions.Description;
			note.ST_NoteType = nameof(StmNoteVisibility.PUB);

			Xsd.Shipment toShipment = new Xsd.Shipment();

			TestJobDec.JE_DeclarationReference = "S12345678";
			TestJobDec.JE_MessageSubType = "DEC";
			adapter.ExportShipmentValues(toShipment, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			TestJobDec.ResumeApportionment();
			Xsd.NotesNote xmlNote = toShipment.Notes.GetNote(Enterprise.DataTransfer.Xml.XsdVersion1.NotesNoteNoteType.CustomsDeliveryInstructions);
			AssertEquals("NOTETEXT", xmlNote.NoteData);
			AssertEquals("AgentReference", "S12345678", toShipment.ShipmentDetails.AgentReference);
			AssertEquals("DeclarationStyle", "DEC", toShipment.ShipmentDetails.DeclarationStyle);
			AssertEquals("Ser", toShipment.ShipmentDetails.ServiceLevel);
			AssertEquals(Xsd.TransportMode.SEA, toShipment.ShipmentDetails.TransportMode);
			AssertEquals("MarksAndNumbersShort", toShipment.ShipmentDetails.MarksAndNumbers);
			AssertEquals(Xsd.ShipmentIdentifierType.Housebill, toShipment.ShipmentIdentifier[0].ShipmentIdentifierType);
			AssertEquals("HOUSEBILL", toShipment.ShipmentIdentifier[0].Value);

			AssertContains(toShipment.ShipmentDetails.PortOfOrigin.Port.Value, TestJobDec.GetPotentialOriginPortNames());
			AssertEquals(new DateTime(2005, 3, 17, 15, 57, 0), toShipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime);
			Assert(toShipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime.IsValid);

			AssertEquals(TestJobDec.JE_RL_NKFinalDestination, toShipment.ShipmentDetails.PortofDestination.Port.Value);
			AssertEquals(new DateTime(2005, 3, 17, 15, 59, 0), toShipment.ShipmentDetails.PortofDestination.EstimatedDateTime);
			Assert(toShipment.ShipmentDetails.PortofDestination.EstimatedDateTime.IsValid);

			AssertEquals("GOODSDESCRIPTION", toShipment.ShipmentDetails.GoodsDescription.ToUpper());
			AssertEquals("OwnerRef", toShipment.ShipmentDetails.OwnerReference);
			AssertEquals(240m, toShipment.ShipmentDetails.Weight.Value);
			AssertEquals("To", toShipment.ShipmentDetails.Weight.DimensionType);
			AssertEquals(229m, toShipment.ShipmentDetails.Volume.Value);
			AssertEquals(0m, toShipment.ShipmentDetails.ChargeableWeight.Value);
			AssertEquals("To", toShipment.ShipmentDetails.Volume.DimensionType);
			AssertEquals(101m, toShipment.ShipmentDetails.TotalOuterPacksQty.Value);
			AssertEquals("PKG", toShipment.ShipmentDetails.TotalOuterPacksQty.DimensionType);
			AssertEquals(58m, toShipment.ShipmentDetails.TotalInnerPacksQty.Value);
			AssertEquals("PKG", toShipment.ShipmentDetails.TotalInnerPacksQty.DimensionType);
			AssertEquals("Shi", toShipment.ShipmentDetails.Incoterm);

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LOCALCLIENT";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address = localClient.Addresses.MainAddress;
			address.OA_Address1 = "X435E3R6PK226SDFZ7TTNXEH7GX0EM2HRJ13NDJK423980";

			JobHeader jobHeader = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, TestJobDec.PK).AddToFilter(JobHeaderSchema.JH_GC, TestJobDec.CompanyPK));
			if (jobHeader != null)
			{
				var staffAssignment = localClient.StaffAssignments.AddNew();
				staffAssignment.O8_GS_NKPersonResponsible = jobHeader.JH_GS_NKRepSales;
				staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
				jobHeader.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			}
			adapter.ExportShipmentValues(toShipment, TestJobDec, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("JA", toShipment.ShipmentDetails.SalesRep);
			AssertEquals("LOCALCLIENT", toShipment.ShipmentDetails.LocalClient.EDICode);

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_ActualChargeable = 245m;
			TestJobDec.JE_JS = shipment.PK;
		}

		public void TestExportShipmentValues_BookingReferenceAndExportStm()
		{
			Xsd.Shipment toShipment = new Xsd.Shipment();
			adapter.ExportShipmentValues(toShipment, TestJobDec, new ValueObjectExportContext(Notify));
			AssertEquals("Standalone BookingReference", string.Empty, toShipment.ShipmentDetails.BookingReference);
			AssertEquals("ExporterStatement", string.Empty, toShipment.ShipmentDetails.ExporterStatement);

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_BookingReference = "BK12345678";
			TestJobDec.JE_JS = shipment.PK;
			TestJobDec.Shipment.DocsAndCartage.JP_ExportStatement = "STM";
			adapter.ExportShipmentValues(toShipment, TestJobDec, new ValueObjectExportContext(Notify));
			AssertEquals("Linked BookingReference", "BK12345678", toShipment.ShipmentDetails.BookingReference);
			AssertEquals("ExporterStatement", "STM", toShipment.ShipmentDetails.ExporterStatement);
		}

		public void TestExportShipmentValues_TEU()
		{
			Xsd.Shipment toShipment = new Xsd.Shipment();
			adapter.ExportShipmentValues(toShipment, TestJobDec, new ValueObjectExportContext(Notify));
			Assert("No TEU", !toShipment.ShipmentDetails.TEUSpecified);
			AssertEquals("Zero TEU", ZDecimal.Zero, toShipment.ShipmentDetails.TEU);

			TestJobDec.CusContainers.AddNew();
			TestJobDec.CusContainers[0].CO_RC = Factory.New<RefContainer>().PK;
			TestJobDec.CusContainers[0].CO_ContainerNumber = "CONTNR1";
			TestJobDec.CusContainers[0].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			TestJobDec.CusContainers[0].Container.RC_TEU = 2.3m;
			TestJobDec.CusContainers.AddNew();
			TestJobDec.CusContainers[1].CO_RC = Factory.New<RefContainer>().PK;
			TestJobDec.CusContainers[1].CO_ContainerNumber = "CONTNR2";
			TestJobDec.CusContainers[1].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			TestJobDec.CusContainers[1].Container.RC_TEU = 3.4m;
			TestJobDec.CusContainers.AddNew();
			TestJobDec.CusContainers[2].CO_RC = Factory.New<RefContainer>().PK;
			TestJobDec.CusContainers[2].CO_ContainerNumber = "CONTNR3";
			TestJobDec.CusContainers[2].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			TestJobDec.CusContainers[2].Container.RC_TEU = 4.5m;
			adapter.ExportShipmentValues(toShipment, TestJobDec, new ValueObjectExportContext(Notify));
			AssertEquals("TEU", 5.7m, toShipment.ShipmentDetails.TEU);
		}

		public void TestExportContainers()
		{
			BaseJobDeclaration jobDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseCusContainer cusContainer = jobDec.CusContainers.AddNew();
			cusContainer.CO_Seal = "SEAL";
			cusContainer.CO_SecondSeal = "SEAL2";
			cusContainer.CO_Weight = 12.33m;
			cusContainer.CO_ContainerNumber = "3CC111A7-747";

			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.RC_ISOType = "9510";
			refContainer.RC_Length = 45.000M;
			refContainer.RC_Width = 8.000M;
			refContainer.RC_Height = 9.000M;
			refContainer.RC_Code = "99AA";
			cusContainer.CO_RC = refContainer.PK;

			CommonContainer freightContainer = Factory.NewWithValidTestData<CommonContainer>();
			cusContainer.CO_JC = freightContainer.PK;

			#region Export Process
			freightContainer.JC_EmptyRequired = testDate.AddDays(20);
			freightContainer.JC_DepartureEstimatedPickup = testDate.AddDays(21);
			freightContainer.JC_DepartureCartageAdvised = testDate.AddDays(22);
			freightContainer.JC_DepartureSlotReference = "export slot ref";
			freightContainer.JC_DepartureSlotDateTime = testDate.AddDays(23);
			freightContainer.JC_DepartureCartageRef = "export cart ref";
			freightContainer.JC_ContainerYardEmptyPickupGateOut = testDate.AddDays(24);
			freightContainer.JC_FCLWharfGateIn = testDate.AddDays(25);
			freightContainer.JC_DepartureCartageComplete = testDate.AddDays(26);
			freightContainer.JC_FCLOnBoardVessel = testDate.AddDays(27);
			freightContainer.DepartureTruckWaitTime = testDate.AddDays(28);
			freightContainer.DepartureTruckWaitCost = 29M;

			freightContainer.JC_DepartureDeliveryByRail = ZBool.True;

			OrgHeader pickupFromOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupFromOrg.OH_FullName = "pickup from org";
			OrgAddress pickupFromOrgAddress = pickupFromOrg.Addresses.AddNew(OrgAddressType.Office, true);
			pickupFromOrgAddress.OA_Address1 = "pickup from addr 1";
			freightContainer.JC_OA_DepartureContainerYardAddress = pickupFromOrgAddress.PK;
			#endregion

			#region Import Process
			freightContainer.JC_FCLAvailable = testDate.AddDays(1);
			freightContainer.JC_ArrivalCTOStorageStartDate = testDate.AddDays(2);
			freightContainer.JC_LCLAvailable = testDate.AddDays(3);
			freightContainer.JC_LCLStorageCommences = testDate.AddDays(4);
			freightContainer.JC_FCLUnloadFromVessel = testDate.AddDays(5);
			freightContainer.JC_ArrivalSlotDateTime = testDate.AddDays(6);
			freightContainer.JC_ArrivalSlotReference = "import slot ref";
			freightContainer.JC_ArrivalCartageRef = "import cart ref";
			freightContainer.JC_FCLWharfGateOut = testDate.AddDays(7);
			freightContainer.JC_ArrivalEstimatedDelivery = testDate.AddDays(8);
			freightContainer.JC_ArrivalCartageAdvised = testDate.AddDays(9);
			freightContainer.JC_ArrivalCartageComplete = testDate.AddDays(10);
			freightContainer.JC_EmptyReadyForReturn = testDate.AddDays(11);
			freightContainer.JC_EmptyReturnedBy = testDate.AddDays(12);
			freightContainer.JC_ContainerYardEmptyReturnGateIn = testDate.AddDays(13);

			freightContainer.JC_FCLHeldInTransitStaging = false;

			freightContainer.ArrivalCTOStorageDays = 11;
			freightContainer.ArrivalCTOStorageCost = 11M;
			freightContainer.ArrivalTruckWaitTime = testDate.AddDays(14);
			freightContainer.ArrivalTruckWaitCost = 14M;
			freightContainer.ArrivalCarrierDetentionDays = 12;
			freightContainer.ArrivalCarrierDetentionCost = 12M;

			OrgHeader deliverEmptyToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverEmptyToOrg.OH_FullName = "deliver empty to org";
			OrgAddress deliverEmptyToOrgAddress = deliverEmptyToOrg.Addresses.AddNew(OrgAddressType.Office, true);
			deliverEmptyToOrgAddress.OA_Address1 = "deliver empty to add 1";
			freightContainer.JC_OA_ArrivalContainerYardAddress = deliverEmptyToOrgAddress.PK;
			#endregion

			#region Custom Fields
			cusContainer.CO_CustomAttrib1 = "blah";
			cusContainer.CO_CustomDate1 = testDate;
			cusContainer.CO_CustomDecimal1 = 10M;
			cusContainer.CO_CustomFlag1 = false;
			#endregion

			Xsd.ConsolConsolDetail consolDetail = new Xsd.ConsolConsolDetail();
			adapter.ExportContainers(consolDetail, jobDec, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.Container xsdContainer = consolDetail.Containers[0];

			AssertEquals("3CC111A7-747", xsdContainer.ContainerNumber);
			AssertEquals(9m, xsdContainer.ContainerType.Height);
			Assert(xsdContainer.ContainerType.HeightSpecified);
			AssertEquals("9510", xsdContainer.ContainerType.ISOCode);
			AssertEquals(45m, xsdContainer.ContainerType.Length);
			Assert(xsdContainer.ContainerType.LengthSpecified);
			AssertEquals(8m, xsdContainer.ContainerType.Width);
			Assert(xsdContainer.ContainerType.WidthSpecified);
			AssertEquals(Xsd.ContainerMode.FCL, xsdContainer.PackingMode);
			AssertEquals("SEAL", xsdContainer.Seal);
			AssertEquals("SEAL2", xsdContainer.Seal2);
			AssertEquals(12.33m, xsdContainer.Weight);
			AssertEquals("Container Code", "99AA", xsdContainer.ContainerType.ContainerCode);

			#region Export Process
			AssertEquals("EmptyRequiredBy", freightContainer.JC_EmptyRequired, xsdContainer.ExportProcess.EmptyRequiredBy);
			AssertEquals("EstimatedFullPickup", freightContainer.JC_DepartureEstimatedPickup, xsdContainer.ExportProcess.EstimatedFullPickup);
			AssertEquals("CartageAdvised", freightContainer.JC_DepartureCartageAdvised, xsdContainer.ExportProcess.CartageAdvised);
			AssertEquals("SlotBookingRef", freightContainer.JC_DepartureSlotReference, xsdContainer.ExportProcess.SlotBookingRef);
			AssertEquals("SlotDate", freightContainer.JC_DepartureSlotDateTime, xsdContainer.ExportProcess.SlotDate);
			AssertEquals("CartageRef", freightContainer.JC_DepartureCartageRef, xsdContainer.ExportProcess.CartageRef);
			AssertEquals("ContainerYardGateOut", freightContainer.JC_ContainerYardEmptyPickupGateOut, xsdContainer.ExportProcess.ContainerYardGateOut);
			AssertEquals("WharfGateIn", freightContainer.JC_FCLWharfGateIn, xsdContainer.ExportProcess.WharfGateIn);
			AssertEquals("CartageComplete", freightContainer.JC_DepartureCartageComplete, xsdContainer.ExportProcess.CartageComplete);
			AssertEquals("ShippedOnboard", freightContainer.JC_FCLOnBoardVessel, xsdContainer.ExportProcess.ShippedOnboard);
			AssertEquals("DemurrageTime", freightContainer.DepartureTruckWaitTime, xsdContainer.ExportProcess.DemurrageTime);
			AssertEquals("DemurrageCharge", freightContainer.DepartureTruckWaitCost, xsdContainer.ExportProcess.DemurrageCharge);

			Assert(xsdContainer.ExportProcess.IsArrivingAtCTOByRailSpecified);
			AssertEquals("IsArrivingAtCTOByRail", freightContainer.JC_DepartureDeliveryByRail, xsdContainer.ExportProcess.IsArrivingAtCTOByRail);

			AssertEquals("Pick up from Org", pickupFromOrg.OH_FullName, xsdContainer.ExportProcess.PickupEmptyFrom.Organisation.OrganisationDetails.Name);
			AssertNotNull(xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0]);
			AssertEquals("Pick up from address", pickupFromOrgAddress.OA_Address1, xsdContainer.ExportProcess.PickupEmptyFrom.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			#endregion

			#region Import Process
			AssertEquals("FCLAvailable", freightContainer.JC_FCLAvailable, xsdContainer.ImportProcess.FCLAvailable);
			AssertEquals("FCLStorage", freightContainer.JC_ArrivalCTOStorageStartDate, xsdContainer.ImportProcess.FCLStorage);
			AssertEquals("LCLAvailable", freightContainer.JC_LCLAvailable, xsdContainer.ImportProcess.LCLAvailable);
			AssertEquals("LCLStorage", freightContainer.JC_LCLStorageCommences, xsdContainer.ImportProcess.LCLStorage);
			AssertEquals("WharfUnload", freightContainer.JC_FCLUnloadFromVessel, xsdContainer.ImportProcess.WharfUnload);
			AssertEquals("SlotBookingRef", freightContainer.JC_ArrivalSlotDateTime, xsdContainer.ImportProcess.SlotDate);
			AssertEquals("SlotDate", freightContainer.JC_ArrivalSlotReference, xsdContainer.ImportProcess.SlotBookingRef);
			AssertEquals("CartageRef", freightContainer.JC_ArrivalCartageRef, xsdContainer.ImportProcess.CartageRef);
			AssertEquals("WharfGateOut", freightContainer.JC_FCLWharfGateOut, xsdContainer.ImportProcess.WharfGateOut);
			AssertEquals("EstimatedDelivery", freightContainer.JC_ArrivalEstimatedDelivery, xsdContainer.ImportProcess.EstimatedDelivery);
			AssertEquals("CartageAdvised", freightContainer.JC_ArrivalCartageAdvised, xsdContainer.ImportProcess.CartageAdvised);
			AssertEquals("CartageComplete", freightContainer.JC_ArrivalCartageComplete, xsdContainer.ImportProcess.CartageComplete);

			AssertEquals("EmptyReady", freightContainer.JC_EmptyReadyForReturn, xsdContainer.ImportProcess.EmptyReady);
			AssertEquals("EmptyReturnRequiredBy", freightContainer.JC_EmptyReturnedBy, xsdContainer.ImportProcess.EmptyReturnRequiredBy);
			AssertEquals("EmptyReturnedOn", freightContainer.JC_ContainerYardEmptyReturnGateIn, xsdContainer.ImportProcess.EmptyReturnedOn);

			Assert(xsdContainer.ImportProcess.PickupByRailSpecified);
			AssertEquals("PickupByRail", freightContainer.JC_ArrivalPickupByRail, xsdContainer.ImportProcess.PickupByRail);
			Assert(xsdContainer.ImportProcess.HeldForFCLTransitStagingSpecified);
			AssertEquals("HeldForFCLTransitStaging", freightContainer.JC_FCLHeldInTransitStaging, xsdContainer.ImportProcess.HeldForFCLTransitStaging);

			AssertEquals("StorageDays", freightContainer.ArrivalCTOStorageDays.ToString(), xsdContainer.ImportProcess.StorageDays);
			AssertEquals("StorageCharge", freightContainer.ArrivalCTOStorageCost, xsdContainer.ImportProcess.StorageCharge);
			AssertEquals("DemurrageTime", freightContainer.ArrivalTruckWaitTime, xsdContainer.ImportProcess.DemurrageTime);
			AssertEquals("DemurrageCharge", freightContainer.ArrivalTruckWaitCost, xsdContainer.ImportProcess.DemurrageCharge);
			AssertEquals("DetentionDays", freightContainer.ArrivalCarrierDetentionDays.ToString(), xsdContainer.ImportProcess.DetentionDays);
			AssertEquals("DetentionCharge", freightContainer.ArrivalCarrierDetentionCost, xsdContainer.ImportProcess.DetentionCharge);

			AssertEquals("Deliver Empty to Org", deliverEmptyToOrg.OH_FullName, xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Name);
			AssertNotNull(xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0]);
			AssertEquals("Deliver Empty to address", deliverEmptyToOrgAddress.OA_Address1, xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			#endregion

			#region Custom Fields
			AssertEquals(cusContainer.CO_CustomAttrib1, xsdContainer.Custom.CustomAttribute1);
			AssertEquals(cusContainer.CO_CustomDate1, xsdContainer.Custom.Date1);
			Assert(xsdContainer.Custom.Decimal1Specified);
			AssertEquals(cusContainer.CO_CustomDecimal1, xsdContainer.Custom.Decimal1);
			Assert(xsdContainer.Custom.Flag1Specified);
			AssertEquals(cusContainer.CO_CustomFlag1, xsdContainer.Custom.Flag1);
			#endregion
		}

		public virtual void TestExportBillContainerPacks()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();

			AddPackageToJobDec(jobDec, "HOUSEBILL 1", "MB1", "000001", 1);
			AddPackageToJobDec(jobDec, "HOUSEBILL 2", "MB1", "000002", 2);

			Xsd.ConsolAndShipment consolAndShipment = adapter.ExportToValueObject(jobDec, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(2, consolAndShipment.Shipment.Declaration.BillContainerPacks.Count);
			Xsd.DeclarationBillContainerPack billContainerPack = consolAndShipment.Shipment.Declaration.BillContainerPacks[0];
			AssertEquals("HOUSEBILL 1", billContainerPack.BillNumber);
			AssertEquals("MB1", billContainerPack.MasterbillNumber);
			AssertEquals("000001", billContainerPack.ContainerNumber);
			AssertEquals(1M, billContainerPack.PackQty.Value);

			billContainerPack = consolAndShipment.Shipment.Declaration.BillContainerPacks[1];
			AssertEquals("HOUSEBILL 2", billContainerPack.BillNumber);
			AssertEquals("MB1", billContainerPack.MasterbillNumber);
			AssertEquals("000002", billContainerPack.ContainerNumber);
			AssertEquals(2M, billContainerPack.PackQty.Value);
		}

		[ExpectNoExceptions]
		public void TestExportBillContainerPacks_NoBill()
		{
			var jobDec = Factory.New<BaseJobDeclaration>();

			AddPackageToJobDec(jobDec, "HOUSEBILL 1", "MB1", "000001", 1);
			jobDec.PackingGroups[0].CR_CU_HouseBill = ZGuid.Empty;
			adapter.ExportToValueObject(jobDec, new ValueObjectExportContext(new NotificationBuffer()));
		}

		[ExpectNoExceptions()]
		public virtual void TestExportBillContainerPacks_NoContainer()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();

			AddPackageToJobDec(jobDec, "HOUSEBILL 1", "MB1", "000001", 1);
			jobDec.PackingGroups[0].CR_CO_Container = ZGuid.Empty;

			Xsd.ConsolAndShipment consolAndShipment = adapter.ExportToValueObject(jobDec, new ValueObjectExportContext(new NotificationBuffer()));
		}

		public void TestExportBondedWarehouseAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZORG";
			org.OH_FullName = "ORGNAME";
			org.Addresses[0].OA_Address1 = "blah";

			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.WarehouseDocAddress.E2_OA_Address = org.Addresses[0].PK;

			Xsd.ConsolAndShipment xsdDec = adapter.ExportToValueObject(jobDec, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("BondedWarehouse.IsSpecified", true, xsdDec.Shipment.Declaration.BondedWarehouse.IsSpecified);
			AssertEquals("AddressSequenceRef", 1, xsdDec.Shipment.Declaration.BondedWarehouse.AddressSequenceRef);
			Xsd.Organisation xsdOrg = xsdDec.Shipment.Declaration.BondedWarehouse.Organisation;
			AssertEquals("EDICode", "XYZORG", xsdOrg.EDICode);
			AssertEquals("OrganisationName", "ORGNAME", xsdOrg.OrganisationDetails.Name);
			AssertEquals("Addresses.Count", 1, xsdOrg.OrganisationDetails.Addresses.Count);
			AssertEquals("AddressLine1", "blah", xsdOrg.OrganisationDetails.Addresses[0].AddressLine1);
		}

		public void TestAddExportEvent()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			StmALog[] dataExportEvents = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("No DEX event should be added to declaration", 0, dataExportEvents.Length);

			adapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));
			dataExportEvents = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to declaration", 1, dataExportEvents.Length);

			adapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));
			dataExportEvents = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to declaration", 2, dataExportEvents.Length);
		}

		public void TestExportCustomAttributes()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.DocsAndCartage.JP_CustomAttrib1 = "CustAttrib1";
			jobDec.DocsAndCartage.JP_CustomAttrib2 = "CustAttrib2";
			jobDec.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2005, 11, 3);
			jobDec.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2005, 11, 30);
			jobDec.DocsAndCartage.JP_CustomDecimal1 = 23m;
			jobDec.DocsAndCartage.JP_CustomDecimal2 = 86m;
			jobDec.DocsAndCartage.JP_CustomFlag1 = true;
			jobDec.DocsAndCartage.JP_CustomFlag2 = false;

			Xsd.ConsolAndShipment result = adapter.ExportToValueObject(jobDec, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Custom Attribute 1", "CustAttrib1", result.Shipment.ShipmentDetails.Custom.CustomAttribute1);
			AssertEquals("Custom Attribute 2", "CustAttrib2", result.Shipment.ShipmentDetails.Custom.CustomAttribute2);
			AssertEquals("Custom Date 1", new ZDateTime(2005, 11, 3), result.Shipment.ShipmentDetails.Custom.Date1);
			AssertEquals("Custom Date 2", new ZDateTime(2005, 11, 30), result.Shipment.ShipmentDetails.Custom.Date2);
			AssertEquals("Custom Decimal 1", 23m, result.Shipment.ShipmentDetails.Custom.Decimal1);
			AssertEquals("Custom Decimal 2", 86m, result.Shipment.ShipmentDetails.Custom.Decimal2);
			AssertEquals("Custom Flag 1", Xsd.TrueFalse.@true, result.Shipment.ShipmentDetails.Custom.Flag1);
			AssertEquals("Custom Flag 2", Xsd.TrueFalse.@false, result.Shipment.ShipmentDetails.Custom.Flag2);

			AssertEquals("Custom Decimal 1 Should Be specified", true, result.Shipment.ShipmentDetails.Custom.Decimal1Specified);
			AssertEquals("Custom Decimal 2 Should Be specified", true, result.Shipment.ShipmentDetails.Custom.Decimal2Specified);
			AssertEquals("Custom Flat 1 Should be specified", true, result.Shipment.ShipmentDetails.Custom.Flag1Specified);
			AssertEquals("Custom Flat 2 Should be specified", true, result.Shipment.ShipmentDetails.Custom.Flag1Specified);
		}

		public void TestExportShipmentType()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Xsd.ConsolAndShipment result = adapter.ExportToValueObject(jobDec, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("MessageType/ShipmentType should be EXW", Xsd.ShipmentType.EXW, result.Shipment.ShipmentDetails.ShipmentType);
			Assert("ShipmentType should be specified", result.Shipment.ShipmentDetails.ShipmentTypeSpecified);

			jobDec.JE_MessageType = ZString.Empty;
			result = adapter.ExportToValueObject(jobDec, new ValueObjectExportContext(new NotificationBuffer()));
			Assert("ShipmentType should NOT be specified", !result.Shipment.ShipmentDetails.ShipmentTypeSpecified);
		}

		public void TestExportPreAdviceIdentifier()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();
			preadvice.EF_JE = dec.PK;

			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(Xsd.Consol));
			MemoryStream stream = new MemoryStream();
			serializer.ExportXmlData(stream, adapter, new BusinessObject[] { dec }, new ValueObjectExportContext(new NotificationBuffer()));
			stream.Position = 0;
			string xml = new StreamReader(stream).ReadToEnd();
			AssertEquals("Pre-advice identifier included in the xml", true, xml.Contains("<ShipmentIdentifier ShipmentIdentifierType=\"PreadviceIdentifier\" />"));
		}

		public void TestExportLandedCostingHeadings()
		{
			if (IsExportToValueObjectSupported)
			{
				var collection = new LandedCostingGroupCollection();
				var landedCostingGroup1 = collection.AddNew();
				landedCostingGroup1.GroupID = 1;
				landedCostingGroup1.GroupName = "ONE";
				landedCostingGroup1.CostDistributionCode = CostDistributionMechanismList.Codes.Actual;

				var landedCostingGroup2 = collection.AddNew();
				landedCostingGroup2.GroupID = 2;
				landedCostingGroup2.GroupName = "TWO";
				landedCostingGroup2.CostDistributionCode = CostDistributionMechanismList.Codes.Item;

				var landedCostingGroup3 = collection.AddNew();
				landedCostingGroup3.GroupID = 3;
				landedCostingGroup3.GroupName = "THREE";
				landedCostingGroup3.CostDistributionCode = CostDistributionMechanismList.Codes.ActualVolume;

				FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

				var processedTime = ZDateTime.Now;
				var landedCostingHeadings = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.LandedCosting.ILandedCostHeader)));
				landedCostingHeadings[LandedCostHeaderSchema.LT_DateOfProcessing] = processedTime;
				landedCostingHeadings[LandedCostHeaderSchema.LT_ParentID] = TestJobDec.PK;
				landedCostingHeadings[LandedCostHeaderSchema.LT_ParentTableCode] = JobDeclarationSchema.Constants.Prefix;
				landedCostingHeadings[LandedCostHeaderSchema.LT_LandedCostType] = "ACT";

				var invoiceLine = TestJobDec.InvoiceLines[0];

				var landedCostingHistory = ((IBusinessObjectCollection)landedCostingHeadings["Histories"]).AddNew();
				landedCostingHistory[LandedCostHistorySchema.LH_ParentID] = invoiceLine.PK;
				landedCostingHistory[LandedCostHistorySchema.LH_ParentTableCode] = JobComInvoiceLineSchema.Constants.Prefix;

				var accChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
				var refCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery());
				var commonJobComInvoiceHeader = Factory.LoadTop1<CommonJobComInvoiceHeader>(new ZQuery());

				var landedCostInput = ((BusinessObjectCollection)landedCostingHeadings["CostInputs"]).AddNew();
				landedCostInput[LandCostInputSchema.LI_ParentID] = commonJobComInvoiceHeader.PK;
				landedCostInput["LinkedObjectUniqueCode"] = ((ILandedCostDistributeTo)commonJobComInvoiceHeader).UniqueCode;
				landedCostInput[LandCostInputSchema.LI_ParentTableCode] = JobComInvoiceHeaderSchema.Constants.Prefix;
				landedCostInput[LandCostInputSchema.LI_AC_ChargeCode] = accChargeCode.PK;
				landedCostInput[LandCostInputSchema.LI_RX_NKCostCurrency] = refCurrency.RX_Code;
				landedCostInput["LCGroupString"] = "1";
				landedCostInput[LandCostInputSchema.LI_ChargeDescription] = "Charge Description";
				landedCostInput[LandCostInputSchema.LI_DistributeCostBy] = "VOL";
				landedCostInput[LandCostInputSchema.LI_CostAmount] = new ZDecimal(1234);
				landedCostInput[LandCostInputSchema.LI_ServiceExRate] = new ZDecimal(3);

				Factory.Save();

				var declarationXml = new Xsd.Declaration();
				var buffer = new NotificationBuffer();

				adapter.ExportLandedCostingHeadings(declarationXml, ImportJobDec, new ValueObjectExportContext(buffer));

				AssertEquals("declaration is specified", true, declarationXml.IsSpecified);
				AssertEquals("landedcosting header is specified", true, declarationXml.LandedCostingHeader.IsSpecified);
				AssertEquals("Data of landed costing", processedTime, declarationXml.LandedCostingHeader.LandedCostingDate);

				var customsChargeLCItemSettings = ((ILandedCostHistoryMaster)landedCostingHeadings).CustomsChargeLCItemSettings;

				AssertEquals("Special Tax Label 1", true, AssertSpecialTaxHeadings(declarationXml.LandedCostingHeader.SpecialTaxHeadings, customsChargeLCItemSettings.FirstOrDefault(x => x.CostType == "ST1")?.Description ?? ZString.Empty, 1));
				AssertEquals("Special Tax Label 2", true, AssertSpecialTaxHeadings(declarationXml.LandedCostingHeader.SpecialTaxHeadings, customsChargeLCItemSettings.FirstOrDefault(x => x.CostType == "ST2")?.Description ?? ZString.Empty, 2));
				AssertEquals("Special Tax Label 3", true, AssertSpecialTaxHeadings(declarationXml.LandedCostingHeader.SpecialTaxHeadings, customsChargeLCItemSettings.FirstOrDefault(x => x.CostType == "ST3")?.Description ?? ZString.Empty, 3));

				AssertEquals("Grp 1", true, AssertGroupHeadings(declarationXml.LandedCostingHeader.LandedCostingGroupHeaders, "ONE", CostDistributionMechanismList.Codes.Actual, 1));
				AssertEquals("Grp 2", true, AssertGroupHeadings(declarationXml.LandedCostingHeader.LandedCostingGroupHeaders, "TWO", CostDistributionMechanismList.Codes.Item, 2));
				AssertEquals("Grp 3", true, AssertGroupHeadings(declarationXml.LandedCostingHeader.LandedCostingGroupHeaders, "THREE", CostDistributionMechanismList.Codes.ActualVolume, 3));
				AssertEquals("Grp 4", false, AssertGroupHeadings(declarationXml.LandedCostingHeader.LandedCostingGroupHeaders, "", "", 4));
				AssertEquals("Grp 5", false, AssertGroupHeadings(declarationXml.LandedCostingHeader.LandedCostingGroupHeaders, "", "", 5));
				AssertEquals("Grp 6", false, AssertGroupHeadings(declarationXml.LandedCostingHeader.LandedCostingGroupHeaders, "", "", 6));

				AssertEquals("LandCostItem", 1, declarationXml.LandedCostingHeader.TransportAndLogisticsCosts.Count);
				AssertEquals(accChargeCode.AC_Code, declarationXml.LandedCostingHeader.TransportAndLogisticsCosts[0].ChargeCode);
				AssertEquals("Charge Description", declarationXml.LandedCostingHeader.TransportAndLogisticsCosts[0].ChargeDescription);
				AssertEquals("1", declarationXml.LandedCostingHeader.TransportAndLogisticsCosts[0].ChargeGroup);
				AssertEquals(((ILandedCostDistributeTo)commonJobComInvoiceHeader).UniqueCode, declarationXml.LandedCostingHeader.TransportAndLogisticsCosts[0].DistributionLevel);
				AssertEquals(Xsd.LandedCostDistributionCode.VOL, declarationXml.LandedCostingHeader.TransportAndLogisticsCosts[0].DistributionBy);
				AssertEquals(new ZDecimal(1234), declarationXml.LandedCostingHeader.TransportAndLogisticsCosts[0].DistributionAmount);
				AssertEquals(refCurrency.RX_Code, declarationXml.LandedCostingHeader.TransportAndLogisticsCosts[0].Curr);
				AssertEquals(new ZDecimal(3), declarationXml.LandedCostingHeader.TransportAndLogisticsCosts[0].ExRate);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestExportPickupAndDeliveryInformation()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_RL_NKOrigin = "NZAKL";
			jobDec.JE_RL_NKFinalDestination = "AUSYD";
			jobDec.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;
			jobDec.JE_MessageType = "IMP";

			Factory.Save();

			AssertEquals("Job Dec is Import", true, jobDec.IsImport);
			NotificationBuffer buffer = new NotificationBuffer();
			Xsd.ShipmentShipmentDetails shipmentDetails = new Xsd.ShipmentShipmentDetails();
			adapter.ExportPickupAndDeliveryInformation(jobDec, shipmentDetails, new ValueObjectExportContext(buffer));
			AssertEquals("shipmentDetails 's delivery CFS is specified", true, shipmentDetails.Deliver.CFS.Address.IsSpecified);
			AssertEquals("shipmentDetails 's pickup CFS is not specified", false, shipmentDetails.Pickup.CFS.Address.IsSpecified);
			Xsd.AddressReference reference = shipmentDetails.Deliver.CFS.Address;
			AssertEquals("Address sequence", 1, reference.AddressSequenceRef);
			AssertEquals("Company Name", org.OH_Code, reference.Organisation.EDICode);

			jobDec.JE_MessageType = "EXP";
			Factory.Save();

			AssertEquals("Job Dec is Export", true, jobDec.IsExport);
			buffer.Clear();
			shipmentDetails = new Xsd.ShipmentShipmentDetails();
			adapter.ExportPickupAndDeliveryInformation(jobDec, shipmentDetails, new ValueObjectExportContext(buffer));
			AssertEquals("shipmentDetails 's pickup CFS is specified", true, shipmentDetails.Pickup.CFS.Address.IsSpecified);
			AssertEquals("shipmentDetails 's delivery CFS is specified", false, shipmentDetails.Deliver.CFS.Address.IsSpecified);
			reference = shipmentDetails.Pickup.CFS.Address;
			AssertEquals("Address sequence", 1, reference.AddressSequenceRef);
			AssertEquals("Company Name", org.OH_Code, reference.Organisation.EDICode);
		}

		public virtual void TestExportCustomsEntry()
		{
			BaseJobDeclaration declaration = ImportJobDec;

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			MergeAndSetupCustomsCharges(declaration);

			AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertNotEquals("Precondition: entryHeader.Charges.Count", 0, entryHeader.Charges.Count);

			AssertEquals("Precondition: MergedLines.Count", 2, entryHeader.MergedLines.Count);
			AssertNotEquals("Precondition: MergedLines[0].Fees.Count", 0, entryHeader.MergedLines[0].Fees.Count);
			AssertNotEquals("Precondition: MergedLines[1].Fees.Count", 0, entryHeader.MergedLines[1].Fees.Count);

			entryHeader.EntryNumber = "12345678";
			declaration.JE_EntrySubmittedDate = new ZDateTime(2006, 1, 1);

			Xsd.Consols consolXsd = adapter.ExportConsolsValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.CustomsEntryCollection entryHeaderXsds = consolXsd.Consol[0].Shipments[0].ShipmentDetails.CustomsEntries;
			AssertEquals("entryHeaderXsds.Count", 1, entryHeaderXsds.Count);

			Xsd.CustomsEntry entryHeaderXsd = entryHeaderXsds[0];
			Xsd.MonetaryAmount customsValueXsd = entryHeaderXsd.CustomsValue;
			AssertEquals("entryHeaderXsd.CustomsValue", 127.00m, customsValueXsd.Value);
			AssertEquals("entryHeaderXsd.EntryDate", new ZDateTime(2006, 1, 1), entryHeaderXsd.EntryDate);
			AssertEquals("entryHeaderXsd.ExchangeRate", 1.00m, entryHeaderXsd.ExchangeRate);
			AssertEquals("entryHeaderXsd.TransportAndInsurance", 150.00m, entryHeaderXsd.TransportAndInsurance.Value);

			Xsd.CustomsEntryNumber entryNumberXsd = entryHeaderXsd.CustomsEntryNumber;
			AssertNotNull("entryNumberXsd (entryHeaderXsd.CustomsEntryNumber)", entryNumberXsd);
			AssertEquals("entryNumberXsd.Number", "12345678", entryNumberXsd.Number);
			AssertEquals("entryNumberXsd.Country", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, entryNumberXsd.Country);
			AssertNotEquals("entryNumberXsd.Type", ZString.Empty, entryNumberXsd.Type);

			Xsd.ChargesChargeCollection entryHeaderChargeXsds = entryHeaderXsd.Charges;
			AssertNotEquals("entryHeaderChargeXsds.Count", 0, entryHeaderChargeXsds.Count);

			Xsd.CustomsEntryLineCollection entryLineXsds = entryHeaderXsd.CustomsEntryLines;
			AssertEquals("entryLineXsds.Count", 2, entryLineXsds.Count);

			Xsd.CustomsEntryLine entryLine1 = entryHeaderXsd.CustomsEntryLines[0];
			AssertNotEquals("entryLine1.Fees.Count", 0, entryLine1.Fees.Count);

			Xsd.CustomsEntryLine entryLine2 = entryHeaderXsd.CustomsEntryLines[0];
			AssertNotEquals("entryLine2.Fees.Count", 0, entryLine2.Fees.Count);
		}

		public void TestExportARInvoices()
		{
			bool exportARInvoices = CustomsDataRegistry.Instance.IncludeARInvoicesInXMLFile.Value;
			CustomsDataRegistry.Instance.IncludeARInvoicesInXMLFile.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = ImportJobDec;
			Factory.Save();

			JobHeader jobheader = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK))[0];

			BusinessObject aRInvoice = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoice)));
			aRInvoice[AccTransactionHeaderSchema.AH_JH] = jobheader.PK;
			aRInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = declaration.JE_DeclarationReference;

			BusinessObject aRInvoiceLine = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoiceLine)));
			aRInvoiceLine[AccTransactionLinesSchema.AL_AH] = aRInvoice.PK;
			aRInvoiceLine[AccTransactionLinesSchema.AL_JH] = jobheader.PK;
			aRInvoiceLine[AccTransactionLinesSchema.AL_AG] = Factory.NewWithValidTestData<AccGLHeader>().PK;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = (ZGuid)aRInvoiceLine[AccTransactionLinesSchema.PK];
			charge.JR_JH = jobheader.PK;

			Factory.Save();

			NotificationBuffer buffer = new NotificationBuffer();
			Xsd.ConsolAndShipment consol = adapter.ExportToValueObject(ImportJobDec, new ValueObjectExportContext(buffer));

			AssertEquals(1, consol.Shipment.ARInvoices.Count);

			CustomsDataRegistry.Instance.IncludeARInvoicesInXMLFile.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exportARInvoices);
		}

		public void TestExportDeclarationWithDocData()
		{
			BaseJobDeclaration declaration = ImportJobDec;
			declaration.DocNote.SetSystemDefinedFieldValue("Insurance Policy Number", "FGTR456");

			NotificationBuffer buffer = new NotificationBuffer();
			Xsd.ConsolAndShipment consol = adapter.ExportToValueObject(declaration, new ValueObjectExportContext(buffer));

			AssertEquals("Insurance Policy Number", consol.Shipment.DocData.SystemDefinedData[0].Name);
			AssertEquals("FGTR456", consol.Shipment.DocData.SystemDefinedData[0].Value);
		}

		public void TestImportDeclarationWithDocData()
		{
			BaseJobDeclaration declaration = ImportJobDec;
			declaration.DocNote.SetSystemDefinedFieldValue("Insurance Policy Number", "FGTR456");

			NotificationBuffer buffer = new NotificationBuffer();
			Xsd.ConsolAndShipment consol = new Xsd.ConsolAndShipment();
			adapter.ExportToValueObject(declaration, consol, new ValueObjectExportContext(buffer));

			BaseJobDeclaration newDeclaration = Factory.New<BaseJobDeclaration>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			adapter.ImportFromValueObject(newDeclaration, consol, context);

			AssertEquals("FGTR456", newDeclaration.DocNote.GetSystemDefinedFieldValue("Insurance Policy Number"));
		}

		public virtual void TestGetNewInvoicesGenerator()
		{
			InvoicesGeneratorFromXSD generator = adapter.GetNewInvoicesGenerator(GetJobDeclaration());
			AssertNotNull(generator);
			AssertEquals(InvoicesGeneratorType, generator.GetType());
		}

		public void TestGetMatchedOrganisation()
		{
			Xsd.Organisation orgValue = new Xsd.Organisation();
			orgValue.OrganisationDetails = new Xsd.OrganisationDetail();
			orgValue.OrganisationDetails.Name = "ORGNAME";
			orgValue.OrganisationDetails.WebAddress = "www.input.edi.com.au";
			orgValue.EDICode = "ZZZORG";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ZGuid orgPK = adapter.GetMatchedOrganisation(orgValue, context);

			AssertEquals("ORGNAME", (Factory.Load<OrgHeader>(orgPK)).OH_FullName);
		}

		public void TestGetNewInvoiceAdapter()
		{
			InvoiceValueObjectDataAdapter invoiceAdapter = adapter.GetNewInvoiceAdapter(GetJobDeclaration());

			AssertNotNull(invoiceAdapter);
			AssertEquals(typeof(InvoiceValueObjectDataAdapter), invoiceAdapter.GetType());
		}

		public void TestGetNewGroupInvoiceAdapter()
		{
			GroupInvoiceValueObjectDataAdapter groupInvoiceAdapter = adapter.GetNewGroupInvoiceAdapter(GetJobDeclaration());

			AssertNotNull(groupInvoiceAdapter);
			AssertEquals(typeof(GroupInvoiceValueObjectDataAdapter), groupInvoiceAdapter.GetType());
		}

		protected void AssertBusinessObjectFound(string message, Xsd.ConsolAndShipment declarationValue, BaseJobDeclaration declaration, bool expectMatch)
		{
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			BaseJobDeclaration foundDeclaration = adapter.FindBusinessObject(declarationValue, context);
			if (expectMatch)
			{
				AssertNotNull(message, foundDeclaration);
				AssertEquals(message, declaration.PK, foundDeclaration.PK);
			}
			else
			{
				AssertNull(message, foundDeclaration);
			}
		}

		protected bool ItemInXmlAddInfoCollection(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
		{
			foreach (Xsd.AdditionalCustomsInformation addCusInfo in addCusInfos)
			{
				if (addCusInfo.CustomsDetailType == itemType)
				{
					return true;
				}
			}
			return false;
		}

		protected string GetXmlAddInfoValue(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
		{
			foreach (Xsd.AdditionalCustomsInformation addCusInfo in addCusInfos)
			{
				if (addCusInfo.CustomsDetailType == itemType)
				{
					return addCusInfo.CustomsDetailValue;
				}
			}
			return "";
		}

		protected virtual Type InvoicesGeneratorType => typeof(InvoicesGeneratorFromXSD);

		protected bool DoesDeclarationHaveThisBill(BaseJobDeclaration declaration, ZString billNum, ZString billType)
		{
			return declaration.Bills.FindByBillNumberAndType(billNum, billType) != null;
		}

		protected virtual bool ShouldTestonFirstArrivalInfo => true;

		protected virtual void AssertDTAF(Xsd.Consol toConsol) => AssertEquals("AUSYD", toConsol.ConsolDetail.PortFirstArrival.Port.Value);

		protected virtual void MergeAndSetupCustomsCharges(BaseJobDeclaration declaration)
		{
			SendsMessagesToCustomsShutterUpperer mergeResult = new SendsMessagesToCustomsShutterUpperer(false);
			mergeResult.AnswerToContinueWithAction = true;
			declaration.DoMerge(mergeResult);
			AssertEquals("Precondition: mergeResult.LastErrorsAsString", "", mergeResult.LastErrorsAsString);
			AssertEquals("Precondition: mergeResult.InvalidOperationText", null, mergeResult.InvalidOperationText);
		}

		BaseJobDeclaration importJobDec;
		protected BaseJobDeclaration ImportJobDec
		{
			get
			{
				if (importJobDec == null)
				{
					importJobDec = TestJobDec;
					importJobDec.JE_RL_NKOrigin = "THBKK";
					importJobDec.JE_RL_NKPortOfLoading = "THBKK";
					importJobDec.JE_RL_NKPortOfArrival = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					importJobDec.JE_RL_NKFinalDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					importJobDec.JE_MessageType = JobMessageTypeList.Codes.Import;

					OrgHeader overseasAgentOrForwarder = Factory.New<OrgHeader>();
					overseasAgentOrForwarder.OH_FullName = "OverseasAgentOrForwarder";
					overseasAgentOrForwarder.OH_IsForwarder = true;
					overseasAgentOrForwarder.OH_RL_NKClosestPort = "THBKK";
					OrgAddress address1 = overseasAgentOrForwarder.Addresses.MainAddress;
					address1.OA_Address1 = "WEERSDfd6P5A3VWWVC2VZRTTTNXAS7G3RXT6M2FRJS3AD7D964";
					overseasAgentOrForwarder.OH_Code = "GR23BXD0783L";
					importJobDec.JE_OH_Forwarder = overseasAgentOrForwarder.PK;
					importJobDec.ResumeApportionment();
				}

				return importJobDec;
			}
		}

		BaseJobDeclaration exportJobDec;
		protected BaseJobDeclaration ExportJobDec
		{
			get
			{
				if (exportJobDec == null)
				{
					exportJobDec = TestJobDec;
					exportJobDec.JE_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					exportJobDec.JE_RL_NKFinalDestination = "THBKK";
					exportJobDec.JE_MessageType = JobMessageTypeList.Codes.Export;

					OrgHeader overseasAgentOrForwarder = Factory.New<OrgHeader>();
					overseasAgentOrForwarder.OH_FullName = "OverseasAgentOrForwarder";
					overseasAgentOrForwarder.OH_IsForwarder = true;
					overseasAgentOrForwarder.OH_RL_NKClosestPort = "THBKK";
					OrgAddress address1 = overseasAgentOrForwarder.Addresses.MainAddress;
					address1.OA_Address1 = "WEERSDfd6P5A3VWWVC2VZRTTTNXAS7G3RXT6M2FRJS3AD7D964";
					overseasAgentOrForwarder.OH_Code = "GR23BXD0783L";
					exportJobDec.JE_OH_Forwarder = overseasAgentOrForwarder.PK;
					exportJobDec.ResumeApportionment();
				}

				return exportJobDec;
			}
		}

		BaseJobDeclaration testJobDec;
		protected BaseJobDeclaration TestJobDec => testJobDec ?? (testJobDec = GetNewPopulatedJobDeclaration());

		protected virtual BaseJobDeclaration GetNewPopulatedJobDeclaration()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "Lloyds1";
			vessel.RV_Code = "NEWVES";

			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.DisableDefaultPackingInformation = true;

			OrgHeader importer = Factory.New<OrgHeader>();
			OrgHeader supplier = Factory.New<OrgHeader>();

			importer.OH_FullName = "Importer";
			importer.OH_IsConsignee = true;
			importer.OH_IsConsignor = true;
			importer.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress importerAddress = importer.Addresses.MainAddress;
			importerAddress.OA_Address1 = "FAWGE3AR6PKAFVWWY42VZ7TMTNXEH7G5RX0EM2HRJ13ND7WE0J";
			importer.OH_Code = "T5GISEQI2KZR";

			supplier.OH_FullName = "Supplier";
			supplier.OH_IsConsignee = true;
			supplier.OH_IsConsignor = true;
			supplier.OH_RL_NKClosestPort = "AUMEL";
			OrgAddress supplierAddress = supplier.Addresses.MainAddress;
			supplierAddress.OA_Address1 = "7J1B7RFZ4WSH3LJTGUG7P55D1ABG3TMYMKFCPHQ2VSK4UDXQF0";
			supplier.OH_Code = "L1FZUT4BYOD8";

			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = "IMP";
			declaration.JE_MasterBill = "MASTERBILL";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_VoyageFlightNo = "VoyageFlig";
			declaration.JE_ExportDate = new ZDateTime(1998, 1, 2);
			declaration.JE_DateOfArrival = new ZDateTime(2004, 1, 2);
			declaration.JE_DateAtOrigin = new ZDateTime(1998, 1, 2);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2004, 1, 2);
			declaration.JE_RS_NKServiceLevel = "Ser";
			declaration.JE_HouseBill = "HOUSEBILL";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OwnerRef = "OwnerRef";
			declaration.JE_GS_NKCusAgent = "BOB";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;

			declaration.DisableDefaultPackingInformation = false;
			declaration.JE_TotalNoOfPacks = 101;
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			declaration.DisableDefaultPackingInformation = true;

			declaration.JE_TotalNoOfPieces = 58;
			declaration.JE_TotalVolume = 229;
			declaration.JE_TotalVolumeUnit = "To";
			declaration.JE_TotalWeight = 240;
			declaration.JE_TotalWeightUnit = "To";
			declaration.JE_GoodsDescription = "GoodsDescription";
			declaration.JE_ShipmentIncoTerm = "Shi";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2004, 1, 2);
			declaration.JE_VesselName = "ADMIRALENGRACHT";
			declaration.JE_RL_NKPortOfLoading = "AUMEL";
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "AUSYD";

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "NLU12345678";
			container.CO_Seal = "SEAL";
			container.CO_SecondSeal = "SEAL2";
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container.CO_Weight = 12.33m;

			StmNote marksAndNumbersNote = declaration.Notes.AddNew();
			marksAndNumbersNote.ST_ParentID = declaration.PK;
			marksAndNumbersNote.ST_Table = declaration.TableName;
			marksAndNumbersNote.ST_Description = "Marks & Numbers";
			marksAndNumbersNote.ST_NoteText = "MarksAndNumbersShort";

			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INVOICENUMBER";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			invoiceHeader.JZ_InvoiceAmount = 57;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(1998, 3, 4);
			invoiceHeader.JZ_Volume = 159;
			invoiceHeader.JZ_VolumeUQ = "Vo";
			invoiceHeader.JZ_Weight = 168;
			invoiceHeader.JZ_WeightUQ = "We";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_IncoTerm = "Inc";
			invoiceHeader.JZ_CU_RelatedHouseBill = declaration.PrimaryHouseBill.PK;

			BaseInvoiceCharge invoiceHeaderCharge = invoiceHeader.Charges.AddNew();
			invoiceHeaderCharge.J7_Amount = 32;
			invoiceHeaderCharge.J7_ChargeType = "Cha";
			invoiceHeaderCharge.J7_RX_NKCurrency = "AUD";
			invoiceHeaderCharge.J7_IsGSTApplicable = false;
			invoiceHeaderCharge.J7_IsDutiable = false;
			invoiceHeaderCharge.J7_IsIncludedInITOT = false;

			BaseInvoiceCharge invoiceHeaderFreight = invoiceHeader.Charges.AddNew();
			invoiceHeaderFreight.J7_ChargeType = Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight;
			invoiceHeaderFreight.J7_Amount = 120.00m;

			BaseInvoiceCharge invoiceHeaderInsurance = invoiceHeader.Charges.AddNew();
			invoiceHeaderInsurance.J7_ChargeType = Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance;
			invoiceHeaderInsurance.J7_Amount = 30.00m;

			BaseInvoiceCharge invoiceHeaderPacking = invoiceHeader.Charges.AddNew();
			invoiceHeaderPacking.J7_ChargeType = Enterprise.Customs.Common.CustomsChargeTypeList.Codes.PackingCost;
			invoiceHeaderPacking.J7_Amount = 70.00m;

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Description = "CATS HEADS";
			invoiceLine1.JI_Tariff = Line1TariffCode;
			invoiceLine1.JI_LinePrice = 25.00m;

			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "RED CRAYONS";
			invoiceLine2.JI_Tariff = Line2TariffCode;
			invoiceLine2.JI_LinePrice = 32.00m;

			declaration.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			declaration.Invoices[0].JZ_JZ_GroupInvoiceFK = declaration.JobComInvoiceGroupHeaders[0].PK;

			declaration.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2005, 4, 25);
			declaration.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2005, 4, 25);

			declaration.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2005, 2, 1);
			declaration.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2005, 3, 1);
			declaration.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2005, 4, 1);
			declaration.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 5, 1);
			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			declaration.ImporterDeliveryAddress.E2_Address1 = "ADDRESS1";
			declaration.ImporterDeliveryAddress.E2_Address2 = "ADDRESS2";
			declaration.ImporterDeliveryAddress.E2_City = "CITY";
			declaration.ImporterDeliveryAddress.E2_State = "NSW";
			declaration.ImporterDeliveryAddress.E2_Postcode = "PCODE";
			declaration.ImporterDeliveryAddress.E2_RN_NKCountryCode = "AU";

			declaration.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2005, 2, 1);
			declaration.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2005, 3, 1);
			declaration.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2005, 4, 1);
			declaration.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2005, 5, 1);
			declaration.SupplierPickupAddress.E2_AddressOverride = true;
			declaration.SupplierPickupAddress.E2_Address1 = "ADDRESS1";
			declaration.SupplierPickupAddress.E2_Address2 = "ADDRESS2";
			declaration.SupplierPickupAddress.E2_City = "CITY";
			declaration.SupplierPickupAddress.E2_State = "VIC";
			declaration.SupplierPickupAddress.E2_Postcode = "PCODE";
			declaration.SupplierPickupAddress.E2_RN_NKCountryCode = "AU";
			declaration.DocsAndCartage.JP_CustomAttrib1 = "Cus1";
			declaration.DocsAndCartage.JP_CustomAttrib2 = "Cus2";
			declaration.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2005, 12, 2);
			declaration.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2005, 12, 1);
			declaration.DocsAndCartage.JP_CustomDecimal1 = 2m;
			declaration.DocsAndCartage.JP_CustomDecimal2 = 20m;
			declaration.DocsAndCartage.JP_CustomFlag1 = true;
			declaration.DocsAndCartage.JP_CustomFlag2 = false;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			BasePackingGroup packGroup = declaration.PrimaryHouseBill.PackingGroups.Count == 0 ? declaration.PrimaryHouseBill.PackingGroups.AddNew() : declaration.PrimaryHouseBill.PackingGroups[0];
			packGroup.CR_CO_Container = container.PK;
			packGroup.AddTotalOuterPackageIfRequired(101);

			GlbStaff salesRep = Factory.New<GlbStaff>();
			salesRep.GS_Code = "JA";
			salesRep.GS_FullName = "JESSICA ALLEN";
			salesRep.GS_IsSalesRep = true;

			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_GS_NKRepSales = salesRep.GS_Code;
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			declaration.ResumeApportionment();
			return declaration;
		}

		protected override ValueObjectDataAdapter<BaseJobDeclaration, Xsd.ConsolAndShipment> GetNewBizObjXmlDataAdapter() => TestDeclarationValueObjectDataAdapter.New();

		protected override string ExpectedRootCollectionElementName => "Consols";

		protected override string ExpectedRootElementName => "Consol";

		protected override bool IsExportToCollectionSupported => false;

		protected virtual string EmptyDeclarationFileName => "EmptyDeclaration.xml";
		protected virtual string Line1TariffCode => "0101";
		protected virtual string Line2TariffCode => "9090";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyJobDeclaration(), TestFileHelper.GetPathForTesting(EmptyDeclarationFileName), ValidationKind.None, "Empty Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			BaseCusContainer cusContainer = TestJobDec.CusContainers[0];
			CommonContainer freightContainer = Factory.NewWithValidTestData<CommonContainer>();
			cusContainer.CO_JC = freightContainer.PK;

			#region Export Process
			freightContainer.JC_EmptyRequired = testDate.AddDays(20);
			freightContainer.JC_DepartureEstimatedPickup = testDate.AddDays(21);
			freightContainer.JC_DepartureCartageAdvised = testDate.AddDays(22);
			freightContainer.JC_DepartureSlotReference = "export slot ref";
			freightContainer.JC_DepartureSlotDateTime = testDate.AddDays(23);
			freightContainer.JC_DepartureCartageRef = "export cart ref";
			freightContainer.JC_ContainerYardEmptyPickupGateOut = testDate.AddDays(24);
			freightContainer.JC_FCLWharfGateIn = testDate.AddDays(25);
			freightContainer.JC_DepartureCartageComplete = testDate.AddDays(26);
			freightContainer.JC_FCLOnBoardVessel = testDate.AddDays(27);
			freightContainer.DepartureTruckWaitTime = testDate.AddDays(28);
			freightContainer.DepartureTruckWaitCost = 29M;

			freightContainer.JC_DepartureDeliveryByRail = ZBool.True;

			OrgHeader pickupFromOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupFromOrg.OH_FullName = "pickup from org";
			OrgAddress pickupFromOrgAddress = pickupFromOrg.Addresses.AddNew(OrgAddressType.Office, true);
			pickupFromOrgAddress.OA_Address1 = "pickup from addr 1";
			freightContainer.JC_OA_DepartureContainerYardAddress = pickupFromOrgAddress.PK;
			#endregion

			#region Import Process
			freightContainer.JC_FCLAvailable = testDate.AddDays(1);
			freightContainer.JC_ArrivalCTOStorageStartDate = testDate.AddDays(2);
			freightContainer.JC_LCLAvailable = testDate.AddDays(3);
			freightContainer.JC_LCLStorageCommences = testDate.AddDays(4);
			freightContainer.JC_FCLUnloadFromVessel = testDate.AddDays(5);
			freightContainer.JC_ArrivalSlotDateTime = testDate.AddDays(6);
			freightContainer.JC_ArrivalSlotReference = "import slot ref";
			freightContainer.JC_ArrivalCartageRef = "import cart ref";
			freightContainer.JC_FCLWharfGateOut = testDate.AddDays(7);
			freightContainer.JC_ArrivalEstimatedDelivery = testDate.AddDays(8);
			freightContainer.JC_ArrivalCartageAdvised = testDate.AddDays(9);
			freightContainer.JC_ArrivalCartageComplete = testDate.AddDays(10);
			freightContainer.JC_EmptyReadyForReturn = testDate.AddDays(11);
			freightContainer.JC_EmptyReturnedBy = testDate.AddDays(12);
			freightContainer.JC_ContainerYardEmptyReturnGateIn = testDate.AddDays(13);

			freightContainer.JC_FCLHeldInTransitStaging = false;

			freightContainer.ArrivalCTOStorageDays = 11;
			freightContainer.ArrivalCTOStorageCost = 11M;
			freightContainer.ArrivalTruckWaitTime = testDate.AddDays(14);
			freightContainer.ArrivalTruckWaitCost = 14M;
			freightContainer.ArrivalCarrierDetentionDays = 12;
			freightContainer.ArrivalCarrierDetentionCost = 12M;

			OrgHeader deliverEmptyToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverEmptyToOrg.OH_FullName = "deliver empty to org";
			OrgAddress deliverEmptyToOrgAddress = deliverEmptyToOrg.Addresses.AddNew(OrgAddressType.Office, true);
			deliverEmptyToOrgAddress.OA_Address1 = "deliver empty to add 1";
			freightContainer.JC_OA_ArrivalContainerYardAddress = deliverEmptyToOrgAddress.PK;
			#endregion

			#region Custom Fields
			cusContainer.CO_CustomAttrib1 = "blah";
			cusContainer.CO_CustomDate1 = testDate;
			cusContainer.CO_CustomDecimal1 = 10M;
			cusContainer.CO_CustomFlag1 = false;
			#endregion

			return GetFullyPopulatedBizObjSample(TestJobDec);
		}

		protected virtual BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample(BaseJobDeclaration declaration)
		{
			return new BusinessObjectAndExpectedOutputFileName(TestJobDec, TestFileHelper.GetPathForTesting("PopulatedDeclaration.xml"), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected BaseJobDeclaration GetEmptyJobDeclaration()
		{
			BaseJobDeclaration jobDec = GetJobDeclaration();
			jobDec.DisableDefaultPackingInformation = true;
			jobDec.JE_MessageSubType = "NCF";
			jobDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			jobDec.ResumeApportionment();
			return jobDec;
		}

		protected override void OnBeforeImportFromValueObjectForExportImportExportTest(BusinessObject bizObjOriginallyExportedFrom, BusinessObject bizObjToImportTo)
		{
			base.OnBeforeImportFromValueObjectForExportImportExportTest(bizObjOriginallyExportedFrom, bizObjToImportTo);

			JobHeader originalJobHeader = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, bizObjOriginallyExportedFrom.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			if (originalJobHeader != null && !originalJobHeader.JH_GS_NKRepSales.IsEmpty)
			{
				JobHeader newJobHeader = Factory.NewJobForTesting<JobHeader>();
				newJobHeader.JH_GS_NKRepSales = originalJobHeader.JH_GS_NKRepSales;
				newJobHeader.JH_ParentID = bizObjToImportTo.PK;
				((BaseJobDeclaration)bizObjToImportTo).ResumeApportionment();
			}
		}

		protected override void NeedApportionment(BusinessObject bizObjToImportTo)
		{
			JobHeader originalJobHeader = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, bizObjToImportTo.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			if (originalJobHeader != null && !originalJobHeader.JH_GS_NKRepSales.IsEmpty)
			{
				((BaseJobDeclaration)bizObjToImportTo).ResumeApportionment();
			}
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"Consol/ConsolDetail/Voyage/LoadPorts/DepartureCTO/Organisation",
					"Consol/ConsolDetail/Voyage/DischargePorts/ArrivalCTO/Organisation",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Consignee",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Consignor",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/NotifyParty/Organisation",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Orders/OrderDetail/Buyer",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Orders/OrderDetail/Supplier",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Orders/OrderLines/OrderLineDeliveries/DeliveryDetails/Address/Organisation",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Packages/DGContact/Organisation",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/ImportBroker",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/ExportBroker",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Invoices/Consignor",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Orders/OrderDetail/Buyer",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Orders/OrderDetail/Supplier",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Orders/OrderLines/OrderLineDeliveries/DeliveryDetails/Address/Organisation",
					"Consol/ConsolDetail/SendingAgent",
					"Consol/ConsolDetail/ReceivingAgent",
					"Consol/ConsolDetail/Carrier",
					"Consol/ConsolDetail/CoLoadWith",
					"Consol/ConsolDetail/Arrival/CTO/Organisation",
					"Consol/ConsolDetail/Arrival/Depot/Organisation",
					"Consol/ConsolDetail/Arrival/ContainerYard/Organisation",
					"Consol/ConsolDetail/Departure/CTO/Organisation",
					"Consol/ConsolDetail/Departure/Depot/Organisation",
					"Consol/ConsolDetail/Departure/ContainerYard/Organisation",
					"Consol/Shipments/ShipmentDetails/Consignee",
					"Consol/Shipments/ShipmentDetails/Consignor",
					"Consol/Shipments/ShipmentDetails/NotifyParty/Organisation",
					"Consol/Shipments/ShipmentDetails/Orders/OrderDetail/Buyer",
					"Consol/Shipments/ShipmentDetails/Orders/OrderDetail/Supplier",
					"Consol/Shipments/ShipmentDetails/Orders/OrderLines/OrderLineDeliveries/DeliveryDetails/Address/Organisation",
					"Consol/Shipments/ShipmentDetails/Packages/DGContact/Organisation",
					"Consol/Shipments/ShipmentDetails/ImportBroker",
					"Consol/Shipments/ShipmentDetails/ExportBroker",
					"Consol/Shipments/ShipmentDetails/LocalClient",
					"Consol/Shipments/ShipmentDetails/TEU",
					"Consol/Shipments/Invoices",
					"Consol/Shipments/Invoices/Consignor",
					"Consol/Shipments/Orders/OrderDetail/Buyer",
					"Consol/Shipments/Orders/OrderDetail/Supplier",
					"Consol/Shipments/Orders/OrderLines/OrderLineDeliveries/DeliveryDetails/Address/Organisation",
					"Shipment/ShipmentDetails/Consignee",
					"Shipment/ShipmentDetails/Consignor",
					"Shipment/ShipmentDetails/NotifyParty/Organisation",
					"Shipment/ShipmentDetails/Orders/OrderDetail/Buyer",
					"Shipment/ShipmentDetails/Orders/OrderDetail/Supplier",
					"Shipment/ShipmentDetails/Orders/OrderLines/OrderLineDeliveries/DeliveryDetails/Address/Organisation",
					"Shipment/ShipmentDetails/Packages/DGContact/Organisation",
					"Shipment/ShipmentDetails/ImportBroker",
					"Shipment/ShipmentDetails/ExportBroker",
					"Shipment/ShipmentDetails/LocalClient",
					"Shipment/ShipmentDetails/TEU",
					"Shipment/Invoices",
					"Shipment/Invoices/Consignor",
					"Shipment/Orders/OrderDetail/Buyer",
					"Shipment/Orders/OrderDetail/Supplier",
					"Shipment/Orders/OrderLines/OrderLineDeliveries/DeliveryDetails/Address/Organisation",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Invoices",
					"Consol/Events",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Events",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Events",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Orders/Events",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Orders/OrderDetail/Events",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Orders/Events",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Orders/OrderDetail/Events",
					"Consol/Shipments/Events",
					"Consol/Shipments/ShipmentDetails/Orders/Events",
					"Consol/Shipments/ShipmentDetails/Orders/OrderDetail/Events",
					"Consol/Shipments/Orders/Events",
					"Consol/Shipments/Orders/OrderDetail/Events",
					"Shipment/Events",
					"Shipment/ShipmentDetails/Orders/Events",
					"Shipment/ShipmentDetails/Orders/OrderDetail/Events",
					"Shipment/Orders/Events",
					"Shipment/Orders/OrderDetail/Events",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/ShipmentDetails/Orders",
					"Consol/ConsolDetail/Voyage/Sailings/Consols/Shipments/Orders",
					"Consol/Shipments/ShipmentDetails/Orders",
					"Consol/Shipments/Orders",
					"Shipment/ShipmentDetails/Orders",
					"Shipment/Orders",
					"Consol/ConsolDetail/Voyage",
					"Consol/ConsolDetail/PlannedLegs",
					"Consol/Shipments/ShipmentDetails/Packages",
					"Shipment/ShipmentDetails/Packages",
					"Consol/ConsolDetail/PortFirstForeign",
					"Consol/ConsolDetail/PortLastForeign",
					"Consol",
					"Shipment",
					"Consol/ConsolDetail/ExternalAgentReference",
					"Shipment/Custom",
					"Consol/Shipment/ShipmentDetails/Deliver/CFS/",
					"Consol/Shipment/ShipmentDetails/Pickup/CFS/",
					"Consol/Shipment/ShipmentDetails/Deliver/CFS/Address",
					"Consol/Shipment/ShipmentDetails/Pickup/CFS/Address"
				};
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
			jobDec = GetJobDeclaration();
			jobDec.DisableDefaultPackingInformation = true;
			adapter = new TestDeclarationValueObjectDataAdapter();
			testDate = new ZDateTime(2007, 9, 28);
			jobDec.ResumeApportionment();
		}
		BaseJobDeclaration jobDec;
		TestDeclarationValueObjectDataAdapter adapter;
		ZDateTime testDate;
		IDisposable distributeByForExport;

		protected virtual BaseJobDeclaration GetJobDeclaration() => Factory.New<BaseJobDeclaration>();

		NotificationBuffer notify;
		NotificationBuffer Notify => notify ?? (notify = new NotificationBuffer());

		void AssertHasContainer(BaseJobDeclaration declaration, string containerNumber, string containerCode)
		{
			bool result = false;
			RefContainer refContainer = new RefContainer.Loader(Factory).LoadFromCode(containerCode);
			AssertNotNull("RefContainer should exist", refContainer);
			foreach (BaseCusContainer container in declaration.CusContainers)
			{
				if (container.CO_ContainerNumber == containerNumber && container.CO_RC == refContainer.PK)
				{
					result = true;
					break;
				}
			}
			AssertEquals("Container " + containerNumber + " container Code " + containerCode + " expected", true, result);
		}

		void AddContainerValue(Xsd.ContainerCollection containerCollection, string containerNumber, string containerCode, string isoCode)
		{
			Xsd.Container containerValue = containerCollection.AddNew();
			containerValue.ContainerNumber = containerNumber;
			containerValue.ContainerType.ContainerCode = containerCode;
			containerValue.ContainerType.ISOCode = isoCode;
		}

		void TestImportConsolValues_ReceivingSendingRecievingAgent(Xsd.Consol consol, ValueObjectImportContext context)
		{
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();
			consolAndShipment.Shipment.ShipmentDetails.ShipmentType = Xsd.ShipmentType.EXP;
			consol.ConsolDetail.ReceivingAgent = new Xsd.Organisation();
			consol.ConsolDetail.ReceivingAgent.EDICode = "RECEIVER";
			consol.ConsolDetail.ReceivingAgent.OrganisationDetails = new Xsd.OrganisationDetail();
			consol.ConsolDetail.ReceivingAgent.OrganisationDetails.Name = "RECEIVER";
			consol.ConsolDetail.ReceivingAgent.IsSpecified = true;
			consolAndShipment.Consol = consol;
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);
			AssertNotNull("Must import", jobDec.Forwarder);
			Assert("Receiver", jobDec.Forwarder.OH_Code == "RECEIV");

			consolAndShipment.Shipment.ShipmentDetails.ShipmentType = Xsd.ShipmentType.IMP;
			consolAndShipment.Shipment.ShipmentDetails.ShipmentTypeSpecified = true;
			consol.ConsolDetail.ReceivingAgent.IsSpecified = false;
			consol.ConsolDetail.SendingAgent = new Xsd.Organisation();
			consol.ConsolDetail.SendingAgent.EDICode = "SENDER";
			consol.ConsolDetail.SendingAgent.OrganisationDetails = new Xsd.OrganisationDetail();
			consol.ConsolDetail.SendingAgent.OrganisationDetails.Name = "SENDER";
			consol.ConsolDetail.SendingAgent.IsSpecified = true;
			consolAndShipment.Consol = consol;
			adapter.ImportFromValueObject(jobDec, consolAndShipment, context);
			AssertNotNull("Must import", jobDec.Forwarder);
			Assert("Sender", jobDec.Forwarder.OH_Code == "SENDER");
		}

		void AddBillsToDeclarationForPackTesting(BaseJobDeclaration jobDec, Xsd.ConsolAndShipment consolAndShipment)
		{
			Bill houseBill = jobDec.Bills[0];

			houseBill.CU_HouseBill = "HOUSEBILL 1";

			Bill masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB1";
			houseBill.CU_MasterBill = "MB1";

			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 2", "MB2", "CONT 2", 12);
			houseBill = jobDec.Bills.AddNew();
			houseBill.CU_HouseBill = "HOUSEBILL 2";

			masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB2";
			houseBill.CU_MasterBill = "MB2";

			AddBillContainerPackToXml(consolAndShipment, "HOUSEBILL 2", "MB3", "CONT 2", 13);
			houseBill = jobDec.Bills.AddNew();
			houseBill.CU_HouseBill = "HOUSEBILL 2";

			masterBill = jobDec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB3";
			houseBill.CU_MasterBill = "MB3";
		}

		void AssertPackageDetails(BaseJobDeclaration jobDec)
		{
			BasePackingGroup package = jobDec.PackingGroups[0];
			AssertEquals("HOUSEBILL 1", package.Bill.CU_HouseBill);
			AssertEquals("MB1", package.Bill.CU_MasterBill);
			AssertEquals(11, package.TotalPackageCount());
			AssertEquals(1, package.Packages.Count);

			package = jobDec.PackingGroups[1];
			AssertEquals("HOUSEBILL 2", package.Bill.CU_HouseBill);
			AssertEquals("MB2", package.Bill.CU_MasterBill);
			AssertEquals("CONT 2", package.Container.CO_ContainerNumber);
			AssertEquals(12, package.TotalPackageCount());
			AssertEquals(1, package.Packages.Count);

			package = jobDec.PackingGroups[2];
			AssertEquals("HOUSEBILL 2", package.Bill.CU_HouseBill);
			AssertEquals("MB3", package.Bill.CU_MasterBill);
			AssertEquals("CONT 2", package.Container.CO_ContainerNumber);
			AssertEquals(13, package.TotalPackageCount());
			AssertEquals(1, package.Packages.Count);
		}

		void AddBillContainerPackToXml(Xsd.ConsolAndShipment consolAndShipment, string housebill, string masterbill, string containerNo, int packQty)
		{
			if (!string.IsNullOrEmpty(housebill))
			{
				Xsd.ShipmentIdentifier shipmentIdentifier = consolAndShipment.Shipment.ShipmentIdentifier.AddNew();
				shipmentIdentifier.Value = housebill;
				shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			}

			Xsd.Container container = consolAndShipment.Consol.ConsolDetail.Containers.AddNew();
			container.ContainerNumber = containerNo;

			Xsd.DeclarationBillContainerPack billContainerPack = consolAndShipment.Shipment.Declaration.BillContainerPacks.AddNew();
			billContainerPack.BillNumber = housebill;
			billContainerPack.MasterbillNumber = masterbill;
			billContainerPack.ContainerNumber = containerNo;
			billContainerPack.PackQty.Value = packQty;
		}

		ZQuery DIMEventQuery(ZString reference)
		{
			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, reference);

			return query;
		}

		void AddPackageToJobDec(BaseJobDeclaration jobDec, string housebill, string masterbill, string containerNo, int packQty)
		{
			jobDec.DisableDefaultPackingInformation = true;

			if (!string.IsNullOrEmpty(masterbill))
			{
				Bill masterBill = jobDec.Bills.AddNew();
				masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
				masterBill.CU_BillNum = masterbill;
			}

			Bill houseBill = jobDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = housebill;
			houseBill.CU_MasterBill = masterbill;

			BasePackingGroup package = jobDec.PackingGroups.AddNew();
			package.CR_CU_HouseBill = houseBill.PK;
			package.CR_CO_Container = Factory.New(typeof(BaseCusContainer)).PK;
			package.Container.CO_ContainerNumber = containerNo;
			package.AddTotalOuterPackageIfRequired(packQty);
		}

		bool AssertSpecialTaxHeadings(Xsd.LandedCostingSpecialTaxHeadingCollection headings, ZString expectedLabel, int sequence)
		{
			bool result = false;

			foreach (Xsd.LandedCostingSpecialTaxHeading current in headings)
			{
				if (current.Sequence == sequence)
				{
					AssertEquals("special tax label", expectedLabel, current.Description);
					result = true;
					break;
				}
			}

			return result;
		}

		bool AssertGroupHeadings(Xsd.LandedCostingGroupHeaderCollection headings, ZString expectedLabel, ZString costDistributionCode, int grpID)
		{
			bool result = false;

			foreach (Xsd.LandedCostingGroupHeader current in headings)
			{
				if (current.GroupID == grpID)
				{
					AssertEquals("Group ID", expectedLabel, current.GroupDescription);
					AssertEquals("Cost Distribution Code", costDistributionCode, current.CostDistributionCode.ToString());
					result = true;
					break;
				}
			}

			return result;
		}

		protected class TestDeclarationValueObjectDataAdapter : DeclarationValueObjectDataAdapter
		{
			public new static TestDeclarationValueObjectDataAdapter New() => new TestDeclarationValueObjectDataAdapter();

			public new BaseJobDeclaration FindBusinessObject(Xsd.ConsolAndShipment value, IValueObjectImportContext context)
				=> base.FindBusinessObject(value, context);

			public new InvoiceValueObjectDataAdapter GetNewInvoiceAdapter(BaseJobDeclaration jobDec) => base.GetNewInvoiceAdapter(jobDec);

			#region Import

			public new void ImportFromValueObject(BaseJobDeclaration bizObj, Xsd.ConsolAndShipment value, IValueObjectImportContext context)
			{
				base.ImportFromValueObject(bizObj, value, context);
			}

			public new void ImportShipmentDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, Xsd.Shipment shipment, IValueObjectImportContext context)
			{
				base.ImportShipmentDetails(jobDec, consol, shipment, context);
			}

			public new void ImportContainers(BaseJobDeclaration jobDec, Xsd.ContainerCollection containers, IValueObjectImportContext context)
			{
				base.ImportContainers(jobDec, containers, context);
			}

			public new void ImportVesselInformation(BaseJobDeclaration jobDec, Xsd.SailingWithVesselVoyage consolDetailVessel, IValueObjectImportContext context)
			{
				base.ImportVesselInformation(jobDec, consolDetailVessel, context);
			}

			public new void ImportVesselInformationDates(Xsd.SailingWithVesselVoyage xmlVessel, BaseJobDeclaration jobDec, IValueObjectImportContext context)
			{
				base.ImportVesselInformationDates(xmlVessel, jobDec, context);
			}

			public new void ImportConsolDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, IValueObjectImportContext context)
			{
				base.ImportConsolDetails(jobDec, consol, context);
			}

			public new void ImportMasterbillDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, IValueObjectImportContext context)
			{
				base.ImportMasterbillDetails(jobDec, consol, context);
			}

			public new void ImportHousebillDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, Xsd.Shipment shipment, IValueObjectImportContext context)
			{
				base.ImportHousebillDetails(jobDec, consol, shipment, context);
			}

			#endregion

			#region Export

			public new void ExportDocAddresses(JobDocAddressDependentCollection docAddresses, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
			{
				base.ExportDocAddresses(docAddresses, shipmentValue, context);
			}

			public new Xsd.ConsolAndShipment ExportToValueObject(BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				return base.ExportToValueObject(jobDec, context);
			}

			public new void ExportContainers(Xsd.ConsolConsolDetail xsdConsol, BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				base.ExportContainers(xsdConsol, jobDec, context);
			}

			public new void ExportShipmentValues(Xsd.Shipment toShipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				base.ExportShipmentValues(toShipment, jobDec, context);
			}

			public new void ExportConsolValues(Xsd.Consol toConsol, BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				base.ExportConsolValues(toConsol, jobDec, context);
			}

			public new void ExportMasterbillDetails(Xsd.Consol toConsol, BaseJobDeclaration jobDec)
			{
				base.ExportMasterbillDetails(toConsol, jobDec);
			}

			public new Xsd.InvoiceHeaderCollection ExportInvoiceHeaders(BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				return base.ExportInvoiceHeaders(jobDec, context);
			}

			public new Xsd.Consols ExportConsolsValueObject(BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				return base.ExportConsolsValueObject(jobDec, context);
			}

			public new void ExportHousebillDetails(Xsd.Shipment toShipment, BaseJobDeclaration jobDec)
			{
				base.ExportHousebillDetails(toShipment, jobDec);
			}

			public new void ExportLandedCostingHeadings(Xsd.Declaration declarationXml, BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				base.ExportLandedCostingHeadings(declarationXml, jobDec, context);
			}

			public new void ExportPickupAndDeliveryInformation(BaseJobDeclaration jobdec, Xsd.ShipmentShipmentDetails shipmentDetails, IValueObjectExportContext context)
			{
				base.ExportPickupAndDeliveryInformation(jobdec, shipmentDetails, context);
			}

			#endregion

			public new GroupInvoiceValueObjectDataAdapter GetNewGroupInvoiceAdapter(BaseJobDeclaration jobDec) => base.GetNewGroupInvoiceAdapter(jobDec);

			public ZGuid GetMatchedOrganisation(Xsd.Organisation organisation, IValueObjectImportContext context)
			{
				return base.GetMatchedOrganisation(organisation, context, null, OrganisationTypes.None);
			}

			protected override void SetShipmentTypeDetailsCore(BaseJobDeclaration jobDec)
			{
				jobDec.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				SetShipmentDetailsCoreWasExecuted = true;
			}

			protected override ZString DataImportReference => DIMReference;

			public bool SetShipmentDetailsCoreWasExecuted;

			public ZString DIMReference;

			public new InvoicesGeneratorFromXSD GetNewInvoicesGenerator(BaseJobDeclaration declaration) => base.GetNewInvoicesGenerator(declaration);
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
			distributeByForExport?.Dispose();
		}

		protected TestFileHelper TestFileHelper => testFileHelper ??= new();
		TestFileHelper testFileHelper;
	}
}
