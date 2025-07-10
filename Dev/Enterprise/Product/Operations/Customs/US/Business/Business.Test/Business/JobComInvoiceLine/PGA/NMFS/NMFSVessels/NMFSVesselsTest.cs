using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSVessels))]
	public class NMFSVesselsTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NMFSVessels>
	{
		public void TestProperties()
		{
			NMFSDetail.Parent.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			var harvestingVessel = NMFSDetail.HarvestingVessles.AddNew();
			AssertEquals("harvestingDetail.US_HarvestedCountryInfo.ReadOnly", false, harvestingVessel.US_HarvestedCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_TranshipmentPlaceInfo.ReadOnly", false, harvestingVessel.US_TranshipmentPlaceInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_FirstLandingCountryInfo.ReadOnly", false, harvestingVessel.US_FirstLandingCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_HarvestedVesselInfo.ReadOnly", false, harvestingVessel.US_HarvestedVesselInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_NetWeightInfo.ReadOnly", false, harvestingVessel.US_NetWeightInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_NetWeightUQInfo.ReadOnly", false, harvestingVessel.US_NetWeightUQInfo.ReadOnly);
		}

		public void TestINMFSVesselMembersForSIM()
		{
			var harvestingVessel = NMFSDetail.HarvestingVessles.AddNew();
			harvestingVessel.US_HarvestedCountry = "C1";
			harvestingVessel.US_TranshipmentPlace = "C2";
			harvestingVessel.US_FirstLandingCountry = "C3";
			harvestingVessel.US_HarvestedVessel = "V1";
			harvestingVessel.US_NetWeight = 1.38;
			harvestingVessel.US_NetWeightUQ = "KG";

			INMFSVessel vessel = harvestingVessel;
			AssertEquals("HarvestedCountry", "C1", vessel.HarvestedCountry);
			AssertEquals("HarvestedVessel", "V1", vessel.HarvestedVessel);
			AssertEquals("TranshipmentPlace", "C2", vessel.TranshipmentPlace);
			AssertEquals("FitsLandingCountry", "C3", vessel.FirstLandingCountry);
			AssertEquals("NetWeight", 1.38m, vessel.NetWeight);
			AssertEquals("NetWeightUQ", "KG", vessel.NetWeightUQ);
		}

		public void TestClone()
		{
			var harvestingVessel = NMFSDetail.HarvestingVessles.AddNew();
			harvestingVessel.US_HarvestedCountry = "C1";
			harvestingVessel.US_TranshipmentPlace = "C2";
			harvestingVessel.US_FirstLandingCountry = "C3";
			harvestingVessel.US_HarvestedVessel = "V1";
			harvestingVessel.US_NetWeight = 1.38;
			harvestingVessel.US_NetWeightUQ = "KG";

			var clonedHarvestingVessel = (NMFSVessels)harvestingVessel.Clone();
			AssertEquals("clonedHarvestingVessel.US_HarvestedCountry", "C1", clonedHarvestingVessel.US_HarvestedCountry);
			AssertEquals("clonedHarvestingVessel.US_TranshipmentPlace", "C2", clonedHarvestingVessel.US_TranshipmentPlace);
			AssertEquals("clonedHarvestingVessel.US_FirstLandingCountry", "C3", clonedHarvestingVessel.US_FirstLandingCountry);
			AssertEquals("clonedHarvestingVessel.US_HarvestedVessel", "V1", clonedHarvestingVessel.US_HarvestedVessel);
			AssertEquals("clonedHarvestingVessel.US_NetWeight", 1.38m, clonedHarvestingVessel.US_NetWeight);
			AssertEquals("clonedHarvestingVessel.US_NetWeightUQ", "KG", clonedHarvestingVessel.US_NetWeightUQ);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			var harvestingVessles = harvestingDetail.HarvestingVessles.AddNew();
			harvestingVessles.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			return harvestingVessles;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NMFSDetail.HarvestingVessles.AddNew();
		}

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
					nmfsLine.US_SourceType = "HCF";
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
		#endregion
	}
}
