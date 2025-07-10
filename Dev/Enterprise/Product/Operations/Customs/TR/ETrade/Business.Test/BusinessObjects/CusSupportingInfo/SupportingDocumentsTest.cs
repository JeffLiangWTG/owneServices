using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(SupportingDocuments))]
	public class SupportingDocumentsTest : CusSupportingInfoTest<SupportingDocuments>
	{
		public void TestDocumentDescription()
		{
			AssertEquals(supportingDocuments.DocumentDescription, documentDescription);
		}

		public void TestValidation()
		{
			AssertEquals("Validation", typeof(SupportingDocumentsValidation), supportingDocuments.Validation.GetType());
		}

		public void TestLookups()
		{
			AssertEquals("Lookups", typeof(SupportingDocumentsLookups), supportingDocuments.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<SupportingDocuments>();
			var type = header.CSI_Type;
			AssertEquals("BIL", type);

			var statusList = header.CSI_Status;
			AssertEquals(SupportingDocumentStatusList.Codes.EXS, statusList);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var supportingDocumentsForBill = bill.SupportingDocumentsForBill.AddNew();
			supportingDocumentsForBill.CSI_DataModel = "TR";
			return supportingDocumentsForBill;
		}

		protected override IEnumerable<SupportingDocuments> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var supportingDocumentsForBill = bill.SupportingDocumentsForBill.AddNew();
			Factory.Save();
			yield return supportingDocumentsForBill;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			supportingDocuments = bill.SupportingDocumentsForBill.AddNew();
			var yesterday = ZDateTime.Now.AddDays(-1);
			var tomorrow = ZDateTime.Now.AddDays(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TRDOC", "TRDOC");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRDOC", "0101", documentDescription, yesterday, tomorrow);
			Factory.Save();
			SetData();
		}

		void SetData()
		{
			supportingDocuments.CSI_Code = "0101";
		}

		readonly ZString documentDescription = "TestDescription";
		SupportingDocuments supportingDocuments;
		AsycudaBill bill;
		AsycudaManifestHeader header;
	}
}
