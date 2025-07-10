using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class USNMFSVesselsAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_NetWeight()
		{
			Vessel.US_NetWeight = 1.2;
			AssertNoMessageErrorContaining(Vessel.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Vessel.US_NetWeightInfo, "Please enter a number greater than 0.");
			Vessel.US_NetWeight = ZDecimal.Zero;
			AssertHasMessageErrorContaining(Vessel.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
			Vessel.US_NetWeight = -1.0;
			AssertHasMessageErrorContaining(Vessel.US_NetWeightInfo, "Please enter a number greater than 0.");
		}

		public void TestCheckUS_NetWeightUQ()
		{
			Vessel.US_NetWeightUQ = "XX";
			AssertHasMessageErrorContaining(Vessel.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
			Vessel.US_NetWeight = 1.0;
			Vessel.US_NetWeightUQ = "KG";
			AssertNoMessageErrorContaining(Vessel.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(Vessel.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			Vessel.US_NetWeightUQ = "";
			AssertHasMessageErrorContaining(Vessel.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_HarvestedVessel()
		{
			Vessel.US_HarvestedVessel = "XXYYZZ";
			AssertNoMessageErrors("vessel code validation against lookup list was intentionnaly disabled", Vessel.US_HarvestedVesselInfo);

			Vessel.US_HarvestedVessel = "";
			AssertHasMessageErrorContaining(Vessel.US_HarvestedVesselInfo, MandatoryValidation.YouHaveNotEntered);
			var vl = Factory.New<RefVessel>();
			vl.RV_Code = "TestTestTest";
			Vessel.US_HarvestedVessel = vl.RV_Code;
			AssertNoMessageErrorContaining(Vessel.US_HarvestedVesselInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_HarvestedCountry()
		{
			Vessel.US_HarvestedCountry = "XX";
			AssertHasMessageErrorContaining(Vessel.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
			Vessel.US_HarvestedCountry = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(Vessel.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(Vessel.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
			Vessel.US_HarvestedCountry = "";
			AssertHasMessageErrorContaining(Vessel.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_TranshipmentPlace()
		{
			Vessel.US_TranshipmentPlace = "";
			Vessel.AddInfoValidation.ValidateUS_TranshipmentPlace();
			AssertHasMessageError(Vessel.US_TranshipmentPlaceInfo, USNMFSVesselsAddInfoValidation.TransshipmentPlaceOrFirstLandingCountryRequired);
			AssertNoMessageErrorContaining(Vessel.US_TranshipmentPlaceInfo, ListValidation.InvalidCodeMessageError);

			Vessel.US_TranshipmentPlace = "XX";
			AssertHasMessageErrorContaining(Vessel.US_TranshipmentPlaceInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Vessel.US_TranshipmentPlaceInfo, USNMFSVesselsAddInfoValidation.TransshipmentPlaceOrFirstLandingCountryRequired);

			Vessel.US_TranshipmentPlace = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(Vessel.US_TranshipmentPlaceInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Vessel.US_TranshipmentPlaceInfo, USNMFSVesselsAddInfoValidation.TransshipmentPlaceOrFirstLandingCountryRequired);

			Vessel.US_TranshipmentPlace = "ZZ";
			AssertNoMessageErrorContaining(Vessel.US_TranshipmentPlaceInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Vessel.US_TranshipmentPlaceInfo, USNMFSVesselsAddInfoValidation.TransshipmentPlaceOrFirstLandingCountryRequired);
		}

		public void TestCheckUS_FirstLandingCountry()
		{
			Vessel.US_FirstLandingCountry = "";
			Vessel.AddInfoValidation.ValidateUS_FirstLandingCountry();
			AssertHasMessageError(Vessel.US_FirstLandingCountryInfo, USNMFSVesselsAddInfoValidation.TransshipmentPlaceOrFirstLandingCountryRequired);
			AssertNoMessageErrorContaining(Vessel.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);

			Vessel.US_FirstLandingCountry = "XX";
			AssertHasMessageErrorContaining(Vessel.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Vessel.US_FirstLandingCountryInfo, USNMFSVesselsAddInfoValidation.TransshipmentPlaceOrFirstLandingCountryRequired);

			Vessel.US_FirstLandingCountry = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(Vessel.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Vessel.US_FirstLandingCountryInfo, USNMFSVesselsAddInfoValidation.TransshipmentPlaceOrFirstLandingCountryRequired);

			Vessel.US_FirstLandingCountry = "ZZ";
			AssertNoMessageErrorContaining(Vessel.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Vessel.US_FirstLandingCountryInfo, USNMFSVesselsAddInfoValidation.TransshipmentPlaceOrFirstLandingCountryRequired);
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		NMFSLine NMFSLine
		{
			get
			{
				if (nmfsLine == null)
				{
					nmfsLine = InvoiceLine.NMFSLines.AddNew();
					nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
					NMFSLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
				}
				return nmfsLine;
			}
		}
		NMFSLine nmfsLine;

		NMFSHarvestingDetail NMFSDetail
		{
			get
			{
				if (nmfsDetail == null)
				{
					nmfsDetail = NMFSLine.HarvestingDetails.AddNew();
				}
				return nmfsDetail;
			}
		}

		NMFSHarvestingDetail nmfsDetail;

		NMFSVessels Vessel
		{
			get
			{
				if (vessel == null)
				{
					vessel = NMFSDetail.HarvestingVessles.AddNew();
				}
				return vessel;
			}
		}

		NMFSVessels vessel;

		#endregion
	}
}
