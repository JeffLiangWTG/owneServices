using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGAFD01Test : OGABIRDUpdateTest
	{
		public void TestUpdateFDAManufacturerAddressByCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			var fda = Factory.New<FDA>();
			var notifications = new NotificationBuffer();

			var oGAFD01 = new OGAFD01() { FDAActualManufacturerNumber = "PHEVEAPP80SAB" };
			((IBIRDOGALineRecord)oGAFD01).Update(fda, notifications);
			AssertEquals("Matched address found by MID", orgAddress1.PK, fda.US_FDAManufacturerAddress);

			notifications.Clear();
			fda.US_FDAManufacturerAddress = ZGuid.Empty;
			oGAFD01 = new OGAFD01() { FDAActualManufacturerNumber = "  PHEVEAPP80SAB" };
			((IBIRDOGALineRecord)oGAFD01).Update(fda, notifications);
			AssertEquals("No matched address found or created because MID is invalid", ZGuid.Empty, fda.US_FDAManufacturerAddress);
			AssertContains(ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, "  PHEVEAPP80SAB"), notifications.AsString.Trim());

			notifications.Clear();
			fda.US_FDAManufacturerAddress = ZGuid.Empty;
			oGAFD01 = new OGAFD01() { FDAActualManufacturerNumber = "PHEVEAPP80SCD" };
			((IBIRDOGALineRecord)oGAFD01).Update(fda, notifications);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertEquals("New organization and address is created", org.MainAddress.PK, fda.US_FDAManufacturerAddress);
		}

		public void TestUpdateFDAShipperAddressByCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			var fda = Factory.New<FDA>();
			var notifications = new NotificationBuffer();

			var oGAFD01 = new OGAFD01() { FDAActualShipperSupplierNumber = "PHEVEAPP80SAB" };
			((IBIRDOGALineRecord)oGAFD01).Update(fda, notifications);
			AssertEquals("Matched address found by MID", orgAddress1.PK, fda.US_FDAShipperAddress);

			notifications.Clear();
			fda.US_FDAManufacturerAddress = ZGuid.Empty;
			oGAFD01 = new OGAFD01() { FDAActualShipperSupplierNumber = "  PHEVEAPP80SAB" };
			((IBIRDOGALineRecord)oGAFD01).Update(fda, notifications);
			AssertEquals("No matched address found or created because MID is invalid", ZGuid.Empty, fda.US_FDAShipperAddress);
			AssertContains(ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, "  PHEVEAPP80SAB"), notifications.AsString.Trim());

			notifications.Clear();
			fda.US_FDAManufacturerAddress = ZGuid.Empty;
			oGAFD01 = new OGAFD01() { FDAActualShipperSupplierNumber = "PHEVEAPP80SCD" };
			((IBIRDOGALineRecord)oGAFD01).Update(fda, notifications);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertEquals("New organization and address is created", org.MainAddress.PK, fda.US_FDAShipperAddress);
		}

		public void TestFDAShipperWritesToFDALine()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AEABCEXP6390ALE", GlbCompany.CurrentCompany.Country);

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "KRERWYUI7890AS", GlbCompany.CurrentCompany.Country);
			Factory.Save();

			var fd01SLN = new OGAFD01();
			fd01SLN.FDALineNumber = 1;
			fd01SLN.FDAProductCode = "23-2878";
			fd01SLN.CargoStorageStatus = CargoStorageCodeList.Codes.AmbientTemperature;
			fd01SLN.FDACountryOfProduction = "AU";
			fd01SLN.AffirmationOfComplianceCode = "";
			fd01SLN.AffirmationOfComplianceQualifier = "Mr. Broker";
			fd01SLN.FDAActualManufacturerNumber = "AEABCEXP6390ALE";
			fd01SLN.FDAActualShipperSupplierNumber = "KRERWYUI7890AS";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			var fda = invoiceLine.FDAs.AddNew();

			((IBIRDOGALineRecord)fd01SLN).Update(fda, new NotificationCollection());
			AssertEquals(0, fda.AffirmationCodes.Count);
			fd01SLN.AffirmationOfComplianceCode = AffirmationCodeConstants.Codes.SLN;
			((IBIRDOGALineRecord)fd01SLN).Update(fda, new NotificationCollection());
			AssertEquals(1, fda.AffirmationCodes.Count);
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_SupplierAddress);
			AssertEquals(shipper.MainAddress.PK, fda.US_FDAShipperAddress);
		}

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var fd01SLN = new OGAFD01();
			fd01SLN.FDALineNumber = 1;
			fd01SLN.FDAProductCode = "23-2878";
			fd01SLN.CargoStorageStatus = CargoStorageCodeList.Codes.AmbientTemperature;
			fd01SLN.FDACountryOfProduction = "AU";
			fd01SLN.AffirmationOfComplianceCode = AffirmationCodeConstants.Codes.SLN;
			fd01SLN.AffirmationOfComplianceQualifier = "Mr. Broker";
			fd01SLN.FDAActualManufacturerNumber = "AUABCEXP6390ALE";
			fd01SLN.FDAActualShipperSupplierNumber = "GBERWYUI7890AS";

			var fd01TEM = new OGAFD01();
			fd01TEM.FDALineNumber = 1;
			fd01TEM.FDAProductCode = "23-2878";
			fd01TEM.CargoStorageStatus = CargoStorageCodeList.Codes.AmbientTemperature;
			fd01TEM.FDACountryOfProduction = "AU";
			fd01TEM.AffirmationOfComplianceCode = AffirmationCodeConstants.Codes.TEM;
			fd01TEM.AffirmationOfComplianceQualifier = "NONE";
			fd01TEM.FDAActualManufacturerNumber = "AUABCEXP6390ALE";
			fd01TEM.FDAActualShipperSupplierNumber = "GBERWYUI7890AS";

			var fd01PND = new OGAFD01();
			fd01PND.FDALineNumber = 1;
			fd01PND.FDAProductCode = "23-2878";
			fd01PND.CargoStorageStatus = CargoStorageCodeList.Codes.AmbientTemperature;
			fd01PND.FDACountryOfProduction = "AU";
			fd01PND.AffirmationOfComplianceCode = AffirmationCodeConstants.Codes.PND;
			fd01PND.FDAActualManufacturerNumber = "AUABCEXP6390ALE";
			fd01PND.FDAActualShipperSupplierNumber = "GBERWYUI7890AS";

			var fd01PNC = new OGAFD01();
			fd01PNC.FDALineNumber = 1;
			fd01PNC.FDAProductCode = "23-2878";
			fd01PNC.CargoStorageStatus = CargoStorageCodeList.Codes.AmbientTemperature;
			fd01PNC.FDACountryOfProduction = "AU";
			fd01PNC.AffirmationOfComplianceCode = AffirmationCodeConstants.Codes.PNC;
			fd01PNC.AffirmationOfComplianceQualifier = "YF78EW879HJJ";
			fd01PNC.FDAActualManufacturerNumber = "AUABCEXP6390ALE";
			fd01PNC.FDAActualShipperSupplierNumber = "GBERWYUI7890AS";

			return new IBIRDOGALineRecord[] { fd01SLN, fd01TEM, fd01PND, fd01PNC };
		}

		protected override void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
			base.PrepareJob(invoiceLine, ogaLine, ogaRecord);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fdaLine = (FDA)ogaLine;
			fdaLine.US_FDAForcePN = true;
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine)
		{
			var fdaLine = invoiceLine.FDAs.AddNew();
			fdaLine.US_OFT = "";

			return fdaLine;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(OGAFD01);

		protected override void SetUp()
		{
			base.SetUp();

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUABCEXP6390ALE", GlbCompany.CurrentCompany.Country);

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "GBERWYUI7890AS", GlbCompany.CurrentCompany.Country);

			Factory.Save();
		}
	}
}
