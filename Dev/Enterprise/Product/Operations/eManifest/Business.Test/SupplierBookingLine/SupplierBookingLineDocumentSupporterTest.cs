using System;
using System.Drawing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.eManifest.Business.Testing
{
	[TestedType(typeof(SupplierBookingLineDocumentSupporter))]
	internal class SupplierBookingLineDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			AssertEquals(true, supplierBookingLine.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.MaintainShipmentCustomiseDocuments, supplierBookingLine.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.SupplierBookingLine, supplierBookingLine.DocumentSupporter.BusinessContext);
		}

		public void TestGetAlternateBranding()
		{
			var factory = new BusinessObjectFactory();
			var company1 = factory.NewWithValidTestData<OrgHeader>();
			company1.OH_Code = "ABC";
			company1.OH_IsLocalTransport = true;
			company1.OH_IsShippingProvider = true;

			var company2 = factory.NewWithValidTestData<OrgHeader>();
			company2.OH_Code = "DEF";
			company2.OH_IsLocalTransport = true;
			company2.OH_IsShippingProvider = true;

			var company3 = factory.NewWithValidTestData<OrgHeader>();
			company3.OH_Code = "GHI";
			company3.OH_IsLocalTransport = true;
			company3.OH_IsShippingProvider = true;

			factory.Save();

			var brandingCollection = new LocalTransportCompanyBrandingCollection();

			var branding1 = brandingCollection.AddNew();
			branding1.LocalTransportCompanyPK = company1.PK;
			branding1.Image = new Bitmap(10, 10);

			var branding2 = brandingCollection.AddNew();
			branding2.LocalTransportCompanyPK = company2.PK;
			branding2.Image = new Bitmap(10, 10);

			DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brandingCollection);

			var supplierBookingLine = factory.New<SupplierBookingLine>();

			supplierBookingLine.DL_OH_LastMileCarrier = ZGuid.Empty;
			AssertEquals(null, supplierBookingLine.DocumentSupporter.GetAlternativeBranding());

			supplierBookingLine.DL_OH_LastMileCarrier = company1.PK;
			AssertEquals(branding1.Code, supplierBookingLine.DocumentSupporter.GetAlternativeBranding().Code);

			supplierBookingLine.DL_OH_LastMileCarrier = company2.PK;
			AssertEquals(branding2.Code, supplierBookingLine.DocumentSupporter.GetAlternativeBranding().Code);

			supplierBookingLine.DL_OH_LastMileCarrier = company3.PK;
			AssertEquals(null, supplierBookingLine.DocumentSupporter.GetAlternativeBranding());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			supplierBookingLine = Factory.New<SupplierBookingLine>();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<SupplierBookingLine>();
		}

		SupplierBookingLine supplierBookingLine;

		#endregion
	}
}
