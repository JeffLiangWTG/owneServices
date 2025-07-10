using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocSendingObject))]
	class SupportingDocSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocument()
		{
			var eDocOnManifest2 = manifest.DocManagerInfo.AddFileOrDocument(new byte[1], "Manifest.pdf", "CIV");
			sendingObject.EDoc = eDocOnManifest.UniqueKey;
			AssertEquals(eDocOnManifest.UniqueKey, sendingObject.Document.UniqueKey);
			sendingObject.EDoc = eDocOnManifest2.UniqueKey;
			AssertEquals(eDocOnManifest2.UniqueKey, sendingObject.Document.UniqueKey);
		}

		public void TestAvailableEDocs()
		{
			AssertEquals(1, sendingObject.AvailableEDocs.Count);
			AssertEquals(eDocOnManifest.UniqueKey, sendingObject.AvailableEDocs[0].PK);
			manifest.DocManagerInfo.AddFileOrDocument(new byte[1], "ManifestDoc2.pdf", "CIV");
			AssertEquals(2, sendingObject.AvailableEDocs.Count);
			manifest.DocManagerInfo.AddFileOrDocument(new byte[1], "ManifestDoc3.pdf", "CIV", description: "ManifestDoc3");
			AssertEquals(3, sendingObject.AvailableEDocs.Count);
			manifest.DocManagerInfo.AddFileOrDocument(new byte[1], "ManifestDoc4.txt", "VIC", description: "ManifestDoc4");
			AssertEquals(3, sendingObject.AvailableEDocs.Count);
		}

		public void TestAvailableCaseNumbersForSendingSupportingDocs()
		{
			var expectedCaseNumbers = new List<string>();
			var header = Factory.New<AsycudaManifestHeader>();
			var caseNumber = header.CaseNumbers.AddNew();
			ParepareCaseNumber(expectedCaseNumbers, caseNumber, "HDR-01");
			caseNumber = header.CaseNumbers.AddNew();
			ParepareCaseNumber(expectedCaseNumbers, caseNumber, "HDR-02");
			var bill = header.Bills.AddNew();
			caseNumber = bill.CaseNumbers.AddNew();
			ParepareCaseNumber(expectedCaseNumbers, caseNumber, "BIL-A-01");
			caseNumber = bill.CaseNumbers.AddNew();
			ParepareCaseNumber(expectedCaseNumbers, caseNumber, "BIL-A-02");
			bill = header.Bills.AddNew();
			caseNumber = bill.CaseNumbers.AddNew();
			ParepareCaseNumber(expectedCaseNumbers, caseNumber, "BIL-B-01");
			caseNumber = bill.CaseNumbers.AddNew();
			ParepareCaseNumber(expectedCaseNumbers, caseNumber, "BIL-B-02");
			var sendingObject = SupportingDocSendingObject.New(header);
			var caseNumbersFromSendingObject = sendingObject.CaseNumbers.Cast<ZA.Business.CaseNumber>().ToList();
			AssertEquals("Prerequisite: Total number of case numbers expected.", 6, expectedCaseNumbers.Count);
			AssertEquals("Total number of cases numbers found.", expectedCaseNumbers.Count, caseNumbersFromSendingObject.Count);
			Action<string> testAction = caseReferenceNumber =>
			{
				var caseNumbersFiltered = caseNumbersFromSendingObject.FindAll(x => x.CY_Data == caseReferenceNumber).ToList();
				AssertEquals("Case number " + caseReferenceNumber + " must appear only once.", 1, caseNumbersFiltered.Count);
				var caseNumberFound = caseNumbersFiltered[0];
				AssertEquals("Inspecting case number value.", caseReferenceNumber, caseNumberFound.CY_Data);
			};
			expectedCaseNumbers.ForEach(testAction);
		}

		public void TestDefaultDocType()
		{
			CreateCusMaps();

			sendingObject.EDoc = eDocOnManifest.UniqueKey;
			AssertEquals("Document Type mapping CIV -> INV", "INV", sendingObject.DocumentType);

			sendingObject.EDoc = manifest.DocManagerInfo.AddFileOrDocument(new byte[1], "ManifestDoc2.pdf", "DGF").UniqueKey;
			AssertEquals("Document Type mapping DGF -> DGS", "DGS", sendingObject.DocumentType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifest = Factory.New<AsycudaManifestHeader>();
			eDocOnManifest = manifest.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			sendingObject = SupportingDocSendingObject.New(manifest);
		}

		protected AsycudaManifestHeader manifest;
		protected SupportingDocSendingObject sendingObject;
		protected IeDoc eDocOnManifest;

		protected override BusinessObject GetNewBusinessObject()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return SupportingDocSendingObject.New(manifest);
		}

		void ParepareCaseNumber(List<string> expectedCaseNumbers, ZA.Business.CaseNumber caseNumber, string caseReferenceNumber)
		{
			caseNumber.CY_Type = "CAS";
			caseNumber.CY_Code = "SUP";
			caseNumber.CY_Data = caseReferenceNumber;
			expectedCaseNumbers.Add(caseNumber.CY_Data);
		}

		void CreateCusMaps()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var zaCountry = Core.Constants.CountryCodes.SouthAfrica;
			helper.CreateCusMapType("ZADOC", "OUT", "ZA Supporting Document Types", false);
			helper.CreateCusMap("ZADOC", "DGF", "DGS", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			helper.CreateCusMap("ZADOC", "CIV", "INV", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			Factory.Save();
		}
	}
}
