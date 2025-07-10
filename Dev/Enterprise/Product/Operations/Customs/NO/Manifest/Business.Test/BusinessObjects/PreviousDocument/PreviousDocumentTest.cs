using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NO.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(PreviousDocument))]
[BooleanRegistryItemTest(typeof(NOCustomsDataRegistry), nameof(NOCustomsDataRegistry.EnableNOManifests))]
sealed class PreviousDocumentTest : CusSupportingInfoTest<PreviousDocument>
{
	public void TestValidation() =>	AssertType<PreviousDocumentValidation>(Factory.New<PreviousDocument>().Validation);

	public void TestLookups()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		var lookups = previousDocument.Lookups;
		CombineAssertions(() =>
		{
			AssertType<PreviousDocumentLookups>(lookups);
			AssertSame(lookups, previousDocument.Lookups);
		});
	}

	public void TestCSI_Code_Attributes() => CombineAssertions(() =>
		AssertEntity<PreviousDocument>()
			.HasProperty(x => x.CSI_Code)
			.WithCaption("Type")
			.WithList($"{nameof(PreviousDocument.Lookups)}.{nameof(PreviousDocumentLookups.CodeList)}")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 4));

	public void TestDocumentDescription_Attributes() => CombineAssertions(() =>
		AssertEntity<PreviousDocument>()
			.HasProperty(x => x.DocumentDescription)
			.WithCaption("Description"));

	public void TestCSI_ReferenceNumber_Attributes() => CombineAssertions(() =>
		AssertEntity<PreviousDocument>()
			.HasProperty(x => x.CSI_ReferenceNumber)
			.WithCaption("Number")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 70));

	public void TestDocumentDescription()
	{
		var today = ZDateTime.Now;
		var yesterday = today.AddDays(-1);
		var tomorrow = today.AddDays(1);

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var previousDocumentType = NO.Business.UniversalReferenceConstants.RefCusCodeListType.PreviousDocumentsForManifestBill;
		_ = helper.CreateCusCodeType(previousDocumentType, "Previous Documents");
		_ = helper.CreateCusCodeList(Core.Constants.CountryCodes.Norway, previousDocumentType, "Y001", "Wholly obtained in Lebanon and transported directly from that country to the Community.", yesterday, tomorrow);
		_ = helper.CreateCusCodeList(Core.Constants.CountryCodes.Norway, previousDocumentType, "NT08", "Test code added.", yesterday, tomorrow);
		_ = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN, previousDocumentType, "NX02", "Sample EUN Code", yesterday, tomorrow);

		Factory.Save();

		var bill = Factory.New<AsycudaBill>();
		var previousDocuments = bill.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			previousDocuments.CSI_Code = "Y001";
			AssertEquals("Wholly obtained in Lebanon and transported directly from that country to the Community.", previousDocuments.DocumentDescription);

			previousDocuments.CSI_Code = "NT08";
			AssertEquals("Test code added.", previousDocuments.DocumentDescription);

			previousDocuments.CSI_Code = "NX02";
			AssertEquals("Sample EUN Code", previousDocuments.DocumentDescription);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Norway;
		header.AMA_ManifestType = NOManifestTypes.Codes.DMO;
		var bill = header.Bills.AddNew();
		var previousDocumentsOnBill = bill.PreviousDocuments.AddNew();

		yield return previousDocumentsOnBill;
	}
}
