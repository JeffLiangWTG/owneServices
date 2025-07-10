using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin
{
	abstract class CertificateOfOriginBuilderTests<TCertificate, TLineItem, TOriginCriterionList> : TestCaseWithFactory
		where TCertificate : CertificateOfOriginDocDataObject<TLineItem>
		where TLineItem : CertificateOfOriginLineItemDocDataObject
		where TOriginCriterionList : CodeDescriptionPairList, new()
	{
		protected const string SignatureRequiredMessage = "Signature required, please upload to your CW1 profile.";
		const string EmailRequiredMessage = "Email address required. Please enter in your CW1 profile.";
		const string SignatureOfDeclaration = "Signature of Declaration";
		const string CertificateOfOriginNumber = "62.000000000.2024.000111";

		protected virtual string InvalidPacklineOriginError => "Origin is required.";
		protected virtual string InvalidPacklineMarksAndNumbersError => "Marks and numbers are required.";

		protected abstract CertificateOfOriginBuilder<TCertificate, TLineItem, TOriginCriterionList> CreateBuilder(ForwardingShipment shipment);

		public void TestBuild_PopulateLineItemFromPackline_WithoutInvoiceLine()
		{
			var shipment = CreateShipmentCore();
			var lineItems = CreateLineItemsCore(shipment).ToArray();

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			AssertNotNull(certificate);
			AssertNotNull(certificate.AgreementInfo);

			AssertEquals(ZString.Empty, certificate.CertificateOfOriginNumber);

			AssertEquals("DepartureDate", new ZDateTime(2020, 01, 01, 00, 00, 00), certificate.DepartureDate);
			AssertEquals("ArrivalDate", new ZDateTime(2020, 02, 01, 00, 00, 00), certificate.ArrivalDate);
			AssertEquals("VoyageFlightNumber", "KH6754", certificate.VoyageFlightNumber);
			AssertEquals("Vessel", "VesselData", certificate.Vessel.Name);
			AssertEquals("TransportMode", Constants.TransportModes.Sea, certificate.TransportMode.Code);

			AssertEquals("ProducerAddressStateString", "False\nFalse\nFalse", certificate.ProducerAddressStateString);
			AssertEquals("ProducerAddressState_IsSameAsExporter", ZBool.False, certificate.ProducerAddressState.IsSameAsExporter);
			AssertEquals("ProducerAddressState_IsUnknown", ZBool.False, certificate.ProducerAddressState.IsUnknown);
			AssertEquals("ProducerAddressState_ExcludeFromPDF", ZBool.False, certificate.ProducerAddressState.ExcludeFromPDF);

			AssertEquals("LineItemsCount", 1, certificate.LineItems.Count);
			AssertEquals("LineItemSource", LineItemSource.PackLine, certificate.Source);

			var certificateLineItems = certificate.LineItems.ToArray();

			AssertEquals("LineItem1_ItemNumber", new ZShort(1), certificateLineItems[0].ItemNumber);
			AssertEquals("LineItem1_MarksAndNumbers", lineItems[0].JL_MarksAndNumbers, certificateLineItems[0].MarksAndNumbers);
			AssertEquals("LineItem1_GoodsDescription", lineItems[0].JL_DetailedDescription, certificateLineItems[0].GoodsDescription);
			AssertEquals("LineItem1_HarmonizedCode", lineItems[0].JL_HarmonisedCode, certificateLineItems[0].HarmonizedCode.Code);
			AssertEquals("LineItem1_GrossOrNetWeight", lineItems[0].JL_ActualWeight, certificateLineItems[0].Quantity.Value);
			AssertEquals("LineItem1_OriginCode", lineItems[0].Origin.Code, certificateLineItems[0].OriginCode);
			AssertEquals("LineItem1_OriginCriterionList", builder.GetDefaultOriginCriterionCode(), certificateLineItems[0].OriginCriterion.Code);
			AssertEquals("LineItem1_PackageCount", lineItems[0].JL_PackageCount, certificateLineItems[0].PackageCount);
			AssertEquals("LineItem1_PackageType", lineItems[0].JL_F3_NKPackType, certificateLineItems[0].PackageType.Code);
			AssertEquals("LineItem1_ActualWeightUQ", lineItems[0].JL_ActualWeightUQ, certificateLineItems[0].Quantity.Unit.Code);
			AssertEquals("LineItem1_IsMarksAndNumbersEditable", false, certificateLineItems[0].IsMarksAndNumbersEditable);

			AssertEquals("LineItem1_NetWeight", 0m, certificateLineItems[0].QuantityNet.Value);
			AssertEquals("LineItem1_NetWeightUQ", string.Empty, certificateLineItems[0].QuantityNet.Unit.Code);
		}

		public void Test_LineItemDescriptionMaxLine_Packingline()
		{
			var shipment = CreateShipmentCore();
			var lineItem = shipment
				.OuterPackLines
				.AddNew()
				.PopulateData(
					shipment,
					detailedDescription: "111111111\r\n222222222\r\n333333333\r\n444444444\r\n555555555\r\n666666666\r\n777777777",
					originCode: Core.Constants.CountryCodes.NewZealand,
					harmonizedCodeCountry: null);
			PopulatePackLine(lineItem);
			var certificate = CreateBuilder(shipment).Build();
			AssertEquals("LineItemsCount", 1, certificate.LineItems.Count);
			AssertEquals("LineItemSource", LineItemSource.PackLine, certificate.Source);
			var certificateLineItems = certificate.LineItems.ToArray();
			AssertEquals("LineItem1_GoodsDescription", "111111111\r\n222222222\r\n333333333\r\n444444444", certificateLineItems[0].GoodsDescription);
		}

		public void Test_LineItemDescriptionMaxLine_CommercialInvoice()
		{
			var shipment = CreateShipmentCore();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_GB = GlbCompany.CurrentCompany.PK;
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = shipment.PK;
			invoice.JZ_InvoiceNumber = "INV0001";
			invoice.JZ_InvoiceDate = ZDateTime.Now;
			declaration.Invoices.Add(invoice);
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Description = "111111111\r\n222222222\r\n333333333\r\n444444444\r\n555555555\r\n666666666\r\n777777777";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = "TN";
			invoiceLine.JI_LinePrice = 2.12;
			invoiceLine.JI_NetWeight = 50.12;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoice.InvoiceLines.Add(invoiceLine);
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			AssertNotNull(certificate);
			AssertEquals("LineItemsCount", 1, certificate.LineItems.Count);
			AssertEquals("LineItemSource", LineItemSource.Invoice, certificate.Source);
			var certificateLineItems = certificate.LineItems.ToArray();
			AssertEquals("LineItem1_GoodsDescription", "111111111\r\n222222222\r\n333333333\r\n444444444", certificateLineItems[0].GoodsDescription);
		}

		public void TestBuild_PopulateLineItemFromInvoice_WithPackLine()
		{
			var shipment = CreateShipmentCore();
			var lineItems = CreateLineItemsCore(shipment).ToArray();
			var declaration = CreateDeclaration(shipment);

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			AssertNotNull(certificate);
			AssertNotNull(certificate.AgreementInfo);

			AssertEquals(ZString.Empty, certificate.CertificateOfOriginNumber);

			AssertEquals("DepartureDate", new ZDateTime(2020, 01, 01, 00, 00, 00), certificate.DepartureDate);
			AssertEquals("ArrivalDate", new ZDateTime(2020, 02, 01, 00, 00, 00), certificate.ArrivalDate);
			AssertEquals("VoyageFlightNumber", "KH6754", certificate.VoyageFlightNumber);
			AssertEquals("Vessel", "VesselData", certificate.Vessel.Name);
			AssertEquals("TransportMode", Constants.TransportModes.Sea, certificate.TransportMode.Code);

			AssertEquals("ProducerAddressStateString", "False\nFalse\nFalse", certificate.ProducerAddressStateString);
			AssertEquals("ProducerAddressState_IsSameAsExporter", ZBool.False, certificate.ProducerAddressState.IsSameAsExporter);
			AssertEquals("ProducerAddressState_IsUnknown", ZBool.False, certificate.ProducerAddressState.IsUnknown);
			AssertEquals("ProducerAddressState_ExcludeFromPDF", ZBool.False, certificate.ProducerAddressState.ExcludeFromPDF);

			AssertEquals("LineItemsCount", 1, certificate.LineItems.Count);
			AssertEquals("LineItemSource", LineItemSource.Invoice, certificate.Source);

			var certificateLineItems = certificate.LineItems.ToArray();

			AssertEquals("LineItem1_ItemNumber", new ZShort(1), certificateLineItems[0].ItemNumber);
			AssertEquals("LineItem1_GoodsDescription", declaration.Invoices[0].InvoiceLines[0].JI_Description, certificateLineItems[0].GoodsDescription);
			AssertEquals("LineItem1_HarmonizedCode", declaration.Invoices[0].InvoiceLines[0].JI_Tariff.Replace(".", ""), certificateLineItems[0].HarmonizedCode.Code);
			AssertEquals("LineItem1_GrossOrNetWeight", declaration.Invoices[0].InvoiceLines[0].JI_Weight, certificateLineItems[0].Quantity.Value);
			AssertEquals("LineItem1_PackageCount", declaration.Invoices[0].InvoiceLines[0].JI_InvoiceQuantity.ToZInt(), certificateLineItems[0].PackageCount);
			AssertEquals("LineItem1_PackageType", declaration.Invoices[0].InvoiceLines[0].JI_InvoiceUQ, certificateLineItems[0].PackageType.Code);
			AssertEquals("LineItem1_ActualWeightUQ", declaration.Invoices[0].InvoiceLines[0].JI_WeightUQ, certificateLineItems[0].Quantity.Unit.Code);
			AssertEquals("LineItem1_NetWeight", declaration.Invoices[0].InvoiceLines[0].JI_NetWeight, certificateLineItems[0].QuantityNet.Value);
			AssertEquals("LineItem1_NetWeightUQ", declaration.Invoices[0].InvoiceLines[0].JI_NetWeightUQ, certificateLineItems[0].QuantityNet.Unit.Code);
			AssertEquals("LineItem1_IsMarksAndNumbersEditable", true, certificateLineItems[0].IsMarksAndNumbersEditable);

			AssertEquals("Invoice_Number", declaration.Invoices[0].JZ_InvoiceNumber, certificateLineItems[0].Invoice.Number);
			AssertEquals("Invoice_Date", declaration.Invoices[0].JZ_InvoiceDate, certificateLineItems[0].Invoice.Date);
			AssertEquals("Invoice_Amount", declaration.Invoices[0].InvoiceLines[0].JI_LinePrice, certificateLineItems[0].Invoice.Amount.Amount);
			AssertEquals("Invoice_Currency", declaration.Invoices[0].InvoiceLines[0].LinePriceRefCurrency.Code, certificateLineItems[0].Invoice.Amount.Currency.Code);
		}

		public void TestBuild_PopulateLineItemFromInvoice_WithPackLine_NZCompany()
		{
			var shipment = CreateShipmentCore();
			var lineItems = CreateLineItemsCore(shipment).ToArray();
			var declaration = CreateDeclarationNz(shipment);

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			AssertNotNull(certificate);
			AssertNotNull(certificate.AgreementInfo);

			AssertEquals(ZString.Empty, certificate.CertificateOfOriginNumber);

			AssertEquals("DepartureDate", new ZDateTime(2020, 01, 01, 00, 00, 00), certificate.DepartureDate);
			AssertEquals("ArrivalDate", new ZDateTime(2020, 02, 01, 00, 00, 00), certificate.ArrivalDate);
			AssertEquals("VoyageFlightNumber", "KH6754", certificate.VoyageFlightNumber);
			AssertEquals("Vessel", "VesselData", certificate.Vessel.Name);
			AssertEquals("TransportMode", Constants.TransportModes.Sea, certificate.TransportMode.Code);

			AssertEquals("ProducerAddressStateString", "False\nFalse\nFalse", certificate.ProducerAddressStateString);
			AssertEquals("ProducerAddressState_IsSameAsExporter", ZBool.False, certificate.ProducerAddressState.IsSameAsExporter);
			AssertEquals("ProducerAddressState_IsUnknown", ZBool.False, certificate.ProducerAddressState.IsUnknown);
			AssertEquals("ProducerAddressState_ExcludeFromPDF", ZBool.False, certificate.ProducerAddressState.ExcludeFromPDF);

			AssertEquals("LineItemsCount", 1, certificate.LineItems.Count);
			AssertEquals("LineItemSource", LineItemSource.Invoice, certificate.Source);

			var certificateLineItems = certificate.LineItems.ToArray();

			AssertEquals("LineItem1_ItemNumber", new ZShort(1), certificateLineItems[0].ItemNumber);
			AssertEquals("LineItem1_MarksAndNumbers", ((Enterprise.Integration.Customs.NZ.IJobComInvoiceLine)declaration.Invoices[0].InvoiceLines[0]).PackagingMarks1, certificateLineItems[0].MarksAndNumbers);
			AssertEquals("LineItem1_GoodsDescription", declaration.Invoices[0].InvoiceLines[0].JI_Description, certificateLineItems[0].GoodsDescription);
			AssertEquals("LineItem1_HarmonizedCode", declaration.Invoices[0].InvoiceLines[0].JI_Tariff, certificateLineItems[0].HarmonizedCode.Code);
			AssertEquals("LineItem1_GrossOrNetWeight", declaration.Invoices[0].InvoiceLines[0].JI_Weight, certificateLineItems[0].Quantity.Value);
			AssertEquals("LineItem1_PackageCount", declaration.Invoices[0].InvoiceLines[0].JI_InvoiceQuantity.ToZInt(), certificateLineItems[0].PackageCount);
			AssertEquals("LineItem1_PackageType", declaration.Invoices[0].InvoiceLines[0].JI_InvoiceUQ, certificateLineItems[0].PackageType.Code);
			AssertEquals("LineItem1_ActualWeightUQ", declaration.Invoices[0].InvoiceLines[0].JI_WeightUQ, certificateLineItems[0].Quantity.Unit.Code);
			AssertEquals("LineItem1_NetWeight", declaration.Invoices[0].InvoiceLines[0].JI_NetWeight, certificateLineItems[0].QuantityNet.Value);
			AssertEquals("LineItem1_NetWeightUQ", declaration.Invoices[0].InvoiceLines[0].JI_NetWeightUQ, certificateLineItems[0].QuantityNet.Unit.Code);
			AssertEquals("LineItem1_IsMarksAndNumbersEditable", false, certificateLineItems[0].IsMarksAndNumbersEditable);

			AssertEquals("Invoice_Number", declaration.Invoices[0].JZ_InvoiceNumber, certificateLineItems[0].Invoice.Number);
			AssertEquals("Invoice_Date", declaration.Invoices[0].JZ_InvoiceDate, certificateLineItems[0].Invoice.Date);
			AssertEquals("Invoice_Amount", declaration.Invoices[0].InvoiceLines[0].JI_LinePrice, certificateLineItems[0].Invoice.Amount.Amount);
		}

		public void TestBuild_PopulateLineItemFromInvoice_WithoutPackLine()
		{
			var shipment = CreateShipmentCore();
			var declaration = CreateDeclaration(shipment);

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			AssertNotNull(certificate);
			AssertNotNull(certificate.AgreementInfo);

			AssertEquals(ZString.Empty, certificate.CertificateOfOriginNumber);

			AssertEquals("DepartureDate", new ZDateTime(2020, 01, 01, 00, 00, 00), certificate.DepartureDate);
			AssertEquals("ArrivalDate", new ZDateTime(2020, 02, 01, 00, 00, 00), certificate.ArrivalDate);
			AssertEquals("VoyageFlightNumber", "KH6754", certificate.VoyageFlightNumber);
			AssertEquals("Vessel", "VesselData", certificate.Vessel.Name);
			AssertEquals("TransportMode", Constants.TransportModes.Sea, certificate.TransportMode.Code);

			AssertEquals("ProducerAddressStateString", "False\nFalse\nFalse", certificate.ProducerAddressStateString);
			AssertEquals("ProducerAddressState_IsSameAsExporter", ZBool.False, certificate.ProducerAddressState.IsSameAsExporter);
			AssertEquals("ProducerAddressState_IsUnknown", ZBool.False, certificate.ProducerAddressState.IsUnknown);
			AssertEquals("ProducerAddressState_ExcludeFromPDF", ZBool.False, certificate.ProducerAddressState.ExcludeFromPDF);

			AssertEquals("LineItemsCount", 1, certificate.LineItems.Count);
			AssertEquals("LineItemSource", LineItemSource.Invoice, certificate.Source);

			var certificateLineItems = certificate.LineItems.ToArray();

			AssertEquals("LineItem1_ItemNumber", new ZShort(1), certificateLineItems[0].ItemNumber);
			AssertEquals("LineItem1_GoodsDescription", declaration.Invoices[0].InvoiceLines[0].JI_Description, certificateLineItems[0].GoodsDescription);
			AssertEquals("LineItem1_HarmonizedCode", declaration.Invoices[0].InvoiceLines[0].JI_Tariff.Replace(".", ""), certificateLineItems[0].HarmonizedCode.Code);
			AssertEquals("LineItem1_GrossOrNetWeight", declaration.Invoices[0].InvoiceLines[0].JI_Weight, certificateLineItems[0].Quantity.Value);
			AssertEquals("LineItem1_PackageCount", declaration.Invoices[0].InvoiceLines[0].JI_InvoiceQuantity.ToZInt(), certificateLineItems[0].PackageCount);
			AssertEquals("LineItem1_PackageType", declaration.Invoices[0].InvoiceLines[0].JI_InvoiceUQ, certificateLineItems[0].PackageType.Code);
			AssertEquals("LineItem1_ActualWeightUQ", declaration.Invoices[0].InvoiceLines[0].JI_WeightUQ, certificateLineItems[0].Quantity.Unit.Code);
			AssertEquals("LineItem1_NetWeight", declaration.Invoices[0].InvoiceLines[0].JI_NetWeight, certificateLineItems[0].QuantityNet.Value);
			AssertEquals("LineItem1_NetWeightUQ", declaration.Invoices[0].InvoiceLines[0].JI_NetWeightUQ, certificateLineItems[0].QuantityNet.Unit.Code);
			AssertEquals("LineItem1_IsMarksAndNumbersEditable", true, certificateLineItems[0].IsMarksAndNumbersEditable);

			AssertEquals("Invoice_Number", declaration.Invoices[0].JZ_InvoiceNumber, certificateLineItems[0].Invoice.Number);
			AssertEquals("Invoice_Date", declaration.Invoices[0].JZ_InvoiceDate, certificateLineItems[0].Invoice.Date);
			AssertEquals("Invoice_Amount", declaration.Invoices[0].InvoiceLines[0].JI_LinePrice, certificateLineItems[0].Invoice.Amount.Amount);
			AssertEquals("Invoice_Currency", declaration.Invoices[0].InvoiceLines[0].LinePriceRefCurrency.Code, certificateLineItems[0].Invoice.Amount.Currency.Code);
		}

		public virtual void TestPorts()
		{
			var certificate = CreateBuilder(CreateShipmentCore()).Build();
			AssertNotNull(certificate);

			AssertEquals("PortOfLoading", "NZAKL", certificate.PortOfLoading.Code);
			AssertEquals("PortOfDischarge", "CNSZX", certificate.PortOfDischarge.Code);
			AssertEquals("PortOfOrigin", "NZAKL", certificate.PortOfOrigin.Code);
			AssertEquals("PortOfDestination", "CNSZX", certificate.PortOfDestination.Code);
		}

		public BaseJobDeclaration CreateDeclaration(ForwardingShipment shipment)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_GB = GlbCompany.CurrentCompany.PK;

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = shipment.PK;
			invoice.JZ_InvoiceNumber = "INV0001";
			invoice.JZ_InvoiceDate = ZDateTime.Now;
			invoice.JZ_InvoiceAmount = 5.15;
			invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Australia;

			declaration.Invoices.Add(invoice);

			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Description = "Description";
			invoiceLine.JI_Tariff = "2030.20";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = "TN";
			invoiceLine.JI_InvoiceUQ = "IUQ";
			invoiceLine.JI_PartNo = "JIPARTNO";
			invoiceLine.JI_LinePrice = 2.12;
			invoiceLine.JI_NetWeight = 50.12;
			invoiceLine.JI_NetWeightUQ = "KG";

			invoice.InvoiceLines.Add(invoiceLine);

			return declaration;
		}

		public BaseJobDeclaration CreateDeclarationNz(ForwardingShipment shipment)
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.NZ.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_GB = GlbCompany.CurrentCompany.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = shipment.PK;
			invoice.JZ_InvoiceNumber = "INV0001";
			invoice.JZ_InvoiceDate = ZDateTime.Now;
			invoice.JZ_InvoiceAmount = 5.15;

			declaration.Invoices.Add(invoice);

			var invoiceLine = (Enterprise.Integration.Customs.NZ.IJobComInvoiceLine)invoice.AddNewInvoiceLine();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Description = "Description";
			invoiceLine.JI_Tariff = "Tariff";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = "TN";
			invoiceLine.JI_InvoiceUQ = "IUQ";
			invoiceLine.PackagingMarks1 = "TestMarks";

			return (BaseJobDeclaration)declaration;
		}

		public void TestPopulateAddresses()
		{
			var shipment = CreateShipmentCore();

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertNotNull(certificate);

			AssertAddressData(shipment.ConsigneeDocumentaryAddress, certificate.ImporterAddress);
			AssertAddressData(shipment.ConsignorDocumentaryAddress, certificate.ExporterAddress);
			AssertAddressData(shipment.ManufacturerDocAddress, certificate.ProducerAddress);
			AssertEquals("PortOfDestination", shipment.JS_RL_NKDestination, certificate.PortOfDestination.Code);
		}

		public void TestPopulateApplicantCompanyAddress()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);

			var forwarderName = "Norris Forwarding Ltd";
			var forwarderAddressLine1 = "1 New Street";
			var forwarderAddressLine2 = "New Town";
			var forwarderCity = "Auckland";
			var forwarderStateCode = "AUK";
			var forwarderState = "Auckland";
			var forwarderPostcode = "H91Y6CT";
			var forwarderCountryCode = "NZ";
			var forwarderCountry = "New Zealand";
			var forwarderFax = "091593217";
			var forwarderBusinessRegNumber = "099-999-999";

			var forwarderContact = "Lando Norris";
			var forwarderPhone = "091593218";
			var forwarderEmail = "lando@norrisforwarding.com";

			GlbCompany.CurrentCompany.CompanyName = forwarderName;
			GlbCompany.CurrentCompany.Address1 = forwarderAddressLine1;
			GlbCompany.CurrentCompany.Address2 = forwarderAddressLine2;
			GlbCompany.CurrentCompany.City = forwarderCity;
			GlbCompany.CurrentCompany.Postcode = forwarderPostcode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = forwarderCountryCode;
			GlbCompany.CurrentCompany.StateCode = forwarderStateCode;
			GlbCompany.CurrentCompany.GC_Fax = forwarderFax;
			GlbCompany.CurrentCompany.GC_BusinessRegNo = forwarderBusinessRegNumber;

			GlbStaff.CurrentUser.GS_FullName = forwarderContact;
			GlbStaff.CurrentUser.GS_WorkPhone = forwarderPhone;
			GlbStaff.CurrentUser.GS_EmailAddress = forwarderEmail;

			var certificate = builder.Build();
			AssertNotNull(certificate);

			AssertEquals(forwarderName, certificate.ApplicantCompanyAddress.CompanyName);
			AssertEquals(forwarderAddressLine1, certificate.ApplicantCompanyAddress.AddressLine1);
			AssertEquals(forwarderAddressLine2, certificate.ApplicantCompanyAddress.AddressLine2);
			AssertEquals(forwarderCity, certificate.ApplicantCompanyAddress.City);
			AssertEquals(forwarderState, certificate.ApplicantCompanyAddress.State);
			AssertEquals(forwarderPostcode, certificate.ApplicantCompanyAddress.Postcode);
			AssertEquals(forwarderCountry, certificate.ApplicantCompanyAddress.Country.Name);
			AssertEquals(forwarderFax, certificate.ApplicantCompanyAddress.Fax);
			AssertEquals(forwarderBusinessRegNumber, certificate.ApplicantCompanyAddress.TaxNumber);
			AssertEquals(forwarderContact, certificate.ApplicantCompanyAddress.Contact);
			AssertEquals(forwarderPhone, certificate.ApplicantCompanyAddress.Phone);
			AssertEquals(forwarderEmail, certificate.ApplicantCompanyAddress.Email);
		}

		public void TestCurrentUserAddressData()
		{
			var certificate = CreateBuilder(CreateShipmentCore()).Build();
			
			AssertNotNull(certificate);
			AssertCurrentUserAddressData(certificate.CurrentUser);
		}

		public void TestPopulateRemarks()
		{
			var shipment = CreateShipmentCore();
			var note = "simple note";
			shipment.Notes.AddNew(isCustomDescription: false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, note);
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			AssertNotNull(certificate);
			AssertEquals($"{note}\r\n", certificate.Remarks);
		}

		public void TestPopulateSignature()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertNotNull(certificate);
			AssertEquals(SignatureOfDeclaration, GlbStaff.CurrentUser.SignatureImage, certificate.Signature.Signature);
			AssertEquals("Name of Declaration", GlbStaff.CurrentUser.GS_FullName, certificate.Signature.Name);

			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 1);
			certificate = builder.Build();

			AssertImageEquals(SignatureOfDeclaration, GlbStaff.CurrentUser.SignatureImage, (Image)certificate.Signature.Signature);
		}

		public void TestBuild_ShouldPopulateHarmonisedCodeFromList_WhenListContainsHarmonisedCodeCountry()
		{
			var shipment = CreateShipmentCore();
			var lineItems = CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertNotNull(certificate);
			AssertEquals("LineItemsCount", 1, certificate.LineItems.Count);

			var lineItem = certificate.LineItems.First();
			AssertEquals(
				"LineItem1_HarmonizedCode",
				builder.HarmonisedCodeCountry.IsEmpty
					? lineItems[0].JL_HarmonisedCode
					: lineItems[0].HarmonisedCodes[0].JLH_Code,
				lineItem.HarmonizedCode.Code);
		}

		#region AddressValidations

		public void Test_Validation_Addresses_Exporter()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var certificate = builder.Build();
			Test_Validation_Address(
				AddressType.Exporter,
				certificate.ExporterAddress,
				validateEmail: builder.InitialFlags.ValidateConsignorEmail,
				validatePhone: builder.InitialFlags.ValidateConsignorPhone);
		}

		public void Test_Validation_Addresses_Importer()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			var address = certificate.ImporterAddress;
			var addressType = AddressType.Importer;
			var formalName = "Consignee";

			CombineAssertions(() =>
			{
				// Assertion for all valid initial flags
				Test_AddressValidation(address);

				// Assert Email and Phone
				if (builder.InitialFlags.ValidateConsigneeEmail == ContactValidation.Always ||
					(builder.InitialFlags.ValidateConsigneeEmail == ContactValidation.IfAddressSet && !string.IsNullOrEmpty(certificate.ImporterAddress.AddressFormatted)))
				{
					// AssertHasMessageError
					Test_AddressValidation(address, expectedEmailError: $"Email Address is required. ({formalName})", email: null);
				}
				else
				{
					// AssertNoMessageErrors
					Test_AddressValidation(address, email: null);
				}

				if (builder.InitialFlags.ValidateConsigneePhone == ContactValidation.Always ||
					(builder.InitialFlags.ValidateConsigneePhone == ContactValidation.IfAddressSet && !string.IsNullOrEmpty(certificate.ImporterAddress.AddressFormatted)))
				{
					// AssertHasMessageError
					Test_AddressValidation(address, expectedPhoneError: $"Phone Number is required. ({formalName})", phone: null);
				}
				else
				{
					// AssertNoMessageErrors
					Test_AddressValidation(address, phone: null);
				}

				// Assert Address => By default we only use AddressValidaiton.Full for Importer/Consignee address
				if (builder.InitialFlags.ValidateConsigneeAddress)
				{
					// AssertHasMessageError
					Test_AddressValidation(address, expectedError: $"{addressType} Company Name is required. ({formalName})", companyName: null);
					Test_AddressValidation(address);
					Test_AddressValidation(address, expectedError: $"{addressType} Company Name is required. ({formalName})", companyName: string.Empty);

					Test_AddressValidation(address);
					Test_AddressValidation(address, expectedError: $"{addressType} Address 1 is required. ({formalName})", addressLine: null);
					Test_AddressValidation(address, expectedError: $"{addressType} City is required. ({formalName})", cityName: null);
					Test_AddressValidation(address, expectedError: $"{addressType} Country is required. ({formalName})", countryName: null);

					Test_AddressValidation(address, expectedError: $"{addressType} Address 1 is required. ({formalName})", addressLine: string.Empty);
					Test_AddressValidation(address, expectedError: $"{addressType} City is required. ({formalName})", cityName: string.Empty);
					Test_AddressValidation(address, expectedError: $"{addressType} Country is required. ({formalName})", countryName: string.Empty);

					address.Country.Name = "Valid";
					address.AddressFormattedInfo.ClearAllNotifications();
					address.Country = null;
					AssertHasMessageError(address.AddressFormattedInfo, $"{addressType} Country is required. ({formalName})");
				}
			});
		}

		public void Test_Validation_Addresses_Producer()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var certificate = builder.Build();
			var address = certificate.ProducerAddress;
			if (builder.InitialFlags.ValidateProducerAddress == AddressValidation.FullIfDifferentFromConsignor)
			{
				if (certificate.ExporterAddress.AddressFormatted != certificate.ProducerAddress.AddressFormatted)
				{
					Test_Validation_Address(
					AddressType.Producer,
					certificate.ProducerAddress,
					AddressValidation.Full);
				}
				else
				{
					Test_Validation_Address(
					AddressType.Producer,
					certificate.ProducerAddress,
					AddressValidation.None);
				}
			}
			else
			{
				Test_Validation_Address(
					AddressType.Producer,
					certificate.ProducerAddress,
					builder.InitialFlags.ValidateProducerAddress);
			}

			// Assert Email and Phone
			if (!certificate.ProducerAddressState.IsSameAsExporter)
			{
				if (builder.InitialFlags.ValidateProducerEmail == ContactValidation.Always ||
				(builder.InitialFlags.ValidateProducerEmail == ContactValidation.IfAddressSet && !string.IsNullOrEmpty(certificate.ProducerAddress.AddressFormatted)))
				{
					Test_AddressValidation(address, expectedEmailError: "Email Address is required. (Producer)", email: null);
				}
				else
				{
					Test_AddressValidation(address, email: null);
				}

				if (builder.InitialFlags.ValidateProducerPhone == ContactValidation.Always ||
					(builder.InitialFlags.ValidateProducerPhone == ContactValidation.IfAddressSet && !string.IsNullOrEmpty(certificate.ProducerAddress.AddressFormatted)))
				{
					Test_AddressValidation(address, expectedPhoneError: "Phone Number is required. (Producer)", phone: null);
				}
				else
				{
					Test_AddressValidation(address, phone: null);
				}
			}
		}

		public void Test_Validation_Address(
			AddressType addressType,
			Address address,
			AddressValidation addressValidation = AddressValidation.Full,
			bool validateEmail = false,
			bool validatePhone = false)
		{
			var formalName = addressType switch
			{
				AddressType.Importer => "Consignee",
				AddressType.Exporter => "Consignor",
				AddressType.Producer => "Manufacturer",
				_ => string.Empty
			};

			if (addressValidation != AddressValidation.Complex)
			{
				CombineAssertions(() =>
				{
					Test_AddressValidation(address);

					if (validateEmail)
					{
						Test_AddressValidation(address, expectedEmailError: $"Email Address is required. ({formalName})", email: null);
					}

					if (validatePhone)
					{
						Test_AddressValidation(address, expectedPhoneError: $"Phone Number is required. ({formalName})", phone: null);
					}

					if (addressValidation == AddressValidation.None)
					{
						Test_AddressValidation(address, companyName: null, addressLine: null, cityName: null, countryName: null);
						Test_AddressValidation(address);
						Test_AddressValidation(address, companyName: string.Empty, addressLine: string.Empty, cityName: string.Empty, countryName: string.Empty);
					}
					else
					{
						Test_AddressValidation(address, expectedError: $"{addressType} Company Name is required. ({formalName})", companyName: null);
						Test_AddressValidation(address);
						Test_AddressValidation(address, expectedError: $"{addressType} Company Name is required. ({formalName})", companyName: string.Empty);
					}

					if (addressValidation == AddressValidation.Full)
					{
						Test_AddressValidation(address, expectedError: $"{addressType} Address 1 is required. ({formalName})", addressLine: null);
						Test_AddressValidation(address, expectedError: $"{addressType} City is required. ({formalName})", cityName: null);
						Test_AddressValidation(address, expectedError: $"{addressType} Country is required. ({formalName})", countryName: null);

						Test_AddressValidation(address, expectedError: $"{addressType} Address 1 is required. ({formalName})", addressLine: string.Empty);
						Test_AddressValidation(address, expectedError: $"{addressType} City is required. ({formalName})", cityName: string.Empty);
						Test_AddressValidation(address, expectedError: $"{addressType} Country is required. ({formalName})", countryName: string.Empty);

						address.Country.Name = "Valid";
						address.AddressFormattedInfo.ClearAllNotifications();
						address.Country = null;
						AssertHasMessageError(address.AddressFormattedInfo, $"{addressType} Country is required. ({formalName})");
					}
				});
			}
			else
			{
				Assert(true);
			}
		}

		void Test_AddressValidation(
			Address address,
			string expectedError = null,
			string companyName = "Valid",
			string addressLine = "Valid",
			string cityName = "Valid",
			string countryName = "Valid",
			string email = "valid@validemail.com",
			string expectedEmailError = null,
			string phone = "0123456789",
			string expectedPhoneError = null)
		{
			address.AddressFormattedInfo.ClearAllNotifications();

			address.CompanyName = companyName;
			if (address.Country != null)
			{
				address.Country.Name = countryName;
			}
			address.City = cityName;
			address.AddressLine1 = addressLine;
			address.Email = email;
			address.Phone = phone;

			if (!string.IsNullOrEmpty(expectedError))
			{
				AssertHasMessageError(address.AddressFormattedInfo, expectedError);
			}
			else
			{
				AssertNoMessageErrors(address.AddressFormattedInfo);
			}

			if (!string.IsNullOrEmpty(expectedEmailError))
			{
				AssertHasMessageError(address.EmailInfo, expectedEmailError);
			}
			else
			{
				AssertNoMessageErrors(address.EmailInfo);
			}

			if (!string.IsNullOrEmpty(expectedPhoneError))
			{
				AssertHasMessageError(address.PhoneInfo, expectedPhoneError);
			}
			else
			{
				AssertNoMessageErrors(address.PhoneInfo);
			}
		}

		public void Test_ValidationReturnsError_When_ProducerCompanyNameIsEmpty()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			if (builder.InitialFlags.ValidateProducerAddress == AddressValidation.Complex)
			{
				var certificate = builder.Build();
				ProducerAddressValidationTestHelper(certificate, new AddressState { IsSameAsExporter = true, IsUnknown = false, ExcludeFromPDF = true }, string.Empty, expectsError: false);
				ProducerAddressValidationTestHelper(certificate, new AddressState { IsSameAsExporter = false, IsUnknown = true, ExcludeFromPDF = true }, string.Empty, expectsError: false);
				ProducerAddressValidationTestHelper(certificate, new AddressState { IsSameAsExporter = false, IsUnknown = false, ExcludeFromPDF = true }, string.Empty, expectsError: true);
			}
			else
			{
				Assert(true);
			}
		}

		void ProducerAddressValidationTestHelper(
			TCertificate certificate,
			AddressState addressState,
			string companyName,
			bool expectsError)
		{
			certificate.ProducerAddress.AddressFormattedInfo.ClearAllNotifications();
			certificate.ProducerAddress.CompanyName = companyName;
			certificate.ProducerAddressStateString = addressState.GetAddressStateString();

			if (expectsError)
			{
				AssertHasMessageError(certificate.ProducerAddress.AddressFormattedInfo, "Producer details entry is mandatory for JEVS submission. Click in box 2 for Producer options.");
			}
			else
			{
				AssertNoMessageErrors(certificate.ProducerAddress.AddressFormattedInfo);
			}
		}

		public void Test_Validation_Addresses_ApplicantAddress()
		{
			var certificate = CreateBuilder(CreateShipmentCore()).Build();

			CombineAssertions(() =>
			{
				Test_Validation_Addresses_ApplicantAddress(certificate);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant Company Name is required. Please enter in CW1 profile.", companyName: null);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant Address 1 is required. Please enter in CW1 profile.", addressLine: null);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant City is required. Please enter in CW1 profile.", cityName: null);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant Country is required. Please enter in CW1 profile.", countryName: null);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant Registered Number is required. Please enter in CW1 profile.", taxNumber: null);

				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant Company Name is required. Please enter in CW1 profile.", companyName: string.Empty);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant Address 1 is required. Please enter in CW1 profile.", addressLine: string.Empty);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant City is required. Please enter in CW1 profile.", cityName: string.Empty);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant Country is required. Please enter in CW1 profile.", countryName: string.Empty);
				Test_Validation_Addresses_ApplicantAddress(certificate, "Applicant Registered Number is required. Please enter in CW1 profile.", taxNumber: string.Empty);

				certificate.ApplicantCompanyAddress.Country.Name = "Valid";
				certificate.ApplicantCompanyAddressErrorInfo.ClearAllNotifications();
				certificate.ApplicantCompanyAddress.Country = null;
				AssertHasMessageError(certificate.ApplicantCompanyAddressErrorInfo, "Applicant Country is required. Please enter in CW1 profile.");
			});
		}

		void Test_Validation_Addresses_ApplicantAddress(
			TCertificate certificate,
			string expectedError = null,
			string companyName = "Valid",
			string addressLine = "Valid",
			string cityName = "Valid",
			string countryName = "Valid",
			string taxNumber = "Valid")
		{
			certificate.ApplicantCompanyAddressErrorInfo.ClearAllNotifications();

			certificate.ApplicantCompanyAddress.CompanyName = companyName;
			certificate.ApplicantCompanyAddress.Country.Name = countryName;
			certificate.ApplicantCompanyAddress.City = cityName;
			certificate.ApplicantCompanyAddress.AddressLine1 = addressLine;
			certificate.ApplicantCompanyAddress.TaxNumber = taxNumber;

			if (!string.IsNullOrEmpty(expectedError))
			{
				AssertHasMessageError(certificate.ApplicantCompanyAddressErrorInfo, expectedError);
			}
			else
			{
				AssertNoMessageErrors(certificate.ApplicantCompanyAddressErrorInfo);
			}
		}

		public void Test_Validation_Addresses_CurrentUserAddress()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var certificate = builder.Build();

			var currentUserCityValidationFlag = builder.InitialFlags.ValidateCurrentUserCity;

			CombineAssertions(() =>
			{
				Test_Validation_Addresses_CurrentUserAddress(certificate);
				Test_Validation_Addresses_CurrentUserAddress(certificate, "Company Name of logged in user is required.", companyName: null);
				Test_Validation_Addresses_CurrentUserAddress(certificate, "City of logged in user is required.", cityName: null, cityValidationFlag: currentUserCityValidationFlag);
				Test_Validation_Addresses_CurrentUserAddress(certificate, "Country of logged in user is required.", countryName: null);
				Test_Validation_Addresses_CurrentUserAddress(certificate, "Email address required. Please enter in your CW1 profile.", email: null);

				Test_Validation_Addresses_CurrentUserAddress(certificate, "Company Name of logged in user is required.", companyName: string.Empty);
				Test_Validation_Addresses_CurrentUserAddress(certificate, "City of logged in user is required.", cityName: string.Empty, cityValidationFlag: currentUserCityValidationFlag);
				Test_Validation_Addresses_CurrentUserAddress(certificate, "Country of logged in user is required.", countryName: string.Empty);
				Test_Validation_Addresses_CurrentUserAddress(certificate, "Email address required. Please enter in your CW1 profile.", email: string.Empty);

				certificate.CurrentUser.Country.Name = "Valid";
				certificate.CurrentUserErrorInfo.ClearAllNotifications();
				certificate.CurrentUser.Country = null;
				AssertHasMessageError(certificate.CurrentUserErrorInfo, "Country of logged in user is required.");
			});
		}

		void Test_Validation_Addresses_CurrentUserAddress(
			TCertificate certificate,
			string expectedError = null,
			string companyName = "Valid",
			string cityName = "Valid",
			string countryName = "Valid",
			string email = "abc@xyz.com",
			bool cityValidationFlag = true)
		{
			certificate.CurrentUserErrorInfo.ClearAllNotifications();

			certificate.CurrentUser.CompanyName = companyName;
			certificate.CurrentUser.City = cityName;
			certificate.CurrentUser.Country.Name = countryName;
			certificate.CurrentUser.Email = email;

			if (!string.IsNullOrEmpty(expectedError))
			{
				if(string.IsNullOrEmpty(cityName) && !cityValidationFlag)
				{
					AssertNoMessageErrors(certificate.CurrentUserErrorInfo);
				}
				else
				{
					AssertHasMessageError(certificate.CurrentUserErrorInfo, expectedError);
				}
			}
			else
			{
				AssertNoMessageErrors(certificate.CurrentUserErrorInfo);
			}
		}

		#endregion

		public void Test_Validation_ReturnErrorMessage_When_TransportReference_IsEmpty()
		{
			var builder = CreateBuilder(CreateShipmentCore(createConsol: true));

			ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(
				builder.Build(),
				builder.InitialFlags.ValidateTransportReference);
		}

		protected void ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(TCertificate certificate, bool expectValue)
		{
			AssertNoMessageErrors(certificate.VoyageFlightNumberInfo);
			AssertNoMessageErrors(certificate.Vessel.NameInfo);

			var expectedError = "Vessel/Flight/Train/Vehicle Name or Number is required.";

			void AssertBothError()
			{
				AssertHasMessageError(certificate.VoyageFlightNumberInfo, expectedError);
				AssertHasMessageError(certificate.Vessel.NameInfo, expectedError);
			}

			void AssertNeitherErrors()
			{
				AssertNoMessageErrors(certificate.VoyageFlightNumberInfo);
				AssertNoMessageErrors(certificate.Vessel.NameInfo);
			}

			foreach (var nullOrEmpty in new string[] { null, string.Empty })
			{
				certificate.VoyageFlightNumber = nullOrEmpty;
				certificate.Vessel.Name = nullOrEmpty;

				if (expectValue)
				{
					AssertBothError();
				}
				else
				{
					AssertNeitherErrors();
				}

				certificate.VoyageFlightNumber = "Valid VFN";

				AssertNeitherErrors();

				certificate.VoyageFlightNumber = nullOrEmpty;

				if (expectValue)
				{
					AssertBothError();
				}
				else
				{
					AssertNeitherErrors();
				}

				certificate.Vessel.Name = "Valid VN";

				AssertNeitherErrors();

				certificate.Vessel.Name = nullOrEmpty;

				if (expectValue)
				{
					AssertBothError();
				}
				else
				{
					AssertNeitherErrors();
				}

				certificate.VoyageFlightNumber = "Valid VFN";

				AssertNeitherErrors();

				certificate.Vessel.Name = "Valid VN";

				AssertNeitherErrors();
			}

			certificate.VoyageFlightNumber = null;

			AssertNeitherErrors();

			certificate.Vessel.Name = string.Empty;

			if (expectValue)
			{
				AssertBothError();
			}
			else
			{
				AssertNeitherErrors();
			}

			certificate.VoyageFlightNumber = "Valid VFN";
			certificate.Vessel.Name = "Valid VN";

			AssertNeitherErrors();

			certificate.VoyageFlightNumber = string.Empty;

			AssertNeitherErrors();

			certificate.Vessel.Name = null;

			if (expectValue)
			{
				AssertBothError();
			}
			else
			{
				AssertNeitherErrors();
			}
		}

		public void Test_Validation_OptionallyReturnErrorMessage_When_DepartureDateIsEmpty()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			certificate.DepartureDate = ZDateTime.Today;

			AssertNoMessageErrors(certificate.DepartureDateInfo);

			certificate.DepartureDate = ZDateTime.Empty;

			if (builder.InitialFlags.ValidateDepartureDate)
			{
				AssertHasMessageError(certificate.DepartureDateInfo, "Departure Date is mandatory.");
			}
			else
			{
				AssertNoMessageErrors(certificate.DepartureDateInfo);
			}
		}

		public void Test_Validation_OptionallyReturnErrorMessage_When_ArrivalDateIsEmpty()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			certificate.ArrivalDate = ZDateTime.Today;

			AssertNoMessageErrors(certificate.ArrivalDateInfo);

			certificate.ArrivalDate = ZDateTime.Empty;

			if (builder.InitialFlags.ValidateArrivalDate)
			{
				AssertHasMessageError(certificate.ArrivalDateInfo, "ETA is required.");
			}
			else
			{
				AssertNoMessageErrors(certificate.ArrivalDateInfo);
			}
		}

		#region AddPacklineValidation

		public void Test_Validation_LineItem_MarksAndNumbers()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();

			AssertNoMessageErrors(lineItem.MarksAndNumbersInfo);

			lineItem.MarksAndNumbers = string.Empty;

			if (builder.InitialFlags.ValidateLineItemMarksAndNumbers)
			{
				AssertHasMessageError(lineItem.MarksAndNumbersInfo, InvalidPacklineMarksAndNumbersError);
			}
			else
			{
				AssertNoMessageErrors(lineItem.MarksAndNumbersInfo);
			}

			lineItem.MarksAndNumbers = null;

			if (builder.InitialFlags.ValidateLineItemMarksAndNumbers)
			{
				AssertHasMessageError(lineItem.MarksAndNumbersInfo, InvalidPacklineMarksAndNumbersError);
			}
			else
			{
				AssertNoMessageErrors(lineItem.MarksAndNumbersInfo);
			}

			lineItem.MarksAndNumbers = "Marks & Numbers";

			AssertNoMessageErrors(lineItem.MarksAndNumbersInfo);
		}

		public void Test_Validation_ReturnErrorMessage_When_LineItemDescriptionOfGoodsIsEmpty()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var certificate = CreateBuilder(shipment).Build();

			var lineItem = certificate.LineItems.First();

			AssertNoMessageError(lineItem.GoodsDescriptionInfo, "Goods Description is mandatory.");

			lineItem.GoodsDescription = string.Empty;

			AssertHasMessageError(lineItem.GoodsDescriptionInfo, "Goods Description is mandatory.");

			lineItem.GoodsDescription = null;

			AssertHasMessageError(lineItem.GoodsDescriptionInfo, "Goods Description is mandatory.");
		}

		public virtual void Test_Validation_ReturnErrorMessage_When_LineItemGrossWeightIsInvalid()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			var lineItem = certificate.LineItems.First();

			if (lineItem.Quantity is Measurement quantity &&
				quantity.Unit is CodeDescription unitCode)
			{
				AssertNoMessageErrors(quantity.ValueInfo);
				AssertNoMessageErrors(unitCode.CodeInfo);

				if (builder.InitialFlags.ValidateLineItemWeight)
				{
					CombineAssertions(() =>
					{
						OptionallyAssert_UnitCodeValidation(unitCode, expectError: true, codeString: null);
						OptionallyAssert_UnitCodeValidation(unitCode, expectError: false, codeString: "KG");
						OptionallyAssert_UnitCodeValidation(unitCode, expectError: true, codeString: string.Empty);

						OptionallyAssert_QuantityValueValidation(quantity, expectError: true, quantityValue: 0);
						OptionallyAssert_QuantityValueValidation(quantity, expectError: true, quantityValue: -1);
						OptionallyAssert_QuantityValueValidation(quantity, expectError: false, quantityValue: 1);
					});
				}
				else
				{
					CombineAssertions(() =>
					{
						OptionallyAssert_UnitCodeValidation(unitCode, expectError: false, codeString: null);
						OptionallyAssert_UnitCodeValidation(unitCode, expectError: false, codeString: string.Empty);
						OptionallyAssert_UnitCodeValidation(unitCode, expectError: false, codeString: "KG");

						OptionallyAssert_QuantityValueValidation(quantity, expectError: false, quantityValue: 0);
						OptionallyAssert_QuantityValueValidation(quantity, expectError: false, quantityValue: -1);
						OptionallyAssert_QuantityValueValidation(quantity, expectError: false, quantityValue: 1);
					});
				}
			}
		}

		void OptionallyAssert_UnitCodeValidation(CodeDescription unitCode, bool expectError, string codeString)
		{
			unitCode.CodeInfo.ClearAllNotifications();
			unitCode.Code = codeString;

			if (expectError)
			{
				AssertHasMessageError(unitCode.CodeInfo, "Weight unit is mandatory.");
			}
			else
			{
				AssertNoMessageErrors(unitCode.CodeInfo);
			}
		}

		void OptionallyAssert_QuantityValueValidation(Measurement quantity, bool expectError, int quantityValue)
		{
			quantity.ValueInfo.ClearAllNotifications();
			quantity.Value = quantityValue;

			if (expectError)
			{
				AssertHasMessageError(quantity.ValueInfo, "Weight is mandatory and must be greater than zero.");
			}
			else
			{
				AssertNoMessageErrors(quantity.ValueInfo);
			}
		}

		public void Test_Validation_LineItem_HarmonizedCode()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();

			AssertNoMessageErrors(lineItem.HarmonizedCode.CodeInfo);

			lineItem.HarmonizedCode.Code = string.Empty;
			if (builder.InitialFlags.ValidateLineHarmonizedCode)
			{
				AssertHasMessageError(lineItem.HarmonizedCode.CodeInfo, "Harmonized code is required.");
			}
			else
			{
				AssertNoMessageErrors(lineItem.HarmonizedCode.CodeInfo);
			}

			lineItem.HarmonizedCode.Code = null;
			if (builder.InitialFlags.ValidateLineHarmonizedCode)
			{
				AssertHasMessageError(lineItem.HarmonizedCode.CodeInfo, "Harmonized code is required.");
			}
			else
			{
				AssertNoMessageErrors(lineItem.HarmonizedCode.CodeInfo);
			}
		}

		public void Test_ValidationReturnsError_When_HarmonizedCodeGreaterThanSixCharacters()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			var lineItem = certificate.LineItems.First();

			AssertNoMessageErrors(lineItem.HarmonizedCode.CodeInfo);

			lineItem.HarmonizedCode.Code = "1234567";
			if (builder.InitialFlags.ValidateLineHarmonizedCode || !string.IsNullOrEmpty(lineItem.HarmonizedCode.Code))
			{
				AssertHasMessageError(lineItem.HarmonizedCode.CodeInfo, "Harmonized Code cannot exceed 6 characters.");
			}
			else
			{
				AssertNoMessageErrors(lineItem.HarmonizedCode.CodeInfo);
			}

			lineItem.HarmonizedCode.Code = "123456";
			AssertNoMessageErrors(lineItem.HarmonizedCode.CodeInfo);
		}

		public void Test_HarmonizedCodes_OnlyFirst6DigitsTakenInFormBuilder_PackLinesUsed()
		{
			var shipment = CreateShipmentCore();
			var forwardingPackLines = CreateLineItemsCore(shipment, hsCodeLength: 9).ToArray();

			AssertEquals("Test data not created as per requirement","123456789", forwardingPackLines[0].JL_HarmonisedCode);

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			var lineItem = certificate.LineItems.First();

			AssertEquals("Only first 6 characters of Harmonized Code should be taken", "123456", lineItem.HarmonizedCode.Code);
		}

		public void Test_HarmonizedCodes_OnlyFirst6DigitsTakenInFormBuilder_InvoiceLinesUsed()
		{
			var shipment = CreateShipmentCore();
			var declaration = CreateDeclaration(shipment);
			declaration.Invoices[0].InvoiceLines[0].JI_Tariff = "2020.12.34";

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();
			var lineItem = certificate.LineItems.First();
			AssertEquals("Only first 6 characters of Harmonized Code should be taken", "202012", lineItem.HarmonizedCode.Code);
		}

		public void Test_Validation_LineItem_OriginCode()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();

			AssertNoMessageErrors(lineItem.OriginInfo);

			lineItem.Origin = string.Empty;

			if (builder.InitialFlags.ValidateLineItemOrigin)
			{
				AssertHasMessageError(lineItem.OriginInfo, InvalidPacklineOriginError);
			}
			else
			{
				AssertNoMessageErrors(lineItem.OriginInfo);
			}

			lineItem.Origin = "Australia";

			AssertNoMessageErrors(lineItem.OriginInfo);

			lineItem.Origin = null;

			if (builder.InitialFlags.ValidateLineItemOrigin)
			{
				AssertHasMessageError(lineItem.OriginInfo, InvalidPacklineOriginError);
			}
			else
			{
				AssertNoMessageErrors(lineItem.OriginInfo);
			}
		}

		public void Test_Validation_LineItem_OriginCriterion()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();

			var criterionList = new TOriginCriterionList();
			var criterionListOptions = string.Join(", ", criterionList.ToArray().Select(i => i.Code));

			var originCriterion = (CodeDescription)lineItem.OriginCriterion;

			if (builder.InitialFlags.ValidateLineItemOriginCriterion)
			{
				originCriterion.Code = criterionList[0].Code;
			}

			AssertNoMessageErrors(originCriterion.CodeInfo);

			originCriterion.Code = string.Empty;

			if (builder.InitialFlags.ValidateLineItemOriginCriterion)
			{
				AssertHasMessageError(originCriterion.CodeInfo, "Origin Conferring Criterion is required.");
			}
			else
			{
				AssertNoMessageErrors(originCriterion.CodeInfo);
			}

			originCriterion.Code = null;

			if (builder.InitialFlags.ValidateLineItemOriginCriterion)
			{
				AssertHasMessageError(originCriterion.CodeInfo, "Origin Conferring Criterion is required.");
			}
			else
			{
				AssertNoMessageErrors(originCriterion.CodeInfo);
			}

			originCriterion.Code = "CODE";

			if (builder.InitialFlags.ValidateLineItemOriginCriterion)
			{
				AssertHasMessageError(originCriterion.CodeInfo, $"Origin Conferring Criterion is required to be one of the following; '{criterionListOptions}'.");
			}
			else
			{
				AssertNoMessageErrors(originCriterion.CodeInfo);
			}
		}

		public void Test_Validate_ReturnsErrorMessage_When_InvoiceNumberIsEmpty()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();

			if (builder.InitialFlags.InvoiceType == InvoiceType.None)
			{
				lineItem.Invoice = new Invoice();
				AssertNoMessageErrors(lineItem.Invoice.NumberInfo);
			}
			else
			{
				var errorMessage = "Invoice Number required.";

				lineItem.Invoice.Number = "INV0001";

				AssertNoMessageErrors(lineItem.Invoice.NumberInfo);

				lineItem.Invoice.Number = ZString.Empty;

				AssertHasMessageError(lineItem.Invoice.NumberInfo, errorMessage);

				lineItem.Invoice.Number = "INV0001";

				AssertNoMessageErrors(lineItem.Invoice.NumberInfo);

				lineItem.Invoice.Number = ZString.Empty;

				AssertHasMessageError(lineItem.Invoice.NumberInfo, errorMessage);
			}
		}

		public void Test_Validate_ReturnsErrorMessage_When_InvoiceDateIsEmpty()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();

			if (builder.InitialFlags.InvoiceType == InvoiceType.None)
			{
				lineItem.Invoice = new Invoice();
				AssertNoMessageErrors(lineItem.Invoice.DateInfo);
			}
			else
			{
				var errorMessage = "Invoice Date required.";

				lineItem.Invoice.Date = new ZDateTime(2023, 07, 11);

				AssertNoMessageErrors(lineItem.Invoice.DateInfo);

				lineItem.Invoice.Date = ZDateTime.Empty;

				AssertHasMessageError(lineItem.Invoice.DateInfo, errorMessage);

				lineItem.Invoice.Date = new ZDateTime(2023, 07, 11);

				AssertNoMessageErrors(lineItem.Invoice.DateInfo);

				lineItem.Invoice.Date = ZDateTime.Invalid;

				AssertHasMessageError(lineItem.Invoice.DateInfo, errorMessage);
			}
		}

		public void Test_Validate_OptionallyReturnsErrorMessage_When_InvoiceCurrencyIsEmpty()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();

			var currency = (CodeDescription)lineItem.Invoice.Amount.Currency;

			currency.Code = "AUD";

			AssertNoMessageErrors(currency.CodeInfo);

			currency.Code = ZString.Empty;

			if (builder.InitialFlags.InvoiceType == InvoiceType.Complete)
			{
				AssertHasMessageError(currency.CodeInfo, "Invoice Currency required.");
			}
			else
			{
				AssertNoMessageErrors(currency.CodeInfo);
			}

			currency.Code = "AUD";

			AssertNoMessageErrors(currency.CodeInfo);

			currency.Code = null;

			if (builder.InitialFlags.InvoiceType == InvoiceType.Complete)
			{
				AssertHasMessageError(currency.CodeInfo, "Invoice Currency required.");
			}
			else
			{
				AssertNoMessageErrors(currency.CodeInfo);
			}
		}

		public void Test_Validate_OptionallyReturnsErrorMessage_When_InvoiceAmountIsEmpty()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();

			lineItem.Invoice.Amount.Amount = new ZDecimal(10.00m);

			AssertNoMessageErrors(lineItem.Invoice.Amount.AmountInfo);

			lineItem.Invoice.Amount.Amount = -0.01m;

			if (builder.InitialFlags.InvoiceType == InvoiceType.Complete)
			{
				AssertHasMessageError(lineItem.Invoice.Amount.AmountInfo, "Invoice Amount required and must not be a negative value.");
			}
			else
			{
				AssertNoMessageErrors(lineItem.Invoice.Amount.AmountInfo);
			}

			lineItem.Invoice.Amount.Amount = 10.00m;

			AssertNoMessageErrors(lineItem.Invoice.Amount.AmountInfo);
		}

		public void Test_Validation_LineItem_PackageTypeDescription()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			var lineItem = certificate.LineItems.First();
			var packageType = (CodeDescription)lineItem.PackageType;

			AssertNoMessageErrors(packageType.DescriptionInfo);

			packageType.Code = ZString.Empty;
			AssertHasMessageError(packageType.DescriptionInfo, "Code of package type is required.");

			packageType.Code = "PLT";
			AssertNoMessageErrors(packageType.DescriptionInfo);
		}

		#endregion

		public void Test_Validation_PortValidation_PortOfLoading()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var certificate = builder.Build();

			AssertNoMessageErrors(((Country)certificate.PortOfLoading.Country).NameInfo);

			certificate.PortOfLoading.Country.Name = string.Empty;

			if (builder.InitialFlags.ValidatePortOfLoading)
			{
				AssertHasMessageError(((Country)certificate.PortOfLoading.Country).NameInfo, "Port of Loading is required. Please enter in Details section of Shipment.");
			}
			else
			{
				AssertNoMessageErrors(((Country)certificate.PortOfLoading.Country).NameInfo);
			}

			certificate.PortOfLoading.Country.Code = "GB";

			AssertNoMessageErrors(((Country)certificate.PortOfLoading.Country).NameInfo);

			certificate.PortOfLoading.Country.Name = null;

			if (builder.InitialFlags.ValidatePortOfLoading)
			{
				AssertHasMessageError(((Country)certificate.PortOfLoading.Country).NameInfo, "Port of Loading is required. Please enter in Details section of Shipment.");
			}
			else
			{
				AssertNoMessageErrors(((Country)certificate.PortOfLoading.Country).NameInfo);
			}
		}

		public void Test_Validation_PortValidation_PortOfDischarge()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var certificate = builder.Build();

			AssertNoMessageErrors(((Country)certificate.PortOfDischarge.Country).NameInfo);

			certificate.PortOfDischarge.Country.Name = string.Empty;

			if (builder.InitialFlags.ValidatePortOfDischarge)
			{
				AssertHasMessageError(((Country)certificate.PortOfDischarge.Country).NameInfo, "Port of Discharge is required. Please enter in Details section of Shipment.");
			}
			else
			{
				AssertNoMessageErrors(((Country)certificate.PortOfDischarge.Country).NameInfo);
			}

			certificate.PortOfDischarge.Country.Code = Core.Constants.CountryCodes.UnitedKingdom;

			AssertNoMessageErrors(((Country)certificate.PortOfDischarge.Country).NameInfo);

			certificate.PortOfDischarge.Country.Name = null;

			if (builder.InitialFlags.ValidatePortOfDischarge)
			{
				AssertHasMessageError(((Country)certificate.PortOfDischarge.Country).NameInfo, "Port of Discharge is required. Please enter in Details section of Shipment.");
			}
			else
			{
				AssertNoMessageErrors(((Country)certificate.PortOfDischarge.Country).NameInfo);
			}
		}

		public void Test_Validation_PortValidation_PortOfOrigin()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var certificate = builder.Build();

			AssertNoMessageErrors(((Country)certificate.PortOfOrigin.Country).NameInfo);

			certificate.PortOfOrigin.Country.Name = string.Empty;

			if (builder.InitialFlags.ValidatePortOfOrigin)
			{
				AssertHasMessageError(((Country)certificate.PortOfOrigin.Country).NameInfo, "Port of Origin is required.");
			}
			else
			{
				AssertNoMessageErrors(((Country)certificate.PortOfOrigin.Country).NameInfo);
			}

			certificate.PortOfOrigin.Country.Code = "GB";

			AssertNoMessageErrors(((Country)certificate.PortOfOrigin.Country).NameInfo);

			certificate.PortOfOrigin.Country.Name = null;

			if (builder.InitialFlags.ValidatePortOfOrigin)
			{
				AssertHasMessageError(((Country)certificate.PortOfOrigin.Country).NameInfo, "Port of Origin is required.");
			}
			else
			{
				AssertNoMessageErrors(((Country)certificate.PortOfOrigin.Country).NameInfo);
			}
		}

		public void Test_Validation_PortValidation_PortOfDestination()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var certificate = builder.Build();

			AssertNoMessageErrors(((Country)certificate.PortOfDestination.Country).NameInfo);

			certificate.PortOfDestination.Country.Name = string.Empty;

			if (builder.InitialFlags.ValidatePortOfDestination)
			{
				AssertHasMessageError(((Country)certificate.PortOfDestination.Country).NameInfo, "Port of Destination is required.");
			}
			else
			{
				AssertNoMessageErrors(((Country)certificate.PortOfDestination.Country).NameInfo);
			}

			certificate.PortOfDestination.Country.Code = "GB";

			AssertNoMessageErrors(((Country)certificate.PortOfDestination.Country).NameInfo);

			certificate.PortOfDestination.Country.Name = null;

			if (builder.InitialFlags.ValidatePortOfDestination)
			{
				AssertHasMessageError(((Country)certificate.PortOfDestination.Country).NameInfo, "Port of Destination is required.");
			}
			else
			{
				AssertNoMessageErrors(((Country)certificate.PortOfDestination.Country).NameInfo);
			}
		}

		public void Test_Build_PortOfLoadingShouldMatchShipmentLoadingPort_When_CertificateIsCreated()
		{
			var shipment = CreateShipmentCore();
			var certificate = CreateBuilder(shipment).Build();
			AssertEquals(shipment.JS_RL_NKLoadPort, certificate.PortOfLoading.Code);
		}

		public void Test_Build_PortOfLoadingShouldMatchShipmentLoadingPort_When_ConsolsEmpty()
		{
			var shipment = CreateShipmentCore(false);
			var certificate = CreateBuilder(shipment).Build();
			AssertEquals(shipment.JS_RL_NKLoadPort, certificate.PortOfLoading.Code);
		}

		public void TestBuild_PortOfDischargeShouldMatchShipmentDischargePort_When_CertificateIsCreated()
		{
			var shipment = CreateShipmentCore();
			var certificate = CreateBuilder(shipment).Build();
			AssertEquals(shipment.JS_RL_NKDischargePort, certificate.PortOfDischarge.Code);
		}

		public void Test_Build_PortOfDischargeShouldMatchShipmentDischargePort_When_ConsolsEmpty()
		{
			var shipment = CreateShipmentCore(false);
			var certificate = CreateBuilder(shipment).Build();
			AssertEquals(shipment.JS_RL_NKDischargePort, certificate.PortOfDischarge.Code);
		}

		public void Test_Build_DepartureDateShouldComeFromShipment_When_RegardlessOfConsol()
		{
			var shipment = CreateShipmentCore(false);
			var shipmentWithConsol = CreateShipmentCore();
			var certificate = CreateBuilder(shipment).Build();
			var certificateWithConsol = CreateBuilder(shipmentWithConsol).Build();
			AssertEquals(shipment.JS_E_DEP, certificate.DepartureDate);
			AssertEquals(shipmentWithConsol.JS_E_DEP, certificateWithConsol.DepartureDate);
		}

		public void Test_Build_ArrivalDateShouldComeFromShipment_When_RegardlessOfConsol()
		{
			var shipment = CreateShipmentCore(false);
			var shipmentWithConsol = CreateShipmentCore();
			var certificate = CreateBuilder(shipment).Build();
			var certificateWithConsol = CreateBuilder(shipmentWithConsol).Build();
			AssertEquals(shipment.JS_E_ARV, certificate.ArrivalDate);
			AssertEquals(shipmentWithConsol.JS_E_ARV, certificateWithConsol.ArrivalDate);
		}

		public void TestBuild_SignatureValidationErrorIsDisplayed_When_UserSignatureNotUploaded()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);

			GlbStaff.CurrentUser.SignatureImage = null;
			var certificate = builder.Build();

			if (builder.InitialFlags.ValidateSignature)
			{
				AssertHasMessageError(certificate.SignatureErrorInfo, SignatureRequiredMessage);
			}
			else
			{
				AssertNoMessageErrors(certificate.SignatureErrorInfo);
			}

			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 1);
			certificate = builder.Build();
			AssertNoMessageErrors(certificate.SignatureErrorInfo);
		}

		public void Test_DocSending_AddEDocsAsDocSendingObjects()
		{
			const string Name = "CommercialInvoice.pdf";
			const string Description = "Commercial Invoice";

			var shipment = CreateShipmentCore();

			var eDoc1 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], Name, Constants.RefDocTypes.CommercialInvoice);
			var eDoc2 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", Constants.RefDocTypes.Invoice);
			var eDoc3 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "ShippingOrder.pdf", Constants.RefDocTypes.ShippingOrder);

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertEquals(expected: 3, certificate.DocSendingCollection.Count);

			AssertEquals(eDoc1.UniqueKey, certificate.DocSendingCollection[0].Id);
			AssertEquals(eDoc2.UniqueKey, certificate.DocSendingCollection[1].Id);
			AssertEquals(eDoc3.UniqueKey, certificate.DocSendingCollection[2].Id);

			AssertNotNull(certificate.DocSendingCollection[0].ImageData);
			AssertNotNull(certificate.DocSendingCollection[1].ImageData);
			AssertNotNull(certificate.DocSendingCollection[2].ImageData);

			var invoice1DocSendingObject = certificate.DocSendingCollection[0];

			AssertEquals(Name, invoice1DocSendingObject.Name);
			AssertEquals(Constants.RefDocTypes.CommercialInvoice, invoice1DocSendingObject.DocumentType);
			AssertEquals(Description, invoice1DocSendingObject.Description);
			AssertEquals(expected: true, invoice1DocSendingObject.Include);
			AssertEquals(expected: false, invoice1DocSendingObject.Certify);

			AssertEquals(expected: true, certificate.DocSendingCollection[1].Include);
			AssertEquals(expected: false, certificate.DocSendingCollection[2].Include);
		}

		public void Test_ConsignorHasPowerOfAttorney_AddEDocToDocSending()
		{
			const string POAName = "Power of Attorney.pdf";
			const string POADescription = "Power of Attorney";

			const string CIVName = "CommercialInvoice.pdf";

			var shipment = CreateShipmentCore();

			var eDoc = shipment
				.Consignor
				.DocManagerInfo()
				.AddFileOrDocument(new byte[1], POAName, Constants.RefDocTypes.PowerOfAttorney);

			shipment
				.Consignor
				.DocManagerInfo()
				.AddFileOrDocument(new byte[1], CIVName, Constants.RefDocTypes.CommercialInvoice);

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertEquals(expected: 1, certificate.DocSendingCollection.Count);

			var doc = certificate.DocSendingCollection[0];

			AssertEquals(eDoc.UniqueKey, doc.Id);
			AssertNotNull(doc.ImageData);
			AssertEquals(POAName, doc.Name);
			AssertEquals(Constants.RefDocTypes.PowerOfAttorney, doc.DocumentType);
			AssertEquals(POADescription, doc.Description);
			AssertEquals(expected: true, doc.Include);
			AssertEquals(expected: false, doc.Certify);
		}

		public void Test_DocSending_FilterDocType()
		{
			Test_DocSending_FilterDocType(Constants.RefDocTypes.CertificateOfOrigin);
			Test_DocSending_FilterDocType(Constants.RefDocTypes.RequestDocument);
		}

		void Test_DocSending_FilterDocType(string docType)
		{
			var shipment = CreateShipmentCore();

			var eDoc1 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", Constants.RefDocTypes.Invoice);
			var eDoc2 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "ShippingOrder.pdf", Constants.RefDocTypes.ShippingOrder);
			var eDoc3 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "CertificateOfOrigin.pdf", docType);

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertEquals(eDoc1.UniqueKey, certificate.DocSendingCollection[0].Id);
			AssertEquals(eDoc2.UniqueKey, certificate.DocSendingCollection[1].Id);
			AssertEquals(expected: 2, certificate.DocSendingCollection.Count);
			AssertEquals(expected: 0, certificate.DocSendingCollection.Where(x => x.DocumentType == eDoc3.DocType).Count());
		}

		public void Test_SendAmendment_CertificateOfOriginNumberPresent()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);
			shipment.Logs.CreateOrRecreateEventLog(
				Events.MessageAccepted,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				string.Empty,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, builder.ShipmentDocumentName),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, CertificateOfOriginNumber));
			var certificate = builder.Build();

			AssertEquals(CertificateOfOriginNumber, certificate.CertificateOfOriginNumber);
		}

		public void Test_Validate_ReturnsErrorMessage_When_CurrentUserEmailIsEmpty()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertHasMessageError(certificate.CurrentUserErrorInfo, EmailRequiredMessage);

			GlbStaff.CurrentUser.GS_EmailAddress = "abc@xyz.com";
			certificate = builder.Build();
			AssertNoMessageError(certificate.CurrentUserErrorInfo, EmailRequiredMessage);
		}

		#region Implement

		protected ForwardingShipment CreateShipmentCore(
			bool createConsol = true,
			string originCountry = Constants.CountryCodes.NewZealand,
			string destinationCountry = Constants.CountryCodes.China,
			bool applyCustomisations = true)
		{
			var shipment = Factory
				.New<ForwardingShipment>()
				.PopulateData(
					originCountry: originCountry,
					destinationCountry: destinationCountry);

			CreateAddresses(shipment, originCountry: originCountry, destinationCountry: destinationCountry);
			if (createConsol)
			{
				shipment.Consols.Add(
					CreateConsolCore(
						originCountry: originCountry,
						destinationCountry: destinationCountry,
						applyCustomisations: applyCustomisations));
			}

			if (applyCustomisations)
			{
				PopulateShipment(shipment);
			}

			PopulateAddresses(shipment);

			return shipment;
		}

		protected virtual void PopulateShipment(ForwardingShipment shipment)
		{
		}

		protected virtual void PopulateAddresses(ForwardingShipment shipment)
		{
		}

		ForwardingConsol CreateConsolCore(
			string originCountry = Constants.CountryCodes.NewZealand,
			string destinationCountry = Constants.CountryCodes.China,
			bool applyCustomisations = true)
		{
			var consol = Factory
				.New<ForwardingConsol>()
				.PopulateData(
					originCountry: originCountry,
					destinationCountry: destinationCountry,
					etd: new ZDateTime(2020, 01, 01, 00, 00, 00),
					eta: new ZDateTime(2020, 02, 01, 00, 00, 00));

			if (applyCustomisations)
			{
				PopulateConsol(consol);
			}

			return consol;
		}

		protected virtual void PopulateConsol(ForwardingConsol consol)
		{
		}

		protected IReadOnlyCollection<ForwardingPackLine> CreateLineItemsCore(
			ForwardingShipment shipment,
			bool addHarmonizedCodeForCountry = false,
			string originCode = Core.Constants.CountryCodes.NewZealand,
			int numberOfLineItems = 1,
			int hsCodeLength = 6)
		{
			var lineItems = new List<ForwardingPackLine>();

			for (var x = 0; x < Math.Max(1, numberOfLineItems); x++)
			{
				var lineItem = shipment
					.OuterPackLines
					.AddNew()
					.PopulateData(
						shipment,
						detailedDescription: "111111111 222222222 333333333 444444444 555555555\r\n666666666 777777777 888888888 999999999 000000000 111111111 222222222 333333333 444444444", //Count starts at 0
						originCode: originCode,
						hsCodeLength: hsCodeLength,
						harmonizedCodeCountry: addHarmonizedCodeForCountry
							? CreateBuilder(shipment).HarmonisedCodeCountry
							: null);

				PopulatePackLine(lineItem);

				lineItems.Add(lineItem);
			}

			return lineItems.AsReadOnly();
		}

		protected virtual void PopulatePackLine(ForwardingPackLine lineItem)
		{
		}

		void CreateAddresses(
			ForwardingShipment shipment,
			string originCountry = Constants.CountryCodes.NewZealand,
			string destinationCountry = Constants.CountryCodes.China)
		{
			var consignorDocumentaryAddress = Factory
				.New<OrgHeader>()
				.PopulateForCountry(originCountry, fullName: "Consignor");
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorDocumentaryAddress.MainAddress.PK;

			var consigneeDocumentaryAddress = Factory
				.New<OrgHeader>()
				.PopulateForCountry(destinationCountry, fullName: "Consignee");
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryAddress.MainAddress.PK;

			var manufacturerDocumentaryAddress = Factory
				.New<OrgHeader>()
				.PopulateForCountry(originCountry, fullName: "Manufacturer");
			shipment.ManufacturerDocAddress.E2_OA_Address = manufacturerDocumentaryAddress.MainAddress.PK;
		}

		#endregion

		#region Internal Types

		internal enum AddressType
		{
			Importer,
			Exporter,
			Producer
		}

		#endregion
	}
}
