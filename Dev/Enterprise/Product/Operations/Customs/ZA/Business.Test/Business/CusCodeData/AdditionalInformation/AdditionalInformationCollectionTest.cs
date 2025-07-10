using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AdditionalInformationCollection))]
	sealed class AdditionalInformationCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<AdditionalInformation>
	{
		public void TestOnRemoved()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var rcc = helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
			var rcv = helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
			addInfo1.CY_Order = 1;
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditValue;
			addInfo2.CY_Order = 2;
			var addInfo3 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo3.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate;
			addInfo3.CY_Order = 3;
			var addInfo4 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo4.CY_Code = UniversalReferenceConstants.AdditionalInformation.RebateCreditValue;
			addInfo4.CY_Order = 3;
			entryLine.AdditionalInformationCodes.RemoveAndDelete(addInfo3);
			AssertNotEquals("CY_Order = 0 if !HasMultiplePairs", ZShort.Zero, addInfo1.CY_Order);
			AssertNotEquals("CY_Order = 0 if !HasMultiplePairs", ZShort.Zero, addInfo2.CY_Order);
			entryLine.AdditionalInformationCodes.RemoveAndDelete(addInfo4);
			AssertEquals("CY_Order = 0 if !HasMultiplePairs", ZShort.Zero, addInfo1.CY_Order);
			AssertEquals("CY_Order = 0 if !HasMultiplePairs", ZShort.Zero, addInfo2.CY_Order);
		}

		protected override CusCodeDataCollection<AdditionalInformation> GetCusCodeDataCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return new AdditionalInformationCollection(entryLine);
		}

		public void TestTypedIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var collection = new AdditionalInformationCollection(entryLine);
			var info = collection.AddNew();
			AssertEquals(info, collection[0]);
		}

		public void TestAddNewOriginal()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var collection = new AdditionalInformationCollection(entryLine);
			AssertEquals(typeof(AdditionalInformation), collection.AddNew().GetType());
		}

		public void TestFindAdditionalInformationByCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var collection = new AdditionalInformationCollection(entryLine);
			var addInfo1 = collection.AddNew("AAA");
			var addInfo2 = collection.AddNew("BBB");
			var addInfo3 = collection.AddNew("CCC");
			var foundAddInfo1 = collection["AAA"];
			var foundAddInfo2 = collection["BBB"];
			var foundAddInfo3 = collection["CCC"];
			AssertEquals(addInfo1, foundAddInfo1);
			AssertEquals(addInfo2, foundAddInfo2);
			AssertEquals(addInfo3, foundAddInfo3);
		}
	}
}
