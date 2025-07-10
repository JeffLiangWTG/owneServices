using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base
{
	abstract class CertificateOfOriginDocumentTests : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		protected abstract string CountryCode { get; }
		protected abstract ZGuid TemplatePivotPK { get; }
		protected abstract string CreateContent();
		protected abstract string CreateContentForShipmentUsingInvoiceLines();

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AssertDocumentContent();
			}
		}

		void AssertDocumentContent()
		{
			var shipment = CreateShipment();
			CreatePackingLineItems(shipment);
			AssertContents(shipment, TemplatePivotPK, CreateContent());
		}

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public virtual void TestDocumentContentWithInvoices()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = CreateShipment<ForwardingShipmentForTest>();
				CreateInvoiceLineItems(shipment);
				AssertContents(shipment, TemplatePivotPK, CreateContentForShipmentUsingInvoiceLines());
			}
		}

		#region Implement

		protected ForwardingShipment CreateShipment(
			string originCountry = Constants.CountryCodes.NewZealand,
			string destinationCountry = Constants.CountryCodes.China) =>
				CreateShipment<ForwardingShipment>(originCountry, destinationCountry);

		protected T CreateShipment<T>(
			string originCountry = Constants.CountryCodes.NewZealand,
			string destinationCountry = Constants.CountryCodes.China) where T : ForwardingShipment
		{
			var shipment = Factory
				.New<T>()
				.PopulateData(
					originCountry: originCountry,
					destinationCountry: destinationCountry,
					departure: new ZDateTime(2021, 11, 11, 00, 00, 00));

			PopulateShipment(shipment);

			CreateAddresses(shipment);

			shipment.Consols.Add(CreateConsol(
				originCountry: originCountry,
				destinationCountry: destinationCountry));

			return shipment;
		}

		protected virtual void PopulateShipment(ForwardingShipment shipment)
		{
		}

		ForwardingConsol CreateConsol(
			string originCountry = Constants.CountryCodes.NewZealand,
			string destinationCountry = Constants.CountryCodes.China)
		{
			var consol = Factory
				.New<ForwardingConsol>()
				.PopulateData(
					originCountry: originCountry,
					destinationCountry: destinationCountry);

			PopulateConsol(consol);

			return consol;
		}

		protected virtual void PopulateConsol(ForwardingConsol consol)
		{
		}

		protected void CreatePackingLineItems(ForwardingShipment shipment, int numberOfLineItems = 1)
		{
			for (var x = 0; x < Math.Max(1, numberOfLineItems); x++)
			{
				PopulatePackingLineItem(shipment
					.OuterPackLines
					.AddNew()
					.PopulateData(shipment));
			}
		}

		protected virtual void PopulatePackingLineItem(ForwardingPackLine lineItem)
		{
		}

		protected void CreateInvoiceLineItems(ForwardingShipmentForTest shipment, int numberOfLineItems = 1)
		{
			var declarations = new List<BaseJobDeclaration>(numberOfLineItems);
			for (var x = 0; x < Math.Max(1, numberOfLineItems); x++)
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				var invoiceHeader = declaration.Invoices.AddNew();
				PopulateInvoiceHeader(invoiceHeader.PopulateData());

				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				PopulateInvoiceLine(invoiceLine.PopulateData());

				declarations.Add(declaration);
			}

			shipment.SetDeclaration(declarations.ToArray());
		}

		protected virtual void PopulateInvoiceHeader(BaseJobComInvoiceHeader invoiceHeader)
		{
		}
		protected virtual void PopulateInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
		}

		void CreateAddresses(ForwardingShipment shipment)
		{
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = CreateConsignorAddress(shipment).MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = CreateConsigneeAddress(shipment).MainAddress.PK;
			shipment.ManufacturerDocAddress.E2_OA_Address = CreateManufacturerAddress(shipment).MainAddress.PK;
			PopulateAdditionalAddresses(shipment);
		}

		protected virtual OrgHeader CreateConsignorAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Australia, fullName: "Consignor");

		protected virtual OrgHeader CreateConsigneeAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Singapore, fullName: "Consignee");

		protected virtual OrgHeader CreateManufacturerAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Australia, fullName: "Manufacturer");

		protected virtual void PopulateAdditionalAddresses(ForwardingShipment shipment)
		{
		}

		#endregion
	}
}
