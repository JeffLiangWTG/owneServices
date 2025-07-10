using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FishingInformation))]
	public class FishingInformationTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<FishingInformation>
	{
		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			var fishing = invoiceLine.FishingInformations.AddNew();
			AssertEquals("FishingInformation.IsVesselMethod", false, fishing.IsVesselMethod);
			Factory.Save();
			AssertEquals("FishingInformation.IsDeleted", true, fishing.IsDeleted);

			fishing = invoiceLine.FishingInformations.AddNew();
			fishing.US_MethodOfHarvest = SourceTypeCodesList.Codes.Vessel;
			AssertEquals("FishingInformation.IsVesselMethod", true, fishing.IsVesselMethod);
			Factory.Save();
			AssertEquals("FishingInformation.IsDeleted", false, fishing.IsDeleted);
		}

		public void TestDefaultVesselCountryAndVesselIMOWhenSetVesselName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			var fishing = invoiceLine.FishingInformations.AddNew();
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "vessel test";
			vessel.RV_LloydsNumber = "0002";
			vessel.RV_RN_NKCountryOfReg = "US";
			fishing.US_VesselName = "Test";
			AssertEquals(ZString.Empty, fishing.US_VesselCountry);
			AssertEquals(ZString.Empty, fishing.US_VesselIMO);

			fishing.US_VesselCountry = "CA";
			fishing.US_VesselIMO = "0001";
			fishing.US_VesselName = "Test 1";
			AssertEquals("CA", fishing.US_VesselCountry);
			AssertEquals("0001", fishing.US_VesselIMO);

			fishing.US_VesselName = "vessel test";
			AssertEquals("US", fishing.US_VesselCountry);
			AssertEquals("0002", fishing.US_VesselIMO);
		}

		protected override IEnumerable<FishingInformation> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			var fishing = invoiceLine.FishingInformations.AddNew();
			fishing.US_MethodOfHarvest = SourceTypeCodesList.Codes.Vessel;
			yield return fishing;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			var fishing = invoiceLine.FishingInformations.AddNew();
			fishing.US_MethodOfHarvest = SourceTypeCodesList.Codes.Vessel;
			return fishing;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			var fishing = invoiceLine.FishingInformations.AddNew();
			fishing.US_MethodOfHarvest = SourceTypeCodesList.Codes.Vessel;
			return fishing;
		}
	}
}
