using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.eManifest.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.eManifest.Business.Testing
{
	[TestedType(typeof(SupplierBookingHeaderDocumentSupporter))]
	internal class SupplierBookingHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			AssertEquals(true, supplierBookingHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.MaintainShipmentCustomiseDocuments, supplierBookingHeader.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.SupplierBookingHdr, supplierBookingHeader.DocumentSupporter.BusinessContext);
		}

		public void TestSupportedChildBusinessContexts()
		{
			AssertEquals(1, supplierBookingHeader.DocumentSupporter.SupportedChildBusinessContexts.Length);
			AssertEquals(BusinessContext.SupplierBookingLine, supplierBookingHeader.DocumentSupporter.SupportedChildBusinessContexts[0]);
		}

		public void TestGetChildCollection()
		{
			AssertEquals(0, supplierBookingHeader.DocumentSupporter.GetChildCollection(null, BusinessContext.SupplierBookingLine, null).Length);

			var supplierBookingLine1 = supplierBookingHeader.BookingLines.AddNew();
			supplierBookingLine1.DL_ConsigneeReference = "3";

			var supplierBookingLine2 = supplierBookingHeader.BookingLines.AddNew();
			supplierBookingLine2.DL_ConsigneeReference = "1";

			var supplierBookingLine3 = supplierBookingHeader.BookingLines.AddNew();
			supplierBookingLine3.DL_ConsigneeReference = "2";

			AssertEquals(3, supplierBookingHeader.DocumentSupporter.GetChildCollection(null, BusinessContext.SupplierBookingLine, null).Length);

			AssertEquals(supplierBookingLine2, supplierBookingHeader.DocumentSupporter.GetChildCollection(null, BusinessContext.SupplierBookingLine, null)[0]);
			AssertEquals(supplierBookingLine3, supplierBookingHeader.DocumentSupporter.GetChildCollection(null, BusinessContext.SupplierBookingLine, null)[1]);
			AssertEquals(supplierBookingLine1, supplierBookingHeader.DocumentSupporter.GetChildCollection(null, BusinessContext.SupplierBookingLine, null)[2]);
		}

		public void TestShouldUseStreamingMode()
		{
			//The document name 'HVLV Delivery Label' is hard coded for checking if streaming mode should be used.
			//Currently it is only used on two document menu items - 'HVLV Delivery Label' with business context shipment and HVLVBookingHeader
			//If the these two menu items are modified sometime in the future we should check if streaming mode should still be applied
			for (int i = 0; i < HVLVDeliveryLabelHelper.NumberOfDocsPerPackForStreamingMode + 1; i++)
			{
				supplierBookingHeader.BookingLines.AddNew();
			}

			var documentSupporter = new SupplierBookingHeaderDocumentSupporter(supplierBookingHeader);
			ZQuery query = new ZQuery(StmMenuItemSchema.SU_MenuName, HVLVDeliveryLabelHelper.MenuItemName);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.SupplierBookingHdr));

			Assert(supplierBookingHeader.BookingLines.Count > HVLVDeliveryLabelHelper.NumberOfDocsPerPackForStreamingMode);
			var hvlvForBookingHeader = Factory.LoadTop1<DocumentCommand>(query);
			AssertNotNull("Should check for printing mode if this menu item is modified", hvlvForBookingHeader);
			AssertEquals("Should check for printing mode if this menu item is modified", new ZGuid("1CAEFEDB-D6BD-4F2B-AFC0-DA34A1A99F65"), hvlvForBookingHeader.PK);
			Assert(((ISupportCustomizedDocumentPrintSet)documentSupporter).ShouldCustomizedDocumentPrintSet(hvlvForBookingHeader));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			supplierBookingHeader = Factory.New<SupplierBookingHeader>();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var header = Factory.New<SupplierBookingHeader>();

			for (int i = 0; i < HVLVDeliveryLabelHelper.NumberOfDocsPerPackForStreamingMode + 1; i++)
			{
				header.BookingLines.AddNew();
			}

			return header;
		}

		SupplierBookingHeader supplierBookingHeader;

		#endregion
	}
}
