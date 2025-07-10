using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGADT01Test : OGABIRDUpdateTest
	{
		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var dt01 = new OGADT01();
			dt01.DOTLineNumber = 1;
			dt01.BoxNumber = "01";
			dt01.BoxCertification = "Y";
			dt01.DOTBondSuretyCode = "891";
			dt01.ClarificationCode = ClarificationCodeList.Codes.Equipment;
			dt01.CountryISO = "AU";
			dt01.NHTSAPermissionLetterOfficialOrdersCertification = YesNoDefaultList.Codes.Yes;
			dt01.ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter = "";
			dt01.PassportNumber = "3333";

			var dtTire = new OGADT01();
			dtTire.DOTLineNumber = 1;
			dtTire.BoxNumber = "09";
			dtTire.BoxCertification = "Y";
			dtTire.ClarificationCode = ClarificationCodeList.Codes.Tire;
			dtTire.CountryISO = "AU";
			dtTire.NHTSAPermissionLetterOfficialOrdersCertification = "";
			dtTire.ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter = YesNoDefaultList.Codes.Yes;
			dtTire.PassportNumber = "3333";
			dtTire.TireManufacturerIDCode = "054";
			dtTire.TireManufacturerBrandName = "Brand Name";
			return new IBIRDOGALineRecord[] { dt01, dtTire };
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.DOTs.AddNew();

		protected override void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
			base.PrepareJob(invoiceLine, ogaLine, ogaRecord);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(OGADT01);
	}
}
