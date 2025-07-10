using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(InvoiceOrgHeaderAddressLinkUpdater))]
	public class InvoiceOrgHeaderAddressLinkUpdaterTest : TestCaseWithFactory
	{
		public void TestImporterOrganizationLinkToAddress()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var invoiceHeader = Factory.CreateInvoiceHeader(shipment);
			AssertOrganizationLinkToAddress
			(
				nameof(InvoiceOrgHeaderIndex.Importer),
				[x => invoiceHeader.GIH_OH_Importer = x, x => invoiceHeader.GIH_OA_ImporterAddress = x],
				[() => invoiceHeader.GIH_OH_Importer, () => invoiceHeader.GIH_OA_ImporterAddress]
			);
		}

		public void TestSupplierOrganizationLinkToAddress()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var invoiceHeader = Factory.CreateInvoiceHeader(shipment);
			AssertOrganizationLinkToAddress
			(
				nameof(InvoiceOrgHeaderIndex.Supplier),
				[x => invoiceHeader.GIH_OH_Supplier = x, x => invoiceHeader.GIH_OA_SupplierAddress = x],
				[() => invoiceHeader.GIH_OH_Supplier, () => invoiceHeader.GIH_OA_SupplierAddress]
			);
		}

		public void TestImporterAddressLinkToOrganization()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var invoiceHeader = Factory.CreateInvoiceHeader(shipment);
			AssertAddressLinkToOrganization
			(
				nameof(InvoiceOrgHeaderIndex.Importer),
				x => invoiceHeader.GIH_OA_ImporterAddress = x,
				() => invoiceHeader.GIH_OA_ImporterAddress,
				() => invoiceHeader.GIH_OH_Importer
			);
		}

		public void TestSupplierAddressLinkToOrganization()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var invoiceHeader = Factory.CreateInvoiceHeader(shipment);
			AssertAddressLinkToOrganization
			(
				nameof(InvoiceOrgHeaderIndex.Supplier),
				x => invoiceHeader.GIH_OA_SupplierAddress = x,
				() => invoiceHeader.GIH_OA_SupplierAddress,
				() => invoiceHeader.GIH_OH_Supplier
			);
		}

		#region Common Assertions

		void AssertAddressLinkToOrganization(string sourceOrganization, Action<ZGuid> setOrgAddress, Func<ZGuid> getOrgAddressPK, Func<ZGuid> getOrgHeaderPK)
		{
			var orgHeader = Factory.CreateNewOrganization();
			var orgAddress = orgHeader.MainAddress;
			setOrgAddress(orgAddress.PK);

			CombineAssertions($"{sourceOrganization} address link to organization", () =>
			{
				AssertEquals(orgAddress.PK, getOrgAddressPK());
				AssertEquals(orgHeader.PK, getOrgHeaderPK());
			});
		}

		void AssertOrganizationLinkToAddress(string sourceOrganization, Action<ZGuid>[] setOrgHeaderAddress, Func<ZGuid>[] getOrgHeaderAddress)
		{
			var orgHeader = Factory.CreateNewOrganization();
			var orgAddress = orgHeader.MainAddress;

			setOrgHeaderAddress[0](orgHeader.PK);
			setOrgHeaderAddress[1](orgAddress.PK);
			AssertResults($"Precondition: {sourceOrganization}", orgHeader.PK, orgAddress.PK, getOrgHeaderAddress[0](), getOrgHeaderAddress[1]());

			var orgAddress2 = Factory.CreateNewOrganizationAddress(orgHeader);
			setOrgHeaderAddress[1](orgAddress2.PK);
			AssertResults($"{sourceOrganization} (Additional Address):", orgHeader.PK, orgAddress2.PK, getOrgHeaderAddress[0](), getOrgHeaderAddress[1]());

			setOrgHeaderAddress[0](ZGuid.Empty);
			AssertResults($"{sourceOrganization}", ZGuid.Empty, ZGuid.Empty, getOrgHeaderAddress[0](), getOrgHeaderAddress[1]());

			void AssertResults(string message, ZGuid expectedHeaderPK, ZGuid expectedAddressPK, ZGuid actualHeaderPK, ZGuid actualAddressPK)
			{
				AssertEquals($"{message} organization PK should be: {expectedHeaderPK}", expectedHeaderPK, actualHeaderPK);
				AssertEquals($"{message} address PK should be: {expectedAddressPK}", expectedAddressPK, actualAddressPK);
			}
		}

		#endregion
	}
}
