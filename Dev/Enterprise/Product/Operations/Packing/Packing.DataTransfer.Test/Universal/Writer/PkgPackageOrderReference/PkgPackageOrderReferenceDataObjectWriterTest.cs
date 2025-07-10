using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	class PkgPackageOrderReferenceDataObjectWriterTest : PackingTestCaseWithFactory
	{
		public void TestGetDataObjectRequiresOrderLineDictionary()
		{
			var orderReference = Factory.New<PkgPackageOrderReference>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PkgPackageOrderReferenceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderReference)), null).GetDataObject(orderReference));
		}

		public void TestGetDataObject_OrderReference()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var package = pkgPackageJob.Packages.AddNew("BOX");

			var orderReference = Factory.New<PkgPackageOrderReference>();
			orderReference.KPO_KP_Package = package.PK;
			orderReference.KPO_OrderNumber = "ORN";
			orderReference.KPO_BatchNumber = "BAT";
			orderReference.KPO_CommercialInvoiceNumber = "CIN";
			orderReference.KPO_ExpiryDate = new ZDate(2024, 01, 01);
			orderReference.KPO_LineReference = "LNE";
			orderReference.KPO_SKUPartNumber = "SKU";
			orderReference.KPO_SerialNumber = "SRN";

			Factory.Save();

			var orderReferenceDictionary = new Dictionary<ZGuid, ZInt>();
			orderReferenceDictionary.Add(package.PK, 1);

			var shipment = new PkgPackageOrderReferenceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderReference)), orderReferenceDictionary).GetDataObject(orderReference);

			AssertNotNull("DataContext should be created", shipment.DataContext);
			AssertEquals("ORN", shipment.Order.OrderNumber);
			var orderLine = shipment.Order.OrderLineCollection.First();
			AssertEquals("BAT", orderLine.BatchNumber);
			AssertEquals("CIN", orderLine.CommercialInvoiceNumber);
			AssertEquals(new ZDateTime(2024, 01, 01), orderLine.ExpiryDate);
			AssertEquals("LNE", orderLine.LineReference);
			AssertEquals(1, orderLine.Link);
			AssertEquals("SKU", orderLine.Product.Code);
			AssertEquals("SRN", orderLine.SerialNumber);
		}
	}
}
